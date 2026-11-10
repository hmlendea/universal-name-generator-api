# Concurrency and scheduling

This document describes the threading model, async boundaries, and shared-state protection for the Universal Name Generator API.

## Threading model

The API follows the standard ASP.NET Core threading model:

- **Request processing:** Each request runs on a thread pool thread.
- **Async/await:** Used throughout for I/O (file reads, though current implementation is synchronous).
- **No background threads:** No `IHostedService`, `BackgroundService`, or `Task.Run` fire-and-forget.
- **No custom thread pools:** Uses the default .NET thread pool.

## Async boundaries

| Operation | Sync/Async | Notes |
|-----------|------------|-------|
| HTTP request handling | Async | ASP.NET Core default |
| File reads (schemas, word lists) | **Sync** | `File.ReadAllLines`, `XmlSerializer.Deserialize` |
| Generation | Sync | Pure in-memory computation |
| Logging | Async | NuciLog async sinks |
| Middleware | Async | NuciAPI middleware |

**Note:** File reads are currently synchronous. This blocks the request thread during I/O. For high-throughput scenarios, consider `File.ReadAllLinesAsync` and async XML deserialisation.

## Shared state

### Per-request (scoped)

| State | Owner | Protection |
|-------|-------|------------|
| Schema cache | `NameGeneratorService` | Scoped lifetime = one request |
| Generator cache | `NameGeneratorService` | Scoped lifetime = one request |
| Word lists loaded | `NameGeneratorService` | Local variables, not shared |

No locks needed: each request gets its own service instance.

### Singleton

| State | Owner | Protection |
|-------|-------|------------|
| `DataStoreSettings` | DI singleton | Immutable after startup |
| `SecuritySettings` | DI singleton | Immutable after startup |
| `NuciLogger` | DI singleton | Thread-safe by design |
| `NuciLoggerSettings` | DI singleton | Immutable after startup |

No locks needed: immutable after configuration binding.

### Process-wide

| State | Owner | Protection |
|-------|-------|------------|
| File system | Operator | External |
| Log file | NuciLog | Internal synchronisation |

## Concurrency limits

No explicit concurrency limits. The API scales with the ASP.NET Core thread pool. Under load:

- Thread pool grows to handle concurrent requests.
- Each request allocates its own service instance and caches.
- Memory use grows with concurrent requests × schema complexity.

## Scheduling

No scheduled work. No timers, no periodic tasks, no background jobs.

## Resource disposal

- `NameGeneratorService` implements no `IDisposable`.
- Repositories implement no `IDisposable`.
- File handles: `File.ReadAllLines` opens and closes per call.
- Log file: NuciLog manages its own handle.

## Thread safety guarantees

| Component | Thread-safe? | Reason |
|-----------|--------------|--------|
| `NameGeneratorService` | Yes (per request) | Scoped lifetime, no shared mutable state |
| `GenerationSchemaRepository` | Yes | Stateless, reads only |
| `WordRepository` | Yes | Stateless, reads only |
| Generators | Yes | Stateless, `Random.Shared` is thread-safe |
| Settings POCOs | Yes | Immutable after startup |
| `NuciLogger` | Yes | Framework guarantee |

## Potential contention points

1. **File I/O:** Synchronous file reads on the request thread. Under high concurrency with many distinct word lists, thread pool threads block on disk I/O.
2. **Log file:** NuciLog serialises writes internally.
3. **Random:** `Random.Shared` is thread-safe but may contend under extreme load.

## Recommendations

For higher throughput:

1. Make file reads async (`File.ReadAllLinesAsync`, custom async XML deserialisation).
2. Add a singleton word list cache with `IMemoryCache` or `ConcurrentDictionary`.
3. Consider `ArrayPool` for large word list arrays.
4. Monitor thread pool saturation metrics.

## Testing

No specific concurrency tests. Integration tests run sequentially. Architecture tests do not cover threading.

See [Testing](../testing.md).