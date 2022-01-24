namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
	public UnaryExpressionSyntax(in SyntaxToken<string> operatorToken, 
	                             in ExpressionSyntax operand)
	{
		Operator = operatorToken;
		Operand = operand;
	}

	public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
	public SyntaxToken<string> Operator { get; }
	public ExpressionSyntax Operand { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Operator;
		yield return Operand;
	}
}