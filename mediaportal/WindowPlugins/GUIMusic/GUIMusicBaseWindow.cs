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
	/// Summary description for GUIMusicBaseWindow.
	/// </summary>
	public class GUIMusicBaseWindow: GUIWindow
	{
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
			Albums= 3
		}

		protected bool useAlbumView   = true;
		protected bool useLargeIconView   = true;
		protected bool useSmallIconView   = true;
		protected bool useListView   = true;
		protected View currentView		 = View.List;
		protected MusicDatabase		      m_database = new MusicDatabase();
		[SkinControlAttribute(50)]		protected GUIFacadeControl facadeView;
		[SkinControlAttribute(7)]			protected GUISelectButtonControl btnType;
		[SkinControlAttribute(2)]			protected GUIButtonControl btnViewAs;

		public GUIMusicBaseWindow()
		{
		}

		protected virtual string SerializeName
		{
			get
			{
				return String.Empty;
			}
		}
		#region Serialisation
		void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				currentView=(View)xmlreader.GetValueAsInt(SerializeName,"view", (int)View.List);
			}
			switch (currentView)
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
			}

		}

		void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue(SerializeName,"view",(int)currentView);
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
				
				if (currentView != View.List) return true;
				return false;
			}
		}

		protected bool ViewByLargeIcon
		{
			get
			{
				if (currentView == View.LargeIcons) return true;
				return false;
			}
		}

		
		protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
		{
			if (control == btnViewAs)
			{
				bool shouldContinue=false;
				do 
				{
					shouldContinue=false;
					switch (currentView)
					{
						case View.List : 
							currentView = View.Icons;
							if (!useSmallIconView || facadeView.ThumbnailView==null)
								shouldContinue=true;
							else
								facadeView.View=GUIFacadeControl.ViewMode.SmallIcons;
							break;
						case View.Icons : 
							currentView = View.LargeIcons;
							if (!useLargeIconView || facadeView.ThumbnailView==null)
								shouldContinue=true;
							else 
								facadeView.View=GUIFacadeControl.ViewMode.LargeIcons;
							break;
						case View.LargeIcons: 
							currentView = View.Albums;
							if (!useAlbumView || facadeView.AlbumListView==null)
								shouldContinue=true;
							else 
								facadeView.View=GUIFacadeControl.ViewMode.AlbumView;
							break;
						case View.Albums: 
							currentView = View.List;
							if (!useListView || facadeView.ListView==null)
								shouldContinue=true;
							else 
								facadeView.View=GUIFacadeControl.ViewMode.List;
							break;
					}
				} while (shouldContinue);
				SelectCurrentItem();
				GUIControl.FocusControl(GetID, controlId);
				return;
			}
			
			if (control==btnType)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
				OnMessage(msg);
				int nSelected = (int)msg.Param1;
				int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
				switch (nSelected)
				{
					case 0 : //	files
						nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
						break;
					case 1 : //	albums
						nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_ALBUM;
						break;
					case 2 : //	artist
						nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_ARTIST;
						break;
					case 3 : //	genres
						nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
						break;
					case 4 : //	top100
						nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
						break;
					case 5 : //	favorites
						nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FAVORITES;
						break;
					case 6 : //	years
						nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_YEARS;
						break;
				}

				if (nNewWindow != GetID)
				{
					MusicState.StartWindow = nNewWindow;
					GUIWindowManager.ReplaceWindow(nNewWindow);
				}
				return ;
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
			GUIControl.SelectItemControl(GetID, btnType.GetID, MusicState.StartWindow - (int)GUIWindow.Window.WINDOW_MUSIC_FILES);

			GUIControl.HideControl(GetID, facadeView.GetID);
      
			int iControl = facadeView.GetID;
			GUIControl.ShowControl(GetID, iControl);
			GUIControl.FocusControl(GetID, iControl);
      

			string strLine = "";
			View view = currentView;
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
			}
			GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);
		}

		protected virtual void OnInfo(int item)
		{
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
	}
}
