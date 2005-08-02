using System;

namespace System.ComponentModel
{
	public class RunWorkerCompletedEventArgs : AsyncCompletedEventArgs
	{          
		#region Constructors

		public RunWorkerCompletedEventArgs(object result, Exception exception, bool isCancelled) : base(isCancelled, exception, null)
		{                
			_result = result;
		}

		#endregion Constructors

		#region Properties

		public object Result
		{
			get
			{ 
				if(base.Exception != null)
					throw base.Exception;

				if(base.Cancelled)
					throw new InvalidOperationException("Background operation was cancelled.");

				return _result;
			}
		}

		#endregion Properties

		#region Fields

		readonly object				_result;

		#endregion Fields
	}
}
