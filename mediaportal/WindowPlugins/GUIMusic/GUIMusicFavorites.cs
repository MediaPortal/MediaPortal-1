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
  public class GUIMusicFavorites : GUIMusicBaseWindow
  {
    enum Mode 
    {
      Duration,
      TimesPlayed,
			Rating
    }
    #region Base variabeles

    DirectoryHistory m_history = new DirectoryHistory();
    string m_strDirectory = "";
    int m_iItemSelected = -1;
    VirtualDirectory m_directory = new VirtualDirectory();
    #endregion
    Mode              _CurrentMode =Mode.Duration;
		
		[SkinControlAttribute(11)]			protected GUIButtonControl btnSearch=null;
		[SkinControlAttribute(6)]			protected GUIButtonControl btnChangeInfo=null;

    public GUIMusicFavorites()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_FAVORITES;
      
      m_directory.AddDrives();
      m_directory.SetExtensions(Utils.AudioExtensions);
    }

		#region overrides
    public override bool Init()
    {
      m_strDirectory = "";
      return Load(GUIGraphicsContext.Skin + @"\mymusicfavorites.xml");
    }
		protected override string SerializeName
		{
			get
			{
				return "mymusicfavorites";
			}
		}
		protected override View CurrentView
		{
			get
			{
				if (m_strDirectory==String.Empty) return currentViewRoot;
				else return currentView;
			}
			set
			{
				if (m_strDirectory==String.Empty)
					currentViewRoot=value;
				else
					currentView = value;
			}
		}
		protected override bool AllowView(View view)
		{
			if (view==View.Albums) return false;
			if (view==View.FilmStrip) return false;
			return true;
		}


		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
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
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnSearch)
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
			if (control==btnChangeInfo)
			{
			 switch (_CurrentMode)
			 {
				 case Mode.Duration: 
					 _CurrentMode=Mode.TimesPlayed;
					 break;
				 case Mode.TimesPlayed:
					 _CurrentMode=Mode.Rating;
					 break;
				 case Mode.Rating:
					 _CurrentMode=Mode.Duration;
					 break;
			 }
				LoadDirectory(m_strDirectory);
				UpdateButtonStates();
				GUIControl.FocusControl(GetID, btnChangeInfo.GetID);
			}
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
				dlg.AddLocalizedString(933); //Remove from favorites
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
					m_database.RemoveSongFromFavorites(item.Path);
					LoadDirectory(m_strDirectory);
					break;
				case 4:// Rating
					OnSetRating( GetSelectedItemNo());
					break;
      }
    }

		protected override void SetLabels()
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
					case Mode.Rating:
						if (tag!=null)
						{
							item.Label2 = String.Format("{0}",tag.Rating);
						}
						break;
        }

      }
    }

    
		protected override void UpdateButtonStates()
		{
			base.UpdateButtonStates ();
    
      switch(_CurrentMode)
      {  
        case Mode.Duration:
          GUIControl.SetControlLabel(GetID, btnChangeInfo.GetID, GUILocalizeStrings.Get(720));
          break;
        case Mode.TimesPlayed:
          GUIControl.SetControlLabel(GetID, btnChangeInfo.GetID, GUILocalizeStrings.Get(721));
					break;
				case Mode.Rating:
					GUIControl.SetControlLabel(GetID, btnChangeInfo.GetID, GUILocalizeStrings.Get(935));
					break;
      }
    }

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

		#endregion
		
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
			GUIControl.ClearControl(GetID, facadeView.GetID);
            
			string strObjects = "";

			ArrayList itemlist = new ArrayList();
			ArrayList songs = new ArrayList();
			m_database.GetSongsByFavorites(out songs);
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
				tag.Rating=song.Rating;
				item.MusicTag = tag;
				item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
		
				itemlist.Add(item);
			}
      

			string strSelectedItem = m_history.Get(m_strDirectory);
			int iItem = 0;
			foreach (GUIListItem item in itemlist)
			{
				facadeView.Add(item);
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

			if (m_iItemSelected >= 0)
			{
				GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
			}
		}
	  
		void DisplayFavoriteList(int searchKind,string searchText)
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
				facadeView.Add(item);
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
		}

		
		void keyboard_TextChanged(int kindOfSearch,string data)
		{
			DisplayFavoriteList(kindOfSearch,data);
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
