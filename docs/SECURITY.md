# Universal Name Generator API - Security Documentation

## Overview

This document provides comprehensive security documentation for the Universal Name Generator API, including authentication mechanisms, threat model, security controls, and deployment recommendations.

---

## Authentication

### API Key Authentication

**Mechanism**: Bearer token authentication via `Authorization` header

**Implementation**:
- `NuciApiAuthorisation.ApiKey()` from `NuciAPI.Controllers`
- Constant-time comparison via `NuciSecurity.HMAC`
- Supports `Bearer` prefix or direct key

**Request Format**:
```http
Authorization: Bearer YOUR_API_KEY
```

**Validation Rules**:
- Key must match configured `securitySettings:apiKey`
- Case-sensitive comparison
- Whitespace trimmed from key
- `Bearer` prefix case-insensitive
- Multiple whitespace between `Bearer` and key allowed

**Security Properties**:
- Constant-time comparison prevents timing attacks
- No key exposure in error messages
- No key logging
- HMAC token support for additional integrity

### HMAC Token Support

**Purpose**: Additional request integrity verification

**Header**: `X-HMAC: {token}`

**Implementation**:
- `NuciSecurity.HMAC` library
- `HmacOrder` attributes on request properties
- Optional but recommended for production

**Configuration**:
```csharp
[HmacOrder(1)]
public string Schema { get; set; }

[HmacOrder(2)]
[Range(1, 100000)]
public int Count { get; set; } = 1;
```

---

## Security Middleware

### Scanner Protection

**Middleware**: `NuciAPI.Middleware.Security`

**Purpose**: Detect and block malicious scanner requests

**Implementation**:
```csharp
applicationBuilder.UseNuciApiScannerProtection();
```

**Detection Rules**:
- Known scanner user agents
- Suspicious request patterns
- Malformed HTTP requests
- SQL injection attempts
- XSS attack patterns

**Response**:
- Blocked requests receive 403 Forbidden
- Blocked requests logged
- No information leakage about detection rules

### Exception Handling

**Middleware**: `NuciAPI.Middleware.ExceptionHandling`

**Purpose**: Translate exceptions to standardized HTTP errors

**Security Properties**:
- No stack trace exposure in production
- No internal path leakage
- Standardized error format
- Sensitive data redaction

**Error Format**:
```json
{
    "success": false,
    "error": {
        "code": "ErrorCode",
        "message": "User-friendly message"
    }
}
```

---

## Threat Model

### Threats

| Threat | Likelihood | Impact | Mitigation |
|--------|-----------|--------|------------|
| Unauthorized access | High | High | API key authentication |
| API key leakage | Medium | High | Secret management, rotation |
| Injection attacks | Medium | High | Input validation, parameterized queries |
| Denial of service | Medium | Medium | Rate limiting, resource limits |
| Data exfiltration | Low | Medium | No sensitive data storage |
| Man-in-the-middle | Low | High | HTTPS enforcement |
| Configuration exposure | Low | High | Secret management, environment variables |

### Attack Surface

**HTTP Endpoints**:
- `GET /Names`: Primary endpoint, authenticated
- Static files: Development only
- Error pages: Development only

**Data Sources**:
- XML schema files: Read-only, local
- Word list files: Read-only, local
- Configuration files: Read-only, local

**External Dependencies**:
- NuGet packages: Trusted, versioned
- .NET runtime: Microsoft-maintained
- No external API calls

---

## Security Controls

### Input Validation

**Request Validation**:
- `Count`: Range [1, 100000] via `[Range]` attribute
- `Schema`: String format, no length limit
- Query parameters: Case-insensitive binding
- Unknown parameters: Ignored

**Data Validation**:
- Schema format: Validated during parsing
- Word list format: Validated during parsing
- Generator parameters: Validated during command parsing

**Validation Errors**:
- 400 Bad Request with validation details
- No internal information leakage
- Standardized error format

### Output Sanitization

**Generated Names**:
- No HTML encoding (programmatic consumption)
- No SQL escaping (no database)
- Length constrained by schema parameters
- Content determined by word lists

**Error Messages**:
- No stack traces in production
- No internal paths exposed
- No sensitive data included
- Standardized format

### Access Control

**Authentication**:
- API key required for all requests
- No anonymous access
- No role-based access control (single key)

**Authorization**:
- All authenticated requests have same access
- No resource-level authorization
- No data isolation between clients

### Data Protection

**Data at Rest**:
- No sensitive data stored
- Word lists and schemas: Local files
- Configuration: Local files or environment variables
- Logs: Local files (no sensitive data)

**Data in Transit**:
- HTTPS recommended for production
- No TLS enforcement in code (deployment responsibility)
- No data encryption at application level

**Data in Memory**:
- API key: In memory (configuration)
- Generated names: In memory (transient)
- No persistent sensitive data

### Logging Security

**Log Content**:
- Schema identifier: Logged
- Count: Logged
- Timestamp: Logged
- API key: NOT logged
- Request body: NOT logged
- Client IP: NOT logged (by default)

**Log Protection**:
- File permissions: Operator responsibility
- Log rotation: External tool
- Log access: Operator responsibility

---

## Deployment Security

### Network Security

**HTTPS**:
- `UseHttpsRedirection()` middleware
- TLS termination: Reverse proxy or load balancer
- Certificate management: Deployment responsibility
- HSTS: Recommended but not enforced

**Firewall**:
- Expose only necessary ports (5000, 5001)
- Restrict access to trusted networks
- Monitor for unauthorized access attempts

**Rate Limiting**:
- Not implemented in application
- Recommended: Reverse proxy or API gateway
- Suggested limits: 100 requests/minute, 1000/hour

### Container Security

**Docker**:
- Run as non-root user
- Read-only filesystem where possible
- No privileged containers
- Minimal base image
- No secrets in image

**Kubernetes**:
- Resource limits (CPU, memory)
- Security contexts
- Network policies
- Secret management (K8s Secrets)
- Pod security standards

### Configuration Security

**Secret Management**:
- Never commit API keys to source control
- Use environment variables or secret stores
- Rotate keys periodically
- Different keys per environment
- Monitor for key exposure

**Configuration Validation**:
- Validate required settings at startup
- Reject default placeholder values in production
- Log configuration errors (without sensitive values)

---

## Security Testing

### Current Testing

**Authentication Tests**:
- 33 test cases for valid/invalid API keys
- Header format variations
- Missing header handling
- Query parameter injection rejection

**Scanner Protection Tests**:
- Malicious request detection
- Request blocking
- Logging verification

**Validation Tests**:
- 56 test cases for input validation
- Boundary conditions
- Malformed input handling

### Recommended Security Testing

**Penetration Testing**:
- API key brute force
- Injection attacks
- Authentication bypass
- Information disclosure

**Fuzzing**:
- Schema format fuzzing
- Word list format fuzzing
- Query parameter fuzzing

**Dependency Scanning**:
- NuGet package vulnerability scanning
- .NET runtime vulnerability scanning
- Regular dependency updates

**Security Auditing**:
- Code review for security issues
- Configuration review
- Log review for sensitive data

---

## Security Best Practices

### API Key Management

1. **Generate strong keys**: Use `openssl rand -base64 32` or similar
2. **Store securely**: Environment variables, secret stores
3. **Rotate regularly**: Every 90 days or on suspicion of compromise
4. **Monitor usage**: Track API key usage patterns
5. **Revoke compromised keys**: Immediate rotation on compromise

### Deployment Checklist

- [ ] API key set via environment variable
- [ ] HTTPS enabled with valid certificate
- [ ] Log file permissions set (600)
- [ ] Data directory permissions set (644)
- [ ] Firewall rules configured
- [ ] Rate limiting implemented
- [ ] Monitoring and alerting configured
- [ ] Backup strategy in place
- [ ] Incident response plan documented

### Incident Response

**Security Incident Types**:
- API key compromise
- Unauthorized access
- Data exfiltration
- Denial of service
- Vulnerability disclosure

**Response Steps**:
1. Contain: Isolate affected systems
2. Assess: Determine scope and impact
3. Eradicate: Remove threat
4. Recover: Restore normal operations
5. Learn: Document lessons learned

---

## Compliance

### Data Protection

**GDPR**:
- No personal data collected
- No user tracking
- No cookies
- No third-party data sharing

**CCPA**:
- No personal data collected
- No sale of personal information
- No targeted advertising

### Industry Standards

**OWASP Top 10**:
- A01:2021 Broken Access Control: ✅ API key authentication
- A02:2021 Cryptographic Failures: ⚠️ HTTPS recommended
- A03:2021 Injection: ✅ Input validation
- A04:2021 Insecure Design: ✅ Minimal attack surface
- A05:2021 Security Misconfiguration: ⚠️ Deployment responsibility
- A06:2021 Vulnerable Components: ✅ Versioned dependencies
- A07:2021 Identification and Authentication Failures: ✅ API key authentication
- A08:2021 Software and Data Integrity Failures: ✅ No external data
- A09:2021 Logging and Monitoring Failures: ✅ Request logging
- A10:2021 Server-Side Request Forgery: ✅ No SSRF vectors

---

## Security Monitoring

### Metrics to Monitor

**Authentication**:
- Failed authentication attempts
- API key usage patterns
- Unusual access patterns

**Performance**:
- Request latency
- Error rates
- Resource utilization

**Security**:
- Blocked scanner requests
- Validation failures
- Exception rates

### Alerting

**Critical Alerts**:
- Multiple failed authentication attempts
- Unusual API key usage
- High error rates
- Resource exhaustion

**Warning Alerts**:
- Elevated request volume
- Increased validation failures
- Log file size approaching limit

---

## Security Summary

The Universal Name Generator API implements a security-first design with:

- **API key authentication** with constant-time comparison
- **Scanner protection** middleware
- **Comprehensive input validation**
- **Standardized error handling** without information leakage
- **No sensitive data storage**
- **Minimal attack surface**
- **No external data dependencies**

Security is primarily a deployment responsibility, with the application providing the necessary controls and the operator responsible for:
- API key management
- HTTPS configuration
- Network security
- Monitoring and alerting
- Incident response

The application follows security best practices and provides a solid foundation for secure deployment in production environments.