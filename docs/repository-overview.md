# Repository overview

## Purpose

The Universal Name Generator API provides an ASP.NET Core REST API for generating random names from configurable generation schemas and reusable word lists. It is the API counterpart of the Universal Name Generator family of projects.

## Scope

The repository owns:

- A single HTTP endpoint (`GET /Names`) that generates names from a schema identifier and an optional count.
- Schema interpretation: parsing `{command,...}` generator directives embedded in schema strings.
- Four generation strategies: `random`, `randomiser`, `random-selector`, and `markov`.
- Word list loading from `.lst` files and generation schema loading from `GenerationSchemas.xml`.
- API key authorisation, CORS, request logging, exception handling, and scanner protection middleware.
- File-backed data access with no database.

The repository does not own:

- A user interface or web front end.
- A database or persistent application state.
- External service integrations or telemetry.
- Word list or schema authoring tools (data files are supplied by the operator).

## Principal capabilities

| Capability | Entry point | Owner |
|------------|-------------|-------|
| Generate names by schema identifier | `GET /Names?schema=...&count=...` | `NamesController` -> `NameGeneratorService` |
| Random string generation from literal choices | `{random,choices,min,max}` | `NameGeneratorService.GetRandomStrings` |
| Randomiser composition from word lists | `{randomiser,separator,min,max,wordlists}` | `RandomiserNameGenerator` |
| Random selector from word list values | `{random-selector,min,max,wordlists}` | `RandomSelectorNameGenerator` |
| Markov chain generation | `{markov,min,max,wordlists}` | `MarkovNameGenerator` (from `NuciGenerators.Text.MarkovChain`) |
| Word casing and filter lists | Schema `WordCase` and `FilterlistPath` | `NameGeneratorService` |

## Entry points

| Entry point | Location | Role |
|-------------|----------|------|
| Process entry | `UniversalNameGenerator.API/Program.cs` | Host build and run |
| Composition root | `UniversalNameGenerator.API/Startup.cs` | Service registration and middleware ordering |
| HTTP boundary | `UniversalNameGenerator.API/Controllers/NamesController.cs` | Single authenticated endpoint |
| Application service | `UniversalNameGenerator.API/Service/NameGeneratorService.cs` | Schema interpretation and generation orchestration |
| Data access | `UniversalNameGenerator.API/DataAccess/Repositories/` | XML schema and `.lst` word list repositories |

## Solution organisation

The solution `UniversalNameGenerator.API.slnx` contains four projects:

1. `UniversalNameGenerator.API` - The ASP.NET Core web application.
2. `UniversalNameGenerator.API.UnitTests` - NUnit unit tests for the service and data objects.
3. `UniversalNameGenerator.API.IntegrationTests` - NUnit integration tests using `WebApplicationFactory`.
4. `UniversalNameGenerator.API.ArchitectureTests` - ArchUnitNET and Roslyn convention tests.

See [Repository structure](./repository-structure.md) for the full tree.

## Runtime topology

A single deployable process. No background workers, queues, or distributed components. All generation occurs in memory per request. File reads occur at service construction and per word list load.

## System-wide invariants

- Every `GET /Names` request requires a valid API key in the `Authorization` header.
- Schema identifiers are matched with ordinal string comparison.
- Generator commands are case-sensitive ordinal matches against `random`, `randomiser`, `random-selector`, and `markov`.
- Word list files use the `.lst` extension and are resolved relative to `dataStoreSettings:wordListsRootDirectory`.
- The API never writes to the data store; it only reads.

See [Invariants](./invariants.md) for the complete list.
