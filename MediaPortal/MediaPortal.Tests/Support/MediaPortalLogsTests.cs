using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Support;
using System.IO;

namespace MediaPortal.Tests.Support
{
  [TestFixture]
  public class MediaPortalLogsTests
  {
    string outputDir = "Support\\TestData\\TestOutput";
    string logFile = "Support\\TestData\\TestOutput\\MediaPortal.log";
    [SetUp]
    public void Init()
    {
      Directory.CreateDirectory(outputDir);
      foreach (string file in Directory.GetFiles(outputDir))
      {
        File.Delete(file);
      }
    }

    [Test]
    public void CreateLogsWithNoErrors()
    {
      MediaPortalLogs mplogs = new MediaPortalLogs("Support\\TestData");
      mplogs.CreateLogs(outputDir);
      Assert.IsTrue(File.Exists(outputDir + "\\MediaPortal.log"), "Log file not copied!");
    }

    [Test]
    public void CopyOverExistingFiles()
    {
      MediaPortalLogs mplogs = new MediaPortalLogs("Support\\TestData");
      FileHelper.Touch(logFile);
      mplogs.CreateLogs(outputDir);
    }
  }
}
