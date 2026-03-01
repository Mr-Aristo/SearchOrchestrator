using System.Threading.Channels;
using Moq;
using SearchOrchestrator.Application.Interfaces;
using SearchOrchestrator.Application.Services;
using SearchOrchestrator.Domain.Entities;
using TaskStatus = SearchOrchestrator.Domain.Entities.TaskStatus;

namespace SearchOrchestrator.Tests.Services;

public class OrchestratorServiceTests
{
    private readonly Mock<IIndexTaskRepository> _mockRepository;
    private readonly Mock<ISearchEngineClient> _mockSearchClient;
    private readonly Channel<Guid> _taskQueue;
    private readonly OrchestratorService _service;

    public OrchestratorServiceTests()
    {
        _mockRepository = new Mock<IIndexTaskRepository>();
        _mockSearchClient = new Mock<ISearchEngineClient>();
        _taskQueue = Channel.CreateUnbounded<Guid>();

        _service = new OrchestratorService(
            _mockRepository.Object,
            _mockSearchClient.Object,
            _taskQueue);
    }

    /// <summary>
    /// Verifies that when an idempotency key already exists and the associated indexing task is pending, the
    /// StartIndexingAsync method returns the existing task identifier instead of creating a new task.
    /// </summary>
    /// <remarks>This test ensures idempotent behavior by confirming that repeated requests with the same
    /// idempotency key do not result in duplicate indexing tasks when the original task is still pending.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>

    [Fact]
    public async Task StartIndexingAsync_WhenIdempotencyKeyExistsAndPending_ShouldReturnExistingTaskId()
    {
        // Arrange 
        var idempotencyKey = "test-key-123";
        var existingTaskId = Guid.NewGuid();
        var existingTask = new IndexTask
        {
            Id = existingTaskId,
            IdempotencyKey = idempotencyKey,
            Status = TaskStatus.Pending
        };

        _mockRepository.Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey))
                       .ReturnsAsync(existingTask);

        // Act 
        var resultId = await _service.StartIndexingAsync("/path/to/files", idempotencyKey);

        // Assert 
        Assert.Equal(existingTaskId, resultId);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<IndexTask>()), Times.Never);
    }

    /// <summary>
    /// Verifies that a new indexing request results in the creation of a new indexing task and that the task is
    /// correctly queued for processing.
    /// </summary>
    /// <remarks>This test ensures that when a new idempotency key is provided, the service creates a new
    /// indexing task, adds it to the repository, and enqueues its identifier for processing. It also verifies that the
    /// returned identifier is not empty and matches the queued task.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task StartIndexingAsync_WhenNewRequest_ShouldCreateTaskAndQueueIt()
    {
        // Arrange
        var idempotencyKey = "new-key-456";
        _mockRepository.Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey))
                       .ReturnsAsync((IndexTask?)null); 

        // Act
        var resultId = await _service.StartIndexingAsync("/path/to/new/files", idempotencyKey);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        _mockRepository.Verify(r => r.AddAsync(It.Is<IndexTask>(t => t.Id == resultId)), Times.Once); 

        Assert.True(_taskQueue.Reader.TryRead(out var queuedId));
        Assert.Equal(resultId, queuedId);
    }

    /// <summary>
    /// Verifies that the SearchAsync method of the service delegates the search operation to the underlying search
    /// client.
    /// </summary>
    /// <remarks>This unit test ensures that when SearchAsync is called on the service, it correctly invokes
    /// the SearchAsync method of the injected search client with the provided query and cancellation token. The test
    /// also checks that the results returned by the service match those from the search client, and that the search
    /// client is called exactly once.</remarks>
    /// <returns></returns>
    [Fact]
    public async Task SearchAsync_ShouldDelegateToSearchClient()
    {
        // Arrange
        var query = "test document";
        var expectedResults = new List<string> { "Result 1", "Result 2" };

        _mockSearchClient.Setup(c => c.SearchAsync(query, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(expectedResults);

        // Act
        var results = await _service.SearchAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResults, results);
        _mockSearchClient.Verify(c => c.SearchAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}