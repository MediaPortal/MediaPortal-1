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
  public class SkipRepeatsConditionTests
  {    
    [Test]
    public void SkipRepeatsConditionTest()
    {
      DateTime date = DateTime.Now;     

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.PreviouslyShown = false;
      prg.IdProgram = 1;
      prg.StartTime = date;
      programs.Add(prg);
     

      prg = new ProgramDTO();
      prg.PreviouslyShown = true;
      prg.IdProgram = 2;
      prg.StartTime = date;
      programs.Add(prg);

      var skipRepeatsCondition = new SkipRepeatsCondition();
      IQueryable<ProgramDTO> prgsQuery = skipRepeatsCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);
    }

    [Test]
    public void SkipRepeatsConditionNothingFoundTest()
    {
      DateTime date = DateTime.Now;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.PreviouslyShown = false;
      prg.IdProgram = 1;
      prg.StartTime = date;
      programs.Add(prg);


      prg = new ProgramDTO();
      prg.PreviouslyShown = false;
      prg.IdProgram = 2;
      prg.StartTime = date;
      programs.Add(prg);

      var skipRepeatsCondition = new SkipRepeatsCondition();
      IQueryable<ProgramDTO> prgsQuery = skipRepeatsCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(2, prgsQuery.Count());      
    }
   }
}
