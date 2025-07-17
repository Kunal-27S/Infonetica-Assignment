using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WorkflowEngine.Models;

public class WorkflowInstance
{
    public string Id { get; set; } = string.Empty;
    public string DefinitionId { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public List<InstanceHistory> History { get; set; } = new();

    public WorkflowInstance()
    {
    }

    public WorkflowInstance(string id, string definitionId, string initialState)
    {
        Id = id;
        DefinitionId = definitionId;
        CurrentState = initialState;
        History = new List<InstanceHistory>
        {
            new InstanceHistory
            {
                ActionId = "INITIAL",
                FromState = "",
                ToState = initialState,
                Timestamp = DateTime.UtcNow
            }
        };
    }
}

public class InstanceHistory
{
    public string ActionId { get; set; } = string.Empty;
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
