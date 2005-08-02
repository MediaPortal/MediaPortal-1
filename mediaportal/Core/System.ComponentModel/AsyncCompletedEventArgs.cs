using System;

namespace System.ComponentModel
{
	public class AsyncCompletedEventArgs : EventArgs
	{   
		#region Constructors

		public AsyncCompletedEventArgs(bool isCancelled, Exception exception, object state)
		{
			_isCancelled = isCancelled;
			_exception = exception;
			_state = state;
		}

		#endregion Constructors

		#region Properties

		public bool Cancelled
		{
			get { return _isCancelled; }
		}

		public Exception Exception
		{
			get { return _exception; }
		}

		public object State
		{
			get { return _state; }
		}

		#endregion Properties

		#region Fields

		readonly Exception			_exception;
		readonly bool				_isCancelled;
		readonly object				_state;

		#endregion Fields
	}
}
