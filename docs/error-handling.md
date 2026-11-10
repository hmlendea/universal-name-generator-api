# Error handling

This document describes the error taxonomy, exception handling middleware, and response contracts for the Universal Name Generator API.

## Error taxonomy

| Category | HTTP Status | Error Code | Description |
|----------|-------------|------------|-------------|
| Authentication | 401 | `AuthenticationError` | Missing or invalid API key. |
| Authorisation | 403 | `Forbidden` | Scanner protection or policy denial. |
| Validation | 400 | `ValidationError` | Missing/invalid query parameters. |
| Not found | 400 | `SchemaNotFound` | Schema identifier not found. |
| Not found | 404 | `NotFound` | Unknown endpoint. |
| Internal | 500 | `InternalError` | Unhandled exception. |

## Exception handling middleware

`UseNuciApiExceptionHandling` (from `NuciAPI.Middleware.ExceptionHandling`) wraps the entire pipeline.

### Behaviour

1. Catches all unhandled exceptions.
2. Logs the exception with correlation ID.
3. Maps exception types to error responses.
4. Returns `NuciApiErrorResponse` with appropriate status code.

### Exception mapping

| Exception | Status | Error Code |
|-----------|--------|------------|
| `KeyNotFoundException` (schema) | 400 | `SchemaNotFound` |
| `FileNotFoundException` (word list) | 500 | `InternalError` |
| `FormatException` (parsing) | 500 | `InternalError` |
| `NotSupportedException` (unknown command) | 500 | `InternalError` |
| `ArgumentException` (validation) | 400 | `ValidationError` |
| Any other | 500 | `InternalError` |

## Error response contract

All errors return a `NuciApiErrorResponse`:

```json
{
  "success": false,
  "errorCode": "ValidationError",
  "errorMessage": "Schema 'unknown' not found."
}
```

| Field | Type | Description |
|-------|------|-------------|
| `success` | bool | Always `false` for errors. |
| `errorCode` | string | Machine-readable error code. |
| `errorMessage` | string | Human-readable message (no sensitive data). |

## Controller-level validation

`GetNamesRequest` uses data annotations and NuciAPI attributes:

```csharp
public class GetNamesRequest : NuciApiRequest
{
    [NuciApiProperty(HmacOrder = 1)]
    public string Schema { get; set; }

    [NuciApiProperty(HmacOrder = 2, DefaultValue = 1)]
    public int Count { get; set; } = 1;
}
```

Model binding validates:
- `Schema` is present (required).
- `Count` is within 1-100000 (enforced by service).

## Service-level validation

`NameGeneratorService.GetNames` validates:

```csharp
public IReadOnlyCollection<string> GetNames(string schemaId, int count)
{
    if (count < 1 || count > 100000)
        throw new ArgumentException("Count must be between 1 and 100000.");

    var schemas = GetSchemas();
    var schema = schemas.FirstOrDefault(s => s.Id == schemaId);
    if (schema == null)
        throw new KeyNotFoundException($"Schema '{schemaId}' not found.");

    return GenerateNames(schema, count);
}
```

## Logging

All exceptions are logged by the exception handling middleware with:

- Correlation ID
- Exception type and message
- Stack trace (non-production)
- Operation context (if available)

See [Logging](../logging.md).

## Testing

Integration tests in `NamesControllerExceptionHandlingTests.cs` verify:

- Each error condition returns the correct status and error code.
- Error responses match the `NuciApiErrorResponse` contract.
- No stack traces or sensitive data in responses.

See [Testing](../testing.md).