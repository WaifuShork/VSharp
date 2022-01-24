namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class ClassDeclarationSyntax : MemberSyntax
{
	public ClassDeclarationSyntax(SyntaxToken<Bracket> openBracketToken,
								  ModifierSyntaxList modifiers,
								  SyntaxToken<Bracket> closeBracketToken,
	                              SyntaxToken<Keyword> keyword,
	                              SyntaxToken<Identifier> identifier,
	                              BlockStatementSyntax block)
	{
		OpenBracketToken = openBracketToken;
		Modifiers = modifiers;
		CloseBracketToken = closeBracketToken;
		Keyword = keyword;
		Identifier = identifier;
		Block = block;
	}
	
	public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;
	public SyntaxToken<Bracket> OpenBracketToken { get; }
	public ModifierSyntaxList Modifiers { get; }
	public SyntaxToken<Bracket> CloseBracketToken { get; }
	public SyntaxToken<string> Keyword { get; }
	public SyntaxToken<string> Identifier { get; }
	public BlockStatementSyntax Block { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenBracketToken;
		yield return Modifiers;
		yield return CloseBracketToken;
		yield return Keyword;
		yield return Identifier;
		yield return Block;
	}
}