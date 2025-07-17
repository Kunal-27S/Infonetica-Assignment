
using WorkflowEngine.Models;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Register WorkflowService as singleton
builder.Services.AddSingleton<WorkflowService>();

var app = builder.Build();

//  Health check
app.MapGet("/", () => "Workflow Engine is running.");

//  Create new workflow definition
app.MapPost("/definitions", (WorkflowDefinition definition, WorkflowService service) =>
{
    if (string.IsNullOrWhiteSpace(definition.Id))
        return Results.BadRequest("Workflow definition must have an Id.");

    if (service.AddWorkflowDefinition(definition, out var error))
        return Results.Ok(definition);

    return Results.BadRequest(error);
});

// Get workflow definition by ID
app.MapGet("/definitions/{id}", (string id, WorkflowService service) =>
{
    var definition = service.GetWorkflowDefinition(id);
    return definition is not null ? Results.Ok(definition) : Results.NotFound("Definition not found.");
});

//  List all workflow definitions
app.MapGet("/definitions", (WorkflowService service) =>
{
    return Results.Ok(service.GetAllWorkflowDefinitions());
});

// Start new workflow instance
app.MapPost("/instances/{definitionId}", (string definitionId, WorkflowService service) =>
{
    var instance = service.StartInstance(definitionId);
    return instance is not null ? Results.Ok(instance) : Results.BadRequest("Workflow definition not found or no initial state.");
});

// Execute action on instance
app.MapPost("/instances/{instanceId}/actions/{actionId}", (string instanceId, string actionId, WorkflowService service) =>
{
    if (service.ExecuteAction(instanceId, actionId, out var error))
        return Results.Ok();

    return Results.BadRequest(error);
});

// Get instance by ID
app.MapGet("/instances/{instanceId}", (string instanceId, WorkflowService service) =>
{
    var instance = service.GetInstance(instanceId);
    return instance is not null ? Results.Ok(instance) : Results.NotFound("Instance not found.");
});

//List all instances
app.MapGet("/instances", (WorkflowService service) =>
{
    return Results.Ok(service.GetAllInstances());
});

app.Run();


