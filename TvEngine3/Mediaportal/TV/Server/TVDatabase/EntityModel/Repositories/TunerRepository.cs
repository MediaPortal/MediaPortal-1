using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class TunerRepository : GenericRepository<Model>, ITunerRepository
  {
    public TunerRepository()
    {
    }

    public TunerRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public TunerRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<Tuner> IncludeAllRelations(IQueryable<Tuner> query)
    {
      IQueryable<Tuner> includeRelations =
        query.
          Include(t => t.ChannelMaps).
          Include(t => t.ChannelMaps.Select(m => m.Channel).Select(c => c.TuningDetails)).
          Include(t => t.TunerGroup).
          Include(t => t.DiseqcMotors).
          Include(t => t.TunerProperties).
          Include(t => t.AnalogTunerSettings).
          Include(t => t.AnalogTunerSettings.VideoEncoder).
          Include(t => t.AnalogTunerSettings.AudioEncoder);
      ;
      return includeRelations;
    }

    public IQueryable<Tuner> IncludeAllRelations(IQueryable<Tuner> query, TunerIncludeRelationEnum includeRelations)
    {
      bool tunerGroup = includeRelations.HasFlag(TunerIncludeRelationEnum.TunerGroup);
      bool channelMaps = includeRelations.HasFlag(TunerIncludeRelationEnum.ChannelMaps);
      bool channelMapsChannelTuningDetails = includeRelations.HasFlag(TunerIncludeRelationEnum.ChannelMapsChannelTuningDetails);
      bool diseqcMotors = includeRelations.HasFlag(TunerIncludeRelationEnum.DiseqcMotors);
      bool tunerProperties = includeRelations.HasFlag(TunerIncludeRelationEnum.TunerProperties);
      bool analogTunerSettings = includeRelations.HasFlag(TunerIncludeRelationEnum.AnalogTunerSettings);

      if (tunerGroup)
      {
        query = query.Include(t => t.TunerGroup);
      }
      if (channelMaps)
      {
        query = query.Include(t => t.ChannelMaps);
      }
      if (channelMapsChannelTuningDetails)
      {
        query = query.Include(t => t.ChannelMaps.Select(m => m.Channel).Select(c => c.TuningDetails));
      }
      if (diseqcMotors)
      {
        query = query.Include(t => t.DiseqcMotors);
      }
      if (tunerProperties)
      {
        query = query.Include(t => t.TunerProperties);
      }
      if (analogTunerSettings)
      {
        query = query.Include(t => t.AnalogTunerSettings).
                      Include(t => t.AnalogTunerSettings.VideoEncoder).
                      Include(t => t.AnalogTunerSettings.AudioEncoder);
      }
      return query;
    }
  }
}