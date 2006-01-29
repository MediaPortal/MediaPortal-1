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


namespace MediaPortal.InputDevices
{
  /// <summary>
  /// 
  /// </summary>
  public class X10Remote
  {
    X10RemoteForm x10Form;
    InputHandler x10Handler;
    bool controlEnabled = false;
    bool logVerbose = false;

    public X10Remote()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "x10", false);
        logVerbose = xmlreader.GetValueAsBool("remote", "x10VerboseLog", false);
      }
      if (controlEnabled)
        x10Handler = new InputHandler("x10");
    }

    public void Init(IntPtr hwnd)
    {
      if (!controlEnabled)
        return;

      if (logVerbose)
        Log.Write("x10Remote: Start");
      try
      {
        x10Form = new X10RemoteForm(new AxX10._DIX10InterfaceEvents_X10CommandEventHandler(this.IX10_X10Command));
      }
      catch (System.Runtime.InteropServices.COMException)
      {
        controlEnabled = false;
        Log.Write("x10Remote: Can't initialize");
      }
    }

    public void DeInit()
    {
      if (!controlEnabled)
        return;

      x10Form.Close();
      x10Form.Dispose();
      x10Form = null;
      if (logVerbose)
        Log.Write("x10Remote: Stop");
    }


    public void IX10_X10Command(object sender, AxX10._DIX10InterfaceEvents_X10CommandEvent e)
    {
      if (e.eKeyState.ToString() == "X10KEY_ON" || e.eKeyState.ToString() == "X10KEY_REPEAT")
      {
        x10Handler.MapAction((int)Enum.Parse(typeof(X10.EX10Command), e.eCommand.ToString()));
        if (logVerbose)
          Log.Write("x10Remote: Action mapped");
      }
      if (logVerbose)
      {
        Log.Write("x10Remote: Command Start --------------------------------------------");
        Log.Write("x10Remote: e            = {0}", e.ToString());
        Log.Write("x10Remote: bszCommand   = {0}", e.bszCommand.ToString());
        Log.Write("x10Remote: eCommand     = {0} - {1}", (int)Enum.Parse(typeof(X10.EX10Command), e.eCommand.ToString()), e.eCommand.ToString());
        Log.Write("x10Remote: eCommandType = {0}", e.eCommandType.ToString());
        Log.Write("x10Remote: eKeyState    = {0}", e.eKeyState.ToString());
        Log.Write("x10Remote: lAddress     = {0}", e.lAddress.ToString());
        Log.Write("x10Remote: lSequence    = {0}", e.lSequence.ToString());
        Log.Write("x10Remote: varTimestamp = {0}", e.varTimestamp.ToString());
        Log.Write("x10Remote: Command End ----------------------------------------------");
      }
    }
  }
}
