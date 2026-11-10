# Configuration

This document describes the configuration schema, sources, precedence, and settings reference for the Universal Name Generator API.

## Configuration sources

The API uses ASP.NET Core's default configuration providers in this precedence order (highest wins):

1. Command-line arguments
2. Environment variables
3. `appsettings.{Environment}.json`
4. `appsettings.json`

## Settings reference

### dataStoreSettings

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `wordListsRootDirectory` | string | Yes | `Data/Wordlists` | Root directory for `.lst` word list files. |
| `generationSchemasPath` | string | Yes | `Data/GenerationSchemas.xml` | Path to the XML schema file. |

Paths are relative to the application's working directory unless absolute.

### securitySettings

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `apiKey` | string | Yes | `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]` | API key for authentication. Must be replaced before running. |

The placeholder `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]` is not a valid key. Generate a secure random string for production.

### nuciLoggerSettings

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `logFilePath` | string | No | `logfile.log` | Path to the log file. |
| `isFileOutputEnabled` | bool | No | `true` | Whether to write logs to the file. |

### Kestrel (ASP.NET Core defaults)

The API uses Kestrel defaults. Override via `appsettings.json` or environment variables:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://localhost:5000" },
      "Https": { "Url": "https://localhost:5001" }
    }
  }
}
```

### CORS (NuciAPI)

NuciAPI CORS middleware reads from configuration. See NuciAPI documentation for the schema.

## appsettings.json example

```json
{
  "dataStoreSettings": {
    "wordListsRootDirectory": "Data/Wordlists",
    "generationSchemasPath": "Data/GenerationSchemas.xml"
  },
  "securitySettings": {
    "apiKey": "your-secure-api-key-here"
  },
  "nuciLoggerSettings": {
    "logFilePath": "logfile.log",
    "isFileOutputEnabled": true
  },
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://localhost:5000" },
      "Https": { "Url": "https://localhost:5001" }
    }
  }
}
```

## Environment variables

Use double underscore `__` for nesting:

```bash
export dataStoreSettings__wordListsRootDirectory=/var/data/wordlists
export dataStoreSettings__generationSchemasPath=/var/data/GenerationSchemas.xml
export securitySettings__apiKey=your-secure-api-key
export nuciLoggerSettings__logFilePath=/var/log/api.log
export nuciLoggerSettings__isFileOutputEnabled=true
```

## Configuration binding

`Startup.ConfigureServices` binds sections to POCOs:

```csharp
services.Configure<DataStoreSettings>(configuration.GetSection("dataStoreSettings"));
services.Configure<SecuritySettings>(configuration.GetSection("securitySettings"));
```

`ServiceCollectionExtensions.AddCustomServices` unwraps them from `IOptions<T>` and registers as singletons:

```csharp
services.AddSingleton(sp => sp.GetRequiredService<IOptions<DataStoreSettings>>().Value);
services.AddSingleton(sp => sp.GetRequiredService<IOptions<SecuritySettings>>().Value);
```

## Reload behaviour

Configuration is read at startup. Changes to `appsettings.json` or environment variables require a process restart. No hot reload is implemented.

## Secret management

The `apiKey` is a secret. Do not commit it to version control. Use environment variables or a secret manager in production. The placeholder in `appsettings.json` prevents accidental commits of real keys.

## Validation

No automatic configuration validation is implemented. The API fails at runtime if required settings are missing or invalid:

- Missing `apiKey` -> authentication fails for all requests.
- Missing data paths -> file not found exceptions on first request.

See [Troubleshooting](../troubleshooting.md) for common configuration issues.