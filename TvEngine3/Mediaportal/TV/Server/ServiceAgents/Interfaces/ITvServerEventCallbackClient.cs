using Mediaportal.TV.Server.TVControl.Events;

namespace Mediaportal.TV.Server.TVService.ServiceAgents.Interfaces
{
  public interface ITvServerEventCallbackClient
  {
    void CallbackTvServerEvent(TvServerEventArgs eventArgs);    
  }
}