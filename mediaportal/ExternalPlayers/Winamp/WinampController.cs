#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Threading;
using System.Text;
using System.IO;
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
    public const Int32 HWND_TOPMOST    = -1;
    public const Int32 HWND_NOTOPMOST  = -2;

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr FindWindow(
			[MarshalAs(UnmanagedType.LPTStr)] string lpClassName,
			[MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);
		
		[StructLayout(LayoutKind.Sequential)]
			struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int cbData;
			[MarshalAs(UnmanagedType.LPStr)] public string lpData;
		}

    [DllImport("User32")]
    private static extern int SetForegroundWindow(IntPtr hwnd);

    // Activates a window
    [DllImportAttribute("User32.DLL")] 
    private static extern bool ShowWindow(IntPtr hWnd,int nCmdShow); 

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    private const int SW_SHOW = 5; 
    private const int SW_RESTORE = 9; 

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam,
			[In()] ref COPYDATASTRUCT lParam);
		
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static extern int SendMessageA(IntPtr hwnd, int wMsg, int wParam,
			int lParam);

		const int WM_COMMAND = 0x111;
    const int WA_CLOSE          = 40001; // close winamp
    const int WA_REPEAT         = 40022; // Toggle repeat
    const int WA_SHUFFLE        = 40023; // Toggle shuffle
    const int WA_EQ             = 40036; // Toggle EQ 
    const int WA_PREVTRACK      = 40044; // previous track 
    const int WA_PLAY           = 40045; // play current play list
    const int WA_PAUSE          = 40046; // pause/unpause
    const int WA_STOP           = 40047; // stop playing
    const int WA_NEXTTRACK      = 40048; // next track
    const int WA_VOLUMEUP       = 40058; // increase 1%
    const int WA_VOLUMEDOWN     = 40059; // Lower volume by 1%
    const int WA_FASTREWIND     = 40144; // Fast-rewind 5 seconds
    const int WA_FADESTOP		= 40147; // Fade out and stops
    const int WA_FASTFORW       = 40148; // Fast-forward 5 seconds.
    const int WA_STARTPLAYLIST  = 40154; // Start of playlist
    const int WA_STOPNEXT       = 40157; // stops after current track.
    const int WA_PLAYLISTEND    = 40158; // Go to end of playlist
    const int WA_BACK10         = 40197; // Moves back 10 tracks in playlist

    const int WM_COPYDATA = 0x004a; 
    const int WM_USER = 1024;//0x4A;
    const int WA_VERSION        = 0;    // Returns the version of Winamp.
    const int WA_NOTHING        = 0;
    const int WA_FILETOPLAYLIST = 100;  // Adds a file to playlist.
    const int WA_CLEARPLAYLIST  = 101;  // Clears playlist. 
    const int WA_PLAYTRACK      = 102;  // Begins play of selected track. 
    const int WA_GETSTATUS      = 104;  // Returns: playing=1, paused=3, stopped=all others 
    const int WA_POSITION       = 105;  // If data is 0, returns the position in milliseconds of the playing track
                                        // If data is 1, returns current track length in seconds.
                                        // Returns -1 if not playing or if an error occurs. 
    const int WA_SEEKPOS        = 106;  // Seeks within the current track by 'data' milliseconds
                                        // Returns -1 if not playing, 1 on eof, or 0 if successful
    const int WA_WRITEPLAYLIST  = 120;  // Writes out the current playlist to Winampdir\winamp.m3u, and returns
                                        // the current position in the playlist.
    const int WA_SETPLAYLISTPOS = 121;  // Sets the playlist position to 'data'.
    const int WA_SETVOLUME      = 122;  // Sets the volume to 'data' (0 to 255).
    const int WA_SETBALANCE     = 123;  // Sets the balance to 'data' (0 left to 255 right).
    const int WA_PLAYLISTLEN    = 124;  // Returns length of the current playlist, in tracks. 
    const int WA_PLAYLISTPOS    = 125;  // Returns current playlist position, in tracks. 
    const int WA_RESTART        = 135;  // Restarts Winamp.
		const int WA_REFRESHPLCACHE = 247;  // Clear PlayList Cache

		
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
      m_hwnd = FindWindow(m_windowName, null);
      if(m_hwnd.ToInt32() <= 0) // try to find it and start it since it's not found
      {
        IntPtr mpHwnd = GetActiveWindow();
        if(StartWinamp())
        {
          m_hwnd = FindWindow(m_windowName, null);
          if(m_hwnd.ToInt32() > 0)
            m_winampCreatedHere = true;
          ShowWindow(mpHwnd,SW_RESTORE); 		
          SetForegroundWindow(mpHwnd);
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
            System.Console.Out.WriteLine("directory = {0}", val);

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
          pluginWindow = FindWindow(m_windowName, null);
          if ( pluginWindow.ToInt32() > 0 ) // window handle was found
            break;
        }
        m_winampProcess = newProcess;
      }

      catch(Exception e)
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
        sr = new StreamReader(iniFile);

        //Read the first line of text
        string line = sr.ReadLine();

        //Continue to read until you reach end of file
        while (line != null) 
        {
          if(line.ToLower().Trim().StartsWith("minimized"))
          {
            if(line.ToLower().Trim().StartsWith("minimized=1"))
            {
              sr.Close();
              sr = null;
              return; // already in minimize... nothing to do...
            }
            else
              line = "minimized=1";
          }
          buff.Append(line);
          buff.Append("\r\n");
          //Read the next line
          line = sr.ReadLine();
        }
        //close the file
        sr.Close();
        sr = null;

        //Pass the filepath and filename to the StreamWriter Constructor
        sw = new StreamWriter(iniFile);

        //Write the file back
        sw.WriteLine(buff.ToString());

        //Close the file
        sw.Close();
        sw = null;
      }
      catch(Exception e)
      {
          Console.WriteLine("Exception: " + e.Message);
      }
      if(sr != null)
        sr.Close();
      if(sw != null)
        sw.Close();
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
			SendMessageA(m_hwnd, WM_COMMAND, WA_STOP, WA_NOTHING);
		}
		public void Play()  
		{			
			//IntPtr hwnd = FindWindow(m_windowName, null);			
			SendMessageA(m_hwnd, WM_COMMAND, WA_PLAY, WA_NOTHING);
		}
		public void Pause()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);			
			SendMessageA(m_hwnd, WM_COMMAND, WA_PAUSE, WA_NOTHING);

		}
		public void IncreaseVolume()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);			
			SendMessageA(m_hwnd, WM_USER, 0, WA_VOLUMEUP);
		}

		public void DecreaseVolume()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);			
			SendMessageA(m_hwnd, WM_USER, 0, WA_VOLUMEDOWN);
		}
		public void Restart()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);
			SendMessageA(m_hwnd, WM_USER, 0, WA_RESTART);

		}
		public void SetPlaylistPosition(int intPos)
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);			
			SendMessageA(m_hwnd, WM_USER, intPos, WA_SETPLAYLISTPOS);
		}
		public void ClearPlayList()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);			
			SendMessageA(m_hwnd, WM_USER, 0, WA_CLEARPLAYLIST);
		}
		public void ClearPlayListCache()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);
			SendMessageA(m_hwnd, WM_USER, 0, WA_REFRESHPLCACHE);			
		}
		public int GetPlayListLength()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);
			int length = SendMessageA(m_hwnd, WM_USER, 0, WA_PLAYLISTLEN);
			return length;
		}
		public void AppendToPlayList(string filename) 
		{				
			COPYDATASTRUCT cds;
			cds.dwData = (IntPtr) WA_FILETOPLAYLIST;
			cds.lpData = filename;
			cds.cbData = filename.Length + 1;
			
			//IntPtr hwnd = FindWindow(m_windowName, null);
			
			SendMessage(m_hwnd, WM_COPYDATA, 0, ref cds);
		}

		public int Status()
		{
			//IntPtr hwnd = FindWindow(m_windowName, null);			
			int status = SendMessageA(m_hwnd, WM_USER, 0, WA_GETSTATUS);
			return status;
		}
    public int GetCurrentSongDuration()
    {
        //IntPtr hwnd = FindWindow(m_windowName, null);			
        int duration = SendMessageA(m_hwnd, WM_USER, 1, WA_POSITION);
        return duration;
    }

    public double Position
    {
      set
      {
        //IntPtr hwnd = FindWindow(m_windowName, null);	
        SendMessageA(m_hwnd, WM_USER, (int)value, WA_SEEKPOS);
      }
      get
      {
        //IntPtr hwnd = FindWindow(m_windowName, null);			
        int position = SendMessageA(m_hwnd, WM_USER, 0, WA_POSITION) / 1000;
        return position;
      }
    }

    public int Volume
    {
      set
      {
        //IntPtr hwnd = FindWindow(m_windowName, null);	
        SendMessageA(m_hwnd, WM_USER, value, WA_SETVOLUME);
      }
    }
	}
	
}
