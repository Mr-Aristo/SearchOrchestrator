using Microsoft.AspNetCore.Mvc;
using SearchOrchestrator.API.Modals;
using SearchOrchestrator.Application.Services;

namespace SearchOrchestrator.API.Endpoints;

public static class OrchestratorEndpoints
{
    public static void MapOrchestratorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1");

        group.MapPost("/indexing", async ([FromBody] IndexRequest request, OrchestratorService orchestratorService) =>
        {
            var idempotencyKey = request.IdempotencyKey ?? request.SourcePath.GetHashCode().ToString();
            var taskId = await orchestratorService.StartIndexingAsync(request.SourcePath, idempotencyKey);

            return Results.Accepted($"/api/v1/indexing/{taskId}", new { TaskId = taskId, Message = "Indexing task accepted." });
        })
        .WithName("StartIndexing")
        .WithOpenApi();

        // 2. Görev Durumu Sorgulama
        group.MapGet("/indexing/{taskId:guid}", async (Guid taskId, OrchestratorService orchestratorService) =>
        {
            var task = await orchestratorService.GetTaskStatusAsync(taskId);
            if (task == null) return Results.NotFound();

            return Results.Ok(new
            {
                task.Id,
                Status = task.Status.ToString(),
                task.ErrorMessage,
                task.CreatedAt
            });
        })
        .WithName("GetTaskStatus")
        .WithOpenApi();


        group.MapGet("/search", async ([FromQuery] string query, OrchestratorService orchestratorService, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(query)) return Results.BadRequest("Query is required.");

            var results = await orchestratorService.SearchAsync(query, cancellationToken);
            return Results.Ok(results);
        })
        .WithName("Search")
        .WithOpenApi();
    }
}