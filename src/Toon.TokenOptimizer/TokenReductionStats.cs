namespace Toon.TokenOptimizer;

/// <summary>
/// Statistics about token reduction when converting to TOON format.
/// </summary>
public class TokenReductionStats
{
    /// <summary>
    /// Estimated number of tokens in JSON format.
    /// </summary>
    public int JsonTokens { get; set; }

    /// <summary>
    /// Estimated number of tokens in TOON format.
    /// </summary>
    public int ToonTokens { get; set; }

    /// <summary>
    /// Number of tokens saved by using TOON format.
    /// </summary>
    public int TokensSaved => JsonTokens - ToonTokens;

    /// <summary>
    /// Percentage reduction in token count.
    /// </summary>
    public double ReductionPercent => JsonTokens > 0 
        ? (double)TokensSaved / JsonTokens * 100 
        : 0;

    /// <summary>
    /// The JSON representation of the data.
    /// </summary>
    public string JsonOutput { get; set; } = string.Empty;

    /// <summary>
    /// The TOON representation of the data.
    /// </summary>
    public string ToonOutput { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the token reduction statistics.
    /// </summary>
    public override string ToString()
    {
        return $"JSON: {JsonTokens} tokens, TOON: {ToonTokens} tokens, Saved: {TokensSaved} ({ReductionPercent:F1}%)";
    }
}
