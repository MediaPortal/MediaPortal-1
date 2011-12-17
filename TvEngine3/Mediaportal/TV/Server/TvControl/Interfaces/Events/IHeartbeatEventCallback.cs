using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Events
{
  public interface IHeartbeatEventCallback 
  {
    [OperationContract(IsOneWay = true)]
    void HeartbeatRequestReceived();
  }
}
