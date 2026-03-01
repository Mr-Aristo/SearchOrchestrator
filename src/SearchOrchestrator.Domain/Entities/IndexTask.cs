namespace SearchOrchestrator.Domain.Entities;

public class IndexTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SourcePath { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}
