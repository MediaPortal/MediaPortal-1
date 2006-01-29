/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Microsoft.Win32;
using Message=ProcessPlugins.ExternalDisplay.Setting.Message;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This plug-in can show status information on an external display like LCD, VFD, ...
  /// </summary>
  /// <author>JoeDalton</author>
  public class ExternalDisplay : IPlugin, ISetupForm
  {
    private const int WindowID = 9876; //The ID for this plugin
    private IDisplay display = null;
    private bool stopRequested = false;
    private DisplayHandler handler;
    private Status status = Status.Idle;
    private DateTime lastAction = DateTime.MinValue; //Keeps track of when last action occurred
    private PropertyBrowser browser = null;
    private Thread t;

    #region IPlugin implementation

    /// <summary>
    /// This method will be called by mediaportal to start our process plugin
    /// </summary>
    public void Start()
    {
      //Subscribe to the PowerModeChanged event
      SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
      if (Settings.Instance.ShowPropertyBrowser)
      {
        browser = new PropertyBrowser();
        browser.Show();
      }
      DoStart();
    }

    private void DoStart()
    {
      if (t != null && t.IsAlive) //Already started?
      {
        return;
      }
      Log.Write("ExternalDisplay plugin starting...");
      try
      {
        //Initialize display
        display = Settings.Instance.LCDType;
        if (display == null)
        {
          Log.Write("ExternalDisplay: Requested display type not found.  Plugin not started!!!");
          return;
        }
        if (display is LCDHypeWrapper && !VerifyDriverLynxDriver())
        {
          Log.Write(
            "ExternalDisplay: DriverLYNX Port I/O Driver (needed for the choosen display type) not detected.  Plugin not started!");
          return;
        }
        display.Initialize(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth,
                           Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight,
                           Settings.Instance.GraphicWidth, Settings.Instance.GraphicComDelay,
                           Settings.Instance.BackLight, Settings.Instance.Contrast);
        //Start the background thread
        stopRequested = false;
        t = new Thread(new ThreadStart(Run));
        t.Start();
        //subscribe for action notification
        GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
      }
      catch (Exception ex)
      {
        Log.Write("ExternalDisplay.DoStart: Exception while starting plugin: " + ex.Message);
        if (t != null && t.IsAlive)
          t.Abort();
        t = null;
      }
    }


    /// <summary>
    /// Background thread for steering the display
    /// </summary>
    public void Run()
    {
      bool doLog = Settings.Instance.ExtensiveLogging;
      if (doLog)
        Log.Write("ExternalDisplay: Entering run loop.");
      try
      {
        if (doLog)
          Log.Write("ExternalDisplay: Creating displayhandler.");
        handler = new DisplayHandler(display);
        if (doLog)
          Log.Write("ExternalDisplay: Starting displayhandler.");
        handler.Start();
        //Start property browser if needed
        while (!stopRequested)
        {
          DoWork();
          handler.DisplayLines();
          if (doLog)
            Log.Write("ExternalDisplay: Sleeping...");
          Thread.Sleep(Settings.Instance.ScrollDelay);
        }
        //stop display handler
        if (doLog)
          Log.Write("ExternalDisplay: Stopping displayhandler.");
        handler.Stop();
      }
      catch (Exception ex)
      {
        Log.Write("ExternalDisplay.Run: " + ex.Message);
      }
      if (doLog)
        Log.Write("ExternalDisplay: Exiting run loop.");
    }

    /// <summary>
    /// This method will be called by mediaportal to stop the process plugin
    /// </summary>
    public void Stop()
    {
      SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
      DoStop();
      //stop display
      display.Dispose();
      display = null; //to avoid calling after it is disposed
    }

    private void DoStop()
    {
      try
      {
        if (t == null || !t.IsAlive)
        {
          return;
        }
        stopRequested = true;
        while (t.IsAlive)
        {
          Application.DoEvents();
          Thread.Sleep(100);
        }
        t = null;
      }
      catch (Exception ex)
      {
        Log.Write("ExternalDisplay.Stop: " + ex.Message);
      }
    }

    #endregion

    #region ISetupForm implementation

    /// <summary>
    /// Gets the name of this plugin
    /// </summary>
    /// <returns>"External Display"</returns>
    public string PluginName()
    {
      return "External Display";
    }

    /// <summary>
    /// Gets the description of this plugin
    /// </summary>
    /// <returns>A fixed description</returns>
    public string Description()
    {
      return "Plug-in to show current status information on an external display (LCD, VFD, ...)";
    }

    /// <summary>
    /// Gets the author of this plugin
    /// </summary>
    /// <returns>"Joe Dalton"</returns>
    public string Author()
    {
      return "JoeDalton";
    }

    /// <summary>
    /// Instructs our plugin to show its setup form
    /// </summary>
    public void ShowPlugin()
    {
      Form settings = new SetupForm();
      settings.ShowDialog();
    }

    /// <summary>
    /// Returns whether our plugin can be enabled and disabled
    /// </summary>
    /// <returns>true</returns>
    public bool CanEnable()
    {
      return true;
    }

    /// <summary>
    /// Returns the Window ID of this plugin
    /// </summary>
    /// <returns>9876</returns>
    public int GetWindowId()
    {
      return WindowID;
    }

    /// <summary>
    /// Returns whether this plugin is enabled per default
    /// </summary>
    /// <returns>false</returns>
    public bool DefaultEnabled()
    {
      return false;
    }

    /// <summary>
    /// Returns whether this plugin has a setup screen
    /// </summary>
    /// <returns>true</returns>
    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    /// If the plugin should have its own button on the home menu of Mediaportal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>false : plugin does not need its own button on home</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    #endregion

    /// <summary>
    /// This method is responsible for determining the current status of MediaPortal,
    /// composing the wanted message, and sending that message to the <see cref="DisplayHandler"/>
    /// that will send it to the attached display.
    /// </summary>
    /// <remarks>
    /// This method is automatically called every 100ms when mediaportal is running
    /// </remarks>
    private void DoWork()
    {
      try
      {
        if (Settings.Instance.ExtensiveLogging)
          Log.Write("ExternalDisplay: Processing status.");
        Debug.Assert(display != null);
        GUIWindow.Window activeWindow = (GUIWindow.Window) GUIWindowManager.ActiveWindow;
        //Determine MediaPortal status
        status = Status.Idle;
        if (g_Player.Player != null)
        {
          GUIPropertyManager.SetProperty("#paused", g_Player.Paused ? "true" : string.Empty);
          if (g_Player.IsDVD)
          {
            status = Status.PlayingDVD;
          }
          else if (g_Player.IsRadio)
          {
            status = Status.PlayingRadio;
          }
          else if (g_Player.IsMusic)
          {
            status = Status.PlayingMusic;
          }
          else if (g_Player.IsTimeShifting)
          {
            status = Status.Timeshifting;
          }
          else if (g_Player.IsTVRecording)
          {
            status = Status.PlayingRecording;
          }
          else if (g_Player.IsTV)
          {
            status = Status.PlayingTV;
          }
          else if (g_Player.IsVideo)
          {
            status = Status.PlayingVideo;
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#paused", string.Empty);
          if (IsTVWindow((int) activeWindow))
          {
            status = Status.PlayingTV;
          }
        }
        if (DateTime.Now - lastAction < new TimeSpan(0, 0, 5))
        {
          status = Status.Action;
        }
        //Update propertybrowser's Status and ActiveWindow fields
        if (browser != null)
        {
          if (Settings.Instance.ExtensiveLogging)
            Log.Write("ExternalDisplay: Updating PropertyBrowser.");
          browser.SetStatus(status);
          browser.SetActiveWindow(activeWindow);
        }
        //Determine what message to send to the display
        foreach (Message msg in Settings.Instance.Messages)
        {
          if ((msg.Status == Status.Any || msg.Status == status) &&
              (msg.Windows.Count == 0 || msg.Windows.Contains((int) activeWindow)))
          {
            if (msg.Process(handler))
            {
              return;
            }
            break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write("ExternalDisplay.DoWork: " + ex.Message);
      }
    }


    /// <summary>
    /// This function gets called when 
    /// <list>
    /// windows is about to suspend
    /// windows is about to hibernate
    /// windows resumes
    /// a powermode change is detected (AC -> battery or battery -> AC)
    /// </list>
    /// The <see cref="PowerModeChangedEventArgs.Mode"/> property can be used to determine the exact cause.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      switch (e.Mode)
      {
        case PowerModes.Suspend:
          Log.Write("ExternalDisplay: Suspend or Hibernation detected, shutting down plugin");
          DoStop();
          break;
        case PowerModes.Resume:
          Log.Write("ExternalDisplay: Resume from Suspend or Hibernation detected, starting plugin");
          DoStart();
          break;
      }
    }

    /// <summary>
    /// Gets called when an <see cref="MediaPortal.GUI.Library.Action"/> occured in MediaPortal
    /// </summary>
    /// <remarks>95% of the time, actions are caused by the user pressing a key</remarks>
    /// <param name="action">The <see cref="MediaPortal.GUI.Library.Action"/> that occurred.</param>
    private void GUIWindowManager_OnNewAction(Action action)
    {
      lastAction = DateTime.Now; //Update last action time
    }

    private bool IsTVWindow(int windowId)
    {
      if (windowId == (int) GUIWindow.Window.WINDOW_TV) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TVFULLSCREEN) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TVGUIDE) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_RECORDEDTV) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_RECORDEDTVGENRE) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_SCHEDULER) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_SEARCHTV) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TELETEXT) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_CONFLICTS) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_NO_SIGNAL) return true;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV_PROGRAM_INFO) return true;
      return false;
    }

    /// <summary>
    /// Checks whether the DriverLYNX Port I/O driver is installed
    /// </summary>
    internal static bool VerifyDriverLynxDriver()
    {
      FileInfo dll = new FileInfo(string.Concat(Environment.SystemDirectory, @"\DLPORTIO.dll"));
      FileInfo sys = new FileInfo(string.Concat(Environment.SystemDirectory, @"\DRIVERS\DLPORTIO.SYS"));
      if (!(dll.Exists && sys.Exists))
      {
        return false;
      }
      return true;
    }
  }
}