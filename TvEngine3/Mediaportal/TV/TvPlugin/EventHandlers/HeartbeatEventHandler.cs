using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVService.ServiceAgents;

namespace Mediaportal.TV.TvPlugin.EventHandlers
{
  public class HeartbeatEventHandler : IHeartbeatEventCallback
  {    
    #region Implementation of IHeartbeatEventCallback

    public void HeartbeatRequestReceived()
    {
      //ignore      
    }

    #endregion
  }
}
