using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Toon.TokenOptimizer;

/// <summary>
/// Converts objects to and from TOON (Token Optimized Object Notation) format.
/// TOON reduces token usage by 40-60% when sending data to LLMs.
/// </summary>
public static class ToonConverter
{
    /// <summary>
    /// Converts an object to TOON format.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>A TOON-formatted string.</returns>
    /// <exception cref="ToonSerializationException">Thrown when serialization fails.</exception>
    public static string ToToon(object? obj, ToonOptions? options = null)
    {
        options ??= ToonOptions.Default;
        
        if (obj == null)
            return string.Empty;

        try
        {
            return ConvertToToon(obj, options, 0);
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
    /// Calculates token reduction statistics for an object.
    /// </summary>
    /// <param name="obj">The object to analyze.</param>
    /// <param name="options">Optional conversion options.</param>
    /// <returns>Statistics about the token reduction.</returns>
    public static TokenReductionStats GetTokenReduction(object? obj, ToonOptions? options = null)
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
        var toonOutput = ToToon(obj, options);

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

    #region Private Methods

    private static string ConvertToToon(object obj, ToonOptions options, int depth)
    {
        if (depth > options.MaxDepth)
            throw new ToonSerializationException($"Maximum depth of {options.MaxDepth} exceeded");

        var type = obj.GetType();

        // Handle primitives and simple types
        if (IsPrimitiveType(type))
            return FormatPrimitive(obj, options);

        // Handle arrays and collections
        if (obj is IEnumerable enumerable && type != typeof(string))
            return ConvertEnumerableToToon(enumerable, options, depth);

        // Handle complex objects
        return ConvertObjectToToon(obj, options, depth);
    }

    private static string ConvertEnumerableToToon(IEnumerable enumerable, ToonOptions options, int depth)
    {
        var items = enumerable.Cast<object?>().ToList();
        
        if (items.Count == 0)
            return $"{options.Prefix}[]";

        // Check if all items are of the same complex type (for header row optimization)
        var firstItem = items.FirstOrDefault(i => i != null);
        if (firstItem != null && !IsPrimitiveType(firstItem.GetType()))
        {
            return ConvertObjectArrayToToon(items, firstItem.GetType(), options, depth);
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

    private static string ConvertObjectArrayToToon(List<object?> items, Type itemType, ToonOptions options, int depth)
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
                return ConvertToToon(value, options, depth + 1);
            });
            
            rows.Add(string.Join(options.Delimiter.ToString(), values));
        }

        sb.Append(string.Join(options.ArrayDelimiter.ToString(), rows));
        
        return sb.ToString();
    }

    private static string ConvertObjectToToon(object obj, ToonOptions options, int depth)
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
                    : ConvertToToon(value, options, depth + 1);

            pairs.Add($"{prop.Name}{options.Delimiter}{formattedValue}");
        }

        sb.Append(string.Join(options.ArrayDelimiter.ToString(), pairs));
        
        return sb.ToString();
    }

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
