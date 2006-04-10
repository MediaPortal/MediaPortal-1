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
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.InputDevices
{
	public class HidListener
	{
    bool controlEnabled = false;
    bool logVerbose = false;           // Verbose logging
    InputHandler inputHandler;

    public void Init(IntPtr hwnd)
    {
      Init();
    }

    void Init()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HID", true);
        logVerbose = xmlreader.GetValueAsBool("remote", "HIDVerboseLog", false);
      }
      try
      {
        inputHandler = new InputHandler("General HID");
      }
      catch (System.IO.FileNotFoundException)
      {
        controlEnabled = false;
        Log.Write("HID: can't find default mapping file - reinstall MediaPortal");
      }
      catch (System.Xml.XmlException)
      {
        controlEnabled = false;
        Log.Write("HID: error in default mapping file - reinstall MediaPortal");
      }
      catch (System.ApplicationException)
      {
        controlEnabled = false;
        Log.Write("HID: version mismatch in default mapping file - reinstall MediaPortal");
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

			// we are only interested in WM_APPCOMMAND
			if(msg.Msg != 0x0319)
				return false;

			// find out which request the MCE remote handled last
			if((AppCommands)((msg.LParam.ToInt32() >> 16) & ~0xF000) == InputDevices.LastHidRequest)
			{
				// possible that it is the same request mapped to an app command?
				if(Environment.TickCount - InputDevices.LastHidRequestTick < 300)
					return true;
			}

			InputDevices.LastHidRequest = (AppCommands)((msg.LParam.ToInt32() >> 16) & ~0xF000);

      if (logVerbose) Log.Write("HID: Command: {0} - {1}", ((msg.LParam.ToInt32() >> 16) & ~0xF000), InputDevices.LastHidRequest.ToString());

      if (!inputHandler.MapAction((msg.LParam.ToInt32() >> 16) & ~0xF000))
        return false;

			msg.Result = new IntPtr(1);

			return true;
		}
	}
}
