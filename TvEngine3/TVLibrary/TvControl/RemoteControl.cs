/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using TvControl;
using TvLibrary.Log;

namespace TvControl
{
  /// <summary>
  /// Class which holds the connection with the master tv-server
  /// </summary>
  public class RemoteControl
  {
    static IController _tvControl;
    static string _hostName = "localhost";

    /// <summary>
    /// Gets or sets the name the hostname of the master tv-server.
    /// </summary>
    /// <value>The name of the host.</value>
    static public string HostName
    {
      get
      {
        return _hostName;
      }
      set
      {
        if (_hostName != value)
        {
          _tvControl = null;
          _hostName = value;
        }
      }
    }
    /// <summary>
    /// returns an the <see cref="T:TvControl.IController"/> interface to the tv server
    /// </summary>
    /// <value>The instance.</value>
    static public IController Instance
    {
      get
      {
        try
        {
          if (_tvControl != null)
            return _tvControl;

          _tvControl = (IController)Activator.GetObject(typeof(IController), String.Format("tcp://{0}:31456/TvControl", _hostName));
          int card = _tvControl.Cards;
          return _tvControl;
        }
        catch (RemotingTimeoutException exrt)
        {
          try
          {
            Log.Error("RemoteControl: Timeout getting server Instance; retrying in 5 seconds - {0}", exrt.Message);
            // maybe the DB wasn't up yet - 2nd try...
            System.Threading.Thread.Sleep(5000);
            _tvControl = (IController)Activator.GetObject(typeof(IController), String.Format("tcp://{0}:31456/TvControl", _hostName));
            int card = _tvControl.Cards;
            return _tvControl;

          }
          // didn't help - do nothing
          catch (Exception)
          {
          }
        }
        catch (RemotingException exr)
        {
          Log.Error("RemoteControl: Error getting server Instance - {0}", exr.Message);
        }
        catch (Exception exg)
        {
          Log.Error("RemoteControl: Error getting server Instance - {0}", exg.Message);
        }

        return _tvControl;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is connected with the tv server
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
    /// </value>
    static public bool IsConnected
    {
      get
      {
        try
        {
          int cards = RemoteControl.Instance.Cards;
          return true;
        }
        catch (Exception)
        {
        }
        return false;
      }
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    static public void Clear()
    {
      _tvControl = null;
    }
  }
}
