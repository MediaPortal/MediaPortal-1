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

#region Usings

using System;
using System.Drawing;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

#endregion

namespace MediaPortal.GUI.Home
{
  /// <summary>
  /// The implementation of the GUIHome Window base class.  (This window is coupled to the myHome.xml skin file).
  /// </summary>
  public abstract class GUIHomeBaseWindow : GUIWindow
  {
    #region Properties (Skin)

    [SkinControl(200)] protected GUILabelControl lblDate = null;
    [SkinControl(201)] protected GUILabelControl lblTime = null;
    [SkinControl(50)] protected GUIMenuControl menuMain = null;
    [SkinControl(99)] protected GUIVideoControl videoWindow = null;

    #endregion

    #region Variables

    protected bool _useMyPlugins = true;
    protected bool _fixedScroll = true; // fix scrollbar in the middle of menu
    protected bool _enableAnimation = true;
    protected DateTime _updateTimer = DateTime.MinValue;
    protected int _notifyTVTimeout = 15;
    protected bool _playNotifyBeep = true;
    protected int _preNotifyConfig = 60;
    protected GUIOverlayWindow _overlayWin = null;
    private static bool _addedGlobalMessageHandler = false;

    #endregion

    #region Constructor

    public GUIHomeBaseWindow()
    {
      LoadSettings();
      //do this only once, we dont want a global message handler for every derived class
      if (!_addedGlobalMessageHandler)
      {
        _addedGlobalMessageHandler = true;
        GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
      }
    }

    #endregion

    #region Serialisation

    protected virtual void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _fixedScroll = xmlreader.GetValueAsBool("home", "scrollfixed", true); // fix scrollbar in the middle of menu
        _useMyPlugins = xmlreader.GetValueAsBool("home", "usemyplugins", true); // use previous menu handling
        _enableAnimation = xmlreader.GetValueAsBool("home", "enableanimation", true);
        _notifyTVTimeout = xmlreader.GetValueAsInt("mytv", "notifyTVTimeout", 15);
        _playNotifyBeep = xmlreader.GetValueAsBool("mytv", "notifybeep", true);
        _preNotifyConfig = xmlreader.GetValueAsInt("mytv", "notifyTVBefore", 300);
      }
    }

    #endregion

    #region Override

    /// <summary>
    /// OnWindowLoaded() gets called when the window is fully loaded and all controls are initialized
    /// In this home plugin, its now time to add the button for each dynamic plugin
    /// </summary>
    protected override void OnWindowLoaded()
    {
      base.OnWindowLoaded();
      if (menuMain != null)
      {
        menuMain.FixedScroll = _fixedScroll;
        menuMain.EnableAnimation = _enableAnimation;
      }
      LoadButtonNames();
      menuMain.ButtonInfos.Sort(menuMain.Compare);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      //set video window position
      if (videoWindow != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width,
                                                       videoWindow.Height);
      }
    }

    protected virtual void LoadButtonNames()
    {
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          if (lblDate != null)
          {
            lblDate.Label = GUIPropertyManager.GetProperty("#date");
          }
          if (lblTime != null)
          {
            lblTime.Label = GUIPropertyManager.GetProperty("#time");
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, message.Param1, 0, null);
          GUIWindowManager.SendThreadMessage(msg);
          break;
      }
      return base.OnMessage(message);
    }

    public override void Process()
    {
      // Set the date & time
      if (DateTime.Now.Second != _updateTimer.Second)
      {
        _updateTimer = DateTime.Now;
        if (lblDate != null)
        {
          lblDate.Label = GUIPropertyManager.GetProperty("#date");
        }
        if (lblTime != null)
        {
          lblTime.Label = GUIPropertyManager.GetProperty("#time");
        }
      }
    }

    #endregion

    #region Methods

    public string GetFocusTextureFileName(string FileName)
    {
      if (!FileName.ToLower().Contains("button_"))
      {
        FileName = "button_" + FileName;
      }
      return GetMediaFileName(FileName);
    }

    public string GetNonFocusTextureFileName(string FileName)
    {
      if (!FileName.ToLower().Contains("buttonnf_"))
      {
        FileName = "buttonnf_" + FileName;
      }
      return GetMediaFileName(FileName);
    }

    public string GetHoverFileName(string FileName)
    {
      string name = Path.GetFileName(FileName);
      string dir = Path.GetDirectoryName(FileName);
      if (dir.Length > 0)
      {
        dir = dir + "\\";
      }
      if (!name.ToLower().Contains("hover_"))
      {
        FileName = dir + "hover_" + name;
      }
      return GetMediaFileName(FileName);
    }

    public string GetNonFocusHoverFileName(string FileName)
    {
      string name = Path.GetFileName(FileName);
      string dir = Path.GetDirectoryName(FileName);
      if (dir.Length > 0)
      {
        dir = dir + "\\";
      }
      if (!name.ToLower().Contains("nonfocushover_"))
      {
        FileName = dir + "nonfocushover_" + name;
      }
      return GetMediaFileName(FileName);
    }

    protected string GetMediaFileName(string name)
    {
      if (Path.GetPathRoot(name) == "")
      {
        name = String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin, name);
      }
      if ((Path.HasExtension(name)) && (File.Exists(name)))
      {
        return Path.GetFileName(name);
      }

      string filename = Path.ChangeExtension(name, ".png");
      if (File.Exists(filename))
      {
        return Path.GetFileName(filename);
      }

      filename = Path.ChangeExtension(name, ".gif");
      if (File.Exists(filename))
      {
        return Path.GetFileName(filename);
      }

      filename = Path.ChangeExtension(name, ".bmp");
      if (File.Exists(filename))
      {
        return Path.GetFileName(filename);
      }

      filename = Path.ChangeExtension(name, ".xml");
      if (File.Exists(filename))
      {
        return "media\\" + Path.GetFileName(filename);
      }

      return string.Empty;
    }

    #endregion

    #region OnGlobalMessage routines

    private void OnGlobalMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM:
          //if (GUIGraphicsContext.IsFullScreenVideo) return;
          GUIDialogNotify dialogNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
          TVProgramDescription notify = message.Object as TVProgramDescription;
          if (notify == null)
          {
            return;
          }
          int minUntilStart = _preNotifyConfig/60;
          if (minUntilStart > 1)
          {
            dialogNotify.SetHeading(String.Format(GUILocalizeStrings.Get(1018), minUntilStart));
          }
          else
          {
            dialogNotify.SetHeading(1019); // Program is about to begin
          }
          dialogNotify.SetText(String.Format("{0}\n{1}", notify.Title, notify.Description));
          string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, notify.Channel);
          dialogNotify.SetImage(strLogo);
          dialogNotify.TimeOut = _notifyTVTimeout;
          if (_playNotifyBeep)
          {
            Util.Utils.PlaySound("notify.wav", false, true);
          }
          dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
          break;

        case GUIMessage.MessageType.GUI_MSG_NOTIFY:
          ShowNotify(message.Label, message.Label2, message.Label3);
          break;

        case GUIMessage.MessageType.GUI_MSG_ASKYESNO:
          string Head = "", Line1 = "", Line2 = "", Line3 = "";
          ;
          if (message.Param1 != 0)
          {
            Head = GUILocalizeStrings.Get(message.Param1);
          }
          else if (message.Label != string.Empty)
          {
            Head = message.Label;
          }
          if (message.Param2 != 0)
          {
            Line1 = GUILocalizeStrings.Get(message.Param2);
          }
          else if (message.Label2 != string.Empty)
          {
            Line1 = message.Label2;
          }
          if (message.Param3 != 0)
          {
            Line2 = GUILocalizeStrings.Get(message.Param3);
          }
          else if (message.Label3 != string.Empty)
          {
            Line2 = message.Label3;
          }
          if (message.Param4 != 0)
          {
            Line3 = GUILocalizeStrings.Get(message.Param4);
          }
          else if (message.Label4 != string.Empty)
          {
            Line3 = message.Label4;
          }
          if (AskYesNo(Head, Line1, Line2, Line3))
          {
            message.Param1 = 1;
          }
          else
          {
            message.Param1 = 0;
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_SHOW_WARNING:
          {
            string strHead = "", strLine1 = "", strLine2 = "";
            if (message.Param1 != 0)
            {
              strHead = GUILocalizeStrings.Get(message.Param1);
            }
            else if (message.Label != string.Empty)
            {
              strHead = message.Label;
            }
            if (message.Param2 != 0)
            {
              strLine1 = GUILocalizeStrings.Get(message.Param2);
            }
            else if (message.Label2 != string.Empty)
            {
              strLine2 = message.Label2;
            }
            if (message.Param3 != 0)
            {
              strLine2 = GUILocalizeStrings.Get(message.Param3);
            }
            else if (message.Label3 != string.Empty)
            {
              strLine2 = message.Label3;
            }
            ShowInfo(strHead, strLine1, strLine2);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_GET_STRING:
          VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard)
          {
            return;
          }
          keyboard.Reset();
          keyboard.Text = message.Label;
          keyboard.DoModal(GUIWindowManager.ActiveWindow);
          if (keyboard.IsConfirmed)
          {
            message.Label = keyboard.Text;
          }
          else
          {
            message.Label = "";
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_GET_PASSWORD:
          VirtualKeyboard keyboard2 = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard2)
          {
            return;
          }
          keyboard2.Reset();
          keyboard2.Password = true;
          keyboard2.Text = message.Label;
          keyboard2.DoModal(GUIWindowManager.ActiveWindow);
          if (keyboard2.IsConfirmed)
          {
            message.Label = keyboard2.Text;
          }
          else
          {
            message.Label = "";
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD:
          using (Profile.Settings xmlreader = new Profile.MPSettings()
            )
          {
            if (!xmlreader.GetValueAsBool("general", "hidewrongpin", false))
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
              if (dlgYesNo == null)
              {
                return;
              }
              dlgYesNo.SetHeading(771); // The entered PIN could not be accepted
              dlgYesNo.SetLine(1, 772); // Do you want to try again?
              dlgYesNo.SetDefaultToYes(true);
              dlgYesNo.DoModal(GetID);
              message.Object = dlgYesNo.IsConfirmed;
            }
            else
            {
              message.Object = false;
            }
          }
          break;
      }
    }

    private void ShowInfo(string strHeading, string strLine1, string strLine2)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow(2002);
      pDlgOK.SetHeading(strHeading);
      pDlgOK.SetLine(1, strLine1);
      pDlgOK.SetLine(2, strLine2);
      pDlgOK.SetLine(3, "");
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
    }

    private void ShowNotify(string strHeading, string description, string imgFileName)
    {
      GUIDialogNotify dlgYesNo = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
      dlgYesNo.SetHeading(strHeading);
      dlgYesNo.SetText(description);
      dlgYesNo.SetImage(imgFileName);
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
    }

    private bool AskYesNo(string strHeading, string strLine1, string strLine2, string strLine3)
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      dlgYesNo.SetHeading(strHeading);
      dlgYesNo.SetLine(1, strLine1);
      dlgYesNo.SetLine(2, strLine2);
      dlgYesNo.SetLine(3, strLine3);
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
      return dlgYesNo.IsConfirmed;
    }

    #endregion
  }
}