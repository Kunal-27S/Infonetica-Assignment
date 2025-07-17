# Workflow Engine API

A simple API backend for modeling workflows as state machines.

## Getting Started

1. Build and run the project:
   ```bash
   dotnet build
   dotnet run
   ```
2. The API will be available at http://localhost:5092

## How It Works

### 1. Create a Workflow
First, define your workflow with states and actions:

```json
POST /definitions
{
    "id": "ORDER_PROCESS",
    "name": "Order Processing",
    "states": {
        "PENDING": { "id": "PENDING", "name": "Pending", "isInitial": true },
        "PROCESSING": { "id": "PROCESSING", "name": "Processing" },
        "COMPLETED": { "id": "COMPLETED", "name": "Completed", "isFinal": true }
    },
    "actions": {
        "START_PROCESS": { "id": "START_PROCESS", "fromStates": ["PENDING"], "toState": "PROCESSING" },
        "COMPLETE_ORDER": { "id": "COMPLETE_ORDER", "fromStates": ["PROCESSING"], "toState": "COMPLETED" }
    }
}
```

### 2. Start a Workflow
Once defined, start a new instance:

```json
POST /instances/ORDER_PROCESS
```

### 3. Execute Actions
Transition between states using actions:

```json
POST /instances/INSTANCE-1234567/actions/START_PROCESS
```

## API Endpoints

### Workflow Definitions
- `POST /definitions` - Create a new workflow
- `GET /definitions/{id}` - Get a specific workflow
- `GET /definitions` - List all workflows

### Workflow Instances
- `POST /instances/{definitionId}` - Start a new instance
- `POST /instances/{instanceId}/actions/{actionId}` - Execute an action
- `GET /instances/{instanceId}` - Get instance details
- `GET /instances` - List all instances

## Response Format

All responses follow this simple format:

```json
{
    "success": true/false,
    "data": { ... }
}
```

## Rules & Tips

1. Each workflow must have exactly one initial state
2. Actions can come from multiple states but go to only one state
3. Final states can't have outgoing actions
4. All data is stored in memory (it's temporary)
5. States and actions can have descriptions (optional)

## Common Errors

- "Workflow definition must have an Id"
- "Workflow definition with this ID already exists"
- "Workflow definition must have exactly one initial state"
- "Instance not found"
- "Action cannot be executed from current state"

## Assumptions

1. **Data Storage**:
   - Uses in-memory storage
   - All data is lost when the application restarts
   - No database persistence

2. **Workflow Structure**:
   - Each workflow must have exactly one initial state
   - Actions can originate from multiple states but always end in one state
   - Final states cannot have outgoing actions
   - All states and actions are enabled by default

3. **Security**:
   - No authentication/authorization
   - No rate limiting
   - No request validation beyond basic schema

4. **Performance**:
   - No caching
   - No request/response logging
   - No error tracking
