namespace VSharp.Core.Extensions;

using WaifuShork.Extensions;

public static class GenericExtensions
{
	public static object Box<T>(this T value) where T : struct
	{
		return value;
	}

	public static T Unbox<T>(this object value) where T : struct
	{
		return value.As<T>();
	}
}