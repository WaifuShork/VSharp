using System.Collections;

namespace VSharp.Core.Analysis.Diagnostics;

public class DiagnosticFactory
{
	private readonly List<DiagnosticInfo> m_errors = new();
	private readonly List<DiagnosticInfo> m_warnings = new();

	public IEnumerable<DiagnosticInfo> Errors => m_errors;
	public IEnumerable<DiagnosticInfo> Warnings => m_warnings;

	public IEnumerable<DiagnosticInfo> All => new List<DiagnosticInfo>()
		.AppendRange(m_errors)
		.AppendRange(m_warnings);

	public DiagnosticInfo MakeError(DiagnosticInfo error)
	{
		m_errors.Add(error);
		return error;
	}

	public IEnumerable<DiagnosticInfo> MakeErrors(IEnumerable<DiagnosticInfo> errors)
	{
		foreach (var error in errors)
		{
			m_errors.Add(error);
			yield return error;
		}
	}
	
	public DiagnosticInfo MakeWarning(DiagnosticInfo warning)
	{
		m_warnings.Add(warning);
		return warning;
	}

	public IEnumerable<DiagnosticInfo> MakeWarnings(IEnumerable<DiagnosticInfo> warnings)
	{
		foreach (var warning in warnings)
		{
			m_warnings.Add(warning);
			yield return warning;
		} 
	}
}