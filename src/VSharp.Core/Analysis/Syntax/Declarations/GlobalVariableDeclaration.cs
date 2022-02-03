namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class GlobalVariableDeclaration : MemberSyntax
{
	public GlobalVariableDeclaration(SyntaxToken<Bracket> openBracketToken,
	                                 ModifierListSyntax modifiers,
	                                 SyntaxToken<Bracket> closeBracketToken,
	                                 SyntaxToken<string> type,
	                                 SyntaxToken<string> identifier,
	                                 SyntaxToken<string>? equalsToken,
	                                 ExpressionSyntax? initializer,
	                                 SyntaxToken<Delimiter> semicolon)
	{
		OpenBracketToken = openBracketToken;
		Modifiers = modifiers;
		CloseBracketToken = closeBracketToken;
		Type = type;
		Identifier = identifier;
		EqualsToken = equalsToken;
		Initializer = initializer;
		Semicolon = semicolon;
	}
	
	public override SyntaxKind Kind => SyntaxKind.GlobalVariableDeclaration;
	public SyntaxToken<Bracket> OpenBracketToken { get; }
	public ModifierListSyntax Modifiers { get; }
	public SyntaxToken<Bracket> CloseBracketToken { get; }
	public SyntaxToken<string> Type { get; }
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string>? EqualsToken { get; }
	public ExpressionSyntax? Initializer { get; }
	public SyntaxToken<Delimiter> Semicolon { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenBracketToken;
		yield return Modifiers;
		yield return CloseBracketToken;
		yield return Type;
		yield return Identifier;
		if (EqualsToken is not null)
		{
			yield return EqualsToken;
		}
		if (Initializer is not null)
		{
			yield return Initializer;
		}

		yield return Semicolon;
	}
}