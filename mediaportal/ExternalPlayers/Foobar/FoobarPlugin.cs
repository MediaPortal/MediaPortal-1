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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace MediaPortal.FoobarPlugin
{
  /// <summary>
  /// Foobar plugin class
  /// </summary>
  [PluginIcons("ExternalPlayers.Foobar.foobarlogo.png", "ExternalPlayers.Foobar.foobarlogodisabled.png")]
  public class FoobarPlugin : IExternalPlayer
  {
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(
      [MarshalAs(UnmanagedType.LPTStr)] string lpClassName,
      [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

    [StructLayout(LayoutKind.Sequential)]
    private struct COPYDATASTRUCT
    {
      public IntPtr dwData;
      public int cbData;
      public IntPtr lpData;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam,
                                          [In()] ref COPYDATASTRUCT lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessageA(IntPtr hwnd, int wMsg, int wParam,
                                           int lParam);

    private const int WM_HTTPSERVER_MSG_CMD = 0x8898;
    private const int WM_HTTPSERVER_MSG_GETSTATE = 0x00;
    private const int WM_HTTPSERVER_MSG_GETPLAYLENGTH = 0x01;
    private const int WM_HTTPSERVER_MSG_GETPLAYBACKTIME = 0x02;
    private const int WM_HTTPSERVER_MSG_STOP = 0x03;
    private const int WM_HTTPSERVER_MSG_PAUSE = 0x04;
    private const int WM_HTTPSERVER_MSG_SETVOLUME = 0x05;
    private const int WM_HTTPSERVER_MSG_GETVOLUME = 0x06;
    private const int WM_HTTPSERVER_MSG_SEEK = 0x07;
    private const int WM_HTTPSERVER_MSG_SETACTIVEPLAYLIST = 0x08;
    private const int WM_HTTPSERVER_MSG_CLEARPLAYLIST = 0x09;
    private const int WM_COPYDATA = 0x004a;

    [DllImport("User32")]
    private static extern int SetForegroundWindow(IntPtr hwnd);

    // Activates a window
    [DllImport("User32.DLL")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;

    private const string m_author = "int_20h/rtv";
    private const string m_player = "Foobar2000";
    private const string m_version = "2.0b";

    // set in configuration
    private string[] m_supportedExtensions = new string[0];
    private string m_execPath = null;
    private string m_hostname = "localhost";
    private int m_port = 8989;
    private string m_startupparameter = null;
    private string m_windowName = null;

    // internally maintain
    private string m_strCurrentFile = null;

    private Process m_foobarProcess = null;
    private IntPtr m_hwnd = IntPtr.Zero;
    private IntPtr m_hwndPlugin = IntPtr.Zero;
    private bool m_bStoppedManualy = false;
    private bool _notifyPlaying = false;
    private bool _isCDA = false;

    /// <summary>
    /// Empty constructor.  Nothing to initialize 
    /// </summary>
    public FoobarPlugin()
    {
      //Thread.CurrentThread.Name = "FoobarPlugin";
      // empty constructor
    }

    /// <summary>
    /// Starts the player if it is not running...
    /// </summary>
    private void startPlayerIfNecessary()
    {
      if (m_execPath != null && m_execPath.Length > 0)
      {
        if (m_windowName != null && m_windowName.Length > 0)
        {
          m_hwnd = FindWindow(null, m_windowName);
          m_hwndPlugin = FindWindow("foo_httpserver_ctrl", null);
        }
        if (m_hwnd.ToInt32() <= 0) // try to find it and start it since it's not found
        {
          IntPtr mpHwnd = GetActiveWindow();
          RunProgram(m_execPath, m_startupparameter);
          Log.Info("ExternalPlayers: Started foobar2000 with {0}", m_startupparameter);
          ShowWindow(mpHwnd, SW_RESTORE);
          SetForegroundWindow(mpHwnd);
          m_hwnd = FindWindow(null, m_windowName);
          m_hwndPlugin = FindWindow("foo_httpserver_ctrl", null);
        }
      }
    }


    /// <summary>
    /// Runs a particular program in the local file system
    /// </summary>
    /// <param name="exeName"></param>
    /// <param name="argsLine"></param>
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
        newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
        newProcess.Start();

        IntPtr httpPluginWindow;
        for (int i = 0; i < (2*6); i++) // wait 6 seconds for Foobar2k to start.
        {
          Thread.Sleep(500);
          httpPluginWindow = FindWindow("foo_httpserver_ctrl", null);
          if (httpPluginWindow.ToInt32() > 0) // window handle was found
          {
            break;
          }
        }
        m_foobarProcess = newProcess;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Read the configuration file for parameters needed for the plugin to function correctly
    /// </summary>
    private void readConfig()
    {
      string strExt = null;
      string execPath = null;
      string hostname = null;
      string port = null;
      string windowName = null;
      string startupparameter = null;

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        // extensions to play by this player
        strExt = xmlreader.GetValueAsString("foobarplugin", "enabledextensions", "");
        // where is foobar executable
        execPath = xmlreader.GetValueAsString("foobarplugin", "path", "");
        // which host to talk to
        hostname = xmlreader.GetValueAsString("foobarplugin", "host", "localhost");
        // which port to talk to
        port = xmlreader.GetValueAsString("foobarplugin", "port", "8989");
        // what's the window name of the program
        windowName = xmlreader.GetValueAsString("foobarplugin", "windowname", "");
        // additional startup options
        startupparameter = xmlreader.GetValueAsString("foobarplugin", "startupparameter",
                                                      " /hide /command:\"Playback/ReplayGain/Album\"");
      }
      if (strExt != null && strExt.Length > 0)
      {
        m_supportedExtensions = strExt.Split(new char[] {','});
      }
      if (execPath != null && execPath.Length > 0)
      {
        m_execPath = execPath;
      }
      if (hostname != null && hostname.Length > 0)
      {
        m_hostname = hostname;
      }
      if (port != null && port.Length > 0)
      {
        try
        {
          m_port = Convert.ToInt32(port);
        }
        catch
        {
        }
      }
      if (startupparameter != null && startupparameter.Length > 0)
      {
        m_startupparameter = startupparameter;
      }
      if (windowName != null && windowName.Length > 0)
      {
        m_windowName = windowName;
      }
      else
      {
        // lets figure it out by openning the installer.ini file and look at the version.
        // The window name is "foobar2000 v<version>" so, if we need to find the window,
        // we need the version number!
        StringBuilder buff = new StringBuilder();
        StreamReader sr = null;
        string version = null;

        try
        {
          string path = m_execPath.ToLower();
          int index = path.LastIndexOf("foobar");
          string iniFile = m_execPath.Substring(0, index) + "installer.ini";

          //Pass the file path and file name to the StreamReader constructor
          sr = new StreamReader(iniFile);
          //Read the first line of text
          string line = sr.ReadLine();
          //Continue to read until you reach end of file

          while (line != null)
          {
            if (line.ToLower().Trim().StartsWith("version"))
            {
              index = line.IndexOf("=");
              version = line.Substring(index + 1).Trim();
            }
            //Read the next line
            line = sr.ReadLine();
          }
          //close the file
          sr.Close();
          sr = null;
        }
        catch (Exception e)
        {
          Console.WriteLine("Exception: " + e.Message);
        }
        if (sr != null)
        {
          sr.Close();
          sr = null;
        }

        if (version != null)
        {
          m_windowName = "foobar2000 v" + version;
        }
      }
    }

    /// <summary>
    /// This method performs the GET method via HTTP to request a specific page
    /// </summary>
    /// <param name="strURL">The URL to get a request from</param>
    /// <returns></returns>
    private string GetHTTP(string strURL)
    {
      string retval = null;

      // Initialize the WebRequest.
      WebRequest myRequest = WebRequest.Create(strURL);

      // Return the response. 
      WebResponse myResponse = myRequest.GetResponse();

      Stream ReceiveStream = myResponse.GetResponseStream();

      // 1252 is encoding for Windows format
      Encoding encode = Encoding.GetEncoding(1252);
      StreamReader sr = new StreamReader(ReceiveStream, encode);
      retval = sr.ReadToEnd();

      // Close the response to free resources.
      myResponse.Close();

      return retval;
    }

    /// <summary>
    /// Executes the given command and pass the given parameters to the foobar httpserver plugin
    /// </summary>
    /// <param name="command">The command to execute in foobar</param>
    /// <param name="param1">The first parameter if necessary</param>
    /// <param name="param2">The second parameter if necessary</param>
    /// <returns></returns>
    private string ExecuteCommand(string command, string param1, string param2)
    {
      if (command == null || command.Length == 0)
      {
        return "";
      }
      if (m_execPath == null || m_execPath.Length == 0)
      {
        return "ERROR: MP plugin not configured";
      }

      string retval = null;
      string url = "http://" + m_hostname + ":" + m_port + "/?cmd=" + command + "&param1=" + param1 + "&param2=" +
                   param2;
      string response = GetHTTP(url);

      // the command is good and was executed correctly
      if (response.IndexOf("cmd=" + command) != -1)
      {
        string val = "retval=";
        int cmdValueIndex = response.IndexOf(val) + val.Length;
        retval = response.Substring(cmdValueIndex).Trim();
      }
      else
      {
        retval = "ERROR: mismatch command";
      }
      return retval;
    }

    /// <summary>
    /// Supported extensions are defined in the configuration of the plugin.  Therefore,
    /// this is taken from the configuration file of MediaPortal to what the user
    /// defines it
    /// </summary>
    /// <returns>A string array of all the extensions supported (i.e. .mp3 .cda .mid</returns>
    public override string[] GetAllSupportedExtensions()
    {
      readConfig();
      return m_supportedExtensions;
    }

    /// <summary>
    /// This method is called by the plugin screen to show the configuration for the foobar plugin
    /// </summary>
    public override void ShowPlugin()
    {
      FoobarConfigForm confForm = new FoobarConfigForm();
      confForm.ShowDialog();
    }

    /// <summary>
    /// Description property
    /// </summary>
    /// <returns></returns>
    public override string Description()
    {
      if (m_supportedExtensions.Length == 0)
      {
        return "Advanced audio player - http://www.foobar2000.org";
      }
      return base.Description();
    }

    /// <summary>
    /// Author property.  Only Get is defined.
    /// </summary>
    public override string AuthorName
    {
      get { return m_author; }
    }


    /// <summary>
    /// Player name property.  Only Get is defined.
    /// </summary>
    public override string PlayerName
    {
      get { return m_player; }
    }

    /// <summary>
    /// Version number property
    /// </summary>
    public override string VersionNumber
    {
      get { return m_version; }
    }

    /// <summary>
    /// This method is called to find out if this plugin supports a given filename.  The
    /// method uses the extension to figure out if it supports it
    /// </summary>
    /// <param name="filename">The filename to check if the plugin supports it.</param>
    /// <returns>True or false depending if the file is supported or not</returns>
    public override bool SupportsFile(string filename)
    {
      readConfig();
      string ext = null;
      int dot = filename.LastIndexOf("."); // couldn't find the dot to get the extension
      if (dot == -1)
      {
        return false;
      }

      ext = filename.Substring(dot).Trim();
      if (ext.Length == 0)
      {
        return false; // no extension so return false;
      }

      ext = ext.ToLower();

      for (int i = 0; i < m_supportedExtensions.Length; i++)
      {
        if (m_supportedExtensions[i].Equals(ext))
        {
          return true;
        }
      }
      // could not match the extension, so return false;
      return false;
    }

    /// <summary>
    /// Play the given file
    /// </summary>
    /// <param name="strFile"></param>
    /// <returns></returns>
    public override bool Play(string strFile)
    {
      // stop other media which might be active until now.
      if (g_Player.Playing)
      {
        g_Player.Stop();
      }


      _isCDA = false;
      if (strFile.IndexOf(".cda") >= 0)
      {
        string strTrack = "";
        int pos = strFile.IndexOf(".cda");
        if (pos >= 0)
        {
          pos--;
          while (Char.IsDigit(strFile[pos]) && pos > 0)
          {
            strTrack = strFile[pos] + strTrack;
            pos--;
          }
        }

        string strDrive = strFile.Substring(0, 1);
        strDrive += ":";
        strFile = String.Format("{0}Track{1}.cda", strDrive, strTrack);
        _isCDA = true;
      }
      try
      {
        m_bStoppedManualy = false;
        // check if the player is running, if not run it.
        startPlayerIfNecessary();

        string playListName = "MPPlaylist";
        COPYDATASTRUCT cds;
        cds.dwData = (IntPtr) 1; // find/create playlist
        cds.lpData = Marshal.StringToCoTaskMemAnsi(playListName);
        cds.cbData = playListName.Length + 1;

        // create the playlist or switch to it if there
        int index = SendMessage(m_hwndPlugin, WM_COPYDATA, 0, ref cds);
        Marshal.FreeCoTaskMem(cds.lpData);

        // set the playlist as the active one
        SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_SETACTIVEPLAYLIST, index);

        // Clear playlist
        SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_CLEARPLAYLIST, 0);


        Encoding encode = Encoding.Default;
        // Let's encode the filename so any extended ASCII character is played correctly
        // by the Foobar Plugin
        byte[] byData = encode.GetBytes(HttpUtility.UrlEncode(strFile).Replace("+", "%20"));
        cds.dwData = (IntPtr) 0; // Play song
        cds.lpData = Marshal.AllocCoTaskMem(byData.Length + 1);
        cds.cbData = byData.Length + 1;
        // write all the bytes to the lpData byte by byte
        for (int i = 0; i < byData.Length; ++i)
        {
          Marshal.WriteByte(cds.lpData, i, byData[i]);
        }
        // write the end of string '\0'
        Marshal.WriteByte(cds.lpData, byData.Length, (byte) 0);
        if (SendMessage(m_hwndPlugin, WM_COPYDATA, 0, ref cds) == 0)
        {
          Marshal.FreeCoTaskMem(cds.lpData);
          Thread.Sleep(1000); // wait for 1 secs so that foobar starts playing
          m_strCurrentFile = strFile;
          _notifyPlaying = true;
          return true;
        }
        else
        {
          _notifyPlaying = false;
          Marshal.FreeCoTaskMem(cds.lpData);
          return false;
        }
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// This method returns the Duration of the current song that is been played
    /// </summary>
    public override double Duration
    {
      get { return SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_GETPLAYLENGTH, 0); }
    }

    /// <summary>
    /// This method returns the current play back time of the song
    /// </summary>
    public override double CurrentPosition
    {
      get { return SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_GETPLAYBACKTIME, 0); }
    }


    /// <summary>
    /// This method pauses the currently played song
    /// </summary>
    public override void Pause()
    {
      // pause
      SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_PAUSE, 0);
    }

    /// <summary>
    /// This method returns if the player is in paused state
    /// </summary>
    public override bool Paused
    {
      get
      {
        int state = SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_GETSTATE, 0);
        return (state == 1);
      }
    }


    /// <summary>
    /// This method returns if the player is in playing state
    /// </summary>
    public override bool Playing
    {
      get
      {
        int state = SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_GETSTATE, 0);
        return (state == 0);
      }
    }


    public override bool Ended
    {
      get
      {
        if (m_bStoppedManualy)
        {
          return false;
        }

        int state = SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_GETSTATE, 0);
        return (state == 2);
      }
    }


    /// <summary>
    /// This method returns if the player is in stop state
    /// </summary>
    public override bool Stopped
    {
      get
      {
        if (!m_bStoppedManualy)
        {
          return false;
        }
        int state = SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_GETSTATE, 0);
        return (state == 2);
      }
    }


    /// <summary>
    /// This method returns the file that is been played
    /// </summary>
    public override string CurrentFile
    {
      get { return m_strCurrentFile; }
    }

    /// <summary>
    /// This method stops the player from the played song
    /// </summary>
    public override void Stop()
    {
      m_bStoppedManualy = true;
      SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_STOP, 0);
      _notifyPlaying = false;
    }


    /// <summary>
    /// Volume property for the player
    /// </summary>
    public override int Volume
    {
      // GetVolume
      get { return SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_GETVOLUME, 0); }
      set { SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_SETVOLUME, value); }
    }

    /// <summary>
    /// Seek a relative time in the song from the current position
    /// </summary>
    /// <param name="dTime">time in seconds</param>
    public override void SeekRelative(double dTime)
    {
      try
      {
        double dCurTime = CurrentPosition;
        dTime = dCurTime + dTime;
        if (dTime < 0.0d)
        {
          dTime = 0.0d;
        }
        if (dTime < Duration)
        {
          SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_SEEK, (int) dTime);
        }
      }
      catch
      {
      }
    }

    /// <summary>
    /// Seek a time in the song from the beginning of the song
    /// </summary>
    /// <param name="dTime">time in seconds</param>
    public override void SeekAbsolute(double dTime)
    {
      try
      {
        // seek
        if (dTime < 0.0d)
        {
          dTime = 0.0d;
        }
        if (dTime < Duration)
        {
          SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_SEEK, (int) dTime);
        }
      }
      catch
      {
      }
    }

    /// <summary>
    /// Seek a relative time in the song from the current position as a percentage
    /// </summary>
    /// <param name="iPercentage">percentage amount</param>
    public override void SeekRelativePercentage(int iPercentage)
    {
      double dCurrentPos = CurrentPosition;
      double dDuration = Duration;

      double fCurPercent = (dCurrentPos/Duration)*100.0d;
      double fOnePercent = Duration/100.0d;
      fCurPercent = fCurPercent + (double) iPercentage;
      fCurPercent *= fOnePercent;
      if (fCurPercent < 0.0d)
      {
        fCurPercent = 0.0d;
      }
      if (fCurPercent < Duration)
      {
        try
        {
          SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_SEEK, (int) fCurPercent);
        }
        catch
        {
        }
      }
    }

    /// <summary>
    /// Seek a time in the song from the beginning of the song as a percentage
    /// </summary>
    /// <param name="iPercentage">percentage amount</param>
    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (iPercentage < 0)
      {
        iPercentage = 0;
      }
      if (iPercentage >= 100)
      {
        iPercentage = 100;
      }
      double fPercent = Duration/100.0f;
      fPercent *= (double) iPercentage;
      try
      {
        SendMessageA(m_hwndPlugin, WM_HTTPSERVER_MSG_CMD, WM_HTTPSERVER_MSG_SEEK, (int) fPercent);
      }
      catch
      {
      }
    }


    /// <summary>
    /// The main entry point for testing this class stand alone.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      //
      // TODO: Add code to start application here
      //
      FoobarPlugin plugin = new FoobarPlugin();
      plugin.Stop();
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }

      if (_notifyPlaying && CurrentPosition >= 10.0)
      {
        _notifyPlaying = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
        msg.Label = CurrentFile;
        GUIWindowManager.SendThreadMessage(msg);
      }
    }

    public override bool IsCDA
    {
      get { return _isCDA; }
    }
  } // public class FoobarPlugin : MediaPortal.Player.IExternalPlayer
}