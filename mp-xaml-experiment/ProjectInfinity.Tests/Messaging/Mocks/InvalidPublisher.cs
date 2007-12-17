using System;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Tests.Messaging.Mocks
{
  internal class InvalidPublisher
  {
    [MessagePublication(typeof (MockMessage))]
    public event EventHandler<EventArgs> Publish;
  }
}