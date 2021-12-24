using VSharp.Core.Analysis.Syntax;

namespace VSharp.Core.Analysis.Lexing;

public partial class Lexer
{
	// TODO: everything with numbers, this is a lazy implementation for now
	private void ScanNumericLiteral()
	{
		while ((char.IsDigit(Current) || Current == '.' && char.IsDigit(Next)) && !IsAtEnd)
		{
			Advance();
		}

		var text = GetFullSpan();
		m_value = double.Parse(text);
		m_kind = SyntaxKind.Float64LiteralToken;
	}
}