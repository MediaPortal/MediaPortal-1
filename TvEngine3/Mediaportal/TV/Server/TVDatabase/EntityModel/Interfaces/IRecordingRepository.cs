using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IRecordingRepository : IRepository<TvModel>
  {
    Recording GetRecording(int idRecording);
    IQueryable<Recording> ListAllRecordingsByMediaType(MediaTypeEnum mediaType);
    IQueryable<Recording> IncludeAllRelations(IQueryable<Recording> query);

  }
}
