using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using TvEngine.PowerScheduler.Interfaces;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.PowerScheduler.Handlers
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
          _enabled = SettingsManagement.GetValue("PowerSchedulerActivePortsHandlerEnabled", true);
          _useAwayMode = SettingsManagement.GetValue("PowerSchedulerActivePortsHandlerAwayMode", false);
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
