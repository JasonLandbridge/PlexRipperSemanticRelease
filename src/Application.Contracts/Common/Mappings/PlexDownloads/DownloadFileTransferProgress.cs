using PlexRipper.Domain;

namespace Application.Contracts;

public record DownloadFileTransferProgress : IDownloadFileTransferProgress
{
    public required long FileTransferSpeed { get; set; }

    public required long FileDataTransferred { get; set; }

    public required int CurrentFileTransferPathIndex { get; set; }

    public required long CurrentFileTransferBytesOffset { get; set; }
}
