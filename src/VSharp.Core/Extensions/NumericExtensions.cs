namespace VSharp.Core.Extensions;

using WaifuShork.Extensions;

public static class NumericExtensions
{
	[PublicAPI]
	public static bool IsInvalidHash(this int value)
	{
		return value == 0;
	}
	
	[PublicAPI]
	public static bool IsWithinBounds(this Range range, SourceText bounds)
	{
		return range.IsWithinBounds(bounds.Text);
	}
	
	[PublicAPI]
	public static bool IsWithinBounds<T>(this T index, SourceText bounds)
		where T : INumber<T>, IComparisonOperators<T, T>
	{
		return index.IsWithinBounds(bounds.Text);
	}
	
	[PublicAPI]
	public static bool IsWithinBounds(this Range range, string bounds)
	{
		return range.Start.Value.IsWithinBounds(bounds) && 
		       range.End.Value.IsWithinBounds(bounds);
	}

	[PublicAPI]
	public static bool IsWithinBounds<T>(this T index, in string bounds)
		where T : INumber<T>, IComparisonOperators<T, T>
	{
		// it's pretty much impossible to negatively index,
		// so therefor bounds checks should literally never be negative
		var valueIndex = index.As<ulong>();
		return valueIndex >= 0 && valueIndex <= (ulong) bounds.Length;
	}
}