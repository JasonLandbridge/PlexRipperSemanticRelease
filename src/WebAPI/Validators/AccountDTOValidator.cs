﻿using FluentValidation;
using PlexRipper.Domain.ValueObjects;

namespace PlexRipper.WebAPI.Validators
{
    public class AccountDTOValidator : AbstractValidator<AccountDTO>
    {
        public AccountDTOValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Please specify a username");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Please specify a password");
        }
    }
}