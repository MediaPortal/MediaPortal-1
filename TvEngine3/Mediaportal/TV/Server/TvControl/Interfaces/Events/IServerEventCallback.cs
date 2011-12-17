using System.ServiceModel;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Events
{
  public interface IServerEventCallback : ITvServerEventCallback, ICiMenuEventCallback, IHeartbeatEventCallback
  {
    [OperationContract(IsOneWay = true)]
    void Ping();  
  }
}
