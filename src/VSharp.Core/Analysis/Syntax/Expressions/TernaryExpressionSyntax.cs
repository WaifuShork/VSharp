namespace VSharp.Core.Analysis.Syntax.Expressions;

public sealed class TernaryExpressionSyntax : ExpressionSyntax
{
	public TernaryExpressionSyntax(in ExpressionSyntax condition,
	                               in SyntaxToken<Any> questionMarkToken,
	                               in ExpressionSyntax consequent, 
	                               in SyntaxToken<Any> colonToken,
	                               in ExpressionSyntax alternative)
	{
		Condition = condition;
		QuestionMarkToken = questionMarkToken;
		Consequent = consequent;
		ColonToken = colonToken;
		Alternative = alternative;
	}

	public override SyntaxKind Kind => SyntaxKind.TernaryExpression;
	public ExpressionSyntax Condition { get; }
	public SyntaxToken<string> QuestionMarkToken { get; }
	public ExpressionSyntax Consequent { get; }
	public SyntaxToken<string> ColonToken { get; }
	public ExpressionSyntax Alternative { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Condition;
		yield return QuestionMarkToken;
		yield return Consequent;
		yield return ColonToken;
		yield return Alternative;
	}
}