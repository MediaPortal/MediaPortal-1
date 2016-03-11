#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Prevent standby if the local (single-seat) client is active (used by PowerScheduler client plugin)
  /// </summary>
  internal class LocalClientStandbyHandler : IStandbyHandler
  {
    private IStandbyHandler _remote;
    private int _tag;
    public readonly string _url;

    public LocalClientStandbyHandler(String url, int tag)
    {
      try
      {
        _remote = (IStandbyHandler)Activator.GetObject(typeof(IStandbyHandler), url);
        this._tag = tag;
        this._url = url;
      }
      catch (Exception ex)
      {
        Log.Debug("LocalClientStandbyHandler: {0}", ex.Message);
      }
    }

    public void Close()
    {
      _remote = null;
    }

    #region IStandbyHandler Members

    public bool DisAllowShutdown
    {
      get
      {
        try
        {
          if (_remote == null)
            return false;
          return _remote.DisAllowShutdown;
        }
        catch (Exception ex)
        {
          // broken remote handler, nullify this one (dead)
          Log.Debug("LocalClientStandbyHandler: {0}", ex.Message);
          _remote = null;
          return false;
        }
      }
    }

    public void UserShutdownNow()
    {
      try
      {
        if (_remote == null)
          return;
        _remote.UserShutdownNow();
      }
      catch (Exception ex)
      {
        // broken remote handler, nullify this one (dead)
        Log.Debug("LocalClientStandbyHandler: {0}", ex.Message);
        _remote = null;
      }
    }

    public string HandlerName
    {
      get
      {
        try
        {
          if (_remote == null)
            return "<dead#" + _tag + ">";
          return _remote.HandlerName;
        }
        catch (Exception ex)
        {
          // broken remote handler, nullify this one (dead)
          Log.Debug("LocalClientStandbyHandler: {0}", ex.Message);
          _remote = null;
          return "<dead#" + _tag + ">";
        }
      }
    }

    #endregion
  }
}