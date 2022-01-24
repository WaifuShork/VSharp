namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class FunctionDeclarationSyntax : MemberSyntax
{
	// [public, static] Identifier(parameters) -> returnType { }
	// [public, immutable] Identifier(parameters) -> returnType { }
	// [public, mutable] Identifier(parameters) -> returnType { }
	public FunctionDeclarationSyntax(in SyntaxToken<Bracket> openBracketToken,
	                                 in ModifierSyntaxList modifiers,
	                                 in SyntaxToken<Bracket> closeBracketToken,
	                                 in SyntaxToken<string> identifier,
	                                 in SyntaxToken<Bracket> openParenToken,
	                                 in ParameterSyntaxList parameters,
	                                 in SyntaxToken<Bracket> closeParenToken,
	                                 in SyntaxToken<string> arrowToken,
	                                 in SyntaxToken<string> returnType,
	                                 in BlockStatementSyntax block)
	{
		OpenBracketToken = openBracketToken;
		Modifiers = modifiers;
		CloseBracketToken = closeBracketToken;
		Identifier = identifier;
		OpenParenToken = openParenToken;
		Parameters = parameters;
		CloseParenToken = closeParenToken;
		ArrowToken = arrowToken;
		ReturnType = returnType;
		Block = block;
	}
	
	public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
	public SyntaxToken<Bracket> OpenBracketToken { get; }
	public ModifierSyntaxList Modifiers { get; }
	public SyntaxToken<Bracket> CloseBracketToken { get; }	
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string> OpenParenToken { get; }
	public ParameterSyntaxList Parameters { get; }
	public SyntaxToken<string> CloseParenToken { get; }
	public SyntaxToken<string> ArrowToken { get; }
	public SyntaxToken<string> ReturnType { get; }
	public BlockStatementSyntax Block { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenBracketToken;
		yield return Modifiers;
		yield return CloseBracketToken;
		yield return Identifier;
		yield return OpenParenToken;
		yield return Parameters;
		yield return CloseParenToken;
		yield return ArrowToken;
		yield return ReturnType;
		yield return Block;
	}
}