using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace VSharp.Core.Analysis.Text;

public static class UnicodeCharacterInfo
{

	private const byte UnicodeCategoryMask = 0x1F;
	private static ReadOnlySpan<byte> AsciiCharInfo => new byte[]
	{
		0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x8E, 0x8E, 0x8E, 0x8E, 0x8E, 0x0E, 0x0E, // U+0000..U+000F
		0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, 0x0E, // U+0010..U+001F
		0x8B, 0x18, 0x18, 0x18, 0x1A, 0x18, 0x18, 0x18, 0x14, 0x15, 0x18, 0x19, 0x18, 0x13, 0x18, 0x18, // U+0020..U+002F
		0x48, 0x48, 0x48, 0x48, 0x48, 0x48, 0x48, 0x48, 0x48, 0x48, 0x18, 0x18, 0x19, 0x19, 0x19, 0x18, // U+0030..U+003F
		0x18, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, // U+0040..U+004F
		0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x14, 0x18, 0x15, 0x1B, 0x12, // U+0050..U+005F
		0x1B, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, // U+0060..U+006F
		0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x14, 0x19, 0x15, 0x19, 0x0E, // U+0070..U+007F
	};

	public static bool IsValidIdentifier([NotNullWhen(returnValue: true)] string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}

		if (!IsIdentifierStartCharacter(name[0]))
		{
			return false;
		}

		var nameLength = name.Length;
		for (var i = 1; i < nameLength; i++)
		{
			if (!IsIdentifierPartCharacter(name[i]))
			{
				return false;
			}
		}

		return true;
	}
	
	public static bool IsIdentifierStartCharacter(char ch)
	{
		if (ch < 'a')
		{
			if (ch < 'A')
			{
				return false;
			}

			return ch <= 'Z' || ch == '_';
		}

		if (ch <= 'z')
		{
			return true;
		}

		if (ch <= '\u007F')
		{
			return false;
		}
		
		return IsLetterChar(GetUnicodeCategory(new Rune(ch)));
	}

	public static bool IsIdentifierPartCharacter(char ch)
	{
		if (ch < 'a')
		{
			if (ch < 'A')
			{
				return false;
			}

			return ch <= 'Z' || ch == '_';
		}

		if (ch <= 'z')
		{
			return true;
		}

		if (ch <= '\u007F')
		{
			return false;
		}

		var category = GetUnicodeCategory(new Rune(ch));
		return IsLetterChar(category)
		       || IsDecimalDigitChar(category)
		       || IsConnectingChar(category)
		       || IsCombiningChar(category)
		       || IsFormattingChar(category);
	}
	
	private static bool IsLetterChar(UnicodeCategory category)
	{
		switch (category)
		{
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.ModifierLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.LetterNumber:
				return true;
			default:
				return false;
		}
	}

	public static UnicodeCategory GetUnicodeCategory(Rune value)
	{
		if (value.IsAscii)
		{
			return (UnicodeCategory)(AsciiCharInfo[value.Value] & UnicodeCategoryMask);
		}
		
		return GetUnicodeCategoryNonAscii(value);
	}
	
	private static bool IsCombiningChar(UnicodeCategory category)
	{
		switch (category)
		{
			case UnicodeCategory.NonSpacingMark:
			case UnicodeCategory.SpacingCombiningMark:
				return true;
			default:
				return false;
		}
	}
	
	public static UnicodeCategory GetUnicodeCategoryNonAscii(Rune value)
	{
		Debug.Assert(!value.IsAscii, "shouldn't use this non-optimized code path for ASCII characters");
		return CharUnicodeInfo.GetUnicodeCategory(value.Value);
	}

	private static bool IsDecimalDigitChar(UnicodeCategory category)
	{
		return category == UnicodeCategory.DecimalDigitNumber;
	}
	
	private static bool IsConnectingChar(UnicodeCategory category)
	{
		return category == UnicodeCategory.ConnectorPunctuation;
	}
	
	public static bool IsFormattingChar(char ch)
	{
		return ch > 127 && IsFormattingChar(GetUnicodeCategory(new Rune(ch)));
	}
	
	private static bool IsFormattingChar(UnicodeCategory category)
	{
		return category == UnicodeCategory.Format;
	}
}