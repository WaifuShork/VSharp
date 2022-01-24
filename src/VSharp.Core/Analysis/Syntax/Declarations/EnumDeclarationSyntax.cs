namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class EnumDeclarationSyntax : MemberSyntax
{
	public EnumDeclarationSyntax(in SyntaxToken<Bracket> openBracketToken,
	                             in ModifierSyntaxList modifiers,
	                             in SyntaxToken<Bracket> closeBracketToken,
	                             in SyntaxToken<Keyword> keyword,
	                             in SyntaxToken<Identifier> identifier,
	                             in EnumBlockStatementSyntax block)
	{
		OpenBracketToken = openBracketToken;
		Modifiers = modifiers;
		CloseBracketToken = closeBracketToken;
		Keyword = keyword;
		Identifier = identifier;
		Block = block;
	}
	
	public override SyntaxKind Kind => SyntaxKind.EnumDeclaration;
	public SyntaxToken<Bracket> OpenBracketToken { get; }
	public ModifierSyntaxList Modifiers { get; }
	public SyntaxToken<Bracket> CloseBracketToken { get; }
	public SyntaxToken<string> Keyword { get; }
	public SyntaxToken<string> Identifier { get; }
	public EnumBlockStatementSyntax Block { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenBracketToken;
		yield return Modifiers;
		yield return CloseBracketToken;
		yield return Identifier;
		yield return Block;
	}
}