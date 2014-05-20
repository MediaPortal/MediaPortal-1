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

using System;
using System.Collections;
using System.Diagnostics;
using TvDatabase;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Represents a network adapter installed on the machine.
  /// Properties of this class can be used to obtain current network speed.
  /// </summary>
  internal class NetworkAdapter
  {
    #region Variables

    private PerformanceCounter _dlCounter, _ulCounter; // Performance counters to monitor download and upload speed.
    private long _dlValueOld, _ulValueOld; // Download Upload counter value one second earlier, in bytes.
    private DateTime _lastSampleTime = DateTime.Now;
    private long _dlSpeed, _ulSpeed; // Download Upload peak values in KB/s
    private string _name; // The name of the adapter.

    #endregion

    #region Private methods

    /// <summary>
    /// Instances of this class are supposed to be created only in an NetworkMonitorHandler.
    /// </summary>
    internal NetworkAdapter(string name)
    {
      _name = name;

      // Create performance counters for the adapter.
      _dlCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", this._name);
      _ulCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", this._name);

      // Since dlValueOld and ulValueOld are used in method update() to calculate network speed,
      // they must have be initialized.
      _dlValueOld = _dlCounter.NextSample().RawValue;
      _ulValueOld = _ulCounter.NextSample().RawValue;
    }

    /// <summary>
    /// Obtain new sample from performance counters, and update the values saved in dlSpeed, ulSpeed, etc.
    /// This method is supposed to be called only in NetworkMonitorHandler, one time every second.
    /// </summary>
    internal void Update()
    {
      DateTime thisSampleTime = DateTime.Now;

      // Download Upload counter value in bytes.
      long dlValue = _dlCounter.NextSample().RawValue;
      long ulValue = _ulCounter.NextSample().RawValue;

      // Calculates download and upload speed.
      double monitorInterval = thisSampleTime.Subtract(_lastSampleTime).TotalSeconds;
      _lastSampleTime = thisSampleTime;
      _dlSpeed = (long)((dlValue - _dlValueOld) / monitorInterval / 1024);
      _ulSpeed = (long)((ulValue - _ulValueOld) / monitorInterval / 1024);

      _dlValueOld = dlValue;
      _ulValueOld = ulValue;
    }

    internal string Name
    {
      get { return _name; }
    }

    internal long DlSpeed
    {
      get { return _dlSpeed; }
    }

    internal long UlSpeed
    {
      get { return _ulSpeed; }
    }

    #endregion
  }

  /// <summary>
  /// Prevent standby if there is network activity (configurable) 
  /// </summary>
  public class ActiveNetworkStandbyHandler : IStandbyHandler, IStandbyHandlerEx
  {
    #region Variables

    private Int32 _idleLimit; // Minimum transferrate considered as network activity in KB/s.

    private ArrayList _monitoredAdapters = new ArrayList(); // The list of monitored adapters on the computer.
    private int _preventers = 0;

    /// <summary>
    /// Use away mode setting
    /// </summary>
    private bool _useAwayMode = false;

    #endregion

    #region Constructor

    public ActiveNetworkStandbyHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent +=
          new PowerSchedulerEventHandler(NetworkMonitorHandler_OnPowerSchedulerEvent);
    }

    #endregion

    #region Private methods

    private void NetworkMonitorHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      if (ps == null)
        return;

      TvBusinessLayer layer = new TvBusinessLayer();
      PowerSetting setting;
      bool enabled;

      switch (args.EventType)
      {
        case PowerSchedulerEventType.Elapsed:

          // Check if standby should be prevented
          setting = ps.Settings.GetSetting("NetworkMonitorEnabled");
          enabled = Convert.ToBoolean(layer.GetSetting("PowerSchedulerNetworkMonitorEnabled", "false").Value);

          if (setting.Get<bool>() != enabled) // Setting changed
          {
            setting.Set<bool>(enabled);
            if (enabled) // Start
            {
              Log.Debug(LogType.PS, "NetworkMonitorHandler: Network monitor started");
              StartNetworkMonitor();
            }
            else // Stop
            {
              Log.Debug(LogType.PS, "NetworkMonitorHandler: Network monitor stopped");
              StopNetworkMonitor();
            }
          }

          // Get minimum transferrate considered as network activity
          if (enabled) 
          {
            setting = ps.Settings.GetSetting("NetworkMonitorIdleLimit");
            _idleLimit = Int32.Parse(layer.GetSetting("PowerSchedulerNetworkMonitorIdleLimit", "2").Value);
            if (setting.Get<Int32>() != _idleLimit)
            {
              setting.Set<Int32>(_idleLimit);
              Log.Debug(LogType.PS, "NetworkMonitorHandler: Idle limit in KB/s: {0}", _idleLimit);
            }

            // Check if away mode should be used
            setting = ps.Settings.GetSetting("NetworkMonitorAwayMode");
            _useAwayMode = Convert.ToBoolean(layer.GetSetting("PowerSchedulerNetworkMonitorAwayMode", "false").Value);
            if (setting.Get<bool>() != _useAwayMode)
            {
              setting.Set<bool>(_useAwayMode);
              Log.Debug(LogType.PS, "NetworkMonitorHandler: Use away mode: {0}", _useAwayMode);
            }
          }

          break;
      }
    }

    private void StartNetworkMonitor()
    {
      try
      {
        _monitoredAdapters.Clear();

        PerformanceCounterCategory category =
          new PerformanceCounterCategory("Network Interface");

        // Enumerates network adapters installed on the computer.
        foreach (string name in category.GetInstanceNames())
        {
          // This one exists on every computer.
          if (name == "MS TCP Loopback interface") continue;

          // Create an instance of NetworkAdapter class.        
          NetworkAdapter adapter = new NetworkAdapter(name);

          _monitoredAdapters.Add(adapter); // Add it to monitored adapters
        }
      }    
      catch (Exception ex)
      {
        Log.Error(LogType.PS, "NetworkMonitorHandler: Exception in StartNetworkMonitor: {0}", ex);
        Log.Info(LogType.PS, "NetworkMonitorHandler: Exception in StartNetworkMonitor: {0}", ex);
      }
    }

    // Disable the timer, and clear the monitoredAdapters list.
    private void StopNetworkMonitor()
    {
      _monitoredAdapters.Clear();
    }

    #endregion

    #region IStandbyHandler implementation

    public bool DisAllowShutdown
    {
      get { return (StandbyMode != StandbyMode.StandbyAllowed); }
    }

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get { return "Active Network"; }
    }

    #endregion

    #region IStandbyHandlerEx Members

    public StandbyMode StandbyMode
    {
      get
      {
        _preventers = 0;

        foreach (NetworkAdapter adapter in _monitoredAdapters)
        {
          try
          {
            adapter.Update();
            if ((adapter.DlSpeed >= _idleLimit) || (adapter.UlSpeed >= _idleLimit))
            {
              // Log.Debug(LogType.PS, "NetworkMonitorHandler: standby prevented: {0}", adapter.Name);
              // Log.Debug(LogType.PS, "NetworkMonitorHandler: dlSpeed: {0}", adapter.DlSpeed);
              // Log.Debug(LogType.PS, "NetworkMonitorHandler: ulSpeed: {0}", adapter.UlSpeed);
              _preventers++;
            }
          }
          catch (Exception ex)
          {
            Log.Error(LogType.PS, "NetworkMonitorHandler: Exception in updating adapter {0}: {1}", adapter.Name, ex.Message);
            Log.Info(LogType.PS, "NetworkMonitorHandler: Exception in updating adapter {0}: {1}", adapter.Name, ex.Message);
          }
        }

        if (_preventers > 0)
        {
          return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
        }
        return StandbyMode.StandbyAllowed;
      }
    }

    #endregion
  }
}