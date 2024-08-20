﻿using RestSharp;

namespace PlexRipper.PlexApi.Helpers;

public static class PlexApiExtensions
{
    /// <summary>
    /// Adds the required headers and also the authorization header.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="authToken"></param>
    public static RestRequest AddToken(this RestRequest request, string authToken)
    {
        request.AddQueryParameter("X-Plex-Token", authToken);
        return request;
    }

    /// <summary>
    /// Adds the required headers and also the authorization header.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="clientId">The unique ClientId belonging to a <see cref="PlexAccount"/></param>
    public static RestRequest AddPlexHeaders(this RestRequest request, string clientId)
    {
        // TODO Add seed to database PlexAccount and use that to generate the below values

        request.AddQueryParameter("X-Plex-Product", "Plex Web");
        request.AddQueryParameter("X-Plex-Version", "4.132.2");
        request.AddPlexClientIdentifier();
        request.AddQueryParameter("X-Plex-Platform", "Chrome");
        request.AddQueryParameter("X-Plex-Platform-Version", "127.0");
        request.AddQueryParameter("X-Plex-Sync-Version", "2");
        request.AddQueryParameter("X-Plex-Features", "external-media,indirect-media");
        request.AddQueryParameter("X-Plex-Model", "bundled");
        request.AddQueryParameter("X-Plex-Device", "Windows");
        request.AddQueryParameter("X-Plex-Device-Name", "Chrome");
        request.AddQueryParameter("X-Plex-Device-Screen-Resolution", "1673x1297,2560x1440");
        request.AddQueryParameter("X-Plex-Language", "en");
        request.AddQueryParameter("X-Plex-Session-Id", Guid.NewGuid().ToString());

        return request;
    }

    public static RestRequest AddPlexClientIdentifier(this RestRequest request, string clientId = "")
    {
        request.AddQueryParameter("X-Plex-Client-Identifier", string.IsNullOrEmpty(clientId) ? "Chrome" : clientId);
        return request;
    }

    public static RestRequest AddLimitHeaders(this RestRequest request, int startIndex, int batchSize)
    {
        request.AddQueryParameter("X-Plex-Container-Start", startIndex.ToString());
        request.AddQueryParameter("X-Plex-Container-Size", batchSize.ToString());
        return request;
    }
}
