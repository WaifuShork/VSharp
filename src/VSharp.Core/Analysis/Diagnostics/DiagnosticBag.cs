namespace VSharp.Core.Analysis.Diagnostics;

using Text;
using Syntax;
using System.Collections;

public sealed class DiagnosticBag : IEnumerable<DiagnosticInfo>
{
	private readonly DiagnosticFactory m_factory = new();
	public Lazy<IEnumerable<DiagnosticInfo>> Cache => new(() => m_factory.All, LazyThreadSafetyMode.ExecutionAndPublication);

	public void AddRange(IReadOnlyList<DiagnosticInfo> diagnostics)
	{
		_ = m_factory.MakeWarnings(diagnostics.Where(d => d.IsWarning));
		_ = m_factory.MakeErrors(diagnostics.Where(d => d.IsError) );
	}

	public static T GenerateErrorNode<T>(DiagnosticInfo info) where T : SyntaxNode
	{
		if (Activator.CreateInstance(typeof(T), info) is T valid)
		{
			return valid;
		}

		throw new InvalidOperationException($"{typeof(T)} is node a valid Error Node Type, you may only use: " +
		                                    $"{nameof(ErrorNode)}, " +
		                                    $"{nameof(ErrorMemberNode)}, " +
		                                    $"{nameof(ErrorStatementNode)} and " +
		                                    $"{nameof(ErrorExpressionNode)}");
	}
	
	private DiagnosticInfo ReportStaticError(TextLocation location, string message)
	{
		return m_factory.MakeError(new DiagnosticInfo(DiagnosticKind.StaticError, location, message));
	}

	private DiagnosticInfo ReportStaticWarning(TextLocation location, string message)
	{
		return m_factory.MakeWarning(new DiagnosticInfo(DiagnosticKind.StaticError, location, message));
	}
	
	public void ReportDivideByZero(TextLocation location, out DiagnosticInfo diagnostic)
	{
		diagnostic = ReportStaticError(location, "division by zero");
	}

	public void ReportUnterminatedMultiLineComment(TextLocation location, out DiagnosticInfo diagnostic)
	{
		diagnostic = ReportStaticError(location, "unterminated multi-line comment");
	}

	public void ReportBadCharacter(TextLocation location, char character, out DiagnosticInfo diagnostic)
	{
		diagnostic = ReportStaticError(location, $"bad character input: '{character}'");
	}

	public void ReportEmptyCharConst(TextLocation location, out DiagnosticInfo diagnostic)
	{
		var message = LookupMessage("Error.EmptyCharacterConstant");
		diagnostic = ReportStaticError(location, message);
	}

	public void ReportInvalidCharacterConst(TextLocation location, out DiagnosticInfo diagnostic)
	{
		var message = LookupMessage("Error.InvalidCharacterConstant");
		diagnostic = ReportStaticError(location, message);
	}

	public void ReportUnterminatedCharacterConstant(TextLocation location, out DiagnosticInfo diagnostic)
	{
		diagnostic = ReportStaticError(location, "unterminated character literal");
	}
	
	public void ReportUnterminatedString(TextLocation location, out DiagnosticInfo diagnostic)
	{
		var message = LookupMessage("Error.UnterminatedStringLiteral");
		diagnostic = ReportStaticError(location, message);
	}

	public void ReportUnexpectedCharacter(TextLocation location, char actual, out DiagnosticInfo diagnostic)
	{
		var templateMessage = LookupMessage("Error.UnexpectedCharacter");
		diagnostic = ReportStaticError(location, string.Format(templateMessage, actual));
	}

	public void ReportUnexpectedToken(TextLocation location, SyntaxKind actual, out DiagnosticInfo diagnostic)
	{
		var templateMessage = LookupMessage("Error.UnexpectedWithoutExpectedToken");
		diagnostic = ReportStaticError(location, string.Format(templateMessage, actual));
	}
	
	public void ReportUnexpectedToken(TextLocation location, SyntaxKind actual, SyntaxKind expected, out DiagnosticInfo diagnostic)
	{
		var templateMessage = LookupMessage("Error.UnexpectedToken");
		diagnostic = ReportStaticError(location, string.Format(templateMessage, actual, expected));
	}
	
	public void ReportImplicitlyTypedVariableNoInitializer(TextLocation location, out DiagnosticInfo diagnostic)
	{
		var templateMessage = LookupMessage("Error.ImplicitlyTypedVariableNoInitializer");
		diagnostic = ReportStaticError(location, templateMessage);
	}
	
	public void ReportEmptyModifierList(TextLocation location, out DiagnosticInfo diagnostic)
	{
		var templateMessage = LookupMessage("Error.EmptyModifierList");
		diagnostic = ReportStaticError(location, templateMessage);
	}
	
	private static string LookupMessage(string key)
	{
		return VSharpResources.GetResource(key) ?? "ResourceNotDefined";
	}
	
	public IEnumerator<DiagnosticInfo> GetEnumerator()
	{
		return Cache.Value.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}