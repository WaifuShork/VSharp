namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class BooleanLiteralExpressionSyntax : ExpressionSyntax
{
	public BooleanLiteralExpressionSyntax(in SyntaxToken<bool> boolToken)
	{
		BoolToken = boolToken;
	}

	public override SyntaxKind Kind => SyntaxKind.BoolLiteralExpression;
	public SyntaxToken<bool> BoolToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return BoolToken;
	}
}