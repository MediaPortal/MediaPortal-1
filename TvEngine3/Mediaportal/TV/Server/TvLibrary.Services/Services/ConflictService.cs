using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{  
  public class ConflictService : IConflictService
  {    
    public IList<Conflict> ListAllConflicts()
    {
      return ConflictManagement.ListAllConflicts().ToList();
    }

    public Conflict SaveConflict(Conflict conflict)
    {
      return ConflictManagement.SaveConflict(conflict);
    }

    public Conflict GetConflict(int idConflict)
    {
      return ConflictManagement.GetConflict(idConflict);
    }
  }
}
