namespace VSharp.Core.Analysis.Syntax;

public enum SyntaxKind : ushort
{ 
	None = 0,
	
	BadToken,
	EndOfFileToken,
	WhiteSpaceToken,
	NewLineToken,
	SingleLineCommentToken,
	MultiLineCommentToken,

	OpenParenToken, // (
	CloseParenToken, // )
	OpenBraceToken, // {
	CloseBraceToken,  // }
	OpenBracketToken, // [
	CloseBracketToken, // ]

	// Unary
	// these are also unary in single + or - form
	TildeToken, // ~
	BangToken, // !
	PlusPlusToken, // ++
	MinusMinusToken, // --

	// Binary operators
	PlusToken, // +
	MinusToken, // -
	AsteriskToken, // *
	FSlashToken, // /
	PercentToken, // %
	CaretToken, // ^
	QuestionMarkToken, // ?

	// Compound assignment
	CaretEqualsToken, // ^=
	PlusEqualsToken, // += 
	MinusEqualsToken, // -= 
	AsteriskEqualsToken, // *= 
	FSlashEqualsToken, // /=
	PercentEqualsToken, // *=
	PipeEqualsToken, // |=
	AmpersandEqualsToken, // &=
	LessLessEqualsToken, // <<=
	GreaterGreaterEqualsToken, // >>= 
	TildeEqualsToken, // ~=

	BangEqualsToken, // != 
	EqualsToken, // =
	EqualsEqualsToken, // ==
	LessToken, // < 
	LessEqualsToken, // <=
	GreaterToken, // >
	GreaterEqualsToken, // >=
	PipeToken, // |
	PipePipeToken, // ||
	AmpersandToken, // &
	AmpersandAmpersandToken, // &&

	// Shifts
	LessLessToken, // << 
	GreaterGreaterToken, // >>

	// Separators
	DotToken, // . 
	CommaToken, // ,
	ColonToken, // :
	SemicolonToken, // ;

	// Compiler reserved keywords
	TypeOfKeyword, // typeof
	NameOfKeyword, // nameof
	SizeOfKeyword, // sizeof
	NewKeyword, // new

	PublicKeyword, // public
	PrivateKeyword, // private

	ClassKeyword, // class
	StructKeyword, // struct
	StaticKeyword, // static
	ImmutableKeyword,
	MutableKeyword,
	ThisKeyword,
	ValueKeyword,

	NilKeyword, // nil
	TrueKeyword, // true
	FalseKeyword, // false

	ObjectKeyword,
	StringKeyword, // string
	CharKeyword, // char
	BoolKeyword, // bool 

	Int8Keyword, // int8 
	UInt8Keyword, // uint8 
	Int16Keyword, // int16
	UInt16Keyword, // uint16
	Int32Keyword, // int32 
	UInt32Keyword, // uint32 
	Int64Keyword, // int64 
	UInt64Keyword, // uint64

	Float32Keyword, // float32
	Float64Keyword, // float64 

	// User defined identifiers, such as:
	// "class Foo {}", or "int8 bar;", this would store just "Foo" or "bar",
	// along with some necessities to bind the identifier properly
	IdentifierToken,

	// Literal values
	// Lexing is the only time we have the easiest access to the literals true type,
	// assigning it to a true literal token will allow us to any_cast<T> the type to
	// the proper .NET type for binding/compilation. Switching enums is also cheaper
	// than type validation :)
	StringLiteralToken,
	CharLiteralToken,
	BoolLiteralToken,

	Int8LiteralToken,
	UInt8LiteralToken,
	Int16LiteralToken,
	UInt16LiteralToken,
	Int32LiteralToken,
	UInt32LiteralToken,
	Int64LiteralToken,
	UInt64LiteralToken,

	Float32LiteralToken,
	Float64LiteralToken,
	
	BinaryExpression,
	NumberExpression,
	ParenthesizedExpression,
	LiteralExpression,
	UnaryExpression,
	NameExpression,
	AssignmentExpression

}