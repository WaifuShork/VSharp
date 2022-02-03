namespace VSharp.Core.Analysis.Syntax;

using System.Globalization;

[PublicAPI]
public class SyntaxCache
{
	private readonly CultureInfo m_compilerCulture;

	public SyntaxCache(in CultureInfo? compilerCulture = default)
	{
		// We always want to use the US English representation for ToLower and ToUpper for searching 
		// with keywords and using compiler defined keywords
		m_compilerCulture = compilerCulture ?? CultureInfo.GetCultureInfo("en-us");
	}
	
	public bool IsUserOrPredefinedKind(in ISyntaxToken token)
	{
		if (s_keywordCache.Value.TryGetValue(token.Text, out _))
		{
			return true;
		}

		return token.Kind == SyntaxKind.IdentifierToken;
	}

	public SyntaxKind LookupKeyword(in string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return SyntaxKind.BadToken;
		}
		
		if (s_keywordCache.Value.TryGetValue(input, out var keyword))
		{
			return keyword;
		}

		return SyntaxKind.IdentifierToken;
	}

	public string? LookupText(in SyntaxKind kind)
	{
		if (s_stringCache.Value.TryGetValue(kind, out var text))
		{
			return text;
		}

		return null;
	}

	public int GetBinaryPrecedence(in SyntaxKind kind)
	{
		if (m_binaryPrecedences.Value.TryGetValue(kind, out var precedence))
		{
			return precedence;
		}

		return 0;
	}
	
	public int GetUnaryPrecedence(in SyntaxKind kind)
	{
		if (m_unaryPrecedences.Value.TryGetValue(kind, out var precedence))
		{
			return precedence;
		}

		return 0;
	}
	
	// <op> <right>
	private readonly Lazy<Dictionary<SyntaxKind, int>> m_unaryPrecedences = new(() => new Dictionary<SyntaxKind, int>
	{
		{ SyntaxKind.PlusToken, 15 },
		{ SyntaxKind.PlusPlusToken, 15 },
		{ SyntaxKind.MinusToken, 15 },
		{ SyntaxKind.MinusMinusToken, 15 },
		{ SyntaxKind.BangToken, 15 },
		{ SyntaxKind.TildeToken, 15 },
		
		{ SyntaxKind.IdentifierToken, 0 }
		
	}, LazyThreadSafetyMode.ExecutionAndPublication);
	
	// <left> <op> <right>
	private readonly Lazy<Dictionary<SyntaxKind, int>> m_binaryPrecedences = new(() => new Dictionary<SyntaxKind, int>
	{
		{ SyntaxKind.AsteriskToken, 13 },
		{ SyntaxKind.FSlashToken, 13 },
		{ SyntaxKind.PercentToken, 13 },
		
		{ SyntaxKind.PlusToken, 12 },
		{ SyntaxKind.MinusToken, 12 },
		
		{ SyntaxKind.LessLessToken, 11 },
		{ SyntaxKind.GreaterGreaterToken, 11 },
		
		{ SyntaxKind.LessToken, 9 },
		{ SyntaxKind.LessEqualsToken, 9 },
		{ SyntaxKind.GreaterToken, 9 },
		{ SyntaxKind.GreaterEqualsToken, 9 },
		
		{ SyntaxKind.EqualsEqualsToken, 8 },
		{ SyntaxKind.EqualsEqualsEqualsToken, 8 },
		{ SyntaxKind.BangEqualsEqualsToken, 8 },
		{ SyntaxKind.BangEqualsToken, 8 },
		
		{ SyntaxKind.AmpersandToken, 7 },
		
		{ SyntaxKind.CaretToken, 6 },
		
		{ SyntaxKind.PipeToken, 5 },
		
		{ SyntaxKind.AmpersandAmpersandToken, 4 },
		
		{ SyntaxKind.PipePipeToken, 3 },
		{ SyntaxKind.QuestionMarkToken, 2 },
		
		{ SyntaxKind.IdentifierToken, 0 }
		
	}, LazyThreadSafetyMode.ExecutionAndPublication);

	private readonly Lazy<Dictionary<SyntaxKind, string>> s_stringCache = new(() => new Dictionary<SyntaxKind, string>
	{
		{ SyntaxKind.OpenParenToken, "(" },
		{ SyntaxKind.CloseParenToken, ")" },
		{ SyntaxKind.OpenBraceToken, "{" },
		{ SyntaxKind.CloseBraceToken, "}" },
		{ SyntaxKind.OpenBracketToken, "[" },
		{ SyntaxKind.CloseBracketToken, "]" },

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
		{ SyntaxKind.EqualsEqualsEqualsToken, "==="},
		{ SyntaxKind.BangEqualsEqualsToken, "!=="},
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
		
		{ SyntaxKind.ArrowToken, "->" },
		{ SyntaxKind.AtToken, "@" },
	
		{ SyntaxKind.TypeOfKeyword, "typeof"},
		{ SyntaxKind.NameOfKeyword, "nameof"},
		{ SyntaxKind.SizeOfKeyword, "sizeof"},
		{ SyntaxKind.NewKeyword, "new"},
		{ SyntaxKind.LetKeyword, "let"},
		{ SyntaxKind.ConstKeyword, "const"},

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
		{ SyntaxKind.GenericKeyword, "generic"},
		{ SyntaxKind.WhenKeyword, "when"},
		{ SyntaxKind.ConstructorKeyword, "ctor"},
	
		{ SyntaxKind.Int8Keyword, "int8"},
		{ SyntaxKind.UInt8Keyword, "uint8"},
		{ SyntaxKind.Int16Keyword, "int16"},
		{ SyntaxKind.UInt16Keyword, "uint16"},
		{ SyntaxKind.Int32Keyword, "int32"},
		{ SyntaxKind.UInt32Keyword, "uint32"},
		{ SyntaxKind.Int64Keyword, "int64"},
		{ SyntaxKind.UInt64Keyword, "uint64"},
		{ SyntaxKind.InfinityIntKeyword, "infint"},
		
		{ SyntaxKind.Float32Keyword, "float32"},
		{ SyntaxKind.Float64Keyword, "float64"},
		
		{ SyntaxKind.VarKeyword, "var"},
		{ SyntaxKind.UseKeyword, "use"},
		{ SyntaxKind.FromKeyword, "from"},
		{ SyntaxKind.ModuleKeyword, "module"},
		{ SyntaxKind.EnumKeyword, "enum"},
		
	}, LazyThreadSafetyMode.ExecutionAndPublication);
	
	private readonly Lazy<Dictionary<string, SyntaxKind>> s_keywordCache = new(() => new Dictionary<string, SyntaxKind>
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
		{ "enum", SyntaxKind.EnumKeyword },
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
		{ "infint", SyntaxKind.InfinityIntKeyword },

		{ "float32", SyntaxKind.Float32Keyword },
		{ "float64", SyntaxKind.Float64Keyword },

		{ "object", SyntaxKind.ObjectKeyword },
		{ "string", SyntaxKind.StringKeyword },
		{ "char", SyntaxKind.CharKeyword },
		{ "bool", SyntaxKind.BoolKeyword },
		{ "true", SyntaxKind.TrueKeyword },
		{ "false", SyntaxKind.FalseKeyword },
		
		{ "var", SyntaxKind.VarKeyword },
		{ "use", SyntaxKind.UseKeyword },
		{ "from", SyntaxKind.FromKeyword },
		{ "module", SyntaxKind.ModuleKeyword },
		{ "generic", SyntaxKind.GenericKeyword },
		{ "when", SyntaxKind.WhenKeyword },
		{ "ctor", SyntaxKind.ConstructorKeyword },
		{ "alloc", SyntaxKind.AllocKeyword },
		{ "heap", SyntaxKind.HeapKeyword },
		{ "stack", SyntaxKind.StackKeyword },
	}, LazyThreadSafetyMode.PublicationOnly);
}