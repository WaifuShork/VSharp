namespace VSharp.Core.Analysis.Diagnostics;

using Text;
using System.Text;
using System.Drawing;
using System.Globalization;
using WaifuShork.Common.Extensions;

public static class DiagnosticPrinter
{
	private static bool CanBeColored => OperatingSystem.IsWindows();
	private const string Tab = "    ";
	private const string RightChevron = "»";
	private const string LeftChevron = "«";
	private static readonly string EqualsChar = CanBeColored ? "=".ColorizeForeground(Color.SkyBlue) : "=";

	private static readonly StringBuilder ErrorBuilder = new();

	public static string Format(this DiagnosticInfo diagnostic)
	{
		// since this is a static instance, we just allocate up front and clear for each call
		ErrorBuilder.Clear(); 
		var source = diagnostic.Location.Source;
		
		var lineIndex = source.GetLineIndex(diagnostic.Span.Start);
		var line = source.Lines[lineIndex];
		var lineNumber = lineIndex + 1;
		var character = diagnostic.Span.Start + line.Start + 1;

		var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
		var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

		var prefix = source.Substring(prefixSpan);
		var error = source.Substring(diagnostic.Span);
		var suffix = source.Substring(suffixSpan);

		ErrorBuilder.AppendLine();

		var time = $"({FormatTime(DateTime.Now)}) -{RightChevron} ";
		var errorPrefix = ErrorPrefix(diagnostic.IsError, diagnostic.IsStatic);
		// (12:12:12) -> error(static): error message
		ErrorBuilder.AppendLine($"{time}{errorPrefix}{diagnostic.ToString().TryColor(Color.DarkRed)}");

		// --> src/script.vs(1:1)
		var lineInfo = LineNumber(source.FileName, lineNumber, character);
		ErrorBuilder.AppendLine(lineInfo);

		// |
		ErrorBuilder.AppendLine(NoLnPaddedWall(lineNumber) + Tab);
		
		// 1 |   // some code
		var errorMessage = $"{LnPaddedWall(lineNumber)}{Tab}{prefix}{error.TryColor(Color.Red)}{suffix}";
		ErrorBuilder.AppendLine(errorMessage);	
		
		const string hint = "hints disabled";
		// if (!string.IsNullOrWhiteSpace(diagnostic.Hint))
		// {
		// 	hint = diagnostic.Hint;
		// }
		
		//   |    ^^ point to error with hint
		var errorPointer = NoLnPaddedWall(lineNumber) + Tab + PinpointError(prefix.Length, error.Length) + $" {hint}";
		ErrorBuilder.AppendLine(errorPointer);
		ErrorBuilder.AppendLine(NoLnPaddedWall(lineNumber));
		
		/* TODO: support error/warn notes and helpful links to correct if possible 
		//	 = note: any additional error notes
		//	 = help: further help, links and what not
		if (!string.IsNullOrWhiteSpace(diagnostic.Notes))
		{
			// = note: message
			ErrorBuilder.AppendLine(new string(' ', lineNumber) + $" {SafeEqualsChar} note: {diagnostic.Notes}");
		}

		if (!string.IsNullOrWhiteSpace(diagnostic.HelpMessage))
		{
			// = help: message
			ErrorBuilder.AppendLine(new string(' ', lineNumber) + $" {SafeEqualsChar} help: {diagnostic.HelpMessage}");
		}
		*/
		
		return ErrorBuilder.ToString();
	}

	private static string FormatTime(DateTime time)
	{
		return time.ToString("T", CultureInfo.InvariantCulture).TryColor(Color.Turquoise);
	}
	
	// Pads a wall to match regardless of line number, since:
	// 1   | 
	// and 
	// 100 | 
	// would be different widths, if we need to do
	//     |
	// 100 |
	//     |
	// it's easier with this function 
	private static string NoLnPaddedWall(int padAmount)
	{
		return (new string(' ', padAmount) + " |").TryColor(Color.SkyBlue);
	}

	// Creates a wall padding width a visible line number that will match the rest of the padding,
	// event pads without a line number
	private static string LnPaddedWall(int lineNumber)
	{ 
		return $"{lineNumber} |".TryColor(Color.SkyBlue);
	}

	private static string ErrorPrefix(bool isError, bool isStatic)
	{
		// error(static)
		// error(runtime)
		// warning(static)
		// warning(runtime)
		return (isError
			// error(static) or error(runtime)
			? "error".TryColor(Color.Red)
			: "warning".TryColor(Color.Orange)) + (isStatic
			? $"({"static".TryColor(Color.Turquoise)}): "
			: $"({"runtime".TryColor(Color.Turquoise)}): ");
	}
	
	private static string LineNumber(string fileName, int lineNumber, int characterPos)
	{
		// --> src/script.vs:1:2
		var arrow = $"--{RightChevron}".TryColor(Color.SkyBlue);
		return $"{arrow} {fileName}({lineNumber}:{characterPos})";
	}
		
	private static string PinpointError(int prefixLength, int errorLength)
	{
		// |    1 + false
		// |      ^
		return new string(' ', prefixLength) + new string('^', errorLength).TryColor(Color.Goldenrod);
	}

	private static string TryColor(this string msg, Color color)
	{
		return CanBeColored ? msg.ColorizeForeground(color) : msg;
	}
}