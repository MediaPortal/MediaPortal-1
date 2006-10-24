using System;
using System.Collections.Generic;
using System.Text;

namespace TvEngine.Events
{
  public delegate void TvServerEventHandler(object sender, TvServerEventArgs eventArgs);

  public interface ITvServerEvent
  {
    event TvServerEventHandler OnFired;
  }
}
