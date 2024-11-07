using Application.Contracts;
using Data.Contracts;
using Logging.Interface;

namespace PlexRipper.Application;

public record ServerOnlineStatusChangedNotification : INotification
{
    public ServerOnlineStatusChangedNotification(int plexServerId, bool isOnline)
    {
        PlexServerId = plexServerId;
        IsOnline = isOnline;
    }

    public int PlexServerId { get; }

    public bool IsOnline { get; }
}

public class ServerOnlineStatusChangedHandler : INotificationHandler<ServerOnlineStatusChangedNotification>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IDownloadQueue _downloadQueue;

    public ServerOnlineStatusChangedHandler(ILog log, IPlexRipperDbContext dbContext, IDownloadQueue downloadQueue)
    {
        _log = log;
        _dbContext = dbContext;
        _downloadQueue = downloadQueue;
    }

    public async Task Handle(ServerOnlineStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.IsOnline)
        {
            var plexServerName = await _dbContext.GetPlexServerNameById(
                notification.PlexServerId,
                cancellationToken: cancellationToken
            );
            _log.Information(
                "Server {PlexServerName} came online, checking DownloadQueue to resume downloads",
                plexServerName
            );
            await _downloadQueue.CheckDownloadQueue([notification.PlexServerId]);
        }
    }
}
