using System;
using System.Collections;
using System.Net;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// My music / years.
  /// </summary>
  public class GUIMusicYears: GUIMusicBaseWindow
  { 
    #region Base variabeles
    enum Mode
    {
      ShowYears,
      ShowSongs
    }
    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory="";
    int               m_iItemSelected=-1;   
    VirtualDirectory  m_directory = new VirtualDirectory();
    #endregion
    Mode              m_Mode=Mode.ShowYears;
    int								m_Year=-1;

		[SkinControlAttribute(9)]			protected GUIButtonControl btnSearch;

    public GUIMusicYears()
    {
      GetID=(int)GUIWindow.Window.WINDOW_MUSIC_YEARS;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.AudioExtensions);
    }


		#region overrides
    public override bool Init()
    {
      m_strDirectory="";
      return Load (GUIGraphicsContext.Skin+@"\mymusicyears.xml");
    }
		protected override string SerializeName
		{
			get
			{
				return "musicyears";
			}
		}
		protected override bool AllowView(MediaPortal.GUI.Music.GUIMusicBaseWindow.View view)
		{
			if (view==View.Albums) return false;
			if (view==View.FilmStrip) return false;
			return true;
		}

		protected override bool CurrentSortAsc
		{
			get
			{
				if (m_Mode==Mode.ShowYears) 
					return m_bSortAscendingRoot;
				return m_bSortAscending;
			}
			set
			{
				if (m_Mode==Mode.ShowYears) 
					m_bSortAscendingRoot=value;
				else
					m_bSortAscending=value;
			}
		}
		protected override SortMethod CurrentSortMethod
		{
			get
			{
				if (m_Mode==Mode.ShowYears) 
					return currentSortMethodRoot;
				return currentSortMethod;
			}
			set
			{
				if (m_Mode==Mode.ShowYears) 
					currentSortMethodRoot=value;
				else
					currentSortMethod=value;
			}
		}

		protected override View CurrentView
		{
			get
			{
				if (m_Mode==Mode.ShowYears) 
					return currentViewRoot;
				return currentView;
			}
			set
			{
				if (m_Mode==Mode.ShowYears) 
					currentViewRoot=value;
				else
					currentView=value;
			}
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
            LoadDirectory(m_strDirectory,Mode.ShowYears);
          }
        }
        return;
      }
      base.OnAction(action);
    }

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();          
			LoadDirectory(m_strDirectory,m_Mode);
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			if (GUIMusicFiles.IsMusicWindow(newWindowId))
			{
				MusicState.StartWindow=newWindowId;
			}
			m_iItemSelected=GetSelectedItemNo();
			base.OnPageDestroy (newWindowId);
		}

		protected override void OnShowContextMenu()
		{
			if (m_Mode==Mode.ShowYears) return;
      GUIListItem item=GetSelectedItem();
      int itemNo=GetSelectedItemNo();
      if (item==null) return;

      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg==null) return;
      dlg.Reset();
      dlg.SetHeading(924); // menu

      dlg.Add( GUILocalizeStrings.Get(928)); //IMDB
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
        case 0: // IMDB
          OnInfo(itemNo);
          break;

        case 1: // play
          OnClick(itemNo);	
          break;
					
        case 2: // add to playlist
          OnQueueItem(itemNo);	
          break;
					
        case 3: // show playlist
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
					break;
				case 4: // add to favorites
					m_database.AddSongToFavorites(item.Path);
					break;
				case 5:// Rating
					OnSetRating(GetSelectedItemNo());
					break;
      }
    }

    protected override void OnRetrieveCoverArt(GUIListItem item)
    {
      if (m_Mode==Mode.ShowYears)
      {
        Utils.SetDefaultIcons(item);
				return;
      }
      else if (m_Mode==Mode.ShowSongs)
      {
        // get thumbs...
       base.OnRetrieveCoverArt(item);
      }
      if (ViewByIcon &&!ViewByLargeIcon) 
        item.IconImage=item.IconImageBig;
    }

		protected override void OnClick(int iItem)
		{
			GUIListItem item = GetSelectedItem();
			if (item==null) return;
			m_iItemSelected=-1;
			if (m_Mode==Mode.ShowYears)
			{
				m_Year=Int32.Parse(item.Path);
				LoadDirectory(m_strDirectory,Mode.ShowSongs);
				SwitchView();
			}
			else
			{
				if ( item.Label=="..")
				{
					m_Year=-1;
					LoadDirectory(m_strDirectory,Mode.ShowYears);
					SwitchView();
					return;
				}

        
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


				//  Save current window and directory to know where the selected item was
				MusicState.TempPlaylistWindow=GetID;
				MusicState.TempPlaylistDirectory=m_strDirectory;

				PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
				PlayListPlayer.Play(iItem-nFolderCount);
			}
		}
    
		protected override void OnQueueItem(int iItem)
		{
			// add item 2 playlist
			GUIListItem pItem=GetItem(iItem);
			if (m_Mode==Mode.ShowYears)
			{
				int year=Int32.Parse(pItem.Path);
				OnQueueYear(year);
			}
			else if (m_Mode==Mode.ShowSongs)
			{
				AddItemToPlayList(pItem);
			}
			//move to next item
			GUIControl.SelectItemControl(GetID, facadeView.GetID,iItem+1);
		}

		#endregion
		void LoadDirectory(string strNewDirectory, Mode newMode)
    {
      Mode oldMode=m_Mode;
      GUIListItem SelectedItem = GetSelectedItem();
      if (SelectedItem!=null) 
      {
        if (SelectedItem.IsFolder && SelectedItem.Label!="..")
        {
          m_history.Set(SelectedItem.Label, m_strDirectory);
        }
      }
      m_Mode=newMode;
			m_strDirectory=m_Year.ToString();
			GUIControl.ClearControl(GetID, facadeView.GetID);;
            
      string strObjects="";
      string strNavigation="";

      ArrayList itemlist=new ArrayList();
      if (m_Mode==Mode.ShowYears)
      {
				m_strDirectory="";
        strNavigation=GUILocalizeStrings.Get(133);
				for (int year=1900; year <= DateTime.Now.Year; year+=10)
				{
					int endYear=year+9;
					if (endYear>DateTime.Now.Year) endYear=DateTime.Now.Year;
					GUIListItem item=new GUIListItem();
					item.Label=String.Format("{0}-{1}", year, endYear);
					item.Path=year.ToString();
					item.IsFolder=true;
					itemlist.Add(item);
				}
				m_Year=-1;

      } //if (m_Mode==Mode.ShowYears)
      else if (m_Mode==Mode.ShowSongs)
      {
        strNavigation=GUILocalizeStrings.Get(639)+" "+m_Year;
        GUIListItem pItem = new GUIListItem ("..");
        pItem.Path="";
        pItem.IsFolder=true;
        Utils.SetDefaultIcons(pItem);
        if (ViewByIcon &&!ViewByLargeIcon) 
          pItem.IconImage=pItem.IconImageBig;
        itemlist.Add(pItem);
        ArrayList Albums = new ArrayList();
        ArrayList songs=new ArrayList();
        m_database.GetSongsByYear(m_Year,m_Year+9,ref songs);
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
          Utils.SetDefaultIcons(item);

          itemlist.Add(item);
        }
      } // else if (m_Mode==Mode.ShowYears)

     
      string strSelectedItem=m_history.Get(m_strDirectory); 
      int iItem=0;
      foreach (GUIListItem item in itemlist)
      {
				facadeView.Add(item);
      }
      OnSort();
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
				GUIControl.SelectItemControl(GetID,facadeView.GetID,iItem);
		  }
	  }


    void OnQueueYear(int year)
    {
      ArrayList songs=new ArrayList();
      m_database.GetSongsByYear(year,year+9, ref songs);
      foreach (Song song in songs)
      {
        PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
        playlistItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
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
          playlistItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName=pItem.Path;
          playlistItem.Description=pItem.Label;
          playlistItem.Duration=pItem.Duration;
          PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
        }
      }
    }
		
  }
}
