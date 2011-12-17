using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface ICanceledScheduleService
  {
    [OperationContract]
    CanceledSchedule SaveCanceledSchedule(CanceledSchedule canceledSchedule);
  }
}
