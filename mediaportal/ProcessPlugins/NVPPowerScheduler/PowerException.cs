using System;

namespace MediaPortal.PowerScheduler
{
	/// <summary>
	/// Reports error from calls to the Windows Power Management API.
	/// </summary>
	public class PowerException : Exception
	{
		/// <summary>
		/// Create a new instance of the exception.
		/// </summary>
		/// <param name="sReason">Describes the management call which failed.</param>
		public PowerException(string sReason) : base(sReason)
		{
		}
	}
}
