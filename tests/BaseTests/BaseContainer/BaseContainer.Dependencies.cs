﻿using System;
using System.Threading.Tasks;
using Environment;
using Logging;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI;

namespace PlexRipper.BaseTests
{
    public partial class BaseContainer : IDisposable
    {
        #region Fields

        private readonly WebApplicationFactory<Startup> _factory;

        private readonly PlexMockServer _mockServer;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a Autofac container and sets up a test database.
        /// </summary>
        private BaseContainer(PlexMockServer mockServer = null, UnitTestDataConfig config = null)
        {
            _mockServer = mockServer;
            _factory = new PlexRipperWebApplicationFactory<Startup>(config);
            ApiClient = _factory.CreateClient();
        }

        public static async Task<BaseContainer> Create(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            Log.Debug($"Setting up BaseContainer with database: {config.MemoryDbName}");

            EnvironmentExtensions.SetIntegrationTestMode(true);

            PlexMockServer mockServer = null;
            if (config.MockServerConfig is not null)
            {
                mockServer = new PlexMockServer(config.MockServerConfig);
                config.MockServerConfig.DownloadUri = mockServer.GetDownloadUri;
                config.MockServerConfig.ServerUri = mockServer.ServerUri;
            }

            // Database context can be setup once and then retrieved by its DB name.
            await MockDatabase.GetMemoryDbContext(config.MemoryDbName).Setup(config);
            var container = new BaseContainer(mockServer, config);

            if (config.DownloadSpeedLimit > 0)
            {
                await container.SetDownloadSpeedLimit(config);
            }

            return container;
        }

        #endregion

        #region Properties

        public System.Net.Http.HttpClient ApiClient { get; }

        #region Autofac Resolve

        public IFileSystem FileSystem => Resolve<IFileSystem>();

        public IDownloadCommands GetDownloadCommands => Resolve<IDownloadCommands>();

        public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

        public IDownloadTaskFactory GetDownloadTaskFactory => Resolve<IDownloadTaskFactory>();

        public IDownloadTaskValidator GetDownloadTaskValidator => Resolve<IDownloadTaskValidator>();

        public IDownloadTracker GetDownloadTracker => Resolve<IDownloadTracker>();

        public IFolderPathService GetFolderPathService => Resolve<IFolderPathService>();

        public IPlexAccountService GetPlexAccountService => Resolve<IPlexAccountService>();

        public IPlexApiService GetPlexApiService => Resolve<IPlexApiService>();

        public IPlexDownloadService GetPlexDownloadService => Resolve<IPlexDownloadService>();

        public IPlexLibraryService GetPlexLibraryService => Resolve<IPlexLibraryService>();

        public IPlexRipperHttpClient GetPlexRipperHttpClient => Resolve<IPlexRipperHttpClient>();

        public IPlexServerService GetPlexServerService => Resolve<IPlexServerService>();

        public IUserSettings GetUserSettings => Resolve<IUserSettings>();

        public IMediator Mediator => Resolve<IMediator>();

        public IPathProvider PathProvider => Resolve<IPathProvider>();

        public ITestStreamTracker TestStreamTracker => Resolve<ITestStreamTracker>();

        public ITestApplicationTracker TestApplicationTracker => Resolve<ITestApplicationTracker>();

        public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

        public IDownloadScheduler DownloadScheduler => Resolve<IDownloadScheduler>();

        #endregion

        #endregion

        #region Public Methods

        private T Resolve<T>()
        {
            return _factory.Server.Services.GetRequiredService<T>();
        }

        #endregion

        public void Dispose()
        {
            _factory?.Dispose();
            ApiClient?.Dispose();
        }
    }
}