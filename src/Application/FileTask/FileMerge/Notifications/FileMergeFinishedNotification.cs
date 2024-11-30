using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.Notifications;

public record FileMergeFinishedNotification(DownloadTaskKey Key) : INotification;

public class FileMergeFinishedHandler : INotificationHandler<FileMergeFinishedNotification>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IFileMergeSystem _fileMergeSystem;

    public FileMergeFinishedHandler(
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IFileMergeSystem fileMergeSystem
    )
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _fileMergeSystem = fileMergeSystem;
    }

    public async Task Handle(FileMergeFinishedNotification notification, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.GetDownloadTaskFileAsync(notification.Key, CancellationToken.None);
        if (downloadTask is null)
        {
            ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), notification.Key.Id).LogError();
            return;
        }

        // TODO - Delete the directory of the tv-show
        _fileMergeSystem.DeleteDirectoryFromFilePath(downloadTask.FilePaths.First());

        await _dbContext.SetDownloadStatus(notification.Key, DownloadStatus.Completed);

        await _mediator.Send(new DownloadTaskUpdatedNotification(notification.Key), cancellationToken);
    }
}
