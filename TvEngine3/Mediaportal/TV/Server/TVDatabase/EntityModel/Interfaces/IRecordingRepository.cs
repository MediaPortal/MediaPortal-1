using System.Linq;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IRecordingRepository : IRepository<Model>
  {
    Recording GetRecording(int idRecording);
    IQueryable<Recording> ListAllRecordingsByMediaType(MediaType mediaType);
    IQueryable<Recording> IncludeAllRelations(IQueryable<Recording> query);
  }
}
