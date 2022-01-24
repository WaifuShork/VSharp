using System.Collections;

namespace VSharp.Core.Analysis.Syntax;

[PublicAPI]
public sealed class ModifierSyntaxList : SyntaxNode, IEnumerable<SyntaxToken<Identifier>>
{
	public ModifierSyntaxList(in IReadOnlyList<SyntaxToken<Modifier>> modifiers)
	{
		Modifiers = modifiers;
	}
	
	public override SyntaxKind Kind => SyntaxKind.ModifierList;
	public IReadOnlyList<SyntaxToken<Modifier>> Modifiers { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		return Modifiers;
	}

	public IEnumerator<SyntaxToken<string>> GetEnumerator()
	{
		return Modifiers.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}