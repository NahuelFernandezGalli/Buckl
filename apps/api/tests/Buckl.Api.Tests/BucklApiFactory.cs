using Buckl.Api.Authentication;
using Buckl.Application.Abstractions;
using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence.Repositories;
using Buckl.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Buckl.Api.Tests;

/// <summary>The API hosted in memory against its own Postgres container, as the application role,
/// with a fixed clock and the test-only probe controllers. One per test assembly.</summary>
public sealed class BucklApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public PostgresDatabase Database { get; } = new();

    public async ValueTask InitializeAsync() => await Database.InitializeAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await Database.DisposeAsync();
    }

    public HttpClient CreateClientFor(string subject)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(DevelopmentAuthenticationHandler.UserHeader, subject);

        return client;
    }

    public async Task<UserId> ProvisionAsync(string subject, CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();

        return await scope.ServiceProvider
            .GetRequiredService<IUserProvisioning>()
            .EnsureUserAsync(subject, cancellationToken);
    }

    public async Task SeedAsync(UserId owner, IEnumerable<Garment> garments, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(garments);

        await using var scope = await UserScope.BeginAsync(Database, owner, cancellationToken);
        var repository = new EfGarmentRepository(scope.Context);

        foreach (var garment in garments)
        {
            await repository.AddAsync(garment, cancellationToken);
        }

        await scope.SaveAndCommitAsync(cancellationToken);
    }

    public async Task SeedProductAsync(Product product, CancellationToken cancellationToken)
    {
        var anyUser = await Database.InsertUserAsync(cancellationToken);
        await using var scope = await UserScope.BeginAsync(Database, anyUser, cancellationToken);
        await new EfProductRepository(scope.Context).AddAsync(product, cancellationToken);
        await scope.SaveAndCommitAsync(cancellationToken);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("ConnectionStrings:Buckl", Database.AppConnectionString);
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(new FixedTimeProvider(TestClock.Now));
            services.AddControllers().AddApplicationPart(typeof(BucklApiFactory).Assembly);
        });
    }
}
