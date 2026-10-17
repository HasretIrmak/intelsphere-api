using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using IntelSphere.API.Models;
using Xunit;

namespace IntelSphere.API.Tests;

public class DashboardApiTests : ApiTestBase
{
    private const string OverviewUrl = "/api/v1/Dashboard/overview";

    private Task SeedDomainsAsync(params int[] riskScores)
    {
        return SeedAsync(async db =>
        {
            for (var i = 0; i < riskScores.Length; i++)
            {
                db.MonitoredDomains.Add(new MonitoredDomain
                {
                    DomainName = $"company{i}.example.com",
                    CompanyName = $"Company {i}",
                    ContactEmail = $"security@company{i}.example.com",
                    RiskScore = riskScores[i],
                    CreatedAt = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task Overview_OnEmptyDatabase_ReportsSecureWithNoCompanies()
    {
        var response = await Client.GetAsync(OverviewUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var stats = json.GetProperty("systemGeneralStatistics");
        Assert.Equal(0, stats.GetProperty("totalMonitoredDomains").GetInt32());
        Assert.Equal(0, stats.GetProperty("totalBreachSources").GetInt32());
        Assert.Equal(0, stats.GetProperty("totalLeakedCredentialsInDatabase").GetInt32());
        Assert.Equal(0.0, stats.GetProperty("monitoredCompaniesAverageRiskScore").GetDouble());
        Assert.Equal("LOW LEVEL / SECURE", json.GetProperty("generalCyberThreatLevel").GetString());
        Assert.Equal(0, json.GetProperty("topRiskyCompanies").GetArrayLength());
    }

    [Fact]
    public async Task Overview_ReturnsCountsAverageAndTopThreeRiskiestCompanies()
    {
        await SeedDomainsAsync(100, 60, 20, 0);
        await SeedAsync(async db =>
        {
            var source = new BreachSource
            {
                SourceName = "Sample Dump",
                BreachDate = new DateTime(2024, 1, 15),
                CreatedAt = DateTime.UtcNow,
                Severity = "High"
            };
            db.BreachSources.Add(source);
            await db.SaveChangesAsync();

            db.LeakedCredentials.Add(new LeakedCredential
            {
                Email = "a@example.com",
                Domain = "example.com",
                PasswordHash = "sample-hash",
                BreachSourceId = source.Id,
                DiscoveredAt = DateTime.UtcNow
            });
            db.LeakedCredentials.Add(new LeakedCredential
            {
                Email = "b@example.com",
                Domain = "example.com",
                PasswordHash = "sample-hash",
                BreachSourceId = source.Id,
                DiscoveredAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        });

        var response = await Client.GetAsync(OverviewUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var stats = json.GetProperty("systemGeneralStatistics");
        Assert.Equal(4, stats.GetProperty("totalMonitoredDomains").GetInt32());
        Assert.Equal(1, stats.GetProperty("totalBreachSources").GetInt32());
        Assert.Equal(2, stats.GetProperty("totalLeakedCredentialsInDatabase").GetInt32());
        Assert.Equal(45.0, stats.GetProperty("monitoredCompaniesAverageRiskScore").GetDouble());
        Assert.Equal("MEDIUM LEVEL CYBER ACTIVITY", json.GetProperty("generalCyberThreatLevel").GetString());

        var top = json.GetProperty("topRiskyCompanies");
        Assert.Equal(3, top.GetArrayLength());
        Assert.Equal(100, top[0].GetProperty("riskScore").GetInt32());
        Assert.Equal("Critical", top[0].GetProperty("riskLevel").GetString());
        Assert.Equal(60, top[1].GetProperty("riskScore").GetInt32());
        Assert.Equal("High", top[1].GetProperty("riskLevel").GetString());
        Assert.Equal(20, top[2].GetProperty("riskScore").GetInt32());
        Assert.Equal("Medium/Low", top[2].GetProperty("riskLevel").GetString());
    }

    [Theory]
    [InlineData(80, "CRITICAL CONDITION (RED ALERT)")]
    [InlineData(60, "HIGH THREAT (ORANGE ALERT)")]
    [InlineData(20, "MEDIUM LEVEL CYBER ACTIVITY")]
    public async Task Overview_MapsAverageRiskScoreToThreatLevel(int riskScore, string expectedLevel)
    {
        await SeedDomainsAsync(riskScore);

        var response = await Client.GetAsync(OverviewUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedLevel, json.GetProperty("generalCyberThreatLevel").GetString());
    }
}
