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
  public class ProgramConditionDateTests
  {

    #region date based tests

    #region equals tests
    [Test]
    public void ProgramConditionDateEqualsTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      DateTime now = DateTime.Now;
      ProgramDTO prg = new ProgramDTO();
      prg.StartTime = now;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.StartTime = now.AddDays(1);
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<DateTime>("StartTime", now, ConditionOperator.Equals);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(now, prgsQuery.FirstOrDefault().StartTime);            
    }

    [Test]
    public void ProgramConditionDateEqualsNothingFoundTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      DateTime now = DateTime.Now;
      ProgramDTO prg = new ProgramDTO();
      prg.StartTime = now.AddDays(1);
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.StartTime = now.AddDays(2);
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<DateTime>("StartTime", now, ConditionOperator.Equals);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());

    }

    #endregion

    #endregion
  }
}
