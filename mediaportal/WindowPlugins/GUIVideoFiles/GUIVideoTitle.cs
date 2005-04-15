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
	public class GUIVideoTitle: GUIVideoBaseWindow, IComparer
	{ 
		#region Base variabeles

		DirectoryHistory  m_history = new DirectoryHistory();
		string            m_strDirectory=String.Empty;
		int               m_iItemSelected=-1;   
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
			m_strDirectory=String.Empty;
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
		protected override SortMethod CurrentSortMethod
		{
			get
			{
				return (SortMethod)sortby[handler.CurrentLevel];
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
			LoadDirectory(m_strDirectory);
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			m_iItemSelected=facadeView.SelectedListItemIndex;
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
		
		protected override void OnClick(int iItem)
		{
			GUIListItem item = facadeView.SelectedListItem;
			if (item==null) return;
			if (item.IsFolder)
			{
				m_iItemSelected=-1;
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


		protected override  void OnQueueItem(int iItem)
		{
			// add item 2 playlist
			GUIListItem pItem=facadeView[iItem];
			if (handler.CurrentLevel < handler.MaxLevels-1)
			{
				//queue
				handler.Select(pItem.AlbumInfoTag as IMDBMovie);
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
						AddItemToPlayList(item);
					}
				}
			}
			else
			{
				AddItemToPlayList(pItem);
			}
			//move to next item
			GUIControl.SelectItemControl(GetID, facadeView.GetID,iItem+1);

		}
		#endregion

		
		protected override void LoadDirectory(string strNewDirectory)
		{
			GUIListItem SelectedItem = facadeView.SelectedListItem;
			if (SelectedItem!=null) 
			{
				if (SelectedItem.IsFolder && SelectedItem.Label!="..")
				{
					m_history.Set(SelectedItem.Label, m_strDirectory);
				}
			}
			m_strDirectory=strNewDirectory;
			
			GUIControl.ClearControl(GetID,facadeView.GetID);
            
			string strObjects=String.Empty;

			ArrayList itemlist = new ArrayList();
			ArrayList movies=handler.Execute();
			if (handler.CurrentLevel>0)
			{
				GUIListItem pItem = new GUIListItem ("..");
				pItem.Path=String.Empty;
				pItem.IsFolder=true;
				Utils.SetDefaultIcons(pItem);
				itemlist.Add(pItem);
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
			SetIMDBThumbs(itemlist);

			SetLabels();
			OnSort();
			for (int i=0; i< facadeView.Count;++i)
			{
				GUIListItem item =facadeView[iItem];
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
			dlgYesNo.SetHeading(GUILocalizeStrings.Get(664));
			dlgYesNo.SetLine(1,movie.Title);
			dlgYesNo.SetLine(2, String.Empty);
			dlgYesNo.SetLine(3, String.Empty);
			dlgYesNo.DoModal(GetID);

			if (!dlgYesNo.IsConfirmed) return;
			
			DoDeleteItem(item);
						
			m_iItemSelected=facadeView.SelectedListItemIndex;
			if (m_iItemSelected>0) m_iItemSelected--;
			LoadDirectory(m_strDirectory);
			if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,m_iItemSelected);
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

		protected override void OnInfo(int iItem)
		{
			GUIListItem item=facadeView[iItem];
			if (item==null) return;
			IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
			if (movie==null) return;
			if (movie.ID<0) return;
	
			GUIVideoFiles.ShowIMDB(movie.ID);
		}

		void SetIMDBThumbs(ArrayList items)
		{
			for (int x = 0; x < items.Count; ++x)
			{
				GUIListItem pItem = (GUIListItem)items[x];
				IMDBMovie movie = pItem.AlbumInfoTag as IMDBMovie;
				if (movie!=null)
				{
					if (movie.ID>=0)
					{
						string strThumb;
						strThumb = Utils.GetCoverArt(Thumbs.MovieTitle,movie.Title);
						if (System.IO.File.Exists(strThumb))
						{
							pItem.ThumbnailImage = strThumb;
							pItem.IconImageBig = strThumb;
							pItem.IconImage = strThumb;
						}
					}
					else if (movie.Actor!=String.Empty)
					{
						string strThumb;
						strThumb = Utils.GetCoverArt(Thumbs.MovieActors,movie.Actor);
						if (System.IO.File.Exists(strThumb))
						{
							pItem.ThumbnailImage = strThumb;
							pItem.IconImageBig = strThumb;
							pItem.IconImage = strThumb;
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
				string strThumb;
				strThumb = Utils.GetLargeCoverArtName(Thumbs.MovieTitle,movie.Title);
				if (System.IO.File.Exists(strThumb))
				{
					facadeView.FilmstripView.InfoImageFileName=strThumb;
				}
			}
			else if (movie.Actor!=String.Empty)
			{
				string strThumb;
				strThumb = Utils.GetLargeCoverArtName(Thumbs.MovieActors,movie.Actor);
				if (System.IO.File.Exists(strThumb))
				{
					facadeView.FilmstripView.InfoImageFileName=strThumb;
				}
			}
		}
	}
}
