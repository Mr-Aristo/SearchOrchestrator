namespace SearchOrchestrator.Application.Interfaces;

public interface IIndexTaskRepository
{
    Task<IndexTask?> GetByIdAsync(Guid id);
    Task<IndexTask?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task AddAsync(IndexTask task);
    Task UpdateAsync(IndexTask task);
}
