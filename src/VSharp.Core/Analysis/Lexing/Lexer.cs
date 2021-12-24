using System.Diagnostics;
using VSharp.Core.Analysis.Diagnostics;
using VSharp.Core.Analysis.Syntax;
using VSharp.Core.Analysis.Text;

namespace VSharp.Core.Analysis.Lexing;

public enum TriviaKind
{
	Leading,
	Trailing
}

public partial class Lexer
{
	// A constant value that would never logically represent a valid token or character 
	// in the stream of characters from the source text
    private const char InvalidCharacter = char.MaxValue; // (char) 0xFFFF

    // NOTE: each thread will have their own SyntaxCache for thread-safety purposes,
    // I didn't really like it being a static data holder class that several threads
    // would have access to simultaneously..
    private readonly SyntaxCache m_syntaxCache = new();
    
    private readonly SourceText m_source;
    private readonly int m_srcLength;

    private readonly List<SyntaxTrivia> m_triviaBuilder = new();
    private readonly DiagnosticBag m_diagnostics = new();
    
    private int m_start; // start position of the current lexeme 
    private int m_position; // lexer position in the source text 
    private int m_line; // current line of the "file" 
    private SyntaxKind m_kind; // the type of the token
    private object? m_value; // value of the token, only applies for things like numbers, strings, and other literals

    private Lexer(SourceText source)
    {
	    m_source = source;
	    m_srcLength = source.Length;

        m_start = 0;
        m_position = 0;
        m_line = 1;
        m_kind = SyntaxKind.BadToken;
        m_value = null!;
    }

    private char Current => Peek(0);
    private char Next => Peek(1);

    private bool IsAtEnd => m_position >= m_srcLength;
    private Lazy<IReadOnlyList<DiagnosticInfo>> Diagnostics => m_diagnostics.Cache;

    public static IReadOnlyList<SyntaxToken> ScanSyntaxTokens(SourceText source, out IReadOnlyList<DiagnosticInfo> diagnostics)
    {
	    var lexer = new Lexer(source);
	    var counter = 0;
	    var tokens = new List<SyntaxToken>();
	    while (true)
        {
	        lexer.ScanSyntaxTrivia(TriviaKind.Leading);

	        var tokenStart = lexer.m_position;
	        var leadingTrivia = lexer.m_triviaBuilder;
	        var line = lexer.m_line;
	        
	        lexer.ScanSyntaxToken();

	        var tokenKind = lexer.m_kind;
	        var tokenValue = lexer.m_value;
	        var tokenLength = lexer.m_position - lexer.m_start;
	        
	        lexer.ScanSyntaxTrivia(TriviaKind.Trailing);
	        var trailingTrivia = lexer.m_triviaBuilder;

	        // NOTE: First lookup will be delayed due to lazy initialisation
	        var text = lexer.m_syntaxCache.LookupText(tokenKind);
	        if (string.IsNullOrWhiteSpace(text))
	        {
		        text = lexer.m_source.Substring(tokenStart, tokenLength);
	        }
	        
	        var token = new SyntaxToken(tokenKind, text, tokenValue ?? text, counter, line, leadingTrivia, trailingTrivia);
	        
	        // I don't think I even need the EOF token included in the list 
	        if (token.Kind == SyntaxKind.EndOfFileToken)
	        {
		        break;
	        }
	        
	        if (token.Kind == SyntaxKind.WhiteSpaceToken ||
	            token.Kind == SyntaxKind.BadToken)
	        {
		        continue;
	        }

	        tokens.Add(token);
	        counter++;
        }

	    diagnostics = lexer.Diagnostics.Value;
	    return tokens;
    }

    // TODO: return syntax token here
    private void ScanSyntaxToken()
    {
	    // Reset the lexer read information for the new token
        m_start = m_position;
        m_value = null;
        m_kind = SyntaxKind.BadToken; // always assume the worst

        // Scan leading trivia, and trailing after entering a token
        
        var token = default(SyntaxToken);
        switch (Current)
        {
            case InvalidCharacter:
                m_kind = SyntaxKind.EndOfFileToken;
                token = new SyntaxToken(m_kind, "EOF", "EOF", 0, m_line, ArraySegment<SyntaxTrivia>.Empty, ArraySegment<SyntaxTrivia>.Empty);
                break;
            case '.':
				m_kind = SyntaxKind.DotToken;
				Advance();
				break;
			case ',':
				m_kind = SyntaxKind.CommaToken;
				Advance();
				break;
			case '(':
				m_kind = SyntaxKind.OpenParenToken;
				Advance();
				break;
			case ')':
				m_kind = SyntaxKind.CloseParenToken;
				Advance();
				break;
			case '{':
				m_kind = SyntaxKind.OpenBraceToken;
				Advance();
				break;
			case '}':
				m_kind = SyntaxKind.CloseBraceToken;
				Advance();
				break;
			case '[':
				m_kind = SyntaxKind.OpenBracketToken;
				Advance();
				break;
			case ']':
				m_kind = SyntaxKind.CloseBracketToken;
				Advance();
				break;
			case ':':
				m_kind = SyntaxKind.ColonToken;
				Advance();
				break;
			case ';':
				m_kind = SyntaxKind.SemicolonToken;
				Advance();
				break;
			case '?':
				m_kind = SyntaxKind.QuestionMarkToken;
				Advance();
				break;
			case '^':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.CaretEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.CaretToken;
						break;
				}
				break;
			case '~':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.TildeEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.TildeToken;
						break;
				}
				break;
			case '+':
				Advance();
				switch (Current)
				{
					case '+':
						m_kind = SyntaxKind.PlusPlusToken;
						Advance();
						break;
					case '=':
						m_kind = SyntaxKind.PlusEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.PlusToken;
						break;
				}
				break;
			case '-':
				Advance();
				switch (Current)
				{
				case '-':
					m_kind = SyntaxKind.MinusMinusToken;
					Advance();
					break;
				case '=':
					m_kind = SyntaxKind.MinusEqualsToken;
					Advance();
					break;
				default:
					m_kind = SyntaxKind.MinusToken;
					break;
				}
				break;
			case '/':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.FSlashEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.FSlashToken;
						break;
				}
				break;
			case '*':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.AsteriskEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.AsteriskToken;
						break;
				}
				break;
			case '%':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.PercentEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.PercentToken;
						break;
				}
				break;
			case '=':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.EqualsEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.EqualsToken;
						break;
				}
				break;
			case '>':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.GreaterEqualsToken;
						Advance();
						break;
					case '>':
						Advance();
						if (Current == '=')
						{
							m_kind = SyntaxKind.GreaterGreaterEqualsToken;
							Advance();
						}
						else
						{
							m_kind = SyntaxKind.GreaterGreaterToken;
						}
						break;
					default:
						m_kind = SyntaxKind.GreaterToken;
						break;
				}
				break;
			case '<':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.LessEqualsToken;
						Advance();
						break;
					case '<':
						Advance();
						if (Current == '=')
						{
							m_kind = SyntaxKind.LessLessEqualsToken;
							Advance();
						}
						else
						{
							m_kind = SyntaxKind.LessLessToken;
						}
						break;
					default:
						m_kind = SyntaxKind.LessToken;
						break;
				}
				break;
			case '!':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.BangEqualsToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.BangToken;
						break;
				}
				break;
			case '|':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.PipeEqualsToken;
						Advance();
						break;
					case '|':
						m_kind = SyntaxKind.PipePipeToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.PipeToken;
						break;
				}
				break;
			case '&':
				Advance();
				switch (Current)
				{
					case '=':
						m_kind = SyntaxKind.AmpersandEqualsToken;
						Advance();
						break;
					case '&':
						m_kind = SyntaxKind.AmpersandAmpersandToken;
						Advance();
						break;
					default:
						m_kind = SyntaxKind.AmpersandToken;
						break;
				}
				break;
            case '"':
			case '\'':
				ScanStringOrCharLiteral();
				break;
            case var letter when letter.IsIdentifierStartCharacter():// char.IsLetter(letter) || letter == '_':
	            ScanIdentifierOrKeyword();
	            break;
            case var digit when digit.IsQualifiedDigit(): // char.IsDigit(digit):
                ScanNumericLiteral();
	            break;
            default:
	            var span = new TextSpan(m_position, 1);
	            var location = new TextLocation(m_source, span);
	            m_diagnostics.ReportUnexpectedToken(location, Current);
	            Advance();
	            break;
        }
        
        Console.WriteLine(token);
    }

    private string GetFullSpan()
    {
	    unchecked
	    {
		    var length = m_position - m_start;
		    
		    Debug.Assert(m_start >= 0 && length >= 0, "unable to assert (m_start >= 0 && length >= 0)");
		    Debug.Assert(m_start <= int.MaxValue && length <= int.MaxValue, "unable to assert (m_start <= int.MaxValue && length <= int.MaxValue)");

		    return m_source.Substring(m_start, length);
	    }
    }

    private void ScanIdentifierOrKeyword()
    {
	    while ((char.IsLetterOrDigit(Current) || Current == '_') && !IsAtEnd)
	    {
		    Advance();
	    }

	    var text = GetFullSpan();
	    m_value = text;
	    m_kind = m_syntaxCache.LookupKeyword(text);
    }

    private char Peek(ulong offset = 0)
    {
	    // ulong to force forward peeking only
	    checked // if we overflow, we have a big problem
	    {
		    var index = m_position + (int) offset;
		    if (index >= m_srcLength)
		    {
			    return InvalidCharacter;
		    }

		    return m_source[index];
	    }
    }
    
    private void Advance(ulong amount = 1)
    {
	    if (m_position >= m_srcLength)
	    {
		    return;
	    }
	    
	    checked // we need to know if this overflows because then there's a big issue
	    {
		    // ulong because it doesn't make much sense to advance...backwards? 
		    m_position += (int) amount;
	    }
    }
}