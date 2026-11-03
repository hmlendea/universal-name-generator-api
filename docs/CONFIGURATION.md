# Universal Name Generator API - Configuration Documentation

## Overview

This document provides comprehensive documentation for all configuration options in the Universal Name Generator API, including settings structure, environment variables, and deployment considerations.

---

## Configuration Structure

The application uses ASP.NET Core's configuration system with the following sources (in order of precedence):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. Environment variables
4. Command-line arguments
5. User secrets (development)
6. Azure Key Vault / other secret stores (production)

---

## Configuration Sections

### DataStoreSettings

**Section**: `dataStoreSettings`

**Purpose**: Configure file system paths for generation data

**Properties**:

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `wordListsRootDirectory` | string | Yes | "" | Root directory containing `.lst` word list files |
| `generationSchemasPath` | string | Yes | "" | Path to `GenerationSchemas.xml` file |

**Example**:
```json
{
  "dataStoreSettings": {
    "wordListsRootDirectory": "Data/Wordlists",
    "generationSchemasPath": "Data/GenerationSchemas.xml"
  }
}
```

**Path Resolution**:
- Relative paths resolved from application base directory
- Absolute paths supported
- Environment variables can be used: `%WORDLISTS_PATH%`

**Validation**:
- Paths must exist at runtime
- No startup validation; errors occur on first request
- Directory permissions must allow read access

---

### SecuritySettings

**Section**: `securitySettings`

**Purpose**: Configure authentication and security

**Properties**:

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `apiKey` | string | Yes | "" | API key for request authorization |

**Example**:
```json
{
  "securitySettings": {
    "apiKey": "your-secure-api-key-here"
  }
}
```

**Security Requirements**:
- Must be a strong, randomly generated key
- Minimum 32 characters recommended
- Must not be committed to source control
- Use placeholder in `appsettings.json`: `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]`
- Set actual value via environment variable or secret store

**Environment Variable**:
```bash
export SecuritySettings__ApiKey="your-secure-api-key-here"
```

---

### NuciLoggerSettings

**Section**: `nuciLoggerSettings`

**Purpose**: Configure logging behavior

**Properties**:

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `logFilePath` | string | No | "logfile.log" | File path for log output |
| `isFileOutputEnabled` | boolean | No | true | Enable/disable file logging |

**Example**:
```json
{
  "nuciLoggerSettings": {
    "logFilePath": "logs/universal-name-generator.log",
    "isFileOutputEnabled": true
  }
}
```

**Log Output**:
- Console output always enabled (ASP.NET Core default)
- File output controlled by `isFileOutputEnabled`
- Log rotation not implemented; external log rotation recommended

---

## Complete Configuration Example

### appsettings.json (Production Template)

```json
{
  "dataStoreSettings": {
    "wordListsRootDirectory": "/opt/universal-name-generator/data/wordlists",
    "generationSchemasPath": "/opt/universal-name-generator/data/GenerationSchemas.xml"
  },
  "securitySettings": {
    "apiKey": "[[UNIVERSAL_NAME_GENERATOR_API_KEY]]"
  },
  "nuciLoggerSettings": {
    "logFilePath": "/var/log/universal-name-generator/api.log",
    "isFileOutputEnabled": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "dataStoreSettings": {
    "wordListsRootDirectory": "Data/Wordlists",
    "generationSchemasPath": "Data/GenerationSchemas.xml"
  },
  "securitySettings": {
    "apiKey": "dev-api-key-for-testing-only"
  },
  "nuciLoggerSettings": {
    "logFilePath": "logfile.log",
    "isFileOutputEnabled": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "UniversalNameGenerator.API": "Debug"
    }
  }
}
```

---

## Environment Variables

All configuration can be overridden via environment variables using the double-underscore (`__`) separator:

### DataStoreSettings
```bash
export DataStoreSettings__WordListsRootDirectory="/custom/path/wordlists"
export DataStoreSettings__GenerationSchemasPath="/custom/path/GenerationSchemas.xml"
```

### SecuritySettings
```bash
export SecuritySettings__ApiKey="your-production-api-key"
```

### NuciLoggerSettings
```bash
export NuciLoggerSettings__LogFilePath="/custom/log/path/api.log"
export NuciLoggerSettings__IsFileOutputEnabled="true"
```

### ASP.NET Core Standard
```bash
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://0.0.0.0:5000;https://0.0.0.0:5001"
export ASPNETCORE_Kestrel__Certificates__Default__Path="/path/to/cert.pfx"
export ASPNETCORE_Kestrel__Certificates__Default__Password="cert-password"
```

---

## Configuration Binding

### Service Registration

**Path**: `UniversalNameGenerator.API/ServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddConfigurations(
    this IServiceCollection services,
    IConfiguration configuration)
{
    DataStoreSettings dataStoreSettings = new();
    SecuritySettings securitySettings = new();

    configuration.Bind(DataStoreSettingsSectionName, dataStoreSettings);
    configuration.Bind(SecuritySettingsSectionName, securitySettings);

    return services
        .AddSingleton(dataStoreSettings)
        .AddSingleton(securitySettings)
        .AddNuciLoggerSettings(configuration);
}
```

### Binding Behavior

- **Case-insensitive** property matching
- **Nested objects** supported via `__` separator
- **Arrays** supported via numeric indices
- **Default values** used when configuration missing
- **No validation** at binding time; validation at usage time

---

## Deployment Configuration

### Docker Configuration

**Dockerfile Example**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["UniversalNameGenerator.API/UniversalNameGenerator.API.csproj", "UniversalNameGenerator.API/"]
RUN dotnet restore "UniversalNameGenerator.API/UniversalNameGenerator.API.csproj"
COPY . .
WORKDIR "/src/UniversalNameGenerator.API"
RUN dotnet build "UniversalNameGenerator.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UniversalNameGenerator.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Data/ Data/
ENTRYPOINT ["dotnet", "UniversalNameGenerator.API.dll"]
```

**docker-compose.yml**:
```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DataStoreSettings__WordListsRootDirectory=/app/Data/Wordlists
      - DataStoreSettings__GenerationSchemasPath=/app/Data/GenerationSchemas.xml
      - SecuritySettings__ApiKey=${API_KEY}
      - NuciLoggerSettings__LogFilePath=/app/logs/api.log
      - NuciLoggerSettings__IsFileOutputEnabled=true
    volumes:
      - ./Data:/app/Data
      - ./logs:/app/logs
    restart: unless-stopped
```

### Kubernetes Configuration

**ConfigMap**:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: universal-name-generator-config
data:
  DataStoreSettings__WordListsRootDirectory: "/data/wordlists"
  DataStoreSettings__GenerationSchemasPath: "/data/GenerationSchemas.xml"
  NuciLoggerSettings__LogFilePath: "/var/log/api.log"
  NuciLoggerSettings__IsFileOutputEnabled: "true"
  ASPNETCORE_ENVIRONMENT: "Production"
```

**Secret**:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: universal-name-generator-secrets
type: Opaque
stringData:
  SecuritySettings__ApiKey: "your-secure-api-key-here"
```

**Deployment**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: universal-name-generator
spec:
  replicas: 3
  selector:
    matchLabels:
      app: universal-name-generator
  template:
    metadata:
      labels:
        app: universal-name-generator
    spec:
      containers:
      - name: api
        image: universal-name-generator:latest
        ports:
        - containerPort: 5000
        - containerPort: 5001
        envFrom:
        - configMapRef:
            name: universal-name-generator-config
        - secretRef:
            name: universal-name-generator-secrets
        volumeMounts:
        - name: data
          mountPath: /data
        - name: logs
          mountPath: /var/log
      volumes:
      - name: data
        persistentVolumeClaim:
          claimName: universal-name-generator-data
      - name: logs
        persistentVolumeClaim:
          claimName: universal-name-generator-logs
```

---

## Configuration Validation

### Startup Validation

The application does not perform comprehensive configuration validation at startup. Validation occurs at runtime:

1. **DataStoreSettings**: Validated when first request accesses files
2. **SecuritySettings**: Validated on first authentication attempt
3. **NuciLoggerSettings**: Validated when logging first writes to file

### Recommended Validation

Add startup validation in `Startup.ConfigureServices`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ... existing code ...
    
    // Validate required settings
    var dataStoreSettings = configuration.GetSection("dataStoreSettings").Get<DataStoreSettings>();
    if (string.IsNullOrWhiteSpace(dataStoreSettings?.WordListsRootDirectory))
        throw new InvalidOperationException("dataStoreSettings:wordListsRootDirectory is required");
    if (string.IsNullOrWhiteSpace(dataStoreSettings?.GenerationSchemasPath))
        throw new InvalidOperationException("dataStoreSettings:generationSchemasPath is required");
    
    var securitySettings = configuration.GetSection("securitySettings").Get<SecuritySettings>();
    if (string.IsNullOrWhiteSpace(securitySettings?.ApiKey))
        throw new InvalidOperationException("securitySettings:apiKey is required");
    if (securitySettings.ApiKey == "[[UNIVERSAL_NAME_GENERATOR_API_KEY]]")
        throw new InvalidOperationException("securitySettings:apiKey must be changed from default placeholder");
}
```

---

## Configuration Best Practices

### Security

1. **Never commit secrets** to source control
2. **Use environment variables** or secret stores for production
3. **Rotate API keys** periodically
4. **Use different keys** for different environments
5. **Monitor for key exposure** in logs or error messages

### File Paths

1. **Use absolute paths** in production
2. **Ensure directory permissions** allow read access
3. **Separate data from code** (use volumes in containers)
4. **Backup generation data** regularly
5. **Monitor disk space** for log files

### Logging

1. **Enable file logging** in production
2. **Configure log rotation** externally (logrotate, etc.)
3. **Set appropriate log levels** (Information for production)
4. **Monitor log file size** and growth
5. **Exclude sensitive data** from logs (already implemented)

### Environment Management

1. **Use environment-specific** configuration files
2. **Set ASPNETCORE_ENVIRONMENT** appropriately
3. **Validate configuration** in each environment
4. **Document required settings** for each environment
5. **Automate configuration** deployment

---

## Configuration Reference

### All Configuration Keys

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `dataStoreSettings:wordListsRootDirectory` | string | Yes | "" | Word lists directory |
| `dataStoreSettings:generationSchemasPath` | string | Yes | "" | Schema XML file path |
| `securitySettings:apiKey` | string | Yes | "" | Authentication API key |
| `nuciLoggerSettings:logFilePath` | string | No | "logfile.log" | Log file path |
| `nuciLoggerSettings:isFileOutputEnabled` | boolean | No | true | Enable file logging |
| `Logging:LogLevel:Default` | string | No | "Information" | Default log level |
| `Logging:LogLevel:Microsoft.AspNetCore` | string | No | "Warning" | ASP.NET Core log level |
| `ASPNETCORE_ENVIRONMENT` | string | No | "Production" | Hosting environment |
| `ASPNETCORE_URLS` | string | No | "http://localhost:5000" | Server URLs |

---

## Troubleshooting Configuration

### Common Issues

**Issue**: "Schema 'xyz' was not discovered"
- **Cause**: Schema not in XML file or file not found
- **Fix**: Verify `generationSchemasPath` and XML content

**Issue**: "File not found" for word lists
- **Cause**: Incorrect `wordListsRootDirectory` or missing `.lst` files
- **Fix**: Verify path and file existence

**Issue**: Authentication always fails
- **Cause**: API key mismatch or missing header
- **Fix**: Verify `securitySettings:apiKey` matches client header

**Issue**: No log output
- **Cause**: `isFileOutputEnabled` false or path not writable
- **Fix**: Enable file logging and verify permissions

**Issue**: CORS errors in browser
- **Cause**: Origin not in allowed list
- **Fix**: Add origin to `AllowedCorsOrigins` in `Startup.cs`

### Debugging Configuration

**View Effective Configuration**:
```csharp
// In Startup.ConfigureServices
var config = configuration.Get<AppConfiguration>();
_logger.LogInformation("Effective config: {@Config}", config);
```

**Environment Variable Debugging**:
```bash
# Print all environment variables with prefix
env | grep -E "^(DataStoreSettings|SecuritySettings|NuciLoggerSettings)__"
```

---

## Configuration Migration

### Version History

| Version | Changes |
|---------|---------|
| 1.0.0 | Initial configuration structure |
| 1.1.0 | Added NuciLoggerSettings |
| 1.1.1 | No configuration changes |

### Breaking Changes

None in current version history.

---

## Summary

The Universal Name Generator API uses a simple, file-based configuration model with three primary sections:

1. **DataStoreSettings**: File system paths for generation data
2. **SecuritySettings**: API key for authentication
3. **NuciLoggerSettings**: Logging behavior

All settings can be overridden via environment variables, making the application suitable for containerized and cloud deployments. The configuration is bound to strongly-typed objects at startup and registered as singletons for efficient access throughout the application lifetime.