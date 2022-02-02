namespace VSharp.Core.Analysis.Syntax.Expressions;

public sealed class ConstructorCallExpressionSyntax : ExpressionSyntax
{
	public ConstructorCallExpressionSyntax(SyntaxToken<Keyword> allocKeyword,
	                                       SyntaxToken<Any>? colonToken,
	                                       SyntaxToken<Keyword>? allocator,
	                                       SyntaxToken<Identifier> type,
	                                       SyntaxToken<Bracket> openParenToken, 
	                                       SyntaxList<SyntaxToken<Identifier>> arguments, 
	                                       SyntaxToken<Bracket> closeParenToken)
	{
		AllocKeyword = allocKeyword;
		ColonToken = colonToken;
		Allocator = allocator;
		Type = type;
		OpenParenToken = openParenToken;
		Arguments = arguments;
		CloseParenToken = closeParenToken;
	}
	
	public override SyntaxKind Kind => SyntaxKind.ConstructorCallExpression;
	public SyntaxToken<string> AllocKeyword { get; }
	public SyntaxToken<string>? ColonToken { get; }
	public SyntaxToken<string>? Allocator { get; }
	public SyntaxToken<string> Type { get; }
	public SyntaxToken<string> OpenParenToken { get; }
	public SyntaxList<SyntaxToken<Identifier>> Arguments { get; }
	public SyntaxToken<string> CloseParenToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return AllocKeyword;
		if (ColonToken is not null)
		{
			yield return ColonToken;
		}

		if (Allocator is not null)
		{
			yield return Allocator;
		}

		yield return Type;
		yield return OpenParenToken;
		foreach (var argument in Arguments)
		{
			yield return argument;
		}
		yield return CloseParenToken;
	}
}