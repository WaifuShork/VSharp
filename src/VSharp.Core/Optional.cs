namespace VSharp.Core;

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

    public override string ToString()
    {
        return HasValue
            ? Value?.ToString() ?? "null"
            : "unspecified";
    }
}