using SearchOrchestrator.Application;
using SearchOrchestrator.Infrastructure;
using SearchOrchestrator.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapOrchestratorEndpoints();

app.Run();