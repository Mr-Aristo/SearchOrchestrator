using Microsoft.AspNetCore.Mvc.Testing;
using SearchOrchestrator.API;
using SearchOrchestrator.API.Endpoints; 
using SearchOrchestrator.API.Modals;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SearchOrchestrator.Tests.API;

public class OrchestratorEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrchestratorEndpointsTests(WebApplicationFactory<Program> factory)
    {
        // run the API in-memory for testing
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that posting a valid indexing request returns an Accepted status code and a non-empty task identifier.
    /// </summary>
    /// <remarks>This test ensures that the indexing API responds with HTTP 202 Accepted and provides a valid task ID
    /// when a new indexing request is submitted. The test uses a predefined idempotency key and source path to simulate a
    /// typical indexing scenario.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task PostIndexing_ShouldReturnAcceptedAndTaskId()
    {
        // Arrange
        var request = new IndexRequest
        {
            SourcePath = "/data/docs",
            IdempotencyKey = "api-test-key-1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/indexing", request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var responseContent = await response.Content.ReadFromJsonAsync<IndexingResponse>();
        Assert.NotNull(responseContent);
        Assert.NotEqual(Guid.Empty, responseContent.TaskId);
    }

    /// <summary>
    /// Verifies that a GET request to the search endpoint without a query parameter returns a BadRequest response.
    /// </summary>
    /// <remarks>This test ensures that the API correctly handles requests missing required query parameters by
    /// returning an HTTP 400 Bad Request status code.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GetSearch_WithoutQuery_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/search?query=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record IndexingResponse(Guid TaskId, string Message);
}