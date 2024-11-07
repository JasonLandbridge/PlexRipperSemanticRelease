﻿using System.Reactive.Linq;
using Autofac;
using ByteSizeLib;
using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;
using PlexRipper.PlexApi;
using Settings.Contracts;

namespace PlexRipper.Application.UnitTests;

public class PlexDownloadClientStopAsyncUnitTests : BaseUnitTest<PlexDownloadClient>
{
    public PlexDownloadClientStopAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldEmitStopDownloadStatus_WhenDownloadClientIsStoppedSuccessfully()
    {
        //Arrange
        await SetupDatabase(
            82345,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        SetupHttpClient(x => x.SetupDownloadFile(100));
        var downloadSpeedLimit = 1000;
        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync();
        await dbContext.DownloadWorkerTasks.AddRangeAsync(downloadTask.GenerateDownloadWorkerTasks(1));
        await dbContext.SaveChangesAsync();

        // PlexDownloadClientMocks
        mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimit(It.IsAny<string>()))
            .Returns(downloadSpeedLimit)
            .Verifiable(Times.Once);

        mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimitObservable(It.IsAny<string>()))
            .Returns(Observable.Return(downloadSpeedLimit))
            .Verifiable(Times.Once);

        var updateList = new List<IDownloadTaskProgress>();
        var statusList = new List<DownloadStatus>();

        async Task AddDownloadTaskUpdateAsync(DownloadTaskUpdatedNotification notification)
        {
            var task = await IDbContext.GetDownloadTaskAsync(notification.Key);
            task.ShouldNotBeNull();
            updateList.Add(task);
            statusList.Add(task.DownloadStatus);
        }

        mock.Mock<IMediator>()
            .Setup(m => m.Send(It.IsAny<DownloadTaskUpdatedNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<DownloadTaskUpdatedNotification, CancellationToken>(
                (notification, _) => AddDownloadTaskUpdateAsync(notification).GetAwaiter().GetResult()
            )
            .Verifiable(Times.AtLeastOnce);

        // DownloadWorkerMocks
        var destinationStream = new MemoryStream();
        mock.Mock<IDownloadFileStream>()
            .Setup(x => x.CreateDownloadFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .Returns(Result.Ok<Stream>(destinationStream))
            .Verifiable(Times.Once);

        var downloadStream = new ThrottledStream(new MemoryStream(new byte[(int)ByteSize.FromMebiBytes(10).Bytes]));
        mock.Mock<IPlexApiClient>()
            .Setup(x =>
                x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(downloadStream)
            .Verifiable(Times.Once);

        // Act
        var sut = mock.Create<PlexDownloadClient>(
            new NamedParameter(
                "downloadWorkerFactory",
                (DownloadWorkerTask task) => mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", task))
            ),
            new NamedParameter(
                "clientFactory",
                (PlexApiClientOptions options) => mock.Create<PlexApiClient>(new NamedParameter("options", options))
            )
        );

        await sut.Setup(downloadTask.ToKey());

        var startResult = sut.Start();
        await Task.Delay(1500);
        var stopResult = await sut.StopAsync();

        // Wait for the process to complete
        await sut.DownloadProcessTask;

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        stopResult.IsSuccess.ShouldBeTrue();

        updateList.Count.ShouldBeGreaterThanOrEqualTo(2);
        statusList.Count.ShouldBeGreaterThanOrEqualTo(2);

        statusList.Last().ShouldBe(DownloadStatus.Stopped);
    }
}
