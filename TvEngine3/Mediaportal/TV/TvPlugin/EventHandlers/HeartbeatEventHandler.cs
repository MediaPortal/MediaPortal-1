using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVService.ServiceAgents;
using Mediaportal.TV.Server.TVService.ServiceAgents.Interfaces;

namespace Mediaportal.TV.TvPlugin.EventHandlers
{
  public class HeartbeatEventHandler : IHeartbeatEventCallbackClient
  {    
    #region Implementation of IHeartbeatEventCallbackClient

    public void HeartbeatRequestReceived()
    {
      //ignore      
    }

    #endregion
  }
}
