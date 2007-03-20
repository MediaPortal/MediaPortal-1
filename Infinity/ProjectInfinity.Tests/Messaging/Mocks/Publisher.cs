using System;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Tests.Messaging.Mocks
{
  public class Publisher
  {
    [MessagePublication(typeof (MockMessage))]
    public event EventHandler<MockMessage> Publish;

    public void DoPublish()
    {
      if (Publish != null)
      {
        Publish(this, new MockMessage("Hello"));
      }
    }
  }
}