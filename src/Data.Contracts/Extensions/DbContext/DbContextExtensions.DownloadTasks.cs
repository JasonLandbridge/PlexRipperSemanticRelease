using FluentResults;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<string>> GetDownloadUrl(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        string fileLocationUrl,
        CancellationToken cancellationToken = default
    )
    {
        var plexServerConnectionResult = await dbContext.ChoosePlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogError();

        var plexServerConnection = plexServerConnectionResult.Value;
        var plexServer = plexServerConnection.PlexServer;

        var tokenResult = await dbContext.GetPlexServerTokenAsync(plexServerId, cancellationToken);
        if (tokenResult.IsFailed)
        {
            _log.Error("Could not find a valid token for server {ServerName}", plexServer?.Name ?? "Unknown");
            return tokenResult.ToResult();
        }

        var downloadUrl = plexServerConnection.GetDownloadUrl(fileLocationUrl, tokenResult.Value);
        return Result.Ok(downloadUrl);
    }

    public static async Task<DownloadTaskKey?> GetDownloadTaskKeyAsync(
        this IPlexRipperDbContext dbContext,
        Guid guid,
        CancellationToken cancellationToken = default
    )
    {
        if (guid == Guid.Empty)
            return null;

        var queries = new List<IQueryable<DownloadTaskKey>>()
        {
            dbContext.DownloadTaskTvShow.ProjectToKey(),
            dbContext.DownloadTaskTvShowSeason.ProjectToKey(),
            dbContext.DownloadTaskTvShowEpisode.ProjectToKey(),
            dbContext.DownloadTaskTvShowEpisodeFile.ProjectToKey(),
            dbContext.DownloadTaskMovie.ProjectToKey(),
            dbContext.DownloadTaskMovieFile.ProjectToKey(),
        };

        foreach (var query in queries)
        {
            var downloadTaskKey = await query.FirstOrDefaultAsync(x => x.Id == guid, cancellationToken);
            if (downloadTaskKey is not null)
                return downloadTaskKey;
        }

        return null;
    }

    public static async Task<DownloadTaskType> GetDownloadTaskTypeAsync(
        this IPlexRipperDbContext dbContext,
        Guid guid,
        CancellationToken cancellationToken = default
    )
    {
        if (guid == Guid.Empty)
            return DownloadTaskType.None;

        if (await dbContext.DownloadTaskTvShow.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.TvShow;

        if (await dbContext.DownloadTaskMovie.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.Movie;

        if (await dbContext.DownloadTaskTvShowSeason.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.Season;

        if (await dbContext.DownloadTaskTvShowEpisode.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.Episode;

        if (await dbContext.DownloadTaskTvShowEpisodeFile.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.EpisodeData;

        if (await dbContext.DownloadTaskMovieFile.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.MovieData;

        return DownloadTaskType.None;
    }

    /// <summary>
    /// Retrieves a <see cref="DownloadTaskGeneric"/> from the database based on the <paramref name="id"/> and <paramref name="type"/> with all its children and related entities.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IPlexRipperDbContext"/> to query. </param>
    /// <param name="key"> The <see cref="DownloadTaskKey"/> to retrieve the <see cref="DownloadTaskGeneric"/> by. </param>
    /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
    /// <returns> The <see cref="DownloadTaskGeneric"/> if found, otherwise null. </returns>
    public static Task<DownloadTaskGeneric?> GetDownloadTaskAsync(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    ) => dbContext.GetDownloadTaskAsync(key.Id, key.Type, cancellationToken);

    /// <summary>
    /// Retrieves a <see cref="DownloadTaskGeneric"/> from the database based on the <paramref name="id"/> and <paramref name="type"/> with all its children and related entities.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IPlexRipperDbContext"/> to query. </param>
    /// <param name="id"> The id of the <see cref="DownloadTaskGeneric"/> to retrieve. </param>
    /// <param name="type"> The type of the root <see cref="DownloadTaskGeneric"/> to retrieve. </param>
    /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
    /// <returns> The <see cref="DownloadTaskGeneric"/> if found, otherwise null. </returns>
    public static async Task<DownloadTaskGeneric?> GetDownloadTaskAsync(
        this IPlexRipperDbContext dbContext,
        Guid id,
        DownloadTaskType type = DownloadTaskType.None,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (id == Guid.Empty)
                return null;

            if (type == DownloadTaskType.None)
                type = await dbContext.GetDownloadTaskTypeAsync(id, cancellationToken);

            switch (type)
            {
                // DownloadTaskType.Movie
                case DownloadTaskType.Movie:
                    var downloadTaskMovie = await dbContext
                        .DownloadTaskMovie.IncludeAll()
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskMovie?.ToGeneric() ?? null;

                // DownloadTaskType.MovieData
                case DownloadTaskType.MovieData:
                case DownloadTaskType.MoviePart:
                    var downloadTaskMovieFile = await dbContext
                        .DownloadTaskMovieFile.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.DownloadWorkerTasks)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskMovieFile?.ToGeneric() ?? null;

                // DownloadTaskType.TvShow
                case DownloadTaskType.TvShow:
                    var downloadTaskTvShow = await dbContext
                        .DownloadTaskTvShow.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.Children)
                        .ThenInclude(x => x.Children)
                        .ThenInclude(x => x.Children)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShow?.ToGeneric() ?? null;

                // DownloadTaskType.TvShowSeason
                case DownloadTaskType.Season:
                    var downloadTaskTvShowSeason = await dbContext
                        .DownloadTaskTvShowSeason.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.Children)
                        .ThenInclude(x => x.Children)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShowSeason?.ToGeneric() ?? null;

                // DownloadTaskType.Episode
                case DownloadTaskType.Episode:
                    var downloadTaskTvShowEpisode = await dbContext
                        .DownloadTaskTvShowEpisode.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.Children)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShowEpisode?.ToGeneric() ?? null;

                // DownloadTaskType.EpisodeData
                case DownloadTaskType.EpisodeData:
                case DownloadTaskType.EpisodePart:
                    var downloadTaskTvShowEpisodeFile = await dbContext
                        .DownloadTaskTvShowEpisodeFile.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.DownloadWorkerTasks)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShowEpisodeFile?.ToGeneric() ?? null;

                default:
                    return null;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Error(ex);
            throw;
        }
    }

    public static async Task<DownloadTaskFileBase?> GetDownloadTaskFileAsync(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            // DownloadTaskType.MovieData
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                return await dbContext
                    .DownloadTaskMovieFile.Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.DownloadWorkerTasks)
                    .FirstOrDefaultAsync(x => x.Id == key.Id, cancellationToken);

            // DownloadTaskType.EpisodeData
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                return await dbContext
                    .DownloadTaskTvShowEpisodeFile.Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.DownloadWorkerTasks)
                    .FirstOrDefaultAsync(x => x.Id == key.Id, cancellationToken);
            default:
                return null;
        }
    }

    public static async Task<List<DownloadTaskGeneric>> GetAllDownloadTasksByServerAsync(
        this IPlexRipperDbContext dbContext,
        int plexServerId = 0,
        bool asTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var downloadTasks = new List<DownloadTaskGeneric>();

        var downloadTasksMovies = await dbContext
            .DownloadTaskMovie.AsTracking(
                asTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking
            )
            .IncludeAll()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        var downloadTasksTvShows = await dbContext
            .DownloadTaskTvShow.AsTracking(
                asTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking
            )
            .IncludeAll()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        downloadTasks.AddRange(downloadTasksMovies.Select(x => x.ToGeneric()));
        downloadTasks.AddRange(downloadTasksTvShows.Select(x => x.ToGeneric()));

        // Sort by CreatedAt
        downloadTasks.Sort((x, y) => DateTime.Compare(x.CreatedAt, y.CreatedAt));

        return downloadTasks;
    }

    public static Task<DownloadTaskTvShow?> GetDownloadTaskTvShowByMediaKeyQuery(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        int mediaKey,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .DownloadTaskTvShow.AsTracking()
            .IncludeAll()
            .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId && x.Key == mediaKey, cancellationToken);
    }

    public static async Task UpdateDownloadProgress(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        IDownloadTaskProgress progress,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.MovieData:
                await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                                .SetProperty(x => x.DataReceived, progress.DataReceived)
                                .SetProperty(x => x.DataTotal, progress.DataTotal),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                                .SetProperty(x => x.DataReceived, progress.DataReceived)
                                .SetProperty(x => x.DataTotal, progress.DataTotal),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.Movie:
            case DownloadTaskType.TvShow:
            case DownloadTaskType.Season:
            case DownloadTaskType.Episode:
                _log.Here()
                    .Error(
                        "{Name} of type {Type} is not supported in {MethodName}",
                        nameof(DownloadTaskType),
                        key.Type,
                        nameof(UpdateDownloadProgress)
                    );
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static async Task UpdateDownloadFileTransferProgress(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        IDownloadFileTransferProgress progress
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.MovieData:
                await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p =>
                        p.SetProperty(x => x.FileTransferSpeed, progress.FileTransferSpeed)
                            .SetProperty(x => x.FileDataTransferred, progress.FileDataTransferred)
                            .SetProperty(x => x.CurrentFileTransferPathIndex, progress.CurrentFileTransferPathIndex)
                            .SetProperty(x => x.CurrentFileTransferBytesOffset, progress.CurrentFileTransferBytesOffset)
                    );
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p =>
                        p.SetProperty(x => x.FileTransferSpeed, progress.FileTransferSpeed)
                            .SetProperty(x => x.FileDataTransferred, progress.FileDataTransferred)
                            .SetProperty(x => x.CurrentFileTransferPathIndex, progress.CurrentFileTransferPathIndex)
                            .SetProperty(x => x.CurrentFileTransferBytesOffset, progress.CurrentFileTransferBytesOffset)
                    );
                break;
            case DownloadTaskType.Movie:
            case DownloadTaskType.TvShow:
            case DownloadTaskType.Season:
            case DownloadTaskType.Episode:
                _log.Here()
                    .Error(
                        "{Name} of type {Type} is not supported in {MethodName}",
                        nameof(DownloadTaskType),
                        key.Type,
                        nameof(UpdateDownloadProgress)
                    );
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static async Task UpdateDownloadWorkerProgress(
        this IPlexRipperDbContext dbContext,
        IList<DownloadWorkerTaskProgress> updates,
        CancellationToken cancellationToken = default
    )
    {
        var updatesDict = updates.ToDictionary(x => x.Id);

        // Fetch relevant tasks in one query and apply updates
        var downloadWorkerTasks = await dbContext
            .DownloadWorkerTasks.AsTracking()
            .Where(x => updatesDict.Keys.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var downloadWorkerTask in downloadWorkerTasks)
        {
            // Apply updates directly using the dictionary
            if (updatesDict.TryGetValue(downloadWorkerTask.Id, out var update))
            {
                downloadWorkerTask.DownloadStatus = update.Status;
                downloadWorkerTask.ElapsedTime = update.ElapsedTime;
                downloadWorkerTask.BytesReceived = update.DataReceived;
                downloadWorkerTask.DownloadSpeed = update.DownloadSpeed;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task<List<DownloadTaskKey>> GetDownloadableChildTaskKeys(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        var downloadTask = await dbContext.GetDownloadTaskAsync(key, cancellationToken);
        if (downloadTask is null)
            return new List<DownloadTaskKey>();

        var keys = new List<DownloadTaskKey>();

        FindDownloadableTaskKeys([downloadTask]);

        return keys;

        void FindDownloadableTaskKeys(List<DownloadTaskGeneric> tasks)
        {
            if (!tasks.Any())
                return;

            foreach (var task in tasks)
            {
                if (task.IsDownloadable)
                    keys.Add(task.ToKey());

                if (task.Children.Any())
                    FindDownloadableTaskKeys(task.Children);
            }
        }
    }

    public static async Task<List<DownloadTaskGeneric>> GetDownloadableChildTasks(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        var keys = await dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);

        var results = await Task.WhenAll(keys.Select(x => dbContext.GetDownloadTaskAsync(x, cancellationToken)));

        return results.Where(x => x != null).ToList()!;
    }
}
