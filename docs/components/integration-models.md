# Integration models

This document covers the request/response contracts and NuciAPI base types used at the HTTP boundary.

## GetNamesRequest

`UniversalNameGenerator.API/Models/GetNamesRequest.cs`

```csharp
public class GetNamesRequest : NuciApiRequest
{
    [NuciApiProperty(HmacOrder = 1)]
    public string Schema { get; set; }

    [NuciApiProperty(HmacOrder = 2, DefaultValue = 1)]
    public int Count { get; set; } = 1;
}
```

### Properties

| Property | Type | HMAC Order | Default | Constraints |
|----------|------|------------|---------|-------------|
| `Schema` | string | 1 | None | Required, matches schema `Id` |
| `Count` | int | 2 | 1 | 1 to 100000 |

The `NuciApiProperty` attributes control HMAC signing order for request integrity verification. The `DefaultValue` attribute provides the default when the query parameter is omitted.

### Binding

The controller binds query string parameters to this request object through the NuciAPI model binder. The `Schema` parameter maps to the `schema` query string, and `Count` maps to `count`.

## GetNamesResponse

`UniversalNameGenerator.API/Models/GetNamesResponse.cs`

```csharp
public class GetNamesResponse : NuciApiResponse
{
    public IReadOnlyCollection<string> Names { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Names` | `IReadOnlyCollection<string>` | Generated names array |

Inherits from `NuciApiResponse` which provides the base response envelope.

## NuciAPI base types

The API uses NuciAPI response envelopes for all HTTP responses.

### NuciApiRequest

Base class for all request models. Provides HMAC signing support.

### NuciApiResponse

Base class for all response models. Contains:

```csharp
public abstract class NuciApiResponse
{
    public bool Success { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
```

### NuciApiSuccessResponse<T>

Generic success envelope:

```csharp
public class NuciApiSuccessResponse<T> : NuciApiResponse
{
    public T Response { get; set; }
}
```

The controller returns `NuciApiSuccessResponse<GetNamesResponse>` on success.

### NuciApiErrorResponse

Error envelope:

```csharp
public class NuciApiErrorResponse : NuciApiResponse
{
    public NuciApiErrorResponse(string errorCode, string errorMessage)
    {
        Success = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}
```

Returned by the exception handling middleware for all error conditions.

## Controller return type

`NamesController.ProcessRequest` returns:

```csharp
Task<NuciApiResponse<GetNamesResponse>>
```

The NuciAPI middleware serialises this to JSON with the envelope structure.

## JSON examples

### Success response

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

### Error response

```json
{
  "success": false,
  "errorCode": "ValidationError",
  "errorMessage": "Schema 'unknown' not found."
}
```

## Testing

Integration tests in `UniversalNameGenerator.API.IntegrationTests/Assertions/NuciApiResponseAssertions.cs` provide fluent assertions for the response envelope.

See [Testing](../testing.md).