﻿using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexTvShows;

public class GetMultiplePlexTvShowsByIdsWithEpisodesQueryValidator
    : AbstractValidator<GetMultiplePlexTvShowsByIdsWithEpisodesQuery>
{
    public GetMultiplePlexTvShowsByIdsWithEpisodesQueryValidator()
    {
        RuleFor(x => x.Ids.Count).GreaterThan(0);
    }
}

public class GetMultiplePlexTvShowsByIdsWithEpisodesQueryHandler
    : BaseHandler,
        IRequestHandler<GetMultiplePlexTvShowsByIdsWithEpisodesQuery, Result<List<PlexTvShow>>>
{
    public GetMultiplePlexTvShowsByIdsWithEpisodesQueryHandler(ILog log, PlexRipperDbContext dbContext)
        : base(log, dbContext) { }

    public async Task<Result<List<PlexTvShow>>> Handle(
        GetMultiplePlexTvShowsByIdsWithEpisodesQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = PlexTvShowsQueryable;

        if (request.IncludeLibrary)
            query = query.IncludePlexLibrary();

        if (request.IncludeServer)
            query = query.IncludePlexServer();

        if (request.IncludeData)
        {
            query = query.Include(x => x.Seasons).ThenInclude(x => x.Episodes);
        }

        var plexTvShow = await query.Where(x => request.Ids.Contains(x.Id)).ToListAsync(cancellationToken);

        return Result.Ok(plexTvShow);
    }
}
