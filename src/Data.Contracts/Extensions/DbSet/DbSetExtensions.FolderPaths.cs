using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    /// <summary>
    /// Get the current download <see cref="FolderPath"/>.
    /// </summary>
    /// <returns>The <see cref="FolderPath"/> of the download folder.</returns>
    public static async Task<FolderPath?> GetDownloadFolderAsync(
        this IQueryable<FolderPath> dbSet,
        CancellationToken token = default
    )
    {
        return await dbSet.FirstOrDefaultAsync(x => x.Id == 1, token);
    }
}
