using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class OnDayConditionTests
  {    
    
    [Test]
    public void OnDayConditionTest()
    {
      DateTime date = DateTime.Now;     

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      date = SetDayOfWeek(date, DayOfWeek.Monday);      
      prg.StartTime = date;
      programs.Add(prg);
     

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      date = SetDayOfWeek(date, DayOfWeek.Saturday);      
      prg.StartTime = date;
      programs.Add(prg);

      IList<DayOfWeek> ondays = new ObservableCollection<DayOfWeek>();

      ondays.Add(DayOfWeek.Monday);
      var dateCondition = new OnDayCondition(ondays);

      IQueryable<ProgramDTO> prgsQuery = dateCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);
    }

    private static DateTime SetDayOfWeek(DateTime date, DayOfWeek dayOfWeek)
    {
      while (date.DayOfWeek != dayOfWeek)
      {
        date = date.AddDays(1);
      }
      return date;
    }

    [Test]
    public void OnDayConditionNothingFoundTest()
    {
      DateTime date = DateTime.Now;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      date = SetDayOfWeek(date, DayOfWeek.Monday);
      prg.StartTime = date;
      programs.Add(prg);


      prg = new ProgramDTO();
      prg.IdProgram = 2;
      date = SetDayOfWeek(date, DayOfWeek.Saturday);
      prg.StartTime = date;
      programs.Add(prg);

      IList<DayOfWeek> ondays = new ObservableCollection<DayOfWeek>();

      ondays.Add(DayOfWeek.Tuesday);
      var dateCondition = new OnDayCondition(ondays);

      IQueryable<ProgramDTO> prgsQuery = dateCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    }
   }
}
