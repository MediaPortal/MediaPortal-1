using System;
using System.Collections;
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
	public class GUIMusicPlayList : GUIWindow
	{
		enum Controls
		{
			CONTROL_BTNVIEWASICONS = 2, 
			CONTROL_BTNSHUFFLE = 20, 
			CONTROL_BTNSAVE = 21, 
			CONTROL_BTNCLEAR = 22, 

			CONTROL_BTNPLAY = 23, 
			CONTROL_BTNNEXT = 24, 
			CONTROL_BTNPREVIOUS = 25,
			//added by Sam
			CONTROL_BTNPSHUFFLE = 26,

			CONTROL_LABELFILES = 12, 

			CONTROL_LIST = 50, 
			CONTROL_THUMBS = 51

		};
		#region Base variabeles
		enum View
		{
			VIEW_AS_LIST = 0, 
			VIEW_AS_ICONS = 1, 
			VIEW_AS_LARGEICONS = 2, 
		}
		View currentView = View.VIEW_AS_LIST;
		
		DirectoryHistory m_history = new DirectoryHistory();
		string m_strDirectory = "";
		int m_iItemSelected = -1;
		int m_iLastControl = 0;
		int m_nTempPlayListWindow = 0;
		string m_strTempPlayListDirectory = "";
    string m_strCurrentFile = "";
    bool m_bUseID3 = true;
		VirtualDirectory m_directory = new VirtualDirectory();
		#endregion
    
    Database	m_database = new Database();

		//added by Sam
  const int MaxNumPShuffleSongPredict = 12;
		private bool PShuffleOn = false;

		public GUIMusicPlayList()
		{
			GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST;
      
			m_directory.AddDrives();
			m_directory.SetExtensions(Utils.AudioExtensions);
      LoadSettings();
    }
    ~GUIMusicPlayList()
    {
    }

		public override bool Init()
		{
			m_strDirectory = System.IO.Directory.GetCurrentDirectory();
			//added by Sam
			 GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
			return Load(GUIGraphicsContext.Skin + @"\myMusicplaylist.xml");
		}
//added by Sam
		void OnThreadMessage(GUIMessage message)
		{
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED: 
					if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC && PShuffleOn==true)
					{
						PlayList pl = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
						pl.Remove(pl[0].FileName );
						UpdatePartyShuffle();
						PlayListPlayer.CurrentSong = 0;
					}
					break;

				//special case for when the next button is pressed - stopping the prev song does not cause a Playback_Ended event
				case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
					if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC && PShuffleOn==true && PlayListPlayer.CurrentSong!=0)
					{
						PlayList pl = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
						pl.Remove(pl[0].FileName );
						UpdatePartyShuffle();
						PlayListPlayer.CurrentSong = 0;
					}
					break;
			}
		}
//end of changes by Sam

    #region Serialisation
    void LoadSettings()
    {
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp = "";
        strTmp = (string)xmlreader.GetValue("musicplaylist","viewby");
        if (strTmp != null)
        {
          if (strTmp == "list") currentView = View.VIEW_AS_LIST;
          else if (strTmp == "icons") currentView = View.VIEW_AS_ICONS;
          else if (strTmp == "largeicons") currentView = View.VIEW_AS_LARGEICONS;
        }
        m_bUseID3 = xmlreader.GetValueAsBool("musicfiles","showid3",true);
      }
    }

    void SaveSettings()
    {
      using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentView)
        {
          case View.VIEW_AS_LIST : 
            xmlwriter.SetValue("musicplaylist","viewby","list");
            break;
          case View.VIEW_AS_ICONS : 
            xmlwriter.SetValue("musicplaylist","viewby","icons");
            break;
          case View.VIEW_AS_LARGEICONS : 
            xmlwriter.SetValue("musicplaylist","viewby","largeicons");
            break;
        }
      }
    }
    #endregion


		#region BaseWindow Members
		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
			{
				return;
			}

			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				GUIWindowManager.PreviousWindow(); //ActivateWindow( (int)GUIWindow.Window.WINDOW_HOME);
				return;
			}
			if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
			{
				GUIWindowManager.PreviousWindow();
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
					m_database.Open();
					LoadSettings();
					ShowThumbPanel();
          
					LoadDirectory("");
          
					if (m_iItemSelected >= 0)
					{
						GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, m_iItemSelected);
						GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, m_iItemSelected);
					}
					if ((m_iLastControl == (int)Controls.CONTROL_THUMBS || m_iLastControl == (int)Controls.CONTROL_LIST) && GetItemCount() <= 0)
					{
						m_iLastControl = (int)Controls.CONTROL_BTNVIEWASICONS;
						GUIControl.FocusControl(GetID, m_iLastControl);
					}
          SelectCurrentPlayingSong();
					return true;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					m_iItemSelected = GetSelectedItemNo();
					SaveSettings();
          m_database.Close();
          break;

				case GUIMessage.MessageType.GUI_MSG_CLICKED : 
					int iControl = message.SenderControlId;
					if (iControl == (int)Controls.CONTROL_BTNVIEWASICONS)
					{
						switch (currentView)
						{
							case View.VIEW_AS_LIST : 
								currentView = View.VIEW_AS_ICONS;
								break;
							case View.VIEW_AS_ICONS : 
								currentView = View.VIEW_AS_LARGEICONS;
								break;
							case View.VIEW_AS_LARGEICONS : 
								currentView = View.VIEW_AS_LIST;
								break;
						}
            ShowThumbPanel();
            GUIControl.FocusControl(GetID, iControl);
					}
          
					if (iControl == (int)Controls.CONTROL_THUMBS || iControl == (int)Controls.CONTROL_LIST)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
						GUIGraphicsContext.SendMessage(msg);
						int iItem = (int)msg.Param1;
						int iAction = (int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
						{
							OnClick(iItem);
						}
						if (iAction == (int)Action.ActionType.ACTION_QUEUE_ITEM)
						{
							OnQueueItem(iItem);
						}

					}

					else if (iControl == (int)Controls.CONTROL_BTNSHUFFLE)
					{
						ShufflePlayList();
					}
					else if (iControl == (int)Controls.CONTROL_BTNSAVE)
					{
						SavePlayList();
					}
					else if (iControl == (int)Controls.CONTROL_BTNCLEAR)
					{
						ClearPlayList();
					}
					else if (iControl == (int)Controls.CONTROL_BTNPLAY)
					{
						PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
						PlayListPlayer.Reset();
						PlayListPlayer.Play(GetSelectedItemNo());
						UpdateButtons();
					}
					else if (iControl == (int)Controls.CONTROL_BTNNEXT)
					{
						PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
						PlayListPlayer.PlayNext(true);
            SelectCurrentPlayingSong();
					}
					else if (iControl == (int)Controls.CONTROL_BTNPREVIOUS)
					{
						PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
						PlayListPlayer.PlayPrevious();
            SelectCurrentPlayingSong();
					}

						//added by Sam
					else if (iControl == (int)Controls.CONTROL_BTNPSHUFFLE )
					{
						//get state of button
						GUIToggleButtonControl btnPShuffle = GetControl((int)Controls.CONTROL_BTNPSHUFFLE ) as GUIToggleButtonControl;
            if (btnPShuffle==null) return true;
						if (btnPShuffle.Selected) 
						{
							PShuffleOn = true;
							UpdatePartyShuffle();
							LoadDirectory("");
							GUIListItem item=GetItem(0);
              if (item!=null) item.Shaded = false;
							PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
							PlayListPlayer.Reset();
							PlayListPlayer.Play(0);
						}
						else PShuffleOn=false;

						UpdateButtons();
					}
					break;

				case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED : 
				{
					for (int i = 0; i < GetItemCount(); ++i)
					{
						GUIListItem item = GetItem(i);
						if (item != null && item.Selected)
						{
							item.Selected = false;
							break;
						}
					}

					UpdateButtons();
				}
					break;

				case GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED : 
				{
					//	global playlist changed outside playlist window
					//added by Sam
					//if party shuffle...
					if (PShuffleOn==true) 
					{
						LoadDirectory("");
						UpdateButtons();
					}
					//ended changes

					if ((m_iLastControl == (int)Controls.CONTROL_THUMBS || m_iLastControl == (int)Controls.CONTROL_LIST) && GetItemCount() <= 0)
					{
						m_iLastControl = (int)Controls.CONTROL_BTNVIEWASICONS;
						GUIControl.FocusControl(GetID, m_iLastControl);
					}

					SelectCurrentPlayingSong();
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
			GUIGraphicsContext.SendMessage(msg);
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
      GUIControl.FocusControl(GetID, iControl);
      

			string strLine = "";
			switch (currentView)
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
			}
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEWASICONS, strLine);


			if (GetItemCount() > 0)
			{
				GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BTNCLEAR);
				GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BTNPLAY);
				if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
				{
					GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BTNNEXT);
					GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BTNPREVIOUS);
				}
				else
				{
					GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNNEXT);
					GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNPREVIOUS);
				}
			}
			else
			{
				GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNCLEAR);
				GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNPLAY);
				GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNNEXT);
				GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNPREVIOUS);
			}

			//disable shuffle/save/previous if party shuffle is on
			GUIToggleButtonControl btnPShuffle = (GUIToggleButtonControl)GetControl((int)Controls.CONTROL_BTNPSHUFFLE );
			if (btnPShuffle.Selected) 
			{
				GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNSHUFFLE);
				GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNPREVIOUS);
				GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNSAVE );
			}
			else
			{
				GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BTNSHUFFLE);
				GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BTNSAVE );
			}
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

    void OnRetrieveMusicInfo(ref ArrayList items)
    {
      if (items.Count <= 0) return;
      Database dbs = new Database();
      dbs.Open();
      Song song = new Song();
      foreach (GUIListItem item in items)
      {
        string strExtension = System.IO.Path.GetExtension(item.Path);
        if (strExtension.Equals(".cda"))
        {
          if (GUIMusicFiles.MusicCD != null)
          {
            string strfile=item.Label.ToLower();
            if (strfile.IndexOf(".cda")>=0) strfile=strfile.Substring(0,item.Label.Length-4); // skip .cda
            int pos=strfile.IndexOf("track") + "track".Length;
            int iTrack=Convert.ToInt32(strfile.Substring(pos));
            MusicTag tag = new MusicTag();
            Freedb.CDTrackDetail track = GUIMusicFiles.MusicCD.getTrack(iTrack);

            tag = new MusicTag();
            tag.Album = GUIMusicFiles.MusicCD.Title;
            tag.Artist = track.Artist == null ? GUIMusicFiles.MusicCD.Artist : track.Artist;
            tag.Genre = GUIMusicFiles.MusicCD.Genre;
            tag.Duration = track.Duration;
            tag.Title = track.Title;
            tag.Track = track.TrackNumber;
            item.MusicTag = tag;
          }
        }
        else
        {
          if (dbs.GetSongByFileName(item.Path, ref song))
          {
            MusicTag tag = new MusicTag();
            tag.Album = song.Album;
            tag.Artist = song.Artist;
            tag.Genre = song.Genre;
            tag.Duration = song.Duration;
            tag.Title = song.Title;
            tag.Track = song.Track;
            item.MusicTag = tag;
          }
          else if (m_bUseID3)
          {
            item.MusicTag = TagReader.TagReader.ReadTag(item.Path);
          }
        }
      }
      dbs.Close();
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

			PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
			/* copy playlist from general playlist*/
			int iCurrentSong = -1;
			if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
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

        Utils.SetDefaultIcons(pItem);
        if (item.Played)
        {
          pItem.Shaded = true;
        }

				if (item.Duration > 0)
				{
					int nDuration = item.Duration;
					if (nDuration > 0)
					{
						string str = Utils.SecondsToHMSString(nDuration);
						pItem.Label2 = str;
					}
					else
						pItem.Label2 = "";
				}
        pItem.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        itemlist.Add(pItem);
      }
      OnRetrieveMusicInfo(ref itemlist);
			iCurrentSong = 0;
			strFileName = "";
			//	Search current playlist item
			if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(m_strDirectory) >= 0 && g_Player.Playing 
				&& PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP) 
				|| (GetID == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC 
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
 
			string strSelectedItem = m_history.Get(m_strDirectory);
			int iItem = 0;
			foreach (GUIListItem item in itemlist)
			{
				GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_LIST, item);
				GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_THUMBS, item);
        
				//	synchronize playlist with current directory
				if (strFileName.Length > 0 && item.Path == strFileName)
				{
					item.Selected = true;
				}
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
      SetLabels();
      UpdateButtons();
		}
		#endregion

    
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
          }
					
          int nDuration = tag.Duration;
          if (nDuration > 0)
          {
            item.Label2 = Utils.SecondsToHMSString(nDuration);
          }
        }
      }
    }


		void ClearFileItems()
		{
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS);
		}
  
		void ClearPlayList()
		{
			//added/changed by Sam
			//if party shuffle
			if (PShuffleOn==true) 
			{
				if (g_Player.Playing && PlayListPlayer.CurrentPlaylist==PlayListPlayer.PlayListType.PLAYLIST_MUSIC )
				{
					for (int i=1; i<GetItemCount(); i++)
					{
						PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Remove(GetItem(i).Path );
					}
				}
				else
				{
					for (int i=0; i<GetItemCount(); i++)
					{
						PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Remove(GetItem(i).Path );
					}
				}
				//LoadDirectory("");
				UpdatePartyShuffle();
				LoadDirectory("");
			}
				//otherwise, if not party shuffle...
			else
			{			
				ClearFileItems();
				PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Clear();
				if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
					PlayListPlayer.Reset();
				LoadDirectory("");
				UpdateButtons();
				GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BTNVIEWASICONS);
			}
			//ended changes
		}

		void OnClick(int iItem)
		{
			GUIListItem item = GetSelectedItem();
			if (item == null) return;
			if (item.IsFolder) return;
      
			//added/changed by Sam
			//check if party shuffle is on
			if (PShuffleOn==true) 
			{
				if (g_Player.Playing && PlayListPlayer.CurrentPlaylist==PlayListPlayer.PlayListType.PLAYLIST_MUSIC ) 
				{
					for (int i=1; i<GetSelectedItemNo(); i++)
					{
						PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Remove(GetItem(i).Path );
					}
					//LoadDirectory("");
					UpdatePartyShuffle();
					LoadDirectory("");
				}
				else
				{
					for (int i=0; i<GetSelectedItemNo(); i++)
					{
						PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Remove(GetItem(i).Path );
					}
					//LoadDirectory("");
					UpdatePartyShuffle();
					LoadDirectory("");
					PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
					PlayListPlayer.Reset();
					PlayListPlayer.Play(0);
				}
			}
				//otherwise if party shuffle is not on, do this...
			else
			{
				string strPath = item.Path;
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
				PlayListPlayer.Reset();
				PlayListPlayer.Play(iItem);
				SelectCurrentPlayingSong();
			}
			//ended changes
		}
    
		void OnQueueItem(int iItem)
		{
			RemovePlayListItem(iItem);
		}

		void RemovePlayListItem(int iItem)
		{
			//added by Sam
			if (PShuffleOn==true && g_Player.Playing && PlayListPlayer.CurrentPlaylist==PlayListPlayer.PlayListType.PLAYLIST_MUSIC )
			{
				if (iItem == 0) return;
			}

			GUIListItem pItem = GetItem(iItem);
      if (pItem==null) return;
			string strFileName = pItem.Path;

			PlayListPlayer.Remove(PlayListPlayer.PlayListType.PLAYLIST_MUSIC,strFileName);
      
			//added by Sam
			//check if party shuffle is on
			if (PShuffleOn==true) UpdatePartyShuffle();

			LoadDirectory(m_strDirectory);
			UpdateButtons();
			GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iItem);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iItem);
      SelectCurrentPlayingSong();
		}

		void ShufflePlayList()
		{

			ClearFileItems();
			PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
	
      if (playlist.Count <= 0) return;
      string strFileName = "";
      if (PlayListPlayer.CurrentSong >= 0)
      {
        if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
        {
          PlayList.PlayListItem item = playlist[PlayListPlayer.CurrentSong];
          strFileName = item.FileName;
        }
      }
			playlist.Shuffle();
			if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
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

      SelectCurrentPlayingSong();
		}
		
		
    bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return false;
      keyboard.Reset();
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        return true;
      }
      return false;
    }

		void SavePlayList()
		{
			string strNewFileName = "";
			if (GetKeyboard(ref strNewFileName))
			{
				string strPath = System.IO.Path.GetFileNameWithoutExtension(strNewFileName);
        string strPlayListPath = "";
        using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
        {
          strPlayListPath = xmlreader.GetValueAsString("music","playlists","");
          strPlayListPath = Utils.RemoveTrailingSlash(strPlayListPath);
        }

		

        strPath += ".m3u";
        if (strPlayListPath.Length != 0)
        {
          strPath = strPlayListPath + @"\" + strPath;
        }
				PlayListM3U playlist = new PlayListM3U();
				for (int i = 0; i < GetItemCount(); ++i)
				{
					GUIListItem pItem = GetItem(i);
					PlayList.PlayListItem newItem = new PlayList.PlayListItem();
					newItem.FileName = pItem.Path;
					newItem.Description = pItem.Label;
					newItem.Duration = pItem.Duration;
          newItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
					playlist.Add(newItem);
				}
				playlist.Save(strPath);
			}
		}
    public override void Process()
    {
      if (!m_strCurrentFile.Equals(g_Player.CurrentFile))
      {
        m_strCurrentFile = g_Player.CurrentFile;
        GUIMessage msg;
        if (g_Player.Playing)
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED, GetID, 0, 0, 0, 0, null);
          OnMessage(msg);
        }
        else
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED, GetID, 0, 0, 0, 0, null);
          OnMessage(msg);
        }
      }
    }
    void SelectCurrentPlayingSong()
    {
      if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
      {
        int iSong = PlayListPlayer.CurrentSong;
        if (iSong >= 0 && iSong <= GetItemCount())
        {
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iSong);
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iSong);

          GUIControl.FocusItemControl(GetID, (int)Controls.CONTROL_LIST, iSong);
        }
      }
    }


		//added by Sam
		void AddRandomSongToPlaylist(ref Song song)
		{
			//check duplication
			PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
			for (int i=0; i<playlist.Count; i++)
			{
				PlayList.PlayListItem item = playlist[i];
				if (item.FileName == song.FileName) return;
			}
			
			//add to playlist
      PlayList.PlayListItem playlistItem = new PlayList.PlayListItem();
      playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
			playlistItem.FileName = song.FileName ;
			playlistItem.Description = song.Track + ". " + song.Artist + " - " + song.Title ;
			playlistItem.Duration = song.Duration ;

			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Add(playlistItem);

		}

		//added by Sam
		void UpdatePartyShuffle()
		{
			Database dbs = new Database();
			dbs.Open();
			
			PlayList list=PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
			if (list.Count>=MaxNumPShuffleSongPredict) return;
			
			int i;
			Song song = new Song();	
			//if not enough songs, add all available songs
			if (dbs.GetNumOfSongs()<MaxNumPShuffleSongPredict)
			{			
				ArrayList songs = new ArrayList();
				dbs.GetAllSongs(ref songs);
				
				for (i=0; i<songs.Count; i++)
				{
					song = (Song)songs[i];
					AddRandomSongToPlaylist(ref song);
				}
			}
				//otherwise add until number of songs = MaxNumPShuffleSongPredict
			else
			{
				i=list.Count;
				while (i<MaxNumPShuffleSongPredict)
				{
					song.Clear();
					dbs.GetRandomSong(ref song);	
					AddRandomSongToPlaylist( ref song);
					i=list.Count;
				}
			}
				//LoadDirectory(""); - will cause errors when playlist screen is not active
				dbs.Close();
		}
	}
}
