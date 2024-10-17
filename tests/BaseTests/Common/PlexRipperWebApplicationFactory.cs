using Autofac;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PlexRipper.WebAPI;

namespace PlexRipper.BaseTests;

public class PlexRipperWebApplicationFactory : WebApplicationFactory<Program>
{
    public Seed Seed { get; }

    public readonly string MemoryDbName;

    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(PlexRipperWebApplicationFactory));
    public readonly List<PlexMockServer> PlexMockServers = [];

    public List<Uri> PlexMockServerUris => PlexMockServers.Select(server => server.ServerUri).ToList();

    private readonly UnitTestDataConfig _config;

    public PlexRipperWebApplicationFactory(Seed seed, string memoryDbName, Action<UnitTestDataConfig>? options = null)
    {
        this.WithWebHostBuilder(builder =>
        {
            // Disable caching by using custom configurations
            builder.UseSetting("cacheEnabled", "false");
        });

        Seed = seed;

        MemoryDbName = memoryDbName;
        _config = UnitTestDataConfig.FromOptions(options);
        SetupPlexMockServers(seed, _config);
    }

    private void SetupPlexMockServers(Seed seed, UnitTestDataConfig config)
    {
        var mockConfig = MockPlexApiConfig.FromOptions(config.PlexMockApiOptions);

        foreach (var serverConfig in mockConfig.MockServers)
            PlexMockServers.Add(new PlexMockServer(seed, serverConfig));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureContainer<ContainerBuilder>(autoFacBuilder =>
            autoFacBuilder.RegisterModule(new TestModule { MemoryDbName = MemoryDbName, Config = _config })
        );

        try
        {
            return base.CreateHost(builder);
        }
        catch (Exception e)
        {
            _log.Fatal(e);
            throw;
        }
    }

    protected override void Dispose(bool disposing)
    {
        PlexMockServers.ForEach(server => server.Dispose());

        base.Dispose(disposing);
    }
}
