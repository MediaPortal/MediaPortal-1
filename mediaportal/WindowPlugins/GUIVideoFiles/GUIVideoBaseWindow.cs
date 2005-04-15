using System;
using System.Collections;
using System.Net;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Video.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for GUIVideoBaseWindow.
	/// </summary>
	public class GUIVideoBaseWindow: GUIWindow, IComparer
	{
		protected enum SortMethod
		{
			Name=0,
			Date=1,
			Size=2,
			Year=3,
			Rating=4,
			Label=5,
		}

		protected enum View
		{
			List = 0, 
			Icons = 1, 
			LargeIcons = 2,
			FilmStrip=3
		}
		protected   const string ThumbsFolder=@"thumbs\Videos\Title";
		protected   const string ActorThumbsFolder=@"thumbs\Videos\Actors";

		protected   View currentView		    = View.List;
		protected   View currentViewRoot    = View.List;
		protected   SortMethod currentSortMethod = SortMethod.Name;
		protected   SortMethod currentSortMethodRoot = SortMethod.Name;
		protected   bool       m_bSortAscending;
		protected   bool       m_bSortAscendingRoot;
		protected   VideoViewHandler handler;
		
		[SkinControlAttribute(50)]		protected GUIFacadeControl facadeView=null;
		[SkinControlAttribute(2)]			protected GUIButtonControl btnViewAs=null;
		[SkinControlAttribute(3)]			protected GUIButtonControl btnSortBy=null;
		[SkinControlAttribute(4)]			protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(5)]			protected GUIButtonControl btnViews=null;
		[SkinControlAttribute(6)]			protected GUIButtonControl btnPlayDVD=null;

		public GUIVideoBaseWindow()
		{
			handler= new VideoViewHandler();
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
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				currentView=(View)xmlreader.GetValueAsInt(SerializeName,"view", (int)View.List);
				currentViewRoot=(View)xmlreader.GetValueAsInt(SerializeName,"viewroot", (int)View.List);

				currentSortMethod=(SortMethod)xmlreader.GetValueAsInt(SerializeName,"sortmethod", (int)SortMethod.Name);
				currentSortMethodRoot=(SortMethod)xmlreader.GetValueAsInt(SerializeName,"sortmethodroot", (int)SortMethod.Name);
				m_bSortAscending=xmlreader.GetValueAsBool(SerializeName,"sortasc", true);
				m_bSortAscendingRoot=xmlreader.GetValueAsBool(SerializeName,"sortascroot", true);
			}

			SwitchView();
		}

		protected virtual void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
				GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
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
							CurrentSortMethod=SortMethod.Year;
							break;
						case SortMethod.Year:
							CurrentSortMethod=SortMethod.Rating;
							break;
						case SortMethod.Rating:
							CurrentSortMethod=SortMethod.Label;
							break;
						case SortMethod.Label:
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
				

			if (control==btnPlayDVD)
			{
				OnPlayDVD();
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
				case View.FilmStrip: 
					strLine = GUILocalizeStrings.Get(733);
					break;
			}
			GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);


			switch (CurrentSortMethod)
			{
				case SortMethod.Name:
					strLine=GUILocalizeStrings.Get(365);
					break;
				case SortMethod.Date:
					strLine=GUILocalizeStrings.Get(104);
					break;
				case SortMethod.Size:
					strLine=GUILocalizeStrings.Get(105);
					break;
				case SortMethod.Year:
					strLine=GUILocalizeStrings.Get(366);
					break;
				case SortMethod.Rating:
					strLine=GUILocalizeStrings.Get(367);
					break;
				case SortMethod.Label:
					strLine=GUILocalizeStrings.Get(430);
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

		
		protected override void OnPageLoad()
		{
			LoadSettings();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			SaveSettings();
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
			if (x == y) return 0;
			GUIListItem item1 = (GUIListItem)x;
			GUIListItem item2 = (GUIListItem)y;
			if (item1 == null) return - 1;
			if (item2 == null) return - 1;
			if (item1.IsFolder && item1.Label == "..") return - 1;
			if (item2.IsFolder && item2.Label == "..") return - 1;
			if (item1.IsFolder && !item2.IsFolder) return - 1;
			else if (!item1.IsFolder && item2.IsFolder) return 1;


			switch (CurrentSortMethod)
			{
				case SortMethod.Year : 
				{
					if (CurrentSortAsc)
					{
						if (item1.Year > item2.Year) return 1;
						if (item1.Year < item2.Year) return - 1;
					}
					else
					{
						if (item1.Year > item2.Year) return - 1;
						if (item1.Year < item2.Year) return 1;
					}
					return 0;
				}
				case SortMethod.Rating : 
				{
					if (CurrentSortAsc)
					{
						if (item1.Rating > item2.Rating) return 1;
						if (item1.Rating < item2.Rating) return - 1;
					}
					else
					{
						if (item1.Rating > item2.Rating) return - 1;
						if (item1.Rating < item2.Rating) return 1;
					}
					return 0;
				}

				case SortMethod.Name: 
          
					if (CurrentSortAsc)
					{
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						return String.Compare(item2.Label, item1.Label, true);
					}
        
				case SortMethod.Label : 
					if (CurrentSortAsc)
					{
						return String.Compare(item1.DVDLabel, item2.DVDLabel, true);
					}
					else
					{
						return String.Compare(item2.DVDLabel, item1.DVDLabel, true);
					}

			} 
			return 0;
		}
		#endregion


		protected virtual void SetLabels()
		{
			for (int i=0; i < GetItemCount();++i)
			{
				GUIListItem item=GetItem(i);
				IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
				if (movie!=null && movie.ID>0)
				{
					if (CurrentSortMethod==SortMethod.Name)
						item.Label2 = movie.Rating.ToString();
					else if (CurrentSortMethod==SortMethod.Year)
						item.Label2 = movie.Year.ToString();
					else if (CurrentSortMethod==SortMethod.Rating)
						item.Label2 = movie.Rating.ToString();
					else if (CurrentSortMethod==SortMethod.Label)
						item.Label2 = movie.DVDLabel.ToString();
				}
				else
				{
					string strSize1 = "",strDate="";
					if (item.FileInfo != null) strSize1 = Utils.GetSize(item.FileInfo.Length);
					if (item.FileInfo != null) strDate = item.FileInfo.CreationTime.ToShortDateString() + " " + item.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					if (CurrentSortMethod==SortMethod.Name)
						item.Label2 = strSize1;
					else if (CurrentSortMethod==SortMethod.Date)
						item.Label2 = strDate;
					else
						item.Label2 = strSize1;
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

		
		protected void OnShowViews()
		{
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu
			dlg.Add ( GUILocalizeStrings.Get(342));//videos
			foreach (ViewDefinition view in handler.Views)
			{
				dlg.Add( view.Name); //play
			}
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			if (dlg.SelectedLabel==0)
			{
				int nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
				VideoState.StartWindow = nNewWindow;
				if (nNewWindow!=GetID)
				{
					GUIWindowManager.ReplaceWindow(nNewWindow);
				}
			}
			else
			{
				ViewDefinition selectedView = (ViewDefinition)handler.Views[dlg.SelectedLabel-1];
				handler.CurrentView=selectedView.Name;
				VideoState.View=selectedView.Name;
				int nNewWindow=(int)GUIWindow.Window.WINDOW_VIDEO_TITLE;
				if (GetID!=nNewWindow)
				{
					VideoState.StartWindow = nNewWindow;
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

		void OnInfoFile(GUIListItem item)
		{
		}

		void OnInfoFolder(GUIListItem item)
		{
		}

		protected virtual void OnInfo(int iItem)
		{
		}

		protected void OnPlayDVD()
		{
			Log.Write("GUIVideoFiles playDVD");
			g_Player.PlayDVD();
		}
		

		protected virtual void AddItemToPlayList(GUIListItem pItem) 
		{
			if (!pItem.IsFolder)
			{
				//TODO
				if (Utils.IsVideo(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
				{
					PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
					playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
					playlistItem.FileName=pItem.Path;
					playlistItem.Description=pItem.Label;
					playlistItem.Duration=pItem.Duration;
					PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_VIDEO ).Add(playlistItem);
				}
			}
		}
	}
}
