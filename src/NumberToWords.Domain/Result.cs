namespace NumberToWords.Domain;

/// <summary>
/// The outcome of an operation that can fail because of the input it was given.
/// </summary>
/// <remarks>
/// Bad input from a web form is ordinary, not exceptional, so the parser returns it instead
/// of throwing. A mistyped amount then takes a branch, and callers have to handle the
/// failure before they can reach the value.
/// </remarks>
/// <typeparam name="TValue">The type produced when the operation succeeds.</typeparam>
public readonly struct Result<TValue> : IEquatable<Result<TValue>>
    where TValue : class
{
    private readonly TValue? _value;
    private readonly ValidationError? _error;

    private Result(TValue? value, ValidationError? error)
    {
        _value = value;
        _error = error;
    }

    /// <summary>Gets a value indicating whether the operation produced a value.</summary>
    public bool IsSuccess => _error is null;

    /// <summary>Gets the produced value.</summary>
    /// <exception cref="InvalidOperationException">The operation failed.</exception>
    public TValue Value => _value
        ?? throw new InvalidOperationException("This result failed, so it holds no value. Check IsSuccess first.");

    /// <summary>Gets the reason the operation failed.</summary>
    /// <exception cref="InvalidOperationException">The operation succeeded.</exception>
    public ValidationError Error => _error
        ?? throw new InvalidOperationException("This result succeeded, so it holds no error. Check IsSuccess first.");

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue>(value, error: null);
    }

    /// <summary>Creates a failed result carrying <paramref name="error"/>.</summary>
    public static Result<TValue> Failure(ValidationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<TValue>(value: null, error);
    }

    /// <inheritdoc />
    public bool Equals(Result<TValue> other) =>
        EqualityComparer<TValue?>.Default.Equals(_value, other._value) && _error == other._error;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result<TValue> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_value, _error);

    /// <summary>Compares two results for equality.</summary>
    public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);

    /// <summary>Compares two results for inequality.</summary>
    public static bool operator !=(Result<TValue> left, Result<TValue> right) => !left.Equals(right);
}
