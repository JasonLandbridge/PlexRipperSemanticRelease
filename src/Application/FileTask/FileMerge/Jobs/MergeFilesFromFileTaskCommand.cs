using System.Reactive.Subjects;
using Application.Contracts;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

public record MergeFilesFromFileTaskCommand(FileTask FileTask, Subject<FileMergeProgress> FileMergeProgress)
    : IRequest<Result<FileTask>>;

public class MergeFilesFromFileTaskCommandValidator : AbstractValidator<MergeFilesFromFileTaskCommand>
{
    public MergeFilesFromFileTaskCommandValidator()
    {
        RuleFor(x => x.FileTask.FilePathsCompressed).NotEmpty();
        RuleFor(x => x.FileTask.FilePaths.Count).GreaterThan(0);
        RuleFor(x => x.FileTask.DestinationFilePath).NotEmpty();
        RuleFor(x => x.FileTask.FileName).NotEmpty();
    }
}

public class MergeFilesFromFileTaskCommandHandler : IRequestHandler<MergeFilesFromFileTaskCommand, Result<FileTask>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IFileSystem _fileSystem;
    private readonly IDirectorySystem _directorySystem;
    private Stream? _readStream;
    private Stream? _writeStream;

    private const int _bufferSize = 524288;

    public MergeFilesFromFileTaskCommandHandler(
        ILog log,
        IMediator mediator,
        IFileSystem fileSystem,
        IDirectorySystem directorySystem
    )
    {
        _log = log;
        _mediator = mediator;
        _fileSystem = fileSystem;
        _directorySystem = directorySystem;
    }

    public async Task<Result<FileTask>> Handle(
        MergeFilesFromFileTaskCommand command,
        CancellationToken cancellationToken
    )
    {
        var fileTask = command.FileTask;
        var fileMergeProgress = command.FileMergeProgress;
        var transferStarted = DateTime.UtcNow;

        try
        {
            _log.Here()
                .Debug(
                    "Starting file merge process for {FilePathsCount} parts into a file {FileName}",
                    fileTask.FilePaths.Count,
                    fileTask.FileName
                );

            var writeStreamResult = await OpenOrCreateMergeStream(fileTask.DestinationFilePath);
            if (writeStreamResult.IsFailed)
            {
                return writeStreamResult.ToResult();
            }

            _writeStream = writeStreamResult.Value;

            // Resume the file merge if it was previously interrupted
            if (fileTask.CurrentBytesOffset > 0)
            {
                _writeStream.Seek(fileTask.CurrentBytesOffset, SeekOrigin.Begin);
            }

            long totalRead = 0;

            for (var index = fileTask.CurrentFilePathIndex; index < fileTask.FilePaths.Count; index++)
            {
                var filePath = fileTask.FilePaths[index];

                var inputStreamResult = _fileSystem.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (inputStreamResult.IsFailed)
                {
                    return inputStreamResult.ToResult();
                }

                cancellationToken.ThrowIfCancellationRequested();

                _readStream = inputStreamResult.Value;

                fileTask.CurrentFilePathIndex = index;
                fileTask.CurrentBytesOffset = 0;

                var buffer = new byte[_bufferSize];
                int bytesRead;
                while ((bytesRead = await _readStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
                {
                    await _writeStream.WriteAsync(buffer, 0, bytesRead, CancellationToken.None);

                    fileTask.CurrentBytesOffset += bytesRead;
                    totalRead += bytesRead;
                    SendProgress(totalRead);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                _readStream.Close();

                _log.Debug(
                    "The file at {FilePath} has been merged into the single media file at {DestinationPath}",
                    filePath,
                    fileTask.DestinationFilePath,
                    0
                );
                _log.Debug("Deleting file {FilePath} since it has been merged already", filePath);
                var deleteResult = _fileSystem.DeleteFile(filePath);
                if (deleteResult.IsFailed)
                {
                    await _mediator.SendNotificationAsync(deleteResult);
                    deleteResult.LogError();
                }
            }

            fileTask.CurrentBytesOffset = 0;
            SendProgress(totalRead);
        }
        catch (OperationCanceledException)
        {
            _log.Warning("The file merge operation was cancelled for file task {FileTaskId}", fileTask.Id);
            return Result.Ok(fileTask);
        }
        catch (Exception ex)
        {
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

            fileMergeProgress.OnCompleted();
        }

        return Result.Ok(fileTask);

        void SendProgress(long dataTransferred)
        {
            var elapsedTime = DateTime.UtcNow.Subtract(transferStarted);
            fileMergeProgress.OnNext(
                new FileMergeProgress
                {
                    Id = fileTask.Id,
                    DownloadTaskId = fileTask.DownloadTaskId,
                    DownloadTaskType = fileTask.DownloadTaskType,
                    CurrentFilePathIndex = fileTask.CurrentFilePathIndex,
                    DataTransferred = dataTransferred,
                    DataTotal = fileTask.FileSize,
                    PlexLibraryId = fileTask.DownloadTaskKey.PlexLibraryId,
                    PlexServerId = fileTask.DownloadTaskKey.PlexServerId,
                    TransferSpeed = DataFormat.GetTransferSpeed(dataTransferred, elapsedTime.TotalSeconds),
                }
            );
        }
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
}
