﻿using Settings.Contracts;

namespace PlexRipper.Settings.Models;

public class DownloadManagerSettings : IDownloadManagerSettings
{
    public int DownloadSegments { get; set; }
}
