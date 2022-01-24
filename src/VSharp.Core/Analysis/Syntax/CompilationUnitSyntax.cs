namespace VSharp.Core.Analysis.Syntax;

[PublicAPI]
public class CompilationUnitSyntax : SyntaxNode
{
	public CompilationUnitSyntax(ModuleDeclarationSyntax module, SyntaxToken<Delimiter> endOfFileToken)
	{
		Module = module;
		EndOfFileToken = endOfFileToken;
	}

	public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
	public ModuleDeclarationSyntax Module { get; }
	public SyntaxToken<Delimiter> EndOfFileToken { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return Module;
		yield return EndOfFileToken;
	}
}