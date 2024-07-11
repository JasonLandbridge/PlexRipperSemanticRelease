﻿using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexApi.UnitTests;

public class TestDTO
{
    [JsonConverter(typeof(LongValueConverter))]
    public long ContentChangedAt { get; set; }
}
