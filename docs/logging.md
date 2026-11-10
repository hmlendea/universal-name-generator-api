# Logging

This document describes the NuciLog framework, structured fields, sinks, and redaction used by the Universal Name Generator API.

## Framework

The API uses `NuciLog` for structured logging. It is registered as a singleton `ILogger` in `ServiceCollectionExtensions`:

```csharp
services.AddSingleton<ILogger>(sp => new NuciLogger(sp.GetRequiredService<IOptions<NuciLoggerSettings>>().Value));
```

## Configuration

NuciLog is configured via `nuciLoggerSettings` in `appsettings.json`:

```json
{
  "nuciLoggerSettings": {
    "logFilePath": "logfile.log",
    "isFileOutputEnabled": true
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `logFilePath` | string | `logfile.log` | Path to the log file. |
| `isFileOutputEnabled` | bool | `true` | Whether to write to the file sink. |

## Structured logging

NuciLog uses operation names and info keys for structured logging.

### Operations

Defined in `UniversalNameGenerator.API/Logging/MyOperation.cs`:

```csharp
public static class MyOperation
{
    public const string GenerateNames = "GenerateNames";
    public const string GetSchemas = "GetSchemas";
}
```

| Operation | When logged |
|-----------|-------------|
| `GenerateNames` | Each `GET /Names` request that reaches the service. |
| `GetSchemas` | When the schema cache is populated (first request). |

### Info keys

Defined in `UniversalNameGenerator.API/Logging/MyLogInfoKey.cs`:

```csharp
public static class MyLogInfoKey
{
    public const string Schema = "Schema";
    public const string Count = "Count";
}
```

| Info key | Type | Description |
|----------|------|-------------|
| `Schema` | string | The schema identifier requested. |
| `Count` | int | The requested name count. |

### Log entry example

```json
{
  "timestamp": "2026-11-10T10:30:45.123Z",
  "level": "Information",
  "operation": "GenerateNames",
  "schema": "arabic-toponyms",
  "count": 5,
  "durationMs": 12,
  "correlationId": "abc-123"
}
```

## Sinks

- **File sink:** Enabled by `isFileOutputEnabled`. Writes JSON lines to `logFilePath`.
- **Console sink:** Enabled by ASP.NET Core default logging. Writes to stdout/stderr.

## Redaction

NuciLog does not automatically redact sensitive values. The API does not log the API key or any secrets. The `Authorization` header is not logged by the request logging middleware.

## Request logging middleware

`UseNuciApiRequestLogging` logs incoming requests and responses with:

- HTTP method and path
- Status code
- Duration
- Correlation ID
- Selected headers (not Authorization)

## Log levels

| Level | Usage |
|-------|-------|
| `Trace` | Verbose diagnostic detail. |
| `Debug` | Debugging information. |
| `Information` | Normal operations (GenerateNames, GetSchemas). |
| `Warning` | Recoverable issues (missing filter list, empty generation). |
| `Error` | Failures (exceptions, missing files). |
| `Critical` | Process-threatening failures. |

## Testing

Integration tests verify request logging behaviour. Unit tests do not test logging directly.

See [Testing](../testing.md).