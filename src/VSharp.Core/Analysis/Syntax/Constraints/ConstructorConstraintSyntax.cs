namespace VSharp.Core.Analysis.Syntax.Constraints;

[PublicAPI]
public sealed class ConstructorConstraintSyntax : ConstraintSyntax
{
	public ConstructorConstraintSyntax(in SyntaxToken<Keyword> ctorKeyword,
	                                   in SyntaxToken<string> openParenToken, 
	                                   in IReadOnlyList<SyntaxToken<Identifier>> parameters, 
	                                   in SyntaxToken<string> closeParenToken)
	{
		ConstructorKeyword = ctorKeyword;
		OpenParenToken = openParenToken;
		Parameters = parameters;
		CloseParenToken = closeParenToken;
	}
	
	public override SyntaxKind Kind => SyntaxKind.ConstructorConstraint;
	public SyntaxToken<Keyword> ConstructorKeyword { get; }
	public SyntaxToken<Any> OpenParenToken { get; }
	public IReadOnlyList<SyntaxToken<Identifier>> Parameters { get; }
	public SyntaxToken<Any> CloseParenToken { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return ConstructorKeyword;

		yield return OpenParenToken;
		foreach (var parameter in Parameters)
		{
			yield return parameter;
		}
		yield return CloseParenToken;
	}
}