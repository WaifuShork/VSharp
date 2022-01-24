using FluentAssertions;

namespace VSharp.Core.Analysis.Lexing;

using System.Text;
using Microsoft.Toolkit.Diagnostics;

public partial class Lexer
{
	private ISyntaxToken ScanStringLiteral(char quoteChar, in IReadOnlyList<SyntaxTrivia> leadingTrivia)
	{
		quoteChar.Should().Be('"', "quote character was not \"");
		
		m_start = m_position;
		Advance();
		
		var sb = new StringBuilder();
		var done = false;
		
		while (!done && !IsAtEnd)
		{
			switch (Current)
			{
				case '\\':
					sb.Append(ScanEscapeSequence());
					break;
				case '\r':
				case '\n':
				case '\0':
				case var current when current is CharacterInfo.InvalidCharacter || current.IsNewLine() || IsAtEnd:
					var span = new TextSpan(m_start, 1);
					var location = new TextLocation(m_source, span);
					m_diagnostics.ReportUnterminatedString(location, out _);
					done = true;
					break;
				case '"':
					Advance();
					done = true;
					break;
				default:
					sb.Append(Current);
					Advance();
					break;
			}
		}

		var text = sb.ToString();
		var trailing = ScanSyntaxTrivia(SyntaxKind.TrailingTrivia);
		return CreateToken(leadingTrivia, SyntaxKind.StringLiteralToken, text, text, trailing);
	}

	private ISyntaxToken ScanCharLiteral(char quoteChar, in IReadOnlyList<SyntaxTrivia> leadingTrivia)
	{
		quoteChar.Should().Be('\'', "quote character was not '");
		m_start = m_position;
		Advance();
		
		var sb = new StringBuilder();
		var done = false;
		var escaped = false;

		TextSpan span;
		TextLocation location;
		
		while (!done && !IsAtEnd)
		{
			switch (Current)
			{
				case '\\':
					escaped = true;
					sb.Append(ScanEscapeSequence());
					break;
				case '\r':
				case '\n':
				case '\0':
				case var current when current is CharacterInfo.InvalidCharacter || current.IsNewLine() || IsAtEnd:
					span = new TextSpan(m_start, 1);
					location = new TextLocation(m_source, span);
					m_diagnostics.ReportUnterminatedCharacterConstant(location, out _);
					done = true;
					break;
				case '\'':
					Advance();
					done = true;
					break;
				default:
					sb.Append(Current);
					Advance();
					break;
			}
		}
		
		span = new TextSpan(m_start, 1);
		location = new TextLocation(m_source, span);

		var text = sb.ToString();
		if (text.Length == 0)
		{
			m_diagnostics.ReportEmptyCharConst(location, out _);
		}
		if (text.Length > 1 && !escaped)
		{
			m_diagnostics.ReportInvalidCharacterConst(location, out _);
		}
		
		var trailing = ScanSyntaxTrivia(SyntaxKind.TrailingTrivia);
		var codePointHex = text[0].ToHexString();
		return CreateToken(leadingTrivia, SyntaxKind.CharLiteralToken, codePointHex, text[0], trailing);
	}

	private char ScanEscapeSequence()
	{
		Current.Should().Be('\\', "current was not \\");
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
				ch = '\u000C';
				break;
			case 'n':
				ch = '\u000A';
				break;
			case 'r':
				ch = '\u000D';
				break;
			case 't':
				ch = '\u0009';
				break;
			case 'v':
				ch = '\u000B';
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