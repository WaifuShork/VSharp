namespace VSharp.Core.Analysis.Syntax;

using System.Numerics;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

[PublicAPI]
[TypeConverter(typeof(SyntaxKindConverter))]
public struct SyntaxKind : IEquatable<SyntaxKind>, IComparable<SyntaxKind>, IComparable, IConvertible
{
	private static readonly List<FieldInfo> s_fields;
	private static readonly List<SyntaxKind> s_fieldValues;

	private const bool c_zeroInit = true;
	public BigInteger Value { get; private set; }
	
	/// <summary>
	/// Creates a value taking ZeroInit into consideration.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	private static BigInteger CreateValue(int index)
	{
		if(c_zeroInit && index == 0)
		{
			return 0;
		}
		
		var idx = c_zeroInit ? index - 1 : index;
		return new BigInteger(1) << idx;
	}

	static SyntaxKind()
	{
		s_fields = typeof(SyntaxKind).GetFields(BindingFlags.Public | BindingFlags.Static).ToList();
		s_fieldValues = new List<SyntaxKind>();
		
		for(var i = 0; i < s_fields.Count; i++)
		{
			var field = s_fields[i];
			var fieldVal = new SyntaxKind
			{
				Value = CreateValue(i)
			};
			
			field.SetValue(null, fieldVal);
			s_fieldValues.Add(fieldVal);
		}
	}

	/// <summary>
	/// OR operator. Or together BigFlags instances.
	/// </summary>
	/// <param name="lhs"></param>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public static SyntaxKind operator |(SyntaxKind lhs, SyntaxKind rhs)
	{
		return new SyntaxKind
		{
			Value = lhs.Value | rhs.Value
		};
	}

	/// <summary>
	/// AND operator. And together BigFlags instances.
	/// </summary>
	/// <param name="lhs"></param>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public static SyntaxKind operator &(SyntaxKind lhs, SyntaxKind rhs)
	{
		return new SyntaxKind
		{
			Value = lhs.Value & rhs.Value
		};
	}

	/// <summary>
	/// XOR operator. Xor together BigFlags instances.
	/// </summary>
	/// <param name="lhs"></param>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public static SyntaxKind operator ^(SyntaxKind lhs, SyntaxKind rhs)
	{
		return new SyntaxKind
		{
			Value = lhs.Value ^ rhs.Value
		};
	}

	/// <summary>
	/// Equality operator.
	/// </summary>
	/// <param name="lhs"></param>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public static bool operator ==(SyntaxKind lhs, SyntaxKind rhs)
	{
		return lhs.Equals(rhs);
	}

	/// <summary>
	/// Inequality operator.
	/// </summary>
	/// <param name="lhs"></param>
	/// <param name="rhs"></param>
	/// <returns></returns>
	public static bool operator !=(SyntaxKind lhs, SyntaxKind rhs)
	{
		return !(lhs == rhs);
	}

	/// <summary>
	/// Overridden. Returns a comma-separated string.
	/// </summary>
	/// <returns></returns>
	public override string ToString()
	{
		if(c_zeroInit && Value == 0)
		{
			return s_fields[0].Name;
		}
		
		var names = new List<string>();
		for(var i = 0; i < s_fields.Count; i++)
		{
			if (c_zeroInit && i == 0)
			{
				continue;
			}

			var value = CreateValue(i);
			if ((Value & value) == value)
			{
				names.Add(s_fields[i].Name);
			}
		}

		return string.Join(", ", names);
	}

	/// <summary>
	/// Overridden. Compares equality with another object.
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public override bool Equals(object? obj)
	{
		if(obj is SyntaxKind)
		{
			return Equals((SyntaxKind)obj);
		}

		return false;
	}

	/// <summary>
	/// Overridden. Gets the hash code of the internal BitArray.
	/// </summary>
	/// <returns></returns>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	/// <summary>
	/// Strongly-typed equality method.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool Equals(SyntaxKind other)
	{
		return Value == other.Value;
	}

	/// <summary>
	/// Compares based on highest bit set. Instance with higher
	/// bit set is bigger.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public int CompareTo(SyntaxKind other)
	{
		return Value.CompareTo(other.Value);
	}

	int IComparable.CompareTo(object? obj)
	{
		if(obj is SyntaxKind)
		{
			return CompareTo((SyntaxKind)obj);
		}

		return -1;
	}

	/// <summary>
	/// Checks <paramref name="flags"/> to see if all the bits set in
	/// that flags are also set in this flags.
	/// </summary>
	/// <param name="flags"></param>
	/// <returns></returns>
	public bool HasFlag(in SyntaxKind flags)
	{
		return (this & flags) == flags;
	}

	/// <summary>
	/// Gets the names of this BigFlags enumerated type.
	/// </summary>
	/// <returns></returns>
	public static string[] GetNames()
	{
		return s_fields.Select(x => x.Name).ToArray();
	}

	/// <summary>
	/// Gets all the values of this BigFlags enumerated type.
	/// </summary>
	/// <returns></returns>
	public static SyntaxKind[] GetValues()
	{
		return s_fieldValues.ToArray();
	}

	/// <summary>
	/// Standard TryParse pattern. Parses a BigFlags result from a string.
	/// </summary>
	/// <param name="s"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool TryParse(string? s, out SyntaxKind result)
	{
		result = new SyntaxKind();
		if (string.IsNullOrWhiteSpace(s))
		{
			return true;
		}

		var fieldNames = s.Split(',');
		foreach(var fieldName in fieldNames)
		{
			var field = s_fields.FirstOrDefault(x => string.Equals(x.Name, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));
			if(null == field)
			{
				result = new SyntaxKind();
				return false;
			}
			
			var i = s_fields.IndexOf(field);
			result.Value |= CreateValue(i);
		}

		return true;
	}
	
	/// <summary>
	/// Returns TypeCode.Object.
	/// </summary>
	/// <returns></returns>
	public TypeCode GetTypeCode()
	{
		return TypeCode.Object;
	}

	bool IConvertible.ToBoolean(IFormatProvider? provider)
	{
		throw new NotSupportedException();
	}

	byte IConvertible.ToByte(IFormatProvider? provider)
	{
		return Convert.ToByte(Value, provider);
	}

	char IConvertible.ToChar(IFormatProvider? provider)
	{
		throw new NotSupportedException();
	}

	DateTime IConvertible.ToDateTime(IFormatProvider? provider)
	{
		throw new NotSupportedException();
	}

	decimal IConvertible.ToDecimal(IFormatProvider? provider)
	{
		return Convert.ToDecimal(Value, provider);
	}

	double IConvertible.ToDouble(IFormatProvider? provider)
	{
		return Convert.ToDouble(Value, provider);
	}

	short IConvertible.ToInt16(IFormatProvider? provider)
	{
		return Convert.ToInt16(Value, provider);
	}

	int IConvertible.ToInt32(IFormatProvider? provider)
	{
		return Convert.ToInt32(Value, provider);
	}

	long IConvertible.ToInt64(IFormatProvider? provider)
	{
		return Convert.ToInt64(Value, provider);
	}

	sbyte IConvertible.ToSByte(IFormatProvider? provider)
	{
		return Convert.ToSByte(Value, provider);
	}

	float IConvertible.ToSingle(IFormatProvider? provider)
	{
		return Convert.ToSingle(Value, provider);
	}

	string IConvertible.ToString(IFormatProvider? provider)
	{
		return ToString();
	}

	object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
	{
		var tc = TypeDescriptor.GetConverter(this);
		return tc.ConvertTo(this, conversionType)!;
	}

	ushort IConvertible.ToUInt16(IFormatProvider? provider)
	{
		return Convert.ToUInt16(Value, provider);
	}

	uint IConvertible.ToUInt32(IFormatProvider? provider)
	{
		return Convert.ToUInt32(Value, provider);
	}

	ulong IConvertible.ToUInt64(IFormatProvider? provider)
	{
		return Convert.ToUInt64(Value, provider);
	}
	
	// Enum Values
	// ReSharper disable UnassignedReadonlyField
	public static readonly SyntaxKind None;
	public static readonly SyntaxKind BadToken;
	public static readonly SyntaxKind EndOfFileToken;
	public static readonly SyntaxKind LeadingTrivia;
	public static readonly SyntaxKind TrailingTrivia;
	public static readonly SyntaxKind WhiteSpaceToken;
	public static readonly SyntaxKind NewLineToken;
	public static readonly SyntaxKind SingleLineCommentToken;
	public static readonly SyntaxKind MultiLineCommentToken;
	public static SyntaxKind AllComments => SingleLineCommentToken | MultiLineCommentToken;
	
	// Brackets
	public static readonly SyntaxKind OpenParenToken;
	public static readonly SyntaxKind CloseParenToken;
	public static readonly SyntaxKind OpenBraceToken;
	public static readonly SyntaxKind CloseBraceToken;
	public static readonly SyntaxKind OpenBracketToken;
	public static readonly SyntaxKind CloseBracketToken;
	public static SyntaxKind AllBrackets => OpenParenToken | CloseParenToken 
	                                      | OpenBraceToken | CloseBraceToken 
	                                      | OpenBracketToken | CloseBracketToken;
	
	
	public static readonly SyntaxKind DotToken;
	public static readonly SyntaxKind CommaToken;	
	public static readonly SyntaxKind ArrowToken;

	public static readonly SyntaxKind AtToken;
	public static readonly SyntaxKind ColonToken;
	public static readonly SyntaxKind SemicolonToken;
	
	/*
	 * NOTICE:
	 * Some unary tokens are *also* valid binary tokens, so I'm not able to perfectly organize
	 */
	// Unary Tokens
	public static readonly SyntaxKind TildeToken;
	public static readonly SyntaxKind BangToken;
	public static readonly SyntaxKind PlusPlusToken;
	public static readonly SyntaxKind MinusMinusToken;
	public static SyntaxKind AllUnaryOperators => TildeToken      | BangToken | PlusPlusToken
	                                            | MinusMinusToken | PlusToken | MinusToken;

	public static SyntaxKind AllPostfix => PlusPlusToken | MinusMinusToken;
	public static SyntaxKind AllPrefix => PlusPlusToken | MinusMinusToken;

	// Binary Tokens
	public static readonly SyntaxKind CaretToken;
	public static readonly SyntaxKind PlusToken;
	public static readonly SyntaxKind MinusToken;
	public static readonly SyntaxKind AsteriskToken;
	public static readonly SyntaxKind FSlashToken;
	public static readonly SyntaxKind PercentToken;
	public static readonly SyntaxKind QuestionMarkToken;
	public static readonly SyntaxKind BangEqualsToken;
	public static readonly SyntaxKind EqualsToken;
	public static readonly SyntaxKind EqualsEqualsToken;
	public static readonly SyntaxKind LessToken;
	public static readonly SyntaxKind LessEqualsToken;
	public static readonly SyntaxKind GreaterToken;
	public static readonly SyntaxKind GreaterEqualsToken;
	public static readonly SyntaxKind PipeToken;
	public static readonly SyntaxKind PipePipeToken;
	public static readonly SyntaxKind AmpersandToken;
	public static readonly SyntaxKind AmpersandAmpersandToken;
	public static readonly SyntaxKind LessLessToken;
	public static readonly SyntaxKind GreaterGreaterToken;
	public static SyntaxKind AllBinaryOperators => CaretToken         | PlusToken			    | MinusToken 
	                                             | AsteriskToken      | FSlashToken			    | PercentToken 
												 | QuestionMarkToken  | BangEqualsToken		    | EqualsEqualsToken 
												 | LessToken          | LessEqualsToken		    | GreaterToken 
												 | GreaterEqualsToken | PipeToken			    | PipePipeToken 
												 | AmpersandToken     | AmpersandAmpersandToken | LessLessToken 
												 | GreaterGreaterToken;
	
	// Compound Tokens
	public static readonly SyntaxKind CaretEqualsToken;
	public static readonly SyntaxKind PlusEqualsToken;
	public static readonly SyntaxKind MinusEqualsToken;
	public static readonly SyntaxKind AsteriskEqualsToken;
	public static readonly SyntaxKind FSlashEqualsToken;
	public static readonly SyntaxKind PercentEqualsToken;
	public static readonly SyntaxKind PipeEqualsToken;
	public static readonly SyntaxKind AmpersandEqualsToken;
	public static readonly SyntaxKind LessLessEqualsToken;
	public static readonly SyntaxKind GreaterGreaterEqualsToken;
	public static readonly SyntaxKind TildeEqualsToken;
	public readonly SyntaxKind AllCompoundOperators => CaretEqualsToken          | PlusEqualsToken      | MinusEqualsToken 
													 | AsteriskEqualsToken       | FSlashEqualsToken    | PercentEqualsToken 
													 | PipeEqualsToken           | AmpersandEqualsToken | LessLessEqualsToken 
													 | GreaterGreaterEqualsToken | TildeEqualsToken;
	
	// Keywords
	public static readonly SyntaxKind TypeOfKeyword;
	public static readonly SyntaxKind NameOfKeyword;
	public static readonly SyntaxKind SizeOfKeyword;
	public static readonly SyntaxKind NewKeyword;
	public static readonly SyntaxKind LetKeyword; 
	public static readonly SyntaxKind VarKeyword;
	public static readonly SyntaxKind ConstKeyword;
	public static readonly SyntaxKind PublicKeyword;
	public static readonly SyntaxKind PrivateKeyword;
	public static readonly SyntaxKind ClassKeyword;
	public static readonly SyntaxKind StructKeyword;
	public static readonly SyntaxKind EnumKeyword;
	public static readonly SyntaxKind StaticKeyword;
	public static readonly SyntaxKind ImmutableKeyword;
	public static readonly SyntaxKind MutableKeyword;
	public static readonly SyntaxKind ThisKeyword;
	public static readonly SyntaxKind ValueKeyword;
	public static readonly SyntaxKind NilKeyword;
	public static readonly SyntaxKind TrueKeyword;
	public static readonly SyntaxKind FalseKeyword;
	public static readonly SyntaxKind ObjectKeyword;
	public static readonly SyntaxKind StringKeyword;
	public static readonly SyntaxKind CharKeyword;
	public static readonly SyntaxKind BoolKeyword;
	public static readonly SyntaxKind Int8Keyword;
	public static readonly SyntaxKind UInt8Keyword;
	public static readonly SyntaxKind Int16Keyword;
	public static readonly SyntaxKind UInt16Keyword;
	public static readonly SyntaxKind Int32Keyword;
	public static readonly SyntaxKind UInt32Keyword;
	public static readonly SyntaxKind Int64Keyword;
	public static readonly SyntaxKind UInt64Keyword;
	public static readonly SyntaxKind Float32Keyword;
	public static readonly SyntaxKind Float64Keyword;
	public static readonly SyntaxKind UseKeyword;
	public static readonly SyntaxKind FromKeyword;
	public static readonly SyntaxKind ModuleKeyword;
	public static readonly SyntaxKind GenericKeyword;
	public static readonly SyntaxKind WhenKeyword;
	public static readonly SyntaxKind ConstructorKeyword;
	public static SyntaxKind AllKeywords => TypeOfKeyword  | NameOfKeyword  | SizeOfKeyword 
	                                      | NewKeyword     | LetKeyword     | ConstKeyword 
	                                      | PublicKeyword  | PrivateKeyword | ClassKeyword 
	                                      | StructKeyword  | StaticKeyword  | ImmutableKeyword 
	                                      | MutableKeyword | ThisKeyword    | ValueKeyword 
	                                      | NilKeyword     | TrueKeyword    | FalseKeyword
	                                      | ObjectKeyword  | StringKeyword  | CharKeyword
	                                      | BoolKeyword    | Int8Keyword    | UInt8Keyword 
	                                      | Int16Keyword   | UInt16Keyword  | Int32Keyword 
	                                      | UInt32Keyword  | Int64Keyword   | UInt64Keyword 
	                                      | Float32Keyword | Float64Keyword | VarKeyword 
	                                      | UseKeyword     | FromKeyword    | ModuleKeyword 
	                                      | GenericKeyword | WhenKeyword    | ConstructorKeyword | EnumKeyword;

	public static SyntaxKind AllModifiers => ImmutableKeyword | MutableKeyword 
	                                       | PrivateKeyword   | PublicKeyword;

	public static SyntaxKind AllPredefinedTypes => ObjectKeyword  | StringKeyword | CharKeyword 
	                                             | BoolKeyword    | Int8Keyword   | UInt8Keyword 
	                                             | Int16Keyword   | UInt16Keyword | Int32Keyword 
	                                             | UInt32Keyword  | Int64Keyword  | UInt64Keyword 
	                                             | Float32Keyword | Float64Keyword;

	public static SyntaxKind LocalVariableDeclaration => AllPredefinedOrUserTypes | VarKeyword 
	                                                   | ImmutableKeyword		  | MutableKeyword;
	
	public static SyntaxKind AllPredefinedOrUserTypes => AllPredefinedTypes | IdentifierToken | VarKeyword;

	
	// Lonely Identifier (lol)
	public static readonly SyntaxKind IdentifierToken;

	// Literals
	public static readonly SyntaxKind StringLiteralToken;
	public static readonly SyntaxKind CharLiteralToken;
	public static readonly SyntaxKind BoolLiteralToken;
	public static readonly SyntaxKind Int8LiteralToken;
	public static readonly SyntaxKind UInt8LiteralToken;
	public static readonly SyntaxKind Int16LiteralToken;
	public static readonly SyntaxKind UInt16LiteralToken;
	public static readonly SyntaxKind Int32LiteralToken;
	public static readonly SyntaxKind UInt32LiteralToken;
	public static readonly SyntaxKind Int64LiteralToken;
	public static readonly SyntaxKind UInt64LiteralToken;
	public static readonly SyntaxKind Float32LiteralToken;
	public static readonly SyntaxKind Float64LiteralToken;
	
	public static SyntaxKind AllLiterals => StringLiteralToken | CharLiteralToken   | BoolLiteralToken    
	                                      | Int8LiteralToken   | UInt8LiteralToken  | Int16LiteralToken   
	                                      | UInt16LiteralToken | Int32LiteralToken  | UInt32LiteralToken  
	                                      | Int64LiteralToken  | UInt64LiteralToken | Float32LiteralToken 
	                                      | Float64LiteralToken;
	
	public static SyntaxKind AllNumericLiterals => Int8LiteralToken   | UInt8LiteralToken  | Int16LiteralToken   
	                                             | UInt16LiteralToken | Int32LiteralToken  | UInt32LiteralToken  
	                                             | Int64LiteralToken  | UInt64LiteralToken | Float32LiteralToken 
	                                             | Float64LiteralToken;
	
	// Node Types
	public static readonly SyntaxKind BinaryExpression;
	public static readonly SyntaxKind NumberExpression;
	public static readonly SyntaxKind ParenthesizedExpression;
	public static readonly SyntaxKind LiteralExpression;
	public static readonly SyntaxKind UnaryExpression;
	public static readonly SyntaxKind NameExpression;
	public static readonly SyntaxKind AssignmentExpression;
	public static readonly SyntaxKind ExpressionStatement;
	public static readonly SyntaxKind VariableDeclaration;
	public static readonly SyntaxKind BlockStatement;
	public static readonly SyntaxKind CompilationUnit;
	public static readonly SyntaxKind ImportDirective;
	public static readonly SyntaxKind TernaryExpression;
	public static readonly SyntaxKind FunctionDeclaration;
	public static readonly SyntaxKind GenericFunctionDeclaration;
	public static readonly SyntaxKind ModuleDeclaration;
	public static readonly SyntaxKind GlobalVariableDeclaration;
	public static readonly SyntaxKind EnumFieldDeclaration;
	public static readonly SyntaxKind Parameter;
	public static readonly SyntaxKind ModifierList;
	public static readonly SyntaxKind ParameterList;
	public static readonly SyntaxKind ClassDeclaration;
	public static readonly SyntaxKind StructDeclaration;
	public static readonly SyntaxKind EnumDeclaration;
	public static readonly SyntaxKind StringLiteralExpression;
	public static readonly SyntaxKind CharLiteralExpression;
	public static readonly SyntaxKind BoolLiteralExpression;
	public static readonly SyntaxKind Int8LiteralExpression;
	public static readonly SyntaxKind UInt8LiteralExpression;
	public static readonly SyntaxKind Int16LiteralExpression;
	public static readonly SyntaxKind UInt16LiteralExpression;
	public static readonly SyntaxKind Int32LiteralExpression;
	public static readonly SyntaxKind UInt32LiteralExpression;
	public static readonly SyntaxKind Int64LiteralExpression;
	public static readonly SyntaxKind UInt64LiteralExpression;
	public static readonly SyntaxKind Float32LiteralExpression;
	public static readonly SyntaxKind Float64LiteralExpression;
	public static SyntaxKind AllNodes => CompilationUnit       | NumberExpression		   | ParenthesizedExpression
	                                   | LiteralExpression     | UnaryExpression           | NameExpression
	                                   | AssignmentExpression  | ExpressionStatement       | VariableDeclaration
	                                   | BlockStatement        | BinaryExpression          | ImportDirective
	                                   | TernaryExpression     | FunctionDeclaration       | GenericFunctionDeclaration
	                                   | ModuleDeclaration     | GlobalVariableDeclaration | Parameter
	                                   | ModifierList          | ClassDeclaration          | StructDeclaration 
	                                   | AllLiteralExpressions | ParameterList             | EnumDeclaration | EnumFieldDeclaration;

	public static SyntaxKind AllLiteralExpressions => StringLiteralExpression | CharLiteralExpression   | BoolLiteralExpression 
	                                                | Int8LiteralExpression   | UInt8LiteralExpression  | Int16LiteralExpression 
	                                                | UInt16LiteralExpression | Int32LiteralExpression  | UInt32LiteralExpression 
	                                                | Int64LiteralExpression  | UInt64LiteralExpression | Float32LiteralExpression 
	                                                | Float64LiteralExpression;
	
	public static readonly SyntaxKind GenericConstraint;
	// public static readonly SyntaxKind GenericConstraintDeclaration;
	
	public static readonly SyntaxKind TypeConstraint;
	public static readonly SyntaxKind MethodConstraint;
	public static readonly SyntaxKind ConstructorConstraint;
	public static SyntaxKind AllConstraints => GenericConstraint     | MethodConstraint 
	                                         | ConstructorConstraint | TypeConstraint;

	// ReSharper restore UnassignedReadonlyField
}

/// <summary>
/// Converts objects to and from BigFlags instances.
/// </summary>
public class SyntaxKindConverter : TypeConverter
{
	/// <summary>
	/// Can convert to string only.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="destinationType"></param>
	/// <returns></returns>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
	{
		return destinationType == typeof(string);
	}

	/// <summary>
	/// Can convert from any object.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="sourceType"></param>
	/// <returns></returns>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return true;
	}

	/// <summary>
	/// Converts BigFlags to a string.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="culture"></param>
	/// <param name="value"></param>
	/// <param name="destinationType"></param>
	/// <returns></returns>
	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (value is SyntaxKind && CanConvertTo(destinationType))
		{
			return value.ToString();
		}

		return null;
	}

	/// <summary>
	/// Attempts to parse <paramref name="value"/> and create and
	/// return a new BigFlags instance.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="culture"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (SyntaxKind.TryParse(Convert.ToString(value), out var result))
		{
			return result;
		}

		return null;
	}
}