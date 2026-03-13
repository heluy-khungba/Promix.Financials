using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Infrastructure;
using Promix.Financials.Infrastructure.Persistence;
using Promix.Financials.Infrastructure.Persistence.Seeding;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var cs = config.GetConnectionString("Promix")
         ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:Promix");

var services = new ServiceCollection();
services.AddInfrastructure(cs);

var sp = services.BuildServiceProvider();

using var scope = sp.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<PromixDbContext>();
var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

await SeedData.EnsureSeedAsync(db, hasher);

Console.WriteLine("✅ Database migrated + seeded successfully.");