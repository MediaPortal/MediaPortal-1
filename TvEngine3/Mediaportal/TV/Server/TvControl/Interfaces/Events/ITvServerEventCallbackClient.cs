using Mediaportal.TV.Server.TVControl.Events;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Events
{
  public interface ITvServerEventCallbackClient
  {
    void CallbackTvServerEvent(TvServerEventArgs eventArgs);    
  }
}