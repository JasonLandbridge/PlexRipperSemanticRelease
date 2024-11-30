using System.Reactive.Linq;
using System.Reactive.Subjects;
using Application.Contracts;
using ByteSizeLib;
using FileSystem.Contracts;

namespace PlexRipper.Application.UnitTests;

public class MergeFilesFromFileTaskCommandUnitTests : BaseUnitTest<MergeFilesFromFileTaskCommandHandler>
{
    public MergeFilesFromFileTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnInvalidValidation_WhenFilePathsCompressedIsEmpty()
    {
        // Arrange
        var fileTask = FakeData.GetFileTask(2352, filePathsCount: 0).Generate();
        var progress = new Subject<FileMergeProgress>();

        var command = new MergeFilesFromFileTaskCommand(fileTask, progress);

        // Act
        var validate = await new MergeFilesFromFileTaskCommandValidator().ValidateAsync(command);

        // Assert
        validate.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDestinationDirectoryCreationFails()
    {
        // Arrange
        var fileTask = FakeData.GetFileTask(2352).Generate();
        var progress = new Subject<FileMergeProgress>();

        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Fail("Failed to create directory"));

        var command = new MergeFilesFromFileTaskCommand(fileTask, progress);

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
        var fileTask = FakeData.GetFileTask(2352).Generate();
        var progress = new Subject<FileMergeProgress>();
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

        var command = new MergeFilesFromFileTaskCommand(fileTask, progress);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldDeleteAllFilesAndMarkFileTaskCompleted_WhenItIsDoneMerging()
    {
        // Arrange
        var fileSizeInMb = 10;
        var fileTask = FakeData.GetFileTask(2352).Generate();
        var progress = new Subject<FileMergeProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var progressList = new List<FileMergeProgress>();
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
            .Verifiable(Times.Exactly(fileTask.FilePaths.Count));
        mock.Mock<IFileSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(fileTask.FilePaths.Count));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        // Act
        var command = new MergeFilesFromFileTaskCommand(fileTask, progress);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = result.Value;
        fileTaskPaused.CurrentFilePathIndex.ShouldBe(3);
        fileTaskPaused.CurrentBytesOffset.ShouldBe(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }

    [Fact]
    public async Task ShouldBePauseTheFileTask_WhenCancellationTokenIsCalled()
    {
        // Arrange
        var fileSizeInMb = 10;
        var fileTask = FakeData.GetFileTask(2352).Generate();
        var progress = new Subject<FileMergeProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<FileMergeProgress>();
        progress
            .AsObservable()
            .Subscribe(x =>
            {
                progressList.Add(x);

                // Wait until 1 file has been merged
                if (
                    x.DataTransferred > (long)ByteSize.FromMebiBytes(fileSizeInMb + 2).Bytes
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
        var command = new MergeFilesFromFileTaskCommand(fileTask, progress);
        var result = await _sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = result.Value;
        fileTaskPaused.CurrentFilePathIndex.ShouldBe(1);
        fileTaskPaused.CurrentBytesOffset.ShouldBeGreaterThan(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }

    [Fact]
    public async Task ShouldBeAbleToResumeAndFinish_WhenPreviousFileTaskHasBeenPaused()
    {
        // Arrange
        var fileSizeInMb = 10;
        var fileTask = FakeData.GetFileTask(2352).Generate();
        fileTask.CurrentFilePathIndex = 2;
        fileTask.CurrentBytesOffset = 2348;

        var progress = new Subject<FileMergeProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<FileMergeProgress>();
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
        var command = new MergeFilesFromFileTaskCommand(fileTask, progress);
        var result = await _sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = result.Value;
        fileTaskPaused.CurrentFilePathIndex.ShouldBe(3);
        fileTaskPaused.CurrentBytesOffset.ShouldBe(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }
}
