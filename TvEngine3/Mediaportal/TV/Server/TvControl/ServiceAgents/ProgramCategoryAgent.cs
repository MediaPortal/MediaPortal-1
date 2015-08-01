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

    public ProgramCategory SaveProgramCategory(ProgramCategory programCategory)
    {
      programCategory.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveProgramCategory(programCategory);
    }

    public IList<ProgramCategory> SaveProgramCategories(IEnumerable<ProgramCategory> programCategories)
    {
      foreach (ProgramCategory programCategory in programCategories)
      {
        programCategory.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveProgramCategories(programCategories);
    }

    public IList<GuideCategory> ListAllGuideCategories()
    {
      return _channel.ListAllGuideCategories();
    }

    public GuideCategory SaveGuideCategory(GuideCategory guideCategory)
    {
      guideCategory.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveGuideCategory(guideCategory);
    }
  }
}