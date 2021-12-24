using VSharp.Core.Analysis.Syntax;
using VSharp.Core.Analysis.Text;

namespace VSharp.Core.Analysis.Lexing;

public partial class Lexer
{
	private void ScanSyntaxTrivia(TriviaKind kind)
    {
	    m_triviaBuilder.Clear();
	    var done = false;
	    while (!done && !IsAtEnd)
	    {
		    m_start = m_position;
		    m_kind = SyntaxKind.BadToken;
		    m_value = null;

		    switch (Current)
		    {
			    // we've clearly hit something wonky
			    case InvalidCharacter:
				    done = true;
				    break;
			    case '/':
				    if (Next == '/')
				    {
					    ScanSingleLineComment();
				    }
				    else if (Next == '*')
				    {
					    ScanMultiLineComment();
				    }
				    else
				    {
					    done = true;
				    }

				    break;
			    case var _ when Current.IsNewLine():
				    if (kind == TriviaKind.Trailing)
				    {
					    done = true;
				    }
				    ScanLineBreak();
				    break;
				case var _ when Current.IsWhiteSpace(): 
				    ScanWhiteSpace();
				    break;
			    case var _ when Current.IsWhiteSpace():
				    ScanWhiteSpace();
				    break;
			    default:
				    done = true;
				    break;
		    }

		    var length = m_position - m_start;
		    if (length <= 0)
		    {
			    continue;
		    }
		    
		    var text = GetFullSpan();
		    var trivia = new SyntaxTrivia(m_kind, m_start, text);
		    m_triviaBuilder.Add(trivia);
	    }
    }

    private void ScanLineBreak()
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

	    m_kind = SyntaxKind.NewLineToken;
    }

    private void ScanSingleLineComment()
    {
	    Advance(2);
	    var done = false;
	    while (!done && !IsAtEnd)
	    {
		    switch (Current)
		    {
			    case InvalidCharacter:
			    case var _ when Current.IsNewLine():
				    done = true;
				    break;
			    default:
				    Advance();
				    break;
		    }
	    }

	    m_kind = SyntaxKind.SingleLineCommentToken;
    }

    private void ScanMultiLineComment()
    {
	    Advance(2);
	    var done = false;
	    while (!done && !IsAtEnd)
	    {
		    switch (Current)
		    {
			    case InvalidCharacter:
				    var span = new TextSpan(m_start, 2);
				    var location = new TextLocation(m_source, span);
				    // unterminated multiline comment
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

	    m_kind = SyntaxKind.MultiLineCommentToken;
    }
    
    private void ScanWhiteSpace()
    {
	    while (char.IsWhiteSpace(Current) && !IsAtEnd)
	    {
		    Advance();
	    }

	    m_kind = SyntaxKind.WhiteSpaceToken;
    }
}