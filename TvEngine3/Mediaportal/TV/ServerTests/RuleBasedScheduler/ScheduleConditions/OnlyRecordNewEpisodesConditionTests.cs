using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class OnlyRecordNewEpisodesConditionTests
  {    
    [Test]
    public void OnlyRecordNewEpisodesConditionTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.EpisodeNum = 1;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.EpisodeNum = 2;
      programs.Add(prg);

            
      IList<int> skipEpisodes = new ObservableCollection<int>();
      skipEpisodes.Add(1);

      var onlyRecordNewEpisodesCondition = new OnlyRecordNewEpisodesCondition(skipEpisodes);

      IQueryable<ProgramDTO> prgsQuery = onlyRecordNewEpisodesCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(2, prgsQuery.FirstOrDefault().IdProgram);            
    }

    [Test]
    public void OnlyRecordNewEpisodesConditionNothingFoundTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.EpisodeNum = 1;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.EpisodeNum = 2;
      programs.Add(prg);


      IList<int> skipEpisodes = new ObservableCollection<int>();
      skipEpisodes.Add(3);

      var onlyRecordNewEpisodesCondition = new OnlyRecordNewEpisodesCondition(skipEpisodes);

      IQueryable<ProgramDTO> prgsQuery = onlyRecordNewEpisodesCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(2, prgsQuery.Count());     
    }
   }
}
