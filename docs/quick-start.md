# Quick start

This guide gets the Universal Name Generator API built, running, and responding to an authenticated request.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A shell with `curl` (for the verification request)

## Clone and restore

```bash
git clone https://github.com/hmlendea/universal-name-generator-api.git
cd universal-name-generator-api
dotnet restore
```

## Build

```bash
dotnet build UniversalNameGenerator.API/UniversalNameGenerator.API.csproj
```

## Configure

The API requires a non-placeholder API key. Edit `UniversalNameGenerator.API/appsettings.json`:

```json
{
  "securitySettings": {
    "apiKey": "your-actual-api-key"
  }
}
```

The default value `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]` is a placeholder. Do not commit a real key.

The default data store settings point at `Data/Wordlists` and `Data/GenerationSchemas.xml`. Ensure these paths exist relative to the working directory, or override them in `appsettings.json` or through environment variables.

## Run

```bash
cd UniversalNameGenerator.API
dotnet run
```

The host listens on the configured HTTP and HTTPS endpoints (default `http://localhost:5000` and `https://localhost:5001`).

## Verify

```bash
curl -G "http://localhost:5000/Names" \
  -H "Authorization: Bearer your-actual-api-key" \
  --data-urlencode "schema=arabic-toponyms" \
  --data-urlencode "count=5"
```

A successful response contains a JSON object with a `names` array.

## Test

```bash
dotnet test
```

This runs the unit, integration, and architecture test projects.

## Next steps

- [Repository overview](./repository-overview.md) for purpose and scope.
- [Architecture](./architecture.md) for the system shape.
- [API usage examples](./api-usage-examples.md) for request and response examples.
- [Configuration](./configuration.md) for the full settings reference.
