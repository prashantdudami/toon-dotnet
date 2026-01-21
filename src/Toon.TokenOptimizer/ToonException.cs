namespace Toon.TokenOptimizer;

/// <summary>
/// Exception thrown when TOON serialization or deserialization fails.
/// </summary>
public class ToonException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ToonException class.
    /// </summary>
    public ToonException() : base() { }

    /// <summary>
    /// Initializes a new instance of the ToonException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ToonException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the ToonException class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ToonException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when TOON format parsing fails.
/// </summary>
public class ToonParseException : ToonException
{
    /// <summary>
    /// The position in the input where the error occurred.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Initializes a new instance of the ToonParseException class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the input where the error occurred.</param>
    public ToonParseException(string message, int position = -1) : base(message)
    {
        Position = position;
    }
}

/// <summary>
/// Exception thrown when TOON serialization fails.
/// </summary>
public class ToonSerializationException : ToonException
{
    /// <summary>
    /// The type that failed to serialize.
    /// </summary>
    public Type? TargetType { get; }

    /// <summary>
    /// Initializes a new instance of the ToonSerializationException class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="targetType">The type that failed to serialize.</param>
    /// <param name="innerException">The exception that caused the serialization failure.</param>
    public ToonSerializationException(string message, Type? targetType = null, Exception? innerException = null) 
        : base(message, innerException!)
    {
        TargetType = targetType;
    }
}
