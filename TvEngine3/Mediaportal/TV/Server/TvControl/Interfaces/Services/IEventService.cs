using System.ServiceModel;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(CallbackContract = typeof(IServerEventCallback))]
  public interface IEventService
  {
    [OperationContract (IsOneWay = true)]    
    void Subscribe(string userName);

    [OperationContract(IsOneWay = true)]    
    void Unsubscribe(string userName);
  }
}
