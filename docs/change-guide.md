# Change guide

This document describes how to modify common areas of the Universal Name Generator API safely.

## Adding a generator command

1. **Implement the generator** (if not in `NuciGenerators.Text`):
   - Create a new class in `Service/NameGenerators/<CommandName>/`.
   - Implement `INameGenerator` or use a static method.
   - Add unit tests.

2. **Register the command** in `NameGeneratorService.ExecuteCommand`:
   ```csharp
   case "newcommand":
       return NewCommandGenerator.Generate(schema, words);
   ```

3. **Update schema documentation** in `behaviour/name-generation.md`.

4. **Add integration test** for the new command.

5. **Verify** with `dotnet test`.

## Adding a word list

1. Create `{name}.lst` in `Data/Wordlists/` (or configured directory).
2. Follow the format: one `id_value` per line, `#` comments.
3. Reference the word list ID in a schema's command parameters.
4. No code changes required.

## Adding a generation schema

1. Add a `GenerationSchemaDataObject` element to `GenerationSchemas.xml`.
2. Set `Id`, `Name`, `Category`, `Schema`, `FilterlistPath` (optional), `WordCase` (optional).
3. Ensure all referenced word lists exist.
4. Restart the API (or wait for next request if scoped cache).

## Modifying the API key

1. Generate a secure random string (32+ characters).
2. Set `securitySettings:apiKey` in `appsettings.json` or environment variable.
3. Restart the API.
4. Update clients.

## Changing data file paths

1. Update `dataStoreSettings:wordListsRootDirectory` and/or `generationSchemasPath`.
2. Ensure the new paths exist and are readable.
3. Restart the API.

## Adding a configuration setting

1. Add property to the appropriate settings POCO (`DataStoreSettings`, `SecuritySettings`, or new class).
2. Update `appsettings.json` with default.
3. Bind in `Startup.ConfigureServices` if new section.
4. Register in `ServiceCollectionExtensions` if needed.
4. Inject where required.

## Adding middleware

1. Add NuciAPI middleware package or custom middleware.
2. Insert in `Startup.Configure` at the correct position.
3. Order matters: exception handling first, authentication before endpoints.
4. Add integration tests for the new middleware behaviour.

## Adding an endpoint

1. Create controller interface (`I<Name>Controller`) and implementation.
2. Follow NuciAPI `ProcessRequest` pattern.
3. Return `NuciApiResponse<T>`.
4. Register controller (automatic via assembly scanning).
5. Add integration tests: routing, auth, behaviour, error handling.
6. Update `docs/api-reference/`.

## Modifying the schema string format

**Breaking change.** Requires:
1. Update `NameGeneratorService.ParseSchemaString` and `ExecuteCommand`.
2. Update all existing schemas in `GenerationSchemas.xml`.
3. Update documentation.
4. Version the API (new major version).
5. Communicate to consumers.

## Upgrading Nuci* packages

1. Update all Nuci* packages to the same version in `UniversalNameGenerator.API.csproj`.
2. Run `dotnet restore`.
3. Run `dotnet build`.
4. Run `dotnet test`.
5. Check for breaking changes in NuciAPI release notes.
6. Update middleware pipeline if required.

## Upgrading .NET version

1. Update `TargetFramework` in all `.csproj` files to new version (e.g., `net11.0`).
2. Update `dotnet-version` in GitHub Actions workflows.
3. Run `dotnet restore`, `dotnet build`, `dotnet test`.
4. Address any compilation errors or obsolete API usage.

## Adding a test

### Unit test
1. Create test class in `UniversalNameGenerator.API.UnitTests/` mirroring the source structure.
2. Use Moq for dependencies.
3. Follow NUnit patterns.
4. Name: `<ClassName>Tests.cs`.

### Integration test
1. Create test class in `UniversalNameGenerator.API.IntegrationTests/Controllers/` or appropriate area.
2. Inherit from `NamesControllerTestBase` or create new base.
3. Use `IntegrationGenerationDataStore` for test data.
4. Use `NuciApiResponseAssertions` for response verification.

### Architecture test
1. Add rule to `ProductionDependencyArchitectureTests.cs` or `SourceFileConventionTests.cs`.
2. Use ArchUnitNET fluent API.
3. Test must fail before the fix, pass after.

## Refactoring guidelines

- Run `dotnet test` before and after.
- Keep changes small and focused.
- Update documentation in the same PR.
- Don't change public contracts without versioning.
- Preserve ordinal string comparison semantics.
- Don't introduce shared mutable state.

## Code review checklist

- [ ] All tests pass.
- [ ] Documentation updated.
- [ ] No secrets in code or config.
- [ ] No breaking changes without version bump.
- [ ] Architecture tests pass.
- [ ] Security scan passes.
- [ ] Logging uses structured keys.
- [ ] Error handling uses middleware.