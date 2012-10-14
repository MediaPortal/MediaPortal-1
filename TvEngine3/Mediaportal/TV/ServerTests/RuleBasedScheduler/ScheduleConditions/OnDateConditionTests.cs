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
  public class OnDateConditionTests
  {    
    
    [Test]
    public void OnDateConditionTest()
    {
      DateTime onDate = DateTime.Now;     

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.StartTime = onDate;
      programs.Add(prg);
     

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.StartTime = onDate.AddDays(1);
      programs.Add(prg);      
      
      var dateCondition = new OnDateCondition(onDate);

      IQueryable<ProgramDTO> prgsQuery = dateCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);            
    }

    [Test]
    public void OnDateConditionNothingFoundTest()
    {
      DateTime onDate = DateTime.Now;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.StartTime = onDate.AddDays(2);
      programs.Add(prg);


      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.StartTime = onDate.AddDays(1);
      programs.Add(prg);

      var dateCondition = new OnDateCondition(onDate);

      IQueryable<ProgramDTO> prgsQuery = dateCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    }
   }
}
