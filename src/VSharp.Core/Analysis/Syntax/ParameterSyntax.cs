namespace VSharp.Core.Analysis.Syntax;

[PublicAPI]
public sealed class ParameterSyntax : SyntaxNode
{
	public ParameterSyntax(SyntaxToken<UserType> type, 
	                       SyntaxToken<Identifier> identifier)
	{
		Type = type;
		Identifier = identifier;
	}

	public override SyntaxKind Kind => SyntaxKind.Parameter;
	public SyntaxToken<UserType> Type { get; }
	public SyntaxToken<Identifier> Identifier { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Type;
		yield return Identifier;
	}
}