using System;
using System.Collections;
using System.Net;
using System.Globalization;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Video

{
  /// <summary>
  /// Summary description for Class1.
  /// 
  /// </summary>
	public class GUIVideoFiles : GUIWindow, IComparer, ISetupForm
	{
    
    [Serializable]
    public class MapSettings
    {
      protected int   _SortBy;
      protected int   _ViewAs;
      protected bool  _Stack;
      protected bool _SortAscending ;

      public MapSettings()
      {
        _SortBy=0;//name
        _ViewAs=0;//list
        _Stack=true;
        _SortAscending=true;
      }


      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy;}
        set { _SortBy=value;}
      }
      
      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs;}
        set { _ViewAs=value;}
      }
      
      [XmlElement("Stack")]
      public bool Stack
      {
        get { return _Stack;}
        set { _Stack=value;}
      }
      
      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending;}
        set { _SortAscending=value;}
      }
    }

		enum Controls
		{
			CONTROL_BTNVIEWASICONS = 2, 
			CONTROL_BTNSORTBY = 3, 
			CONTROL_BTNSORTASC = 4, 
			CONTROL_BTNTYPE = 5, 
			CONTROL_PLAY_DVD = 6, 
			CONTROL_STACK = 7, 
			CONTROL_BTNSCAN = 8, 
			CONTROL_IMDB = 9, 
			CONTROL_VIEW = 10, 
			CONTROL_LABELFILES = 12, 
  };
		#region Base variabeles
		enum SortMethod
		{
			SORT_NAME = 0, 
			SORT_DATE = 1, 
			SORT_SIZE = 2
		}

		enum View
		{
			VIEW_AS_LIST = 0, 
			VIEW_AS_ICONS = 1, 
      VIEW_AS_LARGEICONS = 2, 
      VIEW_AS_FILMSTRIP   =   3,
		}

    const string ThumbsFolder=@"thumbs\Videos\Title";
		DirectoryHistory m_history = new DirectoryHistory();
		string            m_strDirectory = "";
		int               m_iItemSelected = -1;
		static VirtualDirectory  m_directory = new VirtualDirectory();
    MapSettings       _MapSettings = new MapSettings();
    bool              m_askBeforePlayingDVDImage = false;
		#endregion

		public GUIVideoFiles()
		{
			GetID = (int)GUIWindow.Window.WINDOW_VIDEOS;
      
			m_directory.AddDrives();
			m_directory.SetExtensions(Utils.VideoExtensions);
      //m_directory.AddExtension(".m3u");
      //m_directory.AddExtension(".pls");
      //m_directory.AddExtension(".b4s");

    }
    ~GUIVideoFiles()
    {
    }

		public override void DeInit()
		{
			SaveSettings();
		}

		public override bool Init()
		{
      g_Player.PlayBackStopped +=new MediaPortal.Player.g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.PlayBackEnded +=new MediaPortal.Player.g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStarted +=new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
      m_strDirectory = "";
			try
			{
				System.IO.Directory.CreateDirectory(@"Thumbs\videos");
				System.IO.Directory.CreateDirectory(@"Thumbs\videos\genre");
				System.IO.Directory.CreateDirectory(ThumbsFolder);
			}
			catch(Exception){}
			LoadSettings();
			bool result=Load(GUIGraphicsContext.Skin + @"\myVideo.xml");
			LoadSettings();
			return result;
		}

    #region Serialisation
    void LoadSettings()
    {
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
				
				VideoState.StartWindow=xmlreader.GetValueAsInt("movies","startWindow", GetID);
        m_directory.Clear();
        string strDefault = xmlreader.GetValueAsString("movies", "default","");
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}",i);
          string strSharePath = String.Format("sharepath{0}",i);
          string strPincode = String.Format("pincode{0}",i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd  = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("movies", strShareName, "");
          share.Path = xmlreader.GetValueAsString("movies", strSharePath, "");
          share.Pincode = xmlreader.GetValueAsInt("movies", strPincode, - 1);
          
          share.IsFtpShare= xmlreader.GetValueAsBool("movies", shareType, false);
          share.FtpServer= xmlreader.GetValueAsString("movies", shareServer,"");
          share.FtpLoginName= xmlreader.GetValueAsString("movies", shareLogin,"");
          share.FtpPassword= xmlreader.GetValueAsString("movies", sharePwd,"");
          share.FtpPort= xmlreader.GetValueAsInt("movies", sharePort,21);
          share.FtpFolder= xmlreader.GetValueAsString("movies", remoteFolder,"/");

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default=true;
              if (m_strDirectory.Length==0) m_strDirectory = share.Path;
            }
            m_directory.Add(share);
          }
          else break;
        }
        m_askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
      }
    }

    void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("movies","startWindow",VideoState.StartWindow.ToString());

			}
    }
    #endregion




		#region BaseWindow Members
		public override void OnAction(Action action)
		{
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = GetItem(0);
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }

			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				GUIWindowManager.PreviousWindow();
				return;
			}
			if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
				return;
			}


			if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
			{
				ShowContextMenu();
			}
			base.OnAction(action);
		}

		
		public override bool OnMessage(GUIMessage message)
		{
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
          if (VideoState.StartWindow != GetID)
          {
            GUIWindowManager.ReplaceWindow(VideoState.StartWindow);
            return false;
          }
					base.OnMessage(message);
          LoadFolderSettings(m_strDirectory);
					ShowThumbPanel();
					LoadDirectory(m_strDirectory);
					return true;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					m_iItemSelected = GetSelectedItemNo();
					
          SaveFolderSettings(m_strDirectory);
					break;

				case GUIMessage.MessageType.GUI_MSG_CLICKED : 
					int iControl = message.SenderControlId;
					if (iControl == (int)Controls.CONTROL_BTNVIEWASICONS)
					{
            switch ((View)_MapSettings.ViewAs)
            {
              case View.VIEW_AS_LIST : 
                _MapSettings.ViewAs = (int)View.VIEW_AS_ICONS;
                break;
              case View.VIEW_AS_ICONS : 
                _MapSettings.ViewAs = (int)View.VIEW_AS_LARGEICONS;
                break;
              case View.VIEW_AS_LARGEICONS : 
                _MapSettings.ViewAs = (int)View.VIEW_AS_FILMSTRIP;
                break;
            case View.VIEW_AS_FILMSTRIP : 
              _MapSettings.ViewAs = (int)View.VIEW_AS_LIST;
              break;
            }
            ShowThumbPanel();
            GUIControl.FocusControl(GetID, iControl);
					}
          
					if (iControl == (int)Controls.CONTROL_BTNSORTASC)
					{
            _MapSettings.SortAscending = !_MapSettings.SortAscending;
            OnSort();
            GUIControl.FocusControl(GetID, iControl);
					}

          if (iControl == (int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            switch ( (SortMethod)_MapSettings.SortBy)
            {
              case SortMethod.SORT_NAME : 
                _MapSettings.SortBy = (int)SortMethod.SORT_DATE;
                break;
              case SortMethod.SORT_DATE : 
                _MapSettings.SortBy = (int)SortMethod.SORT_SIZE;
                break;
              case SortMethod.SORT_SIZE : 
                _MapSettings.SortBy = (int)SortMethod.SORT_NAME;
                break;
            }
            OnSort();
            GUIControl.FocusControl(GetID, iControl);
          }

          if (iControl == (int)Controls.CONTROL_BTNTYPE)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int nSelected = (int)msg.Param1;
				    int nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
				    switch (nSelected)
				    {
				      case 0 : //	Movies
					      nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
					      break;
				      case 1 : //	Genre
					      nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_GENRE;
					      break;
				      case 2 : //	Actors
					      nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_ACTOR;
					      break;
				      case 3 : //	Year
					      nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_YEAR;
					      break;
				      case 4 : //	Titel
					      nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_TITLE;
					      break;
				    }

				    if (nNewWindow != GetID)
				    {
					    
              VideoState.StartWindow = nNewWindow;
					    GUIWindowManager.ReplaceWindow(nNewWindow);
				    }

            return true;
          }
					if (iControl == (int)Controls.CONTROL_VIEW)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
						OnMessage(msg);
						int iItem = (int)msg.Param1;
						int iAction = (int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SHOW_INFO) 
						{
							OnInfo(iItem);
              LoadDirectory(m_strDirectory);
						}
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
						{
							OnClick(iItem);
						}
						if (iAction == (int)Action.ActionType.ACTION_QUEUE_ITEM)
						{
							OnQueueItem(iItem);
						}

					}
					if (iControl == (int)Controls.CONTROL_IMDB) 
					{
            OnManualIMDB();
            LoadDirectory(m_strDirectory);
					}
					if (iControl == (int)Controls.CONTROL_BTNSCAN) 
					{
            if (m_directory.IsRemote(m_strDirectory)) return true;
            ArrayList itemlist = m_directory.GetDirectory(m_strDirectory);
            OnScan(itemlist);
            LoadDirectory(m_strDirectory);
					}
					if (iControl == (int)Controls.CONTROL_STACK)
					{
						_MapSettings.Stack = !_MapSettings.Stack;
            LoadDirectory(m_strDirectory);
            UpdateButtons();
					}
					if (iControl == (int)Controls.CONTROL_PLAY_DVD)
					{
						OnPlayDVD();
					}
					break;

          case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
            if (g_Player.Playing && g_Player.IsDVD)
            {
							Log.Write("GUIVideo:stop dvd since DVD is ejected");
              g_Player.Stop();
            }

            if (GUIWindowManager.ActiveWindow==GetID)
            {
              if (Utils.IsDVD(m_strDirectory))
              {
                m_strDirectory="";
                LoadDirectory(m_strDirectory);
              }
            }
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
          pControl.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          GUIFacadeControl pControl2=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
          pControl2.OnMessage(message);
          break;
			}
			return base.OnMessage(message);
		}


		bool ViewByIcon
		{
			get 
			{
        if (_MapSettings.ViewAs != (int)View.VIEW_AS_LIST) return true;
				return false;
			}
		}

		bool ViewByLargeIcon
		{
			get
			{
        if (_MapSettings.ViewAs == (int)View.VIEW_AS_LARGEICONS) return true;
				return false;
			}
		}

		GUIListItem GetSelectedItem()
		{
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,(int)Controls.CONTROL_VIEW);
			return item;
		}

		GUIListItem GetItem(int iItem)
    {
      GUIListItem item = GUIControl.GetListItem(GetID,(int)Controls.CONTROL_VIEW,iItem);
      return item;
		}

		int GetSelectedItemNo()
    {

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.CONTROL_VIEW,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
		}
		int GetItemCount()
    {
      return GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_VIEW);
		}

		void UpdateButtons()
		{
      int iSelect = 0;
      switch (VideoState.StartWindow)
      {
        case(int)GUIWindow.Window.WINDOW_VIDEOS : 
          iSelect = 0;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_GENRE : 
          iSelect = 1;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_ACTOR : 
          iSelect = 2;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_YEAR : 
          iSelect = 3;
          break;
        case(int)GUIWindow.Window.WINDOW_VIDEO_TITLE : 
          iSelect = 4;
          break;
      }
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_BTNTYPE, iSelect);

      
			string strLine = "";
      View view = (View)_MapSettings.ViewAs;
      SortMethod method = (SortMethod )_MapSettings.SortBy;
      bool bAsc = _MapSettings.SortAscending;
			switch (view)
			{
				case View.VIEW_AS_LIST : 
					strLine = GUILocalizeStrings.Get(101);
					break;
				case View.VIEW_AS_ICONS : 
					strLine = GUILocalizeStrings.Get(100);
					break;
				case View.VIEW_AS_LARGEICONS : 
					strLine = GUILocalizeStrings.Get(417);
          break;
        case View.VIEW_AS_FILMSTRIP:
          strLine=GUILocalizeStrings.Get(733);
        break;
			}
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEWASICONS, strLine);

			switch (method)
			{
				case SortMethod.SORT_NAME : 
					strLine = GUILocalizeStrings.Get(103);
					break;
				case SortMethod.SORT_DATE : 
					strLine = GUILocalizeStrings.Get(104);
					break;
				case SortMethod.SORT_SIZE : 
					strLine = GUILocalizeStrings.Get(105);
					break;
			}
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSORTBY, strLine);

			if (bAsc)
				GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
			else
				GUIControl.SelectControl(GetID, (int)Controls.CONTROL_BTNSORTASC);

      if (_MapSettings.Stack)
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STACK, GUILocalizeStrings.Get(347));
      }
      else
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STACK, GUILocalizeStrings.Get(346));
      }
		}

		void ShowThumbPanel()
		{
			int iItem = GetSelectedItemNo();
      GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
      if ( _MapSettings.ViewAs== (int)View.VIEW_AS_LARGEICONS )
      {
        pControl.View=GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_ICONS)
      {
        pControl.View=GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_LIST)
      {
        pControl.View=GUIFacadeControl.ViewMode.List;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_FILMSTRIP)
      {
        pControl.View=GUIFacadeControl.ViewMode.Filmstrip;
      }

			if (iItem > -1)
			{
				GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iItem);
			}
			UpdateButtons();
		}

    void LoadFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      object o;
      FolderSettings.GetFolderSetting(strDirectory,"VideoFiles",typeof(GUIVideoFiles.MapSettings), out o);
      if (o!=null) _MapSettings = o as MapSettings;
      if (_MapSettings==null) _MapSettings  = new MapSettings();
    }
    void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      FolderSettings.AddFolderSetting(strDirectory,"VideoFiles",typeof(GUIVideoFiles.MapSettings), _MapSettings);
    }

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
      if (strNewDirectory != m_strDirectory && _MapSettings!=null) 
      {
        SaveFolderSettings(m_strDirectory);
      }

      if (strNewDirectory != m_strDirectory || _MapSettings==null) 
      {
        LoadFolderSettings(strNewDirectory);
      }

      m_strDirectory = strNewDirectory;

			string strObjects = "";

      ArrayList itemlist;

      // Mounting and loading a DVD image file takes a long time,
      // so display a message letting the user know that something 
      // is happening.
      if (!m_askBeforePlayingDVDImage && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(m_strDirectory)))
      {
        GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
        if (dlgProgress != null)
        {
          dlgProgress.SetHeading(13013);
          dlgProgress.SetLine(1, System.IO.Path.GetFileNameWithoutExtension(m_strDirectory));
          dlgProgress.StartModal(GetID);
          dlgProgress.Progress();
        }

        itemlist = m_directory.GetDirectory(m_strDirectory);
        
        if (dlgProgress!=null) dlgProgress.Close();

        // Remember the directory that the image file is in rather than the
        // image file itself.  This prevents repeated playing of the image file.
        if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(m_strDirectory)))
        {
          m_strDirectory = System.IO.Path.GetDirectoryName(m_strDirectory);
        }
      }
      else
      {
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_VIEW);
            
        itemlist = m_directory.GetDirectory(m_strDirectory);
      }

      if (_MapSettings.Stack)
      {
        ArrayList itemfiltered = new ArrayList();
        for (int x = 0; x < itemlist.Count; ++x)
        {
          bool bAdd = true;
          GUIListItem item1 = (GUIListItem)itemlist[x];
          for (int y = 0; y < itemlist.Count; ++y)
          {
            GUIListItem item2 = (GUIListItem)itemlist[y];
            if (x != y)
            {
              if (!item1.IsFolder || !item2.IsFolder)
              {
                if (Utils.ShouldStack(item1.Path, item2.Path))
                {
                  if (String.Compare(item1.Path, item2.Path, true) > 0)
                  {
                    bAdd = false;
                  }
                }
              }
            }
          }
          if (bAdd)
          {
            string strLabel = item1.Label;
            Utils.RemoveStackEndings(ref strLabel);
            item1.Label = strLabel;
            itemfiltered.Add(item1);
          }
        }
        itemlist = itemfiltered;
      }
      
      SetIMDBThumbs(itemlist);
			string strSelectedItem = m_history.Get(m_strDirectory);
			int iItem = 0;
			GUIFacadeControl list = (GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
			foreach (GUIListItem item in itemlist)
      {
        item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
				list.Add(item);
			}
			OnSort();
			for (int i = 0; i < GetItemCount(); ++i)
			{
				GUIListItem item = GetItem(i);
				if (item.Label == strSelectedItem)
				{
					GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iItem);
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
			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELFILES, strObjects);
      ShowThumbPanel();
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, m_iItemSelected);
      }

		}
		#endregion

		#region Sort Members
		void OnSort()
		{
			GUIFacadeControl list = (GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
			list.Sort(this);

			UpdateButtons();
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

			string strSize1 = "";
			string strSize2 = "";
			if (item1.FileInfo != null) strSize1 = Utils.GetSize(item1.FileInfo.Length);
			if (item2.FileInfo != null) strSize2 = Utils.GetSize(item2.FileInfo.Length);

      SortMethod method = (SortMethod)_MapSettings.SortBy;
      bool bAsc = _MapSettings.SortAscending;

			switch (method)
			{
				case SortMethod.SORT_NAME : 
					item1.Label2 = strSize1;
					item2.Label2 = strSize2;

					if (bAsc)
					{
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						return String.Compare(item2.Label, item1.Label, true);
					}
        

				case SortMethod.SORT_DATE : 
					if (item1.FileInfo == null) return - 1;
					if (item2.FileInfo == null) return - 1;
          
					item1.Label2 = item1.FileInfo.CreationTime.ToShortDateString() + " " + item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					item2.Label2 = item2.FileInfo.CreationTime.ToShortDateString() + " " + item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					if (bAsc)
					{
						return DateTime.Compare(item1.FileInfo.CreationTime, item2.FileInfo.CreationTime);
					}
					else
					{
						return DateTime.Compare(item2.FileInfo.CreationTime, item1.FileInfo.CreationTime);
					}

				case SortMethod.SORT_SIZE : 
					if (item1.FileInfo == null) return - 1;
					if (item2.FileInfo == null) return - 1;
					item1.Label2 = strSize1;
					item2.Label2 = strSize2;
					if (bAsc)
					{
						return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
					}
					else
					{
						return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
					}
			} 
			return 0;
		}
		#endregion

		
		void OnClick(int iItem)
		{
			GUIListItem item = GetSelectedItem();
			if (item == null) return;
			//++ Mars Warrior @ 03-sep-2004
			// This guy has his movies in a separate folder and has also DVD's on Hard disk.
			// The default behaviour of MP is to browse folders, thereby ignoring:
			// - DVD structure
			// - Single movie in a directory structure (my choice ;-))
			//
			// This modification checks for this:
			// - If DVD is found, play the DVD instead of browsing the (DVD) directory
			// - If the directory contains a single movie, play the movie, instead of browsing the directory
			//
			// Mars Warrior @ 11-sep-2004:
			// - The play single movie option is removed again, since it gave to many problems and even
			//	 blocked certain functions of MP ;-(
			//	 In the (near) future it will be possible to play a directory, which in fact results in
			//	 exactly the same behaviour for a single movie: the movie is played.
			//
			//	 A nice add-on would be to display (just as ACDSee) the image(s) in the folder image,
			//	 which makes it visible for the user how many movies reside in the subdirectory...

			bool bFolderIsMovie = false;

			if (item.IsFolder)
			{
				// Check if folder is actually a DVD. If so don't browse this folder, but play the DVD!
        if ((System.IO.File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO")) && (item.Label!=".."))
        {
          bFolderIsMovie = true;
          item.Path      = item.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        else 
        {
          bFolderIsMovie = false;

					/* Mars Warrior @ 11-sep-2004 -- REMOVED
          // Check if folder is a folder containig a single movie file. If so, (again), don't
          // browse the folder, but play the movie!
          ArrayList items = this.m_directory.GetDirectory(item.Path);
          int iVideoFilesCount = 0;
          string strVideoFile = "";
          for (int i = 0; i < items.Count; ++i)
          {
            GUIListItem pItemTmp = (GUIListItem)items[i];
            if (Utils.IsVideo(pItemTmp.Path) && !PlayListFactory.IsPlayList(pItemTmp.Path))
            {
              iVideoFilesCount++;
              if (iVideoFilesCount == 1) strVideoFile = pItemTmp.Path;
            }
          }
          if (iVideoFilesCount == 1)
          {
            bFolderIsMovie = true;
            item.Path      = strVideoFile;
          }
          else bFolderIsMovie = false;
					-- REMOVED */
        }
			}

			if ((item.IsFolder) && (!bFolderIsMovie))
			//-- Mars Warrior @ 03-sep-2004
			{
				m_iItemSelected = -1;
				LoadDirectory(item.Path);
			}
			else
			{
        if (m_directory.IsRemote(item.Path) )
        {
          if (!m_directory.IsRemoteFileDownloaded(item.Path,item.FileInfo.Length) )
          {
            if (!m_directory.ShouldWeDownloadFile(item.Path)) return;
            if (!m_directory.DownloadRemoteFile(item.Path,item.FileInfo.Length))
            {  

              //show message that we are unable to download the file
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,0,0,0,0,0,0);
              msg.Param1=916;
              msg.Param2=920;
              msg.Param3=0;
              msg.Param4=0;
              GUIWindowManager.SendMessage(msg);

              return;
            }
            else
            {
              
              //download subtitle files
              string[] sub_exts = {  ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss", ".ass", ".idx",".ifo" };
              // check if movie has subtitles
              for (int i = 0; i < sub_exts.Length; i++)
              {
                string strSubTitleFile = item.Path;
                strSubTitleFile = System.IO.Path.ChangeExtension(strSubTitleFile, sub_exts[i]);
                string localSubFile=m_directory.GetLocalFilename(strSubTitleFile);
                Utils.FileDelete(localSubFile);
                m_directory.DownloadRemoteFile(item.Path,0);
              }
            }
          }
        }
        
        if (item.FileInfo != null)
        {
          if (!m_directory.IsRemoteFileDownloaded(item.Path,item.FileInfo.Length) ) return;
        }
        string strFileName = item.Path;
        strFileName=m_directory.GetLocalFilename(strFileName);

				// Set selected item
				m_iItemSelected = GetSelectedItemNo();
				if (PlayListFactory.IsPlayList(strFileName))
				{
					LoadPlayList(strFileName);
					return;
				}
				
				
        if (!CheckMovie(strFileName)) return;
        bool bAskResume=true;        
				if (_MapSettings.Stack)
				{
          int iSelectedFile = 0;
          int iDuration = 0;
					ArrayList movies = new ArrayList();
					{
            //get all movies belonging to each other
						ArrayList items = m_directory.GetDirectory(m_strDirectory);

            //check if we can resume 1 of those movies
            int stoptime=0;
            bool asked=false;
            ArrayList newItems = new ArrayList();
            for (int i = 0; i < items.Count; ++i)
            {
              GUIListItem pItemTmp = (GUIListItem)items[i];
              if (Utils.ShouldStack(pItemTmp.Path, item.Path))
              {   
                if (!asked) iSelectedFile++;                
                IMDBMovie movieDetails = new IMDBMovie();
                int fileid=VideoDatabase.GetFileId(pItemTmp.Path);
                int movieid=VideoDatabase.GetMovieId(item.Path);
                if ( (movieid >= 0) && (fileid >= 0) )
                {                 
									VideoDatabase.GetMovieInfo(item.Path, ref movieDetails);
                  string title=System.IO.Path.GetFileName(item.Path);
                  Utils.RemoveStackEndings(ref title);
                  if (movieDetails.Title!=String.Empty) title=movieDetails.Title;
                  
                  stoptime=VideoDatabase.GetMovieStopTime(fileid);
                  if (stoptime>0)
                  {
                    if (!asked)
                    {
                      asked=true;
                      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                      if (null == dlgYesNo) return;
                      dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                      dlgYesNo.SetLine(1, title);
                      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936)+" "+Utils.SecondsToHMSString(iDuration + stoptime));
                      dlgYesNo.SetDefaultToYes(true);
                      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                      if (dlgYesNo.IsConfirmed) 
                      {
                        bAskResume=false;
                        newItems.Add(pItemTmp);
                      }
                      else
                      {
                        VideoDatabase.DeleteMovieStopTime(fileid);
                        newItems.Add(pItemTmp);
                      }
                    } //if (!asked)
                    else
                    {
                      newItems.Add(pItemTmp);
                    }
                  } //if (stoptime>0)
                  else
                  {
                    newItems.Add(pItemTmp);
                  }

                  // Total movie duration
                  iDuration += VideoDatabase.GetMovieDuration(fileid);
                } 
                else //if (movieid >=0)
                {
                  newItems.Add(pItemTmp);
                }
              }//if (Utils.ShouldStack(pItemTmp.Path, item.Path))
            }

            for (int i = 0; i < newItems.Count; ++i)
						{
							GUIListItem pItemTmp = (GUIListItem)newItems[i];
							if (Utils.IsVideo(pItemTmp.Path) && !PlayListFactory.IsPlayList(pItemTmp.Path))
							{
                if (Utils.ShouldStack(pItemTmp.Path, item.Path))
                {
                  movies.Add(pItemTmp.Path);
                }
							}
						}
            if (movies.Count == 0)
            {
              movies.Add(strFileName);
            }
					}
					if (movies.Count <= 0) return;				
					if (movies.Count > 1)
					{
						//TODO
						movies.Sort();
						for (int i = 0; i < movies.Count; ++i)
						{
							AddFileToDatabase((string)movies[i]);
						}

            if (bAskResume)
            {
              GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
              if (null != dlg)
              {
                dlg.SetNumberOfFiles(movies.Count);
                dlg.DoModal(GetID);
                iSelectedFile = dlg.SelectedFile;
                if (iSelectedFile < 1) return;
              }
            }
					}
					else if (movies.Count==1)
					{
						AddFileToDatabase((string)movies[0]);
					}
					PlayListPlayer.Reset();
					PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP;
					PlayList playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP);
					playlist.Clear();
					for (int i = 0; i < (int)movies.Count; ++i)
					{
						strFileName = (string)movies[i];
						PlayList.PlayListItem itemNew = new PlayList.PlayListItem();
						itemNew.FileName = strFileName;
            itemNew.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
						playlist.Add(itemNew);
					}

					// play movie...
					PlayMovieFromPlayList(bAskResume, iSelectedFile-1);
					return;
				}

				// play movie...
				AddFileToDatabase(strFileName);

        PlayListPlayer.Reset();
        PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP;
        PlayList newPlayList = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP);
        newPlayList.Clear();
        PlayList.PlayListItem NewItem = new PlayList.PlayListItem();
        NewItem.FileName = strFileName;
        NewItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
        newPlayList.Add(NewItem);
        PlayMovieFromPlayList(true);
/*
				//TODO
        if (g_Player.Play(strFileName))
        {
          if (Utils.IsVideo(strFileName))
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
        }*/
			}
		}
    
		void OnQueueItem(int iItem)
		{
			// add item 2 playlist
			GUIListItem pItem = GetItem(iItem);
      
      if (pItem==null) return;
      if (pItem.IsRemote) return;
      if (PlayListFactory.IsPlayList(pItem.Path))
      {
        LoadPlayList(pItem.Path);
        return;
      }

			AddItemToPlayList(pItem);
	
			//move to next item
			GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iItem + 1);

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
				if (Utils.IsVideo(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
				{
					PlayList.PlayListItem playlistItem = new PlayList.PlayListItem();
					playlistItem.FileName = pItem.Path;
					playlistItem.Description = pItem.Label;
					playlistItem.Duration = pItem.Duration;
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Video;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Add(playlistItem);
				}
			}
		}

		void LoadPlayList(string strPlayList)
		{
			PlayList playlist = PlayListFactory.Create(strPlayList);
			if (playlist == null) return;
			if (!playlist.Load(strPlayList))
			{
        GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dlgOK != null)
        {
				  dlgOK.SetHeading(6);
				  dlgOK.SetLine(1, 477);
				  dlgOK.SetLine(2, "");
				  dlgOK.DoModal(GetID);
        }
				return;
			}

			if (playlist.Count == 1)
			{
				//TODO
				Log.Write("GUIVideoFiles play:{0}",playlist[0].FileName);
        if (g_Player.Play(playlist[0].FileName))
        {
          if (Utils.IsVideo(playlist[0].FileName))
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
        }
				return;
			}

			// clear current playlist
			PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Clear();

			// add each item of the playlist to the playlistplayer
			for (int i = 0; i < playlist.Count; ++i)
			{
				PlayList.PlayListItem playListItem = playlist[i];
				PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Add(playListItem);
			}

			
			// if we got a playlist
			if (PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO).Count > 0)
			{
				// then get 1st song
				playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_VIDEO);
				PlayList.PlayListItem item = playlist[0];

				// and start playing it
				PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_VIDEO;
				PlayListPlayer.Reset();
				PlayListPlayer.Play(0);

				// and activate the playlist window if its not activated yet
				if (GetID == GUIWindowManager.ActiveWindow)
				{
					GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
				}
			}
		}

		void AddFileToDatabase(string strFile)
		{
      if (!Utils.IsVideo(strFile)) return;
      //if ( Utils.IsNFO(strFile)) return;
      if (PlayListFactory.IsPlayList(strFile)) return;

      if (!VideoDatabase.HasMovieInfo(strFile))
      {
        // set initial movie info
      VideoDatabase.AddMovieFile(strFile);
  
        IMDBMovie movieDetails = new IMDBMovie();             
        int iMovieId=VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
        if (iMovieId>=0)
        {
          if (Utils.IsDVD(strFile))
          {
            //DVD
            movieDetails.DVDLabel = Utils.GetDriveName(System.IO.Path.GetPathRoot(strFile));
            movieDetails.Title = movieDetails.DVDLabel;
          }
          else if (strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >=0)
          {
            //DVD folder
            strFile = strFile.Substring(0,strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
            movieDetails.DVDLabel = System.IO.Path.GetFileName(strFile);
            movieDetails.Title = movieDetails.DVDLabel;
          }
          else
          {
            //Movie 
            movieDetails.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);
          }
          VideoDatabase.SetMovieInfoById(iMovieId, ref movieDetails);
        }
      }
		}

		bool OnScan(ArrayList items)
		{
		  // remove username + password from m_strDirectory for display in Dialog

      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(189);
        dlgProgress.SetLine(1, "");
        dlgProgress.SetLine(2, m_strDirectory);
        dlgProgress.StartModal(GetID);
      }

      OnRetrieveVideoInfo(items);
      if (dlgProgress != null)
      {
        dlgProgress.SetLine(2, m_strDirectory);
        if (dlgProgress.IsCanceled) return false;
      }
	
      bool bCancel = false;
      for (int i = 0; i < items.Count; ++i)
      {
        GUIListItem pItem = (GUIListItem)items[i];
        if (dlgProgress != null)
        {
          if (dlgProgress.IsCanceled) 
          {
            bCancel = true;
            break;
          }
        }
        if (pItem.IsFolder)
        {
          if (pItem.Label != "..")
          {
            // load subfolder
            string strDir = m_strDirectory;
            m_strDirectory = pItem.Path;

            bool FolderIsDVD=false;
						// Mars Warrior @ 03-sep-2004
						// Check for single movie in directory and make sure (just as with DVDs) that
						// the folder gets the nice folder.jpg image ;-)
						bool bFolderIsMovie = false;

						ArrayList subDirItems = m_directory.GetDirectory(pItem.Path);
            foreach (GUIListItem item in subDirItems)
            {
              if (item.Label.ToLower().Equals("video_ts"))
              {
                FolderIsDVD=true;
                break;
              }
            }

						// Check if folder is a folder containig a single movie file. If so, (again), don't
						// browse the folder, but play the movie!
						int iVideoFilesCount = 0;
						string strVideoFile = "";
						foreach (GUIListItem item in subDirItems)
						{
							if (Utils.IsVideo(item.Path) && !PlayListFactory.IsPlayList(item.Path))
							{
								iVideoFilesCount++;
								if (iVideoFilesCount == 1) strVideoFile = item.Path;
							}
						}
						if (iVideoFilesCount == 1)
						{
							bFolderIsMovie = true;
						}
						else bFolderIsMovie = false;

						if ((!FolderIsDVD) && (!bFolderIsMovie))
						{
							if (!OnScan(subDirItems))
							{
								bCancel = true;
							}
						}
						else if (FolderIsDVD)
						{
							string strFilePath=String.Format(@"{0}\VIDEO_TS\VIDEO_TS.IFO",pItem.Path);
							OnRetrieveVideoInfo(strFilePath, pItem.Label, pItem.Path);
						}
						else if (bFolderIsMovie)
						{
							OnRetrieveVideoInfo(strVideoFile, pItem.Label, pItem.Path);
						}
						//-- Mars Warrior
				
            m_strDirectory = strDir;
            if (bCancel) break;
          }
        }
      }
	
      if (dlgProgress != null) dlgProgress.Close();
      return!bCancel;
		}

    /// <summary>
    /// Searches IMDB for a movie and if found gets the details about the 1st movie found
    /// details are put in the video database under the file mentioned by strFileName
    /// also a thumbnail is downloaded to thumbs\ and
    /// if strPath is filled in a srtpath\folder.jpg is created
    /// </summary>
    /// <param name="strFileName">path+filename to which this imdb info will belong</param>
    /// <param name="strMovieName">IMDB search string</param>
    /// <param name="strPath">path where folder.jpg should be created</param>
    void OnRetrieveVideoInfo(string strFileName, string strMovieName, string strPath)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      AddFileToDatabase(strFileName);
      if (!VideoDatabase.HasMovieInfo(strFileName))
      {
        // do IMDB lookup...
        if (dlgProgress != null)
        {
          dlgProgress.SetHeading(197);
          dlgProgress.SetLine(1, strMovieName);
          dlgProgress.SetLine(2, "");
          dlgProgress.SetLine(3, "");
          dlgProgress.Progress();
          if (dlgProgress.IsCanceled) return;
        }


        IMDB imdb = new IMDB();
        imdb.Find(strMovieName);
              
        int iMoviesFound = imdb.Count;
        if (iMoviesFound > 0)
        {
          IMDBMovie movieDetails = new IMDBMovie();
          movieDetails.SearchString = strMovieName;
          IMDB.IMDBUrl url = imdb[0];

          // show dialog that we're downloading the movie info
          if (dlgProgress != null)
          {
            dlgProgress.SetHeading(198);
            //dlgProgress.SetLine(0, strMovieName);
            dlgProgress.SetLine(1, url.Title);
            dlgProgress.SetLine(2, "");
            dlgProgress.Progress();
            if (dlgProgress.IsCanceled) return;
          }

          if (imdb.GetDetails(url, ref movieDetails))
          {
            // get & save thumbnail
            VideoDatabase.SetMovieInfo(strFileName, ref movieDetails);
            string strThumb = "";
            string strImage = movieDetails.ThumbURL;
            if (strImage.Length > 0 && movieDetails.ThumbURL.Length > 0)
            { 
              string LargeThumb=Utils.GetLargeCoverArtName(ThumbsFolder,movieDetails.Title);
              strThumb = Utils.GetCoverArtName(ThumbsFolder,movieDetails.Title);
              Utils.FileDelete(strThumb);
              Utils.FileDelete(LargeThumb);
              
              string strExtension = System.IO.Path.GetExtension(strImage);
              if (strExtension.Length > 0)
              {
                string strTemp = "temp" + strExtension;
                Utils.FileDelete(strTemp);
                if (dlgProgress != null)
                {
                  dlgProgress.SetLine(2, 415);
                  dlgProgress.Progress();
                  if (dlgProgress.IsCanceled) return;
                }
                      
                Utils.DownLoadImage(strImage, strTemp);
                if (System.IO.File.Exists(strTemp))
                {
                  MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
                  MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);
                  if (strPath.Length > 0)
                  {
                    try
                    {
                      Utils.FileDelete(strPath+@"\folder.jpg");
                      System.IO.File.Copy(strThumb, strPath+@"\folder.jpg");
                    }
                    catch(Exception){ }
                  }
                }
                Utils.FileDelete(strTemp);
              }//if ( strExtension.Length>0)
              else
              {
                Log.Write("image has no extension:{0}", strImage);
              }
            }
          }
        }
      }
    }
    
    /// <summary>
    /// Retrieves the imdb info for an array of items.
    /// </summary>
    /// <param name="items"></param>
    void OnRetrieveVideoInfo(ArrayList items)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      // for every file found
      for (int i = 0; i < items.Count; ++i)
	    {
        GUIListItem pItem = (GUIListItem)items[i];
		  if (!pItem.IsFolder 
			  || ( pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
        {
          if (Utils.IsVideo(pItem.Path) && /*!Utils.IsNFO(pItem.Path) && */!PlayListFactory.IsPlayList(pItem.Path))
          {
            string strItem = String.Format("{0}/{1}", i + 1, items.Count);
            if (dlgProgress != null)
            {
              dlgProgress.SetLine(1, strItem);
              dlgProgress.SetLine(2, System.IO.Path.GetFileName(pItem.Path));
              dlgProgress.Progress();
              if (dlgProgress.IsCanceled) return;
            }
            string strMovieName = System.IO.Path.GetFileName(pItem.Path);
            OnRetrieveVideoInfo(pItem.Path, strMovieName,"");
          }
        }
      }
    }
		
    void OnInfo(int iItem)
		{
      m_iItemSelected = GetSelectedItemNo();
      GUIDialogSelect dlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      bool bFolder = false;
      string strFolder = "";
      int iSelectedItem = GetSelectedItemNo();
      GUIListItem pItem = GetSelectedItem();
      if (pItem==null) return;
      if (pItem.IsRemote) return;
      string strFile = pItem.Path;
      string strMovie = pItem.Label;
      // Use DVD label as movie name
      if (Utils.IsDVD(pItem.Path) && (pItem.DVDLabel != ""))
      {
        strMovie = pItem.DVDLabel;
      }
      if (pItem.IsFolder)
      {
        // IMDB is done on a folder, find first file in folder
        strFolder = pItem.Path;
        bFolder = true;

		  bool bFoundFile = false;

		  string strExtension=System.IO.Path.GetExtension(pItem.Path).ToLower();
		  if (VirtualDirectory.IsImageFile(strExtension))
		  {
			  strFile = pItem.Path;
			  bFoundFile = true;
		  } 
		  else
		  {
			  ArrayList vecitems = m_directory.GetDirectory(pItem.Path);
        for (int i = 0; i < vecitems.Count; ++i)
        {
          pItem = (GUIListItem)vecitems[i];
          if (!pItem.IsFolder 
          || ( pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
          {
						if (Utils.IsVideo(pItem.Path) && /*!Utils.IsNFO(pItem.Path) && */!PlayListFactory.IsPlayList(pItem.Path))
						{
							strFile = pItem.Path;
							bFoundFile = true;
							break;
						}
          }
          else
          {
						if (pItem.Path.ToLower().IndexOf("video_ts") >=0)
            {
              strFile = String.Format(@"{0}\VIDEO_TS.IFO",pItem.Path);
              bFoundFile = true;
              break;
            }
						
				  }
			  }
		  }
        if (!bFoundFile) 
        {
          // no Video file in this folder?
          // then just lookup IMDB info and show it
          if ( ShowIMDB(-1,strMovie, strFolder, strFolder, false))
            LoadDirectory(m_strDirectory);
          GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iSelectedItem);
          return;
        }
      }

      AddFileToDatabase(strFile);

 

      if ( ShowIMDB(-1,strMovie, strFile, strFolder, bFolder))
        LoadDirectory(m_strDirectory);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW, iSelectedItem);
		}

		
    static public bool ShowIMDB(int iMovieId)
    {
      ArrayList movies = new ArrayList();
      VideoDatabase.GetFiles(iMovieId, ref movies);
      if (movies.Count <= 0) return false;
      string strFilePath = (string)movies[0];
      string strFile = System.IO.Path.GetFileName(strFilePath);
      return ShowIMDB(iMovieId,strFile, strFilePath, "" ,false);

    }
    /// <summary>
    /// Download & shows IMDB info for a file
    /// </summary>
    /// <param name="strMovie">IMDB search criteria</param>
    /// <param name="strFile">path+file where imdb info should be saved for</param>
    /// <param name="strFolder">path where folder.jpg should be created (if bFolder==true)</param>
    /// <param name="bFolder">true create a folder.jpg, false dont create a folder.jpg</param>
    static public bool ShowIMDB(int iMovieId,string strMovie, string strFile, string strFolder, bool bFolder)
    {
      GUIDialogOK 		 		pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress 	pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIDialogSelect 		pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      GUIVideoInfo pDlgInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);
 
      IMDB								  imdb = new IMDB();
      bool									bUpdate = false;
      bool									bFound = false;
 
      if (null == pDlgOK) return false;
      if (null == pDlgProgress) return false;
      if (null == pDlgSelect) return false;
      if (null == pDlgInfo) return false;
      string strMovieName = System.IO.Path.GetFileNameWithoutExtension(strMovie);

      if (VideoDatabase.HasMovieInfo(strFile))
      {
        IMDBMovie movieDetails = new IMDBMovie();
        if (iMovieId>=0)
          VideoDatabase.GetMovieInfoById(iMovieId, ref movieDetails);
        else
          VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
        pDlgInfo.Movie = movieDetails;
        pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);
        if (!pDlgInfo.NeedsRefresh)
        { 
          if (bFolder && strFile != "")
          {
            // copy icon to folder also;
            string strThumbOrg = Utils.GetCoverArt(ThumbsFolder,movieDetails.Title);
            string strFolderImage = System.IO.Path.GetFullPath(strFolder);
            strFolderImage += "\\folder.jpg";
            if (System.IO.File.Exists(strThumbOrg) )
            {
              Utils.FileDelete(strFolderImage);
              try
              {
                Utils.FileDelete(strFolderImage);
                System.IO.File.Copy(strThumbOrg, strFolderImage);
              }
              catch (Exception)
              {
              }
            }
          }
          return true;
        }
        
        if (!Util.Win32API.IsConnectedToInternet()) return false;
        if (iMovieId>=0)
          VideoDatabase.DeleteMovieInfoById(iMovieId);
        else
          VideoDatabase.DeleteMovieInfo(strFile);
        GetStringFromKeyboard(ref strMovieName);
      }
      else
      {
        if (Utils.IsDVD(strFile))
        {
          GetStringFromKeyboard(ref strMovieName);
        }
      }
      
      if (!Util.Win32API.IsConnectedToInternet()) return false;

      bool bContinue = false;
      do
      {
        bContinue = false;
        if (!bFound)
        {
          // show dialog that we're busy querying www.imdb.com
          pDlgProgress.SetHeading(197);
          //pDlgProgress.SetLine(0, strMovieName);
          pDlgProgress.SetLine(1, strMovieName);
          pDlgProgress.SetLine(2, "");
          pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
          pDlgProgress.Progress();

          bool						bError = true;
  		    imdb.Find(strMovieName);
          if (imdb.Count > 0) 
          {
            pDlgProgress.Close();

            int iMoviesFound = imdb.Count;
            if (iMoviesFound > 0)
            {
              int iSelectedMovie = 0;
              if (iMoviesFound > 1)
              {
                // more then 1 movie found
                // ask user to select 1
                pDlgSelect.SetHeading(196);
                pDlgSelect.Reset();
                for (int i = 0; i < iMoviesFound; ++i)
                {
                  IMDB.IMDBUrl url = imdb[i];
                  pDlgSelect.Add(url.Title);
                }
                pDlgSelect.EnableButton(true);
                pDlgSelect.SetButtonLabel(413); // manual
                pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

                // and wait till user selects one
                iSelectedMovie = pDlgSelect.SelectedLabel;
                if (iSelectedMovie < 0)
                {
                  if (!pDlgSelect.IsButtonPressed) return false;
                  GetStringFromKeyboard(ref strMovieName);
                  if (strMovieName == "") return false;
                  bContinue = true;
                  bError = false;
                }
              }
          
              if (iSelectedMovie >= 0)
              {
                IMDBMovie movieDetails = new IMDBMovie();
                movieDetails.SearchString = strFile;
                IMDB.IMDBUrl url = imdb[iSelectedMovie];
    				
                // show dialog that we're downloading the movie info
                pDlgProgress.SetHeading(198);
                //pDlgProgress.SetLine(0, strMovieName);
                pDlgProgress.SetLine(1, url.Title);
                pDlgProgress.SetLine(2, "");
                pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
                pDlgProgress.Progress();
                if (imdb.GetDetails(url, ref movieDetails))
                {
                  // got all movie details :-)
                  pDlgProgress.Close();
                  bError = false;

                  // now show the imdb info
                  if (iMovieId>=0)
                    VideoDatabase.SetMovieInfoById(iMovieId, ref movieDetails);
                  else
                    VideoDatabase.SetMovieInfo(strFile, ref movieDetails);
                  pDlgInfo.Movie = movieDetails;
                  pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);
                  if (!pDlgInfo.NeedsRefresh)
                  { 
                    bUpdate = true;
                    if (bFolder && strFile != "")
                    {
                      // copy icon to folder also;
                      string strThumbOrg = Utils.GetCoverArt(ThumbsFolder,movieDetails.Title);
                      if (System.IO.File.Exists(strThumbOrg))
                      {
                        string strFolderImage = System.IO.Path.GetFullPath(strFolder);
                        strFolderImage += "\\folder.jpg"; //TODO                  
                        try
                        {
                          Utils.FileDelete(strFolderImage);
                          System.IO.File.Copy(strThumbOrg, strFolderImage);
                        }
                        catch (Exception)
                        {
                        }

                      }
                    }
                  }
                  else
                  {
                    bContinue = true;
                    strMovieName = System.IO.Path.GetFileNameWithoutExtension(strMovie);
                    GetStringFromKeyboard(ref strMovieName);
                  }
                }
                else
                {
                  pDlgProgress.Close();
                }
              }
            }
            else
            {
              pDlgProgress.Close();
              GetStringFromKeyboard(ref strMovieName);
              if (strMovieName == "") return false;
              bContinue = true;
              bError = false;
            }
          }
          else
          {
            pDlgProgress.Close();
            GetStringFromKeyboard(ref strMovieName);
            if (strMovieName == "") return false;
            bContinue = true;
            bError = false;
          }

          if (bError)
          {
            // show dialog...
            pDlgOK.SetHeading(195);
            pDlgOK.SetLine(1, strMovieName);
            pDlgOK.SetLine(2, "");
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          }
        }
      } while (bContinue);

      if (bUpdate)
      {
        return true;
      }
      return false;
    }

    void SetIMDBThumbs(ArrayList items)
    {
      GUIListItem pItem ;
      ArrayList movies = new ArrayList();
			
      for (int x = 0; x < items.Count; ++x)
      {
        pItem = (GUIListItem)items[x];
        if (pItem.IsFolder)
				{
					if (pItem.ThumbnailImage!=String.Empty) continue;
          if (System.IO.File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            movies.Clear();
            string file= pItem.Path + @"\VIDEO_TS";
            VideoDatabase.GetMoviesByPath(file, ref movies);
            for (int i = 0; i < movies.Count; ++i)
            {
              IMDBMovie info = (IMDBMovie)movies[i];
              string strFile = "VIDEO_TS.IFO";
              if (info.File[0] == '\\' || info.File[0] == '/')
                info.File = info.File.Substring(1);

              if (strFile.Length > 0)
              {
                if (info.File == strFile /*|| pItem->GetLabel() == info.Title*/)
                {
                  string strThumb;
                  if (Utils.IsDVD(pItem.Path))
                    pItem.Label=String.Format( "({0}:) {1}",  pItem.Path.Substring(0,1),  info.Title );
                  strThumb = Utils.GetCoverArt(ThumbsFolder, info.Title );
                  if (System.IO.File.Exists(strThumb))
                  {
                    pItem.ThumbnailImage = strThumb;
                    pItem.IconImageBig = strThumb;
                    pItem.IconImage = strThumb;
                  }
                  break;
                }
              }
            } // of for (int i = 0; i < movies.Count; ++i)
          } // of if (System.IO.File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        } // of if (pItem.IsFolder)
      } // of for (int x = 0; x < items.Count; ++x)

      movies.Clear();
      VideoDatabase.GetMoviesByPath(m_strDirectory, ref movies);
      for (int x = 0; x < items.Count; ++x)
      {
        pItem = (GUIListItem)items[x];
        if (!pItem.IsFolder 
        || ( pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
        {
					if (pItem.ThumbnailImage!=String.Empty) continue;
          for (int i = 0; i < movies.Count; ++i)
          {
            IMDBMovie info = (IMDBMovie)movies[i];
            string strFile = System.IO.Path.GetFileName(pItem.Path);
            if (info.File[0] == '\\' || info.File[0] == '/')
              info.File = info.File.Substring(1);

            if (strFile.Length > 0)
            {
              if (info.File == strFile /*|| pItem->GetLabel() == info.Title*/)
              {
                string strThumb;
                strThumb = Utils.GetCoverArt(ThumbsFolder, info.Title );
                if (System.IO.File.Exists(strThumb))
                {
                  pItem.ThumbnailImage = strThumb;
                  pItem.IconImageBig = strThumb;
                  pItem.IconImage = strThumb;
                }
                break;
              }
            }
          } // of for (int i = 0; i < movies.Count; ++i)
        } // of if (!pItem.IsFolder)
      } // of for (int x = 0; x < items.Count; ++x)
    }

    public bool CheckMovie(string strFileName)
    {
      if (!VideoDatabase.HasMovieInfo(strFileName)) return true;
     
      IMDBMovie movieDetails = new IMDBMovie();
      int movieid=VideoDatabase.GetMovieInfo(strFileName, ref movieDetails);
      if (movieid < 0) return true;
      return CheckMovie(movieid);
    }

    static public bool CheckMovie(int movieid)
    {
      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfoById(movieid, ref movieDetails);

      if (!Utils.IsDVD(movieDetails.Path)) return true;
      string cdlabel="";
      cdlabel=Utils.GetDriveSerial(movieDetails.Path);
      if (cdlabel.Equals(movieDetails.CDLabel)) return true;

      GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlg == null) return true;
      while (true)
      {
 	      dlg.SetHeading(428);
	      dlg.SetLine(1, 429);
        dlg.SetLine(2, movieDetails.DVDLabel);
	      dlg.SetLine(3, movieDetails.Title);
	      dlg.DoModal(GUIWindowManager.ActiveWindow);
        if (dlg.IsConfirmed) 
        {
          cdlabel=Utils.GetDriveSerial(movieDetails.Path);
          if (cdlabel.Equals(movieDetails.CDLabel)) return true;
        }
        else break;
      }
      return false;
		}
	
    void OnManualIMDB()
		{
      m_iItemSelected = GetSelectedItemNo();
      string strInput = "";
      GetStringFromKeyboard(ref strInput);
      if (strInput == "") return;

      //string strThumb;
//      CUtil::GetThumbnail("Z:\\",strThumb);
//      ::DeleteFile(strThumb.c_str());

      if (ShowIMDB(-1,strInput, "", "", false))
        LoadDirectory(m_strDirectory);
      return;
    }

    static public void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return;
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GUIWindowManager.ActiveWindow);
			strLine = "";
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }
	
		void OnPlayDVD()
		{
			Log.Write("GUIVideoFiles playDVD");
      g_Player.PlayDVD();
		}
		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}


    public bool HasSetup()
    {
      return false;
    }
		public string PluginName()
		{
			return "My Videos";
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
			strButtonText = GUILocalizeStrings.Get(3);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string Description()
		{
			return "Plugin to watch and organize your videos";
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIVideoFiles.ShowPlugin implementation
		}

		#endregion

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
      if (filmstrip==null) return;
    
      if (item.IsFolder) filmstrip.InfoImageFileName=item.ThumbnailImage;
      else filmstrip.InfoImageFileName=Utils.ConvertToLargeCoverArt(item.ThumbnailImage);
    }

    static public void PlayMovieFromPlayList(bool bAskResume)
    {
      PlayMovieFromPlayList(bAskResume, -1);
    }
    static public void PlayMovieFromPlayList(bool bAskResume, int iMovieIndex)
    {
      string filename;
      if (iMovieIndex == -1)
        filename=PlayListPlayer.GetNext();
      else
        filename=PlayListPlayer.Get(iMovieIndex);

      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfo(filename, ref movieDetails);
      int fileid=VideoDatabase.GetFileId(filename);
      int movieid=VideoDatabase.GetMovieId(filename);
      int stoptime=0;
      if ( (movieid >= 0) && (fileid >= 0) )
      {        
        stoptime=VideoDatabase.GetMovieStopTime(fileid);
        if (stoptime>0)
        {
          string title=System.IO.Path.GetFileName(filename);
          VideoDatabase.GetMovieInfoById( movieid, ref movieDetails);
          if (movieDetails.Title!=String.Empty) title=movieDetails.Title;
          
          if (bAskResume)
          {
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
            dlgYesNo.SetLine(1, title);
            dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936)+Utils.SecondsToHMSString(stoptime) );
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
            
            if (!dlgYesNo.IsConfirmed) stoptime=0;
          }
        }
      }
      
      if (iMovieIndex==-1)
      {
        PlayListPlayer.PlayNext(true);
      }
      else
      {
        PlayListPlayer.Play(iMovieIndex);
      }

      if (g_Player.Playing && stoptime > 0)
      {
        g_Player.SeekAbsolute(stoptime);
      }
    }

    private void OnPlayBackStopped(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
    {
      if (type!=g_Player.MediaType.Video) return;

      // Handle all movie files from MovieId
      ArrayList movies = new ArrayList();
      int iMovieId=VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFiles(iMovieId, ref movies);
      if (movies.Count <= 0) return;
      for (int i=0 ; i<movies.Count ; i++)
      {
        string strFilePath = (string)movies[i];
        int fileid=VideoDatabase.GetFileId(strFilePath);
        if (fileid<0) break;
        if ( (filename == strFilePath) && (stoptime > 0) )
          VideoDatabase.SetMovieStopTime(fileid,stoptime);
        else
          VideoDatabase.DeleteMovieStopTime(fileid);
      }
    }

    private void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
			if (type!=g_Player.MediaType.Video) return;

      // Handle all movie files from MovieId
      ArrayList movies = new ArrayList();
      int iMovieId=VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFiles(iMovieId, ref movies);
      for (int i=0 ; i<movies.Count ; i++)
      {
        string strFilePath = (string)movies[i];
        int fileid=VideoDatabase.GetFileId(strFilePath);
        if (fileid<0) break;
        VideoDatabase.DeleteMovieStopTime(fileid);
      }
    }
    private void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type!=g_Player.MediaType.Video) return;
      AddFileToDatabase(filename);

      int fileid=VideoDatabase.GetFileId(filename);
      if (fileid!=-1)
      {
        int iDuration=(int)g_Player.Duration;
        VideoDatabase.SetMovieDuration(fileid, iDuration);
      }
    }

		void ShowContextMenu()
		{
			GUIListItem item=GetSelectedItem();
			int itemNo=GetSelectedItemNo();
			if (item==null) return;

      GUIControl cntl=GetControl((int)Controls.CONTROL_VIEW);
      if (cntl==null) return; // Control not found

			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu

			if (!cntl.Focus)
			{
				// Menu button context menuu
				dlg.AddLocalizedString(368); //IMDB
				if (!m_directory.IsRemote(m_strDirectory)) dlg.AddLocalizedString(102); //Scan
				if (!_MapSettings.Stack) dlg.AddLocalizedString(346); //Stack
				else dlg.AddLocalizedString(347); //Unstack
				dlg.AddLocalizedString(654); //Eject
			}
			else
			{
				if ((System.IO.Path.GetFileName(item.Path) != "") || Utils.IsDVD(item.Path))
				{
					if (Utils.getDriveType(item.Path) != 5) dlg.AddLocalizedString(925); //delete
					dlg.AddLocalizedString(368); //IMDB
					dlg.AddLocalizedString(208); //play
					dlg.AddLocalizedString(926); //Queue
				}
				if (Utils.getDriveType(item.Path) == 5) dlg.AddLocalizedString(654); //Eject
			}

			dlg.DoModal( GetID);
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 925: // Delete
					OnDeleteItem(item);
					break;

				case 368: // IMDB
					OnInfo(itemNo);
					break;

				case 208: // play
					OnClick(itemNo);	
					break;
					
				case 926: // add to playlist
					OnQueueItem(itemNo);	
					break;
					
				case 136: // show playlist
					GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
					break;

        case 654: // Eject
					if (Utils.getDriveType(item.Path) != 5) Utils.EjectCDROM();
					else Utils.EjectCDROM(System.IO.Path.GetPathRoot(item.Path));
          LoadDirectory("");
          break;

				case 341: //Play dvd
					OnPlayDVD();
					break;

				case 346: //Stack
					_MapSettings.Stack = true;
					LoadDirectory(m_strDirectory);
					UpdateButtons();
					break;

				case 347: //Unstack
					_MapSettings.Stack = false;
					LoadDirectory(m_strDirectory);
					UpdateButtons();
					break;

				case 102: //Scan
					ArrayList itemlist = m_directory.GetDirectory(m_strDirectory);
					OnScan(itemlist);
					LoadDirectory(m_strDirectory);
					break;
			}
		}

		void OnDeleteItem(GUIListItem item)
		{
			if (item.IsRemote) return;
			
			IMDBMovie movieDetails = new IMDBMovie();

			int movieid=VideoDatabase.GetMovieInfo(item.Path, ref movieDetails);

			string strFileName=System.IO.Path.GetFileName(item.Path);
			if (movieid>=0) strFileName=movieDetails.Title;
		
      //get all movies belonging to each other
      if (_MapSettings.Stack)
      {           
        bool bStackedFile=false;
        ArrayList items = m_directory.GetDirectory(m_strDirectory);
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem pItemTmp = (GUIListItem)items[i];
          if (Utils.ShouldStack(pItemTmp.Path, item.Path))
          {   
            bStackedFile=true;
            strFileName=System.IO.Path.GetFileName(pItemTmp.Path);
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null==dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
            dlgYesNo.SetLine(1,strFileName);
            dlgYesNo.SetLine(2, "");
            dlgYesNo.SetLine(3, "");
            dlgYesNo.DoModal(GetID);

            if (!dlgYesNo.IsConfirmed) break;
            DoDeleteItem(pItemTmp);
          }
        }

        if (!bStackedFile)
        {
          // delete single file
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null==dlgYesNo) return;
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
          dlgYesNo.SetLine(1,strFileName);
          dlgYesNo.SetLine(2, "");
          dlgYesNo.SetLine(3, "");
          dlgYesNo.DoModal(GetID);

          if (!dlgYesNo.IsConfirmed) return;
          DoDeleteItem(item);
        }
      }
      else
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null==dlgYesNo) return;
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
        dlgYesNo.SetLine(1,strFileName);
        dlgYesNo.SetLine(2, "");
        dlgYesNo.SetLine(3, "");
        dlgYesNo.DoModal(GetID);

        if (!dlgYesNo.IsConfirmed) return;
        DoDeleteItem(item);
      }
      
			m_iItemSelected=GetSelectedItemNo();
			if (m_iItemSelected>0) m_iItemSelected--;
			LoadDirectory(m_strDirectory);
			if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_VIEW,m_iItemSelected);
			}
		}

    void DoDeleteItem(GUIListItem item)
    {
      if (item.IsFolder)
      {
				if (item.IsRemote) return;
        if (item.Label != "..")
        {
          ArrayList items = new ArrayList();
          items=m_directory.GetDirectoryUnProtected(item.Path,false);
          foreach(GUIListItem subItem in items)
          {
            DoDeleteItem(subItem);
          }
          Utils.DirectoryDelete(item.Path);
        }
      }
      else		
      {
				VideoDatabase.DeleteMovie(item.Path);
				TVDatabase.RemoveRecordedTVByFileName(item.Path);

				if (item.IsRemote) return;
				Utils.FileDelete(item.Path);
			}
		}

		static public bool IsFolderPinProtected(string folder)
		{
			int pinCode=0;
			if (m_directory.IsProtectedShare(folder,out pinCode)) return true;
			return false;
		}
  }
}
