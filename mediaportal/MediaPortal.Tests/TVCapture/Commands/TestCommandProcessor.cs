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
