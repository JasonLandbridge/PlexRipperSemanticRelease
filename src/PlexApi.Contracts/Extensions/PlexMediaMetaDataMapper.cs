using PlexRipper.Domain;

namespace PlexApi.Contracts;







// public static class PlexMediaMetaDataMapper
// {
//     public static PlexMediaMetaData ToPlexMediaMetaData(this PlexMediaContainerDTO source)
//     {
//         if (
//             source?.MediaContainer == null
//             || !source.MediaContainer.Metadata.Any()
//             || !source.MediaContainer.Metadata.First().Media.Any()
//         )
//             return null;
//
//         var metaData = source?.MediaContainer?.Metadata?.First();
//         var medium = metaData.Media.First();
//         var part = medium.Part.Any() ? medium.Part.First() : null;
//
//         return new PlexMediaMetaData
//         {
//             Duration = medium.Duration,
//             Bitrate = medium.Bitrate,
//             Width = medium.Width,
//             Height = medium.Height,
//             AspectRatio = medium.AspectRatio,
//             AudioChannels = medium.AudioChannels,
//             AudioCodec = medium.AudioCodec,
//             VideoCodec = medium.VideoCodec,
//             VideoResolution = medium.VideoResolution,
//             MediaFormat = medium.Container,
//             VideoFrameRate = medium.VideoFrameRate,
//             AudioProfile = medium.AudioProfile,
//             VideoProfile = medium.VideoProfile,
//             FilePath = part != null ? part.File : "",
//             Title = metaData.Title,
//             ObfuscatedFilePath = part != null ? part.Key : "",
//             TitleTvShow = metaData.GrandparentTitle,
//             TitleTvShowSeason = metaData.ParentTitle,
//             RatingKey = int.TryParse(metaData.RatingKey, out var result) ? result : 0,
//         };
//     }
// }
