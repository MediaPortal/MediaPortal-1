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
  public class StartingBetweenConditionTests
  {    
    [Test]
    public void StartingBetweenConditionTest()
    {
      DateTime date = DateTime.Now;     

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.StartTime = date;
      programs.Add(prg);
     

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.StartTime = date.AddHours(3);
      programs.Add(prg);

      DateTime startTimeInterval = date;
      DateTime endTimeInterval = date.AddHours(2);
      var startingBetweenCondition = new StartingBetweenCondition(startTimeInterval, endTimeInterval);
      IQueryable<ProgramDTO> prgsQuery = startingBetweenCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);
    }

    [Test]
    public void StartingBetweenConditionNothingFoundTest()
    {
      DateTime date = DateTime.Now;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.StartTime = date;
      programs.Add(prg);


      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.StartTime = date.AddHours(3);
      programs.Add(prg);

      DateTime startTimeInterval = date.AddHours(4);
      DateTime endTimeInterval = date.AddHours(5);
      var startingBetweenCondition = new StartingBetweenCondition(startTimeInterval, endTimeInterval);
      IQueryable<ProgramDTO> prgsQuery = startingBetweenCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    }
   }
}
