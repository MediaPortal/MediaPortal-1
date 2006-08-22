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

using System;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.Xml.Serialization;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Topbar;

namespace MyMail
{

  /// <summary>
  /// Zusammenfassung für Class1.
  /// </summary>
  public class MyMailPlugin : GUIWindow, ISetupForm, IShowPlugin
  {

    public MyMailPlugin()
    {
      GetID = 8000;
    }
    ~MyMailPlugin()
    {
    }
    #region "Declares"
    //
    enum Controls
    {
      CONTROL_LIST = 50,
      CONTROL_LABELFILES = 1,
      CONTROL_REFRESH_ALL = 20,
      CONTROL_SWITCH_AUTOCHECK,
      CONTROL_SET_ALL_UNREAD

    }
    //
    enum Views
    {
      VIEW_MAILBOX = 1,
      VIEW_MAILS,
      VIEW_ERROR_HAPPEND // here we dont set the list to view what happend
    }
    //
    enum MailActions
    {
      ACTION_VIEW_MAILBOX = 1, // thats reserved. dont use it.
      ACTION_LIST_MAILS,
      ACTION_REFRESH_MAILBOX,
      ACTION_TIMER_EVENT,
      ACTION_DO_NOTHING,
      ACTION_DELETE_MAIL
    }
    //
    enum AutocheckStatus
    {
      AUTOCHECK_OFF = 0,
      AUTOCHECK_ON
    }
    //
    ArrayList m_mailBox = new ArrayList(); // stored mailboxes
    MailClass m_mc = new MailClass(); // our class with the comm-parts
    MailBox m_currMailBox; // the selected Mailbox
    bool m_autoCheck;
    bool m_bAutoCheck; // indicates auto check mails on/off
    int m_currentView;
    int m_currMailAction;
    MailBox m_prevMailBox;
    // this timer checks the inbox(es) for mail
    System.Windows.Forms.Timer m_checkMailTimer = new Timer();
    ArrayList m_strUserName = new ArrayList();
    ArrayList m_strUserPass = new ArrayList();
    ArrayList m_knownMails = new ArrayList();
    #endregion
    #region ISetupForm
    public bool HasSetup()
    {
      return true;
    }
    public string PluginName()
    {
      return "My Mail";
    }
    public string Description() // Return the description which should b shown in the plugin menu
    {
      return "Receive and read your mails in MediaPortal";
    }
    public string Author() // Return the author which should b shown in the plugin menu
    {
      return "_Agree_";
    }
    public void ShowPlugin() // show the setup dialog
    {
      System.Windows.Forms.Form setup = new MailSetupFrom();
      setup.ShowDialog();
    }
    public bool CanEnable() // Indicates whether plugin can be enabled/disabled
    {
      return true;
    }
    public int GetWindowId() // get ID of plugin window
    {
      return GetID;
    }
    public bool DefaultEnabled() // Indicates if plugin is enabled by default;
    {
      return false;
    }
    public bool GetHome(out string strButtonText, out string strButtonImage, out string
      strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(8000);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "hover_my mail.png";
      return true;
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    public override bool Init()
    {
      // the event handler function gets the mail-data
      m_mc.GotMailData += new MyMail.MailClass.GotMailDataEventHandler(m_mc_GotMailData);
      m_mc.InformUser += new MyMail.MailClass.InformEventHandler(m_mc_InformUser);
      LoadSettings();

      if (m_mailBox.Count > 0) // when at least 1 mailbox is given
      {
        m_checkMailTimer.Tick += new EventHandler(checkMails);
        m_checkMailTimer.Start();
        DisplayOverlayNotify(false, "");
        m_checkMailTimer.Enabled = true;
        m_currentView = (int)Views.VIEW_MAILBOX; // for init show mailboxes
      }
      else
      {
        DisplayOverlayNotify(false, "");
      }
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mymail.xml");

      return bResult;
    }
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(8000));

          if (m_mailBox.Count == 0) // when at least 1 mailbox is given,
          {
            GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            if (dlgOK != null)
            {
              dlgOK.SetHeading(8010);
              dlgOK.SetLine(1, 8011);
              dlgOK.SetLine(2, 8012);
              dlgOK.SetLine(3, "");
              dlgOK.DoModal(GetID);
            }
          }
          else
          {
            SetAutoCheckButton();
            m_autoCheck = false;
            m_checkMailTimer.Stop();
            if (m_currentView == (int)Views.VIEW_MAILBOX)
              SetMailBoxList();
            else
              SetMailsList();
          }
          return true;



        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          SaveSettings();
          m_checkMailTimer.Start();
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl = message.SenderControlId;
          if (iControl == (int)Controls.CONTROL_SET_ALL_UNREAD)
          {
            // unread all
            foreach (MailBox mb in m_mailBox)
            {
              if (System.IO.File.Exists(mb.MailboxFolder + @"\knownmails.txt"))
                System.IO.File.Delete(mb.MailboxFolder + @"\knownmails.txt");
              if (System.IO.File.Exists(mb.MailboxFolder + @"\transferList.txt"))
                System.IO.File.Delete(mb.MailboxFolder + @"\transferList.txt");
            }
            SetMailBoxList();
          }
          if (iControl == (int)Controls.CONTROL_SWITCH_AUTOCHECK)
          {
            if (m_bAutoCheck == true)
              m_bAutoCheck = false;
            else
              m_bAutoCheck = true;

            GUIToggleButtonControl theControl = (GUIToggleButtonControl)GUIWindowManager.GetWindow(GetID).GetControl(iControl);
            if (theControl != null)
            {
              theControl.Selected = m_bAutoCheck;
              SetAutoCheckButton();
            }
          }

          if (iControl == (int)Controls.CONTROL_REFRESH_ALL)
          {
            RefreshAllBoxes(0);
          }

          if (iControl == (int)Controls.CONTROL_LIST)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int iItem = (int)msg.Param1;
            int iAction = (int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
          }
          break;

      }

      return base.OnMessage(message);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
      base.OnAction(action);
    }
    private void checkMails(object sender, System.EventArgs e)
    {
      if (m_mailBox.Count > 0)
        if (m_bAutoCheck)
        {
          m_autoCheck = true;
          RefreshAllBoxes(0);
        }
    }
    //
    // handle data income
    private void m_mc_InformUser(int msgNum, object data)
    {
      if ((int)GUIWindowManager.ActiveWindow == GetID)
      {
        string strObjects = String.Format("{0} {1}...", GUILocalizeStrings.Get(msgNum), (string)data);
        GUIPropertyManager.SetProperty("#itemcount", strObjects);
        GUIControl ctrl = GetControl((int)Controls.CONTROL_LABELFILES);
        if (ctrl != null)
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
      }

    }
    private void m_mc_GotMailData(object eventObject, string mailText, int results)
    {
      int newMailsCount = 0;
      // error handling
      switch (results)
      {
        case 1:// timeout error
          if ((int)GUIWindowManager.ActiveWindow == GetID)
          {
            string strObjects = String.Format("{0} '{1}'", "(!)Timeout", m_currMailBox.BoxLabel);
            GUIPropertyManager.SetProperty("#itemcount", strObjects);
            GUIControl ctrl = GetControl((int)Controls.CONTROL_LABELFILES);
            if (ctrl != null)
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
          }
          m_currentView = (int)Views.VIEW_ERROR_HAPPEND;
          break;
        case 999:// this is an extended error, the message is from the mail server
          if ((int)GUIWindowManager.ActiveWindow == GetID)
          {
            string strObjects = String.Format("{0} '{1}'", (string)eventObject, m_currMailBox.BoxLabel);
            GUIPropertyManager.SetProperty("#itemcount", strObjects);
            GUIControl ctrl = GetControl((int)Controls.CONTROL_LABELFILES);
            if (ctrl != null)
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
          }
          m_currentView = (int)Views.VIEW_ERROR_HAPPEND;
          break;
      }//
      //
      switch (m_currMailAction)
      {
        case (int)MailActions.ACTION_REFRESH_MAILBOX:
          int mbNumber = GetMailBoxNumber(m_currMailBox);
          if (m_mailBox.Count > 0)
          {
            mbNumber++;
            RefreshAllBoxes(mbNumber);
          }
          break;
        case (int)MailActions.ACTION_LIST_MAILS:
          break;
        case (int)MailActions.ACTION_DO_NOTHING:
          break;
        case (int)MailActions.ACTION_TIMER_EVENT:
          break;
        case (int)MailActions.ACTION_DELETE_MAIL:
          break;

      }
      if (m_autoCheck == true)
      {
        string data = GUILocalizeStrings.Get(8023);
        foreach (MailBox mb in m_mailBox)
        {
          newMailsCount += m_mc.CountNewMail(mb);
          data += " in Mailbox '" + mb.BoxLabel + "': " + Convert.ToString(m_mc.CountNewMail(mb)) + " " + GUILocalizeStrings.Get(8004) + " ";
        }
        if (newMailsCount > 0)
        {
          MediaPortal.Util.Utils.PlaySound("notify.wav", false, true);
          DisplayOverlayNotify(true, data);
        }
      }
    }
    // build the list of mailboxes
    void SetMailBoxList()
    {
      m_currentView = (int)Views.VIEW_MAILBOX;
      string strObjects = "";
      if ((int)GUIWindowManager.ActiveWindow == GetID)
      {

        GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
        ArrayList itemlist = new ArrayList();
        // make item list
        foreach (MailBox mb in m_mailBox)
        {
          m_mc.SetMailboxPath(mb.MailboxFolder, mb.AttachmentFolder);
          int nmc = m_mc.CountNewMail(mb);
          GUIListItem item = new GUIListItem(mb.Username);
          if (nmc != 0)
            item.IconImage = "newMailIcon.png";
          item.Label = mb.BoxLabel + " (" + Convert.ToString(mb.MailCount) + ")";
          item.Label2 = "(" + mb.ServerAddress + ")";
          item.Label3 = "";
          item.Path = mb.Username;
          item.IsFolder = false;
          itemlist.Add(item);

        }

        // list items
        foreach (GUIListItem item in itemlist)
        {
          GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_LIST, item);
        }
        strObjects = String.Format("{0} {1}", itemlist.Count, GUILocalizeStrings.Get(632));
        GUIPropertyManager.SetProperty("#itemcount", strObjects);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
      }

    }
    //
    void ClearItemList()
    {
      if ((int)GUIWindowManager.ActiveWindow == GetID)
      {
        GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
        string strObjects = String.Format("{0} {1}", 0, GUILocalizeStrings.Get(632));
        GUIPropertyManager.SetProperty("#itemcount", strObjects);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
      }
    }
    // add a incoming mail
    int GetMailBoxNumber(MailBox theMailbox)
    {
      int count = 0;
      foreach (MailBox mb in m_mailBox)
      {
        if (mb.Equals(theMailbox))
          return count;
        count++;
      }
      return -1;
    }
    void SetMailsList()
    {
      if ((int)GUIWindowManager.ActiveWindow == GetID && m_currMailBox.MailCount > 0)
      {
        m_currentView = (int)Views.VIEW_MAILS;
        ArrayList itemlist = new ArrayList();
        GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
        System.IO.FileInfo[] theMails = null;
        m_mc.SetMailboxPath(m_currMailBox.MailboxFolder, m_currMailBox.AttachmentFolder);
        int mCount = m_mc.GetEMailList(m_currMailBox.MailboxFolder, ref theMails);
        string path = m_currMailBox.MailboxFolder + @"\";
        foreach (System.IO.FileInfo fInfo in theMails)
        {
          eMail theMail = m_mc.ParseMailText(m_mc.LoadEMail(path + fInfo.Name), false);
          GUIListItem item = new GUIListItem(theMail.Subject);
          if (m_mc.IsMailKnown(path + fInfo.Name) == false)
            item.IconImage = "newMailIcon.png";
          item.Label = theMail.Subject;
          item.Label2 = theMail.From;
          item.Label3 = "";
          item.Path = path + fInfo.Name;
          item.IsFolder = false;
          itemlist.Add(item);
        }
        //
        GUIListItem dirUp = new GUIListItem("..");
        dirUp.Path = "mailboxlist"; // to get where we are
        dirUp.IsFolder = false;
        dirUp.ThumbnailImage = "";
        dirUp.IconImage = "defaultFolderBack.png";
        dirUp.IconImageBig = "defaultFolderBackBig.png";
        itemlist.Insert(0, dirUp);
        //
        foreach (GUIListItem item in itemlist)
        {
          GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_LIST, item);
        }
        string strObjects = String.Format("{0} {1}", itemlist.Count - 1, GUILocalizeStrings.Get(632));
        GUIPropertyManager.SetProperty("#itemcount", strObjects);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
        DisplayOverlayNotify(false, ""); // remove the notify
      }

    }
    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("mymail", "autoCheck", m_bAutoCheck);
      }


    }
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
      {
        int boxCount = 0;
        MailBox tmpBox;
        m_mailBox.Clear();
        m_checkMailTimer.Interval = xmlreader.GetValueAsInt("mymail", "timer", 300000);
        m_bAutoCheck = xmlreader.GetValueAsBool("mymail", "autoCheck", false);
        boxCount = xmlreader.GetValueAsInt("mymail", "mailBoxCount", 0);
        System.IO.FileInfo[] theMails = null;
        if (boxCount > 0)
        {
          for (int i = 0; i < boxCount; i++)
          {
            string[] boxData = null;
            string mailBoxString = xmlreader.GetValueAsString("mymail", "mailBox" + Convert.ToString(i), "");
            if (mailBoxString.Length > 0)
            {
              boxData = mailBoxString.Split(new char[] { ';' });
              //<OKAY_AWRIGHT>
              if ((boxData.Length == 8) || ((boxData.Length > 8) && (boxData[8] == "T")))
              {
                tmpBox = new MailBox(boxData[0], boxData[1], boxData[2], boxData[3], Convert.ToInt16(boxData[4]), Convert.ToByte(boxData[5]), boxData[6], boxData[7]);
                //</OKAY_AWRIGHT>
                tmpBox.MailCount = m_mc.GetEMailList(tmpBox.MailboxFolder, ref theMails);
                if (tmpBox != null)
                  m_mailBox.Add(tmpBox);
              }
            }
          }
        }

      }
    }

    // set mail box
    void OnClick(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item == null) return;
      switch (m_currentView)
      {
        case (int)Views.VIEW_MAILBOX:
          if (iItem >= 0 && iItem <= m_mailBox.Count - 1)
          {
            m_currMailBox = (MailBox)m_mailBox[iItem];
            m_mc.SetMailboxPath(m_currMailBox.MailboxFolder, m_currMailBox.AttachmentFolder);
            m_prevMailBox = m_currMailBox;
            if (m_currMailBox.MailCount > 0)
            {
              m_currMailAction = (int)MailActions.ACTION_LIST_MAILS;
              m_currentView = (int)Views.VIEW_MAILS;
              SetMailsList();
            }
          }
          break;
        case (int)Views.VIEW_MAILS:
          if (iItem >= 0)
          {
            if (iItem == 0)
              SetMailBoxList();
            else
              if (m_currMailBox.MailCount > 0)
              {
                //get email here
                string mailText = m_mc.LoadEMail(item.Path);
                eMail theMail = m_mc.ParseMailText(mailText, true);
                if (theMail != null)
                {
                  m_mc_InformUser(8025, " (please wait...)");
                  m_mc.SetMailboxPath(m_currMailBox.MailboxFolder, m_currMailBox.AttachmentFolder);
                  m_mc.SetMailToKnownState(mailText);
                  ShowMail(theMail);
                  if (m_currMailBox.MailCount == 0)
                    SetMailBoxList();

                }
              }

          }
          break;
      }
    }

    void RefreshAllBoxes(int mbNumber)
    {
      if (m_mailBox.Count > 0)
      {
        if (mbNumber <= m_mailBox.Count - 1)
        {

          GUIControl.DisableControl(GetID, (int)Controls.CONTROL_REFRESH_ALL);

          m_currMailBox = (MailBox)m_mailBox[mbNumber];
          if ((int)GUIWindowManager.ActiveWindow == GetID)
          {
            string strObjects = String.Format("{0} '{1}'", GUILocalizeStrings.Get(8005), m_currMailBox.BoxLabel);
            GUIPropertyManager.SetProperty("#itemcount", strObjects);
            GUIControl ctrl = GetControl((int)Controls.CONTROL_LABELFILES);
            if (ctrl != null)
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
          }
          m_currMailAction = (int)MailActions.ACTION_REFRESH_MAILBOX;

          m_mc.ReadMailBox(ref m_currMailBox);
        }
        else
        {
          GUIControl.EnableControl(GetID, (int)Controls.CONTROL_REFRESH_ALL);

          if (m_currentView == (int)Views.VIEW_MAILBOX)
          {
            SetMailBoxList();
          }

          if (m_currentView == (int)Views.VIEW_MAILS)
          {
            if (m_prevMailBox != null)
            {
              m_currMailBox = m_prevMailBox;
              m_prevMailBox = null;
              SetMailsList();
            }
          }
        }
      }

    }
    //mb.BoxLabel+"("+Convert.ToString(mb.NewMailCount)+"/"+Convert.ToString(mb.MailCount)+")";
    GUIListItem GetSelectedItem()
    {
      int iControl;
      iControl = (int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      return item;
    }
    void ShowMail(eMail theMail)
    {
      MailInfo mailWindow = (MailInfo)GUIWindowManager.GetWindow(8001);
      mailWindow.SetEMail = theMail;
      mailWindow.SetMailBox = m_currMailBox;
      GUIWindowManager.ActivateWindow(8001);
    }
    void SetAutoCheckButton()
    {
      GUIToggleButtonControl theControl = (GUIToggleButtonControl)GUIWindowManager.GetWindow(GetID).GetControl((int)Controls.CONTROL_SWITCH_AUTOCHECK);
      if (theControl != null)
      {
        theControl.Selected = m_bAutoCheck;
      }
    }

    void DisplayOverlayNotify(bool state, string data)
    {
      MailOverlay mailOverlay = (MailOverlay)GUIWindowManager.GetWindow(8002);
      if (mailOverlay != null)
      {
        GUIFadeLabel fader = (GUIFadeLabel)mailOverlay.GetControl(2);

        if (fader != null)
        {
          fader.Label = data;
          fader.IsVisible = state;
        }
      }
    }

  }
}