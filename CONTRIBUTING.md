# Contributing to Universal Name Generator API

This document covers the guidelines and processes for contributing to this project, including how to report issues, suggest enhancements, submit code changes, and follow project standards.

## 📑 Table of Contents

- [How to Contribute](#-how-to-contribute)
  - [Reporting Issues](#reporting-issues)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Code Contributions](#code-contributions)
- [Code Style](#-code-style)
- [Testing](#-testing)
- [Documentation](#-documentation)
- [Code of Conduct](#-code-of-conduct)

## 🤝 How to Contribute

### Reporting Issues

- Search existing issues first.
- Use the issue templates if available.
- Provide clear reproduction steps.
- Include environment details.

### Suggesting Enhancements

- Check the roadmap and existing discussions.
- Explain the use case and expected behavior.
- Consider implementation complexity.

### Code Contributions

#### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

#### Development Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/universal-name-generator-api.git
cd universal-name-generator-api

# Install dependencies
dotnet restore
```

#### Making Changes

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes.
4. Run tests: `dotnet test UniversalNameGenerator.API.slnx`
5. Commit with clear and descriptive messages.
6. Push to your fork.
7. Open a Pull Request.

#### Pull Request Guidelines

- Target the `master` branch.
- Keep PRs focused and atomic.
- Update documentation if applicable.
- Add tests for any new functionality.
- Ensure the CI checks pass.

## 🎨 Code Style

Follow the project's coding standards:
- C# coding conventions as defined in the project
- Run `dotnet format` before committing

## 🧪 Testing

```bash
# Run all tests
dotnet test UniversalNameGenerator.API.slnx

# Run specific test suite
dotnet test UniversalNameGenerator.API.UnitTests/UniversalNameGenerator.API.UnitTests.csproj
dotnet test UniversalNameGenerator.API.IntegrationTests/UniversalNameGenerator.API.IntegrationTests.csproj
```

## 📚 Documentation

- Update relevant docs for changes.
- Follow the documentation style guide.
- Preview changes locally if possible.

## 📋 Code of Conduct

This project follows the [Security Policy](./SECURITY.md).