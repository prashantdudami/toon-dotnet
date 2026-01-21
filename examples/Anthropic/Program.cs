// ============================================================================
// TOON Token Optimizer - Anthropic Claude Example
// ============================================================================
// This example shows how to use TOON with Anthropic Claude to reduce token
// costs when sending structured data for analysis.
//
// Install: dotnet add package Toon.TokenOptimizer --version 1.0.0
// Run:     dotnet run
//
// For actual Claude API calls, you would use HttpClient or the Anthropic SDK.
// ============================================================================

using System.Text;
using System.Text.Json;
using Toon.TokenOptimizer;

Console.WriteLine("=== TOON + Anthropic Claude Example ===\n");

// -----------------------------------------------------------------------------
// Step 1: Prepare structured data (e.g., user activity logs)
// -----------------------------------------------------------------------------

var userActivities = new[]
{
    new { UserId = "U001", Action = "login", Timestamp = "2025-01-20T09:15:00Z", Duration = 0, Success = true },
    new { UserId = "U001", Action = "view_dashboard", Timestamp = "2025-01-20T09:15:30Z", Duration = 45, Success = true },
    new { UserId = "U001", Action = "export_report", Timestamp = "2025-01-20T09:16:15Z", Duration = 12, Success = true },
    new { UserId = "U002", Action = "login", Timestamp = "2025-01-20T09:20:00Z", Duration = 0, Success = false },
    new { UserId = "U002", Action = "login", Timestamp = "2025-01-20T09:20:30Z", Duration = 0, Success = true },
    new { UserId = "U002", Action = "update_profile", Timestamp = "2025-01-20T09:21:00Z", Duration = 8, Success = true },
    new { UserId = "U003", Action = "login", Timestamp = "2025-01-20T09:25:00Z", Duration = 0, Success = true },
    new { UserId = "U003", Action = "view_settings", Timestamp = "2025-01-20T09:25:15Z", Duration = 20, Success = true },
    new { UserId = "U003", Action = "change_password", Timestamp = "2025-01-20T09:25:35Z", Duration = 5, Success = true },
    new { UserId = "U003", Action = "logout", Timestamp = "2025-01-20T09:26:00Z", Duration = 0, Success = true },
};

// -----------------------------------------------------------------------------
// Step 2: Compare all formats
// -----------------------------------------------------------------------------

var comparison = ToonConverter.CompareFormats(userActivities);

Console.WriteLine("Token Savings Analysis:");
Console.WriteLine("-----------------------");
Console.WriteLine($"Activities:       {userActivities.Length}");
Console.WriteLine($"JSON:             {comparison.JsonTokens} tokens");
Console.WriteLine($"Standard TOON:    {comparison.StandardToonTokens} tokens ({comparison.StandardToonReductionPercent:F1}% saved)");
Console.WriteLine($"Compact TOON:     {comparison.CompactToonTokens} tokens ({comparison.CompactToonReductionPercent:F1}% saved)");
Console.WriteLine();

// Use Compact for API calls
string compactData = ToonConverter.ToCompactToon(userActivities);
Console.WriteLine("Compact TOON Output (for Claude API):");
Console.WriteLine("--------------------------------------");
Console.WriteLine(compactData);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Step 3: Build Claude-optimized prompt
// -----------------------------------------------------------------------------

string claudePrompt = $"""
    Analyze these user activity logs and identify any patterns or anomalies.
    
    The data is in TOON Compact format (Token Optimized Object Notation):
    - Schema shown in brackets [field1|field2|...]
    - Data rows follow with pipe-separated values
    
    Activity Logs:
    {compactData}
    
    Please analyze:
    1. User behavior patterns
    2. Any failed actions and their context
    3. Session duration estimates
    4. Security concerns (if any)
    5. Recommendations for improving user experience
    """;

Console.WriteLine("Claude Prompt:");
Console.WriteLine("--------------");
Console.WriteLine(claudePrompt);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Step 4: Example API call structure (using HttpClient)
// -----------------------------------------------------------------------------

Console.WriteLine("Example API Call (HttpClient):");
Console.WriteLine("------------------------------");

var requestBody = new
{
    model = "claude-3-5-sonnet-20241022",
    max_tokens = 1024,
    messages = new[]
    {
        new
        {
            role = "user",
            content = claudePrompt
        }
    }
};

string jsonRequest = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine(jsonRequest);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Show the actual API call code (commented for reference)
// -----------------------------------------------------------------------------

Console.WriteLine("To call Claude API, use this code:");
Console.WriteLine("-----------------------------------");
Console.WriteLine("""
using var client = new HttpClient();
client.DefaultRequestHeaders.Add("x-api-key", Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));
client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

var response = await client.PostAsync(
    "https://api.anthropic.com/v1/messages",
    new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);
""");

Console.WriteLine();
Console.WriteLine("=== Anthropic Example Complete ===");
Console.WriteLine("\nClaude understands TOON format natively - no special parsing needed!");
