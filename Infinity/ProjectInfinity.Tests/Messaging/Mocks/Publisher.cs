using System;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Tests.Messaging.Mocks
{
  public class Publisher
  {
    [MessagePublication(typeof(MockMessage))]
    public event EventHandler<MessageEventArgs<string>> Publish;

    public void DoPublish()
    {
      if (Publish != null)
      {
        Publish(this, new MessageEventArgs<string>("Hello"));
      }
    }
  }
}