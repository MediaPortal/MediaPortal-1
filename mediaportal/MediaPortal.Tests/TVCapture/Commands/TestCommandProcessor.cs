using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ProcessPlugins.DiskSpace;
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

    #region switch timeshift off/on
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

    }
    #endregion

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
      CompareDates(proc.TVCards[0].TimeShiftingStarted, dtNow);
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
      Assert.AreEqual(dt1.Second, dt2.Second);
    }
  }
}
