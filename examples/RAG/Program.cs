// ============================================================================
// TOON Token Optimizer - RAG (Retrieval-Augmented Generation) Example
// ============================================================================
// This example demonstrates how TOON can significantly reduce token usage
// when injecting retrieved documents into LLM context for RAG applications.
//
// Install: dotnet add package Toon.TokenOptimizer --version 1.0.0
// Run:     dotnet run
// ============================================================================

using Toon.TokenOptimizer;

Console.WriteLine("=== TOON for RAG Applications ===\n");

// -----------------------------------------------------------------------------
// Scenario: Customer Support RAG System
// -----------------------------------------------------------------------------
// You have a knowledge base of support articles. When a user asks a question,
// you retrieve relevant articles and inject them into the LLM context.
// With TOON, you can fit more articles in the same context window.

var knowledgeBaseArticles = new[]
{
    new {
        Id = "KB001",
        Title = "How to Reset Your Password",
        Category = "Account",
        LastUpdated = "2025-01-15",
        Content = "To reset your password, click the 'Forgot Password' link on the login page. Enter your email address and follow the instructions sent to your inbox.",
        Views = 15420
    },
    new {
        Id = "KB002", 
        Title = "Billing Cycle Explained",
        Category = "Billing",
        LastUpdated = "2025-01-10",
        Content = "Your billing cycle starts on the day you subscribed. You will be charged on the same day each month. To view your billing date, go to Settings > Billing.",
        Views = 8930
    },
    new {
        Id = "KB003",
        Title = "Two-Factor Authentication Setup",
        Category = "Security",
        LastUpdated = "2025-01-12",
        Content = "Enable 2FA by going to Settings > Security > Two-Factor Authentication. You can use an authenticator app or SMS verification for added security.",
        Views = 12150
    },
    new {
        Id = "KB004",
        Title = "Cancellation and Refund Policy",
        Category = "Billing",
        LastUpdated = "2025-01-08",
        Content = "You can cancel your subscription at any time from Settings > Subscription. Refunds are available within 14 days of purchase for annual plans.",
        Views = 6780
    },
    new {
        Id = "KB005",
        Title = "Data Export and Download",
        Category = "Account",
        LastUpdated = "2025-01-14",
        Content = "Export your data by going to Settings > Privacy > Export Data. You will receive a download link via email within 24 hours containing all your account data.",
        Views = 4520
    }
};

// -----------------------------------------------------------------------------
// Compare JSON vs TOON for RAG context injection
// -----------------------------------------------------------------------------

string toonArticles = ToonConverter.ToToon(knowledgeBaseArticles);
var stats = ToonConverter.GetTokenReduction(knowledgeBaseArticles);

Console.WriteLine("RAG Context Token Analysis:");
Console.WriteLine("===========================");
Console.WriteLine($"Articles retrieved: {knowledgeBaseArticles.Length}");
Console.WriteLine($"JSON tokens:        {stats.JsonTokens}");
Console.WriteLine($"TOON tokens:        {stats.ToonTokens}");
Console.WriteLine($"Tokens saved:       {stats.TokensSaved}");
Console.WriteLine($"Reduction:          {stats.ReductionPercent:F1}%");
Console.WriteLine();

// -----------------------------------------------------------------------------
// Show the TOON output
// -----------------------------------------------------------------------------

Console.WriteLine("TOON-formatted articles:");
Console.WriteLine("------------------------");
Console.WriteLine(toonArticles);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Example RAG prompt with TOON
// -----------------------------------------------------------------------------

string userQuestion = "How do I cancel my subscription and get a refund?";

string ragPrompt = $"""
    You are a helpful customer support assistant. Answer the user's question 
    based on the knowledge base articles provided below.
    
    Knowledge Base (TOON format - pipe-delimited, schema on first line):
    {toonArticles}
    
    User Question: {userQuestion}
    
    Instructions:
    - Only use information from the knowledge base articles
    - Cite the article ID (e.g., KB004) when referencing information
    - If the answer isn't in the knowledge base, say so
    """;

Console.WriteLine("Complete RAG Prompt:");
Console.WriteLine("--------------------");
Console.WriteLine(ragPrompt);
Console.WriteLine();

// -----------------------------------------------------------------------------
// Scaling analysis
// -----------------------------------------------------------------------------

Console.WriteLine("Scaling Analysis:");
Console.WriteLine("-----------------");

int[] articleCounts = { 5, 10, 25, 50, 100 };

foreach (int count in articleCounts)
{
    var scaledArticles = Enumerable.Range(1, count).Select(i => new
    {
        Id = $"KB{i:D3}",
        Title = $"Knowledge Base Article {i}",
        Category = i % 3 == 0 ? "Billing" : i % 2 == 0 ? "Account" : "Security",
        LastUpdated = "2025-01-15",
        Content = $"This is the content for article {i}. It contains helpful information about the topic.",
        Views = 1000 + (i * 100)
    }).ToArray();

    var scaledStats = ToonConverter.GetTokenReduction(scaledArticles);
    
    Console.WriteLine($"  {count,3} articles: JSON={scaledStats.JsonTokens,5} | TOON={scaledStats.ToonTokens,5} | Saved={scaledStats.ReductionPercent:F0}%");
}

Console.WriteLine();
Console.WriteLine("=== RAG Example Complete ===");
Console.WriteLine("\nKey Insight: TOON savings increase with more documents!");
Console.WriteLine("At 100 documents, you save ~50% of tokens, allowing you to");
Console.WriteLine("include more context within the same token budget.");
