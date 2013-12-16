using System;
using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class ProgramRepository : GenericRepository<Model>, IProgramRepository
  {
    public ProgramRepository()
    {
    }

    public ProgramRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public ProgramRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<Program> GetNowProgramsForChannelGroup(int idGroup)
    {
      IQueryable<int> channels = GetQuery<Channel>(c => c.GroupMaps.Any(g => g.IdGroup == idGroup)).Select(g => g.IdChannel);

      DateTime now = DateTime.Now;
      IQueryable<Program> programs = GetQuery<Program>().Where(p => channels.Contains(p.IdChannel) && p.EndTime > now && p.StartTime <= now);
      return programs;
    }

    public IQueryable<Program> GetNextProgramsForChannelGroup(int idGroup)
    {
      IQueryable<int> channels = GetQuery<Channel>(c => c.GroupMaps.Any(g => g.IdGroup == idGroup)).Select(g => g.IdChannel);
      DateTime now = DateTime.Now;

      var q = GetQuery<Program>().Where(p =>
            channels.Contains(p.IdChannel) && p.StartTime > now)
        .GroupBy(p => p.IdChannel)
        .Select(pg => new
        {
          idChannel = pg.Key,
          minStartTime = pg.Min(p => p.StartTime)
        });
      IQueryable<Program> programs = GetQuery<Program>().Where(p => q.Any(pmin => p.IdChannel == pmin.idChannel && p.StartTime == pmin.minStartTime));

      return programs;
    }

    public IQueryable<Program> GetNowAndNextProgramsForChannel(int idChannel)
    {
      DateTime now = DateTime.Now;
      var programs =
        GetQuery<Program>().Where(p => p.IdChannel == idChannel && p.EndTime >= now).Include(p => p.Channel)
        .Include(p => p.ProgramCategory)
        .Include(p => p.ProgramCredits)
        .OrderBy(p => p.StartTime)
        .Take(2);
      return programs;
    }

    public void DeleteAllProgramsWithChannelId(int idChannel)
    {
      Delete<Program>(p => p.IdChannel == idChannel);
      UnitOfWork.SaveChanges();
    }

    public IQueryable<Program> FindAllProgramsByChannelId(int idChannel)
    {
      var findAllProgramsByChannelId = GetQuery<Program>().Where(p => p.IdChannel == idChannel);
      return findAllProgramsByChannelId;
    }

    public Program GetProgramAt(DateTime date, int idChannel)
    {
      var programAt = GetQuery<Program>().Where(p => p.IdChannel == idChannel && p.EndTime > date && p.StartTime <= date);
      programAt = IncludeAllRelations(programAt).OrderBy(p => p.StartTime);
      return programAt.FirstOrDefault();
    }

    public Program GetProgramAt(DateTime date, string title)
    {
      var programAt = GetQuery<Program>().Where(p => p.Title == title && p.EndTime > date && p.StartTime <= date);
      programAt = IncludeAllRelations(programAt).OrderBy(p => p.StartTime);
      return programAt.FirstOrDefault();
    }

    public IQueryable<Program> GetProgramsByTitle(IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.EndTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.Title.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.Title == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.Title.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.Title.EndsWith(searchCriteria));
        }
      }

      return query.OrderBy(p => p.Title).ThenBy(p => p.StartTime);
    }

    public IQueryable<Program> GetProgramsByCategory(IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.EndTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.ProgramCategory.Category.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.ProgramCategory.Category == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.ProgramCategory.Category.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.ProgramCategory.Category.EndsWith(searchCriteria));
        }
      }
      return query.OrderBy(p => p.Title).ThenBy(p => p.StartTime);
    }

    public IQueryable<Program> GetProgramsByDescription(IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.EndTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.Description.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.Description == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.Description.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.Description.EndsWith(searchCriteria));
        }
      }

      return query.OrderBy(p => p.Description).ThenBy(p => p.StartTime);
    }

    public IQueryable<Program> GetProgramsByTimesInterval(DateTime startTime, DateTime endTime)
    {
      var programsByTimesInterval = GetQuery<Program>().Where(p => p.Channel.VisibleInGuide &&
                                                                      (p.EndTime > startTime && p.EndTime < endTime)
                                                                      || (p.StartTime >= startTime && p.StartTime <= endTime)
                                                                      || (p.StartTime <= startTime && p.EndTime >= endTime)
                                                                    ).OrderBy(p => p.StartTime)
                                                                    .Include(p => p.ProgramCategory).Include(p => p.Channel);
      return programsByTimesInterval;
    }

    public IQueryable<Program> GetProgramsByStartEndTimes(DateTime startTime, DateTime endTime)
    {
      var query = GetQuery<Program>(p => p.Channel.VisibleInGuide && p.StartTime < endTime && p.EndTime > startTime);
      return query;
    }

    public IQueryable<Program> IncludeAllRelations(IQueryable<Program> query)
    {
      var includeRelations = query.Include(p => p.ProgramCategory).Include(p => p.Channel);
      return includeRelations;
    }
  }
}
