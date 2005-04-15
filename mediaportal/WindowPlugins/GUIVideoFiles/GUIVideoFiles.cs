using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Globalization;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

namespace MediaPortal.GUI.Video

{
  /// <summary>
  /// Summary description for Class1.
  /// 
  /// </summary>
	public class GUIVideoFiles : GUIVideoBaseWindow, ISetupForm,IMDB.IProgress
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
      
      [XmlElement("Stack")]
      public bool Stack
      {
        get { return _Stack;}
        set { _Stack=value;}
      }
      
      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending;}
        set { _SortAscending=value;}
      }
    }

		static IMDB								  imdb ;
		DirectoryHistory m_history = new DirectoryHistory();
		string            m_strDirectory = "";
		int               m_iItemSelected = -1;
		static VirtualDirectory  m_directory = new VirtualDirectory();
    MapSettings       _MapSettings = new MapSettings();
    bool              m_askBeforePlayingDVDImage = false;

		public GUIVideoFiles()
		{
			GetID = (int)GUIWindow.Window.WINDOW_VIDEOS;
      
			m_directory.AddDrives();
			m_directory.SetExtensions(Utils.VideoExtensions);
			if (!System.IO.File.Exists("videoViews.xml"))
			{
				//genres
				FilterDefinition filter1,filter2;
				ViewDefinition viewGenre = new ViewDefinition();
				viewGenre.Name="Genres";
				filter1 = new FilterDefinition();filter1.Where="genre";;filter1.SortAscending=true;
				filter2 = new FilterDefinition();filter2.Where="title";;filter2.SortAscending=true;
				viewGenre.Filters.Add(filter1);
				viewGenre.Filters.Add(filter2);

				//artists
				ViewDefinition viewArtists = new ViewDefinition();
				viewArtists.Name="Actors";
				filter1 = new FilterDefinition();filter1.Where="actor";;filter1.SortAscending=true;
				filter2 = new FilterDefinition();filter2.Where="title";;filter2.SortAscending=true;
				viewArtists.Filters.Add(filter1);
				viewArtists.Filters.Add(filter2);

				//title
				ViewDefinition viewTitles = new ViewDefinition();
				viewTitles.Name="Title";
				filter1 = new FilterDefinition();filter1.Where="title";;filter1.SortAscending=true;
				viewTitles.Filters.Add(filter1);

				//years
				ViewDefinition viewYears = new ViewDefinition();
				viewYears.Name="Years";
				filter1 = new FilterDefinition();filter1.Where="year";;filter1.SortAscending=true;
				filter2 = new FilterDefinition();filter2.Where="title";;filter2.SortAscending=true;
				viewYears.Filters.Add(filter1);
				viewYears.Filters.Add(filter2);

				ArrayList listViews = new ArrayList();
				listViews.Add(viewGenre);
				listViews.Add(viewArtists);
				listViews.Add(viewTitles);
				listViews.Add(viewYears);

				using(FileStream fileStream = new FileStream("videoViews.xml", FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					SoapFormatter formatter = new SoapFormatter();
					formatter.Serialize(fileStream, listViews);
					fileStream.Close();
				}
			}
    }

		public override void DeInit()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("movies","startWindow",VideoState.StartWindow.ToString());

			}
		}

		public override bool Init()
		{
			imdb = new IMDB(this);
      g_Player.PlayBackStopped +=new MediaPortal.Player.g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.PlayBackEnded +=new MediaPortal.Player.g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStarted +=new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
      m_strDirectory = "";
			try
			{
				System.IO.Directory.CreateDirectory(@"Thumbs\videos");
				System.IO.Directory.CreateDirectory(@"Thumbs\videos\genre");
				System.IO.Directory.CreateDirectory(Thumbs.MovieTitle);
			}
			catch(Exception){}
			LoadSettings();
			bool result=Load(GUIGraphicsContext.Skin + @"\myVideo.xml");
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				
				VideoState.StartWindow=xmlreader.GetValueAsInt("movies","startWindow", GetID);
			}
			LoadSettings();
			return result;
		}

    #region Serialisation
    protected override void LoadSettings()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
				
        m_directory.Clear();
        string strDefault = xmlreader.GetValueAsString("movies", "default","");
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}",i);
          string strSharePath = String.Format("sharepath{0}",i);
          string strPincode = String.Format("pincode{0}",i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd  = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("movies", strShareName, "");
          share.Path = xmlreader.GetValueAsString("movies", strSharePath, "");
          share.Pincode = xmlreader.GetValueAsInt("movies", strPincode, - 1);
          
          share.IsFtpShare= xmlreader.GetValueAsBool("movies", shareType, false);
          share.FtpServer= xmlreader.GetValueAsString("movies", shareServer,"");
          share.FtpLoginName= xmlreader.GetValueAsString("movies", shareLogin,"");
          share.FtpPassword= xmlreader.GetValueAsString("movies", sharePwd,"");
          share.FtpPort= xmlreader.GetValueAsInt("movies", sharePort,21);
          share.FtpFolder= xmlreader.GetValueAsString("movies", remoteFolder,"/");

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
        m_askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
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
			base.OnAction(action);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			if (VideoState.StartWindow != GetID)
			{
				GUIWindowManager.ReplaceWindow(VideoState.StartWindow);
				return ;
			}
			LoadFolderSettings(m_strDirectory);
			LoadDirectory(m_strDirectory);
		}


		protected override void OnPageDestroy(int newWindowId)
		{
			m_iItemSelected = GetSelectedItemNo();
					
			SaveFolderSettings(m_strDirectory);
			base.OnPageDestroy (newWindowId);
		}

		
		public override bool OnMessage(GUIMessage message)
		{
			switch (message.Message)
			{
          case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
            if (g_Player.Playing && g_Player.IsDVD)
            {
							Log.Write("GUIVideo:stop dvd since DVD is ejected");
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
					facadeView.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:

					facadeView.OnMessage(message);
          break;

				case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
					m_strDirectory=message.Label;
					LoadDirectory(m_strDirectory);
					break;

				case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
				case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
					if (m_strDirectory == "" || m_strDirectory.Substring(0,2)==message.Label)
					{
						m_strDirectory = "";
						LoadDirectory(m_strDirectory);
					}
					break;
			}
			return base.OnMessage(message);
		}


		
    void LoadFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      object o;
      FolderSettings.GetFolderSetting(strDirectory,"VideoFiles",typeof(GUIVideoFiles.MapSettings), out o);
      if (o!=null) _MapSettings = o as MapSettings;
      if (_MapSettings==null) _MapSettings  = new MapSettings();
    }
    void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      FolderSettings.AddFolderSetting(strDirectory,"VideoFiles",typeof(GUIVideoFiles.MapSettings), _MapSettings);
    }

		protected override void LoadDirectory(string strNewDirectory)
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

			string strObjects = "";

      ArrayList itemlist;

      // Mounting and loading a DVD image file takes a long time,
      // so display a message letting the user know that something 
      // is happening.
      if (!m_askBeforePlayingDVDImage && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(m_strDirectory)))
      {
				itemlist = PlayMountedImageFile(GetID, m_strDirectory);

        // Remember the directory that the image file is in rather than the
        // image file itself.  This prevents repeated playing of the image file.
        if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(m_strDirectory)))
        {
          m_strDirectory = System.IO.Path.GetDirectoryName(m_strDirectory);
        }
      }
      else
      {
			GUIControl.ClearControl(GetID, facadeView.GetID);
            
        itemlist = m_directory.GetDirectory(m_strDirectory);
      }

      if (_MapSettings.Stack)
      {
        ArrayList itemfiltered = new ArrayList();
        for (int x = 0; x < itemlist.Count; ++x)
        {
          bool bAdd = true;
          GUIListItem item1 = (GUIListItem)itemlist[x];
          for (int y = 0; y < itemlist.Count; ++y)
          {
            GUIListItem item2 = (GUIListItem)itemlist[y];
            if (x != y)
            {
              if (!item1.IsFolder || !item2.IsFolder)
              {
                if (Utils.ShouldStack(item1.Path, item2.Path))
                {
                  if (String.Compare(item1.Path, item2.Path, true) > 0)
                  {
                    bAdd = false;
                  }
                }
              }
            }
          }
          if (bAdd)
          {
            string strLabel = item1.Label;
            Utils.RemoveStackEndings(ref strLabel);
            item1.Label = strLabel;
            itemfiltered.Add(item1);
          }
        }
        itemlist = itemfiltered;
      }
      
      SetIMDBThumbs(itemlist);
			string strSelectedItem = m_history.Get(m_strDirectory);
			int iItem = 0;
			foreach (GUIListItem item in itemlist)
      {
        item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
				facadeView.Add(item);
			}
			OnSort();
			for (int i = 0; i < GetItemCount(); ++i)
			{
				GUIListItem item = GetItem(i);
				if (item.Label == strSelectedItem)
				{
					GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
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
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
      }

		}
		#endregion


		
		protected override void OnClick(int iItem)
		{
			GUIListItem item = GetSelectedItem();
			if (item == null) return;
			bool bFolderIsMovie = false;

			if (item.IsFolder)
			{
				// Check if folder is actually a DVD. If so don't browse this folder, but play the DVD!
        if ((System.IO.File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO")) && (item.Label!=".."))
        {
          bFolderIsMovie = true;
          item.Path      = item.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        else 
        {
          bFolderIsMovie = false;

					/* Mars Warrior @ 11-sep-2004 -- REMOVED
          // Check if folder is a folder containig a single movie file. If so, (again), don't
          // browse the folder, but play the movie!
          ArrayList items = this.m_directory.GetDirectory(item.Path);
          int iVideoFilesCount = 0;
          string strVideoFile = "";
          for (int i = 0; i < items.Count; ++i)
          {
            GUIListItem pItemTmp = (GUIListItem)items[i];
            if (Utils.IsVideo(pItemTmp.Path) && !PlayListFactory.IsPlayList(pItemTmp.Path))
            {
              iVideoFilesCount++;
              if (iVideoFilesCount == 1) strVideoFile = pItemTmp.Path;
            }
          }
          if (iVideoFilesCount == 1)
          {
            bFolderIsMovie = true;
            item.Path      = strVideoFile;
          }
          else bFolderIsMovie = false;
					-- REMOVED */
        }
			}

			if ((item.IsFolder) && (!bFolderIsMovie))
			//-- Mars Warrior @ 03-sep-2004
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
            else
            {
              
              //download subtitle files
              string[] sub_exts = {  ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss", ".ass", ".idx",".ifo" };
              // check if movie has subtitles
              for (int i = 0; i < sub_exts.Length; i++)
              {
                string strSubTitleFile = item.Path;
                strSubTitleFile = System.IO.Path.ChangeExtension(strSubTitleFile, sub_exts[i]);
                string localSubFile=m_directory.GetLocalFilename(strSubTitleFile);
                Utils.FileDelete(localSubFile);
                m_directory.DownloadRemoteFile(item.Path,0);
              }
            }
          }
        }
        
        if (item.FileInfo != null)
        {
          if (!m_directory.IsRemoteFileDownloaded(item.Path,item.FileInfo.Length) ) return;
        }
        string strFileName = item.Path;
        strFileName=m_directory.GetLocalFilename(strFileName);

				// Set selected item
				m_iItemSelected = GetSelectedItemNo();
				if (PlayListFactory.IsPlayList(strFileName))
				{
					LoadPlayList(strFileName);
					return;
				}
				
				
        if (!CheckMovie(strFileName)) return;
        bool bAskResume=true;        
				if (_MapSettings.Stack)
				{
          int iSelectedFile = 0;
          int iDuration = 0;
					ArrayList movies = new ArrayList();
					{
            //get all movies belonging to each other
						ArrayList items = m_directory.GetDirectory(m_strDirectory);

            //check if we can resume 1 of those movies
            int stoptime=0;
            bool asked=false;
            ArrayList newItems = new ArrayList();
            for (int i = 0; i < items.Count; ++i)
            {
              GUIListItem pItemTmp = (GUIListItem)items[i];
              if (Utils.ShouldStack(pItemTmp.Path, item.Path))
              {   
                if (!asked) iSelectedFile++;                
                IMDBMovie movieDetails = new IMDBMovie();
                int fileid=VideoDatabase.GetFileId(pItemTmp.Path);
                int movieid=VideoDatabase.GetMovieId(item.Path);
                if ( (movieid >= 0) && (fileid >= 0) )
                {                 
									VideoDatabase.GetMovieInfo(item.Path, ref movieDetails);
                  string title=System.IO.Path.GetFileName(item.Path);
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

            for (int i = 0; i < newItems.Count; ++i)
						{
							GUIListItem pItemTmp = (GUIListItem)newItems[i];
							if (Utils.IsVideo(pItemTmp.Path) && !PlayListFactory.IsPlayList(pItemTmp.Path))
							{
                if (Utils.ShouldStack(pItemTmp.Path, item.Path))
                {
                  movies.Add(pItemTmp.Path);
                }
							}
						}
            if (movies.Count == 0)
            {
              movies.Add(strFileName);
            }
					}
					if (movies.Count <= 0) return;				
					if (movies.Count > 1)
					{
						//TODO
						movies.Sort();
						for (int i = 0; i < movies.Count; ++i)
						{
							AddFileToDatabase((string)movies[i]);
						}

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
					else if (movies.Count==1)
					{
						AddFileToDatabase((string)movies[0]);
					}
					PlayListPlayer.Reset();
					PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP;
					PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP);
					playlist.Clear();
					for (int i = 0; i < (int)movies.Count; ++i)
					{
						strFileName = (string)movies[i];
						PlayList.PlayListItem itemNew = new PlayList.PlayListItem();
						itemNew.FileName = strFileName;
            itemNew.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
						playlist.Add(itemNew);
					}

					// play movie...
					PlayMovieFromPlayList(bAskResume, iSelectedFile-1);
					return;
				}

				// play movie...
				AddFileToDatabase(strFileName);

        PlayListPlayer.Reset();
        PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP;
        PlayList newPlayList = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP);
        newPlayList.Clear();
        PlayList.PlayListItem NewItem = new PlayList.PlayListItem();
        NewItem.FileName = strFileName;
        NewItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
        newPlayList.Add(NewItem);
        PlayMovieFromPlayList(true);
/*
				//TODO
        if (g_Player.Play(strFileName))
        {
          if (Utils.IsVideo(strFileName))
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
        }*/
			}
		}
    
		protected override void OnQueueItem(int iItem)
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
			GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem + 1);

		}

		protected override void AddItemToPlayList(GUIListItem pItem) 
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
				if (Utils.IsVideo(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
				{
					PlayList.PlayListItem playlistItem = new PlayList.PlayListItem();
					playlistItem.FileName = pItem.Path;
					playlistItem.Description = pItem.Label;
					playlistItem.Duration = pItem.Duration;
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Add(playlistItem);
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
				//TODO
				Log.Write("GUIVideoFiles play:{0}",playlist[0].FileName);
        if (g_Player.Play(playlist[0].FileName))
        {
          if (Utils.IsVideo(playlist[0].FileName))
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
        }
				return;
			}

			// clear current playlist
			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Clear();

			// add each item of the playlist to the playlistplayer
			for (int i = 0; i < playlist.Count; ++i)
			{
				PlayList.PlayListItem playListItem = playlist[i];
				PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Add(playListItem);
			}

			
			// if we got a playlist
			if (PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Count > 0)
			{
				// then get 1st song
				playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO);
				PlayList.PlayListItem item = playlist[0];

				// and start playing it
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
				PlayListPlayer.Reset();
				PlayListPlayer.Play(0);

				// and activate the playlist window if its not activated yet
				if (GetID == GUIWindowManager.ActiveWindow)
				{
					GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
				}
			}
		}

		void AddFileToDatabase(string strFile)
		{
      if (!Utils.IsVideo(strFile)) return;
      //if ( Utils.IsNFO(strFile)) return;
      if (PlayListFactory.IsPlayList(strFile)) return;

      if (!VideoDatabase.HasMovieInfo(strFile))
      {
        // set initial movie info
				VideoDatabase.AddMovieFile(strFile);
  
        IMDBMovie movieDetails = new IMDBMovie();             
        int iMovieId=VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
        if (iMovieId>=0)
        {
          if (Utils.IsDVD(strFile))
          {
            //DVD
            movieDetails.DVDLabel = Utils.GetDriveName(System.IO.Path.GetPathRoot(strFile));
            movieDetails.Title = movieDetails.DVDLabel;
          }
          else if (strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >=0)
          {
            //DVD folder
            strFile = strFile.Substring(0,strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
            movieDetails.DVDLabel = System.IO.Path.GetFileName(strFile);
            movieDetails.Title = movieDetails.DVDLabel;
          }
          else
          {
            //Movie 
            movieDetails.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);
          }
          VideoDatabase.SetMovieInfoById(iMovieId, ref movieDetails);
        }
      }
		}

		bool OnScan(ArrayList items)
		{
		  // remove username + password from m_strDirectory for display in Dialog

      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(189);
        dlgProgress.SetLine(1, "");
        dlgProgress.SetLine(2, m_strDirectory);
        dlgProgress.StartModal(GetID);
      }

      OnRetrieveVideoInfo(items);
      if (dlgProgress != null)
      {
        dlgProgress.SetLine(2, m_strDirectory);
        if (dlgProgress.IsCanceled) return false;
      }
	
      bool bCancel = false;
      for (int i = 0; i < items.Count; ++i)
      {
        GUIListItem pItem = (GUIListItem)items[i];
        if (dlgProgress != null)
        {
          if (dlgProgress.IsCanceled) 
          {
            bCancel = true;
            break;
          }
        }
        if (pItem.IsFolder)
        {
          if (pItem.Label != "..")
          {
            // load subfolder
            string strDir = m_strDirectory;
            m_strDirectory = pItem.Path;

            bool FolderIsDVD=false;
						// Mars Warrior @ 03-sep-2004
						// Check for single movie in directory and make sure (just as with DVDs) that
						// the folder gets the nice folder.jpg image ;-)
						bool bFolderIsMovie = false;

						ArrayList subDirItems = m_directory.GetDirectory(pItem.Path);
            foreach (GUIListItem item in subDirItems)
            {
              if (item.Label.ToLower().Equals("video_ts"))
              {
                FolderIsDVD=true;
                break;
              }
            }

						// Check if folder is a folder containig a single movie file. If so, (again), don't
						// browse the folder, but play the movie!
						int iVideoFilesCount = 0;
						string strVideoFile = "";
						foreach (GUIListItem item in subDirItems)
						{
							if (Utils.IsVideo(item.Path) && !PlayListFactory.IsPlayList(item.Path))
							{
								iVideoFilesCount++;
								if (iVideoFilesCount == 1) strVideoFile = item.Path;
							}
						}
						if (iVideoFilesCount == 1)
						{
							bFolderIsMovie = true;
						}
						else bFolderIsMovie = false;

						if ((!FolderIsDVD) && (!bFolderIsMovie))
						{
							if (!OnScan(subDirItems))
							{
								bCancel = true;
							}
						}
						else if (FolderIsDVD)
						{
							string strFilePath=String.Format(@"{0}\VIDEO_TS\VIDEO_TS.IFO",pItem.Path);
							OnRetrieveVideoInfo(strFilePath, pItem.Label, pItem.Path);
						}
						else if (bFolderIsMovie)
						{
							OnRetrieveVideoInfo(strVideoFile, pItem.Label, pItem.Path);
						}
						//-- Mars Warrior
				
            m_strDirectory = strDir;
            if (bCancel) break;
          }
        }
      }
	
      if (dlgProgress != null) dlgProgress.Close();
      return!bCancel;
		}

    /// <summary>
    /// Searches IMDB for a movie and if found gets the details about the 1st movie found
    /// details are put in the video database under the file mentioned by strFileName
    /// also a thumbnail is downloaded to thumbs\ and
    /// if strPath is filled in a srtpath\folder.jpg is created
    /// </summary>
    /// <param name="strFileName">path+filename to which this imdb info will belong</param>
    /// <param name="strMovieName">IMDB search string</param>
    /// <param name="strPath">path where folder.jpg should be created</param>
    void OnRetrieveVideoInfo(string strFileName, string strMovieName, string strPath)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      AddFileToDatabase(strFileName);
      if (!VideoDatabase.HasMovieInfo(strFileName))
      {
        // do IMDB lookup...
        if (dlgProgress != null)
        {
          dlgProgress.SetHeading(197);
          dlgProgress.SetLine(1, strMovieName);
          dlgProgress.SetLine(2, "");
          dlgProgress.SetLine(3, "");
          dlgProgress.Progress();
          if (dlgProgress.IsCanceled) return;
        }


        imdb.Find(strMovieName);
              
        int iMoviesFound = imdb.Count;
        if (iMoviesFound > 0)
        {
          IMDBMovie movieDetails = new IMDBMovie();
          movieDetails.SearchString = strMovieName;
          IMDB.IMDBUrl url = imdb[0];

          // show dialog that we're downloading the movie info
          if (dlgProgress != null)
          {
            dlgProgress.SetHeading(198);
            //dlgProgress.SetLine(0, strMovieName);
            dlgProgress.SetLine(1, url.Title);
            dlgProgress.SetLine(2, "");
            dlgProgress.Progress();
            if (dlgProgress.IsCanceled) return;
          }

          if (imdb.GetDetails(url, ref movieDetails))
          {
            // get & save thumbnail
						AmazonImageSearch search = new AmazonImageSearch();
						search.Search(movieDetails.Title);
						if (search.Count>0)
						{
							movieDetails.ThumbURL=search[0];
						}
            VideoDatabase.SetMovieInfo(strFileName, ref movieDetails);
            string strThumb = "";
            string strImage = movieDetails.ThumbURL;
            if (strImage.Length > 0 && movieDetails.ThumbURL.Length > 0)
            { 
              string LargeThumb=Utils.GetLargeCoverArtName(Thumbs.MovieTitle,movieDetails.Title);
              strThumb = Utils.GetCoverArtName(Thumbs.MovieTitle,movieDetails.Title);
              Utils.FileDelete(strThumb);
              Utils.FileDelete(LargeThumb);
              
              string strExtension = System.IO.Path.GetExtension(strImage);
              if (strExtension.Length > 0)
              {
                string strTemp = "temp" + strExtension;
                Utils.FileDelete(strTemp);
                if (dlgProgress != null)
                {
                  dlgProgress.SetLine(2, 415);
                  dlgProgress.Progress();
                  if (dlgProgress.IsCanceled) return;
                }
                      
                Utils.DownLoadImage(strImage, strTemp);
                if (System.IO.File.Exists(strTemp))
                {
                  MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
                  MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);
                  if (strPath.Length > 0)
                  {
                    try
                    {
                      Utils.FileDelete(strPath+@"\folder.jpg");
                      System.IO.File.Copy(strThumb, strPath+@"\folder.jpg");
                    }
                    catch(Exception){ }
                  }
                }
                Utils.FileDelete(strTemp);
              }//if ( strExtension.Length>0)
              else
              {
                Log.Write("image has no extension:{0}", strImage);
              }
            }
          }
        }
      }
    }
    
    /// <summary>
    /// Retrieves the imdb info for an array of items.
    /// </summary>
    /// <param name="items"></param>
    void OnRetrieveVideoInfo(ArrayList items)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      // for every file found
      for (int i = 0; i < items.Count; ++i)
	    {
        GUIListItem pItem = (GUIListItem)items[i];
		  if (!pItem.IsFolder 
			  || ( pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
        {
          if (Utils.IsVideo(pItem.Path) && /*!Utils.IsNFO(pItem.Path) && */!PlayListFactory.IsPlayList(pItem.Path))
          {
            string strItem = String.Format("{0}/{1}", i + 1, items.Count);
            if (dlgProgress != null)
            {
              dlgProgress.SetLine(1, strItem);
              dlgProgress.SetLine(2, System.IO.Path.GetFileName(pItem.Path));
              dlgProgress.Progress();
              if (dlgProgress.IsCanceled) return;
            }
            string strMovieName = System.IO.Path.GetFileName(pItem.Path);
            OnRetrieveVideoInfo(pItem.Path, strMovieName,"");
          }
        }
      }
    }
		
    protected override void OnInfo(int iItem)
		{
      m_iItemSelected = GetSelectedItemNo();
      GUIDialogSelect dlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      bool bFolder = false;
      string strFolder = "";
      int iSelectedItem = GetSelectedItemNo();
      GUIListItem pItem = GetSelectedItem();
      if (pItem==null) return;
      if (pItem.IsRemote) return;
      string strFile = pItem.Path;
      string strMovie = pItem.Label;
      // Use DVD label as movie name
      if (Utils.IsDVD(pItem.Path) && (pItem.DVDLabel != ""))
      {
        strMovie = pItem.DVDLabel;
      }
      if (pItem.IsFolder)
      {
        // IMDB is done on a folder, find first file in folder
        strFolder = pItem.Path;
        bFolder = true;

		  bool bFoundFile = false;

		  string strExtension=System.IO.Path.GetExtension(pItem.Path).ToLower();
		  if (VirtualDirectory.IsImageFile(strExtension))
		  {
			  strFile = pItem.Path;
			  bFoundFile = true;
		  } 
		  else
		  {
			  ArrayList vecitems = m_directory.GetDirectory(pItem.Path);
        for (int i = 0; i < vecitems.Count; ++i)
        {
          pItem = (GUIListItem)vecitems[i];
          if (!pItem.IsFolder 
          || ( pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
          {
						if (Utils.IsVideo(pItem.Path) && /*!Utils.IsNFO(pItem.Path) && */!PlayListFactory.IsPlayList(pItem.Path))
						{
							strFile = pItem.Path;
							bFoundFile = true;
							break;
						}
          }
          else
          {
						if (pItem.Path.ToLower().IndexOf("video_ts") >=0)
            {
              strFile = String.Format(@"{0}\VIDEO_TS.IFO",pItem.Path);
              bFoundFile = true;
              break;
            }
						
				  }
			  }
		  }
        if (!bFoundFile) 
        {
          // no Video file in this folder?
          // then just lookup IMDB info and show it
          if ( ShowIMDB(-1,strMovie, strFolder, strFolder, false))
            LoadDirectory(m_strDirectory);
          GUIControl.SelectItemControl(GetID, facadeView.GetID, iSelectedItem);
          return;
        }
      }

      AddFileToDatabase(strFile);

 

      if ( ShowIMDB(-1,strMovie, strFile, strFolder, bFolder))
        LoadDirectory(m_strDirectory);
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iSelectedItem);
		}

		
    static public bool ShowIMDB(int iMovieId)
    {
      ArrayList movies = new ArrayList();
      VideoDatabase.GetFiles(iMovieId, ref movies);
      if (movies.Count <= 0) return false;
      string strFilePath = (string)movies[0];
      string strFile = System.IO.Path.GetFileName(strFilePath);
      return ShowIMDB(iMovieId,strFile, strFilePath, "" ,false);

    }
    /// <summary>
    /// Download & shows IMDB info for a file
    /// </summary>
    /// <param name="strMovie">IMDB search criteria</param>
    /// <param name="strFile">path+file where imdb info should be saved for</param>
    /// <param name="strFolder">path where folder.jpg should be created (if bFolder==true)</param>
    /// <param name="bFolder">true create a folder.jpg, false dont create a folder.jpg</param>
    static public bool ShowIMDB(int iMovieId,string strMovie, string strFile, string strFolder, bool bFolder)
    {
      GUIDialogOK 		 		pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress 	pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIDialogSelect 		pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      GUIVideoInfo pDlgInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);
 
      bool									bUpdate = false;
      bool									bFound = false;
 
      if (null == pDlgOK) return false;
      if (null == pDlgProgress) return false;
      if (null == pDlgSelect) return false;
      if (null == pDlgInfo) return false;
      string strMovieName = System.IO.Path.GetFileNameWithoutExtension(strMovie);

			IMDBMovie movieDetails = new IMDBMovie();
			if (iMovieId>=0)
				VideoDatabase.GetMovieInfoById(iMovieId, ref movieDetails);
			else 
			{
				if (VideoDatabase.HasMovieInfo(strFile))
				{
					VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
				}
			}

			if (movieDetails.ID>=0)
      {
        pDlgInfo.Movie = movieDetails;
        pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);
        if (!pDlgInfo.NeedsRefresh)
        { 
          if (bFolder && strFile != "")
          {
            // copy icon to folder also;
            string strThumbOrg = Utils.GetCoverArt(Thumbs.MovieTitle,movieDetails.Title);
            string strFolderImage = System.IO.Path.GetFullPath(strFolder);
            strFolderImage += "\\folder.jpg";
            if (System.IO.File.Exists(strThumbOrg) )
            {
              Utils.FileDelete(strFolderImage);
              try
              {
                Utils.FileDelete(strFolderImage);
                System.IO.File.Copy(strThumbOrg, strFolderImage);
              }
              catch (Exception)
              {
              }
            }
          }
          return true;
        }
        
        if (!Util.Win32API.IsConnectedToInternet()) return false;
        if (iMovieId>=0)
          VideoDatabase.DeleteMovieInfoById(iMovieId);
        else
          VideoDatabase.DeleteMovieInfo(strFile);
        GetStringFromKeyboard(ref strMovieName);
      }
      else
      {
        if (Utils.IsDVD(strFile))
        {
          GetStringFromKeyboard(ref strMovieName);
        }
      }
      
      if (!Util.Win32API.IsConnectedToInternet()) return false;

      bool bContinue = false;
      do
      {
        bContinue = false;
        if (!bFound)
        {
          // show dialog that we're busy querying www.imdb.com
          pDlgProgress.SetHeading(197);
          //pDlgProgress.SetLine(0, strMovieName);
          pDlgProgress.SetLine(1, strMovieName);
          pDlgProgress.SetLine(2, "");
          pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
          pDlgProgress.Progress();

          bool						bError = true;
  		    imdb.Find(strMovieName);
          if (imdb.Count > 0) 
          {
            pDlgProgress.Close();

            int iMoviesFound = imdb.Count;
            if (iMoviesFound > 0)
            {
              int iSelectedMovie = 0;
              if (iMoviesFound > 1)
              {
                // more then 1 movie found
                // ask user to select 1
                pDlgSelect.SetHeading(196);
                pDlgSelect.Reset();
                for (int i = 0; i < iMoviesFound; ++i)
                {
                  IMDB.IMDBUrl url = imdb[i];
                  pDlgSelect.Add(url.Title);
                }
                pDlgSelect.EnableButton(true);
                pDlgSelect.SetButtonLabel(413); // manual
                pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

                // and wait till user selects one
                iSelectedMovie = pDlgSelect.SelectedLabel;
                if (iSelectedMovie < 0)
                {
                  if (!pDlgSelect.IsButtonPressed) return false;
                  GetStringFromKeyboard(ref strMovieName);
                  if (strMovieName == "") return false;
                  bContinue = true;
                  bError = false;
                }
              }
          
              if (iSelectedMovie >= 0)
              {
                movieDetails = new IMDBMovie();
                movieDetails.SearchString = strFile;
                IMDB.IMDBUrl url = imdb[iSelectedMovie];
    				
                // show dialog that we're downloading the movie info
                pDlgProgress.SetHeading(198);
                //pDlgProgress.SetLine(0, strMovieName);
                pDlgProgress.SetLine(1, url.Title);
                pDlgProgress.SetLine(2, "");
                pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
                pDlgProgress.Progress();
                if (imdb.GetDetails(url, ref movieDetails))
                {
                  // got all movie details :-)
									AmazonImageSearch search = new AmazonImageSearch();
									search.Search(movieDetails.Title);
									if (search.Count>0)
									{
										movieDetails.ThumbURL=search[0];
									}
									//get all actors...
									DownloadActors(movieDetails);
									DownloadDirector(movieDetails);
                  pDlgProgress.Close();
                  bError = false;

                  // now show the imdb info
                  if (iMovieId>=0)
                    VideoDatabase.SetMovieInfoById(iMovieId, ref movieDetails);
                  else
                    VideoDatabase.SetMovieInfo(strFile, ref movieDetails);
                  pDlgInfo.Movie = movieDetails;
                  pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);
                  if (!pDlgInfo.NeedsRefresh)
                  { 
                    bUpdate = true;
                    if (bFolder && strFile != "")
                    {
                      // copy icon to folder also;
                      string strThumbOrg = Utils.GetCoverArt(Thumbs.MovieTitle,movieDetails.Title);
                      if (System.IO.File.Exists(strThumbOrg))
                      {
                        string strFolderImage = System.IO.Path.GetFullPath(strFolder);
                        strFolderImage += "\\folder.jpg"; //TODO                  
                        try
                        {
                          Utils.FileDelete(strFolderImage);
                          System.IO.File.Copy(strThumbOrg, strFolderImage);
                        }
                        catch (Exception)
                        {
                        }

                      }
                    }
                  }
                  else
                  {
                    bContinue = true;
                    strMovieName = System.IO.Path.GetFileNameWithoutExtension(strMovie);
                    GetStringFromKeyboard(ref strMovieName);
                  }
                }
                else
                {
                  pDlgProgress.Close();
                }
              }
            }
            else
            {
              pDlgProgress.Close();
              GetStringFromKeyboard(ref strMovieName);
              if (strMovieName == "") return false;
              bContinue = true;
              bError = false;
            }
          }
          else
          {
            pDlgProgress.Close();
            GetStringFromKeyboard(ref strMovieName);
            if (strMovieName == "") return false;
            bContinue = true;
            bError = false;
          }

          if (bError)
          {
            // show dialog...
            pDlgOK.SetHeading(195);
            pDlgOK.SetLine(1, strMovieName);
            pDlgOK.SetLine(2, "");
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          }
        }
      } while (bContinue);

      if (bUpdate)
      {
        return true;
      }
      return false;
    }

    void SetIMDBThumbs(ArrayList items)
    {
      GUIListItem pItem ;
      ArrayList movies = new ArrayList();
			
      for (int x = 0; x < items.Count; ++x)
      {
        pItem = (GUIListItem)items[x];
        if (pItem.IsFolder)
				{
					if (pItem.ThumbnailImage!=String.Empty) continue;
          if (System.IO.File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            movies.Clear();
            string file= pItem.Path + @"\VIDEO_TS";
            VideoDatabase.GetMoviesByPath(file, ref movies);
            for (int i = 0; i < movies.Count; ++i)
            {
              IMDBMovie info = (IMDBMovie)movies[i];
              string strFile = "VIDEO_TS.IFO";
              if (info.File[0] == '\\' || info.File[0] == '/')
                info.File = info.File.Substring(1);

              if (strFile.Length > 0)
              {
                if (info.File == strFile /*|| pItem->GetLabel() == info.Title*/)
                {
                  string strThumb;
                  if (Utils.IsDVD(pItem.Path))
                    pItem.Label=String.Format( "({0}:) {1}",  pItem.Path.Substring(0,1),  info.Title );
                  strThumb = Utils.GetCoverArt(Thumbs.MovieTitle, info.Title );
                  if (System.IO.File.Exists(strThumb))
                  {
                    pItem.ThumbnailImage = strThumb;
                    pItem.IconImageBig = strThumb;
                    pItem.IconImage = strThumb;
                  }
                  break;
                }
              }
            } // of for (int i = 0; i < movies.Count; ++i)
          } // of if (System.IO.File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        } // of if (pItem.IsFolder)
      } // of for (int x = 0; x < items.Count; ++x)

      movies.Clear();
      VideoDatabase.GetMoviesByPath(m_strDirectory, ref movies);
      for (int x = 0; x < items.Count; ++x)
      {
        pItem = (GUIListItem)items[x];
        if (!pItem.IsFolder 
        || ( pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
        {
					if (pItem.ThumbnailImage!=String.Empty) continue;
          for (int i = 0; i < movies.Count; ++i)
          {
            IMDBMovie info = (IMDBMovie)movies[i];
            string strFile = System.IO.Path.GetFileName(pItem.Path);
            if (info.File[0] == '\\' || info.File[0] == '/')
              info.File = info.File.Substring(1);

            if (strFile.Length > 0)
            {
              if (info.File == strFile /*|| pItem->GetLabel() == info.Title*/)
              {
                string strThumb;
                strThumb = Utils.GetCoverArt(Thumbs.MovieTitle, info.Title );
                if (System.IO.File.Exists(strThumb))
                {
                  pItem.ThumbnailImage = strThumb;
                  pItem.IconImageBig = strThumb;
                  pItem.IconImage = strThumb;
                }
                break;
              }
            }
          } // of for (int i = 0; i < movies.Count; ++i)
        } // of if (!pItem.IsFolder)
      } // of for (int x = 0; x < items.Count; ++x)
    }

    public bool CheckMovie(string strFileName)
    {
      if (!VideoDatabase.HasMovieInfo(strFileName)) return true;
     
      IMDBMovie movieDetails = new IMDBMovie();
      int movieid=VideoDatabase.GetMovieInfo(strFileName, ref movieDetails);
      if (movieid < 0) return true;
      return CheckMovie(movieid);
    }

    static public bool CheckMovie(int movieid)
    {
		IMDBMovie movieDetails = new IMDBMovie();
		VideoDatabase.GetMovieInfoById(movieid, ref movieDetails);

		if (!Utils.IsDVD(movieDetails.Path)) return true;
		string cdlabel="";
		cdlabel=Utils.GetDriveSerial(movieDetails.Path);
		if (cdlabel.Equals(movieDetails.CDLabel)) return true;

		GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
		if (dlg == null) return true;
		while (true)
		{
			dlg.SetHeading(428);
			dlg.SetLine(1, 429);
			dlg.SetLine(2, movieDetails.DVDLabel);
			dlg.SetLine(3, movieDetails.Title);
			dlg.DoModal(GUIWindowManager.ActiveWindow);
			if (dlg.IsConfirmed) 
			{
				if (movieDetails.CDLabel.StartsWith("nolabel"))
				{
					ArrayList movies = new ArrayList();
					VideoDatabase.GetFiles(movieid, ref movies);
					if (System.IO.File.Exists(/*movieDetails.Path+movieDetails.File*/(string)movies[0]))
					{
						cdlabel = Utils.GetDriveSerial(movieDetails.Path);
						VideoDatabase.UpdateCDLabel(movieDetails, cdlabel);
						movieDetails.CDLabel = cdlabel;
						return true;
					}
				}
				else
				{
					cdlabel=Utils.GetDriveSerial(movieDetails.Path);
					if (cdlabel.Equals(movieDetails.CDLabel)) return true;
				}
			}
			else break;
		}
		return false;
	}
	
    void OnManualIMDB()
		{
      m_iItemSelected = GetSelectedItemNo();
      string strInput = "";
      GetStringFromKeyboard(ref strInput);
      if (strInput == "") return;

      //string strThumb;
//      CUtil::GetThumbnail("Z:\\",strThumb);
//      ::DeleteFile(strThumb.c_str());

      if (ShowIMDB(-1,strInput, "", "", false))
        LoadDirectory(m_strDirectory);
      return;
    }

    static public void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return;
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GUIWindowManager.ActiveWindow);
			strLine = "";
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }
	
		#region ISetupForm Members

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
			return "My Videos";
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
			strButtonText = GUILocalizeStrings.Get(3);
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
			return "Plugin to watch and organize your videos";
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIVideoFiles.ShowPlugin implementation
		}

		#endregion

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
      if (filmstrip==null) return;
    
      if (item.IsFolder) filmstrip.InfoImageFileName=item.ThumbnailImage;
      else filmstrip.InfoImageFileName=Utils.ConvertToLargeCoverArt(item.ThumbnailImage);
    }

    static public void PlayMovieFromPlayList(bool bAskResume)
    {
      PlayMovieFromPlayList(bAskResume, -1);
    }
    static public void PlayMovieFromPlayList(bool bAskResume, int iMovieIndex)
    {
      string filename;
      if (iMovieIndex == -1)
        filename=PlayListPlayer.GetNext();
      else
        filename=PlayListPlayer.Get(iMovieIndex);

      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfo(filename, ref movieDetails);
      int fileid=VideoDatabase.GetFileId(filename);
      int movieid=VideoDatabase.GetMovieId(filename);
      int stoptime=0;
			byte[] resumeData = null;
			if ( (movieid >= 0) && (fileid >= 0) )
      {
        stoptime=VideoDatabase.GetMovieStopTimeAndResumeData(fileid, out resumeData);
				Log.Write("GUIVideoFiles::OnPlayBackStopped fileid={0} stoptime={1} resumeData={2}", fileid, stoptime, resumeData);
				if (stoptime>0)
        {
          string title=System.IO.Path.GetFileName(filename);
          VideoDatabase.GetMovieInfoById( movieid, ref movieDetails);
          if (movieDetails.Title!=String.Empty) title=movieDetails.Title;
          
          if (bAskResume)
          {
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
            dlgYesNo.SetLine(1, title);
            dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936)+Utils.SecondsToHMSString(stoptime) );
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
            
            if (!dlgYesNo.IsConfirmed) stoptime=0;
          }
        }
      }
      
      if (iMovieIndex==-1)
      {
        PlayListPlayer.PlayNext(true);
      }
      else
      {
        PlayListPlayer.Play(iMovieIndex);
      }

      if (g_Player.Playing && stoptime > 0)
      {
				if (g_Player.IsDVD)
				{
					g_Player.Player.SetResumeState(resumeData);
				}
				else
				{
					g_Player.SeekAbsolute(stoptime);
				}
      }
    }

		static public ArrayList PlayMountedImageFile(int WindowID, string file)
		{
			GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (dlgProgress != null)
			{
				dlgProgress.SetHeading(13013);
				dlgProgress.SetLine(1, System.IO.Path.GetFileNameWithoutExtension(file));
				dlgProgress.StartModal(WindowID);
				dlgProgress.Progress();
			}

			ArrayList itemlist = m_directory.GetDirectory(file);
			if (DaemonTools.IsMounted(file) && !g_Player.Playing)
			{
				string strDir = DaemonTools.GetVirtualDrive();

				// Check if the mounted image is actually a DVD. If so, bypass
				// autoplay to play the DVD without user intervention
				if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
				{
					PlayListPlayer.Reset();
					PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP;
					PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP);
					playlist.Clear();

					PlayList.PlayListItem newitem = new PlayList.PlayListItem();
					newitem.FileName = strDir + @"\VIDEO_TS\VIDEO_TS.IFO";
					newitem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
					playlist.Add(newitem);

					Log.Write("\"Autoplaying\" DVD image mounted on {0}",strDir);
					PlayMovieFromPlayList(true);
				}
			}
        
			if (dlgProgress!=null) dlgProgress.Close();

			return itemlist;
		}
		
    private void OnPlayBackStopped(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
    {
      if (type!=g_Player.MediaType.Video) return;

      // Handle all movie files from MovieId
      ArrayList movies = new ArrayList();
      int iMovieId=VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFiles(iMovieId, ref movies);
      if (movies.Count <= 0) return;
      for (int i=0 ; i<movies.Count ; i++)
      {
        string strFilePath = (string)movies[i];
        int fileid=VideoDatabase.GetFileId(strFilePath);
        if (fileid<0) break;
				if ( (filename == strFilePath) && (stoptime > 0) )
				{
					byte[] resumeData = null;
					g_Player.Player.GetResumeState(out resumeData);
					Log.Write("GUIVideoFiles::OnPlayBackStopped fileid={0} stoptime={1} resumeData={2}", fileid, stoptime, resumeData);
					VideoDatabase.SetMovieStopTimeAndResumeData(fileid,stoptime,resumeData);
				}
				else
					VideoDatabase.DeleteMovieStopTime(fileid);
      }
    }

    private void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
			if (type!=g_Player.MediaType.Video) return;

      // Handle all movie files from MovieId
      ArrayList movies = new ArrayList();
      int iMovieId=VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFiles(iMovieId, ref movies);
      for (int i=0 ; i<movies.Count ; i++)
      {
        string strFilePath = (string)movies[i];
        int fileid=VideoDatabase.GetFileId(strFilePath);
        if (fileid<0) break;
        VideoDatabase.DeleteMovieStopTime(fileid);
      }
    }
    private void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type!=g_Player.MediaType.Video) return;
      AddFileToDatabase(filename);

      int fileid=VideoDatabase.GetFileId(filename);
      if (fileid!=-1)
      {
        int iDuration=(int)g_Player.Duration;
        VideoDatabase.SetMovieDuration(fileid, iDuration);
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

			if (!facadeView.Focus)
			{
				// Menu button context menuu
				dlg.AddLocalizedString(368); //IMDB
				if (!m_directory.IsRemote(m_strDirectory)) dlg.AddLocalizedString(102); //Scan
				if (!_MapSettings.Stack) dlg.AddLocalizedString(346); //Stack
				else dlg.AddLocalizedString(347); //Unstack
				dlg.AddLocalizedString(654); //Eject
			}
			else
			{
				if ((System.IO.Path.GetFileName(item.Path) != "") || Utils.IsDVD(item.Path))
				{
					if (Utils.getDriveType(item.Path) != 5) dlg.AddLocalizedString(925); //delete
					dlg.AddLocalizedString(368); //IMDB
					dlg.AddLocalizedString(208); //play
					dlg.AddLocalizedString(926); //Queue
				}
				if (Utils.getDriveType(item.Path) == 5) dlg.AddLocalizedString(654); //Eject
			}

			dlg.DoModal( GetID);
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 925: // Delete
					OnDeleteItem(item);
					break;

				case 368: // IMDB
					OnInfo(itemNo);
					break;

				case 208: // play
					OnClick(itemNo);	
					break;
					
				case 926: // add to playlist
					OnQueueItem(itemNo);	
					break;
					
				case 136: // show playlist
					GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
					break;

        case 654: // Eject
					if (Utils.getDriveType(item.Path) != 5) Utils.EjectCDROM();
					else Utils.EjectCDROM(System.IO.Path.GetPathRoot(item.Path));
          LoadDirectory("");
          break;

				case 341: //Play dvd
					OnPlayDVD();
					break;

				case 346: //Stack
					_MapSettings.Stack = true;
					LoadDirectory(m_strDirectory);
					UpdateButtonStates();
					break;

				case 347: //Unstack
					_MapSettings.Stack = false;
					LoadDirectory(m_strDirectory);
					UpdateButtonStates();
					break;

				case 102: //Scan
					ArrayList itemlist = m_directory.GetDirectory(m_strDirectory);
					OnScan(itemlist);
					LoadDirectory(m_strDirectory);
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
		
      //get all movies belonging to each other
      if (_MapSettings.Stack)
      {           
        bool bStackedFile=false;
        ArrayList items = m_directory.GetDirectory(m_strDirectory);
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem pItemTmp = (GUIListItem)items[i];
          if (Utils.ShouldStack(pItemTmp.Path, item.Path))
          {   
            bStackedFile=true;
            strFileName=System.IO.Path.GetFileName(pItemTmp.Path);
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null==dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
            dlgYesNo.SetLine(1,strFileName);
            dlgYesNo.SetLine(2, "");
            dlgYesNo.SetLine(3, "");
            dlgYesNo.DoModal(GetID);

            if (!dlgYesNo.IsConfirmed) break;
            DoDeleteItem(pItemTmp);
          }
        }

        if (!bStackedFile)
        {
          // delete single file
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null==dlgYesNo) return;
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
          dlgYesNo.SetLine(1,strFileName);
          dlgYesNo.SetLine(2, "");
          dlgYesNo.SetLine(3, "");
          dlgYesNo.DoModal(GetID);

          if (!dlgYesNo.IsConfirmed) return;
          DoDeleteItem(item);
        }
      }
      else
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null==dlgYesNo) return;
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
        dlgYesNo.SetLine(1,strFileName);
        dlgYesNo.SetLine(2, "");
        dlgYesNo.SetLine(3, "");
        dlgYesNo.DoModal(GetID);

        if (!dlgYesNo.IsConfirmed) return;
        DoDeleteItem(item);
      }
      
			m_iItemSelected=GetSelectedItemNo();
			if (m_iItemSelected>0) m_iItemSelected--;
			LoadDirectory(m_strDirectory);
			if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,m_iItemSelected);
			}
		}

    void DoDeleteItem(GUIListItem item)
    {
      if (item.IsFolder)
      {
				if (item.IsRemote) return;
        if (item.Label != "..")
        {
          ArrayList items = new ArrayList();
          items=m_directory.GetDirectoryUnProtected(item.Path,false);
          foreach(GUIListItem subItem in items)
          {
            DoDeleteItem(subItem);
          }
          Utils.DirectoryDelete(item.Path);
        }
      }
      else		
      {
				VideoDatabase.DeleteMovie(item.Path);
				TVDatabase.RemoveRecordedTVByFileName(item.Path);

				if (item.IsRemote) return;
				Utils.FileDelete(item.Path);
			}
		}

		static public bool IsFolderPinProtected(string folder)
		{
			int pinCode=0;
			if (m_directory.IsProtectedShare(folder,out pinCode)) return true;
			return false;
		}
		static void DownloadThumnail(string folder,string url, string name)
		{
			if (url==null) return;
			if (url.Length==0) return;
			string strThumb = Utils.GetCoverArtName(folder,name);
			string LargeThumb = Utils.GetLargeCoverArtName(folder,name);
			if (!System.IO.File.Exists(strThumb))
			{
				string strExtension;
				strExtension = System.IO.Path.GetExtension(url);
				if (strExtension.Length > 0)
				{
					string strTemp = "temp";
					strTemp += strExtension;
					Utils.FileDelete(strTemp);
             
					Utils.DownLoadImage(url, strTemp);
					if (System.IO.File.Exists(strTemp))
					{
						MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
						MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);
					}
					else Log.Write("Unable to download {0}->{1}", url,strTemp);
					Utils.FileDelete(strTemp);
				}
			}
		}
		static void DownloadDirector(IMDBMovie movieDetails)
		{
			string actor=movieDetails.Director;
			string strThumb = Utils.GetCoverArtName(Thumbs.MovieActors,actor);
			if (!System.IO.File.Exists(strThumb))
			{
				imdb.FindActor(actor);
				IMDBActor imdbActor=new IMDBActor();
				for (int x=0; x < imdb.Count;++x)
				{
					imdb.GetActorDetails(imdb[x],out imdbActor);
					if (imdbActor.ThumbnailUrl!=null && imdbActor.ThumbnailUrl.Length>0) break;
				}
				if (imdbActor.ThumbnailUrl!=null)
				{
					if (imdbActor.ThumbnailUrl.Length!=0)
					{
						DownloadThumnail(Thumbs.MovieActors,imdbActor.ThumbnailUrl,actor);
					}
					else Log.Write("url=empty for actor {0}", actor);
				}
				else Log.Write("url=null for actor {0}", actor);
			}
		}
		static void DownloadActors(IMDBMovie movieDetails)
		{
			string[] actors=movieDetails.Cast.Split('\n');
			if (actors.Length>1)
			{
				for (int i=1; i < actors.Length;++i)
				{
					int pos =actors[i].IndexOf(" as ");
					if (pos <0) continue;
					string actor=actors[i].Substring(0,pos);
					string strThumb = Utils.GetCoverArtName(Thumbs.MovieActors,actor);
					if (!System.IO.File.Exists(strThumb))
					{
						imdb.FindActor(actor);
						IMDBActor imdbActor=new IMDBActor();
						for (int x=0; x < imdb.Count;++x)
						{
							imdb.GetActorDetails(imdb[x],out imdbActor);
							if (imdbActor.ThumbnailUrl!=null && imdbActor.ThumbnailUrl.Length>0) break;
						}
						if (imdbActor.ThumbnailUrl!=null)
						{
							if (imdbActor.ThumbnailUrl.Length!=0)
							{
								DownloadThumnail(Thumbs.MovieActors,imdbActor.ThumbnailUrl,actor);
							}
							else Log.Write("url=empty for actor {0}", actor);
						}
						else Log.Write("url=null for actor {0}", actor);
					}
				}
			}
		}

		static public void PlayMovie(int iMovieId)
		{
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
          
				bool askBeforePlayingDVDImage = false;

				using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
				}

				if (!askBeforePlayingDVDImage)
				{
					GUIVideoFiles.PlayMountedImageFile(GUIWindowManager.ActiveWindow, (string)movies[0]);
				}
				else
				{
					// GetDirectory mounts the image file
					ArrayList itemlist = m_directory.GetDirectory((string)movies[0]);
				}

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
				if ((Utils.ShouldStack(pItemTmp.Path, (string)movies[0])) && (movies.Count > 1))
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
						dlg.DoModal(GUIWindowManager.ActiveWindow);
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
    

		#region IProgress Members

		public void OnProgress(string line1, string line2, string line3, int percent)
		{
			if (!GUIWindowManager.IsRouted) return;
			GUIDialogProgress 	pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			pDlgProgress.SetLine(1, line1);
			pDlgProgress.SetLine(2, line2);
			pDlgProgress.SetPercentage(percent);
			pDlgProgress.Progress();
		}

		#endregion
	}
}
