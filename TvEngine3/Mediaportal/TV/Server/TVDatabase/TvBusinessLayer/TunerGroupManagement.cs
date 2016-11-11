using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class TunerGroupManagement
  {
    public static IList<TunerGroup> ListAllTunerGroups()
    {
      using (ITunerGroupRepository tunerGroupRepository = new TunerGroupRepository())
      {
        var query = tunerGroupRepository.GetQuery<TunerGroup>().OrderBy(tg => tg.Name);
        return tunerGroupRepository.IncludeAllRelations(query).ToList();
      }
    }

    public static TunerGroup GetTunerGroup(int idTunerGroup)
    {
      using (ITunerGroupRepository tunerGroupRepository = new TunerGroupRepository())
      {
        var query = tunerGroupRepository.GetQuery<TunerGroup>(tg => tg.IdTunerGroup == idTunerGroup);
        return tunerGroupRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static TunerGroup SaveTunerGroup(TunerGroup group)
    {
      using (ITunerGroupRepository tunerGroupRepository = new TunerGroupRepository())
      {
        tunerGroupRepository.AttachEntityIfChangeTrackingDisabled(tunerGroupRepository.ObjectContext.TunerGroups, group);
        tunerGroupRepository.ApplyChanges(tunerGroupRepository.ObjectContext.TunerGroups, group);
        tunerGroupRepository.UnitOfWork.SaveChanges();
        group.AcceptChanges();
        return group;
      }
    }

    public static void DeleteTunerGroup(int idTunerGroup)
    {
      using (ITunerGroupRepository tunerGroupRepository = new TunerGroupRepository(true))
      {
        tunerGroupRepository.Delete<TunerGroup>(g => g.IdTunerGroup == idTunerGroup);
        tunerGroupRepository.UnitOfWork.SaveChanges();
      }
    }
  }
}