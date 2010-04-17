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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Summary description for WinLirc.
  /// </summary>
  public class WinLirc
  {
    public const Int32 TOGGLE_HIDEWINDOW = 0x080;
    public const Int32 TOGGLE_UNHIDEWINDOW = 0x040;
    public const Int32 HWND_TOPMOST = -1;
    public const Int32 HWND_NOTOPMOST = -2;

    private const int WM_COPYDATA = 0x004a;

    private const string TAB = "	";
    protected IntPtr m_hwnd = IntPtr.Zero;
    protected string m_windowName;
    protected string m_remote = "";
    protected string m_repeat = "0";
    protected string m_pathtowinlirc = "0";
    protected bool m_bEnabled = false;
    //protected bool m_bMultipleRemotes = true;
    //protected bool m_bNeedsEnter = false;
    protected bool m_bInitRetry = true;
    protected int m_IRdelay = 300;

    public WinLirc()
    {
      Init();
    }

    public bool Init()
    {
      Log.Info("Initialising WinLirc...");
      //load settings
      using (Settings xmlreader = new MPSettings())
      {
        m_bEnabled = xmlreader.GetValueAsString("WINLIRC", "enabled", "false") == "true";
        if (m_bEnabled == false)
        {
          return true;
        }
        m_pathtowinlirc = xmlreader.GetValueAsString("WINLIRC", "winlircpath", "");
        string delay = xmlreader.GetValueAsString("WINLIRC", "delay", "300");
        try
        {
          m_IRdelay = Int32.Parse(delay);
          if (m_IRdelay < 0 || m_IRdelay >= 1000)
          {
            m_IRdelay = 300;
          }
        }
        catch (Exception)
        {
          m_IRdelay = 300;
        }

        //m_bMultipleRemotes = xmlreader.GetValueAsString("WINLIRC", "use_multiple_remotes", "true") == "true";
        //m_remote = xmlreader.GetValueAsString("WINLIRC", "remote", "") ;
        //m_repeat = xmlreader.GetValueAsString("WINLIRC", "repeat", "0");
        //m_bNeedsEnter = xmlreader.GetValueAsString("WINLIRC", "needs_enter", "false") == "true";
      }

      //find winlirc
      m_windowName = "WinLIRC";
      m_hwnd = Win32API.FindWindow(null, m_windowName);

      //check we found it - if not, start it!
      if (m_hwnd.ToInt32() <= 0) // try to find it and start it since it's not found
      {
        Log.Info("WinLirc window not found, starting WinLirc");
        IntPtr mpHwnd = Win32API.GetActiveWindow(); //Get MP
        StartWinLirc(m_pathtowinlirc); //Start Winlirc
        Win32API.ShowWindow(mpHwnd, Win32API.ShowWindowFlags.Restore); //restore MP		
        Win32API.SetForegroundWindow(mpHwnd); //restore MP
      }
      if (m_hwnd.ToInt32() > 0)
      {
        Log.Info("Winlirc OK");
        return true;
      }
      Log.Info("Winlirc process not found");
      return false;
    }

    public bool StartWinLirc(string exeName)
    {
      if (exeName == null)
      {
        return false;
      }
      if (exeName == string.Empty)
      {
        return false;
      }
      ProcessStartInfo psI = new ProcessStartInfo(exeName, "");
      Process newProcess = new Process();

      try
      {
        newProcess.StartInfo.FileName = exeName;
        newProcess.StartInfo.Arguments = "";
        newProcess.StartInfo.UseShellExecute = true;
        newProcess.StartInfo.CreateNoWindow = true;
        //newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        newProcess.Start();

        for (int i = 0; i < 15; i++) // wait for up to 3 seconds for WinLirc to start.
        {
          Thread.Sleep(200);
          m_hwnd = Win32API.FindWindow(null, m_windowName);
          if (m_hwnd.ToInt32() > 0) // window handle was found
          {
            break;
          }
        }
      }
      catch (Exception)
      {
        Log.Info("Unable to start WinLIRC from {0}", exeName);
        return false;
      }
      return true;
    }

    public void ChangeTunerChannel(string channel_data)
    {
      //leave is not enabled
      if (m_bEnabled == false)
      {
        return;
      }

      try
      {
        if (channel_data == null)
        {
          return;
        }
        if (channel_data == string.Empty)
        {
          return;
        }
        if (m_hwnd.ToInt32() == 0)
        {
          Log.Info("WinLirc HWND is invalid. Check WinLirc is running");
          return;
        }

        //by default, use the remote set on config page
        string IRData;

        //our copy struct
        Win32API.COPYDATASTRUCT cds;

        string[] sets = channel_data.Split("|".ToCharArray());
        foreach (string command in sets)
        {
          //make up the Channel Change parts...
          //channelparts[0] will be name of remote
          //channelparts[1] will be repeat count
          //channelparts[2] will be code(s)
          string[] channelparts = { m_remote, m_repeat, command }; //default to using m_remote:m_repeat:command

          //now if channel_data has a ':', split that & use it instead!
          //NOTE: channel_data should be Remote:Repeat:Codes
          channelparts = command.Split(":".ToCharArray(), 3);

          if (channelparts.Length != 3)
          {
            Log.Info("WinLirc: '" + command +
                     "' is invalid.  Check External Channel follows the correct format (Remote:Repeat:Code 1,Code 2,Code n)");
            continue;
          }

          Log.Info("WinLirc ChangeTunerChannel: Remote; " + channelparts[0] + " Channel; " + channelparts[2]);

          //go thru chan numbers / commands & output to winLIRC
          string[] Ops = channelparts[2].Split(",".ToCharArray());
          foreach (string s in Ops)
          {
            if (s == "")
            {
              continue;
            }
            //IRData must be "remote+TAB+code+TAB+repeatcount"
            IRData = channelparts[0] + TAB + s + TAB + m_repeat;
            cds.dwData = (IntPtr)0;
            cds.lpData = Marshal.StringToHGlobalUni(IRData);
            cds.cbData = IRData.Length + 1;
            Win32API.SendMessage(m_hwnd, WM_COPYDATA, 0, ref cds);
            Thread.Sleep(m_IRdelay);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("Exception occured in winlirc plugin:{0}", ex.ToString());
      }
    }
  }
}