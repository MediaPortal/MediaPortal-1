using System;
using System.Collections;
using System.Net;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicFavorites : GUIWindow
  {
    enum Mode 
    {
      Duration,
      TimesPlayed,
			Rating
    }
    enum Controls
    {
      CONTROL_BTNVIEWASICONS = 2, 
      CONTROL_BTNSORTBY = 3, 
      CONTROL_BTNSORTASC = 4, 
      CONTROL_BTNTYPE = 7,  
      CONTROL_BTN_CHANGE_INFO=6,
      CONTROL_BTN_RESETTOP100 = 9, 

      CONTROL_LIST = 50, 
      CONTROL_THUMBS = 51, 
      CONTROL_LABELFILES = 12, 
			CONTROL_LABEL = 15,
		CONTROL_SEARCH = 11
    };
    #region Base variabeles

    enum View
    {
      VIEW_AS_LIST = 0, 
      VIEW_AS_ICONS = 1, 
      VIEW_AS_LARGEICONS = 2, 
    }
    View currentView = View.VIEW_AS_LIST;
    View currentViewRoot = View.VIEW_AS_LIST;

    DirectoryHistory m_history = new DirectoryHistory();
    string m_strDirectory = "";
    int m_iItemSelected = -1;
    VirtualDirectory m_directory = new VirtualDirectory();
    #endregion
    MusicDatabase		      m_database = new MusicDatabase();
    Mode              _CurrentMode =Mode.Duration;

    public GUIMusicFavorites()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_FAVORITES;
      
      m_directory.AddDrives();
      m_directory.SetExtensions(Utils.AudioExtensions);
      LoadSettings();
    }
    ~GUIMusicFavorites()
    {
    }

    public override bool Init()
    {
      m_strDirectory = "";
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      return Load(GUIGraphicsContext.Skin + @"\mymusicfavorites.xml");
    }
    #region Serialisation
    void LoadSettings()
    {
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp = "";
        strTmp = (string)xmlreader.GetValue("musictop100","viewby");
        if (strTmp != null)
        {
          if (strTmp == "list") currentView = View.VIEW_AS_LIST;
          else if (strTmp == "icons") currentView = View.VIEW_AS_ICONS;
        }
        strTmp = (string)xmlreader.GetValue("musictop100","viewbyroot");
        if (strTmp != null)
        {
          if (strTmp == "list") currentViewRoot = View.VIEW_AS_LIST;
          else if (strTmp == "icons") currentViewRoot = View.VIEW_AS_ICONS;
        }
      }
    }

    void SaveSettings()
    {
      using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentView)
        {
          case View.VIEW_AS_LIST : 
            xmlwriter.SetValue("musictop100","viewby","list");
            break;
          case View.VIEW_AS_ICONS : 
            xmlwriter.SetValue("musictop100","viewby","icons");
            break;
        }
        switch (currentViewRoot)
        {
          case View.VIEW_AS_LIST : 
            xmlwriter.SetValue("musictop100","viewbyroot","list");
            break;
          case View.VIEW_AS_ICONS : 
            xmlwriter.SetValue("musictop100","viewbyroot","icons");
            break;
        }
      }
    }
    #endregion


    void OnThreadMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC : 
          string strFile = message.Label;
          if (Utils.IsAudio(strFile))
          {
            if (GUIWindowManager.ActiveWindow != GetID)
            {
              m_database.IncrTop100CounterByFileName(strFile);
            }
          }
          break;
      }
    }

    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        //GoParentFolder();
        return;
      }

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				GUIWindowManager.PreviousWindow();
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
					LoadSettings();
          
          ShowThumbPanel();
          LoadDirectory(m_strDirectory);
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
          m_iItemSelected = GetSelectedItemNo();
					if (GUIMusicFiles.IsMusicWindow(message.Param1))
					{
						MusicState.StartWindow=message.Param1;
					}
          SaveSettings();
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED : 
          int iControl = message.SenderControlId;
          if (iControl == (int)Controls.CONTROL_BTNVIEWASICONS)
          {
            if (m_strDirectory == "")
            {
              switch (currentViewRoot)
              {
                case View.VIEW_AS_LIST : 
                  currentViewRoot = View.VIEW_AS_ICONS;
                  break;
                case View.VIEW_AS_ICONS : 
                  currentViewRoot = View.VIEW_AS_LIST;
                  break;
              }
            }
            else
            {
              switch (currentView)
              {
                case View.VIEW_AS_LIST : 
                  currentView = View.VIEW_AS_ICONS;
                  break;
                case View.VIEW_AS_ICONS : 
                  currentView = View.VIEW_AS_LIST;
                  break;
              }
            }
            ShowThumbPanel();
            GUIControl.FocusControl(GetID, iControl);
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
          
          if (iControl == (int)Controls.CONTROL_BTNTYPE)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int nSelected = (int)msg.Param1;
            int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
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
              GUIWindowManager.ReplaceWindow(nNewWindow);
            }

            return true;
          }
          if (iControl == (int)Controls.CONTROL_THUMBS || iControl == (int)Controls.CONTROL_LIST)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int iItem = (int)msg.Param1;
            int iAction = (int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SHOW_INFO) 
            {
              OnInfo(iItem);
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

          if (iControl==(int)Controls.CONTROL_BTN_RESETTOP100)
          {
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null != dlgYesNo)
            {
              dlgYesNo.SetHeading(GUILocalizeStrings.Get(718));
              dlgYesNo.SetLine(1, GUILocalizeStrings.Get(719));
              dlgYesNo.SetLine(2, "");
              dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
              if (!dlgYesNo.IsConfirmed) return true;
              m_database.ResetTop100();
              LoadDirectory(m_strDirectory);
            }
          }
          if (iControl==(int)Controls.CONTROL_BTN_CHANGE_INFO)
          {
            switch (_CurrentMode)
            {
              case Mode.Duration: 
                  _CurrentMode=Mode.TimesPlayed;
              break;
              case Mode.TimesPlayed:
                  _CurrentMode=Mode.Rating;
								break;
							case Mode.Rating:
								_CurrentMode=Mode.Duration;
								break;
            }
            LoadDirectory(m_strDirectory);
            GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BTN_CHANGE_INFO);
          }
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

      dlg.Add( GUILocalizeStrings.Get(208)); //play
      dlg.Add( GUILocalizeStrings.Get(926)); //Queue
			dlg.Add( GUILocalizeStrings.Get(136)); //PlayList
			if (!item.IsFolder && !item.IsRemote)
			{
				dlg.AddLocalizedString(933); //Remove from favorites
				dlg.AddLocalizedString(931); //Rating
			}

      dlg.DoModal( GetID);
      if (dlg.SelectedLabel==-1) return;
      switch (dlg.SelectedLabel)
      {
        case 0: // play
          OnClick(itemNo);	
          break;
					
        case 1: // add to playlist
          OnQueueItem(itemNo);	
          break;
					
        case 2: // show playlist
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
					break;
				case 3: // add to favorites
					m_database.RemoveSongFromFavorites(item.Path);
					LoadDirectory(m_strDirectory);
					break;
				case 4:// Rating
					OnSetRating( GetSelectedItemNo());
					break;
      }
    }

		void OnSetRating(int itemNumber)
		{
			GUIListItem item = GetItem(itemNumber);
			if (item ==null) return;
			GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_DIALOG_RATING);
			if (item.MusicTag!=null) 
			{
				dialog.Rating=((MusicTag)item.MusicTag).Rating;
				dialog.SetTitle(String.Format("{0}-{1}", ((MusicTag)item.MusicTag).Artist, ((MusicTag)item.MusicTag).Title) );
			}
			dialog.FileName=item.Path;
			dialog.DoModal(GetID);
			if (item.MusicTag!=null) 
			{
				((MusicTag)item.MusicTag).Rating=dialog.Rating;
			}
			m_database.SetRating(item.Path,dialog.Rating);
			if (dialog.Result == GUIDialogSetRating.ResultCode.Previous)
			{
				while (itemNumber >0)
				{
					itemNumber--;
					item = GetItem(itemNumber);
					if (!item.IsFolder && !item.IsRemote)
					{
						OnSetRating(itemNumber);
						return;
					}
				}
			}

			if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
			{
				while (itemNumber+1 < GetItemCount() )
				{
					itemNumber++;
					item = GetItem(itemNumber);
					if (!item.IsFolder && !item.IsRemote)
					{
						OnSetRating(itemNumber);
						return;
					}
				}
			}
		}

    bool ViewByIcon
    {
      get 
      {
        if (m_strDirectory == "")
        {
          if (currentViewRoot != View.VIEW_AS_LIST) return true;
        }
        else
        {
          if (currentView != View.VIEW_AS_LIST) return true;
        }
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (m_strDirectory == "")
        {
          if (currentViewRoot == View.VIEW_AS_LARGEICONS) return true;
        }
        else
        {
          if (currentView == View.VIEW_AS_LARGEICONS) return true;
        }
        return false;
      }
    }

		void SetLabels()
		{
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
                item.Label = String.Format("{0:00}. {1} - {2}",i + 1, tag.Artist, tag.Title);
              else
                item.Label = String.Format("{0:00}. {1} - {2}",i + 1, tag.Artist, tag.Title);
            }
            else
            {
              if (tag.Track > 0)
                item.Label = String.Format("{0:00}. {1} ",i + 1, tag.Title);
              else
                item.Label = String.Format("{0:00}. {1}",i + 1, tag.Title);
            }
          }
        }
        
        item.Label2="";
        switch (_CurrentMode)
        {
          case Mode.Duration:
            if (tag != null)
            {
              int nDuration = tag.Duration;
              if (nDuration > 0)
              {
                item.Label2 = Utils.SecondsToHMSString(nDuration);
              }
              
            }
          break;
          case Mode.TimesPlayed:
            if (tag!=null)
            {
              item.Label2 = String.Format("{0}x",tag.TimesPlayed);
            }
						break;
					case Mode.Rating:
						if (tag!=null)
						{
							item.Label2 = String.Format("{0}",tag.Rating);
						}
						break;
        }

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
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_BTNTYPE, MusicState.StartWindow - (int)GUIWindow.Window.WINDOW_MUSIC_FILES);

      GUIControl.HideControl(GetID, (int)Controls.CONTROL_LIST);
      GUIControl.HideControl(GetID, (int)Controls.CONTROL_THUMBS);
      
      int iControl = (int)Controls.CONTROL_LIST;
      if (ViewByIcon)
        iControl = (int)Controls.CONTROL_THUMBS;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);
      

      string strLine = "";
      View view = currentView;
      if (m_strDirectory == "") view = currentViewRoot;
      switch (view)
      {
        case View.VIEW_AS_LIST : 
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.VIEW_AS_ICONS : 
          strLine = GUILocalizeStrings.Get(100);
          break;
      }
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEWASICONS, strLine);

      GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNSORTBY);
      GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
    
      switch(_CurrentMode)
      {  
        case Mode.Duration:
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_CHANGE_INFO, GUILocalizeStrings.Get(720));
          break;
        case Mode.TimesPlayed:
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_CHANGE_INFO, GUILocalizeStrings.Get(721));
					break;
				case Mode.Rating:
					GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTN_CHANGE_INFO, GUILocalizeStrings.Get(935));
					break;
      }
    }
	  void keyboard_TextChanged(int kindOfSearch,string data)
	  {
		  DisplayTop100List(kindOfSearch,data);
	  }
    void ShowThumbPanel()
    {
      int iItem = GetSelectedItemNo();
      
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iItem);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iItem);
      }
      UpdateButtons();
    }
    
    void OnRetrieveCoverArt(GUIListItem item)
    {
      Utils.SetDefaultIcons(item);
      MusicTag tag = (MusicTag)item.MusicTag;
      string strThumb=GUIMusicFiles.GetCoverArt(item.IsFolder,item.Path,tag);
      if (strThumb!=String.Empty)
      {
        item.ThumbnailImage = strThumb;
        item.IconImageBig = strThumb;
        item.IconImage = strThumb;
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
      m_strDirectory = strNewDirectory;
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS);
            
      string strObjects = "";

			ArrayList itemlist = new ArrayList();
			ArrayList songs = new ArrayList();
			m_database.GetSongsByFavorites(out songs);
			foreach (Song song in songs)
			{
				GUIListItem item = new GUIListItem();
				item.Label = song.Title;
				item.IsFolder = false;
				item.Path = song.FileName;
				item.Duration = song.Duration;
					
				MusicTag tag = new MusicTag();
				tag.Title = song.Title;
				tag.Album = song.Album;
				tag.Artist = song.Artist;
				tag.Duration = song.Duration;
				tag.Genre = song.Genre;
				tag.Track = song.Track;
				tag.Year = song.Year;
        tag.TimesPlayed=song.TimesPlayed;
				tag.Rating=song.Rating;
        item.MusicTag = tag;
        item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
		
				itemlist.Add(item);
			}
      

      string strSelectedItem = m_history.Get(m_strDirectory);
      int iItem = 0;
      foreach (GUIListItem item in itemlist)
      {
        GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_LIST, item);
        GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_THUMBS, item);
      }
      
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.Label == strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iItem);
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iItem);
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
			GUIPropertyManager.SetProperty("#itemcount", strObjects);
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABEL, m_strDirectory);
			SetLabels();
      ShowThumbPanel();

      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, m_iItemSelected);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, m_iItemSelected);
      }
		}
	  
	  void DisplayTop100List(int searchKind,string searchText)
	  {
		  GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
		  GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS);
            

		  ArrayList itemlist = new ArrayList();
		  ArrayList songs = new ArrayList();
		  m_database.GetTop100(searchKind,searchText,ref songs);
		  foreach (Song song in songs)
		  {
			  GUIListItem item = new GUIListItem();
			  item.Label = song.Title;
			  item.IsFolder = false;
			  item.Path = song.FileName;
			  item.Duration = song.Duration;
					
			  MusicTag tag = new MusicTag();
			  tag.Title = song.Title;
			  tag.Album = song.Album;
			  tag.Artist = song.Artist;
			  tag.Duration = song.Duration;
			  tag.Genre = song.Genre;
			  tag.Track = song.Track;
			  tag.Year = song.Year;
			  tag.TimesPlayed=song.TimesPlayed;
			  item.MusicTag = tag;
			  Utils.SetDefaultIcons(item);
		
			  itemlist.Add(item);
		  }
		  //
		  m_history.Set(m_strDirectory, m_strDirectory); //save where we are
		  GUIListItem dirUp=new GUIListItem("..");
		  dirUp.Path=m_strDirectory; // to get where we are
		  dirUp.IsFolder=true;
		  dirUp.ThumbnailImage="";
		  dirUp.IconImage="defaultFolderBack.png";
		  dirUp.IconImageBig="defaultFolderBackBig.png";
		  itemlist.Insert(0,dirUp);
		  //

		  foreach (GUIListItem item in itemlist)
		  {
        MusicTag tag = (MusicTag)item.MusicTag;
        string strThumb=GUIMusicFiles.GetCoverArt(item.IsFolder,item.Path,tag);
        if (strThumb!=String.Empty)
        {
          item.ThumbnailImage = strThumb;
          item.IconImageBig = strThumb;
          item.IconImage = strThumb;
        }
		  }

		  foreach (GUIListItem item in itemlist)
		  {
			  GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_LIST, item);
			  GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_THUMBS, item);
		  }
      
		  int iTotalItems = itemlist.Count;
		  if (itemlist.Count>0)
		  {
			  GUIListItem rootItem=(GUIListItem)itemlist[0];
			  if (rootItem.Label=="..") iTotalItems--;
		  }
		  string strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
		  GUIPropertyManager.SetProperty("#itemcount", strObjects);
		  GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
		  GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABEL, m_strDirectory);
		  SetLabels();
		  ShowThumbPanel();
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
				// play item
				//play and add current directory to temporary playlist
				int nFolderCount = 0;
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
					PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
					playlistItem.FileName = pItem.Path;
					playlistItem.Description = pItem.Label;
					int iDuration = 0;
					MusicTag tag = (MusicTag)pItem.MusicTag;
					if (tag != null) iDuration = tag.Duration;
					playlistItem.Duration = iDuration;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
				}

        //	Save current window and directory to know where the selected item was
        MusicState.TempPlaylistWindow = GetID;
        MusicState.TempPlaylistDirectory = m_strDirectory;

				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
				PlayListPlayer.Play(iItem - nFolderCount);
      }
    }
    
    void OnQueueItem(int iItem)
    {
      // add item 2 playlist
      GUIListItem pItem = GetItem(iItem);
      AddItemToPlayList(pItem);
	
      //move to next item
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iItem + 1);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iItem + 1);

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
        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item);
        }
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
          dlgOK.SetLine(1, 477);
          dlgOK.SetLine(2, "");
          dlgOK.DoModal(GetID);
        }
        return;
      }

      if (playlist.Count == 1)
			{
				Log.Write("GUIMusicFavorites Play:{0}",playlist[0].FileName);
        g_Player.Play(playlist[0].FileName);
        return;
      }

      // clear current playlist
      PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Clear();

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
		
    
    void OnInfo(int iItem)
    {
    }
  }
}
