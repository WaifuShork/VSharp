using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Spectre.Console;
using WaifuShork.Common.Attributes;
using WaifuShork.Common.Extensions;
using WaifuShork.Extensions;

namespace VSharp.Core;

public static class VSharpResources
{
	private const string CoreResourceFile = "resources.xml";
	
	[GuaranteedNoThrow]
	private static string GetResourceString(Assembly assembly, string resourceFile)
	{
		try
		{
			var assemblyName = assembly.FullName ?? "VSharp.Core";
			var trimDex = assemblyName.IndexOf(",", StringComparison.CurrentCulture);
			var name = assemblyName[..trimDex];

			using var stream = assembly.GetManifestResourceStream($"{name}.{resourceFile}");
			if (stream is null)
			{
				throw new NullReferenceException($"{typeof(Stream)} was null when attempting to load manifest resource stream");
			}

			using var reader = new StreamReader(stream, Encoding.UTF8);
			return reader.ReadToEnd();
		}
		catch (Exception exception)
		{
			AnsiConsole.WriteException(exception);
			return "";
		}
	}

	public static string? GetResource(string resourceKey)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var document = XDocument.Parse(GetResourceString(assembly, CoreResourceFile));

		return document.Descendants("key")
			.Where(descendent =>
			{
				var attributeName = descendent.Attribute("name")?.Value;
				if (attributeName.IsNullOrWhiteSpace())
				{
					return false;
				}

				return resourceKey.Equals(attributeName);
			})
			.Select(descendent => descendent.Value)
			.FirstOrDefault();
	}
}