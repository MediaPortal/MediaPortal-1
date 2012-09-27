using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Extensions;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;

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
      IQueryable<int> channels = GetQuery<Channel>(c => c.GroupMaps.Any(g => g.IdGroup == idGroup)).Select(g=>g.IdChannel);

      DateTime now = DateTime.Now;
      IQueryable<Program> programs = GetQuery<Program>().Where(p =>
                      channels.Contains(p.idChannel) &&
                      p.endTime > now && p.startTime <= now);
      return programs;
    }

    public IQueryable<Program> GetNextProgramsForChannelGroup(int idGroup)
    {
      IQueryable<int> channels = GetQuery<Channel>(c => c.GroupMaps.Any(g => g.IdGroup == idGroup)).Select(g => g.IdChannel);
      DateTime now = DateTime.Now;      

      var q = GetQuery<Program>().Where(p =>
            channels.Contains(p.idChannel) && p.startTime > now)
        .GroupBy(p => p.idChannel)
        .Select(pg => new
        {
          idChannel = pg.Key,
          minStartTime = pg.Min(p => p.startTime)
        });
      IQueryable<Program> programs =
              GetQuery<Program>().Where(p =>
                  q.Any(pmin =>
                    p.idChannel == pmin.idChannel &&
                    p.startTime == pmin.minStartTime));

      return programs;
    }

    public IQueryable<Program> GetNowAndNextProgramsForChannel(int idChannel)
    {
      DateTime now = DateTime.Now;
      var programs =
        GetQuery<Program>().Where(p => p.idChannel == idChannel && p.endTime >= now).Include(p => p.Channel)
        .Include(p => p.ProgramCategory)
        .Include(p => p.ProgramCredits)
        .OrderBy(p => p.startTime)
        .Take(2);
      return programs;
    }

    public void DeleteAllProgramsWithChannelId(int idChannel)
    {
      Delete<Program>(p => p.idChannel == idChannel);
      UnitOfWork.SaveChanges();
    }

    public IQueryable<Program> FindAllProgramsByChannelId(int idChannel)
    {
      var findAllProgramsByChannelId = GetQuery<Program>().Where(p => p.idChannel == idChannel);
      return findAllProgramsByChannelId;
    }

    public Program GetProgramAt(DateTime date, int idChannel)
    {
      var programAt = GetQuery<Program>().Where(p => p.idChannel == idChannel && p.endTime > date && p.startTime <= date);
      programAt = IncludeAllRelations(programAt).OrderBy(p => p.startTime);
      return programAt.FirstOrDefault();
    }

    public Program GetProgramAt(DateTime date, string title)
    {
      var programAt = GetQuery<Program>().Where(p => p.title == title && p.endTime > date && p.startTime <= date);
      programAt = IncludeAllRelations(programAt).OrderBy(p => p.startTime);
      return programAt.FirstOrDefault();
    }

    public IQueryable<Program> GetProgramsByTitle(IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.endTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.title.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.title == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.title.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.title.EndsWith(searchCriteria));
        }
      }

      return query.OrderBy(p => p.title).OrderBy(p => p.startTime);
    }

    public IQueryable<Program> GetProgramsByCategory(IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.endTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.ProgramCategory.category.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.ProgramCategory.category == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.ProgramCategory.category.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.ProgramCategory.category.EndsWith(searchCriteria));
        }
      }

      return query.OrderBy(p => p.title).OrderBy(p => p.startTime);
    }

    public IQueryable<Program> GetProgramsByDescription(IQueryable<Program> query, string searchCriteria, StringComparisonEnum stringComparison)
    {
      DateTime now = DateTime.Now;
      query = query.Where(p => p.Channel.VisibleInGuide && p.endTime > now);

      if (!string.IsNullOrEmpty(searchCriteria))
      {
        bool startsWith = (stringComparison.HasFlag(StringComparisonEnum.StartsWith));
        bool endsWith = (stringComparison.HasFlag(StringComparisonEnum.EndsWith));

        if (startsWith && endsWith)
        {
          query = query.Where(p => p.description.Contains(searchCriteria));
        }
        else if (!startsWith && !endsWith)
        {
          query = query.Where(p => p.description == searchCriteria);
        }
        else if (startsWith)
        {
          query = query.Where(p => p.description.StartsWith(searchCriteria));
        }
        else
        {
          query = query.Where(p => p.description.EndsWith(searchCriteria));
        }
      }

      return query.OrderBy(p => p.description).OrderBy(p => p.startTime);
    }

    public IQueryable<Program> GetProgramsByTimesInterval(DateTime startTime, DateTime endTime)
    {
      var programsByTimesInterval = GetQuery<Program>().Where(p => p.Channel.VisibleInGuide &&
                                                                      (p.endTime > startTime && p.endTime < endTime)
                                                                      || (p.startTime >= startTime && p.startTime <= endTime)
                                                                      || (p.startTime <= startTime && p.endTime >= endTime)
                                                                    ).OrderBy(p => p.startTime)
                                                                    .Include(p => p.ProgramCategory).Include(p => p.Channel);
      return programsByTimesInterval;
    }



    public IQueryable<Program> GetProgramsByStartEndTimes(DateTime startTime, DateTime endTime)
    {      
      var query = GetQuery<Program>(p => p.Channel.VisibleInGuide && p.startTime < endTime && p.endTime > startTime);
      return query;
    }

    public IQueryable<Program> IncludeAllRelations(IQueryable<Program> query)
    {
      var includeRelations = query.Include(p => p.ProgramCategory)
        .Include(p => p.Channel);

      return includeRelations;
    }

  }
}
