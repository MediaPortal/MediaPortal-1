using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Mediaportal.TV.Server.TVControl.Events;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Events
{
  public interface ITvServerEventCallback
  {
    [OperationContract(IsOneWay = true)]
    void CallbackTvServerEvent(TvServerEventArgs eventArgs);    
  }
}
