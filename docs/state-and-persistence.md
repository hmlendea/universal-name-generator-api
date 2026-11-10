# State and persistence

This document describes the file-backed state, in-memory caches, and lifecycle of the Universal Name Generator API.

## Persistent state

The API has no database. All persistent state is in operator-managed files:

| File | Format | Owner | Mutability |
|------|--------|-------|------------|
| `GenerationSchemas.xml` | XML | Operator | Read-only at runtime |
| `*.lst` word lists | Plain text | Operator | Read-only at runtime |
| `logfile.log` | JSON lines | API | Append-only |

### GenerationSchemas.xml

- Location: `dataStoreSettings:generationSchemasPath` (default `Data/GenerationSchemas.xml`).
- Contains all generation schemas as an XML array.
- Read once at first request, cached in memory.
- Changes require process restart to take effect.

### Word list files (.lst)

- Location: `dataStoreSettings:wordListsRootDirectory` (default `Data/Wordlists/`).
- One file per word list: `{id}.lst`.
- Read on demand when a schema references the word list.
- Cached per service instance (scoped, so per request).
- Changes require process restart to take effect (new request = new service instance = fresh read).

### Log file

- Location: `nuciLoggerSettings:logFilePath` (default `logfile.log`).
- Written by NuciLog when `isFileOutputEnabled` is true.
- Append-only, no rotation implemented.
- Operator manages rotation/archival.

## In-memory caches

### Schema cache

- Location: `NameGeneratorService._schemasCache` (Dictionary<string, GenerationSchema>).
- Lifetime: Scoped (per request).
- Populated: First call to `GetSchemas()` or `GetNames()`.
- Contents: All schemas mapped from data objects.
- Invalidation: New request = new service instance = fresh cache.

### Generator cache

- Location: `NameGeneratorService._generatorCache` (Dictionary<string, INameGenerator>).
- Lifetime: Scoped (per request).
- Populated: First use of a generator for a schema.
- Contents: Generator instances per schema.
- Invalidation: New request = new service instance = fresh cache.

### Word list cache

- No explicit cache in `WordRepository`. Each `GetWords` call reads the file.
- `NameGeneratorService` loads word lists per generation call and passes them to generators.
- Could be optimised with a cache if needed.

## Lifecycle

### Startup

1. Host starts.
2. Configuration bound.
3. Services registered.
4. Middleware pipeline built.
5. No data files read yet.

### First request

1. Request reaches `NameGeneratorService`.
2. `GetSchemas()` loads and caches all schemas.
3. `GetNames()` loads required word lists.
4. Generation executes.
5. Response returned.

### Subsequent requests

1. New scoped `NameGeneratorService` instance.
2. Fresh schema cache populated on first call.
3. Fresh generator cache.
4. Word lists read again (no cross-request cache).

### Shutdown

1. Host stops.
2. In-memory caches discarded.
3. Log file closed.

## Operator responsibilities

- Manage `GenerationSchemas.xml` and `.lst` files.
- Ensure files are readable by the API process.
- Restart the API after data file changes.
- Manage log file rotation.

## Data integrity

- No writes to data files by the API.
- No transactions or consistency guarantees across files.
- Schema references to word lists are not validated at startup (only at generation time).
- Missing word lists cause 500 errors at generation time.

## Testing

Integration tests use `IntegrationGenerationDataStore` to create isolated temporary data files for each test run.

See [Testing](../testing.md).