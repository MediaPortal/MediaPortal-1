using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class ScheduleRepository : GenericRepository<Model>, IScheduleRepository
  {
    public ScheduleRepository()
    {
    }

    public ScheduleRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public ScheduleRepository(Model context)
      : base(context)
    {
    }
    public IQueryable<Schedule> IncludeAllRelations(IQueryable<Schedule> query)
    {
      IQueryable<Schedule> includeRelations = query.Include(s => s.Channel)
        .Include(s => s.Channel.TuningDetails)
        .Include(s => s.Recordings)
        .Include(s => s.Schedules)
        .Include(s => s.ConflictingSchedules)
        .Include(s => s.Conflicts)
        .Include(s => s.ParentSchedule)
        .Include(s => s.Channel);

      return includeRelations;
    }

    public IQueryable<Schedule> IncludeAllRelations(IQueryable<Schedule> query, ScheduleIncludeRelationEnum includeRelations)
    {
      bool channel = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Channel);
      bool channelTuningDetails = includeRelations.HasFlag(ScheduleIncludeRelationEnum.ChannelTuningDetails);
      bool conflictingSchedules = includeRelations.HasFlag(ScheduleIncludeRelationEnum.ConflictingSchedules);
      bool conflicts = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Conflicts);
      bool parentSchedule = includeRelations.HasFlag(ScheduleIncludeRelationEnum.ParentSchedule);
      bool recordings = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Recordings);
      bool schedules = includeRelations.HasFlag(ScheduleIncludeRelationEnum.Schedules);

      if (channel)
      {
        query = query.Include(s => s.Channel);
      }

      if (channelTuningDetails)
      {
        query = query.Include(s => s.Channel.TuningDetails);
      }

      if (conflictingSchedules)
      {
        query = query.Include(s => s.ConflictingSchedules);
      }

      if (conflicts)
      {
        query = query.Include(s => s.Conflicts);
      }

      if (parentSchedule)
      {
        query = query.Include(s => s.ParentSchedule);
      }

      if (recordings)
      {
        query = query.Include(s => s.Recordings);
      }

      if (schedules)
      {
        query = query.Include(s => s.Schedules);
      }

      return query;
    }
  }
}
