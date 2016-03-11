using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvDatabase;
using TvLibrary.Log;

namespace TvEngine.PowerScheduler.Handlers
{
  class ActivePortsHandler : IStandbyHandler, IStandbyHandlerEx
  {
    private bool _useAwayMode = false;
    private bool _enabled = true;

    public ActivePortsHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent += new PowerSchedulerEventHandler(ActivePortsHandler_OnPowerSchedulerEvent);

    }

    private void ActivePortsHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:
          TvBusinessLayer layer = new TvBusinessLayer();
          _enabled = Convert.ToBoolean(layer.GetSetting("PowerSchedulerActivePortsHandlerEnabled", "true").Value);
          _useAwayMode = Convert.ToBoolean(layer.GetSetting("PowerSchedulerActivePortsHandlerAwayMode", "false").Value);
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

    private static bool IsPortBeingUsed(int port)
    {
      return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Any(
                              tcpConnectionInformation => tcpConnectionInformation.LocalEndPoint.Port == port);
    }

    public void UserShutdownNow() { }

    public string HandlerName
    {
      get { return "ActivePorts"; }
    }

    public StandbyMode StandbyMode
    {
      get
      {
        if (_enabled)
        {
          if (IsPortBeingUsed(3389) || IsPortBeingUsed(5900))
          {
            return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
          }
        }
        return StandbyMode.StandbyAllowed;
      }
    }
  }
}
