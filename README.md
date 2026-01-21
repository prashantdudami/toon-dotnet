# TOON Token Optimizer for .NET

[![NuGet Version](https://img.shields.io/nuget/v/Toon.TokenOptimizer.svg)](https://www.nuget.org/packages/Toon.TokenOptimizer)
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

var users = new[] 
{
    new { Name = "Alice", Age = 30, City = "NYC" },
    new { Name = "Bob", Age = 25, City = "LA" }
};

// Standard TOON v3.0 format (human-readable, indentation-based)
string standard = ToonConverter.ToToon(users);
// Output:
// [2]{Age,City,Name}:
//   30,NYC,Alice
//   25,LA,Bob

// Compact TOON format (maximum token savings)
string compact = ToonConverter.ToCompactToon(users);
// Output: ~[Age|City|Name]:30|NYC|Alice,25|LA|Bob

// Convert back to object (auto-detects format)
var result = ToonConverter.FromToon<User[]>(compact);

// Compare formats to see token savings
var stats = ToonConverter.CompareFormats(users);
Console.WriteLine(stats);
// JSON: 89 tokens | Standard TOON: 42 (52.8% saved) | Compact TOON: 28 (68.5% saved)
```

## Usage with LLMs

### Azure OpenAI

```csharp
using Azure.AI.OpenAI;
using Toon.TokenOptimizer;

var data = GetComplexData();
// Use Compact format for maximum token savings in API calls
string optimizedData = ToonConverter.ToCompactToon(data);

var prompt = $"""
    Analyze this data (TOON Compact format - pipe-delimited):
    {optimizedData}
    
    Provide insights on trends and anomalies.
    """;

// Send to Azure OpenAI with 50-65% fewer tokens
```

### Anthropic Claude

```csharp
using Toon.TokenOptimizer;

var documents = GetRAGDocuments();
// Compact format for LLM calls, Standard format for debugging
string toonDocs = ToonConverter.ToCompactToon(documents);

// Include in Claude context with significant token savings
```

## Two Formats, One Library

| Format | Method | Use Case | Token Savings |
|--------|--------|----------|---------------|
| **Standard** | `ToToon()` | Human review, debugging | 40-50% |
| **Compact** | `ToCompactToon()` | Maximum LLM efficiency | 50-65% |

### Standard TOON (Official v3.0 Spec)
```
[3]{id,name,role}:
  1,Alice,admin
  2,Bob,user
  3,Charlie,viewer
```
- Indentation-based, human-readable
- Count in brackets, schema in braces
- Best for: debugging, human review, logging

### Compact TOON (Maximum Savings)
```
~[id|name|role]:1|Alice|admin,2|Bob|user,3|Charlie|viewer
```
- Single-line, minimal structure
- Maximum token reduction
- Best for: LLM API calls, cost optimization

## API Reference

### ToonConverter.ToToon() - Standard Format

```csharp
// Standard TOON v3.0 format (human-readable)
string toon = ToonConverter.ToToon(obj);
string toon = ToonConverter.ToToon(obj, options);
```

### ToonConverter.ToCompactToon() - Compact Format

```csharp
// Compact format (maximum token savings)
string compact = ToonConverter.ToCompactToon(obj);
string compact = ToonConverter.ToCompactToon(obj, options);
```

### ToonConverter.Serialize() - Explicit Format Selection

```csharp
// Choose format explicitly
string output = ToonConverter.Serialize(obj, ToonFormat.Standard);
string output = ToonConverter.Serialize(obj, ToonFormat.Compact);
```

### ToonConverter.FromToon<T>() - Deserialization

```csharp
// Auto-detects format and deserializes
var result = ToonConverter.FromToon<MyClass>(toonString);
var result = ToonConverter.FromToon<MyClass>(toonString, options);
```

### ToonConverter.CompareFormats() - Format Comparison

```csharp
// Compare all three formats (JSON, Standard, Compact)
var stats = ToonConverter.CompareFormats(obj);
Console.WriteLine($"JSON: {stats.JsonTokens} tokens");
Console.WriteLine($"Standard TOON: {stats.StandardToonTokens} ({stats.StandardToonReductionPercent:F1}% saved)");
Console.WriteLine($"Compact TOON: {stats.CompactToonTokens} ({stats.CompactToonReductionPercent:F1}% saved)");
```

### ToonConverter.GetTokenReduction() - Token Analytics

```csharp
// Get token reduction stats (defaults to Compact format)
var stats = ToonConverter.GetTokenReduction(obj);
var stats = ToonConverter.GetTokenReduction(obj, ToonFormat.Standard);
var stats = ToonConverter.GetTokenReduction(obj, ToonFormat.Compact);

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

## Advanced Features

### Safe Parsing (TryFromToon)

```csharp
if (ToonConverter.TryFromToon<User[]>(toonString, out var users))
{
    // Successfully parsed
    Console.WriteLine($"Loaded {users.Length} users");
}
else
{
    // Handle invalid input
    Console.WriteLine("Invalid TOON format");
}
```

### Async/Stream Support

```csharp
// Write to stream
await using var fileStream = File.Create("data.toon");
await ToonConverter.ToToonAsync(fileStream, users);

// Read from stream
await using var readStream = File.OpenRead("data.toon");
var loadedUsers = await ToonConverter.FromToonAsync<User[]>(readStream);
```

### Validation

```csharp
// Check if string is valid TOON format
if (ToonConverter.IsValidToon(input))
{
    var data = ToonConverter.FromToon<MyType>(input);
}
```

### Preset Options

```csharp
// Optimized for minimum tokens
var minimal = ToonOptions.MinimalTokens;

// Strict validation mode
var strict = ToonOptions.StrictMode;
```

## Why Toon.TokenOptimizer?

| Feature | Toon.TokenOptimizer | Others |
|---------|---------------------|--------|
| **Token Reduction Stats** | ✅ Built-in analytics | ❌ |
| **Multi-Target** | .NET 6/7/8/10 + Standard 2.0 | Often .NET 9+ only |
| **LLM-Focused** | ✅ Designed for AI workloads | Generic serialization |
| **Safe Parsing** | ✅ TryFromToon | Limited |
| **Async Support** | ✅ Full async/stream | Varies |
| **75+ Unit Tests** | ✅ Comprehensive coverage | Varies |

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
