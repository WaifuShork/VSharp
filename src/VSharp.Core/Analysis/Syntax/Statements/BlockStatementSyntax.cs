namespace VSharp.Core.Analysis.Syntax.Statements;

[PublicAPI]
public sealed class BlockStatementSyntax : StatementSyntax
{
	public BlockStatementSyntax(SyntaxToken<string> openBraceToken, 
	                            SyntaxList<StatementSyntax> statements, 
	                            SyntaxToken<string> closeBraceToken)
	{
		OpenBraceToken = openBraceToken;
		Statements = statements;
		CloseBraceToken = closeBraceToken;
	}

	public override SyntaxKind Kind => SyntaxKind.BlockStatement;
	public SyntaxToken<string> OpenBraceToken { get; }
	public SyntaxList<StatementSyntax> Statements { get; }
	public SyntaxToken<string> CloseBraceToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenBraceToken;
		foreach (var statement in Statements)
		{
			yield return statement;
		}

		yield return CloseBraceToken;
	}
}

[PublicAPI]
public sealed class EnumBlockStatementSyntax : StatementSyntax
{
	public EnumBlockStatementSyntax(SyntaxToken<string> openBraceToken, 
	                                SyntaxList<EnumFieldDeclarationSyntax> fields, 
	                                SyntaxToken<string> closeBraceToken)
	{
		OpenBraceToken = openBraceToken;
		Fields = fields;
		CloseBraceToken = closeBraceToken;
	}

	public override SyntaxKind Kind => SyntaxKind.BlockStatement;
	public SyntaxToken<string> OpenBraceToken { get; }
	public SyntaxList<EnumFieldDeclarationSyntax> Fields { get; }
	public SyntaxToken<string> CloseBraceToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return OpenBraceToken;
		foreach (var statement in Fields)
		{
			yield return statement;
		}

		yield return CloseBraceToken;
	}
}