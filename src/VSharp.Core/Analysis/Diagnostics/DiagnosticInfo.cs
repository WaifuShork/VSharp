using VSharp.Core.Analysis.Text;

namespace VSharp.Core.Analysis.Diagnostics;

public sealed class DiagnosticInfo
{
	public DiagnosticInfo(DiagnosticKind kind, TextLocation location, string message)
	{
		Message = message;
		Location = location;

		IsError = kind == DiagnosticKind.Error;
		IsWarning = kind == DiagnosticKind.Warning;
	}
	
	public bool IsError { get; }
	public bool IsWarning { get; }
	
	public TextLocation Location { get; }
	public string Message { get; }

	public string TargetSourceSnippet => Location.Text.Substring(Location.Span.Start, Location.Span.End - Location.Span.Start);

	public override string ToString()
	{
		return new string(Message);
	}
}