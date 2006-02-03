using System;
using MediaPortal.Support;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Tests.Support
{
  [TestFixture]
  public class DxDiagLogTests
  {
    private class MyProcRunner : ProcessRunner
    {
      public bool hasRun = false;
      public override void Run()
      {
        base.Run();
        if (LastExitCode == 0)
          hasRun = true;
      }
    }

    [Explicit]
    [Test]
    public void CreateLog()
    {
      MyProcRunner runner = new MyProcRunner();
      DxDiagLog dxlog = new DxDiagLog(runner);
      string tempFile = Environment.GetEnvironmentVariable("SystemDrive") + "\\_dxdiag.txt";
      FileHelper.Touch(tempFile);

      dxlog.CreateLogs("TestData\\TestOutput");

      Assert.IsTrue(runner.Executable.EndsWith("dxdiag.exe"), "Wrong thing has been run!");
      Assert.IsTrue(runner.hasRun);
      Assert.IsFalse(File.Exists(tempFile));
    }
  }
}
