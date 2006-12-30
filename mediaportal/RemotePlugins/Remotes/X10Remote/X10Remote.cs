#region Copyright (C) 2005-2006 Team MediaPortal - CoolHammer, mPod
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: CoolHammer, mPod
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

#endregion

using System;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using X10;


namespace MediaPortal.InputDevices
{
  /// <summary>
  /// 
  /// </summary>
  public class X10Remote
  {

    X10.X10Interface X10Inter = null;
    IConnectionPointContainer icpc = null;
    IConnectionPoint icp = null;
    X10Sink X10Sink = null;
    int cookie = 0;
    InputHandler _inputHandler = null;
    bool _controlEnabled = false;
    bool _logVerbose = false;
    bool _x10Medion = true;
    bool _x10Ati = false;
    bool _x10Firefly = false;
    bool _x10UseChannelControl = false;
    int _x10Channel = 0;
    public bool _remotefound = false;
    //This struct stores information needed to tell whether a key is a repeat (bug in X10 after standby)
    
    

    public X10Remote()
    {
      
    }

    public void Init()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        Log.Info("X10 Remote Debug: {0}",Config.Dir.Config.ToString());
        _controlEnabled = xmlreader.GetValueAsBool("remote", "X10", false);
        _x10Medion = xmlreader.GetValueAsBool("remote", "X10Medion", false);
        _x10Ati = xmlreader.GetValueAsBool("remote", "X10ATI",false);
        _x10Firefly = xmlreader.GetValueAsBool("remote", "X10Firefly", false);
        _logVerbose = xmlreader.GetValueAsBool("remote", "X10VerboseLog", false);
        _x10UseChannelControl = xmlreader.GetValueAsBool("remote", "X10UseChannelControl", false);
        _x10Channel = xmlreader.GetValueAsInt("remote", "X10Channel", 0);
      }

      if (_inputHandler == null)
      {
        Log.Info("X10 enabled control : {0}", _controlEnabled.ToString());
        if (_controlEnabled)
          if (_x10Medion)
            _inputHandler = new InputHandler("Medion X10");
          else if (_x10Ati)
            _inputHandler = new InputHandler("ATI X10");
          else if (_x10Firefly)
            _inputHandler = new InputHandler("Firefly X10");
          else
            _inputHandler = new InputHandler("Other X10");
        else
          return;

        if (!_inputHandler.IsLoaded)
        {
          _controlEnabled = false;
          Log.Info("X10: Error loading default mapping file - please reinstall MediaPortal");
          return;
        }

        if (_logVerbose)
        {
          if (_x10Medion)
            Log.Info("X10Remote: Start Medion");
          else if (_x10Ati)
            Log.Info("X10Remote: Start ATI");
          else if (_x10Firefly)
            Log.Info("X10Remote: Start Firefly");
          else
            Log.Info("X10Remote: Start Other");
        }

      }

      //Setup the X10 Remote
      try
      {
        if (X10Inter == null)
        {
          X10Inter = new X10Interface();
          if (X10Inter == null)
          {
            Log.Info("X10 debug: Could not get interface");
            return;
          }
          X10Sink = new X10Sink(_inputHandler, _logVerbose);
          icpc = (IConnectionPointContainer)X10Inter;
          Guid IID_InterfaceEvents = typeof(_DIX10InterfaceEvents).GUID;
          icpc.FindConnectionPoint(ref IID_InterfaceEvents, out icp);
          icp.Advise(X10Sink, out cookie);
          _remotefound = true;
        }
      }
      catch (System.Runtime.InteropServices.COMException)
      {
        Log.Info("X10 Debug: Com error");
      }

   }

    public void DeInit()
    {
      if (!_controlEnabled)
        return;
      
      
      _inputHandler = null;

      if (_logVerbose)
        Log.Info("X10Remote: Stop");
    }

   

  }
}