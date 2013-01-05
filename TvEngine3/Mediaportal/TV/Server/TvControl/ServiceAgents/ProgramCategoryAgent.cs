using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class ProgramCategoryAgent : ServiceAgent<IProgramCategoryService>, IProgramCategoryService
  {
    public ProgramCategoryAgent(string hostname)
      : base(hostname)
    {
    }

    public IList<ProgramCategory> ListAllProgramCategories()
    {
      return _channel.ListAllProgramCategories();
    }

    public IList<TvGuideCategory> ListAllTvGuideCategories()
    {
      return _channel.ListAllTvGuideCategories();
    }

    public TvGuideCategory SaveTvGuideCategory(TvGuideCategory tvGuideCategory)
    {
      tvGuideCategory.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveTvGuideCategory(tvGuideCategory);
    }

    public ProgramCategory SaveProgramCategory(ProgramCategory programCategory)
    {
      programCategory.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveProgramCategory(programCategory);
    }
  }
}