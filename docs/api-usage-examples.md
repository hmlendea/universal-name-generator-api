# API usage examples

This document provides example requests and responses for the Universal Name Generator API.

## Base URL

```
http://localhost:5000
```

Replace with your deployment URL.

## Authentication

All requests require the `Authorization` header:

```bash
curl -H "Authorization: Bearer YOUR_API_KEY" ...
```

Or without Bearer prefix:

```bash
curl -H "Authorization: YOUR_API_KEY" ...
```

## Get names

### Request

```http
GET /Names?schema=arabic-toponyms&count=5
Authorization: Bearer YOUR_API_KEY
```

### Parameters

| Parameter | Type | Required | Default | Constraints |
|-----------|------|----------|---------|-------------|
| `schema` | string | Yes | - | Must match a schema `Id` exactly |
| `count` | integer | No | 1 | 1-100000 |

### Successful response (200 OK)

```json
{
  "success": true,
  "data": {
    "names": [
      "Al-Qahirah",
      "Dimashq",
      "Baghdad",
      "Riyadh",
      "Abu Dhabi"
    ]
  },
  "errorCode": null,
  "errorMessage": null
}
```

### Error responses

#### 400 Bad Request - Missing schema

```json
{
  "success": false,
  "data": null,
  "errorCode": "ValidationError",
  "errorMessage": "The schema parameter is required."
}
```

#### 400 Bad Request - Schema not found

```json
{
  "success": false,
  "data": null,
  "errorCode": "SchemaNotFound",
  "errorMessage": "Schema 'unknown-schema' not found."
}
```

#### 400 Bad Request - Count out of range

```json
{
  "success": false,
  "data": null,
  "errorCode": "ValidationError",
  "errorMessage": "Count must be between 1 and 100000."
}
```

#### 401 Unauthorized

```json
{
  "success": false,
  "data": null,
  "errorCode": "Unauthorized",
  "errorMessage": "Invalid or missing API key."
}
```

#### 500 Internal Server Error

```json
{
  "success": false,
  "data": null,
  "errorCode": "InternalServerError",
  "errorMessage": "An error occurred while processing your request."
}
```

## Example schemas

### Arabic toponyms (randomiser)

**Schema:** `arabic-toponyms`
**Command:** `{randomiser,arabic-prefixes,arabic-roots,arabic-suffixes}`

```bash
curl -H "Authorization: Bearer YOUR_API_KEY" \
  "http://localhost:5000/Names?schema=arabic-toponyms&count=3"
```

Response:
```json
{
  "success": true,
  "data": {
    "names": [
      "Al-Qahirah",
      "Abu Dhabi",
      "Beni Mellal"
    ]
  },
  "errorCode": null,
  "errorMessage": null
}
```

### Fantasy names (random-selector)

**Schema:** `fantasy-names`
**Command:** `{random-selector,fantasy-prefixes,fantasy-roots,fantasy-suffixes}`

```bash
curl -H "Authorization: Bearer YOUR_API_KEY" \
  "http://localhost:5000/Names?schema=fantasy-names&count=5"
```

Response:
```json
{
  "success": true,
  "data": {
    "names": [
      "Eldarion",
      "Thranduil",
      "Galadriel",
      "Celeborn",
      "Elrond"
    ]
  },
  "errorCode": null,
  "errorMessage": null
}
```

### Simple choices (random)

**Schema:** `coin-flip`
**Command:** `{random,Heads,Tails}`

```bash
curl -H "Authorization: Bearer YOUR_API_KEY" \
  "http://localhost:5000/Names?schema=coin-flip&count=10"
```

Response:
```json
{
  "success": true,
  "data": {
    "names": [
      "Heads",
      "Tails",
      "Heads",
      "Heads",
      "Tails",
      "Tails",
      "Heads",
      "Tails",
      "Heads",
      "Heads"
    ]
  },
  "errorCode": null,
  "errorMessage": null
}
```

### Markov chain (markov)

**Schema:** `elvish-places`
**Command:** `{markov,elvish-words,3}`

```bash
curl -H "Authorization: Bearer YOUR_API_KEY" \
  "http://localhost:5000/Names?schema=elvish-places&count=3"
```

Response:
```json
{
  "success": true,
  "data": {
    "names": [
      "Lothlorien",
      "Rivendell",
      "Mirkwood"
    ]
  },
  "errorCode": null,
  "errorMessage": null
}
```

## cURL examples

### Basic request

```bash
curl -X GET "http://localhost:5000/Names?schema=arabic-toponyms&count=5" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -H "Accept: application/json"
```

### With verbose output

```bash
curl -v -X GET "http://localhost:5000/Names?schema=arabic-toponyms&count=5" \
  -H "Authorization: Bearer YOUR_API_KEY"
```

### Save to file

```bash
curl -X GET "http://localhost:5000/Names?schema=arabic-toponyms&count=100" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -o names.json
```

### Using environment variable for key

```bash
export API_KEY="YOUR_API_KEY"
curl -H "Authorization: Bearer $API_KEY" \
  "http://localhost:5000/Names?schema=arabic-toponyms&count=5"
```

## PowerShell examples

### Basic request

```powershell
$headers = @{ "Authorization" = "Bearer YOUR_API_KEY" }
Invoke-RestMethod -Uri "http://localhost:5000/Names?schema=arabic-toponyms&count=5" -Headers $headers
```

### Save to file

```powershell
$headers = @{ "Authorization" = "Bearer YOUR_API_KEY" }
Invoke-RestMethod -Uri "http://localhost:5000/Names?schema=arabic-toponyms&count=100" -Headers $headers | ConvertTo-Json -Depth 5 | Out-File names.json
```

## Python examples

### Using requests

```python
import requests

headers = {"Authorization": "Bearer YOUR_API_KEY"}
params = {"schema": "arabic-toponyms", "count": 5}

response = requests.get("http://localhost:5000/Names", headers=headers, params=params)
data = response.json()

if data["success"]:
    for name in data["data"]["names"]:
        print(name)
else:
    print(f"Error: {data['errorCode']} - {data['errorMessage']}")
```

### Using httpx (async)

```python
import httpx

async def get_names():
    headers = {"Authorization": "Bearer YOUR_API_KEY"}
    params = {"schema": "arabic-toponyms", "count": 5}

    async with httpx.AsyncClient() as client:
        response = await client.get("http://localhost:5000/Names", headers=headers, params=params)
        data = response.json()

        if data["success"]:
            return data["data"]["names"]
        else:
            raise Exception(f"{data['errorCode']}: {data['errorMessage']}")
```

## JavaScript/TypeScript examples

### Using fetch

```javascript
async function getNames(schema, count = 5) {
  const response = await fetch(
    `http://localhost:5000/Names?schema=${schema}&count=${count}`,
    {
      headers: {
        "Authorization": "Bearer YOUR_API_KEY",
        "Accept": "application/json"
      }
    }
  );

  const data = await response.json();

  if (data.success) {
    return data.data.names;
  } else {
    throw new Error(`${data.errorCode}: ${data.errorMessage}`);
  }
}

// Usage
getNames("arabic-toponyms", 5).then(names => console.log(names));
```

### Using axios

```javascript
const axios = require('axios');

const client = axios.create({
  baseURL: 'http://localhost:5000',
  headers: {
    'Authorization': 'Bearer YOUR_API_KEY'
  }
});

async function getNames(schema, count = 5) {
  const response = await client.get('/Names', { params: { schema, count } });
  const data = response.data;

  if (data.success) {
    return data.data.names;
  } else {
    throw new Error(`${data.errorCode}: ${data.errorMessage}`);
  }
}
```

## C# examples

### Using HttpClient

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class NameGeneratorClient
{
    private readonly HttpClient _httpClient;

    public NameGeneratorClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string[]> GetNamesAsync(string schema, int count = 5)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<NamesData>>(
            $"/Names?schema={schema}&count={count}");

        if (response.Success)
        {
            return response.Data.Names;
        }

        throw new Exception($"{response.ErrorCode}: {response.ErrorMessage}");
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}

public class NamesData
{
    public string[] Names { get; set; }
}
```

## Batch generation

Generate multiple schemas in parallel (client-side):

```bash
#!/bin/bash
API_KEY="YOUR_API_KEY"
BASE_URL="http://localhost:5000"

schemas=("arabic-toponyms" "fantasy-names" "elvish-places")

for schema in "${schemas[@]}"; do
  curl -s -H "Authorization: Bearer $API_KEY" \
    "$BASE_URL/Names?schema=$schema&count=10" \
    -o "${schema}.json" &
done

wait
echo "All requests completed"
```

## Rate limiting

No built-in rate limiting. Implement at reverse proxy or client-side:

```bash
# Client-side delay
for i in {1..100}; do
  curl -H "Authorization: Bearer $API_KEY" \
    "http://localhost:5000/Names?schema=arabic-toponyms&count=10"
  sleep 0.1
done
```

## Response envelope

All responses follow the NuciAPI envelope:

```csharp
public abstract class NuciApiResponse
{
    public bool Success { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}

public class NuciApiSuccessResponse<T> : NuciApiResponse
{
    public T Data { get; set; }
}

public class NuciApiErrorResponse : NuciApiResponse
{
    // ErrorCode and ErrorMessage populated
}
```

Success: `Success=true`, `Data` populated, `ErrorCode=null`, `ErrorMessage=null`.
Error: `Success=false`, `Data=null`, `ErrorCode` and `ErrorMessage` populated.