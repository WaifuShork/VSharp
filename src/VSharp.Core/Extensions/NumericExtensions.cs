namespace VSharp.Core.Extensions;

public static class NumericExtensions
{
	public static bool IsWithinBounds<T>(this T index, string bounds)
		where T : INumber<T>, IComparisonOperators<T, T>
	{
		if (typeof(T) == typeof(ISignedNumber<>))
		{
			var signedIndex = (long) Convert.ChangeType(index, TypeCode.Int64);
			return signedIndex <= bounds.Length - 1 && signedIndex >= 0;
		}

		if (typeof(T) == typeof(IUnsignedNumber<>))
		{
			var unsignedIndex = (ulong)Convert.ChangeType(index, TypeCode.UInt64);
			return unsignedIndex <= (ulong)bounds.Length - 1 && unsignedIndex >= 0;
		}

		return false;
	}
}