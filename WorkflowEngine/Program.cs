
using WorkflowEngine.Models;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Register WorkflowService as singleton
builder.Services.AddSingleton<WorkflowService>();

var app = builder.Build();

// Enable JSON response formatting
app.Use(async (context, next) =>
{
    context.Response.ContentType = "application/json";
    await next();
});

// Health check
app.MapGet("/", () => Results.Ok(new { message = "Workflow Engine is running." }));

// Create new workflow definition
app.MapPost("/definitions", async (WorkflowDefinition definition, WorkflowService service) =>
{
    if (string.IsNullOrWhiteSpace(definition.Id))
        return Results.BadRequest(new { error = "Workflow definition must have an Id." });

    if (service.AddWorkflowDefinition(definition, out var error))
    {
        return Results.Ok(new 
        { 
            success = true,
            message = "Workflow definition created successfully",
            data = definition
        });
    }

    return Results.BadRequest(new { 
        success = false,
        error = error 
    });
});

// Get workflow definition by ID
app.MapGet("/definitions/{id}", async (string id, WorkflowService service) =>
{
    var definition = service.GetWorkflowDefinition(id);
    if (definition is not null)
        return Results.Ok(new { 
            success = true,
            data = definition
        });
    
    return Results.NotFound(new { 
        success = false,
        error = "Definition not found." 
    });
});

// List all workflow definitions
app.MapGet("/definitions", async (WorkflowService service) =>
{
    var definitions = service.GetAllWorkflowDefinitions();
    return Results.Ok(new { 
        success = true,
        data = new 
        {
            count = definitions.Count(),
            definitions = definitions
        }
    });
});

// Start new workflow instance
app.MapPost("/instances/{definitionId}", async (string definitionId, WorkflowService service) =>
{
    var instance = service.StartInstance(definitionId);
    if (instance is not null)
    {
        return Results.Ok(new {
            success = true,
            message = "Workflow instance created successfully",
            data = instance
        });
    }
    
    return Results.BadRequest(new { 
        success = false,
        error = "Workflow definition not found or no initial state." 
    });
});

// Execute action on instance
app.MapPost("/instances/{instanceId}/actions/{actionId}", async (string instanceId, string actionId, WorkflowService service) =>
{
    if (service.ExecuteAction(instanceId, actionId, out var error))
        return Results.Ok(new { 
            success = true,
            message = "Action executed successfully",
            data = new
            {
                instanceId = instanceId,
                actionId = actionId
            }
        });

    return Results.BadRequest(new { 
        success = false,
        error = error 
    });
});

// Get instance by ID
app.MapGet("/instances/{instanceId}", async (string instanceId, WorkflowService service) =>
{
    var instance = service.GetInstance(instanceId);
    if (instance is not null)
        return Results.Ok(new { 
            success = true,
            data = instance
        });
    
    return Results.NotFound(new { 
        success = false,
        error = "Instance not found." 
    });
});

// List all instances
app.MapGet("/instances", async (WorkflowService service) =>
{
    var instances = service.GetAllInstances();
    return Results.Ok(new { 
        success = true,
        data = new { 
            count = instances.Count(),
            instances = instances
        }
    });
});

app.Run();


