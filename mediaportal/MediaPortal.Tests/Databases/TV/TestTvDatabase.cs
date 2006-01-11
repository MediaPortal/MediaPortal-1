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
    public void GetChannels()
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
  }
}
