[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/universal-name-generator)](https://github.com/hmlendea/universal-name-generator-api/releases/latest)
[![Build Status](https://github.com/hmlendea/universal-name-generator-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/universal-name-generator-api/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# Universal Name Generator API

Universal Name Generator API is an ASP.NET Core REST API for generating random names based on configurable generation schemas backed by wordlists.

The API supports:

- generating names by schema ID
- Markov chain and randomiser generation strategies

## Table of Contents

- [Overview](#overview)
- [Requirements](#requirements)
- [Configuration](#configuration)
- [Run the API](#run-the-api)
- [API Reference](#api-reference)
- [Development](#development)
- [Release](#release)
- [Contributing](#contributing)
- [Related Projects](#related-projects)
- [License](#license)

## Overview

Base route:

- `/Names`

Controller actions:

- `GET /Names` — generate names for a given schema

## Requirements

- .NET SDK/runtime with support for `net10.0`

## Configuration

Default configuration is defined in `appsettings.json`:

```json
{
  "dataStoreSettings": {
    "wordListsRootDirectory": "Data/Wordlists",
    "generationSchemasPath": "Data/GenerationSchemas.xml"
  },
  "securitySettings": {
    "apiKey": "[[UNIVERSAL_NAME_GENERATOR_API_KEY]]"
  },
  "nuciLoggerSettings": {
    "logFilePath": "logfile.log",
    "isFileOutputEnabled": true
  }
}
```

Important settings:

- `dataStoreSettings.wordListsRootDirectory`: path to the directory containing wordlist `.lst` files.
- `dataStoreSettings.generationSchemasPath`: path to the `GenerationSchemas.xml` file.
- `securitySettings.apiKey`: API key required for authorization.

## Run the API

```bash
dotnet restore
dotnet run
```

By default, ASP.NET Core prints the listening URLs in the console.

## API Reference

### Authentication

All requests require an API key passed as a query parameter or via the `Authorization` header.

### Get Names

`GET /Names`

Query parameters:

| Parameter | Type   | Required | Default | Description                                |
|-----------|--------|----------|---------|--------------------------------------------|
| `apiKey`  | string | yes      |         | API key for authorization                  |
| `schema`  | string | yes      |         | Schema ID from `GenerationSchemas.xml`     |
| `count`   | int    | no       | `1`     | Number of names to generate (1–100000)     |

Example request:

```text
GET /Names?apiKey=YOUR_API_KEY&schema=arabic-toponyms&count=5
```

Success response shape:

```json
{
  "names": [
    "Bashanar",
    "Quwarith",
    "Ummaraq",
    "Habalis",
    "Sharzeen"
  ],
  "success": true,
  "message": "Operation completed successfully.",
  "code": "SUCCESS",
  "hmac": null
}
```

## Development

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from: `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## Contributing

Contributions are welcome.

Please:

- keep the changes cross-platform
- keep the pull requests focused and consistent with the existing style
- update the documentation when the behaviour changes
- add or update the tests for any new behaviour

## Related Projects

- [Universal Name Generator](https://github.com/hmlendea/universal-name-generator)
- [Universal Name Generator API](https://github.com/hmlendea/universal-name-generator-api)

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
