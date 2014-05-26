using System;
using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
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

    public Recording GetRecording(int idRecording)
    {
      Recording recording = GetQuery<Recording>(c => c.IdRecording == idRecording)
        .Include(r => r.Channel)
        .Include(r => r.RecordingCredits)
        .Include(r => r.Schedule)
        .Include(r => r.ProgramCategory)
        .FirstOrDefault();
      return recording;
    }

    public IQueryable<Recording> ListAllRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      IQueryable<Recording> recordings = GetQuery<Recording>(r => r.MediaType == (int)mediaType)
        .Include(r => r.Channel)
        .Include(r => r.RecordingCredits)
        .Include(c => c.Schedule)
        .Include(r => r.ProgramCategory);
      return recordings;
    }

    public IQueryable<Recording> IncludeAllRelations(IQueryable<Recording> query)
    {
      var includeRelations = query.Include(r => r.Channel)
                                  .Include(r => r.RecordingCredits)
                                  .Include(c => c.Schedule)
                                  .Include(r => r.ProgramCategory);
      return includeRelations;
    }
  }
}
