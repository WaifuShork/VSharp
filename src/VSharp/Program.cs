using System.Text;
using System.Globalization;
using Color = System.Drawing.Color;

using VSharp.Core.Extensions;
using VSharp.Core.Configuration;
using VSharp.Core.Analysis.Text;
using VSharp.Core.Analysis.Syntax;
using VSharp.Core.Analysis.Lexing;
using VSharp.Core.Analysis.Diagnostics;

using Spectre.Console;
using WaifuShork.Common.Extensions;

namespace VSharp;

public static class VSharp
{
    private static async Task<int> Main(string[] args)
    { 
        if (args.Length == 0)
        {
            return await ExecuteReplAsync(args);
        }
        
        // get arguments after the initial "vsharp run" / "vsharp start" command
        var newArgs = args.Length > 1 ? args[1..args.Length] : null;
        if (args[0] == "run")
        {
            return await CompileAsync(ArraySegment<SourceText>.Empty, newArgs);
        }
        if (args[0] == "start")
        {
            return await ExecuteReplAsync(newArgs);
        }
        
        return 1;
    }

    private static async Task<int> CompileAsync(IReadOnlyList<SourceText?> sourceFiles, string[]? args = null)
    {
        // TODO: parse arguments properly
        if (sourceFiles is null || sourceFiles.Count == 0)
        {
            return 1;
        }

        IReadOnlyList<DiagnosticInfo> diagnostics;
        IReadOnlyList<ISyntaxToken> tokens;
        
        if (sourceFiles.Count > 1)
        {
            var threadedLexer = new ThreadedLexer(sourceFiles);
            threadedLexer.Wait();
            tokens = threadedLexer.GetAllTokens();
            diagnostics = threadedLexer.GetStaticDiagnostics();
            if (!diagnostics.Any())
            {
                foreach (var token in tokens)
                {
                    await Console.Out.WriteLineAsync(token.ToString(Formatting.Expanded));
                }    
            }
            else
            {
                foreach (var diagnostic in diagnostics)
                {
                    diagnostic.Format();
                }
            }

            return 0;    
        }

        var source = sourceFiles[0];
        if (source is null || source.ToString(CultureInfo.CurrentCulture).IsNullOrWhiteSpace())
        {
            return 1;
        }
        
        try
        {
            tokens = Lexer.ScanSyntaxTokens(source, out diagnostics);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 1;
        }

        if (!diagnostics.Any())
        {
            foreach (var token in tokens)
            {
                await Console.Out.WriteLineAsync(token.ToString(Formatting.Expanded));
            }
        }

        foreach (var diagnostic in diagnostics)
        {
            await diagnostic.DumpAsync();
        }

        return 1;
    }
    
    private static async Task<int> ExecuteReplAsync(string[]? args = null)
    {
        await Console.Out.WriteLineAsync($"VSharp [{LanguageVersion.VSharpAlpha.String().ColorizeForeground(Color.Purple)}]");

        var currentLine = 1;
        var textBuilder = new StringBuilder();
        while (true)
        {
            await Console.Out.WriteAsync($"({currentLine})> ".ColorizeForeground(Color.Cyan)); 
            
            var input = await Console.In.ReadLineAsync();
            if (input == "#reset")
            {
                currentLine = 1;
                textBuilder.Clear();
                Console.Clear();
                await Console.Out.WriteLineAsync($"VSharp [{LanguageVersion.VSharpAlpha.String().ColorizeForeground(Color.Purple)}]");
                continue;
            }
            if (input == "#submit")
            {
                currentLine = 1;
                goto output;    
            }
            
            if (currentLine < int.MaxValue)
            {
                currentLine++;
                textBuilder.AppendLine(input);
                continue;
            }
            
            output:
            var text = textBuilder.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                break;
            }
            
            var source = SourceText.From(text);
            var syntaxTree = SyntaxTree.Parse(source);
            
            await syntaxTree.Root.WriteToAsync(Console.Out);

            if (syntaxTree.Diagnostics.Any())
            {
                await syntaxTree.Diagnostics.DumpDiagnosticsAsync();
            }

            textBuilder.Clear();
            /*if (!syntaxTree.Diagnostics.Any())
            {
                var e = new Evaluator(syntaxTree.Root);
                var result = e.Evaluate();
                Console.WriteLine(result);
            }
            else
            {
                await syntaxTree.Diagnostics.DumpDiagnosticsAsync();
            }*/
        }
        
        return 0;
    }

    private static string[]? CollectSourceFiles(string directory, string extension)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(directory, "error: directory cannot be null, empty, or whitespace".ColorizeForeground(Color.DarkRed));
            ArgumentNullException.ThrowIfNull(extension, "error: extension cannot be null, empty, or whitespace".ColorizeForeground(Color.DarkRed));
            ArgumentNullException.ThrowIfNull(directory, $"error: the directory '{directory}' cannot be located".ColorizeForeground(Color.DarkRed));
            return Directory.GetFiles(directory, $"*.{extension}", SearchOption.AllDirectories);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return null;
        }
    }

    private static async Task<string?> ReadLineAsync(TimeSpan timeout)
    {
        var task = Task.Factory.StartNew(Console.In.ReadLineAsync);
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        return ReferenceEquals(task, completedTask) ? await task.Result : string.Empty;
    }
}



