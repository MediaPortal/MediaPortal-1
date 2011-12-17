using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface ICardRepository : IRepository<Model>
  {
    IQueryable<Card> IncludeAllRelations(IQueryable<Card> query);
  }
}