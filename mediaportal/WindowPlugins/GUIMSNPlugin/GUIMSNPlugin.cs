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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using System.IO;
using XihSolutions.DotMSN;
using XihSolutions.DotMSN.Core;
using XihSolutions.DotMSN.DataTransfer;

namespace MediaPortal.GUI.MSN
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMSNPlugin : GUIWindow, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {
    #region enums
    enum Controls
    {
      CONTROL_BTNVIEWASICONS = 2,
      CONTROL_BTNSORTBY = 3,
      CONTROL_BTNSORTASC = 4,
      CONTROL_BTNSTATUS = 5,
      CONTROL_BTN_CONNECT = 6,
      CONTROL_BTN_NEXT = 7,

      CONTROL_LIST = 50,
      CONTROL_THUMBS = 51,
      CONTROL_LABELFILES = 12,
      CONTROL_EJECT = 13
    }


    #endregion

    #region  variables
    // this object will be the interface to the dotMSN library
    static private XihSolutions.DotMSN.Messenger _messenger = null;
    static private Conversation _currentconversation = null;
    static private PresenceStatus _currentStatus = PresenceStatus.Online;
    GUIDialogProgress _dlgProgress;
    bool _isDialogVisible = false;
    // bool m_bConnected=false;
    #endregion

    #region Base variabeles
    enum SortMethod
    {
      SORT_NAME = 0,
      SORT_STATUS = 1,
    }

    enum View : int
    {
      VIEW_AS_LIST = 0,
      VIEW_AS_ICONS = 1,
      VIEW_AS_LARGEICONS = 2,
    }
    View currentView = View.VIEW_AS_LIST;
    SortMethod currentSortMethod = SortMethod.SORT_NAME;
    bool m_bSortAscending = true;
    bool _refreshContactList = false;
    static DateTime dateLastTyped = DateTime.MinValue;
    static string contactname = "";
    #endregion

    public GUIMSNPlugin()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MSN;
    }
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\my messenger.xml");
    }

    public override void PreInit()
    {
      bool autosignin = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        autosignin = xmlreader.GetValueAsInt("MSNmessenger", "autosignin", 0) != 0;
      }
      if (autosignin)
        StartMSN(false);
    }

    static public XihSolutions.DotMSN.Messenger Messenger
    {
      get { return _messenger; }
    }
    static public Conversation CurrentConversation
    {
      get
      {
        try
        {
          if (_messenger == null) return null;
          if (_messenger.Connected == false) return null;
          return _currentconversation;
        }
        catch (Exception)
        {
        }
        return null;
      }
    }
    static public void CloseConversation()
    {
      if (_currentconversation != null)
      {
        if (_currentconversation.Switchboard.IsSessionEstablished)
        {
          _currentconversation.Switchboard.Close();
        }
      }
      _currentconversation = null;
      contactname = String.Empty;
    }
    static public bool IsTyping
    {
      get
      {
        TimeSpan ts = DateTime.Now - dateLastTyped;
        if (ts.TotalMilliseconds < 3000) return true;
        return false;
      }
    }
    static public string ContactName
    {
      get { return contactname; }
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


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION:
          // Close conversation
          CloseConversation();
          break;

        case GUIMessage.MessageType.GUI_MSG_NEW_LINE_ENTERED:
          if (GUIMSNPlugin.Messenger != null)
          {
            GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE, (int)GUIWindow.Window.WINDOW_MSN_CHAT, GetID, 0, 0, 0, null);
            msg2.Label = message.Label;
            msg2.SendToTargetWindow = true;
            GUIGraphicsContext.SendMessage(msg2);

            if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_MSN_CHAT)
            {
              GUIMessage msg3 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE, GUIWindowManager.ActiveWindow, GetID, 0, 0, 0, null);
              msg3.Label = message.Label;
              msg3.SendToTargetWindow = true;
              GUIGraphicsContext.SendMessage(msg3);
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          _isDialogVisible = false;
          _dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

          base.OnMessage(message);
          if (_messenger == null)
          {
            try
            {
              _messenger = new Messenger();
            }
            catch (Exception)
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
              return true;
            }
          }

          ShowThumbPanel();
          UpdateButtons();
          UpdateStatusButton();
          UpdateSortButton();
          Update();
          _refreshContactList = true;
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          if (_isDialogVisible)
          {
            _isDialogVisible = false;
            _dlgProgress.Close();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl = message.SenderControlId;
          if (iControl == (int)Controls.CONTROL_BTN_CONNECT)
          {
            if (_messenger != null && _messenger.Connected)
            {
              StopMSN();
            }
            else
            {
              if (_dlgProgress != null)
              {
                _dlgProgress.SetHeading(901);//MSN Messenger
                _dlgProgress.SetLine(1, GUILocalizeStrings.Get(910));//Signing in...
                _dlgProgress.SetLine(2, "");
                _dlgProgress.SetLine(3, "");
                _dlgProgress.StartModal(GetID);
                _dlgProgress.Progress();
              }
              _isDialogVisible = true;

              StartMSN(true);

            }

            ShowThumbPanel();
            UpdateButtons();
            Update();
          }
          if (iControl == (int)Controls.CONTROL_BTNVIEWASICONS)
          {
            switch (currentView)
            {
              case View.VIEW_AS_LIST:
                currentView = View.VIEW_AS_ICONS;
                break;
              case View.VIEW_AS_ICONS:
                currentView = View.VIEW_AS_LARGEICONS;
                break;
              case View.VIEW_AS_LARGEICONS:
                currentView = View.VIEW_AS_LIST;
                break;
              default:
                currentView = View.VIEW_AS_LIST;
                break;
            }
            UpdateButtons();
            ShowThumbPanel();
            GUIControl.FocusControl(GetID, iControl);
          }

          if (iControl == (int)Controls.CONTROL_BTNSORTASC)
          {
            m_bSortAscending = !m_bSortAscending;
            OnSort();
            FillContactList();
            UpdateButtons();
            GUIControl.FocusControl(GetID, iControl);
          }

          if (iControl == (int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            switch (currentSortMethod)
            {
              case SortMethod.SORT_NAME:
                currentSortMethod = SortMethod.SORT_STATUS;
                break;

              case SortMethod.SORT_STATUS:
                currentSortMethod = SortMethod.SORT_NAME;
                break;
            }

            FillContactList();
            OnSort();
            UpdateSortButton();
          }

          if (iControl == (int)Controls.CONTROL_THUMBS || iControl == (int)Controls.CONTROL_LIST)
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

          if (iControl == (int)Controls.CONTROL_BTNSTATUS) // Set new status
          {
            _currentStatus = (PresenceStatus)GetControl((int)Controls.CONTROL_BTNSTATUS).SelectedItem;
            FillContactList();
            UpdateStatusButton();
          }
          break;

      }
      return base.OnMessage(message);
    }

    void UpdateSortButton()
    {
      string strLine = "";
      switch (currentSortMethod)
      {
        case SortMethod.SORT_NAME:
          strLine = GUILocalizeStrings.Get(103);//Sort by: Name
          break;

        case SortMethod.SORT_STATUS:
          strLine = GUILocalizeStrings.Get(685);//Sort by: Status
          break;
      }
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSORTBY, strLine);
    }

    void UpdateStatusButton()
    {
      if (_messenger == null)
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSTATUS, GUILocalizeStrings.Get(961));//MSN Status
        return;
      }
      string strLine = "";
      if (_messenger.Connected && _messenger.Nameserver.IsSignedIn)
      {
        switch (_currentStatus)
        {
          case PresenceStatus.Busy:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.Busy);
            strLine = GUILocalizeStrings.Get(949);//Busy
            break;
          case PresenceStatus.BRB:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.BRB);
            strLine = GUILocalizeStrings.Get(950);//Be Right Back
            break;
          case PresenceStatus.Away:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.Away);
            strLine = GUILocalizeStrings.Get(951);//Away
            break;
          case PresenceStatus.Phone:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.Phone);
            strLine = GUILocalizeStrings.Get(952);//Phone
            break;
          case PresenceStatus.Lunch:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.Lunch);
            strLine = GUILocalizeStrings.Get(953);//Lunch
            break;
          case PresenceStatus.Hidden:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.Hidden);
            strLine = GUILocalizeStrings.Get(954);//Hidden
            break;
          case PresenceStatus.Online:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.Online);
            strLine = GUILocalizeStrings.Get(948);//Online
            break;
          default:
            _messenger.Nameserver.SetPresenceStatus(PresenceStatus.Online);
            strLine = GUILocalizeStrings.Get(948);//Online
            break;
        }

        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSTATUS, strLine);
      }
      else
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSTATUS, GUILocalizeStrings.Get(961));//MSN Status
      }
    }

    bool ViewByIcon
    {
      get
      {
        if (currentView != View.VIEW_AS_LIST) return true;
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (currentView == View.VIEW_AS_LARGEICONS) return true;
        return false;
      }
    }

    GUIListItem GetSelectedItem()
    {
      int iControl;
      if (ViewByIcon)
      {
        iControl = (int)Controls.CONTROL_THUMBS;
      }
      else
        iControl = (int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      int iControl;
      if (ViewByIcon)
      {
        iControl = (int)Controls.CONTROL_THUMBS;
      }
      else
        iControl = (int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetListItem(GetID, iControl, iItem);
      return item;
    }

    int GetSelectedItemNo()
    {
      int iControl;
      if (ViewByIcon)
      {
        iControl = (int)Controls.CONTROL_THUMBS;
      }
      else
        iControl = (int)Controls.CONTROL_LIST;

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      int iControl;
      if (ViewByIcon)
      {
        iControl = (int)Controls.CONTROL_THUMBS;
      }
      else
        iControl = (int)Controls.CONTROL_LIST;

      return GUIControl.GetItemCount(GetID, iControl);
    }


    void UpdateButtons()
    {
      GUIControl.HideControl(GetID, (int)Controls.CONTROL_LIST);
      GUIControl.HideControl(GetID, (int)Controls.CONTROL_THUMBS);

      int iControl = (int)Controls.CONTROL_LIST;
      if (ViewByIcon)
        iControl = (int)Controls.CONTROL_THUMBS;

      GUIControl.ShowControl(GetID, iControl);
      //      GUIControl.FocusControl(GetID,iControl);

      string strLine = "";
      switch (currentView)
      {
        case View.VIEW_AS_LIST:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.VIEW_AS_ICONS:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case View.VIEW_AS_LARGEICONS:
          strLine = GUILocalizeStrings.Get(417);
          break;
      }
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEWASICONS, strLine);


      bool bAsc = m_bSortAscending;
      if (bAsc)
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
    }

    void ShowThumbPanel()
    {
      int iItem = GetSelectedItemNo();
      if (ViewByLargeIcon)
      {
        GUIThumbnailPanel pControl = (GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
        pControl.ShowBigIcons(true);
      }
      else
      {
        GUIThumbnailPanel pControl = (GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
        pControl.ShowBigIcons(false);
      }
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iItem);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iItem);
      }
      UpdateButtons();
    }

    void Update()
    {
      if (_messenger != null && _messenger.Connected && _messenger.Nameserver.IsSignedIn)
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_CONNECT, GUILocalizeStrings.Get(904));//sign out
      }
      else
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_CONNECT, GUILocalizeStrings.Get(903));//Sign in
      }

    }

    void OnClick(int iItem)
    {
      if (_isDialogVisible) return;

      GUIListItem item = GetSelectedItem();
      if (item == null) return;
      if ((CurrentConversation != null) && (ContactName != ((Contact)item.AlbumInfoTag).Name)) return;
      if (_messenger == null) return;
      if (!_messenger.Connected) return;

      Contact contact = (Contact)item.AlbumInfoTag;

      if (ContactName == ((Contact)item.AlbumInfoTag).Name)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN_CHAT);
      }
      else
      {
        if (_dlgProgress != null)
        {
          _dlgProgress.SetHeading(901);//MSN Messenger
          _dlgProgress.SetLine(1, GUILocalizeStrings.Get(909));//Connecting...
          _dlgProgress.SetLine(2, contact.Name);
          _dlgProgress.SetLine(3, "");
          _dlgProgress.StartModal(GetID);
          _dlgProgress.Progress();
        }

        _isDialogVisible = true;
        _currentconversation = _messenger.CreateConversation();
        _currentconversation.Invite(contact);
      }
    }
    #region Sort Members
    void OnSort()
    {
      GUIListControl list = (GUIListControl)GetControl((int)Controls.CONTROL_LIST);
      list.Sort(this);
      GUIThumbnailPanel panel = (GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
      panel.Sort(this);

      UpdateButtons();
    }
    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2) return 0;
      if (item1 == null) return -1;
      if (item2 == null) return -1;
      if (item1.IsFolder && item1.Label == "..") return -1;
      if (item2.IsFolder && item2.Label == "..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;


      Contact contact1 = item1.AlbumInfoTag as Contact;
      Contact contact2 = item2.AlbumInfoTag as Contact;

      SortMethod method = currentSortMethod;
      bool bAscending = m_bSortAscending;
      switch (method)
      {
        case SortMethod.SORT_NAME:
          if (bAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }
        case SortMethod.SORT_STATUS:
          if (bAscending)
          {
            return String.Compare(contact1.Status.ToString(), contact2.Status.ToString(), true);
          }
          else
          {
            return String.Compare(contact2.Status.ToString(), contact1.Status.ToString(), true);
          }
        default:
          if (bAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }
      }
    }
    #endregion

    public override void Process()
    {
      if (_messenger == null) return;
      if (_messenger.Connected == false) return;
      if (_refreshContactList)
      {
        if (_isDialogVisible)
        {
          _isDialogVisible = false;
          _dlgProgress.Close();
        }
        FillContactList();
      }
    }

    void FillContactList()
    {
      _refreshContactList = false;
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS);

      int iContacts = 0;
      if (_messenger != null)
      {
        if (_messenger.Connected && _messenger.Nameserver.IsSignedIn)
        {
          foreach (Contact contact in _messenger.ContactList.All)
          {
            if (contact.OnBlockedList) continue;
            // if the contact is not offline we can send messages and we want to show
            // it in the contactlistview
            Log.Info("Contact:{0} status:{1}", contact.Name, contact.Status.ToString());
            if (contact.Status != PresenceStatus.Offline)
            {
              GUIListItem item = new GUIListItem(contact.Name);
              item.Label2 = contact.Status.ToString();
              item.IsFolder = false;
              item.AlbumInfoTag = contact;
              item.IconImage = "Messenger_Buddies.png";
              item.IconImageBig = "Messenger_Buddies.png";

              GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_LIST, item);
              GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_THUMBS, item);
              iContacts++;
            }
          }
        }
      }
      string strObjects = String.Format("{0} {1}", iContacts, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", strObjects);
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);


      OnSort();
      UpdateStatusButton();
      Update();
    }


    private void StopMSN()
    {
      Log.Info("MSN:Stop MSN");
      _refreshContactList = true;
      if (_messenger == null) return;
      try
      {
        if (_messenger.Connected)
          _messenger.Disconnect();
        _messenger = null;
      }
      catch (Exception)
      {
      }
      FillContactList();
      Update();
    }

    // Called when the button 'Connected' is clicked
    private void StartMSN(bool showDialog)
    {
      Log.Info("MSN:Start MSN");
      string emailadres = "";
      string password = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        emailadres = xmlreader.GetValueAsString("MSNmessenger", "email", "");
        password = xmlreader.GetValueAsString("MSNmessenger", "password", "");
        switch (xmlreader.GetValueAsString("MSNmessenger", "initialstatus", "Online").ToLower())
        {
          case "online": _currentStatus = PresenceStatus.Online; break;
          case "busy": _currentStatus = PresenceStatus.Busy; break;
          case "berightback": _currentStatus = PresenceStatus.BRB; break;
          case "away": _currentStatus = PresenceStatus.Away; break;
          case "phone": _currentStatus = PresenceStatus.Phone; break;
          case "lunch": _currentStatus = PresenceStatus.Lunch; break;
          case "hidden": _currentStatus = PresenceStatus.Hidden; break;
          case "idle": _currentStatus = PresenceStatus.Idle; break;
        }
      }
      try
      {
        // make sure we don't use the default settings, since they're invalid
        if (emailadres == "")
        {
          if (!showDialog) return;
          _isDialogVisible = false;
          _dlgProgress.Close();
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, GUIWindowManager.ActiveWindow, 0, 0, 0, 0, null);
          msg.Param1 = 905;
          msg.Param2 = 906;
          msg.Param2 = -1;
          msg.SendToTargetWindow = true;
          GUIWindowManager.SendMessage(msg);
          return;
        }
        else
        {
          _messenger = new XihSolutions.DotMSN.Messenger();
          _messenger.Credentials.Account = emailadres;
          _messenger.Credentials.Password = password;
          // We need to use a valid ClientCode/ID for Server Pings (Challenges)
          // otherwise we receive a server error 540 after a while and loose the connection
          _messenger.Credentials.ClientCode = "Q1P7W2E4J9R8U3S5";
          _messenger.Credentials.ClientID = "msmsgs@msnmsgr.com";
          Log.Info("MSN: email:{0} pwd:*********", emailadres);
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
          {
            bool useProxy = xmlreader.GetValueAsBool("MSNmessenger", "useproxy", false);
            if (useProxy)
            {
              _messenger.Nameserver.ConnectivitySettings = new ConnectivitySettings();
              _messenger.Nameserver.ConnectivitySettings.ProxyHost = xmlreader.GetValueAsString("MSNmessenger", "proxyhost", "");
              _messenger.Nameserver.ConnectivitySettings.ProxyPort = xmlreader.GetValueAsInt("MSNmessenger", "proxyport", 8080);
              _messenger.Nameserver.ConnectivitySettings.ProxyUsername = xmlreader.GetValueAsString("MSNmessenger", "proxyusername", "");
              _messenger.Nameserver.ConnectivitySettings.ProxyPassword = xmlreader.GetValueAsString("MSNmessenger", "proxypassword", "");

              int proxyType = xmlreader.GetValueAsInt("MSNmessenger", "proxytype", 1);
              if (proxyType == 1) _messenger.Nameserver.ConnectivitySettings.ProxyType = ProxyType.Socks5;
              else _messenger.Nameserver.ConnectivitySettings.ProxyType = ProxyType.Socks4;
              // _messenger.Nameserver.ConnectivitySettings.ProxyType = settings;
              Log.Info("MSN: proxy:{0}:{1} {2}/****",
                  _messenger.Nameserver.ConnectivitySettings.ProxyHost,
                  _messenger.Nameserver.ConnectivitySettings.ProxyPort,
                  _messenger.Nameserver.ConnectivitySettings.ProxyUsername);
            }
          }
          _messenger.NameserverProcessor.ConnectionEstablished += new EventHandler(ConnectionEstablished);
          _messenger.Nameserver.SignedIn += new EventHandler(Nameserver_SignedIn);
          _messenger.Nameserver.SignedOff += new SignedOffEventHandler(Nameserver_SignedOff);
          _messenger.NameserverProcessor.ConnectingException += new ProcessorExceptionEventHandler(NameserverProcessor_ConnectingException);
          _messenger.Nameserver.ExceptionOccurred += new HandlerExceptionEventHandler(Nameserver_ExceptionOccurred);
          _messenger.Nameserver.AuthenticationError += new HandlerExceptionEventHandler(Nameserver_AuthenticationError);
          _messenger.Nameserver.ServerErrorReceived += new ErrorReceivedEventHandler(Nameserver_ServerErrorReceived);
          _messenger.ConversationCreated += new ConversationCreatedEventHandler(ConversationCreated);
          //          _messenger.TransferInvitationReceived += new MSNSLPInvitationReceivedEventHandler(messenger_TransferInvitationReceived);

          // setup the callbacks
          // we log when someone goes online
          _messenger.Nameserver.AutoSynchronize = true;
          _messenger.Nameserver.ContactOnline += new ContactChangedEventHandler(Nameserver_ContactOnline);
          _messenger.Nameserver.ContactOffline += new ContactChangedEventHandler(Nameserver_ContactOffline);
          _messenger.Nameserver.ContactStatusChanged += new ContactStatusChangedEventHandler(Nameserver_ContactStatusChanged);

          // notify us when synchronization is completed
          _messenger.Connect();

        }
      }
      catch (Exception ex)
      {
        Log.Info("MSN:Connect exception:{0}", ex.Message);
        _isDialogVisible = false;
        _dlgProgress.Close();


        if (!showDialog) return;
        // in case of an error, report this to the user (or developer)
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);
        pDlgOK.SetHeading(GUILocalizeStrings.Get(901));//MSN Messenger
        pDlgOK.SetLine(1, GUILocalizeStrings.Get(907));//Could not connect to MSN
        pDlgOK.SetLine(2, "");
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
        return;
      }
    }
    private void Nameserver_ServerErrorReceived(object sender, MSNErrorEventArgs e)
    {
      // when the MSN server sends an error code we want to be notified.
      //MessageBox.Show(e.MSNError.ToString(), "Server error received");
      Log.Info("MSN:Server error received");
    }

    void Nameserver_ContactStatusChanged(object sender, ContactStatusChangeEventArgs e)
    {
      Log.Info("MSN:contact status changed:{0} {1}", e.Contact.Name, e.Contact.Status.ToString());
      _refreshContactList = true;
    }

    void Nameserver_ContactOffline(object sender, ContactEventArgs e)
    {
      Log.Info("MSN:contact offline:{0}.", e.Contact.Name);
      _refreshContactList = true;
    }

    void Nameserver_ContactOnline(object sender, ContactEventArgs e)
    {
      Log.Info("MSN:contact online:{0}.", e.Contact.Name);
      _refreshContactList = true;
    }


    /// <summary>
    /// Connected with MSN server
    /// </summary>
    private void ConnectionEstablished(object sender, EventArgs e)
    {
      Log.Info("MSN:connection established.");
    }

    /// <summary>
    /// Signed in to MSN server
    /// </summary>
    private void Nameserver_SignedIn(object sender, EventArgs e)
    {
      Log.Info("MSN:signed in.");
      if (_isDialogVisible)
      {
        _isDialogVisible = false;
        _dlgProgress.Close();
      }
      _refreshContactList = true;
    }

    /// <summary>
    /// Signed off from MSN server
    /// </summary>
    private void Nameserver_SignedOff(object sender, SignedOffEventArgs e)
    {
      Update();
      Log.Info("MSN:signed off.");
    }

    /// <summary>
    /// Failed to connect/sign in to MSN server
    /// </summary>
    private void Nameserver_ExceptionOccurred(object sender, ExceptionEventArgs e)
    {
      // ignore the unauthorized exception, since we're handling that error in another method.
      if (e.Exception is UnauthorizedException)
        return;

      Log.Info("MSN:unable to connect:{0}", e.Exception.Message);
      if (_isDialogVisible)
      {
        _isDialogVisible = false;
        _dlgProgress.Close();
      }
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);
      pDlgOK.SetHeading(GUILocalizeStrings.Get(901));//MSN Messenger
      pDlgOK.SetLine(1, GUILocalizeStrings.Get(907));//Could not connect to MSN
      pDlgOK.SetLine(2, "");
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
    }

    /// <summary>
    /// Failed to connect to MSN server
    /// </summary>
    private void NameserverProcessor_ConnectingException(object sender, ExceptionEventArgs e)
    {
      Log.Info("MSN:unable to connect:{0}", e.Exception.Message);
      if (_isDialogVisible)
      {
        _isDialogVisible = false;
        _dlgProgress.Close();
      }
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);
      pDlgOK.SetHeading(GUILocalizeStrings.Get(901));//MSN Messenger
      pDlgOK.SetLine(1, GUILocalizeStrings.Get(907));//Could not connect to MSN
      pDlgOK.SetLine(2, "");
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
    }

    /// <summary>
    /// Failed to sign in username/pwd incorrect
    /// </summary>
    private void Nameserver_AuthenticationError(object sender, ExceptionEventArgs e)
    {
      Log.Info("MSN:unable to connect:invalid username/password");
      if (_isDialogVisible)
      {
        _isDialogVisible = false;
        _dlgProgress.Close();
      }
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);
      pDlgOK.SetHeading(GUILocalizeStrings.Get(901));//MSN Messenger
      pDlgOK.SetLine(1, GUILocalizeStrings.Get(907));//Could not connect to MSN
      pDlgOK.SetLine(2, "");
      pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
    }





    /// <summary>
    /// When the MSN server responds we can setup a conversation (the other party agreed)
    /// the Messenger.ConversationCreated event is called so we can initialize the
    /// Conversation object.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConversationCreated(object sender, ConversationCreatedEventArgs e)
    {
      e.Conversation.Switchboard.TextMessageReceived += new TextMessageReceivedEventHandler(Switchboard_TextMessageReceived);
      e.Conversation.Switchboard.SessionClosed += new SBChangedEventHandler(Switchboard_SessionClosed);
      e.Conversation.Switchboard.ContactJoined += new ContactChangedEventHandler(Switchboard_ContactJoined);
      e.Conversation.Switchboard.ContactLeft += new ContactChangedEventHandler(Switchboard_ContactLeft);
      e.Conversation.Switchboard.UserTyping += new UserTypingEventHandler(Switchboard_UserTyping);
      e.Conversation.Switchboard.AllContactsLeft += new SBChangedEventHandler(Switchboard_AllContactsLeft);
      // we want to be accept filetransfer invitations
      //e.Conversation.FileTransferHandler.InvitationReceived +=new DotMSN.FileTransferHandler.FileTransferInvitationHandler(FileTransferHandler_FileTransferInvitation);
      UpdateStatusButton();
      if (_currentconversation == null)
      {
        _currentconversation = e.Conversation;
      }
    }

    void Switchboard_AllContactsLeft(object sender, EventArgs e)
    {
      CloseConversation();
    }

    private void Switchboard_SessionClosed(object sender, EventArgs e)
    {
      //conversation has stopped...
      CloseConversation();
    }

    // this is actually just annoying but it proves the concept
    void Switchboard_UserTyping(object sender, ContactEventArgs e)
    {
      dateLastTyped = DateTime.Now;
      //contactname=e.Contact.Name;
      //MessageBox.Show(this, e.Contact.Name + " is typing");
    }


    /// <summary>
    /// After the first contact has joined you can actually send messages to the
    /// other contact(s)!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Switchboard_ContactJoined(object sender, ContactEventArgs e)
    {
      // someone joined our conversation! remember that this also occurs when you are
      // only talking to 1 other person. Log this event.
      //Log.Write += e.Contact.Name + " joined the conversation.\r\n";

      if (_isDialogVisible)
      {
        _isDialogVisible = false;
        _dlgProgress.Close();
      }

      if (_currentconversation == null)
      {
        // new conversation
        contactname = e.Contact.Name;
      }

      if ((!GUIGraphicsContext.IsFullScreenVideo) && (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_MSN_CHAT))
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN_CHAT);
      }

      // Send message	
      SendStatusMessage(e.Contact.Name, e.Contact.Mail, GUILocalizeStrings.Get(959));//has joined the conversation
    }

    private void Switchboard_ContactLeft(object sender, ContactEventArgs e)
    {
      // Send message
      SendStatusMessage(e.Contact.Name, e.Contact.Mail, GUILocalizeStrings.Get(960));//has left the conversation
    }


    #region ISetupFormEx Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Use the MSN messenger in MediaPortal";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return (int)GUIWindow.Window.WINDOW_MSN;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      // TODO:  Add GUIMSNPlugin.GetHome implementation
      strButtonText = GUILocalizeStrings.Get(901);//MSN messenger
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string PluginName()
    {
      return "MSN Messenger";
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      MessengerSetup setup = new MessengerSetup();
      setup.ShowDialog();
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    private void ContactOffline(Messenger sender, ContactEventArgs e)
    {
      _refreshContactList = true;
    }

    private void ContactStatusChange(Messenger sender, ContactStatusChangeEventArgs e)
    {
      _refreshContactList = true;
    }

    private void Switchboard_TextMessageReceived(object sender, TextMessageEventArgs e)
    {
      string FormattedText = String.Format("{0}:{1}", e.Sender.Name, e.Message.Text);

      AddMessage(FormattedText);
    }

    private void SendStatusMessage(string sName, string sMail, string sText)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MSN_STATUS_MESSAGE, GUIWindowManager.ActiveWindow, GetID, 0, 0, 0, null);
      msg.Label = sName;
      msg.Label2 = sMail;
      msg.Label3 = sText;
      msg.SendToTargetWindow = true;
      GUIGraphicsContext.SendMessage(msg);
    }

    private void AddMessage(string text)
    {
      // Route to chat window
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE, (int)GUIWindow.Window.WINDOW_MSN_CHAT, GetID, 0, 0, 0, null);
      msg.Label = text;
      msg.SendToTargetWindow = true;
      GUIGraphicsContext.SendMessage(msg);

      // Route to active window
      if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_MSN_CHAT)
      {
        GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE, GUIWindowManager.ActiveWindow, GetID, 0, 0, 0, null);
        msg2.Label = text;
        msg2.SendToTargetWindow = true;
        GUIGraphicsContext.SendMessage(msg2);
      }
    }

  }
}
