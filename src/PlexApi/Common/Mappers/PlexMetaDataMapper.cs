using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.PlexApi;

public static class PlexMetaDataMapper
{
    #region Single Conversions

    public static PlexMovie ToPlexMovie(this GetLibraryItemsMetadata source) =>
        source.ToPlexMedia().ToPlexMovie(source);

    public static PlexTvShow ToPlexTvShow(this GetLibraryItemsMetadata source) =>
        source.ToPlexMedia().ToPlexTvShow(source);

    public static PlexTvShowSeason ToPlexTvShowSeason(this GetLibraryItemsMetadata source) =>
        source.ToPlexMedia().ToPlexTvShowSeason(source);

    public static PlexTvShowEpisode ToPlexTvShowEpisode(this GetLibraryItemsMetadata source) =>
        source.ToPlexMedia().ToPlexTvShowEpisode(source);

    public static PlexMedia ToPlexMedia(this GetLibraryItemsMetadata source)
    {
        return new PlexMedia
        {
            Id = 0,
            Title = source.Title,
            Year = source.Year ?? 0,

            // This is set later on
            SortIndex = 0,

            SearchTitle = source.Title.ToSearchTitle(),
            Guid = source.Guid,

            Guid_IMDB = source.MediaGuid?.Find(x => x.Id.Contains("imdb"))?.Id.Replace("imdb://", "") ?? null,
            Guid_TMDB = source.MediaGuid?.Find(x => x.Id.Contains("tmdb"))?.Id.Replace("tmdb://", "") ?? null,
            Guid_TVDB = source.MediaGuid?.Find(x => x.Id.Contains("tvdb"))?.Id.Replace("tvdb://", "") ?? null,

            // Duration is in milliseconds and we want seconds
            Duration = source.Duration.GetValueOrDefault() / 1000,
            MediaSize = source.Media?.Sum(y => y.Part.Sum(z => z.Size)) ?? 0,
            ChildCount = source.ChildCount ?? 0,
            AddedAt = DateTimeExtensions.FromUnixTime(source.AddedAt),
            UpdatedAt = DateTimeExtensions.FromUnixTime(source.UpdatedAt),
            MediaData = new PlexMediaContainer() { MediaData = source.Media?.ToPlexMediaData() ?? [] },

            Type = PlexMediaType.None,
            Key = int.Parse(source.RatingKey),
            MetaDataKey = RetrieveMetaDataKey(source),
            Studio = source.Studio ?? string.Empty,
            Summary = source.Summary,
            ContentRating = source.ContentRating ?? string.Empty,
            Rating = source.Rating ?? 0,
            OriginallyAvailableAt = source.OriginallyAvailableAt.ToDateTime(),
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            HasArt = !string.IsNullOrEmpty(source.Art),
            HasBanner = !string.IsNullOrEmpty(source.Banner),
            HasTheme = !string.IsNullOrEmpty(source.Theme),

            // Ignore the following
            FullTitle = string.Empty,
            PlexLibrary = default,
            PlexServer = default,
            PlexLibraryId = default,
            PlexServerId = default,
            FullThumbUrl = string.Empty,
            FullBannerUrl = string.Empty,
        };
    }

    #endregion

    #region List Conversions

    public static List<PlexMovie> ToPlexMovies(this List<GetLibraryItemsMetadata> source) =>
        source.Select(value => value.ToPlexMovie()).ToList();

    public static List<PlexTvShow> ToPlexTvShows(this List<GetLibraryItemsMetadata> source) =>
        source.Select(value => value.ToPlexTvShow()).ToList();

    public static List<PlexTvShowSeason> ToPlexTvShowSeasons(this List<GetLibraryItemsMetadata> source) =>
        source.Select(value => value.ToPlexTvShowSeason()).ToList();

    public static List<PlexTvShowEpisode> ToPlexTvShowEpisodes(this List<GetLibraryItemsMetadata> source) =>
        source.Select(value => value.ToPlexTvShowEpisode()).ToList();

    #endregion

    /// <summary>
    /// Retrieves the MetaDataKey from either the ThumbUrl,BannerUrl, ArtUrl or ThemeUrl.
    /// It is assumed that all MetaDataKeys are the same, returns 0 if nothing is found.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    private static int RetrieveMetaDataKey(GetLibraryItemsMetadata metadata)
    {
        List<string> list = [metadata.Thumb ?? "", metadata.Art ?? "", metadata.Theme ?? ""];

        foreach (var entry in list)
            if (!string.IsNullOrEmpty(entry))
            {
                // We want the last number
                // Example: /library/metadata/457047/thumb/1587006741
                var splitStrings = entry.Split('/').ToList();
                if (splitStrings.Count > 2)
                {
                    if (int.TryParse(splitStrings.Last(), out var result))
                        return result;
                }
            }

        return 0;
    }
}
