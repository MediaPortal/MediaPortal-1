using System;
using System.Collections;
using System.Net;
using System.Globalization;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Radio.Database;
using MediaPortal.Playlists;

namespace MediaPortal.GUI.Radio
{
  /// <summary>
  /// controls en-/disablen 
  ///   - player (wel of niet) played radio/tv/music/video's...
  ///   - buddy enable/disable
  ///   -
  /// -keuzes aan select button  (sort by ....)
  /// -root/sub view->generiek maken
  /// -class welke de list/thumbnail view combinatie doet
  /// 
  /// </summary>
  public class GUIRadio: GUIWindow, IComparer,ISetupForm
  {
    enum Controls
    {
      CONTROL_BTNVIEWASICONS=		2,
      CONTROL_BTNSORTBY		=			3,
      CONTROL_BTNSORTASC	=			4,
      CONTROL_BTN_PREV    =     6,
      CONTROL_BTN_NEXT    =     7,
			
      CONTROL_LIST				=			50,
      CONTROL_THUMBS			=			51,
      CONTROL_LABELFILES  =       12,
      CONTROL_EJECT  =       13

    };
    #region Base variabeles
    enum SortMethod
    {
      SORT_NAME=0,
      SORT_TYPE=1,
      SORT_GENRE=2,
      SORT_BITRATE=3,
      SORT_NUMBER
    }

    enum View:int
    {
      VIEW_AS_LIST    =       0,
      VIEW_AS_ICONS    =      1,
      VIEW_AS_LARGEICONS  =   2,
    }
    const string      ThumbsFolder=@"Thumbs\Radio";
    View              currentView=View.VIEW_AS_LIST;
    SortMethod        currentSortMethod=SortMethod.SORT_NAME;
    bool              m_bSortAscending=true;
    VirtualDirectory	m_directory = new VirtualDirectory();
    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory="";
		string            m_strRadioFolder="";
    int               m_iItemSelected=-1;   
		PlayList          m_Playlist=null;
    #endregion

    public GUIRadio()
    {
      GetID=(int)GUIWindow.Window.WINDOW_RADIO;
      
      LoadSettings();
    }

    ~GUIRadio()
    {
    }

    
    public override bool Init()
    {
      m_strDirectory="";
      bool bResult=Load (GUIGraphicsContext.Skin+@"\MyRadio.xml");
			try
			{
				System.IO.Directory.CreateDirectory(ThumbsFolder);
			}
			catch(Exception){}
      return bResult;
    }


    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
				m_strRadioFolder=xmlreader.GetValueAsString("radio","folder","");

        string strTmp="";
        strTmp=(string)xmlreader.GetValue("myradio","viewby");
        if (strTmp!=null)
        {
          if (strTmp=="list") currentView=View.VIEW_AS_LIST;
          else if (strTmp=="icons") currentView=View.VIEW_AS_ICONS;
          else if (strTmp=="largeicons") currentView=View.VIEW_AS_LARGEICONS;
        }
        
        strTmp=(string)xmlreader.GetValue("myradio","sort");
        if (strTmp!=null)
        {
          if (strTmp=="name") currentSortMethod=SortMethod.SORT_NAME;
          else if (strTmp=="type") currentSortMethod=SortMethod.SORT_TYPE;
          else if (strTmp=="genre") currentSortMethod=SortMethod.SORT_GENRE;
          else if (strTmp=="bitrate") currentSortMethod=SortMethod.SORT_BITRATE;
          else if (strTmp=="number") currentSortMethod=SortMethod.SORT_NUMBER;
        }

        m_bSortAscending=xmlreader.GetValueAsBool("myradio","sortascending",true);

      }
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentView)
        {
          case View.VIEW_AS_LIST: 
            xmlwriter.SetValue("myradio","viewby","list");
            break;
          case View.VIEW_AS_ICONS: 
            xmlwriter.SetValue("myradio","viewby","icons");
            break;
          case View.VIEW_AS_LARGEICONS: 
            xmlwriter.SetValue("myradio","viewby","largeicons");
            break;
        }
        
        switch (currentSortMethod)
        {
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("myradio","sort","name");
            break;
          case SortMethod.SORT_TYPE:
            xmlwriter.SetValue("myradio","sort","type");
            break;
          case SortMethod.SORT_GENRE:
            xmlwriter.SetValue("myradio","sort","genre");
            break;
          case SortMethod.SORT_BITRATE:
            xmlwriter.SetValue("myradio","sort","bitrate");
            break;
          case SortMethod.SORT_NUMBER:
            xmlwriter.SetValue("myradio","sort","number");
            break;
        }

        xmlwriter.SetValueAsBool("myradio","sortascending",m_bSortAscending);
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

      base.OnAction(action);
    }

		
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
					LoadSettings();

					m_Playlist=null;
					m_directory = new VirtualDirectory();
					Share share = new Share("default",m_strRadioFolder);
					share.Default=true;
					m_directory.Add(share);
					m_directory.AddExtension(".pls");
					m_directory.AddExtension(".asx");
          
          ShowThumbPanel();
          LoadDirectory(m_strDirectory);
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          m_iItemSelected=GetSelectedItemNo();
          SaveSettings();
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
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
            OnMessage(msg);         
            int iItem=(int)msg.Param1;
            int iAction=(int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
          }
          break;

          case GUIMessage.MessageType.GUI_MSG_PLAY_RADIO_STATION:
            if (message.Label.Length==0) return true;
            PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
            PlayListPlayer.Reset();

            ArrayList stations = new ArrayList();
            RadioDatabase.GetStations(ref stations);
            foreach (RadioStation station in stations)
            {
              PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
              if (station.URL==String.Empty)
                playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Radio;
              else
                playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.AudioStream;
              playlistItem.FileName = GetPlayPath(station);
              playlistItem.Description = station.Name;
              playlistItem.Duration = 0;
              PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
            }
            PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
            foreach (RadioStation station in stations)
            {
              if ( station.Name.Equals(message.Label))
              {
                PlayListPlayer.Play( GetPlayPath(station));
                return true;
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
			int iCount=0;

			if (m_Playlist!=null)
			{
				GUIListItem item = new GUIListItem();
				item.Label="..";
				item.Path=m_strDirectory;
				item.IsFolder=true;
				item.MusicTag = null;
				item.ThumbnailImage="";
				Utils.SetDefaultIcons(item);
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);

				for (int i=0; i < m_Playlist.Count;++i)
				{
					item = new GUIListItem();
					item.Label=m_Playlist[i].Description;
					item.Path=m_Playlist[i].FileName;
					item.IsFolder=false;
					item.MusicTag = null;
					item.ThumbnailImage="";
					item.IconImageBig="DefaultMyradioStreamBig.png";
					item.IconImage="DefaultMyradioStream.png";

					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
					iCount++;
				}
			}
			else
			{
				if (m_strDirectory.Length==0 || m_strDirectory.Equals(m_strRadioFolder))
				{
					ArrayList stations = new ArrayList();
					RadioDatabase.GetStations(ref stations);
					foreach (RadioStation station in stations)
					{
						GUIListItem item = new GUIListItem();
						item.Label=station.Name;
						item.IsFolder=false;
						item.MusicTag = station;
            if (station.URL.Length>5)
            {
              item.IconImageBig="DefaultMyradioStreamBig.png";
              item.IconImage="DefaultMyradioStream.png";
              item.ThumbnailImage=Utils.GetCoverArt(ThumbsFolder,station.Name);
              //if (item.ThumbnailImage=="")
              //  item.ThumbnailImage="DefaultMyradioStream.png";
            }
            else
            {
              item.IconImageBig="DefaultMyradioBig.png";
              item.IconImage="DefaultMyradio.png";
              item.ThumbnailImage=Utils.GetCoverArt(ThumbsFolder,station.Name);
              //if (item.ThumbnailImage=="")
              //  item.ThumbnailImage="DefaultMyradioBig.png";
            }

						GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
						GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
						iCount++;
					}
				}

				if (m_strRadioFolder.Length!=0)
				{
					string strFolder=m_strDirectory;
					if (strFolder.Length==0) strFolder=m_strRadioFolder;
					ArrayList items = new ArrayList();
					items=m_directory.GetDirectory(strFolder);
					foreach (GUIListItem item in items)
					{
            if (!item.IsFolder)
            {
              item.MusicTag = null;
              //item.ThumbnailImage="DefaultMyradioStream.png";
              item.IconImageBig="DefaultMyradioStreamBig.png";
              item.IconImage="DefaultMyradioStream.png";
            }
            else
            {
              if (item.Label.Equals(".."))
              {
                if (m_strDirectory.Length==0 || m_strDirectory.Equals(m_strRadioFolder)) continue;
              }
            }

						GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
						GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
						iCount++;
					}
				}
			}

      OnSort();
      strObjects=String.Format("{0} {1}", iCount, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
      ShowThumbPanel();
      
      if (m_iItemSelected>=0)
      {
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LIST,m_iItemSelected);
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_THUMBS,m_iItemSelected);
      }

    }
    #endregion

    void SetLabels()
    {
      SortMethod method=currentSortMethod;

      for (int i=0; i < GetItemCount();++i)
      {
        GUIListItem item=GetItem(i);
				if (item.MusicTag!=null)
				{
					RadioStation station=(RadioStation) item.MusicTag;
          if (method==SortMethod.SORT_BITRATE) 
          {
            if (station.BitRate>0)
              item.Label2=station.BitRate.ToString();
            else
            {
              double dFreq=station.Frequency;
              dFreq/=1000000d;
              item.Label2=System.String.Format("{0:###.##} MHz.", dFreq);
            }
          }
          else item.Label2=station.Genre;
				}
      }
    }


    #region Sort Members
    void OnSort()
    {
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


      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;
      RadioStation station1 = item1.MusicTag as RadioStation ;
      RadioStation station2 = item2.MusicTag as RadioStation ;
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
        
				case SortMethod.SORT_TYPE:
					string strURL1="";
					string strURL2="";
          if (station1!=null) strURL1=station1.URL;
          else 
          {
            if ( item1.IconImage.ToLower().Equals("defaultmyradiostream.png") )
              strURL1="1";
          }

					if (station2!=null) strURL2=station2.URL;
          else 
          {
            if ( item2.IconImage.ToLower().Equals("defaultmyradiostream.png") )
              strURL2="1";
          }

          if (strURL1.Equals(strURL2))
          {
            if (bAscending)
            {
              return String.Compare(item1.Label ,item2.Label,true);
            }
            else
            {
              return String.Compare(item2.Label ,item1.Label,true);
            }
          }
					if (bAscending)
					{
						if (strURL1.Length>0) return 1;
						else return -1;
					}
					else
					{
						if (strURL1.Length>0) return -1;
						else return 1;
					}
				//break;

				case SortMethod.SORT_GENRE:
					if (station1!=null && station2!=null)
					{
						if (station1.Genre.Equals(station2.Genre))
							goto case SortMethod.SORT_BITRATE;
						if (bAscending)
						{
							return String.Compare(station1.Genre,station2.Genre,true);
						}
						else
						{
							return String.Compare(station2.Genre,station1.Genre,true);
						}
					}
					else
					{
						return 0;
					}
				//break;
          
        case SortMethod.SORT_NUMBER:
          if (station1!=null && station2!=null)
          {
            if (bAscending)
            {
              if (station1.Channel>station2.Channel) return 1;
              else return -1;
            }
            else
            {
              if (station2.Channel>station1.Channel) return 1;
              else return -1;
            }
          }
          
          if (station1!=null) return -1;
          if (station2!=null) return 1;
          return 0;
        //break;
        case SortMethod.SORT_BITRATE:
					if (station1!=null && station2!=null)
					{
						if (bAscending)
						{
							if (station1.BitRate>station2.BitRate) return 1;
							else return -1;
						}
						else
						{
							if (station2.BitRate>station1.BitRate) return 1;
							else return -1;
						}
					}
					return 0;
      }
      return 0;
    }
    #endregion

		
    void OnClick(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      if (item.IsFolder)
      {
        if (m_Playlist!=null)
        {
          m_Playlist=null;
        }
        m_iItemSelected=-1;
        LoadDirectory(item.Path);
      }
      else
      {
        Play(item);
      }
    }

    void FillPlayList()
    {
      PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
      PlayListPlayer.Reset();

      // are we looking @ a playlist
      if (m_Playlist!=null)
      {
        //yes, then add current playlist to playlist player
        for (int i=0; i < m_Playlist.Count;++i)
        {
          PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
          playlistItem.Type = m_Playlist[i].Type;
          playlistItem.FileName = m_Playlist[i].FileName;
          playlistItem.Description = m_Playlist[i].Description;
          playlistItem.Duration = m_Playlist[i].Duration;
          PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
        }
      }
      else
      {
        //add current directory to playlist player
        for (int i=0; i < GetItemCount();++i)
        {
          GUIListItem item = GetItem(i);
          if (item.IsFolder) continue;

          // if item is a playlist
          if (Utils.IsPlayList(item.Path))
          {
            // then load the playlist
            PlayList playlist = PlayListFactory.Create(item.Path);
            if (playlist!=null)
            {
              playlist.Load(item.Path);
              // and if it contains any items
              if (playlist.Count>0)
              {
                // then add the 1st item to the playlist player
                PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
                playlistItem.FileName = playlist[0].FileName;
                playlistItem.Description = playlist[0].Description;
                playlistItem.Duration = playlist[0].Duration;
                playlistItem.Type = playlist[0].Type;
                PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
              }
            }
          }
          else
          {
            // item is just a normal file like .asx, .pls
            // or a radio station from the setup.
            RadioStation station = item.MusicTag as RadioStation;
            PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
            if (station!=null)
            {
              playlistItem.FileName = GetPlayPath(station);
              if (station.URL==String.Empty) playlistItem.Type=Playlists.PlayList.PlayListItem.PlayListItemType.Radio;
              else playlistItem.Type=Playlists.PlayList.PlayListItem.PlayListItemType.AudioStream;
            }
            else
            {
              playlistItem.Type=Playlists.PlayList.PlayListItem.PlayListItemType.AudioStream;
              playlistItem.FileName = item.Path;
            }
            playlistItem.Description = item.Label;
            playlistItem.Duration = 0;
            PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
          }
        }
      }
      PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
    }
    
    void Play(GUIListItem item)
    {
			if (Utils.IsPlayList(item.Path))
			{
				m_Playlist = PlayListFactory.Create(item.Path);
				if (m_Playlist!=null)
				{
					m_Playlist.Load(item.Path);
					if (m_Playlist.Count==1)
					{
            // add current directory 2 playlist and play this item
						string strURL=m_Playlist[0].FileName;
						m_Playlist=null;
            FillPlayList();
            PlayListPlayer.Play(strURL);
            return;
					}
					if (m_Playlist.Count==0)
					{
						m_Playlist=null;
					}
					LoadDirectory(m_strDirectory);
				}
			}
			else
			{
				if (m_Playlist!=null)
				{
          // add current playlist->playlist and play selected item
					string strURL=item.Path;
          FillPlayList();
          PlayListPlayer.Play(strURL);
          return;
				}

        // add current directory 2 playlist and play this item
				RadioStation station = item.MusicTag as RadioStation;
        FillPlayList();

				if (station!=null)
        {
          PlayListPlayer.Play ( GetPlayPath(station));
        }
				else
				{
					PlayListPlayer.Play (item.Path);
				}
			}
		}

    string GetPlayPath(RadioStation station)
    {
      if (station.URL.Length>5)
      {
        return station.URL;
      }
      else
      {
        string strFile=String.Format("{0}.radio",station.Frequency);
        return strFile;
      }
    }
		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public string PluginName()
		{
			return "My Radio";
		}

    public bool HasSetup()
    {
      return false;
    }
		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = GUILocalizeStrings.Get(665);
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
			return "Plugin to listen to local & internet radio";
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIRadio.ShowPlugin implementation
		}

		#endregion
	}
}