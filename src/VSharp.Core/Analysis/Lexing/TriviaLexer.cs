using FluentAssertions;

namespace VSharp.Core.Analysis.Lexing;

using System.Text;
using Microsoft.Toolkit.Diagnostics;

public partial class Lexer
{
	// Trivia gets the same position as the token it's attached to because that makes it easier 
	// to locate a trivia based on the token, no extra math will or off-by-one errors will need to
	// be taken care of with extra precautions
	private IReadOnlyList<SyntaxTrivia> ScanSyntaxTrivia(in SyntaxKind triviaKind)
	{
		var builder = new List<SyntaxTrivia>();
		var done = false;
	    while (!done && !IsAtEnd)
	    {
		    m_start = m_position;

		    switch (Current)
		    {
			    // we've clearly hit something wonky
			    case CharacterInfo.InvalidCharacter:
			    {
				    done = true;
				    break;
			    }
			    case '/':
			    {
				    if (Next == '/')
				    {
					    builder.Add(ScanSingleLineComment());
				    }
				    else if (Next == '*')
				    {
					    builder.Add(ScanMultiLineComment());
				    }
				    else
				    {
					    done = true;
				    }
				    
				    break;
			    }
			    case var _ when Current.IsNewLine():
			    {
				    if (triviaKind == SyntaxKind.TrailingTrivia)
				    {
					    done = true;
				    }
				    builder.Add(ScanLineBreak());
				    break;
			    }
			    case var _ when Current.IsWhiteSpace():
			    {
				    builder.Add(ScanWhiteSpace());
				    break;
			    }
			    case var _ when Current.IsWhiteSpace():
			    {
				    builder.Add(ScanWhiteSpace());
				    break;
			    }
			    default:
			    {
				    done = true;
				    break;
			    }
		    } 
	    }

	    return builder;
    }

    private SyntaxTrivia ScanLineBreak()
    {
	    if (Current == '\r' && Next == '\n')
	    {
		    m_line++;
		    Advance(2);
	    }
	    else
	    {
		    Advance();
	    }
	    
	    return new SyntaxTrivia(in SyntaxKind.NewLineToken, m_tokenPosition, GetFullSpan());
    }

    private SyntaxTrivia ScanSingleLineComment()
    {
	    Current.Should().Be('/');
	    Next.Should().Be('/');
	    Advance(2); // jump past the two forward slashes 
	    var done = false;
	    while (!done && !IsAtEnd)
	    {
		    switch (Current)
		    {
			    case CharacterInfo.InvalidCharacter: // a comment can be on the last line so eof character is okay
			    case var _ when Current.IsNewLine():
				    done = true;
				    break;
			    default:
				    Advance();
				    break;
		    }
	    }
	    
	    return new SyntaxTrivia(SyntaxKind.SingleLineCommentToken, m_tokenPosition, GetFullSpan());
    }

    private SyntaxTrivia ScanMultiLineComment()
    {
	    Current.Should().Be('/');
	    Next.Should().Be('*');
	    Advance(2); // jump past the forward slash and asterisk 
	    var done = false;
	    while (!done && !IsAtEnd)
	    {
		    switch (Current)
		    {
			    case CharacterInfo.InvalidCharacter:
				    var span = new TextSpan(m_start, 2);
				    var location = new TextLocation(m_source, span);
				    m_diagnostics.ReportUnterminatedMultiLineComment(location, out _);
				    done = true;
				    break;
			    case '*':
				    if (Next == '/')
				    {
					    Advance();
					    done = true;
				    }
				    Advance();
				    break;
			    default:
				    if (Current.IsNewLine())
				    {
					    m_line++;
				    }
				    Advance();
				    break;
		    }
	    }
	    
	    return new SyntaxTrivia(SyntaxKind.MultiLineCommentToken, m_tokenPosition, GetFullSpan());
    }
    
    private SyntaxTrivia ScanWhiteSpace()
    {
	    while (char.IsWhiteSpace(Current) && !IsAtEnd)
	    {
		    Advance();
	    }

	    var whitespace = GetFullSpan();
	    var sb = new StringBuilder();
	    
	    // let's identify the whitespace types
	    foreach (var space in whitespace)
	    {
		    if (whitespace[^1] == space)
		    {
			    sb.Append(space.ToHexString());
		    }
		    else
		    {
			    sb.Append($"{space.ToHexString()},");
		    }
	    }
	    return new SyntaxTrivia(SyntaxKind.WhiteSpaceToken, m_tokenPosition, sb.ToString());
    }
}