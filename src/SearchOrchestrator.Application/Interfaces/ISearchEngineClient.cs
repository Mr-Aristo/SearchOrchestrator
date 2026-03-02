using System;
using System.Collections.Generic;
using System.Text;

namespace SearchOrchestrator.Application.Interfaces;

/// <summary>
/// Contract for communicating with the External Search Engine.
/// Within the scope of this test project, this interface is mocked by "MockSearchEngineClient".
/// In a real-world scenario, this would be an HttpClient implementation making requests to the external service's REST API.
/// </summary>
public interface ISearchEngineClient
{
    /// <summary>
    /// Sends a command to the external search engine to read and index files from the specified source.
    /// The orchestrator only delegates the task; the actual file reading/indexing is handled entirely by the external service.
    /// </summary>
    /// <param name="sourcePath">The source path or URI where the files to be indexed are located.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns true if the external service successfully accepts and completes the indexing request.</returns>
    Task<bool> TriggerIndexingAsync(string sourcePath, CancellationToken cancellationToken);

    /// <summary>
    /// Forwards the user's search text to the external search engine and performs a search over the indexed data.
    /// </summary>
    /// <param name="query">The text string the user is searching for.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An enumerable collection of strings representing the search results.</returns>
    Task<IEnumerable<string>> SearchAsync(string query, CancellationToken cancellationToken);
}
