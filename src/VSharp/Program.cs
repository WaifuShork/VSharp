using Spectre.Console;
using VSharp.Core.Utilities.IO;
using VSharp.Core.Analysis.Text;
using VSharp.Core.Analysis.Lexing;
using VSharp.Core.Analysis.Syntax;
using WaifuShork.Common.Extensions;
using Color = System.Drawing.Color;

namespace VSharp;

public static class VSharp
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            return await ExecuteReplAsync(args);
        }

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

        var threadedLexer = new ThreadedLexer(sourceFiles);
        if (threadedLexer.IsCompleted)
        {
            // then we parse, but none of this code will stay here
            var tokens = threadedLexer.GetAllTokens();
            foreach (var token in tokens)
            {
                await Vonsole.WriteLineAsync(token.ToString(Formatting.Indented));
            }
        }

        return 0;
    }
    
    private static async Task<int> ExecuteReplAsync(string[]? args = null)
    {
        await Vonsole.WriteLineAsync($"VSharp {"v0.5".ColorizeForeground(Color.Purple)}");
        while (true)
        {
            await Console.Out.WriteAsync("> ".ColorizeForeground(Color.Cyan));
            var input = await Console.In.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            var threadedLexer = new ThreadedLexer(new[] { SourceText.From(input) });
            while (!threadedLexer.IsCompleted)
            {
                await Task.Delay(5);
            }

            var tokens = threadedLexer.GetAllTokens();
            var diagnostics = threadedLexer.GetAllDiagnostics();

            if (!diagnostics.Any())
            {
                foreach (var token in tokens)
                {
                    await Vonsole.WriteLineAsync(token.ToString(Formatting.Indented));
                }
            }
            else
            {
                foreach (var diagnostic in diagnostics)
                {
                    await Vonsole.WriteAsync("");
                    await Vonsole.WriteLineAsync(diagnostic.Message.ColorizeForeground(Color.DarkRed));
                }
            }
            

            /*var tokens = Lexer.ScanSyntaxTokens(SourceText.From(input), out var diagnostics);
            if (diagnostics.Any())
            {
                foreach (var diagnostic in diagnostics)
                {
                    await VConsole.WriteLineAsync(diagnostic.Message);
                }
            }
            
            foreach (var token in tokens)
            {
                Console.WriteLine(token.ToString());
            }*/
        }

        return 0;
    }

    private static string[]? CollectSourceFiles(string directory, string extension)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            Console.Error.WriteLine("error: directory cannot be null, empty, or whitespace".ColorizeForeground(Color.DarkRed));
            return null;
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            Console.Error.WriteLine("error: extension cannot be null, empty, or whitespace".ColorizeForeground(Color.DarkRed));
            return null;
        }

        if (!Directory.Exists(directory))
        {
            Console.Error.WriteLine($"error: the directory '{directory}' cannot be located".ColorizeForeground(Color.DarkRed));
            return null;
        }

        try
        {
            return Directory.GetFiles(directory, $"*.{extension}", SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            Console.Error.WriteLine("error: you have insufficient permissions".ColorizeForeground(Color.DarkRed));
            AnsiConsole.WriteException(unauthorizedAccessException);
        }
        catch (PathTooLongException pathTooLongException)
        {
            Console.Error.WriteLine("error: the specified path, file name, or both exceed the system-defined maximum length".ColorizeForeground(Color.DarkRed));
            AnsiConsole.WriteException(pathTooLongException);
        }
        catch (IOException ioException)
        {
            Console.Error.WriteLine("error: path is a file name. -or- A network error has occurred".ColorizeForeground(Color.DarkRed));
            AnsiConsole.WriteException(ioException);
        }
        
        return null;
    }
}