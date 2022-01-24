namespace VSharp.Core.Analysis.Syntax;

[PublicAPI]
public sealed class ParameterSyntax : SyntaxNode
{
	public ParameterSyntax(in SyntaxToken<string> type, 
	                       in SyntaxToken<string> identifier)
	{
		Type = type;
		Identifier = identifier;
	}

	public override SyntaxKind Kind => SyntaxKind.Parameter;
	public SyntaxToken<string> Type { get; }
	public SyntaxToken<string> Identifier { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Type;
		yield return Identifier;
	}
}