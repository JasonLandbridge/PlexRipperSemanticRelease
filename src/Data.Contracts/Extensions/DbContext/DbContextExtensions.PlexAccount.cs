using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<List<PlexAccount>>> GetPlexAccountsWithAccessAsync(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    )
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogError();

        var query = await dbContext
            .PlexAccountServers.Include(x => x.PlexAccount)
            .Where(x => x.PlexServerId == plexServerId)
            .Select(x => x.PlexAccount)
            .ToListAsync(cancellationToken);

        if (!query.Any())
        {
            return Result
                .Fail(
                    $"There were no PlexAccounts that have access to PlexServer with id: {dbContext.GetPlexServerNameById(plexServerId, cancellationToken)}"
                )
                .LogError();
        }

        return Result.Ok(query);
    }

    public static async Task<Result<PlexAccount>> ChoosePlexAccountToConnect(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken ct = default
    )
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId);

        var plexAccountsResult = await dbContext.GetPlexAccountsWithAccessAsync(plexServerId, ct);
        if (plexAccountsResult.IsFailed)
            return plexAccountsResult.ToResult();

        var plexAccounts = plexAccountsResult.Value.FindAll(x => x.IsEnabled);
        if (!plexAccounts.Any())
        {
            return Result
                .Fail($"There are no enabled accounts that can access PlexServer with id: {plexServerId}")
                .LogError();
        }

        if (plexAccounts.Count == 1)
            return Result.Ok(plexAccounts.First());

        // Prefer to use a non-main account
        var dummyAccount = plexAccounts.FirstOrDefault(x => !x.IsMain);
        if (dummyAccount is not null)
            return Result.Ok(dummyAccount);

        var mainAccount = plexAccounts.FirstOrDefault(x => x.IsMain);
        if (mainAccount is not null)
            return Result.Ok(mainAccount);

        return Result.Fail($"No account could be chosen to connect to PlexServer with id: {plexServerId}").LogError();
    }

    public static async Task<Result<List<PlexServer>>> GetAccessiblePlexServers(
        this IPlexRipperDbContext dbContext,
        int plexAccountId,
        CancellationToken cancellationToken = default
    )
    {
        var plexAccount = await dbContext
            .PlexAccounts.AsNoTracking()
            .Include(x => x.PlexAccountServers)
            .ThenInclude(x => x.PlexServer)
            .FirstOrDefaultAsync(x => x.Id == plexAccountId, cancellationToken);

        if (plexAccount == null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId);

        if (!plexAccount.PlexServers.Any())
            _log.Warning("No accessible PlexServers found for PlexAccount: {DisplayName}", plexAccount.DisplayName);

        return Result.Ok(plexAccount.PlexServers);
    }

    public static async Task<bool> IsUsernameAvailable(
        this IPlexRipperDbContext dbContext,
        string username,
        CancellationToken cancellationToken = default
    )
    {
        var plexAccount = await dbContext.PlexAccounts.FirstOrDefaultAsync(
            x => x.Username == username,
            cancellationToken
        );
        return plexAccount == null;
    }

    public static async Task<string> GetPlexAccountDisplayName(
        this IPlexRipperDbContext dbContext,
        int plexAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
                .PlexAccounts.Where(x => x.Id == plexAccountId)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken) ?? "MISSING DISPLAY NAME";
    }
}
