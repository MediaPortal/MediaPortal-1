using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ProcessPlugins.DiskSpace;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.Tests.TVCapture.Commands
{
  [TestFixture]
  public class TestMultiTunerStrategy
  {
    #region constants
    const int HIGHEST = 1;
    const int MEDIUM = 0;
    const int LOWEST = 2;
    #endregion

    CommandProcessor _processor;
    TVCaptureDevice[] _cards;
    [SetUp]
    public void Init()
    {
      TVChannel ch;
      TVDatabase.ClearAll();

      // add 3 channels
      ch = new TVChannel("RTL 4"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 5"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("SBS 6"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 7"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("MTV"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("CNN"); TVDatabase.AddChannel(ch);
      List<TVChannel> channelList = new List<TVChannel>();
      TVDatabase.GetChannels(ref channelList);
      foreach (TVChannel chan in channelList)
      {
        TVDatabase.MapChannelToCard(chan.ID, 1);
        TVDatabase.MapChannelToCard(chan.ID, 2);
        TVDatabase.MapChannelToCard(chan.ID, 3);
      }

      //add 3 cards
      _processor = new CommandProcessor();
      _cards = new TVCaptureDevice[3];
      _cards[0] = _processor.TVCards.AddDummyCard("dummy2");
      _cards[1] = _processor.TVCards.AddDummyCard("dummy3");
      _cards[2] = _processor.TVCards.AddDummyCard("dummy4");

      //setup priority for all cards from low-hi (2->0->1)
      _cards[HIGHEST].Priority = 10; //highest
      _cards[MEDIUM].Priority = 5;
      _cards[LOWEST].Priority=1;

      //setup recording paths for all cards
      _cards[HIGHEST].RecordingPath = "e:";
      _cards[MEDIUM].RecordingPath = "d:";
      _cards[LOWEST].RecordingPath = "c:";
    }
    [Test]
    public void RecordOn2Tuners()
    {
      TVRecording rec = AddSchedule("RTL 4", DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(10));
      DoSchedule();
      VerifyRecord(HIGHEST, rec);

      TVRecording rec2 = AddSchedule("RTL 5", DateTime.Now.AddMinutes(-22), DateTime.Now.AddMinutes(33));
      DoSchedule();
      VerifyRecord(MEDIUM, rec2);
    }

    [Test]
    public void RecordOn3Tuners()
    {
      TVRecording rec=AddSchedule("RTL 4", DateTime.Now.AddMinutes(-10), DateTime.Now.AddMinutes(10));
      DoSchedule();
      VerifyRecord(HIGHEST, rec);

      TVRecording rec2 = AddSchedule("RTL 5", DateTime.Now.AddMinutes(-22), DateTime.Now.AddMinutes(33));
      DoSchedule();
      VerifyRecord(MEDIUM, rec2);

      TVRecording rec3 = AddSchedule("SBS 6", DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1));
      DoSchedule();
      VerifyRecord(LOWEST, rec3);
    }

    [Test]
    public void RecordOn3TunersZap()
    {
      RecordOn3Tuners();
      Zap("RTL 4");
      VerifyTimeShift(HIGHEST);
      Zap("RTL 5");
      VerifyTimeShift(MEDIUM);
      Zap("SBS 6");
      VerifyTimeShift(LOWEST);
    }

    [Test]
    public void TestNoFreeCard()
    {
      RecordOn3Tuners();
      Zap("SBS 6");

      Zap("CNN");
      VerifyTimeShift(LOWEST);
    }

    [Test]
    public void Record2ZapOnThird()
    {
      RecordOn2Tuners();
      Zap("SBS 6");
      VerifyTimeShift(LOWEST);
      Zap("MTV");
      VerifyTimeShift(LOWEST);
      Zap("CNN");
      VerifyTimeShift(LOWEST);
      Zap("RTL 4");
      VerifyTimeShift(HIGHEST);
      Zap("RTL 5");
      VerifyTimeShift(MEDIUM);
    }

    [Test]
    public void ZapAfterRecordingStopped()
    {
      RecordOn3Tuners();
      Zap("SBS 6");
      
      StopRecord();
      VerifyNotRecording(LOWEST);

      Zap("CNN");
      VerifyTimeShift(LOWEST);
    }

    #region helper functions
    void VerifyTimeShift(int card)
    {
      Assert.IsTrue(_cards[card].IsTimeShifting);
      Assert.AreEqual(_processor.CurrentCardIndex ,card);
      Assert.AreEqual(_processor.TVChannelName ,_cards[card].TVChannel);
    }

    void VerifyNotRecording(int card)
    {
      Assert.IsFalse(_cards[card].IsRecording);
      Assert.AreEqual(_cards[card].CurrentTVRecording, null);
    }

    void VerifyRecord(int card, TVRecording rec)
    {
      Assert.IsTrue(_cards[card].IsRecording);
      Assert.AreEqual(_cards[card].CurrentTVRecording.Channel, rec.Channel);
      Assert.AreEqual(_cards[card].CurrentTVRecording.Title, rec.Title);
      Assert.AreEqual(_cards[card].CurrentTVRecording.End, rec.End);
      Assert.AreEqual(_cards[card].CurrentTVRecording.Start, rec.Start);
    }

    void Zap(string channelName)
    {
      _processor.AddCommand(new TimeShiftTvCommand(channelName));
      _processor.ProcessCommands();
    }
    void DoSchedule()
    {
      _processor.AddCommand(new CheckRecordingsCommand());
      _processor.ProcessCommands();
      _processor.ProcessScheduler();
    }
    void StopRecord()
    {
      _processor.AddCommand(new StopRecordingCommand());
      _processor.ProcessCommands();
    }

    TVRecording AddSchedule(string channelName, DateTime dtStart, DateTime dtEnd)
    {
      DateTime startTime = dtStart;
      DateTime endTime = dtEnd;
      TVRecording rec = new TVRecording();
      rec.Channel = channelName;
      rec.Title = "unknown";
      rec.Start = Utils.datetolong(startTime);
      rec.End = Utils.datetolong(endTime);
      rec.RecType = TVRecording.RecordingType.Once;
      TVDatabase.AddRecording(ref rec);
      return rec;
    }
    #endregion
  }
}
