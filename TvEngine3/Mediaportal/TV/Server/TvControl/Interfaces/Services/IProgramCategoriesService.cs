using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  // Define a service contract.
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IProgramCategoryService
  {
    [OperationContract]
    IList<ProgramCategory> ListAllProgramCategories();

    [OperationContract]
    IList<TvGuideCategory> ListAllTvGuideCategories();

    [OperationContract]
    TvGuideCategory SaveTvGuideCategory(TvGuideCategory tvGuideCategory);

    [OperationContract]
    ProgramCategory SaveProgramCategory(ProgramCategory programCategory);
  }
}
