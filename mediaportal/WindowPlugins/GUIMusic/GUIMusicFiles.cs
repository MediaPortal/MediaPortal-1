using System;
using System.Collections;
using System.Net;
using System.Xml.Serialization;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicFiles : GUIWindow, IComparer, ISetupForm
  {
    [Serializable]
    public class MapSettings
    {
      protected int   _SortBy;
      protected int   _ViewAs;
      protected bool  _Stack;
      protected bool _SortAscending ;

      public MapSettings()
      {
        _SortBy=0;//name
        _ViewAs=0;//list
        _Stack=true;
        _SortAscending=true;
      }


      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy;}
        set { _SortBy=value;}
      }
      
      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs;}
        set { _ViewAs=value;}
      }
      
      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending;}
        set { _SortAscending=value;}
      }
    }


    enum Controls
    {
      CONTROL_BTNVIEWASICONS = 2, 
      CONTROL_BTNSORTBY = 3, 
      CONTROL_BTNSORTASC = 4, 
      CONTROL_BTNTYPE = 6, 
      CONTROL_BTNPLAYLISTS = 7, 
      CONTROL_BTNSCAN = 9, 
			
      CONTROL_VIEW = 50, 
      CONTROL_LABELFILES = 12, 
      CONTROL_EJECT = 13,
      CONTROL_SEARCH = 8

    };
    #region Base variabeles
    enum SortMethod
    {
      SORT_NAME = 0, 
      SORT_DATE = 1, 
      SORT_SIZE = 2, 
      SORT_TRACK = 3, 
      SORT_DURATION = 4, 
      SORT_TITLE = 5, 
      SORT_ARTIST = 6, 
      SORT_ALBUM = 7, 
      SORT_FILENAME = 8
    }

    enum View
    {
      VIEW_AS_LIST = 0, 
      VIEW_AS_ICONS = 1, 
      VIEW_AS_LARGEICONS = 2, 
      VIEW_AS_FILMSTRIP   =   3,
    }
    static public string AlbumThumbsFolder=@"thumbs\music\albums";
    static public string ArtistsThumbsFolder=@"thumbs\music\artists";
    static public string GenreThumbsFolder=@"thumbs\music\genre";
    MapSettings       _MapSettings = new MapSettings();
		
    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory = "";
    int               m_iItemSelected = -1;
    VirtualDirectory  m_directory = new VirtualDirectory();
    Database	        m_database = new Database();
    bool			        m_bScan = false;
    bool              m_bUseID3 = true;
    bool              m_bAutoShuffle = true;
    string            m_strDiscId="";    
    int               m_iSelectedAlbum=-1;
    static Freedb.CDInfoDetail m_musicCD = null;

    #endregion
    
    public GUIMusicFiles()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
      
      m_directory.AddDrives();
      m_directory.SetExtensions(Utils.AudioExtensions);
      //m_directory.AddExtension(".m3u");
      //m_directory.AddExtension(".pls");
      //m_directory.AddExtension(".b4s");
      // set the eventhandler for the virt keyboard
      LoadSettings();
    }
    ~GUIMusicFiles()
    {
    }

    
    static public Freedb.CDInfoDetail MusicCD
    {
      get { return m_musicCD; }
      set { m_musicCD = value; }
    }

    public override bool Init()
    {
      m_strDirectory = "";
      System.IO.Directory.CreateDirectory(@"thumbs\music");
      System.IO.Directory.CreateDirectory(@"thumbs\music\albums");
      System.IO.Directory.CreateDirectory(@"thumbs\music\artists");
      System.IO.Directory.CreateDirectory(@"thumbs\music\genre");
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mymusicsongs.xml");
      return bResult;
    }


    #region Serialisation
    void LoadSettings()
    {
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {

        m_bUseID3 = xmlreader.GetValueAsBool("musicfiles","showid3",true);
        m_bAutoShuffle = xmlreader.GetValueAsBool("musicfiles","autoshuffle",true);

        string strDefault = xmlreader.GetValueAsString("music", "default","");
        m_directory.Clear();
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}",i);
          string strSharePath = String.Format("sharepath{0}",i);
          string strPincode = String.Format("pincode{0}",i);;

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd  = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("music", strShareName, "");
          share.Path = xmlreader.GetValueAsString("music", strSharePath, "");
          share.Pincode = xmlreader.GetValueAsInt("music", strPincode, - 1);
          
          share.IsFtpShare= xmlreader.GetValueAsBool("music", shareType, false);
          share.FtpServer= xmlreader.GetValueAsString("music", shareServer,"");
          share.FtpLoginName= xmlreader.GetValueAsString("music", shareLogin,"");
          share.FtpPassword= xmlreader.GetValueAsString("music", sharePwd,"");
          share.FtpPort= xmlreader.GetValueAsInt("music", sharePort,21);
          share.FtpFolder= xmlreader.GetValueAsString("music", remoteFolder,"/");

          if (share.Name.Length > 0)
          { 
            if (strDefault == share.Name)
            {
              share.Default=true;
              if (m_strDirectory.Length==0) m_strDirectory = share.Path;
            }
            m_directory.Add(share);
          }
          else break;
        }
      }
    }

    void SaveSettings()
    {
      using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
      {
      }
    }
    #endregion

    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = GetItem(0);
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
        return;
      }

      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }

      if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        ShowContextMenu();
      }
      base.OnAction(action);
    }

		
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
          base.OnMessage(message);
          if (MusicState.StartWindow != GetID)
          {
            GUIWindowManager.ActivateWindow(MusicState.StartWindow);
            return false;
          }
          m_database.Open();
          LoadSettings();
          LoadFolderSettings(m_strDirectory);

          ShowThumbPanel();
          LoadDirectory(m_strDirectory);
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
          m_iItemSelected = GetSelectedItemNo();
          SaveSettings();
          SaveFolderSettings(m_strDirectory);
          m_database.Close();
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED : 
          int iControl = message.SenderControlId;
          if (iControl == (int)Controls.CONTROL_BTNVIEWASICONS)
          {
            switch ((View)_MapSettings.ViewAs)
            {
              case View.VIEW_AS_LIST : 
                _MapSettings.ViewAs = (int)View.VIEW_AS_ICONS;
                break;
              case View.VIEW_AS_ICONS : 
                _MapSettings.ViewAs = (int)View.VIEW_AS_LARGEICONS;
                break;
              case View.VIEW_AS_LARGEICONS : 
                _MapSettings.ViewAs = (int)View.VIEW_AS_FILMSTRIP;
                break;
              case View.VIEW_AS_FILMSTRIP : 
                _MapSettings.ViewAs = (int)View.VIEW_AS_LIST;
                break;
            }
            ShowThumbPanel();
            GUIControl.FocusControl(GetID, iControl);
          }
          
          if (iControl == (int)Controls.CONTROL_BTNSORTASC)
          {
            _MapSettings.SortAscending= !_MapSettings.SortAscending;
            OnSort();
            UpdateButtons();
            GUIControl.FocusControl(GetID, iControl);
          }


          if (iControl == (int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            switch ((SortMethod)_MapSettings.SortBy)
            {
              case SortMethod.SORT_NAME : 
                _MapSettings.SortBy = (int)SortMethod.SORT_DATE;
                break;
              case SortMethod.SORT_DATE : 
                _MapSettings.SortBy = (int)SortMethod.SORT_SIZE;
                break;
              case SortMethod.SORT_SIZE : 
                _MapSettings.SortBy = (int)SortMethod.SORT_TRACK;
                break;
              case SortMethod.SORT_TRACK : 
                _MapSettings.SortBy = (int)SortMethod.SORT_DURATION;
                break;
              case SortMethod.SORT_DURATION : 
                _MapSettings.SortBy = (int)SortMethod.SORT_TITLE;
                break;
              case SortMethod.SORT_TITLE : 
                _MapSettings.SortBy = (int)SortMethod.SORT_ARTIST;
                break;
              case SortMethod.SORT_ARTIST : 
                _MapSettings.SortBy = (int)SortMethod.SORT_ALBUM;
                break;
              case SortMethod.SORT_ALBUM : 
                _MapSettings.SortBy = (int)SortMethod.SORT_FILENAME;
                break;
              case SortMethod.SORT_FILENAME : 
                _MapSettings.SortBy = (int)SortMethod.SORT_NAME;
                break;
            }
            OnSort();
            GUIControl.FocusControl(GetID, iControl);
          }

          if (iControl == (int)Controls.CONTROL_BTNTYPE)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int nSelected = (int)msg.Param1;
            int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
            switch (nSelected)
            {
              case 0 : //	files
                nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
                break;
              case 1 : //	albums
                nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_ALBUM;
                break;
              case 2 : //	artist
                nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_ARTIST;
                break;
              case 3 : //	genres
                nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
                break;
              case 4 : //	top100
                nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
                break;
            }

            if (nNewWindow != GetID)
            {
              MusicState.StartWindow = nNewWindow;
              GUIWindowManager.ActivateWindow(nNewWindow);
            }

            return true;
          }
          if (iControl == (int)Controls.CONTROL_VIEW)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int iItem = (int)msg.Param1;
            int iAction = (int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SHOW_INFO) 
            {
              
              OnInfo(iItem);
              LoadDirectory(m_strDirectory);
            }
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
            if (iAction == (int)Action.ActionType.ACTION_QUEUE_ITEM)
            {
              OnQueueItem(iItem);
            }
          }
          if (iControl == (int)Controls.CONTROL_BTNSCAN)
          {
            OnScan();
          }
					
          // search-button handling
          if (iControl == (int)Controls.CONTROL_SEARCH)
          {
            int activeWindow=(int)GUIWindowManager.ActiveWindow;
            VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
            keyBoard.Text = "";
            keyBoard.Reset();
            keyBoard.TextChanged+=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged); // add the event handler
            keyBoard.DoModal(activeWindow); // show it...
            keyBoard.TextChanged-=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged);	// remove the handler			
            System.GC.Collect(); // collect some garbage
          }
          //
          
          if (iControl == (int)Controls.CONTROL_EJECT)
          {
            Utils.EjectCDROM();
          }
          if (iControl == (int)Controls.CONTROL_BTNPLAYLISTS)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD:
          // start playing current CD
          string strDriveLetter=message.Label;
          
          PlayList list=PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP);
          list.Clear();

          list=PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
          list.Clear();

          GUIListItem pItem=new GUIListItem();
          pItem.Path=strDriveLetter;
          pItem.IsFolder=true;
          AddItemToPlayList(pItem) ;
          if (PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Count > 0 &&  !g_Player.Playing)
          {
            PlayListPlayer.Reset();
            PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
            PlayListPlayer.Play(0);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
          GUIMusicFiles.MusicCD=null;
          if (g_Player.Playing && Utils.IsCDDA(g_Player.CurrentFile))
          {
            g_Player.Stop();
          }
          if (GUIWindowManager.ActiveWindow==GetID)
          {
            if (Utils.IsDVD(m_strDirectory))
            {
              m_strDirectory="";
              LoadDirectory(m_strDirectory);
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
          pControl.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          GUIFacadeControl pControl2=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
          pControl2.OnMessage(message);
          break;
      }
      return base.OnMessage(message);
    }

    void ShowContextMenu()
    {
      GUIListItem item=GetSelectedItem();
      int itemNo=GetSelectedItemNo();
      if (item==null) return;

      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg==null) return;
      dlg.Reset();
      dlg.SetHeading(924); // menu

      dlg.Add( GUILocalizeStrings.Get(928)); //find coverart
      dlg.Add( GUILocalizeStrings.Get(208)); //play
      dlg.Add( GUILocalizeStrings.Get(926)); //Queue
			dlg.Add( GUILocalizeStrings.Get(136)); //PlayList
			dlg.Add( GUILocalizeStrings.Get(929)); //close window

      dlg.DoModal( GetID);
      if (dlg.SelectedLabel==-1) return;
      switch (dlg.SelectedLabel)
      {
        case 0: // find coverart
          OnInfo(itemNo);
          break;

        case 1: // play
          OnClick(itemNo);	
          break;
					
        case 2: // add to playlist
          OnQueueItem(itemNo);	
          break;
					
        case 3: // show playlist
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
          break;
      }
    }

    bool ViewByIcon
    {
      get 
      {
        if (_MapSettings.ViewAs != (int)View.VIEW_AS_LIST) return true;
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (_MapSettings.ViewAs == (int)View.VIEW_AS_LARGEICONS) return true;
        return false;
      }
    }

    GUIListItem GetSelectedItem()
    {
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,(int)Controls.CONTROL_VIEW);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      GUIListItem item = GUIControl.GetListItem(GetID,(int)Controls.CONTROL_VIEW,iItem);
      return item;
    }

    int GetSelectedItemNo()
    {

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.CONTROL_VIEW,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      return GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_VIEW);
    }

    void UpdateButtons()
    {
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_BTNTYPE, MusicState.StartWindow - (int)GUIWindow.Window.WINDOW_MUSIC_FILES);

      string strLine = "";
      View view = (View)_MapSettings.ViewAs;
      switch (view)
      {
        case View.VIEW_AS_LIST : 
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.VIEW_AS_ICONS : 
          strLine = GUILocalizeStrings.Get(100);
          break;
        case View.VIEW_AS_LARGEICONS : 
          strLine = GUILocalizeStrings.Get(417);
          break;
        case View.VIEW_AS_FILMSTRIP:
          strLine=GUILocalizeStrings.Get(733);
          break;
      }
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEWASICONS, strLine);

      SortMethod sortmethod = (SortMethod)_MapSettings.SortBy;
      switch (sortmethod)
      {
        case SortMethod.SORT_NAME : 
          strLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.SORT_DATE : 
          strLine = GUILocalizeStrings.Get(104);
          break;
        case SortMethod.SORT_SIZE : 
          strLine = GUILocalizeStrings.Get(105);
          break;
        case SortMethod.SORT_TRACK : 
          strLine = GUILocalizeStrings.Get(266);
          break;
        case SortMethod.SORT_DURATION : 
          strLine = GUILocalizeStrings.Get(267);
          break;
        case SortMethod.SORT_TITLE : 
          strLine = GUILocalizeStrings.Get(268);
          break;
        case SortMethod.SORT_ARTIST : 
          strLine = GUILocalizeStrings.Get(269);
          break;
        case SortMethod.SORT_ALBUM : 
          strLine = GUILocalizeStrings.Get(270);
          break;
        case SortMethod.SORT_FILENAME : 
          strLine = GUILocalizeStrings.Get(363);
          break;
      }
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSORTBY, strLine);

      bool bAsc = _MapSettings.SortAscending;
      if (bAsc)
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
    }

    void ShowThumbPanel()
    {
      int iItem = GetSelectedItemNo();
      GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
      if ( _MapSettings.ViewAs== (int)View.VIEW_AS_LARGEICONS )
      {
        pControl.View=GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_ICONS)
      {
        pControl.View=GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_LIST)
      {
        pControl.View=GUIFacadeControl.ViewMode.List;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_FILMSTRIP)
      {
        pControl.View=GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iItem);
      }
      UpdateButtons();
    }

    void DisplayFilesList(int searchKind,string strSearchText)
    {
			
      string strObjects = "";
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_VIEW);
      ArrayList itemlist = new ArrayList();
      m_database.GetSongs(searchKind,strSearchText,ref itemlist);
      // this will set all to move up
      // from a search result
      m_history.Set(m_strDirectory, m_strDirectory); //save where we are
      GUIListItem dirUp=new GUIListItem("..");
      dirUp.Path=m_strDirectory; // to get where we are
      dirUp.IsFolder=true;
      dirUp.ThumbnailImage="";
      dirUp.IconImage="defaultFolderBack.png";
      dirUp.IconImageBig="defaultFolderBackBig.png";
      itemlist.Insert(0,dirUp);
      //
      OnRetrieveMusicInfo(ref itemlist);
      foreach (GUIListItem item in itemlist)
      {
        item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_VIEW, item);
      }
      OnSort();
      int iTotalItems = itemlist.Count;
      if (itemlist.Count > 0)
      {
        GUIListItem rootItem = (GUIListItem)itemlist[0];
        if (rootItem.Label == "..") iTotalItems--;
      }

      strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects);
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
      ShowThumbPanel();
      
    }
    void LoadFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      object o;
      FolderSettings.GetFolderSetting(strDirectory,"MusicFiles",typeof(GUIMusicFiles.MapSettings), out o);
      if (o!=null) _MapSettings = o as MapSettings;
      if (_MapSettings==null) _MapSettings  = new MapSettings();
    }
    void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      FolderSettings.AddFolderSetting(strDirectory,"MusicFiles",typeof(GUIMusicFiles.MapSettings), _MapSettings);
    }

    void OnRetrieveCoverArt(GUIListItem item)
    {
      Utils.SetDefaultIcons(item);
      MusicTag tag = item.MusicTag as MusicTag;
      string thumb=GUIMusicFiles.GetCoverArt(item.IsFolder, item.Path,  tag);
      if (thumb!=String.Empty)
      {
        item.ThumbnailImage = thumb;
        item.IconImageBig = thumb;
        item.IconImage = thumb;
      }
    }

    void LoadDirectory(string strNewDirectory)
    {
      GUIListItem SelectedItem = GetSelectedItem();
      if (SelectedItem != null) 
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          m_history.Set(SelectedItem.Label, m_strDirectory);
        }
      }
      if (strNewDirectory != m_strDirectory && _MapSettings!=null) 
      {
        SaveFolderSettings(m_strDirectory);
      }

      if (strNewDirectory != m_strDirectory || _MapSettings==null) 
      {
        LoadFolderSettings(strNewDirectory);
      }
      m_strDirectory = strNewDirectory;
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_VIEW);
            
      string strObjects = "";

      ArrayList itemlist = m_directory.GetDirectory(m_strDirectory);
      
      string strSelectedItem = m_history.Get(m_strDirectory);
      int iItem = 0;
      OnRetrieveMusicInfo(ref itemlist);
      foreach (GUIListItem item in itemlist)
      {
        item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
              
        item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_VIEW, item);
      }
      OnSort();
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.Label == strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iItem);
          break;
        }
        iItem++;
      }
      int iTotalItems = itemlist.Count;
      if (itemlist.Count > 0)
      {
        GUIListItem rootItem = (GUIListItem)itemlist[0];
        if (rootItem.Label == "..") iTotalItems--;
      }
      strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects);
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
      ShowThumbPanel();
      
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, m_iItemSelected);
      }
    }
    #endregion

    void SetLabels()
    {
      SortMethod method = (SortMethod)_MapSettings.SortBy;
      bool bAscending = _MapSettings.SortAscending;
      

      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        MusicTag tag = (MusicTag)item.MusicTag;
        if (tag != null)
        {
          if (tag.Title.Length > 0)
          {
            if (tag.Artist.Length > 0)
            {
              if (tag.Track > 0)
                item.Label = String.Format("{0:00}. {1} - {2}",tag.Track, tag.Artist, tag.Title);
              else
                item.Label = String.Format("{0} - {1}",tag.Artist, tag.Title);
            }
            else
            {
              if (tag.Track > 0)
                item.Label = String.Format("{0:00}. {1} ",tag.Track, tag.Title);
              else
                item.Label = String.Format("{0}",tag.Title);
            }
            if (method == SortMethod.SORT_ALBUM)
            {
              if (tag.Album.Length > 0 && tag.Title.Length > 0)
              {
                item.Label = String.Format("{0} - {1}", tag.Album, tag.Title);
              }
            }
          }
        }
        
        
        if (method == SortMethod.SORT_SIZE || method == SortMethod.SORT_FILENAME)
        {
          if (item.IsFolder) item.Label2 = "";
          else
          {
            if (item.Size > 0)
            {
              item.Label2 = Utils.GetSize(item.Size);
            }
          
            if (method == SortMethod.SORT_FILENAME)
            {
              if (item.Path.EndsWith(".cda"))  // do some triming since the name is long due to the extra info from freedb
              {
                int index = item.Path.IndexOf("Track");
                item.Label = item.Path.Substring(index);
              }
              else
                item.Label = Utils.GetFilename(item.Path);
            }
          }
        }
        else if (method == SortMethod.SORT_DATE)
        {
          if (item.FileInfo != null)
          {
            item.Label2 = item.FileInfo.CreationTime.ToShortDateString() + " " + item.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          }
        }
        else
        {
          item.Label2 = "";
          if (tag != null)
          {
            int nDuration = tag.Duration;
            if (nDuration > 0)
            {
              item.Label2 = Utils.SecondsToHMSString(nDuration);
            }
          }
        }
      }
    }


    #region Sort Members
    void OnSort()
    {
      SetLabels();
      GUIFacadeControl list= (GUIFacadeControl )GetControl((int)Controls.CONTROL_VIEW);
      list.Sort(this);

      UpdateButtons();
    }

    public int Compare(object x, object y)
    {
      if (x == y) return 0;
      GUIListItem item1 = (GUIListItem)x;
      GUIListItem item2 = (GUIListItem)y;
      if (item1 == null) return - 1;
      if (item2 == null) return - 1;
      if (item1.IsFolder && item1.Label == "..") return - 1;
      if (item2.IsFolder && item2.Label == "..") return - 1;
      if (item1.IsFolder && !item2.IsFolder) return - 1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;

      string strSize1 = "";
      string strSize2 = "";
      if (item1.FileInfo != null) strSize1 = Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo != null) strSize2 = Utils.GetSize(item2.FileInfo.Length);

      SortMethod method = (SortMethod)_MapSettings.SortBy;
      bool bAscending = _MapSettings.SortAscending;
      switch (method)
      {
        case SortMethod.SORT_NAME : 
          if (bAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }
        

        case SortMethod.SORT_DATE : 
          if (item1.FileInfo == null) return - 1;
          if (item2.FileInfo == null) return - 1;
          
          item1.Label2 = item1.FileInfo.CreationTime.ToShortDateString() + " " + item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.CreationTime.ToShortDateString() + " " + item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          if (bAscending)
          {
            return DateTime.Compare(item1.FileInfo.CreationTime, item2.FileInfo.CreationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.CreationTime, item1.FileInfo.CreationTime);
          }

        case SortMethod.SORT_SIZE : 
          if (item1.FileInfo == null) return - 1;
          if (item2.FileInfo == null) return - 1;
          if (bAscending)
          {
            return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
          }
          else
          {
            return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
          }

        case SortMethod.SORT_TRACK : 
          int iTrack1 = 0;
          int iTrack2 = 0;
          if (item1.MusicTag != null) iTrack1 = ((MusicTag)item1.MusicTag).Track;
          if (item2.MusicTag != null) iTrack2 = ((MusicTag)item2.MusicTag).Track;
          if (bAscending)
          {
            return (int)(iTrack1 - iTrack2);
          }
          else
          {
            return (int)(iTrack2 - iTrack1);
          }
          
        case SortMethod.SORT_DURATION : 
          int iDuration1 = 0;
          int iDuration2 = 0;
          if (item1.MusicTag != null) iDuration1 = ((MusicTag)item1.MusicTag).Duration;
          if (item2.MusicTag != null) iDuration2 = ((MusicTag)item2.MusicTag).Duration;
          if (bAscending)
          {
            return (int)(iDuration1 - iDuration2);
          }
          else
          {
            return (int)(iDuration2 - iDuration1);
          }
          
        case SortMethod.SORT_TITLE : 
          string strTitle1 = item1.Label;
          string strTitle2 = item2.Label;
          if (item1.MusicTag != null) strTitle1 = ((MusicTag)item1.MusicTag).Title;
          if (item2.MusicTag != null) strTitle2 = ((MusicTag)item2.MusicTag).Title;
          if (bAscending)
          {
            return String.Compare(strTitle1, strTitle2, true);
          }
          else
          {
            return String.Compare(strTitle2, strTitle1, true);
          }
        
        case SortMethod.SORT_ARTIST : 
          string strArtist1 = "";
          string strArtist2 = "";
          if (item1.MusicTag != null) strArtist1 = ((MusicTag)item1.MusicTag).Artist;
          if (item2.MusicTag != null) strArtist2 = ((MusicTag)item2.MusicTag).Artist;
          if (bAscending)
          {
            return String.Compare(strArtist1, strArtist2, true);
          }
          else
          {
            return String.Compare(strArtist2, strArtist1, true);
          }
        
        case SortMethod.SORT_ALBUM : 
          string strAlbum1 = "";
          string strAlbum2 = "";
          if (item1.MusicTag != null) strAlbum1 = ((MusicTag)item1.MusicTag).Album;
          if (item2.MusicTag != null) strAlbum2 = ((MusicTag)item2.MusicTag).Album;
          if (bAscending)
          {
            return String.Compare(strAlbum1, strAlbum2, true);
          }
          else
          {
            return String.Compare(strAlbum2, strAlbum1, true);
          }
          

        case SortMethod.SORT_FILENAME : 
          string strFile1 = System.IO.Path.GetFileName(item1.Path);
          string strFile2 = System.IO.Path.GetFileName(item2.Path);
          if (bAscending)
          {
            return String.Compare(strFile1, strFile2, true);
          }
          else
          {
            return String.Compare(strFile2, strFile1, true);
          }
          
      } 
      return 0;
    }
    #endregion

		
    void OnClick(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item == null) return;
      if (item.IsFolder)
      {
        m_iItemSelected = -1;
        LoadDirectory(item.Path);
      }
      else
      {
        if (m_directory.IsRemote(item.Path) )
        {
          if (!m_directory.IsRemoteFileDownloaded(item.Path,item.FileInfo.Length) )
          {
            if (!m_directory.ShouldWeDownloadFile(item.Path)) return;
            if (!m_directory.DownloadRemoteFile(item.Path,item.FileInfo.Length))
            {
              //show message that we are unable to download the file
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,0,0,0,0,0,0);
              msg.Param1=916;
              msg.Param2=920;
              msg.Param3=0;
              msg.Param4=0;
              GUIWindowManager.SendMessage(msg);

              return;
            }
          }
          return;
        }

        if (PlayListFactory.IsPlayList(item.Path))
        {
          LoadPlayList(item.Path);
          return;
        }
        //play and add current directory to temporary playlist
        int nFolderCount = 0;
        int nRemoteCount =0;
        PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
        PlayListPlayer.Reset();
        for (int i = 0; i < (int) GetItemCount(); i++) 
        {
          GUIListItem pItem = GetItem(i);
          if (pItem.IsFolder) 
          {
            nFolderCount++;
            continue;
          }
          if (pItem.IsRemote)
          {
            nRemoteCount++;
            continue;
          }
          if (!PlayListFactory.IsPlayList(pItem.Path))
          {
            PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
            playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
            playlistItem.FileName = pItem.Path;
            playlistItem.Description = pItem.Label;
            playlistItem.Duration = pItem.Duration;
            PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
          }
          else
          {

            if (i < GetSelectedItemNo()) nFolderCount++;
            continue;
          }
        }

        //	Save current window and directory to know where the selected item was
        MusicState.TempPlaylistWindow = GetID;
        MusicState.TempPlaylistDirectory = m_strDirectory;

        PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
        PlayListPlayer.Play(item.Path);
      }
    }
    
    void OnQueueItem(int iItem)
    {
      // add item 2 playlist
      GUIListItem pItem = GetItem(iItem);
      if (pItem==null) return;
      if (pItem.IsRemote) return;
      if (PlayListFactory.IsPlayList(pItem.Path))
      {
        LoadPlayList(pItem.Path);
        return;
      }
      AddItemToPlayList(pItem);
	
      //move to next item
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iItem + 1);
      if (PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Count > 0 &&  !g_Player.Playing)
      {
        PlayListPlayer.Reset();
        PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Play(0);
      }

    }

    void AddItemToPlayList(GUIListItem pItem) 
    {
      if (pItem.IsFolder)
      {
        // recursive
        if (pItem.Label == "..") return;
        string strDirectory = m_strDirectory;
        m_strDirectory = pItem.Path;
		    
        ArrayList itemlist = m_directory.GetDirectory(m_strDirectory);
        OnRetrieveMusicInfo(ref itemlist);
        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item);
        }
        m_strDirectory = strDirectory;
      }
      else
      {
        //TODO
        if (Utils.IsAudio(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
        {
          PlayList.PlayListItem playlistItem = new PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName = pItem.Path;
          playlistItem.Description = pItem.Label;
          playlistItem.Duration = pItem.Duration;
          PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Add(playlistItem);
        }
      }
    }

    void LoadPlayList(string strPlayList)
    {
      PlayList playlist = PlayListFactory.Create(strPlayList);
      if (playlist == null) return;
      if (!playlist.Load(strPlayList))
      {
        GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dlgOK != null)
        {
          dlgOK.SetHeading(6);
          //dlgOK.SetLine(0, "");
          dlgOK.SetLine(1, 477);
          dlgOK.SetLine(2, "");
          dlgOK.DoModal(GetID);
        }
        return;
      }

      if (playlist.Count == 1)
      {
        g_Player.Play(playlist[0].FileName);
        return;
      }

      // clear current playlist
      PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Clear();

      if (m_bAutoShuffle)
      {
        playlist.Shuffle();
      }
      // add each item of the playlist to the playlistplayer
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayList.PlayListItem playListItem = playlist[i];
        PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Add(playListItem);
      }

			
      // if we got a playlist
      if (PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Count > 0)
      {
        // then get 1st song
        playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
        PlayList.PlayListItem item = playlist[0];

        // and start playing it
        PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Reset();
        PlayListPlayer.Play(0);

        // and activate the playlist window if its not activated yet
        if (GetID == GUIWindowManager.ActiveWindow)
        {
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        }
      }
    }

    void OnRetrieveMusicInfo(ref ArrayList items)
    {
      int nFolderCount = 0;
      foreach (GUIListItem item in items)
      {
        if (item.IsFolder) nFolderCount++;
      }

      // Skip items with folders only
      if (nFolderCount == (int)items.Count)
        return;

      GUIDialogProgress dlg = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

      if (m_strDirectory.Length == 0) return;
      //string strItem;
      ArrayList songsMap = new ArrayList();
      // get all information for all files in current directory from database 
      m_database.GetSongsByPath2(m_strDirectory, ref songsMap);

      //int_20h. musicCD is the information about the cd...
      //GUIMusicFiles.MusicCD = null;

      bool bCDDAFailed=false;
      // for every file found, but skip folder
      for (int i = 0; i < (int)items.Count; ++i)
      {
        GUIListItem pItem = (GUIListItem)items[i];
        if (pItem.IsRemote) continue;
        if (pItem.IsFolder) continue;
        if (pItem.Path.Length == 0) continue;
        string strFilePath = System.IO.Path.GetFullPath(pItem.Path);
        strFilePath = strFilePath.Substring(0, strFilePath.Length - (1 + System.IO.Path.GetFileName(pItem.Path).Length));
        if (strFilePath != m_strDirectory)
        {
          return;
        }
        string strExtension = System.IO.Path.GetExtension(pItem.Path);
        if (m_bScan  && strExtension.ToLower().Equals(".cda")) continue;
        if (m_bScan && dlg != null)
          dlg.ProgressKeys();

        // dont try reading id3tags for folders or playlists
        if (!pItem.IsFolder && !PlayListFactory.IsPlayList(pItem.Path))
        {
          // is tag for this file already loaded?
          bool bNewFile = false;
          MusicTag tag = (MusicTag)pItem.MusicTag;
          if (tag == null)
          {
            // no, then we gonna load it. But dont load tags from cdda files
            if (strExtension != ".cda")  // int_20h: changed cdda to cda.
            {
              // first search for file in our list of the current directory
              Song song = new Song();
              bool bFound = false;
              foreach (SongMap song1 in songsMap)
              {
                if (song1.m_strPath == pItem.Path)
                {
                  song = song1.m_song;
                  bFound = true;
                  tag = new MusicTag();
                  pItem.MusicTag = tag;
                  break;
                }
              }
							
              if (!bFound && !m_bScan)
              {
                // try finding it in the database
                string strPathName;
                string strFileName;
                m_database.Split(pItem.Path, out strPathName, out strFileName);
                if (strPathName != m_strDirectory)
                {
                  if (m_database.GetSongByFileName(pItem.Path, ref song))
                  {
                    bFound = true;
                  }
                }
              }

              if (!bFound)
              {
                // if id3 tag scanning is turned on OR we're scanning the directory
                // then parse id3tag from file
                if (m_bUseID3 || m_bScan)
                {
                  // get correct tag parser
                  tag = TagReader.TagReader.ReadTag(pItem.Path);
                  if (tag != null)
                  {
                    pItem.MusicTag = tag;
                    bNewFile = true;
                  }
                }
              }
              else // of if ( !bFound )
              {
                tag.Album = song.Album;
                tag.Artist = song.Artist;
                tag.Genre = song.Genre;
                tag.Duration = song.Duration;
                tag.Title = song.Title;
                tag.Track = song.Track;
              }
            }//if (strExtension!=".cda" )
            else // int_20h: if it is .cda then get info from freedb
            {
              if (m_bScan)
                continue;

              if (bCDDAFailed) continue;
              if (!Util.Win32API.IsConnectedToInternet()) continue;

              try
              {
                // check internet connectivity
                GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                if (null != pDlgOK && !Util.Win32API.IsConnectedToInternet())
                {
                  pDlgOK.SetHeading(703);
                  //pDlgOK.SetLine(0, "");
                  pDlgOK.SetLine(1, 703);
                  pDlgOK.SetLine(2, "");
                  pDlgOK.DoModal(GetID);
                  throw new Exception("no internet");
                }
                else if (!Util.Win32API.IsConnectedToInternet())
                {
                  throw new Exception("no internet");
                }

                Freedb.FreeDBHttpImpl freedb = new Freedb.FreeDBHttpImpl();
                char driveLetter = System.IO.Path.GetFullPath(pItem.Path).ToCharArray()[0];
                // try finding it in the database
                string strPathName, strCDROMPath;
                //int_20h fake the path with the cdInfo
                strPathName = driveLetter + ":/" + freedb.GetCDDBDiscIDInfo(driveLetter, '+');
                strCDROMPath = strPathName + "+" + System.IO.Path.GetFileName(pItem.Path);

                Song song = new Song();
                bool bFound = false;
                if (m_database.GetSongByFileName(strCDROMPath, ref song))
                {
                  bFound = true;
                }

                if (!bFound && GUIMusicFiles.MusicCD == null)
                {
                  try
                  {
                    freedb.Connect(); // should be replaced with the Connect that receives a http freedb site...
                    Freedb.CDInfo[] cds = freedb.GetDiscInfo(driveLetter);
                    if (cds!=null)
                    {
                      if (cds.Length == 1)
                      {
                        GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[0].Category, cds[0].DiscId);
                        m_strDiscId=cds[0].DiscId;
                      }
                      else if (cds.Length > 1)
                      {
                        if (m_strDiscId==cds[0].DiscId)
                        {
                            GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[m_iSelectedAlbum].Category, cds[m_iSelectedAlbum].DiscId);
                        }
                        else
                        {
                          m_strDiscId=cds[0].DiscId;
                          //show dialog with all albums found
                          string szText = GUILocalizeStrings.Get(181);
                          GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
                          if (null != pDlg)
                          {
                            pDlg.Reset();
                            pDlg.SetHeading(szText);
                            for (int j = 0; j < cds.Length; j++)
                            {
                              Freedb.CDInfo info = cds[j];
                              pDlg.Add(info.Title);
                            }
                            pDlg.DoModal(GetID);

                            // and wait till user selects one
                            m_iSelectedAlbum = pDlg.SelectedLabel;
                            if (m_iSelectedAlbum < 0) return;
                            GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[m_iSelectedAlbum].Category, cds[m_iSelectedAlbum].DiscId);
                          }
                        }
                      }
                    }
                    freedb.Disconnect();
                    if (GUIMusicFiles.MusicCD==null) bCDDAFailed=true;
                  }
                  catch(Exception)
                  {
                    GUIMusicFiles.MusicCD=null;
                    bCDDAFailed=true;
                  }

                }

                if (!bFound && GUIMusicFiles.MusicCD != null) // if musicCD was configured correctly...
                {
                  int trackno=GetCDATrackNumber(pItem.Path);
                  Freedb.CDTrackDetail track = GUIMusicFiles.MusicCD.getTrack(trackno);

                  tag = new MusicTag();
                  tag.Album = GUIMusicFiles.MusicCD.Title;
                  tag.Artist = track.Artist == null ? GUIMusicFiles.MusicCD.Artist : track.Artist;
                  tag.Genre = GUIMusicFiles.MusicCD.Genre;
                  tag.Duration = track.Duration;
                  tag.Title = track.Title;
                  tag.Track = track.TrackNumber;
                  pItem.MusicTag = tag;
                  bNewFile = true;
                  pItem.Label = pItem.Path; // 
                  pItem.Path = strCDROMPath; // to be stored in the database
                }
                
                else if (bFound)
                {
                  tag = new MusicTag();
                  tag.Album = song.Album;
                  tag.Artist = song.Artist;
                  tag.Genre = song.Genre;
                  tag.Duration = song.Duration;
                  tag.Title = song.Title;
                  tag.Track = song.Track;
                  pItem.MusicTag = tag;
                }

              }// end of try
              catch (Exception e)
              {
                // log the problem...
                Log.Write("OnRetrieveMusicInfo: {0}",e.ToString());
              }
            }
          }//if (!tag.Loaded() )
          else if (m_bScan)
          {
            bNewFile = true;
            foreach (SongMap song1 in songsMap)
            {
              if (song1.m_strPath == pItem.Path)
              {
                bNewFile = false;
              }
            }
          }

          if (tag != null && m_bScan && bNewFile)
          {
            Song song = new Song();
            song.Title = tag.Title;
            song.Genre = tag.Genre;
            song.FileName = pItem.Path;
            song.Artist = tag.Artist;
            song.Album = tag.Album;
            song.Year = tag.Year;
            song.Track = tag.Track;
            song.Duration = tag.Duration;

            m_database.AddSong(song, false);
          }
        }//if (!pItem.IsFolder)
      }
    }

    bool DoScan(string strDir, ref ArrayList items)
    {
      GUIDialogProgress dlg = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlg != null)
      {
        string strPath = System.IO.Path.GetFileName(strDir);
        dlg.SetLine(2, strPath);
        dlg.Progress();
      }

      OnRetrieveMusicInfo(ref items);
      m_database.CheckVariousArtistsAndCoverArt();
			
      if (dlg != null && dlg.IsCanceled) return false;
			
      bool bCancel = false;
      for (int i = 0; i < (int)items.Count; ++i)
      {
        GUIListItem pItem = (GUIListItem)items[i];
        if (pItem.IsRemote) continue;
        if (dlg != null && dlg.IsCanceled)
        {
          bCancel = true;
          break;
        }
        if (pItem.IsFolder)
        {
          if (pItem.Label != "..")
          {
            // load subfolder
            string strPrevDir = m_strDirectory;
            m_strDirectory = pItem.Path;
            ArrayList subDirItems = m_directory.GetDirectory(m_strDirectory);
            if (!DoScan(m_strDirectory, ref subDirItems))
            {
              bCancel = true;
            }
            m_strDirectory = strPrevDir;
            if (bCancel) break;
          }
        }
      }
			
      return!bCancel;
    }

    void OnScan()
    {
      GUIDialogProgress dlg = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIGraphicsContext.Overlay = false;

      m_bScan = true;
      ArrayList items = new ArrayList();
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem pItem=GetItem(i);
        if (!pItem.IsRemote) 
          items.Add(pItem);
      }
      if (null != dlg)
      {
        string strPath = System.IO.Path.GetFileName(m_strDirectory);
        dlg.SetHeading(189);
        dlg.SetLine(1, 330);
        //dlg.SetLine(1, "");
        dlg.SetLine(2, strPath);
        dlg.StartModal(GetID);
      }
      
      m_database.BeginTransaction();
      if (DoScan(m_strDirectory, ref items))
      {
        dlg.SetLine(1, 328);
        dlg.SetLine(2, "");
        dlg.SetLine(3, 330);
        dlg.Progress();
        m_database.CommitTransaction();
      }
      else
        m_database.RollbackTransaction();
      m_database.EmptyCache();
      dlg.Close();
      // disable scan mode
      m_bScan = false;
      GUIGraphicsContext.Overlay = OverlayAllowed;
		
      LoadDirectory(m_strDirectory);
    }

    void OnInfo(int iItem)
    {
      m_iItemSelected = GetSelectedItemNo();
      int iSelectedItem = GetSelectedItemNo();
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIListItem pItem = GetItem(iItem);

      if (pItem.IsRemote) return;
      string strPath = "";
      if (pItem.IsFolder)
      {
        strPath = pItem.Path;
      }
      else
      {
        string strFileName;
        m_database.Split(pItem.Path, out strPath, out strFileName);
      }

      //	Try to find an album name for this item.
      //	Only save to database, if album name is found there.
      bool bSaveDb = false;
      string strAlbumName = pItem.Label;
      MusicTag tag = (MusicTag)(pItem.MusicTag);
      if (tag != null)
      {
        if (tag.Album.Length > 0) 
        {
          strAlbumName = tag.Album;
          bSaveDb = true;
        }
        else if (tag.Title.Length > 0) 
        {
          strAlbumName = tag.Title;
          bSaveDb = true;
        }
      }

      MusicAlbumInfo infoTag = (MusicAlbumInfo)pItem.AlbumInfoTag;
      if (pItem.IsFolder)
      {

        //	if a folder has an album set,
        //	we can be sure that it is in database
        AlbumInfo album = new AlbumInfo();
        if (infoTag != null)
        {
          string strAlbum = infoTag.Title;
          if (strAlbum.Length > 0)
          {
            strAlbumName = strAlbum;
            bSaveDb = true;
          }
        }/* TODO
        else if (m_database.GetAlbumByPath(pItem.Path, ref album))
        {	
          //	Normal folder, query database for album name
          if (album.Album.Length>0)
          {
            strAlbumName=album.Album;
            bSaveDb=true;
          }
        }*/
        else
        {
          //	No album name found for folder. Look into
          //	the directory, but don't save to database
          ArrayList items = new ArrayList();
          items = m_directory.GetDirectory(pItem.Path);
          OnRetrieveMusicInfo(ref items);

          //	Get first album name found in directory
          foreach (GUIListItem newitem in items)
          {
            MusicAlbumInfo info = (MusicAlbumInfo)newitem.AlbumInfoTag;
            if (info != null)
            {
              if (info.Title.Length > 0)
              {
                strAlbumName = info.Title;
                break;
              }
            }
          }
        }
      }
      else if (infoTag != null)
      {
        //	Handle files
        AlbumInfo album = new AlbumInfo();
        string strAlbum = infoTag.Title;
        //	Is album in database?
        /* TODO
        if (m_database.GetAlbumByPath(strPath, ref album))
        {
          //	yes, save query results to database
          strAlbumName=album.Album;
          bSaveDb=true;
        }
        else
          //	no, don't save
          strAlbumName=strAlbum; */
      }


      // check cache
      AlbumInfo albuminfo = new AlbumInfo();
      if (m_database.GetAlbumInfo(strAlbumName, strPath, ref albuminfo))
      {
        ArrayList songs = new ArrayList();
        MusicAlbumInfo album = new MusicAlbumInfo();
        album.Set(albuminfo);

        GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
        if (null != pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album = album;
          pDlgAlbumInfo.Tag=tag;

          pDlgAlbumInfo.DoModal(GetID);
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(strAlbumName);
            OnInfo(iItem);
          }
          return;
          //GetStringFromKeyboard(ref strAlbumName);
        }
      }

      // show dialog box indicating we're searching the album
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(185);
        dlgProgress.SetLine(1, strAlbumName);
        //dlgProgress.SetLine(1, "");
        dlgProgress.SetLine(2, "");
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      bool bDisplayErr = false;
	
      // find album info
      MusicInfoScraper scraper = new MusicInfoScraper();
      if (scraper.FindAlbuminfo(strAlbumName))
      {
        if (dlgProgress != null) dlgProgress.Close();
        // did we found at least 1 album?
        int iAlbumCount = scraper.Count;
        if (iAlbumCount >= 1)
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum = 0;
          if (iAlbumCount > 1)
          {
            //show dialog with all albums found
            string szText = GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              for (int i = 0; i < iAlbumCount; ++i)
              {
                MusicAlbumInfo info = scraper[i];
                pDlg.Add(info.Title2);
              }
              pDlg.DoModal(GetID);

              // and wait till user selects one
              iSelectedAlbum = pDlg.SelectedLabel;
              if (iSelectedAlbum < 0) return;
            }
          }

          // ok, now show dialog we're downloading the album info
          MusicAlbumInfo album = scraper[iSelectedAlbum];
          if (null != dlgProgress) 
          {
            dlgProgress.SetHeading(185);
            dlgProgress.SetLine(1, album.Title2);
            dlgProgress.SetLine(2, "");
            dlgProgress.StartModal(GetID);
            dlgProgress.Progress();
          }

          // download the album info
          bool bLoaded = album.Loaded;
          if (!bLoaded) 
            bLoaded = album.Load();
          if (bLoaded)
          {
            // set album title from musicinfotag, not the one we got from allmusic.com
            album.Title = strAlbumName;
            // set path, needed to store album in database
            album.AlbumPath = strPath;

            if (bSaveDb)
            {
              albuminfo = new AlbumInfo();
              albuminfo.Album = album.Title;
              albuminfo.Artist = album.Artist;
              albuminfo.Genre = album.Genre;
              albuminfo.Tones = album.Tones;
              albuminfo.Styles = album.Styles;
              albuminfo.Review = album.Review;
              albuminfo.Image = album.ImageURL;
              albuminfo.Rating = album.Rating;
              albuminfo.Tracks = album.Tracks;
              try
              {
                albuminfo.Year = Int32.Parse(album.DateOfRelease);
              }
              catch (Exception)
              {
              }
              //albuminfo.Path   = album.AlbumPath;
              // save to database
              m_database.AddAlbumInfo(albuminfo);


            }
            if (null != dlgProgress) 
              dlgProgress.Close();

            // ok, show album info
            GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
            if (null != pDlgAlbumInfo)
            {
              pDlgAlbumInfo.Album = album;
              pDlgAlbumInfo.Tag=tag;

              pDlgAlbumInfo.DoModal(GetID);
              if (pDlgAlbumInfo.NeedsRefresh)
              {
                m_database.DeleteAlbumInfo(album.Title);
                OnInfo(iItem);
                return;
              }
              if (pItem.IsFolder)
              {
                string thumb=GetAlbumThumbName(album.Artist, album.Title);
                if (System.IO.File.Exists(thumb))
                {
                  string folderjpg=String.Format(@"{0}\folder.jpg",Utils.RemoveTrailingSlash(pItem.Path));
                  Utils.FileDelete(folderjpg);
                  System.IO.File.Copy(thumb, folderjpg);
                }
              }
            }
          }
          else
          {
            // failed 2 download album info
            bDisplayErr = true;
          }
        }
        else 
        {
          // no albums found
          bDisplayErr = true;
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr = true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null != dlgProgress) 
          dlgProgress.Close();
        if (null != pDlgOK)
        {
          pDlgOK.SetHeading(187);
          pDlgOK.SetLine(1, 187);
          pDlgOK.SetLine(2, "");
          pDlgOK.DoModal(GetID);
        }
      }
    }
    void keyboard_TextChanged(int kindOfSearch,string data)
    {
      DisplayFilesList(kindOfSearch,data);
    }
    void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return;
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    static public string GetCoverArt(bool isfolder, string filename,  MusicTag tag)
    {
      string strFolderThumb =String.Empty;
      if (isfolder)
      {
        strFolderThumb = String.Format(@"{0}\folder.jpg",Utils.RemoveTrailingSlash(filename) );
        if (System.IO.File.Exists(strFolderThumb))
        {
          return strFolderThumb;
        }
        return string.Empty;
      }

      string strAlbumName = String.Empty;
      string strArtistName = String.Empty;
      if (tag != null)
      {
        if (tag.Album.Length > 0) strAlbumName=tag.Album;
        if (tag.Artist.Length>0) strArtistName=tag.Artist;
      }

      // use covert art thumbnail for albums
      string strThumb = GUIMusicFiles.GetAlbumThumbName(strArtistName, strAlbumName);
      if (System.IO.File.Exists(strThumb))
      {
        return strThumb;
      }

      // no album art? then use folder.jpg
      string strPathName;
      string strFileName;
      Database	        m_database = new Database();
      m_database.Split(filename, out strPathName, out strFileName);
      strFolderThumb = strPathName + @"\folder.jpg";
      if (System.IO.File.Exists(strFolderThumb))
      {
        return strFolderThumb;
      }
      return string.Empty;
    }

    static public string GetAlbumThumbName(string ArtistName, string AlbumName)
    {
      if (ArtistName==String.Empty) return String.Empty;
      if (AlbumName==String.Empty) return String.Empty;
      string name=String.Format("{0}-{1}", ArtistName, AlbumName);
      return Utils.GetCoverArtName(GUIMusicFiles.AlbumThumbsFolder, name);
    }


		#region ISetupForm Members

		public bool DefaultEnabled()
		{
			return true;
		}
		public bool CanEnable()
		{
			return true;
		}
    
    public bool HasSetup()
    {
      return false;
    }

		public string PluginName()
		{
			return "My Music";
		}

		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = GUILocalizeStrings.Get(2);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string Description()
		{
			return "Plugin to play & organize your music";
		}

		public void ShowPlugin()
		{
		}

		#endregion


    int GetCDATrackNumber(string strFile)
    {
      string strTrack="";
      int pos=strFile.IndexOf(".cda");
      if (pos >=0)
      {
        pos--;
        while (Char.IsDigit(strFile[pos]) && pos>0) 
        {
          strTrack=strFile[pos]+strTrack;
          pos--;
        }
      }

      try
      {
        int iTrack = Convert.ToInt32(strTrack);
        return iTrack;
      }
      catch(Exception)
      {
      }
      return 1;
    }
    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
      if (filmstrip==null) return;
      filmstrip.InfoImageFileName=item.ThumbnailImage;
    }
	}
}
