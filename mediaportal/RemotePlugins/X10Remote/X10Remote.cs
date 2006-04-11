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
    X10RemoteForm x10Form = null;
    InputHandler x10Handler = null;
    bool controlEnabled = false;
    bool logVerbose = false;
    bool x10Medion = true;
    bool x10Ati = false;
    bool x10UseChannelControl = false;
    int x10Channel = 0;

    public X10Remote()
    {
    }

    public void Init(IntPtr hwnd)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "X10", false);
        x10Medion = xmlreader.GetValueAsBool("remote", "X10Medion", true);
        x10Ati = xmlreader.GetValueAsBool("remote", "X10ATI", true);
        logVerbose = xmlreader.GetValueAsBool("remote", "X10VerboseLog", false);
        x10UseChannelControl = xmlreader.GetValueAsBool("remote", "X10UseChannelControl", false);
        x10Channel = xmlreader.GetValueAsInt("remote", "X10Channel", 0);
      }
      if (x10Handler == null)
      {
        if (controlEnabled)
          if (x10Medion)
            x10Handler = new InputHandler("Medion X10");
          else if (x10Ati)
            x10Handler = new InputHandler("ATI X10");
          else
            x10Handler = new InputHandler("Other X10");
        else
          return;

        if (logVerbose)
        {
          if (x10Medion)
            Log.Write("X10Remote: Start Medion");
          else if (x10Ati)
            Log.Write("X10Remote: Start ATI");
          else
            Log.Write("X10Remote: Start Other");
        }
      }
      if (x10Form == null)
      {
        try
        {
          x10Form = new X10RemoteForm(new AxX10._DIX10InterfaceEvents_X10CommandEventHandler(this.IX10_X10Command));
        }
        catch (System.Runtime.InteropServices.COMException)
        {
          controlEnabled = false;
          Log.Write("X10Remote: Can't initialize");
        }
      }
    }

    public void DeInit()
    {
      if (!controlEnabled)
        return;

      x10Form.Close();
      x10Form.Dispose();
      x10Form = null;

      x10Handler = null;

      if (logVerbose)
        Log.Write("X10Remote: Stop");
    }


    public void IX10_X10Command(object sender, AxX10._DIX10InterfaceEvents_X10CommandEvent e)
    {
      if (logVerbose)
      {
        Log.Write("X10Remote: Command Start --------------------------------------------");
        Log.Write("X10Remote: e            = {0}", e.ToString());
        Log.Write("X10Remote: bszCommand   = {0}", e.bszCommand.ToString());
        Log.Write("X10Remote: eCommand     = {0} - {1}", (int)Enum.Parse(typeof(X10.EX10Command), e.eCommand.ToString()), e.eCommand.ToString());
        Log.Write("X10Remote: eCommandType = {0}", e.eCommandType.ToString());
        Log.Write("X10Remote: eKeyState    = {0}", e.eKeyState.ToString());
        Log.Write("X10Remote: lAddress     = {0}", e.lAddress.ToString());
        Log.Write("X10Remote: lSequence    = {0}", e.lSequence.ToString());
        Log.Write("X10Remote: varTimestamp = {0}", e.varTimestamp.ToString());
        Log.Write("X10Remote: Command End ----------------------------------------------");
      }
      
      if (e.eKeyState.ToString() == "X10KEY_ON" || e.eKeyState.ToString() == "X10KEY_REPEAT")
      {
        if (x10UseChannelControl && (e.lAddress != x10Channel))
          return;

        try
        {
          x10Handler.MapAction((int)Enum.Parse(typeof(X10.EX10Command), e.eCommand.ToString()));
          if (logVerbose)
            Log.Write("X10Remote: Action mapped");
        }
        catch (ApplicationException)
        {
          if (logVerbose)
            Log.Write("X10Remote: Action not mapped");
        }
      }
    }
  }
}
