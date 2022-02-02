using System.Numerics;
using FluentAssertions;

namespace VSharp.Core.Analysis.Lexing;

public partial class Lexer
{
	// TODO: everything with numbers, this is a lazy implementation for now
	private ISyntaxToken ScanNumericLiteral(in IReadOnlyList<SyntaxTrivia> leading)
	{
		m_start = m_position;
		var numberLength = 0;
		var isIdentifier = false;
		var isFloat = false;
		// var hasMultipleDecimal = false;
		
		while ((char.IsLetterOrDigit(Current) || Current == '.' && char.IsDigit(Next)) && NotAtEnd)
		{
			if (CurrentIs('.'))
			{
				isFloat = true;
			}
			if (Current.IsIdentifierPartCharacter() || Current.IsIdentifierPartCharacter())
			{
				isIdentifier = true;
				numberLength = m_position - m_start;
				break;
			}
			
			Advance();
		}

		if (isIdentifier)
		{
			return ScanIdentifierToken(leading, numberLength);
		}

		if (CurrentIs('!'))
		{
			Advance();
			var rawText = GetFullSpan();
			var trail = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
			var factText = rawText.Replace("!", "");
			if (!BigInteger.TryParse(factText, out var num))
			{
				return CreateToken(leading, in SyntaxKind.None, rawText, rawText, trail);
			}
			
			num = num.UncachedFactorial();
			return CreateToken(leading, in SyntaxKind.InfinityIntLiteralToken, rawText, num, trail);
		}

		var text = GetFullSpan();
		var trailing = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
		if (!double.TryParse(text, out var value))
		{
			return CreateToken(leading, in SyntaxKind.None, text, text, trailing);
		}
		
		return CreateToken(leading, in SyntaxKind.Float64LiteralToken, text, value, trailing);
	}
}