using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaDTOMapper
{
    #region PlexMovie

    public static PlexMediaDTO ToDTO(this PlexMovie source) =>
        new()
        {
            Id = source.Id,
            TvShowId = default,
            TvShowSeasonId = default,
            MediaData = source.MovieData.ToDTO(),
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            SortIndex = source.SortIndex,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            GrandChildCount = 0,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = [],
            MetaDataKey = source.MetaDataKey,
            PlexToken = string.Empty,
        };

    #endregion

    #region PlexTvShow

    public static PlexMediaDTO ToDTO(this PlexTvShow plexTvShow)
    {
        var dto = plexTvShow.ToDTOMapper();
        dto.Children = [];

        foreach (var tvShowSeason in plexTvShow.Seasons)
            dto.Children.Add(tvShowSeason.ToDTO());

        return dto;
    }

    private static PlexMediaDTO ToDTOMapper(this PlexTvShow source) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.Id,
            TvShowSeasonId = default,
            MediaData = [],
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            SortIndex = source.SortIndex,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            GrandChildCount = source.GrandChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Seasons.ConvertAll(ToDTO),
            MetaDataKey = source.MetaDataKey,
            PlexToken = string.Empty,
        };

    #endregion

    #region PlexSeason

    public static PlexMediaDTO ToDTO(this PlexTvShowSeason plexTvShowSeason)
    {
        var dto = plexTvShowSeason.ToDTOMapper();
        dto.Children = [];

        foreach (var episode in plexTvShowSeason.Episodes)
            dto.Children.Add(episode.ToDTO());

        return dto;
    }

    private static PlexMediaDTO ToDTOMapper(this PlexTvShowSeason source) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.TvShowId,
            TvShowSeasonId = source.Id,
            MediaData = [],
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            SortIndex = source.SortIndex,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            GrandChildCount = 0,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Episodes.ConvertAll(ToDTO),
            MetaDataKey = source.MetaDataKey,
            PlexToken = string.Empty,
        };

    #endregion

    #region PlexEpisode

    public static PlexMediaDTO ToDTO(this PlexTvShowEpisode source) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.TvShowId,
            TvShowSeasonId = source.TvShowSeasonId,
            MediaData = source.EpisodeData.ToDTO(),
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            SortIndex = source.SortIndex,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            GrandChildCount = 0,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = [],
            MetaDataKey = source.MetaDataKey,
            PlexToken = string.Empty,
        };

    #endregion
}
