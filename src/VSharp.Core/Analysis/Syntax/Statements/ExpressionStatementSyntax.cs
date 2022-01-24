namespace VSharp.Core.Analysis.Syntax.Statements;

[PublicAPI]
public sealed class ExpressionStatementSyntax : StatementSyntax
{
	public ExpressionStatementSyntax(ExpressionSyntax expression)
	{
		Expression = expression;
	}

	public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
	public ExpressionSyntax Expression { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Expression;
	}
}