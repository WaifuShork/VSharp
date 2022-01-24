namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	public ParenthesizedExpressionSyntax(SyntaxToken<string> openParenToken, ExpressionSyntax expression, SyntaxToken<string> closeParenToken)
	{
		OpenParenToken = openParenToken;
		Expression = expression;
		CloseParenToken = closeParenToken;
	}

	public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
	public SyntaxToken<string> OpenParenToken { get; }
	public ExpressionSyntax Expression { get; }
	public SyntaxToken<string> CloseParenToken { get; }
	
	public override  IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenParenToken;
		yield return Expression;
		yield return CloseParenToken;
	}
}