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
		}
		
		protected View currentView		 = View.List;
		protected View currentViewRoot = View.List;
		protected MusicDatabase		      m_database = new MusicDatabase();
		[SkinControlAttribute(50)]		protected GUIListControl listViewSmall;
		[SkinControlAttribute(51)]		protected GUIListControl listViewBig;
		[SkinControlAttribute(2)]			protected GUIButtonControl btnViewAs;
		[SkinControlAttribute(7)]			protected GUISelectButtonControl btnType;

		public GUIMusicBaseWindow()
		{
		}
		protected GUIListItem GetSelectedItem()
		{
			if (ViewByIcon)
				return listViewBig.SelectedListItem;
			else
				return listViewSmall.SelectedListItem;
		}

		protected GUIListItem GetItem(int iItem)
		{
			if (ViewByIcon)
				return listViewBig[iItem];
			else
				return listViewSmall[iItem];
		}

		protected int GetSelectedItemNo()
		{
			if (ViewByIcon)
				return listViewBig.SelectedListItemIndex;
			else
				return listViewSmall.SelectedListItemIndex;
		}

		protected int GetItemCount()
		{
			if (ViewByIcon)
				return listViewBig.Count;
			else
				return listViewSmall.Count;
		}

		protected bool ViewByIcon
		{
			get 
			{
				if (CurrentLevel==Level.Root)
				{
					if (currentViewRoot != View.List) return true;
				}
				else
				{
					if (currentView != View.List) return true;
				}
				return false;
			}
		}

		protected bool ViewByLargeIcon
		{
			get
			{
				if (CurrentLevel==Level.Root)
				{
					if (currentViewRoot == View.LargeIcons) return true;
				}
				else
				{
					if (currentView == View.LargeIcons) return true;
				}
				return false;
			}
		}

		
		protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
		{
			if (control == btnViewAs)
			{
				if (CurrentLevel==Level.Root)
				{
					switch (currentViewRoot)
					{
						case View.List : 
							currentViewRoot = View.Icons;
							break;
						case View.Icons : 
							currentViewRoot = View.List;
							break;
					}
				}
				else
				{
					switch (currentView)
					{
						case View.List : 
							currentView = View.Icons;
							break;
						case View.Icons : 
							currentView = View.List;
							break;
					}
				}
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

			if (control == listViewBig || control == listViewSmall)
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
				GUIControl.SelectItemControl(GetID, listViewSmall.GetID, iItem);
				GUIControl.SelectItemControl(GetID, listViewSmall.GetID, iItem);
			}
			UpdateButtonStates();
		}

		protected virtual Level CurrentLevel
		{
			get 
			{
				return Level.Root;
			}
		}
		protected virtual void UpdateButtonStates()
		{
			GUIControl.SelectItemControl(GetID, btnType.GetID, MusicState.StartWindow - (int)GUIWindow.Window.WINDOW_MUSIC_FILES);

			GUIControl.HideControl(GetID, listViewBig.GetID);
			GUIControl.HideControl(GetID, listViewSmall.GetID);
      
			int iControl = listViewSmall.GetID;
			if (ViewByIcon)
				iControl = listViewBig.GetID;
			GUIControl.ShowControl(GetID, iControl);
			GUIControl.FocusControl(GetID, iControl);
      

			string strLine = "";
			View view = currentView;
			if (CurrentLevel==Level.Root) 
				view = currentViewRoot;
			switch (view)
			{
				case View.List : 
					strLine = GUILocalizeStrings.Get(101);
					break;
				case View.Icons : 
					strLine = GUILocalizeStrings.Get(100);
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
	}
}
