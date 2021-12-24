using System.Runtime.InteropServices;

namespace VSharp.Core.Analysis.Syntax;

public class SyntaxCache
{
	public SyntaxKind LookupKeyword(in string input)
	{
		try
		{
			if (s_keywordCache.Value.TryGetValue(input, out var keyword))
			{
				return keyword;
			}

			return SyntaxKind.IdentifierToken;
		}
		// throws:
		// - MissingMemberException
		// - MemberAccessException
		// - InvalidOperationException
		catch (Exception)
		{
			// the only time this will ever be returned, is if there's a very big threading issue with the ThreadedLexer
			return SyntaxKind.BadToken;
		}
	}

	public string? LookupText(SyntaxKind kind)
	{
		try
		{
			if (s_stringCache.Value.TryGetValue(kind, out var text))
			{
				return text;
			}

			return null;
		}
		// throws:
		// - MissingMemberException
		// - MemberAccessException
		// - InvalidOperationException
		catch (Exception)
		{
			return null;
		}
	}

	public readonly Lazy<Dictionary<SyntaxKind, string>> s_stringCache = new(() => new Dictionary<SyntaxKind, string>
	{
		{SyntaxKind.OpenParenToken, "("},
		{SyntaxKind.CloseParenToken, ")"},
		{SyntaxKind.OpenBraceToken, "{"},
		{SyntaxKind.CloseBraceToken, "}"},
		{SyntaxKind.OpenBracketToken, "["},
		{SyntaxKind.CloseBracketToken, "]"},

		{ SyntaxKind.TildeToken, "~"},
		{ SyntaxKind.BangToken, "!"},
		{ SyntaxKind.PlusPlusToken, "++"},
		{ SyntaxKind.MinusMinusToken, "--"},
		
		{ SyntaxKind.PlusToken, "+"},
		{ SyntaxKind.MinusToken, "-"},
		{ SyntaxKind.AsteriskToken, "*"},
		{ SyntaxKind.FSlashToken, "/"},
		{ SyntaxKind.PercentToken, "%"},
		{ SyntaxKind.CaretToken, "^"},
		{ SyntaxKind.QuestionMarkToken, "?"},
		
		{ SyntaxKind.CaretEqualsToken, "^="},
		{ SyntaxKind.PlusEqualsToken, "+="},
		{ SyntaxKind.MinusEqualsToken, "-="},
		{ SyntaxKind.AsteriskEqualsToken, "*="},
		{ SyntaxKind.FSlashEqualsToken, "/="},
		{ SyntaxKind.PercentEqualsToken, "%="},
		{ SyntaxKind.PipeEqualsToken, "|="},
		{ SyntaxKind.AmpersandEqualsToken, "&="},
		{ SyntaxKind.LessLessEqualsToken, "<<="},
		{ SyntaxKind.GreaterGreaterEqualsToken, ">>="},
		{ SyntaxKind.TildeEqualsToken, "~="},
		
		{ SyntaxKind.BangEqualsToken, "!="},
		{ SyntaxKind.EqualsToken, "="},
		{ SyntaxKind.EqualsEqualsToken, "=="},
		{ SyntaxKind.LessToken, "<"},
		{ SyntaxKind.LessEqualsToken, "<="},
		{ SyntaxKind.GreaterToken, ">"},
		{ SyntaxKind.GreaterEqualsToken, ">="},
		{ SyntaxKind.PipeToken, "|"},
		{ SyntaxKind.PipePipeToken, "||"},
		{ SyntaxKind.AmpersandToken, "&"},
		{ SyntaxKind.AmpersandAmpersandToken, "&&"},
		
		{ SyntaxKind.LessLessToken, "<<"},
		{ SyntaxKind.GreaterGreaterToken, ">>"},
	
		{ SyntaxKind.DotToken, "."},
		{ SyntaxKind.CommaToken, ","},
		{ SyntaxKind.ColonToken, ":"},
		{ SyntaxKind.SemicolonToken, ";"},
	
		{ SyntaxKind.TypeOfKeyword, "typeof"},
		{ SyntaxKind.NameOfKeyword, "nameof"},
		{ SyntaxKind.SizeOfKeyword, "sizeof"},
		{ SyntaxKind.NewKeyword, "new"},
	
		{ SyntaxKind.PublicKeyword, "public"},
		{ SyntaxKind.PrivateKeyword, "private"},
		
		{ SyntaxKind.ClassKeyword, "class"},
		{ SyntaxKind.StructKeyword, "struct"},
		{ SyntaxKind.StaticKeyword, "static"},
		{ SyntaxKind.ImmutableKeyword, "immutable"},
		{ SyntaxKind.MutableKeyword, "mutable"},
		{ SyntaxKind.ThisKeyword, "this"},
		{ SyntaxKind.ValueKeyword, "value"},
		
		{ SyntaxKind.NilKeyword, "nil"},
		{ SyntaxKind.TrueKeyword, "true"},
		{ SyntaxKind.FalseKeyword, "false"},
		
		{ SyntaxKind.ObjectKeyword, "object"},
		{ SyntaxKind.StringKeyword, "string"},
		{ SyntaxKind.CharKeyword, "char"},
		{ SyntaxKind.BoolKeyword, "bool"},
	
		{ SyntaxKind.Int8Keyword, "int8"},
		{ SyntaxKind.UInt8Keyword, "uint8"},
		{ SyntaxKind.Int16Keyword, "int16"},
		{ SyntaxKind.UInt16Keyword, "uint16"},
		{ SyntaxKind.Int32Keyword, "int32"},
		{ SyntaxKind.UInt32Keyword, "uint32"},
		{ SyntaxKind.Int64Keyword, "int64"},
		{ SyntaxKind.UInt64Keyword, "uint64"},
		
		{ SyntaxKind.Float32Keyword, "float32"},
		{ SyntaxKind.Float64Keyword, "float64"},
		
	}, LazyThreadSafetyMode.ExecutionAndPublication);
	
	public readonly Lazy<Dictionary<string, SyntaxKind>> s_keywordCache = new(() => new Dictionary<string, SyntaxKind>
	{
		{ "typeof", SyntaxKind.TypeOfKeyword },
		{ "nameof", SyntaxKind.NameOfKeyword },
		{ "sizeof", SyntaxKind.SizeOfKeyword },
		{ "new", SyntaxKind.NewKeyword },

		{ "public", SyntaxKind.PublicKeyword },
		{ "private", SyntaxKind.PrivateKeyword },
		{ "immutable", SyntaxKind.ImmutableKeyword },
		{ "mutable", SyntaxKind.MutableKeyword },
		{ "this", SyntaxKind.ThisKeyword },
		{ "nil", SyntaxKind.NilKeyword },
		{ "class", SyntaxKind.ClassKeyword },
		{ "struct", SyntaxKind.StructKeyword },
		{ "value", SyntaxKind.ValueKeyword },
		{ "static", SyntaxKind.StaticKeyword },

		{ "int8", SyntaxKind.Int8Keyword },
		{ "uint8", SyntaxKind.UInt8Keyword },
		{ "int16", SyntaxKind.Int16Keyword },
		{ "uint16", SyntaxKind.UInt16Keyword },
		{ "int32", SyntaxKind.Int32Keyword },
		{ "uint32", SyntaxKind.UInt32Keyword },
		{ "int64", SyntaxKind.Int64Keyword },
		{ "uint64", SyntaxKind.UInt64Keyword },

		{ "float32", SyntaxKind.Float32Keyword },
		{ "float64", SyntaxKind.Float64Keyword },

		{ "object", SyntaxKind.ObjectKeyword },
		{ "string", SyntaxKind.StringKeyword },
		{ "char", SyntaxKind.CharKeyword },
		{ "bool", SyntaxKind.BoolKeyword },
		{ "true", SyntaxKind.TrueKeyword },
		{ "false", SyntaxKind.FalseKeyword },
	}, LazyThreadSafetyMode.PublicationOnly);
}