namespace ProjectInfinity.Messaging
{
  public class MessageEventArgs<T> : Message
  {
    private T argument;

    public MessageEventArgs(T argument)
    {
      this.argument = argument;
    }

    public T Argument
    {
      get { return argument; }
    }
  }
}