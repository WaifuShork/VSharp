namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class EnumDeclarationSyntax : MemberSyntax
{
	public EnumDeclarationSyntax(SyntaxToken<Bracket> openBracketToken,
	                             ModifierSyntaxList modifiers,
	                             SyntaxToken<Bracket> closeBracketToken,
	                             SyntaxToken<Keyword> keyword,
	                             SyntaxToken<Identifier> identifier,
	                             EnumBlockStatementSyntax block)
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