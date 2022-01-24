namespace VSharp.Core.Analysis.Syntax.Statements;

[PublicAPI]
public sealed class VariableDeclarationSyntax : StatementSyntax
{
	public VariableDeclarationSyntax(SyntaxToken<string> keyword, 
	                                 SyntaxToken<string> identifier, 
	                                 SyntaxToken<string>? equalsToken, 
	                                 ExpressionSyntax? initializer, 
	                                 SyntaxToken<Delimiter> semicolon,
	                                 bool isMutable)
	{
		Keyword = keyword;
		Identifier = identifier;
		EqualsToken = equalsToken;
		Initializer = initializer;
		Semicolon = semicolon;
		IsMutable = isMutable;
	}
	
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
	public SyntaxToken<string> Keyword { get; }
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string>? EqualsToken { get; }
	public ExpressionSyntax? Initializer { get; }
	public SyntaxToken<Delimiter> Semicolon { get; }
	public bool IsMutable { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
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