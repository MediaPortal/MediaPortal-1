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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Microsoft.Win32;
using Message=MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting.Message;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  [PluginIcons("ProcessPlugins.MiniDisplay.MiniDisplay.lcd.gif",
    "ProcessPlugins.MiniDisplay.MiniDisplay.lcd_deactivated.gif")]
  public class MiniDisplay : IPlugin, ISetupForm
  {
    #region variables

    private PropertyBrowser browser;
    private IDisplay display;
    private DisplayHandler handler;
    private DateTime lastAction = DateTime.MinValue;
    private Status status;
    private bool stopRequested;
    private Thread t;

    #endregion

    #region ctor

    public MiniDisplay()
    {
    }

    #endregion

    #region ISetupForm members

    public string PluginName()
    {
      return "MiniDisplay";
    }

    public string Description()
    {
      return "Shows current status information on an External VFD/LCD display";
    }

    public string Author()
    {
      return "CybrMage";
    }

    public void ShowPlugin()
    {
      Log.Info("MiniDisplay.ShowPlugin(): Called");
      new SetupForm().ShowDialog();
      Log.Info("MiniDisplay.ShowPlugin(): Completed");
    }

    public bool CanEnable()
    {
      return true;
    }

    public int GetWindowId()
    {
      return -1; //????
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = string.Empty;
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
      return false;
    }

    #endregion

    #region IPlugin members

    public void Start()
    {
      Log.Info("MiniDisplay.Start(): called");
      Log.Info("MiniDisplay.Start(): {0}", MiniDisplayHelper.Plugin_Version);
      Log.Info("MiniDisplay.Start(): plugin starting...");
      if (!File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml")))
      {
        Log.Info("MiniDisplay.Start(): plugin not configured... Unable to start");
      }
      else
      {
        MiniDisplayHelper.MPStatus = new SystemStatus();
        MiniDisplayHelper.InitSystemStatus(ref MiniDisplayHelper.MPStatus);
        this.GetTVSource();
        if (Settings.Instance.ShowPropertyBrowser)
        {
          lock (MiniDisplayHelper.PropertyBrowserMutex)
          {
            Log.Info("MiniDisplay.Start(): opening PropertyBrowser.");
            this.browser = new PropertyBrowser();
            this.browser.FormClosing += new FormClosingEventHandler(this.browser_Closing);
            this.browser.Show();
            MiniDisplayHelper._PropertyBrowserAvailable = true;
          }
        }
        this.DoStart();
        Log.Info("MiniDisplay.Start(): completed");
      }
    }

    public void Stop()
    {
      Log.Info("MiniDisplay.Stop(): called");
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Debug("MiniDisplay: Plugin is being stopped.");
      }
      MiniDisplayHelper._PropertyBrowserAvailable = false;
      SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
      this.DoStop();
      if (this.browser != null)
      {
        Log.Info("MiniDisplay.Stop(): closing PropertyBrowser.");
        this.browser.Close();
        this.browser = null;
      }
      if (this.display != null)
      {
        this.display.Dispose();
        this.display = null;
      }
      Log.Info("MiniDisplay.Stop(): completed");
    }

    #endregion

    #region Thread handling

    private void DoStart()
    {
      if ((this.t == null) || !this.t.IsAlive)
      {
        try
        {
          this.display = Settings.Instance.LCDType;
          if (this.display == null)
          {
            Log.Info("MiniDisplay.DoStart(): Internal display type not found.  Plugin not started!!!");
            return;
          }
          Log.Info("MiniDisplay.DoStart(): Starting background thread");
          this.stopRequested = false;
          this.t = new Thread(new ThreadStart(this.Run));
          this.t.Priority = ThreadPriority.Lowest;
          this.t.Name = "MiniDisplay";
          this.t.TrySetApartmentState(ApartmentState.MTA);
          this.t.Start();
          GUIWindowManager.OnNewAction += new OnActionHandler(this.GUIWindowManager_OnNewAction);
          Thread.Sleep(100);
          if (!this.t.IsAlive)
          {
            Log.Info("MiniDisplay.DoStart(): ERROR - backgrund thread NOT STARTED");
          }
        }
        catch (Exception exception)
        {
          Log.Info("MiniDisplay.DoStart: Exception while starting plugin: " + exception.Message);
          if ((this.t != null) && this.t.IsAlive)
          {
            this.t.Abort();
          }
          this.t = null;
        }
        Log.Info("MiniDisplay.DoStart(): Completed");
      }
    }

    private void DoStop()
    {
      Log.Info("MiniDisplay.DoStop(): Called.");
      try
      {
        if ((this.t == null) || !this.t.IsAlive)
        {
          Log.Info("MiniDisplay.DoStop(): ERROR - background thread not running.");
        }
        else
        {
          this.stopRequested = true;
          Log.Info("MiniDisplay.DoStop(): Requesting background thread to stop.");
          DateTime time = DateTime.Now.AddSeconds(5.0);
          while (this.t.IsAlive && (DateTime.Now.Ticks < time.Ticks))
          {
            if (Settings.Instance.ExtensiveLogging)
            {
              Log.Info("MiniDisplay.DoStop: Background thread still alive, waiting 100ms...");
            }
            Thread.Sleep(100);
          }
          if (DateTime.Now.Ticks > time.Ticks)
          {
            this.t.Abort();
            Thread.Sleep(100);
            Log.Info("MiniDisplay.DoStop(): Forcing display thread shutdown. t.IsAlive = {0}",
                     this.t.IsAlive);
          }
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Info("MiniDisplay.DoStop(): Background thread has stopped.");
          }
          this.t = null;
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }
    
    private void DoWork()
    {
      try
      {
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("MiniDisplay Processing status.");
        }
        GUIWindow.Window activeWindow = (GUIWindow.Window) GUIWindowManager.ActiveWindow;
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("Active window is {0}", activeWindow.ToString());
        }
        this.status = Status.Idle;
        if (g_Player.Player != null)
        {
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug("Active player detected");
          }
          GUIPropertyManager.SetProperty("#paused", g_Player.Paused ? "true" : string.Empty);
          if (g_Player.IsDVD)
          {
            this.status = Status.PlayingDVD;
          }
          else if (g_Player.IsRadio)
          {
            this.status = Status.PlayingRadio;
          }
          else if (g_Player.IsMusic)
          {
            this.status = Status.PlayingMusic;
          }
          else if (g_Player.IsTimeShifting)
          {
            this.status = Status.Timeshifting;            
          }
          else if (g_Player.IsTVRecording)
          {
            this.status = Status.PlayingRecording;            
          }
          else if (g_Player.IsTV)
          {
            this.status = Status.PlayingTV;            
          }
          else if (g_Player.IsVideo)
          {
            this.status = Status.PlayingVideo;            
          }          
        }
        else
        {
          GUIPropertyManager.SetProperty("#paused", string.Empty);
          if (this.IsTVWindow((int) activeWindow))
          {
            this.status = Status.PlayingTV;
          }          
        }
        if ((DateTime.Now - this.lastAction) < new TimeSpan(0, 0, MiniDisplayHelper.GetIdleTimeout()))
        {
          this.status = Status.Action;
        }
        if (GUIWindowManager.IsRouted)
        {
          string dialogTitle = string.Empty;
          string dialogHighlightedItem = string.Empty;
          GUIWindow.Window activeWindowEx = (GUIWindow.Window) GUIWindowManager.ActiveWindowEx;
          if (this.GetDialogInfo(activeWindowEx, ref dialogTitle, ref dialogHighlightedItem))
          {
            this.status = Status.Dialog;
            GUIPropertyManager.GetProperty("#currentmodule");
            GUIPropertyManager.SetProperty("#DialogLabel", dialogTitle);
            GUIPropertyManager.SetProperty("#DialogItem", dialogHighlightedItem);
            if (Settings.Instance.ExtensiveLogging)
            {
              Log.Debug("DIALOG window is {0}: \"{1}\", \"{2}\"", activeWindowEx.ToString(), dialogTitle, dialogHighlightedItem);
            }
          }
        }
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("Detected status is {0}", status.ToString());
        }
        lock (MiniDisplayHelper.StatusMutex)
        {
          MiniDisplayHelper.MPStatus.CurrentPluginStatus = this.status;
          MiniDisplayHelper.MPStatus.MP_Is_Idle = false;
          if (this.status.Equals(Status.Idle))
          {
            MiniDisplayHelper.MPStatus.MP_Is_Idle = true;
          }
          MiniDisplayHelper.MPStatus.CurrentIconMask = MiniDisplayHelper.SetPluginIcons();
          if (this.status.Equals(Status.PlayingDVD))
          {
            MiniDisplayHelper.MPStatus.Media_IsDVD = true;
          }
          if (this.status.Equals(Status.PlayingRadio))
          {
            MiniDisplayHelper.MPStatus.Media_IsRadio = true;
          }
          if (this.status.Equals(Status.PlayingMusic))
          {
            MiniDisplayHelper.MPStatus.Media_IsMusic = true;
          }
          if (this.status.Equals(Status.PlayingRecording))
          {
            MiniDisplayHelper.MPStatus.Media_IsTVRecording = true;
          }
          if (this.status.Equals(Status.PlayingTV))
          {
            MiniDisplayHelper.MPStatus.Media_IsTV = true;
          }
          if (this.status.Equals(Status.Timeshifting))
          {
            MiniDisplayHelper.MPStatus.Media_IsTVRecording = true;
          }
          if (this.status.Equals(Status.PlayingVideo))
          {
            MiniDisplayHelper.MPStatus.Media_IsVideo = true;
          }
          MiniDisplayHelper.ShowSystemStatus(ref MiniDisplayHelper.MPStatus);
        }
        lock (MiniDisplayHelper.PropertyBrowserMutex)
        {
          if (((this.browser != null) && !this.browser.IsDisposed) && MiniDisplayHelper._PropertyBrowserAvailable)
          {
            if (Settings.Instance.ExtensiveLogging)
            {
              Log.Info("MiniDisplayPlugin.DoWork(): Updating PropertyBrowser.");
            }
            this.browser.SetStatus(this.status);
            this.browser.SetActiveWindow(activeWindow);
          }
        }
        foreach (Message message in Settings.Instance.Messages)
        {
          if (((message.Status == Status.Any) || (message.Status == this.status)) &&
              ((message.Windows.Count == 0) || message.Windows.Contains((int) activeWindow)))
          {
            if (!message.Process(this.handler))
            {
            }
            return;
          }
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    public void Run()
    {
      SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
      bool extensiveLogging = Settings.Instance.ExtensiveLogging;
      bool flag2 = false;
      if (extensiveLogging)
      {
        Log.Info("MiniDisplay.Run(): Entering MiniDisplay run loop.");
      }
      try
      {
        if (extensiveLogging)
        {
          Log.Info("MiniDisplay.Run(): Creating MiniDisplay displayhandler.");
        }
        this.handler = new DisplayHandler(this.display);
        if (extensiveLogging)
        {
          Log.Info("MiniDisplay.Run(): Starting MiniDisplay displayhandler.");
        }
        this.handler.Start();
        while (!this.stopRequested)
        {
          if (!Settings.Instance.Type.Equals("MCEDisplay"))
          {
            try
            {
              // It's not safe to call this method in other states than running, since
              // it calls the window manager. It might cause a dead lock in other states
              if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
              {
                DoWork();
              }
            }
            catch (Exception exception)
            {
              Log.Debug("MiniDisplay.Run(): CAUGHT EXCEPTION in DoWork() - {0}", exception);
              if (exception.Message.Contains("ThreadAbortException"))
              {
                this.stopRequested = true;
              }
            }
            try
            {
              // It's not safe to call this method in other states than running, since
              // it calls the window manager. It might cause a dead lock in other states
              if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
              {
                handler.DisplayLines();
              }
            }
            catch (Exception exception2)
            {
              Log.Debug("MiniDisplay.Run(): CAUGHT EXCEPTION in handler.DisplayLines() - {0}", exception2);
              if (exception2.Message.Contains("ThreadAbortException"))
              {
                this.stopRequested = true;
              }
            }
            if (extensiveLogging)
            {
              Log.Debug("MiniDisplay.Run(): MiniDisplay Sleeping...");
            }
            Thread.Sleep(Settings.Instance.ScrollDelay);
            if (extensiveLogging)
            {
              Log.Debug("MiniDisplay.Run(): MiniDisplay Sleeping... DONE");
            }
          }
          else
          {
            Thread.Sleep(100);
          }
        }
        if (extensiveLogging)
        {
          Log.Info("MiniDisplay.Run(): Stopping MiniDisplay displayhandler.");
        }
        flag2 = true;
        this.handler.Stop();
      }
      catch (ThreadAbortException)
      {
        Log.Error("MiniDisplay.Run(): CAUGHT ThreadAbortException");
        if (!flag2)
        {
          this.handler.Stop();
          flag2 = true;
        }
      }
      catch (Exception exception3)
      {
        Log.Error("MiniDisplay.Run(): CAUGHT EXCEPTION: {0}", exception3);
      }
      if (extensiveLogging)
      {
        Log.Info("MiniDisplay.Run(): Exiting MiniDisplay run loop.");
      }
    }

    #endregion

    #region Helper methods

    public bool GetDialogInfo(GUIWindow.Window dialogWindow, ref string DialogTitle, ref string DialogHighlightedItem)
    {
      GUIListControl control = null;
      bool focus = false;
      switch (dialogWindow)
      {
        case GUIWindow.Window.WINDOW_DIALOG_YES_NO:
          {
            GUIDialogYesNo window = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) dialogWindow);
            DialogTitle = string.Empty;
            foreach (object obj16 in window.controlList)
            {
              if (obj16.GetType() == typeof (GUIFadeLabel))
              {
                GUIFadeLabel label3 = obj16 as GUIFadeLabel;
                if (DialogTitle == string.Empty)
                {
                  if (label3.Label != string.Empty)
                  {
                    DialogTitle = label3.Label;
                  }
                }
                else if (label3.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + label3.Label;
                }
              }
              if (obj16.GetType() == typeof (GUILabelControl))
              {
                GUILabelControl control14 = obj16 as GUILabelControl;
                if (DialogTitle == string.Empty)
                {
                  if (control14.Label != string.Empty)
                  {
                    DialogTitle = control14.Label;
                  }
                }
                else if (control14.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + control14.Label;
                }
              }
              if (obj16.GetType() == typeof (GUIButtonControl))
              {
                GUIButtonControl control15 = obj16 as GUIButtonControl;
                if (!control15.Focus)
                {
                  continue;
                }
                DialogHighlightedItem = control15.Description;
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_PROGRESS:
          {
            GUIDialogProgress progress = (GUIDialogProgress) GUIWindowManager.GetWindow((int) dialogWindow);
            foreach (object obj6 in progress.controlList)
            {
              if (obj6.GetType() == typeof (GUILabelControl))
              {
                GUILabelControl control6 = obj6 as GUILabelControl;
                if (control6.GetID == 1)
                {
                  DialogTitle = control6.Label;
                }
              }
            }
            foreach (object obj7 in progress.controlList)
            {
              if (obj7.GetType() == typeof (GUIProgressControl))
              {
                GUIProgressControl control7 = obj7 as GUIProgressControl;
                DialogHighlightedItem = "Progress: " + control7.Percentage.ToString() + "%";
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_SELECT:
          {
            GUIDialogSelect select = (GUIDialogSelect) GUIWindowManager.GetWindow((int) dialogWindow);
            control = null;
            focus = false;
            foreach (object obj9 in select.controlList)
            {
              if (obj9.GetType() == typeof (GUIListControl))
              {
                control = obj9 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string strLabel = string.Empty;
              string str5 = string.Empty;
              string strThumb = string.Empty;
              string strIndex = string.Empty;
              control.GetSelectedItem(ref strLabel, ref str5, ref strThumb, ref strIndex);
              DialogHighlightedItem = strLabel;
            }
            else
            {
              foreach (object obj10 in select.controlList)
              {
                if (obj10.GetType() == typeof (GUIButtonControl))
                {
                  GUIButtonControl control10 = obj10 as GUIButtonControl;
                  if (control10.Focus)
                  {
                    DialogHighlightedItem = control10.Description;
                  }
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_OK:
          {
            GUIDialogOK gok = (GUIDialogOK) GUIWindowManager.GetWindow((int) dialogWindow);
            foreach (object obj5 in gok.controlList)
            {
              if (obj5.GetType() == typeof (GUIButtonControl))
              {
                GUIButtonControl control4 = obj5 as GUIButtonControl;
                if (control4.Focus)
                {
                  DialogHighlightedItem = control4.Description;
                  if (Settings.Instance.ExtensiveLogging)
                  {
                    Log.Info(
                      "MiniDisplay.GetDialogInfo(): found WINDOW_DIALOG_OK buttoncontrol ID = {0} Label = \"{1}\" Desc = \"{2}\"",
                      control4.GetID, control4.Label, control4.Description);
                  }
                }
              }
              if (obj5.GetType() == typeof (GUIFadeLabel))
              {
                GUIFadeLabel label = obj5 as GUIFadeLabel;
                if (DialogTitle == string.Empty)
                {
                  if (label.Label != string.Empty)
                  {
                    DialogTitle = label.Label;
                  }
                }
                else if (label.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + label.Label;
                }
              }
              if (obj5.GetType() == typeof (GUILabelControl))
              {
                GUILabelControl control5 = obj5 as GUILabelControl;
                if (DialogTitle == string.Empty)
                {
                  if (control5.Label != string.Empty)
                  {
                    DialogTitle = control5.Label;
                  }
                  continue;
                }
                if (control5.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + control5.Label;
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_SELECT2:
          {
            GUIDialogSelect2 select2 = (GUIDialogSelect2) GUIWindowManager.GetWindow((int) dialogWindow);
            control = null;
            focus = false;
            foreach (object obj11 in select2.controlList)
            {
              if (obj11.GetType() == typeof (GUIListControl))
              {
                control = obj11 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string str7 = string.Empty;
              string str8 = string.Empty;
              string str9 = string.Empty;
              string strIndex = string.Empty;
              control.GetSelectedItem(ref str7, ref str8, ref str9, ref strIndex);
              DialogHighlightedItem = str7;
            }
            else
            {
              foreach (object obj12 in select2.controlList)
              {
                if (obj12.GetType() == typeof (GUIButtonControl))
                {
                  GUIButtonControl control11 = obj12 as GUIButtonControl;
                  if (control11.Focus)
                  {
                    DialogHighlightedItem = control11.Description;
                  }
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_MENU:
          {
            GUIDialogMenu menu = (GUIDialogMenu) GUIWindowManager.GetWindow((int) dialogWindow);
            foreach (object obj13 in menu.controlList)
            {
              if (obj13.GetType() == typeof (GUILabelControl))
              {
                GUILabelControl control12 = obj13 as GUILabelControl;
                if (!control12.Label.Trim().ToLower().Equals("menu"))
                {
                  DialogTitle = control12.Label;
                }
              }
            }
            control = null;
            focus = false;
            foreach (object obj14 in menu.controlList)
            {
              if (obj14.GetType() == typeof (GUIListControl))
              {
                control = obj14 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string str10 = string.Empty;
              string str11 = string.Empty;
              string str12 = string.Empty;
              string strIndex = string.Empty;
              control.GetSelectedItem(ref str10, ref str11, ref str12, ref strIndex);
              DialogHighlightedItem = str10;
            }
            else
            {
              foreach (object obj15 in menu.controlList)
              {
                if (obj15.GetType() == typeof (GUIButtonControl))
                {
                  GUIButtonControl control13 = obj15 as GUIButtonControl;
                  if (control13.Focus)
                  {
                    DialogHighlightedItem = control13.Description;
                  }
                }
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_RATING:
          {
            GUIDialogSetRating rating = (GUIDialogSetRating) GUIWindowManager.GetWindow((int) dialogWindow);
            DialogTitle = string.Empty;
            foreach (object obj8 in rating.controlList)
            {
              if (obj8.GetType() == typeof (GUIFadeLabel))
              {
                GUIFadeLabel label2 = obj8 as GUIFadeLabel;
                if (DialogTitle == string.Empty)
                {
                  if (label2.Label != string.Empty)
                  {
                    DialogTitle = label2.Label;
                  }
                }
                else if (label2.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + label2.Label;
                }
              }
              if (obj8.GetType() == typeof (GUILabelControl))
              {
                GUILabelControl control8 = obj8 as GUILabelControl;
                if (DialogTitle == string.Empty)
                {
                  if (control8.Label != string.Empty)
                  {
                    DialogTitle = control8.Label;
                  }
                }
                else if (control8.Label != string.Empty)
                {
                  DialogTitle = DialogTitle + " - " + control8.Label;
                }
              }
              if (obj8.GetType() == typeof (GUIButtonControl))
              {
                GUIButtonControl control9 = obj8 as GUIButtonControl;
                if (!control9.Focus)
                {
                  continue;
                }
                DialogHighlightedItem = control9.Description;
              }
            }
            return true;
          }
        case GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT:
          {
            GUIDialogMenuBottomRight right = (GUIDialogMenuBottomRight) GUIWindowManager.GetWindow((int) dialogWindow);
            DialogTitle = string.Empty;
            foreach (object obj2 in right.controlList)
            {
              if (obj2.GetType() == typeof (GUILabelControl))
              {
                GUILabelControl control2 = obj2 as GUILabelControl;
                if (!control2.Label.Trim().ToLower().Equals("menu") && (control2.Label != string.Empty))
                {
                  if (DialogTitle == string.Empty)
                  {
                    DialogTitle = control2.Label;
                  }
                  else
                  {
                    DialogTitle = DialogTitle + " - " + control2.Label;
                  }
                }
              }
            }
            control = null;
            focus = false;
            foreach (object obj3 in right.controlList)
            {
              if (obj3.GetType() == typeof (GUIListControl))
              {
                control = obj3 as GUIListControl;
                focus = control.Focus;
              }
            }
            if ((control != null) & focus)
            {
              string str = string.Empty;
              string str2 = string.Empty;
              string str3 = string.Empty;
              string strIndex = string.Empty;
              control.GetSelectedItem(ref str, ref str2, ref str3, ref strIndex);
              DialogHighlightedItem = str;
            }
            else
            {
              foreach (object obj4 in right.controlList)
              {
                if (obj4.GetType() == typeof (GUIButtonControl))
                {
                  GUIButtonControl control3 = obj4 as GUIButtonControl;
                  if (control3.Focus)
                  {
                    DialogHighlightedItem = control3.Description;
                  }
                }
              }
            }
            return true;
          }
      }
      return false;
    }

    private void GetTVSource()
    {
      MiniDisplayHelper.UseTVServer = false;
      if ((File.Exists(Config.GetFolder(Config.Dir.Base) + @"\TvControl.dll") &&
           File.Exists(Config.GetFolder(Config.Dir.Base) + @"\TvLibrary.Interfaces.dll")) &&
          File.Exists(Config.GetFolder(Config.Dir.Plugins) + @"\Windows\TvPlugin.dll"))
      {
        using (Profile.Settings settings = new Profile.MPSettings())
        {
          if (settings.GetValueAsString("tvservice", "hostname", "") != string.Empty)
          {
            Log.Info("MiniDisplay.GetTVSource(): Found configured TVServer installation");
            MiniDisplayHelper.UseTVServer = true;
          }
          else
          {
            Log.Info("MiniDisplay.GetTVSource(): Found TVServer installation");
          }
        }
      }
    }

    private bool IsTVWindow(int windowId)
    {
      return ((windowId == 1) ||
              ((windowId == 602) ||
               ((windowId == 600) ||
                ((windowId == 603) ||
                 ((windowId == 606) ||
                  ((windowId == 605) ||
                   ((windowId == 601) ||
                    ((windowId == 604) ||
                     ((windowId == 7700) ||
                      ((windowId == 7701) ||
                       ((windowId == 607) ||
                        ((windowId == 608) ||
                         ((windowId == 609) ||
                          ((windowId == 611) ||
                           ((windowId == 612) ||
                            ((windowId == 613) || ((windowId == 610) || ((windowId == 749) || (windowId == 748)))))))))))))))))));
    }

    #endregion

    #region Event handling
    private void GUIWindowManager_OnNewAction(Action action)
    {
      this.lastAction = DateTime.Now;
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Debug("MiniDisplay: SystemPowerModeChanged event was raised.");
      }
      switch (e.Mode)
      {
        case PowerModes.Resume:
          Log.Info("MiniDisplay: Resume from Suspend or Hibernation detected, starting plugin");
          SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
          this.DoStart();
          break;

        case PowerModes.StatusChange:
          break;

        case PowerModes.Suspend:
          Log.Info("MiniDisplay: Suspend or Hibernation detected, shutting down plugin");
          this.DoStop();
          return;

        default:
          return;
      }
    }

    private void browser_Closing(object sender, FormClosingEventArgs e)
    {
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.browser_Closing(): PropertyBrowser is closing.");
      }
      this.browser = null;
    }

    #endregion
  }
}