using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.Tests.UnitTests
{
    public class DownloadManagerTests
    {
        private BaseContainer Container { get; }

        public DownloadManagerTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();

            WireMockServer server = MockServer.GetPlexMockServer();

            Log.Debug($"Server running at: {server.Urls[0]}");
        }

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenEmptyListIsGiven()
        {
            //Arrange
            var downloadManager = Container.GetDownloadManager;

            // Act
            var result = await downloadManager.AddToDownloadQueueAsync(new List<DownloadTask>());

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenInvalidDownloadTasksAreGiven()
        {
            //Arrange
            var downloadManager = Container.GetDownloadManager;

            // Act
            var result = await downloadManager.AddToDownloadQueueAsync(new List<DownloadTask>
            {
                new(),
                new(),
            });

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

    }
}