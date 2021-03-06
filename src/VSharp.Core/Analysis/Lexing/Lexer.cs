using FluentAssertions;

namespace VSharp.Core.Analysis.Lexing;

using System.Collections.Immutable;

/*public enum TriviaKind
{
	Leading,
	Trailing
}*/

public partial class Lexer
{
	// NOTE: each thread will have their own SyntaxCache for thread-safety purposes,
    // I didn't really like it being a static data holder class that several threads
    // would have access to simultaneously..
    private readonly SyntaxCache m_syntaxCache = new();
    
    private SourceText m_source;
    private int m_srcLength => m_source.Length;

    private readonly DiagnosticBag m_diagnostics = new();
    
    private int m_start; // start position of the current lexeme 
    private int m_position; // lexer position in the source text 
    private int m_line; // current line of the "file" 
    private int m_tokenPosition;

    private Lexer(in SourceText source)
    {
	    m_source = source;

        m_start = 0;
        m_position = 0;
        m_line = 1;
        m_tokenPosition = 0;
    }

    private char Current => Peek(0);
    private char Next => Peek(1);

    public int Start => m_start;
    public int LexerPosition => m_position;
    public int Line => m_line;
    public int TokenPosition => m_tokenPosition;

    private bool IsAtEnd => m_position >= m_srcLength;
    private bool NotAtEnd => !IsAtEnd;

    private bool CurrentIs(char current)
    {
	    return Current == current;
    }

    private bool CurrentIsNot(char current)
    {
	    return !CurrentIs(current);
    }
    
    private bool NextIs(char next)
    {
	    return Next == next;
    }

    private bool NextIsNot(char next)
    {
	    return !NextIs(next);
    }
    
    private Lazy<IReadOnlyList<DiagnosticInfo>> Diagnostics => m_diagnostics.Cache;

    public static IReadOnlyList<ISyntaxToken> ScanSyntaxTokens(in SourceText source, out IReadOnlyList<DiagnosticInfo> diagnostics)
    {
	    var lexer = new Lexer(in source);
	    var tokens = new List<ISyntaxToken>();

	    while (true)
	    {
		    var token = lexer.ScanSyntaxToken();
		    if (token.Is(in SyntaxKind.BadToken))
		    {
			    continue;
		    }

		    // We collected a successful token, so increment
		    lexer.m_tokenPosition++;
		    tokens.Add(token);

		    if (token.Is(in SyntaxKind.EndOfFileToken))
		    {
			    break;
		    }
	    }
	    
	    diagnostics = lexer.Diagnostics.Value;
	    return tokens;
    }

    private SyntaxToken<T> CreateToken<T>(in SyntaxKind kind, string text, T value)
    {
	    return new SyntaxToken<T>(Array.Empty<SyntaxTrivia>(), in kind, text, value, m_tokenPosition, m_line, Array.Empty<SyntaxTrivia>());
    }

    private SyntaxToken<T> CreateToken<T>(IReadOnlyList<SyntaxTrivia> leading, in SyntaxKind kind, string text, T value, IReadOnlyList<SyntaxTrivia> trailing)
    {
	    return new SyntaxToken<T>(leading, in kind, text, value, m_tokenPosition, m_line, trailing);
    }
    
    private SyntaxToken<T> CreateToken<T>(IReadOnlyList<SyntaxTrivia> leading, in SyntaxKind kind, string text, T value, int position, IReadOnlyList<SyntaxTrivia> trailing)
    {
	    return new SyntaxToken<T>(leading, in kind, text, value, position, m_line, trailing);
    }

    private SyntaxToken<string> CreateToken(IReadOnlyList<SyntaxTrivia> leading, in SyntaxKind kind, string text, IReadOnlyList<SyntaxTrivia> trailing)
    {
	    return CreateToken(leading, in kind, text, text, trailing);
    }
    
    // TODO: return syntax token here
    private ISyntaxToken ScanSyntaxToken()
    {
	    // Reset the lexer read information for the new token
        m_start = m_position;
        
        var leadingTrivia = ScanSyntaxTrivia(in SyntaxKind.LeadingTrivia);
        var token = default(ISyntaxToken);
        
        switch (Current)
        {
	        case CharacterInfo.InvalidCharacter:
	        {
		        return SyntaxToken<string>.EndOfFile(m_tokenPosition, m_line, leadingTrivia);
	        }
            case '.':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.DotToken, ".", trailingTrivia);
            }
            case ',':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.CommaToken, ",", trailingTrivia);
            }
			case '(':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.OpenParenToken, "(", trailingTrivia);
            }
			case ')':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.CloseParenToken, ")", trailingTrivia);
            }
			case '{':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.OpenBraceToken, "{", trailingTrivia);
            }
			case '}':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.CloseBraceToken, "}", trailingTrivia);
            }
			case '[':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.OpenBracketToken, "[", trailingTrivia);
            }
			case ']':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.CloseBracketToken, "]", trailingTrivia);
            }
			case ':':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.ColonToken, ":", trailingTrivia);
            }
			case ';':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.SemicolonToken, ";", trailingTrivia);
            }
			case '?':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	            return CreateToken(leadingTrivia, in SyntaxKind.QuestionMarkToken, "?", trailingTrivia);
            }
	        case '@':
	        {
		        Advance();
		        var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
		        return CreateToken(leadingTrivia, in SyntaxKind.AtToken, "@", trailingTrivia);
	        }
			case '^':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.CaretEqualsToken, "^=", trailingTrivia);
					}

					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.CaretToken, "^", trailingTrivia);
					}
				}
			case '~':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.TildeEqualsToken, "~=", trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.TildeEqualsToken, "~", trailingTrivia);
					}
				}
			case '+':
				Advance();
				switch (Current)
				{
					case '+':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PlusPlusToken, "++", trailingTrivia);
					}
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PlusEqualsToken, "+=", trailingTrivia);
					}
					default:
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PlusToken, "+", trailingTrivia);
					}
				}
			case '-':
				Advance();
				switch (Current)
				{
					case '-':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.MinusMinusToken, "--", trailingTrivia);
					}
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.MinusEqualsToken, "-=", trailingTrivia);
					}
					case '>':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.ArrowToken, "->", trailingTrivia);
					}
					default:            
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.MinusToken, "-", trailingTrivia);
					}
				}
			case '/':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.FSlashEqualsToken, "/=", trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.FSlashToken, "/", trailingTrivia);
					}
				}
			case '*':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.AsteriskEqualsToken, "*=", trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.AsteriskToken, "*", trailingTrivia);
					}
				}
			case '%':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PercentEqualsToken, "%=", trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PercentToken, "%", trailingTrivia);
					}
				}
			case '=':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.EqualsEqualsToken, "==", trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.EqualsToken, "=", trailingTrivia);
					}
				}
			case '>':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.GreaterEqualsToken, ">=", trailingTrivia);
					}
					case '>':
						Advance();
						if (Current == '=')
						{
							Advance();
							var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
							return CreateToken(leadingTrivia, in SyntaxKind.GreaterGreaterEqualsToken, ">>=", trailingTrivia);
						}
						else
						{
							var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
							return CreateToken(leadingTrivia, in SyntaxKind.GreaterGreaterToken, ">>", trailingTrivia);
						}	
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.GreaterToken, ">", trailingTrivia);
					}
				}
			case '<':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.LessEqualsToken, "<=", trailingTrivia);
					}
					case '<':
						Advance();
						if (Current == '=')
						{
							Advance();
							var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
							return CreateToken(leadingTrivia, in SyntaxKind.LessLessEqualsToken, "<<=", trailingTrivia);
						}
						else
						{
							var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
							return CreateToken(leadingTrivia, in SyntaxKind.LessLessToken, "<<", trailingTrivia);
						}	
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.LessToken, "<", trailingTrivia);
					}
				}
			case '!':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.BangEqualsToken, "!=", trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.BangToken, "!", trailingTrivia);
					}
				}
			case '|':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PipeEqualsToken, "!=", trailingTrivia);
					}
					case '|':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PipePipeToken, "||", trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.PipeToken, "|", trailingTrivia);
					}
				}
			case '&':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.AmpersandEqualsToken, "&=", trailingTrivia);
					}	
					case '&':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.AmpersandAmpersandToken, "&&", trailingTrivia);
					}
					default:
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
						return CreateToken(leadingTrivia, in SyntaxKind.AmpersandToken, "&", trailingTrivia);
					}
				}
	        case '"':
	        {
		        return ScanStringLiteral('"', leadingTrivia);
	        }
	        case '\'':
	        {
		        return ScanCharLiteral('\'', leadingTrivia);
	        }
	        case var letter when letter.IsIdentifierStartCharacter():
	        {
		        return ScanIdentifierOrKeyword(leadingTrivia);
	        }
	        case var digit when digit.IsQualifiedDigit():
	        {
		        return ScanNumericLiteral(leadingTrivia);
	        }
	        default:
	        {
		        var span = new TextSpan(m_position, 1);
		        var location = new TextLocation(m_source, span);
		        m_diagnostics.ReportUnexpectedCharacter(location, Current, out _);
		        Advance();
		        break;
	        }
        }

        // Unlikely to be null, but still
        return token ?? CreateToken(leadingTrivia, in SyntaxKind.BadToken, "ERROR", ImmutableArray<SyntaxTrivia>.Empty);
    }

    private string GetFullSpan()
    {
	    unchecked
	    {
		    var length = m_position - m_start;

		    length.Should().BePositive("length must be greater than zero to obtain full span");
		    (m_start < 0 && length < 0).Should().BeFalse("start and length cannot be negative");
		    (m_start < int.MaxValue && length < int.MaxValue).Should().BeTrue("start and length cannot be greater than int.MaxValue");
		    
		    return m_source.Substring(in m_start, in length);
	    }
    }

    private string GetFullSpan(int from, int to)
    {
	    var length = to - from;
	    return m_source.Substring(in from, in length);
    }

    private ISyntaxToken ScanIdentifierToken(in IReadOnlyList<SyntaxTrivia> leading, in int32 numberLength)
    {
	    // language spec denotes that numeric prefixed identifiers can only have a number less than 10 in length,
	    // the value of the number is inconsequential due to the fact it will become a string 
	    numberLength.Should().BeLessOrEqualTo(10);
	    while (Current.IsIdentifierPartCharacter())
	    {
		    Advance();
	    }

	    // insert an underscore for CLR compatibility
	    var text = "_" + GetFullSpan();
	    var trailing = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	    return CreateToken(leading, in SyntaxKind.IdentifierToken, text, text, trailing);
    }
    
    private ISyntaxToken ScanIdentifierOrKeyword(in IReadOnlyList<SyntaxTrivia> leading)
    {
	    m_start = m_position;
	    while ((char.IsLetterOrDigit(Current) || CurrentIs('_')) && NotAtEnd)
	    {
		    Advance();
	    }

	    var text = GetFullSpan();
	    var kind = m_syntaxCache.LookupKeyword(in text);
	    var trailing = ScanSyntaxTrivia(in SyntaxKind.TrailingTrivia);
	    if (kind != SyntaxKind.TrueKeyword && kind != SyntaxKind.FalseKeyword)
	    {
		    return CreateToken(leading, in kind, text, text, trailing);
	    }

	    return text == "true"
		    ? CreateToken(leading, in kind, text, true, trailing)
		    : CreateToken(leading, in kind, text, false, trailing);
    }

    private char Peek(in int offset = 0)
    {
	    offset.Should().BeGreaterOrEqualTo(0, "cannot peek backwards");
	    
	    checked // if we overflow, we have a big problem
	    {
		    var index = m_position + offset;
		    if (index >= m_srcLength)
		    {
			    return CharacterInfo.InvalidCharacter;
		    }

		    return m_source[index];
	    }
    }
    
    private void Advance(in int amount = 1)
    {
	    if (m_position >= m_srcLength)
	    {
		    return;
	    }
	    
	    checked // we need to know if this overflows because then there's a big issue
	    {
		    // ulong because it doesn't make much sense to advance...backwards? 
		    m_position += amount;
		    m_position.Should().BeLessThanOrEqualTo(m_srcLength, "cannot advance past the full source length");
	    }
    }
}