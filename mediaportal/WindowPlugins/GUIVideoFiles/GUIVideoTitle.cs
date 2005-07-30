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
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Video.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class GUIVideoTitle: GUIVideoBaseWindow
	{ 
		#region Base variabeles

		DirectoryHistory  m_history = new DirectoryHistory();
		string            currentFolder=String.Empty;
		int               currentSelectedItem=-1;   
		VirtualDirectory  m_directory = new VirtualDirectory();
		int[]  views      = new int[50];
		bool[] sortasc    = new bool[50];
		int[] sortby      = new int[50];


		#endregion

		public GUIVideoTitle()
		{
			for (int i=0; i < sortasc.Length;++i)
				sortasc[i]=true;
			GetID=(int)GUIWindow.Window.WINDOW_VIDEO_TITLE;
      
			m_directory.AddDrives();
			m_directory.SetExtensions (Utils.VideoExtensions);
		}

		#region overrides
		public override bool Init()
		{
			currentFolder=String.Empty;
			handler.CurrentView="Title";
			return Load (GUIGraphicsContext.Skin+@"\myvideoTitle.xml");
		}
		protected override string SerializeName
		{
			get
			{
				return "myvideo"+handler.CurrentView;
			}
		}

		protected override View CurrentView
		{
			get
			{
				return (View)views[handler.CurrentLevel];
			}
			set
			{
				views[handler.CurrentLevel] = (int)value;
			}
		}

		protected override bool CurrentSortAsc
		{
			get
			{
				return sortasc[handler.CurrentLevel];
			}
			set
			{
				sortasc[handler.CurrentLevel]=value;
			}
		}
		protected override VideoSort.SortMethod CurrentSortMethod
		{
			get
			{
				return (VideoSort.SortMethod)sortby[handler.CurrentLevel];
			}
			set
			{
				sortby[handler.CurrentLevel]=(int)value;
			}
		}
		protected override bool AllowView(View view)
		{
			return true;
		}

		public override void OnAction(Action action)
		{

			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				if (facadeView.Focus)
				{
					GUIListItem item = facadeView[0];
					if (item != null)
					{
						if (item.IsFolder && item.Label == "..")
						{
							if (handler.CurrentLevel>0)
							{
								handler.CurrentLevel--;
								LoadDirectory(item.Path);
								return;
							}
						}
					}
				}
			}
			if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
			{
				GUIListItem item = facadeView[0];
				if (item!=null)
				{
					if (item.IsFolder && item.Label=="..")
					{
						handler.CurrentLevel--;
						LoadDirectory(item.Path);
					}
				}
				return;
			}
			base.OnAction(action);
		}

		protected override void OnPageLoad()
		{
			string view=VideoState.View;
			if (view==String.Empty)
				view=((ViewDefinition)handler.Views[0]).Name;

			handler.CurrentView = view;
			base.OnPageLoad ();
			LoadDirectory(currentFolder);
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			currentSelectedItem=facadeView.SelectedListItemIndex;
			if (newWindowId==(int)GUIWindow.Window.WINDOW_VIDEO_TITLE ||
					newWindowId==(int)GUIWindow.Window.WINDOW_VIDEOS)
			{
				VideoState.StartWindow=newWindowId;
			}
			base.OnPageDestroy (newWindowId);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
		}
		
		protected override void OnClick(int itemIndex)
		{
			GUIListItem item = facadeView.SelectedListItem;
			if (item==null) return;
			if (item.IsFolder)
			{
				currentSelectedItem=-1;
				if (item.Label=="..")
					handler.CurrentLevel--;
				else
					handler.Select(item.AlbumInfoTag as IMDBMovie);
				LoadDirectory(item.Path);
			}
			else
			{
				IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
				if (movie==null) return;
				if (movie.ID<0) return;
				GUIVideoFiles.Reset(); // reset pincode
				GUIVideoFiles.PlayMovie(movie.ID);
			}
		}
    


		protected override  void OnShowContextMenu()
		{
			GUIListItem item=facadeView.SelectedListItem;
			int itemNo=facadeView.SelectedListItemIndex;
			if (item==null) return;
			IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
			if (movie==null) return;
			if (movie.ID<0) return;

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu
			dlg.Add( GUILocalizeStrings.Get(925)); //delete
			dlg.Add( GUILocalizeStrings.Get(368)); //IMDB
			dlg.Add( GUILocalizeStrings.Get(208)); //play

			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			switch (dlg.SelectedLabel)
			{
				case 0: // Delete
					OnDeleteItem(item);
					break;

				case 1: // IMDB
					OnInfo(itemNo);
					break;

				case 2: // play
					OnClick(itemNo);	
					break;
			}
		}


		protected override  void OnQueueItem(int itemIndex)
		{
			// add item 2 playlist
			GUIListItem listItem=facadeView[itemIndex];
			ArrayList files = new ArrayList();
			if (handler.CurrentLevel < handler.MaxLevels-1)
			{
				//queue
				handler.Select(listItem.AlbumInfoTag as IMDBMovie);
				ArrayList movies = handler.Execute();
				handler.CurrentLevel--;
				foreach (IMDBMovie movie in movies)
				{
					if (movie.ID>0)
					{
						GUIListItem item = new GUIListItem();
						item.Path=movie.File;
						item.Label=movie.Title;
						item.Duration=movie.RunTime*60;
						item.IsFolder=false;
						VideoDatabase.GetFiles(movie.ID,ref files);
						foreach (string file in files)
						{
							item.AlbumInfoTag=movie;
							item.Path=file;
							AddItemToPlayList(item);
						}

					}
				}
			}
			else
			{
				IMDBMovie movie =listItem.AlbumInfoTag as IMDBMovie;
				VideoDatabase.GetFiles(movie.ID,ref files);
				foreach (string file in files)
				{
					listItem.Path=file;
					AddItemToPlayList(listItem);
				}
			}
			//move to next item
			GUIControl.SelectItemControl(GetID, facadeView.GetID,itemIndex+1);

		}
		#endregion

		
		protected override void LoadDirectory(string strNewDirectory)
		{
			GUIListItem SelectedItem = facadeView.SelectedListItem;
			if (SelectedItem!=null) 
			{
				if (SelectedItem.IsFolder && SelectedItem.Label!="..")
				{
					m_history.Set(SelectedItem.Label, currentFolder);
				}
			}
			currentFolder=strNewDirectory;
			
			GUIControl.ClearControl(GetID,facadeView.GetID);
            
			string objectCount=String.Empty;

			ArrayList itemlist = new ArrayList();
			ArrayList movies=handler.Execute();
			if (handler.CurrentLevel>0)
			{
				GUIListItem listItem = new GUIListItem ("..");
				listItem.Path=String.Empty;
				listItem.IsFolder=true;
				Utils.SetDefaultIcons(listItem);
				itemlist.Add(listItem);
			}
			foreach (IMDBMovie movie in movies)
			{
				GUIListItem item=new GUIListItem();
				item.Label=movie.Title;
				if (handler.CurrentLevel+1 < handler.MaxLevels)
					item.IsFolder=true;
				else
					item.IsFolder=false;
				item.Path=movie.File;
				item.Duration=movie.RunTime*60;
				item.AlbumInfoTag=movie;
				item.Year=movie.Year;
				item.DVDLabel=movie.DVDLabel;
				item.OnItemSelected	+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);

				itemlist.Add(item);
			}
      
			string selectedItemLabel=m_history.Get(currentFolder);	
			int itemIndex=0;
			foreach (GUIListItem item in itemlist)
			{
				facadeView.Add(item);
			}

			int itemCount=itemlist.Count;
			if (itemlist.Count>0)
			{
				GUIListItem rootItem=(GUIListItem)itemlist[0];
				if (rootItem.Label=="..") itemCount--;
			}
			objectCount=String.Format("{0} {1}", itemCount, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",objectCount);
			SetIMDBThumbs(itemlist);

			SetLabels();
			OnSort();
		
			FilterDefinition def =  handler.View.Filters[handler.CurrentLevel] as FilterDefinition;
			if (def!=null)
			{
				if (def.DefaultView=="List")
					CurrentView=GUIVideoBaseWindow.View.List;
				if (def.DefaultView=="Icons")
					CurrentView=GUIVideoBaseWindow.View.Icons;
				if (def.DefaultView=="Big Icons")
					CurrentView=GUIVideoBaseWindow.View.LargeIcons;
				if (def.DefaultView=="Albums")
					CurrentView=GUIVideoBaseWindow.View.List;
				if (def.DefaultView=="Filmstrip")
					CurrentView=GUIVideoBaseWindow.View.FilmStrip;
			}
	
			SwitchView();
			for (int i=0; i< facadeView.Count;++i)
			{
				GUIListItem item =facadeView[itemIndex];
				if (item.Label==selectedItemLabel)
				{
					GUIControl.SelectItemControl(GetID,facadeView.GetID,itemIndex);
					break;
				}
				itemIndex++;
			}
			if (currentSelectedItem>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,currentSelectedItem);
			}
		}
	  

		protected override void SetLabels()
		{
			base.SetLabels ();
			for (int i=0; i < facadeView.Count;++i)
			{
				GUIListItem item=facadeView[i];
				handler.SetLabel(item.AlbumInfoTag as IMDBMovie, ref item);
			}
		}
		void OnDeleteItem(GUIListItem item)
		{
			if (item.IsRemote) return;
			IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
			if (movie==null) return;
			if (movie.ID<0) return;

			GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (null==dlgYesNo) return;
			dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
			dlgYesNo.SetLine(1,movie.Title);
			dlgYesNo.SetLine(2, String.Empty);
			dlgYesNo.SetLine(3, String.Empty);
			dlgYesNo.DoModal(GetID);

			if (!dlgYesNo.IsConfirmed) return;
			
			DoDeleteItem(item);
						
			currentSelectedItem=facadeView.SelectedListItemIndex;
			if (currentSelectedItem>0) currentSelectedItem--;
			LoadDirectory(currentFolder);
			if (currentSelectedItem>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,currentSelectedItem);
			}
		}

		void DoDeleteItem(GUIListItem item)
		{
			IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
			if (movie==null) return;
			if (movie.ID<0) return;
			if (item.IsFolder )return;
			if (!item.IsRemote)
			{
				VideoDatabase.DeleteMovieInfoById(movie.ID);
			}
		}

		protected override void OnInfo(int itemIndex)
		{
			GUIListItem item=facadeView[itemIndex];
			if (item==null) return;
			IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
			if (movie==null) return;
			if (movie.ID>=0)
			{
				GUIVideoFiles.ShowIMDB(movie.ID);
			}
			if (movie.actorId>=0)
			{
				ShowActorInfo(movie.actorId);
			}
		}
		void ShowActorInfo(int actorId)
		{
			GUIVideoArtistInfo infoDlg = (GUIVideoArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_ARTIST_INFO);
			if (infoDlg==null) return;
			IMDBActor actor= VideoDatabase.GetActorInfo(actorId);
			if (actor==null) return;
			infoDlg.Actor=actor;
			infoDlg.DoModal(GetID);
		}

		void SetIMDBThumbs(ArrayList items)
		{
			for (int x = 0; x < items.Count; ++x)
			{
				GUIListItem listItem = (GUIListItem)items[x];
				IMDBMovie movie = listItem.AlbumInfoTag as IMDBMovie;
				if (movie!=null)
				{
					if (movie.ID>=0)
					{
						string coverArtImage;
						coverArtImage = Utils.GetCoverArt(Thumbs.MovieTitle,movie.Title);
						if (System.IO.File.Exists(coverArtImage))
						{
							listItem.ThumbnailImage = coverArtImage;
							listItem.IconImageBig = coverArtImage;
							listItem.IconImage = coverArtImage;
						}
						
					}
					else if (movie.Actor!=String.Empty)
					{
						string coverArtImage;
						coverArtImage = Utils.GetCoverArt(Thumbs.MovieActors,movie.Actor);
						if (System.IO.File.Exists(coverArtImage))
						{
							listItem.ThumbnailImage = coverArtImage;
							listItem.IconImageBig = coverArtImage;
							listItem.IconImage = coverArtImage;
						}
					}
				}
			}
		}

		private void item_OnItemSelected(GUIListItem item, GUIControl parent)
		{
			IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
			if(movie==null) movie = new IMDBMovie();
			movie.SetProperties();
			if (movie.ID>=0)
			{
				string coverArtImage;
				coverArtImage = Utils.GetLargeCoverArtName(Thumbs.MovieTitle,movie.Title);
				if (System.IO.File.Exists(coverArtImage))
				{
					facadeView.FilmstripView.InfoImageFileName=coverArtImage;
				}
			}
			else if (movie.Actor!=String.Empty)
			{
				string coverArtImage;
				coverArtImage = Utils.GetLargeCoverArtName(Thumbs.MovieActors,movie.Actor);
				if (System.IO.File.Exists(coverArtImage))
				{
					facadeView.FilmstripView.InfoImageFileName=coverArtImage;
				}
			}
		}
	}
}
