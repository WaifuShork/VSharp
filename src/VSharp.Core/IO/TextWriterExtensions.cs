
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using JetBrains.Annotations;
using VSharp.Core.Analysis.Syntax;
using VSharp.Core.Utilities;
using WaifuShork.Common.Extensions;
using WaifuShork.Common.Text;

namespace VSharp.Core.IO;

using System.Text;
using Analysis.Diagnostics;
using Writer = WaifuShork.Common.Extensions.TextWriterExtensions;

public static class TextWriterExtensions
{
	private static readonly Lazy<SyntaxCache> s_cache = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

	public static bool IsConsole(this TextWriter writer)
	{
		if (writer == Console.Out)
		{
			return !Console.IsOutputRedirected;
		}

		if (writer == Console.Error)
		{
			return !Console.IsErrorRedirected && !Console.IsOutputRedirected;
		}

		if (writer is IndentedTextWriter itw && itw.InnerWriter.IsConsole())
		{
			return true;
		}

		return false;
	}

	private static Color s_foregroundColor;

	public static void SetForeground(this TextWriter writer, Color color)
	{
		if (writer.IsConsole())
		{
			s_foregroundColor = color;
		}
	}

	public static void ResetColor(this TextWriter writer)
	{
		if (writer.IsConsole())
		{
			// it'll work for now
			s_foregroundColor = Color.FromName(Console.ForegroundColor.ToString("G"));
		}
	}

	[StringFormatMethod("template")]
	public static async Task FWriteLineAsync<T>(this TextWriter writer, string template, params T[] args)
	{
		await writer.WriteLineAsync(string.Format(template, args).ColorizeForeground(s_foregroundColor));
	}

	[StringFormatMethod("template")]
	public static async Task FWriteAsync<T>(this TextWriter writer, string template, params T[] args)
	{
		await writer.WriteAsync(string.Format(template, args).ColorizeForeground(s_foregroundColor));
	}
	
	public static async Task WriteKeywordAsync(this TextWriter writer, SyntaxKind kind)
	{
		var text = s_cache.Value.LookupText(kind);
		Debug.Assert(text is not null);
		await writer.WriteAsync(text.ColorizeForeground(Color.RoyalBlue));
	}
	
	public static async Task WriteKeywordAsync(this TextWriter writer, string text)
	{
		await writer.WriteAsync(text.ColorizeForeground(Color.RoyalBlue));
	}
	
	public static async Task WriteIdentifierAsync(this TextWriter writer, string text)
	{
		await writer.WriteAsync(text.ColorizeForeground(Color.LightSkyBlue));
	}

	public static async Task WriteNumberAsync<T>(this TextWriter writer, T number) where T : INumber<T>
	{
		var value = number.ToString();
		Debug.Assert(value is not null);
		await writer.WriteAsync(value.ColorizeForeground(Color.Cyan));
	}

	public static async Task WriteStringAsync(this TextWriter writer, string text)
	{
		await writer.WriteAsync(text.ColorizeForeground(Color.Magenta));
	}
	
	public static async Task WriteSpaceAsync(this TextWriter writer)
	{
		await writer.WriteAsync(" ");
	}
	
	public static async Task WritePunctuationAsync(this TextWriter writer, SyntaxKind kind)
	{
		var text = s_cache.Value.LookupText(kind);
		Debug.Assert(text is not null);
		await writer.WritePunctuationAsync(text);
	}
	
	public static async Task WritePunctuationAsync(this TextWriter writer, string text)
	{
		await writer.WriteAsync(text.ColorizeForeground(Color.Gray));
	}
	
	public static async Task WriteDiagnosticsAsync(this TextWriter writer, IEnumerable<DiagnosticInfo> diagnostics)
	{
		var sb = new StringBuilder();
		foreach (var diagnostic in diagnostics.Where(d => d.Location.Source is not null)
		                                      .OrderBy(d => d.Location.FileName)
		                                      .ThenBy(d => d.Location.Span.Start)
		                                      .ThenBy(d => d.Location.Span.End))
		{
			sb.AppendLine(diagnostic.Format());
		}

		await writer.WriteLineAsync(sb);
	}
}