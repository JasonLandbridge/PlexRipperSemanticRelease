namespace PlexRipper.Domain;

public record DownloadWorkerTaskProgress : IDownloadTaskProgress
{
    public long DataTotal { get; set; }

    public decimal Percentage { get; set; }

    public long DataReceived { get; set; }

    public long DownloadSpeed { get; set; }
}
