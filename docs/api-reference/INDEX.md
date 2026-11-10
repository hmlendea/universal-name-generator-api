# API reference index

This section documents the public HTTP API of the Universal Name Generator API.

## Endpoint summary

| Method | Path | Authentication | Description |
|--------|------|----------------|-------------|
| `GET` | `/Names` | API key (required) | Generate names from a schema |

The API exposes a single endpoint. All other paths return 404.

## Authentication

Every request to `/Names` must present a valid API key in the `Authorization` header. The `Bearer` prefix is optional.

```
Authorization: Bearer <api-key>
Authorization: <api-key>
```

The API key is compared in constant time using `NuciSecurity.HMAC`. Invalid or missing keys return 401 Unauthorized.

See [Security](../security.md) for the full security model.

## Versioning

The API does not implement explicit versioning. The current contract is version 1. Breaking changes require a new major version of the deployment.

## Rate limiting

No rate limiting is implemented. Operators must apply rate limiting at a reverse proxy or network boundary if required.

## Content types

- Request: no body; parameters are query string.
- Response: `application/json` (NuciAPI response envelope).

## Error responses

Errors are returned as NuciAPI error response envelopes with appropriate HTTP status codes. See [Error handling](../error-handling.md).

## Endpoint documentation

- [NamesController](./names-controller.md) - `GET /Names` complete reference.
