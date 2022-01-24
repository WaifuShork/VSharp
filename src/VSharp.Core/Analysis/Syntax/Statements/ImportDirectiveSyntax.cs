namespace VSharp.Core.Analysis.Syntax.Statements;

[PublicAPI]
public class ImportDirectiveSyntax : MemberSyntax
{
	public ImportDirectiveSyntax(SyntaxToken<string> useKeyword, 
	                             SyntaxToken<string>? openBracketToken,
	                             IReadOnlyList<SyntaxToken<string>>? identifiers,
	                             SyntaxToken<string>? closeBracketToken,
	                             SyntaxToken<string>? fromKeyword,
	                             SyntaxToken<string> moduleIdentifier)
	{
		UseKeyword = useKeyword;
		OpenBracketToken = openBracketToken;
		Identifiers = identifiers;
		CloseBracketToken = closeBracketToken;
		FromKeyword = fromKeyword; 
		ModuleIdentifier = moduleIdentifier;
	}

	public ImportDirectiveSyntax(SyntaxToken<string> useKeyword, SyntaxToken<string> moduleIdentifier)
		: this(useKeyword, null, null, null, null, moduleIdentifier) { }
	
	public override SyntaxKind Kind => SyntaxKind.ImportDirective;
	public SyntaxToken<string> UseKeyword { get; }
	public SyntaxToken<string>? OpenBracketToken { get; }
	public IReadOnlyList<SyntaxToken<string>>? Identifiers { get; }
	public SyntaxToken<string>? CloseBracketToken { get; }
	public SyntaxToken<string>? FromKeyword { get; }
	public SyntaxToken<string> ModuleIdentifier { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return UseKeyword;
		if (OpenBracketToken is not null)
		{
			yield return OpenBracketToken;
		}

		if (Identifiers is not null && Identifiers.IsNotEmpty())
		{
			foreach (var identifier in Identifiers)
			{
				yield return identifier;
			}
		}

		if (CloseBracketToken is not null)
		{
			yield return CloseBracketToken;
		}

		if (FromKeyword is not null)
		{
			yield return FromKeyword;
		}
		yield return ModuleIdentifier;
	}
}