namespace VSharp.Core.Analysis.Syntax.Constraints;

[PublicAPI]
public sealed class TypeConstraintSyntax : ConstraintSyntax
{
	public TypeConstraintSyntax(in SyntaxToken<Identifier> identifier)
	{
		Identifier = identifier;
	}
	
	public override SyntaxKind Kind => SyntaxKind.TypeConstraint;
	public SyntaxToken<Identifier> Identifier { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Identifier;
	}
}