using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Tests.Messaging.Mocks
{
  internal class Subscriber
  {
    private bool received = false;

    public bool Received
    {
      get { return received; }
    }

    [MessageSubscription(typeof(MockMessage))]
    private void Receive(object sender, MessageEventArgs<string> e)
    {
      received = true;
      ServiceScope.Get<ILogger>().Debug("Subscriber received " + e.Argument);
    }

    public void Reset()
    {
      received = false;
    }
  }
}