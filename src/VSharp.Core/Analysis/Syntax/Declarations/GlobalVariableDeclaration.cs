namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class GlobalVariableDeclaration : MemberSyntax
{
	public GlobalVariableDeclaration(in SyntaxToken<Bracket> openBracketToken,
	                                 in ModifierSyntaxList modifiers,
	                                 in SyntaxToken<Bracket> closeBracketToken,
	                                 in SyntaxToken<string> type,
	                                 in SyntaxToken<string> identifier,
	                                 in SyntaxToken<string>? equalsToken,
	                                 in ExpressionSyntax? initializer,
	                                 in SyntaxToken<Delimiter> semicolon)
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
	public ModifierSyntaxList Modifiers { get; }
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