﻿using FluentResults;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public interface IDownloadCommands
{
    /// <summary>
    /// Restart the <see cref="DownloadTask"/> by deleting the PlexDownloadClient and starting a new one.
    /// </summary>
    /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to restart.</param>
    /// <returns>Is successful.</returns>
    Task<Result> RestartDownloadTask(int downloadTaskId);

    /// <summary>
    /// Pause a currently downloading <see cref="DownloadTask"/>.
    /// </summary>
    /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to pause.</param>
    /// <returns>Is successful.</returns>
    Task<Result> PauseDownloadTask(int downloadTaskId);

    /// <summary>
    /// Stops and disposes of the PlexDownloadClient executing the <see cref="DownloadTask"/> if it is downloading.
    /// </summary>
    /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to stop.</param>
    /// <returns>If successful a list of the DownloadTasks that were stopped.</returns>
    Task<Result> StopDownloadTasks(int downloadTaskId);

    /// <summary>
    /// Stops and deletes (active) PlexDownloadClients and removes <see cref="DownloadTask"/> from the database.
    /// </summary>
    /// <param name="downloadTaskIds">The list of <see cref="DownloadTask"/> to delete.</param>
    /// <returns><see cref="Result"/> fails on error.</returns>
    Task<Result<bool>> DeleteDownloadTaskClients(List<int> downloadTaskIds);
}