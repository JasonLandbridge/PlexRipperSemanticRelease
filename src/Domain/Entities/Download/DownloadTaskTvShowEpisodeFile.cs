namespace PlexRipper.Domain;

public class DownloadTaskTvShowEpisodeFile : DownloadTaskFileBase
{
    #region Relationships

    public required DownloadTaskTvShowEpisode Parent { get; set; }

    public required Guid ParentId { get; set; }

    #endregion

    #region Helpers

    public override PlexMediaType MediaType => PlexMediaType.Episode;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.EpisodeData;

    public override bool IsDownloadable => true;

    public override int Count => 1;

    #endregion
}