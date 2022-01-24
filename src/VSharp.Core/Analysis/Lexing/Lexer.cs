using FluentAssertions;

namespace VSharp.Core.Analysis.Lexing;

using System.Collections.Immutable;

public enum TriviaKind
{
	Leading,
	Trailing
}

public partial class Lexer
{
	// NOTE: each thread will have their own SyntaxCache for thread-safety purposes,
    // I didn't really like it being a static data holder class that several threads
    // would have access to simultaneously..
    private readonly SyntaxCache m_syntaxCache = new();
    
    private readonly SourceText m_source;
    private readonly int m_srcLength;

    private readonly DiagnosticBag m_diagnostics = new();
    
    private int m_start; // start position of the current lexeme 
    private int m_position; // lexer position in the source text 
    private int m_line; // current line of the "file" 
    private int m_tokenPosition;

    private Lexer(in SourceText source)
    {
	    m_source = source;
	    m_srcLength = source.Length;

        m_start = 0;
        m_position = 0;
        m_line = 1;
        m_tokenPosition = 0;
    }

    private char Current => Peek(0);
    private char Next => Peek(1);

    private bool IsAtEnd => m_position >= m_srcLength;
    private Lazy<IReadOnlyList<DiagnosticInfo>> Diagnostics => m_diagnostics.Cache;

    public static IReadOnlyList<ISyntaxToken> ScanSyntaxTokens(in SourceText source, out IReadOnlyList<DiagnosticInfo> diagnostics)
    {
	    var lexer = new Lexer(in source);
	    var tokens = new List<ISyntaxToken>();

	    while (true)
	    {
		    var token = lexer.ScanSyntaxToken();
		    if (token.Kind == SyntaxKind.BadToken)
		    {
			    continue;
		    }

		    // We collected a successful token, so increment
		    lexer.m_tokenPosition++;
		    tokens.Add(token);

		    if (token.Kind == SyntaxKind.EndOfFileToken)
		    {
			    break;
		    }
	    }
	    
	    diagnostics = lexer.Diagnostics.Value;
	    return tokens;
    }

    private SyntaxToken<T> CreateToken<T>(in SyntaxKind kind, in string text, in T value)
    {
	    return new SyntaxToken<T>(Array.Empty<SyntaxTrivia>(), in kind, in text, in value, in m_tokenPosition, in m_line, Array.Empty<SyntaxTrivia>());
    }

    private SyntaxToken<T> CreateToken<T>(in IReadOnlyList<SyntaxTrivia> leading, in SyntaxKind kind, in string text, in T value, in IReadOnlyList<SyntaxTrivia> trailing)
    {
	    return new SyntaxToken<T>(in leading, in kind, in text, in value, in m_tokenPosition, in m_line, in trailing);
    }

    private SyntaxToken<string> CreateToken(in IReadOnlyList<SyntaxTrivia> leading, in SyntaxKind kind, in string text, in IReadOnlyList<SyntaxTrivia> trailing)
    {
	    return CreateToken(in leading, in kind, in text, in text, in trailing);
    }
    
    // TODO: return syntax token here
    private ISyntaxToken ScanSyntaxToken()
    {
	    // Reset the lexer read information for the new token
        m_start = m_position;
        
        var leadingTrivia = ScanSyntaxTrivia(TriviaKind.Leading);
        var token = default(ISyntaxToken);
        
        switch (Current)
        {
	        case CharacterInfo.InvalidCharacter:
	        {
		        return SyntaxToken<string>.EndOfFile(in m_tokenPosition, in m_line, in leadingTrivia);
	        }
            case '.':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.DotToken, ".", in trailingTrivia);
            }
            case ',':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.CommaToken, ",", in trailingTrivia);
            }
			case '(':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.OpenParenToken, "(", in trailingTrivia);
            }
			case ')':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.CloseParenToken, ")", in trailingTrivia);
            }
			case '{':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.OpenBraceToken, "{", in trailingTrivia);
            }
			case '}':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.CloseBraceToken, "}", in trailingTrivia);
            }
			case '[':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.OpenBracketToken, "[", in trailingTrivia);
            }
			case ']':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.CloseBracketToken, "]", in trailingTrivia);
            }
			case ':':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.ColonToken, ":", in trailingTrivia);
            }
			case ';':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.SemicolonToken, ";", in trailingTrivia);
            }
			case '?':
            {
	            Advance();
	            var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
	            return CreateToken(in leadingTrivia, in SyntaxKind.QuestionMarkToken, "?", in trailingTrivia);
            }
	        case '@':
	        {
		        Advance();
		        var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
		        return CreateToken(in leadingTrivia, in SyntaxKind.AtToken, "@", in trailingTrivia);
	        }
			case '^':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.CaretEqualsToken, "^=", in trailingTrivia);
					}

					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.CaretToken, "^", in trailingTrivia);
					}
				}
			case '~':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.TildeEqualsToken, "~=", in trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.TildeEqualsToken, "~", in trailingTrivia);
					}
				}
			case '+':
				Advance();
				switch (Current)
				{
					case '+':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PlusPlusToken, "++", in trailingTrivia);
					}
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PlusEqualsToken, "+=", in trailingTrivia);
					}
					default:
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PlusToken, "+", in trailingTrivia);
					}
				}
			case '-':
				Advance();
				switch (Current)
				{
					case '-':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.MinusMinusToken, "--", in trailingTrivia);
					}
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.MinusEqualsToken, "-=", in trailingTrivia);
					}
					case '>':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.ArrowToken, "->", in trailingTrivia);
					}
					default:            
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.MinusToken, "-", in trailingTrivia);
					}
				}
			case '/':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.FSlashEqualsToken, "/=", in trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.FSlashToken, "/", in trailingTrivia);
					}
				}
			case '*':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.AsteriskEqualsToken, "*=", in trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.AsteriskToken, "*", in trailingTrivia);
					}
				}
			case '%':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PercentEqualsToken, "%=", in trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PercentToken, "%", in trailingTrivia);
					}
				}
			case '=':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.EqualsEqualsToken, "==", in trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.EqualsToken, "=", in trailingTrivia);
					}
				}
			case '>':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.GreaterEqualsToken, ">=", in trailingTrivia);
					}
					case '>':
						Advance();
						if (Current == '=')
						{
							Advance();
							var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
							return CreateToken(in leadingTrivia, in SyntaxKind.GreaterGreaterEqualsToken, ">>=", in trailingTrivia);
						}
						else
						{
							var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
							return CreateToken(in leadingTrivia, in SyntaxKind.GreaterGreaterToken, ">>", in trailingTrivia);
						}	
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.GreaterToken, ">", in trailingTrivia);
					}
				}
			case '<':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.LessEqualsToken, "<=", in trailingTrivia);
					}
					case '<':
						Advance();
						if (Current == '=')
						{
							Advance();
							var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
							return CreateToken(in leadingTrivia, in SyntaxKind.LessLessEqualsToken, "<<=", in trailingTrivia);
						}
						else
						{
							var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
							return CreateToken(in leadingTrivia, in SyntaxKind.LessLessToken, "<<", in trailingTrivia);
						}	
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.LessToken, "<", in trailingTrivia);
					}
				}
			case '!':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.BangEqualsToken, "!=", in trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.BangToken, "!", in trailingTrivia);
					}
				}
			case '|':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PipeEqualsToken, "!=", in trailingTrivia);
					}
					case '|':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PipePipeToken, "||", in trailingTrivia);
					}
					default:
					{
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.PipeToken, "|", in trailingTrivia);
					}
				}
			case '&':
				Advance();
				switch (Current)
				{
					case '=':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.AmpersandEqualsToken, "&=", in trailingTrivia);
					}	
					case '&':
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.AmpersandAmpersandToken, "&&", in trailingTrivia);
					}
					default:
					{
						Advance();
						var trailingTrivia = ScanSyntaxTrivia(TriviaKind.Trailing);
						return CreateToken(in leadingTrivia, in SyntaxKind.AmpersandToken, "&", in trailingTrivia);
					}
				}
	        case '"':
	        {
		        return ScanStringLiteral('"', in leadingTrivia);
	        }
	        case '\'':
	        {
		        return ScanCharLiteral('\'', in leadingTrivia);
	        }
	        case var letter when letter.IsIdentifierStartCharacter():
	        {
		        return ScanIdentifierOrKeyword(in leadingTrivia);
	        }
	        case var digit when digit.IsQualifiedDigit():
	        {
		        return ScanNumericLiteral(in leadingTrivia);
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
        return token ?? CreateToken(in leadingTrivia, in SyntaxKind.BadToken, "ERROR", ImmutableArray<SyntaxTrivia>.Empty);
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
	    var trailing = ScanSyntaxTrivia(TriviaKind.Trailing);
	    return CreateToken(in leading, in SyntaxKind.IdentifierToken, in text, in text, in trailing);
    }
    
    private ISyntaxToken ScanIdentifierOrKeyword(in IReadOnlyList<SyntaxTrivia> leading)
    {
	    m_start = m_position;
	    while ((char.IsLetterOrDigit(Current) || Current == '_') && !IsAtEnd)
	    {
		    Advance();
	    }

	    var text = GetFullSpan();
	    var kind = m_syntaxCache.LookupKeyword(in text);
	    var trailing = ScanSyntaxTrivia(TriviaKind.Trailing);
	    if (kind != SyntaxKind.TrueKeyword && kind != SyntaxKind.FalseKeyword)
	    {
		    return CreateToken(in leading, in kind, in text, in text, in trailing);
	    }

	    return text == "true"
		    ? CreateToken(in leading, in kind, in text, true, in trailing)
		    : CreateToken(in leading, in kind, in text, false, in trailing);
    }

    private char Peek(int offset = 0)
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
    
    private void Advance(int amount = 1)
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