// ============================================================================
// TOON Token Optimizer - Quick Start Example
// ============================================================================
// Install: dotnet add package Toon.TokenOptimizer
// Run:     dotnet run
// ============================================================================

using Toon.TokenOptimizer;

Console.WriteLine("=== TOON Token Optimizer - Quick Start ===\n");

// -----------------------------------------------------------------------------
// Example 1: Two Formats - Standard and Compact
// -----------------------------------------------------------------------------

var customers = new[]
{
    new { Name = "Alice", Age = 30, City = "New York" },
    new { Name = "Bob", Age = 25, City = "Los Angeles" },
    new { Name = "Charlie", Age = 35, City = "Chicago" }
};

Console.WriteLine("STANDARD TOON (human-readable, official v3.0 spec):");
Console.WriteLine("---------------------------------------------------");
string standardToon = ToonConverter.ToToon(customers);
Console.WriteLine(standardToon);
Console.WriteLine();

Console.WriteLine("COMPACT TOON (maximum token savings):");
Console.WriteLine("-------------------------------------");
string compactToon = ToonConverter.ToCompactToon(customers);
Console.WriteLine(compactToon);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Example 2: Compare all formats
// -----------------------------------------------------------------------------

var comparison = ToonConverter.CompareFormats(customers);

Console.WriteLine("Format Comparison:");
Console.WriteLine("------------------");
Console.WriteLine($"JSON tokens:          {comparison.JsonTokens}");
Console.WriteLine($"Standard TOON tokens: {comparison.StandardToonTokens} ({comparison.StandardToonReductionPercent:F1}% saved)");
Console.WriteLine($"Compact TOON tokens:  {comparison.CompactToonTokens} ({comparison.CompactToonReductionPercent:F1}% saved)");
Console.WriteLine();

// -----------------------------------------------------------------------------
// Example 3: Larger dataset
// -----------------------------------------------------------------------------

var products = Enumerable.Range(1, 100).Select(i => new
{
    Sku = $"SKU-{i:D5}",
    Name = $"Product {i}",
    Price = 9.99m + i,
    InStock = i % 2 == 0
}).ToArray();

var productComparison = ToonConverter.CompareFormats(products);

Console.WriteLine("Large Dataset (100 products):");
Console.WriteLine("-----------------------------");
Console.WriteLine($"JSON tokens:          {productComparison.JsonTokens}");
Console.WriteLine($"Standard TOON tokens: {productComparison.StandardToonTokens} ({productComparison.StandardToonReductionPercent:F1}% saved)");
Console.WriteLine($"Compact TOON tokens:  {productComparison.CompactToonTokens} ({productComparison.CompactToonReductionPercent:F1}% saved)");
Console.WriteLine();

// -----------------------------------------------------------------------------
// Example 4: Choose format explicitly with Serialize()
// -----------------------------------------------------------------------------

var users = new[]
{
    new { Id = 1, Email = "alice@example.com" },
    new { Id = 2, Email = "bob@example.com" }
};

Console.WriteLine("Explicit Format Selection:");
Console.WriteLine("--------------------------");
Console.WriteLine("ToonFormat.Standard:");
Console.WriteLine(ToonConverter.Serialize(users, ToonFormat.Standard));
Console.WriteLine();
Console.WriteLine("ToonFormat.Compact:");
Console.WriteLine(ToonConverter.Serialize(users, ToonFormat.Compact));
Console.WriteLine();

Console.WriteLine("=== Quick Start Complete ===");
Console.WriteLine("\nKey takeaways:");
Console.WriteLine("- Use ToToon() for human-readable output (debugging, logging)");
Console.WriteLine("- Use ToCompactToon() for maximum token savings (LLM API calls)");
Console.WriteLine("- Use CompareFormats() to see savings across all formats");
