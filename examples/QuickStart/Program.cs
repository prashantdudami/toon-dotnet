// ============================================================================
// TOON Token Optimizer - Quick Start Example
// ============================================================================
// Install: dotnet add package Toon.TokenOptimizer --version 1.0.0
// Run:     dotnet run
// ============================================================================

using Toon.TokenOptimizer;

Console.WriteLine("=== TOON Token Optimizer - Quick Start ===\n");

// -----------------------------------------------------------------------------
// Example 1: Convert a simple array of objects to TOON
// -----------------------------------------------------------------------------

var customers = new[]
{
    new { Name = "Alice", Age = 30, City = "New York" },
    new { Name = "Bob", Age = 25, City = "Los Angeles" },
    new { Name = "Charlie", Age = 35, City = "Chicago" }
};

Console.WriteLine("Original Data (3 customers):");
Console.WriteLine("-----------------------------");

string toonOutput = ToonConverter.ToToon(customers);
Console.WriteLine(toonOutput);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Example 2: See the token savings
// -----------------------------------------------------------------------------

var stats = ToonConverter.GetTokenReduction(customers);

Console.WriteLine("Token Reduction Statistics:");
Console.WriteLine("---------------------------");
Console.WriteLine($"JSON tokens:  {stats.JsonTokens}");
Console.WriteLine($"TOON tokens:  {stats.ToonTokens}");
Console.WriteLine($"Tokens saved: {stats.TokensSaved}");
Console.WriteLine($"Reduction:    {stats.ReductionPercent:F1}%");
Console.WriteLine();

// -----------------------------------------------------------------------------
// Example 3: Larger dataset shows bigger savings
// -----------------------------------------------------------------------------

var products = Enumerable.Range(1, 100).Select(i => new
{
    Sku = $"SKU-{i:D5}",
    Name = $"Product {i}",
    Price = 9.99m + i,
    InStock = i % 2 == 0
}).ToArray();

var productStats = ToonConverter.GetTokenReduction(products);

Console.WriteLine("Large Dataset (100 products):");
Console.WriteLine("-----------------------------");
Console.WriteLine($"JSON tokens:  {productStats.JsonTokens}");
Console.WriteLine($"TOON tokens:  {productStats.ToonTokens}");
Console.WriteLine($"Tokens saved: {productStats.TokensSaved}");
Console.WriteLine($"Reduction:    {productStats.ReductionPercent:F1}%");
Console.WriteLine();

// -----------------------------------------------------------------------------
// Example 4: Custom options
// -----------------------------------------------------------------------------

var options = new ToonOptions
{
    Delimiter = '|',
    ArrayDelimiter = ',',
    Prefix = '~',
    IncludeNulls = false
};

var users = new[]
{
    new { Id = 1, Email = "alice@example.com" },
    new { Id = 2, Email = "bob@example.com" }
};

string customToon = ToonConverter.ToToon(users, options);
Console.WriteLine("Custom Options Example:");
Console.WriteLine("-----------------------");
Console.WriteLine(customToon);
Console.WriteLine();

Console.WriteLine("=== Quick Start Complete ===");
Console.WriteLine("\nNext steps:");
Console.WriteLine("- See OpenAI example for Azure OpenAI integration");
Console.WriteLine("- See RAG example for retrieval-augmented generation");
