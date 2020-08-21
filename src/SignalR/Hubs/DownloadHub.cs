﻿using Microsoft.AspNetCore.SignalR;
using PlexRipper.Domain;
using PlexRipper.Domain.Types;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces.SignalR;

namespace PlexRipper.SignalR.Hubs
{
    public class DownloadHub : Hub
    {
        public Task DownloadProgressAsync(DownloadProgress downloadProgress)
        {
            if (Clients != null)
            {
                return Clients.All.SendAsync("DownloadProgress", downloadProgress);
            }
            Log.Error("Clients is null, make sure a client has been subscribed!");
            return new Task(() => { });
        }
    }
}