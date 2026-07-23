using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Tests.Integration;

public sealed class RecruitmentApiFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    public RecruitmentApiFactory()
    {
        // These must exist before Program.cs starts.
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Host=localhost;Database=unused;Username=unused;Password=unused");

        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "test-jwt-key-that-is-longer-than-thirty-two-characters-123456");

        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "RecruitmentPlatform.Tests");

        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "RecruitmentPlatform.Tests");

        Environment.SetEnvironmentVariable(
            "Testing__EnsureCreated",
            "true");

        Environment.SetEnvironmentVariable(
            "AiProvider__Enabled",
            "false");

        Environment.SetEnvironmentVariable(
            "CloudStorage__Provider",
            "Local");

        _connection.Open();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType ==
                        typeof(DbContextOptions<ApplicationDbContext>) ||
                    descriptor.ServiceType ==
                        typeof(DbContextOptions) ||
                    descriptor.ServiceType ==
                        typeof(ApplicationDbContext) ||
                    descriptor.ServiceType.FullName?.Contains(
                        "IDbContextOptionsConfiguration",
                        StringComparison.Ordinal) == true)
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(
                options =>
                    options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}