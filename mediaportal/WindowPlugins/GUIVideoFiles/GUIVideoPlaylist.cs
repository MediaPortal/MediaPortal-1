using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;
using MediaPortal.Video.Database;


namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
	public class GUIVideoPlayList : GUIVideoBaseWindow
	{
		#region Base variabeles
		
    
		DirectoryHistory	m_history = new DirectoryHistory();
		string						m_strDirectory = String.Empty;
		int								m_iItemSelected = -1;
		int								previousControlId = 0;
		int								m_nTempPlayListWindow = 0;
		string						m_strTempPlayListDirectory = String.Empty;
		VirtualDirectory	m_directory = new VirtualDirectory();
		#endregion

		[SkinControlAttribute(20)]			protected GUIButtonControl btnShuffle=null;
		[SkinControlAttribute(21)]			protected GUIButtonControl btnSave=null;
		[SkinControlAttribute(22)]			protected GUIButtonControl btnClear=null;
		[SkinControlAttribute(23)]			protected GUIButtonControl btnPlay=null;
		[SkinControlAttribute(24)]			protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl btnPrevious=null;

		public GUIVideoPlayList()
		{
			GetID = (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST;
    }

		public override bool Init()
		{
			m_strDirectory = System.IO.Directory.GetCurrentDirectory();
			bool result= Load(GUIGraphicsContext.Skin + @"\myvideoplaylist.xml");
			m_directory.AddDrives();
			m_directory.SetExtensions(Utils.VideoExtensions);
			return result;
		}

		protected override string SerializeName
		{
			get
			{
				return "videoplaylist";
			}
		}



		#region BaseWindow Members
		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
			{
				GUIWindowManager.ShowPreviousWindow();
				return;
			}

			base.OnAction(action);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadSettings();
			LoadDirectory(String.Empty);
			if ((previousControlId == facadeView.GetID) && facadeView.Count <= 0)
			{
				previousControlId = btnViewAs.GetID;
				GUIControl.FocusControl(GetID, previousControlId);
			}
			if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
			{
				int iSong = PlayListPlayer.CurrentSong;
				if (iSong >= 0 && iSong <= facadeView.Count)
				{
					GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
				}
			}
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			m_iItemSelected = facadeView.SelectedListItemIndex;
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnShuffle)
			{
				OnShufflePlayList();
			}
			else if (control==btnSave)
			{
				OnSavePlayList();
			}
			else if (control==btnClear)
			{
				OnClearPlayList();
			}
			else if (control==btnPlay)
			{
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
				PlayListPlayer.Reset();
				PlayListPlayer.Play(facadeView.SelectedListItemIndex);
				UpdateButtonStates();
			}
			else if (control==btnNext)
			{
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
				GUIVideoFiles.PlayMovieFromPlayList(true);
			}
			else if (control==btnPrevious)
			{
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
				PlayListPlayer.PlayPrevious();
			}
		}


		public override bool OnMessage(GUIMessage message)
		{
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED : 
				{
					for (int i = 0; i < facadeView.Count; ++i)
					{
						GUIListItem item = facadeView[i];
						if (item != null && item.Selected)
						{
							item.Selected = false;
							break;
						}
					}

					UpdateButtonStates();
				}
					break;

				case GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED : 
				{
					//	global playlist changed outside playlist window
					LoadDirectory(String.Empty);

					if (previousControlId == facadeView.GetID && facadeView.Count <= 0)
					{
						previousControlId = btnViewAs.GetID;
						GUIControl.FocusControl(GetID, previousControlId);
          }
          SelectCurrentVideo();

				}
					break;
			}
			return base.OnMessage(message);
		}

		protected override void UpdateButtonStates()
		{
			base.UpdateButtonStates();
			if (facadeView.Count > 0)
			{
				btnClear.Disabled=false;
				btnPlay.Disabled=false;
				if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
				{
					btnNext.Disabled=false;
					btnPrevious.Disabled=false;
				}
				else
				{
					btnNext.Disabled=true;
					btnPrevious.Disabled=true;
				}
			}
			else
			{
				btnClear.Disabled=true;
				btnPlay.Disabled=true;
				btnNext.Disabled=true;
				btnPrevious.Disabled=true;
			}
		}

		protected override void LoadDirectory(string strNewDirectory)
		{
			GUIListItem SelectedItem = facadeView.SelectedListItem;
			if (SelectedItem != null) 
			{
				if (SelectedItem.IsFolder && SelectedItem.Label != "..")
				{
					m_history.Set(SelectedItem.Label, m_strDirectory);
				}
			}
			m_strDirectory = strNewDirectory;
			facadeView.Clear();
            
			string strObjects = String.Empty;

			ArrayList itemlist = new ArrayList();

			PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO);
			/* copy playlist from general playlist*/
			int iCurrentSong = -1;
			if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
				iCurrentSong = PlayListPlayer.CurrentSong;

			string strFileName;
			for (int i = 0; i < playlist.Count; ++i)
			{
				PlayList.PlayListItem item = playlist[i];
				strFileName = item.FileName;
		
				GUIListItem pItem = new GUIListItem(item.Description);
				pItem.Path = strFileName;
				pItem.IsFolder = false;
				//pItem.m_bIsShareOrDrive = false;

				if (item.Duration > 0)
				{
					int nDuration = item.Duration;
					if (nDuration > 0)
					{
						string str = Utils.SecondsToHMSString(nDuration);
						pItem.Label2 = str;
					}
					else
						pItem.Label2 = String.Empty;
				}
				itemlist.Add(pItem);
        Utils.SetDefaultIcons(pItem);
        
      }

			iCurrentSong = 0;
			strFileName = String.Empty;
			//	Search current playlist item
			if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(m_strDirectory) >= 0 && g_Player.Playing 
				&& PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP) 
				|| (GetID == (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO 
				&& g_Player.Playing))
			{
				iCurrentSong = PlayListPlayer.CurrentSong;
				if (iCurrentSong >= 0)
				{
					playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.CurrentPlaylist);
					if (iCurrentSong < playlist.Count)
					{
						PlayList.PlayListItem item = playlist[iCurrentSong];
						strFileName = item.FileName;
					}
				}
			}

      SetIMDBThumbs(itemlist);
      
			string strSelectedItem = m_history.Get(m_strDirectory);
			int iItem = 0;
			foreach (GUIListItem item in itemlist)
			{
				facadeView.Add(item);
        
				//	synchronize playlist with current directory
				if (strFileName.Length > 0 && item.Path == strFileName)
				{
					item.Selected = true;
				}
			}
			for (int i = 0; i < facadeView.Count; ++i)
			{
				GUIListItem item = facadeView[i];
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

		void ClearFileItems()
		{
			facadeView.Clear();
		}
  
		void OnClearPlayList()
		{
      m_iItemSelected = -1;
			ClearFileItems();
			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Clear();
			if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
				PlayListPlayer.Reset();
			LoadDirectory(String.Empty);
			UpdateButtonStates();
			GUIControl.FocusControl(GetID, btnViewAs.GetID);
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
            }
          }
        }
      }

      movies.Clear();
      VideoDatabase.GetMoviesByPath(m_strDirectory, ref movies);
      for (int x = 0; x < items.Count; ++x)
      {
        pItem = (GUIListItem)items[x];
        if (!pItem.IsFolder)
        {
          IMDBMovie info = new IMDBMovie();
          int movieid=VideoDatabase.GetMovieInfo(pItem.Path,ref info);
          if (movieid>=0)
          {
            string strThumb;
            strThumb = Utils.GetCoverArt(Thumbs.MovieTitle, info.Title );
            if (System.IO.File.Exists(strThumb))
            {
              pItem.ThumbnailImage = strThumb;
              pItem.IconImageBig = strThumb;
              pItem.IconImage = strThumb;
            }
          }
        }
      }
    }


		protected override void OnClick(int itemNumber)
		{
      m_iItemSelected = facadeView.SelectedListItemIndex;
			GUIListItem item = facadeView.SelectedListItem;
			if (item == null) return;
			if (item.IsFolder) return;
      
			string strPath = item.Path;
			PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
			PlayListPlayer.Reset();
			PlayListPlayer.Play(itemNumber);
		}
    
		protected override void OnQueueItem(int iItem)
		{
			RemovePlayListItem(iItem);
		}

		void RemovePlayListItem(int iItem)
		{
			GUIListItem pItem = facadeView[iItem];
      if (pItem==null) return;
			string strFileName = pItem.Path;
			
      PlayListPlayer.Remove(PlayListPlayer.PlayListType.PLAYLIST_VIDEO,strFileName);

			LoadDirectory(m_strDirectory);
			UpdateButtonStates();
			GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      SelectCurrentVideo();
		}

		void OnShufflePlayList()
		{
      m_iItemSelected = facadeView.SelectedListItemIndex;
			ClearFileItems();
			PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO);
	
      if (playlist.Count <= 0) return;
      string strFileName = String.Empty;
      if (PlayListPlayer.CurrentSong >= 0)
      {
        if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
        {
          PlayList.PlayListItem item = playlist[PlayListPlayer.CurrentSong];
          strFileName = item.FileName;
        }
      }
			playlist.Shuffle();
			if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
				PlayListPlayer.Reset();

			if (strFileName.Length > 0) 
			{
				for (int i = 0; i < playlist.Count; i++)
				{
					PlayList.PlayListItem item = playlist[i];
					if (item.FileName == strFileName)
						PlayListPlayer.CurrentSong = i;
				}
			}

			LoadDirectory(m_strDirectory);

		}
		
		void OnSavePlayList()
		{
      m_iItemSelected = facadeView.SelectedListItemIndex;
			string strNewFileName = String.Empty;
			if (GetKeyboard(ref strNewFileName))
			{
        string strPlayListPath = String.Empty;
        using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          strPlayListPath = xmlreader.GetValueAsString("movies","playlists",String.Empty);
          strPlayListPath = Utils.RemoveTrailingSlash(strPlayListPath);
        }

				string strPath = System.IO.Path.GetFileNameWithoutExtension(strNewFileName);
		
				strPath += ".m3u";
        if (strPlayListPath.Length != 0)
        {
          strPath = strPlayListPath + @"\" + strPath;
        }
				PlayListM3U playlist = new PlayListM3U();
				for (int i = 0; i < facadeView.Count; ++i)
				{
					GUIListItem pItem = facadeView[i];
					PlayList.PlayListItem newItem = new PlayList.PlayListItem();
					newItem.FileName = pItem.Path;
					newItem.Description = pItem.Label;
          newItem.Duration = pItem.Duration;
          newItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
					playlist.Add(newItem);
				}
				playlist.Save(strPath);
			}
		}
    void SelectCurrentVideo()
    {
      if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
      {
        int iSong = PlayListPlayer.CurrentSong;
        if (iSong >= 0 && iSong <= facadeView.Count)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
        }
      }
    }
	}
}
