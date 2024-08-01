﻿using Settings.Contracts;

namespace PlexRipper.Settings;

/// <summary>
/// Used to model the settings, which is then used to serialize to json.
/// </summary>
public class SettingsModule : ISettingsModel
{
    #region Properties

    public IGeneralSettings GeneralSettings { get; set; }

    public IConfirmationSettings ConfirmationSettings { get; set; }

    public IDateTimeSettings DateTimeSettings { get; set; }

    public IDisplaySettings DisplaySettings { get; set; }

    public IDownloadManagerSettings DownloadManagerSettings { get; set; }

    public ILanguageSettings LanguageSettings { get; set; }

    public IDebugSettings DebugSettings { get; set; }

    public IServerSettings ServerSettings { get; set; }

    #endregion

    public static SettingsModule DefaultSettings() =>
        new()
        {
            GeneralSettings = new GeneralSettingsModule(),
            DebugSettings = new DebugSettingsModule(),
            ConfirmationSettings = new ConfirmationSettingsModule(),
            DateTimeSettings = new DateTimeSettingsModule(),
            DisplaySettings = new DisplaySettingsModule(),
            DownloadManagerSettings = new DownloadManagerSettingsModule(),
            LanguageSettings = new LanguageSettingsModule(),
            ServerSettings = new ServerSettingsModule(),
        };
}
