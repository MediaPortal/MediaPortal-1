using System;
using System.Collections;
using System.Net;
using System.Globalization;
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
  public class GUIMusicGenres: GUIWindow, IComparer
  {
    enum Controls
    {
      CONTROL_BTNVIEWASICONS=		2,
      CONTROL_BTNSORTBY		=			3,
      CONTROL_BTNSORTASC	=			4,
      CONTROL_BTNTYPE    =      6,
      CONTROL_BTNPLAYLISTS=     7,
      CONTROL_BTNSCAN     =     9,
      CONTROL_BTNREC      =     10,
      CONTROL_LIST				=			50,
      CONTROL_THUMBS			=			51,
      CONTROL_LABELFILES  =       12,
			CONTROL_LABEL=       15,
		CONTROL_SEARCH = 8

    };
    #region Base variabeles
    enum SortMethod
    {
      SORT_NAME=0,
      SORT_DATE=1,
      SORT_SIZE=2,
      SORT_TRACK=3,
      SORT_DURATION=4,
      SORT_TITLE=5,
      SORT_ARTIST=6,
      SORT_ALBUM=7,
      SORT_FILENAME=8
    }

    enum View
    {
      VIEW_AS_LIST    =       0,
      VIEW_AS_ICONS    =      1,
      VIEW_AS_LARGEICONS  =   2,
    }
    View              currentView=View.VIEW_AS_LIST;
    SortMethod        currentSortMethod=SortMethod.SORT_TITLE;
    View              currentViewRoot=View.VIEW_AS_LIST;
    SortMethod        currentSortMethodRoot=SortMethod.SORT_NAME;
    bool              m_bSortAscending=true;
    bool              m_bSortAscendingRoot=true;

    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory="";
    int               m_iItemSelected=-1;   
    VirtualDirectory  m_directory = new VirtualDirectory();
    #endregion
    Database		      m_database = new Database();
    public GUIMusicGenres()
    {
      GetID=(int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.AudioExtensions);
      LoadSettings();
    }
    ~GUIMusicGenres()
    {
    }

    public override bool Init()
    {
      m_strDirectory="";
      return Load (GUIGraphicsContext.Skin+@"\mymusicgenres.xml");
    }

    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp="";
        strTmp=(string)xmlreader.GetValue("musicgenre","viewby");
        if (strTmp!=null)
        {
          if (strTmp=="list") currentView=View.VIEW_AS_LIST;
          else if (strTmp=="icons") currentView=View.VIEW_AS_ICONS;
          else if (strTmp=="largeicons") currentView=View.VIEW_AS_LARGEICONS;
        }
        strTmp=(string)xmlreader.GetValue("musicgenre","viewbyroot");
        if (strTmp!=null)
        {
          if (strTmp=="list") currentViewRoot=View.VIEW_AS_LIST;
          else if (strTmp=="icons") currentViewRoot=View.VIEW_AS_ICONS;
          else if (strTmp=="largeicons") currentViewRoot=View.VIEW_AS_LARGEICONS;
        }
        strTmp=(string)xmlreader.GetValue("musicgenre","sort");
        if (strTmp!=null)
        {
          if (strTmp=="album") currentSortMethod=SortMethod.SORT_ALBUM;
          else if (strTmp=="artist") currentSortMethod=SortMethod.SORT_ARTIST;
          else if (strTmp=="name") currentSortMethod=SortMethod.SORT_NAME;
          else if (strTmp=="date") currentSortMethod=SortMethod.SORT_DATE;
          else if (strTmp=="size") currentSortMethod=SortMethod.SORT_SIZE;
          else if (strTmp=="track") currentSortMethod=SortMethod.SORT_TRACK;
          else if (strTmp=="duration") currentSortMethod=SortMethod.SORT_DURATION;
          else if (strTmp=="filename") currentSortMethod=SortMethod.SORT_FILENAME;
          else if (strTmp=="title") currentSortMethod=SortMethod.SORT_TITLE;
        }
        strTmp=(string)xmlreader.GetValue("musicgenre","sortroot");
        if (strTmp!=null)
        {
          if (strTmp=="album") currentSortMethodRoot=SortMethod.SORT_ALBUM;
          else if (strTmp=="artist") currentSortMethodRoot=SortMethod.SORT_ARTIST;
          else if (strTmp=="name") currentSortMethodRoot=SortMethod.SORT_NAME;
          else if (strTmp=="date") currentSortMethodRoot=SortMethod.SORT_DATE;
          else if (strTmp=="size") currentSortMethodRoot=SortMethod.SORT_SIZE;
          else if (strTmp=="track") currentSortMethodRoot=SortMethod.SORT_TRACK;
          else if (strTmp=="duration") currentSortMethodRoot=SortMethod.SORT_DURATION;
          else if (strTmp=="filename") currentSortMethodRoot=SortMethod.SORT_FILENAME;
          else if (strTmp=="title") currentSortMethodRoot=SortMethod.SORT_TITLE;
        }
        m_bSortAscending=xmlreader.GetValueAsBool("musicgenre","sortascending",true);
        m_bSortAscendingRoot=xmlreader.GetValueAsBool("musicgenre","sortascendingroot",true);
      }
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentView)
        {
          case View.VIEW_AS_LIST: 
            xmlwriter.SetValue("musicgenre","viewby","list");
            break;
          case View.VIEW_AS_ICONS: 
            xmlwriter.SetValue("musicgenre","viewby","icons");
            break;
          case View.VIEW_AS_LARGEICONS: 
            xmlwriter.SetValue("musicgenre","viewby","largeicons");
            break;
        }
        switch (currentViewRoot)
        {
          case View.VIEW_AS_LIST: 
            xmlwriter.SetValue("musicgenre","viewbyroot","list");
            break;
          case View.VIEW_AS_ICONS: 
            xmlwriter.SetValue("musicgenre","viewbyroot","icons");
            break;
          case View.VIEW_AS_LARGEICONS: 
            xmlwriter.SetValue("musicgenre","viewbyroot","largeicons");
            break;
        }
        switch (currentSortMethod)
        {
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("musicgenre","sort","name");
            break;
          case SortMethod.SORT_DATE:
            xmlwriter.SetValue("musicgenre","sort","date");
            break;
          case SortMethod.SORT_SIZE:
            xmlwriter.SetValue("musicgenre","sort","size");
            break;
          case SortMethod.SORT_TRACK:
            xmlwriter.SetValue("musicgenre","sort","track");
            break;
          case SortMethod.SORT_DURATION:
            xmlwriter.SetValue("musicgenre","sort","duration");
            break;
          case SortMethod.SORT_TITLE:
            xmlwriter.SetValue("musicgenre","sort","title");
            break;
          case SortMethod.SORT_ALBUM:
            xmlwriter.SetValue("musicgenre","sort","album");
            break;
          case SortMethod.SORT_ARTIST:
            xmlwriter.SetValue("musicgenre","sort","artist");
            break;
          case SortMethod.SORT_FILENAME:
            xmlwriter.SetValue("musicgenre","sort","filename");
            break;
        }
        switch (currentSortMethodRoot)
        {
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("musicgenre","sortroot","name");
            break;
          case SortMethod.SORT_DATE:
            xmlwriter.SetValue("musicgenre","sortroot","date");
            break;
          case SortMethod.SORT_SIZE:
            xmlwriter.SetValue("musicgenre","sortroot","size");
            break;
          case SortMethod.SORT_TRACK:
            xmlwriter.SetValue("musicgenre","sortroot","track");
            break;
          case SortMethod.SORT_DURATION:
            xmlwriter.SetValue("musicgenre","sortroot","duration");
            break;
          case SortMethod.SORT_TITLE:
            xmlwriter.SetValue("musicgenre","sortroot","title");
            break;
          case SortMethod.SORT_ALBUM:
            xmlwriter.SetValue("musicgenre","sortroot","album");
            break;
          case SortMethod.SORT_ARTIST:
            xmlwriter.SetValue("musicgenre","sortroot","artist");
            break;
          case SortMethod.SORT_FILENAME:
            xmlwriter.SetValue("musicgenre","sortroot","filename");
            break;
        }

        xmlwriter.SetValueAsBool("musicgenre","sortascending",m_bSortAscending);
        xmlwriter.SetValueAsBool("musicgenre","sortascendingroot",m_bSortAscendingRoot);
      }
    }
    #endregion

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
					m_database.Open();
					LoadSettings();
          
          ShowThumbPanel();
          LoadDirectory(m_strDirectory);
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          m_iItemSelected=GetSelectedItemNo();
          m_database.Close();
          SaveSettings();
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
            ShowThumbPanel();
            GUIControl.FocusControl(GetID,iControl);
          }
          
          if (iControl==(int)Controls.CONTROL_BTNSORTASC)
          {
            if (m_strDirectory=="")
              m_bSortAscendingRoot=!m_bSortAscendingRoot;
            else
              m_bSortAscending=!m_bSortAscending;
            OnSort();
            UpdateButtons();
            GUIControl.FocusControl(GetID,iControl);
          }


          if (iControl==(int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            if (m_strDirectory=="")
            {
                currentSortMethodRoot=SortMethod.SORT_NAME;
            }
            else
            {
              switch (currentSortMethod)
              {
                case SortMethod.SORT_NAME:
                  currentSortMethod=SortMethod.SORT_DATE;
                  break;
                case SortMethod.SORT_DATE:
                  currentSortMethod=SortMethod.SORT_SIZE;
                  break;
                case SortMethod.SORT_SIZE:
                  currentSortMethod=SortMethod.SORT_TRACK;
                  break;
                case SortMethod.SORT_TRACK:
                  currentSortMethod=SortMethod.SORT_DURATION;
                  break;
                case SortMethod.SORT_DURATION:
                  currentSortMethod=SortMethod.SORT_TITLE;
                  break;
                case SortMethod.SORT_TITLE:
                  currentSortMethod=SortMethod.SORT_ALBUM;
                  break;
                case SortMethod.SORT_ARTIST:
                  currentSortMethod=SortMethod.SORT_ALBUM;
                  break;
                case SortMethod.SORT_ALBUM:
                  currentSortMethod=SortMethod.SORT_FILENAME;
                  break;
                case SortMethod.SORT_FILENAME:
                  currentSortMethod=SortMethod.SORT_NAME;
                  break;
              }
            }
            OnSort();
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
            int nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
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
            }

            if (nNewWindow!=GetID)
            {
              MusicState.StartWindow=nNewWindow;
              GUIWindowManager.ActivateWindow(nNewWindow);
            }

            return true;
          }
          if (iControl==(int)Controls.CONTROL_THUMBS||iControl==(int)Controls.CONTROL_LIST)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            int iItem=(int)msg.Param1;
            int iAction=(int)message.Param1;
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
          break;
      }
      return base.OnMessage(message);
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

		void SetLabels()
		{
      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;
      if (m_strDirectory=="")
      {
        method=currentSortMethodRoot;
        bAscending=m_bSortAscendingRoot;
      }

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
            if (method==SortMethod.SORT_ALBUM)
            {
              if (tag.Album.Length>0 && tag.Title.Length>0)
              {
                item.Label=String.Format("{0} - {1}", tag.Album,tag.Title);
              }
            }
          }
        }
        
        
        if (method==SortMethod.SORT_SIZE||method==SortMethod.SORT_FILENAME)
        {
          if (item.IsFolder) item.Label2="";
          else
          {
            if (item.Size>0)
            {
              item.Label2=Utils.GetSize( item.Size);
            }
            if (method==SortMethod.SORT_FILENAME)
            {
              item.Label=Utils.GetFilename(item.Path);
            }
          }
        }
        else if (method==SortMethod.SORT_DATE)
        {
          if (item.FileInfo!=null)
          {
            item.Label2 =item.FileInfo.CreationTime.ToShortDateString() + " "+item.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          }
        }
        else
        {
          item.Label2 = "";
          if (tag!=null)
          {
            int nDuration=tag.Duration;
            if (nDuration>0)
            {
              item.Label2=Utils.SecondsToHMSString(nDuration);
            }
          }
        }
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
      OnMessage(msg);         
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
      GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_BTNTYPE,MusicState.StartWindow-(int)GUIWindow.Window.WINDOW_MUSIC_FILES);

      GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST);
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_THUMBS);
      
      int iControl=(int)Controls.CONTROL_LIST;
      if (ViewByIcon)
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

      SortMethod sortmethod=currentSortMethod;
      if (m_strDirectory=="")
        sortmethod=currentSortMethodRoot;
      switch (sortmethod)
      {
        case SortMethod.SORT_NAME:
          strLine=GUILocalizeStrings.Get(103);
          break;
        case SortMethod.SORT_DATE:
          strLine=GUILocalizeStrings.Get(104);
          break;
        case SortMethod.SORT_SIZE:
          strLine=GUILocalizeStrings.Get(105);
          break;
        case SortMethod.SORT_TRACK:
          strLine=GUILocalizeStrings.Get(266);
          break;
        case SortMethod.SORT_DURATION:
          strLine=GUILocalizeStrings.Get(267);
          break;
        case SortMethod.SORT_TITLE:
          strLine=GUILocalizeStrings.Get(268);
          break;
        case SortMethod.SORT_ARTIST:
          strLine=GUILocalizeStrings.Get(269);
          break;
        case SortMethod.SORT_ALBUM:
          strLine=GUILocalizeStrings.Get(270);
          break;
        case SortMethod.SORT_FILENAME:
          strLine=GUILocalizeStrings.Get(363);
          break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY,strLine);

      bool bAsc=m_bSortAscending;
      if (m_strDirectory=="")
        bAsc=m_bSortAscendingRoot;
      if (bAsc)
        GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);

        GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BTNSORTBY);
        GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
    }
	  void keyboard_TextChanged(int kindOfSearch,string data)
	  {
		  DisplayGeneresList(kindOfSearch,data);
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

    void OnRetrieveCoverArt(GUIListItem item)
    {
      if (m_strDirectory.Length==0)
      {
        string strThumb=Utils.GetCoverArt(GUIMusicFiles.GenreThumbsFolder,item.Label);
        item.IconImage=strThumb;
        item.IconImageBig=strThumb;
        item.ThumbnailImage=strThumb;
        Utils.SetDefaultIcons(item);
      }
      else
      {
        Utils.SetDefaultIcons(item);
        string strThumb=GUIMusicFiles.GetCoverArt(item.IsFolder,item.Path,item.MusicTag as MusicTag);
        if (strThumb!=String.Empty)
        {
          item.ThumbnailImage=strThumb;
          item.IconImageBig=strThumb;
          item.IconImage=strThumb;
        }
      }
    }

    void LoadDirectory(string strNewDirectory)
    {
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
            
      string strObjects="";

			ArrayList itemlist=new ArrayList();
			if (m_strDirectory.Length==0)
			{
				ArrayList genres=new ArrayList();
				m_database.GetGenres(ref genres);
				foreach(string strGenre in genres)
				{
					GUIListItem item=new GUIListItem();
					item.Label=strGenre;
					item.Path=strGenre;
          item.IsFolder=true;
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
				itemlist.Add(pItem);

				ArrayList songs=new ArrayList();
				m_database.GetSongsByGenre(m_strDirectory,ref songs);
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
					item.MusicTag=tag;
          item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
		
					itemlist.Add(item);
				}
			}
     

      
      string strSelectedItem=m_history.Get(m_strDirectory);	
      int iItem=0;
      foreach (GUIListItem item in itemlist)
      {
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
      }

      int iTotalItems=itemlist.Count;
      if (itemlist.Count>0)
      {
        GUIListItem rootItem=(GUIListItem)itemlist[0];
        if (rootItem.Label=="..") iTotalItems--;
      }
      strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
			
			GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABEL,m_strDirectory);
			SetLabels();
      ShowThumbPanel();
      OnSort();
      for (int i=0; i< GetItemCount();++i)
      {
        GUIListItem item =GetItem(i);
        if (item.Label==strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LIST,iItem);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_THUMBS,iItem);
          break;
        }
        iItem++;
      }
      if (m_iItemSelected>=0)
      {
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LIST,m_iItemSelected);
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_THUMBS,m_iItemSelected);
      }
    }
	  
	  void DisplayGeneresList(int searchKind,string searchText)
	  {
		  GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
		  GUIControl.ClearControl(GetID,(int)Controls.CONTROL_THUMBS);
            
		  string strObjects="";

		
		  ArrayList itemlist=new ArrayList();
		  ArrayList genres=new ArrayList();
		  m_database.GetGenres(searchKind,searchText,ref genres);
			  foreach(string strGenre in genres)
			  {
				  GUIListItem item=new GUIListItem();
				  item.Label=strGenre;
				  item.Path=strGenre;
				  item.IsFolder=true;
				  string strThumb=Utils.GetCoverArt(GUIMusicFiles.GenreThumbsFolder,item.Label);
				  item.IconImage=strThumb;
				  item.IconImageBig=strThumb;
				  item.ThumbnailImage=strThumb;

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
			  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
			  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
		  }

		  int iTotalItems=itemlist.Count;
		  if (itemlist.Count>0)
		  {
			  GUIListItem rootItem=(GUIListItem)itemlist[0];
			  if (rootItem.Label=="..") iTotalItems--;
		  }
		  strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
		  GUIPropertyManager.SetProperty("#itemcount",strObjects);
		  GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
		  GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABEL,m_strDirectory);
		  SetLabels();
		  ShowThumbPanel();
		  OnSort();

	  }

	  #endregion


		
    void OnClick(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      if (item.IsFolder)
      {
        m_iItemSelected=-1;
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
			dlg.Add( GUILocalizeStrings.Get(929)); //close window

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
      }
    }

    void OnQueueGenre(string strGenre)
    {
      if (strGenre==null) return;
      if (strGenre=="") return;
      ArrayList albums=new ArrayList();
      m_database.GetSongsByGenre(strGenre, ref albums);
      foreach (Song song in albums)
      {
        PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
        playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
        playlistItem.FileName=song.FileName;
        playlistItem.Description=song.Title;
        playlistItem.Duration=song.Duration;
        PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
      }
      
      if (!g_Player.Playing)
      {
        PlayListPlayer.CurrentPlaylist =PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Play(0);
      }
    }

    void OnQueueItem(int iItem)
    {
      // add item 2 playlist
      GUIListItem pItem=GetItem(iItem);
      if (m_strDirectory=="")
      {
        string strArtist=pItem.Label;
        OnQueueGenre(strArtist);
      }
      else
      {
        AddItemToPlayList(pItem);
      }
      //move to next item
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST,iItem+1);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS,iItem+1);

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
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
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
		

    #region Sort Members
    void OnSort()
    {
      SortMethod method=currentSortMethod;

      SetLabels();
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

      string strSize1="";
      string strSize2="";
      if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);

      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;
      if (m_strDirectory=="")
      {
        method=currentSortMethodRoot;
        bAscending=m_bSortAscendingRoot;
      }
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
        

        case SortMethod.SORT_DATE:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          
          item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          if (bAscending)
          {
            return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
          }

        case SortMethod.SORT_SIZE:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          if (bAscending)
          {
            return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
          }
          else
          {
            return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
          }

        case SortMethod.SORT_TRACK:
          int iTrack1=0;
          int iTrack2=0;
          if (item1.MusicTag!=null) iTrack1=((MusicTag)item1.MusicTag).Track;
          if (item2.MusicTag!=null) iTrack2=((MusicTag)item2.MusicTag).Track;
          if (bAscending)
          {
            return (int)(iTrack1 - iTrack2);
          }
          else
          {
            return (int)(iTrack2 - iTrack1);
          }
          
        case SortMethod.SORT_DURATION:
          int iDuration1=0;
          int iDuration2=0;
          if (item1.MusicTag!=null) iDuration1=((MusicTag)item1.MusicTag).Duration;
          if (item2.MusicTag!=null) iDuration2=((MusicTag)item2.MusicTag).Duration;
          if (bAscending)
          {
            return (int)(iDuration1 - iDuration2);
          }
          else
          {
            return (int)(iDuration2 - iDuration1);
          }
          
        case SortMethod.SORT_TITLE:
          string strTitle1=item1.Label;
          string strTitle2=item2.Label;
          if (item1.MusicTag!=null) strTitle1=((MusicTag)item1.MusicTag).Title;
          if (item2.MusicTag!=null) strTitle2=((MusicTag)item2.MusicTag).Title;
          if (bAscending)
          {
            return String.Compare(strTitle1 ,strTitle2,true);
          }
          else
          {
            return String.Compare(strTitle2 ,strTitle1,true);
          }
        
        case SortMethod.SORT_ARTIST:
          string strArtist1="";
          string strArtist2="";
          if (item1.MusicTag!=null) strArtist1=((MusicTag)item1.MusicTag).Artist;
          if (item2.MusicTag!=null) strArtist2=((MusicTag)item2.MusicTag).Artist;
          if (bAscending)
          {
            return String.Compare(strArtist1 ,strArtist2,true);
          }
          else
          {
            return String.Compare(strArtist2 ,strArtist1,true);
          }
        
        case SortMethod.SORT_ALBUM:
          string strAlbum1="";
          string strAlbum2="";
          if (item1.MusicTag!=null) strAlbum1=((MusicTag)item1.MusicTag).Album;
          if (item2.MusicTag!=null) strAlbum2=((MusicTag)item2.MusicTag).Album;
          if (bAscending)
          {
            return String.Compare(strAlbum1 ,strAlbum2,true);
          }
          else
          {
            return String.Compare(strAlbum2 ,strAlbum1,true);
          }
          

        case SortMethod.SORT_FILENAME:
          string strFile1=System.IO.Path.GetFileName(item1.Path);
          string strFile2=System.IO.Path.GetFileName(item2.Path);
          if (bAscending)
          {
            return String.Compare(strFile1 ,strFile2,true);
          }
          else
          {
            return String.Compare(strFile2 ,strFile1,true);
          }
          
      } 
      return 0;
    }
    #endregion

    
    void OnInfo(int iItem)
    {
    }
  }
}
