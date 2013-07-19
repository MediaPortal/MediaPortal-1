using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class LnbTypeManagement
  {
    public static LnbType GetLnbType(int idLnbType)
    {
      using (IRepository<TvModel> lnbTypeRepository = new GenericRepository<TvModel>())
      {
        LnbType query = lnbTypeRepository.Single<LnbType>(l => l.IdLnbType == idLnbType);
        return query;
      }
    }

    public static IList<LnbType> ListAllLnbTypes()
    {
      using (IRepository<TvModel> lnbTypeRepository = new GenericRepository<TvModel>())
      {
        return lnbTypeRepository.GetAll<LnbType>().ToList();
      }
    }
  }
}
