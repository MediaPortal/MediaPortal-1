using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ProcessPlugins.DiskSpace;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.Tests.Commands
{
  [TestFixture]
  public class TestCommandProcessor
  {
    #region dummy command
    public class DummyCommand : CardCommand
    {
      bool _executed = false;
      public bool Executed
      {
        get { return _executed; }
      }
      public override void Execute(CommandProcessor handler)
      {
        _executed = true;
      }
    }
    #endregion

    #region general tests
    [SetUp]
    public void Init()
    {
      TVChannel ch;
      TVDatabase.ClearAll();

      // add 3 channels
      ch = new TVChannel("RTL 4"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("RTL 5"); TVDatabase.AddChannel(ch);
      ch = new TVChannel("SBS 6"); TVDatabase.AddChannel(ch);
    }

    [Test]
    public void TestCtor()
    {
      CommandProcessor proc = new CommandProcessor();
      Assert.AreEqual(proc.CurrentCardIndex, -1);
      Assert.AreEqual(proc.TVChannelName, String.Empty);
    }
    [Test]
    public void TestDummyCommand()
    {
      DummyCommand cmd = new DummyCommand();
      Assert.IsFalse(cmd.Executed);
      CommandProcessor proc = new CommandProcessor();
      proc.AddCommand(cmd);
      proc.ProcessCommands();
      Assert.IsTrue(cmd.Executed);
    }

    [Test]
    public void TestCheckRecordingsCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      proc.scheduler.UpdateTimer();
      Assert.IsFalse(proc.scheduler.TimeToProcessRecordings);

      proc.AddCommand(new CheckRecordingsCommand());
      proc.ProcessCommands();

      Assert.IsTrue(proc.scheduler.TimeToProcessRecordings);
    }

    [Test]
    public void TestIsBusy()
    {
      CommandProcessor proc = new CommandProcessor();
      Assert.IsFalse(proc.IsBusy);
      proc.AddCommand(new CheckRecordingsCommand());
      Assert.IsTrue(proc.IsBusy);
      proc.ProcessCommands();
      Assert.IsFalse(proc.IsBusy);
    }
    #endregion

    #region test tv viewing
    [Test]
    [Category("view tv")]
    public void TestViewStartCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
    }

    [Test]
    [Category("view tv")]
    public void TestViewZap()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
      WatchTv(proc, "RTL 5");
    }

    [Test]
    [Category("view tv")]
    public void TestViewStopCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
      StopTv(proc);
    }
    #endregion

    #region test tv viewing with timeshifting on
    [Test]
    [Category("timeshift tv")]
    public void TestTimeShiftStartCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      TimeShiftTv(proc, "RTL 4");
    }

    [Test]
    [Category("timeshift tv")]
    public void TestTimeShiftZap()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //lets watch TV
      TimeShiftTv(proc, "RTL 4");

      //switch channels
      TimeShiftTv(proc, "RTL 5");
    }

    [Test]
    [Category("timeshift tv")]
    public void TestTimeShiftStopCommand()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //lets watch TV
      TimeShiftTv(proc, "RTL 4");
      //lets stop TV
      StopTv(proc);

    }
    #endregion

    #region test switch timeshift off/on
    [Test]
    [Category("switch timeshift on/off")]
    public void TestSwitchTimeShiftOnOff()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //watch TV
      WatchTv(proc, "RTL 4");
      TimeShiftTv(proc, "RTL 4");
      WatchTv(proc, "RTL 4");
      StopTv(proc);

    }
    #endregion

    #region test recording
    [Test]
    [Category("Recording")]
    public void TestStartRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      StartRecord(proc, "RTL 4");
    }
    [Test]
    [Category("Recording")]
    public void TestStopRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      StartRecord(proc, "RTL 4");
      TimeShiftTv(proc, "RTL 4");
      StopRecord(proc);
    }
    [Test]
    [Category("Recording")]
    public void TestStartRecording2()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      TimeShiftTv(proc, "RTL 5");
      StartRecord(proc, "RTL 4");
      StopRecord(proc);
    }
    [Test]
    [Category("Recording")]
    public void TestStartRecording3()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      WatchTv(proc, "RTL 5");
      StartRecord(proc, "RTL 4");
      StopRecord(proc);
    }
    [Test]
    [Category("Recording")]
    public void TestRecordingInPast()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      AddSchedule("RTL 4", DateTime.Now.AddHours(-6), DateTime.Now.AddHours(-5));
      DoSchedule(proc);
      Assert.IsFalse(card1.IsRecording);
    }
    [Test]
    [Category("Recording")]
    public void TestRecordingInFuture()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      AddSchedule("RTL 4", DateTime.Now.AddHours(+1), DateTime.Now.AddHours(+2));
      DoSchedule(proc);
      Assert.IsFalse(card1.IsRecording);
    }
    [Test]
    [Category("Recording")]
    public void TestPreRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      AddSchedule("RTL 4", DateTime.Now.AddMinutes(2), DateTime.Now.AddHours(+2));
      DoSchedule(proc);
      Assert.IsTrue(card1.IsRecording);
    }

    [Test]
    [Category("Recording")]
    public void TestZapWhileRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      StartRecord(proc, "RTL 4");

      TimeShiftTv(proc, "RTL 4");

      //zap to rtl 5 (should fail)
      proc.AddCommand(new TimeShiftTvCommand("RTL 5"));
      proc.ProcessCommands();
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, "RTL 4");
      Assert.AreEqual(proc.TVCards[0].TVChannel, "RTL 4");
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(proc.CurrentCardIndex), @"C:\card1\live.tv");

      StopRecord(proc);
    }
    [Test]
    [Category("Recording")]
    public void TestTurnOffTimeshiftWhileRecording()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //record TV
      TimeShiftTv(proc, "RTL 4");
      StartRecord(proc, "RTL 4");

      //zap to rtl 5 (should fail)
      proc.AddCommand(new ViewTvCommand("RTL 5"));
      proc.ProcessCommands();
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, "RTL 4");
      Assert.AreEqual(proc.TVCards[0].TVChannel, "RTL 4");
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(proc.CurrentCardIndex), @"C:\card1\live.tv");

      StopRecord(proc);
    }
    #endregion

    #region test audio switching
    [Test]
    [Category("Audio stream selection")]
    public void TestGetAudioLanguageList()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      TimeShiftTv(proc, "RTL 4");
      ArrayList list = card1.GetAudioLanguageList();
      Assert.AreEqual(list.Count, 3);
      Assert.AreEqual((int)list[0], 123);
      Assert.AreEqual((int)list[1], 456);
      Assert.AreEqual((int)list[2], 789);
    }
    [Test]
    [Category("Audio stream selection")]
    public void TestSwitchAudioStreamWithTimeShiftEnabled()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      TimeShiftTv(proc, "RTL 4");
      proc.AddCommand(new SetAudioLanguageCommand(123));
      proc.ProcessCommands();
      Assert.AreEqual(card1.GetAudioLanguage(), 123);

      proc.AddCommand(new SetAudioLanguageCommand(456));
      proc.ProcessCommands();
      Assert.AreEqual(card1.GetAudioLanguage(), 456);



      proc.AddCommand(new SetAudioLanguageCommand(789));
      proc.ProcessCommands();
      Assert.AreEqual(card1.GetAudioLanguage(), 789);
    }

    [Test]
    [Category("Audio stream selection")]
    public void TestSwitchAudioStreamWithTimeShiftDisabled()
    {
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");
      WatchTv(proc, "RTL 4");
      proc.AddCommand(new SetAudioLanguageCommand(123));
      proc.ProcessCommands();
      Assert.AreEqual(card1.GetAudioLanguage(), 123);

      proc.AddCommand(new SetAudioLanguageCommand(456));
      proc.ProcessCommands();
      Assert.AreEqual(card1.GetAudioLanguage(), 456);



      proc.AddCommand(new SetAudioLanguageCommand(789));
      proc.ProcessCommands();
      Assert.AreEqual(card1.GetAudioLanguage(), 789);
    }
    #endregion

    #region helper functions
    void DoSchedule(CommandProcessor proc)
    {
      proc.AddCommand(new CheckRecordingsCommand());
      proc.ProcessCommands();
      proc.ProcessScheduler();
    }
    void StopRecord(CommandProcessor proc)
    {
      DateTime dtNow = DateTime.Now;
      TVRecording rec = proc.TVCards[0].CurrentTVRecording;
      proc.AddCommand(new StopRecordingCommand());
      proc.ProcessCommands();
      proc.ProcessScheduler();
      Assert.IsFalse(proc.TVCards[0].IsRecording);
      Assert.IsFalse(proc.TVCards[0].IsPostRecording);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording, null);
      Assert.AreEqual(proc.TVCards[0].RecordingFileName.Length, 0);

      //check if recording has been canceled
      List<TVRecording> recs = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recs);
      CompareDates(recs[0].CanceledTime, dtNow);

      //check if recorded tv has been added
      List<TVRecorded> recList = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recList);
      Assert.AreEqual(rec.Channel, recList[0].Channel);
      Assert.AreEqual(rec.Title, recList[0].Title);
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
    void StartRecord(CommandProcessor proc, string channelName, DateTime dtStart, DateTime dtEnd)
    {
      string chanName = proc.TVChannelName;
      int cardIndex = proc.CurrentCardIndex;
      TVRecording rec=AddSchedule(channelName,  dtStart,  dtEnd);

      //start recording it
      DoSchedule(proc);

      int cardNo;
      bool result = proc.IsRecordingSchedule(rec, out  cardNo);

      Assert.IsTrue(result);
      Assert.AreEqual(cardNo, 0);
      Assert.AreEqual(proc.CurrentCardIndex, cardIndex);
      if (cardIndex==0)
        Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(proc.TVCards[0].TVChannel, rec.Channel);
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.IsTrue(proc.TVCards[0].IsRecording);
      Assert.IsFalse(proc.TVCards[0].IsPostRecording);
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(0), @"C:\card1\live.tv");
      Assert.AreEqual(proc.TVCards[0].CurrentProgramRecording, null);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.Title, rec.Title);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.Start, rec.Start);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.End, rec.End);
      Assert.AreEqual(proc.TVCards[0].CurrentTVRecording.Channel, rec.Channel);
      Assert.IsTrue(proc.TVCards[0].RecordingFileName.Length > 0);
    }
    void StartRecord(CommandProcessor proc, string channelName)
    {
      StartRecord(proc, channelName, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(+1));
    }

    void StopTv(CommandProcessor proc)
    {
      proc.AddCommand(new StopTvCommand());
      proc.ProcessCommands();
      Assert.AreEqual(proc.CurrentCardIndex, -1);
      Assert.AreEqual(proc.TVChannelName, "");
      Assert.AreEqual(proc.TVCards[0].TVChannel, "");
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsFalse(proc.TVCards[0].IsTimeShifting);
      CompareDates(proc.TVCards[0].TimeShiftingStarted, DateTime.MinValue);
    }
    void TimeShiftTv(CommandProcessor proc, string channelName)
    {
      bool isTimeShifting = proc.TVCards[0].IsTimeShifting;
      //timeshift TV
      DateTime dtNow = DateTime.Now;
      proc.AddCommand(new TimeShiftTvCommand(channelName));
      proc.ProcessCommands();
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(proc.TVCards[0].TVChannel, channelName);
      Assert.IsFalse(proc.TVCards[0].View);
      Assert.IsTrue(proc.TVCards[0].IsTimeShifting);
      Assert.AreEqual(proc.TVCards[0].TimeShiftFileName, @"live.tv");
      Assert.AreEqual(proc.GetTimeShiftFileName(proc.CurrentCardIndex), @"C:\card1\live.tv");
      if (!isTimeShifting)
      {
        CompareDates(proc.TVCards[0].TimeShiftingStarted, dtNow);
      }
    }

    void WatchTv(CommandProcessor proc, string channelName)
    {
      //watch TV
      proc.AddCommand(new ViewTvCommand(channelName));
      proc.ProcessCommands();
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(proc.TVCards[0].TVChannel, channelName);
      Assert.IsTrue(proc.TVCards[0].View);
      Assert.IsFalse(proc.TVCards[0].IsTimeShifting);
      CompareDates(proc.TVCards[0].TimeShiftingStarted, DateTime.MinValue);
    }
    void CompareDates(DateTime dt1, DateTime dt2)
    {
      Assert.AreEqual(dt1.Year, dt2.Year);
      Assert.AreEqual(dt1.Month, dt2.Month);
      Assert.AreEqual(dt1.Day, dt2.Day);
      Assert.AreEqual(dt1.Hour, dt2.Hour);
      Assert.AreEqual(dt1.Minute, dt2.Minute);
    // Assert.AreEqual(dt1.Second, dt2.Second);
    }
    #endregion
  }
}
