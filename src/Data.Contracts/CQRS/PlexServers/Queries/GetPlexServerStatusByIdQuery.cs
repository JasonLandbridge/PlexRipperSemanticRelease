﻿using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexServerStatusByIdQuery : IRequest<Result<PlexServerStatus>>
{
    public GetPlexServerStatusByIdQuery(int id, bool includePlexServer = false)
    {
        Id = id;
        IncludePlexServer = includePlexServer;
    }

    public int Id { get; }

    public bool IncludePlexServer { get; }
}
