using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.WebEPG;

namespace MediaPortal.Tests.WebEPG.Parser
{
  [TestFixture]
  [Category("EPG")]
  public class ProgramDataTest
  {
    [Test]
    public void SetElementTime()
    {
      ProgramData testData = new ProgramData();
      testData.ChannelID = "myChannel.tv";

      testData.SetElement("<#START>", "22:22");
      Assert.IsTrue(testData.StartTime.Hour == 22);
      Assert.IsTrue(testData.StartTime.Minute == 22);

      testData.SetElement("<#START>", "24:24");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 24);

      testData.SetElement("<#START>", "0:24");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 24);

      testData.SetElement("<#START>", "-0:24");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 24);

      testData.SetElement("<#END>", "22.22");
      Assert.IsTrue(testData.EndTime.Hour == 22);
      Assert.IsTrue(testData.EndTime.Minute == 22);

      testData.SetElement("<#END>", "22h22");
      Assert.IsTrue(testData.EndTime.Hour == 22);
      Assert.IsTrue(testData.EndTime.Minute == 22);
    }
  }
}
