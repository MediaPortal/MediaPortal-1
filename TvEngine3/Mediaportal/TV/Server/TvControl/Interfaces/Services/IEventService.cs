using System.ServiceModel;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;

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
