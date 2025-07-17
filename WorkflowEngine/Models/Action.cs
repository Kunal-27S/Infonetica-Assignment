using System.Collections.Generic;

namespace WorkflowEngine.Models;

public class Action
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public List<string> FromStates { get; set; } = new();
    public string ToState { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Action()
    {
    }

    public Action(string id, string name, List<string> fromStates, string toState, bool enabled = true)
    {
        Id = id;
        Name = name;
        FromStates = fromStates;
        ToState = toState;
        Enabled = enabled;
    }
}
