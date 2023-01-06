using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadQueue_CheckDownloadQueue_UnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueue_CheckDownloadQueue_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveNoUpdates_WhenGivenAnEmptyList()
    {
        // Arrange
        mock.SetupMediator(It.IsAny<GetAllDownloadTasksInPlexServersQuery>)
            .ReturnsAsync(Result.Ok(new List<PlexServer>()));

        List<DownloadTask> startCommands = new();

        // Act
        _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
        _sut.Setup();
        await _sut.CheckDownloadQueue(new List<int>());

        // Assert
        startCommands.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldHaveNoStartCommands_WhenATaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
        mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
            .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));
        mock.SetupMediator(It.IsAny<GetPlexServerNameByIdQuery>)
            .ReturnsAsync((GetPlexServerNameByIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.FirstOrDefault(x => x.PlexServerId == query.Id).Title));

        List<DownloadTask> startCommands = new();
        var plexServers = await DbContext.PlexServers
            .AsTracking()
            .Include(x => x.PlexLibraries)
            .ThenInclude(x => x.DownloadTasks)
            .ToListAsync();

        var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
        startedDownloadTask.DownloadStatus = DownloadStatus.Downloading;
        startedDownloadTask.Children.SetToDownloading();
        await DbContext.SaveChangesAsync();

        // Act
        _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
        await _sut.CheckDownloadQueueServer(1);

        // Assert
        startCommands.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldHaveOneDownloadTaskStarted_WhenGivenMovieDownloadTasks()
    {
        // Arrange
        Seed = 5000;
        List<DownloadTask> startCommands = new();
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
        mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
            .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));
        mock.SetupMediator(It.IsAny<GetPlexServerNameByIdQuery>)
            .ReturnsAsync((GetPlexServerNameByIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.FirstOrDefault(x => x.PlexServerId == query.Id).Title));

        // Act
        _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
        await _sut.CheckDownloadQueueServer(downloadTasks.First().PlexServerId);

        // Assert
        var startedDownloadTask = downloadTasks[0].Children[0];
        startCommands.Count.ShouldBe(1);
        startCommands[0].Id.ShouldBe(startedDownloadTask.Id);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenAMovieDownloadTasksWithCompleted()
    {
        // Arrange
        Seed = 5000;
        List<DownloadTask> startCommands = new();
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
        mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
            .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));
        mock.SetupMediator(It.IsAny<GetPlexServerNameByIdQuery>)
            .ReturnsAsync((GetPlexServerNameByIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.FirstOrDefault(x => x.PlexServerId == query.Id).Title));

        // ** Set first task to Completed
        var movieDownloadTask = DbContext.DownloadTasks.Include(x => x.Children).AsTracking().First();
        movieDownloadTask.DownloadStatus = DownloadStatus.Completed;
        movieDownloadTask.Children.ForEach(x => x.DownloadStatus = DownloadStatus.Completed);
        await DbContext.SaveChangesAsync();
        DownloadTask startedDownloadTask = null;

        // Act
        _sut.StartDownloadTask.Subscribe(update => startedDownloadTask = update);
        await _sut.CheckDownloadQueueServer(downloadTasks.First().PlexServerId);

        // Assert
        startedDownloadTask.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenATvShowsDownloadTasksWithCompleted()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 10;
            config.TvShowDownloadTasksCount = 2;
            config.TvShowSeasonDownloadTasksCount = 2;
            config.TvShowEpisodeDownloadTasksCount = 2;
        });

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();
        mock.SetupMediator(It.IsAny<GetDownloadTasksByPlexServerIdQuery>)
            .ReturnsAsync((GetDownloadTasksByPlexServerIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.Where(x => x.PlexServerId == query.PlexServerId).ToList()));
        mock.SetupMediator(It.IsAny<GetPlexServerNameByIdQuery>)
            .ReturnsAsync((GetPlexServerNameByIdQuery query, CancellationToken _) =>
                Result.Ok(downloadTasks.FirstOrDefault(x => x.PlexServerId == query.Id).Title));

        DownloadTask startedDownloadTask = null;

        // ** Set first task to Completed
        var tvShowDownloadTask = downloadTasks[0];
        tvShowDownloadTask.DownloadStatus = DownloadStatus.Completed;
        tvShowDownloadTask.Children = tvShowDownloadTask.Children.SetToCompleted();
        await DbContext.SaveChangesAsync();

        // Act
        _sut.StartDownloadTask.Subscribe(update => startedDownloadTask = update);
        await _sut.CheckDownloadQueueServer(1);

        // Assert
        startedDownloadTask.ShouldNotBeNull();
        startedDownloadTask.Id.ShouldBe(downloadTasks[1].Children[0].Children[0].Children[0].Id);
    }
}