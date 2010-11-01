using System;
using System.Collections;

namespace Tst
{
	/// <summary>
	/// A synchronized (thread-safe) wrapper for the <see cref="TstDictionary"/>.
	/// </summary>
	public class TstSynchronizedDictionary : TstDictionary
	{
		private TstDictionary wrapped;

		/// <summary>
		/// Creates a synchronized wrapper around the 
		/// <see cref="TstDictionary"/> <paramref name="dic"/>.
		/// </summary>
		/// <param name="dic">Dictionary to synchronize</param>
		public TstSynchronizedDictionary(TstDictionary dic)
			: base()
		{
			this.wrapped = dic;
		}

		/// <summary>
		/// Wrapped dictionary
		/// </summary>
		private TstDictionary Wrapped
		{
			get
			{
				return Wrapped;
			}
		}

		/// <summary>
		/// Synchronized Add method.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public override void Add(String key, Object value)
		{
			lock(Wrapped.SyncRoot)
			{
				Wrapped.Add (key, value);
			}
		}

		/// <summary>
		/// Synchronized clear method
		/// </summary>
		public override void Clear()
		{
			lock(Wrapped.SyncRoot)
			{
				Wrapped.Clear ();
			}
		}

		/// <summary>
		/// Synchronized remove method
		/// </summary>
		/// <param name="key"></param>
		public override void Remove(String key)
		{
			lock(Wrapped.SyncRoot)
			{
				Wrapped.Remove (key);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override Object Clone()
		{
			return Wrapped.Clone();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public override bool ContainsKey(String key)
		{
			return Wrapped.ContainsKey(key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public override void CopyTo(Array array, int arrayIndex)
		{
			Wrapped.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		public override int Count
		{
			get
			{
				return Wrapped.Count;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public override TstDictionaryEntry Find(String key)
		{
			return Wrapped.Find (key);
		}

		/// <summary>
		/// Gets a non-synchronized enumerator
		/// </summary>
		/// <remarks>
		/// Enumerating through a collection is intrinsically not a 
		/// thread-safe procedure. 
		/// Even when a collection is synchronized, other threads could still 
		/// modify the collection, which causes the enumerator to throw an 
		/// exception. 
		/// To guarantee thread safety during enumeration, you can either lock 
		/// the collection during the entire enumeration or catch the 
		/// exceptions resulting from changes made by other threads.
		/// </remarks>
		/// <returns>Enumerator</returns>
		public override TstDictionaryEnumerator GetEnumerator()
		{
			return Wrapped.GetEnumerator();
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsFixedSize
		{
			get
			{
				return Wrapped.IsFixedSize;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsSynchronized
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsReadOnly
		{
			get
			{
				return Wrapped.IsReadOnly;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override ICollection Keys
		{
			get
			{
				return Wrapped.Keys;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override ICollection Values
		{
			get
			{
				return Wrapped.Values;
			}
		}

		/// <summary>
		/// Synchronized indexer
		/// </summary>
		public override Object this[String key]
		{
			get
			{
				return Wrapped[key];
			}
			set
			{
				lock(Wrapped.SyncRoot)
				{
					Wrapped[key] = value;
				}
			}
		}

		/// <summary>
		/// SyncRoot object
		/// </summary>
		public override Object SyncRoot
		{
			get
			{
				return Wrapped.SyncRoot;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public override ICollection PartialMatch(string key)
		{
			return Wrapped.PartialMatch (key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="wildChar"></param>
		/// <returns></returns>
		public override ICollection PartialMatch(string key, char wildChar)
		{
			return Wrapped.PartialMatch (key, wildChar);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public override ICollection NearNeighbors(string key, int distance)
		{
			return Wrapped.NearNeighbors(key, distance);
		}
	}
}
