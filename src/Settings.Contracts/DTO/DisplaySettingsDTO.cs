﻿using PlexRipper.Domain;

namespace Settings.Contracts;

public class DisplaySettingsDTO : IDisplaySettings
{
    public required ViewMode TvShowViewMode { get; set; }

    public required ViewMode MovieViewMode { get; set; }

    public required PlexMediaType AllOverviewViewMode { get; set; }
}
