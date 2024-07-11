﻿namespace PlexRipper.Domain.Autofac;

public class ScopedDependency : IScopedDependency
{
    public ScopedDependency(string scope)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }

    public string Scope { get; }
}
