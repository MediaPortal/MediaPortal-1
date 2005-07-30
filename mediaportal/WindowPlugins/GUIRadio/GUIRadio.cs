/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Net;
using System.Globalization;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Radio.Database;
using MediaPortal.Playlists;

namespace MediaPortal.GUI.Radio
{
	/// <summary>
	/// controls en-/disablen 
	///   - player (wel of niet) played radio/tv/music/video's...
	///   - buddy enable/disable
	///   -
	/// -keuzes aan select button  (sort by ....)
	/// -root/sub view->generiek maken
	/// -class welke de list/thumbnail view combinatie doet
	/// 
	/// </summary>
	public class GUIRadio: GUIWindow, IComparer,ISetupForm
	{
		[SkinControlAttribute(2)]			protected GUIButtonControl btnViewAs=null;
		[SkinControlAttribute(3)]			protected GUISortButtonControl btnSortBy=null;
		[SkinControlAttribute(6)]			protected GUIButtonControl btnPrevious=null;
		[SkinControlAttribute(7)]			protected GUIButtonControl btnNext=null;
		[SkinControlAttribute(50)]		protected GUIListControl listView=null;
		[SkinControlAttribute(51)]		protected GUIThumbnailPanel thumbnailView=null;

		#region Base variabeles
		enum SortMethod
		{
			Name=0,
			Type=1,
			Genre=2,
			Bitrate=3,
			Number
		}

		enum View:int
		{
			List    =       0,
			Icons    =      1,
			BigIcons  =   2,
		}
		View              currentView=View.List;
		SortMethod        currentSortMethod=SortMethod.Name;
		bool              sortAscending=true;
		VirtualDirectory	virtualDirectory = new VirtualDirectory();
		DirectoryHistory  directoryHistory = new DirectoryHistory();
		string            currentFolder=String.Empty;
		string            startFolder=String.Empty;
		string            currentRadioFolder=String.Empty;
		int               selectedItemIndex=-1;   
		PlayList          currentPlayList=null;
		#endregion

		public GUIRadio()
		{
			GetID=(int)GUIWindow.Window.WINDOW_RADIO;
      
			LoadSettings();
		}

    
		public override bool Init()
		{
			currentFolder=String.Empty;
			bool bResult=Load (GUIGraphicsContext.Skin+@"\MyRadio.xml");
			return bResult;
		}


		#region Serialisation
		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				currentRadioFolder=xmlreader.GetValueAsString("radio","folder",String.Empty);

				string tmpLine=String.Empty;
				tmpLine=(string)xmlreader.GetValue("myradio","viewby");
				if (tmpLine!=null)
				{
					if (tmpLine=="list") currentView=View.List;
					else if (tmpLine=="icons") currentView=View.Icons;
					else if (tmpLine=="largeicons") currentView=View.BigIcons;
				}
        
				tmpLine=(string)xmlreader.GetValue("myradio","sort");
				if (tmpLine!=null)
				{
					if (tmpLine=="name") currentSortMethod=SortMethod.Name;
					else if (tmpLine=="type") currentSortMethod=SortMethod.Type;
					else if (tmpLine=="genre") currentSortMethod=SortMethod.Genre;
					else if (tmpLine=="bitrate") currentSortMethod=SortMethod.Bitrate;
					else if (tmpLine=="number") currentSortMethod=SortMethod.Number;
				}

				sortAscending=xmlreader.GetValueAsBool("myradio","sortascending",true);

			}
		}

		void SaveSettings()
		{
			using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				switch (currentView)
				{
					case View.List: 
						xmlwriter.SetValue("myradio","viewby","list");
						break;
					case View.Icons: 
						xmlwriter.SetValue("myradio","viewby","icons");
						break;
					case View.BigIcons: 
						xmlwriter.SetValue("myradio","viewby","largeicons");
						break;
				}
        
				switch (currentSortMethod)
				{
					case SortMethod.Name:
						xmlwriter.SetValue("myradio","sort","name");
						break;
					case SortMethod.Type:
						xmlwriter.SetValue("myradio","sort","type");
						break;
					case SortMethod.Genre:
						xmlwriter.SetValue("myradio","sort","genre");
						break;
					case SortMethod.Bitrate:
						xmlwriter.SetValue("myradio","sort","bitrate");
						break;
					case SortMethod.Number:
						xmlwriter.SetValue("myradio","sort","number");
						break;
				}

				xmlwriter.SetValueAsBool("myradio","sortascending",sortAscending);
			}
		}
		#endregion

		#region BaseWindow Members
		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				if (listView.Focus || thumbnailView.Focus)
				{
					GUIListItem item = GetItem(0);
					if (item!=null)
					{
						if (item.IsFolder && item.Label=="..")
						{
							LoadDirectory(item.Path);
							return;
						}
					}
				}
			}

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
			if (action.wID==Action.ActionType.ACTION_SHOW_PLAYLIST)
			{
				GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
				return;
			}

			base.OnAction(action);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadSettings();

			currentPlayList=null;
			virtualDirectory = new VirtualDirectory();
			Share share = new Share("default",currentRadioFolder);
			share.Default=true;
			virtualDirectory.Add(share);
			virtualDirectory.AddExtension(".pls");
			virtualDirectory.AddExtension(".asx");
          
			ShowThumbPanel();
			LoadDirectory(currentFolder);
			
			btnSortBy.SortChanged += new SortEventHandler(SortChanged);
		}
		
		protected override void OnPageDestroy(int newWindowId)
		{
			selectedItemIndex=GetSelectedItemNo();
			SaveSettings();
			base.OnPageDestroy (newWindowId);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnViewAs)
			{
				currentView = (View)btnViewAs.SelectedItem;
				ShowThumbPanel();
				GUIControl.FocusControl(GetID,controlId);
			}
          
			if (control==btnSortBy) // sort by
			{
				currentSortMethod = (SortMethod)btnSortBy.SelectedItem;
				OnSort();
				GUIControl.FocusControl(GetID,controlId);
			}

			if (control==listView || control==thumbnailView)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,controlId,0,0,null);
				OnMessage(msg);         
				int itemIndex=(int)msg.Param1;
				if (actionType==Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(itemIndex);
				}
			}
		}


		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				
				case GUIMessage.MessageType.GUI_MSG_PLAY_RADIO_STATION:
					if (message.Label.Length==0) return true;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
					PlayListPlayer.Reset();

					ArrayList stations = new ArrayList();
					RadioDatabase.GetStations(ref stations);
					foreach (RadioStation station in stations)
					{
						PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
						if (station.URL==String.Empty)
							playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Radio;
						else
							playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.AudioStream;
						playlistItem.FileName = GetPlayPath(station);
						playlistItem.Description = station.Name;
						playlistItem.Duration = 0;
						PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
					}
					PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
					foreach (RadioStation station in stations)
					{
						if ( station.Name.Equals(message.Label))
						{
							PlayListPlayer.Play( GetPlayPath(station));
							return true;
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
				if (currentView != View.List) return true;
				return false;
			}
		}

		bool ViewByLargeIcon
		{
			get
			{
				if (currentView == View.BigIcons) return true;
				return false;
			}
		}

		GUIListItem GetSelectedItem()
		{
			if (ViewByIcon)
				return thumbnailView.SelectedListItem;
			else
				return listView.SelectedListItem;
		}

		GUIListItem GetItem(int itemIndex)
		{
			if (ViewByIcon)
			{
				if (itemIndex >=thumbnailView.Count) return null;
				return thumbnailView[itemIndex];
			}
			else
			{
				if (itemIndex >=listView.Count) return null;
				return listView[itemIndex];
			}
		}

		int GetSelectedItemNo()
		{
			if (ViewByIcon)
				return thumbnailView.SelectedListItemIndex;
			else
				return listView.SelectedListItemIndex;
		}

		int GetItemCount()
		{
			if (ViewByIcon)
				return thumbnailView.Count;
			else
				return listView.Count;
		}

		void UpdateButtons()
		{
			listView.IsVisible=false;
			thumbnailView.IsVisible=false;
      
			int iControl=listView.GetID;
			if (ViewByIcon)
				iControl=thumbnailView.GetID;

			GUIControl.ShowControl(GetID,iControl);
			GUIControl.FocusControl(GetID,iControl);
     
			btnSortBy.IsAscending = sortAscending;
		}

		void ShowThumbPanel()
		{
			int itemIndex=GetSelectedItemNo(); 
			thumbnailView.ShowBigIcons(ViewByLargeIcon);
			if (itemIndex>-1)
			{
				GUIControl.SelectItemControl(GetID, listView.GetID,itemIndex);
				GUIControl.SelectItemControl(GetID, thumbnailView.GetID,itemIndex);
			}
			UpdateButtons();
		}

		void LoadDirectory(string strNewDirectory)
		{
			GUIListItem SelectedItem = GetSelectedItem();
			if (SelectedItem!=null) 
			{
				if (SelectedItem.IsFolder && SelectedItem.Label!="..")
				{
					directoryHistory.Set(SelectedItem.Label, currentFolder);
				}
			}
			currentFolder=strNewDirectory;
			listView.Clear();
			thumbnailView.Clear();

			string objectCount=String.Empty;
			int totalItems=0;

			if (currentPlayList!=null)
			{
				GUIListItem item = new GUIListItem();
				item.Label="..";
				item.Path=currentFolder;
				item.IsFolder=true;
				item.MusicTag = null;
				item.ThumbnailImage=String.Empty;
				Utils.SetDefaultIcons(item);
				listView.Add(item);
				thumbnailView.Add(item);

				for (int i=0; i < currentPlayList.Count;++i)
				{
					item = new GUIListItem();
					item.Label=currentPlayList[i].Description;
					item.Path=currentPlayList[i].FileName;
					item.IsFolder=false;
					item.MusicTag = null;
					item.ThumbnailImage=String.Empty;
					item.IconImageBig="DefaultMyradioStreamBig.png";
					item.IconImage="DefaultMyradioStream.png";

					listView.Add(item);
					thumbnailView.Add(item);
					totalItems++;
				}
			}
			else
			{
				if (currentFolder.Length==0 || currentFolder.Equals(currentRadioFolder))
				{
					ArrayList stations = new ArrayList();
					RadioDatabase.GetStations(ref stations);
					foreach (RadioStation station in stations)
					{
						GUIListItem item = new GUIListItem();
						item.Label=station.Name;
						item.IsFolder=false;
						item.MusicTag = station;
						if (station.URL.Length>5)
						{
							item.IconImageBig="DefaultMyradioStreamBig.png";
							item.IconImage = "DefaultMyradioStream.png";
							string thumbnail = Utils.GetCoverArt(Thumbs.Radio, station.Name);
							if (System.IO.File.Exists(thumbnail))
							{
								item.IconImageBig = thumbnail;
								item.IconImage = thumbnail;
								item.ThumbnailImage = thumbnail;
							}
						}
						else
						{
							if (station.Scrambled)
							{
								item.IconImageBig="DefaultMyradioBigLocked.png";
								item.IconImage="DefaultMyradioLocked.png";
							}
							else
							{
								item.IconImageBig="DefaultMyradioBig.png";
								item.IconImage="DefaultMyradio.png";
							}
							string thumbnail=Utils.GetCoverArt(Thumbs.Radio,station.Name);
							if (System.IO.File.Exists(thumbnail))
							{
								item.IconImageBig = thumbnail;
								item.IconImage = thumbnail;
								item.ThumbnailImage = thumbnail;
							}
							//if (item.ThumbnailImage==String.Empty)
							//  item.ThumbnailImage="DefaultMyradioBig.png";
						}

						listView.Add(item);
						thumbnailView.Add(item);
						totalItems++;
					}
				}

				if (currentRadioFolder.Length!=0)
				{
					string folerName=currentFolder;
					if (folerName.Length==0) folerName=currentRadioFolder;
					ArrayList items = new ArrayList();
					items=virtualDirectory.GetDirectory(folerName);
					foreach (GUIListItem item in items)
					{
						if (!item.IsFolder)
						{
							item.MusicTag = null;
							//item.ThumbnailImage="DefaultMyradioStream.png";
							item.IconImageBig="DefaultMyradioStreamBig.png";
							item.IconImage="DefaultMyradioStream.png";
						}
						else
						{
							if (item.Label.Equals(".."))
							{
								if (currentFolder.Length==0 || currentFolder.Equals(currentRadioFolder)) continue;
							}
						}

						listView.Add(item);
						thumbnailView.Add(item);
						totalItems++;
					}
				}
			}

			OnSort();
			objectCount=String.Format("{0} {1}", totalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",objectCount);
			ShowThumbPanel();
      
			if (selectedItemIndex>=0)
			{
				GUIControl.SelectItemControl(GetID,listView.GetID,selectedItemIndex);
				GUIControl.SelectItemControl(GetID,thumbnailView.GetID,selectedItemIndex);
			}

		}
		#endregion

		void SetLabels()
		{
			SortMethod method=currentSortMethod;

			for (int i=0; i < GetItemCount();++i)
			{
				GUIListItem item=GetItem(i);
				if (item.MusicTag!=null)
				{
					RadioStation station=(RadioStation) item.MusicTag;
					if (method==SortMethod.Bitrate) 
					{
						if (station.BitRate>0)
							item.Label2=station.BitRate.ToString();
						else
						{
							double frequency=station.Frequency;
							frequency/=1000000d;
							item.Label2=System.String.Format("{0:###.##} MHz.", frequency);
						}
					}
					else 
					{
						if (station.Genre==Strings.Unknown && station.Frequency>0)
						{
							double frequency=station.Frequency;
							frequency/=1000000d;
							if (frequency>80 && frequency <120)
							{
								item.Label2=System.String.Format("{0:###.##} MHz.", frequency);
							}
							else
							{
								item.Label2=station.Genre;
							}
						}
						else
						{
							item.Label2=station.Genre;
						}
					}
				}
			}
		}

		#region Sort Members
		void OnSort()
		{
			SetLabels();
			listView.Sort(this);
			thumbnailView.Sort(this);

			UpdateButtons();
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


			SortMethod method=currentSortMethod;
			bool bAscending=sortAscending;
			RadioStation station1 = item1.MusicTag as RadioStation ;
			RadioStation station2 = item2.MusicTag as RadioStation ;
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
        
				case SortMethod.Type:
					string strURL1=String.Empty;
					string strURL2=String.Empty;
					if (station1!=null) strURL1=station1.URL;
					else 
					{
						if ( item1.IconImage.ToLower().Equals("defaultmyradiostream.png") )
							strURL1="1";
					}

					if (station2!=null) strURL2=station2.URL;
					else 
					{
						if ( item2.IconImage.ToLower().Equals("defaultmyradiostream.png") )
							strURL2="1";
					}

					if (strURL1.Equals(strURL2))
					{
						if (bAscending)
						{
							return String.Compare(item1.Label ,item2.Label,true);
						}
						else
						{
							return String.Compare(item2.Label ,item1.Label,true);
						}
					}
					if (bAscending)
					{
						if (strURL1.Length>0) return 1;
						else return -1;
					}
					else
					{
						if (strURL1.Length>0) return -1;
						else return 1;
					}
					//break;

				case SortMethod.Genre:
					if (station1!=null && station2!=null)
					{
						if (station1.Genre.Equals(station2.Genre))
							goto case SortMethod.Bitrate;
						if (bAscending)
						{
							return String.Compare(station1.Genre,station2.Genre,true);
						}
						else
						{
							return String.Compare(station2.Genre,station1.Genre,true);
						}
					}
					else
					{
						return 0;
					}
					//break;
          
				case SortMethod.Number:
					if (station1!=null && station2!=null)
					{
						if (bAscending)
						{
							if (station1.Channel>station2.Channel) return 1;
							else return -1;
						}
						else
						{
							if (station2.Channel>station1.Channel) return 1;
							else return -1;
						}
					}
          
					if (station1!=null) return -1;
					if (station2!=null) return 1;
					return 0;
					//break;
				case SortMethod.Bitrate:
					if (station1!=null && station2!=null)
					{
						if (bAscending)
						{
							if (station1.BitRate>station2.BitRate) return 1;
							else return -1;
						}
						else
						{
							if (station2.BitRate>station1.BitRate) return 1;
							else return -1;
						}
					}
					return 0;
			}
			return 0;
		}
		#endregion

		void OnClick(int itemIndex)
		{
			GUIListItem item = GetSelectedItem();
			if (item==null) return;
			if (item.IsFolder)
			{
				if (currentPlayList!=null)
				{
					currentPlayList=null;
				}
				selectedItemIndex=-1;
				LoadDirectory(item.Path);
			}
			else
			{
				Play(item);
			}
		}

		void FillPlayList()
		{
			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
			PlayListPlayer.Reset();

			// are we looking @ a playlist
			if (currentPlayList!=null)
			{
				//yes, then add current playlist to playlist player
				for (int i=0; i < currentPlayList.Count;++i)
				{
					PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
					playlistItem.Type = currentPlayList[i].Type;
					playlistItem.FileName = currentPlayList[i].FileName;
					playlistItem.Description = currentPlayList[i].Description;
					playlistItem.Duration = currentPlayList[i].Duration;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
				}
			}
			else
			{
				//add current directory to playlist player
				for (int i=0; i < GetItemCount();++i)
				{
					GUIListItem item = GetItem(i);
					if (item.IsFolder) continue;

					// if item is a playlist
					if (Utils.IsPlayList(item.Path))
					{
						// then load the playlist
						PlayList playlist = PlayListFactory.Create(item.Path);
						if (playlist!=null)
						{
							playlist.Load(item.Path);
							// and if it contains any items
							if (playlist.Count>0)
							{
								// then add the 1st item to the playlist player
								PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
								playlistItem.FileName = playlist[0].FileName;
								playlistItem.Description = playlist[0].Description;
								playlistItem.Duration = playlist[0].Duration;
								playlistItem.Type = playlist[0].Type;
								PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
							}
						}
					}
					else
					{
						// item is just a normal file like .asx, .pls
						// or a radio station from the setup.
						RadioStation station = item.MusicTag as RadioStation;
						PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
						if (station!=null)
						{
							playlistItem.FileName = GetPlayPath(station);
							if (station.URL==String.Empty) playlistItem.Type=Playlists.PlayList.PlayListItem.PlayListItemType.Radio;
							else playlistItem.Type=Playlists.PlayList.PlayListItem.PlayListItemType.AudioStream;
						}
						else
						{
							playlistItem.Type=Playlists.PlayList.PlayListItem.PlayListItemType.AudioStream;
							playlistItem.FileName = item.Path;
						}
						playlistItem.Description = item.Label;
						playlistItem.Duration = 0;
						PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
					}
				}
			}
			PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
		}
    
		void Play(GUIListItem item)
		{
			if (Utils.IsPlayList(item.Path))
			{
				currentPlayList = PlayListFactory.Create(item.Path);
				if (currentPlayList!=null)
				{
					currentPlayList.Load(item.Path);
					if (currentPlayList.Count==1)
					{
						// add current directory 2 playlist and play this item
						string strURL=currentPlayList[0].FileName;
						currentPlayList=null;
						FillPlayList();
						PlayListPlayer.Play(strURL);
						return;
					}
					if (currentPlayList.Count==0)
					{
						currentPlayList=null;
					}
					LoadDirectory(currentFolder);
				}
			}
			else
			{
				if (currentPlayList!=null)
				{
					// add current playlist->playlist and play selected item
					string strURL=item.Path;
					FillPlayList();
					PlayListPlayer.Play(strURL);
					return;
				}

				// add current directory 2 playlist and play this item
				RadioStation station = item.MusicTag as RadioStation;
				FillPlayList();

				PlayList playlist=PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP);
				for (int i=0; i < playlist.Count;++i)
				{
					PlayList.PlayListItem playItem=playlist[i];
					if (playItem.Description.Equals(item.Label))
					{
						PlayListPlayer.Play (i);
						break;
					}
				}
			}
		}

		string GetPlayPath(RadioStation station)
		{
			if (station.URL.Length>5)
			{
				return station.URL;
			}
			else
			{
				string fileName=String.Format("{0}.radio",station.Frequency);
				return fileName;
			}
		}

		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public string PluginName()
		{
			return "My Radio";
		}

		public bool HasSetup()
		{
			return false;
		}
		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = GUILocalizeStrings.Get(665);
			strButtonImage = String.Empty;
			strButtonImageFocus = String.Empty;
			strPictureImage = String.Empty;
			return true;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string Description()
		{
			return "Plugin to listen to local & internet radio";
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIRadio.ShowPlugin implementation
		}

		#endregion

		void SortChanged(object sender, SortEventArgs e)
		{
			sortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;

			OnSort();
			UpdateButtons();

			GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
		}
	}
}