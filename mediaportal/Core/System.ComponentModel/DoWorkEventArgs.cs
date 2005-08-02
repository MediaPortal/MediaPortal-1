using System;

namespace System.ComponentModel
{
	public class DoWorkEventArgs : CancelEventArgs
	{
		#region Constructors

		public DoWorkEventArgs(object argument)
		{
			_argument = argument;
		}

		#endregion Constructors

		#region Properties

		public object Argument
		{
			get { return _argument; }
		}

		public object Result
		{
			get { return _result; }
			set { _result = value; }
		}

		#endregion Properties

		#region Fields

		readonly object				_argument;
		object						_result;

		#endregion Fields
	}
}
