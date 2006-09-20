using System;
using MediaPortal.Support;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;


namespace MediaPortal.Tests.Support
{
  [TestFixture]
  public class EventLogsTests
  {
    string outputDir = "Support\\TestData\\TestOutput\\";
    string[] logNames = { "Application", "System" };

    private class MyProcRunner : ProcessRunner
    {
      public bool ranTwice = false;
      public int count = 0;
      public override void Run()
      {
        count++;
      }
    }

    [SetUp]
    public void Init()
    {
      foreach (string logName in logNames)
      {
        if (File.Exists(outputDir + logName + ".evt"))
          File.Delete(outputDir + logName + ".evt");
      }
    }

    [Test]
    public void CreateLogs()
    {
      MyProcRunner runner = new MyProcRunner();
      PsLogEventsLogger logs = new PsLogEventsLogger(runner);

      logs.CreateLogs(outputDir);

      Assert.IsTrue(runner.Executable.EndsWith("psloglist.exe"), "Wrong process has been run!");
      Assert.AreEqual(2, runner.count);
    }
  }
}
