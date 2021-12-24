using System.Diagnostics;
using System.Text;

namespace VSharp.Core.Utilities.Caching;

public class PooledStringBuilder
{
	public readonly StringBuilder Builder = new();
	private readonly ObjectPool<PooledStringBuilder> m_pool;

	private PooledStringBuilder(ObjectPool<PooledStringBuilder> pool)
	{
		Debug.Assert(pool is not null);
		m_pool = pool;
	}

	public int Length => Builder.Length;

	public void Free()
	{
		var builder = Builder;

		if (builder.Capacity <= 1024)
		{
			builder.Clear();
			m_pool.Free(this);
		}
		else
		{
			m_pool.ForgetTrackedObject(this);
		}
	}

	public string ToStringAndFree()
	{
		var result = Builder.ToString();
		Free();
		return result;
	}

	public string ToStringAndFree(int startIndex, int length)
	{
		var result = Builder.ToString(startIndex, length);
		Free();
		return result;
	}

	private static readonly ObjectPool<PooledStringBuilder> s_poolInstance = CreatePool();

	public static ObjectPool<PooledStringBuilder> CreatePool(int size = 12)
	{
		ObjectPool<PooledStringBuilder>? pool = null;
		return new ObjectPool<PooledStringBuilder>(() => new PooledStringBuilder(pool!), size);
	}

	public static PooledStringBuilder GetInstance()
	{
		var builder = s_poolInstance.Allocate();
		Debug.Assert(builder.Builder.Length == 0);
		return builder;
	}

	public static implicit operator StringBuilder(PooledStringBuilder builder)
	{
		return builder.Builder;
	}
}