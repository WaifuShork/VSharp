namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public class BinaryExpressionSyntax : ExpressionSyntax
{
	public BinaryExpressionSyntax(ExpressionSyntax left, 
	                              SyntaxToken<string> operatorToken,
	                              ExpressionSyntax right)
	{
		Left = left;
		Operator = operatorToken;
		Right = right;
	}

	public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

	public ExpressionSyntax Left { get; }
	public SyntaxToken<string> Operator { get; }
	public ExpressionSyntax Right { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Left;
		yield return Operator;
		yield return Right;
	}
}