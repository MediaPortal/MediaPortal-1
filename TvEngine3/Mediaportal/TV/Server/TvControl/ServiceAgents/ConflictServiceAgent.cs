using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class ConflictServiceAgent : ServiceAgent<IConflictService>, IConflictService
  {
    public ConflictServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<Conflict> ListAllConflicts()
    {
      return _channel.ListAllConflicts();
    }

    public Conflict SaveConflict(Conflict conflict)
    {
      conflict.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveConflict(conflict);
    }

    public Conflict GetConflict(int idConflict)
    {
      return _channel.GetConflict(idConflict);
    }
  }
}
