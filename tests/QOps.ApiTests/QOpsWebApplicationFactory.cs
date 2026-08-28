using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace QOps.ApiTests;

public sealed class QOpsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:QOpsDatabase"] =
                    "Server=localhost,1433;Database=QOpsIntegrationTests;User Id=sa;Password=QOps_dev_2026!;TrustServerCertificate=True;"
            });
        });
    }
}

[CollectionDefinition(Name)]
public sealed class QOpsApiCollection : ICollectionFixture<QOpsWebApplicationFactory>
{
    public const string Name = "QOps API collection";
}