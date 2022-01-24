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

	private SyntaxTree(SourceText text)
	{
		var parser = new Parser(text);
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

	public static SyntaxTree Parse(SourceText text)
	{
		return new SyntaxTree(text);
		// var parser = new Parser(text);
		// return parser.Parse();
	}

	public static IReadOnlyList<ISyntaxToken> ParseTokens(string text, out IReadOnlyList<DiagnosticInfo> diagnostics)
	{
		var tokens = Lexer.ScanSyntaxTokens(SourceText.From(text), out diagnostics);
		/*foreach (var token in tokens)
		{
			Console.WriteLine(token.ToString(Formatting.Expanded));
		}*/

		return tokens;
	}
	
	public static IReadOnlyList<ISyntaxToken> ParseTokens(string text)
	{
		var tokens = Lexer.ScanSyntaxTokens(SourceText.From(text), out _);
		return tokens;
	}

	/*public static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
	{
		// └──
		var marker = isLast 
			? @"\──".ColorizeForeground(Color.Coral) 
			: "├──".ColorizeForeground(Color.Coral);
		
		Console.Write(indent);
		Console.Write(marker);
		Console.Write(node.Kind.ToString().ColorizeForeground(Color.Magenta));
		PrintValue(node);
		
		Console.WriteLine();
		
		indent += isLast ? "    " : "│   ".ColorizeForeground(Color.Coral);

		var lastChild = node.GetChildren().LastOrDefault();

		foreach (var child in node.GetChildren())
		{
			PrettyPrint(child, indent, child == lastChild);
		}
	}

	private static void PrintValue(SyntaxNode node)
	{
		switch (node)
		{
			case SyntaxToken<bool> b:
				Console.Write($": {b.Value}");
				break;
			case SyntaxToken<double> d:
				Console.Write($": {d.Value}");
				break;
			case SyntaxToken<string> s:
				Console.Write($": {s.Value}");
				break;
			case VariableDeclarationSyntax v:
			{
				var mut = v.IsMutable ? "Mutable" : "Immutable";
				Console.Write($": [State: {mut}]");
				break;
			}
			case GlobalVariableDeclaration g:
			{
				var mut = g.IsMutable ? "Mutable" : "Immutable";
				Console.Write($": [State: {mut}]");
				break;
			}
			case FunctionDeclarationSyntax f:
			{
				var mut = f.IsMutable ? "Mutable" : "Immutable";
				Console.Write($": [State: {mut}]");
				break;
			}
		}
	}*/
}