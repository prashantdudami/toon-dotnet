namespace Toon.TokenOptimizer;

/// <summary>
/// Statistics comparing token usage across JSON, Standard TOON, and Compact TOON formats.
/// </summary>
public class TokenComparisonStats
{
    /// <summary>
    /// Estimated number of tokens in JSON format.
    /// </summary>
    public int JsonTokens { get; set; }

    /// <summary>
    /// Estimated number of tokens in Standard TOON format (v3.0 spec).
    /// </summary>
    public int StandardToonTokens { get; set; }

    /// <summary>
    /// Estimated number of tokens in Compact TOON format.
    /// </summary>
    public int CompactToonTokens { get; set; }

    /// <summary>
    /// Tokens saved by using Standard TOON instead of JSON.
    /// </summary>
    public int StandardToonSaved => JsonTokens - StandardToonTokens;

    /// <summary>
    /// Tokens saved by using Compact TOON instead of JSON.
    /// </summary>
    public int CompactToonSaved => JsonTokens - CompactToonTokens;

    /// <summary>
    /// Additional tokens saved by using Compact TOON instead of Standard TOON.
    /// </summary>
    public int CompactVsStandardSaved => StandardToonTokens - CompactToonTokens;

    /// <summary>
    /// Percentage reduction when using Standard TOON.
    /// </summary>
    public double StandardToonReductionPercent => JsonTokens > 0 
        ? (double)StandardToonSaved / JsonTokens * 100 
        : 0;

    /// <summary>
    /// Percentage reduction when using Compact TOON.
    /// </summary>
    public double CompactToonReductionPercent => JsonTokens > 0 
        ? (double)CompactToonSaved / JsonTokens * 100 
        : 0;

    /// <summary>
    /// The JSON representation of the data.
    /// </summary>
    public string JsonOutput { get; set; } = string.Empty;

    /// <summary>
    /// The Standard TOON representation of the data.
    /// </summary>
    public string StandardToonOutput { get; set; } = string.Empty;

    /// <summary>
    /// The Compact TOON representation of the data.
    /// </summary>
    public string CompactToonOutput { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the token comparison statistics.
    /// </summary>
    public override string ToString()
    {
        return $"JSON: {JsonTokens} tokens | Standard TOON: {StandardToonTokens} ({StandardToonReductionPercent:F1}% saved) | Compact TOON: {CompactToonTokens} ({CompactToonReductionPercent:F1}% saved)";
    }
}
