using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class OnlyRecordNewTitlesConditionTests
  {
    [Test]
    public void OnlyRecordNewTitlesConditionTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.Title = "MonkeY";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.Title = "CAt";
      programs.Add(prg);


      IList<string> skipTitles = new ObservableCollection<string>();
      skipTitles.Add("cAt");

      var recordNewTitlesCondition = new OnlyRecordNewTitlesCondition(skipTitles);

      IQueryable<ProgramDTO> prgsQuery = recordNewTitlesCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);
    }

    [Test]
    public void OnlyRecordNewTitlesConditionNothingFoundTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.Title = "MonkeY";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.Title = "CAt";
      programs.Add(prg);


      IList<string> skipTitles = new ObservableCollection<string>();
      skipTitles.Add("nothing");

      var recordNewTitlesCondition = new OnlyRecordNewTitlesCondition(skipTitles);

      IQueryable<ProgramDTO> prgsQuery = recordNewTitlesCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(2, prgsQuery.Count());
    }

   }
}
