namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class PostfixExpression : ExpressionSyntax
{
	public PostfixExpression(ExpressionSyntax expression, SyntaxToken<Operator> op)
	{
		Expression = expression;
		Operator = op;
	}

	public override SyntaxKind Kind => SyntaxKind.PostfixExpression;
	public ExpressionSyntax Expression { get; }
	public SyntaxToken<Operator> Operator { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Expression;
		yield return Operator;
	}
}