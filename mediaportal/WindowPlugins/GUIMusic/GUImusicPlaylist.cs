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
	public class GUIMusicPlayList : GUIMusicBaseWindow
	{
		#region Base variabeles
		DirectoryHistory m_history = new DirectoryHistory();
		string m_strDirectory = "";
		int m_iItemSelected = -1;
		int m_iLastControl = 0;
		int m_nTempPlayListWindow = 0;
		string m_strTempPlayListDirectory = "";
    string m_strCurrentFile = "";
		VirtualDirectory m_directory = new VirtualDirectory();
		const int MaxNumPShuffleSongPredict = 12;
		private bool PShuffleOn = false;
		#endregion
    
		[SkinControlAttribute(20)]			protected GUIButtonControl btnShuffle=null;
		[SkinControlAttribute(21)]			protected GUIButtonControl btnSave=null;
		[SkinControlAttribute(22)]			protected GUIButtonControl btnClear=null;
		[SkinControlAttribute(23)]			protected GUIButtonControl btnPlay=null;
		[SkinControlAttribute(24)]			protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl btnPrevious=null;
		[SkinControlAttribute(26)]			protected GUIToggleButtonControl btnPartyShuffle=null;


		public GUIMusicPlayList()
		{
			GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST;
      
			m_directory.AddDrives();
			m_directory.SetExtensions(Utils.AudioExtensions);
    }

		#region overrides
		public override bool Init()
		{
			m_strDirectory = System.IO.Directory.GetCurrentDirectory();
			//added by Sam
			GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
			return Load(GUIGraphicsContext.Skin + @"\myMusicplaylist.xml");
		}
		protected override string SerializeName
		{
			get
			{
				return "mymusicplaylist";
			}
		}
		protected override bool AllowView(View view)
		{
			if (view==View.Albums) return false;
			if (view==View.FilmStrip) return false;
			return true;
		}
		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
			{
				GUIWindowManager.PreviousWindow();
				return;
			}

			base.OnAction(action);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
          
			LoadDirectory("");
          
			if (m_iItemSelected >= 0)
			{
				GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
			}
			if ((m_iLastControl == facadeView.GetID) && GetItemCount() <= 0)
			{
				m_iLastControl = btnViewAs.GetID;
				GUIControl.FocusControl(GetID, m_iLastControl);
			}
			if (GetItemCount() <=0)
			{
				GUIControl.FocusControl(GetID, btnViewAs.GetID);
			}
			SelectCurrentPlayingSong();

		}

		protected override void OnPageDestroy(int newWindowId)
		{
			m_iItemSelected = GetSelectedItemNo();
			base.OnPageDestroy (newWindowId);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);

			if (control == btnShuffle)
			{
				ShufflePlayList();
			}
			else if (control==btnSave)
			{
				SavePlayList();
			}
			else if (control == btnClear)
			{
				ClearPlayList();
			}
			else if (control == btnPlay)
			{
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
				PlayListPlayer.Reset();
				if (PShuffleOn == true)
				{
					for (int i=0; i<GetSelectedItemNo(); i++)
					{
						PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Remove(GetItem(i).Path );
					}
					UpdatePartyShuffle();
					//LoadDirectory("");
					PlayListPlayer.Play(0);
				}
				else
				{
					PlayListPlayer.Play(GetSelectedItemNo());
				}
				UpdateButtonStates();
			}
			else if (control==btnNext)
			{
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
				PlayListPlayer.PlayNext(true);
				SelectCurrentPlayingSong();
			}
			else if (control==btnPrevious)
			{
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
				PlayListPlayer.PlayPrevious();
				SelectCurrentPlayingSong();
			}
			else if (control == btnPartyShuffle)
			{
				//get state of button
				if (btnPartyShuffle.Selected) 
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

				UpdateButtonStates();
			}
		}

		
		public override bool OnMessage(GUIMessage message)
		{
			switch (message.Message)
			{
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

					UpdateButtonStates();
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
						UpdateButtonStates();
					}
					//ended changes

					if (m_iLastControl == facadeView.GetID && GetItemCount() <= 0)
					{
						m_iLastControl = btnViewAs.GetID;
						GUIControl.FocusControl(GetID, m_iLastControl);
					}

					SelectCurrentPlayingSong();
				}
					break;
			}
			return base.OnMessage(message);
		}


		protected override void UpdateButtonStates()
		{
			base.UpdateButtonStates ();
		
			if (GetItemCount() > 0)
			{
				GUIControl.EnableControl(GetID, btnClear.GetID);
				GUIControl.EnableControl(GetID, btnPlay.GetID);
				if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
				{
					GUIControl.EnableControl(GetID, btnNext.GetID);
					GUIControl.EnableControl(GetID, btnPrevious.GetID);
				}
				else
				{
					GUIControl.DisableControl(GetID, btnNext.GetID);
					GUIControl.DisableControl(GetID, btnPrevious.GetID);
				}
			}
			else
			{
				GUIControl.DisableControl(GetID, btnClear.GetID);
				GUIControl.DisableControl(GetID, btnPlay.GetID);
				GUIControl.DisableControl(GetID, btnNext.GetID);
				GUIControl.DisableControl(GetID, btnPrevious.GetID);
			}

			//disable shuffle/save/previous if party shuffle is on
			if (btnPartyShuffle.Selected) 
			{
				GUIControl.DisableControl(GetID, btnShuffle.GetID);
				GUIControl.DisableControl(GetID, btnNext.GetID);
				GUIControl.DisableControl(GetID, btnPrevious.GetID );
				GUIControl.DisableControl(GetID, btnSave.GetID );
			}
			else
			{
				GUIControl.EnableControl(GetID, btnShuffle.GetID);
			}
		}


		protected override void OnClick(int iItem)
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
					//LoadDirectory("");
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
    
		protected override  void OnQueueItem(int iItem)
		{
			RemovePlayListItem(iItem);
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

		#endregion
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
						LoadDirectory("");
					}
					break;
			}
		}

		
    void OnRetrieveMusicInfo(ref ArrayList items)
    {
      if (items.Count <= 0) return;
      MusicDatabase dbs = new MusicDatabase();
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
          else if (UseID3)
          {
            item.MusicTag = TagReader.TagReader.ReadTag(item.Path);
          }
        }
      }
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
			m_strDirectory = strNewDirectory;
			GUIControl.ClearControl(GetID, facadeView.GetID);
            
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
				facadeView.Add(item);
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
      SetLabels();
      UpdateButtonStates();
		}

    

		void ClearFileItems()
		{
			GUIControl.ClearControl(GetID, facadeView.GetID);
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
				UpdateButtonStates();
				GUIControl.FocusControl(GetID, btnViewAs.GetID);
			}
			//ended changes
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
			UpdateButtonStates();
			GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
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

		void SelectCurrentPlayingSong()
		{
			if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_MUSIC)
			{
				// delete prev. selected item
				for (int i = 0; i < GetItemCount(); ++i)
				{
					GUIListItem item = GetItem(i);
					if (item != null && item.Selected)
					{
						item.Selected = false;
						break;
					}
				}

				// set current item selected
				int iSong = PlayListPlayer.CurrentSong;
				if (iSong >= 0 && iSong <= GetItemCount())
				{
					GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
					GUIListItem item = GetItem(iSong);
					if (item != null) item.Selected = true;
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
			MusicDatabase dbs = new MusicDatabase();
			
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
		}
	}
}
