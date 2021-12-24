using System.Drawing;
using WaifuShork.Common.Extensions;
using WaifuShork.Common.Formatters;

namespace VSharp.Core.Utilities.IO;

public static class Vonsole
{
	private static readonly SemaphoreSlim s_slim = new(1, 1);
	
	private static readonly TextWriter s_out = TextWriter.Synchronized(Console.Out);
	private static readonly TextReader s_in = TextReader.Synchronized(Console.In);

	public static Color ForegroundColor { get; set; } = Color.White;
	public static Color BackgroundColor { get; set; } = Color.Black;

	private static string Colorize(this string str)
	{
		return str.ColorizeForeground(ForegroundColor).ColorizeBackground(BackgroundColor);
	}
	
	public static async Task WriteAsync<T>(T? item = default)
	{
		await s_slim.WaitAsync();
		try
		{
			if (item is null)
			{
				await s_out.WriteAsync("");
				return;
			}

			if (item is string str)
			{
				await s_out.WriteAsync(str.Colorize());
				return;
			}

			await s_out.WriteAsync((item as string ?? item.ToString() ?? "").Colorize());
		}
		finally
		{
			s_slim.Release();
		}
	}

	public static async Task WriteLineAsync<T>(T? item = default)
	{
		await s_slim.WaitAsync();
		try
		{
			if (item is null)
			{
				await s_out.WriteLineAsync("");
				return;
			}

			if (item is string str)
			{
				await s_out.WriteLineAsync(str.Colorize());
				return;
			}

			await s_out.WriteLineAsync((item as string ?? item.ToString() ?? "").Colorize());
		}
		finally
		{
			s_slim.Release();
		}
	}

	public static async Task<ConsoleKey?> ReadKeyAsync()
	{
		await s_slim.WaitAsync();
		try
		{
			var key = default(ConsoleKey);
			await Task.Run(() => key = Console.ReadKey(true).Key);
			return key;
		}
		catch (Exception exception)
		{
			await WriteLineAsync(exception.Prettify());
		}
		finally
		{
			s_slim.Release();
		}

		return null;
	}

	public static async Task<string?> ReadAsync(ulong amount = 100)
	{
		await s_slim.WaitAsync();
		try
		{
			var buffer = new char[amount];
			await s_in.ReadAsync(buffer);
			var validAmount = buffer.Count(c => c != '\0');
			if (validAmount == 0)
			{
				return null;
			}

			return string.Join("", buffer);
		}
		finally
		{
			s_slim.Release();
		}
	}

	public static async Task<string?> ReadLineAsync()
	{
		await s_slim.WaitAsync();
		try
		{
			return await s_in.ReadLineAsync();
		}
		finally
		{
			s_slim.Release();
		}
	}
}