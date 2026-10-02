# Privacy and Personal Data

This document describes how the Universal Name Generator API handles personal data. The application is a self-hosted ASP.NET Core REST API for generating random names from configurable generation schemas and reusable word lists. No personal data is collected from users; only an API key is required for request authorisation.

**Information reviewed:** 2026-10-02

## 📑 Table of Contents

- What This Document Covers
- Self-Hosted Deployments
- Data We Handle
- Processing and Use
- Storage, Retention, and Deletion
- External Processing and Integrations
- Data Protection and Security
- Document Changes
- Contact

## 🔎 What This Document Covers

This document describes how Universal Name Generator API at https://github.com/hmlendea/universal-name-generator-api handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

This document covers the project maintainers' practices for the source code and release artefacts. Self-hosted instance operators control their instance's configuration, local storage, logs, backups, access controls, retention, and request handling. The project maintainers do not operate a hosted service and do not receive data from self-hosted instances.

No data is sent from a self-hosted instance to project maintainers or external services. There is no telemetry, update checks, crash reports, email, authentication, reverse-proxy, object-storage, or monitoring integrations built into the application.

## 📥 Data We Handle

### Data Provided to the Application

- API key: provided by the instance operator in configuration (`securitySettings:apiKey`) and sent by clients in the `Authorization` header for request authorisation. The API key is not personal data unless the operator chooses a value that identifies a natural person.

### Data Generated or Collected by the Application

- Access logs: when file logging is enabled (`nuciLoggerSettings:isFileOutputEnabled: true`), the application writes log entries to the configured file path (`nuciLoggerSettings:logFilePath`). Log entries may include the requested schema identifier, requested count, and timestamp. No request bodies, API keys, or client identifiers are logged.

### Data Received from Integrations

- No personal data is received from integrations or third parties. The application reads word list files (`.lst`) and a generation schema file (`GenerationSchemas.xml`) from the local filesystem paths configured in `dataStoreSettings`.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Request authorisation — API key (validated against configured value)
- Name generation — schema identifier and count from query parameters
- Access logging — schema identifier, count, and timestamp (when file logging enabled)

## 🗄️ Storage, Retention, and Deletion

- API key: stored in the instance operator's configuration file (e.g., `appsettings.json`). The project does not store or retain this value. The instance operator controls deletion by updating or removing the configuration.
- Word lists and generation schemas: stored as local files in the directories configured via `dataStoreSettings:wordListsRootDirectory` and `dataStoreSettings:generationSchemasPath`. The instance operator controls retention and deletion of these files.
- Access logs: written to the file path configured in `nuciLoggerSettings:logFilePath` when `nuciLoggerSettings:isFileOutputEnabled` is `true`. The instance operator controls log rotation, retention, and deletion. The application does not implement automatic log rotation or deletion.

For self-hosted deployments, the instance operator controls all local storage, deletion, and backups.

## 🔗 External Processing and Integrations

The application has no built-in external data transfer. All processing occurs locally within the instance. No external services, recipients, or integrations process or receive data from the application.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| None | N/A | N/A | N/A |

## 🛡️ Data Protection and Security

- API key authorisation: requests to `GET /Names` require a matching API key in the `Authorization` header (Bearer or direct). The key is compared using constant-time comparison via the NuciSecurity.HMAC library.
- Configuration: the instance operator must not commit the configured API key to source control. The default `appsettings.json` uses a placeholder `[[UNIVERSAL_NAME_GENERATOR_API_KEY]]`.
- Logging: when enabled, logs are written to a local file. The instance operator is responsible for file permissions, log protection, and preventing unauthorised access to log files.
- Network exposure: the instance operator controls network binding, TLS termination, reverse proxies, and firewall rules.
- Updates: the instance operator is responsible for applying .NET runtime and dependency updates.
- No absolute security is promised; the operator must apply defence-in-depth appropriate to their deployment environment.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/universal-name-generator-api/blob/main/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers via GitHub issues at https://github.com/hmlendea/universal-name-generator-api/issues. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the deployment identifier or configuration details if relevant; do not send passwords, access tokens, or other secrets.