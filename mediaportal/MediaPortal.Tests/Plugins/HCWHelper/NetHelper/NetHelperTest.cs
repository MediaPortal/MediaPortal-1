using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NetHelper;

namespace MediaPortal.Tests.Plugins.HCWHelper.NetHelper
{
  [TestFixture]
  [Category("NetHelper")]
  public class NetHelperTest
  {
    int udpPort = 2110;

    [Test]
    public void ListenForIncoming()
    {
      Connection connection = new Connection(true);
      Assert.IsTrue(connection.Start(udpPort));
    }

    [Test]
    public void SendMessage()
    {
      Connection connection = new Connection(true);
      Assert.IsTrue(connection.Send(udpPort, "CMD", "0", DateTime.Now));
    }

  }
}
