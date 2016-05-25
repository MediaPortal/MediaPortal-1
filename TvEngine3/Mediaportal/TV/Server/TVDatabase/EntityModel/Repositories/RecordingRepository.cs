using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class RecordingRepository : GenericRepository<Model>, IRecordingRepository
  {
    public RecordingRepository()
    {
    }

    public RecordingRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public RecordingRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<Recording> IncludeAllRelations(IQueryable<Recording> query)
    {
      var includeRelations = query.Include(r => r.Channel)
                                  .Include(r => r.RecordingCredits)
                                  .Include(r => r.Schedule)
                                  .Include(r => r.ProgramCategory);
      return includeRelations;
    }
  }
}