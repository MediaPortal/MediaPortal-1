using System;
using System.Collections;
using System.Net;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using System.IO;
using DotMSN;

namespace MediaPortal.GUI.MSN
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
  public class GUIMSNPlugin : GUIWindow,IComparer, ISetupForm
  {
    enum Controls
    {
      CONTROL_BTNVIEWASICONS=		2,
      CONTROL_BTNSORTBY		=			3,
      CONTROL_BTNSORTASC	=			4,
      CONTROL_BTN_CONNECT    =     6,
      CONTROL_BTN_NEXT    =     7,
			
      CONTROL_LIST				=			50,
      CONTROL_THUMBS			=			51,
      CONTROL_LABELFILES  =       12,
      CONTROL_EJECT  =       13

    };
    // this object will be the interface to the dotMSN library
    static private DotMSN.Messenger messenger = null;
    static private Conversation currentconversation = null;
    #region Base variabeles
    enum SortMethod
    {
      SORT_NAME=0,
      SORT_STATUS=1,
    }

    enum View:int
    {
      VIEW_AS_LIST    =       0,
      VIEW_AS_ICONS    =      1,
      VIEW_AS_LARGEICONS  =   2,
    }
    View              currentView=View.VIEW_AS_LIST;
    SortMethod        currentSortMethod=SortMethod.SORT_NAME;
    bool              m_bSortAscending=true;
    bool              ReFillContactList=false;
    static DateTime   dateLastTyped=DateTime.MinValue;
    static string     contactname="";
    #endregion

    public GUIMSNPlugin()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MSN;
    }
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\my messenger.xml");
    }


    static public DotMSN.Messenger Messenger
    {
      get { return messenger;}
    }
    static public Conversation CurrentConversation 
    {
      get { return currentconversation;}
    }
    static public bool IsTyping
    {
      get
      {
        TimeSpan ts=DateTime.Now-dateLastTyped;
        if (ts.TotalMilliseconds<3000) return true;
        return false;
      }
    }
    static public string ContactName
    {
      get { return contactname;}
    }
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
        return;
      }

      base.OnAction(action);
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
          base.OnMessage(message);
          if (messenger==null)
          {
            try
            {
              messenger=new Messenger();
            }
            catch(Exception)
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);          
              return true;
            }
          }
          ShowThumbPanel();
          UpdateButtons();
          Update();
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_BTN_CONNECT)
          {
            if (!messenger.Connected)
            {
              StartMSN();
            }
            else
            {
              StopMSN();

            }
            ShowThumbPanel();
            UpdateButtons();
            Update();
          }
          if (iControl==(int)Controls.CONTROL_BTNVIEWASICONS)
          {
            currentView = (View) GetControl((int)Controls.CONTROL_BTNVIEWASICONS).SelectedItem;
            ShowThumbPanel();
            GUIControl.FocusControl(GetID,iControl);
          }
          
          if (iControl==(int)Controls.CONTROL_BTNSORTASC)
          {
            m_bSortAscending=!m_bSortAscending;
            OnSort();
            UpdateButtons();
            GUIControl.FocusControl(GetID,iControl);
          }


          if (iControl==(int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            currentSortMethod = (SortMethod)GetControl((int)Controls.CONTROL_BTNSORTBY).SelectedItem;
            OnSort();
            GUIControl.FocusControl(GetID,iControl);
          }
          if (iControl==(int)Controls.CONTROL_THUMBS||iControl==(int)Controls.CONTROL_LIST)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            GUIGraphicsContext.SendMessage(msg);         
            int iItem=(int)msg.Param1;
            int iAction=(int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
          }
          break;

      }
      return base.OnMessage(message);
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
        iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      int iControl;
      if (ViewByIcon)
      {
        iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetListItem(GetID,iControl,iItem);
      return item;
    }

    int GetSelectedItemNo()
    {
      int iControl;
      if (ViewByIcon)
      {
        iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
      GUIGraphicsContext.SendMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      int iControl;
      if (ViewByIcon)
      {
        iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;

      return GUIControl.GetItemCount(GetID,iControl);
    }


    void UpdateButtons()
    {
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST);
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_THUMBS);
      
      int iControl=(int)Controls.CONTROL_LIST;
      if (ViewByIcon)
        iControl=(int)Controls.CONTROL_THUMBS;
      GUIControl.ShowControl(GetID,iControl);
      GUIControl.FocusControl(GetID,iControl);
      

      bool bAsc=m_bSortAscending;
      if (bAsc)
        GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
    }

    void ShowThumbPanel()
    {
      int iItem=GetSelectedItemNo(); 
      if ( ViewByLargeIcon )
      {
        GUIThumbnailPanel pControl=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
        pControl.ShowBigIcons(true);
      }
      else
      {
        GUIThumbnailPanel pControl=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
        pControl.ShowBigIcons(false);
      }
      if (iItem>-1)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST,iItem);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS,iItem);
      }
      UpdateButtons();
    }

    void Update()
    {
      if (messenger !=null && messenger.Connected)
      {
        GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTN_CONNECT, GUILocalizeStrings.Get(904));
      }
      else
      {
        GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTN_CONNECT, GUILocalizeStrings.Get(903));
      }

    }

    void OnClick(int iItem)
    {
      if (messenger==null) return;
      if (!messenger.Connected) return;
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      Contact contact= (Contact)item.AlbumInfoTag;
      messenger.RequestConversation(contact.Mail);

    }
    #region Sort Members
    void OnSort()
    {
      GUIListControl list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST);
      list.Sort(this);
      GUIThumbnailPanel panel=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
      panel.Sort(this);

      UpdateButtons();
    }
    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;
      if (item1.IsFolder && item1.Label=="..") return -1;
      if (item2.IsFolder && item2.Label=="..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1; 


      Contact contact1=item1.AlbumInfoTag as Contact;
      Contact contact2=item2.AlbumInfoTag as Contact;

      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;
      switch (method)
      {
        case SortMethod.SORT_NAME:
          if (bAscending)
          {
            return String.Compare(item1.Label ,item2.Label,true);
          }
          else
          {
            return String.Compare(item2.Label ,item1.Label,true);
          }
        case SortMethod.SORT_STATUS:
          if (bAscending)
          {
            return String.Compare(contact1.Status.ToString() ,contact2.Status.ToString(),true);
          }
          else
          {
            return String.Compare(contact2.Status.ToString() ,contact1.Status.ToString(),true);
          }
        default:
          if (bAscending)
          {
            return String.Compare(item1.Label ,item2.Label,true);
          }
          else
          {
            return String.Compare(item2.Label ,item1.Label,true);
          }
      }
    }
    #endregion

    public override void Process()
    {
      if (ReFillContactList) FillContactList();
    }

    void FillContactList()
    {
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_THUMBS);

      int iContacts=0;
      foreach(Contact contact in messenger.GetListEnumerator(MSNList.ForwardList))
      {
        // if the contact is not offline we can send messages and we want to show
        // it in the contactlistview
        if(contact.Status != MSNStatus.Offline)
        {
          GUIListItem item =new GUIListItem(contact.Name);
          item.Label2=contact.Status.ToString();
          item.IsFolder=false;
          item.AlbumInfoTag=contact;
          item.IconImage="Messenger_Buddies.png";
          item.IconImageBig="Messenger_Buddies.png";

          GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
          GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
          iContacts++;
        }
      }
      string  strObjects=String.Format("{0} {1}", iContacts, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects);
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
      ReFillContactList=false;
    }


    private void StopMSN()
    {
      ReFillContactList=true;
      if (messenger==null) return;
      try
      {
        messenger.CloseConnection();
      }
      catch(Exception)
      {
      }
    }

    // Called when the button 'Connected' is clicked
    private void StartMSN()
    {
      ReFillContactList=true;
      string emailadres="";
      string password="";	
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        emailadres = xmlreader.GetValueAsString("MSNmessenger","email","");
        password = xmlreader.GetValueAsString("MSNmessenger","password","");
      }
      try
      {				
        // make sure we don't use the default settings, since they're invalid
        if(emailadres == "")
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,GUIWindowManager.ActiveWindow,0,0,0,0,null);
          msg.Param1=905;
          msg.Param2=906;
          msg.Param2=-1;
          GUIWindowManager.SendMessage(msg);
          return;
        }
        else
        {
          messenger = new Messenger();
          // setup the callbacks
          // we log when someone goes online
          messenger.ContactOnline += new Messenger.ContactOnlineHandler(ContactOnline);
          messenger.ContactOffline+=new DotMSN.Messenger.ContactOfflineHandler(ContactOffline);
          messenger.ContactStatusChange +=new DotMSN.Messenger.ContactStatusChangeHandler(ContactStatusChange);

          // we want to do something when we have a conversation
          messenger.ConversationCreated += new Messenger.ConversationCreatedHandler(ConversationCreated);

          // notify us when synchronization is completed
          messenger.SynchronizationCompleted += new Messenger.SynchronizationCompletedHandler(OnSynchronizationCompleted);

          // everything is setup, now connect to the messenger service
          messenger.Connect(emailadres, password);					
          

          // synchronize the whole list.
          // remember you can only do this once per session!
          // after synchronizing the initial status will be set.
          messenger.SynchronizeList();
					

          /* uncomment this when you want to automatically add
           * people who have added you to their contactlist on your own
           * contactlist. (remember the pop-up dialog in MSN Messenger client when someone adds you, this is the 'automatic' method)					 
          foreach(Contact contact in
            messenger.GetListEnumerator(MSNList.ReverseList))
          {						
            messenger.AddContact(contact.Mail);
          }
          */
        }
      }
      catch(MSNException )
      {
        // in case of an error, report this to the user (or developer)
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(2002);
        pDlgOK.SetHeading(GUILocalizeStrings.Get(901));
        pDlgOK.SetLine(1,GUILocalizeStrings.Get(907));
        pDlgOK.SetLine(2,"");
        pDlgOK.DoModal( GUIWindowManager.ActiveWindow);
        return;
      }			
    }

    /// <summary>
    /// Log when the connection is actually established between the two clients.
    /// You can not yet send messages, the other contact must join first (if you have initiated the conversation)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConnectionEstablished(Conversation sender, EventArgs e)
    {
      //Log.Write += "connection established.\r\n";
    }

    // this is actually just annoying but it proves the concept
    private void ContactTyping(Conversation sender, ContactEventArgs e)
    {
      dateLastTyped=DateTime.Now;
      contactname=e.Contact.Name;
      //MessageBox.Show(this, e.Contact.Name + " is typing");
    }

    // log the event when a contact goed online
    private void ContactOnline(Messenger sender, ContactEventArgs e)
    {
      ReFillContactList=true;		
    }

    /// <summary>
    /// When the MSN server responds we can setup a conversation (the other party agreed)
    /// the Messenger.ConversationCreated event is called so we can initialize the
    /// Conversation object.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConversationCreated(Messenger sender, ConversationEventArgs e)
    {
      // we request a conversation or were asked one. Now log this
      //Log.Write += "Conversation object created\r\n";

      // remember there are not yet users in the conversation (except ourselves)
      // they will join _after_ this event. We create another callback to handle this.
      // When user(s) have joined we can start sending messages.
      e.Conversation.ContactJoin += new Conversation.ContactJoinHandler(ContactJoined);			

      // log the event when the two clients are connected
      e.Conversation.ConnectionEstablished += new Conversation.ConnectionEstablishedHandler(ConnectionEstablished);

      // notify us when the other contact is typing something
      e.Conversation.UserTyping  += new Conversation.UserTypingHandler(ContactTyping);			

      // we want to be accept filetransfer invitations
      //e.Conversation.FileTransferHandler.InvitationReceived +=new DotMSN.FileTransferHandler.FileTransferInvitationHandler(FileTransferHandler_FileTransferInvitation);
    }

    /// <summary>
    /// Called when the synchronization is completed. When this happens
    /// we want to fill the listbox on the form.
    /// </summary>
    /// <param name="sender">The messenger object</param>
    /// <param name="e">Contains nothing important</param>
    private void OnSynchronizationCompleted(Messenger sender, EventArgs e)
    {
      ReFillContactList=true;
      messenger.SetStatus(MSNStatus.Online);
    }
    /// <summary>
    /// After the first contact has joined you can actually send messages to the
    /// other contact(s)!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContactJoined(Conversation sender, ContactEventArgs e)
    {
      // someone joined our conversation! remember that this also occurs when you are
      // only talking to 1 other person. Log this event.
      //Log.Write += e.Contact.Name + " joined the conversation.\r\n";

      
      currentconversation = sender;
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN_CHAT);
    }
		

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "MSN messenger plugin";
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
      strButtonText = GUILocalizeStrings.Get(901);
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

    private void ContactOffline(Messenger sender, ContactEventArgs e)
    {
      ReFillContactList=true;
    }

    private void ContactStatusChange(Messenger sender, ContactStatusChangeEventArgs e)
    {
      ReFillContactList=true;
    }
  }
}
