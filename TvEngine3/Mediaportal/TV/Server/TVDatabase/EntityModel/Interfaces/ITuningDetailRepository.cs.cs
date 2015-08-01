using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface ITuningDetailRepository : IRepository<Model>
  {
    IQueryable<TuningDetail> IncludeAllRelations(IQueryable<TuningDetail> query);
  }
}