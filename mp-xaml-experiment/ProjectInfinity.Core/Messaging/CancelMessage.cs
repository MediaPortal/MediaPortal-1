using System.ComponentModel;

namespace ProjectInfinity.Messaging
{
  public class CancelMessage : Message
  {
    private readonly CancelEventArgs _cancel;

    public CancelMessage() : this(false)
    {
    }

    public CancelMessage(bool cancel)
    {
      _cancel = new CancelEventArgs(cancel);
    }

    public CancelMessage(CancelEventArgs e)
    {
      _cancel = e;
    }

    public bool Cancel
    {
      get { return _cancel.Cancel; }
      set { _cancel.Cancel = value; }
    }
  }
}