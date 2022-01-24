namespace VSharp.Core.Analysis.Syntax;

// ====== Operator Precedence Chart ====== 
// some operators might not be currently supported, this is just 
// a general chart/guideline for what all operators could look like
// with their respective precedences for all situations
// 16: a++, a--
// 15: ++a, --a, +a, -a, !, ~ 
// 14: .*, ->*
// 13: a*b, a/b, a%b
// 12: a+b, a-b
// 11: <<, >>
// 10: <=>
//  9: <, <=, >, >= 
//  8: ==, !=
//  7: a&b
//  6: ^
//  5: |
//  4: &&
//  3: ||
//  2: a?b:c, =, +=, -=, *=, /-, %=, <<=, >>=, &=, ^=, |=
//  1: ,

[PublicAPI]
public static class SyntaxFacts
{
	public static bool IsNumberLiteral(this SyntaxKind kind)
	{
		return (SyntaxKind.Int8LiteralToken |
		        SyntaxKind.UInt8LiteralToken |
		        SyntaxKind.Int16LiteralToken |
		        SyntaxKind.UInt16LiteralToken |
		        SyntaxKind.Int32LiteralToken |
		        SyntaxKind.UInt32LiteralToken |
		        SyntaxKind.Int64LiteralToken |
		        SyntaxKind.UInt64LiteralToken |
		        SyntaxKind.Float32LiteralToken |
		        SyntaxKind.Float64LiteralToken).HasFlag(kind);
	}

	public static bool IsPredefinedType(this SyntaxKind kind)
	{
		return (SyntaxKind.Int8Keyword |
		        SyntaxKind.UInt8Keyword |
		        SyntaxKind.Int16Keyword |
		        SyntaxKind.UInt16Keyword |
		        SyntaxKind.Int32Keyword |
		        SyntaxKind.UInt32Keyword |
		        SyntaxKind.Int64Keyword |
		        SyntaxKind.UInt64Keyword |
		        SyntaxKind.Float32Keyword |
		        SyntaxKind.Float64Keyword |
		        SyntaxKind.StringKeyword |
		        SyntaxKind.CharKeyword |
		        SyntaxKind.ObjectKeyword).HasFlag(kind);
	}
}