namespace PlexRipper.BaseTests;

public static partial class DownloadTaskExtensions
{
    public static DownloadTaskGeneric SetDownloadStatus(
        this DownloadTaskGeneric downloadTask,
        DownloadStatus downloadStatus
    )
    {
        downloadTask.DownloadStatus = downloadStatus;
        if (downloadTask.Children.Any())
            downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);

        return downloadTask;
    }

    public static List<DownloadTaskGeneric> SetDownloadStatus(
        this List<DownloadTaskGeneric> downloadTasks,
        DownloadStatus downloadStatus
    )
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.DownloadStatus = downloadStatus;
            if (downloadTask.Children.Any())
                downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);
        }

        return downloadTasks;
    }

    /// <summary>
    /// The percentage is set based on the download status as to ensure <see cref="DownloadTaskPhase"/> is set correctly.
    /// </summary>
    /// <param name="downloadTask"></param>
    /// <param name="downloadStatus"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static void SetPercentageBasedOnStatus(
        this DownloadTaskFileBase downloadTask,
        DownloadStatus downloadStatus
    )
    {
        switch (downloadStatus)
        {
            case DownloadStatus.Queued:
            case DownloadStatus.Stopped:
                downloadTask.DownloadPercentage = 0;
                downloadTask.FileTransferPercentage = 0;
                break;
            case DownloadStatus.Downloading:
            case DownloadStatus.Paused:
            case DownloadStatus.Error:
            case DownloadStatus.ServerUnreachable:
                downloadTask.DownloadPercentage = 1;
                downloadTask.FileTransferPercentage = 0;
                break;
            case DownloadStatus.DownloadFinished:
            case DownloadStatus.Merging:
            case DownloadStatus.Moving:
            case DownloadStatus.MergePaused:
            case DownloadStatus.MovePaused:
            case DownloadStatus.MoveError:
            case DownloadStatus.MergeError:
                downloadTask.DownloadPercentage = 100;
                downloadTask.FileTransferPercentage = 0;
                break;
            case DownloadStatus.MergeFinished:
            case DownloadStatus.MoveFinished:
            case DownloadStatus.Completed:
                downloadTask.DownloadPercentage = 100;
                downloadTask.FileTransferPercentage = 100;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(downloadStatus), downloadStatus, null);
        }
    }
}
