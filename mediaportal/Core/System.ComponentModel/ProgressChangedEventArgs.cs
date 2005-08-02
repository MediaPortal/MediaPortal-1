using System;

namespace System.ComponentModel
{
	public class ProgressChangedEventArgs : EventArgs
	{
		#region Constructors

		public ProgressChangedEventArgs(int progressPercentage) : this(progressPercentage, null)
		{
		}

		public ProgressChangedEventArgs(int progressPercentage, object state)
		{
			_progressPercentage = progressPercentage;
			_state = state;
		}

		#endregion Constructors

		#region Properties

		public int ProgressPercentage
		{
			get { return _progressPercentage; }
		}

		public object State
		{
			get { return _state; }
		}

		#endregion Properties

		#region Fields

		readonly int				_progressPercentage;
		object						_state;

		#endregion Fields
	}
}
