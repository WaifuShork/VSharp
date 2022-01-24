namespace VSharp.Core.Configuration;

[PublicAPI]
public static class LanguageVersion
{
	public static readonly Version VSharpAlpha = new(0, 5);
	public static readonly Version VSharp1 = new(1, 0);

	public static string String(this Version version)
	{
		return $"v{version.ToString()}";
	}
}