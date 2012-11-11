using System;
using System.ServiceModel;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Events
{
  [ServiceKnownType(typeof(VirtualCard))]
  [ServiceKnownType(typeof(SubChannel))]
  [ServiceKnownType(typeof(User))]
  public interface IServerEventCallback
  {
    [OperationContract(AsyncPattern = true)]
    IAsyncResult BeginOnCallbackTvServerEvent(TvServerEventArgs eventArgs, AsyncCallback callback, object asyncState);
    void EndOnCallbackTvServerEvent(IAsyncResult result);

    [OperationContract(AsyncPattern = true)]
    IAsyncResult BeginOnCallbackCiMenuEvent(CiMenu menu, AsyncCallback callback, object asyncState);
    void EndOnCallbackCiMenuEvent(IAsyncResult result);
    
    [OperationContract(AsyncPattern = true)]
    IAsyncResult BeginOnCallbackHeartBeatEvent(AsyncCallback callback, object asyncState);
    void EndOnCallbackHeartBeatEvent(IAsyncResult result);
  }
}
