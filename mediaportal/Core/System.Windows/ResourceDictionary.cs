using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows
{
	public class ResourceDictionary : IDictionary, ICollection, IEnumerable, INameScope, ISupportInitialize
	{
		#region IDictionary Members

		public bool IsReadOnly
		{
			get { return _resources.IsReadOnly; }
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return _resources.GetEnumerator();
		}

		public object this[object key]
		{
			get { return _resources[key]; }
			set { _resources[key] = value; }
		}

		public void Remove(object key)
		{
			_resources.Remove(key);
		}

		public bool Contains(object key)
		{
			return _resources.Contains(key);
		}

		public void Clear()
		{
			_resources.Clear();
		}

		public ICollection Values
		{
			get { return _resources.Values; }
		}

		public void Add(object key, object value)
		{
			_resources[key] = value;
		}

		public ICollection Keys
		{
			get { return _resources.Keys; }
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized
		{
			get { return _resources.IsSynchronized; }
		}

		public int Count
		{
			get { return _resources.Count; }
		}

		public void CopyTo(Array array, int index)
		{
			_resources.CopyTo(array, index);
		}

		public object SyncRoot
		{
			get { return _resources.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _resources.GetEnumerator();
		}

		#endregion

		#region INameScope Members

		public object FindName(string name)
		{
			return _resources[name];
		}

		#endregion

		#region ISupportInitialize Members

		public void BeginInit()
		{
			_beginInitCount++;
		}

		public void EndInit()
		{
			_beginInitCount--;
		}

		#endregion

		#region Methods

		public void RegisterName(string name, object ownerContext)
		{
			_resources[name] = ownerContext;
		}

		public void UnregisterName(string name)
		{
			_resources.Remove(name);
		}

		#endregion Methods

		#region Fields

		int							_beginInitCount;
		Hashtable					_resources = new Hashtable();

		#endregion Fields
	}
}
