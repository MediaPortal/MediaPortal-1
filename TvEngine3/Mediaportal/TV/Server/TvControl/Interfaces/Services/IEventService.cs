using System.ServiceModel;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IHeartbeatEventCallback))]
  public interface IEventServiceHeartbeat
  {   
  }

  [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICiMenuEventCallback))]
  public interface IEventServiceCiMenu
  {
  }

  [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITvServerEventCallback))]
  public interface IEventServiceTvServer
  {
  }

  [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IServerEventCallback))]
  public interface IEventService : IEventServiceHeartbeat, IEventServiceCiMenu, IEventServiceTvServer
  {
    [OperationContract(IsOneWay = true)]
    void Subscribe(string username);

    [OperationContract(IsOneWay = true)]
    void Unsubscribe(string username);
  }

  /*
     In a contract inheritance hierarchy, the ServiceContract's CallbackContract must be a subtype of the CallbackContracts of all of the 
   * CallbackContracts of the ServiceContracts inherited by the original ServiceContract, 
   * Types 
   * IEventServiceCiMenu and 
   * IEventService violate this rule.
   */
}
