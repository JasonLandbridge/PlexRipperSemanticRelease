﻿using PlexRipper.Application;
using Quartz;

namespace BackgroundServices.Jobs;

public class SyncServerJob : IJob
{
    private readonly IPlexServerService _plexServerService;

    public SyncServerJob(IPlexServerService plexServerService)
    {
        _plexServerService = plexServerService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _plexServerService.SyncPlexServers();
    }
}