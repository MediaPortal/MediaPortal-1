using System;
using System.Diagnostics;
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
  public class GUIMusicAlbums: GUIWindow, IComparer 
  {
    enum Controls
    {
      CONTROL_BTNVIEWASICONS    =2,
      CONTROL_BTNSORTBY		      =3,
      CONTROL_BTNSORTASC	      =4,
      CONTROL_BTNTYPE           =6,
      CONTROL_BTNREC            =10,
      CONTROL_LIST				      =50,
      CONTROL_THUMBS			      =51,
      CONTROL_BIGICONS		      =52,
      CONTROL_LABELFILES        =12,
      CONTROL_IMAGE_ALBUM				=13,
      CONTROL_LABELALBUMTEXT		=14,
      CONTROL_LABEL				      =15,
      CONTROL_BTNALLRECENTALBUMS=9,
		CONTROL_SEARCH = 8

    };
    #region Base variabeles
    enum SortMethod
    {
      SORT_ARTIST=6,
      SORT_ALBUM=7,
      SORT_TRACK=8,
      SORT_NAME=9,
			SORT_RATING=10
    }

    enum View
    {
      VIEW_AS_LIST    =       0,
      VIEW_AS_ICONS    =      1,
      VIEW_AS_LARGEICONS    =      2,
    }
    View              currentView=View.VIEW_AS_LIST;
    SortMethod        currentSortMethod=SortMethod.SORT_TRACK;
    bool              m_bSortAscending=true;
    View              currentViewRoot=View.VIEW_AS_LIST;
    SortMethod        currentSortMethodRoot=SortMethod.SORT_ALBUM;
    bool              m_bSortAscendingRoot=true;

    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory="";
    int               m_iItemSelected=-1;   
    VirtualDirectory  m_directory = new VirtualDirectory();
    #endregion
    Database		m_database = new Database();
		bool				m_bMyMusicAlbumShowRecent=false;
		string			m_strAlbum="";

    public GUIMusicAlbums()
    {
      GetID=(int)GUIWindow.Window.WINDOW_MUSIC_ALBUM;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.AudioExtensions);
      LoadSettings();
    }
    ~GUIMusicAlbums()
    {
    }
    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp="";
        strTmp=(string)xmlreader.GetValue("musicalbum","viewby");
        if (strTmp!=null)
        {
          if (strTmp=="list") currentView=View.VIEW_AS_LIST;
          else if (strTmp=="icons") currentView=View.VIEW_AS_ICONS;
          else if (strTmp=="bigicons") currentView=View.VIEW_AS_LARGEICONS;
        }
        strTmp=(string)xmlreader.GetValue("musicalbum","viewbyroot");
        if (strTmp!=null)
        {
          if (strTmp=="list") currentViewRoot=View.VIEW_AS_LIST;
          else if (strTmp=="icons") currentViewRoot=View.VIEW_AS_ICONS;
          else if (strTmp=="bigicons") currentViewRoot=View.VIEW_AS_LARGEICONS;
        }
        strTmp=(string)xmlreader.GetValue("musicalbum","sort");
        if (strTmp!=null)
        {
          if (strTmp=="album") currentSortMethod=SortMethod.SORT_ALBUM;
          else if (strTmp=="artist") currentSortMethod=SortMethod.SORT_ARTIST;
          else if (strTmp=="track") currentSortMethod=SortMethod.SORT_TRACK;
					else if (strTmp=="name") currentSortMethod=SortMethod.SORT_NAME;
					else if (strTmp=="rating") currentSortMethod=SortMethod.SORT_RATING;
        }
        strTmp=(string)xmlreader.GetValue("musicalbum","sortroot");
        if (strTmp!=null)
        {
          if (strTmp=="album") currentSortMethodRoot=SortMethod.SORT_ALBUM;
          else if (strTmp=="artist") currentSortMethodRoot=SortMethod.SORT_ARTIST;
          else if (strTmp=="track") currentSortMethodRoot=SortMethod.SORT_TRACK;
					else if (strTmp=="name") currentSortMethodRoot=SortMethod.SORT_NAME;
					else if (strTmp=="rating") currentSortMethodRoot=SortMethod.SORT_RATING;
        }
        m_bSortAscending=xmlreader.GetValueAsBool("musicalbum","sortascending",true);
        m_bSortAscendingRoot=xmlreader.GetValueAsBool("musicalbum","sortascendingroot",true);

      }
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentView)
        {
          case View.VIEW_AS_LIST: 
            xmlwriter.SetValue("musicalbum","viewby","list");
            break;
          case View.VIEW_AS_ICONS: 
            xmlwriter.SetValue("musicalbum","viewby","icons");
            break;
          case View.VIEW_AS_LARGEICONS: 
            xmlwriter.SetValue("musicalbum","viewby","bigicons");
            break;
        }
        switch (currentViewRoot)
        {
          case View.VIEW_AS_LIST: 
            xmlwriter.SetValue("musicalbum","viewbyroot","list");
            break;
          case View.VIEW_AS_ICONS: 
            xmlwriter.SetValue("musicalbum","viewbyroot","icons");
            break;
          case View.VIEW_AS_LARGEICONS: 
            xmlwriter.SetValue("musicalbum","viewbyroot","bigicons");
            break;
        }
        switch (currentSortMethod)
        {
          case SortMethod.SORT_ALBUM:
            xmlwriter.SetValue("musicalbum","sort","album");
            break;
          case SortMethod.SORT_ARTIST:
            xmlwriter.SetValue("musicalbum","sort","artist");
            break;
          case SortMethod.SORT_TRACK:
            xmlwriter.SetValue("musicalbum","sort","track");
            break;
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("musicalbum","sort","name");
						break;
					case SortMethod.SORT_RATING:
						xmlwriter.SetValue("musicalbum","sort","rating");
						break;
        }
        switch (currentSortMethodRoot)
        {
          case SortMethod.SORT_ALBUM:
            xmlwriter.SetValue("musicalbum","sortroot","album");
            break;
          case SortMethod.SORT_ARTIST:
            xmlwriter.SetValue("musicalbum","sortroot","artist");
            break;
          case SortMethod.SORT_TRACK:
            xmlwriter.SetValue("musicalbum","sortroot","track");
            break;
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("musicalbum","sortroot","name");
						break;
					case SortMethod.SORT_RATING:
						xmlwriter.SetValue("musicalbum","sortroot","rating");
						break;
        }

        xmlwriter.SetValueAsBool("musicalbum","sortascending",m_bSortAscending);
        xmlwriter.SetValueAsBool("musicalbum","sortascendingroot",m_bSortAscendingRoot);
      }
    }
    #endregion

    public override bool Init()
    {
      m_strDirectory="";
      return Load (GUIGraphicsContext.Skin+@"\mymusicalbum.xml");
    }

    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = GetItem(0);
        if (item!=null)
        {
          if (item.IsFolder && item.Label=="..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_HOME);
        return;
      }

      if (action.wID==Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
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
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
					LoadSettings();
					m_database.Open();
        
          LoadDirectory(m_strDirectory);
          ShowThumbPanel();
          OnSort();
          UpdateButtons();
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          SaveSettings();
          m_iItemSelected=GetSelectedItemNo();
          m_database.Close();
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_BTNVIEWASICONS)
          {
            if (m_strDirectory=="")
            {
              switch (currentViewRoot)
              {
                case View.VIEW_AS_LIST:
                  currentViewRoot=View.VIEW_AS_ICONS;
                  break;
                case View.VIEW_AS_ICONS:
                  currentViewRoot=View.VIEW_AS_LARGEICONS;
                  break;
                case View.VIEW_AS_LARGEICONS:
                  currentViewRoot=View.VIEW_AS_LIST;
                  break;
              }
            }
            else
            {
              switch (currentView)
              {
                case View.VIEW_AS_LIST:
                  currentView=View.VIEW_AS_ICONS;
                  break;
                case View.VIEW_AS_ICONS:
                  currentView=View.VIEW_AS_LARGEICONS;
                  break;
                case View.VIEW_AS_LARGEICONS:
                  currentView=View.VIEW_AS_LIST;
                  break;
              }
            }
            LoadDirectory(m_strDirectory);
            GUIControl.FocusControl(GetID,iControl);
          }
          
          if (iControl==(int)Controls.CONTROL_BTNSORTASC)
          {
            if(m_strDirectory=="")
              m_bSortAscendingRoot=!m_bSortAscendingRoot;
            else
              m_bSortAscending=!m_bSortAscending;
            SaveSettings();
            OnSort();
            UpdateButtons();
            GUIControl.FocusControl(GetID,iControl);
          }


          if (iControl==(int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            if (m_strDirectory=="")
            {
              switch (currentSortMethodRoot)
              {
                case SortMethod.SORT_ARTIST:
                  currentSortMethodRoot=SortMethod.SORT_ALBUM;
                  break;
                case SortMethod.SORT_ALBUM:
                  currentSortMethodRoot=SortMethod.SORT_ARTIST;
                  break;
              }
            }
            else
            {
              switch (currentSortMethod)
              {
                case SortMethod.SORT_ARTIST:
                  currentSortMethod=SortMethod.SORT_TRACK;
                  break;
                case SortMethod.SORT_TRACK:
                  currentSortMethod=SortMethod.SORT_NAME;
                  break;
                case SortMethod.SORT_NAME:
                  currentSortMethod=SortMethod.SORT_RATING;
									break;
								case SortMethod.SORT_RATING:
									currentSortMethod=SortMethod.SORT_ARTIST;
									break;
              }
            }
            OnSort();
            UpdateButtons();
            GUIControl.FocusControl(GetID,iControl);
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
          if (iControl==(int)Controls.CONTROL_BTNTYPE)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            int nSelected=(int)msg.Param1;
            int nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_ALBUM;
            switch (nSelected)
            {
              case 0:	//	files
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_FILES;
                break;
              case 1:	//	albums
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_ALBUM;
                break;
              case 2:	//	artist
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_ARTIST;
                break;
              case 3:	//	genres
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
                break;
              case 4:	//	top100
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
								break;
							case 5 : //	favorites
								nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FAVORITES;
								break;
            }

            if (nNewWindow!=GetID)
            {
              MusicState.StartWindow=nNewWindow;
              GUIWindowManager.ActivateWindow(nNewWindow);
            }

            return true;
          }
          if (iControl==(int)Controls.CONTROL_THUMBS||iControl==(int)Controls.CONTROL_LIST||iControl==(int)Controls.CONTROL_BIGICONS)
          {
						int iAction=message.Param1;

						if (iAction == (int)Action.ActionType.ACTION_QUEUE_ITEM)
						{
              OnQueueSelectedItem();
						}
						else if (iAction==(int)Action.ActionType.ACTION_SHOW_INFO)
						{
              OnInfo(GetSelectedItemNo());
              LoadDirectory(m_strDirectory);
						}
						else if (iAction==(int)Action.ActionType.ACTION_SELECT_ITEM)
						{
							OnClick(GetSelectedItemNo());
						}

          }
					if (iControl==(int)Controls.CONTROL_BTNALLRECENTALBUMS)
					{
						m_bMyMusicAlbumShowRecent=!m_bMyMusicAlbumShowRecent;
						SaveSettings();
						m_strAlbum="";
						LoadDirectory(m_strDirectory);
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

      dlg.Add( GUILocalizeStrings.Get(928)); //IMDB
      dlg.Add( GUILocalizeStrings.Get(208)); //play
      dlg.Add( GUILocalizeStrings.Get(926)); //Queue
			dlg.Add( GUILocalizeStrings.Get(136)); //PlayList
			if (!item.IsFolder && !item.IsRemote)
			{
				dlg.AddLocalizedString(930); //Add to favorites
				dlg.AddLocalizedString(931); //Rating
			}

      dlg.DoModal( GetID);
      if (dlg.SelectedLabel==-1) return;
      switch (dlg.SelectedLabel)
      {
        case 0: // IMDB
          OnInfo(itemNo);
          break;

        case 1: // play
          OnClick(itemNo);	
          break;
					
        case 2: // Add song or album to playlist
          OnQueueSelectedItem();	
          break;
					
        case 3: // show playlist
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
					break;
				case 4: // add to favorites
					m_database.AddSongToFavorites(item.Path);
					break;
				case 5:// Rating
					OnSetRating(item);
					break;
      }
    }
		void OnSetRating(GUIListItem item)
		{
				GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_DIALOG_RATING);
			if (item.MusicTag!=null) 
			{
				dialog.Rating=((MusicTag)item.MusicTag).Rating;
			}
			dialog.DoModal(GetID);
			m_database.SetRating(item.Path,dialog.Rating);

		}

    bool ViewByIcon
    {
      get 
      {
        if (m_strDirectory=="")
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
        if (m_strDirectory=="")
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

    GUIListItem GetSelectedItem()
    {
      int iControl;
      if (ViewByLargeIcon)
      {
        iControl=(int)Controls.CONTROL_BIGICONS;
      }
      else if (ViewByIcon)
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
      if (ViewByLargeIcon)
      {
        iControl=(int)Controls.CONTROL_BIGICONS;
      }
      else if (ViewByIcon)
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
      if (ViewByLargeIcon)
      {
        iControl=(int)Controls.CONTROL_BIGICONS;
      }
      else if (ViewByIcon)
      {
        iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      int iControl;
      if (ViewByLargeIcon)
      {
        iControl=(int)Controls.CONTROL_BIGICONS;
      }
      else if (ViewByIcon)
      {
        iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;

      return GUIControl.GetItemCount(GetID,iControl);
    }

    void UpdateButtons()
    {
      GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_BTNTYPE,MusicState.StartWindow-(int)GUIWindow.Window.WINDOW_MUSIC_FILES);

      int iControl=(int)Controls.CONTROL_LIST;
      if (ViewByLargeIcon)
        iControl=(int)Controls.CONTROL_BIGICONS;
      else if (ViewByIcon)
        iControl=(int)Controls.CONTROL_THUMBS;
      GUIControl.ShowControl(GetID,iControl);
      GUIControl.FocusControl(GetID,iControl);
      
      string strLine="";
      View view=currentView;
      if (m_strDirectory=="") view=currentViewRoot;
      switch (view)
      {
        case View.VIEW_AS_LIST:
          strLine=GUILocalizeStrings.Get(101);
          break;
        case View.VIEW_AS_ICONS:
          strLine=GUILocalizeStrings.Get(100);
          break;
        case View.VIEW_AS_LARGEICONS:
          strLine=GUILocalizeStrings.Get(417);
          break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNVIEWASICONS,strLine);

      SortMethod method=currentSortMethod;
      if (m_strDirectory=="") method=currentSortMethodRoot;
      switch (method)
      {
        case SortMethod.SORT_ARTIST:
          strLine=GUILocalizeStrings.Get(269);
          break;
        case SortMethod.SORT_ALBUM:
          strLine=GUILocalizeStrings.Get(270);
          break;
        case SortMethod.SORT_TRACK:
          strLine=GUILocalizeStrings.Get(266);
          break;
        case SortMethod.SORT_NAME:
          strLine=GUILocalizeStrings.Get(268);
					break;
				case SortMethod.SORT_RATING:
					strLine=GUILocalizeStrings.Get(367);
					break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY,strLine);

      bool bSortAsc=m_bSortAscending;
      if (m_strDirectory=="") bSortAsc=m_bSortAscendingRoot;
      if (bSortAsc)
        GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);

			//	Update recently played albums button and album label
			if (m_bMyMusicAlbumShowRecent)
			{
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNALLRECENTALBUMS, GUILocalizeStrings.Get(358 ));
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELALBUMTEXT, GUILocalizeStrings.Get(359 ));
			}
			else
			{
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNALLRECENTALBUMS, GUILocalizeStrings.Get(359 ));
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELALBUMTEXT, GUILocalizeStrings.Get(132) );
			}
    }
	  void keyboard_TextChanged(int kindOfSearch,string data)
	  {
		  DisplayAlbumsList(kindOfSearch,data);
	  }
    void ShowThumbPanel()
    {
      int iItem=GetSelectedItemNo(); 
      if ( ViewByLargeIcon )
      {
        GUIThumbnailPanel pControl=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_BIGICONS);
        pControl.ShowBigIcons(true);
      }
      else if (ViewByIcon)
      {
        GUIListControl pControl=(GUIListControl)GetControl((int)Controls.CONTROL_THUMBS);
      }
      else
      {
        GUIListControl pControl=(GUIListControl)GetControl((int)Controls.CONTROL_THUMBS);
      }
      if (iItem>-1)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST,iItem);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS,iItem);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_BIGICONS,iItem);
      }
      UpdateButtons();
    }

    void OnRetrieveCoverArt(GUIListItem item)
    {
      DateTime dateStart=DateTime.Now;
      MusicTag tag = (MusicTag)item.MusicTag;
      if (tag!=null)
      {
        if (tag.Album.Length>0)
        {
          string strThumb=GUIMusicFiles.GetAlbumThumbName(tag.Artist,tag.Album);
          if (strThumb!=String.Empty && System.IO.File.Exists(strThumb) )
          {
            item.ThumbnailImage=strThumb;
            item.IconImageBig=strThumb;
            item.IconImage=strThumb;
          }
          else
          {
            strThumb = Utils.GetFolderThumb(item.Path);
            if (System.IO.File.Exists(strThumb))
            {
              item.ThumbnailImage=strThumb;
              item.IconImageBig=strThumb;
              item.IconImage=strThumb;
            }
            else if (m_strDirectory.Length==0)
            {
              if (!item.Path.ToLower().Equals("unknown") && item.Path!=String.Empty)
              {
                ArrayList songs=new ArrayList();
                m_database.GetSongsByAlbum(item.Path,ref songs);
                foreach (Song song in songs)
                {
                  strThumb = GUIMusicFiles.GetCoverArt(false,song.FileName,tag);
                  if (strThumb!=String.Empty)
                  {
                    item.ThumbnailImage=strThumb;
                    item.IconImageBig=strThumb;
                    item.IconImage=strThumb;
                    break;
                  }
                }
              }
            }
          }
        }
      }
      TimeSpan ts = DateTime.Now-dateStart;
      Console.WriteLine( String.Format("cover art for {0} took {1} msec", item.Path,ts.TotalMilliseconds));
    }

    void LoadDirectory(string strNewDirectory)
    {
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST);
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_THUMBS);
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_BIGICONS);

      GUIListItem SelectedItem = GetSelectedItem();
      if (SelectedItem!=null) 
      {
        if (SelectedItem.IsFolder && SelectedItem.Label!="..")
        {
          m_history.Set(SelectedItem.Label, m_strDirectory);
        }
      }
      m_strDirectory=strNewDirectory;
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_THUMBS);
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_BIGICONS);
            
      string strObjects="";

      DateTime dtStart=DateTime.Now;//@@@
			ArrayList itemlist=new ArrayList();
			if (m_strDirectory.Length==0)
			{
				ArrayList Albums=new ArrayList();
				if (m_bMyMusicAlbumShowRecent) 
					m_database.GetRecentlyPlayedAlbums(ref Albums);
				else 
					m_database.GetAlbums(ref Albums);
				foreach(AlbumInfo info in Albums)
				{
					GUIListItem item=new GUIListItem();
					item.Label=info.Album;
					item.Label2=info.Artist;
					item.Path=info.Album;
					item.IsFolder=true;
					MusicTag tag=new MusicTag();
					tag.Title=" ";
					tag.Genre=info.Genre;
					tag.Year=info.Year;
					tag.Album=info.Album;
					tag.Artist=info.Artist;
          item.MusicTag=tag;
          
          item.ThumbnailImage="defaultAlbum.png";
          item.IconImageBig="defaultAlbum.png";
          item.IconImage="defaultAlbum.png";

          item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          itemlist.Add(item);

				}
			}
			else
			{
        GUIListItem pItem = new GUIListItem ("..");
        pItem.Path="";
        pItem.IsFolder=true;
        Utils.SetDefaultIcons(pItem);
        if (ViewByIcon &&!ViewByLargeIcon) 
          pItem.IconImage=pItem.IconImageBig;
        itemlist.Add(pItem);
        
        ArrayList songs=new ArrayList();
				m_database.GetSongsByAlbum(m_strAlbum,ref songs);
				foreach (Song song in songs)
				{
					GUIListItem item=new GUIListItem();
					item.Label=song.Title;
					item.IsFolder=false;
					item.Path=song.FileName;
					item.Duration=song.Duration;
					
					MusicTag tag=new MusicTag();
					tag.Title=song.Title;
					tag.Album=song.Album;
					tag.Artist=song.Artist;
					tag.Duration=song.Duration;
					tag.Genre=song.Genre;
					tag.Track=song.Track;
					tag.Year=song.Year;
					tag.Rating=song.Rating;
          item.MusicTag=tag;
          item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
		
					itemlist.Add(item);
				}
			}

      TimeSpan ts=DateTime.Now-dtStart;//@@@
      Trace.WriteLine(String.Format("load:{0}",ts.TotalMilliseconds) );
      
      GUIListControl cntl1=(GUIListControl )GetControl((int)Controls.CONTROL_LIST);
      GUIListControl cntl2=(GUIListControl )GetControl((int)Controls.CONTROL_THUMBS);
      GUIThumbnailPanel cntl3=(GUIThumbnailPanel )GetControl((int)Controls.CONTROL_BIGICONS);
      string strSelectedItem=m_history.Get(m_strDirectory);	
      int iItem=0;
      foreach (GUIListItem item in itemlist)
      {
        cntl1.Add(item);
        cntl2.Add(item);
        cntl3.Add(item);
      }
      
      ts=DateTime.Now-dtStart;//@@@
      Trace.WriteLine(String.Format("add:{0}",ts.TotalMilliseconds) );
      OnSort();
      
      ts=DateTime.Now-dtStart;//@@@
      Trace.WriteLine(String.Format("sort:{0}",ts.TotalMilliseconds) );
      for (int i=0; i< GetItemCount();++i)
      {
        GUIListItem item =GetItem(i);
        if (item.Label==strSelectedItem || strSelectedItem.Length==0)
        {
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LIST,iItem);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_THUMBS,iItem);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_BIGICONS,iItem);
          break;
        }
        iItem++;
      }
      Trace.WriteLine(String.Format("selected:{0}",ts.TotalMilliseconds) );
      int iTotalItems=itemlist.Count;
      if (itemlist.Count>0)
      {
        GUIListItem rootItem=(GUIListItem)itemlist[0];
        if (rootItem.Label=="..") iTotalItems--;
      }
      strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
			GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABEL,m_strAlbum+" ");
			
      ShowThumbPanel();
      if (m_iItemSelected>=0)
      {
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LIST,m_iItemSelected);
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_THUMBS,m_iItemSelected);
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_BIGICONS,m_iItemSelected);
      }
      Trace.WriteLine(String.Format("done:{0}",ts.TotalMilliseconds) );
		}
    #endregion
		
		void SetLabels()
		{

			SortMethod method=currentSortMethod;
			if (m_strDirectory=="") method=currentSortMethodRoot;
			for (int i=0; i < GetItemCount();++i)
			{
				GUIListItem item=GetItem(i);
				MusicTag tag=(MusicTag)item.MusicTag;
				if (tag!=null)
				{
					if (tag.Title.Length>0)
					{
						if (tag.Artist.Length>0)
						{
							if (tag.Track>0)
								item.Label=String.Format("{0:00}. {1} - {2}",tag.Track, tag.Artist, tag.Title);
							else
								item.Label=String.Format("{0} - {1}",tag.Artist, tag.Title);
						}
						else
						{
							if (tag.Track>0)
								item.Label=String.Format("{0:00}. {1} ",tag.Track, tag.Title);
							else
								item.Label=String.Format("{0}",tag.Title);
						}
					}
					int nDuration=tag.Duration;
					if (nDuration>0)
					{
						item.Label2=Utils.SecondsToHMSString(nDuration);
					}
					if (method==SortMethod.SORT_RATING)
					{
						item.Label2=tag.Rating.ToString();
					}
				}
			}
		}

    #region Sort Members
    void OnSort()
    {
      if (m_strDirectory.Length!=0) SetLabels();
      GUIListControl list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST);
      list.Sort(this);
      
      list=(GUIListControl)GetControl((int)Controls.CONTROL_THUMBS);
      list.Sort(this);
      
      GUIThumbnailPanel panel=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_BIGICONS);
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

      string strSize1="";
      string strSize2="";
      if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);

      SortMethod method=currentSortMethod;
      if (m_strDirectory=="") method=currentSortMethodRoot;
      bool bSortAsc=m_bSortAscending;
      if (m_strDirectory=="") bSortAsc=m_bSortAscendingRoot;
      switch (method)
      {
        case SortMethod.SORT_ALBUM:
          if (item1.MusicTag!=null && item2.MusicTag!=null)
          {
            int ipos=0;
            string strAlbum1=((MusicTag)item1.MusicTag).Album;
            string strAlbum2=((MusicTag)item2.MusicTag).Album;
            if (bSortAsc)
            {
              ipos=String.Compare(strAlbum1,strAlbum2,true);
            }
            else
            {
              ipos=String.Compare(strAlbum2,strAlbum1,true);
            }
            if (ipos !=0) return ipos;
            string strArtist1=((MusicTag)item1.MusicTag).Artist;
            string strArtist2=((MusicTag)item2.MusicTag).Artist;
            if (bSortAsc)
            {
              ipos=String.Compare(strArtist1,strArtist2,true);
            }
            else
            {
              ipos=String.Compare(strArtist2,strArtist1,true);
            }
						if (ipos !=0) return ipos;
						string strTitle1=((MusicTag)item1.MusicTag).Title;
						string strTitle2=((MusicTag)item2.MusicTag).Title;
						if (bSortAsc)
						{
							ipos=String.Compare(strTitle1,strTitle2,true);
						}
						else
						{
							ipos=String.Compare(strTitle2,strTitle1,true);
						}
            return ipos;
          }
        break;
					
				case SortMethod.SORT_RATING:
					int iRating1 = 0;
					int iRating2 = 0;
					if (item1.MusicTag != null) iRating1 = ((MusicTag)item1.MusicTag).Rating;
					if (item2.MusicTag != null) iRating2 = ((MusicTag)item2.MusicTag).Rating;
					if (bSortAsc)
					{
						return (int)(iRating1 - iRating2);
					}
					else
					{
						return (int)(iRating2 - iRating1);
					}

        case SortMethod.SORT_TRACK:
          int iTrack1=0;
          int iTrack2=0;
          if (item1.MusicTag!=null) iTrack1=((MusicTag)item1.MusicTag).Track;
          if (item2.MusicTag!=null) iTrack2=((MusicTag)item2.MusicTag).Track;
          if (iTrack1==iTrack2) goto case SortMethod.SORT_NAME;
          if (bSortAsc)
          {
            return (int)(iTrack1 - iTrack2);
          }
          else
          {
            return (int)(iTrack2 - iTrack1);
          }

        case SortMethod.SORT_NAME:
          if (item1.MusicTag!=null && item2.MusicTag!=null)
          {
            string strTitle1=((MusicTag)item1.MusicTag).Title;
            string strTitle2=((MusicTag)item2.MusicTag).Title;
            if (bSortAsc)
            {
              return String.Compare(strTitle1 ,strTitle2,true);
            }
            else
            {
              return String.Compare(strTitle2 ,strTitle1,true);
            }
          }
        break;

        case SortMethod.SORT_ARTIST:
          if (item1.MusicTag!=null && item2.MusicTag!=null)
          {
            int ipos=0;
            string strArtist1=((MusicTag)item1.MusicTag).Artist;
            string strArtist2=((MusicTag)item2.MusicTag).Artist;
            if (bSortAsc)
            {
              ipos=String.Compare(strArtist1,strArtist2,true);
            }
            else
            {
              ipos=String.Compare(strArtist2,strArtist1,true);
            }
            if (ipos !=0) return ipos;

            string strAlbum1=((MusicTag)item1.MusicTag).Album;
            string strAlbum2=((MusicTag)item2.MusicTag).Album;
            if (bSortAsc)
            {
              ipos=String.Compare(strAlbum1,strAlbum2,true);
            }
            else
            {
              ipos=String.Compare(strAlbum2,strAlbum1,true);
            }
						if (ipos !=0) return ipos;
						string strTitle1=((MusicTag)item1.MusicTag).Title;
						string strTitle2=((MusicTag)item2.MusicTag).Title;
						if (bSortAsc)
						{
							ipos=String.Compare(strTitle1,strTitle2,true);
						}
						else
						{
							ipos=String.Compare(strTitle2,strTitle1,true);
						}
            return ipos;
          }
          break;
      } 
      return 0;
    }
    #endregion
	  public void DisplayAlbumsList(int searchKind,string strAlbum1)
	  {
		  ArrayList Albums=new ArrayList(); 
		  string strAlbum=strAlbum1;
		  m_database.GetAlbums(searchKind,strAlbum,ref Albums);
		  if (Albums.Count!=0)
		  {
			  GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST);
			  GUIControl.HideControl(GetID,(int)Controls.CONTROL_THUMBS);
			  GUIControl.HideControl(GetID,(int)Controls.CONTROL_BIGICONS);

			  GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
			  GUIControl.ClearControl(GetID,(int)Controls.CONTROL_THUMBS);
			  GUIControl.ClearControl(GetID,(int)Controls.CONTROL_BIGICONS);
            
			  ArrayList itemlist=new ArrayList();
			  foreach(AlbumInfo info in Albums)
			  {
				  GUIListItem item=new GUIListItem();
				  item.Label=info.Album;
				  item.Label2=info.Artist;
				  item.Path=info.Album;
				  item.IsFolder=true;
				  MusicTag tag=new MusicTag();
				  tag.Title=" ";
				  tag.Genre=info.Genre;
				  tag.Year=info.Year;
				  tag.Album=info.Album;
				  tag.Artist=info.Artist;
				  item.MusicTag=tag;
          
				  item.ThumbnailImage="defaultAlbum.png";
				  item.IconImageBig="defaultAlbum.png";
				  item.IconImage="defaultAlbum.png";

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
          if (tag!=null)
          {
            if (tag.Album.Length>0)
            {
              string strThumb=GUIMusicFiles.GetAlbumThumbName(tag.Artist,tag.Album);
              if (strThumb!=String.Empty && System.IO.File.Exists(strThumb) )
              {
                item.ThumbnailImage=strThumb;
                item.IconImageBig=strThumb;
                item.IconImage=strThumb;
              }
              else
              {
                strThumb = Utils.GetFolderThumb(item.Path);
                if (System.IO.File.Exists(strThumb))
                {
                  item.ThumbnailImage=strThumb;
                  item.IconImageBig=strThumb;
                  item.IconImage=strThumb;
                }
                else if (m_strDirectory.Length==0)
                {
                  ArrayList songs=new ArrayList();
                  m_database.GetSongsByAlbum(item.Path,ref songs);
                  foreach (Song song in songs)
                  {
                    strThumb = GUIMusicFiles.GetCoverArt(false,song.FileName,tag);
                    if (strThumb!=String.Empty)
                    {
                      item.ThumbnailImage=strThumb;
                      item.IconImageBig=strThumb;
                      item.IconImage=strThumb;
                      break;
                    }
                  }
                }
              }
            }
          }
        }
      
			  foreach (GUIListItem item in itemlist)
			  {
				  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
				  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
				  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_BIGICONS,item);
			  }
			  OnSort();

			  int iTotalItems=itemlist.Count;
			  if (itemlist.Count>0)
			  {
				  GUIListItem rootItem=(GUIListItem)itemlist[0];
				  if (rootItem.Label=="..") iTotalItems--;
			  }
			  string strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			  GUIPropertyManager.SetProperty("#itemcount",strObjects);
			  GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
			  GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABEL,m_strAlbum+" ");
			
			  ShowThumbPanel();
		  }
	}
		
    void OnClick(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      if (item.IsFolder)
      {
        m_iItemSelected=-1;
				if (item.MusicTag!=null)
				{
					MusicTag tag=(MusicTag)item.MusicTag;
					m_strAlbum=tag.Album;
				}
        LoadDirectory(item.Path);
      }
      else
      {
				// play item
				//play and add current directory to temporary playlist
				int nFolderCount=0;
				PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP ).Clear();
				PlayListPlayer.Reset();
				for ( int i = 0; i < (int) GetItemCount(); i++ ) 
				{
					GUIListItem pItem=GetItem(i);
					if ( pItem.IsFolder ) 
					{
						nFolderCount++;
						continue;
					}
          PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
					playlistItem.FileName=pItem.Path;
					playlistItem.Description=pItem.Label;
					int iDuration=0;
					MusicTag tag=(MusicTag)pItem.MusicTag;
					if (tag!=null) iDuration=tag.Duration;
					playlistItem.Duration=iDuration;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
				}

				//	Save current window and directory to know where the selected item was
				MusicState.TempPlaylistWindow=GetID;
				MusicState.TempPlaylistDirectory=m_strDirectory;

				PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
				PlayListPlayer.Play(iItem-nFolderCount);
			}
    }
    
    void OnQueueSelectedItem()
    {
      GUIListItem item=GetSelectedItem();
      if (item==null) return;

      if (m_strDirectory=="")
      {
        //	the albumname is needed to queue complete albums
        //	or CGUIWindowMusicBase::AddItemToPlaylist ()
        //	doesn't work for albums
        if (item.MusicTag!=null)
        {
          MusicTag tag=(MusicTag)item.MusicTag;
          OnQueueAlbum(tag.Album);
          int iItem=GetSelectedItemNo();
          //move to next item
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST,iItem+1);
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS,iItem+1);
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_BIGICONS,iItem+1);
        }
      }
      else
      {
        OnQueueItem( GetSelectedItemNo());
      }
    }

    void OnQueueItem(int iItem)
    {
      // add item 2 playlist
      GUIListItem pItem=GetItem(iItem);
      AddItemToPlayList(pItem);
	
      //move to next item
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST,iItem+1);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS,iItem+1);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_BIGICONS,iItem+1);

    }

    void AddItemToPlayList(GUIListItem pItem) 
    {
      if (pItem.IsFolder)
      {
        // recursive
        if (pItem.Label == "..") return;
        string strDirectory=m_strDirectory;
        m_strDirectory=pItem.Path;
		    
        ArrayList itemlist=m_directory.GetDirectory(m_strDirectory);
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
          PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
          playlistItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName=pItem.Path;
          playlistItem.Description=pItem.Label;
          playlistItem.Duration=pItem.Duration;
          PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
        }
      }
    }

    void LoadPlayList(string strPlayList)
    {
      PlayList playlist=PlayListFactory.Create(strPlayList);
      if (playlist==null) return;
      if (!playlist.Load(strPlayList))
      {
        GUIDialogOK dlgOK=(GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dlgOK!=null)
        {
          dlgOK.SetHeading(6);
          dlgOK.SetLine(1,477);
          dlgOK.SetLine(2,"");
          dlgOK.DoModal(GetID);
        }
        return;
      }

      if (playlist.Count==1)
      {
        g_Player.Play(playlist[0].FileName);
        return;
      }

      // clear current playlist
      PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Clear();

      // add each item of the playlist to the playlistplayer
      for (int i=0; i < playlist.Count; ++i)
      {
        PlayList.PlayListItem playListItem =playlist[i];
        PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playListItem);
      }

			
      // if we got a playlist
      if (PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Count >0)
      {
        // then get 1st song
        playlist=PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC );
        PlayList.PlayListItem item=playlist[0];

        // and start playing it
        PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
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
      m_iItemSelected=GetSelectedItemNo();
      int iSelectedItem=GetSelectedItemNo();
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress=(GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIListItem pItem=GetItem(iItem);

      string strPath="";
      if (pItem.IsFolder)
        strPath=pItem.Path;
      else
      {
        string strFileName;
        m_database.Split(pItem.Path, out strPath, out strFileName);
      }

      //	Try to find an album name for this item.
      //	Only save to database, if album name is found there.
      bool bSaveDb=false;
      string strAlbumName=pItem.Label;
      MusicTag tag=(MusicTag)(pItem.MusicTag);
      if (tag!=null)
      {
        if (tag.Album.Length>0) 
        {
          strAlbumName=tag.Album;
          bSaveDb=true;
        }
        else if (tag.Title.Length>0) 
        {
          strAlbumName=tag.Title;
          bSaveDb=true;
        }
      }

      MusicAlbumInfo infoTag = (MusicAlbumInfo)pItem.AlbumInfoTag;
      if (infoTag!=null)
      {
        //	Handle files
        AlbumInfo album=new AlbumInfo();
        string strAlbum=infoTag.Title;
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
          strAlbumName=strAlbum;*/
      }


      // check cache
      AlbumInfo albuminfo=new AlbumInfo();
      if ( m_database.GetAlbumInfo(strAlbumName, strPath, ref albuminfo) )
      {
        ArrayList songs=new ArrayList();
        MusicAlbumInfo album = new MusicAlbumInfo();
        album.Set(albuminfo);

        GUIMusicInfo pDlgAlbumInfo= (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
        if (null!=pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album=album;
          pDlgAlbumInfo.Tag=tag;

          pDlgAlbumInfo.DoModal(GetID);
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(strAlbumName);
            OnInfo(iItem);
          }
          return;
        }
      }

      // check internet connectivity
      if (null!=pDlgOK && !Util.Win32API.IsConnectedToInternet())
      {
        pDlgOK.SetHeading(703);
        pDlgOK.SetLine(1,703);
        pDlgOK.SetLine(2,"");
        pDlgOK.DoModal(GetID);
        return;
      }
      else if(!Util.Win32API.IsConnectedToInternet())
      {
        return;
      }

      // show dialog box indicating we're searching the album
      if (dlgProgress!=null)
      {
        dlgProgress.SetHeading(185);
        dlgProgress.SetLine(1,strAlbumName);
        dlgProgress.SetLine(2,"");
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      bool bDisplayErr=false;

      // find album info
      MusicInfoScraper scraper = new MusicInfoScraper();
      if (scraper.FindAlbuminfo(strAlbumName))
      {
        if (dlgProgress!=null) dlgProgress.Close();
        // did we found at least 1 album?
        int iAlbumCount=scraper.Count;
        if (iAlbumCount >=1)
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum=0;
          if (iAlbumCount > 1)
          {
            //show dialog with all albums found
            string szText=GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg= (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            if (null!=pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              for (int i=0; i < iAlbumCount; ++i)
              {
                MusicAlbumInfo info = scraper[i];
                pDlg.Add(info.Title2);
              }
              pDlg.DoModal(GetID);

              // and wait till user selects one
              iSelectedAlbum= pDlg.SelectedLabel;
              if (iSelectedAlbum< 0) return;
            }
          }

          // ok, now show dialog we're downloading the album info
          MusicAlbumInfo album = scraper[iSelectedAlbum];
          if (null!=dlgProgress) 
          {
            dlgProgress.SetHeading(185);
            dlgProgress.SetLine(1,album.Title2);
            dlgProgress.SetLine(2,"");
            dlgProgress.StartModal(GetID);
            dlgProgress.Progress();
          }

          // download the album info
          bool bLoaded=album.Loaded;
          if (!bLoaded) 
            bLoaded=album.Load();
          if ( bLoaded )
          {
            // set album title from musicinfotag, not the one we got from allmusic.com
            album.Title=strAlbumName;
            // set path, needed to store album in database
            album.AlbumPath=strPath;

            if (bSaveDb)
            {
              albuminfo=new AlbumInfo();
              albuminfo.Album  = album.Title;
              albuminfo.Artist = album.Artist;
              albuminfo.Genre  = album.Genre;
              albuminfo.Tones  = album.Tones;
              albuminfo.Styles = album.Styles;
              albuminfo.Review = album.Review;
              albuminfo.Image  = album.ImageURL;
              albuminfo.Tracks=album.Tracks;
              albuminfo.Rating   = album.Rating;
              try
              {
                albuminfo.Year 		= Int32.Parse( album.DateOfRelease);
              }
              catch (Exception)
              {
              }
              //albuminfo.Path   = album.AlbumPath;
              // save to database
              m_database.AddAlbumInfo(albuminfo);


             

            }
            if (null!=dlgProgress) 
              dlgProgress.Close();

            // ok, show album info
            GUIMusicInfo pDlgAlbumInfo= (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
            if (null!=pDlgAlbumInfo)
            {
              pDlgAlbumInfo.Album=album;
              pDlgAlbumInfo.Tag=tag;

              pDlgAlbumInfo.DoModal(GetID);
              if (pDlgAlbumInfo.NeedsRefresh)
              {
                m_database.DeleteAlbumInfo(album.Title);
                OnInfo(iItem);
                return;
              }
            }
          }
          else
          {
            // failed 2 download album info
            bDisplayErr=true;
          }
        }
        else 
        {
          // no albums found
          bDisplayErr=true;
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr=true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null!=dlgProgress) 
          dlgProgress.Close();
        if (null!=pDlgOK)
        {
          pDlgOK.SetHeading(187);
          pDlgOK.SetLine(1,187);
          pDlgOK.SetLine(2,"");
          pDlgOK.DoModal(GetID);
        }
      }
    }
    void OnQueueAlbum(string strAlbum)
    {
      if (strAlbum==null) return;
      if (strAlbum=="") return;
      ArrayList albums=new ArrayList();
      m_database.GetSongsByAlbum(strAlbum, ref albums);
      foreach (Song song in albums)
      {
        PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
        playlistItem.FileName=song.FileName;
        playlistItem.Description=song.Title;
        playlistItem.Duration=song.Duration;
        playlistItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
        PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
      }
      
      if (!g_Player.Playing)
      {
        PlayListPlayer.CurrentPlaylist =PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Play(0);
      }
    }
    
    void GetStringFromKeyboard( ref string strLine)
    {
      VirtualKeyboard keyboard=(VirtualKeyboard)GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null==keyboard) return;
      keyboard.Reset();
      keyboard.Text=strLine;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine=keyboard.Text;
      }
    }

  }
}
