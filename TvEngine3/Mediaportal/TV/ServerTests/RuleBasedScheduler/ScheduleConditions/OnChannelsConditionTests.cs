using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class OnChannelsConditionTests
  {    
    
    [Test]
    public void OnChannelsConditionTest()
    {
      ChannelDTO ch1 = new ChannelDTO();
      ch1.Name = "ch1";
      ch1.IdChannel = 1;

      ChannelDTO ch2 = new ChannelDTO();
      ch2.Name = "ch2";
      ch2.IdChannel = 2;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.ReferencedChannel = ch1;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.ReferencedChannel = ch2;
      programs.Add(prg);

      IList<ChannelDTO> channels = new ObservableCollection<ChannelDTO>();
      channels.Add(ch1);
      var onChannelsCondition = new OnChannelsCondition(channels);

      IQueryable<ProgramDTO> prgsQuery = onChannelsCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);            
    }

    [Test]
    public void OnChannelsConditionNothingFoundTest()
    {
      ChannelDTO ch1 = new ChannelDTO();
      ch1.Name = "ch1";
      ch1.IdChannel = 1;

      ChannelDTO ch2 = new ChannelDTO();
      ch2.Name = "ch2";
      ch2.IdChannel = 2;

      ChannelDTO ch3 = new ChannelDTO();
      ch2.Name = "ch3";
      ch2.IdChannel = 3;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.ReferencedChannel = ch1;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.ReferencedChannel = ch2;
      programs.Add(prg);

      IList<ChannelDTO> channels = new ObservableCollection<ChannelDTO>();
      channels.Add(ch3);
      var onChannelsCondition = new OnChannelsCondition(channels);

      IQueryable<ProgramDTO> prgsQuery = onChannelsCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    }
   }
}
