namespace VSharp.Core.Utilities;

using JetBrains.Annotations;

public static class Throw<TException> where TException : Exception
{
	[ContractAnnotation("value:null => halt")]
	public static void IfNull<T>(T value, string? message = default)
	{
		If(value is null, message);
	}
	
	[ContractAnnotation("condition:true => halt")]
	public static void If(bool condition, string? message = default) 
	{
		if (condition is false)
		{
			return;
		}

		var exception = string.IsNullOrWhiteSpace(message)
			? Activator.CreateInstance(typeof(TException)) as TException
			: Activator.CreateInstance(typeof(TException), message) as TException;

		if (exception is null)
		{
			return;
		}

		throw exception;
	}
}