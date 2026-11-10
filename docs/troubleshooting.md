# Troubleshooting

Common issues and solutions for the Universal Name Generator API.

## API returns 401 Unauthorized

**Cause:** Missing or invalid API key.

**Solutions:**
1. Check `Authorization` header is present: `Authorization: Bearer <key>` or `Authorization: <key>`.
2. Verify key matches `securitySettings:apiKey` in `appsettings.json`.
3. Ensure no extra whitespace in header value.
4. Check placeholder `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]` was replaced.

## API returns 400 Bad Request - SchemaNotFound

**Cause:** Requested schema ID not found in `GenerationSchemas.xml`.

**Solutions:**
1. Verify schema `Id` matches exactly (case-sensitive, ordinal comparison).
2. Check `GenerationSchemas.xml` contains the schema.
3. Ensure XML is well-formed and deserialises correctly.
4. Restart API after modifying `GenerationSchemas.xml` (no hot reload).

## API returns 400 Bad Request - Count out of range

**Cause:** `count` parameter < 1 or > 100000.

**Solutions:**
1. Ensure `count` is integer between 1 and 100000.
2. Default is 1 if omitted.

## API returns 500 Internal Server Error

**Cause:** Unhandled exception during generation.

**Common causes:**
1. **Missing word list file:** Schema references a `.lst` file that doesn't exist.
   - Check `FilterlistPath` in schema.
   - Verify file exists in `wordListsRootDirectory`.
   - Check file permissions.
2. **Malformed `.lst` file:** Parsing error in `WordRepository`.
   - Verify underscore separator format: `id_value`.
   - Check for encoding issues (UTF-8 expected).
3. **Invalid schema string:** Unknown command or malformed parameters.
   - Check `Schema` field in `GenerationSchemas.xml`.
   - Verify command syntax: `{command,param1,param2}`.
4. **Markov generator failure:** `NuciGenerators.Text` exception.
   - Check word list has sufficient data for Markov order.

**Debugging:**
1. Check log file (`logfile.log` by default).
2. Enable debug logging in `appsettings.json`:
   ```json
   "nuciLoggerSettings": { "isFileOutputEnabled": true, "logFilePath": "logfile.log" }
   ```
3. Reproduce with integration test.

## No names generated (empty array)

**Cause:** Generator produced no valid names within 1000 attempts.

**Solutions:**
1. Check filter list doesn't remove all generated names.
2. Verify word lists have data.
3. For `random` command: ensure choices provided.
4. For `markov`: ensure word list has enough unique tokens.

## Duplicate names in response

**Cause:** Generator produces duplicates; filter list doesn't remove them.

**Solutions:**
1. Increase word list diversity.
2. Use `randomiser` with more word lists.
3. Accept duplicates (current behaviour: returns up to `count` names, may include duplicates).

## Schema changes not reflected

**Cause:** No hot reload; scoped service caches per request but repository reads file each time.

**Solutions:**
1. Restart the API process.
2. Verify file path in `dataStoreSettings:generationSchemasPath`.
3. Check XML syntax.

## Word list changes not reflected

**Cause:** Same as above - file read per request but process restart may be needed if path changed.

**Solutions:**
1. Verify `wordListsRootDirectory` is correct.
2. Check file exists and is readable.
3. Restart if configuration changed.

## High latency / timeouts

**Cause:** Synchronous file I/O on request thread.

**Solutions:**
1. Monitor disk I/O.
2. Consider SSD for data files.
3. Reduce word list sizes.
4. Future: async file reads (not yet implemented).

## High memory usage

**Cause:** Large word lists loaded per request; many concurrent requests.

**Solutions:**
1. Monitor memory per request.
2. Consider singleton cache for word lists (future enhancement).
3. Limit concurrent requests at infrastructure layer.

## CORS errors from browser

**Cause:** Origin not allowed.

**Solutions:**
1. Check `NuciAPI.Middleware.Cors` configuration in `Startup.cs`.
2. Verify `Access-Control-Allow-Origin` header in response.
3. For development: ensure `localhost` port is allowed.

## Scanner protection blocking legitimate requests

**Cause:** `NuciAPI.Middleware.ScannerProtection` detects scanner patterns.

**Solutions:**
1. Check log for blocked request details.
2. Adjust scanner protection rules if needed (NuciAPI configuration).
3. Ensure legitimate clients don't match scanner signatures.

## Logging not appearing

**Cause:** File output disabled or path issue.

**Solutions:**
1. Check `nuciLoggerSettings:isFileOutputEnabled` is `true`.
2. Verify `logFilePath` directory exists and is writable.
3. Check console output (always enabled).

## Integration tests fail

**Common causes:**
1. **Port conflicts:** Tests use random ports; ensure no firewall blocks.
2. **Temp directory permissions:** `IntegrationGenerationDataStore` needs write access.
3. **Missing test data:** Verify test schemas/word lists created correctly.
4. **Timing:** Async operations not awaited.

**Debugging:**
1. Run single test: `dotnet test --filter "FullyQualifiedName~TestName"`
2. Check test output for detailed errors.
3. Verify `IntegrationGenerationDataStore` creates expected files.

## Architecture tests fail

**Cause:** New dependency violates layer rules.

**Solutions:**
1. Check `ProductionDependencyArchitectureTests.cs` for rule.
2. Ensure Controllers -> Service -> DataAccess direction.
3. No direct Controller -> DataAccess.
4. No Service -> Controllers.

## Build fails

**Common causes:**
1. **NuGet restore failed:** Run `dotnet restore`.
2. **Version mismatch:** All Nuci* packages must align.
3. **Target framework:** Ensure .NET 10 SDK installed.
4. **Missing generated files:** Clean and rebuild.

## Deployment issues

**Process won't start:**
1. Check `dotnet --version` matches target framework.
2. Verify `appsettings.json` present and valid JSON.
3. Check data file paths exist.
4. Check port not in use.

**Process crashes:**
1. Check logs for unhandled exceptions.
2. Verify API key configured (not placeholder).
3. Check file permissions on data directory.

## Getting help

1. Check logs first (`logfile.log`).
2. Run integration tests locally.
3. Review `docs/invariants.md` for expected behaviour.
4. Check GitHub Issues for known problems.