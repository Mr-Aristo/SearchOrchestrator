using System;
using System.Collections.Generic;
using System.Text;

namespace SearchOrchestrator.Application.Interfaces;

public interface ISearchEngineClient
{
    Task<bool> TriggerIndexingAsync(string sourcePath, CancellationToken cancellationToken);
    Task<IEnumerable<string>> SearchAsync(string query, CancellationToken cancellationToken);
}
