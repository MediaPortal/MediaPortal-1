#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Summary description for MameDevice.
  /// </summary>
  public class MameDevice : ISetupForm, IPluginReceiver
  {
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private InputHandler _inputHandler;

    public MameDevice()
    {
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Use your MAME input device to control MediaPortal";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return 0;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "mPod";
    }

    public string PluginName()
    {
      return "MAME Devices";
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      MappingForm dlg = new MappingForm("MameDevice");
      dlg.Show();
    }

    #endregion

    #region IPluginReceiver Members

    public bool WndProc(ref Message msg)
    {
      if (!_inputHandler.IsLoaded)
      {
        return false;
      }

      if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
      {
        //disabled: following code produces a stack overflow exception
        //when i start MP and simply press the cursor up arrow

        Log.Info("WM_KEYDOWN: wParam {0}", (int) msg.WParam);
        try
        {
          _inputHandler.MapAction((int) msg.WParam);
        }
        catch (ApplicationException)
        {
          return false;
        }
        msg.Result = IntPtr.Zero;
        return true;
      }
      return false;
    }

    #endregion

    #region IPlugin Members

    public void Start()
    {
      _inputHandler = new InputHandler("MameDevice");
      if (!_inputHandler.IsLoaded)
      {
        Log.Info("MameDevice: Error loading default mapping file - please reinstall MediaPortal");
      }
    }

    public void Stop()
    {
      // TODO:  Add MameDevice.Stop implementation
    }

    #endregion
  }
}