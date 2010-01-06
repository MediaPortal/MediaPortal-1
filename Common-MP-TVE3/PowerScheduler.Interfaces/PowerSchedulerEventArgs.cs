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

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{

  #region Enums

  /// <summary>
  /// All possible event types sent by PowerScheduler
  /// </summary>
  public enum PowerSchedulerEventType
  {
    /// <summary>
    /// Sent when PowerScheduler is started
    /// </summary>
    Started,
    /// <summary>
    /// Sent when PowerScheduler is stopped
    /// </summary>
    Stopped,
    /// <summary>
    /// Sent when Powerscheduler settings have changed
    /// </summary>
    SettingsChanged,
    /// <summary>
    /// Sent when PowerScheduler detects the system state changed from idle to busy
    /// </summary>
    SystemBusy,
    /// <summary>
    /// Sent when PowerScheduler detects the system state changed from busy to idle
    /// </summary>
    SystemIdle,
    /// <summary>
    /// Sent when PowerScheduler is about to put the system into standby or when
    /// powerscheduler detects the system is put into standby
    /// </summary>
    EnteringStandby,
    /// <summary>
    /// Sent when PowerScheduler detects the system resumed from standby
    /// </summary>
    ResumedFromStandby,
    /// <summary>
    /// Sent when PowerScheduler's check interval is due again
    /// (so, sent every PowerSettings.CheckInterval seconds)
    /// </summary>
    Elapsed
  }

  #endregion

  /// <summary>
  /// PowerSchedulerEventArgs are sent by PowerScheduler when the status of PowerScheduler changes.
  /// To find out the exact event type inspect the EventType property
  /// </summary>
  public class PowerSchedulerEventArgs : EventArgs
  {
    #region Variables

    private PowerSchedulerEventType _eventType;
    private object _data;
    private Type _dataType;

    #endregion

    #region Constructor

    public PowerSchedulerEventArgs(PowerSchedulerEventType eventType)
    {
      _eventType = eventType;
    }

    #endregion

    #region Public methods

    public void SetData<T>(object o)
    {
      _dataType = typeof (T);
      _data = o;
    }

    public T GetData<T>()
    {
      Type dataType = typeof (T);
      if (_dataType == dataType)
      {
        return (T)_data;
      }
      return default(T);
    }

    #endregion

    #region Properties

    public PowerSchedulerEventType EventType
    {
      get { return _eventType; }
    }

    #endregion
  }
}