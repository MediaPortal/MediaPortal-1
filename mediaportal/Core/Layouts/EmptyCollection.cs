using System;

namespace System.Collections
{
	public sealed class EmptyCollection : ICollection
	{
		#region Constructors

		private EmptyCollection()
		{
		}

		#endregion Constructors

		#region Methods

		public static EmptyCollection Instance
		{
			get { return _instance == null ? _instance = new EmptyCollection() : _instance; }
		}

		public void CopyTo(Array array, int index)
		{
		}

		public IEnumerator GetEnumerator()
		{
			return EmptyEnumerator.Instance;
		}

		#endregion Methods

		#region Properties

		public int Count
		{
			get { return 0; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return null; }
		}

		#endregion Properties

		#region Fields

		static EmptyCollection		_instance;

		#endregion Fields
	}
}
