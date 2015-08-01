using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class TuningDetailRepository : GenericRepository<Model>, ITuningDetailRepository
  {
    public TuningDetailRepository()
    {
    }

    public TuningDetailRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public TuningDetailRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<TuningDetail> IncludeAllRelations(IQueryable<TuningDetail> query)
    {
      return query.Include(c => c.Channel).Include(c => c.Channel.GroupMaps);
    }
  }
}