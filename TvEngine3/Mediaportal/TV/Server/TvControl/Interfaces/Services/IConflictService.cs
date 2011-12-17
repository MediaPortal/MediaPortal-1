using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IConflictService
  {
    [OperationContract]
    IList<Conflict> ListAllConflicts();

    [OperationContract]
    Conflict SaveConflict(Conflict conflict);

    [OperationContract]
    Conflict GetConflict(int idConflict);
  }
}
