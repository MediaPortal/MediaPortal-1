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
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Music
{
	/// <summary>
	/// Summary description for GUIMusicBaseWindow.
	/// </summary>
	public class GUIMusicBaseWindow: GUIWindow, IComparer
	{
		protected enum SortMethod
		{
			Name=0,
			Date=1,
			Size=2,
			Track=3,
			Duration=4,
			Title=5,
			Artist=6,
			Album=7,
			Filename=8,
			Rating=9
		}

		protected enum Level
		{
			Root,
			Sub
		}
		protected enum View
		{
			List = 0, 
			Icons = 1, 
			LargeIcons = 2, 
			Albums= 3,
			FilmStrip=4
		}
		static public string AlbumThumbsFolder=@"thumbs\music\albums";
		static public string ArtistsThumbsFolder=@"thumbs\music\artists";
		static public string GenreThumbsFolder=@"thumbs\music\genre";

		protected   View currentView		    = View.List;
		protected   View currentViewRoot    = View.List;
		protected   SortMethod currentSortMethod = SortMethod.Name;
		protected   SortMethod currentSortMethodRoot = SortMethod.Name;
		protected   bool       m_bSortAscending;
		protected   bool       m_bSortAscendingRoot;
		private     bool       m_bUseID3=false;
		protected   MusicViewHandler handler;
		protected MusicDatabase		      m_database = new MusicDatabase();
		[SkinControlAttribute(50)]		protected GUIFacadeControl facadeView=null;
		[SkinControlAttribute(2)]			protected GUIButtonControl btnViewAs=null;
		[SkinControlAttribute(3)]			protected GUIButtonControl btnSortBy=null;
		[SkinControlAttribute(4)]			protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(6)]			protected GUIButtonControl btnViews=null;

		public GUIMusicBaseWindow()
		{
				handler= new MusicViewHandler();
		}
		protected bool UseID3
		{
			get { return m_bUseID3;}
			set { m_bUseID3=value;}
		}

		protected virtual bool AllowView(View view)
		{
			return true;
		}
		protected virtual bool AllowSortMethod(SortMethod method)
		{
			return true;
		}
		protected virtual View CurrentView
		{
			get { return currentView;}
			set { currentView=value;}
		}

		protected virtual SortMethod CurrentSortMethod
		{
			get { return currentSortMethod;}
			set { currentSortMethod=value;}
		}
		protected virtual bool CurrentSortAsc
		{
			get { return m_bSortAscending;}
			set { m_bSortAscending=value;}
		}

		protected virtual string SerializeName
		{
			get
			{
				return String.Empty;
			}
		}
		#region Serialisation
		protected virtual void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				currentView=(View)xmlreader.GetValueAsInt(SerializeName,"view", (int)View.List);
				currentViewRoot=(View)xmlreader.GetValueAsInt(SerializeName,"viewroot", (int)View.List);

				currentSortMethod=(SortMethod)xmlreader.GetValueAsInt(SerializeName,"sortmethod", (int)SortMethod.Name);
				currentSortMethodRoot=(SortMethod)xmlreader.GetValueAsInt(SerializeName,"sortmethodroot", (int)SortMethod.Name);
				m_bSortAscending=xmlreader.GetValueAsBool(SerializeName,"sortasc", true);
				m_bSortAscendingRoot=xmlreader.GetValueAsBool(SerializeName,"sortascroot", true);
				m_bUseID3 = xmlreader.GetValueAsBool("musicfiles","showid3",true);
			}

			SwitchView();
		}

		protected virtual void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue(SerializeName,"view",(int)currentView);
				xmlwriter.SetValue(SerializeName,"viewroot",(int)currentViewRoot);
				xmlwriter.SetValue(SerializeName,"sortmethod",(int)currentSortMethod);
				xmlwriter.SetValue(SerializeName,"sortmethodroot",(int)currentSortMethodRoot);
				xmlwriter.SetValueAsBool(SerializeName,"sortasc",m_bSortAscending);
				xmlwriter.SetValueAsBool(SerializeName,"sortascroot",m_bSortAscendingRoot);
			}
		}
		#endregion

		protected GUIListItem GetSelectedItem()
		{
				return facadeView.SelectedListItem;
		}

		protected GUIListItem GetItem(int iItem)
		{
			return facadeView[iItem];
		}

		protected int GetSelectedItemNo()
		{
			return facadeView.SelectedListItemIndex;
		}

		protected int GetItemCount()
		{
			return facadeView.Count;
		}

		protected bool ViewByIcon
		{
			get 
			{
				if (CurrentView != View.List) return true;
				return false;
			}
		}

		protected bool ViewByLargeIcon
		{
			get
			{
				if (CurrentView == View.LargeIcons) return true;
				return false;
			}
		}
		public override void OnAction(Action action)
		{
			if (action.wID==Action.ActionType.ACTION_SHOW_PLAYLIST)
			{
				GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
				return;
			}
			base.OnAction(action);
		}
		
		protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
		{
			if (control == btnViewAs)
			{
				bool shouldContinue=false;
				do 
				{
					shouldContinue=false;
					switch (CurrentView)
					{
						case View.List : 
							CurrentView = View.Icons;
							if (!AllowView(CurrentView) || facadeView.ThumbnailView==null)
								shouldContinue=true;
							else
								facadeView.View=GUIFacadeControl.ViewMode.SmallIcons;
							break;
						case View.Icons : 
							CurrentView = View.LargeIcons;
							if (!AllowView(CurrentView) || facadeView.ThumbnailView==null)
								shouldContinue=true;
							else 
								facadeView.View=GUIFacadeControl.ViewMode.LargeIcons;
							break;
						case View.LargeIcons: 
							CurrentView = View.Albums;
							if (!AllowView(CurrentView) || facadeView.AlbumListView==null)
								shouldContinue=true;
							else 
								facadeView.View=GUIFacadeControl.ViewMode.AlbumView;
							break;
						case View.Albums: 
							CurrentView = View.FilmStrip;
							if (!AllowView(CurrentView) || facadeView.FilmstripView==null)
								shouldContinue=true;
							else 
								facadeView.View=GUIFacadeControl.ViewMode.Filmstrip;
							break;
						case View.FilmStrip: 
							CurrentView = View.List;
							if (!AllowView(CurrentView) || facadeView.ListView==null)
								shouldContinue=true;
							else 
								facadeView.View=GUIFacadeControl.ViewMode.List;
							break;
					}
				} while (shouldContinue);
				SelectCurrentItem();
				GUIControl.FocusControl(GetID, controlId);
				return;
			}//if (control == btnViewAs)

			if (control==btnSortAsc)
			{
				CurrentSortAsc=!CurrentSortAsc;
				OnSort();
				UpdateButtonStates();
				GUIControl.FocusControl(GetID,control.GetID);
			}//if (iControl==btnSortAsc)

			if (control==btnSortBy)
			{
				bool shouldContinue=false;
				do
				{
					shouldContinue=false;
					switch (CurrentSortMethod)
					{
						case SortMethod.Name:
							CurrentSortMethod=SortMethod.Date;
							break;
						case SortMethod.Date:
							CurrentSortMethod=SortMethod.Size;
							break;
						case SortMethod.Size:
							CurrentSortMethod=SortMethod.Track;
							break;
						case SortMethod.Track:
							CurrentSortMethod=SortMethod.Duration;
							break;
						case SortMethod.Duration:
							CurrentSortMethod=SortMethod.Title;
							break;
						case SortMethod.Title:
							CurrentSortMethod=SortMethod.Album;
							break;
						case SortMethod.Album:
							CurrentSortMethod=SortMethod.Filename;
							break;
						case SortMethod.Filename:
							CurrentSortMethod=SortMethod.Rating;
							break;
						case SortMethod.Rating:
							CurrentSortMethod=SortMethod.Name;
							break;
					}
					if (!AllowSortMethod(CurrentSortMethod)) 
						shouldContinue=true;
				} while (shouldContinue);
				OnSort();
				GUIControl.FocusControl(GetID,control.GetID);
			}//if (control==btnSortBy)
			
			if (control==btnViews)
			{
				OnShowViews();
			}
				

			if (control == facadeView )
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
				OnMessage(msg);
				int iItem = (int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SHOW_INFO) 
				{
					OnInfo(iItem);
				}
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(iItem);
				}
				if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
				{
					OnQueueItem(iItem);
				}
			}
		}
		
		protected void SelectCurrentItem()
		{
			int iItem = GetSelectedItemNo();
      if (iItem > -1)
			{
				GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
			}
			UpdateButtonStates();
		}
 
		protected virtual void UpdateButtonStates()
		{
			GUIPropertyManager.SetProperty("#view", handler.CurrentView);
			GUIControl.HideControl(GetID, facadeView.GetID);
      
			int iControl = facadeView.GetID;
			GUIControl.ShowControl(GetID, iControl);
			GUIControl.FocusControl(GetID, iControl);
      

			string strLine = "";
			View view = CurrentView;
			switch (view)
			{
				case View.List : 
					strLine = GUILocalizeStrings.Get(101);
					break;
				case View.Icons : 
					strLine = GUILocalizeStrings.Get(100);
					break;
				case View.LargeIcons: 
					strLine = GUILocalizeStrings.Get(417);
					break;
				case View.Albums: 
					strLine = GUILocalizeStrings.Get(101);
					break;
				case View.FilmStrip: 
					strLine = GUILocalizeStrings.Get(733);
					break;
			}
			GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);

			switch (CurrentSortMethod)
			{
				case SortMethod.Name:
					strLine=GUILocalizeStrings.Get(103);
					break;
				case SortMethod.Date:
					strLine=GUILocalizeStrings.Get(104);
					break;
				case SortMethod.Size:
					strLine=GUILocalizeStrings.Get(105);
					break;
				case SortMethod.Track:
					strLine=GUILocalizeStrings.Get(266);
					break;
				case SortMethod.Duration:
					strLine=GUILocalizeStrings.Get(267);
					break;
				case SortMethod.Title:
					strLine=GUILocalizeStrings.Get(268);
					break;
				case SortMethod.Artist:
					strLine=GUILocalizeStrings.Get(269);
					break;
				case SortMethod.Album:
					strLine=GUILocalizeStrings.Get(270);
					break;
				case SortMethod.Filename:
					strLine=GUILocalizeStrings.Get(363);
					break;
				case SortMethod.Rating:
					strLine=GUILocalizeStrings.Get(367);
					break;
			}
			if (btnSortBy!=null)
				GUIControl.SetControlLabel(GetID,btnSortBy.GetID,strLine);
		
			if (btnSortAsc!=null)
			{
				if (CurrentSortAsc)
					GUIControl.DeSelectControl(GetID,btnSortAsc.GetID);
				else
					GUIControl.SelectControl(GetID,btnSortAsc.GetID);
			}
		}

		protected virtual void OnClick(int item)
		{
		}
		protected virtual void OnQueueItem(int item)
		{
		}

		
		protected void OnSetRating(int itemNumber)
		{
			GUIListItem item = GetItem(itemNumber);
			if (item ==null) return;
			MusicTag tag=item.MusicTag as MusicTag;
			GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_DIALOG_RATING);
			if (tag!=null) 
			{
				dialog.Rating=tag.Rating;
				dialog.SetTitle(String.Format("{0}-{1}", tag.Artist, tag.Title) );
			}
			dialog.FileName=item.Path;
			dialog.DoModal(GetID);
			if (tag!=null) 
			{
				tag.Rating=dialog.Rating;
			}
			m_database.SetRating(item.Path,dialog.Rating);
			if (dialog.Result == GUIDialogSetRating.ResultCode.Previous)
			{
				while (itemNumber >0)
				{
					itemNumber--;
					item = GetItem(itemNumber);
					if (!item.IsFolder && !item.IsRemote)
					{
						OnSetRating(itemNumber);
						return;
					}
				}
			}

			if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
			{
				while (itemNumber+1 < GetItemCount() )
				{
					itemNumber++;
					item = GetItem(itemNumber);
					if (!item.IsFolder && !item.IsRemote)
					{
						OnSetRating(itemNumber);
						return;
					}
				}
			}
		}
		protected override void OnPageLoad()
		{
			LoadSettings();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			SaveSettings();
		}
		
		protected void LoadPlayList(string strPlayList)
		{
			PlayList playlist=PlayListFactory.Create(strPlayList);
			if (playlist==null) return;
			if (!playlist.Load(strPlayList))
			{
				GUIDialogOK dlgOK=(GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (dlgOK!=null)
				{
					dlgOK.SetHeading(6);
					dlgOK.SetLine(1,477);
					dlgOK.SetLine(2,"");
					dlgOK.DoModal(GetID);
				}
				return;
			}

			if (playlist.Count==1)
			{
				Log.Write("GUIMusicYears:Play:{0}",playlist[0].FileName);
				g_Player.Play(playlist[0].FileName);
				return;
			}

			// clear current playlist
			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Clear();

			// add each item of the playlist to the playlistplayer
			for (int i=0; i < playlist.Count; ++i)
			{
				PlayList.PlayListItem playListItem =playlist[i];
				PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playListItem);
			}

      
			// if we got a playlist
			if (PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Count >0)
			{
				// then get 1st song
				playlist=PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC );
				PlayList.PlayListItem item=playlist[0];

				// and start playing it
				PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
				PlayListPlayer.Reset();
				PlayListPlayer.Play(0);

				// and activate the playlist window if its not activated yet
				if (GetID == GUIWindowManager.ActiveWindow)
				{
					GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
				}
			}
		}

		#region Sort Members
		protected virtual void OnSort()
		{
			SetLabels();
			facadeView.Sort(this);
			UpdateButtonStates();
		}

		public int Compare(object x, object y)
		{
			if (x==y) return 0;
			GUIListItem item1=(GUIListItem)x;
			GUIListItem item2=(GUIListItem)y;
			if (item1==null) return -1;
			if (item2==null) return -1;
			if (item1.IsFolder && item1.Label=="..") return -1;
			if (item2.IsFolder && item2.Label=="..") return -1;
			if (item1.IsFolder && !item2.IsFolder) return -1;
			else if (!item1.IsFolder && item2.IsFolder) return 1; 

			string strSize1="";
			string strSize2="";
			if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
			if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);

			SortMethod method=CurrentSortMethod;
			bool bAscending=CurrentSortAsc;

			switch (method)
			{
				case SortMethod.Name:
					if (bAscending)
					{
						return String.Compare(item1.Label ,item2.Label,true);
					}
					else
					{
						return String.Compare(item2.Label ,item1.Label,true);
					}
        

				case SortMethod.Date:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
          
					item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					if (bAscending)
					{
						return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
					}
					else
					{
						return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
					}

				case SortMethod.Rating:
					int iRating1 = 0;
					int iRating2 = 0;
					if (item1.MusicTag != null) iRating1 = ((MusicTag)item1.MusicTag).Rating;
					if (item2.MusicTag != null) iRating2 = ((MusicTag)item2.MusicTag).Rating;
					if (bAscending)
					{
						return (int)(iRating1 - iRating2);
					}
					else
					{
						return (int)(iRating2 - iRating1);
					}

				case SortMethod.Size:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
					if (bAscending)
					{
						return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
					}
					else
					{
						return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
					}

				case SortMethod.Track:
					int iTrack1=0;
					int iTrack2=0;
					if (item1.MusicTag!=null) iTrack1=((MusicTag)item1.MusicTag).Track;
					if (item2.MusicTag!=null) iTrack2=((MusicTag)item2.MusicTag).Track;
					if (bAscending)
					{
						return (int)(iTrack1 - iTrack2);
					}
					else
					{
						return (int)(iTrack2 - iTrack1);
					}
          
				case SortMethod.Duration:
					int iDuration1=0;
					int iDuration2=0;
					if (item1.MusicTag!=null) iDuration1=((MusicTag)item1.MusicTag).Duration;
					if (item2.MusicTag!=null) iDuration2=((MusicTag)item2.MusicTag).Duration;
					if (bAscending)
					{
						return (int)(iDuration1 - iDuration2);
					}
					else
					{
						return (int)(iDuration2 - iDuration1);
					}
          
				case SortMethod.Title:
					string strTitle1=item1.Label;
					string strTitle2=item2.Label;
					if (item1.MusicTag!=null) strTitle1=((MusicTag)item1.MusicTag).Title;
					if (item2.MusicTag!=null) strTitle2=((MusicTag)item2.MusicTag).Title;
					if (bAscending)
					{
						return String.Compare(strTitle1 ,strTitle2,true);
					}
					else
					{
						return String.Compare(strTitle2 ,strTitle1,true);
					}
        
				case SortMethod.Artist:
					string strArtist1="";
					string strArtist2="";
					if (item1.MusicTag!=null) strArtist1=((MusicTag)item1.MusicTag).Artist;
					if (item2.MusicTag!=null) strArtist2=((MusicTag)item2.MusicTag).Artist;
					if (bAscending)
					{
						return String.Compare(strArtist1 ,strArtist2,true);
					}
					else
					{
						return String.Compare(strArtist2 ,strArtist1,true);
					}
        
				case SortMethod.Album:
					string strAlbum1="";
					string strAlbum2="";
					if (item1.MusicTag!=null) strAlbum1=((MusicTag)item1.MusicTag).Album;
					if (item2.MusicTag!=null) strAlbum2=((MusicTag)item2.MusicTag).Album;
					if (bAscending)
					{
						return String.Compare(strAlbum1 ,strAlbum2,true);
					}
					else
					{
						return String.Compare(strAlbum2 ,strAlbum1,true);
					}
          

				case SortMethod.Filename:
					string strFile1=System.IO.Path.GetFileName(item1.Path);
					string strFile2=System.IO.Path.GetFileName(item2.Path);
					if (bAscending)
					{
						return String.Compare(strFile1 ,strFile2,true);
					}
					else
					{
						return String.Compare(strFile2 ,strFile1,true);
					}
          
			} 
			return 0;
		}
		#endregion


		protected virtual void SetLabels()
		{
			SortMethod method=CurrentSortMethod;

			for (int i=0; i < GetItemCount();++i)
			{
				GUIListItem item=GetItem(i);
				MusicTag tag=(MusicTag)item.MusicTag;
				if (tag!=null)
				{
					if (tag.Title.Length>0)
					{
						if (tag.Artist.Length>0)
						{
							if (tag.Track>0)
								item.Label=String.Format("{0:00}. {1} - {2}",tag.Track, tag.Artist, tag.Title);
							else
								item.Label=String.Format("{0} - {1}",tag.Artist, tag.Title);
						}
						else
						{
							if (tag.Track>0)
								item.Label=String.Format("{0:00}. {1} ",tag.Track, tag.Title);
							else
								item.Label=String.Format("{0}",tag.Title);
						}
						if (method==SortMethod.Album)
						{
							if (tag.Album.Length>0 && tag.Title.Length>0)
							{
								item.Label=String.Format("{0} - {1}", tag.Album,tag.Title);
							}
						}
						if (method==SortMethod.Rating)
						{
							item.Label2=String.Format("{0}", tag.Rating);
						}
					}
				}
        
        
				if (method==SortMethod.Size||method==SortMethod.Filename)
				{
					if (item.IsFolder) item.Label2="";
					else
					{
						if (item.Size>0)
						{
							item.Label2=Utils.GetSize( item.Size);
						}
						if (method==SortMethod.Filename)
						{
							item.Label=Utils.GetFilename(item.Path);
						}
					}
				}
				else if (method==SortMethod.Date)
				{
					if (item.FileInfo!=null)
					{
						item.Label2 =item.FileInfo.CreationTime.ToShortDateString() + " "+item.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					}
				}
				else if (method != SortMethod.Rating)
				{
					if (tag!=null)
					{
						int nDuration=tag.Duration;
						if (nDuration>0)
						{
							item.Label2=Utils.SecondsToHMSString(nDuration);
						}
					}
				}
			}
		}
		protected void SwitchView()
		{
			switch (CurrentView)
			{
				case View.List : 
					facadeView.View=GUIFacadeControl.ViewMode.List;
					break;
				case View.Icons : 
					facadeView.View=GUIFacadeControl.ViewMode.SmallIcons;
					break;
				case View.LargeIcons: 
					facadeView.View=GUIFacadeControl.ViewMode.LargeIcons;
					break;
				case View.Albums: 
					facadeView.View=GUIFacadeControl.ViewMode.AlbumView;
					break;
				case View.FilmStrip: 
					facadeView.View=GUIFacadeControl.ViewMode.Filmstrip;
					break;
			}
		}

		
		protected bool GetKeyboard(ref string strLine)
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

		
		protected virtual void OnRetrieveCoverArt(GUIListItem item)
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
		
		protected void OnShowViews()
		{
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu
			dlg.Add ( GUILocalizeStrings.Get(134));//songs
			foreach (ViewDefinition view in handler.Views)
			{
				dlg.Add( view.Name); //play
			}
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			if (dlg.SelectedLabel==0)
			{
				int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
				MusicState.StartWindow = nNewWindow;
				if (nNewWindow!=GetID)
				{
					GUIWindowManager.ReplaceWindow(nNewWindow);
				}
			}
			else
			{
				ViewDefinition selectedView = (ViewDefinition)handler.Views[dlg.SelectedLabel-1];
				handler.CurrentView=selectedView.Name;
				MusicState.View=selectedView.Name;
				int nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
				if (GetID!=nNewWindow)
				{
					MusicState.StartWindow = nNewWindow;
					if (nNewWindow!=GetID)
					{
						GUIWindowManager.ReplaceWindow(nNewWindow);
					}
				}
				else
				{
					LoadDirectory("");
				}
			}
		}
		protected virtual void LoadDirectory(string path)
		{
		}
		
		static public string GetArtistCoverArtName(string artist)
		{
			return Utils.GetCoverArtName(ArtistsThumbsFolder, artist);
		}

		void OnInfoFile(GUIListItem item)
		{
		}

		void OnInfoFolder(GUIListItem item)
		{
		}

		protected virtual void OnInfo(int iItem)
		{
			GUIListItem pItem = GetItem(iItem);

			Song song = pItem.AlbumInfoTag as Song;
			if (song==null)
			{
				if (pItem.IsFolder)
				{
					if (pItem.Path!=String.Empty) OnInfoFile(pItem);
				}
				else
				{
						if (pItem.Path!=String.Empty) OnInfoFolder(pItem);
				}
				return;
			}
			else if (song.songId>=0)
			{
				ShowAlbumInfo(false,song.Artist,song.Album, song.FileName, pItem.MusicTag as MusicTag);
			}
			else if (song.albumId>=0)
			{
				ShowAlbumInfo(false,song.Artist,song.Album, song.FileName, pItem.MusicTag as MusicTag);
			}
		}
		
		protected void ShowAlbumInfo(bool isFolder,string artistName,string strAlbumName, string strPath, MusicTag tag)
		{
			// check cache
			GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			AlbumInfo albuminfo = new AlbumInfo();
			if (m_database.GetAlbumInfo(strAlbumName, strPath, ref albuminfo))
			{
				ArrayList songs = new ArrayList();
				MusicAlbumInfo album = new MusicAlbumInfo();
				album.Set(albuminfo);

				GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
				if (null != pDlgAlbumInfo)
				{
					pDlgAlbumInfo.Album = album;
					pDlgAlbumInfo.Tag=tag;

					pDlgAlbumInfo.DoModal(GetID);
					if (pDlgAlbumInfo.NeedsRefresh)
					{
						m_database.DeleteAlbumInfo(strAlbumName);
						ShowAlbumInfo(isFolder,artistName,strAlbumName, strPath, tag);
					}
					return;
				}
			}

			// show dialog box indicating we're searching the album
			if (dlgProgress != null)
			{
				dlgProgress.SetHeading(185);
				dlgProgress.SetLine(1, strAlbumName );
				dlgProgress.SetLine(2, artistName);
				dlgProgress.SetLine(3, tag.Year.ToString());
				dlgProgress.StartModal(GetID);
				dlgProgress.Progress();
			}
			bool bDisplayErr = false;
	
			// find album info
			MusicInfoScraper scraper = new MusicInfoScraper();
			if (scraper.FindAlbuminfo(strAlbumName))
			{
				if (dlgProgress != null) dlgProgress.Close();
				// did we found at least 1 album?
				int iAlbumCount = scraper.Count;
				if (iAlbumCount >= 1)
				{
					//yes
					// if we found more then 1 album, let user choose one
					int iSelectedAlbum = 0;
					if (iAlbumCount > 1)
					{
						//show dialog with all albums found
						string szText = GUILocalizeStrings.Get(181);
						GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
						if (null != pDlg)
						{
							pDlg.Reset();
							pDlg.SetHeading(szText);
							for (int i = 0; i < iAlbumCount; ++i)
							{
								MusicAlbumInfo info = scraper[i];
								pDlg.Add(info.Title2);
							}
							pDlg.DoModal(GetID);

							// and wait till user selects one
							iSelectedAlbum = pDlg.SelectedLabel;
							if (iSelectedAlbum < 0) return;
						}
					}

					// ok, now show dialog we're downloading the album info
					MusicAlbumInfo album = scraper[iSelectedAlbum];
					if (null != dlgProgress) 
					{
						dlgProgress.SetHeading(185);
						dlgProgress.SetLine(1, album.Title2);
						dlgProgress.SetLine(2, album.Artist);
						dlgProgress.StartModal(GetID);
						dlgProgress.Progress();
					}

					// download the album info
					bool bLoaded = album.Loaded;
					if (!bLoaded) 
						bLoaded = album.Load();
					if (bLoaded)
					{
						// set album title from musicinfotag, not the one we got from allmusic.com
						album.Title = strAlbumName;
						// set path, needed to store album in database
						album.AlbumPath = strPath;
						albuminfo = new AlbumInfo();
						albuminfo.Album = album.Title;
						albuminfo.Artist = album.Artist;
						albuminfo.Genre = album.Genre;
						albuminfo.Tones = album.Tones;
						albuminfo.Styles = album.Styles;
						albuminfo.Review = album.Review;
						albuminfo.Image = album.ImageURL;
						albuminfo.Rating = album.Rating;
						albuminfo.Tracks = album.Tracks;
						try
						{
							albuminfo.Year = Int32.Parse(album.DateOfRelease);
						}
						catch (Exception)
						{
						}
						//albuminfo.Path   = album.AlbumPath;
						// save to database
						m_database.AddAlbumInfo(albuminfo);
						if (null != dlgProgress) 
							dlgProgress.Close();

						// ok, show album info
						GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
						if (null != pDlgAlbumInfo)
						{
							pDlgAlbumInfo.Album = album;
							pDlgAlbumInfo.Tag=tag;

							pDlgAlbumInfo.DoModal(GetID);
							if (pDlgAlbumInfo.NeedsRefresh)
							{
								m_database.DeleteAlbumInfo(album.Title);
								ShowAlbumInfo(isFolder,artistName,strAlbumName, strPath, tag);
								return;
							}
							if (isFolder)
							{
								string thumb=GetAlbumThumbName(album.Artist, album.Title);
								if (System.IO.File.Exists(thumb))
								{
									try
									{
										string folderjpg=String.Format(@"{0}\folder.jpg",Utils.RemoveTrailingSlash(strPath));
										Utils.FileDelete(folderjpg);
										System.IO.File.Copy(thumb, folderjpg);
									}
									catch(Exception)
									{
									}
								}
							}
						}
					}
					else
					{
						// failed 2 download album info
						bDisplayErr = true;
					}
				}
				else 
				{
					// no albums found
					bDisplayErr = true;
				}
			}
			else
			{
				// unable 2 connect to www.allmusic.com
				bDisplayErr = true;
			}
			// if an error occured, then notice the user
			if (bDisplayErr)
			{
				if (null != dlgProgress) 
					dlgProgress.Close();
				if (null != dlgOk)
				{
					dlgOk.SetHeading(187);
					dlgOk.SetLine(1, 187);
					dlgOk.SetLine(2, "");
					dlgOk.DoModal(GetID);
				}
			}
		}
		
		static public string GetAlbumThumbName(string ArtistName, string AlbumName)
		{
			if (ArtistName==String.Empty) return String.Empty;
			if (AlbumName==String.Empty) return String.Empty;
			string name=String.Format("{0}-{1}", ArtistName, AlbumName);
			return Utils.GetCoverArtName(GUIMusicFiles.AlbumThumbsFolder, name);
		}
		protected virtual void AddSongToFavorites(GUIListItem item)
		{
			Song song = item.AlbumInfoTag as Song;
			if (song==null) return;
			if (song.songId<0) return;
			song.Favorite=true;
			m_database.SetFavorite(song);
		}
	}
}
