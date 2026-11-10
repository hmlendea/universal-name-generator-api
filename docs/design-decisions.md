# Design decisions

This document records key architectural and design choices with rationale.

## Modular monolith

**Decision:** Single ASP.NET Core process with layered architecture.

**Rationale:** Simplicity of deployment, debugging, and operations. No distributed system complexity. Horizontal scaling via multiple instances.

**Consequence:** All components share process lifetime. No independent deployment of services.

## File-backed data

**Decision:** Generation schemas in XML, word lists in `.lst` files. No database.

**Rationale:** Zero operational overhead for data storage. Operators manage files directly. Version control friendly.

**Consequence:** No runtime schema evolution. Changes require restart. No concurrent write support.

## NuciAPI ecosystem

**Decision:** Use NuciAPI packages for middleware, response envelopes, logging, security, and DAL.

**Rationale:** Consistent cross-cutting concerns across projects. Reduces boilerplate. Standardised patterns.

**Consequence:** Framework coupling. Version alignment required across Nuci* packages. Migration effort if NuciAPI changes.

## Schema string DSL

**Decision:** Embed generator commands in schema strings: `{command,param1,param2,...}`.

**Rationale:** Compact representation. Single string stores entire generation logic. Human-readable in XML.

**Consequence:** Custom parsing logic in `NameGeneratorService`. No schema validation at startup. Errors surface at generation time.

## Four generator strategies

**Decision:** Support `random`, `randomiser`, `random-selector`, `markov`.

**Rationale:** Covers common name generation patterns. `NuciGenerators.Text` provides implementations.

**Consequence:** Service must dispatch to four strategies. Adding a fifth requires service modification.

## Scoped service with per-request caches

**Decision:** `NameGeneratorService` is scoped. Caches live per request.

**Rationale:** Simplicity. No cache invalidation logic. Fresh data per request.

**Consequence:** Repeated file reads per request. No cross-request caching benefit. Memory allocated per request.

## API key authentication only

**Decision:** Single API key in `Authorization` header. No OAuth, JWT, or roles.

**Rationale:** Simplicity for machine-to-machine use. Constant-time comparison via `NuciSecurity.HMAC`.

**Consequence:** No fine-grained access control. Key rotation requires config change and restart.

## NuciDAL for XML repository

**Decision:** `GenerationSchemaRepository` inherits `NuciDAL.XmlRepository`.

**Rationale:** Standardised XML deserialisation. Consistent with other Nuci projects.

**Consequence:** XML format tied to `GenerationSchemaDataObject` structure. Schema changes require data object changes.

## Three test project types

**Decision:** Unit, integration, and architecture tests as separate projects.

**Rationale:** Clear separation of concerns. Fast unit tests, realistic integration tests, architectural enforcement.

**Consequence:** More projects to maintain. Test infrastructure duplication (data store setup).

## No health checks

**Decision:** No `/health` endpoint implemented.

**Rationale:** Simple process. Infrastructure-level checks sufficient.

**Consequence:** Orchestrators cannot probe application health. Must rely on process/port checks.

## No rate limiting

**Decision:** No built-in rate limiting.

**Rationale:** Operator responsibility at infrastructure layer (reverse proxy, API gateway).

**Consequence:** Vulnerable to abuse if exposed directly.

## No OpenAPI/Swagger

**Decision:** No OpenAPI document generation.

**Rationale:** Single endpoint with simple contract. Documentation in `docs/api-reference/`.

**Consequence:** No automatic client generation. No interactive API explorer.

## Constant-time API key comparison

**Decision:** Use `NuciSecurity.HMAC` for key validation.

**Rationale:** Mitigates timing attacks. Standardised in Nuci ecosystem.

**Consequence:** Dependency on `NuciSecurity.HMAC` package.

## Ordinal string comparison for schema IDs

**Decision:** Schema resolution uses `string.Equals(schemaId, other, StringComparison.Ordinal)`.

**Rationale:** Predictable, case-sensitive matching. No culture dependence.

**Consequence:** `arabic-toponyms` ≠ `Arabic-Toponyms`. Operators must match case exactly.

## Max attempts in generation loop

**Decision:** `GenerateNames` limits to 1000 attempts.

**Rationale:** Prevents infinite loops for generators that produce many duplicates or empty strings.

**Consequence:** May return fewer than requested names. Caller cannot distinguish "exhausted" from "generated exactly count".

## Word casing after generation

**Decision:** Apply casing transformation after filter list removal.

**Rationale:** Filter list matches original casing. Casing is a presentation concern.

**Consequence:** Filter list values must match pre-casing generator output.

## Placeholder API key in appsettings.json

**Decision:** `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]` placeholder.

**Rationale:** Prevents accidental commits of real keys. Forces operator to configure.

**Consequence:** API fails authentication until configured. No secure default.