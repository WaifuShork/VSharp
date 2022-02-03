namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class NameExpressionSyntax : ExpressionSyntax
{
	public NameExpressionSyntax(SyntaxToken<Identifier> identifierToken)
	{
		IdentifierToken = identifierToken;
	}

	public override SyntaxKind Kind => SyntaxKind.NameExpression;
	public SyntaxToken<Identifier> IdentifierToken { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return IdentifierToken;
	}
}