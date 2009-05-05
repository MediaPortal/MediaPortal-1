#region Usings
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;
#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  class RemoteStandbyHandler : IStandbyHandler
  {
    private IStandbyHandler remote;
    private int tag;

    public RemoteStandbyHandler( String URL, int tag )
    {
      remote = (IStandbyHandler)Activator.GetObject(typeof(IStandbyHandler), URL);
      this.tag = tag;
    }

    public void Close()
    {
      remote = null;
    }

    #region IStandbyHandler Members

    public bool DisAllowShutdown
    {
      get 
      {
        if (remote == null) return false;
        try
        {
          return remote.DisAllowShutdown;
        }
        catch (Exception)
        {
          // broken remote handler, nullify this one (dead)
          remote = null;
          return false;
        }
      }
    }
    public void UserShutdownNow()
    {
      if (remote == null) return;
      try
      {
        remote.UserShutdownNow();
      }
      catch (Exception)
      {
        // broken remote handler, nullify this one (dead)
        remote = null;
      }
    }

    public string HandlerName
    {
      get {
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
