namespace VSharp.Core.Analysis.Parsing;

using Text;
using Syntax;
using Diagnostics;
using Syntax.Statements;
using Syntax.Expressions;
using WaifuShork.Common.QuickLinq;
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("ReSharper", "InvertIf")]
public sealed class Parser
{
	private readonly IReadOnlyList<ISyntaxToken> m_tokens;

	private readonly DiagnosticBag m_diagnostics = new();
	private int m_position;
	private readonly SyntaxCache m_cache = new();
	
	public Parser(SourceText source)
	{
		m_tokens = SyntaxTree.ParseTokens(source.Text, out var diagnostics);
		m_diagnostics.AddRange(diagnostics);
	}

	private ISyntaxToken Current => Peek(0);
	private ISyntaxToken Next => Peek(1);
	private bool IsAtEnd => Current.Kind == SyntaxKind.EndOfFileToken || m_position >= m_tokens.Count;
	public IEnumerable<DiagnosticInfo> Diagnostics => m_diagnostics.Cache.Value;

	public CompilationUnitSyntax ParseCompilationUnit()
	{
		var module = ParseModuleDeclaration();
		var eofToken = Match<Delimiter>(SyntaxKind.EndOfFileToken);
		return new CompilationUnitSyntax(module, eofToken);
	}

	private ModuleDeclarationSyntax ParseModuleDeclaration()
	{
		var (openBracket, modifiers, closeBracket) = ParseModifiers();
		var keyword = Match<Keyword>(SyntaxKind.ModuleKeyword);
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		var semicolon = Match<Delimiter>(SyntaxKind.SemicolonToken);
		var members = ParseMembers();
		return new ModuleDeclarationSyntax(openBracket, modifiers, closeBracket, keyword, identifier, semicolon, members);
	}

	private IReadOnlyList<MemberSyntax> ParseMembers()
	{
		var members = new List<MemberSyntax>();
		while (!CurrentIs(SyntaxKind.EndOfFileToken) && !IsAtEnd)
		{
			var member = ParseMember();
			members.Add(member);
		}

		return members;
	}

	private MemberSyntax ParseMember()
	{
		if (CurrentIs(SyntaxKind.UseKeyword))
		{
			return ParseImportDirective();
		}

		if (CurrentIs(SyntaxKind.OpenBracketToken))
		{
			var (openBracket, modifiers, closeBracket) = ParseModifiers();
			if (CurrentIs(SyntaxKind.ClassKeyword))
			{
				return ParseClassDeclaration(in openBracket, in modifiers, in closeBracket);
			}
			if (CurrentIs(SyntaxKind.StructKeyword))
			{
				return ParseStructDeclaration(in openBracket, in modifiers, in closeBracket);
			}
			if (CurrentIs(SyntaxKind.EnumKeyword))
			{
				return ParseEnumDeclaration(in openBracket, in modifiers, in closeBracket);
			}
			if (NextIs(SyntaxKind.OpenParenToken) && TryMatch<Identifier>(SyntaxKind.IdentifierToken, out var identifier))
			{
				return ParseFunctionDeclaration(in openBracket, in modifiers, in closeBracket, in identifier);
			}
			if (TryMatch<Any>(SyntaxKind.AllPredefinedOrUserTypes, out var type))
			{
				identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
				return ParseGlobalVariableDeclaration(in openBracket, in modifiers, in closeBracket, in type, in identifier);
			}
		}
		
		if (TryMatch<Any>(SyntaxKind.AtToken, out var atToken))
		{
			return ParseGenericFunctionDeclaration(in atToken);
		}

		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, out var diagnostic);
		return DiagnosticBag.GenerateErrorNode<ErrorMemberNode>(diagnostic);
	}
	
	private ImportDirectiveSyntax ParseImportDirective()
	{
		var identifiers = new List<SyntaxToken<Any>>();
		var parseNextIdentifier = true;
		var useKeyword = Match<Keyword>(SyntaxKind.UseKeyword);
		
		// use MyModule;
		if (TryMatch<Identifier>(SyntaxKind.IdentifierToken, out var moduleIdentifier))
		{
			return new ImportDirectiveSyntax(useKeyword, moduleIdentifier);
		}
		
		// use [Type] from MyModule
		var openBracketToken = Match<Any>(SyntaxKind.OpenBracketToken);
		while (parseNextIdentifier && !CurrentIs(SyntaxKind.CloseBracketToken) && !IsAtEnd)
		{
			var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
			identifiers.Add(identifier);
			if (TryMatch<Delimiter>(SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			parseNextIdentifier = false;
		}

		var closeBracketToken = Match<Any>(SyntaxKind.CloseBracketToken);
		var fromKeyword = Match<Keyword>(SyntaxKind.FromKeyword);
		moduleIdentifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		_ = Match<Any>(SyntaxKind.SemicolonToken);
		
		return new ImportDirectiveSyntax(useKeyword, openBracketToken, identifiers, closeBracketToken, fromKeyword, moduleIdentifier);
	}
	
	private ClassDeclarationSyntax ParseClassDeclaration(in SyntaxToken<Bracket> openBracket,
	                                                     in ModifierSyntaxList modifiers, 
	                                                     in SyntaxToken<Bracket> closeBracket)
	{
		var keyword = Match<Keyword>(SyntaxKind.ClassKeyword);
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		var block = ParseBlockStatement();
		return new ClassDeclarationSyntax(openBracket, modifiers, closeBracket, keyword, identifier, block);
	}
	
	private StructDeclarationSyntax ParseStructDeclaration(in SyntaxToken<Bracket> openBracket,
	                                                       in ModifierSyntaxList modifiers, 
	                                                       in SyntaxToken<Bracket> closeBracket)
	{
		var keyword = Match<Keyword>(SyntaxKind.StructKeyword);
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		var block = ParseBlockStatement();
		return new StructDeclarationSyntax(openBracket, modifiers, closeBracket, keyword, identifier, block);
	}

	private EnumDeclarationSyntax ParseEnumDeclaration(in SyntaxToken<Bracket> openBracket,
	                                                   in ModifierSyntaxList modifiers, 
	                                                   in SyntaxToken<Bracket> closeBracket)
	{
		var keyword = Match<Keyword>(SyntaxKind.EnumKeyword);
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		var block = ParseEnumBlockStatement();
		return new EnumDeclarationSyntax(openBracket, modifiers, closeBracket, keyword, identifier, block);
	}

	private EnumBlockStatementSyntax ParseEnumBlockStatement()
	{
		var fields = new List<EnumFieldDeclarationSyntax>();
		var openBraceToken = Match<Any>(SyntaxKind.OpenBraceToken);
		while (!CurrentIs(SyntaxKind.CloseBraceToken) && !IsAtEnd)
		{
			var field = ParseEnumFieldDeclaration();
			// var statement = ParseStatement();
			fields.Add(field);
			if (TryMatch<Delimiter>(SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			break;
		}

		var closeBraceToken = Match<Any>(SyntaxKind.CloseBraceToken);
		return new EnumBlockStatementSyntax(openBraceToken, fields, closeBraceToken);
	}
	
	private FunctionDeclarationSyntax ParseFunctionDeclaration(
		in SyntaxToken<Bracket> openBracket,
		in ModifierSyntaxList modifiers, 
		in SyntaxToken<Bracket> closeBracket,
		in SyntaxToken<Identifier> identifier)
	{
		var openParen = Match<Bracket>(SyntaxKind.OpenParenToken);
		var parameters = ParseParameters();
		var closeParen = Match<Bracket>(SyntaxKind.CloseParenToken);
		var arrowToken = Match<Any>(SyntaxKind.ArrowToken);
		
		var returnType = Match<Any>(SyntaxKind.AllPredefinedOrUserTypes);

		var block = ParseBlockStatement();
		return new FunctionDeclarationSyntax(openBracket, modifiers, closeBracket, identifier, openParen, parameters, closeParen, arrowToken, returnType, block);
	}

	private GenericFunctionDeclaration ParseGenericFunctionDeclaration(in SyntaxToken<Any> atToken)
	{
		// @generic<T>
		// [public] Add(T left, T right) -> T { }
		var genericKeyword = Match<Keyword>(SyntaxKind.GenericKeyword);
		var lessToken  = Match<Any>(SyntaxKind.LessToken);
		var genericType = Match<Identifier>(SyntaxKind.IdentifierToken);
		var greaterToken = Match<Any>(SyntaxKind.GreaterToken);
		var constraint = default(GenericConstraintSyntax);
		if (CurrentIs(SyntaxKind.WhenKeyword))
		{
			constraint = ParseGenericConstraintDeclaration();
		}

		var (openBracket, modifiers, closeBracket) = ParseModifiers();
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		var openParen = Match<Bracket>(SyntaxKind.OpenParenToken);
		var parameters = ParseParameters();
		var closeParen = Match<Bracket>(SyntaxKind.CloseParenToken);
		var arrowToken = Match<Any>(SyntaxKind.ArrowToken);

		var returnType = Match<Any>(SyntaxKind.AllPredefinedOrUserTypes);		
		
		var block = ParseBlockStatement();
		return new GenericFunctionDeclaration(atToken,
		                                      genericKeyword,
		                                      lessToken,
		                                      genericType,
		                                      greaterToken,
		                                      constraint, 
		                                      openBracket, modifiers, closeBracket, 
		                                      identifier, 
		                                      openParen, parameters, closeParen, 
		                                      arrowToken, returnType, 
		                                      block);
	}
	
	private GlobalVariableDeclaration ParseGlobalVariableDeclaration(
		in SyntaxToken<Bracket> openBracket,
		in ModifierSyntaxList modifiers, 
		in SyntaxToken<Bracket> closeBracket,
		in SyntaxToken<UType> type,
		in SyntaxToken<Identifier> identifier)
	{
		SyntaxToken<Delimiter> semicolon;
		if (TryMatch<Any>(SyntaxKind.EqualsToken, out var equalsToken))
		{
			var initializer = ParseExpression();
			semicolon = Match<Delimiter>(SyntaxKind.SemicolonToken);
			return new GlobalVariableDeclaration(openBracket, modifiers, closeBracket, type, identifier, equalsToken, initializer, semicolon);
		}
		
		semicolon = Match<Delimiter>(SyntaxKind.SemicolonToken);
		return new GlobalVariableDeclaration(openBracket, modifiers, closeBracket, type, identifier, null, null, semicolon);
	}

	private GenericConstraintSyntax ParseGenericConstraintDeclaration()
	{
		var whenKeyword = Match<Keyword>(SyntaxKind.WhenKeyword);
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		var colonToken = Match<Any>(SyntaxKind.ColonToken);

		var constraints = ParseConstraints();
		return new GenericConstraintSyntax(whenKeyword, identifier, colonToken, constraints);
	}

	private IReadOnlyList<ConstraintSyntax> ParseConstraints()
	{
		var parseNextConstraint = true;
		var constraints = new List<ConstraintSyntax>();
		while (parseNextConstraint && !CurrentIs(SyntaxKind.OpenBracketToken) && !IsAtEnd)
		{
			var constraint = ParseConstraint();
			constraints.Add(constraint);
			if (!TryMatch<Delimiter>(SyntaxKind.CommaToken, out _))
			{
				parseNextConstraint = false;
			}
		}

		return constraints;
	}

	private ConstraintSyntax ParseConstraint()
	{
		if (CurrentIs(SyntaxKind.ConstructorKeyword))
		{
			return ParseConstructorConstraint();
		}
		if (CurrentIs(SyntaxKind.IdentifierToken) && !NextIs(SyntaxKind.DotToken))
		{
			return ParseIdentifierConstraint();
		}
 
		return ParseMethodConstraint();
	}

	private IReadOnlyList<SyntaxToken<Identifier>> ParseGenericParameters()
	{
		var parseNextParameter = true;
		var parameters = new List<SyntaxToken<Identifier>>();
		while (parseNextParameter && !CurrentIs(SyntaxKind.CloseParenToken) && !IsAtEnd)
		{
			parameters.Add(Match<Any>(SyntaxKind.AllPredefinedOrUserTypes));

			if (TryMatch<Delimiter>(SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			parseNextParameter = false;
		}

		return parameters;
	}

	private TypeConstraintSyntax ParseIdentifierConstraint()
	{
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		return new TypeConstraintSyntax(identifier);
	}
	
	private ConstructorConstraintSyntax ParseConstructorConstraint()
	{
		var ctorKeyword = Match<Keyword>(SyntaxKind.ConstructorKeyword);
		var openParen = Match<Any>(SyntaxKind.OpenParenToken);
		var parameters = ParseGenericParameters();
		var closeParen = Match<Any>(SyntaxKind.CloseParenToken);
		return new ConstructorConstraintSyntax(ctorKeyword, openParen, parameters, closeParen);
	}

	private MethodConstraintSyntax ParseMethodConstraint()
	{
		var genericIdentifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		var dotToken = Match<Any>(SyntaxKind.DotToken);
		var methodName = Match<Identifier>(SyntaxKind.IdentifierToken);
		var openParen = Match<Any>(SyntaxKind.OpenParenToken);
		var parameters = ParseGenericParameters();
		var closeParen = Match<Any>(SyntaxKind.CloseParenToken);
		return new MethodConstraintSyntax(genericIdentifier, dotToken, methodName, openParen, parameters, closeParen);
	}
	
	
	private (SyntaxToken<Bracket>, ModifierSyntaxList, SyntaxToken<Bracket>) ParseModifiers()
	{
		var openBracket = Match<Bracket>(SyntaxKind.OpenBracketToken);
		var parseNextParameter = true;
		var modifiers = new List<SyntaxToken<Modifier>>();
		while (parseNextParameter && !CurrentIs(SyntaxKind.CloseBracketToken) && !IsAtEnd)
		{
			modifiers.Add(Match<Keyword>(SyntaxKind.AllModifiers));

			if (TryMatch<Delimiter>(SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			parseNextParameter = false;
		}

		var closeBracket = Match<Bracket>(SyntaxKind.CloseBracketToken);
		
		if (!modifiers.AnyQ())
		{
			// fabricate an error but continue
			m_diagnostics.ReportUnexpectedToken(openBracket.Location, openBracket.Kind, out var diagnostic);
			modifiers.Add(SyntaxToken<Error>.Illegal(Current.Position - 1, Current.Line, Current.LeadingTrivia, diagnostic));
		}
		
		return (openBracket, new ModifierSyntaxList(modifiers),closeBracket);
	}
	
	private ParameterSyntaxList ParseParameters()
	{
		var parseNextParameter = true;
		var parameters = new List<ParameterSyntax>();
		while (parseNextParameter && !CurrentIs(SyntaxKind.CloseParenToken) && !IsAtEnd)
		{
			var parameter = ParseParameter();
			parameters.Add(parameter);
			if (!TryMatch<Delimiter>(SyntaxKind.CommaToken, out _))
			{
				parseNextParameter = false;
			}
		}

		return new ParameterSyntaxList(parameters);
	}

	private ParameterSyntax ParseParameter()
	{
		var type = Match<Identifier>(SyntaxKind.AllPredefinedOrUserTypes);
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		return new ParameterSyntax(type, identifier);
	}

	private StatementSyntax ParseStatement()
	{
		if (CurrentIs(SyntaxKind.OpenBraceToken))
		{
			return ParseBlockStatement();
		}
		if (CurrentIs(SyntaxKind.LocalVariableDeclaration))
		{
			return ParseVariableDeclaration();
		}

		return ParseExpressionStatement();
	}

	private EnumFieldDeclarationSyntax ParseEnumFieldDeclaration()
	{
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);
		if (TryMatch<Any>(SyntaxKind.EqualsToken, out var equals))
		{
			var expression = ParseExpression();
			return new EnumFieldDeclarationSyntax(identifier, equals, expression);
		}

		return new EnumFieldDeclarationSyntax(identifier, null, null);
	}
	
	private BlockStatementSyntax ParseBlockStatement()
	{
		var statements = new List<StatementSyntax>();

		var openBraceToken = Match<Any>(SyntaxKind.OpenBraceToken);
		while (!CurrentIs(SyntaxKind.CloseBraceToken) && !IsAtEnd)
		{
			var statement = ParseStatement();
			statements.Add(statement);
		}

		var closeBraceToken = Match<Any>(SyntaxKind.CloseBraceToken);
		return new BlockStatementSyntax(openBraceToken, statements, closeBraceToken);
	}

	private VariableDeclarationSyntax ParseVariableDeclaration()
	{
		var isMutable = false;
		if (TryMatch<Keyword>(SyntaxKind.ImmutableKeyword, out _))
		{
			isMutable = false;
		}
		if (TryMatch<Keyword>(SyntaxKind.MutableKeyword, out _))
		{
			isMutable = true;
		}

		var keyword = Match<Keyword>(SyntaxKind.AllPredefinedOrUserTypes);
		var identifier = Match<Identifier>(SyntaxKind.IdentifierToken);

		SyntaxToken<Delimiter> semicolon;
		// no initializer, even if we have "var x;", we will still consider it valid syntax
		// and catch the error at the later stages
		if (!TryMatch<Any>(SyntaxKind.EqualsToken, out var equalsToken))
		{
			semicolon = Match<Delimiter>(SyntaxKind.SemicolonToken);
			return new VariableDeclarationSyntax(keyword, identifier, null, null, semicolon, isMutable);
		}

		var initializer = ParseExpression();
		semicolon = Match<Delimiter>(SyntaxKind.SemicolonToken);
		return new VariableDeclarationSyntax(keyword, identifier, equalsToken, initializer, semicolon, isMutable);
	}

	private ExpressionStatementSyntax ParseExpressionStatement()
	{
		var expression = ParseExpression();
		return new ExpressionStatementSyntax(expression);
	}

	private ExpressionSyntax ParseExpression()
	{
		return ParseAssignmentExpression();
	}

	private ExpressionSyntax ParseAssignmentExpression()
	{
		if (TryMatch<Identifier>(SyntaxKind.IdentifierToken, out var identifierToken) && NextIs(SyntaxKind.EqualsToken))
		{
			var operatorToken = Match<Any>(SyntaxKind.EqualsToken);
			var right = ParseAssignmentExpression();
			return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
		}

		
		return ParseUnaryExpression();
	}

	private ExpressionSyntax ParseUnaryExpression(in int parentPrecedence = 0)
	{
		ExpressionSyntax left;
		var currentPrecedence = m_cache.GetUnaryPrecedence(Current.Kind);
		// If our precedence is 0 (which means we've hit a non-operator token) or our current precedence
		// is greater than the given parent precedence, we can successfully construct a node and then continue
		// down the tree if necessary
		if (currentPrecedence != 0 && currentPrecedence >= parentPrecedence)
		{
			var operatorToken = Match<Operator>(SyntaxKind.AllUnaryOperators); 
			var operand = ParseUnaryExpression(in currentPrecedence);
			left = new UnaryExpressionSyntax(operatorToken, operand);
		}
		else
		{
			// we've probably reached a number, open paren, identifier, or any other form of valid expression,
			// but those types don't have any "precedence", they're just an expression on their own, so we
			// just get the value and then pass it into an applicable binary left/right expression 
			left = ParsePrimaryExpression();
		}

		return ParseBinaryExpression(left, parentPrecedence);
	}

	private ExpressionSyntax ParseBinaryExpression(ExpressionSyntax left, in int parentPrecedence = 0)
	{
		while (true)
		{
			var currentPrecedence = m_cache.GetBinaryPrecedence(Current.Kind);
			// if we've reached a non-operator token, or our current precedence is less/equal to the parent precedence,
			// we can successfully break, and just return the left most expression
			if (currentPrecedence == 0 || currentPrecedence <= parentPrecedence)
			{
				break;
			}

			var operatorToken = Match<Operator>(SyntaxKind.AllBinaryOperators); 
			var right = ParseUnaryExpression(in currentPrecedence);
			left = new BinaryExpressionSyntax(left, operatorToken, right);
		}

		return left;
	}

	private ExpressionSyntax ParsePrimaryExpression()
	{
		if (CurrentIs(SyntaxKind.OpenParenToken))
		{
			return ParseParenthesizedExpression();
		}
		if (CurrentIs(SyntaxKind.StringLiteralToken | SyntaxKind.CharLiteralToken))
		{
			return ParseStringLiteral();
		}
		if (CurrentIs(SyntaxKind.TrueKeyword | SyntaxKind.FalseKeyword))
		{
			return ParseBooleanLiteral();
		}
		if (CurrentIs(SyntaxKind.AllNumericLiterals))
		{
			return ParseNumberLiteral();
		}
		if (CurrentIs(SyntaxKind.IdentifierToken))
		{
			return ParseNameExpression();
		}

		/*
		 * NOTE(everyone):
		 *	- The way that SyntaxKind is designed, when given N amount of kinds with the | operator,
		 *	when ToString() is called, it will return them in a separated list format of:
		 *	"OpenParenToken, StringLiteralToken, CharLiteralToken, TrueKeyword, FalseKeyword, AllNumericLiterals, IdentifierToken",
		 *	So the user will be able to see what was expected, instead of being clueless on invalid token error
		 *
		 *	- SyntaxKind.AllNumericLiterals will also be a list of all the number literals, so that list will actually be longer :) 
		 */
		var expected = SyntaxKind.OpenParenToken |
		               SyntaxKind.StringLiteralToken |
		               SyntaxKind.CharLiteralToken |
		               SyntaxKind.TrueKeyword |
		               SyntaxKind.FalseKeyword |
		               SyntaxKind.AllNumericLiterals |
		               SyntaxKind.IdentifierToken;
		
		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, expected, out var diagnostics);
		return DiagnosticBag.GenerateErrorNode<ErrorExpressionNode>(diagnostics);
	}
	
	private ExpressionSyntax ParseParenthesizedExpression()
	{
		var openParen = Match<Bracket>(SyntaxKind.OpenParenToken);
		var expression = ParseExpression();
		var closeParen = Match<Bracket>(SyntaxKind.CloseParenToken);
		return new ParenthesizedExpressionSyntax(openParen, expression, closeParen);
	}
	
	private ExpressionSyntax ParseStringLiteral()
	{
		if (TryMatch<string>(SyntaxKind.StringLiteralToken, out var stringLiteral))
		{
			return new StringLiteralExpressionSyntax(stringLiteral);
		}
		if (TryMatch<char>(SyntaxKind.CharLiteralToken, out var charLiteral))
		{
			return new CharLiteralExpressionSyntax(charLiteral);
		}

		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, out var diagnostic);
		return DiagnosticBag.GenerateErrorNode<ErrorExpressionNode>(diagnostic);
	}
	
	private ExpressionSyntax ParseBooleanLiteral()
	{
		if (TryMatch<bool>(SyntaxKind.TrueKeyword, out var trueKeyword))
		{
			return new BooleanLiteralExpressionSyntax(trueKeyword);
		}
		if (TryMatch<bool>(SyntaxKind.FalseKeyword, out var falseKeyword))
		{
			return new BooleanLiteralExpressionSyntax(falseKeyword);
		}
		
		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, SyntaxKind.TrueKeyword | SyntaxKind.FalseKeyword, out var diagnostic);
		return DiagnosticBag.GenerateErrorNode<ErrorExpressionNode>(diagnostic);
	}

	/*
	 * TODO(shork):
	 *	- Try to simplify this if possible, it may not be, since we're using generic tokens,
	 *	but it's worth looking into.
	 *
	 *	- Possible Ideas:
	 *	1) Dictionary<SyntaxKind, ExpressionSyntax>
	 *	2) ExpressionSyntax SyntaxCache.LookupNumberLiteral()
	 *	3) ???
	 */
	private ExpressionSyntax ParseNumberLiteral()
	{
		if (TryMatch<int8>(SyntaxKind.Int8LiteralToken, out var i8))
		{
			return new Int8LiteralExpressionSyntax(i8);
		}
		if (TryMatch<uint8>(SyntaxKind.UInt8LiteralToken, out var ui8))
		{
			return new UInt8LiteralExpressionSyntax(ui8);
		}
		if (TryMatch<int16>(SyntaxKind.Int16LiteralToken, out var i16))
		{
			return new Int16LiteralExpressionSyntax(i16);
		}
		if (TryMatch<uint16>(SyntaxKind.UInt16LiteralToken, out var ui16))
		{
			return new UInt16LiteralExpressionSyntax(ui16);
		}
		if (TryMatch<int32>(SyntaxKind.Int32LiteralToken, out var i32))
		{
			return new Int32LiteralExpressionSyntax(i32);
		}
		if (TryMatch<uint32>(SyntaxKind.UInt32LiteralToken, out var ui32))
		{
			return new UInt32LiteralExpressionSyntax(ui32);
		}
		if (TryMatch<int64>(SyntaxKind.Int64LiteralToken, out var i64))
		{
			return new Int64LiteralExpressionSyntax(i64);
		}
		if (TryMatch<uint64>(SyntaxKind.UInt64LiteralToken, out var ui64))
		{
			return new UInt64LiteralExpressionSyntax(ui64);
		}
		if (TryMatch<float32>(SyntaxKind.Float32LiteralToken, out var f32))
		{
			return new Float32LiteralExpressionSyntax(f32);
		}
		if (TryMatch<float64>(SyntaxKind.Float64LiteralToken, out var f64))
		{
			return new Float64LiteralExpressionSyntax(f64);
		}

		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, out var diagnostic);
		return DiagnosticBag.GenerateErrorNode<ErrorExpressionNode>(diagnostic); 
	}

	private ExpressionSyntax ParseNameExpression()
	{
		var identifierToken = Match<Identifier>(SyntaxKind.IdentifierToken);
		return new NameExpressionSyntax(identifierToken);
	}
	
	/*
	 * NOTE(everyone):
	 *	- Everything below this is a set helper functions, they do not parse anything on their own,
	 *	they merely provide matching, advancing, peeking, and other various helpers.
	 *
	 *	- If you wish to add any new helper functions, please add them below this note, and keep them
	 *	grouped in a logical fashion, as to ascertain a sense of order. 
	 */
	private bool CurrentIs(SyntaxKind kind)
	{
		if (IsAtEnd && kind != SyntaxKind.EndOfFileToken)
		{
			return false;
		}

		return kind.HasFlag(Current.Kind);
	}

	private bool NextIs(SyntaxKind kind)
	{
		if (IsAtEnd && kind != SyntaxKind.EndOfFileToken)
		{
			return false;
		}

		return kind.HasFlag(Next.Kind);
	}
	
	private ISyntaxToken Peek(int offset)
	{
		var index = m_position + offset;
		if (index >= m_tokens.Count)
		{
			return m_tokens[^1];
		}

		return m_tokens[index];
	}

	private ISyntaxToken Consume()
	{
		var current = Current;
		m_position++;
		return current;
	}

	private bool TryMatch<TValue>(SyntaxKind kind, [NotNullWhen(true)] out SyntaxToken<TValue>? token)
	{
		if (CurrentIs(kind))
		{
			token = Match<TValue>(kind);
			return true;
		}

		token = null;
		return false;
	}

	private SyntaxToken<TValue> Match<TValue>(SyntaxKind kind)
	{
		return (SyntaxToken<TValue>)Match(kind);
	}

	private ISyntaxToken Match(SyntaxKind kind)
	{
		if (CurrentIs(kind))
		{
			return Consume();
		}

		// Whoops, let's make sure we maintain the tree
		// Let's go ahead and eat the token anyways, so we don't get stuck in an infinite loop,
		// we'll keep going until we reach the end
		var current = Consume();
		m_diagnostics.ReportUnexpectedToken(current.Location, current.Kind, kind, out var diagnostic);
		// we'll generate an error token
		return new SyntaxToken<Error>(current.LeadingTrivia,
		                              SyntaxKind.BadToken,
		                              "ERROR",
		                              current.Text,
		                              current.Position,
		                              current.Line,
		                              current.TrailingTrivia, 
		                              diagnostic);
	}
}