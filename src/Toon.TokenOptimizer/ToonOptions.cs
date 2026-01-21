namespace Toon.TokenOptimizer;

/// <summary>
/// Configuration options for TOON serialization and deserialization.
/// </summary>
public class ToonOptions
{
    /// <summary>
    /// The delimiter character used to separate fields. Default is '|'.
    /// </summary>
    public char Delimiter { get; set; } = '|';

    /// <summary>
    /// The delimiter character used to separate array elements. Default is ','.
    /// </summary>
    public char ArrayDelimiter { get; set; } = ',';

    /// <summary>
    /// Whether to include null values in the output. Default is false.
    /// </summary>
    public bool IncludeNulls { get; set; } = false;

    /// <summary>
    /// Maximum depth for nested object serialization. Default is 10.
    /// </summary>
    public int MaxDepth { get; set; } = 10;

    /// <summary>
    /// The prefix character for TOON format. Default is '~'.
    /// </summary>
    public char Prefix { get; set; } = '~';

    /// <summary>
    /// Whether to use header row for arrays of objects. Default is true.
    /// </summary>
    public bool UseHeaderRow { get; set; } = true;

    /// <summary>
    /// The format string for DateTime serialization. Default is "yyyy-MM-ddTHH:mm:ss".
    /// </summary>
    public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss";

    /// <summary>
    /// Whether property names should be case-insensitive during deserialization. Default is true.
    /// </summary>
    public bool PropertyNameCaseInsensitive { get; set; } = true;

    /// <summary>
    /// Creates a new instance of ToonOptions with default values.
    /// </summary>
    public static ToonOptions Default => new();
}
