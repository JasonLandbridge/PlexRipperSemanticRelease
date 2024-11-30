using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskFileBase : DownloadTaskBase, IDownloadFileTransferProgress
{
    [Column(Order = 11)]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the relative obfuscated URL of the media to be downloaded,
    /// e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    [Column(Order = 12)]
    public required string FileLocationUrl { get; init; }

    /// <summary>
    /// Gets or sets get or sets the media quality of this <see cref="DownloadTaskGeneric"/>.
    /// </summary>
    [Column(Order = 15)]
    public required string Quality { get; init; }

    [Column(Order = 16)]
    public required DownloadTaskDirectory DirectoryMeta { get; init; }

    #region Download Progress

    /// <summary>
    /// Gets or sets the percentage of the data received from the DataTotal.
    /// </summary>
    [Column(Order = 4)]
    public required decimal DownloadPercentage { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    [Column(Order = 5)]
    public required long DataReceived { get; set; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// </summary>
    [Column(Order = 6)]
    public required long DataTotal { get; set; }

    /// <summary>
    /// Gets or sets get the download speeds in bytes per second.
    /// </summary>
    [Column(Order = 18)]
    public required long DownloadSpeed { get; set; }

    #endregion

    #region File Transfer Progress

    /// <summary>
    /// Gets or sets the percentage of the data received from the DataTotal.
    /// </summary>
    [Column(Order = 4)]
    public required decimal FileTransferPercentage { get; set; }

    /// <summary>
    /// Gets or sets the file transfer speeds when the finished download is being merged/moved.
    /// </summary>
    [Column(Order = 19)]
    public required long FileTransferSpeed { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    [Column(Order = 5)]
    public required long FileDataTransferred { get; set; }

    public int CurrentFileTransferPathIndex { get; set; }

    public long CurrentFileTransferBytesOffset { get; set; }

    #endregion

    #region Relationships

    public required List<DownloadWorkerTask> DownloadWorkerTasks { get; set; } = [];

    #endregion

    #region Helpers

    public override PlexMediaType MediaType => PlexMediaType.None;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.None;

    public string DestinationFilePath => Path.Join(DestinationDirectory, FileName);

    public List<string> FilePaths => DownloadWorkerTasks.Select(x => x.DownloadFilePath).ToList();

    /// <summary>
    /// Gets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    [NotMapped]
    public string DownloadDirectory
    {
        get
        {
            if (DirectoryMeta.DownloadRootPath == string.Empty)
                return string.Empty;

            switch (DownloadTaskType)
            {
                case DownloadTaskType.MovieData:
                    return Path.Combine(DirectoryMeta.DownloadRootPath, "Movies", DirectoryMeta.MovieFolder);
                case DownloadTaskType.EpisodeData:
                    return Path.Combine(
                        DirectoryMeta.DownloadRootPath,
                        "TvShows",
                        DirectoryMeta.TvShowFolder,
                        DirectoryMeta.SeasonFolder
                    );
                default:
                    Result.Fail<string>($"Invalid DownloadTaskType of type: {DownloadTaskType}").LogError();
                    return string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
    /// </summary>
    [NotMapped]
    public string DestinationDirectory
    {
        get
        {
            if (DirectoryMeta.DestinationRootPath == string.Empty)
                return string.Empty;

            switch (DownloadTaskType)
            {
                case DownloadTaskType.MovieData:
                    return Path.Combine(DirectoryMeta.DestinationRootPath, DirectoryMeta.MovieFolder);
                case DownloadTaskType.EpisodeData:
                    return Path.Combine(
                        DirectoryMeta.DestinationRootPath,
                        DirectoryMeta.TvShowFolder,
                        DirectoryMeta.SeasonFolder
                    );
                default:
                    Result.Fail<string>($"Invalid DownloadTaskType of type: {DownloadTaskType}").LogError();
                    return string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets a joined string of temp file paths of the <see cref="DownloadWorkerTasks"/> delimited by ";".
    /// </summary>
    [NotMapped]
    public string GetFilePathsCompressed =>
        string.Join(';', DownloadWorkerTasks.Select(x => x.DownloadFilePath).ToArray());

    [NotMapped]
    public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

    [NotMapped]
    public long TimeRemaining => DataFormat.GetTimeRemaining(DataTotal - DataReceived, DownloadSpeed);

    [NotMapped]
    public bool IsSingleFile => DownloadWorkerTasks.Count == 1;

    #endregion
}
