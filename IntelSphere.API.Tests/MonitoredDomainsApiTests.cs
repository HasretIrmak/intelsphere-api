using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using IntelSphere.API.DTOs;
using IntelSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntelSphere.API.Tests;

public class MonitoredDomainsApiTests : ApiTestBase
{
    private const string BaseUrl = "/api/v1/MonitoredDomains";

    private async Task<MonitoredDomainResponseDto> CreateAsync(string domainName = "example.com")
    {
        var dto = new MonitoredDomainCreateDto
        {
            DomainName = domainName,
            CompanyName = "Example Corp",
            ContactEmail = "security@example.com"
        };

        var response = await Client.PostAsJsonAsync(BaseUrl, dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<MonitoredDomainResponseDto>();
        Assert.NotNull(created);
        return created!;
    }

    private Task SeedLeaksAsync(string domain, int count)
    {
        return SeedAsync(async db =>
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

            for (var i = 0; i < count; i++)
            {
                db.LeakedCredentials.Add(new LeakedCredential
                {
                    Email = $"user{i}@{domain}",
                    Domain = domain,
                    PasswordHash = "sample-hash",
                    BreachSourceId = source.Id,
                    DiscoveredAt = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task Create_NormalizesDomain_AndStartsWithZeroRiskScore()
    {
        var created = await CreateAsync("  Example.COM  ");

        Assert.Equal("example.com", created.DomainName);
        Assert.Equal(0, created.RiskScore);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForUnknownId()
    {
        var response = await Client.GetAsync($"{BaseUrl}/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_NormalizesDomain_AndStoresNewValues()
    {
        var created = await CreateAsync();
        var update = new MonitoredDomainCreateDto
        {
            DomainName = "  NEW.Example.COM ",
            CompanyName = "Renamed Corp",
            ContactEmail = "soc@example.com"
        };

        var putResponse = await Client.PutAsJsonAsync($"{BaseUrl}/{created.Id}", update);

        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
        var fetched = await Client.GetFromJsonAsync<MonitoredDomainResponseDto>($"{BaseUrl}/{created.Id}");
        Assert.NotNull(fetched);
        Assert.Equal("new.example.com", fetched!.DomainName);
        Assert.Equal("Renamed Corp", fetched.CompanyName);
        Assert.Equal("soc@example.com", fetched.ContactEmail);
    }

    [Fact]
    public async Task Delete_RemovesDomain()
    {
        var created = await CreateAsync();

        var response = await Client.DeleteAsync($"{BaseUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var afterDelete = await Client.GetAsync($"{BaseUrl}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Theory]
    [InlineData(0, 0, "SECURE")]
    [InlineData(1, 25, "MEDIUM DANGER")]
    [InlineData(2, 50, "HIGH RISK")]
    [InlineData(3, 75, "CRITICAL DANGER")]
    [InlineData(5, 100, "CRITICAL DANGER")]
    public async Task Analyze_ScoresTwentyFivePointsPerLeak_CappedAtOneHundred(
        int leakCount, int expectedScore, string expectedStatus)
    {
        var domain = await CreateAsync("example.com");
        if (leakCount > 0)
        {
            await SeedLeaksAsync("example.com", leakCount);
        }

        var response = await Client.PostAsync($"{BaseUrl}/analyze/example.com", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(leakCount, json.GetProperty("totalLeakCount").GetInt32());
        Assert.Equal(expectedScore, json.GetProperty("calculatedCyberRiskScore").GetInt32());
        Assert.Equal(expectedStatus, json.GetProperty("status").GetString());

        // The calculated score must be persisted on the monitored domain.
        var stored = await Client.GetFromJsonAsync<MonitoredDomainResponseDto>($"{BaseUrl}/{domain.Id}");
        Assert.NotNull(stored);
        Assert.Equal(expectedScore, stored!.RiskScore);
    }

    [Fact]
    public async Task Analyze_ReturnsNotFound_ForUntrackedDomain()
    {
        var response = await Client.PostAsync($"{BaseUrl}/analyze/unknown.example.com", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_IsCaseInsensitive()
    {
        await CreateAsync("example.com");
        await SeedLeaksAsync("example.com", 1);

        var response = await Client.PostAsync($"{BaseUrl}/analyze/EXAMPLE.com", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, json.GetProperty("totalLeakCount").GetInt32());
    }

    [Fact]
    public async Task SimulateLeaks_LoadsSampleCredentials_AndCanBeRepeated()
    {
        // The simulation attaches its records to the breach source with Id 1,
        // so a source must exist (a fresh database assigns Id 1 to the first row).
        await SeedAsync(async db =>
        {
            db.BreachSources.Add(new BreachSource
            {
                SourceName = "Sample Dump",
                BreachDate = new DateTime(2024, 1, 15),
                CreatedAt = DateTime.UtcNow,
                Severity = "High"
            });
            await db.SaveChangesAsync();
        });

        var first = await Client.PostAsync($"{BaseUrl}/simulate-leaks", null);
        var countAfterFirst = await QueryAsync(db => db.LeakedCredentials.CountAsync());
        var second = await Client.PostAsync($"{BaseUrl}/simulate-leaks", null);
        var countAfterSecond = await QueryAsync(db => db.LeakedCredentials.CountAsync());

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.True(countAfterFirst > 0);
        Assert.Equal(countAfterFirst, countAfterSecond);
    }
}
