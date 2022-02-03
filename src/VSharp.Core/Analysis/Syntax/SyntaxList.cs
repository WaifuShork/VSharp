namespace VSharp.Core.Analysis.Syntax;

[PublicAPI]
public sealed class SyntaxList<T> : SyntaxNode, IEnumerable<T> where T : SyntaxNode
{
	private readonly List<T> m_elements;

	public SyntaxList(bool isReadOnly = false)
	{
		IsReadOnly = isReadOnly;
		m_elements = new();
	}

	public SyntaxList(int size, bool isReadOnly = false)
	{
		IsReadOnly = isReadOnly;
		m_elements = new(size);
	}

	public SyntaxList(IEnumerable<T> items, bool isReadOnly = false)
	{
		IsReadOnly = isReadOnly;
		m_elements = new(items);
	}

	public int Length => m_elements.Count;
	public bool IsReadOnly { get; private set; }

	public int Capacity
	{
		get => Length;
		set => m_elements.Capacity = value;
	}

	public T this[int index] => m_elements[index];
	public SyntaxList<T> this[Range range] => new(m_elements.GetRange(range.Start.Value, range.End.Value));

	public override SyntaxKind Kind => SyntaxKind.SyntaxList;
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		return m_elements;
	}

	public bool Contains(T item)
	{
		return m_elements.Contains(item);
	}

	public T Last()
	{
		Throw<ArgumentOutOfRangeException>.If(Length <= 0);
		return m_elements[^1];
	}

	public T First()
	{
		Throw<ArgumentOutOfRangeException>.If(Length <= 0);
		return m_elements[0];
	}
	
	public SyntaxList<T> Insert(int index, T item)
	{
		m_elements.Insert(index, item);
		return this;
	}

	public SyntaxList<T> MakeReadOnly()
	{
		IsReadOnly = true;
		return this;
	}

	public IReadOnlyList<T> ToReadOnly()
	{
		return m_elements;
	}

	public async Task<SyntaxList<T>> ForEachAsync(Func<T, CancellationToken, ValueTask> body)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, $"cannot modify a made readonly SyntaxList<{typeof(T)}>");
		await Parallel.ForEachAsync(m_elements, body);
		return this;
	}

	public SyntaxList<T> ForEach(Action<T> body)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, $"cannot modify a made readonly SyntaxList<{typeof(T)}>");
		m_elements.ForEach(body);
		return this;
	}
	
	public SyntaxList<T> Add(T item)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, $"cannot modify a made readonly SyntaxList<{typeof(T)}>");
		m_elements.Add(item);
		return this;
	}

	public SyntaxList<T> AddRange(IEnumerable<T> items)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, $"cannot modify a made readonly SyntaxList<{typeof(T)}>");
		m_elements.AddRange(items);
		return this;
	}

	public SyntaxList<T> Remove(T item)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		m_elements.Remove(item);
		return this;
	}

	public SyntaxList<T> RemoveAt(int index)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		m_elements.RemoveAt(index);
		return this;
	}

	public SyntaxList<T> RemoveRange(IEnumerable<T> items)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		foreach (var item in m_elements.SelectMany(_ => items, (syntax, item) => new { syntax, item })
		                               .Where(t => t.syntax == t.item)
		                               .Select(t => t.item))
		{
			m_elements.Remove(item);
		}

		return this;
	}

	public SyntaxList<T> RemoveRange(int index, int count)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		m_elements.RemoveRange(index, count);
		return this;
	}

	public SyntaxList<T> RemoveRange(int index)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		m_elements.RemoveRange(index, Length);
		return this;
	}

	public SyntaxList<T> Replace(T item, T with)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		return new(m_elements.Select(syntax => syntax == item ? with : syntax));
	}

	public SyntaxList<T> Reverse()
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		m_elements.Reverse();
		return this;
	}

	public void Clear(int index)
	{
		Throw<InvalidOperationException>.If(IsReadOnly, "cannot modify a made readonly list");
		m_elements.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_elements.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}