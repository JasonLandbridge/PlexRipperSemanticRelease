using System.Reactive.Linq;
using System.Reactive.Subjects;
using Application.Contracts;
using ByteSizeLib;
using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class MergeFilesFromFileTaskCommandUnitTests : BaseUnitTest<MergeFilesFromFileTaskCommandHandler>
{
    public MergeFilesFromFileTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnInvalidValidation_WhenFilePathsCompressedIsEmpty()
    {
        // Arrange

        await SetupDatabase(
            2352,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var key = await IDbContext.DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey()).FirstOrDefaultAsync();
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        var command = new MergeFilesFromFileTaskCommand(key, progress);

        // Act
        var validate = await new MergeFilesFromFileTaskCommandValidator().ValidateAsync(command);

        // Assert
        validate.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDestinationDirectoryCreationFails()
    {
        // Arrange
        await SetupDatabase(
            23522,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var key = await IDbContext.DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey()).FirstOrDefaultAsync();
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Fail("Failed to create directory"));

        var command = new MergeFilesFromFileTaskCommand(key, progress);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to create directory");
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenFileOpenFails()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var key = await IDbContext.DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey()).FirstOrDefaultAsync();
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();
        var writeStream = new MemoryStream();

        mock.Mock<IFileSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(Result.Fail("Failed to open file"));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Fail(new Error("")))
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(1));

        var command = new MergeFilesFromFileTaskCommand(key, progress);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldDeleteAllFilesAndMarkFileTaskCompleted_WhenItIsDoneMerging()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var downloadFileTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync();
        downloadFileTask.ShouldNotBeNull();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        mock.Mock<IFileSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileStream(fileSizeInMb));
                return Result.Ok<Stream>(readStreams.Last());
            })
            .Verifiable(Times.Exactly(downloadFileTask.FilePaths.Count));
        mock.Mock<IFileSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(downloadFileTask.FilePaths.Count));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        // Act
        var command = new MergeFilesFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey());
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(3);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBe(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }

    [Fact]
    public async Task ShouldBePauseTheFileTask_WhenCancellationTokenIsCalled()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var downloadFileTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync();
        downloadFileTask.ShouldNotBeNull();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress
            .AsObservable()
            .Subscribe(x =>
            {
                progressList.Add(x);

                // Wait until 1 file has been merged
                if (
                    x.FileDataTransferred > (long)ByteSize.FromMebiBytes(fileSizeInMb + 2).Bytes
                    && !cancellationTokenSource.Token.IsCancellationRequested
                )
                {
                    cancellationTokenSource.CancelAsync();
                }
            });

        mock.Mock<IFileSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileStream(fileSizeInMb));
                return Result.Ok<Stream>(readStreams.Last());
            });
        mock.Mock<IFileSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        // Act
        var command = new MergeFilesFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(
            downloadFileTask.ToKey(),
            CancellationToken.None
        );
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(1);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThan(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }

    [Fact]
    public async Task ShouldBeAbleToResumeAndFinish_WhenPreviousFileTaskHasBeenPaused()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsTracking()
            .Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync();
        downloadFileTask.ShouldNotBeNull();

        downloadFileTask.CurrentFileTransferPathIndex = 2;
        downloadFileTask.CurrentFileTransferBytesOffset = 2348;
        await dbContext.SaveChangesAsync();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        mock.Mock<IFileSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileStream(fileSizeInMb));
                return Result.Ok<Stream>(readStreams.Last());
            });
        mock.Mock<IFileSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(2));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        // Act
        var command = new MergeFilesFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(
            downloadFileTask.ToKey(),
            CancellationToken.None
        );
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(3);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBe(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }
}
