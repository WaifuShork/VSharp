using System.Drawing;
using System.Reflection;
using FluentAssertions;
using WaifuShork.Common.Extensions;

namespace VSharp.Core.Analysis.Syntax;

[PublicAPI]
public abstract class SyntaxNode
{
	public abstract IEnumerable<SyntaxNode> GetChildren();
	public abstract SyntaxKind Kind { get; }
	protected static IEnumerable<SyntaxNode> Default => Array.Empty<SyntaxNode>();

	public static T BuilderFor<T>() where T : SyntaxNode, new()
	{
		return new T();
	}
	
	public virtual TextSpan Span
	{
		get
		{
			var first = GetChildren().First().Span;
			var last = GetChildren().Last().Span;
			return TextSpan.FromBounds(first.Start, last.End);
		}
	}

	public ISyntaxToken GetLastToken()
	{
		switch (this)
		{
			case SyntaxToken<Any> anyToken:
				return anyToken;
			case SyntaxToken<char> charToken:
				return charToken;
			case SyntaxToken<int8> i8Token:
				return i8Token;
			case SyntaxToken<uint8> ui8Token:
				return ui8Token;
			case SyntaxToken<int16> i16Token:
				return i16Token;
			case SyntaxToken<uint16> ui16Token:
				return ui16Token;
			case SyntaxToken<int32> i32Token:
				return i32Token;
			case SyntaxToken<uint32> ui32Token:
				return ui32Token;
			case SyntaxToken<int64> i64Token:
				return i64Token;
			case SyntaxToken<uint64> ui64Token:
				return ui64Token;
			case SyntaxToken<float32> f32Token:
				return f32Token;
			case SyntaxToken<float64> f64Token:
				return f64Token;
		}
		
		return GetChildren().Last().GetLastToken();
	}

	public async Task WriteToAsync(TextWriter writer)
	{
		await PrettyPrintAsync(writer, this);
	}

	private static async Task PrettyPrintAsync(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true)
	{
		// \──
		var marker = isLast 
			? @"└──".ColorizeForeground(Color.Coral) 
			: "├──".ColorizeForeground(Color.Coral);
		
		await Console.Out.WriteAsync(indent);
		await Console.Out.WriteAsync(marker);
		await Console.Out.WriteAsync(node.Kind.ToString().ColorizeForeground(Color.Magenta));
		await PrintValueAsync(node);
		
		await Console.Out.WriteLineAsync();
		
		indent += isLast ? "    " : "│   ".ColorizeForeground(Color.Coral);

		var lastChild = node.GetChildren().LastOrDefault();

		foreach (var child in node.GetChildren())
		{
			await PrettyPrintAsync(writer, child, indent, child == lastChild);
		}
	}
	
	private static async Task PrintValueAsync(SyntaxNode node)
	{
		switch (node)
		{
			case SyntaxToken<bool> b:
				await Console.Out.WriteAsync($": {b.Value}");
				break;
			case SyntaxToken<double> d:
				await Console.Out.WriteAsync($": {d.Value}");
				break;
			case SyntaxToken<string> s:
				await Console.Out.WriteAsync($": {s.Value}");
				break;
			case VariableDeclarationSyntax v:
				var mut = v.MutabilityKeyword is null ? "Mutable" : "Immutable";
				await Console.Out.WriteAsync($": [State: {mut}]");
				break;
		}
	}
	
	public async Task<string> ToStringAsync()
	{
		await using var writer = new StringWriter();
		await WriteToAsync(writer);
		return writer.ToString();
	}
	
	public override string ToString()
	{
		using var writer = new StringWriter();
		WriteToAsync(writer).Wait();
		return writer.ToString();
	}
}