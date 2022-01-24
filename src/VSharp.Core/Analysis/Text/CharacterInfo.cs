namespace VSharp.Core.Analysis.Text;

using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

[PublicAPI]
public static class CharacterInfo
{
	// A constant value that would never logically represent a valid token or character 
	// in the stream of characters from the source text
	public const char InvalidCharacter = char.MaxValue; // (char) 0xFFFF

	public const string Tab = "    ";

	public static bool IsHexDigit(this char c)
	{
		return c >= '0' && c <= '9' 
	        || c >= 'A' && c <= 'F' 
	        || c >= 'a' && c <= 'f';
	}
	
	public static bool IsBinaryDigit(this char c)
	{
		return c == '0' | c == '1';
	}
	
	public static bool IsDecimalDigit(this char c)
	{
		return c >= '0' && c <= '9';
	}
	
	public static bool IsQualifiedDigit(this char c)
	{
		return c.IsDecimalDigit()
		    || c.IsBinaryDigit()
		    || c.IsHexDigit();
	}
	
	public static int HexValue(this char c)
	{
		Debug.Assert(c.IsHexDigit());
		return c.IsDecimalDigit() ? c - '0' : (c & 0xdf) - 'A' + 10;
	}
	
	public static int BinaryValue(this char c)
	{
		Debug.Assert(c.IsBinaryDigit());
		return c - '0';
	}
	
	public static int DecimalValue(this char c)
	{
		Debug.Assert(c.IsDecimalDigit());
		return c - '0';
	}

	// UnicodeCategory value | Unicode designation
	// -----------------------+-----------------------
	// UppercaseLetter         "Lu" (letter, uppercase)
	// LowercaseLetter         "Ll" (letter, lowercase)
	// TitlecaseLetter         "Lt" (letter, title-case)
	// ModifierLetter          "Lm" (letter, modifier)
	// OtherLetter             "Lo" (letter, other)
	// NonSpacingMark          "Mn" (mark, non-spacing)
	// SpacingCombiningMark    "Mc" (mark, spacing combining)
	// EnclosingMark           "Me" (mark, enclosing)
	// DecimalDigitNumber      "Nd" (number, decimal digit)
	// LetterNumber            "Nl" (number, letter)
	// OtherNumber             "No" (number, other)
	// SpaceSeparator          "Zs" (separator, space)
	// LineSeparator           "Zl" (separator, line)
	// ParagraphSeparator      "Zp" (separator, paragraph)
	// Control                 "Cc" (other, control)
	// Format                  "Cf" (other, format)
	// Surrogate               "Cs" (other, surrogate)
	// PrivateUse              "Co" (other, private use)
	// ConnectorPunctuation    "Pc" (punctuation, connector)
	// DashPunctuation         "Pd" (punctuation, dash)
	// OpenPunctuation         "Ps" (punctuation, open)
	// ClosePunctuation        "Pe" (punctuation, close)
	// InitialQuotePunctuation "Pi" (punctuation, initial quote)
	// FinalQuotePunctuation   "Pf" (punctuation, final quote)
	// OtherPunctuation        "Po" (punctuation, other)
	// MathSymbol              "Sm" (symbol, math)
	// CurrencySymbol          "Sc" (symbol, currency)
	// ModifierSymbol          "Sk" (symbol, modifier)
	// OtherSymbol             "So" (symbol, other)
	// OtherNotAssigned        "Cn" (other, not assigned)
	public static bool IsWhiteSpace(this char ch)
	{
		return ch == ' '
	        || ch == '\t'
	        || ch == '\v'
	        || ch == '\f'
	        || ch == '\u00A0' // NO-BREAK SPACE
	        || ch == '\uFEFF'
	        || ch == '\u001A'
	        || (ch > 255 && CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator);
	}
	
	public static bool IsNewLine(this char ch)
	{
		return ch == '\r'
		    || ch == '\n'
		    || ch == '\u0085'
		    || ch == '\u2028'
		    || ch == '\u2029';
	}
	
	public static bool IsIdentifierStartCharacter(this char ch)
	{
		return UnicodeCharacterInfo.IsIdentifierStartCharacter(ch);
	}
	
	public static bool IsIdentifierPartCharacter(this char ch)
	{
		return UnicodeCharacterInfo.IsIdentifierStartCharacter(ch);
	}
	
	public static bool IsValidIdentifier([NotNullWhen(true)] this string? name)
	{
		return UnicodeCharacterInfo.IsValidIdentifier(name);
	}
	
	public static bool ContainsDroppedIdentifierCharacters(this string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}

		if (name[0] == '@')
		{
			return true;
		}

		var nameLength = name.Length;
		for (var i = 0; i < nameLength; i++)
		{
			if (UnicodeCharacterInfo.IsFormattingChar(name[i]))
			{
				return true;
			}
		}

		return false;
	}
	
	public static bool IsNonAsciiQuotationMark(this char ch)
	{
		// CONSIDER: There are others:
		// http://en.wikipedia.org/wiki/Quotation_mark_glyphs#Quotation_marks_in_Unicode
		switch (ch)
		{
			case '\u2018': // left single quotation mark
			case '\u2019': // right single quotation mark
				return true;
			case '\u201C': // left double quotation mark
			case '\u201D': // right double quotation mark
				return true;
			default:
				return false;
		}
	}
}