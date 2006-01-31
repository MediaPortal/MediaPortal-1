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


    [Test]
    public void TestViewTv()
    {
      string channelName = "RTL 4";
      CommandProcessor proc = new CommandProcessor();
      TVCaptureDevice card1 = proc.TVCards.AddDummyCard("dummy1");

      //lets watch TV
      proc.AddCommand(new ViewTvCommand(channelName));
      proc.ProcessCommands();
      Assert.AreEqual(proc.CurrentCardIndex, 0);
      Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(card1.TVChannel, channelName);
      Assert.IsTrue(card1.View);

      //switch channels
      channelName = "RTL 5";
      proc.AddCommand(new ViewTvCommand(channelName));
      proc.ProcessCommands();
      Assert.AreEqual(proc.TVChannelName, channelName);
      Assert.AreEqual(card1.TVChannel, channelName);
      Assert.IsTrue(card1.View);

      //lets stop TV
      proc.AddCommand(new StopTvCommand());
      proc.ProcessCommands();
      Assert.AreEqual(proc.CurrentCardIndex, -1);
      Assert.AreEqual(proc.TVChannelName, "");
      Assert.AreEqual(card1.TVChannel, "");
      Assert.IsFalse(card1.View);
    }
  }
}
