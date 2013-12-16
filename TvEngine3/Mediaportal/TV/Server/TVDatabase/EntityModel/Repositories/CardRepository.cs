using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class CardRepository : GenericRepository<Model>, ICardRepository
  {
    public CardRepository()
    {
    }

    public CardRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public CardRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<Card> IncludeAllRelations(IQueryable<Card> query)
    {
      IQueryable<Card> includeRelations =
        query.
          Include(c => c.ChannelMaps.Select(m => m.Channel).Select(ch => ch.TuningDetails)).
          Include(c => c.ChannelMaps).
          Include(c => c.CardGroupMaps).
          Include(c => c.DisEqcMotors);
      return includeRelations;

    }

    public IQueryable<Card> IncludeAllRelations(IQueryable<Card> query, CardIncludeRelationEnum includeRelations)
    {
      bool cardGroupMaps = includeRelations.HasFlag(CardIncludeRelationEnum.CardGroupMaps);
      bool channelMaps = includeRelations.HasFlag(CardIncludeRelationEnum.ChannelMaps);
      bool channelMapsChannelTuningDetails = includeRelations.HasFlag(CardIncludeRelationEnum.ChannelMapsChannelTuningDetails);
      bool disEqcMotors = includeRelations.HasFlag(CardIncludeRelationEnum.DisEqcMotors);

      if (cardGroupMaps)
      {
        query = query.Include(c => c.CardGroupMaps);
      }
      if (channelMaps)
      {
        query = query.Include(c => c.ChannelMaps);
      }
      if (channelMapsChannelTuningDetails)
      {
        query = query.Include(c => c.ChannelMaps.Select(m => m.Channel).Select(ch => ch.TuningDetails));
      }
      if (disEqcMotors)
      {
        query = query.Include(c => c.DisEqcMotors);
      }
      return query;
    }
  }
}
