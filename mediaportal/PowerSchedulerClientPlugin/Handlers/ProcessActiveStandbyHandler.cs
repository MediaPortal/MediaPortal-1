#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace MediaPortal.Plugins.Process.Handlers
{
  /// <summary>
  /// Prevents standby when one of the given processes is active
  /// </summary>
  public class ProcessActiveStandbyHandler : IStandbyHandlerEx
  {
    #region Variables

    private List<string> _processList = new List<string>();
    private List<string> _preventerList = new List<string>();

    /// <summary>
    /// Use away mode setting
    /// </summary>
    private bool _useAwayMode = false;

    /// <summary>
    /// Check for MP client running setting
    /// </summary>
    private bool _checkForMPClientRunning = false;

    #endregion

    #region Constructor

    public ProcessActiveStandbyHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent +=
          new PowerSchedulerEventHandler(ProcessActiveHandler_OnPowerSchedulerEvent);
    }

    #endregion

    #region Private methods

    private void ProcessActiveHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:

          IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
          if (ps == null)
            return;

          PowerSetting setting = ps.Settings.GetSetting("Processes");

          using (Settings reader = new MPSettings())
          {

            // Get processes to be monitored
            string processes = reader.GetValueAsString("psclientplugin", "Processes", String.Empty);

            // Split processes into processList elements
            List<string> processList = new List<string>();
            foreach (string process in processes.Split(','))
              processList.Add(process.Trim());

            // If something has changed...
            if (!IsEqual(processList, setting.Get<List<string>>()))
            {
              setting.Set<List<string>>(processList);
              _processList = processList;
              Log.Debug(LogType.PS, "ProcessActiveHandler: Preventing standby for processes: {0}", processes);
            }

            // Check if away mode should be used
            setting = ps.Settings.GetSetting("ProcessesAwayMode");
            _useAwayMode = reader.GetValueAsBool("psclientplugin", "ProcessesAwayMode", false);
            if (setting.Get<bool>() != _useAwayMode)
            {
              setting.Set<bool>(_useAwayMode);
              Log.Debug(LogType.PS, "ProcessActiveHandler: Use away mode: {0}", _useAwayMode);
            }

            break;
          }
      }
    }

    private bool IsEqual(List<string> oldConfig, List<string> newConfig)
    {
      if (oldConfig != null && newConfig == null)
        return false;
      if (oldConfig == null && newConfig != null)
        return false;
      foreach (string s in oldConfig)
        if (!newConfig.Contains(s))
          return false;
      foreach (string s in newConfig)
        if (!oldConfig.Contains(s))
          return false;
      return true;
    }

    #endregion

    #region IStandbyHandler implementation

    public bool DisAllowShutdown
    {
      get
      {
        return (StandbyMode != StandbyMode.StandbyAllowed);
      }
    }

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get
      {
        string preventers = String.Empty;
        foreach (string p in _preventerList)
        {
          if (preventers == String.Empty)
            preventers = p;
          else
            preventers = preventers + ", " + p;
        }
        if (preventers == String.Empty)
          return ("Processes");
        else
          return ("Processes" + " (" + preventers + ")");
      }
    }

    #endregion

    #region IStandbyHandlerEx Members

    public StandbyMode StandbyMode
    {
      get
      {
        bool MPClientRunning = false;

        _preventerList.Clear();
        System.Diagnostics.Process[] runningProcesses = System.Diagnostics.Process.GetProcesses();

        foreach (System.Diagnostics.Process rp in runningProcesses)
        {
          string runningProcess = rp.ProcessName;

          if (string.Equals(runningProcess, "MediaPortal", StringComparison.OrdinalIgnoreCase))
            MPClientRunning = true;

          foreach (string process in _processList)
          {
            if (string.Equals(runningProcess, process, StringComparison.OrdinalIgnoreCase))
            {
              if (!_preventerList.Contains(process))
                _preventerList.Add(process);
              break;
            }
          }
          rp.Dispose();
        }

        if (_checkForMPClientRunning && !MPClientRunning)
          _preventerList.Add("MP client inactive");

        if (_preventerList.Count > 0)
        {
          return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
        }
        return StandbyMode.StandbyAllowed;
      }
    }

    #endregion
  }
}