using System.Diagnostics;
using System.Reactive.Subjects;
using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Notifications;

namespace PlexRipper.Application;

public record MergeFilesFromFileTaskCommand(
    DownloadTaskKey Key,
    Subject<IDownloadFileTransferProgress>? FileMergeProgress = null
) : IRequest<Result>;

public class MergeFilesFromFileTaskCommandValidator : AbstractValidator<MergeFilesFromFileTaskCommand>
{
    public MergeFilesFromFileTaskCommandValidator()
    {
        RuleFor(x => x.Key).NotNull();
        RuleFor(x => x.Key.Id).NotEqual(Guid.Empty);
    }
}

public class MergeFilesFromFileTaskCommandHandler : IRequestHandler<MergeFilesFromFileTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IFileSystem _fileSystem;
    private readonly IDirectorySystem _directorySystem;

    private Stream? _readStream;
    private Stream? _writeStream;

    /// <summary>
    /// Based on https://github.com/dotnet/runtime/discussions/74405#discussioncomment-3488674
    /// 1048576 bytes = 1 MB
    /// </summary>
    private const int _bufferSize = 1048576;

    public MergeFilesFromFileTaskCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        IFileSystem fileSystem,
        IDirectorySystem directorySystem
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _fileSystem = fileSystem;
        _directorySystem = directorySystem;
    }

    public async Task<Result> Handle(MergeFilesFromFileTaskCommand command, CancellationToken cancellationToken)
    {
        var key = command.Key;
        var fileMergeProgress = command.FileMergeProgress;

        var downloadTask = await _dbContext.GetDownloadTaskFileAsync(command.Key, CancellationToken.None);
        if (downloadTask == null)
        {
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.Key.Id).LogError();
        }

        var sourceFilePaths = downloadTask.FilePaths;
        _log.Here()
            .Debug(
                "Starting file merge process for {FilePathsCount} parts into a file {FileName}",
                sourceFilePaths.Count,
                downloadTask.FileName
            );

        try
        {
            var writeStreamResult = await OpenOrCreateMergeStream(downloadTask.DestinationFilePath);
            if (writeStreamResult.IsFailed)
            {
                downloadTask.DownloadStatus = downloadTask.IsSingleFile
                    ? DownloadStatus.MoveError
                    : DownloadStatus.MergeError;

                return writeStreamResult.ToResult();
            }

            _writeStream = writeStreamResult.Value;

            // Resume the file merge if it was previously interrupted
            if (downloadTask.CurrentFileTransferBytesOffset > 0)
            {
                _writeStream.Seek(downloadTask.CurrentFileTransferBytesOffset, SeekOrigin.Begin);
            }

            // Update download task status
            await UpdateDownloadTaskStatus(
                key,
                downloadTask.IsSingleFile ? DownloadStatus.Moving : DownloadStatus.Merging
            );

            var stopwatch = Stopwatch.StartNew(); // Start timing for speed calculation
            var previousDataTransferred = downloadTask.FileDataTransferred;

            for (var index = downloadTask.CurrentFileTransferPathIndex; index < sourceFilePaths.Count; index++)
            {
                var filePath = sourceFilePaths[index];

                if (!_fileSystem.FileExists(filePath))
                {
                    var result = Result
                        .Fail($"Filepath: {filePath} does not exist and cannot be used to merge/move the file!")
                        .LogError();

                    await UpdateDownloadTaskStatus(
                        key,
                        downloadTask.IsSingleFile ? DownloadStatus.MoveError : DownloadStatus.MergeError
                    );

                    await _mediator.SendNotificationAsync(result);
                    return result;
                }

                var inputStreamResult = _fileSystem.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (inputStreamResult.IsFailed)
                {
                    return inputStreamResult.ToResult();
                }

                _readStream = inputStreamResult.Value;

                if (downloadTask.CurrentFileTransferBytesOffset > 0)
                {
                    _readStream.Seek(downloadTask.CurrentFileTransferBytesOffset, SeekOrigin.Begin);
                }

                downloadTask.CurrentFileTransferPathIndex = index;
                downloadTask.CurrentFileTransferBytesOffset = 0;

                cancellationToken.ThrowIfCancellationRequested();

                var buffer = new byte[_bufferSize];
                int bytesRead;
                while ((bytesRead = await _readStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
                {
                    await _writeStream.WriteAsync(buffer, 0, bytesRead, CancellationToken.None);

                    downloadTask.CurrentFileTransferBytesOffset += bytesRead;

                    downloadTask.FileDataTransferred += bytesRead;
                    previousDataTransferred += bytesRead;

                    downloadTask.FileTransferSpeed = DataFormat.GetTransferSpeed(
                        downloadTask.FileDataTransferred - previousDataTransferred,
                        stopwatch.Elapsed.TotalSeconds
                    );

                    // Send progress
                    var progress = new DownloadFileTransferProgress
                    {
                        FileTransferSpeed = downloadTask.FileTransferSpeed,
                        FileDataTransferred = downloadTask.FileDataTransferred,
                        CurrentFileTransferPathIndex = downloadTask.CurrentFileTransferPathIndex,
                        CurrentFileTransferBytesOffset = downloadTask.CurrentFileTransferBytesOffset,
                    };

                    fileMergeProgress?.OnNext(progress);

                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        await _dbContext.UpdateDownloadFileTransferProgress(key, progress);
                        await _mediator.Send(new DownloadTaskUpdatedNotification(key), CancellationToken.None);

                        stopwatch.Restart();
                        previousDataTransferred = 0;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                _log.Here()
                    .Debug(
                        "The file at {FilePath} has been merged into the single media file at {DestinationPath}",
                        filePath,
                        downloadTask.DestinationDirectory
                    );

                // Important: Reset the offset between files otherwise it skips parts of the file
                downloadTask.CurrentFileTransferBytesOffset = 0;

                _log.Debug("Deleting file {FilePath} since it has been merged already", filePath);
                await _readStream.DisposeAsync();
                _readStream = null;
                var deleteResult = _fileSystem.DeleteFile(filePath);
                if (deleteResult.IsFailed)
                {
                    await _mediator.SendNotificationAsync(deleteResult);
                    deleteResult.LogError();
                }
            }

            // Important: Reset the offset between files otherwise it skips parts of the file
            downloadTask.CurrentFileTransferBytesOffset = 0;

            _log.Here()
                .Information(
                    "Finished combining {FilePathsCount} files into {FileTaskFileName}",
                    downloadTask.FilePaths.Count,
                    downloadTask.FileName
                );

            downloadTask.DownloadStatus = downloadTask.IsSingleFile
                ? DownloadStatus.MoveFinished
                : DownloadStatus.MergeFinished;
        }
        catch (OperationCanceledException)
        {
            _log.Warning("The file merge operation was cancelled for file task {FileTaskId}", key.Id);

            downloadTask.DownloadStatus = downloadTask.IsSingleFile
                ? DownloadStatus.MovePaused
                : DownloadStatus.MergePaused;

            return Result.Ok();
        }
        catch (Exception ex)
        {
            downloadTask.DownloadStatus = downloadTask.IsSingleFile
                ? DownloadStatus.MoveError
                : DownloadStatus.MergeError;

            return _log.Error(ex).ToResult();
        }
        finally
        {
            if (_readStream != null)
            {
                await _readStream.DisposeAsync();
                _readStream = null;
            }

            if (_writeStream != null)
            {
                await _writeStream.DisposeAsync();
                _writeStream = null;
            }

            var progress = new DownloadFileTransferProgress
            {
                FileTransferSpeed = downloadTask.FileTransferSpeed,
                FileDataTransferred = downloadTask.FileDataTransferred,
                CurrentFileTransferPathIndex = downloadTask.CurrentFileTransferPathIndex,
                CurrentFileTransferBytesOffset = downloadTask.CurrentFileTransferBytesOffset,
            };
            await _dbContext.UpdateDownloadFileTransferProgress(key, progress);

            await UpdateDownloadTaskStatus(key, downloadTask.DownloadStatus);

            fileMergeProgress?.OnNext(progress);

            fileMergeProgress?.OnCompleted();

            if (downloadTask.DownloadStatus is DownloadStatus.MoveFinished or DownloadStatus.MergeFinished)
            {
                await _mediator.Publish(new FileMergeFinishedNotification(key), CancellationToken.None);
            }
        }

        return Result.Ok();
    }

    private async Task<Result<Stream>> OpenOrCreateMergeStream(string fileDestinationPath)
    {
        // Ensure destination directory exists and is otherwise created.
        var result = _directorySystem.CreateDirectoryFromFilePath(fileDestinationPath);
        if (result.IsFailed)
        {
            await _mediator.SendNotificationAsync(result);
            return result.LogError();
        }

        return _fileSystem.Create(fileDestinationPath, _bufferSize, FileOptions.SequentialScan);
    }

    private async Task UpdateDownloadTaskStatus(DownloadTaskKey downloadTaskKey, DownloadStatus newDownloadStatus)
    {
        await _dbContext.SetDownloadStatus(downloadTaskKey, newDownloadStatus);
        await _dbContext
            .DownloadWorkerTasks.Where(x => x.DownloadTaskId == downloadTaskKey.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, newDownloadStatus));

        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTaskKey));
    }
}
