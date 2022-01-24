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
		while ((char.IsLetterOrDigit(Current) || Current == '.' && char.IsDigit(Next)) && !IsAtEnd)
		{
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
		
		var text = GetFullSpan();
		if (!double.TryParse(text, out var value))
		{
			return CreateToken(SyntaxKind.None, text, text);
		}
		
		var trailing = ScanSyntaxTrivia(TriviaKind.Trailing);
		return CreateToken(leading, SyntaxKind.Float64LiteralToken, text, value, trailing);
	}
}