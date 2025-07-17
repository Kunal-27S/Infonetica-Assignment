using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var definitions = new Dictionary<string, WorkflowDefinition>();
var instances = new Dictionary<string, WorkflowInstance>();

// ✅ Health check
app.MapGet("/", () => "Workflow Engine is running.");

// ✅ Create a new workflow definition
app.MapPost("/definitions", (WorkflowDefinition definition) =>
{
    if (definitions.ContainsKey(definition.Id))
        return Results.BadRequest("Definition ID already exists.");

    if (definition.States.Count(s => s.IsInitial) != 1)
        return Results.BadRequest("There must be exactly one initial state.");

    definitions[definition.Id] = definition;
    return Results.Ok("Definition created.");
});

// ✅ Get all workflow definitions
app.MapGet("/definitions", () =>
{
    return Results.Ok(definitions.Values);
});

// ✅ Get a single workflow definition
app.MapGet("/definitions/{id}", (string id) =>
{
    return definitions.TryGetValue(id, out var def)
        ? Results.Ok(def)
        : Results.NotFound("Definition not found.");
});

// ✅ Start workflow instance
app.MapPost("/instances", (StartInstanceRequest request) =>
{
    if (!definitions.ContainsKey(request.DefinitionId))
        return Results.NotFound("Definition not found.");

    var def = definitions[request.DefinitionId];
    var initialState = def.States.First(s => s.IsInitial);

    var instance = new WorkflowInstance
    {
        Id = Guid.NewGuid().ToString(),
        DefinitionId = def.Id,
        CurrentStateId = initialState.Id,
        History = new List<ExecutionRecord>()
    };

    instances[instance.Id] = instance;
    return Results.Ok(instance);
});

// ✅ Get all workflow instances
app.MapGet("/instances", () =>
{
    return Results.Ok(instances.Values);
});

// ✅ Get instance status
app.MapGet("/instances/{id}", (string id) =>
{
    return instances.TryGetValue(id, out var inst)
        ? Results.Ok(inst)
        : Results.NotFound("Instance not found.");
});

// ✅ Execute action on an instance
app.MapPost("/instances/{id}/execute", (string id, ExecuteActionRequest request) =>
{
    if (!instances.TryGetValue(id, out var instance))
        return Results.NotFound("Instance not found.");

    var def = definitions[instance.DefinitionId];
    var action = def.Actions.FirstOrDefault(a => a.Id == request.ActionId);

    if (action == null || !action.Enabled)
        return Results.BadRequest("Invalid or disabled action.");

    if (!action.FromStates.Contains(instance.CurrentStateId))
        return Results.BadRequest("Action not valid from current state.");

    if (!def.States.Any(s => s.Id == action.ToState))
        return Results.BadRequest("Target state does not exist.");

    var current = def.States.First(s => s.Id == instance.CurrentStateId);
    if (current.IsFinal)
        return Results.BadRequest("Cannot act from a final state.");

    instance.CurrentStateId = action.ToState;
    instance.History.Add(new ExecutionRecord { ActionId = action.Id, Timestamp = DateTime.UtcNow });

    return Results.Ok(instance);// 
});

app.Run();


// --- Data Models ---
record State(string Id, string Name, bool IsInitial, bool IsFinal, bool Enabled);
record ActionDef(string Id, string Name, bool Enabled, List<string> FromStates, string ToState);
record WorkflowDefinition(string Id, string Name, List<State> States, List<ActionDef> Actions);

record WorkflowInstance
{
    public required string Id { get; set; }
    public required string DefinitionId { get; set; }
    public required string CurrentStateId { get; set; }
    public required List<ExecutionRecord> History { get; set; }
}

record ExecutionRecord
{
    public required string ActionId { get; set; }
    public required DateTime Timestamp { get; set; }
}

record StartInstanceRequest(string DefinitionId);
record ExecuteActionRequest(string ActionId);
