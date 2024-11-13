using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace PlexRipper.Application;

/// <summary>
/// This job will check the status of all connections for a given Plex Server and runs periodically.
/// </summary>
[DisallowConcurrentExecution]
public class CheckAllConnectionsStatusByPlexServerJob : IJob
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;

    public static JobKey GetJobKey() =>
        new(nameof(CheckAllConnectionsStatusByPlexServerJob), nameof(CheckAllConnectionsStatusByPlexServerJob));

    public CheckAllConnectionsStatusByPlexServerJob(
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

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var cancellationToken = context.CancellationToken;
            var plexServers = _dbContext
                .PlexServers.Include(x => x.PlexServerConnections)
                .Where(x => x.IsEnabled)
                .ToList();

            if (!plexServers.Any())
            {
                return;
            }

            // Send start job status update
            var update = new JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>(
                JobTypes.CheckAllConnectionsStatusByPlexServerJob,
                JobStatus.Started,
                new CheckAllConnectionStatusUpdateDTO
                {
                    PlexServersWithConnectionIds = plexServers.ToDictionary(
                        x => x.Id,
                        x => x.PlexServerConnections.Select(y => y.Id).ToList()
                    ),
                }
            );

            await _signalRService.SendJobStatusUpdateAsync(update);

            await Task.WhenAll(
                plexServers.Select(async plexServer =>
                    await _mediator.Send(
                        new CheckAllConnectionsStatusByPlexServerCommand(plexServer.Id),
                        cancellationToken
                    )
                )
            );

            // Send completed job status update
            update.Status = JobStatus.Completed;
            await _signalRService.SendJobStatusUpdateAsync(update);

            _log.Debug(
                "{JobName} for servers with ids: {PlexServerIds} completed",
                nameof(CheckAllConnectionsStatusByPlexServerJob),
                plexServers
            );
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}
