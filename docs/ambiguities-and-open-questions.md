# Ambiguities and open questions

This document tracks unresolved items, TODOs, and known gaps in the Universal Name Generator API.

## Open questions

### 1. Hot reload for schemas and word lists
**Question:** Should the API support hot reload of `GenerationSchemas.xml` and `.lst` files without restart?
**Current state:** No hot reload. Files read per request but repository instantiated once per scoped service.
**Considerations:** File watchers add complexity. Current per-request read is simple but may not scale.
**Decision needed:** Add `IFileProvider` watching or keep manual restart.

### 2. Cross-request caching
**Question:** Should word lists and schemas be cached across requests?
**Current state:** New `NameGeneratorService` per request (scoped). Files read each request.
**Considerations:** Memory vs CPU tradeoff. Word lists can be large. Cache invalidation on file change needed.
**Decision needed:** Add `IMemoryCache` or `ConcurrentDictionary` singleton cache with file change tokens.

### 3. Async file I/O
**Question:** Should file reads be async to avoid blocking request threads?
**Current state:** `File.ReadAllLines` and `XmlSerializer.Deserialize` are synchronous.
**Considerations:** Under high load with many distinct word lists, thread pool threads block on disk I/O.
**Decision needed:** Implement async XML deserialisation and `File.ReadAllLinesAsync`.

### 4. Unique name guarantee
**Question:** Should the API guarantee unique names in a single response?
**Current state:** No guarantee. Duplicates possible. Max 1000 attempts may return fewer than requested.
**Considerations:** Client-side deduplication is simple. Server-side requires tracking generated names per request.
**Decision needed:** Add `distinct` parameter or document current behaviour clearly.

### 5. Schema validation at startup
**Question:** Should schemas be validated at startup rather than on first request?
**Current state:** Validation happens on first request for a schema. Malformed schemas cause 500 on first use.
**Considerations:** Fail-fast at startup improves operability. Requires parsing all schemas at startup.
**Decision needed:** Add startup validation in `Startup.ConfigureServices` or keep lazy.

### 6. Multiple generator commands per schema
**Question:** Should a schema support multiple `{command,...}` segments?
**Current state:** `GetGeneratedNameParts` loops while delimiters exist, but `ComposeNames` concatenates parts by index. This implies multiple commands produce parts that are joined positionally.
**Considerations:** Current implementation supports multiple commands but behaviour is unclear. Is this intentional?
**Decision needed:** Document or restrict to single command per schema.

### 7. Markov chain configuration
**Question:** Should Markov order and temperature be configurable per schema?
**Current state:** Hardcoded: `MarkovChainOrder = 4`, `MarkovChainTemperature = 0.0f`.
**Considerations:** Different word lists may need different orders. Temperature controls randomness.
**Decision needed:** Add parameters to schema string: `{markov,wordlist,order,temperature}`.

### 8. Rate limiting
**Question:** Should the API implement rate limiting?
**Current state:** No rate limiting. Relies on infrastructure (reverse proxy, API gateway).
**Considerations:** If exposed directly, vulnerable to abuse. Simple middleware could add per-key limits.
**Decision needed:** Add NuciAPI rate limiting middleware or document infrastructure responsibility.

### 9. Health check endpoint
**Question:** Should a `/health` endpoint be added?
**Current state:** No health endpoint. Infrastructure monitors process/port.
**Considerations:** Kubernetes liveness/readiness probes need HTTP endpoint. Simple to add.
**Decision needed:** Add `MapHealthChecks` in `Startup.Configure`.

### 10. OpenAPI/Swagger documentation
**Question:** Should the API generate OpenAPI documents?
**Current state:** No OpenAPI. Documentation in `docs/api-reference/`.
**Considerations:** Enables client generation, API explorer. Requires `Swashbuckle.AspNetCore` or similar.
**Decision needed:** Add Swashbuckle or maintain manual docs.

### 11. API versioning
**Question:** How should API versioning work when breaking changes are needed?
**Current state:** No versioning. Single endpoint at `/Names`.
**Considerations:** URL versioning (`/v1/Names`), header versioning, or media type versioning.
**Decision needed:** Choose strategy before first breaking change.

### 12. Structured logging correlation IDs
**Question:** Should requests have correlation IDs for distributed tracing?
**Current state:** NuciLog logs `Schema` and `Count` but no request ID.
**Considerations:** Correlation IDs help trace requests across logs. Easy to add via middleware.
**Decision needed:** Add `X-Correlation-ID` header propagation.

### 13. Word list encoding
**Question:** What encoding should `.lst` files use?
**Current state:** `File.ReadAllLines` uses UTF-8 by default. Not explicitly specified.
**Considerations:** Explicit UTF-8 avoids platform differences. BOM handling.
**Decision needed:** Specify `Encoding.UTF8` explicitly or document UTF-8 requirement.

### 14. Schema ID character set
**Question:** What characters are allowed in schema IDs?
**Current state:** No validation. Any string works as XML element content and dictionary key.
**Considerations:** URL-safe characters recommended. Special chars may need encoding in query string.
**Decision needed:** Restrict to `[a-z0-9-]` or document allowed set.

### 15. Filter list case sensitivity
**Question:** Should filter list matching be case-sensitive?
**Current state:** Filter values read as-is from file. Matching uses `Contains` (ordinal, case-sensitive).
**Considerations:** Generator output casing applied after filter. Filter must match pre-casing output.
**Decision needed:** Document clearly or add case-insensitive option.

## Known gaps

### Testing
- [ ] No concurrency/load tests
- [ ] No property-based tests for generators
- [ ] No contract tests for NuciAPI envelope
- [ ] Architecture tests don't cover all layer boundaries

### Documentation
- [ ] No runbook for production incidents
- [ ] No capacity planning guide
- [ ] No migration guide for schema format changes

### Operations
- [ ] No log rotation configuration
- [ ] No metrics export (Prometheus, OpenTelemetry)
- [ ] No graceful shutdown handling for in-flight requests

### Security
- [ ] No API key rotation mechanism
- [ ] No audit logging for auth failures
- [ ] No request size limits (large `count` = memory pressure)

### Data
- [ ] No schema for `GenerationSchemas.xml` (XSD)
- [ ] No validation tool for `.lst` files
- [ ] No backup/restore procedure for data files

### Developer experience
- [ ] No dev container / GitHub Codespaces config
- [ ] No pre-commit hooks for formatting
- [ ] No local development certificate setup guide

## Technical debt

### 1. Synchronous file I/O in request pipeline
**Location:** `NameGeneratorService.GetFilterValues`, `XmlRepository`
**Impact:** Thread pool blocking under load
**Effort:** Medium (async XML deserialisation needed)

### 2. Per-request file reads
**Location:** `NameGeneratorService` (scoped)
**Impact:** Repeated disk I/O for same data
**Effort:** Low (singleton cache with file watcher)

### 3. Hardcoded Markov parameters
**Location:** `NameGeneratorService.MarkovChainOrder`, `MarkovChainTemperature`
**Impact:** Inflexible for different word lists
**Effort:** Low (add schema string parameters)

### 4. No schema validation at startup
**Location:** `Startup.ConfigureServices`
**Impact:** Late discovery of malformed schemas
**Effort:** Low (iterate repository at startup)

### 5. Magic numbers in command parsing
**Location:** `NameGeneratorService` index constants
**Impact:** Fragile if command format changes
**Effort:** Low (already constants, but could use parser)

### 6. `Random` instance per generation
**Location:** `GenerateNames` creates `new Random()`
**Impact:** Poor randomness if called rapidly (same seed)
**Effort:** Low (use `Random.Shared` or inject `Random`)

### 7. No request timeout
**Location:** `NamesController.GetNames`
**Impact:** Long-running generation (100k names) blocks thread
**Effort:** Medium (cancellation token propagation)

### 8. Duplicate code in generator creation
**Location:** `GenerateRandomiserNames`, `GenerateRandomSelectorNames`, `GenerateMarkovNames`
**Impact:** Maintenance burden
**Effort:** Low (extract factory method)

## Future enhancements (not committed)

### 1. WebSocket streaming for large generations
Stream names as generated instead of buffering all in memory.

### 2. Schema composition
Allow schemas to reference other schemas.

### 3. Weighted word lists
Support frequency weights in `.lst` files.

### 4. Conditional generation
If/else logic in schema strings.

### 5. Plugin architecture for generators
Load custom generators from assemblies.

### 6. Admin API for schema management
CRUD endpoints for schemas (requires authz model).

### 7. Metrics dashboard
Built-in `/metrics` endpoint with generation stats.

### 8. Multi-tenancy
Separate API keys with isolated schema/word list namespaces.

## Decision log

| Date | Question | Decision | Rationale |
|------|----------|----------|-----------|
| 2026-11-10 | Hot reload | Deferred | Simplicity; restart is acceptable for current scale |
| 2026-11-10 | Cross-request caching | Deferred | Memory pressure unknown; profile first |
| 2026-11-10 | Async I/O | Deferred | Current load low; monitor thread pool |
| 2026-11-10 | Unique names | Documented | Client-side deduplication simpler |
| 2026-11-10 | Startup validation | Deferred | Lazy validation works; fail-fast later |
| 2026-11-10 | Multiple commands | Documented | Current behaviour works; clarify in docs |
| 2026-11-10 | Markov config | Deferred | Hardcoded works for current use cases |
| 2026-11-10 | Rate limiting | Infrastructure | Reverse proxy responsibility |
| 2026-11-10 | Health endpoint | Deferred | Process monitoring sufficient |
| 2026-11-10 | OpenAPI | Manual docs | Single endpoint; low ROI for Swashbuckle |

## How to use this document

1. **Before starting work:** Check if your task relates to an open question.
2. **When deciding:** Record the decision in the decision log with date and rationale.
3. **When discovering gaps:** Add to known gaps or technical debt.
4. **During reviews:** Ask "Is this documented in ambiguities?"
5. **Quarterly:** Review and update status of items.