namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class NameExpressionSyntax : ExpressionSyntax
{
	public NameExpressionSyntax(in SyntaxToken<string> identifierToken)
	{
		IdentifierToken = identifierToken;
	}

	public override SyntaxKind Kind => SyntaxKind.NameExpression;
	public SyntaxToken<string> IdentifierToken { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return IdentifierToken;
	}
}