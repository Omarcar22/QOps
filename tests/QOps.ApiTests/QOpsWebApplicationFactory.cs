using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QOps.ApiTests;

public sealed class QOpsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var connectionString = Environment.GetEnvironmentVariable("QOPS_TEST_DATABASE_CONNECTION");
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("QOPS_TEST_DATABASE_CONNECTION must be configured for API tests.");
            }

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                jwtKey = "TestJwtKeyForDevelopmentOnly_1234567890";
            }

            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:QOpsDatabase"] = connectionString,
                ["Jwt:Key"] = jwtKey,
                ["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "QOps"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }
}

[CollectionDefinition(Name)]
public sealed class QOpsApiCollection : ICollectionFixture<QOpsWebApplicationFactory>
{
    public const string Name = "QOps API collection";
}