using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
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
      ;       
       
      return includeRelations;
    }
  
  }  
}
