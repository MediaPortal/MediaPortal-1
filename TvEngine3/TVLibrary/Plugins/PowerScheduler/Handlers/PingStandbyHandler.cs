using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using TvDatabase;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvEngine.PowerScheduler.Handlers
{
  class PingStandbyHandler : IStandbyHandler, IStandbyHandlerEx
  {
    private bool _useAwayMode = false;
    private bool _enabled = false;
    private TvBusinessLayer _tvbLayer = new TvBusinessLayer();
    private int _counter = 0;

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
          TvBusinessLayer layer = new TvBusinessLayer();
          _enabled = Convert.ToBoolean(layer.GetSetting("PowerSchedulerPingMonitorEnabled", "false").Value);
          _useAwayMode = Convert.ToBoolean(layer.GetSetting("PowerSchedulerPingMonitorAwayMode", "false").Value);
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
          if (_enabled && _counter == 4)
          {
            _counter = 0;
            string hosts = _tvbLayer.GetSetting("PowerSchedulerPingMonitorHosts", "").Value;

            if (!(hosts != ""))
            {
              Log.Debug("PS: PingMonitor: No Hosts in List");
            }
            else
            {
              foreach (string str2 in hosts.Split(";".ToCharArray()))
              {
                if (isComputerAvaible(str2))
                {
                  Log.Debug("PS: PingMonitor: Found at least one active Host {0}", str2);
                  return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
                }
              }
              Log.Debug("PS: PingMonitor: No Host seems to be active");
            }
          }
          _counter++;
          return StandbyMode.StandbyAllowed;
        }
      }

      public static bool isComputerAvaible(string hostName)
      {
        try
        {
          Ping ping = new Ping();
          return (ping.Send(hostName, 100).Status == IPStatus.Success);
        }
        catch (Exception)
        {
          return false;
        }
      }

      public void UserShutdownNow() { }

      public string HandlerName
      {
        get { return "Ping Monitor"; }
      }
  }
}
