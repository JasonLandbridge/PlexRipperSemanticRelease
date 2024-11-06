using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Checks every <see cref="PlexServerConnection"/> in parallel of a <see cref="PlexServer"/> whether it connects or not
/// and then stores that <see cref="PlexServerStatus"/> in the database.
/// </summary>
/// <param name="PlexServerId">The id of the <see cref="PlexServer" /> to check the connections for.</param>
/// <returns>Returns successful result if any connection connected.</returns>
public record CheckAllConnectionsStatusByPlexServerCommand(int PlexServerId) : IRequest<Result<List<PlexServerStatus>>>;

public class CheckAllConnectionsStatusByPlexServerValidator
    : AbstractValidator<CheckAllConnectionsStatusByPlexServerCommand>
{
    public CheckAllConnectionsStatusByPlexServerValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class CheckAllConnectionsStatusByPlexServerHandler
    : IRequestHandler<CheckAllConnectionsStatusByPlexServerCommand, Result<List<PlexServerStatus>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;

    public CheckAllConnectionsStatusByPlexServerHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        ISignalRService signalRService
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _signalRService = signalRService;
    }

    public async Task<Result<List<PlexServerStatus>>> Handle(
        CheckAllConnectionsStatusByPlexServerCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerId = command.PlexServerId;

        var plexServer = await _dbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .GetAsync(plexServerId, cancellationToken);

        if (plexServer == null)
            return ResultExtensions.EntityNotFound(nameof(plexServerId), plexServerId).LogError();

        var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId, cancellationToken);
        if (!plexServer.IsEnabled)
        {
            return ResultExtensions
                .ServerIsNotEnabled(plexServerName, plexServerId, nameof(CheckAllConnectionsStatusByPlexServerCommand))
                .LogError();
        }

        var connections = plexServer.PlexServerConnections.ToList();
        if (!connections.Any())
        {
            return _log.Error("No connections found for the plex server {PlexServerName}", plexServerName).ToResult();
        }

        // Send start job status update
        var update = new JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>(
            JobTypes.CheckAllConnectionsStatusByPlexServerJob,
            JobStatus.Started,
            new CheckAllConnectionStatusUpdateDTO
            {
                PlexServerId = plexServerId,
                PlexServerConnectionIds = connections.Select(x => x.Id).ToList(),
            }
        );
        await _signalRService.SendJobStatusUpdateAsync(update);

        var previousResult = await _dbContext.IsServerOnline(plexServerId, cancellationToken: cancellationToken);

        // Create connection check tasks for all connections
        var connectionTasks = connections.Select(async plexServerConnection =>
            await _mediator.Send(new CheckConnectionStatusByIdCommand(plexServerConnection.Id), cancellationToken)
        );

        var tasksResult = await Task.WhenAll(connectionTasks);
        var combinedResults = Result.Merge(tasksResult);

        await _signalRService.SendRefreshNotificationAsync([DataType.PlexServerConnection], cancellationToken);

        // Send completed job status update
        update.Status = JobStatus.Completed;
        await _signalRService.SendJobStatusUpdateAsync(update);

        // Compare previous and current online status
        var currentOnlineStatus = tasksResult.Any(statusResult => statusResult.ValueOrDefault?.IsSuccessful != null);

        if (previousResult != currentOnlineStatus)
        {
            await _mediator.Publish(
                new ServerOnlineStatusChangedNotification(plexServerId, currentOnlineStatus),
                CancellationToken.None
            );
        }

        if (currentOnlineStatus)
            return Result.Ok(combinedResults.Value.ToList());

        return _log.Error(
                "All connections to plex server with name: {PlexServerName} and id: {PlexServerId} failed to connect",
                plexServerName,
                plexServerId
            )
            .ToResult();
    }
}
