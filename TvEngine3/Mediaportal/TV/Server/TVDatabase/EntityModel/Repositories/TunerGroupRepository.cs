using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class TunerGroupRepository : GenericRepository<Model>, ITunerGroupRepository
  {
    public TunerGroupRepository()
    {
    }

    public TunerGroupRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public TunerGroupRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<TunerGroup> IncludeAllRelations(IQueryable<TunerGroup> query)
    {
      return query.Include(tg => tg.Tuners);
    }
  }
}