using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using VSharp.Core.Analysis.Syntax;
using VSharp.Core.Analysis.Text;

namespace VSharp.Core.Analysis.Lexing;

public partial class Lexer
{
	private void ScanStringOrCharLiteral()
    {
        var quoteChar = Current;
        Debug.Assert(quoteChar is '\'' or '"', "quoteChar == '\'' || quoteChar == '''");
        Advance();
        if (quoteChar == '\'')
        {
	        ScanCharLiteral(quoteChar);
        }
        else if (quoteChar == '"')
        {
	        ScanStringLiteral(quoteChar);
        }
    }

	private void ScanStringLiteral(char quoteChar)
	{
		var sb = new StringBuilder();
		var done = false;
		while (!done && !IsAtEnd)
		{
			switch (Current)
			{
				case '\0':
				case '\r':
				case '\n':
				case InvalidCharacter:
					var span = new TextSpan(m_start, 1);
					var location = new TextLocation(m_source, span);
					m_diagnostics.ReportUnterminatedString(location);
					done = true;
					break;
				case '"':
					if (Next == quoteChar)
					{
						sb.Append(Current);
						Advance(2);
					}
					else
					{
						Advance();
						done = true;
					}

					break;
				default:
					sb.Append(Current);
					Advance();
					break;
			}
		}

		m_kind = SyntaxKind.StringLiteralToken;
		m_value = sb.ToString();
	}

	private void ScanCharLiteral(char quoteChar)
	{
		var sb = new StringBuilder();
		var done = false;
		while (!done && !IsAtEnd)
		{
			switch (Current)
			{
				case '\\':
					sb.Append(ScanEscapeSequence());
					break;
				case InvalidCharacter:
					Console.Error.WriteLine("unterminated character literal");
					done = true;
					break;
				case '\'':
					if (Next == quoteChar)
					{
						sb.Append(Current);
						Advance(2);
					}
					else
					{
						Advance();
						done = true;
					}

					break;
				default:
					sb.Append(Current);
					Advance();
					break;
			}
		}

		m_kind = SyntaxKind.CharLiteralToken;

		var span = new TextSpan(m_start, 1);
		var location = new TextLocation(m_source, span);

		// Console.WriteLine(sb);
		
		if (sb.Length == 0)
		{
			m_diagnostics.ReportEmptyCharConst(location);
			return;
		}
		if (sb.Length > 1)
		{
			m_diagnostics.ReportInvalidCharacterConst(location);
			return;
		}
		
		m_value = sb[0];
	}

	private char ScanEscapeSequence()
	{
		Debug.Assert(Current == '\\', "Current == '\\'");
		Advance();
		var ch = Current;
		switch (Current)
		{
			case '\'':
			case '\"':
			case '\\':
				break;
			case '0':
				ch = '\u0000';
				break;
			case 'a':
				ch = '\u0007';
				break;
			case 'b':
				ch = '\u0008';
				break;
			case 'f':
				ch = '\u000c';
				break;
			case 'n':
				ch = '\u000a';
				break;
			case 'r':
				ch = '\u000d';
				break;
			case 't':
				ch = '\u0009';
				break;
			case 'v':
				ch = '\u000b';
				break;
			case 'x':
			case 'u':
			{
				Advance();
				var ticker = 0;
				var intChar = 0;
				while (Current.IsHexDigit() && ticker < 4)
				{
					intChar = (intChar << 4) + Current.HexValue();
					Advance();
					ticker++;
				}

				ch = (char)intChar;
				break;
			}
			case 'U':
			{
				Advance();
				var ticker = 0;
				var uintChar = (uint) 0;
				while (Current.IsHexDigit() && ticker < 8)
				{
					uintChar = (uint)((uintChar << 8) + Current.HexValue());
					Advance();
					ticker++;
				}

				ch = (char)uintChar;
				break;
			}
		}

		return ch;
	}
}