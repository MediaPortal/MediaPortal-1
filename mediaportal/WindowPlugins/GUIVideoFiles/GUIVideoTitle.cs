using System;
using System.Collections;
using System.Net;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;



namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIVideoTitle : GUIWindow, IComparer
  {
    [Serializable]
    public class MapSettings
    {
      protected int   _SortBy;
      protected int   _ViewAs;
      protected bool _SortAscending ;

      public MapSettings()
      {
        _SortBy=0;//name
        _ViewAs=0;//list
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
			CONTROL_BTNTYPE = 5, 
			CONTROL_PLAY_DVD = 6, 
			CONTROL_STACK = 7, 

			CONTROL_VIEW = 10, 
			CONTROL_LABELFILES = 12, 
			CONTROL_LABEL_GENRE = 100
		};
		#region Base variabeles
		enum SortMethod
		{
			SORT_NAME = 0, 
			SORT_YEAR = 1, 
			SORT_RATING = 2, 
			SORT_LABEL = 3, 
		}

		enum View
		{
			VIEW_AS_LIST = 0, 
			VIEW_AS_ICONS = 1, 
      VIEW_AS_LARGEICONS = 2, 
      VIEW_AS_FILMSTRIP   =   3,
		}
    const string ThumbsFolder=@"thumbs\Videos\Title";
		DirectoryHistory m_history = new DirectoryHistory();
    static VirtualDirectory  m_directory = new VirtualDirectory();
		string m_strDirectory = "";
		int m_iItemSelected = -1;
    MapSettings       _MapSettings = new MapSettings();
		#endregion
    
		public GUIVideoTitle()
		{
			GetID = (int)GUIWindow.Window.WINDOW_VIDEO_TITLE;
      m_directory.AddDrives();
      m_directory.SetExtensions(Utils.VideoExtensions);
      LoadSettings();
    }
    ~GUIVideoTitle()
    {
    }

		public override bool Init()
		{
			m_strDirectory = "";
			return Load(GUIGraphicsContext.Skin + @"\myvideoTitle.xml");
		}


    #region Serialisation
    void LoadSettings()
    {
    }

    void SaveSettings()
    {
    }
    #endregion


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
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
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
          LoadFolderSettings(m_strDirectory);
          
					ShowThumbPanel();
					LoadDirectory(m_strDirectory);
					return true;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					m_iItemSelected = GetSelectedItemNo();
					
          SaveSettings();
          SaveFolderSettings(m_strDirectory);
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
						_MapSettings.SortAscending = !_MapSettings.SortAscending;
            OnSort();
            GUIControl.FocusControl(GetID, iControl);
					}

					if (iControl == (int)Controls.CONTROL_BTNSORTBY) // sort by
					{
						switch ((SortMethod)_MapSettings.SortBy)
						{
							case SortMethod.SORT_NAME : 
								_MapSettings.SortBy = (int)SortMethod.SORT_YEAR;
								break;
							case SortMethod.SORT_YEAR : 
								_MapSettings.SortBy = (int)SortMethod.SORT_RATING;
								break;
							case SortMethod.SORT_RATING : 
								_MapSettings.SortBy = (int)SortMethod.SORT_LABEL;
								break;
							case SortMethod.SORT_LABEL : 
								_MapSettings.SortBy = (int)SortMethod.SORT_NAME;
								break;
						}
            LoadDirectory(m_strDirectory);
            GUIControl.FocusControl(GetID, iControl);
					}

					if (iControl == (int)Controls.CONTROL_BTNTYPE)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
						OnMessage(msg);
						int nSelected = (int)msg.Param1;
						int nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
						switch (nSelected)
						{
							case 0 : //	Movies
								nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
								break;
							case 1 : //	Genre
								nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_GENRE;
								break;
							case 2 : //	Actors
								nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_ACTOR;
								break;
							case 3 : //	Year
								nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_YEAR;
								break;
							case 4 : //	Titel
								nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_TITLE;
								break;
						}

						if (nNewWindow != GetID)
						{
							
              VideoState.StartWindow = nNewWindow;
							GUIWindowManager.ReplaceWindow(nNewWindow);
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
            if (iAction == (int)Action.ActionType.ACTION_DELETE_ITEM)
            {
              OnDelete(iItem);
            }

					}

					if (iControl == (int)Controls.CONTROL_PLAY_DVD)
					{
						OnPlayDVD();
					}
					break;
			}
			return base.OnMessage(message);
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
      int iSelect = 0;
      switch (VideoState.StartWindow)
      {
        case(int)GUIWindow.Window.WINDOW_VIDEOS : 
          iSelect = 0;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_GENRE : 
          iSelect = 1;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_ACTOR : 
          iSelect = 2;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_YEAR : 
          iSelect = 3;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_TITLE : 
          iSelect = 4;
          break;
      }
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_BTNTYPE, iSelect);

      
			string strLine = "";
			switch ((View)_MapSettings.ViewAs)
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

			switch ((SortMethod)_MapSettings.SortBy)
			{
				case SortMethod.SORT_NAME : 
					strLine = GUILocalizeStrings.Get(365);
					break;
				case SortMethod.SORT_YEAR : 
					strLine = GUILocalizeStrings.Get(366);
					break;
				case SortMethod.SORT_RATING : 
					strLine = GUILocalizeStrings.Get(367);
					break;
				case SortMethod.SORT_LABEL : 
					strLine = GUILocalizeStrings.Get(430);
					break;
			}
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSORTBY, strLine);

			if (_MapSettings.SortAscending)
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
    void LoadFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      object o;
      FolderSettings.GetFolderSetting(strDirectory,"VideoTitle",typeof(GUIVideoTitle.MapSettings), out o);
      if (o!=null) _MapSettings = o as MapSettings;
      if (_MapSettings==null) _MapSettings  = new MapSettings();
    }
    void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      FolderSettings.AddFolderSetting(strDirectory,"VideoTitle",typeof(GUIVideoTitle.MapSettings), _MapSettings);
    }

		void LoadDirectory(string strNewDirectory)
		{
			strNewDirectory.Trim();
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
			ArrayList itemlist = new ArrayList();
			if (m_strDirectory == "")
			{
				ArrayList titles = new ArrayList();
				VideoDatabase.GetMovies(ref titles);
				for (int i = 0; i < titles.Count; ++i)
				{
					IMDBMovie details = (IMDBMovie)titles[i];
					GUIListItem item = new GUIListItem(details.Title);
					string folder=details.SearchString;
					if (GUIVideoFiles.IsFolderPinProtected(details.Path)) continue;

					item.Path = details.SearchString;
					item.IsFolder = false;

					string strThumb = Utils.GetCoverArt(ThumbsFolder,details.Title);
          if (System.IO.File.Exists(strThumb))
          {
            item.ThumbnailImage = strThumb;
            item.IconImageBig = strThumb;
            item.IconImage = strThumb;
          }
					item.Rating = details.Rating;
					item.Year = details.Year;
					item.DVDLabel = details.DVDLabel;

					itemlist.Add(item);
				}
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABEL_GENRE, "");
			}

			SetIMDBThumbs(itemlist);
			string strSelectedItem = m_history.Get(m_strDirectory);
			int iItem = 0;
			foreach (GUIListItem item in itemlist)
			{
				if (!item.IsFolder)
				{
					switch ((SortMethod)_MapSettings.SortBy)
					{
						case SortMethod.SORT_NAME : 
							item.Label2 = item.Rating.ToString();
							break;
						case SortMethod.SORT_YEAR : 
							item.Label2 = item.Year.ToString();
							break;
						case SortMethod.SORT_RATING : 
							item.Label2 = item.Rating.ToString();
							break;
						case SortMethod.SORT_LABEL : 
							item.Label2 = item.DVDLabel;
							break;
						default : 
							item.Label2 = item.Rating.ToString();
							break;
					}
				}
        item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
				GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_VIEW, item);
			}
			OnSort();
			iItem = 0;
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
			GUIPropertyManager.SetProperty("#itemcount", strObjects);
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
      ShowThumbPanel();
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, m_iItemSelected);
      }

		}
		#endregion

		#region Sort Members
		void OnSort()
		{
			GUIFacadeControl list = (GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
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


			switch ((SortMethod)_MapSettings.SortBy)
			{
				case SortMethod.SORT_YEAR : 
				{
					if (_MapSettings.SortAscending)
					{
						if (item1.Year > item2.Year) return 1;
						if (item1.Year < item2.Year) return - 1;
					}
					else
					{
						if (item1.Year > item2.Year) return - 1;
						if (item1.Year < item2.Year) return 1;
					}
					return 0;
				}
				case SortMethod.SORT_RATING : 
				{
					if (_MapSettings.SortAscending)
					{
						if (item1.Rating > item2.Rating) return 1;
						if (item1.Rating < item2.Rating) return - 1;
					}
					else
					{
						if (item1.Rating > item2.Rating) return - 1;
						if (item1.Rating < item2.Rating) return 1;
					}
					return 0;
				}

				case SortMethod.SORT_NAME : 
          
					if (_MapSettings.SortAscending)
					{
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						return String.Compare(item2.Label, item1.Label, true);
					}
        
				case SortMethod.SORT_LABEL : 
          if (_MapSettings.SortAscending)
					{
						return String.Compare(item1.DVDLabel, item2.DVDLabel, true);
					}
					else
					{
						return String.Compare(item2.DVDLabel, item1.DVDLabel, true);
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
				m_iItemSelected = GetSelectedItemNo();
				int iMovieId = System.Int32.Parse(item.Path);
				int iSelectedFile = 1;
				ArrayList movies = new ArrayList();
				VideoDatabase.GetFiles(iMovieId, ref movies);
				if (movies.Count <= 0) return;
				if (!GUIVideoFiles.CheckMovie(iMovieId)) return;
        // Image file handling.
        // If the only file is an image file, it should be mounted,
        // allowing autoplay to take over the playing of it.
        // There should only be one image file in the stack, since
        // stacking is not currently supported for image files.
        if (movies.Count == 1 && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension((string)movies[0]).ToLower()))
        {
          m_iItemSelected = -1;
          // GetDirectory mounts the image file
          ArrayList itemlist = m_directory.GetDirectory((string)movies[0]);
          return;
        }

        bool bAskResume=true;        
        int iDuration = 0;
        {
          //get all movies belonging to each other
          ArrayList items = m_directory.GetDirectory(System.IO.Path.GetDirectoryName((string)movies[0]));

          //check if we can resume 1 of those movies
          int stoptime=0;
          bool asked=false;
          ArrayList newItems = new ArrayList();
          for (int i = 0; i < items.Count; ++i)
          {
            GUIListItem pItemTmp = (GUIListItem)items[i];
            if (Utils.ShouldStack(pItemTmp.Path, (string)movies[0]))
            {   
              if (!asked) iSelectedFile++;                
              IMDBMovie movieDetails = new IMDBMovie();
              int fileid=VideoDatabase.GetFileId(pItemTmp.Path);
              if ( (iMovieId >= 0) && (fileid >= 0) )
              {                 
                VideoDatabase.GetMovieInfo((string)movies[0], ref movieDetails);
                string title=System.IO.Path.GetFileName((string)movies[0]);
                Utils.RemoveStackEndings(ref title);
                if (movieDetails.Title!=String.Empty) title=movieDetails.Title;
                    
                stoptime=VideoDatabase.GetMovieStopTime(fileid);
                if (stoptime>0)
                {
                  if (!asked)
                  {
                    asked=true;
                    GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                    if (null == dlgYesNo) return;
                    dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                    dlgYesNo.SetLine(1, title);
                    dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936)+" "+Utils.SecondsToHMSString(iDuration + stoptime));
                    dlgYesNo.SetDefaultToYes(true);
                    dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                    if (dlgYesNo.IsConfirmed) 
                    {
                      bAskResume=false;
                      newItems.Add(pItemTmp);
                    }
                    else
                    {
                      VideoDatabase.DeleteMovieStopTime(fileid);
                            newItems.Add(pItemTmp);
                    }
                  } //if (!asked)
                  else
                  {
                    newItems.Add(pItemTmp);
                  }
                } //if (stoptime>0)
                else
                {
                  newItems.Add(pItemTmp);
                }

                // Total movie duration
                iDuration += VideoDatabase.GetMovieDuration(fileid);
              } 
              else //if (movieid >=0)
              {
                newItems.Add(pItemTmp);
              }
            }//if (Utils.ShouldStack(pItemTmp.Path, item.Path))
          }

          // If we have found stackable items, clear the movies array
          // so, that we can repopulate it.
          if (newItems.Count > 0)
          {
            movies.Clear();
          }

          for (int i = 0; i < newItems.Count; ++i)
          {
            GUIListItem pItemTmp = (GUIListItem)newItems[i];
            if (Utils.IsVideo(pItemTmp.Path) && !PlayListFactory.IsPlayList(pItemTmp.Path))
            {
              movies.Add(pItemTmp.Path);
            }
          }
        }

        if (movies.Count > 1)
				{
          if (bAskResume)
          {
            GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
            if (null != dlg)
            {
              dlg.SetNumberOfFiles(movies.Count);
              dlg.DoModal(GetID);
              iSelectedFile = dlg.SelectedFile;
              if (iSelectedFile < 1) return;
            }
          }
        }
    
				PlayListPlayer.Reset();
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP;
				PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP);
				playlist.Clear();
				for (int i = iSelectedFile - 1; i < movies.Count; ++i)
				{
					string strFileName = (string)movies[i];
					PlayList.PlayListItem newitem = new PlayList.PlayListItem();
          newitem.FileName = strFileName;
          newitem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
					playlist.Add(newitem);
				}

        // play movie...
        GUIVideoFiles.PlayMovieFromPlayList(bAskResume);
			}
		}
    
    void OnDelete(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item == null) return;
      

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo) return;
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(432)); //Delete movie from database
      dlgYesNo.SetLine(1, item.Label);
      dlgYesNo.SetLine(2, "");
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed) return;

      ArrayList movies = new ArrayList();
      int iMovieId = System.Int32.Parse(item.Path);
      VideoDatabase.GetFiles(iMovieId, ref movies);
      VideoDatabase.DeleteMovie((string)movies[0]);
      LoadDirectory(m_strDirectory);
		
    }

		void OnInfo(int iItem)
		{
      m_iItemSelected = GetSelectedItemNo();
			GUIListItem item = GetSelectedItem();
	
			int iMovieId = System.Int32.Parse(item.Path);
	
			GUIVideoFiles.ShowIMDB(iMovieId);
		}

		void SetIMDBThumbs(ArrayList items)
		{
			ArrayList movies = new ArrayList();
			VideoDatabase.GetMoviesByPath(m_strDirectory, ref movies);
			for (int x = 0; x < items.Count; ++x)
			{
				GUIListItem pItem = (GUIListItem)items[x];
				if (!pItem.IsFolder)
				{
					for (int i = 0; i < movies.Count; ++i)
					{
						IMDBMovie info = (IMDBMovie)movies[i];
						string strFile = System.IO.Path.GetFileName(pItem.Path);
						if (info.File[0] == '\\' || info.File[0] == '/')
							info.File = info.File.Substring(1);

						if (strFile.Length > 0)
						{
							if (info.File == strFile /*|| pItem.GetLabel() == info.Title*/)
							{
								string strThumb;
								strThumb = Utils.GetCoverArt(ThumbsFolder,info.Title);
                if (System.IO.File.Exists(strThumb))
                {
                  pItem.ThumbnailImage = strThumb;
                  pItem.IconImageBig = strThumb;
                  pItem.IconImage = strThumb;

                }
								break;
							}
						}
					}
				}
			}
		}


		void OnPlayDVD()
		{
			Log.Write("GUIVideoTitle playDVD");
      g_Player.PlayDVD();
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


    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
      if (filmstrip==null) return;
      if (item.IsFolder) filmstrip.InfoImageFileName=item.ThumbnailImage;
      else filmstrip.InfoImageFileName=Utils.ConvertToLargeCoverArt(item.ThumbnailImage);


    }

		void ShowContextMenu()
		{
			if (m_strDirectory == "") return;
			GUIListItem item=GetSelectedItem();
			int itemNo=GetSelectedItemNo();
			if (item==null) return;

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu
			dlg.Add( GUILocalizeStrings.Get(925)); //delete
			dlg.Add( GUILocalizeStrings.Get(368)); //IMDB
			dlg.Add( GUILocalizeStrings.Get(208)); //play

			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			switch (dlg.SelectedLabel)
			{
				case 0: // Delete
					OnDeleteItem(item);
					break;

				case 1: // IMDB
					OnInfo(itemNo);
					break;

				case 2: // play
					OnClick(itemNo);	
					break;
			}
		}

		void OnDeleteItem(GUIListItem item)
		{
			if (item.IsRemote) return;
			
			IMDBMovie movieDetails = new IMDBMovie();

			int movieid=VideoDatabase.GetMovieInfo(item.Path, ref movieDetails);
			string strFileName=System.IO.Path.GetFileName(item.Path);
			if (movieid>=0) strFileName=movieDetails.Title;

			GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (null==dlgYesNo) return;
			dlgYesNo.SetHeading(GUILocalizeStrings.Get(664));
			dlgYesNo.SetLine(1,strFileName);
			dlgYesNo.SetLine(2, "");
			dlgYesNo.SetLine(3, "");
			dlgYesNo.DoModal(GetID);

			if (!dlgYesNo.IsConfirmed) return;
			
			DoDeleteItem(item);
						
			m_iItemSelected=GetSelectedItemNo();
			if (m_iItemSelected>0) m_iItemSelected--;
			LoadDirectory(m_strDirectory);
			if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_VIEW,m_iItemSelected);
			}
		}

		void DoDeleteItem(GUIListItem item)
		{
			if (item.IsFolder )return;
			if (!item.IsRemote)
			{
				VideoDatabase.DeleteMovie(item.Path);
				TVDatabase.RemoveRecordedTVByFileName(item.Path);
				Utils.FileDelete(item.Path);
			}
		}
	}
}
