#region Usings
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;
#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  class RemoteWakeupHandler : IWakeupHandler
  {
    private IWakeupHandler remote;
    private int tag;

    public RemoteWakeupHandler(String URL, int tag)
    {
      remote = (IWakeupHandler)Activator.GetObject(typeof(IWakeupHandler), URL);
      this.tag = tag;
    }

    public void Close()
    {
      remote = null;
    }

    #region IWakeupHandler Members

    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      if (remote == null) return DateTime.MaxValue;
      try
      {
        return remote.GetNextWakeupTime(earliestWakeupTime);
      }
      catch (Exception)
      {
        // broken remote handler, nullify this one (dead)
        remote = null;
        return DateTime.MaxValue;
      }
    }


    public string HandlerName
    {
      get
      {
        if (remote == null) return "<dead#" + tag + ">";
        try
        {
          return remote.HandlerName;
        }
        catch (Exception)
        {
          // broken remote handler, nullify this one (dead)
          remote = null;
          return "<dead>";
        }
      }
    }

    #endregion

  }
}
