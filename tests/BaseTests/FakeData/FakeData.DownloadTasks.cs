﻿using Bogus;
using ByteSizeLib;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    public static Faker<T> ApplyBaseDownloadTask<T>(this Faker<T> faker, int seed = 0, Action<FakeDataConfig> options = null) where T : DownloadTask
    {
        var config = FakeDataConfig.FromOptions(options);

        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Queued)
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
            .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Movie)
            .RuleFor(x => x.Priority, _ => 0)
            .RuleFor(x => x.DataReceived, _ => 0)
            .RuleFor(x => x.DataTotal, f => config.DownloadFileSizeInMb > 0
                ? (long)ByteSize.FromMebiBytes(config.DownloadFileSizeInMb).Bytes
                : f.Random.Long(1, 10000000))
            .RuleFor(x => x.Percentage, _ => 0)
            .RuleFor(x => x.DownloadSpeed, _ => 0)
            .RuleFor(x => x.FileTransferSpeed, _ => 0)
            .RuleFor(x => x.DownloadWorkerTasks, _ => new List<DownloadWorkerTask>())
            .RuleFor(x => x.FileName, _ => "file.mp4")
            .RuleFor(x => x.FileLocationUrl, _ => PlexMockServerConfig.FileUrl)
            .RuleFor(x => x.DownloadDirectory, f => f.System.FilePath())
            .RuleFor(x => x.DestinationDirectory, f => f.System.FilePath())
            .RuleFor(x => x.ParentId, _ => null)
            .RuleFor(x => x.Parent, _ => null)
            .RuleFor(x => x.Key, _ => _random.Next(1, 10000))
            .RuleFor(x => x.Created, f => f.Date.Recent(30))
            .RuleFor(x => x.Quality, f => f.PickRandom("sd", "720", "1080"))
            .RuleFor(x => x.PlexServerId, _ => 0)
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexLibraryId, _ => 0)
            .RuleFor(x => x.PlexLibrary, _ => null)
            .RuleFor(x => x.Children, _ => new List<DownloadTask>())
            .RuleFor(x => x.DownloadFolder, _ => null)
            .RuleFor(x => x.DownloadFolderId, _ => 1)
            .RuleFor(x => x.DestinationFolder, _ => null)
            .RuleFor(x => x.ServerMachineIdentifier, f => f.Random.Hash())
            .RuleFor(x => x.DestinationFolderId, _ => 2)
            .RuleFor(x => x.RootDownloadTaskId, _ => 0);
    }

    #region Movie

    public static Faker<DownloadTask> GetMovieDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTask>()
            .ApplyBaseDownloadTask(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Movie)
            .RuleFor(x => x.Children, _ =>
            {
                if (config.IncludeMultiPartMovies)
                    return GetMoviePartDownloadTask(seed, options).Generate(2);

                return GetMovieDataDownloadTask(seed, options).Generate(1);
            })
            .RuleFor(x => x.DownloadFolderId, _ => 1)
            .RuleFor(x => x.DestinationFolderId, _ => 2)
            .FinishWith((_, downloadTask) =>
            {
                downloadTask.Children.ForEach(x =>
                {
                    x.Parent = x;
                    x.ParentId = x.Id;
                });
            });
    }

    public static Faker<DownloadTask> GetMovieDataDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTask>()
            .ApplyBaseDownloadTask(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.MovieData);
    }

    public static Faker<DownloadTask> GetMoviePartDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTask>()
            .ApplyBaseDownloadTask(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Movie)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.MoviePart);
    }

    #endregion

    #region TvShow

    public static Faker<DownloadTask> GetTvShowDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTask>()
            .ApplyBaseDownloadTask(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.TvShow)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.TvShow)
            .RuleFor(x => x.Children, _ =>
            {
                var f = GetTvShowSeasonDownloadTask(seed, options);
                if (config.TvShowSeasonDownloadTasksCount > 0)
                    return f.Generate(config.TvShowSeasonDownloadTasksCount);

                return f.GenerateBetween(1, 5);
            });
    }

    public static Faker<DownloadTask> GetTvShowSeasonDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTask>()
            .ApplyBaseDownloadTask(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Season)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Season)
            .RuleFor(x => x.Children, _ =>
            {
                var f = GetTvShowEpisodeDownloadTask(seed, options);
                if (config.TvShowEpisodeDownloadTasksCount > 0)
                    return f.Generate(config.TvShowEpisodeDownloadTasksCount);

                return f.GenerateBetween(5, 10);
            });
    }

    public static Faker<DownloadTask> GetTvShowEpisodeDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTask>()
            .ApplyBaseDownloadTask(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Episode)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Episode)
            .RuleFor(x => x.Children, _ => GetTvShowEpisodeDataDownloadTask(seed, options).Generate(1));
    }

    public static Faker<DownloadTask> GetTvShowEpisodeDataDownloadTask(int seed = 0, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<DownloadTask>()
            .ApplyBaseDownloadTask(seed, options)
            .UseSeed(seed)
            .RuleFor(x => x.MediaType, PlexMediaType.Episode)
            .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.EpisodeData);
    }

    #endregion
}