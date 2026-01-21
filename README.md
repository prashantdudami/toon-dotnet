# TOON Token Optimizer for .NET

[![NuGet](https://img.shields.io/nuget/v/Toon.TokenOptimizer.svg)](https://www.nuget.org/packages/Toon.TokenOptimizer)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Toon.TokenOptimizer.svg)](https://www.nuget.org/packages/Toon.TokenOptimizer)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Token Optimized Object Notation** - Reduce LLM token usage by 40-60% when sending structured data to AI systems.

## The Problem

When sending structured data to Large Language Models (LLMs), traditional JSON format wastes tokens on:
- Repeated property names
- Quotation marks around keys
- Verbose syntax

**Every token costs money** (OpenAI, Anthropic, Azure OpenAI) and reduces context window capacity.

## The Solution

TOON format compresses structured data while maintaining parseability:

```
# Standard JSON (43 tokens)
{"users": [{"name": "Alice", "age": 30}, {"name": "Bob", "age": 25}]}

# TOON format (18 tokens) - 58% reduction
~users[name|age]:Alice|30,Bob|25
```

## Installation

```bash
dotnet add package Toon.TokenOptimizer
```

Or via NuGet Package Manager:
```
Install-Package Toon.TokenOptimizer
```

## Quick Start

```csharp
using Toon.TokenOptimizer;

// Convert object to TOON
var users = new[] 
{
    new { Name = "Alice", Age = 30, City = "NYC" },
    new { Name = "Bob", Age = 25, City = "LA" }
};

string toon = ToonConverter.ToToon(users);
// Output: ~[Name|Age|City]:Alice|30|NYC,Bob|25|LA

// Convert back to object
var result = ToonConverter.FromToon<User[]>(toon);
```

## Usage with LLMs

### Azure OpenAI

```csharp
using Azure.AI.OpenAI;
using Toon.TokenOptimizer;

var data = GetComplexData();
string optimizedData = ToonConverter.ToToon(data);

var prompt = $"""
    Analyze this data (TOON format - pipe-delimited, tilde-prefixed):
    {optimizedData}
    
    Provide insights on trends and anomalies.
    """;

// Send to Azure OpenAI with 40-60% fewer tokens
```

### Anthropic Claude

```csharp
using Toon.TokenOptimizer;

var documents = GetRAGDocuments();
string toonDocs = ToonConverter.ToToon(documents);

// Include in Claude context with significant token savings
```

## API Reference

### ToonConverter.ToToon()

```csharp
// Basic conversion
string toon = ToonConverter.ToToon(obj);

// With options
string toon = ToonConverter.ToToon(obj, new ToonOptions
{
    Delimiter = '|',
    ArrayDelimiter = ',',
    IncludeNulls = false,
    MaxDepth = 10
});
```

### ToonConverter.FromToon<T>()

```csharp
// Deserialize TOON to strongly-typed object
var result = ToonConverter.FromToon<MyClass>(toonString);

// With options
var result = ToonConverter.FromToon<MyClass>(toonString, new ToonOptions());
```

### ToonConverter.GetTokenReduction()

```csharp
// Calculate token savings
var stats = ToonConverter.GetTokenReduction(obj);
Console.WriteLine($"JSON tokens: {stats.JsonTokens}");
Console.WriteLine($"TOON tokens: {stats.ToonTokens}");
Console.WriteLine($"Reduction: {stats.ReductionPercent:F1}%");
```

## Supported Types

- ✅ Primitive types (string, int, double, bool, etc.)
- ✅ Nullable types
- ✅ Arrays and Lists
- ✅ Dictionaries
- ✅ Nested objects
- ✅ Anonymous types
- ✅ Records
- ✅ DateTime and DateTimeOffset

## Benchmarks

| Data Type | JSON Tokens | TOON Tokens | Reduction |
|-----------|-------------|-------------|-----------|
| User list (100 items) | 1,247 | 498 | 60.1% |
| Product catalog | 3,891 | 1,712 | 56.0% |
| RAG documents | 8,234 | 3,456 | 58.0% |
| API response | 2,156 | 892 | 58.6% |

## Related Libraries

- **Python**: [toon-token-optimizer](https://pypi.org/project/toon-token-optimizer/) on PyPI
- **Java**: Coming soon on Maven Central

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

## License

MIT License - see [LICENSE](LICENSE) for details.

## Author

**Prashant Dudami**  
- PyPI: [toon-token-optimizer](https://pypi.org/project/toon-token-optimizer/)
- GitHub: [toon-dotnet](https://github.com/prashantdudami/toon-dotnet)
