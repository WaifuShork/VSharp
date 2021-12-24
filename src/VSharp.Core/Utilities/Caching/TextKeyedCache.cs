namespace VSharp.Core.Utilities.Caching;

public class TextKeyedCache<T>  where T : class
{
	private class SharedEntryValue
	{
		public SharedEntryValue(string text, T item)
		{
			Text = text;
			Item = item;
		}
		
		public readonly string Text;
		public readonly T Item;
	}


	private const int LocalSizeBits = 11;
	private const int LocalSize = (1 << LocalSizeBits);
	private const int LocalSizeMask = LocalSize - 1;

	private const int SharedSizeBits = 16;
	private const int SharedSize = (1 << SharedSizeBits);
	private const int SharedSizeMask = SharedSize - 1;

	private const int SharedBucketBits = 4;
	private const int SharedBucketSize = (1 << SharedBucketBits);
	private const int SharedBucketSizeMask = SharedBucketSize - 1;

	private readonly (string Text, int HashCode, T Item)[] m_localTable = new(string Text, int HashCode, T item)[LocalSize];

	private static readonly (int HashCode, SharedEntryValue Entry)[] s_sharedTable = new (int HashCode, SharedEntryValue Entry)[SharedSize];

	private readonly (int HashCode, SharedEntryValue Entry)[] m_sharedTableInstance = s_sharedTable;

	private readonly StringTable m_strings;

	private Random? m_random;

	private readonly ObjectPool<TextKeyedCache<T>>? m_pool;
	private static readonly ObjectPool<TextKeyedCache<T>> s_staticPool = CreatePool();

	public TextKeyedCache() : this(null) {}

	public TextKeyedCache(ObjectPool<TextKeyedCache<T>>? pool)
	{
		m_pool = pool;
		m_strings = new StringTable();
	}

	private static ObjectPool<TextKeyedCache<T>> CreatePool()
	{
		return new ObjectPool<TextKeyedCache<T>>(pool => 
				new TextKeyedCache<T>(pool), Environment.ProcessorCount * 4);
	}

	public static TextKeyedCache<T> GetInstance()
	{
		return s_staticPool.Allocate();
	}

	public void Free()
	{
		m_pool?.Free(this);
	}

	public T? FindItem(char[] chars, int start, int len, int hashCode)
	{
		ref var localSlot = ref m_localTable[LoadIdxFromHash(hashCode)];
		var text = localSlot.Text;

		if (!string.IsNullOrWhiteSpace(text) && localSlot.HashCode == hashCode)
		{
			if (StringTable.TextEquals(text, chars.AsSpan(start, len)))
			{
				return localSlot.Item;
			}
		}

		var entry = FindSharedEntry(chars, start, len, hashCode);
		if (entry is not null)
		{
			localSlot.HashCode = hashCode;
			localSlot.Text = entry.Text;

			var tk = entry.Item;
			localSlot.Item = tk;

			return tk;
		}

		return null;
	}

	private SharedEntryValue? FindSharedEntry(char[] chars, int start, int len, int hashCode)
	{
		var arr = m_sharedTableInstance;
		var idx = SharedIdxFromHash(hashCode);

		SharedEntryValue? entry = null;
		int hash;

		for (var i = 1; i < SharedBucketSize + 1; i++)
		{
			(hash, entry) = arr[idx];

			if (entry is not null)
			{
				if (hash == hashCode && StringTable.TextEquals(entry.Text, chars.AsSpan(start, len)))
				{
					break;
				}

				entry = null;
			}
			else
			{
				break;
			}

			idx = (idx + i) & SharedSizeMask;
		}

		return entry;
	}

	public void AddItem(char[] chars, int start, int len, int hashCode, T item)
	{
		var text = m_strings.Add(chars, start, len);
		var entry = new SharedEntryValue(text, item);
		AddSharedEntry(hashCode, entry);

		ref var localSlot = ref m_localTable[LocalIdxFromHash(hashCode)];
		localSlot.HashCode = hashCode;
		localSlot.Text = text;
		localSlot.Item = item;
	}

	private void AddSharedEntry(int hashCode, SharedEntryValue entry)
	{
		var arr = m_sharedTableInstance;
		var idx = SharedIdxFromHash(hashCode);

		var curIdx = idx;
		for (var i = 1; i < SharedBucketSize + 1; i++)
		{
			if (arr[curIdx].Entry is null)
			{
				idx = curIdx;
				goto foundIdx;
			}

			curIdx = (curIdx + i) & SharedSizeMask;
		}

		var i1 = NextRandom() & SharedBucketSizeMask;
		idx = (idx + ((i1 * i1 + i1) / 2)) & SharedSizeMask;
		
		foundIdx:
		arr[idx].HashCode = hashCode;
		Volatile.Write(ref arr[idx].Entry, entry);
	}

	private static int LocalIdxFromHash(int hash)
	{
		return hash & LocalSizeMask;
	}
	
	private static int LoadIdxFromHash(int hash)
	{
		return hash & LocalSizeMask;
	}

	private static int SharedIdxFromHash(int hash)
	{
		return (hash ^ (hash >> LocalSizeBits)) & SharedSizeMask;
	}

	private int NextRandom()
	{
		var r = m_random;
		if (r is not null)
		{
			return r.Next();
		}

		r = new Random();
		m_random = r;
		return r.Next();
	}
}