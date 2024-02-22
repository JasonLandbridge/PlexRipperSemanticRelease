﻿using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace Data.UnitTests.Commands;

public class CreateDownloadTasksCommandHandler_UnitTests : BaseUnitTest
{
    public CreateDownloadTasksCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldCreateAllDownloadTasks_WhenAllAreNew()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasks = FakeData.GetTvShowDownloadTask().Generate(1);
        var request = new CreateDownloadTasksCommand(downloadTasks);
        var handler = new CreateDownloadTasksCommandHandler(_log, GetDbContext());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        DbContext.DownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
    }

    [Fact]
    public async Task ShouldCreateOnlyChildDownloadTasks_WhenParentAlreadyExists()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasks = FakeData.GetTvShowDownloadTask().Generate(1);
        await DbContext.BulkInsertAsync(new List<DownloadTask> { downloadTasks.First() });
        downloadTasks[0].Id = 1;
        var request = new CreateDownloadTasksCommand(downloadTasks);
        var handler = new CreateDownloadTasksCommandHandler(_log, GetDbContext());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var flattenDownloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        DbContext.DownloadTasks.Count().ShouldBe(flattenDownloadTasks.Count);
    }

    [Fact]
    public async Task ShouldAllHaveARootDownloadTaskId_WhenDownloadTasksAreChildren()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);
        var downloadTasksTest = FakeData.GetTvShowDownloadTask().Generate(1);
        var request = new CreateDownloadTasksCommand(downloadTasksTest);
        var handler = new CreateDownloadTasksCommandHandler(_log, GetDbContext());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTasksDb = await DbContext.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();

        void HasRootDownloadTaskId(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.RootDownloadTaskId.ShouldBe(1);
                if (downloadTask.Children.Any())
                    HasRootDownloadTaskId(downloadTask.Children);
            }
        }

        HasRootDownloadTaskId(downloadTasksDb.SelectMany(x => x.Children).ToList());
    }
}