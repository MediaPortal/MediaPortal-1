using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;


namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
	public class GUIVideoPlayList : GUIWindow
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

			CONTROL_LABELFILES = 12, 

			CONTROL_LIST = 50, 
			CONTROL_THUMBS = 51

		};
		#region Base variabeles
		enum SortMethod
		{
			SORT_NAME = 0, 
			SORT_DATE = 1, 
			SORT_SIZE = 2
		}

		enum View
		{
			VIEW_AS_LIST = 0, 
			VIEW_AS_ICONS = 1, 
			VIEW_AS_LARGEICONS = 2, 
		}
		View currentView = View.VIEW_AS_LIST;
		
    const string ThumbsFolder=@"thumbs\Videos\Title";
		DirectoryHistory m_history = new DirectoryHistory();
		string m_strDirectory = "";
		int m_iItemSelected = -1;
		int m_iLastControl = 0;
		int m_nTempPlayListWindow = 0;
		string m_strTempPlayListDirectory = "";
		VirtualDirectory m_directory = new VirtualDirectory();
		#endregion
    
		public GUIVideoPlayList()
		{
			GetID = (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST;
      
			m_directory.AddDrives();
			m_directory.SetExtensions(Utils.VideoExtensions);
      LoadSettings();
    }
    ~GUIVideoPlayList()
    {
    }

		public override bool Init()
		{
			m_strDirectory = System.IO.Directory.GetCurrentDirectory();
			return Load(GUIGraphicsContext.Skin + @"\myvideoplaylist.xml");
		}

    #region Serialisation
    void LoadSettings()
    {
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp = "";
        strTmp = (string)xmlreader.GetValue("videoplaylist","viewby");
        if (strTmp != null)
        {
          if (strTmp == "list") currentView = View.VIEW_AS_LIST;
          else if (strTmp == "icons") currentView = View.VIEW_AS_ICONS;
          else if (strTmp == "largeicons") currentView = View.VIEW_AS_LARGEICONS;
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
            xmlwriter.SetValue("videoplaylist","viewby","list");
            break;
          case View.VIEW_AS_ICONS : 
            xmlwriter.SetValue("videoplaylist","viewby","icons");
            break;
          case View.VIEW_AS_LARGEICONS : 
            xmlwriter.SetValue("videoplaylist","viewby","largeicons");
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
        GUIWindowManager.PreviousWindow();
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
          
					LoadSettings();
					ShowThumbPanel();
					LoadDirectory("");
					if ((m_iLastControl == (int)Controls.CONTROL_THUMBS || m_iLastControl == (int)Controls.CONTROL_LIST) && GetItemCount() <= 0)
					{
						m_iLastControl = (int)Controls.CONTROL_BTNVIEWASICONS;
						GUIControl.FocusControl(GetID, m_iLastControl);
					}
					if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
					{
						int iSong = PlayListPlayer.CurrentSong;
						if (iSong >= 0 && iSong <= GetItemCount())
						{
							GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iSong);
							GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iSong);
						}
					}

					return true;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					m_iItemSelected = GetSelectedItemNo();
          SaveSettings();
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
						PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
						PlayListPlayer.Reset();
						PlayListPlayer.Play(GetSelectedItemNo());
						UpdateButtons();
					}
					else if (iControl == (int)Controls.CONTROL_BTNNEXT)
					{
						PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
						
            GUIVideoFiles.PlayMovieFromPlayList(true);
					}
					else if (iControl == (int)Controls.CONTROL_BTNPREVIOUS)
					{
						PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
						PlayListPlayer.PlayPrevious();
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
					LoadDirectory("");

					if ((m_iLastControl == (int)Controls.CONTROL_THUMBS || m_iLastControl == (int)Controls.CONTROL_LIST) && GetItemCount() <= 0)
					{
						m_iLastControl = (int)Controls.CONTROL_BTNVIEWASICONS;
						GUIControl.FocusControl(GetID, m_iLastControl);
          }
          if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
          {
            int iSong = PlayListPlayer.CurrentSong;
            if (iSong >= 0 && iSong <= GetItemCount())
            {
              GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iSong);
              GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iSong);
            }
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
				if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
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
						pItem.Label2 = "";
				}
				itemlist.Add(pItem);
        Utils.SetDefaultIcons(pItem);
      }

			iCurrentSong = 0;
			strFileName = "";
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
      ShowThumbPanel();
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, m_iItemSelected);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, m_iItemSelected);
      }

		}
		#endregion

		void ClearFileItems()
		{
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS);
		}
  
		void ClearPlayList()
		{
      m_iItemSelected = -1;
			ClearFileItems();
			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Clear();
			if (PlayListPlayer.CurrentPlaylist == PlayListPlayer.PlayListType.PLAYLIST_VIDEO)
				PlayListPlayer.Reset();
			LoadDirectory("");
			UpdateButtons();
			GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BTNVIEWASICONS);
		}

		void OnClick(int iItem)
		{
      m_iItemSelected = GetSelectedItemNo();
			GUIListItem item = GetSelectedItem();
			if (item == null) return;
			if (item.IsFolder) return;
      
			string strPath = item.Path;
			PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
			PlayListPlayer.Reset();
			PlayListPlayer.Play(iItem);
		}
    
		void OnQueueItem(int iItem)
		{
			RemovePlayListItem(iItem);
		}

		void RemovePlayListItem(int iItem)
		{
			GUIListItem pItem = GetItem(iItem);
      if (pItem==null) return;
			string strFileName = pItem.Path;
			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Remove(strFileName);
      
			LoadDirectory(m_strDirectory);
			UpdateButtons();
			GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST, iItem);
			GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS, iItem);
		}

		void ShufflePlayList()
		{
      m_iItemSelected = GetSelectedItemNo();
			ClearFileItems();
			PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO);
	
      if (playlist.Count <= 0) return;
      string strFileName = "";
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
      m_iItemSelected = GetSelectedItemNo();
			string strNewFileName = "";
			if (GetKeyboard(ref strNewFileName))
			{
        string strPlayListPath = "";
        using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
        {
          strPlayListPath = xmlreader.GetValueAsString("movies","playlists","");
          strPlayListPath = Utils.RemoveTrailingSlash(strPlayListPath);
        }

				string strPath = System.IO.Path.GetFileNameWithoutExtension(strNewFileName);
		
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
          newItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
					playlist.Add(newItem);
				}
				playlist.Save(strPath);
			}
		}
	}
}
