# Search Orchestrator Service

A lightweight, asynchronous Orchestrator Service built with **.NET (C#)**. This service manages the process of reading/indexing files in an external Search Engine and provides an API for string-based searching.

## 🏗 Architecture Overview

The project follows the principles of **Clean Architecture** to ensure the separation of concerns, testability, and maintainability. It is divided into four logical layers:

1. **Domain (`SearchOrchestrator.Domain`)**: Contains the core business entities (`IndexTask`) and enums (`TaskStatus`). It has zero external dependencies.
2. **Application (`SearchOrchestrator.Application`)**: Contains the core business rules and orchestration logic (`OrchestratorService`). It defines interfaces for external dependencies (Repositories, External Clients).
3. **Infrastructure (`SearchOrchestrator.Infrastructure`)**: Implements the interfaces defined in the Application layer. It includes:
   - `InMemoryTaskRepository`: A thread-safe, in-memory data store for tasks.
   - `MockSearchEngineClient`: A mocked external HTTP client simulating network delays and random failures.
   - `IndexTaskBackgroundService`: A background worker (`IHostedService`) that processes tasks asynchronously.
4. **API (`SearchOrchestrator.API`)**: The entry point of the application using modern **.NET Minimal APIs** and Top-Level Statements.

## 🧠 Key Design Decisions & Orchestration Logic

- **Asynchronous Processing**: Indexing requests immediately return a `202 Accepted` response with a `TaskId`. The actual indexing request to the external search engine is handled asynchronously in the background.
- **Thread-Safe Queuing**: The project uses `System.Threading.Channels` (`Channel<Guid>`) to pass task IDs from the API layer to the Background Worker safely and efficiently.
- **Idempotency**: To prevent duplicate processing and system overload, the API checks for existing active tasks (`Pending` or `InProgress`) using an `IdempotencyKey`. If a duplicate request arrives, it gracefully returns the existing `TaskId` without creating a new job.
- **Error Handling & Resilience**: The `MockSearchEngineClient` occasionally simulates external service failures. The `IndexTaskBackgroundService` catches these exceptions, updates the task status to `Failed`, and logs the error message, ensuring the orchestrator doesn't crash.

## 🚀 API Contracts (Endpoints)

Base URL: `http://localhost:port/api/v1`

| Method | Endpoint | Description | Request Body / Query |
| :--- | :--- | :--- | :--- |
| `POST` | `/indexing` | Starts a new indexing task | `{ "sourcePath": "/data", "idempotencyKey": "opt-key" }` |
| `GET` | `/indexing/{taskId}` | Gets the status of a specific task | *None* |
| `GET` | `/search` | Executes a search query | `?query=your_text` |

## 🛠 How to Run the Project

1. Make sure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed.
2. Clone the repository.
3. Navigate to the API folder and run the application:
   ```bash
   cd SearchOrchestrator.API
   dotnet run

   
