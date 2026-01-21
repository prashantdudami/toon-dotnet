# TOON Token Optimizer - Examples

This folder contains example applications demonstrating how to use the TOON Token Optimizer library in various scenarios.

## Prerequisites

- .NET 8.0 SDK or later
- Install the package: `dotnet add package Toon.TokenOptimizer --version 1.0.0`

## Examples

### 1. QuickStart

Basic usage of TOON Token Optimizer - converting data and measuring token savings.

```bash
cd QuickStart
dotnet run
```

**Demonstrates:**
- Converting arrays of objects to TOON format
- Getting token reduction statistics
- Using custom options

---

### 2. AzureOpenAI

Integration with Azure OpenAI for enterprise AI applications.

```bash
cd AzureOpenAI
dotnet run
```

**Demonstrates:**
- Preparing structured data for Azure OpenAI
- Building prompts with TOON-formatted data
- Token savings for analytics use cases

**Requirements:**
- Azure OpenAI resource
- Set environment variables:
  - `AZURE_OPENAI_ENDPOINT`
  - `AZURE_OPENAI_API_KEY`
  - `AZURE_OPENAI_DEPLOYMENT`

---

### 3. Anthropic

Integration with Anthropic Claude API.

```bash
cd Anthropic
dotnet run
```

**Demonstrates:**
- Formatting data for Claude
- Building Claude-optimized prompts
- Example API call structure

**Requirements:**
- Anthropic API key
- Set environment variable: `ANTHROPIC_API_KEY`

---

### 4. RAG

Retrieval-Augmented Generation (RAG) applications - inject more context with fewer tokens.

```bash
cd RAG
dotnet run
```

**Demonstrates:**
- Knowledge base article formatting
- Token savings at scale (5 to 100 documents)
- Building RAG prompts with TOON data

---

## Running All Examples

```bash
# From the examples directory
cd QuickStart && dotnet run && cd ..
cd AzureOpenAI && dotnet run && cd ..
cd Anthropic && dotnet run && cd ..
cd RAG && dotnet run && cd ..
```

## Token Savings Summary

| Scenario | Typical Savings |
|----------|-----------------|
| Simple objects (3-5 records) | 30-40% |
| Medium datasets (10-50 records) | 40-50% |
| Large datasets (100+ records) | 50-60% |
| RAG with many documents | 50-60% |

## Need Help?

- **Documentation:** See the main [README.md](../README.md)
- **Issues:** Open an issue on GitHub
- **PyPI Version:** [toon-token-optimizer](https://pypi.org/project/toon-token-optimizer/) (Python)
