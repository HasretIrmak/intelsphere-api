using IntelSphere.API.Data;
using IntelSphere.API.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntelSphere.API.Tests;

public class DataModelTests
{
    private static AppDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void MonitoredDomain_IsPersistedAndReadBack()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        context.Set<MonitoredDomain>().Add(new MonitoredDomain
        {
            DomainName = "example.com",
            CompanyName = "Example Corp",
            ContactEmail = "security@example.com",
            RiskScore = 42,
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();

        var saved = context.Set<MonitoredDomain>().Single();
        Assert.Equal("example.com", saved.DomainName);
        Assert.Equal(42, saved.RiskScore);
    }

    [Fact]
    public void DeletingBreachSource_CascadesToLeakedCredentials()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var source = new BreachSource
        {
            SourceName = "Sample Forum Dump",
            BreachDate = new DateTime(2024, 1, 15),
            CreatedAt = DateTime.UtcNow,
            Severity = "High"
        };
        context.Set<BreachSource>().Add(source);
        context.SaveChanges();

        context.Set<LeakedCredential>().Add(new LeakedCredential
        {
            Email = "user@example.com",
            Domain = "example.com",
            PasswordHash = "sample-hash",
            BreachSourceId = source.Id,
            DiscoveredAt = DateTime.UtcNow
        });
        context.SaveChanges();
        Assert.Equal(1, context.Set<LeakedCredential>().Count());

        context.Set<BreachSource>().Remove(source);
        context.SaveChanges();

        Assert.Equal(0, context.Set<LeakedCredential>().Count());
    }
}