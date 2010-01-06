#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  internal class RemoteStandbyHandler : IStandbyHandler
  {
    private IStandbyHandler remote;
    private int tag;
    public readonly string Url;

    public RemoteStandbyHandler(String URL, int tag)
    {
      remote = (IStandbyHandler)Activator.GetObject(typeof (IStandbyHandler), URL);
      this.tag = tag;
      this.Url = URL;
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