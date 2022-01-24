namespace VSharp.Core;

using WaifuShork.Common;

[PublicAPI]
public readonly record struct Optional<T>
{ 
    public Optional(T value)
    {
        Value = value;
        HasValue = Value is not null;
    }

    public bool HasValue { get; }
    public T Value { get; }

    public static implicit operator Optional<T>(T value)
    {
        return new(value);
    }

    public bool Equals(Optional<T> other)
    {
        if (Value is null && other.Value is null)
        {
            return true;
        }

        if (Value is null)
        {
            return HasValue == other.HasValue;
        }

        if (Value.GetType().IsValueType)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        return HasValue == other.HasValue && Value!.Equals(other.Value);

    }
    
    public override string ToString()
    {
        return HasValue
            ? Value?.ToString() ?? "null"
            : "unspecified";
    }

    public override int GetHashCode()
    {
        if (HasValue)
        {
            return (Value!, HasValue).GetHashCode();
        }

        return (int)$"|{HasValue}|".GetFnvHashCode();
    }
}