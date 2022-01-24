namespace VSharp.Core.Analysis.Syntax.Constraints;

[PublicAPI]
public sealed class MethodConstraintSyntax : ConstraintSyntax
{
	public MethodConstraintSyntax(in SyntaxToken<Identifier> identifier,
	                              in SyntaxToken<Any> dotToken,
	                              in SyntaxToken<Identifier> methodName,
	                              in SyntaxToken<Any> openParenToken,
	                              in IReadOnlyList<SyntaxToken<Identifier>> parameters,
	                              in SyntaxToken<Any> closeParenToken)
	{
		Identifier = identifier;
		DotToken = dotToken;
		MethodName = methodName;
		OpenParenToken = openParenToken;
		Parameters = parameters;
		CloseParenToken = closeParenToken;
	}

	public override SyntaxKind Kind => SyntaxKind.MethodConstraint;
	public SyntaxToken<Identifier> Identifier { get; }
	public SyntaxToken<Any> DotToken { get; }
	public SyntaxToken<Identifier> MethodName { get; }
	public SyntaxToken<Any> OpenParenToken { get; }
	public IReadOnlyList<SyntaxToken<Identifier>> Parameters { get; }
	public SyntaxToken<Any> CloseParenToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Identifier;
		yield return DotToken;
		yield return MethodName;
		yield return OpenParenToken;
		foreach (var parameter in Parameters)
		{
			yield return parameter;
		}

		yield return CloseParenToken;
	}
}