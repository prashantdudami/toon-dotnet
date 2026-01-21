# Contributing to TOON Token Optimizer for .NET

Thank you for your interest in contributing! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022, VS Code, or Rider
- Git

### Setup

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR-USERNAME/toon-dotnet.git
   ```
3. Create a branch for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

### Building

```bash
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Making Changes

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Include unit tests for new functionality

### Commit Messages

- Use clear, descriptive commit messages
- Start with a verb (Add, Fix, Update, Remove)
- Reference issue numbers when applicable

Example:
```
Add support for nested object serialization

- Implement recursive conversion for complex types
- Add depth limit to prevent infinite loops
- Add unit tests for nested objects

Fixes #123
```

### Pull Requests

1. Ensure all tests pass
2. Update documentation if needed
3. Add a clear description of your changes
4. Reference any related issues

## Reporting Issues

- Check existing issues before creating a new one
- Include .NET version and OS information
- Provide a minimal reproduction case
- Include expected vs actual behavior

## Questions?

Feel free to open an issue for questions or discussions.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
