namespace VSharp.Core.Extensions;

public static class StringExtensions
{
	public static char At(this string str, int index)
	{
		if (!index.IsWithinBounds(str))
		{
			return '\0';
		}
		
		return str[index];
	}
}