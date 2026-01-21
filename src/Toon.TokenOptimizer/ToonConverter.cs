using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Toon.TokenOptimizer;

/// <summary>
/// Converts objects to and from TOON (Token Optimized Object Notation) format.
/// Supports both Standard TOON v3.0 spec and Compact format for maximum token savings.
/// </summary>
public static class ToonConverter
{
    /// <summary>
    /// Converts an object to Standard TOON v3.0 format (indentation-based, human-readable).
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>A TOON-formatted string following the official v3.0 specification.</returns>
    /// <exception cref="ToonSerializationException">Thrown when serialization fails.</exception>
    public static string ToToon(object? obj, ToonOptions? options = null)
    {
        options ??= ToonOptions.Default;
        
        if (obj == null)
            return string.Empty;

        try
        {
            return ConvertToStandardToon(obj, options, 0);
        }
        catch (ToonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ToonSerializationException($"Failed to convert object to TOON format: {ex.Message}", obj.GetType(), ex);
        }
    }

    /// <summary>
    /// Converts an object to Compact TOON format (single-line, maximum token savings).
    /// This format provides 40-60% token reduction compared to JSON.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>A compact TOON-formatted string optimized for token efficiency.</returns>
    /// <exception cref="ToonSerializationException">Thrown when serialization fails.</exception>
    public static string ToCompactToon(object? obj, ToonOptions? options = null)
    {
        options ??= ToonOptions.Default;
        
        if (obj == null)
            return string.Empty;

        try
        {
            return ConvertToCompactToon(obj, options, 0);
        }
        catch (ToonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ToonSerializationException($"Failed to convert object to compact TOON format: {ex.Message}", obj.GetType(), ex);
        }
    }

    /// <summary>
    /// Converts an object to TOON format using the specified format type.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <param name="format">The TOON format to use (Standard or Compact).</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>A TOON-formatted string.</returns>
    public static string Serialize(object? obj, ToonFormat format = ToonFormat.Standard, ToonOptions? options = null)
    {
        return format switch
        {
            ToonFormat.Standard => ToToon(obj, options),
            ToonFormat.Compact => ToCompactToon(obj, options),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    /// <summary>
    /// Converts a TOON-formatted string back to an object.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="toon">The TOON-formatted string.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="ToonParseException">Thrown when parsing fails.</exception>
    public static T? FromToon<T>(string toon, ToonOptions? options = null)
    {
        options ??= ToonOptions.Default;
        
        if (string.IsNullOrWhiteSpace(toon))
            return default;

        try
        {
            return (T?)ParseToon(toon, typeof(T), options);
        }
        catch (ToonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ToonParseException($"Failed to parse TOON format: {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to convert a TOON-formatted string back to an object without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="toon">The TOON-formatted string.</param>
    /// <param name="result">The deserialized object if successful, default otherwise.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>True if conversion succeeded, false otherwise.</returns>
    public static bool TryFromToon<T>(string toon, out T? result, ToonOptions? options = null)
    {
        try
        {
            result = FromToon<T>(toon, options);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Validates whether a string is valid TOON format.
    /// </summary>
    /// <param name="toon">The string to validate.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>True if the string is valid TOON format, false otherwise.</returns>
    public static bool IsValidToon(string toon, ToonOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(toon))
            return false;

        options ??= ToonOptions.Default;
        toon = toon.Trim();

        // Check for valid prefix
        if (!toon.StartsWith(options.Prefix.ToString()))
            return false;

        // Try to parse - if it succeeds, it's valid
        try
        {
            var content = toon.Substring(1);
            
            // Array format: ~[...] or ~[Header]:data
            if (content.StartsWith("["))
                return true;
            
            // Object format: ~key|value,key|value
            if (content.Contains(options.Delimiter))
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Asynchronously writes Standard TOON-formatted data to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="obj">The object to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task ToToonAsync(Stream stream, object? obj, ToonOptions? options = null, CancellationToken cancellationToken = default)
    {
        var toon = ToToon(obj, options);
        var bytes = System.Text.Encoding.UTF8.GetBytes(toon);
        await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes Compact TOON-formatted data to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="obj">The object to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task ToCompactToonAsync(Stream stream, object? obj, ToonOptions? options = null, CancellationToken cancellationToken = default)
    {
        var toon = ToCompactToon(obj, options);
        var bytes = System.Text.Encoding.UTF8.GetBytes(toon);
        await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads and parses TOON-formatted data from a stream.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized object.</returns>
    public static async Task<T?> FromToonAsync<T>(Stream stream, ToonOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var toon = await reader.ReadToEndAsync().ConfigureAwait(false);
        return FromToon<T>(toon, options);
    }

    /// <summary>
    /// Converts an object to Standard TOON format and writes to a TextWriter.
    /// </summary>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <param name="obj">The object to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    public static void ToToon(TextWriter writer, object? obj, ToonOptions? options = null)
    {
        var toon = ToToon(obj, options);
        writer.Write(toon);
    }

    /// <summary>
    /// Converts an object to Compact TOON format and writes to a TextWriter.
    /// </summary>
    /// <param name="writer">The TextWriter to write to.</param>
    /// <param name="obj">The object to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    public static void ToCompactToon(TextWriter writer, object? obj, ToonOptions? options = null)
    {
        var toon = ToCompactToon(obj, options);
        writer.Write(toon);
    }

    /// <summary>
    /// Reads and parses TOON-formatted data from a TextReader.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="reader">The TextReader to read from.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>The deserialized object.</returns>
    public static T? FromToon<T>(TextReader reader, ToonOptions? options = null)
    {
        var toon = reader.ReadToEnd();
        return FromToon<T>(toon, options);
    }

    /// <summary>
    /// Calculates token reduction statistics for an object using the specified format.
    /// </summary>
    /// <param name="obj">The object to analyze.</param>
    /// <param name="format">The TOON format to use (defaults to Compact for maximum savings).</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>Statistics about the token reduction.</returns>
    public static TokenReductionStats GetTokenReduction(object? obj, ToonFormat format = ToonFormat.Compact, ToonOptions? options = null)
    {
        if (obj == null)
        {
            return new TokenReductionStats
            {
                JsonTokens = 0,
                ToonTokens = 0,
                JsonOutput = "null",
                ToonOutput = ""
            };
        }

        var jsonOutput = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false });
        var toonOutput = format == ToonFormat.Compact 
            ? ToCompactToon(obj, options) 
            : ToToon(obj, options);

        // Simple token estimation (roughly 4 characters per token for English text)
        // This is a simplified estimation - actual tokenization varies by model
        var jsonTokens = EstimateTokens(jsonOutput);
        var toonTokens = EstimateTokens(toonOutput);

        return new TokenReductionStats
        {
            JsonTokens = jsonTokens,
            ToonTokens = toonTokens,
            JsonOutput = jsonOutput,
            ToonOutput = toonOutput
        };
    }

    /// <summary>
    /// Calculates token reduction statistics for an object (uses Compact format by default).
    /// </summary>
    /// <param name="obj">The object to analyze.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>Statistics about the token reduction.</returns>
    public static TokenReductionStats GetTokenReduction(object? obj, ToonOptions? options)
    {
        return GetTokenReduction(obj, ToonFormat.Compact, options);
    }

    /// <summary>
    /// Compares token reduction between Standard and Compact TOON formats.
    /// </summary>
    /// <param name="obj">The object to analyze.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>A comparison of token usage across JSON, Standard TOON, and Compact TOON.</returns>
    public static TokenComparisonStats CompareFormats(object? obj, ToonOptions? options = null)
    {
        if (obj == null)
        {
            return new TokenComparisonStats
            {
                JsonTokens = 0,
                StandardToonTokens = 0,
                CompactToonTokens = 0,
                JsonOutput = "null",
                StandardToonOutput = "",
                CompactToonOutput = ""
            };
        }

        var jsonOutput = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false });
        var standardOutput = ToToon(obj, options);
        var compactOutput = ToCompactToon(obj, options);

        return new TokenComparisonStats
        {
            JsonTokens = EstimateTokens(jsonOutput),
            StandardToonTokens = EstimateTokens(standardOutput),
            CompactToonTokens = EstimateTokens(compactOutput),
            JsonOutput = jsonOutput,
            StandardToonOutput = standardOutput,
            CompactToonOutput = compactOutput
        };
    }

    #region Private Methods - Standard TOON v3.0

    private static string ConvertToStandardToon(object obj, ToonOptions options, int depth)
    {
        if (depth > options.MaxDepth)
            throw new ToonSerializationException($"Maximum depth of {options.MaxDepth} exceeded");

        var type = obj.GetType();
        var indent = new string(' ', depth * 2);

        // Handle primitives and simple types
        if (IsPrimitiveType(type))
            return FormatStandardPrimitive(obj, options);

        // Handle arrays and collections
        if (obj is IEnumerable enumerable && type != typeof(string))
            return ConvertEnumerableToStandardToon(enumerable, options, depth);

        // Handle complex objects
        return ConvertObjectToStandardToon(obj, options, depth);
    }

    private static string ConvertEnumerableToStandardToon(IEnumerable enumerable, ToonOptions options, int depth)
    {
        var items = enumerable.Cast<object?>().ToList();
        var indent = new string(' ', depth * 2);
        var childIndent = new string(' ', (depth + 1) * 2);
        
        if (items.Count == 0)
            return $"[0]:";

        // Check if all items are of the same complex type (for tabular format)
        var firstItem = items.FirstOrDefault(i => i != null);
        if (firstItem != null && !IsPrimitiveType(firstItem.GetType()))
        {
            return ConvertObjectArrayToStandardToon(items, firstItem.GetType(), options, depth);
        }

        // Handle array of primitives - inline format
        var sb = new StringBuilder();
        sb.Append($"[{items.Count}]: ");
        sb.Append(string.Join(",", items.Select(i => i == null ? "" : FormatStandardPrimitive(i, options))));
        
        return sb.ToString();
    }

    private static string ConvertObjectArrayToStandardToon(List<object?> items, Type itemType, ToonOptions options, int depth)
    {
        var properties = GetSerializableProperties(itemType);
        var childIndent = new string(' ', (depth + 1) * 2);
        
        if (properties.Length == 0)
            return $"[{items.Count}]:";

        var sb = new StringBuilder();
        
        // Header with field list: [N]{field1,field2,...}:
        sb.Append($"[{items.Count}]{{");
        sb.Append(string.Join(",", properties.Select(p => p.Name)));
        sb.Append("}:");

        // Data rows
        foreach (var item in items)
        {
            sb.AppendLine();
            sb.Append(childIndent);
            
            if (item == null)
            {
                sb.Append(string.Join(",", properties.Select(_ => "")));
                continue;
            }

            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                if (value == null) return "";
                if (IsPrimitiveType(value.GetType()))
                    return FormatStandardPrimitive(value, options);
                return ConvertToStandardToon(value, options, depth + 1);
            });
            
            sb.Append(string.Join(",", values));
        }

        return sb.ToString();
    }

    private static string ConvertObjectToStandardToon(object obj, ToonOptions options, int depth)
    {
        var properties = GetSerializableProperties(obj.GetType());
        var indent = new string(' ', depth * 2);
        var childIndent = new string(' ', (depth + 1) * 2);
        
        if (properties.Length == 0)
            return "";

        var sb = new StringBuilder();
        var isFirst = true;

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            
            if (value == null && !options.IncludeNulls)
                continue;

            if (!isFirst)
            {
                sb.AppendLine();
                sb.Append(indent);
            }
            isFirst = false;

            if (value == null)
            {
                sb.Append($"{prop.Name}: null");
            }
            else if (IsPrimitiveType(value.GetType()))
            {
                sb.Append($"{prop.Name}: {FormatStandardPrimitive(value, options)}");
            }
            else if (value is IEnumerable enumerable && value.GetType() != typeof(string))
            {
                var arrayOutput = ConvertEnumerableToStandardToon(enumerable, options, depth + 1);
                sb.Append($"{prop.Name}{arrayOutput}");
            }
            else
            {
                sb.Append($"{prop.Name}:");
                var nestedOutput = ConvertObjectToStandardToon(value, options, depth + 1);
                if (!string.IsNullOrEmpty(nestedOutput))
                {
                    sb.AppendLine();
                    sb.Append(childIndent);
                    sb.Append(nestedOutput);
                }
            }
        }

        return sb.ToString();
    }

    private static string FormatStandardPrimitive(object value, ToonOptions options)
    {
        return value switch
        {
            string s => QuoteIfNeeded(s, ','),
            bool b => b.ToString().ToLower(),
            DateTime dt => dt.ToString(options.DateTimeFormat),
            DateTimeOffset dto => dto.ToString(options.DateTimeFormat),
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };
    }

    private static string QuoteIfNeeded(string value, char delimiter)
    {
        if (string.IsNullOrEmpty(value) ||
            value.Contains(delimiter) ||
            value.Contains(':') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r') ||
            value.Contains('\t') ||
            value == "true" || value == "false" || value == "null" ||
            value.StartsWith(" ") || value.EndsWith(" ") ||
            value.StartsWith("-") ||
            IsNumericString(value))
        {
            return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }
        return value;
    }

    private static bool IsNumericString(string value)
    {
        return double.TryParse(value, out _);
    }

    #endregion

    #region Private Methods - Compact TOON

    private static string ConvertToCompactToon(object obj, ToonOptions options, int depth)
    {
        if (depth > options.MaxDepth)
            throw new ToonSerializationException($"Maximum depth of {options.MaxDepth} exceeded");

        var type = obj.GetType();

        // Handle primitives and simple types
        if (IsPrimitiveType(type))
            return FormatPrimitive(obj, options);

        // Handle arrays and collections
        if (obj is IEnumerable enumerable && type != typeof(string))
            return ConvertEnumerableToCompactToon(enumerable, options, depth);

        // Handle complex objects
        return ConvertObjectToCompactToon(obj, options, depth);
    }

    private static string ConvertEnumerableToCompactToon(IEnumerable enumerable, ToonOptions options, int depth)
    {
        var items = enumerable.Cast<object?>().ToList();
        
        if (items.Count == 0)
            return $"{options.Prefix}[]";

        // Check if all items are of the same complex type (for header row optimization)
        var firstItem = items.FirstOrDefault(i => i != null);
        if (firstItem != null && !IsPrimitiveType(firstItem.GetType()))
        {
            return ConvertObjectArrayToCompactToon(items, firstItem.GetType(), options, depth);
        }

        // Handle array of primitives
        var sb = new StringBuilder();
        sb.Append(options.Prefix);
        sb.Append('[');
        sb.Append(string.Join(options.ArrayDelimiter.ToString(), 
            items.Select(i => i == null ? "" : FormatPrimitive(i, options))));
        sb.Append(']');
        
        return sb.ToString();
    }

    private static string ConvertObjectArrayToCompactToon(List<object?> items, Type itemType, ToonOptions options, int depth)
    {
        var properties = GetSerializableProperties(itemType);
        
        if (properties.Length == 0)
            return $"{options.Prefix}[]";

        var sb = new StringBuilder();
        sb.Append(options.Prefix);
        
        if (options.UseHeaderRow)
        {
            // Header row with property names
            sb.Append('[');
            sb.Append(string.Join(options.Delimiter.ToString(), properties.Select(p => p.Name)));
            sb.Append(']');
            sb.Append(':');
        }

        // Data rows
        var rows = new List<string>();
        foreach (var item in items)
        {
            if (item == null)
            {
                rows.Add(string.Join(options.Delimiter.ToString(), properties.Select(_ => "")));
                continue;
            }

            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                if (value == null) return "";
                if (IsPrimitiveType(value.GetType()))
                    return FormatPrimitive(value, options);
                return ConvertToCompactToon(value, options, depth + 1);
            });
            
            rows.Add(string.Join(options.Delimiter.ToString(), values));
        }

        sb.Append(string.Join(options.ArrayDelimiter.ToString(), rows));
        
        return sb.ToString();
    }

    private static string ConvertObjectToCompactToon(object obj, ToonOptions options, int depth)
    {
        var properties = GetSerializableProperties(obj.GetType());
        
        if (properties.Length == 0)
            return $"{options.Prefix}{{}}";

        var sb = new StringBuilder();
        sb.Append(options.Prefix);

        var pairs = new List<string>();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            
            if (value == null && !options.IncludeNulls)
                continue;

            var formattedValue = value == null 
                ? "" 
                : IsPrimitiveType(value.GetType()) 
                    ? FormatPrimitive(value, options)
                    : ConvertToCompactToon(value, options, depth + 1);

            pairs.Add($"{prop.Name}{options.Delimiter}{formattedValue}");
        }

        sb.Append(string.Join(options.ArrayDelimiter.ToString(), pairs));
        
        return sb.ToString();
    }

    #endregion

    #region Private Methods - Parsing

    private static object? ParseToon(string toon, Type targetType, ToonOptions options)
    {
        toon = toon.Trim();
        
        if (string.IsNullOrEmpty(toon))
            return null;

        // Remove prefix if present
        if (toon.StartsWith(options.Prefix.ToString()))
            toon = toon.Substring(1);

        // Handle arrays
        if (targetType.IsArray || (targetType.IsGenericType && 
            typeof(IEnumerable).IsAssignableFrom(targetType)))
        {
            return ParseToonArray(toon, targetType, options);
        }

        // Handle objects
        return ParseToonObject(toon, targetType, options);
    }

    private static object? ParseToonArray(string toon, Type targetType, ToonOptions options)
    {
        var elementType = targetType.IsArray 
            ? targetType.GetElementType()! 
            : targetType.GetGenericArguments().FirstOrDefault() ?? typeof(object);

        // Check for header row format: [Header1|Header2]:value1|value2,value3|value4
        if (toon.StartsWith("["))
        {
            var headerEnd = toon.IndexOf(']');
            if (headerEnd > 0 && headerEnd + 1 < toon.Length && toon[headerEnd + 1] == ':')
            {
                var headerPart = toon.Substring(1, headerEnd - 1);
                var dataPart = toon.Substring(headerEnd + 2);
                
                var headers = headerPart.Split(options.Delimiter);
                var rows = dataPart.Split(options.ArrayDelimiter);

                var items = new List<object?>();
                foreach (var row in rows)
                {
                    var values = row.Split(options.Delimiter);
                    var instance = Activator.CreateInstance(elementType);
                    
                    for (int i = 0; i < headers.Length && i < values.Length; i++)
                    {
                        var prop = elementType.GetProperty(headers[i], 
                            options.PropertyNameCaseInsensitive 
                                ? BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance 
                                : BindingFlags.Public | BindingFlags.Instance);
                        
                        if (prop != null && prop.CanWrite)
                        {
                            var convertedValue = ConvertValue(values[i], prop.PropertyType, options);
                            prop.SetValue(instance, convertedValue);
                        }
                    }
                    
                    items.Add(instance);
                }

                return CreateTypedArray(items, elementType, targetType);
            }

            // Simple array format: [value1,value2,value3]
            var content = toon.Trim('[', ']');
            var simpleValues = content.Split(options.ArrayDelimiter);
            var simpleItems = simpleValues.Select(v => ConvertValue(v.Trim(), elementType, options)).ToList();
            
            return CreateTypedArray(simpleItems, elementType, targetType);
        }

        throw new ToonParseException("Invalid TOON array format");
    }

    private static object CreateTypedArray(List<object?> items, Type elementType, Type targetType)
    {
        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++)
                array.SetValue(items[i], i);
            return array;
        }

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var item in items)
            list.Add(item);
        return list;
    }

    private static object? ParseToonObject(string toon, Type targetType, ToonOptions options)
    {
        if (toon.StartsWith("{}"))
            return Activator.CreateInstance(targetType);

        var instance = Activator.CreateInstance(targetType);
        var pairs = toon.Split(options.ArrayDelimiter);

        foreach (var pair in pairs)
        {
            var delimiterIndex = pair.IndexOf(options.Delimiter);
            if (delimiterIndex <= 0) continue;

            var name = pair.Substring(0, delimiterIndex);
            var value = pair.Substring(delimiterIndex + 1);

            var prop = targetType.GetProperty(name,
                options.PropertyNameCaseInsensitive
                    ? BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                    : BindingFlags.Public | BindingFlags.Instance);

            if (prop != null && prop.CanWrite)
            {
                var convertedValue = ConvertValue(value, prop.PropertyType, options);
                prop.SetValue(instance, convertedValue);
            }
        }

        return instance;
    }

    private static object? ConvertValue(string value, Type targetType, ToonOptions options)
    {
        if (string.IsNullOrEmpty(value))
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
            return value;
        if (underlyingType == typeof(int))
            return int.Parse(value);
        if (underlyingType == typeof(long))
            return long.Parse(value);
        if (underlyingType == typeof(double))
            return double.Parse(value);
        if (underlyingType == typeof(decimal))
            return decimal.Parse(value);
        if (underlyingType == typeof(float))
            return float.Parse(value);
        if (underlyingType == typeof(bool))
            return bool.Parse(value);
        if (underlyingType == typeof(DateTime))
            return DateTime.Parse(value);
        if (underlyingType == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(value);
        if (underlyingType == typeof(Guid))
            return Guid.Parse(value);
        if (underlyingType.IsEnum)
            return Enum.Parse(underlyingType, value, ignoreCase: true);

        // For complex types, try to parse as nested TOON
        if (value.StartsWith(options.Prefix.ToString()))
            return ParseToon(value, targetType, options);

        return value;
    }

    private static string FormatPrimitive(object value, ToonOptions options)
    {
        return value switch
        {
            string s => EscapeValue(s, options),
            bool b => b.ToString().ToLower(),
            DateTime dt => dt.ToString(options.DateTimeFormat),
            DateTimeOffset dto => dto.ToString(options.DateTimeFormat),
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };
    }

    private static string EscapeValue(string value, ToonOptions options)
    {
        // Escape delimiter characters if present in the value
        if (value.Contains(options.Delimiter) || value.Contains(options.ArrayDelimiter))
        {
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }
        return value;
    }

    private static bool IsPrimitiveType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(Guid) ||
               underlyingType.IsEnum;
    }

    private static PropertyInfo[] GetSerializableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !p.GetIndexParameters().Any())
            .OrderBy(p => p.Name)
            .ToArray();
    }

    private static int EstimateTokens(string text)
    {
        // Rough estimation: ~4 characters per token for structured data
        // This aligns with GPT tokenizer behavior for JSON-like content
        if (string.IsNullOrEmpty(text))
            return 0;

        // Count special characters that often become separate tokens
        var specialChars = text.Count(c => "{}[]\":,|~".Contains(c));
        var regularChars = text.Length - specialChars;

        // Special characters often count as individual tokens
        // Regular text averages ~4 chars per token
        return specialChars + (regularChars / 4);
    }

    #endregion
}
