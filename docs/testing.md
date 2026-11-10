# Testing

This document describes the test strategy, project organisation, and coverage for the Universal Name Generator API.

## Test projects

| Project | Purpose | Framework | Scope |
|---------|---------|-----------|-------|
| `UniversalNameGenerator.API.UnitTests` | Fast, isolated unit tests | NUnit 4.x, Moq | Service logic, data objects, repositories, mappings |
| `UniversalNameGenerator.API.IntegrationTests` | End-to-end HTTP tests | NUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing | Controllers, middleware, authentication, CORS, error handling, full request flows |
| `UniversalNameGenerator.API.ArchitectureTests` | Architectural enforcement | ArchUnitNET | Layer boundaries, naming conventions, dependency rules |

## Unit tests

### Structure

```
UniversalNameGenerator.API.UnitTests/
├── DataAccess/
│   ├── DataObjects/
│   │   ├── WordDataObjectTests.cs
│   │   └── GenerationSchemaDataObjectTests.cs
│   └── Repositories/
│       └── WordRepositoryTests.cs
└── Service/
    └── NameGeneratorServiceTests.cs
```

### Patterns

- **Arrange-Act-Assert** with NUnit attributes.
- **Moq** for all external dependencies (repositories, settings).
- **No real file I/O** - all data provided via mocks.
- **Test naming:** `MethodName_Scenario_ExpectedResult`.

### Coverage targets

| Component | Target |
|-----------|--------|
| `NameGeneratorService` | 90%+ |
| `WordRepository` | 80%+ |
| `GenerationSchemaRepository` | 80%+ |
| Mappings | 90%+ |
| Data objects | 100% |

### Running unit tests

```bash
dotnet test UniversalNameGenerator.API.UnitTests/UniversalNameGenerator.API.UnitTests.csproj
```

## Integration tests

### Structure

```
UniversalNameGenerator.API.IntegrationTests/
├── Controllers/
│   ├── NamesControllerTestBase.cs
│   ├── NamesControllerTests.cs
│   ├── NamesControllerAuthorisationTests.cs
│   ├── NamesControllerCorsTests.cs
│   ├── NamesControllerExceptionHandlingTests.cs
│   ├── NamesControllerRoutingTests.cs
│   └── NamesControllerValidationTests.cs
├── EndToEnd/
│   └── NameGenerationEndToEndTests.cs
├── Middleware/
│   └── ScannerProtectionTests.cs
├── Infrastructure/
│   ├── IntegrationGenerationDataStore.cs
│   ├── TestRemoteIpAddressStartupFilter.cs
│   └── UniversalNameGeneratorApiFactory.cs
└── Assertions/
    └── NuciApiResponseAssertions.cs
```

### Test base

`NamesControllerTestBase` provides:
- `UniversalNameGeneratorApiFactory` - `WebApplicationFactory` with test configuration.
- `IntegrationGenerationDataStore` - Creates temporary schemas and word lists.
- `HttpClient` configured for the test server.
- `NuciApiResponseAssertions` - Fluent assertions for NuciAPI envelopes.

### Test data

`IntegrationGenerationDataStore` creates:
- Temporary directory for each test run.
- `GenerationSchemas.xml` with test schemas.
- `.lst` word list files in `Wordlists/`.
- Cleans up on disposal.

### Patterns

- **One test class per concern** (auth, CORS, routing, validation, exceptions).
- **Inherit from `NamesControllerTestBase`** for shared infrastructure.
- **Use `NuciApiResponseAssertions`** for response verification.
- **Test naming:** `Should_ExpectedBehaviour_When_Condition`.

### Running integration tests

```bash
dotnet test UniversalNameGenerator.API.IntegrationTests/UniversalNameGenerator.API.IntegrationTests.csproj
```

## Architecture tests

### Structure

```
UniversalNameGenerator.API.ArchitectureTests/
├── ArchitectureTestBase.cs
├── ProductionDependencyArchitectureTests.cs
└── SourceFileConventionTests.cs
```

### Rules enforced

| Rule | Description |
|------|-------------|
| Controllers → Services | Controllers may only depend on `Service` namespace. |
| Services → DataAccess | Services may only depend on `DataAccess` namespace. |
| No Controller → DataAccess | Controllers must not reference `DataAccess` directly. |
| No Service → Controllers | Services must not reference `Controllers`. |
| No DataAccess → Services | DataAccess must not reference `Services` or `Controllers`. |
| Naming conventions | Classes, interfaces, methods follow project conventions. |
| File placement | Files in correct namespace folders. |

### Running architecture tests

```bash
dotnet test UniversalNameGenerator.API.ArchitectureTests/UniversalNameGenerator.API.ArchitectureTests.csproj
```

## Test execution

### All tests

```bash
dotnet test UniversalNameGenerator.API.slnx
```

### With coverage

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Filter by category

```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName~UnitTests"

# Integration tests only
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Architecture tests only
dotnet test --filter "FullyQualifiedName~ArchitectureTests"
```

## CI integration

GitHub Actions workflow (`.github/workflows/dotnet.yml`) runs:

1. `dotnet restore`
2. `dotnet build --no-restore`
3. `dotnet test --no-build --verbosity normal`

All three test projects must pass for the build to succeed.

## Test data management

### Unit tests

- All data created in-memory via Moq.
- No external dependencies.
- Deterministic and fast.

### Integration tests

- `IntegrationGenerationDataStore` creates isolated test data per test class.
- Uses `Path.GetTempPath()` for isolation.
- Cleans up in `Dispose()`.

### Architecture tests

- No test data. Analyses compiled assemblies.

## Assertions

### Unit tests

- NUnit `Assert.That()` with constraints.
- Moq `Verify()` for interaction verification.

### Integration tests

- `NuciApiResponseAssertions` for NuciAPI envelope verification:
  ```csharp
  response.Should().BeSuccessful()
      .And.HaveData<GetNamesResponse>(r => r.Names.Should().HaveCount(5));
  ```
- FluentAssertions for HTTP responses.

### Architecture tests

- ArchUnitNET fluent API:
  ```csharp
  Classes().That().ResideInNamespace("*.Controllers")
      .Should().OnlyDependOnClassesThat().ResideInNamespace("*.Service", "*.Configuration", "NuciAPI.*");
  ```

## Mocking strategy

| Dependency | Mocked in unit tests? | Real in integration tests? |
|------------|----------------------|---------------------------|
| `INameGeneratorService` | Yes (Moq) | No (real via DI) |
| `GenerationSchemaRepository` | Yes (Moq) | No (real via DI) |
| `WordRepository` | Yes (Moq) | No (real via DI) |
| `DataStoreSettings` | Yes (Moq) | No (real via config) |
| `SecuritySettings` | Yes (Moq) | No (real via config) |
| `NuciLogger` | No (not tested) | Yes (real) |
| `HttpClient` | N/A | Yes (test server) |

## Known gaps

- [ ] No load/performance tests.
- [ ] No contract tests for NuciAPI envelope.
- [ ] No property-based tests for generators.
- [ ] No mutation testing.
- [ ] Architecture tests don't cover all cross-cutting concerns.

## Adding a test

### Unit test

1. Create `<ClassName>Tests.cs` in matching folder under `UnitTests/`.
2. Use `[TestFixture]` class with `[Test]` methods.
3. Mock dependencies in `[SetUp]` or per test.
4. Follow naming convention.

### Integration test

1. Create test class in appropriate folder under `IntegrationTests/`.
2. Inherit from `NamesControllerTestBase` or create new base.
3. Use `IntegrationGenerationDataStore` for test data.
4. Use `NuciApiResponseAssertions` for verification.

### Architecture test

1. Add rule to `ProductionDependencyArchitectureTests.cs` or `SourceFileConventionTests.cs`.
2. Use ArchUnitNET fluent API.
3. Ensure test fails before fix, passes after.

## Debugging tests

### Unit tests

```bash
dotnet test --filter "FullyQualifiedName~NameGeneratorServiceTests" --logger "console;verbosity=detailed"
```

### Integration tests

```bash
dotnet test --filter "FullyQualifiedName~NamesControllerTests" --logger "console;verbosity=detailed"
```

### Architecture tests

```bash
dotnet test --filter "FullyQualifiedName~ProductionDependencyArchitectureTests" --logger "console;verbosity=detailed"
```

## Test maintenance

- Run tests before every commit.
- Update tests when behaviour changes.
- Remove obsolete tests.
- Keep test data minimal and focused.
- Avoid shared mutable state between tests.