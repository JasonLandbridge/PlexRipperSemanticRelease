using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Pauses and disposes of the PlexDownloadClient executing the <see cref="DownloadTaskGeneric"/> if it is downloading.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to Pause.</param>
/// <returns>If successful a list of the DownloadTasks that were Paused.</returns>
public record PauseDownloadTaskCommand(Guid DownloadTaskGuid) : IRequest<Result>;

public class PauseDownloadTaskCommandValidator : AbstractValidator<PauseDownloadTaskCommand>
{
    public PauseDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class PauseDownloadTaskCommandHandler : IRequestHandler<PauseDownloadTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;
    private readonly IFileMergeScheduler _fileMergeScheduler;

    public PauseDownloadTaskCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IDownloadTaskScheduler downloadTaskScheduler,
        IFileMergeScheduler fileMergeScheduler
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
        _fileMergeScheduler = fileMergeScheduler;
    }

    public async Task<Result> Handle(PauseDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        var downloadTasks = await _dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);
        foreach (var downloadTaskKey in downloadTasks)
        {
            var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
                continue;
            }

            _log.Information("Pausing DownloadTask with id {DownloadTaskTitle} from downloading", downloadTask.Title);

            if (await _downloadTaskScheduler.IsDownloading(downloadTaskKey, cancellationToken))
            {
                var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey, cancellationToken);
                if (stopResult.IsFailed)
                {
                    // Since this command is done per server, we can abort since there will at most be 1 download task downloading at a time and if that fails we can't continue
                    return stopResult.LogError();
                }
            }
            else if (await _fileMergeScheduler.IsDownloadTaskMerging(downloadTaskKey))
            {
                var fileTaskId = _dbContext
                    .FileTasks.Where(x => x.DownloadTaskId == downloadTaskKey.Id)
                    .Select(x => x.Id)
                    .FirstOrDefault();
                await _fileMergeScheduler.StopFileMergeJob(fileTaskId);
            }

            if (downloadTask.DownloadStatus is DownloadStatus.Downloading or DownloadStatus.Merging)
            {
                await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Paused);
                await _mediator.Send(new DownloadTaskUpdatedNotification(key), cancellationToken);

                _log.Debug(
                    "DownloadTask {DownloadTaskId} has been Paused, meaning no downloaded files have been deleted",
                    command.DownloadTaskGuid
                );
            }
        }

        return Result.Ok();
    }
}
