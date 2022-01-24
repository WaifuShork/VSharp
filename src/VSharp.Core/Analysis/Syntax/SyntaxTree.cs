namespace VSharp.Core.Analysis.Syntax;

using System.Drawing;
using WaifuShork.Common.Extensions;

[PublicAPI]
public sealed class SyntaxTree
{
	/*public SyntaxTree(IReadOnlyList<DiagnosticInfo> diagnostics, ExpressionSyntax root, ISyntaxToken endOfFileToken)
	{
		Diagnostics = diagnostics;
		Root = root;
		EndOfFileToken = endOfFileToken;
	}*/

	private SyntaxTree(in SourceText text)
	{
		var parser = new Parser(in text);
		var root = parser.ParseCompilationUnit();
		var diagnostics = parser.Diagnostics;

		Source = text;
		Diagnostics = diagnostics.ToList();
		Root = root;
	}

	public SourceText Source { get; }
	
	public IReadOnlyList<DiagnosticInfo> Diagnostics { get; }
	public CompilationUnitSyntax Root { get; }
	// public ISyntaxToken EndOfFileToken { get; }

	public static SyntaxTree Parse(in SourceText text)
	{
		return new SyntaxTree(in text);
		// var parser = new Parser(text);
		// return parser.Parse();
	}

	public static IReadOnlyList<ISyntaxToken> ParseTokens(in string text, out IReadOnlyList<DiagnosticInfo> diagnostics)
	{
		var source = SourceText.From(text);
		var tokens = Lexer.ScanSyntaxTokens(in source, out diagnostics);
		/*foreach (var token in tokens)
		{
			Console.WriteLine(token.ToString(Formatting.Expanded));
		}*/

		return tokens;
	}
	
	public static IReadOnlyList<ISyntaxToken> ParseTokens(in string text)
	{
		var tokens = Lexer.ScanSyntaxTokens(SourceText.From(in text), out _);
		return tokens;
	}
}