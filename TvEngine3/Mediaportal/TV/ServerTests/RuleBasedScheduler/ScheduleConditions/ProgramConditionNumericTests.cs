using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class ProgramConditionNumericTests
  {

    #region numeric based tests

    #region equals tests
    [Test]
    public void ProgramConditionNumericEqualsTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.EpisodeNum = 1;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.EpisodeNum = 2;
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<int>("EpisodeNum", 1, ConditionOperator.Equals);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().EpisodeNum);            
    }

    [Test]
    public void ProgramConditionNumericEqualsNothingFoundTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.EpisodeNum = 2;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.EpisodeNum = 3;
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<int>("EpisodeNum", 1, ConditionOperator.Equals);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());

    }

    #endregion

    #endregion
  }
}
