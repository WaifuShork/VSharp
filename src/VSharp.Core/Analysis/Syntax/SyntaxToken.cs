using System.Drawing;
using System.Text.Json;
using VSharp.Core.Analysis.Text;
using WaifuShork.Common.Extensions;

namespace VSharp.Core.Analysis.Syntax;

public class SyntaxToken : SyntaxNode
{
	public SyntaxToken(SyntaxKind kind, string text, object value, int position,
		int line, IReadOnlyList<SyntaxTrivia> leadingTrivia, IReadOnlyList<SyntaxTrivia> trailingTrivia)
	{
		Kind = kind;
		Text = text;
		Value = value;
		
		Position = position;
		Line = line;
		
		LeadingTrivia = leadingTrivia;
		TrailingTrivia = trailingTrivia;
	}
	
	public override SyntaxKind Kind { get; }
	public string Text { get; }
	public object Value { get; }
	public int Position { get; }
	public int Line { get; }
	
	public IReadOnlyList<SyntaxTrivia> LeadingTrivia { get; }
	public IReadOnlyList<SyntaxTrivia> TrailingTrivia { get; }

	public TextSpan Span => new(Position, Text.Length);
	public TextLocation Location => new(SourceText.From(Text), Span);
	
	public bool IsMissing => Text is null;
	
	public override string ToString()
	{
		return ToString(Formatting.Indented);
	}

	public string ToString(Formatting formatting)
	{
		return formatting switch
		{
			Formatting.Indented => "SyntaxToken\n" + 
			                       "{\n" + 
			                       $"   Kind: {Kind.ToString().ColorizeForeground(Color.Aqua)}\n" + 
			                       $"   Text: {Text.ColorizeForeground(Color.Aqua)}\n" + 
			                       $"   Value: {Value.ToString().ColorizeForeground(Color.Aqua)}\n" + 
			                       $"   Position: {Position.ToString().ColorizeForeground(Color.Aqua)}\n" + 
			                       $"   Line: {Line.ToString().ColorizeForeground(Color.Aqua)}\n" +
			                       // $"   LeadingTrivia: {leading}\n" +
			                       // $"   TrailingTrivia: {trailing}\n" +
			                       "" + "}",
			Formatting.Value => "SyntaxToken { " + 
			                    $"Kind: {Kind.ToString().ColorizeForeground(Color.Aqua)}, " + 
			                    $"Text: {Text.ColorizeForeground(Color.Aqua)}, " + 
			                    $"Value: {Value.ToString().ColorizeForeground(Color.Aqua)}, " + 
			                    $"Position: {Position.ToString().ColorizeForeground(Color.Aqua)}, " + 
			                    $"Line: {Line.ToString().ColorizeForeground(Color.Aqua)} " + "}",
			_ => throw new ArgumentOutOfRangeException(nameof(formatting), formatting, null)
		};
	}

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		return ArraySegment<SyntaxNode>.Empty;
	}

	public static SyntaxToken EndOfFile => 
		new(SyntaxKind.EndOfFileToken, "EOF", "EOF", 0, 0, new ArraySegment<SyntaxTrivia>(), new ArraySegment<SyntaxTrivia>());
}

public enum Formatting
{
	Indented,
	Value
}