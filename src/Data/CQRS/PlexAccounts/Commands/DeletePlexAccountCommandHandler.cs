﻿using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS
{
    public class DeletePlexAccountValidator : AbstractValidator<DeletePlexAccountCommand>
    {
        public DeletePlexAccountValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class DeletePlexAccountHandler : BaseHandler, IRequestHandler<DeletePlexAccountCommand, Result<bool>>
    {
        public DeletePlexAccountHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(DeletePlexAccountCommand command, CancellationToken cancellationToken)
        {
            var plexAccount = await _dbContext.PlexAccounts.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (plexAccount == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexAccount), command.Id);
            }

            _dbContext.PlexAccounts.Remove(plexAccount);
            await _dbContext.SaveChangesAsync(cancellationToken);
            Log.Debug($"Deleted PlexAccount with Id: {command.Id} from the database");

            return Result.Ok(true);
        }
    }
}