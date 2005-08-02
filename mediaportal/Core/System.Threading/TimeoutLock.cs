using System;

namespace System.Threading
{
	public class TimeoutLock : IDisposable
	{
		public static TimeoutLock Lock(object o)
		{
			return Lock(o, TimeSpan.FromSeconds(10));
		}

		public static TimeoutLock Lock(object o, int timeout)
		{
			TimeoutLock tl = new TimeoutLock(o);

			if(Monitor.TryEnter(o, timeout) == false)
			{
#if DEBUG
				System.GC.SuppressFinalize(tl.leakDetector);
#endif
				System.Diagnostics.Debug.Assert(false);
				throw new LockTimeoutException ();
			}

			return tl;
		}

		public static TimeoutLock Lock(object o, TimeSpan timeout)
		{
			return Lock(o, timeout.Milliseconds);
		}

		private TimeoutLock (object o)
		{
			target = o;
#if DEBUG
			leakDetector = new Sentinel();
#endif
		}

		private object target;

		public void Dispose()
		{
			Monitor.Exit(target);

			// It's a bad error if someone forgets to call Dispose,
			// so in Debug builds, we put a finalizer in to detect
			// the error. If Dispose is called, we suppress the
			// finalizer.
#if DEBUG
			GC.SuppressFinalize(leakDetector);
#endif
		}

#if DEBUG
		// (In Debug mode, we make it a class so that we can add a finalizer
		// in order to detect when the object is not freed.)
		private class Sentinel
		{
			~Sentinel()
			{
				// If this finalizer runs, someone somewhere failed to
				// call Dispose, which means we've failed to leave
				// a monitor!
				System.Diagnostics.Debug.Fail("Undisposed lock");
			}
		}

		private Sentinel leakDetector;
#endif
	}

	public class LockTimeoutException : ApplicationException
	{
		public LockTimeoutException () : base("Timeout waiting for lock")
		{
		}
	}
}
