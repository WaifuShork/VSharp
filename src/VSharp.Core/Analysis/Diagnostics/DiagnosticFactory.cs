using System.Diagnostics;

namespace VSharp.Core.Analysis.Diagnostics;

public class DiagnosticFactory
{
	private readonly List<DiagnosticInfo> m_errors = new();
	private readonly List<DiagnosticInfo> m_warnings = new();

	public IReadOnlyList<DiagnosticInfo> Errors => m_errors;
	public IReadOnlyList<DiagnosticInfo> Warnings => m_warnings;

	public void MakeError(DiagnosticInfo error)
	{
		m_errors.Add(error);
	}

	public void MakeWarning(DiagnosticInfo warning)
	{
		m_warnings.Add(warning);
	}
}