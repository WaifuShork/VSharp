namespace VSharp.Core.Extensions;

public enum SeparatorKind
{
	None,
	Space,
	NewLine,
	Comma
}

public static class ListExtensions
{
	public static List<T> AppendRange<T>(this List<T> input, IEnumerable<T> range)
	{
		input.AddRange(range);
		return input;
	}
	
	public static List<T> Slice<T>(this List<T> input, Range range)
	{
		var (start, length) = range.GetOffsetAndLength(input.Count);
		return input.GetRange(start, length);
	}

	public static string ToString<T>(this IEnumerable<T> input, SeparatorKind kind)
	{
		var separator = kind switch
		{
			SeparatorKind.None => "",
			SeparatorKind.Space => " ",
			SeparatorKind.NewLine => Environment.NewLine,
			SeparatorKind.Comma => ", ",
			_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
		};
		return string.Join(separator, input);
	}
}