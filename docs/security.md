# Security

This document describes the security model, API key authorisation, and threat mitigations for the Universal Name Generator API.

## Security model

The API implements a single-layer security model:

- **Authentication:** API key via `Authorization` header.
- **Authorisation:** None beyond authentication (all authenticated requests have equal access).
- **Transport:** HTTPS enforced via redirection middleware.
- **Input validation:** Query parameter binding with constraints.
- **Rate limiting:** None (operator responsibility).

## API key authentication

### Header format

```
Authorization: Bearer <api-key>
Authorization: <api-key>
```

The `Bearer` prefix is optional. The middleware extracts the key from either format.

### Validation

The key is compared against `SecuritySettings.ApiKey` using constant-time comparison from `NuciSecurity.HMAC`:

```csharp
// NuciSecurity.HMAC provides timing-attack resistant comparison
bool isValid = Hmac.Compare(apiKey, configuredKey);
```

### Failure responses

| Condition | HTTP status | Error code |
|-----------|-------------|------------|
| Missing header | 401 | AuthenticationError |
| Invalid key | 401 | AuthenticationError |
| Malformed header | 401 | AuthenticationError |

The exception handling middleware translates authentication failures to `NuciApiErrorResponse`.

## Threat mitigations

| Threat | Mitigation |
|--------|------------|
| Timing attacks on API key | Constant-time comparison via `NuciSecurity.HMAC`. |
| Scanner/probe traffic | `UseNuciApiScannerProtection` blocks known scanner user agents. |
| CORS misconfiguration | NuciAPI CORS middleware with explicit policy. |
| Information disclosure in errors | Exception handling middleware returns generic error messages. |
| HTTP downgrade | HTTPS redirection middleware. |
| Request smuggling | Kestrel defaults, no custom body parsing. |

## Data protection

- No personal data is collected, processed, or stored.
- No secrets are logged.
- The API key is the only secret; it is stored in configuration and compared in constant time.
- Word lists and schemas are operator-supplied data files; the API only reads them.

## Self-hosted deployment responsibilities

Operators must:

1. Replace the placeholder API key with a secure random string.
2. Use HTTPS in production (configure valid certificates).
3. Apply rate limiting at a reverse proxy if needed.
4. Protect the data files from unauthorised modification.
5. Secure the log file if it contains sensitive operational data.

See [PRIVACY.md](../PRIVACY.md) for the full data handling description.

## Security scanning

The repository includes a GitHub Actions security workflow (`.github/workflows/security.yml`) that runs:

- Dependency vulnerability scanning.
- Static analysis.
- Secret scanning.

See [Build and deployment](../build-and-deployment.md).

## Testing

Integration tests in `UniversalNameGenerator.API.IntegrationTests/Controllers/NamesControllerAuthorisationTests.cs` verify:

- Valid key allows access.
- Missing key returns 401.
- Invalid key returns 401.
- Bearer prefix is optional.

See [Testing](../testing.md).