using System;
using System.Collections;
using System.Net;
using System.Globalization;
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
  public class GUIMusicGenres: GUIMusicBaseWindow, IComparer
  { 
    #region Base variabeles

    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory="";
    int               m_iItemSelected=-1;   
		VirtualDirectory  m_directory = new VirtualDirectory();
		[SkinControlAttribute(9)]			protected GUIButtonControl btnSearch=null;
    #endregion

    public GUIMusicGenres()
    {
      GetID=(int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.AudioExtensions);
    }

		#region overrides
    public override bool Init()
    {
      m_strDirectory="";
      return Load (GUIGraphicsContext.Skin+@"\mymusicgenres.xml");
    }
		protected override string SerializeName
		{
			get
			{
				return "mymusicgenres";
			}
		}

		protected override View CurrentView
		{
			get
			{
				if (m_strDirectory=="") return currentViewRoot;
				return currentView;
			}
			set
			{
				if (m_strDirectory=="") 
					currentViewRoot=value;
				else
					currentView=value;
			}
		}

		protected override bool CurrentSortAsc
		{
			get
			{
				if (m_strDirectory==String.Empty) 
					return m_bSortAscendingRoot;
				return m_bSortAscending;
			}
			set
			{
				if (m_strDirectory==String.Empty) 
					m_bSortAscendingRoot=value;
				else
					m_bSortAscending=value;
			}
		}
		protected override SortMethod CurrentSortMethod
		{
			get
			{
				if (m_strDirectory==String.Empty) 
					return currentSortMethodRoot;
				return currentSortMethod;
			}
			set
			{
				if (m_strDirectory==String.Empty) 
					currentSortMethodRoot=value;
				else
					currentSortMethod=value;
			}
		}
		protected override bool AllowView(View view)
		{
			if (view==View.Albums) return false;
			return true;
		}

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
      base.OnAction(action);
    }

		protected override void OnPageLoad()
		{
			LoadDirectory(m_strDirectory);
			base.OnPageLoad ();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			m_iItemSelected=GetSelectedItemNo();
			if (GUIMusicFiles.IsMusicWindow(newWindowId))
			{
				MusicState.StartWindow=newWindowId;
			}
			base.OnPageDestroy (newWindowId);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
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

			base.OnClicked (controlId, control, actionType);
		}

		protected override void OnRetrieveCoverArt(GUIListItem item)
		{
			if (m_strDirectory.Length==0)
			{
				string strThumb=Utils.GetCoverArt(GUIMusicFiles.GenreThumbsFolder,item.Label);
				item.IconImage=strThumb;
				item.IconImageBig=strThumb;
				item.ThumbnailImage=strThumb;
				Utils.SetDefaultIcons(item);
			}
			else
			{
				base.OnRetrieveCoverArt(item);
			}
		}


		
		protected override void OnClick(int iItem)
		{
			GUIListItem item = GetSelectedItem();
			if (item==null) return;
			if (item.IsFolder)
			{
				m_iItemSelected=-1;
				LoadDirectory(item.Path);
			}
			else
			{
				// play item
				//play and add current directory to temporary playlist
				int nFolderCount=0;
				PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP ).Clear();
				PlayListPlayer.Reset();
				for ( int i = 0; i < (int) GetItemCount(); i++ ) 
				{
					GUIListItem pItem=GetItem(i);
					if ( pItem.IsFolder ) 
					{
						nFolderCount++;
						continue;
					}
					PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
					playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
					playlistItem.FileName=pItem.Path;
					playlistItem.Description=pItem.Label;
					int iDuration=0;
					MusicTag tag=(MusicTag)pItem.MusicTag;
					if (tag!=null) iDuration=tag.Duration;
					playlistItem.Duration=iDuration;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
				}

				//	Save current window and directory to know where the selected item was
				MusicState.TempPlaylistWindow=GetID;
				MusicState.TempPlaylistDirectory=m_strDirectory;

				PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
				PlayListPlayer.Play(iItem-nFolderCount);
			}
		}
    
		protected override  void OnShowContextMenu()
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
					OnSetRating(GetSelectedItemNo());
					break;
			}
		}

		protected override  void OnQueueItem(int iItem)
		{
			// add item 2 playlist
			GUIListItem pItem=GetItem(iItem);
			if (m_strDirectory=="")
			{
				string strArtist=pItem.Label;
				OnQueueGenre(strArtist);
			}
			else
			{
				AddItemToPlayList(pItem);
			}
			//move to next item
			GUIControl.SelectItemControl(GetID, facadeView.GetID,iItem+1);

		}
		#endregion

	  void keyboard_TextChanged(int kindOfSearch,string data)
	  {
		  DisplayGeneresList(kindOfSearch,data);
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
			
      GUIControl.ClearControl(GetID,facadeView.GetID);
            
      string strObjects="";

			ArrayList itemlist=new ArrayList();
			if (m_strDirectory.Length==0)
			{
				ArrayList genres=new ArrayList();
				m_database.GetGenres(ref genres);
				foreach(string strGenre in genres)
				{
					GUIListItem item=new GUIListItem();
					item.Label=strGenre;
					item.Path=strGenre;
          item.IsFolder=true;
          item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
					itemlist.Add(item);
				}
			}
			else
			{

				GUIListItem pItem = new GUIListItem ("..");
				pItem.Path="";
        pItem.IsFolder=true;
        Utils.SetDefaultIcons(pItem);
				itemlist.Add(pItem);

				ArrayList songs=new ArrayList();
				m_database.GetSongsByGenre(m_strDirectory,ref songs);
				foreach (Song song in songs)
				{
					GUIListItem item=new GUIListItem();
					item.Label=song.Title;
					item.IsFolder=false;
					item.Path=song.FileName;
					item.Duration=song.Duration;
					
					MusicTag tag=new MusicTag();
					tag.Title=song.Title;
					tag.Album=song.Album;
					tag.Artist=song.Artist;
					tag.Duration=song.Duration;
					tag.Genre=song.Genre;
					tag.Track=song.Track;
					tag.Year=song.Year;
					tag.Rating=song.Rating;
					item.MusicTag=tag;
          item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
		
					itemlist.Add(item);
				}
			}
     

      
      string strSelectedItem=m_history.Get(m_strDirectory);	
      int iItem=0;
      foreach (GUIListItem item in itemlist)
      {
				facadeView.Add(item);
      }

      int iTotalItems=itemlist.Count;
      if (itemlist.Count>0)
      {
        GUIListItem rootItem=(GUIListItem)itemlist[0];
        if (rootItem.Label=="..") iTotalItems--;
      }
      strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			SetLabels();
      OnSort();
      for (int i=0; i< GetItemCount();++i)
      {
        GUIListItem item =GetItem(i);
        if (item.Label==strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID,facadeView.GetID,iItem);
          break;
        }
        iItem++;
      }
      if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,m_iItemSelected);
      }
			SwitchView();
		}
	  
	  void DisplayGeneresList(int searchKind,string searchText)
	  {
		  GUIControl.ClearControl(GetID,facadeView.GetID);
            
		  string strObjects="";

		
		  ArrayList itemlist=new ArrayList();
		  ArrayList genres=new ArrayList();
		  m_database.GetGenres(searchKind,searchText,ref genres);
			  foreach(string strGenre in genres)
			  {
				  GUIListItem item=new GUIListItem();
				  item.Label=strGenre;
				  item.Path=strGenre;
				  item.IsFolder=true;
				  string strThumb=Utils.GetCoverArt(GUIMusicFiles.GenreThumbsFolder,item.Label);
				  item.IconImage=strThumb;
				  item.IconImageBig=strThumb;
				  item.ThumbnailImage=strThumb;

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
				facadeView.Add(item);
		  }

		  int iTotalItems=itemlist.Count;
		  if (itemlist.Count>0)
		  {
			  GUIListItem rootItem=(GUIListItem)itemlist[0];
			  if (rootItem.Label=="..") iTotalItems--;
		  }
		  strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
		  GUIPropertyManager.SetProperty("#itemcount",strObjects);
		  SetLabels();
		  OnSort();

	  }

		
    void OnQueueGenre(string strGenre)
    {
      if (strGenre==null) return;
      if (strGenre=="") return;
      ArrayList albums=new ArrayList();
      m_database.GetSongsByGenre(strGenre, ref albums);
      foreach (Song song in albums)
      {
        PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
        playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
        playlistItem.FileName=song.FileName;
        playlistItem.Description=song.Title;
        playlistItem.Duration=song.Duration;
        PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
      }
      
      if (!g_Player.Playing)
      {
        PlayListPlayer.CurrentPlaylist =PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Play(0);
      }
    }

    void AddItemToPlayList(GUIListItem pItem) 
    {
      if (pItem.IsFolder)
      {
        // recursive
        if (pItem.Label == "..") return;
        string strDirectory=m_strDirectory;
        m_strDirectory=pItem.Path;
		    
        ArrayList itemlist=m_directory.GetDirectory(m_strDirectory);
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
          PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName=pItem.Path;
          playlistItem.Description=pItem.Label;
          playlistItem.Duration=pItem.Duration;
          PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
        }
      }
    }


    
  }
}
