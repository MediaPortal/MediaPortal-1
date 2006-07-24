using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.WebEPG;
using System.IO;
using MediaPortal.Utils.Services;

namespace MediaPortal.Tests.WebEPG.Parser
{
  [TestFixture]
  [Category("EPG")]
  public class ProgramDataTest
  {
    [SetUp]
    public void Init()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      StringWriter logString = new StringWriter();
      Log log = new Log(logString, Log.Level.Debug);
      services.Replace<ILog>(log);
    }

    [Test]
    public void SetElementTime()
    {
      ProgramData testData = new ProgramData();
      testData.ChannelID = "myChannel.tv";

      // #START/#END Tags
      // Test usual hour values, with each separator

      testData.SetElement("<#START>", "0:30");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#END>", "0:30");            // Only one test on endTime
      Assert.IsTrue(testData.EndTime.Hour == 0);        // is enough
      Assert.IsTrue(testData.EndTime.Minute == 30);     // as it exactly behaves as StartTime do

      testData.SetElement("<#START>", "10h30");
      Assert.IsTrue(testData.StartTime.Hour == 10);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#START>", "12.30");
      Assert.IsTrue(testData.StartTime.Hour == 12);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      // Test special  values, with each separator 

      testData.SetElement("<#START>", "24:30");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#START>", "-0.30");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#START>", "-0h09");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 9);

      // Test am/pm 
      testData.SetElement("<#START>", "10:30 pm");
      Assert.IsTrue(testData.StartTime.Hour == 22);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#START>", "10:30pm");
      Assert.IsTrue(testData.StartTime.Hour == 22);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#START>", "10:30 am");
      Assert.IsTrue(testData.StartTime.Hour == 10);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#START>", "10:30am");
      Assert.IsTrue(testData.StartTime.Hour == 10);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("<#START>", "12:00 pm");
      Assert.IsTrue(testData.StartTime.Hour == 12);
      Assert.IsTrue(testData.StartTime.Minute == 0);

      testData.SetElement("<#START>", "12:00 am");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 0);

      // <#DAY>
      testData.SetElement("<#DAY>", "09");
      Assert.IsTrue(testData.Day == 9);

      // <#DESCRIPTION> 
      testData.SetElement("<#DESCRIPTION>", "   This is description, isn't it?   ");
      Assert.IsTrue(testData.Description == "This is description, isn't it?");
  
    }
  }
}
