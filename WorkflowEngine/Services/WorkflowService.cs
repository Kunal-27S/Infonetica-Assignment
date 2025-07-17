using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class WorkflowService
{
    private readonly Dictionary<string, WorkflowDefinition> _definitions = new();
    private readonly Dictionary<string, WorkflowInstance> _instances = new();
    private readonly Random _random = new();

    // Validate workflow definition
    private bool ValidateDefinition(WorkflowDefinition definition, out string error)
    {
        error = string.Empty;

        // Basic validation
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            error = "Workflow definition must have a valid ID.";
            return false;
        }

        // Check for duplicate IDs
        if (_definitions.ContainsKey(definition.Id))
        {
            error = "Workflow definition with this ID already exists.";
            return false;
        }

        // Validate states
        if (definition.States == null || !definition.States.Any())
        {
            error = "Workflow definition must contain at least one state.";
            return false;
        }

        // Check for exactly one initial state
        var initialStateCount = definition.States.Values.Count(s => s.IsInitial);
        if (initialStateCount != 1)
        {
            error = "Workflow definition must have exactly one initial state.";
            return false;
        }

        // Check for duplicate state IDs
        if (definition.States.Values.GroupBy(s => s.Id).Any(g => g.Count() > 1))
        {
            error = "Duplicate state IDs found in workflow definition.";
            return false;
        }

        // Validate actions
        if (definition.Actions == null)
        {
            error = "Workflow definition must contain actions.";
            return false;
        }

        // Check for duplicate action IDs
        if (definition.Actions.Values.GroupBy(a => a.Id).Any(g => g.Count() > 1))
        {
            error = "Duplicate action IDs found in workflow definition.";
            return false;
        }

        foreach (var action in definition.Actions.Values)
        {
            // Validate action states
            if (string.IsNullOrWhiteSpace(action.ToState))
            {
                error = $"Action {action.Id} must have a valid toState.";
                return false;
            }

            if (!definition.States.ContainsKey(action.ToState))
            {
                error = $"Action {action.Id} references non-existent state {action.ToState}";
                return false;
            }

            if (action.FromStates == null || !action.FromStates.Any())
            {
                error = $"Action {action.Id} must have at least one fromState.";
                return false;
            }

            var invalidStates = action.FromStates.Where(s => !definition.States.ContainsKey(s));
            if (invalidStates.Any())
            {
                error = $"Action {action.Id} references non-existent states: {string.Join(", ", invalidStates)}";
                return false;
            }

            // Check if final states have outgoing actions
            foreach (var fromState in action.FromStates)
            {
                if (definition.States[fromState].IsFinal)
                {
                    error = $"Action {action.Id} cannot originate from final state {fromState}";
                    return false;
                }
            }
        }

        return true;
    }

    public bool AddWorkflowDefinition(WorkflowDefinition definition, out string error)
    {
        if (!ValidateDefinition(definition, out error))
            return false;

        _definitions[definition.Id] = definition;
        return true;
    }

    public WorkflowDefinition? GetWorkflowDefinition(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return _definitions.GetValueOrDefault(id);
    }

    public IEnumerable<WorkflowDefinition> GetAllWorkflowDefinitions()
    {
        return _definitions.Values;
    }

    public WorkflowInstance? StartInstance(string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
        {
            return null;
        }

        var definition = GetWorkflowDefinition(definitionId);
        if (definition == null)
        {
            return null;
        }

        var initialState = definition.GetInitialState();
        if (initialState == null)
        {
            return null;
        }

        var instanceId = GenerateInstanceId();
        var instance = new WorkflowInstance(instanceId, definitionId, initialState.Id);
        _instances[instanceId] = instance;
        return instance;
    }

    public bool ExecuteAction(string instanceId, string actionId, out string error)
    {
        error = string.Empty;

        // Validate input
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            error = "Instance ID cannot be empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(actionId))
        {
            error = "Action ID cannot be empty.";
            return false;
        }

        if (!_instances.TryGetValue(instanceId, out var instance))
        {
            error = "Workflow instance not found.";
            return false;
        }

        var definition = GetWorkflowDefinition(instance.DefinitionId);
        if (definition == null)
        {
            error = "Workflow definition not found.";
            return false;
        }

        if (!definition.Actions.TryGetValue(actionId, out var action))
        {
            error = "Action not found in workflow definition.";
            return false;
        }

        // Check if action is enabled
        if (!action.Enabled)
        {
            error = "Action is disabled.";
            return false;
        }

        // Check if current state is valid for this action
        if (!action.FromStates.Contains(instance.CurrentState))
        {
            error = "Action cannot be executed from current state.";
            return false;
        }

        // Check if we're trying to execute action on final state
        if (definition.States[instance.CurrentState].IsFinal)
        {
            error = "Cannot execute actions on final state.";
            return false;
        }

        // Update instance state and history
        instance.CurrentState = action.ToState;
        instance.History.Add(new InstanceHistory
        {
            ActionId = actionId,
            FromState = instance.CurrentState,
            ToState = action.ToState,
            Timestamp = DateTime.UtcNow
        });

        return true;
    }

    public WorkflowInstance? GetInstance(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return null;
        }

        return _instances.GetValueOrDefault(instanceId);
    }

    public IEnumerable<WorkflowInstance> GetAllInstances()
    {
        return _instances.Values;
    }

    private string GenerateInstanceId()
    {
        return $"INSTANCE-{_random.Next(1000000, 9999999)}";
    }
}
