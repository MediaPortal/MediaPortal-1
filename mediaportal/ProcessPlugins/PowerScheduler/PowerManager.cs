#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace MediaPortal.PowerScheduler
{
  /// <summary>
  /// Manages access to the Power Managerment API included in Windows 2000.
  /// </summary>
  public sealed class PowerManager
  {
    /// <summary>
    /// Possible execution states.
    /// </summary>
    [Flags]
    private enum ExecutionState : uint
    {
      /// <summary>
      /// Some error.
      /// </summary>
      Error = 0,

      /// <summary>
      /// System is required, do not hibernate.
      /// </summary>
      SystemRequired = 1,

      /// <summary>
      /// Display is required, do not hibernate.
      /// </summary>
      DisplayRequired = 2,

      /// <summary>
      /// User is active, do not hibernate.
      /// </summary>
      UserPresent = 4,

      /// <summary>
      /// Use together with the above options to report a
      /// state until explicitly changed.
      /// </summary>
      Continuous = 0x80000000
    }

    /// <summary>
    /// Wrap the Power Managerment function <i>SetThreadExecutionState</i>.
    /// </summary>
    [DllImport("kernel32.dll", EntryPoint = "SetThreadExecutionState")]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    /// <summary>
    /// Handler to be called on some wake up notification.
    /// </summary>
    public delegate void ResumeHandler();

    /// <summary>
    /// Handler to be called when hibernation is enabled or disabled.
    /// </summary>
    public delegate void ChangedHandler(bool bForbid);

    /// <summary>
    /// Clients can register with this event to become notified
    /// when <see cref="OnResume"/> is called.
    /// </summary>
    public static event ResumeHandler OnPowerUp;

    /// <summary>
    /// Reports changes in the hibernation stage.
    /// </summary>
    public static event ChangedHandler OnChanged;

    /// <summary>
    /// Overall synchronization.
    /// </summary>
    private static object m_Sync = new object();

    /// <summary>
    /// Current hibernation lock depth. If zero hibernation is allowed.
    /// <seealso cref="AllowHibernation"/>
    /// <seealso cref="ForbidHibernation"/>
    /// </summary>
    private static int m_HibCount = 0;

    /// <summary>
    /// Make sure that no instance of this class can be created.
    /// </summary>
    private PowerManager()
    {
    }

    public static void SystemBusy()
    {
      // Let the system know we are busy and so not to hibernate
      SetThreadExecutionState(ExecutionState.SystemRequired);
    }


    /// <summary>
    /// Forbid hibernation. The caller must make sure that for each call
    /// of this method a call to <see cref="AllowHibernation"/> follows.
    /// The method is fully synchronized.
    /// </summary>
    /// <remarks>
    /// The <see cref="m_HibCount"/> is incremented. If it has been zero
    /// prior to the call of this method <see cref="SetThreadExecutionState"/>
    /// is used to lock hibernation. On success <see cref="OnChanged"/>
    /// is fired.
    /// </remarks>
    /// <exception cref="PowerException">When <see cref="SetThreadExecutionState"/> fails.</exception>
    public static void ForbidHibernation()
    {
      // Must synchronize
      lock (m_Sync)
      {
        // Block hibernation once
        if (0 != m_HibCount++) return;

        // Call kernel 
        if (ExecutionState.Error == SetThreadExecutionState(ExecutionState.SystemRequired | ExecutionState.Continuous)) throw new PowerException("Could not block Hibernation");

        // Report
        if (null != OnChanged) OnChanged(true);
      }
    }

    /// <summary>
    /// Allow hibernation. This method must be called for each invocation
    /// of <see cref="ForbidHibernation"/>. The caller is responsible for
    /// proper nesting.
    /// The method is fully synchronized.
    /// </summary>
    /// <remarks>
    /// The <see cref="m_HibCount"/> is decremented. If it becomes zero
    /// <see cref="SetThreadExecutionState"/> is used to unlock hibernation. 
    /// On success <see cref="OnChanged"/> is fired.
    /// </remarks>
    /// <exception cref="PowerException">When <see cref="SetThreadExecutionState"/> fails.</exception>
    public static void AllowHibernation()
    {
      // Must synchronize
      lock (m_Sync)
      {
        // Unblock
        if (0 != --m_HibCount) return;

        // Call kernel
        if (ExecutionState.Error == SetThreadExecutionState(ExecutionState.Continuous)) throw new PowerException("Could not unblock Hibernation");

        // Report
        if (null != OnChanged) OnChanged(false);
      }
    }

    /// <summary>
    /// Synchronized check whether <see cref="m_HibCount"/> is zero or
    /// not.
    /// </summary>
    public static bool HibernationAllowed
    {
      get
      {
        // Synchronize
        lock (m_Sync) return (m_HibCount < 1);
      }
    }

    /// <summary>
    /// Creates a new <see cref="Thread"/> running <see cref="WakeUp"/>.
    /// </summary>
    /// <remarks>
    /// For proper locking <see cref="ForbidHibernation"/> is called
    /// before the <see cref="Thread.Start"/> is activated.
    /// </remarks>
    public static void OnResume()
    {
      // Create wakeup thread
      Thread pWakeUp = new Thread(new ThreadStart(WakeUp));
      pWakeUp.Name = "PowerManager";
      // Lock system
      ForbidHibernation();

      // Start wakeup thread
      pWakeUp.Start();
    }

    /// <summary>
    /// Fire the <see cref="OnPowerUp"/> event. In any case the
    /// method finished with a call to <see cref="AllowHibernation"/>.
    /// <seealso cref="OnResume"/>
    /// </summary>
    private static void WakeUp()
    {
      // Be safe
      try
      {
        // Trigger events
        if (null != OnPowerUp) OnPowerUp();
      }
      finally
      {
        // May suspend
        AllowHibernation();
      }
    }
  }
}
