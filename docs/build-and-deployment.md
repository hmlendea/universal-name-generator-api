# Build and deployment

This document describes the build pipeline, GitHub Actions workflows, and deployment for the Universal Name Generator API.

## Build

### Local build

```bash
dotnet restore
dotnet build UniversalNameGenerator.API/UniversalNameGenerator.API.csproj
```

Target framework: `net10.0`.

### Release build

```bash
dotnet publish UniversalNameGenerator.API/UniversalNameGenerator.API.csproj -c Release -o ./publish
```

Produces a self-contained or framework-dependent deployment in `./publish`.

## GitHub Actions workflows

The repository uses the following workflows (in `.github/workflows/`):

| Workflow | File | Triggers | Purpose |
|----------|------|----------|---------|
| .NET CI | `dotnet.yml` | Push, PR to master | Restore, build, test. |
| Security | `security.yml` | Push, PR, weekly schedule | Vulnerability scan, CodeQL analysis. |

### dotnet.yml

```yaml
name: .NET

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v7
    - name: Setup .NET
      uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 10.0.x
    - name: Restore
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

### security.yml

```yaml
name: Security

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]
  schedule:
    - cron: '0 0 * * 0'

jobs:
  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest
    permissions:
      security-events: write
      contents: read
    steps:
    - uses: actions/checkout@v7
    - name: Setup .NET
      uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 10.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Check for vulnerable packages
      run: dotnet list package --vulnerable --include-transitive
    - name: Run CodeQL Analysis
      uses: github/codeql-action/init@v4
      with:
        languages: csharp
    - name: Build
      run: dotnet build --no-restore
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v4
```

## Release process

The repository includes a `release.sh` script for automated releases:

```bash
./release.sh <version>
```

The script:
1. Updates version in project files.
2. Creates a Git tag.
3. Builds and publishes.
4. Creates a GitHub release with artifacts.

See `release.sh` for the exact steps.

## Deployment

### Self-hosted

The API is a single ASP.NET Core process. Deployment options:

1. **Framework-dependent:** Copy publish output to target machine with .NET 10.0 runtime installed.
2. **Self-contained:** Publish with `-r <rid>` for a specific runtime identifier.
3. **Container:** Build a Docker image from the publish output.

### Configuration

Production deployments must override:

- `securitySettings:apiKey` - Secure random string.
- `dataStoreSettings:wordListsRootDirectory` - Absolute path to word lists.
- `dataStoreSettings:generationSchemasPath` - Absolute path to schemas.
- `nuciLoggerSettings:logFilePath` - Absolute path to log file.
- Kestrel endpoints - Production URLs and certificates.

Use environment variables or a configuration management system.

### Process management

Run under a process manager:

- **systemd:** Create a service unit.
- **Docker:** Use the container image.
- **Kubernetes:** Deploy as a pod with config maps/secrets.

### Scaling

Horizontal scaling: run multiple instances behind a load balancer. Each instance reads the same data files. No shared state.

### Health checks

No built-in health check endpoint. Operators should implement infrastructure-level health checks (process liveness, port connectivity).

## Artefacts

| Artefact | Location | Description |
|----------|----------|-------------|
| Publish output | `./publish/` | Framework-dependent deployment. |
| Docker image | (build locally) | Container deployment. |
| GitHub Release | GitHub Releases page | Tagged release with assets. |

## Rollback

Redeploy the previous version's artefacts. No database migrations to reverse.