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
        .Include(s => s.Channel)
        .Include(s => s.CanceledSchedules);
      return includeRelations;
    }

    public IQueryable<Schedule> IncludeAllRelations(IQueryable<Schedule> query, ScheduleRelation includeRelations)
    {
      if (includeRelations.HasFlag(ScheduleRelation.Channel))
      {
        query = query.Include(s => s.Channel);
      }
      if (includeRelations.HasFlag(ScheduleRelation.ChannelTuningDetails))
      {
        query = query.Include(s => s.Channel.TuningDetails);
      }
      if (includeRelations.HasFlag(ScheduleRelation.ConflictingSchedules))
      {
        query = query.Include(s => s.ConflictingSchedules);
      }
      if (includeRelations.HasFlag(ScheduleRelation.Conflicts))
      {
        query = query.Include(s => s.Conflicts);
      }
      if (includeRelations.HasFlag(ScheduleRelation.ParentSchedule))
      {
        query = query.Include(s => s.ParentSchedule);
      }
      if (includeRelations.HasFlag(ScheduleRelation.Recordings))
      {
        query = query.Include(s => s.Recordings);
      }
      if (includeRelations.HasFlag(ScheduleRelation.Schedules))
      {
        query = query.Include(s => s.Schedules);
      }
      if (includeRelations.HasFlag(ScheduleRelation.CanceledSchedules))
      {
        query = query.Include(s => s.CanceledSchedules);
      }
      return query;
    }
  }
}