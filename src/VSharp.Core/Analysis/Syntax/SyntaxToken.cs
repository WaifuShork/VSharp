namespace VSharp.Core.Analysis.Syntax;

using Text;
using System.Text;
using System.Drawing;
using WaifuShork.Common;
using System.Diagnostics;
using WaifuShork.Common.Extensions;

/* NOTICE: (but it works for now lmao)
	This is currently uncomfortable, there needs to be a better way to implement,
	there has to be a better way to have a token as generic. For now, all weird 
	tokens like operators, will be associated as SyntaxToken<string>, which makes sense... I guess?
	
	Take note that this interface holds everything but Value, this is to ensure we can 
	create generic tokens, if even it's kind of awkward right now
*/
[PublicAPI]
public interface ISyntaxToken
{
	IReadOnlyList<SyntaxTrivia> LeadingTrivia { get; }
	public SyntaxKind Kind { get; }
	public string Text { get; }
	public int Position { get; }
	public int Line { get; }
	IReadOnlyList<SyntaxTrivia> TrailingTrivia { get; }
	public TextLocation Location { get; }
	public string ToString(in Formatting formatting);
}

[PublicAPI]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SyntaxToken<T> : SyntaxNode, ISyntaxToken
{
	public SyntaxToken(
		in IReadOnlyList<SyntaxTrivia> leadingTrivia, 
		in SyntaxKind kind, 
		in string text, 
		in T? value, 
		in int position,
		in int line, 
		in IReadOnlyList<SyntaxTrivia> trailingTrivia,
		in DiagnosticInfo? diagnostic = null)
	{
		LeadingTrivia = leadingTrivia;

		Kind = kind;
		Text = text;
		Value = value;
		
		Position = position;
		Line = line;
		
		TrailingTrivia = trailingTrivia;
		Diagnostic = diagnostic;
	}

	public override SyntaxKind Kind { get; }
	public string Text { get; }
	public T? Value { get; }
	public int Position { get; }
	public int Line { get; }
	
	public IReadOnlyList<SyntaxTrivia> LeadingTrivia { get; }
	public IReadOnlyList<SyntaxTrivia> TrailingTrivia { get; }

	public DiagnosticInfo? Diagnostic { get; } 
	
	public override TextSpan Span => new(Position, Text.Length);
	public TextLocation Location => new(SourceText.From(Text), Span);
	
	// whitespace will never be a valid token, only a trivia 
	public bool IsMissing => string.IsNullOrWhiteSpace(Text);
	
	public override string ToString()
	{
		return ToString(Formatting.Expanded);
	}

	public string ToString(in Formatting formatting)
	{
		string temp;
		var leading = string.IsNullOrWhiteSpace(temp = FormatTrivia(LeadingTrivia)) ? $"{CharacterInfo.Tab}{CharacterInfo.Tab}None" : temp; 
		var trailing = string.IsNullOrWhiteSpace(temp = FormatTrivia(TrailingTrivia)) ? $"{CharacterInfo.Tab}{CharacterInfo.Tab}None" : temp;
		
		var kind = $"{Kind.ToString().ColorizeForeground(Color.Aqua)}";
		var text = $"{Text.ColorizeForeground(Color.Aqua)}";
		var value = $"{Value?.ToString()?.ColorizeForeground(Color.Aqua)}";
		var position = $"{Position.ToString().ColorizeForeground(Color.Aqua)}";
		var line = $"{Line.ToString().ColorizeForeground(Color.Aqua)}";

		return formatting switch
		{
			Formatting.Expanded => "SyntaxToken\n" + 
			                       "{\n" + 
			                      $"{CharacterInfo.Tab}LeadingTrivia:\n" +
			                      $"{CharacterInfo.Tab}" + "{\n" +
			                      $"{leading}\n" +
			                      $"{CharacterInfo.Tab}" + "}\n" +
			                      $"{CharacterInfo.Tab}Kind: {kind}\n" + 
			                      $"{CharacterInfo.Tab}Text: {text}\n" + 
			                      $"{CharacterInfo.Tab}Value: {value}\n" + 
			                      $"{CharacterInfo.Tab}Position: {position}\n" + 
			                      $"{CharacterInfo.Tab}Line: {line}\n" +
			                      $"{CharacterInfo.Tab}TrailingTrivia:\n" +
			                      $"{CharacterInfo.Tab}" + "{\n" +
			                      $"{trailing}\n" +
			                      $"{CharacterInfo.Tab}" + "}\n" +
			                       "}",
			Formatting.Compact => "SyntaxToken\n" +
			                      "{\n" + 
			                     $"{CharacterInfo.Tab}LeadingTrivia:\n" +
			                     $"{CharacterInfo.Tab}" + "{\n" +
			                     $"{leading}\n" +
			                     $"{CharacterInfo.Tab}" + "}\n" +
			                     $"{CharacterInfo.Tab}Kind: {kind}, " + 
			                     $"{CharacterInfo.Tab}Text: {text}, " + 
			                     $"{CharacterInfo.Tab}Value: {value}, " + 
			                     $"{CharacterInfo.Tab}Position: {position}, " + 
			                     $"{CharacterInfo.Tab}Line: {line} " + 
			                     $"{CharacterInfo.Tab}TrailingTrivia:\n" +
			                     $"{CharacterInfo.Tab}" + "{\n" +
			                     $"{trailing}\n" +
			                     $"{CharacterInfo.Tab}" + "}\n" +
			                      "}",
			
			_ => throw new ArgumentOutOfRangeException(nameof(formatting), formatting, "invalid formatting supplied")
		};
	}

	private static string FormatTrivia(in IReadOnlyList<SyntaxTrivia> trivias)
	{
		var sb = new StringBuilder();
		foreach (var trivia in trivias)
		{
			if (trivias[^1].Equals(trivia)) // is this the last element?
			{
				sb.Append($"{CharacterInfo.Tab}{CharacterInfo.Tab}{trivia.ToString()}");
			}
			else
			{
				sb.AppendLine($"{CharacterInfo.Tab}{CharacterInfo.Tab}{trivia.ToString()},");
			}
		}

		return sb.ToString();
	}
	
	private string DebuggerDisplay => ToString(Formatting.Expanded);

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		return Default;
	}

	public static SyntaxToken<string> EndOfFile(in int position, in int line, in IReadOnlyList<SyntaxTrivia> leading, in DiagnosticInfo? diagnostic = null)
	{
		// it's pretty much impossible to have trailing trivia on the EOF
		return new(in leading, in SyntaxKind.EndOfFileToken, "EOF", "EOF", in position, in line, Array.Empty<SyntaxTrivia>(), in diagnostic);
	}

	public static SyntaxToken<string> Illegal(in int position, in int line, in IReadOnlyList<SyntaxTrivia> leading, in DiagnosticInfo? diagnostic = null)
	{
		return new(in leading, in SyntaxKind.BadToken, "ERROR", "ERROR", in position, in line, Array.Empty<SyntaxTrivia>(), in diagnostic);
	}

	public static bool operator ==(in SyntaxToken<T> left, in SyntaxToken<T> right)
	{
		return left.Equals(in right);
	}
	
	public static bool operator !=(in SyntaxToken<T> left, in SyntaxToken<T> right)
	{
		return !left.Equals(in right);
	}

	public override int GetHashCode()
	{
		var hash = (Kind, Text, Value, Position, Line, LeadingTrivia, TrailingTrivia).GetHashCode();
		if (hash.IsInvalidHash())
		{
			return (int)$"({Kind:X}|{Text}|{Position:X}|{Line:X}|{LeadingTrivia.Count}|{TrailingTrivia.Count})".GetFnvHashCode();
		}

		return hash;
	}
	
	public bool Equals(in SyntaxToken<T> other)
	{
		// incredibly overkill but who cares?
		return Kind == other.Kind 
		    && Text == other.Text 
		    && EqualityComparer<T>.Default.Equals(Value, other.Value) 
		    && Position == other.Position 
		    && Line == other.Line
		    && LeadingTrivia.SequenceEqual(other.LeadingTrivia) 
		    && TrailingTrivia.SequenceEqual(other.TrailingTrivia) 
		    && LeadingTrivia.Equals(other.LeadingTrivia) 
		    && TrailingTrivia.Equals(other.TrailingTrivia);
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj.GetType() != this.GetType())
		{
			return false;
		}

		if (obj is not SyntaxToken<T> st)
		{
			return false;
		}
		
		return Equals(in st);
	}
}

public enum Formatting
{
	Expanded,
	Compact
}