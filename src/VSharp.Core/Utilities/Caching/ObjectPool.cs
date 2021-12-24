#define TRACE_LEAKS
#define DETECT_LEAKS
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace VSharp.Core.Utilities.Caching;

public delegate TReturn Func<TArg, TReturn>(TArg arg);

public class ObjectPool<T> where T : class
{
	private struct Element
	{
		internal T? Value;
	}

	public delegate T Factory();

	private T? m_firstItem;
	private readonly Element[] m_items;

	private readonly Factory m_factory;

	private static readonly ConditionalWeakTable<T, LeakTracker> m_leakTrackers = new();

	private class LeakTracker : IDisposable
	{
		private volatile bool m_disposed;

		public volatile object? Trace = null;

		public void Dispose()
		{
			m_disposed = true;
			GC.SuppressFinalize(this);
		}

		private string? GetTrace()
		{
#if TRACE_LEAKS
			return Trace == null ? "" : Trace.ToString();
#endif
			return "leak tracing information is disabled, define TRACE_LEAKS on ObjectPool to get more info\n";
		}

		~LeakTracker()
		{
			if (!m_disposed && !Environment.HasShutdownStarted)
			{
				var trace = GetTrace();
				Debug.WriteLine("TRACE_OBJECTPOOL_LEAKS_BEGIN\n" +
				                $"Pool detected potential leaking of {typeof(T)}\n" +
				                $"Location of the leak: {GetTrace()} TRACE_OBJECTPOOL_LEAKS_END");
			}
		}
	}

	public ObjectPool(Factory factory)
		: this(factory, Environment.ProcessorCount * 2)
	{
		
	}

	public ObjectPool(Factory factory, int size)
	{
		Debug.Assert(size >= 1);
		m_factory = factory;
		m_items = new Element[size - 1];
	}

	public ObjectPool(Func<ObjectPool<T>, T> factory, int size)
	{
		Debug.Assert(size >= 1);
		m_factory = () => factory(this);
		m_items = new Element[size - 1];
	}

	private T CreateInstance()
	{
		var instance = m_factory();
		return instance;
	}

	public T Allocate()
	{
		var instance = m_firstItem;
		if (instance is null || instance != Interlocked.CompareExchange(ref m_firstItem, null, instance))
		{
			instance = AllocateSlow();
		}
	
#if DETECT_LEAKS
		var tracker = new LeakTracker();
		m_leakTrackers.Add(instance, tracker);
#if TRACE_LEAKS
		var frame = CaptureStackTrace();
		tracker.Trace = frame;
#endif
#endif

		return instance;
	}

	private T AllocateSlow()
	{
		var items = m_items;
		for (var i = 0; i < items.Length; i++)
		{
			var instance = items[i].Value;
			if (instance is not null)
			{
				if (instance == Interlocked.CompareExchange(ref items[i].Value, null, instance))
				{
					return instance;
				}
			}
		}

		return CreateInstance();
	}

	public void Free(T obj)
	{
		Validate(obj);
		ForgetTrackedObject(obj);

		if (m_firstItem is null)
		{
			m_firstItem = obj;
		}
		else
		{
			FreeSlow(obj);
		}
	}

	private void FreeSlow(T obj)
	{
		var items = m_items;
		for (var i = 0; i < items.Length; i++)
		{
			if (items[i].Value is null)
			{
				items[i].Value = obj;
				break;
			}
		}
	}

	[Conditional("DEBUG")]
	public void ForgetTrackedObject(T old, T? replacement = null)
	{
#if DETECT_LEAKS
		if (m_leakTrackers.TryGetValue(old, out var tracker))
		{
			tracker.Dispose();
			m_leakTrackers.Remove(old);
		}
		else
		{
			var trace = CaptureStackTrace();
			Debug.WriteLine($"TRACE_OBJECTPOOL_LEAKS_BEGIN{Environment.NewLine}" +
			                $"Object of type {typeof(T)} was free, but was not from pool{Environment.NewLine}" +
			                $"Callstack: {Environment.NewLine}{trace} TRACE_OBJECTPOOL_LEAKS_END");
		}

		if (replacement is not null)
		{
			tracker = new LeakTracker();
			m_leakTrackers.Add(replacement, tracker);
		}
#endif
	}

	private static Lazy<Type> m_stackTraceType = new(() => Type.GetType("System.Diagnostics.StackTrace") ?? typeof(StackTrace));

	private static object? CaptureStackTrace()
	{
		return Activator.CreateInstance(m_stackTraceType.Value);
	}

	[Conditional("DEBUG")]
	private void Validate(object? obj)
	{
		Debug.Assert(obj is not null, "freeing null?");
		Debug.Assert(m_firstItem != obj, "freeing twice?");

		var items = m_items;
		for (var i = 0; i < items.Length; i++)
		{
			var value = items[i].Value;
			if (value is null)
			{
				return;
			}
			Debug.Assert(value != obj, "freeing twice?");
		}
	}
}