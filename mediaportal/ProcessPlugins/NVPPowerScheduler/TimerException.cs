using System;

namespace MediaPortal.PowerScheduler
{
	/// <summary>
	/// Used by the <see cref="MediaPortal.TV.Recording.WaitableTimer"/>
	/// to report errors.
	/// </summary>
	public class TimerException : Exception
	{
		/// <summary>
		/// Create a new instance of this exception.
		/// </summary>
		/// <param name="sReason">Some text to describe the error condition.</param>
		public TimerException(string sReason) : base(sReason)
		{
		}
	}
}
