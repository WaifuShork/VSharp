using System.Collections;
using VSharp.Core.Analysis.Syntax;
using VSharp.Core.Analysis.Text;
using VSharp.Core.Extensions;

namespace VSharp.Core.Analysis.Diagnostics;

public sealed class DiagnosticBag : IEnumerable<DiagnosticInfo>
{
	private readonly DiagnosticFactory m_factory = new();
	private List<DiagnosticInfo> m_cache => 
		new List<DiagnosticInfo>().AppendRange(m_factory.Errors).AppendRange(m_factory.Warnings);

	public Lazy<IReadOnlyList<DiagnosticInfo>> Cache => new(m_cache);

	public void AddRange(IReadOnlyList<DiagnosticInfo> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
		{
			if (diagnostic.IsWarning)
			{
				m_factory.MakeWarning(diagnostic);
			}
			else if (diagnostic.IsError)
			{
				m_factory.MakeError(diagnostic);
			}
		}
	}
	
	private void ReportError(TextLocation location, string message)
	{
		m_factory.MakeError(new DiagnosticInfo(DiagnosticKind.Error, location, message));
	}

	private void ReportWarning(TextLocation location, string message)
	{
		m_factory.MakeWarning(new DiagnosticInfo(DiagnosticKind.Error, location, message));
	}

	public void ReportDivideByZero(TextLocation location)
	{
		ReportError(location, "division by zero");
	}

	public void ReportUnterminatedMultiLineComment(TextLocation location)
	{
		ReportError(location, "unterminated multi-line comment");
	}

	public void ReportBadCharacter(TextLocation location, char character)
	{
		ReportError(location, $"bad character input: '{character}'");
	}

	public void ReportEmptyCharConst(TextLocation location)
	{
		ReportError(location, "empty character constant");
	}

	public void ReportInvalidCharacterConst(TextLocation location)
	{
		ReportError(location, "invalid character constant");
	}

	public void ReportUnterminatedString(TextLocation location)
	{
		ReportError(location, "unterminated string literal");
	}

	public void ReportUnexpectedToken(TextLocation location, char actual)
	{
		ReportError(location, $"unexpected character '{actual}'");
	}
	
	public void ReportUnexpectedToken(TextLocation location, SyntaxKind actual, SyntaxKind expected)
	{
		ReportError(location, $"unexpected token <{actual:G}>, expected <{expected:G}>");
	}
	
	public IEnumerator<DiagnosticInfo> GetEnumerator()
	{
		return m_cache.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}