using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Support;
using System.IO;

namespace MediaPortal.Tests.Support
{
  [TestFixture]
  public class EventLogLoggerTests
  {
    [Test]
    public void LogEvents()
    {
      EventLogLogger logger = new EventLogLogger("Application");
      logger.CreateLogs("Support\\TestData\\TestOutput");
      Assert.IsTrue(File.Exists("Support\\TestData\\TestOutput\\Application_events.xml"));
    }
  }
}
