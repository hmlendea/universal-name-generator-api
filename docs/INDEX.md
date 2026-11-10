# Universal Name Generator API Documentation Index

This documentation set provides a comprehensive guide to the Universal Name Generator API repository. It is intended for developers, maintainers, and future coding agents who need to understand, modify, or extend the system without re-deriving knowledge from source code.

## Repository summary

The Universal Name Generator API is an ASP.NET Core REST API that generates random names from configurable generation schemas and reusable word lists. It exposes a single authenticated endpoint (`GET /Names`) that interprets schema definitions, applies generation strategies (random, randomiser, random-selector, Markov chain), and returns JSON name arrays. The system is a modular monolith with file-backed data sources, no database, and no external service integrations.

## Root document map

The following documents reside at the repository root and define project-level policies. The `docs/` set complements them and does not duplicate their content.

- [ARCHITECTURE.md](../ARCHITECTURE.md) - Current-state architecture of the ASP.NET Core process, its HTTP boundary, schema-driven generation service, file-backed data sources, middleware, configuration, deployment, and verification boundaries.
- [SECURITY.md](../SECURITY.md) - Vulnerability reporting, scope, disclosure policy, and automated security scanning.
- [PRIVACY.md](../PRIVACY.md) - Personal data handling, self-hosted deployment responsibilities, and data flows.
- [LICENSE](../LICENSE) - GNU General Public License v3.0.
- [README.md](../README.md) - Project overview, capabilities, usage, configuration, and development guide.

## Documentation catalogue

### Root guides

| Document | Description |
|----------|-------------|
| [Quick start](./quick-start.md) | Getting started guide for newcomers: clone, build, run, and test. |
| [Repository overview](./repository-overview.md) | Repository purpose, scope, principal capabilities, and entry points. |
| [Repository structure](./repository-structure.md) | Source tree layout, project organisation, and module boundaries. |
| [Architecture](./architecture.md) | High-level architecture complementing the root `ARCHITECTURE.md`. |

### API reference

| Document | Description |
|----------|-------------|
| [API reference index](./api-reference/INDEX.md) | Endpoint summary table, authentication schemes, versioning, and rate limiting. |
| [NamesController](./api-reference/names-controller.md) | Complete endpoint documentation for `GET /Names`. |

### Behaviour

| Document | Description |
|----------|-------------|
| [Name generation behaviour](./behaviour/name-generation.md) | User-facing name generation behaviour, schema interpretation, and casing. |

### Components

| Document | Description |
|----------|-------------|
| [Host and composition](./components/host-and-composition.md) | `Program`, `Startup`, middleware pipeline, and dependency injection. |
| [Application services](./components/application-services.md) | `NameGeneratorService`, generator strategies, and schema interpretation. |
| [Data access](./components/data-access.md) | XML schema repository, word list repository, and data object mapping. |
| [Integration models](./components/integration-models.md) | Request/response contracts and NuciAPI base types. |

### Flows

| Document | Description |
|----------|-------------|
| [Startup and composition](./flows/startup-and-composition.md) | Host build, configuration binding, service registration, and pipeline ordering. |
| [Name generation request](./flows/name-generation-request.md) | End-to-end flow of an authenticated `GET /Names` request. |

### Cross-cutting concerns

| Document | Description |
|----------|-------------|
| [Configuration](./configuration.md) | Configuration schema, sources, precedence, and settings reference. |
| [Logging](./logging.md) | NuciLog framework, structured fields, sinks, and redaction. |
| [Security](./security.md) | Security model, API key authorisation, and threat mitigations. |
| [Error handling](./error-handling.md) | Error taxonomy, exception handling middleware, and response contracts. |
| [Concurrency and scheduling](./concurrency-and-scheduling.md) | Threading model, async boundaries, and shared-state protection. |

### Data and state

| Document | Description |
|----------|-------------|
| [Data model](./data-model.md) | Domain entities, data objects, XML schema format, and word list format. |
| [State and persistence](./state-and-persistence.md) | File-backed state, in-memory caches, and lifecycle. |
| [Integrations](./integrations.md) | External system integrations (none beyond filesystem and NuGet packages). |

### Operations

| Document | Description |
|----------|-------------|
| [Build and deployment](./build-and-deployment.md) | Build pipeline, GitHub Actions workflows, and deployment. |
| [Dependencies](./dependencies.md) | External and internal dependencies, NuGet packages, and generator libraries. |
| [Testing](./testing.md) | Test strategy, project organisation, and coverage. |

### Maintenance

| Document | Description |
|----------|-------------|
| [Design decisions](./design-decisions.md) | Key architectural and design choices with rationale. |
| [Invariants](./invariants.md) | System-wide invariants and contracts. |
| [Change guide](./change-guide.md) | How to modify common areas safely. |
| [Documentation maintenance](./documentation-maintenance.md) | How to keep documentation current. |
| [Troubleshooting](./troubleshooting.md) | Common issues and solutions. |
| [FAQ](./faq.md) | Frequently asked questions. |
| [API usage examples](./api-usage-examples.md) | Example requests and responses for API usage. |
| [Ambiguities and open questions](./ambiguities-and-open-questions.md) | Unresolved items, TODOs, and known gaps. |

## Navigation aids

### Start here (newcomers)

1. [Quick start](./quick-start.md) - Build and run the API.
2. [Repository overview](./repository-overview.md) - Understand purpose and scope.
3. [Repository structure](./repository-structure.md) - Locate source areas.
4. [Architecture](./architecture.md) - Understand the system shape.
5. [API usage examples](./api-usage-examples.md) - See the API in action.

### Deep dive (component owners)

- [Host and composition](./components/host-and-composition.md)
- [Application services](./components/application-services.md)
- [Data access](./components/data-access.md)
- [Integration models](./components/integration-models.md)
- [Configuration](./configuration.md)
- [Data model](./data-model.md)

### Flows (debuggers)

- [Startup and composition](./flows/startup-and-composition.md)
- [Name generation request](./flows/name-generation-request.md)
- [Error handling](./error-handling.md)

## Maintenance metadata

- **Last reviewed:** 2026-11-10
- **Owner:** Project maintainers (see [README.md](../README.md))
- **Coverage status:** Complete for the current `net10.0` solution. Covers all four projects, the single controller, the name generation service, data access, middleware, configuration, logging, security, testing, and deployment.
- **Update triggers:** Schema format changes, new generator commands, new endpoints, configuration key changes, middleware pipeline changes, dependency upgrades, or deployment topology changes.
