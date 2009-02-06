#region Copyright (C) 2007-2008 Team MediaPortal

/* 
 *	Copyright (C) 2007-2008 Team MediaPortal
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

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{

  #region Enums

  public enum ShutdownMode
  {
    Suspend = 0,
    Hibernate = 1,
    StayOn = 2
  }

  #endregion

  /// <summary>
  /// Holds all PowerScheduler related settings
  /// </summary>
  [Serializable]
  public class PowerSettings : IPowerSettings
  {
    #region Variables

    private bool _shutdownEnabled;
    private bool _wakeupEnabled;
    private bool _forceShutdown;
    private bool _extensiveLogging;
    private int _idleTimeout;
    private int _preWakeupTime;
    private int _preNoShutdownTime;
    private int _checkInterval;
    private int _allowedStart;
    private int _allowedStop;
    private ShutdownMode _shutdownMode = ShutdownMode.StayOn;

    /// <summary>
    /// Placeholder for additional PowerScheduler settings
    /// </summary>
    [NonSerialized] private Dictionary<string, PowerSetting> _settings;

    #endregion

    #region Constructor

    public PowerSettings()
    {
      _settings = new Dictionary<string, PowerSetting>();
    }

    public PowerSettings(PowerSettings s)
    {
      _shutdownEnabled = s.ShutdownEnabled;
      _wakeupEnabled = s.WakeupEnabled;
      _forceShutdown = s.ForceShutdown;
      _extensiveLogging = s.ExtensiveLogging;
      _idleTimeout = s.IdleTimeout;
      _preWakeupTime = s.PreWakeupTime;
      _preNoShutdownTime = s.PreNoShutdownTime;
      _checkInterval = s.CheckInterval;
      _shutdownMode = s.ShutdownMode;
      _settings = s._settings;
      _allowedStart = s.AllowedSleepStartTime;
      _allowedStop = s.AllowedSleepStopTime;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Adds a custom setting
    /// </summary>
    /// <param name="setting">PowerSetting to add</param>
    public void AddSetting(PowerSetting setting)
    {
      if (_settings.ContainsKey(setting.Name))
      {
        throw new ArgumentException("settings already contain key", setting.Name);
      }
      _settings.Add(setting.Name, setting);
    }

    /// <summary>
    /// Checks if the given setting name is configured
    /// </summary>
    /// <param name="name">setting name</param>
    /// <returns>is this setting name configured?</returns>
    public bool HasSetting(string name)
    {
      if (_settings.ContainsKey(name))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Gets a custom PowerScheduler setting
    /// </summary>
    /// <param name="name">setting name</param>
    /// <returns>requested PowerSetting</returns>
    public PowerSetting GetSetting(string name)
    {
      if (_settings.ContainsKey(name))
      {
        return _settings[name];
      }
      else
      {
        PowerSetting setting = new PowerSetting(name);
        AddSetting(setting);
        return setting;
      }
    }

    /// <summary>
    /// Clones the current PowerSettings. Only the default properties are copied;
    /// the additional settings are just referenced. Modifying those for
    /// "local" purposes is unsupported.
    /// </summary>
    /// <returns></returns>
    public PowerSettings Clone()
    {
      return new PowerSettings(this);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Should PowerScheduler actively try to put the system into standby?
    /// </summary>
    public bool ShutdownEnabled
    {
      get { return _shutdownEnabled; }
      set { _shutdownEnabled = value; }
    }

    /// <summary>
    /// Should PowerScheduler check when any plugin wants to wakeup the system?
    /// </summary>
    public bool WakeupEnabled
    {
      get { return _wakeupEnabled; }
      set { _wakeupEnabled = value; }
    }

    /// <summary>
    /// Should the shutdown attemps be forced?
    /// </summary>
    public bool ForceShutdown
    {
      get { return _forceShutdown; }
      set { _forceShutdown = value; }
    }

    /// <summary>
    /// Should PowerScheduler be verbose when logging?
    /// </summary>
    public bool ExtensiveLogging
    {
      get { return _extensiveLogging; }
      set { _extensiveLogging = value; }
    }

    /// <summary>
    /// If _shutdownEnabled, how long (in minutes) to wait before putting the
    /// system into standby
    /// </summary>
    public int IdleTimeout
    {
      get { return _idleTimeout; }
      set
      {
        if (value < 1)
        {
          throw new ArgumentException("IdleTimeout cannot be smaller than 1");
        }
        _idleTimeout = value;
      }
    }

    /// <summary>
    /// if _wakeupEnabled, the time (in seconds) to wakeup the system earlier than
    /// the actual wakeup time
    /// </summary>
    public int PreWakeupTime
    {
      get { return _preWakeupTime; }
      set
      {
        if (value < 0)
        {
          throw new ArgumentException("PrewakeupTime cannot be smaller than 0");
        }
        _preWakeupTime = value;
      }
    }

    /// <summary>
    /// if _wakeupEnabled, the time (in seconds) the system is not allowed to 
    /// go to shutodwn before the actual wakeup time
    /// </summary>
    public int PreNoShutdownTime
    {
      get { return _preNoShutdownTime; }
      set
      {
        if (value < 0)
        {
          throw new ArgumentException("PrenoshutdownTime cannot be smaller than 0");
        }
        _preNoShutdownTime = value;
      }
    }

    /// <summary>
    /// Controls the granularity of the standby/wakeup checks in seconds
    /// </summary>
    public int CheckInterval
    {
      get { return _checkInterval; }
      set
      {
        if (value < 1)
        {
          throw new ArgumentException("CheckInterval cannot be smaller than 1");
        }
        _checkInterval = value;
      }
    }

    /// <summary>
    /// Controls the minimum start hour of suspends. 
    /// </summary>
    public int AllowedSleepStartTime
    {
      get { return _allowedStart; }
      set { _allowedStart = value; }
    }

    /// <summary>
    /// Controls the maximum start hour for a suspend.
    /// </summary>
    public int AllowedSleepStopTime
    {
      get { return _allowedStop; }
      set { _allowedStop = value; }
    }

    /// <summary>
    /// How should put the system into standby? suspend/hibernate/stayon
    /// suspend uses S3, hibernate uses S4, stayon is for debugging purposes and
    /// doesn't put the system into standby at all
    /// </summary>
    public ShutdownMode ShutdownMode
    {
      get { return _shutdownMode; }
      set
      {
        switch (value)
        {
          case ShutdownMode.Suspend:
          case ShutdownMode.Hibernate:
          case ShutdownMode.StayOn:
            _shutdownMode = value;
            break;
          default:
            throw new ArgumentException("unknown ShutdownMode", value.ToString());
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Holds one custom setting for PowerScheduler
  /// </summary>
  public class PowerSetting
  {
    #region Variables

    public readonly string Name;
    private object _object;
    private Type _type;

    #endregion

    #region Constructor

    public PowerSetting(string name)
    {
      Name = name;
    }

    #endregion

    #region Public methods

    public void Set<T>(object o)
    {
      _type = typeof (T);
      _object = o;
    }

    public T Get<T>()
    {
      Type type = typeof (T);
      if (_type == type)
      {
        return (T) _object;
      }
      return default(T);
    }

    #endregion
  }
}