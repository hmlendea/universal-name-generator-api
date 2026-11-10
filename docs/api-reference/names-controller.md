# NamesController

`NamesController` exposes the single endpoint of the Universal Name Generator API. It is defined in `UniversalNameGenerator.API/Controllers/NamesController.cs` and implements `INamesController`.

## Endpoint

### GET /Names

Generates names from a generation schema.

| Attribute | Value |
|-----------|-------|
| Method | `GET` |
| Path | `/Names` |
| Authentication | API key required |
| Request body | None |
| Response type | `NuciApiSuccessResponse<GetNamesResponse>` |

#### Query parameters

| Parameter | Type | Required | Default | Constraints | HMAC order |
|-----------|------|----------|---------|-------------|------------|
| `schema` | `string` | Yes | None | Schema identifier matching a `GenerationSchemaDataObject.Id` | 1 |
| `count` | `int` | No | 1 | 1 to 100000 inclusive | 2 |

#### Request example

```http
GET /Names?schema=arabic-toponyms&count=5 HTTP/1.1
Host: localhost:5000
Authorization: Bearer your-api-key
```

#### Success response

HTTP 200 with a NuciAPI success envelope containing a `GetNamesResponse`:

```json
{
  "success": true,
  "response": {
    "names": [
      "Al-Qahira",
      "Dimashq",
      "Misr",
      "Baghdad",
      "Cordoba"
    ]
  }
}
```

The `names` array contains exactly `count` elements (or fewer if the generator cannot produce more unique names, depending on the generator strategy).

#### Error responses

| Condition | HTTP status | Error code |
|-----------|-------------|------------|
| Missing or invalid API key | 401 Unauthorized | Authentication failure |
| Missing `schema` parameter | 400 Bad Request | Validation failure |
| Unknown schema identifier | 400 Bad Request | Schema not found |
| `count` outside 1-100000 | 400 Bad Request | Validation failure |
| Internal failure | 500 Internal Server Error | Unhandled exception |

See [Error handling](../error-handling.md) for the error envelope shape.

## Controller behaviour

1. The controller receives the request through the NuciAPI `ProcessRequest` pattern.
2. It binds `schema` and `count` from the query string into `GetNamesRequest`.
3. It calls `INameGeneratorService.GetNames(schema, count)`.
4. It wraps the returned names in `GetNamesResponse` and returns a `NuciApiSuccessResponse`.

The controller contains no generation logic. All schema interpretation, word list loading, and generation occurs in `NameGeneratorService`.

## Authorisation

The endpoint requires the API key. The `Startup` pipeline applies authentication middleware before endpoint routing. The controller itself does not perform authorisation checks; the middleware rejects unauthenticated requests before they reach the controller.

See [Security](../security.md).

## Logging

Successful generation logs the `GenerateNames` operation with the `Schema` and `Count` structured info keys. See [Logging](../logging.md).

## Testing

The controller is covered by integration tests in `UniversalNameGenerator.API.IntegrationTests/Controllers/`:

- `NamesControllerTests.cs` - Generation behaviour.
- `NamesControllerAuthorisationTests.cs` - API key enforcement.
- `NamesControllerCorsTests.cs` - CORS headers.
- `NamesControllerExceptionHandlingTests.cs` - Error translation.
- `NamesControllerRoutingTests.cs` - Route resolution.

See [Testing](../testing.md).
