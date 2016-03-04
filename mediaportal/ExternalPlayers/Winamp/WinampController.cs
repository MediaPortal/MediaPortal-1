#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MediaPortal.Util;
using Microsoft.Win32;

namespace MediaPortal.WinampPlayer
{
  /// <summary>
  /// Summary description for winampcontroller.
  /// </summary>
  public class WinampController : MarshalByRefObject, IDisposable
  {
    public const Int32 TOGGLE_HIDEWINDOW = 0x080;
    public const Int32 TOGGLE_UNHIDEWINDOW = 0x040;
    public const Int32 HWND_TOPMOST = -1;
    public const Int32 HWND_NOTOPMOST = -2;

    private const int WM_COMMAND = 0x111;
    private const int WA_CLOSE = 40001; // close winamp
    private const int WA_REPEAT = 40022; // Toggle repeat
    private const int WA_SHUFFLE = 40023; // Toggle shuffle
    private const int WA_EQ = 40036; // Toggle EQ 
    private const int WA_PREVTRACK = 40044; // previous track 
    private const int WA_PLAY = 40045; // play current play list
    private const int WA_PAUSE = 40046; // pause/unpause
    private const int WA_STOP = 40047; // stop playing
    private const int WA_NEXTTRACK = 40048; // next track
    private const int WA_VOLUMEUP = 40058; // increase 1%
    private const int WA_VOLUMEDOWN = 40059; // Lower volume by 1%
    private const int WA_FASTREWIND = 40144; // Fast-rewind 5 seconds
    private const int WA_FADESTOP = 40147; // Fade out and stops
    private const int WA_FASTFORW = 40148; // Fast-forward 5 seconds.
    private const int WA_STARTPLAYLIST = 40154; // Start of playlist
    private const int WA_STOPNEXT = 40157; // stops after current track.
    private const int WA_PLAYLISTEND = 40158; // Go to end of playlist
    private const int WA_BACK10 = 40197; // Moves back 10 tracks in playlist

    private const int WM_COPYDATA = 0x004a;
    private const int WM_USER = 1024; //0x4A;
    private const int WA_VERSION = 0; // Returns the version of Winamp.
    private const int WA_NOTHING = 0;
    private const int WA_FILETOPLAYLIST = 1100; // Adds a file to playlist.
    private const int WA_CLEARPLAYLIST = 101; // Clears playlist. 
    private const int WA_PLAYTRACK = 102; // Begins play of selected track. 
    private const int WA_GETSTATUS = 104; // Returns: playing=1, paused=3, stopped=all others 
    private const int WA_POSITION = 105; // If data is 0, returns the position in milliseconds of the playing track
    // If data is 1, returns current track length in seconds.
    // Returns -1 if not playing or if an error occurs. 
    private const int WA_SEEKPOS = 106; // Seeks within the current track by 'data' milliseconds
    // Returns -1 if not playing, 1 on eof, or 0 if successful
    private const int WA_WRITEPLAYLIST = 120; // Writes out the current playlist to Winampdir\winamp.m3u, and returns
    // the current position in the playlist.
    private const int WA_SETPLAYLISTPOS = 121; // Sets the playlist position to 'data'.
    private const int WA_SETVOLUME = 122; // Sets the volume to 'data' (0 to 255).
    private const int WA_SETBALANCE = 123; // Sets the balance to 'data' (0 left to 255 right).
    private const int WA_PLAYLISTLEN = 124; // Returns length of the current playlist, in tracks. 
    private const int WA_PLAYLISTPOS = 125; // Returns current playlist position, in tracks. 
    private const int WA_TRACKINFO = 126; // Retrieves info about the current playing track. Returns samplerate (i.e. 44100) if 'data' is set to 0, bitrate if 'data' is set to 1, and number of channels if 'data' is set to 2. (requires Winamp 2.05+)
    private const int WA_RESTART = 135; // Restarts Winamp.
    private const int WA_REFRESHPLCACHE = 247; // Clear PlayList Cache


    public const int STOPPED = 0;
    public const int PLAYING = 1;
    public const int PAUSED = 3;

    public const string WINAMPEXE = "winamp";
    public const string WINAMPINI = "winamp.ini";

    protected string m_windowName;
    protected IntPtr m_hwnd = IntPtr.Zero;
    protected Process m_winampProcess;
    protected bool m_winampCreatedHere = false;

    public WinampController()
    {
      m_windowName = "Winamp v1.x";
      m_hwnd = Win32API.FindWindow(m_windowName, null);
      if (m_hwnd.ToInt32() <= 0) // try to find it and start it since it's not found
      {
        IntPtr mpHwnd = Win32API.GetActiveWindow();
        if (StartWinamp())
        {
          m_hwnd = Win32API.FindWindow(m_windowName, null);
          if (m_hwnd.ToInt32() > 0)
          {
            m_winampCreatedHere = true;
          }
          Win32API.ShowWindow(mpHwnd, Win32API.ShowWindowFlags.Restore);
          Win32API.SetForegroundWindow(mpHwnd);
        }
      }
    }

    ~WinampController()
    {
      //Dispose();
    }

    private bool StartWinamp()
    {
      using (RegistryKey pRegKey = Registry.CurrentUser)
      {
        using (RegistryKey subkey = pRegKey.OpenSubKey("Software\\Winamp"))
        {
          Object val = subkey.GetValue("");
          if (val.ToString().Trim().Length > 0)
          {
            Console.Out.WriteLine("directory = {0}", val);

            ChangeIniToMinimize(val + "\\" + WINAMPINI);
            RunProgram(val + "\\" + WINAMPEXE, "");
            return true;
          }
        }
      }
      return false;
    }

    private void RunProgram(string exeName, string argsLine)
    {
      ProcessStartInfo psI = new ProcessStartInfo(exeName, argsLine);
      Process newProcess = new Process();

      try
      {
        newProcess.StartInfo.FileName = exeName;
        newProcess.StartInfo.Arguments = argsLine;
        newProcess.StartInfo.UseShellExecute = true;
        newProcess.StartInfo.CreateNoWindow = true;
        //newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        newProcess.Start();

        IntPtr pluginWindow;
        for (int i = 0; i < 15; i++) // wait for up to 3 seconds for Foobar2k to start.
        {
          Thread.Sleep(200);
          pluginWindow = Win32API.FindWindow(m_windowName, null);
          if (pluginWindow.ToInt32() > 0) // window handle was found
          {
            break;
          }
        }
        m_winampProcess = newProcess;
      }

      catch (Exception e)
      {
        throw e;
      }
    }

    private void ChangeIniToMinimize(string iniFile)
    {
      StringBuilder buff = new StringBuilder();
      StreamReader sr = null;
      StreamWriter sw = null;

      try
      {
        //Pass the file path and file name to the StreamReader constructor
        string line = "";
        using (sr = new StreamReader(iniFile))
        {
          //Read the first line of text
          line = sr.ReadLine();

          //Continue to read until you reach end of file
          while (line != null)
          {
            if (line.ToLowerInvariant().Trim().StartsWith("minimized"))
            {
              if (line.ToLowerInvariant().Trim().StartsWith("minimized=1"))
              {
                sr.Close();
                return; // already in minimize... nothing to do...
              }
              else
              {
                line = "minimized=1";
              }
            }
            buff.Append(line);
            buff.Append("\r\n");
            //Read the next line
            line = sr.ReadLine();
          }
          //close the file
          sr.Close();
        }
        //Pass the filepath and filename to the StreamWriter Constructor
        using (sw = new StreamWriter(iniFile))
        {
          //Write the file back
          sw.WriteLine(buff.ToString());
          //Close the file
          sw.Close();
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("Exception: " + e.Message);
      }
      if (sr != null)
      {
        sr.Close();
      }
      if (sw != null)
      {
        sw.Close();
      }
    }

    public void Dispose()
    {
      /*
        if(m_winampProcess != null)
            m_winampProcess.Close();
        if(m_hwnd.ToInt32() > 0 && m_winampCreatedHere)
            SendMessageA(m_hwnd, WM_COMMAND, WA_CLOSE, WA_NOTHING); // close winamp
      */
    }

    public void Stop()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      Win32API.SendMessageA(m_hwnd, WM_COMMAND, WA_STOP, WA_NOTHING);
    }

    public void Play()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      Win32API.SendMessageA(m_hwnd, WM_COMMAND, WA_PLAY, WA_NOTHING);
    }

    public void Pause()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      Win32API.SendMessageA(m_hwnd, WM_COMMAND, WA_PAUSE, WA_NOTHING);
    }

    public void IncreaseVolume()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_VOLUMEUP);
    }

    public void DecreaseVolume()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_VOLUMEDOWN);
    }

    public void Restart()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);
      Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_RESTART);
    }

    public void SetPlaylistPosition(int intPos)
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      Win32API.SendMessageA(m_hwnd, WM_USER, intPos, WA_SETPLAYLISTPOS);
    }

    public void ClearPlayList()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_CLEARPLAYLIST);
    }

    public void ClearPlayListCache()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);
      Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_REFRESHPLCACHE);
    }

    public int GetPlayListLength()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);
      int length = Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_PLAYLISTLEN);
      return length;
    }

    public void AppendToPlayList(string filename)
    {
      Win32API.COPYDATASTRUCT cds;
      cds.dwData = (IntPtr)WA_FILETOPLAYLIST;
      cds.lpData = Marshal.StringToHGlobalUni(filename);
      cds.cbData = 2 * (filename.Length + 1);
      IntPtr songMemory = cds.lpData;

      Win32API.SendMessage(m_hwnd, WM_COPYDATA, 0, ref cds);
      Marshal.FreeHGlobal(songMemory);
    }

    public int Status()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      int status = Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_GETSTATUS);
      return status;
    }

    public int GetCurrentSongDuration()
    {
      //IntPtr hwnd = FindWindow(m_windowName, null);			
      int duration = Win32API.SendMessageA(m_hwnd, WM_USER, 1, WA_POSITION);
      return duration;
    }

    public double Position
    {
      set
      {
        //IntPtr hwnd = FindWindow(m_windowName, null);	
        Win32API.SendMessageA(m_hwnd, WM_USER, (int)value, WA_SEEKPOS);
      }
      get
      {
        //IntPtr hwnd = FindWindow(m_windowName, null);			
        int position = Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_POSITION) / 1000;
        return position;
      }
    }

    public int Volume
    {
      set
      {
        //IntPtr hwnd = FindWindow(m_windowName, null);	
        Win32API.SendMessageA(m_hwnd, WM_USER, value, WA_SETVOLUME);
      }
    }

    public int Bitrate
    {
      get { return Win32API.SendMessageA(m_hwnd, WM_USER, 1, WA_TRACKINFO); }
    }

    public int SampleRate
    {
      get { return Win32API.SendMessageA(m_hwnd, WM_USER, 0, WA_TRACKINFO); }
    }

    public int Channels
    {
      get { return Win32API.SendMessageA(m_hwnd, WM_USER, 2, WA_TRACKINFO); }
    }
  }
}