namespace VSharp.Core.Analysis.Syntax.Statements;

[PublicAPI]
public sealed class VariableDeclarationSyntax : StatementSyntax
{
	public VariableDeclarationSyntax(SyntaxToken<string>? mutabilityKeyword,
									 SyntaxToken<string> keyword, 
	                                 SyntaxToken<string> identifier, 
	                                 SyntaxToken<string>? equalsToken, 
	                                 ExpressionSyntax? initializer, 
	                                 SyntaxToken<Delimiter> semicolon)
	{
		MutabilityKeyword = mutabilityKeyword;
		Keyword = keyword;
		Identifier = identifier;
		EqualsToken = equalsToken;
		Initializer = initializer;
		Semicolon = semicolon;
	}
	
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
	public SyntaxToken<string>? MutabilityKeyword { get; }
	public SyntaxToken<string> Keyword { get; }
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string>? EqualsToken { get; }
	public ExpressionSyntax? Initializer { get; }
	public SyntaxToken<Delimiter> Semicolon { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		if (MutabilityKeyword is not null)
		{
			yield return MutabilityKeyword;
		}
		yield return Keyword;
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