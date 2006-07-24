#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Windows.Forms;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Utils.Services;

namespace MediaPortal.InputDevices
{
	public class HidListener
	{
    bool controlEnabled = false;
    bool logVerbose = false;           // Verbose logging
    InputHandler _inputHandler;
    protected ILog _log;

    public HidListener()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    public void Init(IntPtr hwnd)
    {
      Init();
    }

    void Init()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HID", false);
        logVerbose = xmlreader.GetValueAsBool("remote", "HIDVerboseLog", false);
      }

      if (controlEnabled)
      {
        _inputHandler = new InputHandler("General HID");
        if (!_inputHandler.IsLoaded)
        {
          controlEnabled = false;
          _log.Info("HID: Error loading default mapping file - please reinstall MediaPortal");
        }
      }
    }

    public void DeInit()
    {
    }

		public bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode)
		{
			action = null;
			key = (char)0;
			keyCode = Keys.A;

      if (controlEnabled)
      {
        // we are only interested in WM_APPCOMMAND
        if (msg.Msg != 0x0319)
          return false;

        AppCommands appCommand = (AppCommands)((msg.LParam.ToInt32() >> 16) & ~0xF000);

        // find out which request the MCE remote handled last
        if ((appCommand == InputDevices.LastHidRequest) && (appCommand != AppCommands.VolumeDown) && (appCommand != AppCommands.VolumeUp))
        {
          // possible that it is the same request mapped to an app command?
          if (Environment.TickCount - InputDevices.LastHidRequestTick < 1000)
            return true;
        }

        InputDevices.LastHidRequest = appCommand;

        if (logVerbose) _log.Info("HID: Command: {0} - {1}", ((msg.LParam.ToInt32() >> 16) & ~0xF000), InputDevices.LastHidRequest.ToString());

        if (!_inputHandler.MapAction((msg.LParam.ToInt32() >> 16) & ~0xF000))
          return false;

        msg.Result = new IntPtr(1);

        return true;
      }
      return false;
		}
	}
}
