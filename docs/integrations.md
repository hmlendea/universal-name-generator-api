# Integrations

This document describes external system integrations for the Universal Name Generator API.

## External integrations

The API has **no external service integrations**. It does not call:

- Databases
- Message queues
- External APIs
- Authentication providers (beyond API key)
- Telemetry services
- Feature flag services
- Configuration services

## Filesystem integration

The only external integration is the local filesystem:

| Integration | Direction | Protocol | Failure semantics |
|-------------|-----------|----------|-------------------|
| `GenerationSchemas.xml` | Read | File I/O | Exception -> 500 |
| `.lst` word lists | Read | File I/O | Exception -> 500 |
| Log file | Write | File I/O | Silent failure (NuciLog) |

## NuGet package dependencies

All external code is delivered via NuGet packages. See [Dependencies](../dependencies.md).

| Package | Role |
|---------|------|
| `NuciAPI.*` | HTTP framework, middleware, response envelopes |
| `NuciDAL` | XML data access |
| `NuciGenerators.Text` | Name generation algorithms |
| `NuciLog` | Structured logging |
| `NuciSecurity.HMAC` | Constant-time comparison |

These are compile-time dependencies, not runtime service integrations.

## Operator-provided data

Operators supply:

1. `GenerationSchemas.xml` - Schema definitions.
2. `.lst` word list files - Generation vocabulary.

These are not "integrations" in the service sense; they are configuration data files.

## Future integration points

If integrations are added, they should follow these patterns:

- **Outbound HTTP:** Use `IHttpClientFactory` with typed clients.
- **Authentication:** Extend the middleware pipeline, not the controller.
- **Configuration:** Add settings sections, bind to POCOs.
- **Logging:** Use existing NuciLog operations and info keys.
- **Testing:** Add integration test infrastructure for the new dependency.

## Testing

No integration tests for external services (none exist). Unit tests mock all external dependencies via interfaces.

See [Testing](../testing.md).