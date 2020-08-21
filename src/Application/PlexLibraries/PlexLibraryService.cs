﻿using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Application.PlexLibraries.Commands;
using PlexRipper.Application.PlexLibraries.Queries;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces.SignalR;
using PlexRipper.Application.PlexTvShows;

namespace PlexRipper.Application.PlexLibraries
{
    public class PlexLibraryService : IPlexLibraryService
    {
        private readonly IMediator _mediator;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IPlexApiService _plexServiceApi;
        private readonly ISignalRService _signalRService;

        public PlexLibraryService(
            IMediator mediator,
            IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi, ISignalRService signalRService)
        {
            _mediator = mediator;
            _plexAuthenticationService = plexAuthenticationService;
            _plexServiceApi = plexServiceApi;
            _signalRService = signalRService;
        }

        /// <summary>
        /// Retrieve the latest <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <returns>If successful</returns>
        public async Task<Result<bool>> RefreshLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer)
        {
            if (plexServer == null)
            {
                string msg = "plexServer was null";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            Log.Debug($"Refreshing PlexLibraries for plexServer: {plexServer.Name}");

            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccount.Id, plexServer.Id);

            if (authToken.IsFailed)
            {
                return Result.Fail(new Error("Failed to retrieve the server auth token"));
            }

            var libraries = await _plexServiceApi.GetLibrarySectionsAsync(authToken.Value, plexServer.BaseUrl);

            if (!libraries.Any())
            {
                string msg = $"plexLibraries returned for server {plexServer.Name} - {plexServer.BaseUrl} was empty";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            return await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccount, plexServer, libraries));
        }

        public async Task<Result<PlexMediaMetaData>> GetMetaDataAsync(PlexAccount plexAccount, PlexMovie movie)
        {
            if (plexAccount == null || movie == null)
            {
                string msg = "Either plexAccount of the movie was null";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            var plexServer = movie.PlexLibrary?.PlexServer;

            // Get plexServer authToken
            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccount.Id, plexServer.Id);

            if (authToken.IsFailed)
            {
                return Result.Fail(new Error("Failed to retrieve the server auth token"));
            }

            // Send api request
            var apiResult = await _plexServiceApi.GetMediaMetaDataAsync(authToken.Value, movie.MetaDataUrl);
            if (apiResult != null)
            {
                return Result.Ok(apiResult);
            }
            return Result.Fail(new Error($"Failed to retrieve the metadata for movie {movie.Title}"));
        }

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> with the media content.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <param name="libraryKey"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        public Task<PlexLibrary> GetLibraryMediaAsync(PlexAccount plexAccount, PlexServer plexServer, string libraryKey,
            bool refresh = false)
        {
            var plexLibrary = plexServer.PlexLibraries.ToList().Find(x => x.Key == libraryKey);
            return GetLibraryMediaAsync(plexAccount, plexLibrary, refresh);
        }

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> with the media content.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexLibrary"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        public async Task<PlexLibrary> GetLibraryMediaAsync(PlexAccount plexAccount, PlexLibrary plexLibrary,
            bool refresh = false)
        {
            if (refresh || !plexLibrary.HasMedia)
            {
                await RefreshLibraryMediaAsync(plexAccount, plexLibrary);
            }
            return plexLibrary;
        }


        /// <summary>
        /// Retrieves the new media metadata from the PlexApi and stores it in the database.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexLibrary">The <see cref="PlexLibrary"/> to retrieve</param>
        /// <returns>Returns the PlexLibrary with the containing media</returns>
        public async Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(PlexAccount plexAccount,
            PlexLibrary plexLibrary)
        {
            if (plexLibrary == null)
            {
                string msg = "The plexLibrary was null";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            // Get plexServer authToken
            var authToken =
                await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccount.Id, plexLibrary.PlexServer.Id);

            if (authToken.IsFailed)
            {
                return Result.Fail(new Error("Failed to retrieve the server auth token"));
            }

            var newPlexLibrary =
                await _plexServiceApi.GetLibraryMediaAsync(plexLibrary, authToken.Value,
                    plexLibrary.PlexServer.BaseUrl);
            var result = Result.Fail($"Failed to refresh library {plexLibrary.Id}");
            if (newPlexLibrary == null)
            {
                return result;
            }

            switch (newPlexLibrary.GetMediaType)
            {
                case PlexMediaType.Movie:
                    result = await _mediator.Send(
                        new CreateOrUpdatePlexMoviesCommand(newPlexLibrary, newPlexLibrary.Movies));
                    break;
                case PlexMediaType.TvShow:
                    return await RefreshPlexTvShowLibrary(authToken.Value, newPlexLibrary);
            }
            if (result.IsFailed)
            {
                return result.ToResult<PlexLibrary>();
            }
            Log.Information($"Successfully refreshed library {newPlexLibrary.Title} with id: {newPlexLibrary.Id}");
            return Result.Ok(newPlexLibrary);
        }

        public async Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(int plexAccountId, int plexLibraryId)
        {
            var plexAccount = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId));
            if (plexAccount.IsFailed)
            {
                return plexAccount.ToResult<PlexLibrary>();
            }

            var plexLibrary = await _mediator.Send(new GetPlexLibraryByIdQuery(plexLibraryId, true));
            if (plexLibrary.IsFailed)
            {
                return plexLibrary;
            }

            return await RefreshLibraryMediaAsync(plexAccount.Value, plexLibrary.Value);
        }


        private async Task<Result<PlexLibrary>> RefreshPlexTvShowLibrary(string authToken, PlexLibrary plexLibrary)
        {
            if (plexLibrary == null)
            {
                return ResultExtensions.IsNull("plexLibrary").LogError();
            }

            if (plexLibrary.GetMediaType != PlexMediaType.TvShow)
            {
                return Result.Fail("PlexLibrary is not of type TvShow").LogError();
            }

            if (plexLibrary.TvShows.Count == 0)
            {
                return Result.Fail("PlexLibrary does not contain any TvShows and thus cannot request the corresponding media").LogError();
            }

            var result = await _mediator.Send(new GetPlexLibraryByIdWithServerQuery(plexLibrary.Id));
            if (result.IsFailed)
            {
                return result;
            }

            // Request seasons and episodes for every tv show
            var plexLibraryDb = result.Value;
            var serverUrl = plexLibraryDb.PlexServer.BaseUrl;
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, 0, plexLibrary.TvShows.Count);

            for (int i = 0; i < plexLibrary.TvShows.Count; i++)
            {
                var plexTvShow = plexLibrary.TvShows[i];
                plexTvShow.Seasons = await _plexServiceApi.GetSeasonsAsync(authToken, serverUrl, plexTvShow);

                // Retrieve the episodes for every season
                if (!plexTvShow.Seasons.Any())
                {
                    continue;
                }

                foreach (var showSeason in plexTvShow.Seasons)
                {
                    showSeason.Episodes = await _plexServiceApi.GetEpisodesAsync(authToken, serverUrl, showSeason);
                }

                // Send progress update to clients
                await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, i, plexLibrary.TvShows.Count);
            }

            Log.Debug($"Finished retrieving all media for library {plexLibraryDb.Title}");
            var updateResult = await _mediator.Send(new CreateOrUpdatePlexTvShowsCommand(plexLibrary, plexLibrary.TvShows));
            if (updateResult.IsFailed)
            {
                return updateResult.ToResult();
            }

            var freshPlexLibrary = await _mediator.Send(new GetPlexLibraryByIdQuery(plexLibrary.Id, false, true));

            // Complete progress update
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, plexLibrary.TvShows.Count,
                plexLibrary.TvShows.Count);
            return freshPlexLibrary;
        }

        #region CRUD

        /// <summary>
        /// Return the PlexLibrary by the Id, will refresh if the library has no media assigned.
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        public async Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId, int plexAccountId)
        {
            await _signalRService.SendLibraryProgressUpdate(libraryId, 0, 1, false);

            var libraryDB = await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId, true, true));

            if (libraryDB.IsFailed)
            {
                await _signalRService.SendLibraryProgressUpdate(libraryId, 1, 1, false);
                return libraryDB;
            }

            if (!libraryDB.Value.HasMedia)
            {
                Log.Information($"PlexLibrary with id {libraryId} has no media, forcing refresh from the PlexApi");
                if (plexAccountId <= 0)
                {
                    Log.Warning(
                        $"plexAccountId was {plexAccountId}, could not refresh the PlexLibrary with id {libraryId}");
                    return null;
                }

                var plexAccount = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId));
                if (plexAccount.IsFailed)
                {
                    return plexAccount.ToResult<PlexLibrary>();
                }

                return await RefreshLibraryMediaAsync(plexAccount.Value, libraryDB.Value);
            }

            await _signalRService.SendLibraryProgressUpdate(libraryId, 1, 1, false);
            return libraryDB;
        }

        #endregion
    }
}