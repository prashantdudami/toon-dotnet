namespace Toon.TokenOptimizer;

/// <summary>
/// Specifies the TOON format to use for serialization.
/// </summary>
public enum ToonFormat
{
    /// <summary>
    /// Official TOON v3.0 specification format.
    /// Uses indentation-based structure for human readability.
    /// Example: users[2]{id,name}:
    ///            1,Alice
    ///            2,Bob
    /// </summary>
    Standard,

    /// <summary>
    /// Compact single-line format optimized for maximum token savings.
    /// Uses pipe delimiters and minimal structure.
    /// Example: ~[id|name]:1|Alice,2|Bob
    /// </summary>
    Compact
}
