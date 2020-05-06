﻿using PlexRipper.Domain.Entities.Plex;
using System;

namespace PlexRipper.Domain.Entities
{
    /// <summary>
    /// This is used as an account wrapper around <see cref="PlexAccount"/>
    /// </summary>
    public class Account : BaseEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsValidated { get; set; }
        public DateTime ValidatedAt { get; set; }
        public PlexAccount PlexAccount { get; set; }
    }
}