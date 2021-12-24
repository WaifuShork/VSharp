using System.Collections.Concurrent;
using VSharp.Core.Analysis.Text;
using VSharp.Core.Analysis.Syntax;
using VSharp.Core.Analysis.Diagnostics;
using VSharp.Core.Extensions;

namespace VSharp.Core.Analysis.Lexing;

/// <summary>
/// A multi-threaded Lexer pool of threads, taking in all source files, lexing them in parallel,
/// and conjoining all the tokens to avoid the slowdowns that come with sequential lexing, file-
/// after file. This shouldn't be spawned for single file lexing, because the overhead it generates
/// won't be work the return you actually gain from the parallelization.
/// 
/// Notes:
/// 1) it is not the goal to speed up the lexer itself, this merely serves as an interface for lexing
///	   large quantities of text files in a safe manner, without duplicating code.
///
/// Rationale:
///	   there's no need to use this interface if you plan on lexing a handful of files (however that's currently-
///	   the default for this program), it really shines when you have several files.	
/// </summary>
public sealed class ThreadedLexer
{
	private readonly ConcurrentDictionary<int, LexerThread> m_lexers;

	public ThreadedLexer(IReadOnlyList<SourceText?> files)
	{
		// ConcurrentDictionary doesn't let you just call a constructor with a count, it forces you to,
		// use the concurrency level and count, so I'm just using the default it will usually assign
		m_lexers = new ConcurrentDictionary<int, LexerThread>(Environment.ProcessorCount, files.Count);
		Parallel.For(0, files.Count, i =>
		{
			var file = files[i];
			// No need to waste resources on an empty file, that way a workers Tokens
			// should never be empty, it will always at least contain some sort of value
			if (file.IsNullOrWhiteSpace())
			{
				return;
			}

			var lexerThread = new LexerThread
			{
				Id = (ulong)i,
				Text = file,
				Worker = new Thread(LexFile),
				Tokens = new(),
				Diagnostics = new(),
			};
			
			// NOTE: AddOrUpdate is called over TryAdd so I can get the newly added value to invoke the new thread start
			var thread = m_lexers.AddOrUpdate(i, lexerThread, (_, _) => lexerThread);
			thread.Worker?.Start(m_lexers[i]);
			thread.Worker?.Join();
		});
	}
	
	public bool IsCompleted
	{
		get
		{
			var finished = m_lexers.Values.Where(thread => !thread.Worker?.IsAlive ?? true).ToList();
			if (finished.Count == m_lexers.Count)
			{
				return true;
			}

			return false;
		}
	}
	
	public IReadOnlyList<SyntaxToken> GetAllTokens()
	{
		var tokenCount = m_lexers.Values.Sum(t => t.Tokens?.Count);
		var tokens = new List<SyntaxToken>(tokenCount ?? 20);
		
		// it always starts from 0, so need to use LINQs OrderBy method, 
		// this also needs to be done synchronously, so tokens aren't added
		// out of order, which would cause unintended syntax errors
		for (var i = 0; i < m_lexers.Count; i++)
		{
			var tempTokens = m_lexers[i].Tokens; 
			// how should I handle if tokens are null? does that means 
			// the Lexer was unable to finish? or something happened?
			// TODO: investigate a good way to report something like a LexerStatus for situations like this
			if (tempTokens is null || tempTokens.Count == 0)
			{
				continue;
			}
			
			// The order each files tokens are added is irrelevant, 
			// as long as all of them are declared and relevant
			tokens.AddRange(tempTokens);
		}

		return tokens;
	}

	public IReadOnlyList<DiagnosticInfo> GetAllDiagnostics()
	{
		var diagnosticCount = m_lexers.Values.Sum(t => t.Diagnostics?.Count);
		var diagnostics = new List<DiagnosticInfo>(diagnosticCount ?? 20);

		for (var i = 0; i < m_lexers.Count; i++)
		{
			var tempDiags = m_lexers[i].Diagnostics;
			if (tempDiags is null || tempDiags.Count == 0)
			{
				continue;
			}
			diagnostics.AddRange(tempDiags);
		}

		return diagnostics;
	}

	private void LexFile(object? obj)
	{
		if (obj is not LexerThread thread)
		{
			return;
		}

		if (thread.Text is null || thread.Text.IsNullOrWhiteSpace())
		{
			return;
		}

		var tokens = Lexer.ScanSyntaxTokens(thread.Text, out var diagnostics);
		thread.Tokens?.AddRange(tokens);
		thread.Diagnostics?.AddRange(diagnostics);
	}
	
	public readonly struct LexerThread
	{
		public ulong Id { get; init; }
		public SourceText? Text { get; init; }
		public Thread? Worker { get; init; }
		public List<SyntaxToken>? Tokens { get; init; }
		public List<DiagnosticInfo>? Diagnostics { get; init; } 
	}
}