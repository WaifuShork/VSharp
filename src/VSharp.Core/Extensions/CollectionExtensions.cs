using System.Text;
using JetBrains.Annotations;
using VSharp.Core.Analysis.Diagnostics;
using VSharp.Core.Utilities;

namespace VSharp.Core.Extensions;

public enum SeparatorKind
{
	None,
	Space,
	NewLine,
	Comma
}

public static class CollectionExtensions
{
	[PublicAPI]
	public static bool IsEmpty<T>(this IEnumerable<T> collection)
	{
		Throw<NullReferenceException>.If(collection is null, "collection cannot be null to check if empty");
		return !collection!.Any();
	}

	[PublicAPI]
	public static bool IsNotEmpty<T>(this IEnumerable<T> collection)
	{
		Throw<NullReferenceException>.If(collection is null, "collection cannot be null to check if not empty");
		return collection!.Any();
	}
	
	[PublicAPI]
	public static T Last<T>(this T[] array)
	{
		Throw<NullReferenceException>.If(array is null, "array cannot be null to get last value");
		return array![^1];
	}

	[PublicAPI]
	public static T Last<T>(this IList<T> list)
	{
		Throw<NullReferenceException>.If(list is null, "list cannot be null to get last value");
		return list![^1];
	}

	[PublicAPI]
	public static TValue Last<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
	{
		Throw<NullReferenceException>.If(dictionary is null, "dictionary cannot be null to get last value");
		return dictionary!.Values.Last();
	}

	[PublicAPI]
	public static async Task DumpDiagnosticsAsync(this IEnumerable<DiagnosticInfo> diagnostics)
	{
		var sb = new StringBuilder();
		foreach (var diagnostic in diagnostics)
		{
			sb.AppendLine(diagnostic.Format());
		}

		await Console.Out.WriteLineAsync(sb.ToString());
	}
	
	[PublicAPI]
	public static List<DiagnosticInfo> WhereStatic(this IEnumerable<DiagnosticInfo> input)
	{
		return input.Where(d => d.IsStatic).ToList();
	}

	[PublicAPI]
	public static List<T> AppendRange<T>(this List<T> input, IEnumerable<T> range)
	{
		input.AddRange(range);
		return input;
	}
	
	[PublicAPI]
	public static List<T> Slice<T>(this List<T> input, Range range)
	{
		var (start, length) = range.GetOffsetAndLength(input.Count);
		return input.GetRange(start, length);
	}

	[PublicAPI]
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