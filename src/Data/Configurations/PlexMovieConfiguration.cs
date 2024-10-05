﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMovieConfiguration : IEntityTypeConfiguration<PlexMovie>
{
    public void Configure(EntityTypeBuilder<PlexMovie> builder)
    {
        // NOTE: This has been added to PlexRipperDbContext.OnModelCreating
        // Based on: https://stackoverflow.com/a/63992731/8205497
        // builder
        //     .Property(x => x.MediaData)
        //     .HasJsonValueConversion();

        builder.Property(c => c.SortTitle).UseCollation(OrderByNaturalExtensions.CollationName);
    }
}
