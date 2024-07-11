﻿using AutoMapper;

namespace PlexRipper.Domain.AutoMapper.ValueConverters;

public class StringToDateTimeUtc : IValueConverter<string, DateTime?>
{
    public DateTime? Convert(string sourceMember, ResolutionContext context)
    {
        if (DateTime.TryParse(sourceMember, out var dateTimeResult))
            return dateTimeResult;

        return null;
    }
}
