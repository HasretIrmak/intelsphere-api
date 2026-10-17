using System;
using System.Collections.Generic;
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

public class HardeningTests : ApiTestBase
{
    private const string SourcesUrl = "/api/v1/BreachSources";
    private const string DomainsUrl = "/api/v1/MonitoredDomains";

    private static MonitoredDomainCreateDto NewDomainDto(
        string domainName = "example.com",
        string email = "security@example.com") => new()
    {
        DomainName = domainName,
        CompanyName = "Example Corp",
        ContactEmail = email
    };

    [Fact]
    public async Task CreateBreachSource_WithEmptyName_ReturnsBadRequest()
    {
        var dto = new BreachSourceCreateDto
        {
            SourceName = "",
            BreachDate = new DateTime(2024, 1, 15),
            Severity = "High"
        };

        var response = await Client.PostAsJsonAsync(SourcesUrl, dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDomain_WithInvalidEmail_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync(DomainsUrl, NewDomainDto(email: "not-an-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDomain_WithBlankDomainName_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync(DomainsUrl, NewDomainDto(domainName: "   "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDomain_LowercasesWithInvariantCulture()
    {
        // With a Turkish culture, ToLower() would turn "I" into a dotless "i".
        var response = await Client.PostAsJsonAsync(DomainsUrl, NewDomainDto(domainName: "INFO.Example.COM"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MonitoredDomainResponseDto>();
        Assert.NotNull(created);
        Assert.Equal("info.example.com", created!.DomainName);
    }

    [Fact]
    public async Task SimulateLeaks_WithoutExistingSource_CreatesOne()
    {
        var response = await Client.PostAsync($"{DomainsUrl}/simulate-leaks", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sourceCount = await QueryAsync(db => db.BreachSources.CountAsync());
        var leakCount = await QueryAsync(db => db.LeakedCredentials.CountAsync());
        Assert.Equal(1, sourceCount);
        Assert.True(leakCount > 0);
    }

    [Fact]
    public async Task SimulateLeaks_UsesOnlyReservedExampleDomains()
    {
        await Client.PostAsync($"{DomainsUrl}/simulate-leaks", null);

        var leaks = await QueryAsync(db => db.LeakedCredentials.ToListAsync());

        Assert.NotEmpty(leaks);
        Assert.All(leaks, leak =>
        {
            Assert.True(
                leak.Domain == "example.com" || leak.Domain == "example.edu",
                $"Sample data must use reserved example domains, but found '{leak.Domain}'.");
            Assert.EndsWith("@" + leak.Domain, leak.Email);
        });
    }

    [Fact]
    public async Task SimulateThenAnalyze_ReportsCriticalRiskForSampleDomain()
    {
        var createResponse = await Client.PostAsJsonAsync(DomainsUrl, NewDomainDto(domainName: "example.edu"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var simulateResponse = await Client.PostAsync($"{DomainsUrl}/simulate-leaks", null);
        Assert.Equal(HttpStatusCode.OK, simulateResponse.StatusCode);

        var analyzeResponse = await Client.PostAsync($"{DomainsUrl}/analyze/example.edu", null);

        Assert.Equal(HttpStatusCode.OK, analyzeResponse.StatusCode);
        var json = await analyzeResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, json.GetProperty("totalLeakCount").GetInt32());
        Assert.Equal(75, json.GetProperty("calculatedCyberRiskScore").GetInt32());
        Assert.Equal("CRITICAL DANGER", json.GetProperty("status").GetString());
    }
}
