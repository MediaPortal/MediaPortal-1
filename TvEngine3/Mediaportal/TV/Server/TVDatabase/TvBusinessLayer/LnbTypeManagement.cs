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
      using (IRepository<Model> lnbTypeRepository = new GenericRepository<Model>())
      {
        LnbType query = lnbTypeRepository.Single<LnbType>(l => l.IdLnbType == idLnbType);
        return query;
      }
    }

    public static IList<LnbType> ListAllLnbTypes()
    {
      using (IRepository<Model> lnbTypeRepository = new GenericRepository<Model>())
      {
        return lnbTypeRepository.GetAll<LnbType>().ToList();
      }
    }
  }
}
