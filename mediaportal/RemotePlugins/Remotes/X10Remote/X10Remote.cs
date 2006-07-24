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
using MediaPortal.Utils.Services;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// 
  /// </summary>
  public class X10Remote
  {
    X10RemoteForm _x10Form = null;
    InputHandler _inputHandler = null;
    bool _controlEnabled = false;
    bool _logVerbose = false;
    bool _x10Medion = true;
    bool _x10Ati = false;
    bool _x10UseChannelControl = false;
    int _x10Channel = 0;
    protected ILog _log;

    public X10Remote()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    public void Init(IntPtr hwnd)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _controlEnabled = xmlreader.GetValueAsBool("remote", "X10", false);
        _x10Medion = xmlreader.GetValueAsBool("remote", "X10Medion", true);
        _x10Ati = xmlreader.GetValueAsBool("remote", "X10ATI", true);
        _logVerbose = xmlreader.GetValueAsBool("remote", "X10VerboseLog", false);
        _x10UseChannelControl = xmlreader.GetValueAsBool("remote", "X10UseChannelControl", false);
        _x10Channel = xmlreader.GetValueAsInt("remote", "X10Channel", 0);
      }
      if (_inputHandler == null)
      {
        if (_controlEnabled)
          if (_x10Medion)
            _inputHandler = new InputHandler("Medion X10");
          else if (_x10Ati)
            _inputHandler = new InputHandler("ATI X10");
          else
            _inputHandler = new InputHandler("Other X10");
        else
          return;

        if (!_inputHandler.IsLoaded)
        {
          _controlEnabled = false;
          _log.Info("X10: Error loading default mapping file - please reinstall MediaPortal");
          return;
        }

        if (_logVerbose)
        {
          if (_x10Medion)
            _log.Info("X10Remote: Start Medion");
          else if (_x10Ati)
            _log.Info("X10Remote: Start ATI");
          else
            _log.Info("X10Remote: Start Other");
        }
      }
      if (_x10Form == null)
      {
        try
        {
          _x10Form = new X10RemoteForm(new AxX10._DIX10InterfaceEvents_X10CommandEventHandler(this.IX10_X10Command));
        }
        catch (System.Runtime.InteropServices.COMException)
        {
          _controlEnabled = false;
          _log.Info("X10Remote: Can't initialize");
        }
      }
    }

    public void DeInit()
    {
      if (!_controlEnabled)
        return;

      _x10Form.Close();
      _x10Form.Dispose();
      _x10Form = null;

      _inputHandler = null;

      if (_logVerbose)
        _log.Info("X10Remote: Stop");
    }

    public void IX10_X10Command(object sender, AxX10._DIX10InterfaceEvents_X10CommandEvent e)
    {
      if (_logVerbose)
      {
        _log.Info("X10Remote: Command Start --------------------------------------------");
        _log.Info("X10Remote: e            = {0}", e.ToString());
        _log.Info("X10Remote: bszCommand   = {0}", e.bszCommand.ToString());
        _log.Info("X10Remote: eCommand     = {0} - {1}", (int)Enum.Parse(typeof(X10.EX10Command), e.eCommand.ToString()), e.eCommand.ToString());
        _log.Info("X10Remote: eCommandType = {0}", e.eCommandType.ToString());
        _log.Info("X10Remote: eKeyState    = {0}", e.eKeyState.ToString());
        _log.Info("X10Remote: lAddress     = {0}", e.lAddress.ToString());
        _log.Info("X10Remote: lSequence    = {0}", e.lSequence.ToString());
        _log.Info("X10Remote: varTimestamp = {0}", e.varTimestamp.ToString());
        _log.Info("X10Remote: Command End ----------------------------------------------");
      }
      
      if (e.eKeyState.ToString() == "X10KEY_ON" || e.eKeyState.ToString() == "X10KEY_REPEAT")
      {
        if (_x10UseChannelControl && (e.lAddress != _x10Channel))
          return;

        if (_inputHandler.MapAction((int)Enum.Parse(typeof(X10.EX10Command), e.eCommand.ToString())))
        {
          if (_logVerbose) _log.Info("X10Remote: Action mapped");
        }
        else
        {
          if (_logVerbose) _log.Info("X10Remote: Action not mapped");
        }
      }
    }

  }
}
