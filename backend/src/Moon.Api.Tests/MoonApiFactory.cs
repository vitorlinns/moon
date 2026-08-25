using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moon.Api.Data;

namespace Moon.Api.Tests;

/// <summary>
/// Sobe a API real (mesmo Program.cs, mesmo pipeline: CORS, CSRF, auth, rate limit) num
/// servidor em memória, apontando pra um banco Postgres de teste separado do de dev
/// (mesma instância, database diferente, pra não misturar dados).
/// </summary>
public class MoonApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=moon_test;Username=moon;Password=moon";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = TestConnectionString,
                // limite bem folgado pra suíte de testes não tropeçar no rate limit
                ["RateLimit:AuthPermitLimit"] = "100000",
                ["RateLimit:AuthWindowSeconds"] = "60",
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Addresses.ExecuteDeleteAsync();
        await db.PaymentMethods.ExecuteDeleteAsync();
        await db.RefreshTokens.ExecuteDeleteAsync();
        await db.Users.ExecuteDeleteAsync();
        await db.AdminRefreshTokens.ExecuteDeleteAsync();
        await db.AdminUsers.ExecuteDeleteAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Addresses.ExecuteDeleteAsync();
        await db.PaymentMethods.ExecuteDeleteAsync();
        await db.RefreshTokens.ExecuteDeleteAsync();
        await db.Users.ExecuteDeleteAsync();
        await db.AdminRefreshTokens.ExecuteDeleteAsync();
        await db.AdminUsers.ExecuteDeleteAsync();
    }
}
