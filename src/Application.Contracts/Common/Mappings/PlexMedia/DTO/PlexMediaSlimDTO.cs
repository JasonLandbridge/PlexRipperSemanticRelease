using PlexRipper.Domain;

namespace Application.Contracts;

public class PlexMediaSlimDTO
{
    public required int Id { get; init; }

    /// <summary>
    /// Unique key identifying this item by the Plex Api. This is used by the PlexServers to differentiate between media items.
    /// e.g: 28550, 1723, 21898.
    /// </summary>
    public required int Key { get; set; }

    /// <summary>
    /// Gets or sets the key used to retrieve thumbnails, art or banners.
    /// E.g. /library/metadata/[Key]/art/[MetadataKey] =>  /library/metadata/529367/art/1593898227.
    /// </summary>
    public required int MetaDataKey { get; init; }

    public required string Title { get; init; } = string.Empty;

    public required string SearchTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the sort index of the media based on the abc sort order. This makes sorting much quicker as it can sort on this index which is unique within the response. Starts from 1.
    /// </summary>
    public required int SortIndex { get; set; }

    public required int Year { get; init; }

    public required int Duration { get; init; }

    public required long MediaSize { get; init; }

    public required int ChildCount { get; init; }

    public required int GrandChildCount { get; init; }

    public required DateTime AddedAt { get; init; }

    public required DateTime? UpdatedAt { get; init; }

    public required int PlexLibraryId { get; init; }

    public required int PlexServerId { get; init; }

    public required PlexMediaType Type { get; init; }

    public required bool HasThumb { get; set; }

    /// <summary>
    /// The token used to authenticate with the Plex server to retrieve the thumbnail.
    /// </summary>
    public required string PlexToken { get; set; } = string.Empty;

    public required List<PlexMediaQualityDTO> Qualities { get; init; } = [];
}
