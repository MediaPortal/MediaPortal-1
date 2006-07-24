using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.TV.Database;

namespace MediaPortal.Tests.Databases.TV
{
  [TestFixture]
  public class TestTvDatabase
  {
    DateTime _startTime;

    [SetUp]
    public void Init()
    {
      _startTime = DateTime.Now ;
      _startTime = new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, _startTime.Hour, _startTime.Minute, 0);

      TVChannel ch;
      TVDatabase.ClearAll();

      // add 3 channels
      ch = new TVChannel("RTL 4"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 5"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("SBS 6"); TVDatabase.AddChannel(ch);

      //add some tv programs
      TVProgram prog;
      prog = new TVProgram("RTL 4", _startTime.AddHours(-4), _startTime.AddHours(-2), "program 1"); TVDatabase.AddProgram(prog);
      prog = new TVProgram("RTL 4", _startTime.AddHours(-2), _startTime.AddHours(1), "program 2"); TVDatabase.AddProgram(prog);
      prog = new TVProgram("RTL 4", _startTime.AddHours(1), _startTime.AddHours(3), "program 3"); TVDatabase.AddProgram(prog);

      prog = new TVProgram("RTL 5", _startTime.AddHours(-6), _startTime.AddHours(3), "show 1"); TVDatabase.AddProgram(prog);

      prog = new TVProgram("SBS 6", _startTime.AddHours(-6), _startTime.AddHours(-5), "eps 1"); TVDatabase.AddProgram(prog);
      prog = new TVProgram("SBS 6", _startTime.AddHours(-5), _startTime.AddHours(1), "eps 2"); TVDatabase.AddProgram(prog);
      prog = new TVProgram("SBS 6", _startTime.AddHours(-1), _startTime.AddHours(1), "eps 3"); TVDatabase.AddProgram(prog);
      prog = new TVProgram("SBS 6", _startTime.AddHours(1), _startTime.AddHours(3), "eps 4"); TVDatabase.AddProgram(prog);
 
    }
    
    [Test]
    public void TestGetChannels()
    {
      List<TVChannel> listChannels = new List<TVChannel>();
      TVDatabase.GetChannels(ref listChannels);
      Assert.AreEqual(3, listChannels.Count);
      Assert.AreEqual(listChannels[0].Name, "RTL 4");
      Assert.AreEqual(listChannels[1].Name, "RTL 5");
      Assert.AreEqual(listChannels[2].Name, "SBS 6");
    }
    
    [Test]
    public void GetProgramByTime()
    {
      TVProgram prog = TVDatabase.GetProgramByTime("RTL 4", _startTime);
      Assert.IsNotNull(prog);
      Assert.AreEqual(prog.Channel, "RTL 4");
      Assert.AreEqual(prog.Title, "program 2");
      Assert.AreEqual(_startTime.AddHours(-2), prog.StartTime);
      Assert.AreEqual(_startTime.AddHours(1), prog.EndTime);

      prog = TVDatabase.GetProgramByTime("RTL 4", _startTime.AddHours(1));
      Assert.IsNotNull(prog);
      Assert.AreEqual(prog.Channel , "RTL 4");
      Assert.AreEqual(prog.Title, "program 3");
      Assert.AreEqual(_startTime.AddHours(1), prog.StartTime);
      Assert.AreEqual(_startTime.AddHours(3), prog.EndTime);
    }

    [Test]
    public void Genres()
    {
      TVDatabase.AddGenre("movie");
      TVDatabase.AddGenre("soap");
      TVDatabase.AddGenre("news");

      List<string> genres = new List<string>();
      TVDatabase.GetGenres(ref genres);
      Assert.AreEqual(genres.Count, 4);
      Assert.AreEqual(genres[0], "unknown");
      Assert.AreEqual(genres[1], "movie");
      Assert.AreEqual(genres[2], "soap");
      Assert.AreEqual(genres[3], "news");
    }
    [Test]
    public void TestQuoteInGenre()
    {
      TVDatabase.AddGenre("mo'v'ie");

      List<string> genres = new List<string>();
      TVDatabase.GetGenres(ref genres);
      Assert.AreEqual(genres.Count, 2);
      Assert.AreEqual(genres[0], "unknown");
      Assert.AreEqual(genres[1], "mo'v'ie");
    }
    [Test]
    public void TestAddChannel()
    {
      TVChannel chan = new TVChannel();
      chan.Name = "bla";
      chan.Number = 55;
      chan.Frequency = 123456000;
      chan.External = true;
      chan.ExternalTunerChannel = "external";
      chan.AutoGrabEpg = true;
      chan.EpgHours = 12;
      chan.Country = 22;
      chan.LastDateTimeEpgGrabbed = new DateTime(2006, 1, 2, 20, 21, 22, 0);
      chan.TVStandard = DirectShowLib.AnalogVideoStandard.PAL_60;
      chan.Scrambled = true;
      chan.VisibleInGuide = true;
      chan.Sort = 123;
      chan.ID= TVDatabase.AddChannel(chan);

      TVChannel chanReturn = TVDatabase.GetChannelById(chan.ID);
      Assert.IsNotNull(chanReturn);
      Assert.AreEqual(chan, chanReturn);
    }

    [Test]
    public void TestQuotesInChannel()
    {
      TVChannel ch = new TVChannel("te'st'");
      ch.ExternalTunerChannel = "exte'r'nal";
      ch.XMLId = "xml'i''d'";
      int channelId=TVDatabase.AddChannel(ch);

      TVChannel addedChannel = TVDatabase.GetChannelById(channelId);
      Assert.AreEqual(addedChannel.Name, ch.Name);
      Assert.AreEqual(addedChannel.ExternalTunerChannel, ch.ExternalTunerChannel);
    }

    [Test]
    public void DontAddIdenticalGenres()
    {
      TVDatabase.AddGenre("movie");
      TVDatabase.AddGenre("movie");
      TVDatabase.AddGenre("movie");

      List<string> genres = new List<string>();
      TVDatabase.GetGenres(ref genres);
      Assert.AreEqual(genres.Count, 2);
    }

    [Test]
    public void TestUpdateChannel()
    {
      List<TVChannel> listChannels = new List<TVChannel>();
      TVDatabase.GetChannels(ref listChannels);
      TVChannel chan=listChannels[0];
      chan.Name = "bla";
      chan.Number = 55;
      chan.Frequency = 123456000;
      chan.External = true;
      chan.ExternalTunerChannel = "external";
      chan.AutoGrabEpg = true;
      chan.EpgHours = 12;
      chan.Country = 22;
      chan.LastDateTimeEpgGrabbed = new DateTime(2006, 1, 2, 20, 21, 22,0);
      chan.TVStandard = DirectShowLib.AnalogVideoStandard.PAL_60;
      chan.Scrambled = true;
      chan.VisibleInGuide = true;
      chan.Sort = 123;
      TVDatabase.UpdateChannel(chan, chan.Sort);

      TVChannel chanReturn = TVDatabase.GetChannelById(chan.ID);
      Assert.IsNotNull(chanReturn);
      Assert.AreEqual(chan, chanReturn);

    }
  }
}
