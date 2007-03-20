using ProjectInfinity.Messaging;

namespace ProjectInfinity.Tests.Messaging.Mocks
{
  public class InvalidReceiver
  {
    [MessageSubscription(typeof (MockMessage))]
    private void Receive(object sender, string e)
    {
    }
  }
}