namespace VSharp.Core.Analysis.Diagnostics;

[Flags]
public enum DiagnosticKind
{
	Error = 1,
	Warning = 2,
	StaticError = 4,
	StaticWarning = 8,
}