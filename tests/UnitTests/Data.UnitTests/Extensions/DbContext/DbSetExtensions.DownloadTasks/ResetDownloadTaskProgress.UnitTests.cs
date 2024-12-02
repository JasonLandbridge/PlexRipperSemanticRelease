using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitTests;

public class ResetDownloadTaskProgressUnitTests : BaseUnitTest
{
    public ResetDownloadTaskProgressUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(DownloadTaskType.Movie)]
    [InlineData(DownloadTaskType.TvShow)]
    [InlineData(DownloadTaskType.Season)]
    [InlineData(DownloadTaskType.Episode)]
    public async Task ShouldReturnError_WhenUnsupportedDownloadTaskTypeProvided(DownloadTaskType type)
    {
        // Arrange
        await SetupDatabase(
            81238,
            config =>
            {
                config.MovieDownloadTasksCount = 5;
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            }
        );

        var dbContext = GetDbContext();
        DownloadTaskKey? key;
        switch (type)
        {
            case DownloadTaskType.Movie:
                key = await dbContext.DownloadTaskMovie.ProjectToKey().FirstOrDefaultAsync();
                break;
            case DownloadTaskType.TvShow:
                key = await dbContext.DownloadTaskTvShow.ProjectToKey().FirstOrDefaultAsync();
                break;
            case DownloadTaskType.Season:
                key = await dbContext.DownloadTaskTvShowSeason.ProjectToKey().FirstOrDefaultAsync();
                break;
            case DownloadTaskType.Episode:
                key = await dbContext.DownloadTaskTvShowEpisode.ProjectToKey().FirstOrDefaultAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        key.ShouldNotBeNull();

        // Act
        var result = await dbContext.ResetDownloadTaskProgress(key, DownloadStatus.Stopped);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message.Contains("not supported"));
    }

    [Fact]
    public async Task ShouldReturnResetDownloadTaskTypeMovieFile_WhenResetDownloadTaskProgressCalled()
    {
        // Arrange
        await SetupDatabase(43481, config => config.MovieDownloadTasksCount = 5);
        var dbContext = GetDbContext();
        var downloadTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync();
        var testDownloadTask = downloadTasks[2];

        testDownloadTask.DataTotal = 5000;
        testDownloadTask.DownloadSpeed = 50;
        testDownloadTask.DataReceived = 50;
        testDownloadTask.FileTransferSpeed = 50;
        testDownloadTask.FileDataTransferred = 50;
        testDownloadTask.CurrentFileTransferPathIndex = 50;
        testDownloadTask.CurrentFileTransferBytesOffset = 50;
        await dbContext.SaveChangesAsync();

        // Act
        var resetResult = await IDbContext.ResetDownloadTaskProgress(testDownloadTask.ToKey(), DownloadStatus.Stopped);

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await dbContext.GetDownloadTaskFileAsync(testDownloadTask.ToKey());
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadSpeed.ShouldBe(0);
        downloadTaskDb.DataReceived.ShouldBe(0);
        downloadTaskDb.FileTransferSpeed.ShouldBe(0);
        downloadTaskDb.FileDataTransferred.ShouldBe(0);
        downloadTaskDb.CurrentFileTransferPathIndex.ShouldBe(0);
        downloadTaskDb.CurrentFileTransferBytesOffset.ShouldBe(0);
        downloadTaskDb.DataTotal.ShouldBe(5000);
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);

        downloadTaskDb.FileName.ShouldBe(testDownloadTask.FileName);
        downloadTaskDb.DownloadDirectory.ShouldBe(testDownloadTask.DownloadDirectory);
        downloadTaskDb.DestinationDirectory.ShouldBe(testDownloadTask.DestinationDirectory);
        downloadTaskDb.FileLocationUrl.ShouldBe(testDownloadTask.FileLocationUrl);
        downloadTaskDb.CreatedAt.ShouldBe(testDownloadTask.CreatedAt);
        downloadTaskDb.Quality.ShouldBe(testDownloadTask.Quality);
        downloadTaskDb.IsDownloadable.ShouldBe(testDownloadTask.IsDownloadable);
        downloadTaskDb.MediaType.ShouldBe(testDownloadTask.MediaType);
        downloadTaskDb.FullTitle.ShouldBe(testDownloadTask.FullTitle);
        downloadTaskDb.PlexServerId.ShouldBe(testDownloadTask.PlexServerId);
        downloadTaskDb.PlexLibraryId.ShouldBe(testDownloadTask.PlexLibraryId);
    }

    [Fact]
    public async Task ShouldReturnResetDownloadTaskTypeEpisodeFile_WhenResetDownloadTaskProgressCalled()
    {
        // Arrange
        await SetupDatabase(
            8231434,
            config =>
            {
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 2;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );
        var dbContext = GetDbContext();
        var downloadTasks = await dbContext.DownloadTaskTvShowEpisodeFile.AsTracking().ToListAsync();
        var testDownloadTask = downloadTasks[4];

        testDownloadTask.DataTotal = 5000;
        testDownloadTask.DownloadSpeed = 50;
        testDownloadTask.DataReceived = 50;
        testDownloadTask.FileTransferSpeed = 50;
        testDownloadTask.FileDataTransferred = 50;
        testDownloadTask.CurrentFileTransferPathIndex = 50;
        testDownloadTask.CurrentFileTransferBytesOffset = 50;
        await dbContext.SaveChangesAsync();

        // Act
        var resetResult = await IDbContext.ResetDownloadTaskProgress(testDownloadTask.ToKey(), DownloadStatus.Stopped);

        // Assert
        resetResult.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await dbContext.GetDownloadTaskFileAsync(testDownloadTask.ToKey());
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadSpeed.ShouldBe(0);
        downloadTaskDb.DataReceived.ShouldBe(0);
        downloadTaskDb.FileTransferSpeed.ShouldBe(0);
        downloadTaskDb.FileDataTransferred.ShouldBe(0);
        downloadTaskDb.CurrentFileTransferPathIndex.ShouldBe(0);
        downloadTaskDb.CurrentFileTransferBytesOffset.ShouldBe(0);
        downloadTaskDb.DataTotal.ShouldBe(5000);

        downloadTaskDb.FileName.ShouldBe(testDownloadTask.FileName);
        downloadTaskDb.DownloadDirectory.ShouldBe(testDownloadTask.DownloadDirectory);
        downloadTaskDb.DestinationDirectory.ShouldBe(testDownloadTask.DestinationDirectory);
        downloadTaskDb.FileLocationUrl.ShouldBe(testDownloadTask.FileLocationUrl);
        downloadTaskDb.CreatedAt.ShouldBe(testDownloadTask.CreatedAt);
        downloadTaskDb.Quality.ShouldBe(testDownloadTask.Quality);
        downloadTaskDb.IsDownloadable.ShouldBe(testDownloadTask.IsDownloadable);
        downloadTaskDb.MediaType.ShouldBe(testDownloadTask.MediaType);
        downloadTaskDb.FullTitle.ShouldBe(testDownloadTask.FullTitle);
        downloadTaskDb.PlexServerId.ShouldBe(testDownloadTask.PlexServerId);
        downloadTaskDb.PlexLibraryId.ShouldBe(testDownloadTask.PlexLibraryId);
    }
}
