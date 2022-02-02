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
	
	public Parser(in SourceText source)
	{
		var text = source.Text;
		m_tokens = SyntaxTree.ParseTokens(in text, out var diagnostics);
		m_diagnostics.AddRange(diagnostics);
	}

	private ISyntaxToken Current => Peek(0);
	private ISyntaxToken Next => Peek(1);
	private bool IsAtEnd => Current.Kind == SyntaxKind.EndOfFileToken || m_position >= m_tokens.Count;
	private bool NotAtEnd => !IsAtEnd;
	public IEnumerable<DiagnosticInfo> Diagnostics => m_diagnostics.Cache.Value;

	public CompilationUnitSyntax ParseCompilationUnit()
	{
		var module = ParseModuleDeclaration();
		var eofToken = Match<Delimiter>(in SyntaxKind.EndOfFileToken);
		return new CompilationUnitSyntax(module, eofToken);
	}

	private ModuleDeclarationSyntax ParseModuleDeclaration()
	{
		var (openBracket, modifiers, closeBracket) = ParseModifiers();
		var keyword = Match<Keyword>(in SyntaxKind.ModuleKeyword);
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var semicolon = Match<Delimiter>(in SyntaxKind.SemicolonToken);
		var members = ParseMembers();
		return new ModuleDeclarationSyntax(openBracket, 
		                                   modifiers, 
		                                   closeBracket, 
		                                   keyword, 
		                                   identifier, 
		                                   semicolon, 
		                                   members);
	}

	private IReadOnlyList<MemberSyntax> ParseMembers()
	{
		var members = new List<MemberSyntax>();
		while (CurrentIsNot(in SyntaxKind.EndOfFileToken) && NotAtEnd)
		{
			var startToken = Current;
			var member = ParseMember();
			members.Add(member);

			/*
			 * NOTE(everyone):
			 *	- If ParseMember() does not consume a token, then we'll end in an infinite loop,
			 *	so to mitigate that we skip the current token, then continue to try and parse another
			 *	member.
			 *
			 *	- There's no need to report errors here because we'd have already failed and reported
			 *	an error during expression/statement parsing, we don't care what went wrong, we just
			 *	don't want to get stuck.
			 */
			if (Current == startToken)
			{
				_ = Consume();
			}
		}

		return members;
	}

	private MemberSyntax ParseMember()
	{
		if (CurrentIs(in SyntaxKind.UseKeyword))
		{
			return ParseImportDirective();
		}

		if (CurrentIs(in SyntaxKind.OpenBracketToken))
		{
			var (openBracket, modifiers, closeBracket) = ParseModifiers();
			if (CurrentIs(in SyntaxKind.ClassKeyword))
			{
				return ParseClassDeclaration(in openBracket, in modifiers, in closeBracket);
			}
			if (CurrentIs(in SyntaxKind.StructKeyword))
			{
				return ParseStructDeclaration(in openBracket, in modifiers, in closeBracket);
			}
			if (CurrentIs(in SyntaxKind.EnumKeyword))
			{
				return ParseEnumDeclaration(in openBracket, in modifiers, in closeBracket);
			}
			if (NextIs(in SyntaxKind.OpenParenToken) && TryMatch<Identifier>(in SyntaxKind.IdentifierToken, out var identifier))
			{
				return ParseFunctionDeclaration(in openBracket, in modifiers, in closeBracket, in identifier);
			}
			if (TryMatch<Any>(SyntaxKind.AllPredefinedOrUserTypes, out var type))
			{
				identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
				return ParseGlobalVariableDeclaration(in openBracket, in modifiers, in closeBracket, in type, in identifier);
			}
		}
		
		if (TryMatch<Any>(in SyntaxKind.AtToken, out var atToken))
		{
			return ParseGenericFunctionDeclaration(in atToken);
		}

		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, out var diagnostic);
		return DiagnosticBag.CreateErrorNode<ErrorMemberNode>(diagnostic);
	}
	
	private ImportDirectiveSyntax ParseImportDirective()
	{
		var identifiers = new List<SyntaxToken<Any>>();
		var parseNextIdentifier = true;
		var useKeyword = Match<Keyword>(in SyntaxKind.UseKeyword);
		
		// use MyModule;
		if (TryMatch<Identifier>(in SyntaxKind.IdentifierToken, out var moduleIdentifier))
		{
			return new ImportDirectiveSyntax(useKeyword, moduleIdentifier);
		}
		
		// use [Type] from MyModule
		var openBracketToken = Match<Any>(in SyntaxKind.OpenBracketToken);
		while (parseNextIdentifier && CurrentIsNot(in SyntaxKind.CloseBracketToken) && NotAtEnd)
		{
			var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
			identifiers.Add(identifier);
			if (TryMatch<Delimiter>(in SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			parseNextIdentifier = false;
		}

		var closeBracketToken = Match<Any>(in SyntaxKind.CloseBracketToken);
		var fromKeyword = Match<Keyword>(in SyntaxKind.FromKeyword);
		moduleIdentifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		_ = Match<Any>(in SyntaxKind.SemicolonToken);

		var immutableIdentifiers = identifiers as IReadOnlyList<SyntaxToken<Any>>;
		
		return new ImportDirectiveSyntax(useKeyword, 
		                                 openBracketToken, 
		                                 immutableIdentifiers, 
		                                 closeBracketToken, 
		                                 fromKeyword,
		                                 moduleIdentifier);
	}

	private ClassDeclarationSyntax ParseClassDeclaration(in SyntaxToken<Bracket> openBracket,
	                                                     in ModifierSyntaxList modifiers, 
	                                                     in SyntaxToken<Bracket> closeBracket)
	{
		var keyword = Match<Keyword>(in SyntaxKind.ClassKeyword);
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var block = ParseBlockStatement();
		return new ClassDeclarationSyntax(openBracket, modifiers, closeBracket, keyword, identifier, block);
	}
	
	private StructDeclarationSyntax ParseStructDeclaration(in SyntaxToken<Bracket> openBracket,
	                                                       in ModifierSyntaxList modifiers, 
	                                                       in SyntaxToken<Bracket> closeBracket)
	{
		var keyword = Match<Keyword>(in SyntaxKind.StructKeyword);
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var block = ParseBlockStatement();
		return new StructDeclarationSyntax(openBracket, modifiers, closeBracket, keyword, identifier, block);
	}

	private EnumDeclarationSyntax ParseEnumDeclaration(in SyntaxToken<Bracket> openBracket,
	                                                   in ModifierSyntaxList modifiers, 
	                                                   in SyntaxToken<Bracket> closeBracket)
	{
		var keyword = Match<Keyword>(in SyntaxKind.EnumKeyword);
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var block = ParseEnumBlockStatement();
		return new EnumDeclarationSyntax(openBracket, modifiers, closeBracket, keyword, identifier, block);
	}

	private EnumBlockStatementSyntax ParseEnumBlockStatement()
	{
		var fields = new List<EnumFieldDeclarationSyntax>();
		var openBraceToken = Match<Any>(in SyntaxKind.OpenBraceToken);
		while (CurrentIsNot(in SyntaxKind.CloseBraceToken) && NotAtEnd)
		{
			var field = ParseEnumFieldDeclaration();
			fields.Add(field);
			if (TryMatch<Delimiter>(in SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			break;
		}

		var closeBraceToken = Match<Any>(in SyntaxKind.CloseBraceToken);
		return new EnumBlockStatementSyntax(openBraceToken, fields, closeBraceToken);
	}
	
	private FunctionDeclarationSyntax ParseFunctionDeclaration(
		in SyntaxToken<Bracket> openBracket,
		in ModifierSyntaxList modifiers, 
		in SyntaxToken<Bracket> closeBracket,
		in SyntaxToken<Identifier> identifier)
	{
		var openParen = Match<Bracket>(in SyntaxKind.OpenParenToken);
		var parameters = ParseParameters();
		var closeParen = Match<Bracket>(in SyntaxKind.CloseParenToken);
		var arrowToken = Match<Any>(in SyntaxKind.ArrowToken);
		
		var returnType = Match<Any>(SyntaxKind.AllPredefinedOrUserTypes);

		var block = ParseBlockStatement();
		return new FunctionDeclarationSyntax(openBracket,
		                                     modifiers, 
		                                     closeBracket,
		                                     identifier,
		                                     openParen,
		                                     parameters,
		                                     closeParen,
		                                     arrowToken,
		                                     returnType,
		                                     block);
	}

	private GenericFunctionDeclaration ParseGenericFunctionDeclaration(in SyntaxToken<Any> atToken)
	{
		// @generic<T>
		// [public] Add(T left, T right) -> T { }
		var genericKeyword = Match<Keyword>(in SyntaxKind.GenericKeyword);
		var lessToken  = Match<Any>(in SyntaxKind.LessToken);
		var genericType = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var greaterToken = Match<Any>(in SyntaxKind.GreaterToken);
		var constraint = default(GenericConstraintSyntax);
		if (CurrentIs(in SyntaxKind.WhenKeyword))
		{
			constraint = ParseGenericConstraintDeclaration();
		}

		var (openBracket, modifiers, closeBracket) = ParseModifiers();
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var openParen = Match<Bracket>(in SyntaxKind.OpenParenToken);
		var parameters = ParseParameters();
		var closeParen = Match<Bracket>(in SyntaxKind.CloseParenToken);
		var arrowToken = Match<Any>(in SyntaxKind.ArrowToken);

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
		if (TryMatch<Any>(in SyntaxKind.EqualsToken, out var equalsToken))
		{
			var initializer = ParseExpression();
			semicolon = Match<Delimiter>(in SyntaxKind.SemicolonToken);
			return new GlobalVariableDeclaration(openBracket, 
			                                     modifiers, 
			                                     closeBracket, 
			                                     type, 
			                                     identifier, 
			                                     equalsToken, 
			                                     initializer, 
			                                     semicolon);
		}
		
		semicolon = Match<Delimiter>(in SyntaxKind.SemicolonToken);
		return new GlobalVariableDeclaration(openBracket, 
		                                     modifiers, 
		                                     closeBracket, 
		                                     type, identifier, 
		                                     null, null, 
		                                     semicolon);
	}

	private GenericConstraintSyntax ParseGenericConstraintDeclaration()
	{
		var whenKeyword = Match<Keyword>(in SyntaxKind.WhenKeyword);
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var colonToken = Match<Any>(in SyntaxKind.ColonToken);

		var constraints = ParseConstraints();
		return new GenericConstraintSyntax(whenKeyword, identifier, colonToken, constraints);
	}

	private IReadOnlyList<ConstraintSyntax> ParseConstraints()
	{
		var parseNextConstraint = true;
		var constraints = new List<ConstraintSyntax>();
		while (parseNextConstraint && CurrentIsNot(in SyntaxKind.OpenBracketToken) && NotAtEnd)
		{
			var constraint = ParseConstraint();
			constraints.Add(constraint);
			if (!TryMatch<Delimiter>(in SyntaxKind.CommaToken, out _))
			{
				parseNextConstraint = false;
			}
		}

		return constraints;
	}

	private ConstraintSyntax ParseConstraint()
	{
		if (CurrentIs(in SyntaxKind.ConstructorKeyword))
		{
			return ParseConstructorConstraint();
		}
		if (CurrentIs(in SyntaxKind.IdentifierToken) && NextIs(in SyntaxKind.DotToken))
		{
			return ParseMethodConstraint();
		}
		
		return ParseIdentifierConstraint();
	}

	private IReadOnlyList<SyntaxToken<Identifier>> ParseGenericParameters()
	{
		var parseNextParameter = true;
		var parameters = new List<SyntaxToken<Identifier>>();
		while (parseNextParameter && CurrentIsNot(in SyntaxKind.CloseParenToken) && NotAtEnd)
		{
			parameters.Add(Match<Any>(SyntaxKind.AllPredefinedOrUserTypes));

			if (TryMatch<Delimiter>(in SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			parseNextParameter = false;
		}

		return parameters;
	}

	private TypeConstraintSyntax ParseIdentifierConstraint()
	{
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		return new TypeConstraintSyntax(identifier);
	}
	
	private ConstructorConstraintSyntax ParseConstructorConstraint()
	{
		var ctorKeyword = Match<Keyword>(in SyntaxKind.ConstructorKeyword);
		var openParen = Match<Any>(in SyntaxKind.OpenParenToken);
		var parameters = ParseGenericParameters();
		var closeParen = Match<Any>(in SyntaxKind.CloseParenToken);
		return new ConstructorConstraintSyntax(ctorKeyword, openParen, parameters, closeParen);
	}

	private MethodConstraintSyntax ParseMethodConstraint()
	{
		var genericIdentifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var dotToken = Match<Any>(in SyntaxKind.DotToken);
		var methodName = Match<Identifier>(in SyntaxKind.IdentifierToken);
		var openParen = Match<Any>(in SyntaxKind.OpenParenToken);
		var parameters = ParseGenericParameters();
		var closeParen = Match<Any>(in SyntaxKind.CloseParenToken);
		return new MethodConstraintSyntax(genericIdentifier, dotToken, methodName, openParen, parameters, closeParen);
	}
	
	private (SyntaxToken<Bracket>, ModifierSyntaxList, SyntaxToken<Bracket>) ParseModifiers()
	{
		var openBracket = Match<Bracket>(in SyntaxKind.OpenBracketToken);
		var parseNextParameter = true;
		var modifiers = new List<SyntaxToken<Modifier>>();
		while (parseNextParameter && CurrentIsNot(in SyntaxKind.CloseBracketToken) && NotAtEnd)
		{
			modifiers.Add(Match<Keyword>(SyntaxKind.AllModifiers));

			if (TryMatch<Delimiter>(in SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			parseNextParameter = false;
		}

		var closeBracket = Match<Bracket>(in SyntaxKind.CloseBracketToken);
		
		if (!modifiers.AnyQ())
		{
			// fabricate an error but continue
			m_diagnostics.ReportUnexpectedToken(openBracket.Location, openBracket.Kind, out var diagnostic);
			modifiers.Add(SyntaxToken<Error>.Illegal(Current.Position - 1, Current.Line, Current.LeadingTrivia, diagnostic));
		}
		
		return (openBracket, new ModifierSyntaxList(modifiers), closeBracket);
	}
	
	private ParameterSyntaxList ParseParameters()
	{
		var parseNextParameter = true;
		var parameters = new List<ParameterSyntax>();
		while (parseNextParameter && CurrentIsNot(in SyntaxKind.CloseParenToken) && NotAtEnd)
		{
			var parameter = ParseParameter();
			parameters.Add(parameter);
			if (!TryMatch<Delimiter>(in SyntaxKind.CommaToken, out _))
			{
				parseNextParameter = false;
			}
		}

		return new ParameterSyntaxList(parameters);
	}

	private ParameterSyntax ParseParameter()
	{
		var type = Match<Identifier>(SyntaxKind.AllPredefinedOrUserTypes);
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		return new ParameterSyntax(type, identifier);
	}

	private StatementSyntax ParseStatement()
	{
		if (CurrentIs(in SyntaxKind.OpenBraceToken))
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
		var identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		if (TryMatch<Any>(in SyntaxKind.EqualsToken, out var equals))
		{
			var expression = ParseExpression();
			return new EnumFieldDeclarationSyntax(identifier, equals, expression);
		}

		return new EnumFieldDeclarationSyntax(identifier, null, null);
	}
	
	private BlockStatementSyntax ParseBlockStatement()
	{
		var statements = new List<StatementSyntax>();

		var openBraceToken = Match<Any>(in SyntaxKind.OpenBraceToken);
		while (CurrentIsNot(in SyntaxKind.CloseBraceToken) && NotAtEnd)
		{
			var statement = ParseStatement();
			// edge case for assignment expressions
			_ = TryMatch<Delimiter>(in SyntaxKind.SemicolonToken, out _);
			statements.Add(statement);
		}

		var closeBraceToken = Match<Any>(in SyntaxKind.CloseBraceToken);
		return new BlockStatementSyntax(openBraceToken, statements, closeBraceToken);
	}

	private VariableDeclarationSyntax ParseVariableDeclaration()
	{
		var mutability = default(SyntaxToken<Keyword>);
		if (TryMatch<Keyword>(in SyntaxKind.ImmutableKeyword, out var immutable))
		{
			mutability = immutable;
		}
		if (TryMatch<Keyword>(in SyntaxKind.MutableKeyword, out var mutable))
		{
			mutability = mutable;
		}

		SyntaxToken<Identifier> identifier;
		SyntaxToken<Delimiter> semicolon;
		SyntaxToken<Any>? equalsToken;
 		
		if (CurrentIs(in SyntaxKind.IdentifierToken) && NextIs(SyntaxKind.AllCompoundOperators))
		{
			identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
			equalsToken = Match<Any>(SyntaxKind.AllCompoundOperators);
			var initializer = ParseExpression();
			semicolon = Match<Delimiter>(in SyntaxKind.SemicolonToken);
			return new VariableDeclarationSyntax(mutability, null, identifier, equalsToken, initializer, semicolon);
		}
		
		var keyword = Match<Keyword>(SyntaxKind.AllPredefinedOrUserTypes);
		identifier = Match<Identifier>(in SyntaxKind.IdentifierToken);
		
		// no initializer, even if we have "var x;", we will still consider it valid syntax
		// and catch the error at the later stages
		if (TryMatch(in SyntaxKind.EqualsToken, out equalsToken))
		{
			var initializer = ParseExpression();
			semicolon = Match<Delimiter>(in SyntaxKind.SemicolonToken);
			return new VariableDeclarationSyntax(mutability, keyword, identifier, equalsToken, initializer, semicolon);
		}

		semicolon = Match<Delimiter>(in SyntaxKind.SemicolonToken);
		return new VariableDeclarationSyntax(mutability, keyword, identifier, null, null, semicolon);
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
		if (TryMatch<Identifier>(in SyntaxKind.IdentifierToken, out var identifierToken) && 
		    TryMatch<Any>(SyntaxKind.AllCompoundOperators, out var equalsToken))
		{
			var expression = ParseExpression();
			return new AssignmentExpressionSyntax(identifierToken, equalsToken, expression);
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

			if (CurrentIs(in SyntaxKind.QuestionMarkToken))
			{
				left = ParseTernaryExpression(in left, in currentPrecedence);
			}
			else 
			{
				var operatorToken = Match<Operator>(SyntaxKind.AllBinaryOperators);
				var right = ParseUnaryExpression(in currentPrecedence);
				left = new BinaryExpressionSyntax(left, operatorToken, right);
			}
		}

		return left;
	}

	// condition is already set in stone, so we use the "in" modifier to ensure there's zero chance of modification
	private TernaryExpressionSyntax ParseTernaryExpression(in ExpressionSyntax condition, in int parentPrecedence)
	{
		var questionMark = Match<Any>(in SyntaxKind.QuestionMarkToken);
		var consequent = ParseUnaryExpression(in parentPrecedence);
		var colon = Match<Any>(in SyntaxKind.ColonToken);
		var alternative = ParseUnaryExpression(in parentPrecedence);
		return new TernaryExpressionSyntax(condition, questionMark, consequent, colon, alternative);
	}
	
	private ExpressionSyntax ParsePrimaryExpression()
	{
		if (CurrentIs(in SyntaxKind.AllocKeyword))
		{
			return ParseConstructorCall();
		}
		if (CurrentIs(in SyntaxKind.OpenParenToken))
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
		if (CurrentIs(in SyntaxKind.IdentifierToken))
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
		return DiagnosticBag.CreateErrorNode<ErrorExpressionNode>(diagnostics);
	}

	private ConstructorCallExpressionSyntax ParseConstructorCall()
	{
		var allocKeyword = Match<Keyword>(in SyntaxKind.AllocKeyword);
		SyntaxToken<Identifier> type;
		SyntaxToken<Bracket> openParen;
		SyntaxList<SyntaxToken<Identifier>> arguments;
		SyntaxToken<Bracket> closeParen;
		if (TryMatch<Any>(in SyntaxKind.ColonToken, out var colon))
		{
			var allocator = Match<Identifier>(SyntaxKind.IdentifierToken | SyntaxKind.HeapKeyword | SyntaxKind.StackKeyword);
			type = Match<Identifier>(SyntaxKind.AllPredefinedOrUserTypes);
			openParen = Match<Bracket>(in SyntaxKind.OpenParenToken);
			arguments = ParseArguments();
			closeParen = Match<Bracket>(in SyntaxKind.CloseParenToken);
			return new ConstructorCallExpressionSyntax(allocKeyword, colon, allocator, type, openParen, arguments, closeParen);
		}
		
		type = Match<Identifier>(SyntaxKind.AllPredefinedOrUserTypes);
		openParen = Match<Bracket>(in SyntaxKind.OpenParenToken);
		arguments = ParseArguments();
		closeParen = Match<Bracket>(in SyntaxKind.CloseParenToken);
		return new ConstructorCallExpressionSyntax(allocKeyword, null, null, type, openParen, arguments, closeParen);
	}

	private SyntaxList<SyntaxToken<Identifier>> ParseArguments()
	{
		var parseNextArgument = true;
		var arguments = new SyntaxList<SyntaxToken<Identifier>>();
		while (parseNextArgument && CurrentIsNot(in SyntaxKind.CloseParenToken) && NotAtEnd)
		{
			arguments.Add(Match<Any>(SyntaxKind.AllArguments));
			if (TryMatch<Delimiter>(in SyntaxKind.CommaToken, out _))
			{
				continue;
			}
			
			parseNextArgument = false;
		}

		return arguments;
	}
	
	private ExpressionSyntax ParseParenthesizedExpression()
	{
		var openParen = Match<Bracket>(in SyntaxKind.OpenParenToken);
		var expression = ParseExpression();
		var closeParen = Match<Bracket>(in SyntaxKind.CloseParenToken);
		return new ParenthesizedExpressionSyntax(openParen, expression, closeParen);
	}
	
	private ExpressionSyntax ParseStringLiteral()
	{
		if (TryMatch<string>(in SyntaxKind.StringLiteralToken, out var stringLiteral))
		{
			return new StringLiteralExpressionSyntax(stringLiteral);
		}
		if (TryMatch<char>(in SyntaxKind.CharLiteralToken, out var charLiteral))
		{
			return new CharLiteralExpressionSyntax(charLiteral);
		}

		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, out var diagnostic);
		return DiagnosticBag.CreateErrorNode<ErrorExpressionNode>(diagnostic);
	}
	
	private ExpressionSyntax ParseBooleanLiteral()
	{
		if (TryMatch<bool>(in SyntaxKind.TrueKeyword, out var trueKeyword))
		{
			return new BooleanLiteralExpressionSyntax(trueKeyword);
		}
		if (TryMatch<bool>(in SyntaxKind.FalseKeyword, out var falseKeyword))
		{
			return new BooleanLiteralExpressionSyntax(falseKeyword);
		}
		
		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, SyntaxKind.TrueKeyword | SyntaxKind.FalseKeyword, out var diagnostic);
		return DiagnosticBag.CreateErrorNode<ErrorExpressionNode>(diagnostic);
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
		if (TryMatch<int8>(in SyntaxKind.Int8LiteralToken, out var i8))
		{
			return new Int8LiteralExpressionSyntax(i8);
		}
		if (TryMatch<uint8>(in SyntaxKind.UInt8LiteralToken, out var ui8))
		{
			return new UInt8LiteralExpressionSyntax(ui8);
		}
		if (TryMatch<int16>(in SyntaxKind.Int16LiteralToken, out var i16))
		{
			return new Int16LiteralExpressionSyntax(i16);
		}
		if (TryMatch<uint16>(in SyntaxKind.UInt16LiteralToken, out var ui16))
		{
			return new UInt16LiteralExpressionSyntax(ui16);
		}
		if (TryMatch<int32>(in SyntaxKind.Int32LiteralToken, out var i32))
		{
			return new Int32LiteralExpressionSyntax(i32);
		}
		if (TryMatch<uint32>(in SyntaxKind.UInt32LiteralToken, out var ui32))
		{
			return new UInt32LiteralExpressionSyntax(ui32);
		}
		if (TryMatch<int64>(in SyntaxKind.Int64LiteralToken, out var i64))
		{
			return new Int64LiteralExpressionSyntax(i64);
		}
		if (TryMatch<uint64>(in SyntaxKind.UInt64LiteralToken, out var ui64))
		{
			return new UInt64LiteralExpressionSyntax(ui64);
		}
		if (TryMatch<float32>(in SyntaxKind.Float32LiteralToken, out var f32))
		{
			return new Float32LiteralExpressionSyntax(f32);
		}
		if (TryMatch<float64>(in SyntaxKind.Float64LiteralToken, out var f64))
		{
			return new Float64LiteralExpressionSyntax(f64);
		}
		if (TryMatch<infint>(in SyntaxKind.InfinityIntLiteralToken, out var infInt))
		{
			return new InfinityIntLiteralExpressionSyntax(infInt);
		}

		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, out var diagnostic);
		return DiagnosticBag.CreateErrorNode<ErrorExpressionNode>(diagnostic); 
	}

	private ExpressionSyntax ParseNameExpression()
	{
		var identifierToken = Match<Identifier>(in SyntaxKind.IdentifierToken);
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
	private bool CurrentIs(in SyntaxKind kind)
	{
		if (IsAtEnd && kind != SyntaxKind.EndOfFileToken)
		{
			return false;
		}

		var given = kind;
		return given.HasFlag(Current.Kind);
	}

	private bool CurrentIsNot(in SyntaxKind kind)
	{
		return !CurrentIs(in kind);
	}

	private bool NextIs(in SyntaxKind kind)
	{
		if (IsAtEnd && kind != SyntaxKind.EndOfFileToken)
		{
			return false;
		}

		var given = kind;
		return given.HasFlag(Next.Kind);
	}

	private bool NextIsNot(in SyntaxKind kind)
	{
		return !NextIs(in kind);
	}
	
	private ISyntaxToken Peek(in int offset)
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

	private bool TryMatch<TValue>(in SyntaxKind kind, [NotNullWhen(true)] out SyntaxToken<TValue>? token)
	{
		if (CurrentIs(in kind))
		{
			token = Match<TValue>(in kind);
			return true;
		}

		token = null;
		return false;
	}

	private SyntaxToken<TValue> Match<TValue>(in SyntaxKind kind)
	{
		return (SyntaxToken<TValue>)Match(in kind);
	}

	private ISyntaxToken Match(in SyntaxKind kind)
	{
		if (CurrentIs(in kind))
		{
			return Consume();
		}

		// Whoops, let's make sure we maintain the tree
		// Let's go ahead and eat the token anyways, so we don't get stuck in an infinite loop,
		// we'll keep going until we reach the end
		m_diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, kind, out var diagnostic);
		// we'll generate an error token
		return new SyntaxToken<Error>(Current.LeadingTrivia,
		                              SyntaxKind.BadToken,
		                              "ERROR",
		                              Current.Text,
		                              Current.Position,
		                              Current.Line,
		                              Current.TrailingTrivia, 
		                              diagnostic);
	}
}