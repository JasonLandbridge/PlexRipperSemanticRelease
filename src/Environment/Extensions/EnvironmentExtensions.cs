﻿namespace Environment;

public static class EnvironmentExtensions
{
    public const string IntegrationTestModeKey = "IntegrationTestMode";

    public const string UnmaskedModeKey = "UNMASKED";

    public const string DevelopmentRootPathKey = "DEVELOPMENT_ROOT_PATH";

    private static readonly string TrueValue = Convert.ToString(true);

    /// <summary>
    /// Determines if the value is true.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsTrue(string? value) =>
        value == Convert.ToString(true) || value == "1" || value == "true" || value == "TRUE";

    public static bool IsIntegrationTestMode() =>
        System.Environment.GetEnvironmentVariable(IntegrationTestModeKey) == TrueValue;

    /// <summary>
    /// This is the path that is used to store the /config, /downloads, /movies and /tvshows folders required to boot PlexRipper in development mode in a non-docker environment.
    /// </summary>
    /// <returns></returns>
    public static string? GetDevelopmentRootPath() => System.Environment.GetEnvironmentVariable(DevelopmentRootPathKey);

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static bool IsUnmasked() => IsTrue(System.Environment.GetEnvironmentVariable(UnmaskedModeKey));

    public static void SetIntegrationTestMode(bool state)
    {
        System.Environment.SetEnvironmentVariable(IntegrationTestModeKey, state.ToString());
    }

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static void SetUnmaskedLogMode(bool state)
    {
        System.Environment.SetEnvironmentVariable(UnmaskedModeKey, state.ToString());
    }
}
