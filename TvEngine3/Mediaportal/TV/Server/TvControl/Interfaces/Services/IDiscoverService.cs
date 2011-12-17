using System;
using System.ServiceModel;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IDiscoverService
  {
    [OperationContract]
    DateTime Ping();
  }
}