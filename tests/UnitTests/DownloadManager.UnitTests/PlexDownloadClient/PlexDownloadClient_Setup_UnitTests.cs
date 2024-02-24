﻿using System.Reactive.Linq;
using System.Reactive.Subjects;
using Data.Contracts;
using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.DownloadManager;
using Settings.Contracts;

namespace DownloadManager.UnitTests;

public class PlexDownloadClient_Setup_UnitTests : BaseUnitTest<PlexDownloadClient>
{
    public PlexDownloadClient_Setup_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
    {
        //Arrange

        // Act
        var result = _sut.Setup(null);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ShouldReturnFailedResult_WhenDownloadTaskPlexServerIsNull()
    {
        //Arrange
        var downloadTask = FakeData.GetMovieDownloadTask().Generate();
        downloadTask.PlexServer = null;

        // Act
        var result = _sut.Setup(downloadTask);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldSendErrorDownloadTaskUpdate_WhenDownloadTaskWorkerHasAnCreateDownloadFileStreamError()
    {
        //Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 5;
            config.MovieDownloadTasksCount = 1;
        });
        var downloadTaskUpdates = new List<DownloadTask>();

        // Capture the update in the callback
        mock.SetupMediator(It.IsAny<DownloadTaskUpdated>).Returns(Task.CompletedTask);
        mock.Mock<IDownloadManagerSettingsModule>().SetupGet(x => x.DownloadSegments).Returns(4);
        mock.Mock<IServerSettingsModule>().Setup(x => x.GetDownloadSpeedLimit(It.IsAny<string>())).Returns(4000);
        mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimitObservable(It.IsAny<string>()))
            .Returns(new Subject<int>().AsObservable());
        mock.Mock<IDownloadFileStream>()
            .Setup(x => x.CreateDownloadFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .Returns(Result.Fail("Error"));

        var downloadTask = DbContext.DownloadTasks.Include(x => x.PlexServer).First(x => x.DownloadTaskType == DownloadTaskType.Movie);
        downloadTask.DownloadWorkerTasks = new List<DownloadWorkerTask>()
        {
            new(downloadTask, 0, 0, 10),
            new(downloadTask, 1, 11, 20),
            new(downloadTask, 2, 21, 30),
            new(downloadTask, 3, 31, 40),
        };

        // Act
        _sut.Setup(downloadTask);
        _sut.Start();
        await _sut.DownloadProcessTask;

        // Assert
        downloadTaskUpdates.Count.ShouldBe(1);
        downloadTaskUpdates.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Error);
    }
}