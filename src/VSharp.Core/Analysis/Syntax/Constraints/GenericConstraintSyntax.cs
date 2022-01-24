namespace VSharp.Core.Analysis.Syntax.Constraints;

[PublicAPI]
public sealed class GenericConstraintSyntax : SyntaxNode
{
	public GenericConstraintSyntax(in SyntaxToken<Keyword> whereKeyword,
	                               in SyntaxToken<Identifier> identifier, 
	                               in SyntaxToken<Any> colonToken,
	                               in IReadOnlyList<ConstraintSyntax> constraints)
	{
		WhereKeyword = whereKeyword;
		Identifier = identifier;
		ColonToken = colonToken;
		Constraints = constraints;
	}
	
	public override SyntaxKind Kind => SyntaxKind.GenericConstraint;
	public SyntaxToken<Keyword> WhereKeyword { get; }
	public SyntaxToken<Identifier> Identifier { get; }
	public SyntaxToken<Any> ColonToken { get; }
	public IReadOnlyList<ConstraintSyntax> Constraints { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return WhereKeyword;
		yield return Identifier;
		yield return ColonToken;
		foreach (var constraint in Constraints)
		{
			yield return constraint;
		}
	}
}