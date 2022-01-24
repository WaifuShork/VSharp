namespace VSharp.Core.Analysis.Diagnostics;

using Text;

public sealed class DiagnosticInfo
{
	public DiagnosticInfo(DiagnosticKind kind, TextLocation location, string message)
	{
		Message = message;
		Location = location;

		IsError = kind == DiagnosticKind.Error;
		IsWarning = kind == DiagnosticKind.Warning;
		IsStatic = kind == DiagnosticKind.StaticError
		        || kind == DiagnosticKind.StaticWarning;
	}
	
	public bool IsError { get; }
	public bool IsWarning { get; }
	public bool IsStatic { get; }
	
	public TextLocation Location { get; }
	public string Message { get; }
	public TextSpan Span => new(Location.Span.Start, Location.Span.Length);

	public string TargetSourceSnippet => Location.Source.Substring(Location.Span.Start, Location.Span.End - Location.Span.Start);

	public override string ToString()
	{
		return Message;
	}

	public Task DumpAsync()
	{
		return Console.Out.WriteLineAsync(this.Format());
	}
}