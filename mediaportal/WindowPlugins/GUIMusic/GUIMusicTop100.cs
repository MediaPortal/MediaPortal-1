using System;
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
  public class GUIMusicTop100 : GUIMusicBaseWindow
  {
    enum Mode 
    {
      Duration,
      TimesPlayed
    }

		#region Base variabeles

    DirectoryHistory m_history = new DirectoryHistory();
    int m_iItemSelected = -1;
    VirtualDirectory m_directory = new VirtualDirectory();
		Mode              _CurrentMode =Mode.Duration;
		protected string m_strDirectory = "";

		[SkinControlAttribute(6)]			protected GUIButtonControl btnChangeInfo;
		[SkinControlAttribute(9)]			protected GUIButtonControl btnResetTop100;
		[SkinControlAttribute(11)]		protected GUIButtonControl btnSearch;
		
		#endregion

    public GUIMusicTop100()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
      
      m_directory.AddDrives();
      m_directory.SetExtensions(Utils.AudioExtensions);
    } 

    public override bool Init()
    {
      m_strDirectory = "";
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      return Load(GUIGraphicsContext.Skin + @"\mymusictop100.xml");
    }
		protected override string SerializeName
		{
			get
			{
				return "musictop100";
			}
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
          
			SelectCurrentItem();
			UpdateButtonStates();
			LoadDirectory(m_strDirectory);
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			m_iItemSelected = GetSelectedItemNo();
			if (GUIMusicFiles.IsMusicWindow(newWindowId))
			{
				MusicState.StartWindow=newWindowId;
			}
			base.OnPageDestroy (newWindowId);
		}


    void OnThreadMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC : 
          string strFile = message.Label;
          if (Utils.IsAudio(strFile))
          {
            if (GUIWindowManager.ActiveWindow != GetID)
            {
              m_database.IncrTop100CounterByFileName(strFile);
            }
          }
          break;
      }
    }

    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }

      base.OnAction(action);
    }

		protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
		{
			// search-button handling
			if (control == btnSearch)
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

			if (control == btnResetTop100)
			{
				GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
				if (null != dlgYesNo)
				{
					dlgYesNo.SetHeading(GUILocalizeStrings.Get(718));
					dlgYesNo.SetLine(1, GUILocalizeStrings.Get(719));
					dlgYesNo.SetLine(2, "");
					dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
					if (!dlgYesNo.IsConfirmed) return ;
					m_database.ResetTop100();
					LoadDirectory(m_strDirectory);
				}
			}
			if (control == btnChangeInfo)
			{
				switch (_CurrentMode)
				{
					case Mode.Duration: 
						_CurrentMode=Mode.TimesPlayed;
						break;
					case Mode.TimesPlayed:
						_CurrentMode=Mode.Duration;
						break;
				}
				LoadDirectory(m_strDirectory);
				GUIControl.FocusControl(GetID, btnChangeInfo.GetID);
			}

			base.OnClicked(controlId, control, actionType);
		}

		protected override void OnShowContextMenu()
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
			if (!item.IsFolder && !item.IsRemote)
			{
				dlg.AddLocalizedString(930); //Add to favorites
				dlg.AddLocalizedString(931); //Rating
			}

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
				case 3: // add to favorites
					m_database.AddSongToFavorites(item.Path);
					break;
				case 4:// Rating
					OnSetRating( GetSelectedItemNo());
					break;

      }
    }

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
                item.Label = String.Format("{0:00}. {1} - {2}",i + 1, tag.Artist, tag.Title);
              else
                item.Label = String.Format("{0:00}. {1} - {2}",i + 1, tag.Artist, tag.Title);
            }
            else
            {
              if (tag.Track > 0)
                item.Label = String.Format("{0:00}. {1} ",i + 1, tag.Title);
              else
                item.Label = String.Format("{0:00}. {1}",i + 1, tag.Title);
            }
          }
        }
        
        item.Label2="";
        switch (_CurrentMode)
        {
          case Mode.Duration:
            if (tag != null)
            {
              int nDuration = tag.Duration;
              if (nDuration > 0)
              {
                item.Label2 = Utils.SecondsToHMSString(nDuration);
              }
              
            }
          break;
          case Mode.TimesPlayed:
            if (tag!=null)
            {
              item.Label2 = String.Format("{0}x",tag.TimesPlayed);
            }
          break;
        }
      }
    }

    
    protected override void UpdateButtonStates()
    {
			base.UpdateButtonStates();

      switch(_CurrentMode)
      {  
        case Mode.Duration:
					btnChangeInfo.Label=GUILocalizeStrings.Get(720);
          break;
				case Mode.TimesPlayed:
					btnChangeInfo.Label=GUILocalizeStrings.Get(721);
          break;
      }
    }
	  void keyboard_TextChanged(int kindOfSearch,string data)
	  {
		  DisplayTop100List(kindOfSearch,data);
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
      GUIControl.ClearControl(GetID, facadeView.GetID);;
            
      string strObjects = "";

			ArrayList itemlist = new ArrayList();
			ArrayList songs = new ArrayList();
			m_database.GetTop100(ref songs);
			foreach (Song song in songs)
			{
				GUIListItem item = new GUIListItem();
				item.Label = song.Title;
				item.IsFolder = false;
				item.Path = song.FileName;
				item.Duration = song.Duration;
					
				MusicTag tag = new MusicTag();
				tag.Title = song.Title;
				tag.Album = song.Album;
				tag.Artist = song.Artist;
				tag.Duration = song.Duration;
				tag.Genre = song.Genre;
				tag.Track = song.Track;
				tag.Year = song.Year;
        tag.TimesPlayed=song.TimesPlayed;
        item.MusicTag = tag;
        item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
		
				itemlist.Add(item);
			}
      

      string strSelectedItem = m_history.Get(m_strDirectory);
      int iItem = 0;
      foreach (GUIListItem item in itemlist)
      {
        GUIControl.AddListItemControl(GetID, facadeView.GetID, item);
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
      SelectCurrentItem();

      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
      }
		}
	  
	  void DisplayTop100List(int searchKind,string searchText)
	  {
		  GUIControl.ClearControl(GetID, facadeView.GetID);
            

		  ArrayList itemlist = new ArrayList();
		  ArrayList songs = new ArrayList();
		  m_database.GetTop100(searchKind,searchText,ref songs);
		  foreach (Song song in songs)
		  {
			  GUIListItem item = new GUIListItem();
			  item.Label = song.Title;
			  item.IsFolder = false;
			  item.Path = song.FileName;
			  item.Duration = song.Duration;
					
			  MusicTag tag = new MusicTag();
			  tag.Title = song.Title;
			  tag.Album = song.Album;
			  tag.Artist = song.Artist;
			  tag.Duration = song.Duration;
			  tag.Genre = song.Genre;
			  tag.Track = song.Track;
			  tag.Year = song.Year;
			  tag.TimesPlayed=song.TimesPlayed;
			  item.MusicTag = tag;
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
        MusicTag tag = (MusicTag)item.MusicTag;
        string strThumb=GUIMusicFiles.GetCoverArt(item.IsFolder,item.Path,tag);
        if (strThumb!=String.Empty)
        {
          item.ThumbnailImage = strThumb;
          item.IconImageBig = strThumb;
          item.IconImage = strThumb;
        }
		  }

		  foreach (GUIListItem item in itemlist)
		  {
			  GUIControl.AddListItemControl(GetID, facadeView.GetID, item);
		  }
      
		  int iTotalItems = itemlist.Count;
		  if (itemlist.Count>0)
		  {
			  GUIListItem rootItem=(GUIListItem)itemlist[0];
			  if (rootItem.Label=="..") iTotalItems--;
		  }
		  string strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
		  GUIPropertyManager.SetProperty("#itemcount", strObjects);
		  SetLabels();
		  SelectCurrentItem();
	  }

	  #endregion
		
    protected override void OnClick(int iItem)
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
				// play item
				//play and add current directory to temporary playlist
				int nFolderCount = 0;
				PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
				PlayListPlayer.Reset();
				for (int i = 0; i < (int) GetItemCount(); i++) 
				{
					GUIListItem pItem = GetItem(i);
					if (pItem.IsFolder) 
					{
						nFolderCount++;
						continue;
					}
					PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
					playlistItem.FileName = pItem.Path;
					playlistItem.Description = pItem.Label;
					int iDuration = 0;
					MusicTag tag = (MusicTag)pItem.MusicTag;
					if (tag != null) iDuration = tag.Duration;
					playlistItem.Duration = iDuration;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
				}

        //	Save current window and directory to know where the selected item was
        MusicState.TempPlaylistWindow = GetID;
        MusicState.TempPlaylistDirectory = m_strDirectory;

				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
				PlayListPlayer.Play(iItem - nFolderCount);
      }
    }
    
    protected override void OnQueueItem(int iItem)
    {
      // add item 2 playlist
      GUIListItem pItem = GetItem(iItem);
      AddItemToPlayList(pItem);
	
      //move to next item
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem + 1);

    }

    void AddItemToPlayList(GUIListItem pItem) 
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
        if (Utils.IsAudio(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
        {
          PlayList.PlayListItem playlistItem = new PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName = pItem.Path;
          playlistItem.Description = pItem.Label;
          playlistItem.Duration = pItem.Duration;
          PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Add(playlistItem);
        }
      }
    }

  }
}
