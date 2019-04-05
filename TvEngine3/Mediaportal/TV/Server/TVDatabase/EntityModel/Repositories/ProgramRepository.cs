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

    public IQueryable<Program> GetNowProgramsForChannelGroup(int idChannelGroup)
    {
      IQueryable<int> channels = GetQuery<Channel>(c => c.ChannelGroupMappings.Any(m => m.IdChannelGroup == idChannelGroup)).Select(m => m.IdChannel);

      DateTime now = DateTime.Now;
      return GetQuery<Program>().Where(p => channels.Contains(p.IdChannel) && p.EndTime > now && p.StartTime <= now);
    }

    public IQueryable<Program> GetNextProgramsForChannelGroup(int idChannelGroup)
    {
      IQueryable<int> channels = GetQuery<Channel>(c => c.ChannelGroupMappings.Any(m => m.IdChannelGroup == idChannelGroup)).Select(m => m.IdChannel);
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
      var programs = GetQuery<Program>().Where(p => p.IdChannel == idChannel && p.EndTime >= now).OrderBy(p => p.StartTime).Take(2);
      return IncludeAllRelations(programs);
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
          if (searchCriteria == "[0-9]")
          {
            query = query.Where(p => p.Title.StartsWith("0") || p.Title.StartsWith("1") || p.Title.StartsWith("2") || p.Title.StartsWith("3") || p.Title.StartsWith("4")
                                || p.Title.StartsWith("5") || p.Title.StartsWith("6") || p.Title.StartsWith("7") || p.Title.StartsWith("8") || p.Title.StartsWith("9"));
          }
          else
          {
            query = query.Where(p => p.Title.StartsWith(searchCriteria));
          }
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
                                                                    ).OrderBy(p => p.StartTime);
      return IncludeAllRelations(programsByTimesInterval);
    }

    public IQueryable<Program> GetProgramsByStartEndTimes(DateTime startTime, DateTime endTime)
    {
      return GetQuery<Program>(p => p.Channel.VisibleInGuide && p.StartTime < endTime && p.EndTime > startTime);
    }

    public IQueryable<Program> IncludeAllRelations(IQueryable<Program> query)
    {
      return query.Include(p => p.ProgramCategory).Include(p => p.Channel).Include(p => p.ProgramCredits);
    }
  }
}
