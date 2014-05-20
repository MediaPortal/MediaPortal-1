using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Threading;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using TvEngine.PowerScheduler.Interfaces;


namespace Mediaportal.TV.Server.Plugins.PowerScheduler.Handlers
{
  class PingStandbyHandler : IStandbyHandler, IStandbyHandlerEx
  {
    private bool _useAwayMode = false;
    private bool _enabled = false;
    private bool _pingRun = false;
    private bool _isActiveHost = false;

    #region Constructor
    
    public PingStandbyHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent += new PowerSchedulerEventHandler(PingMonitorHandler_OnPowerSchedulerEvent);
    }

    #endregion
   
    private void PingMonitorHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:
          _enabled = SettingsManagement.GetValue("PowerSchedulerPingMonitorEnabled", false);
          _useAwayMode = SettingsManagement.GetValue("PowerSchedulerPingMonitorAwayMode", false);
          break;
        }
      }

      public bool DisAllowShutdown
      {
        get
        {
          return (StandbyMode != StandbyMode.StandbyAllowed);
        }
      }

      public StandbyMode StandbyMode
      {
        get
        {
          if (_enabled && _pingRun)
          {
            _pingRun = false;

            if (_isActiveHost)
            {
              _isActiveHost = false;
              return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
            }
            else
            {
              this.LogDebug("PS: PingMonitor: No Host seems to be active");
              return StandbyMode.StandbyAllowed;
            }
          }

          if (_enabled && !_pingRun)
          {
            string hosts = SettingsManagement.GetValue("PowerSchedulerPingMonitorHosts", string.Empty);
            if (string.IsNullOrEmpty(hosts))
            {
              this.LogDebug("PS: PingMonitor: No Hosts in List");
            }
            else
            {
              _pingRun = true;
              foreach (string hostName in hosts.Split(";".ToCharArray()))
              {
                try
                {
                  Ping ping = new Ping();
                  ping.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                  ping.SendAsync(hostName, 100);
                }
                catch (Exception) {}
              }
            }
          }
          return StandbyMode.StandbyAllowed;
        }
      }

      private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
      {
        if (e.Cancelled)
        {
          this.LogDebug("PS: PingCompletedCallback, Ping canceled.");
          return;
        }

        if (e.Reply.Status == IPStatus.Success)
        {
          _isActiveHost = true;
          this.LogDebug("PS: PingMonitor found an active host {0}", e.Reply.Address);
        }
      }

      public void UserShutdownNow() { }

      public string HandlerName
      {
        get { return "Ping Monitor"; }
      }
  }
}
