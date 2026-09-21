[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/universal-name-generator-api)](https://github.com/hmlendea/universal-name-generator-api/releases/latest)
[![Build Status](https://github.com/hmlendea/universal-name-generator-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/universal-name-generator-api/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# Universal Name Generator API

Universal Name Generator API provides an ASP.NET Core REST API for generating random names from configurable generation schemas and reusable word lists.

## 📑 Table of Contents

- [Capabilities](#-capabilities)
- [Usage](#-usage)
- [System Requirements](#-system-requirements)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Authentication and Authorisation](#-authentication-and-authorisation)
- [Development](#-development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [GitHub Actions](#-github-actions)
- [Project Structure](#-project-structure)
- [Architecture](#-architecture)
- [Contributing](#-contributing)
- [Related Projects](#-related-projects)
- [Supporting the Project](#-supporting-the-project)
- [Security](#-security)
- [License](#-license)

## ✨ Capabilities

- Generate names by generation schema identifier
- Generate values with random, randomiser, random-selector, and Markov chain strategies
- Apply configured word casing and filter lists

## 🔐 Authentication and Authorisation

Requests to `GET /Names` require an API key in the `Authorization` header. The key may be sent directly or with the `Bearer` prefix:

```http
Authorization: Bearer YOUR_API_KEY
```

The API key is configured through the `securitySettings:apiKey` setting. Do not commit the configured secret to source control.

## 🚀 Usage

```bash
curl -G "http://localhost:5000/Names" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  --data-urlencode "schema=arabic-toponyms" \
  --data-urlencode "count=5"
```

## 🖥️ System Requirements

- **OS:** Linux, macOS, Windows
- **RAM:** 256 MB minimum
- .NET 10.0 runtime

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/universal-name-generator-api/releases)

## ⚙️ Configuration

All settings are loaded from the configuration file. The subsequent keys are recognised:

| Section | Key | Description |
|---------|-----|-------------|
| `dataStoreSettings` | `wordListsRootDirectory` | Path to the directory containing word list `.lst` files. |
| `dataStoreSettings` | `generationSchemasPath` | Path to the `GenerationSchemas.xml` file. |
| `securitySettings` | `apiKey` | API key required for authorisation. |
| `nuciLoggerSettings` | `logFilePath` | File path for persisted log output. |
| `nuciLoggerSettings` | `isFileOutputEnabled` | Enables or disables file log output. |

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

All NuGet dependencies are restored automatically by `dotnet restore`.

### Build

```bash
dotnet build UniversalNameGenerator.API/UniversalNameGenerator.API.csproj
```

### Run

```bash
dotnet run --project UniversalNameGenerator.API/UniversalNameGenerator.API.csproj
```

### Test

The solution test command executes both the unit and integration test projects. The integration suite hosts the complete production HTTP pipeline and uses isolated temporary generation data.

```bash
dotnet test UniversalNameGenerator.API.slnx
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

### Dependencies

| Package | Purpose |
|---------|---------|
| `NuciAPI` 3.6.1 | Core API abstractions and response models. |
| `NuciAPI.Controllers` 2.3.1 | Controller utilities and API-key authorisation. |
| `NuciAPI.Middleware` 2.0.3 | Base middleware infrastructure. |
| `NuciAPI.Middleware.ExceptionHandling` 1.0.2 | Structured exception handling middleware. |
| `NuciAPI.Middleware.Logging` 1.0.1 | Request and operation logging middleware. |
| `NuciAPI.Middleware.Security` 1.0.6 | API scanner and security middleware integration. |
| `NuciDAL` 3.2.1 | Data access abstractions for repository patterns. |
| `NuciExtensions` 5.3.2 | Shared extension methods used across the service. |
| `NuciGenerators.Text` 1.0.1 | Text generation primitives. |
| `NuciGenerators.Text.MarkovChain` 1.0.1 | Markov chain generator implementation. |
| `NuciLog` 1.2.1 | Logging provider implementation. |
| `NuciLog.Core` 3.1.0 | Logging contracts. |
| `NuciSecurity.HMAC` 4.1.3 | HMAC metadata and validation support. |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [.NET](.github/workflows/dotnet.yml) | Continuous validation | Restores dependencies, builds the solution, and runs the unit and integration tests on Ubuntu with .NET 10.0. |

## 🗂️ Project Structure

The solution contains the subsequent projects:

- [`UniversalNameGenerator.API`](./UniversalNameGenerator.API/): ASP.NET Core Web API application.
- [`UniversalNameGenerator.API.IntegrationTests`](./UniversalNameGenerator.API.IntegrationTests/): Integration tests for HTTP contracts, middleware, and production name generation.
- [`UniversalNameGenerator.API.UnitTests`](./UniversalNameGenerator.API.UnitTests/): Unit tests for API and service components.

The key directories inside `UniversalNameGenerator.API/` are:

| Directory | Purpose |
|-----------|---------|
| `Configuration/` | Application configuration models and settings contracts. |
| `Controllers/` | HTTP endpoints for name generation requests. |
| `DataAccess/` | Data objects and repository implementations for word sources and schemas. |
| `Logging/` | Logging operation and key definitions. |
| `Models/` | Request and response API models. |
| `Service/` | Name generation service logic, models, and strategy implementations. |

## 🏛️ Architecture

See the [architecture documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Maintain the existing public contract intact unless a breaking change is intentional
- Maintain the pull requests as focused and consistent with the existing code style
- Maintain your branch up-to-date with `master`
- Revise the documentation when behaviour changes
- Properly test all changes, including edge cases and error conditions
- Add unit tests for any new or changed functionality

## 🔗 Related Projects

- [Universal Name Generator](https://github.com/hmlendea/universal-name-generator): Desktop application for local name generation workflows.
- [Universal Name Generator API](https://github.com/hmlendea/universal-name-generator-api): REST API service for remote and integrated name generation.

## 💝 Supporting the Project

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/universal-name-generator-api/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 🛡️ Security

Please review the [security policy](./SECURITY.md) before reporting a vulnerability.

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for further information.
