using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IScheduleRepository : IRepository<Model>
  {
    IQueryable<Schedule> IncludeAllRelations(IQueryable<Schedule> query);
  }
}
