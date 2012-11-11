using Mediaportal.TV.Server.TVControl.Interfaces.Events;

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
