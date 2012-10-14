using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface ICardRepository : IRepository<Model>
  {
    IQueryable<Card> IncludeAllRelations(IQueryable<Card> query);
    IQueryable<Card> IncludeAllRelations(IQueryable<Card> query, CardIncludeRelationEnum includeRelations);
  }
}