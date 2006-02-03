using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Support;
using System.IO;

namespace MediaPortal.Tests.Support
{
  [TestFixture]
  public class HotFixInformationLoggerTests
  {
    [Test]
    public void HotFixLogger()
    {
      ILogCreator logger = new HotFixInformationLogger();
      logger.CreateLogs("TestData\\TestOutput");
      Assert.IsTrue(File.Exists("TestData\\TestOutput\\hotfixes.log"));
    }
  }
}

