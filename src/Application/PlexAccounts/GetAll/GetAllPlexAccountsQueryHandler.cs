﻿using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.GetAll;

public class GetAllPlexAccountsQueryHandler : IRequestHandler<GetAllPlexAccountsQuery, Result<List<PlexAccount>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllPlexAccountsQueryHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<List<PlexAccount>>> Handle(GetAllPlexAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.PlexAccounts.AsQueryable();
        if (request.OnlyEnabled)
            query = query.Where(x => x.IsEnabled);

        var plexAccounts = await query.ToListAsync(cancellationToken);
        _log.Debug("Returned {PlexAccountCount} accounts", plexAccounts.Count);
        return Result.Ok(plexAccounts);
    }
}