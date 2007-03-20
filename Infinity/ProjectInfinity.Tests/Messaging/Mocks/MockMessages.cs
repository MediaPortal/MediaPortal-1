using ProjectInfinity.Messaging;

namespace ProjectInfinity.Tests.Messaging.Mocks
{
  public class MockMessage : Message
  {
    private string argument;

    public MockMessage(string arg)
    {
      argument = arg;
    }

    public string Argument
    {
      get { return argument; }
      set { argument = value; }
    }
  }
}