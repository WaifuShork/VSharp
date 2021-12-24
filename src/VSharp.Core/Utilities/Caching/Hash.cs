using System.Collections.Immutable;

namespace VSharp.Core.Utilities.Caching;

public static class Hash
{
	public static int Combine(int newKey, int currentKey)
	{
		return unchecked((currentKey * (int)0xA5555529) + newKey);
	}

	public static int Combine(bool newKeyPart, int currentKey)
	{
		return Combine(currentKey, newKeyPart ? 1 : 0);
	}

	public static int Combine<T>(T newKeyPart, int currentKey) where T : class?
	{
		var hash = unchecked(currentKey * (int)0xA5555529);
		if (newKeyPart is not null)
		{
			return unchecked(hash + newKeyPart.GetHashCode());
		}

		return hash;
	}
	
    public static int CombineValues<T>(IEnumerable<T>? values, int maxItemsToHash = int.MaxValue)
    {
        if (values == null)
        {
            return 0;
        }

        var hashCode = 0;
        var count = 0;
        foreach (var value in values)
        {
            if (count++ >= maxItemsToHash)
            {
                break;
            }

            // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
            if (value != null)
            {
                hashCode = Combine(value.GetHashCode(), hashCode);
            }
        }

        return hashCode;
    }

    internal static int CombineValues<T>(T[]? values, int maxItemsToHash = int.MaxValue)
    {
        if (values == null)
        {
            return 0;
        }

        var maxSize = Math.Min(maxItemsToHash, values.Length);
        var hashCode = 0;

        for (var i = 0; i < maxSize; i++)
        {
            var value = values[i];

            // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
            if (value != null)
            {
                hashCode = Combine(value.GetHashCode(), hashCode);
            }
        }

        return hashCode;
    }

    internal static int CombineValues<T>(ImmutableArray<T> values, int maxItemsToHash = int.MaxValue)
    {
        if (values.IsDefaultOrEmpty)
        {
            return 0;
        }

        var hashCode = 0;
        var count = 0;
        foreach (var value in values)
        {
            if (count++ >= maxItemsToHash)
            {
                break;
            }

            // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
            if (value != null)
            {
                hashCode = Combine(value.GetHashCode(), hashCode);
            }
        }

        return hashCode;
    }

    internal static int CombineValues(IEnumerable<string?>? values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue)
    {
        if (values == null)
        {
            return 0;
        }

        var hashCode = 0;
        var count = 0;
        foreach (var value in values)
        {
            if (count++ >= maxItemsToHash)
            {
                break;
            }

            if (value != null)
            {
                hashCode = Combine(stringComparer.GetHashCode(value), hashCode);
            }
        }

        return hashCode;
    }

    public const int FnvOffsetBias = unchecked((int)2166136261);
    public const int FnvPrime = 16777619;

    public static int GetFNVHashCode(byte[] data)
    {
        var hashCode = FnvOffsetBias;
        foreach (var item in data)
        {
            hashCode = unchecked((hashCode ^ item) * FnvPrime);
        }

        return hashCode;
    }
    
    public static int GetFNVHashCode(ReadOnlySpan<byte> data, out bool isAscii)
    {
        var hashCode = FnvOffsetBias;

        byte asciiMask = 0;

        foreach (var item in data)
        {
            asciiMask |= item;
            hashCode = unchecked((hashCode ^ item) * FnvPrime);
        }

        isAscii = (asciiMask & 0x80) == 0;
        return hashCode;
    }

    public static int GetFNVHashCode(ImmutableArray<byte> data)
    {
        var hashCode = Hash.FnvOffsetBias;

        foreach (var item in data)
        {
            hashCode = unchecked((hashCode ^ item) * FnvPrime);
        }

        return hashCode;
    }
    
    public static int GetFNVHashCode(ReadOnlySpan<char> data)
    {
        var hashCode = FnvOffsetBias;

        foreach (var item in data)
        {
            hashCode = unchecked((hashCode ^ item) * FnvPrime);
        }

        return hashCode;
    }

    public static int GetFNVHashCode(string text, int start, int length)
    {
        return GetFNVHashCode(text.AsSpan(start, length));
    }

    public static int GetCaseInsensitiveFNVHashCode(string text)
    {
        return GetCaseInsensitiveFNVHashCode(text.AsSpan(0, text.Length));
    }

    public static int GetCaseInsensitiveFNVHashCode(ReadOnlySpan<char> data)
    {
        var hashCode = FnvOffsetBias;

        foreach (var item in data)
        {
            hashCode = unchecked((hashCode ^ char.ToLowerInvariant(item)) * FnvPrime);
        }

        return hashCode;
    }

    public static int GetFNVHashCode(string text, int start)
    {
        return GetFNVHashCode(text, start, length: text.Length - start);
    }

    public static int GetFNVHashCode(string text)
    {
        return CombineFNVHash(Hash.FnvOffsetBias, text);
    }

    public static int GetFNVHashCode(System.Text.StringBuilder text)
    {
        var hashCode = Hash.FnvOffsetBias;
        var end = text.Length;

        for (var i = 0; i < end; i++)
        {
            hashCode = unchecked((hashCode ^ text[i]) * FnvPrime);
        }

        return hashCode;
    }

    public static int GetFNVHashCode(char[] text, int start, int length)
    {
        var hashCode = FnvOffsetBias;
        var end = start + length;

        for (var i = start; i < end; i++)
        {
            hashCode = unchecked((hashCode ^ text[i]) * FnvPrime);
        }

        return hashCode;
    }

    public static int GetFNVHashCode(char ch)
    {
        return CombineFNVHash(FnvOffsetBias, ch);
    }

    public static int CombineFNVHash(int hashCode, string text)
    {
        foreach (var ch in text)
        {
            hashCode = unchecked((hashCode ^ ch) * FnvPrime);
        }

        return hashCode;
    }

    public static int CombineFNVHash(int hashCode, char ch)
    {
        return unchecked((hashCode ^ ch) * FnvPrime);
    }
}