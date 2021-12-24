using System.Diagnostics;
using System.Text;

namespace VSharp.Core.Utilities.Caching;

public class StringTable
{
	private struct Entry
	{
		public int HashCode;
		public string Text;
	}
	
	// Size of local cache.
	private const int LocalSizeBits = 11;
	private const int LocalSize = (1 << LocalSizeBits);
	private const int LocalSizeMask = LocalSize - 1;

	// max size of shared cache.
	private const int SharedSizeBits = 16;
	private const int SharedSize = (1 << SharedSizeBits);
	private const int SharedSizeMask = SharedSize - 1;

	// size of bucket in shared cache. (local cache has bucket size 1).
	private const int SharedBucketBits = 4;
	private const int SharedBucketSize = (1 << SharedBucketBits);
	private const int SharedBucketSizeMask = SharedBucketSize - 1;

	private readonly Entry[] m_localTable = new Entry[LocalSize];
	private static readonly Entry[] s_sharedTable = new Entry[SharedSize];

	private int m_localRandom = Environment.TickCount;
	private static int s_sharedRandom = Environment.TickCount;

	private readonly ObjectPool<StringTable>? m_pool;
	private static readonly ObjectPool<StringTable> s_staticPool = CreatePool();

    public StringTable() : this(null) {}
    
	private StringTable(ObjectPool<StringTable>? pool)
	{
		m_pool = pool;
	}

	private static ObjectPool<StringTable> CreatePool()
	{
		return new ObjectPool<StringTable>(pool =>
			new StringTable(pool), Environment.ProcessorCount * 2);
	}

	public static StringTable GetInstance()
	{
		return s_staticPool.Allocate();
	}

	public void Free()
	{
		m_pool?.Free(this);
	}
	
	internal string Add(char[] chars, int start, int len)
        {
            var span = chars.AsSpan(start, len);
            var hashCode = Hash.GetFNVHashCode(chars, start, len);

            // capture array to avoid extra range checks
            var arr = m_localTable;
            var idx = LocalIdxFromHash(hashCode);

            var text = arr[idx].Text;

            if (text != null && arr[idx].HashCode == hashCode)
            {
                var result = arr[idx].Text;
                if (StringTable.TextEquals(result, span))
                {
                    return result;
                }
            }

            string? shared = FindSharedEntry(chars, start, len, hashCode);
            if (shared != null)
            {
                // PERF: the following code does element-wise assignment of a struct
                //       because current JIT produces better code compared to
                //       arr[idx] = new Entry(...)
                arr[idx].HashCode = hashCode;
                arr[idx].Text = shared;

                return shared;
            }

            return AddItem(chars, start, len, hashCode);
        }

        internal string Add(string chars, int start, int len)
        {
            var hashCode = Hash.GetFNVHashCode(chars, start, len);

            // capture array to avoid extra range checks
            var arr = m_localTable;
            var idx = LocalIdxFromHash(hashCode);

            var text = arr[idx].Text;

            if (text != null && arr[idx].HashCode == hashCode)
            {
                var result = arr[idx].Text;
                if (StringTable.TextEquals(result, chars, start, len))
                {
                    return result;
                }
            }

            string? shared = FindSharedEntry(chars, start, len, hashCode);
            if (shared != null)
            {
                // PERF: the following code does element-wise assignment of a struct
                //       because current JIT produces better code compared to
                //       arr[idx] = new Entry(...)
                arr[idx].HashCode = hashCode;
                arr[idx].Text = shared;

                return shared;
            }

            return AddItem(chars, start, len, hashCode);
        }

        internal string Add(char chars)
        {
            var hashCode = Hash.GetFNVHashCode(chars);

            // capture array to avoid extra range checks
            var arr = m_localTable;
            var idx = LocalIdxFromHash(hashCode);

            var text = arr[idx].Text;

            if (text != null)
            {
                var result = arr[idx].Text;
                if (text.Length == 1 && text[0] == chars)
                {
                    return result;
                }
            }

            string? shared = FindSharedEntry(chars, hashCode);
            if (shared != null)
            {
                // PERF: the following code does element-wise assignment of a struct
                //       because current JIT produces better code compared to
                //       arr[idx] = new Entry(...)
                arr[idx].HashCode = hashCode;
                arr[idx].Text = shared;

                return shared;
            }

            return AddItem(chars, hashCode);
        }

        internal string Add(StringBuilder chars)
        {
            var hashCode = Hash.GetFNVHashCode(chars);

            // capture array to avoid extra range checks
            var arr = m_localTable;
            var idx = LocalIdxFromHash(hashCode);

            var text = arr[idx].Text;

            if (text != null && arr[idx].HashCode == hashCode)
            {
                var result = arr[idx].Text;
                if (StringTable.TextEquals(result, chars))
                {
                    return result;
                }
            }

            string? shared = FindSharedEntry(chars, hashCode);
            if (shared != null)
            {
                // PERF: the following code does element-wise assignment of a struct
                //       because current JIT produces better code compared to
                //       arr[idx] = new Entry(...)
                arr[idx].HashCode = hashCode;
                arr[idx].Text = shared;

                return shared;
            }

            return AddItem(chars, hashCode);
        }

        internal string Add(string chars)
        {
            var hashCode = Hash.GetFNVHashCode(chars);

            // capture array to avoid extra range checks
            var arr = m_localTable;
            var idx = LocalIdxFromHash(hashCode);

            var text = arr[idx].Text;

            if (text != null && arr[idx].HashCode == hashCode)
            {
                var result = arr[idx].Text;
                if (result == chars)
                {
                    return result;
                }
            }

            string? shared = FindSharedEntry(chars, hashCode);
            if (shared != null)
            {
                // PERF: the following code does element-wise assignment of a struct
                //       because current JIT produces better code compared to
                //       arr[idx] = new Entry(...)
                arr[idx].HashCode = hashCode;
                arr[idx].Text = shared;

                return shared;
            }

            AddCore(chars, hashCode);
            return chars;
        }


        private static string? FindSharedEntry(char[] chars, int start, int len, int hashCode)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            string? e = null;
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                e = arr[idx].Text;
                int hash = arr[idx].HashCode;

                if (e != null)
                {
                    if (hash == hashCode && TextEquals(e, chars.AsSpan(start, len)))
                    {
                        break;
                    }

                    // this is not e we are looking for
                    e = null;
                }
                else
                {
                    // once we see unfilled entry, the rest of the bucket will be empty
                    break;
                }

                idx = (idx + i) & SharedSizeMask;
            }

            return e;
        }

        private static string? FindSharedEntry(string chars, int start, int len, int hashCode)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            string? e = null;
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                e = arr[idx].Text;
                int hash = arr[idx].HashCode;

                if (e != null)
                {
                    if (hash == hashCode && TextEquals(e, chars, start, len))
                    {
                        break;
                    }

                    // this is not e we are looking for
                    e = null;
                }
                else
                {
                    // once we see unfilled entry, the rest of the bucket will be empty
                    break;
                }

                idx = (idx + i) & SharedSizeMask;
            }

            return e;
        }

        private static string? FindSharedEntryASCII(int hashCode, ReadOnlySpan<byte> asciiChars)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            string? e = null;
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                e = arr[idx].Text;
                int hash = arr[idx].HashCode;

                if (e != null)
                {
                    if (hash == hashCode && TextEqualsASCII(e, asciiChars))
                    {
                        break;
                    }

                    // this is not e we are looking for
                    e = null;
                }
                else
                {
                    // once we see unfilled entry, the rest of the bucket will be empty
                    break;
                }

                idx = (idx + i) & SharedSizeMask;
            }

            return e;
        }

        private static string? FindSharedEntry(char chars, int hashCode)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            string? e = null;
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                e = arr[idx].Text;

                if (e != null)
                {
                    if (e.Length == 1 && e[0] == chars)
                    {
                        break;
                    }

                    // this is not e we are looking for
                    e = null;
                }
                else
                {
                    // once we see unfilled entry, the rest of the bucket will be empty
                    break;
                }

                idx = (idx + i) & SharedSizeMask;
            }

            return e;
        }

        private static string? FindSharedEntry(StringBuilder chars, int hashCode)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            string? e = null;
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                e = arr[idx].Text;
                int hash = arr[idx].HashCode;

                if (e != null)
                {
                    if (hash == hashCode && TextEquals(e, chars))
                    {
                        break;
                    }

                    // this is not e we are looking for
                    e = null;
                }
                else
                {
                    // once we see unfilled entry, the rest of the bucket will be empty
                    break;
                }

                idx = (idx + i) & SharedSizeMask;
            }

            return e;
        }

        private static string? FindSharedEntry(string chars, int hashCode)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            string? e = null;
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                e = arr[idx].Text;
                int hash = arr[idx].HashCode;

                if (e != null)
                {
                    if (hash == hashCode && e == chars)
                    {
                        break;
                    }

                    // this is not e we are looking for
                    e = null;
                }
                else
                {
                    // once we see unfilled entry, the rest of the bucket will be empty
                    break;
                }

                idx = (idx + i) & SharedSizeMask;
            }

            return e;
        }


        private string AddItem(char[] chars, int start, int len, int hashCode)
        {
            var text = new String(chars, start, len);
            AddCore(text, hashCode);
            return text;
        }

        private string AddItem(string chars, int start, int len, int hashCode)
        {
            var text = chars.Substring(start, len);
            AddCore(text, hashCode);
            return text;
        }

        private string AddItem(char chars, int hashCode)
        {
            var text = new String(chars, 1);
            AddCore(text, hashCode);
            return text;
        }

        private string AddItem(StringBuilder chars, int hashCode)
        {
            var text = chars.ToString();
            AddCore(text, hashCode);
            return text;
        }


        private void AddCore(string chars, int hashCode)
        {
            // add to the shared table first (in case someone looks for same item)
            AddSharedEntry(hashCode, chars);

            // add to the local table too
            var arr = m_localTable;
            var idx = LocalIdxFromHash(hashCode);
            arr[idx].HashCode = hashCode;
            arr[idx].Text = chars;
        }

        private void AddSharedEntry(int hashCode, string text)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            // try finding an empty spot in the bucket
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            int curIdx = idx;
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                if (arr[curIdx].Text == null)
                {
                    idx = curIdx;
                    goto foundIdx;
                }

                curIdx = (curIdx + i) & SharedSizeMask;
            }

            // or pick a random victim within the bucket range
            // and replace with new entry
            var i1 = LocalNextRandom() & SharedBucketSizeMask;
            idx = (idx + ((i1 * i1 + i1) / 2)) & SharedSizeMask;

foundIdx:
            arr[idx].HashCode = hashCode;
            Volatile.Write(ref arr[idx].Text, text);
        }

        internal static string AddShared(StringBuilder chars)
        {
            var hashCode = Hash.GetFNVHashCode(chars);

            string? shared = FindSharedEntry(chars, hashCode);
            if (shared != null)
            {
                return shared;
            }

            return AddSharedSlow(hashCode, chars);
        }

        private static string AddSharedSlow(int hashCode, StringBuilder builder)
        {
            string text = builder.ToString();
            AddSharedSlow(hashCode, text);
            return text;
        }

        internal static string AddSharedUTF8(ReadOnlySpan<byte> bytes)
        {
            int hashCode = Hash.GetFNVHashCode(bytes, out bool isAscii);

            if (isAscii)
            {
                string? shared = FindSharedEntryASCII(hashCode, bytes);
                if (shared != null)
                {
                    return shared;
                }
            }

            return AddSharedSlow(hashCode, bytes, isAscii);
        }

        private static string AddSharedSlow(int hashCode, ReadOnlySpan<byte> utf8Bytes, bool isAscii)
        {
            string text;

            unsafe
            {
                fixed (byte* bytes = &utf8Bytes.GetPinnableReference())
                {
                    text = Encoding.UTF8.GetString(bytes, utf8Bytes.Length);
                }
            }

            // Don't add non-ascii strings to table. The hashCode we have here is not correct and we won't find them again.
            // Non-ascii in UTF8-encoded parts of metadata (the only use of this at the moment) is assumed to be rare in 
            // practice. If that turns out to be wrong, we could decode to pooled memory and rehash here.
            if (isAscii)
            {
                AddSharedSlow(hashCode, text);
            }

            return text;
        }

        private static void AddSharedSlow(int hashCode, string text)
        {
            var arr = s_sharedTable;
            int idx = SharedIdxFromHash(hashCode);

            // try finding an empty spot in the bucket
            // we use quadratic probing here
            // bucket positions are (n^2 + n)/2 relative to the masked hashcode
            int curIdx = idx;
            for (int i = 1; i < SharedBucketSize + 1; i++)
            {
                if (arr[curIdx].Text == null)
                {
                    idx = curIdx;
                    goto foundIdx;
                }

                curIdx = (curIdx + i) & SharedSizeMask;
            }

            // or pick a random victim within the bucket range
            // and replace with new entry
            var i1 = SharedNextRandom() & SharedBucketSizeMask;
            idx = (idx + ((i1 * i1 + i1) / 2)) & SharedSizeMask;

foundIdx:
            arr[idx].HashCode = hashCode;
            Volatile.Write(ref arr[idx].Text, text);
        }

        private static int LocalIdxFromHash(int hash)
        {
            return hash & LocalSizeMask;
        }

        private static int SharedIdxFromHash(int hash)
        {
            // we can afford to mix some more hash bits here
            return (hash ^ (hash >> LocalSizeBits)) & SharedSizeMask;
        }

        private int LocalNextRandom()
        {
            return m_localRandom++;
        }

        private static int SharedNextRandom()
        {
            return Interlocked.Increment(ref StringTable.s_sharedRandom);
        }

        internal static bool TextEquals(string array, string text, int start, int length)
        {
            if (array.Length != length)
            {
                return false;
            }

            // use array.Length to eliminate the range check
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] != text[start + i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool TextEquals(string array, StringBuilder text)
        {
            if (array.Length != text.Length)
            {
                return false;
            }

            // interestingly, stringbuilder holds the list of chunks by the tail
            // so accessing positions at the beginning may cost more than those at the end.
            for (var i = array.Length - 1; i >= 0; i--)
            {
                if (array[i] != text[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool TextEqualsASCII(string text, ReadOnlySpan<byte> ascii)
        {
#if DEBUG
            for (var i = 0; i < ascii.Length; i++)
            {
                Debug.Assert((ascii[i] & 0x80) == 0, $"The {nameof(ascii)} input to this method must be valid ASCII.");
            }
#endif

            if (ascii.Length != text.Length)
            {
                return false;
            }

            for (var i = 0; i < ascii.Length; i++)
            {
                if (ascii[i] != text[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool TextEquals(string array, ReadOnlySpan<char> text)
        {
            return text.Equals(array.AsSpan(), StringComparison.Ordinal);
        }
}