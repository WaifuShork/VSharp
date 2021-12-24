namespace VSharp.Core.Analysis.Syntax;

public static class SyntaxFacts
{
	public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
	{
		switch (kind)
		{
			case SyntaxKind.PlusToken:
			case SyntaxKind.MinusMinusToken:
			case SyntaxKind.BangToken:
				return 6;
			default:
				return 0;
		}
	}

	public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
	{
		switch (kind)
		{
			case SyntaxKind.AsteriskToken:
			case SyntaxKind.FSlashToken:
				return 5;
			case SyntaxKind.PlusToken:
			case SyntaxKind.MinusToken:
				return 4;
			case SyntaxKind.EqualsEqualsToken:
			case SyntaxKind.BangEqualsToken:
				return 3;
			case SyntaxKind.AmpersandAmpersandToken:
				return 2;
			case SyntaxKind.PipePipeToken:
				return 1;
			default:
				return 0;
		}
	}
}