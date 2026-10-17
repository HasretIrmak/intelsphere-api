using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IntelSphere.API.DTOs;
using IntelSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntelSphere.API.Tests;

public class BreachSourcesApiTests : ApiTestBase
{
    private const string BaseUrl = "/api/v1/BreachSources";

    private static BreachSourceCreateDto NewDto(string name = "Sample Forum Dump", string severity = "High") => new()
    {
        SourceName = name,
        BreachDate = new DateTime(2024, 1, 15),
        Severity = severity
    };

    private async Task<BreachSourceResponseDto> CreateAsync(BreachSourceCreateDto dto)
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<BreachSourceResponseDto>();
        Assert.NotNull(created);
        return created!;
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoSourcesExist()
    {
        var response = await Client.GetAsync(BaseUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<BreachSourceResponseDto>>();
        Assert.NotNull(items);
        Assert.Empty(items!);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithLocationHeaderAndBody()
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, NewDto());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<BreachSourceResponseDto>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("Sample Forum Dump", created.SourceName);
        Assert.Equal("High", created.Severity);
        Assert.Equal(new DateTime(2024, 1, 15), created.BreachDate);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"{BaseUrl}/{created.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_SetsCreatedAtTimestamp()
    {
        var created = await CreateAsync(NewDto());

        Assert.True(
            created.CreatedAt > new DateTime(2020, 1, 1),
            "CreatedAt must be populated when a source is created.");
    }

    [Fact]
    public async Task GetById_ReturnsSource_WhenItExists()
    {
        var created = await CreateAsync(NewDto("Paste Site Leak", "Medium"));

        var response = await Client.GetAsync($"{BaseUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<BreachSourceResponseDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Paste Site Leak", fetched.SourceName);
        Assert.Equal("Medium", fetched.Severity);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForUnknownId()
    {
        var response = await Client.GetAsync($"{BaseUrl}/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesStoredValues()
    {
        var created = await CreateAsync(NewDto());
        var update = new BreachSourceCreateDto
        {
            SourceName = "Renamed Source",
            BreachDate = new DateTime(2025, 3, 1),
            Severity = "Low"
        };

        var putResponse = await Client.PutAsJsonAsync($"{BaseUrl}/{created.Id}", update);

        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
        var fetched = await Client.GetFromJsonAsync<BreachSourceResponseDto>($"{BaseUrl}/{created.Id}");
        Assert.NotNull(fetched);
        Assert.Equal("Renamed Source", fetched!.SourceName);
        Assert.Equal("Low", fetched.Severity);
        Assert.Equal(new DateTime(2025, 3, 1), fetched.BreachDate);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_ForUnknownId()
    {
        var response = await Client.PutAsJsonAsync($"{BaseUrl}/999", NewDto());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesSource_AndItsLeakedCredentials()
    {
        var created = await CreateAsync(NewDto());
        await SeedAsync(async db =>
        {
            db.LeakedCredentials.Add(new LeakedCredential
            {
                Email = "user@example.com",
                Domain = "example.com",
                PasswordHash = "sample-hash",
                BreachSourceId = created.Id,
                DiscoveredAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        });

        var response = await Client.DeleteAsync($"{BaseUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var afterDelete = await Client.GetAsync($"{BaseUrl}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        var remainingCredentials = await QueryAsync(db => db.LeakedCredentials.CountAsync());
        Assert.Equal(0, remainingCredentials);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_ForUnknownId()
    {
        var response = await Client.DeleteAsync($"{BaseUrl}/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
