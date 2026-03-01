namespace SearchOrchestrator.API.Modals;

public class IndexRequest
{
    public string SourcePath { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
}