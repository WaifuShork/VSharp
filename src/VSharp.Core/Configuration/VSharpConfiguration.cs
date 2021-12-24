using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
using WaifuShork.Common.Extensions;
using WaifuShork.Common.Formatters;
using Color = System.Drawing.Color;

namespace VSharp.Core.Configuration;

public class VSharpConfiguration
{
	[JsonPropertyName("AssemblyName"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public string? AssemblyName { get; init; }
	
	[JsonPropertyName("CompilerOptions"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public CompilerOptions? CompilerOptions { get; init; }
	
	[JsonPropertyName("SourceFiles"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyList<string>? SourceFiles { get; init; }

	[JsonPropertyName("DllReferences"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public IReadOnlyList<string>? DllReferences { get; init; }

	public static async Task<VSharpConfiguration?> LoadAsync(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			await Console.Error.WriteLineAsync("error: you must include a configuration file for compilation".ColorizeForeground(Color.DarkRed));
			return null;
		}

		if (!File.Exists(path))
		{
			await Console.Error.WriteLineAsync($"error: could not locate configuration file '{path}'".ColorizeForeground(Color.DarkRed));
			return null;	
		}

		try
		{
			var contents = await File.ReadAllTextAsync(path);
			await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
			return await JsonSerializer.DeserializeAsync<VSharpConfiguration>(stream);
		}
		catch (Exception exception)
		{
			await Console.Error.WriteLineAsync("error: an exception was thrown".ColorizeForeground(Color.DarkRed));
			AnsiConsole.WriteException(exception);
			return null;
		}
	}
}

public class CompilerOptions
{
	[JsonPropertyName("FileResolver"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public string? FileResolver { get; init; } // recursive, naive
	
	[JsonPropertyName("SdkPath"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public string? SdkPath { get; init; }
	
	[JsonPropertyName("OutputPath"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public string? OutputPath { get; init; }
	
	[JsonPropertyName("WorkingDir"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public string? WorkingDir { get; init; }
}