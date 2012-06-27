using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Events
{
  [ServiceKnownType(typeof(SubChannel))]
  //[ServiceKnownType(typeof(User))]
  //[ServiceKnownType(typeof(ISubChannel))]
  //[ServiceKnownType(typeof(IUser))]
  public interface ITvServerEventCallback
  {
    [OperationContract(IsOneWay = true)]
    void CallbackTvServerEvent(TvServerEventArgs eventArgs);    
  }
}
