# Configurable Workflow Engine

A minimal backend service that implements a configurable state machine workflow engine.

## Quick Start

1. Build and run the project:
   ```bash
   dotnet build
   dotnet run
   ```
2. The API will be available at http://localhost:5092

## API Endpoints

### Workflow Definitions

- `POST /definitions` - Create a new workflow definition
- `GET /definitions/{id}` - Get a specific workflow definition
- `GET /definitions` - List all workflow definitions

### Workflow Instances

- `POST /instances/{definitionId}` - Start a new workflow instance
- `POST /instances/{instanceId}/actions/{actionId}` - Execute an action on an instance
- `GET /instances/{instanceId}` - Get instance details
- `GET /instances` - List all workflow instances

## Response Format

All API responses follow this format:
- Success responses:
```json
{
    "success": true,
    "data": { ... }
}
```

- Error responses:
```json
{
    "success": false,
    "error": "Error message"
}
}
```

## Assumptions

1. In-memory storage - data is lost on restart
2. No authentication/authorization
3. No rate limiting
4. No request validation beyond basic schema

## Error Messages

- Workflow Definition Errors:
  - "Workflow definition must have an Id"
  - "Workflow definition with this ID already exists"
  - "Workflow definition must have exactly one initial state"
  - "Duplicate state IDs found in workflow definition"
  - "Duplicate action IDs found in workflow definition"

- Instance Errors:
  - "Workflow definition not found"
  - "No initial state found in definition"
  - "Instance not found"
  - "Action is disabled"
  - "Action cannot be executed from current state"
  - "Cannot execute actions on final state"
