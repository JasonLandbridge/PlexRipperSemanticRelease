﻿using System.Globalization;
using Logging.Interface;

namespace PlexRipper.Domain;

public static class DataFormat
{
    private static readonly NumberFormatInfo NumberFormat = NumberFormatInfo.InvariantInfo;
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(DataFormat));

    /// <summary>
    /// Format file size or downloaded size string.
    /// </summary>
    /// <param name="byteSize"></param>
    /// <returns></returns>
    public static string FormatSizeString(long byteSize)
    {
        var kiloByteSize = byteSize / 1024D;
        var megaByteSize = kiloByteSize / 1024D;
        var gigaByteSize = megaByteSize / 1024D;
        return byteSize switch
        {
            < 1024 => string.Format(NumberFormat, "{0} B", byteSize),
            < 1048576 => string.Format(NumberFormat, "{0:0.00} kB", kiloByteSize),
            var _ => byteSize < 1073741824
                ? string.Format(NumberFormat, "{0:0.00} MB", megaByteSize)
                : string.Format(NumberFormat, "{0:0.00} GB", gigaByteSize),
        };
    }

    /// <summary>
    /// Format download speed string.
    /// </summary>
    /// <param name="speed"></param>
    /// <returns></returns>
    public static string FormatSpeedString(long speed)
    {
        var kbSpeed = speed / 1024F;
        var mbSpeed = kbSpeed / 1024F;
        if (speed <= 0)
            return string.Empty;
        if (speed < 1024)
            return speed + " B/s";
        if (speed < 1048576)
            return kbSpeed.ToString("#.00", NumberFormat) + " kB/s";

        return mbSpeed.ToString("#.00", NumberFormat) + " MB/s";
    }

    /// <summary>
    /// Format time span string so it can display values of more than 24 hours.
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static string FormatTimeSpanString(TimeSpan span)
    {
        var hours = ((int)span.TotalHours).ToString();
        var minutes = span.Minutes.ToString();
        var seconds = span.Seconds.ToString();
        if ((int)span.TotalHours < 10)
            hours = "0" + hours;
        if (span.Minutes < 10)
            minutes = "0" + minutes;
        if (span.Seconds < 10)
            seconds = "0" + seconds;
        return $"{hours}:{minutes}:{seconds}";
    }

    public static decimal GetPercentage(long bytesReceived, long totalBytes)
    {
        if (totalBytes == 0)
            return 0;

        try
        {
            return (decimal)Math.Round(bytesReceived / (double)totalBytes * 100, 2, MidpointRounding.AwayFromZero);
        }
        catch (Exception e)
        {
            _log.Error(e);
            throw;
        }
    }

    public static decimal GetPercentage(int current, int total)
    {
        if (total == 0)
            return 0;

        try
        {
            return (decimal)Math.Round(current / (double)total * 100, 2, MidpointRounding.AwayFromZero);
        }
        catch (Exception e)
        {
            _log.Error(e);
            throw;
        }
    }

    /// <summary>
    /// Returns the bytes per second.
    /// </summary>
    /// <param name="bytesReceivedPerSecond"></param>
    /// <param name="elapsedTimeInSeconds"></param>
    /// <returns></returns>
    public static int GetTransferSpeed(long bytesReceivedPerSecond, double elapsedTimeInSeconds) =>
        elapsedTimeInSeconds <= 0 ? 0 : (int)Math.Round(bytesReceivedPerSecond / elapsedTimeInSeconds, 2);

    public static long GetTimeRemaining(long bytesRemaining, double downloadSpeed) =>
        downloadSpeed <= 0 ? 0 : Convert.ToInt64(Math.Floor(bytesRemaining / downloadSpeed));
}
