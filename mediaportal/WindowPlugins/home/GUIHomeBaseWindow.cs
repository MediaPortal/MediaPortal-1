#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Topbar;
#endregion

namespace MediaPortal.GUI.Home
{
	/// <summary>
	/// The implementation of the GUIHome Window base class.  (This window is coupled to the home.xml skin file).
	/// </summary>
  public abstract class GUIHomeBaseWindow : GUIWindow
	{
		#region Properties (Skin)
		[SkinControlAttribute(200)]		protected GUILabelControl lblDate = null;
		[SkinControlAttribute(201)]		protected GUILabelControl lblTime = null;
		[SkinControlAttribute(50)]		protected GUIMenuControl  menuMain = null;

		#endregion
		
		#region Variables
		protected bool _useMyPlugins = true;
    protected bool _fixedScroll = true;  // fix scrollbar in the middle of menu
    protected bool _enableAnimation = true;
    protected string _dateFormat = String.Empty;
    protected DateTime _updateTimer = DateTime.MinValue;
    protected int  _notifyTVTimeout = 15;
    protected bool _playNotifyBeep = true;
    protected int  _preNotifyConfig = 60;
    protected GUIOverlayWindow _overlayWin = null;
		#endregion

		#region Constructor
		public GUIHomeBaseWindow()
		{
      LoadSettings();
      GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
    }
		#endregion

    #region Serialisation
    protected virtual void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _fixedScroll     = xmlreader.GetValueAsBool("home", "scrollfixed", true);		      // fix scrollbar in the middle of menu
        _useMyPlugins    = xmlreader.GetValueAsBool("home", "usemyplugins", true);		    // use previous menu handling
        _dateFormat      = xmlreader.GetValueAsString("home", "dateformat", "<Day> <Month> <DD>");
        _enableAnimation = xmlreader.GetValueAsBool("home", "enableanimation", true);
        _notifyTVTimeout = xmlreader.GetValueAsInt("movieplayer", "notifyTVTimeout", 15);
        _playNotifyBeep  = xmlreader.GetValueAsBool("movieplayer", "notifybeep", true);
        _preNotifyConfig = xmlreader.GetValueAsInt("movieplayer", "notifyTVBefore", 300);
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
        menuMain.FixedScroll     = _fixedScroll;
        menuMain.EnableAnimation = _enableAnimation;
      }
      LoadButtonNames();
		}

		protected virtual void LoadButtonNames()
		{
		}
        
		public override bool OnMessage(GUIMessage message)
		{
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          GetDate();
          if (lblDate != null) lblDate.Label = GUIPropertyManager.GetProperty("#homedate");
          if (lblTime != null) lblTime.Label = GUIPropertyManager.GetProperty("#time");
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
      if (DateTime.Now.Minute != _updateTimer.Minute)
      {
        _updateTimer = DateTime.Now;
        GetDate();
        if (lblDate != null) lblDate.Label = GUIPropertyManager.GetProperty("#homedate");
        if (lblTime != null) lblTime.Label = GUIPropertyManager.GetProperty("#time");
      }
    }
        
    #endregion

    #region Methods
    public string GetFocusTextureFileName(string FileName)
    {
      if (!FileName.ToLower().Contains("button_")) FileName = "button_" + FileName;
      return GetMediaFileName(FileName);
    }

    public string GetNonFocusTextureFileName(string FileName)
    {
      if (!FileName.ToLower().Contains("buttonnf_")) FileName = "buttonnf_" + FileName;
      return GetMediaFileName(FileName);
    }

    public string GetHoverFileName(string FileName)
    {
      string name = System.IO.Path.GetFileName(FileName);
      string dir  = System.IO.Path.GetDirectoryName(FileName);
      if (dir.Length > 0) dir = dir + "\\";
      if (!name.ToLower().Contains("hover_")) FileName = dir + "hover_" + name;
      return GetMediaFileName(FileName);
    }

    protected string GetMediaFileName(string name)
    {
      if (System.IO.Path.GetPathRoot(name) == "") 
      {
        name = String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin, name);
      }
      if ((System.IO.Path.HasExtension(name)) && (System.IO.File.Exists(name))) return System.IO.Path.GetFileName(name);

      
      string filename = System.IO.Path.ChangeExtension(name, ".png");
      if (System.IO.File.Exists(filename)) return System.IO.Path.GetFileName(filename);

      filename = System.IO.Path.ChangeExtension(name, ".gif");
      if (System.IO.File.Exists(filename)) return System.IO.Path.GetFileName(filename);

      filename = System.IO.Path.ChangeExtension(name, ".bmp");
      if (System.IO.File.Exists(filename)) return System.IO.Path.GetFileName(filename);

      filename = System.IO.Path.ChangeExtension(name, ".xml");
      if (System.IO.File.Exists(filename)) return "media\\" + System.IO.Path.GetFileName(filename);

      return String.Empty;
    }
    
    protected string GetDate()
    {
      string dateString = _dateFormat;
      if ((dateString == null) || (dateString.Length == 0)) return String.Empty;

      DateTime cur = DateTime.Now;
      string day;
      switch (cur.DayOfWeek)
      {
        case DayOfWeek.Monday: day = GUILocalizeStrings.Get(11); break;
        case DayOfWeek.Tuesday: day = GUILocalizeStrings.Get(12); break;
        case DayOfWeek.Wednesday: day = GUILocalizeStrings.Get(13); break;
        case DayOfWeek.Thursday: day = GUILocalizeStrings.Get(14); break;
        case DayOfWeek.Friday: day = GUILocalizeStrings.Get(15); break;
        case DayOfWeek.Saturday: day = GUILocalizeStrings.Get(16); break;
        default: day = GUILocalizeStrings.Get(17); break;
      }

      string month;
      switch (cur.Month)
      {
        case 1: month = GUILocalizeStrings.Get(21); break;
        case 2: month = GUILocalizeStrings.Get(22); break;
        case 3: month = GUILocalizeStrings.Get(23); break;
        case 4: month = GUILocalizeStrings.Get(24); break;
        case 5: month = GUILocalizeStrings.Get(25); break;
        case 6: month = GUILocalizeStrings.Get(26); break;
        case 7: month = GUILocalizeStrings.Get(27); break;
        case 8: month = GUILocalizeStrings.Get(28); break;
        case 9: month = GUILocalizeStrings.Get(29); break;
        case 10: month = GUILocalizeStrings.Get(30); break;
        case 11: month = GUILocalizeStrings.Get(31); break;
        default: month = GUILocalizeStrings.Get(32); break;
      }

      
      dateString = MediaPortal.Util.Utils.ReplaceTag(dateString, "<Day>", day, "unknown");
      dateString = MediaPortal.Util.Utils.ReplaceTag(dateString, "<DD>", cur.Day.ToString(), "unknown");

      dateString = MediaPortal.Util.Utils.ReplaceTag(dateString, "<Month>", month, "unknown");
      dateString = MediaPortal.Util.Utils.ReplaceTag(dateString, "<MM>", cur.Month.ToString(), "unknown");

      dateString = MediaPortal.Util.Utils.ReplaceTag(dateString, "<Year>", cur.Year.ToString(), "unknown");
      dateString = MediaPortal.Util.Utils.ReplaceTag(dateString, "<YY>", (cur.Year - 2000).ToString("00"), "unknown");

      GUIPropertyManager.SetProperty("#homedate", dateString);
      
      return dateString;
    }
    #endregion

    #region OnGlobalMessage routines
    private void OnGlobalMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM)
      {
        //if (GUIGraphicsContext.IsFullScreenVideo) return;
        GUIDialogNotify dialogNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        TVProgram notify = message.Object as TVProgram;
        if (notify == null) return;
        int minUntilStart = _preNotifyConfig / 60;
        if (minUntilStart > 1)
          dialogNotify.SetHeading(String.Format(GUILocalizeStrings.Get(1018), minUntilStart));
        else
          dialogNotify.SetHeading(1019); // Program is about to begin
        dialogNotify.SetText(String.Format("{0}\n{1}", notify.Title, notify.Description));
        string strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, notify.Channel);
        dialogNotify.SetImage(strLogo);
        dialogNotify.TimeOut = _notifyTVTimeout;
        if (_playNotifyBeep)
          MediaPortal.Util.Utils.PlaySound("notify.wav", false, true);
        dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
      }
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_NOTIFY:
          ShowNotify(message.Label, message.Label2, message.Label3);
          break;
        case GUIMessage.MessageType.GUI_MSG_ASKYESNO:
          string Head = "", Line1 = "", Line2 = "", Line3 = ""; ;
          if (message.Param1 != 0) Head = GUILocalizeStrings.Get(message.Param1);
          else if (message.Label != String.Empty) Head = message.Label;
          if (message.Param2 != 0) Line1 = GUILocalizeStrings.Get(message.Param2);
          else if (message.Label2 != String.Empty) Line1 = message.Label2;
          if (message.Param3 != 0) Line2 = GUILocalizeStrings.Get(message.Param3);
          else if (message.Label3 != String.Empty) Line2 = message.Label3;
          if (message.Param4 != 0) Line3 = GUILocalizeStrings.Get(message.Param4);
          else if (message.Label4 != String.Empty) Line3 = message.Label4;
          if (AskYesNo(Head, Line1, Line2, Line3))
            message.Param1 = 1;
          else
            message.Param1 = 0;
          break;

        case GUIMessage.MessageType.GUI_MSG_SHOW_WARNING:
          {
            string strHead = "", strLine1 = "", strLine2 = "";
            if (message.Param1 != 0) strHead = GUILocalizeStrings.Get(message.Param1);
            else if (message.Label != String.Empty) strHead = message.Label;
            if (message.Param2 != 0) strLine1 = GUILocalizeStrings.Get(message.Param2);
            else if (message.Label2 != String.Empty) strLine2 = message.Label2;
            if (message.Param3 != 0) strLine2 = GUILocalizeStrings.Get(message.Param3);
            else if (message.Label3 != String.Empty) strLine2 = message.Label3;
            ShowInfo(strHead, strLine1, strLine2);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_GET_STRING:
          VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard) return;
          keyboard.Reset();
          keyboard.Text = message.Label;
          keyboard.DoModal(GUIWindowManager.ActiveWindow);
          if (keyboard.IsConfirmed)
          {
            message.Label = keyboard.Text;
          }
          else message.Label = "";
          break;

        case GUIMessage.MessageType.GUI_MSG_GET_PASSWORD:
          // Only one window should act on this.
          if (GetID != (int)GUIWindow.Window.WINDOW_HOME)
            break;
          VirtualKeyboard keyboard2 = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard2) return;
          keyboard2.Reset();
          keyboard2.Password = true;
          keyboard2.Text = message.Label;
          keyboard2.DoModal(GUIWindowManager.ActiveWindow);
          if (keyboard2.IsConfirmed)
          {
            message.Label = keyboard2.Text;
          }
          else message.Label = "";
          break;

        case GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD:
          // Only one window should act on this.
          if (GetID != (int)GUIWindow.Window.WINDOW_HOME)
            break;
          using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            if (!xmlreader.GetValueAsBool("general", "hidewrongpin", false))
            {
              GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              if (dlgYesNo == null)
                return;
              dlgYesNo.SetHeading(771); // The entered PIN could not be accepted
              dlgYesNo.SetLine(1, 772); // Do you want to try again?
              dlgYesNo.SetDefaultToYes(true);
              dlgYesNo.DoModal(GetID);
              message.Object = dlgYesNo.IsConfirmed;
            }
            else
              message.Object = false;
          }
          break;
      }
    }
    void ShowInfo(string strHeading, string strLine1, string strLine2)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);
      pDlgOK.SetHeading(strHeading);
      pDlgOK.SetLine(1, strLine1);
      pDlgOK.SetLine(2, strLine2);
      pDlgOK.SetLine(3, "");
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
    }

    void ShowNotify(string strHeading, string description, string imgFileName)
    {
      GUIDialogNotify dlgYesNo = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      dlgYesNo.SetHeading(strHeading);
      dlgYesNo.SetText(description);
      dlgYesNo.SetImage(imgFileName);
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
    }
    bool AskYesNo(string strHeading, string strLine1, string strLine2, string strLine3)
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
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
