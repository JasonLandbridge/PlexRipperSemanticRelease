﻿using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexLibraries;

public class UpdatePlexLibraryByIdCommandValidator : AbstractValidator<UpdatePlexLibraryByIdCommand>
{
    public UpdatePlexLibraryByIdCommandValidator()
    {
        RuleFor(x => x.PlexLibrary).NotNull();
        RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
        RuleFor(x => x.PlexLibrary.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexLibrary.Type).NotEqual(PlexMediaType.None);
    }
}

public class UpdatePlexLibraryByIdCommandHandler
    : BaseHandler,
        IRequestHandler<UpdatePlexLibraryByIdCommand, Result<bool>>
{
    public UpdatePlexLibraryByIdCommandHandler(ILog log, PlexRipperDbContext dbContext)
        : base(log, dbContext) { }

    public async Task<Result<bool>> Handle(UpdatePlexLibraryByIdCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var plexLibraryDb = await _dbContext
                .PlexLibraries.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == command.PlexLibrary.Id);

            _dbContext.Entry(plexLibraryDb).CurrentValues.SetValues(command.PlexLibrary);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(true);
        }
        catch (Exception e)
        {
            _log.Error(e);
            return Result.Fail(new ExceptionalError(e));
        }
    }
}
