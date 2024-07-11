﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadWorkerTaskConfiguration : IEntityTypeConfiguration<DownloadWorkerTask>
{
    public void Configure(EntityTypeBuilder<DownloadWorkerTask> builder)
    {
        builder
            .HasMany(x => x.DownloadWorkerTaskLogs)
            .WithOne(x => x.DownloadWorkerTask)
            .HasForeignKey(x => x.DownloadWorkerTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(b => b.DownloadStatus)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .IsUnicode(false);
    }
}
