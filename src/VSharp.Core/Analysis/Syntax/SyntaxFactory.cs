namespace VSharp.Core.Analysis.Syntax;

public static class SyntaxFactory
{
	public static SyntaxToken<int8> Literal(Lexer lexer, 
	                                        int8 value, 
	                                        string text, 
	                                        IReadOnlyList<SyntaxTrivia> leading, 
	                                        IReadOnlyList<SyntaxTrivia> trailing,
	                                        DiagnosticInfo? info)
	{
		return new SyntaxToken<int8>(leading, in SyntaxKind.Int8LiteralToken, text, value, lexer.TokenPosition, lexer.Line, trailing, info);
	}
}