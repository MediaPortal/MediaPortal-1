/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

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
		string						currentFolder = String.Empty;
		int								currentSelectedItem = -1;
		int								previousControlId = 0;
		int								m_nTempPlayListWindow = 0;
		string						m_strTempPlayListDirectory = String.Empty;
		VirtualDirectory	m_directory = new VirtualDirectory();
    //PlayListPlayer playlistPlayer;
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
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

		public override bool Init()
		{
			currentFolder = System.IO.Directory.GetCurrentDirectory();
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
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
			{
        int iSong = playlistPlayer.CurrentSong;
				if (iSong >= 0 && iSong <= facadeView.Count)
				{
					GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
				}
			}
			UpdateButtonStates();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			currentSelectedItem = facadeView.SelectedListItemIndex;
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
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
        playlistPlayer.Reset();
        playlistPlayer.Play(facadeView.SelectedListItemIndex);
				UpdateButtonStates();
			}
			else if (control==btnNext)
			{
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
				GUIVideoFiles.PlayMovieFromPlayList(true);
			}
			else if (control==btnPrevious)
			{
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
        playlistPlayer.PlayPrevious();
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
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
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
					m_history.Set(SelectedItem.Label, currentFolder);
				}
			}
			currentFolder = strNewDirectory;
			facadeView.Clear();
            
			string strObjects = String.Empty;

			ArrayList itemlist = new ArrayList();

      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
			/* copy playlist from general playlist*/
			int iCurrentSong = -1;
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
        iCurrentSong = playlistPlayer.CurrentSong;

			string strFileName;
			for (int i = 0; i < playlist.Count; ++i)
			{
				PlayListItem item = playlist[i];
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
			if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(currentFolder) >= 0 && g_Player.Playing
        && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO_TEMP)
        || (GetID == (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO 
				&& g_Player.Playing))
			{
        iCurrentSong = playlistPlayer.CurrentSong;
				if (iCurrentSong >= 0)
				{
          playlist = playlistPlayer.GetPlaylist(playlistPlayer.CurrentPlaylistType);
					if (iCurrentSong < playlist.Count)
					{
						PlayListItem item = playlist[iCurrentSong];
						strFileName = item.FileName;
					}
				}
			}

      SetIMDBThumbs(itemlist);
      
			string strSelectedItem = m_history.Get(currentFolder);
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
      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSelectedItem);
      }

		}


		#endregion

		void ClearFileItems()
		{
			facadeView.Clear();
		}
  
		void OnClearPlayList()
		{
      currentSelectedItem = -1;
			ClearFileItems();
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Clear();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
        playlistPlayer.Reset();
			LoadDirectory(String.Empty);
			UpdateButtonStates();
			GUIControl.FocusControl(GetID, btnViewAs.GetID);
		}
    void SetIMDBThumbs(ArrayList items)
    {
      GUIListItem listItem ;
      ArrayList movies = new ArrayList();
      for (int x = 0; x < items.Count; ++x)
      {
        listItem = (GUIListItem)items[x];
        if (listItem.IsFolder)
        {
          if (System.IO.File.Exists(listItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            movies.Clear();
            string pathName= listItem.Path + @"\VIDEO_TS";
            VideoDatabase.GetMoviesByPath(pathName, ref movies);
            for (int i = 0; i < movies.Count; ++i)
            {
              IMDBMovie movieDetails = (IMDBMovie)movies[i];
              string fileName = "VIDEO_TS.IFO";
              if (movieDetails.File[0] == '\\' || movieDetails.File[0] == '/')
                movieDetails.File = movieDetails.File.Substring(1);

              if (fileName.Length > 0)
              {
                if (movieDetails.File == fileName /*|| pItem->GetLabel() == info.Title*/)
                { 
                  if (Utils.IsDVD(listItem.Path))
                    listItem.Label=String.Format( "({0}:) {1}",  listItem.Path.Substring(0,1),  movieDetails.Title );
                  string coverArtImage = Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title );
                  if (System.IO.File.Exists(coverArtImage))
                  {
                    listItem.ThumbnailImage = coverArtImage;
                    listItem.IconImageBig = coverArtImage;
                    listItem.IconImage = coverArtImage;
                  }
                  break;
                }
              }
            }
          }
        }
      }

      movies.Clear();
      VideoDatabase.GetMoviesByPath(currentFolder, ref movies);
      for (int x = 0; x < items.Count; ++x)
      {
        listItem = (GUIListItem)items[x];
        if (!listItem.IsFolder)
        {
          IMDBMovie movieDetails = new IMDBMovie();
          int idMovie=VideoDatabase.GetMovieInfo(listItem.Path,ref movieDetails);
          if (idMovie>=0)
          {
            string coverArtImage= Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title );
            if (System.IO.File.Exists(coverArtImage))
            {
              listItem.ThumbnailImage = coverArtImage;
              listItem.IconImageBig = coverArtImage;
              listItem.IconImage = coverArtImage;
            }
          }
        }
      }
    }


		protected override void OnClick(int itemIndex)
		{
      currentSelectedItem = facadeView.SelectedListItemIndex;
			GUIListItem item = facadeView.SelectedListItem;
			if (item == null) return;
			if (item.IsFolder) return;

      playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
      playlistPlayer.Reset();
      playlistPlayer.Play(itemIndex);
		}
    
		protected override void OnQueueItem(int itemIndex)
		{
			RemovePlayListItem(itemIndex);
		}

		void RemovePlayListItem(int itemIndex)
		{
			GUIListItem listItem = facadeView[itemIndex];
      if (listItem==null) return;
			string songFileName = listItem.Path;

      playlistPlayer.Remove(PlayListType.PLAYLIST_VIDEO, songFileName);

			LoadDirectory(currentFolder);
			UpdateButtonStates();
			GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);
      SelectCurrentVideo();
		}

		void OnShufflePlayList()
		{
      currentSelectedItem = facadeView.SelectedListItemIndex;
			ClearFileItems();
      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
	
      if (playlist.Count <= 0) return;
      string currentSongFileName = String.Empty;
      if (playlistPlayer.CurrentSong >= 0)
      {
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
        {
          PlayListItem item = playlist[playlistPlayer.CurrentSong];
          currentSongFileName = item.FileName;
        }
      }
			playlist.Shuffle();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
        playlistPlayer.Reset();

			if (currentSongFileName.Length > 0) 
			{
				for (int i = 0; i < playlist.Count; i++)
				{
					PlayListItem playListItem = playlist[i];
					if (playListItem.FileName == currentSongFileName)
            playlistPlayer.CurrentSong = i;
				}
			}

			LoadDirectory(currentFolder);
		}
		
		void OnSavePlayList()
		{
      currentSelectedItem = facadeView.SelectedListItemIndex;
			string playlistFileName = String.Empty;
			if (GetKeyboard(ref playlistFileName))
			{
        string playListPath = String.Empty;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          playListPath = xmlreader.GetValueAsString("movies","playlists",String.Empty);
          playListPath = Utils.RemoveTrailingSlash(playListPath);
        }

				string fullPlayListPath = System.IO.Path.GetFileNameWithoutExtension(playlistFileName);
		
				fullPlayListPath += ".m3u";
        if (playListPath.Length != 0)
        {
          fullPlayListPath = playListPath + @"\" + fullPlayListPath;
        }
				PlayList playlist = new PlayList();
				for (int i = 0; i < facadeView.Count; ++i)
				{
					GUIListItem listItem = facadeView[i];
					PlayListItem playListItem = new PlayListItem();
					playListItem.FileName = listItem.Path;
					playListItem.Description = listItem.Label;
          playListItem.Duration = listItem.Duration;
          playListItem.Type = Playlists.PlayListItem.PlayListItemType.Video;
					playlist.Add(playListItem);
				}
        PlayListM3uIO saver = new PlayListM3uIO();
        saver.Save(playlist, fullPlayListPath);
			}
		}
    void SelectCurrentVideo()
    {
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_VIDEO)
      {
        int currentSongIndex = playlistPlayer.CurrentSong;
        if (currentSongIndex >= 0 && currentSongIndex <= facadeView.Count)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSongIndex);
        }
      }
    }
	}
}
