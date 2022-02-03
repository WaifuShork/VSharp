namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class ModuleDeclarationSyntax : MemberSyntax
{
	public ModuleDeclarationSyntax(SyntaxToken<Bracket> openBracketToken,
	                               ModifierListSyntax modifiers,
	                               SyntaxToken<Bracket> closeBracketToken,
	                               SyntaxToken<string> keyword, 
	                               SyntaxToken<string> identifier,
	                               SyntaxToken<string> semicolon,
	                               IReadOnlyList<MemberSyntax> members)
	{
		OpenBracketToken = openBracketToken;
		Modifiers = modifiers;
		CloseBracketToken = closeBracketToken;
		Keyword = keyword;
		Identifier = identifier;
		Semicolon = semicolon;
		Members = members;
	}
	
	public override SyntaxKind Kind => SyntaxKind.ModuleDeclaration;
	public SyntaxToken<Bracket> OpenBracketToken { get; }
	public ModifierListSyntax Modifiers { get; }
	public SyntaxToken<Bracket> CloseBracketToken { get; }
	public SyntaxToken<string> Keyword { get; }
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string> Semicolon { get; }
	public IReadOnlyList<MemberSyntax> Members { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenBracketToken;
		yield return Modifiers;
		yield return CloseBracketToken;
		yield return Keyword;
		yield return Identifier;
		yield return Semicolon;
		foreach (var member in Members)
		{
			yield return member;
		}
	}
}