﻿using System.Linq;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Data.Common
{
    public static partial class PlexRipperDbContextExtensions
    {
        #region PlexDownloadTasks

        public static IQueryable<PlexServer> IncludeDownloadTasks(this IQueryable<PlexServer> plexServer)
        {
            return plexServer
                .Include(x => x.PlexLibraries)
                .IncludeDownloadTasks("PlexLibraries.DownloadTasks.")
                .AsQueryable();
        }

        public static IQueryable<DownloadTask> IncludeDownloadTasks(this IQueryable<DownloadTask> downloadTasks)
        {
            return downloadTasks.IncludeDownloadTasks("");
        }

        private static IQueryable<T> IncludeDownloadTasks<T>(this IQueryable<T> query, string prefix = "") where T : class
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                query = query.Include(prefix.TrimEnd('.'));
            }

            // The Include path 'Children->Parent' results in a cycle.
            // Cycles are not allowed in no-tracking queries; either use a tracking query or remove the cycle
            query = query.AsTracking();

            // Include downloadTask children up to 5 levels deep
            for (int i = 1; i <= 5; i++)
            {
                var childPath = prefix + string.Concat(Enumerable.Repeat("Children.", i));

                query = query
                    .Include($"{childPath}".TrimEnd('.'))
                    .Include($"{childPath}DownloadFolder")
                    .Include($"{childPath}DestinationFolder")
                    .Include($"{childPath}DownloadWorkerTasks")
                    .Include($"{childPath}PlexServer")
                    .Include($"{childPath}PlexLibrary");
            }

            return query;
        }

        #endregion
    }
}