namespace VSharp.Core.Extensions;

[PublicAPI]
public static class CharExtensions
{
	[PublicAPI]
	public static rune AsRune(this char ch)
	{
		return new(ch);
	}
}