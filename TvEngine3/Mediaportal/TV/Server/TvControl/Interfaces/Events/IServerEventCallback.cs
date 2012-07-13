using System;
using System.ServiceModel;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Events
{      
  [ServiceKnownType(typeof(SubChannel))]
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
