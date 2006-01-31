using System;
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

      proc.AddCommand( new CheckRecordingsCommand());
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
  }
}
