using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json.Serialization;

namespace Settings.Contracts;

/// <inheritdoc cref="IUserSettings"/>
public class UserSettings : IUserSettings
{
    public GeneralSettingsModule GeneralSettings
    {
        get => _generalSettings;
        init => _generalSettings = value;
    }

    public ConfirmationSettingsModule ConfirmationSettings
    {
        get => _confirmationSettings;
        init => _confirmationSettings = value;
    }

    public DateTimeSettingsModule DateTimeSettings
    {
        get => _dateTimeSettings;
        init => _dateTimeSettings = value;
    }

    public DisplaySettingsModule DisplaySettings
    {
        get => _displaySettings;
        init => _displaySettings = value;
    }

    public DownloadManagerSettingsModule DownloadManagerSettings
    {
        get => _downloadManagerSettings;
        init => _downloadManagerSettings = value;
    }

    public LanguageSettingsModule LanguageSettings
    {
        get => _languageSettings;
        init => _languageSettings = value;
    }

    public DebugSettingsModule DebugSettings
    {
        get => _debugSettings;
        init => _debugSettings = value;
    }

    public PlexServerSettingsModule ServerSettings
    {
        get => _serverSettings;
        init => _serverSettings = value;
    }

    private readonly Subject<UserSettings> _settingsUpdated = new();

    private GeneralSettingsModule _generalSettings = GeneralSettingsModule.Create();
    private ConfirmationSettingsModule _confirmationSettings = ConfirmationSettingsModule.Create();
    private DateTimeSettingsModule _dateTimeSettings = DateTimeSettingsModule.Create();
    private DisplaySettingsModule _displaySettings = DisplaySettingsModule.Create();
    private DownloadManagerSettingsModule _downloadManagerSettings = DownloadManagerSettingsModule.Create();
    private LanguageSettingsModule _languageSettings = LanguageSettingsModule.Create();
    private DebugSettingsModule _debugSettings = DebugSettingsModule.Create();
    private PlexServerSettingsModule _serverSettings = PlexServerSettingsModule.Create();

    /// <summary>
    /// The <see cref="UserSettings"/> class is a wrapper class for the individual Settings.
    /// </summary>
    public UserSettings()
    {
        // Alert of any  changes
        Observable
            .Merge(
                ConfirmationSettings.HasChanged.Select(_ => 1),
                DateTimeSettings.HasChanged.Select(_ => 1),
                DisplaySettings.HasChanged.Select(_ => 1),
                DownloadManagerSettings.HasChanged.Select(_ => 1),
                GeneralSettings.HasChanged.Select(_ => 1),
                DebugSettings.HasChanged.Select(_ => 1),
                LanguageSettings.HasChanged.Select(_ => 1),
                ServerSettings.HasChanged.Select(_ => 1)
            )
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ => _settingsUpdated.OnNext(this));
    }

    [JsonIgnore]
    public IObservable<UserSettings> SettingsUpdated => _settingsUpdated.AsObservable();

    public void Reset()
    {
        _generalSettings = GeneralSettingsModule.Create();
        _confirmationSettings = ConfirmationSettingsModule.Create();
        _dateTimeSettings = DateTimeSettingsModule.Create();
        _displaySettings = DisplaySettingsModule.Create();
        _downloadManagerSettings = DownloadManagerSettingsModule.Create();
        _languageSettings = LanguageSettingsModule.Create();
        _debugSettings = DebugSettingsModule.Create();
        _serverSettings = PlexServerSettingsModule.Create();
    }

    public UserSettings UpdateSettings(ISettingsModel sourceSettings)
    {
        _confirmationSettings.Update(sourceSettings.ConfirmationSettings);
        _dateTimeSettings.Update(sourceSettings.DateTimeSettings);
        _displaySettings.Update(sourceSettings.DisplaySettings);
        _downloadManagerSettings.Update(sourceSettings.DownloadManagerSettings);
        _generalSettings.Update(sourceSettings.GeneralSettings);
        _debugSettings.Update(sourceSettings.DebugSettings);
        _languageSettings.Update(sourceSettings.LanguageSettings);
        _serverSettings.Update(sourceSettings.ServerSettings);

        return this;
    }
}
