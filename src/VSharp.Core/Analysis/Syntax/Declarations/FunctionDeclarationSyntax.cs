namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class FunctionDeclarationSyntax : MemberSyntax
{
	// [public, static] Identifier(parameters) -> returnType { }
	// [public, immutable] Identifier(parameters) -> returnType { }
	// [public, mutable] Identifier(parameters) -> returnType { }
	public FunctionDeclarationSyntax(SyntaxToken<Bracket> openBracketToken,
	                                 ModifierListSyntax modifiers,
	                                 SyntaxToken<Bracket> closeBracketToken,
	                                 SyntaxToken<string> identifier,
	                                 SyntaxToken<Bracket> openParenToken,
	                                 ParameterListSyntax parameters,
	                                 SyntaxToken<Bracket> closeParenToken,
	                                 SyntaxToken<string> arrowToken,
	                                 SyntaxToken<string> returnType,
	                                 BlockStatementSyntax block)
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
	public ModifierListSyntax Modifiers { get; }
	public SyntaxToken<Bracket> CloseBracketToken { get; }	
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string> OpenParenToken { get; }
	public ParameterListSyntax Parameters { get; }
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