﻿using Settings.Contracts;

namespace PlexRipper.Settings;

public class DateTimeSettingsModule : BaseSettingsModule<IDateTimeSettings>, IDateTimeSettingsModule
{
    #region Properties

    public override string Name => "DateTimeSettings";

    public string LongDateFormat { get; set; }

    public string ShortDateFormat { get; set; }

    public bool ShowRelativeDates { get; set; }

    public string TimeFormat { get; set; }

    public string TimeZone { get; set; }

    public override IDateTimeSettings DefaultValues() =>
        new DateTimeSettings
        {
            TimeFormat = "HH:mm:ss",
            TimeZone = "UTC",
            LongDateFormat = "EEEE, dd MMMM yyyy",
            ShortDateFormat = "dd/MM/yyyy",
            ShowRelativeDates = true,
        };

    #endregion

    #region Public Methods

    public override IDateTimeSettings GetValues() =>
        new DateTimeSettings
        {
            TimeFormat = TimeFormat,
            TimeZone = TimeZone,
            LongDateFormat = LongDateFormat,
            ShortDateFormat = ShortDateFormat,
            ShowRelativeDates = ShowRelativeDates,
        };

    #endregion
}
