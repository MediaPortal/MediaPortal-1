using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVControl.Interfaces.ServiceAgents
{
  public interface IEventServiceAgent
  {
    void ReConnect();

    void RegisterCiMenuCallbacks(ICiMenuEventCallback handler);
    void UnRegisterCiMenuCallbacks(ICiMenuEventCallback handler, bool serverDown);
    void RegisterHeartbeatCallbacks(IHeartbeatEventCallbackClient handler);
    void UnRegisterHeartbeatCallbacks(IHeartbeatEventCallbackClient handler, bool serverDown);
    void RegisterTvServerEventCallbacks(ITvServerEventCallbackClient handler);
    void UnRegisterTvServerEventCallbacks(ITvServerEventCallbackClient handler, bool serverDown);
    void Disconnect();
  }
}