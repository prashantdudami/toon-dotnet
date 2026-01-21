// ============================================================================
// TOON Token Optimizer - Azure OpenAI Example
// ============================================================================
// This example shows how to use TOON with Azure OpenAI to reduce token costs
// when sending structured data for analysis.
//
// Install packages:
//   dotnet add package Toon.TokenOptimizer --version 1.0.0
//   dotnet add package Azure.AI.OpenAI
//
// Set environment variables:
//   AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
//   AZURE_OPENAI_API_KEY=your-api-key
//   AZURE_OPENAI_DEPLOYMENT=your-deployment-name
// ============================================================================

using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Toon.TokenOptimizer;

Console.WriteLine("=== TOON + Azure OpenAI Example ===\n");

// -----------------------------------------------------------------------------
// Step 1: Prepare your structured data
// -----------------------------------------------------------------------------

var salesData = new[]
{
    new { Region = "North", Product = "Widget A", Q1 = 15000, Q2 = 18000, Q3 = 22000, Q4 = 25000 },
    new { Region = "South", Product = "Widget A", Q1 = 12000, Q2 = 14000, Q3 = 16000, Q4 = 19000 },
    new { Region = "East", Product = "Widget B", Q1 = 8000, Q2 = 9500, Q3 = 11000, Q4 = 13000 },
    new { Region = "West", Product = "Widget B", Q1 = 10000, Q2 = 11000, Q3 = 12500, Q4 = 14500 },
    new { Region = "North", Product = "Widget C", Q1 = 5000, Q2 = 6000, Q3 = 7500, Q4 = 9000 },
    new { Region = "South", Product = "Widget C", Q1 = 4500, Q2 = 5500, Q3 = 6500, Q4 = 8000 },
};

// -----------------------------------------------------------------------------
// Step 2: Convert to TOON format and see the savings
// -----------------------------------------------------------------------------

string toonData = ToonConverter.ToToon(salesData);
var stats = ToonConverter.GetTokenReduction(salesData);

Console.WriteLine("Token Reduction:");
Console.WriteLine($"  JSON: {stats.JsonTokens} tokens");
Console.WriteLine($"  TOON: {stats.ToonTokens} tokens");
Console.WriteLine($"  Saved: {stats.TokensSaved} tokens ({stats.ReductionPercent:F1}%)\n");

Console.WriteLine("TOON Data:");
Console.WriteLine(toonData);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Step 3: Build the prompt with TOON data
// -----------------------------------------------------------------------------

string prompt = $"""
    Analyze this sales data and provide insights.
    
    The data is in TOON format (Token Optimized Object Notation):
    - First line shows the schema (column names separated by |)
    - Following lines are data rows (values separated by |)
    
    Data:
    {toonData}
    
    Please provide:
    1. Overall sales trends
    2. Top performing region
    3. Product performance comparison
    4. Recommendations for next quarter
    """;

Console.WriteLine("Prompt (with TOON data):");
Console.WriteLine("------------------------");
Console.WriteLine(prompt);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Step 4: Call Azure OpenAI (uncomment when you have credentials)
// -----------------------------------------------------------------------------

/*
// Get configuration from environment variables
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") 
    ?? throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT");
string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") 
    ?? throw new InvalidOperationException("Set AZURE_OPENAI_API_KEY");
string deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") 
    ?? throw new InvalidOperationException("Set AZURE_OPENAI_DEPLOYMENT");

// Create client
var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey));

var chatClient = client.GetChatClient(deployment);

// Send request
var response = await chatClient.CompleteChatAsync(
    new ChatMessage[]
    {
        new SystemChatMessage("You are a business analyst. Analyze data and provide actionable insights."),
        new UserChatMessage(prompt)
    });

Console.WriteLine("Azure OpenAI Response:");
Console.WriteLine("-----------------------");
Console.WriteLine(response.Value.Content[0].Text);
*/

Console.WriteLine("=== Example Complete ===");
Console.WriteLine("\nTo run with Azure OpenAI:");
Console.WriteLine("1. Set environment variables (AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_DEPLOYMENT)");
Console.WriteLine("2. Uncomment the Azure OpenAI section in the code");
Console.WriteLine("3. Run: dotnet run");
