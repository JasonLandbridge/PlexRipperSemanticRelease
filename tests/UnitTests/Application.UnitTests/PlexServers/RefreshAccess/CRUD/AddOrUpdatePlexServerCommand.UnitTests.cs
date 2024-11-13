using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class AddOrUpdatePlexServerCommandUnitTests : BaseUnitTest<AddOrUpdatePlexServersCommandHandler>
{
    public AddOrUpdatePlexServerCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldAddAllServers_WhenNoneExistInTheDatabase()
    {
        // Arrange
        var seed = await SetupDatabase(45832543, config => config.PlexServerCount = 0);
        var expectedPlexServers = FakeData.GetPlexServer(seed).Generate(5);

        // Act
        var request = new AddOrUpdatePlexServersCommand(expectedPlexServers);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexServersDbs = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServersDbs.Count.ShouldBe(5);

        foreach (var expectedPlexServer in expectedPlexServers)
        {
            var plexServerDb = plexServersDbs.Find(x => x.MachineIdentifier == expectedPlexServer.MachineIdentifier);
            plexServerDb.ShouldNotBeNull();
            plexServerDb.ShouldBe(expectedPlexServer);
            plexServerDb.PlexServerConnections.Count.ShouldBe(expectedPlexServer.PlexServerConnections.Count);
            plexServerDb.PlexServerConnections.ShouldBe(expectedPlexServer.PlexServerConnections, true);
        }
    }

    [Fact]
    public async Task ShouldKeepTheSameServerConnectionIds_WhenOnlyTheConnectionPropertiesHaveChanged()
    {
        // Arrange
        var seed = await SetupDatabase(23724, config => config.PlexServerCount = 5);
        var plexServers = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServers.Count.ShouldBe(5);

        // Update data setup
        var updatedServers = plexServers.Take(2).ToList();

        // Simulate the server being unchanged, and only the connections having changed except the connection
        // address which it is matched on during updating
        foreach (var updatedServer in updatedServers)
        {
            var connectionCount = updatedServer.PlexServerConnections.Count;
            var updatedConnections = FakeData.GetPlexServerConnections(seed).Generate(connectionCount);
            for (var i = 0; i < connectionCount; i++)
                updatedConnections[i] = new PlexServerConnection
                {
                    Id = updatedConnections[i].Id,
                    Protocol = updatedConnections[i].Protocol,
                    Address = updatedServer.PlexServerConnections[i].Address,
                    Port = updatedConnections[i].Port,
                    Uri = updatedConnections[i].Uri,
                    Local = updatedConnections[i].Local,
                    Relay = updatedConnections[i].Relay,
                    IPv4 = updatedConnections[i].IPv4,
                    IPv6 = updatedConnections[i].IPv6,
                    PlexServer = updatedConnections[i].PlexServer,
                    PlexServerId = updatedConnections[i].PlexServerId,
                    PlexServerStatus = updatedConnections[i].PlexServerStatus,
                    IsCustom = updatedConnections[i].IsCustom,
                };

            updatedServer.PlexServerConnections = updatedConnections;
        }

        // Act
        // Now update
        var request = new AddOrUpdatePlexServersCommand(updatedServers);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexServersDbs = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServersDbs.Count.ShouldBe(5);

        foreach (var expectedServer in updatedServers)
        {
            var plexServerDb = plexServersDbs.Find(x => x.MachineIdentifier == expectedServer.MachineIdentifier);
            plexServerDb.ShouldNotBeNull();

            foreach (var plexServerConnectionDb in plexServerDb.PlexServerConnections)
            {
                var expectedConnection = expectedServer.PlexServerConnections.Find(x =>
                    x.Address == plexServerConnectionDb.Address
                );
                expectedConnection.ShouldNotBeNull();
                plexServerConnectionDb.Id.ShouldBe(expectedConnection.Id);
                plexServerConnectionDb.ShouldBe(expectedConnection);
            }
        }
    }

    [Fact]
    public async Task ShouldUpdateSomeAndSyncServersWithConnections_WhenSomeServerConnectionsHaveChangedAndSomeExistInTheDatabase()
    {
        // Arrange
        var seed = await SetupDatabase(23724, config => config.PlexServerCount = 5);
        var plexServers = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        var changedPlexServers = FakeData.GetPlexServer(seed).Generate(3);

        var expectedPlexServers = new List<PlexServer>()
        {
            changedPlexServers[0],
            changedPlexServers[1],
            changedPlexServers[2],
            plexServers[3],
            plexServers[4],
        };

        // Create updated servers with the same machineId
        for (var i = 0; i < changedPlexServers.Count; i++)
            changedPlexServers[i].MachineIdentifier = plexServers[i].MachineIdentifier;

        // Act
        // First add the 5 servers
        var request = new AddOrUpdatePlexServersCommand(plexServers);
        var addResult = await _sut.Handle(request, CancellationToken.None);

        // Now update
        request = new AddOrUpdatePlexServersCommand(changedPlexServers);
        var updateResult = await _sut.Handle(request, CancellationToken.None);

        // Assert
        addResult.IsSuccess.ShouldBeTrue();
        updateResult.IsSuccess.ShouldBeTrue();
        var plexServersDbs = IDbContext.PlexServers.Include(x => x.PlexServerConnections).ToList();
        plexServersDbs.Count.ShouldBe(5);

        foreach (var expectedPlexServer in expectedPlexServers)
        {
            var plexServerDb = plexServersDbs.Find(x => x.MachineIdentifier == expectedPlexServer.MachineIdentifier);
            plexServerDb.ShouldNotBeNull();
            plexServerDb.ShouldBe(expectedPlexServer);
            plexServerDb.PlexServerConnections.Count.ShouldBe(expectedPlexServer.PlexServerConnections.Count);
            plexServerDb.PlexServerConnections.ShouldBe(expectedPlexServer.PlexServerConnections, true);
        }
    }

    [Fact]
    public async Task ShouldSyncConnectionsAndKeepTheSameServerConnectionIds_WhenSomeHaveConnectionHaveChanged()
    {
        // Arrange
        var seed = await SetupDatabase(
            23724,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 5;
            }
        );

        var plexServer = IDbContext.PlexServers.Include(x => x.PlexServerConnections).FirstOrDefault();
        plexServer.ShouldNotBeNull();
        plexServer.PlexServerConnections.Count.ShouldBe(5);

        // Update data setup
        plexServer.PlexServerConnections.RemoveRange(0, 4);

        var newConnections = FakeData.GetPlexServerConnections(seed).Generate(5);
        plexServer.PlexServerConnections.AddRange(newConnections);

        // Act
        // Now update
        var request = new AddOrUpdatePlexServersCommand([plexServer]);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexServersDb = IDbContext.PlexServers.Include(x => x.PlexServerConnections).FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.PlexServerConnections.Count.ShouldBe(6);

        foreach (var plexServerConnection in plexServersDb.PlexServerConnections)
        {
            var expectedConnection = plexServer.PlexServerConnections.Find(x => x.Equals(plexServerConnection));
            expectedConnection.ShouldNotBeNull();
            plexServerConnection.Id.ShouldBe(expectedConnection.Id);
            plexServerConnection.ShouldBe(expectedConnection);
        }
    }

    [Fact]
    public async Task ShouldKeepCustomConnections_WhenSomeConnectionHaveChanged()
    {
        // Arrange
        var seed = await SetupDatabase(
            233324,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 5;
            }
        );

        var dbContext = IDbContext;
        var plexServer = IDbContext.PlexServers.Include(x => x.PlexServerConnections).FirstOrDefault();
        plexServer.ShouldNotBeNull();
        plexServer.PlexServerConnections.Count.ShouldBe(5);

        // Add custom connections
        var customConnections = FakeData
            .GetPlexServerConnections(seed, isCustom: true, plexServerId: plexServer.Id)
            .Generate(3);
        await dbContext.PlexServerConnections.AddRangeAsync(customConnections);
        await dbContext.SaveChangesAsync();

        // Update data setup
        plexServer.PlexServerConnections.RemoveRange(0, 4);

        var newConnections = FakeData.GetPlexServerConnections(seed).Generate(5);
        plexServer.PlexServerConnections.AddRange(newConnections);

        // Act
        // Now update
        var request = new AddOrUpdatePlexServersCommand([plexServer]);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        var checkConnections = new List<PlexServerConnection>();
        checkConnections.AddRange(plexServer.PlexServerConnections);
        checkConnections.AddRange(customConnections);

        result.IsSuccess.ShouldBeTrue();
        var plexServersDb = IDbContext.PlexServers.Include(x => x.PlexServerConnections).FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.PlexServerConnections.Count.ShouldBe(checkConnections.Count);

        foreach (var plexServerConnection in plexServersDb.PlexServerConnections)
        {
            var expectedConnection = checkConnections.Find(x => x.Equals(plexServerConnection));
            expectedConnection.ShouldNotBeNull();
            plexServerConnection.Id.ShouldBe(expectedConnection.Id);
            plexServerConnection.ShouldBe(expectedConnection);
        }
    }
}
