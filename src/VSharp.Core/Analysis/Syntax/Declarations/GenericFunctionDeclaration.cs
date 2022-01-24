namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class GenericFunctionDeclaration : MemberSyntax
{
	public GenericFunctionDeclaration(SyntaxToken<Any> atToken,
	                                  SyntaxToken<Keyword> genericKeyword,
	                                  SyntaxToken<Bracket> lessToken,
	                                  SyntaxToken<Any> genericType,
	                                  SyntaxToken<Bracket> greaterToken,
	                                  GenericConstraintSyntax? constraint,
	                                  SyntaxToken<Bracket> openBracketToken,
	                                  ModifierSyntaxList modifiers,
	                                  SyntaxToken<Bracket> closeBracketToken,
	                                  SyntaxToken<Identifier> identifier,
	                                  SyntaxToken<Bracket> openParenToken,
	                                  ParameterSyntaxList parameters,
	                                  SyntaxToken<Bracket> closeParenToken,	                                  
	                                  SyntaxToken<Any> arrowToken,
	                                  SyntaxToken<Any> returnType,
	                                  BlockStatementSyntax block)
	{
		AtToken = atToken;
		GenericKeyword = genericKeyword;
		LessToken = lessToken;
		GenericType = genericType;
		GreaterToken = greaterToken;
		Constraint = constraint;
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

	public override SyntaxKind Kind => SyntaxKind.GenericFunctionDeclaration;
	
	public SyntaxToken<Any> AtToken { get; }
	public SyntaxToken<Keyword> GenericKeyword { get; }
	public SyntaxToken<Bracket> LessToken { get; }
 	public SyntaxToken<Any> GenericType { get; }
    public SyntaxToken<Bracket> GreaterToken { get; }
    public GenericConstraintSyntax? Constraint { get; }
	public SyntaxToken<Bracket> OpenBracketToken { get; }
	public ModifierSyntaxList Modifiers { get; }
	public SyntaxToken<Bracket> CloseBracketToken { get; }
	public SyntaxToken<Identifier> Identifier { get; }
	public SyntaxToken<Bracket> OpenParenToken { get; }
	public ParameterSyntaxList Parameters { get; }
	public SyntaxToken<Bracket> CloseParenToken { get; }
	public SyntaxToken<Any> ArrowToken { get; }
	public SyntaxToken<Any> ReturnType { get; }
	public BlockStatementSyntax Block { get; }
	public bool IsMutable { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return AtToken;
		yield return GenericKeyword;
		yield return LessToken;
		yield return GenericType;
		yield return GreaterToken;
		if (Constraint is not null)
		{
			yield return Constraint;
		}
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