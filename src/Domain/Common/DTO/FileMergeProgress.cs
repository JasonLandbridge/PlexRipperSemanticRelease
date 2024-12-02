namespace PlexRipper.Domain;

/// <summary>
/// This is used to track the progress of a <see cref="FileTask"/>.
/// </summary>
public record FileMergeProgress
{
    /// <summary>
    /// This is equal to the <see cref="FileTask"/> Id.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// This is equal to the <see cref="DownloadTaskGeneric"/> Id the <see cref="FileTask"/> is currently handling.
    /// </summary>
    public required Guid DownloadTaskId { get; init; }

    public required DownloadTaskType DownloadTaskType { get; init; }

    public required long DataTransferred { get; init; }

    public required long DataTotal { get; init; }

    /// <summary>
    /// The transfer speed in bytes per second.
    /// </summary>
    public required int TransferSpeed { get; init; }

    public required int CurrentFilePathIndex { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="PlexServer"/> Id the <see cref="FileTask"/> is currently handling.
    /// Note: This is needed in the front-end to update the correct DownloadTask.
    /// </summary>
    public required int PlexServerId { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="PlexLibrary"/> Id the <see cref="FileTask"/> is currently handling.
    /// Note: This is needed in the front-end to update the correct DownloadTask.
    /// </summary>
    public required int PlexLibraryId { get; init; }

    public decimal Percentage => DataFormat.GetPercentage(DataTransferred, DataTotal);

    public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, TransferSpeed);

    public long BytesRemaining => DataTotal - DataTransferred;

    public DownloadTaskKey ToKey() =>
        new()
        {
            Type = DownloadTaskType,
            Id = DownloadTaskId,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };

    public override string ToString() =>
        $"[FileMergeProgress {DownloadTaskId} - {Percentage}% - {DataFormat.FormatSpeedString(TransferSpeed)} - {DataFormat.FormatSizeString(BytesRemaining)} / {DataFormat.FormatSizeString(DataTotal)} - {DataFormat.FormatTimeSpanString(TimeSpan.FromSeconds(TimeRemaining))}]";
}
