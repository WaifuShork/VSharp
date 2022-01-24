namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class StringLiteralExpressionSyntax : ExpressionSyntax
{
	public StringLiteralExpressionSyntax(SyntaxToken<string> str)
	{
		StringToken = str;
	}

	public override SyntaxKind Kind => SyntaxKind.StringLiteralExpression;
	public SyntaxToken<string> StringToken { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return StringToken;
	}
}