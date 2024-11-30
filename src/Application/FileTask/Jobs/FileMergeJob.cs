using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Application.Contracts;
using Data.Contracts;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Notifications;
using Quartz;

namespace PlexRipper.Application;

public class FileMergeJob : IJob
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IFileMergeSystem _fileMergeSystem;
    private readonly Subject<FileMergeProgress> _bytesReceivedProgress = new();
    private readonly TaskCompletionSource<object> _progressCompletionSource = new();

    public FileMergeJob(ILog log, IMediator mediator, IPlexRipperDbContext dbContext, IFileMergeSystem fileMergeSystem)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _fileMergeSystem = fileMergeSystem;
    }

    public static string FileTaskId => "FileTaskId";

    public static JobKey GetJobKey(int id) => new($"{FileTaskId}_{id}", nameof(FileMergeJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var fileTaskId = dataMap.GetIntValue(FileTaskId);
        var token = context.CancellationToken;
        _log.Here()
            .Debug(
                "Executing job: {NameOfFileMergeJob} for {NameOfFileTaskId} with id: {FileTaskId}",
                nameof(FileMergeJob),
                nameof(fileTaskId),
                fileTaskId
            );

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var fileTask = await _dbContext.FileTasks.GetAsync(fileTaskId, token);
            if (fileTask == null)
            {
                ResultExtensions.EntityNotFound(nameof(FileTask), fileTaskId).LogError();
                return;
            }

            var downloadTask = await _dbContext.GetDownloadTaskAsync(fileTask.DownloadTaskKey, token);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), fileTaskId).LogError();
                return;
            }

            _log.Information(
                "Executing {NameOfFileMergeJob} with name {FileTaskFileName} and id {FileTaskId}",
                nameof(FileMergeJob),
                fileTask.FileName,
                fileTaskId
            );

            if (!fileTask.FilePaths.Any())
            {
                _log.Error(
                    "File task: {FileName} with id {FileTaskId} did not have any file paths to merge",
                    fileTask.FileName,
                    fileTask.Id
                );
                return;
            }

            var newDownloadStatus = DownloadStatus.Merging;
            if (fileTask.FilePaths.Count == 1)
                newDownloadStatus = DownloadStatus.Moving;

            downloadTask.DownloadStatus = newDownloadStatus;
            downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = newDownloadStatus);

            await _dbContext.SetDownloadStatus(downloadTask.ToKey(), newDownloadStatus);
            var downloadWorkerIds = downloadTask.DownloadWorkerTasks.Select(x => x.Id).ToList();
            await _dbContext
                .DownloadWorkerTasks.Where(x => downloadWorkerIds.Contains(x.Id))
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, newDownloadStatus), token);

            await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTask.ToKey()), token);

            // Verify all file paths exists
            foreach (var path in fileTask.FilePaths)
                if (!_fileMergeSystem.FileExists(path))
                {
                    var result = Result
                        .Fail($"Filepath: {path} does not exist and cannot be used to merge/move the file!")
                        .LogError();
                    await _mediator.SendNotificationAsync(result);
                    return;
                }

            try
            {
                // Create FileMergeProgress from bytes received progress
                SetupSubscription(token);

                var mergedFileTaskResult = await _mediator.Send(
                    new MergeFilesFromFileTaskCommand(fileTask, _bytesReceivedProgress),
                    token
                );
            }
            catch (Exception e)
            {
                await _mediator.SendNotificationAsync(Result.Fail(new ExceptionalError(e)).LogError());
            }

            // Clean-up
            await _progressCompletionSource.Task;
            _log.Here()
                .Information(
                    "Finished combining {FilePathsCount} files into {FileTaskFileName}",
                    fileTask.FilePaths.Count,
                    fileTask.FileName
                );

            await _mediator.Publish(new FileMergeFinishedNotification(fileTaskId), token);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }

    private void SetupSubscription(CancellationToken token)
    {
        var timeContext = new EventLoopScheduler();

        _bytesReceivedProgress
            .Sample(TimeSpan.FromSeconds(1), timeContext)
            .SelectMany(async data =>
                await _mediator.Publish(new FileMergeProgressNotification(data), token).ToObservable()
            )
            .Subscribe(
                _ => { },
                ex =>
                {
                    _log.Error(ex);
                    _progressCompletionSource.SetException(ex);
                },
                () =>
                {
                    _progressCompletionSource.SetResult(true);
                    timeContext.Dispose();
                }
            );
    }
}
