# Invariants

This document lists system-wide invariants and contracts that must hold for correct operation.

## API contract invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| `GET /Names` requires authentication | Every request must have a valid API key. | Integration tests: `NamesControllerAuthorisationTests` |
| `schema` parameter is required | Missing `schema` returns 400. | Integration tests: validation tests |
| `count` is 1-100000 | Out of range returns 400. | Unit tests: `NameGeneratorServiceTests` |
| Response is NuciAPI envelope | All responses (success/error) use `NuciApiResponse` base. | Integration tests: `NuciApiResponseAssertions` |
| Success response contains `names` array | `GetNamesResponse.Names` is always present. | Unit tests, integration tests |
| Error response has `errorCode` and `errorMessage` | `NuciApiErrorResponse` contract. | Integration tests: exception handling tests |

## Schema resolution invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| Schema IDs are unique | No duplicate `Id` in `GenerationSchemas.xml`. | Not enforced at load; operator responsibility. |
| Schema ID match is ordinal | `string.Equals(id, other, StringComparison.Ordinal)`. | Unit tests: schema resolution tests |
| Unknown schema returns error | `KeyNotFoundException` -> 400 `SchemaNotFound`. | Integration tests |

## Generation invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| Generator commands are case-sensitive | `random` ≠ `Random`. | Unit tests: command parsing tests |
| Max 1000 attempts per `GetNames` | Prevents infinite loops. | Unit tests: `GenerateNames` max attempts |
| Filter list applied before casing | `ApplyFilterList` then `ApplyWordCase`. | Unit tests: filter and casing order |
| Casing enum values are exhaustive | `Lower`, `Upper`, `Title`, `Sentence`. | Unit tests: casing tests |
| `random` command uses literal choices | No word list access. | Unit tests: `GetRandomStrings` tests |
| `randomiser` joins with separator | One value per word list per iteration. | Unit tests: `RandomiserNameGeneratorTests` |
| `random-selector` combines all values | Single pool from all word lists. | Unit tests: `RandomSelectorNameGeneratorTests` |
| `markov` uses NuciGenerators.Text | Delegates to external library. | Unit tests: Markov integration tests |

## Data format invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| `.lst` files use underscore separator | `id_value` format. | Unit tests: `WordRepositoryTests` |
| `.lst` comments start with `#` | Lines starting with `#` ignored. | Unit tests |
| `.lst` empty lines ignored | Blank lines skipped. | Unit tests |
| XML schema uses `ArrayOfGenerationSchemaDataObject` | NuciDAL deserialisation expectation. | Unit tests: XML round-trip |
| `WordCase` default is `Title` | When XML element absent. | Unit tests: mapping tests |

## Configuration invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| `apiKey` placeholder not valid | `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]` fails auth. | Manual verification |
| Paths relative to working directory | Unless absolute. | Integration tests with temp paths |
| Configuration read at startup | No hot reload. | Architecture tests (no reload code) |

## Middleware pipeline invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| Exception handling is first | Catches all downstream. | Architecture tests: middleware order |
| Authentication before endpoints | Unauthenticated never reaches controller. | Integration tests: authorisation tests |
| Scanner protection before logging | Scanners not logged as requests. | Integration tests: scanner tests |

## Concurrency invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| Scoped service per request | New `NameGeneratorService` each request. | DI registration: `AddScoped` |
| No shared mutable state across requests | Caches are per-instance. | Code review |
| `Random.Shared` is thread-safe | Used by generators. | .NET guarantee |

## File system invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| API never writes data files | Read-only access to schemas and word lists. | Code review |
| Log file append-only | NuciLog file sink. | Code review |
| Missing word list = 500 | `FileNotFoundException` not caught. | Integration tests |

## Testing invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| Unit tests use Moq for repositories | No real file I/O. | Code review |
| Integration tests use temp data store | Isolated per test. | `IntegrationGenerationDataStore` |
| Architecture tests enforce boundaries | Controllers -> Service -> DataAccess. | `ProductionDependencyArchitectureTests` |
| All tests pass in CI | `dotnet test` in workflow. | GitHub Actions |

## Deployment invariants

| Invariant | Description | Verification |
|-----------|-------------|--------------|
| Single process deployment | No distributed components. | Architecture |
| Horizontal scaling via instances | Shared-nothing. | Architecture |
| Restart required for data changes | No file watching. | Operations guide |