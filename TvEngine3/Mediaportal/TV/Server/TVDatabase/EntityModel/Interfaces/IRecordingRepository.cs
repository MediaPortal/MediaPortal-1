using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IRecordingRepository : IRepository<Model>
  {
    IQueryable<Recording> IncludeAllRelations(IQueryable<Recording> query);
  }
}