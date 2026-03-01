using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace SearchOrchestrator.Infrastructure.Workers;

public class IndexTaskBackgroundService : BackgroundService
{
    private readonly Channel<Guid> _taskQueue;
    private readonly IIndexTaskRepository _repository;
    private readonly ISearchEngineClient _searchEngineClient;
    private readonly ILogger<IndexTaskBackgroundService> _logger;

    public IndexTaskBackgroundService(
        Channel<Guid> taskQueue,
        IIndexTaskRepository repository,
        ISearchEngineClient searchEngineClient,
        ILogger<IndexTaskBackgroundService> logger)
    {
        _taskQueue = taskQueue;
        _repository = repository;
        _searchEngineClient = searchEngineClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Indexing Service is starting.");

        await foreach (var taskId in _taskQueue.Reader.ReadAllAsync(stoppingToken))
        {
            var task = await _repository.GetByIdAsync(taskId);
            if (task == null) continue;

            try
            {
                task.Status = Domain.Entities.TaskStatus.InProgress;
                await _repository.UpdateAsync(task);
                _logger.LogInformation($"Task {taskId} is in progress...");

         
                await _searchEngineClient.TriggerIndexingAsync(task.SourcePath, stoppingToken);

                task.Status = Domain.Entities.TaskStatus.Completed;
                await _repository.UpdateAsync(task);
                _logger.LogInformation($"Task {taskId} completed successfully.");
            }
            catch (Exception ex)
            {
                task.Status = Domain.Entities.TaskStatus.Failed;
                task.ErrorMessage = ex.Message;
                await _repository.UpdateAsync(task);
                _logger.LogError(ex, $"Task {taskId} failed!");
            }
        }
    }
}