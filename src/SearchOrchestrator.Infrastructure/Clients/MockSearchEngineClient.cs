using SearchOrchestrator.Application.Interfaces;

namespace SearchOrchestrator.Infrastructure.Clients;

public class MockSearchEngineClient : ISearchEngineClient
{
    /// <summary>
    /// Initiates an asynchronous indexing operation for the specified source path using an external search engine.
    /// </summary>
    /// <remarks>The duration of the indexing operation may vary. There is a small chance that the external
    /// service may fail, resulting in an exception. Callers should handle exceptions to ensure robust error
    /// management.</remarks>
    /// <param name="sourcePath">The path to the source data to be indexed. This must be a valid, accessible location.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the indexing operation before it completes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the indexing
    /// operation completes successfully.</returns>
    /// <exception cref="Exception">Thrown if the external search engine encounters a timeout or internal error during indexing.</exception>
    public async Task<bool> TriggerIndexingAsync(string sourcePath, CancellationToken cancellationToken)
    {
        // Simulate variable indexing time and potential failure
        await Task.Delay(Random.Shared.Next(2000, 5000), cancellationToken);

        if (Random.Shared.Next(1, 100) <= 10)
        {
            throw new Exception("External Search Engine Timeout or Internal Error!");
        }

        return true;
    }

    /// <summary>
    /// Asynchronously searches for results that match the specified query string.
    /// </summary>
    /// <param name="query">The search query used to filter results. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the search operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of strings representing
    /// the search results. The collection will be empty if no results are found.</returns>
    public async Task<IEnumerable<string>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        await Task.Delay(500, cancellationToken); // Network delyapy simulation
        return new List<string> { $"Result 1 for '{query}'", $"Result 2 for '{query}'" };
    }
}