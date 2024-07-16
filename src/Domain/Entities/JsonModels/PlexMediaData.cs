﻿namespace PlexRipper.Domain
{
    public record PlexMediaData
    {
        public required string MediaFormat { get; init; }
        public required long Duration { get; init; }
        public required string VideoResolution { get; init; }
        public required int Width { get; init; }
        public required int Height { get; init; }
        public required int Bitrate { get; init; }
        public required string VideoCodec { get; init; }
        public required string VideoFrameRate { get; init; }
        public required double AspectRatio { get; init; }
        public required string VideoProfile { get; init; }
        public required string AudioProfile { get; init; }
        public required string AudioCodec { get; init; }
        public required int AudioChannels { get; init; }
        public required bool OptimizedForStreaming { get; init; }
        public required string Protocol { get; init; }
        public required bool Selected { get; init; }
        public required List<PlexMediaDataPart> Parts { get; init; }

        public bool IsMultiPart => Parts.Count > 1;
    }
}
