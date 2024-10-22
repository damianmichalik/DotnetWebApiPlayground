using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.PostgreSql;
using WebApiPlayground.Api.Infrastructure;

namespace WebApiPlayground.Api.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.4")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithDatabase("webapi-test")
        .Build();

    private Respawner _respawner = null!; 
    private DbConnection _connection = null!;    

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptorType =
                typeof(DbContextOptions<AppDbContext>);

            var descriptor = services
                .SingleOrDefault(s => s.ServiceType == descriptorType);

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString())
            );
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var Db = Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        _connection = Db.Database.GetDbConnection();
        await _connection.OpenAsync();
        Db.Database.Migrate();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public new Task DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }

    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_connection);
    }
}