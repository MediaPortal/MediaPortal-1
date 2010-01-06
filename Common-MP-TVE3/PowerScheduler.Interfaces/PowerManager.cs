#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{
  public class PowerManager
  {
    #region Variables

    /// <summary>
    /// Does the PowerManager allow/prevent standby?
    /// </summary>
    private bool _standbyAllowed = false;

    #endregion

    #region External power management methods and enumerations

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

    [DllImport("kernel32.dll", EntryPoint = "SetThreadExecutionState")]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    #endregion

    #region Power management wrapper methods

    /// <summary>
    /// Set the PowerManager to allow standby
    /// </summary>
    /// <returns>bool indicating whether or not standby is allowed</returns>
    public bool AllowStandby()
    {
      if (_standbyAllowed)
      {
        return true;
      }
      lock (this)
      {
        ExecutionState result = SetThreadExecutionState(ExecutionState.Continuous);
        if (result == ExecutionState.Error)
        {
          return false;
        }
        else
        {
          _standbyAllowed = true;
          return true;
        }
      }
    }

    /// <summary>
    /// Set the PowerManager to prevent standby
    /// </summary>
    /// <returns>bool indicating whether or not standby is prevented</returns>
    public bool PreventStandby()
    {
      if (!_standbyAllowed)
      {
        return true;
      }
      lock (this)
      {
        ExecutionState result = SetThreadExecutionState(ExecutionState.SystemRequired | ExecutionState.Continuous);
        if (result == ExecutionState.Error)
        {
          return false;
        }
        else
        {
          _standbyAllowed = false;
          return true;
        }
      }
    }

    #endregion
  }
}