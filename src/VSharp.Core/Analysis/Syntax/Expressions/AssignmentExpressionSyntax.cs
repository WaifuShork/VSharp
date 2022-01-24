namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
	public AssignmentExpressionSyntax(in SyntaxToken<Identifier> identifier, 
	                                  in SyntaxToken<Any> equalsToken, 
	                                  in ExpressionSyntax expression)
	{
		Identifier = identifier;
		EqualsToken = equalsToken;
		Expression = expression;
	}
	
	public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string> EqualsToken { get; }
	public ExpressionSyntax Expression { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Identifier;
		yield return EqualsToken;
		yield return Expression;
	}
}