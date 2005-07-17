using System;

namespace System.Collections
{
	public sealed class EmptyEnumerator : IEnumerator
	{
		#region Constructors

		private EmptyEnumerator()
		{
		}
		
		#endregion Constructors

		#region Methods

		public void Reset()
		{
		}

		public bool MoveNext()
		{
			return false;
		}

		#endregion Methods

		#region Properties

		public object Current
		{
			get { return null; }
		}

		public static EmptyEnumerator Instance
		{
			get { return _instance == null ? _instance = new EmptyEnumerator() : _instance; }
		}

		#endregion Properties

		#region Fields

		static EmptyEnumerator		_instance;		

		#endregion Fields
	}
}
