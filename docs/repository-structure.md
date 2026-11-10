# Repository structure

This document maps the source tree to responsibilities. Paths are repository-relative.

## Solution layout

```
universal-name-generator-api/
├── UniversalNameGenerator.API/                  # ASP.NET Core web application
├── UniversalNameGenerator.API.UnitTests/        # NUnit unit tests
├── UniversalNameGenerator.API.IntegrationTests/ # NUnit integration tests
├── UniversalNameGenerator.API.ArchitectureTests/# ArchUnitNET + Roslyn convention tests
├── UniversalNameGenerator.API.slnx              # Solution file
├── ARCHITECTURE.md                              # Root architecture document
├── SECURITY.md                                  # Vulnerability disclosure policy
├── PRIVACY.md                                   # Personal data handling
├── LICENSE                                      # GPL-3.0
├── README.md                                    # Project overview
└── release.sh                                   # Release automation script
```

## UniversalNameGenerator.API

The deployable ASP.NET Core application. Target framework `net10.0`.

```
UniversalNameGenerator.API/
├── Program.cs                        # Host entry point
├── Startup.cs                        # Composition root and middleware pipeline
├── IStartup.cs                       # Startup interface for testability
├── ServiceCollectionExtensions.cs    # DI registration helpers
├── appsettings.json                  # Default configuration
├── UniversalNameGenerator.API.csproj # Project manifest
├── Configuration/
│   ├── DataStoreSettings.cs          # dataStoreSettings POCO
│   └── SecuritySettings.cs           # securitySettings POCO
├── Controllers/
│   ├── INamesController.cs           # Controller interface
│   └── NamesController.cs            # GET /Names endpoint
├── DataAccess/
│   ├── DataObjects/
│   │   ├── GenerationSchemaDataObject.cs  # XML-serialised schema record
│   │   └── WordDataObject.cs              # Word list record
│   └── Repositories/
│       ├── GenerationSchemaRepository.cs  # XML schema reader
│       └── WordRepository.cs              # .lst word list reader
├── Logging/
│   ├── MyLogInfoKey.cs               # Structured log keys
│   └── MyOperation.cs                # Operation names
├── Models/
│   ├── GetNamesRequest.cs            # Request contract
│   └── GetNamesResponse.cs           # Response contract
├── Service/
│   ├── INameGeneratorService.cs      # Service contract
│   ├── NameGeneratorService.cs       # Schema interpretation and orchestration
│   ├── Mappings/
│   │   ├── GenerationSchemaMappingExtensions.cs
│   │   └── WordMappingExtensions.cs
│   ├── Models/
│   │   ├── GenerationSchema.cs       # Service model
│   │   └── Word.cs                   # Service model
│   └── NameGenerators/
│       ├── Randomiser/
│       │   └── RandomiserNameGenerator.cs
│       ├── RandomSelector/
│       │   └── RandomSelectorNameGenerator.cs
│       └── (markov and random use NuciGenerators.Text directly)
```

### File responsibilities

| Path | Responsibility |
|------|----------------|
| `Program.cs` | Builds and runs the host via `CreateHostBuilder`. |
| `Startup.cs` | Binds configuration, registers services, orders middleware. |
| `IStartup.cs` | Exposes `Startup` through an interface for test composition. |
| `ServiceCollectionExtensions.cs` | Registers settings singletons, scoped service, and `NuciLogger`. |
| `Configuration/DataStoreSettings.cs` | `wordListsRootDirectory`, `generationSchemasPath`. |
| `Configuration/SecuritySettings.cs` | `apiKey`. |
| `Controllers/NamesController.cs` | `GET /Names` with API key authorisation. |
| `DataAccess/Repositories/GenerationSchemaRepository.cs` | Deserialises `GenerationSchemas.xml`. |
| `DataAccess/Repositories/WordRepository.cs` | Parses `.lst` files. |
| `Service/NameGeneratorService.cs` | Parses schema strings, dispatches generators, applies filters and casing. |
| `Service/NameGenerators/Randomiser/RandomiserNameGenerator.cs` | Joins random elements from each word list. |
| `Service/NameGenerators/RandomSelector/RandomSelectorNameGenerator.cs` | Selects random elements from combined word list values. |
| `Logging/MyLogInfoKey.cs` | `Schema`, `Count` log keys. |
| `Logging/MyOperation.cs` | `GenerateNames`, `GetSchemas` operations. |

## UniversalNameGenerator.API.UnitTests

NUnit tests for `NameGeneratorService`, generator strategies, data objects, and mappings. Uses Moq for repository abstractions.

```
UniversalNameGenerator.API.UnitTests/
├── Service/
│   └── NameGeneratorServiceTests.cs
├── DataAccess/
│   └── (data object and repository tests)
└── UniversalNameGenerator.API.UnitTests.csproj
```

## UniversalNameGenerator.API.IntegrationTests

NUnit tests exercising the HTTP pipeline through `WebApplicationFactory`. Uses `IntegrationGenerationDataStore` to create temporary schemas and word lists.

```
UniversalNameGenerator.API.IntegrationTests/
├── Controllers/
│   ├── NamesControllerTests.cs
│   ├── NamesControllerAuthorisationTests.cs
│   ├── NamesControllerCorsTests.cs
│   ├── NamesControllerExceptionHandlingTests.cs
│   ├── NamesControllerRoutingTests.cs
│   └── NamesControllerTestBase.cs
├── EndToEnd/
│   └── (end-to-end generation scenarios)
├── Infrastructure/
│   ├── IntegrationGenerationDataStore.cs
│   └── UniversalNameGeneratorApiFactory.cs
├── Middleware/
│   └── (middleware behaviour tests)
├── Assertions/
│   └── NuciApiResponseAssertions.cs
└── UniversalNameGenerator.API.IntegrationTests.csproj
```

## UniversalNameGenerator.API.ArchitectureTests

ArchUnitNET rules and Roslyn source-file convention tests.

```
UniversalNameGenerator.API.ArchitectureTests/
├── ArchitectureTestBase.cs                  # Assembly loading helper
├── ProductionDependencyArchitectureTests.cs # Dependency direction rules
├── SourceFileConventionTests.cs             # File naming and layout rules
└── UniversalNameGenerator.API.ArchitectureTests.csproj
```

## Module boundaries

| Boundary | Rule |
|----------|------|
| Controllers -> Service | Controllers depend on `INameGeneratorService` only. |
| Service -> DataAccess | Service depends on repository interfaces and data objects. |
| DataAccess -> Service | Prohibited. Data access does not reference service models. |
| API -> Test projects | Prohibited. Test projects reference the API, never the reverse. |
| Production -> Nuci* packages | Permitted. The Nuci ecosystem supplies API, logging, security, DAL, and generators. |

The architecture tests enforce these boundaries. See [Testing](./testing.md).
