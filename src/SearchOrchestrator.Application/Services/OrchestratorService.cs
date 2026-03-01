

namespace SearchOrchestrator.Application.Services;

public class OrchestratorService
{
    private readonly IIndexTaskRepository _repository;
    private readonly ISearchEngineClient _searchEngineClient;
    private readonly Channel<Guid> _taskQueue;

    public OrchestratorService(
        IIndexTaskRepository repository,
        ISearchEngineClient searchEngineClient,
        Channel<Guid> taskQueue)
    {
        _repository = repository;
        _searchEngineClient = searchEngineClient;
        _taskQueue = taskQueue;
    }

    /// <summary>
    /// Starts a new indexing task for the specified source path, or returns the identifier of an existing pending or
    /// in-progress task with the same idempotency key.
    /// </summary>
    /// <remarks>This method is idempotent with respect to the provided idempotency key. If called multiple
    /// times with the same key while the corresponding task is pending or in progress, the same task identifier is
    /// returned. This helps prevent duplicate indexing operations for the same source data.</remarks>
    /// <param name="sourcePath">The path to the source data to be indexed. This value must not be null or empty.</param>
    /// <param name="idempotencyKey">A unique key used to ensure that repeated requests with the same key do not create duplicate tasks. This value
    /// must not be null or empty.</param>
    /// <returns>A <see cref="Guid"/> representing the identifier of the indexing task. If a task with the specified idempotency
    /// key is already pending or in progress, its identifier is returned; otherwise, a new task is created and its
    /// identifier is returned.</returns>
    public async Task<Guid> StartIndexingAsync(string sourcePath, string idempotencyKey)
    {
        var existingTask = await _repository.GetByIdempotencyKeyAsync(idempotencyKey);
        if (existingTask != null && (existingTask.Status == Domain.Entities.TaskStatus.Pending || existingTask.Status == Domain.Entities.TaskStatus.InProgress))
        {
            return existingTask.Id; 
        }
        var newTask = new IndexTask
        {
            SourcePath = sourcePath,
            IdempotencyKey = idempotencyKey,
            Status = Domain.Entities.TaskStatus.Pending
        };

        await _repository.AddAsync(newTask);
        await _taskQueue.Writer.WriteAsync(newTask.Id);

        return newTask.Id;
    }

    /// <summary>
    /// Asynchronously searches for items that match the specified query string.
    /// </summary>
    /// <param name="query">The search query used to filter results. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the search operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of strings representing
    /// the matching items. The collection will be empty if no items match the query.</returns>
    public async Task<IEnumerable<string>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        return await _searchEngineClient.SearchAsync(query, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves the status of an indexing task identified by the specified task ID.
    /// </summary>
    /// <param name="taskId">The unique identifier of the indexing task whose status is to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IndexTask"/> associated
    /// with the specified ID, or <see langword="null"/> if no such task exists.</returns>
    public async Task<IndexTask?> GetTaskStatusAsync(Guid taskId)
    {
        return await _repository.GetByIdAsync(taskId);
    }

}
