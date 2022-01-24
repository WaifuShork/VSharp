namespace VSharp.Core.Extensions;

using System.Globalization;
using WaifuShork.Extensions;

public static class StringExtensions
{
	public static IEnumerable<string> SplitToLines(this string? input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			yield break;
		}

		using var reader = new StringReader(input);
		string? line;
		while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
		{
			yield return line;
		}
	}
	
	public static char At(this string? str, in int index)
	{
		if (string.IsNullOrWhiteSpace(str) || !index.IsWithinBounds(str))
		{
			return CharacterInfo.InvalidCharacter;
		}
		
		return str[index];
	}

	public static string From(this string? str, in Range range)
	{
		if (string.IsNullOrWhiteSpace(str) || !range.Start.Value.IsWithinBounds(str) && !range.Start.Value.IsWithinBounds(str))
		{
			return CharacterInfo.InvalidCharacter.ToString();
		}

		return str[range];
	}

	public static char FromHex(this string? str)
	{
		if (string.IsNullOrWhiteSpace(str) || !short.TryParse(str, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var chr))
		{
			return CharacterInfo.InvalidCharacter;
		}
		
		return chr.As<char>();
	}
}