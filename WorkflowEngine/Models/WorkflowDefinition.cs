using System.Collections.Generic;

namespace WorkflowEngine.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, State> States { get; set; } = new();
    public Dictionary<string, Action> Actions { get; set; } = new();
    public string? Description { get; set; }

    public WorkflowDefinition()
    {
    }

    public WorkflowDefinition(string id, string name, Dictionary<string, State> states, Dictionary<string, Action> actions)
    {
        Id = id;
        Name = name;
        States = states;
        Actions = actions;
    }

    public State? GetInitialState()
    {
        return States.Values.FirstOrDefault(s => s.IsInitial);
    }
}
