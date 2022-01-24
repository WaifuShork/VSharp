namespace VSharp.Core.Analysis.Syntax.Errors;

[PublicAPI]
public sealed class ErrorExpressionNode : ExpressionSyntax
{
	public ErrorExpressionNode(in DiagnosticInfo diagnostic)
	{
		Diagnostic = diagnostic;
	}
	
	public override SyntaxKind Kind => SyntaxKind.BadToken;
	public DiagnosticInfo Diagnostic { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		return Default;
	}
}