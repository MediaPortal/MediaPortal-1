using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Support;
using System.IO;

namespace MediaPortal.Tests.Support
{
  [TestFixture]
  public class ExceptionLoggerTests
  {
    [Test]
    public void CreateLogger()
    {
      try
      {
        throw new Exception("some message");
      }
      catch (Exception exc)
      {
        ILogCreator logger = new ExceptionLogger(exc);
        logger.CreateLogs("TestData\\TestOutput");
        Assert.IsTrue(File.Exists("TestData\\TestOutput\\exception.log"));
      }
    }
  }
}
