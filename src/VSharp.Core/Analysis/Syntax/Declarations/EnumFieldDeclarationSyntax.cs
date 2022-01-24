namespace VSharp.Core.Analysis.Syntax.Declarations;

[PublicAPI]
public sealed class EnumFieldDeclarationSyntax : StatementSyntax 
{ 
	public EnumFieldDeclarationSyntax(in SyntaxToken<Identifier> identifier, 
	                                  in SyntaxToken<Any>? equalsToken, 
	                                  in ExpressionSyntax? expression)
	{
		Identifier = identifier;
		EqualsToken = equalsToken;
		Expression = expression;
	}
	
	public override SyntaxKind Kind => SyntaxKind.EnumFieldDeclaration;
	public SyntaxToken<string> Identifier { get; }
	public SyntaxToken<string>? EqualsToken { get; }
	public ExpressionSyntax? Expression { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Identifier;
		if (EqualsToken is not null)
		{
			yield return EqualsToken;
		}

		if (Expression is not null)
		{
			yield return Expression;
		}
	}
}