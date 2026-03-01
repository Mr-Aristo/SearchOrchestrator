

namespace SearchOrchestrator.Infrastructure.Repositories;

public class InMemoryTaskRepository : IIndexTaskRepository
{
    private readonly ConcurrentDictionary<Guid, IndexTask> _tasks = new();

    public Task<IndexTask?> GetByIdAsync(Guid id)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task<IndexTask?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        var task = _tasks.Values.FirstOrDefault(t => t.IdempotencyKey == idempotencyKey);
        return Task.FromResult(task);
    }

    public Task AddAsync(IndexTask task)
    {
        _tasks.TryAdd(task.Id, task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(IndexTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _tasks[task.Id] = task; 
        return Task.CompletedTask;
    }
}