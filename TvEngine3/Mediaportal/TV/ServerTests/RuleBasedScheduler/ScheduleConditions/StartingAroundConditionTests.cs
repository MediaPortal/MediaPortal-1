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
  public class StartingAroundConditionTests
  {    
    [Test]
    public void StartingAroundConditionTest()
    {
      DateTime date = DateTime.Now;     

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();      
      prg.IdProgram = 1;
      prg.StartTime = date;
      programs.Add(prg);

      prg = new ProgramDTO();      
      prg.IdProgram = 2;
      prg.StartTime = date.AddDays(1);
      programs.Add(prg);

      var startingAroundCondition = new StartingAroundCondition(date, 10);
      IQueryable<ProgramDTO> prgsQuery = startingAroundCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);
    }

    [Test]
    public void StartingAroundConditionNothingFoundTest()
    {
      DateTime date = DateTime.Now;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.StartTime = date;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.StartTime = date.AddDays(1);
      programs.Add(prg);

      var startingAroundCondition = new StartingAroundCondition(date.AddMinutes(-15), 10);
      IQueryable<ProgramDTO> prgsQuery = startingAroundCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());      
    }
   }
}
