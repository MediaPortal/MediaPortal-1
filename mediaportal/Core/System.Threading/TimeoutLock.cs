#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Diagnostics;

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

      if (Monitor.TryEnter(o, timeout) == false)
      {
#if DEBUG
        System.GC.SuppressFinalize(tl.leakDetector);
#endif
        Debug.Assert(false);
        throw new LockTimeoutException();
      }

      return tl;
    }

    public static TimeoutLock Lock(object o, TimeSpan timeout)
    {
      return Lock(o, timeout.Milliseconds);
    }

    private TimeoutLock(object o)
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
    public LockTimeoutException() : base("Timeout waiting for lock") {}
  }
}