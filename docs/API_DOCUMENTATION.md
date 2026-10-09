# Universal Name Generator API - API Documentation

## Overview

This document provides comprehensive documentation for the Universal Name Generator API, including endpoints, request/response formats, authentication, and usage examples.

---

## Base URL

```
https://api.universal-name-generator.example.com/v1
```

---

## Authentication

### API Key

All requests require an API key in the `Authorization` header:

```http
Authorization: Bearer YOUR_API_KEY
```

**Configuration**:
- Set via `securitySettings:apiKey` in configuration
- Must not be committed to source control
- Default placeholder: `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]`

**Validation**:
- Constant-time comparison via `NuciSecurity.HMAC`
- Supports `Bearer` prefix or direct key
- Case-sensitive

---

## Endpoints

### Generate Names

**Endpoint**: `GET /Names`

**Purpose**: Generate random names based on a schema

**Request Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| schema | string | No | Schema identifier (e.g., "astora-settlements") |
| count | integer | No | Number of names to generate (default: 1, max: 100000) |

**Headers**:
- `Authorization: Bearer {apiKey}` (required)
- `X-HMAC: {token}` (optional, recommended for additional security)

**Example Request**:
```http
GET /Names?schema=astora-settlements&count=5
Authorization: Bearer NucileRullz!
```

**Success Response**:
```json
{
    "success": true,
    "data": {
        "names": [
            "Solara",
            "Cluj-Napoca",
            "Oradea",
            "Nucilandia",
            "Solaire"
        ]
    }
}
```

**Error Responses**:

1. **Authentication Failure** (401):
```json
{
    "success": false,
    "error": {
        "code": "AuthenticationFailure",
        "message": "Authentication failed"
    }
}
```

2. **Validation Error** (400):
```json
{
    "success": false,
    "error": {
        "code": "ValidationFailure",
        "message": "Validation failed for one or more fields"
    }
}
```

3. **Schema Not Found** (404):
```json
{
    "success": false,
    "error": {
        "code": "SchemaNotFound",
        "message": "Schema 'unknown-schema' was not discovered"
    }
}
```

---

## Schema Format

Schemas define how names are generated. They contain generator commands in `{command}` syntax:

```
{random,wordlist1|wordlist2,1,10}text{randomiser,wordlist1|wordlist2,separator,1,10}{markov,wordlist1|wordlist2,1,10}
```

**Command Syntax**:

1. **Random**: Simple random string generation
   - `{random,wordlist1|wordlist2,minLength,maxLength}`
   - Generates random strings by concatenating random elements

2. **Randomiser**: Random combination with separator
   - `{randomiser,separator,wordlist1|wordlist2,minLength,maxLength,wordlistKeys}`
   - Joins random elements from multiple wordlists with separator

3. **Random Selector**: Random selection from word lists
   - `{random-selector,minLength,maxLength,wordlistKeys}`
   - Selects random elements from specified wordlists

4. **Markov Chain**: Markov chain-based generation
   - `{markov,minLength,maxLength,wordlistKeys}`
   - Uses N-gram model for realistic name generation

---

## Word List Format

Word lists are stored in `.lst` files with the following format:

```
identifier_value1
identifier_value2
# This is a comment
another_identifier_another_value
```

**Parsing Rules**:
- Lines starting with `#` are comments and ignored
- Format: `identifier_value` where `_` separates identifier from value
- Values are grouped by identifier
- If no `_` present, entire line is identifier with single value

**Example Word List**:
```
settlement_Solara
settlement_Cluj-Napoca
settlement_Oradea
settlement_Nucilandia
settlement_Solaire
# End of file
```

---

## Usage Examples

### cURL Example

```bash
curl -G "https://api.universal-name-generator.example.com/v1/Names" \
  -H "Authorization: Bearer NucileRullz!" \
  --data-urlencode "schema=arabic-toponyms" \
  --data-urlencode "count=5"
```

### JavaScript Example

```javascript
async function generateNames() {
    const response = await fetch(
        "https://api.universal-name-generator.example.com/v1/Names?schema=astora-settlements&count=5",
        {
            headers: {
                "Authorization": "Bearer NucileRullz!"
            }
        }
    );
    const data = await response.json();
    console.log(data.data.names);
}
```

### Python Example

```python
import requests

url = "https://api.universal-name-generator.example.com/v1/Names"
params = {
    "schema": "city-selection",
    "count": 5
}
headers = {
    "Authorization": "Bearer NucileRullz!"
}

response = requests.get(url, params=params, headers=headers)
print(response.json()['data']['names'])
```

---

## Error Handling

### Common Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| AuthenticationFailure | 401 | Invalid or missing API key |
| ValidationFailure | 400 | Invalid request parameters |
| SchemaNotFound | 404 | Requested schema not found |
| InternalServerError | 500 | Unexpected server error |

### Error Response Format

```json
{
    "success": false,
    "error": {
        "code": "ErrorCode",
        "message": "Error description"
    }
}
```

---

## Rate Limiting

The API implements rate limiting to prevent abuse. Current limits:

- 100 requests per minute per API key
- 1000 requests per hour per API key

Exceeding these limits will result in a `429 Too Many Requests` response.

---

## Versioning

The API follows semantic versioning (SemVer) with the following format:

```
/v{major}/Names
```

**Current Version**: v1

---

## Changelog

**v1.0.0**: Initial release
- Basic name generation functionality
- Random, randomiser, random-selector, and Markov chain strategies
- XML schema and `.lst` word list support
- API key authentication

**v1.1.0**: Added integration tests
- Enhanced error handling
- Improved documentation

**v1.1.1**: Dependency updates
- Bug fixes
- Documentation improvements

---

## Support

For questions or issues, please contact the project maintainers via GitHub issues at:

```
https://github.com/hmlendea/universal-name-generator-api/issues
```

For self-hosted instances, contact the instance operator unless the project explicitly handles the request.