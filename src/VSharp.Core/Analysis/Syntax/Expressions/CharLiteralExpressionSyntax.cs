namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class CharLiteralExpressionSyntax : ExpressionSyntax
{
	public CharLiteralExpressionSyntax(in SyntaxToken<char> charToken)
	{
		CharToken = charToken;
	}
	
	public override SyntaxKind Kind => SyntaxKind.CharLiteralExpression;
	public SyntaxToken<char> CharToken { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return CharToken;
	}
}