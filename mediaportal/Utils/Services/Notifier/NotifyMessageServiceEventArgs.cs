using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Services
{
  public class NotifyMessageServiceEventArgs : EventArgs
  {
    public NotifyMessageServiceEventTypeEnum EventType;
    public INotifyMessage Message;
    public object Tag;
  }
}
