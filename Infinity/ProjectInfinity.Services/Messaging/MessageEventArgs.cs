using System;

namespace ProjectInfinity.Messaging
{
  public class MessageEventArgs<T> : EventArgs
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