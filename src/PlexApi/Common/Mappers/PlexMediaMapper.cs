namespace PlexRipper.PlexApi;

public static class PlexMediaMapper
{
    public static PlexMovie ToPlexMovie(this PlexMedia source) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortTitle = source.SortTitle,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullThumbUrl = source.FullThumbUrl,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = source.FullTitle,
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            Type = source.Type,
        };

    public static PlexTvShow ToPlexTvShow(this PlexMedia source) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortTitle = source.SortTitle,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullThumbUrl = source.FullThumbUrl,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = source.FullTitle,
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            Type = source.Type,
        };

    public static PlexTvShowSeason ToPlexTvShowSeason(this PlexMedia source) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortTitle = source.SortTitle,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullThumbUrl = source.FullThumbUrl,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = source.FullTitle,
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            Type = source.Type,
        };

    public static PlexTvShowEpisode ToPlexTvShowEpisode(this PlexMedia source) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortTitle = source.SortTitle,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullThumbUrl = source.FullThumbUrl,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = source.FullTitle,
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            Type = source.Type,
        };
}
