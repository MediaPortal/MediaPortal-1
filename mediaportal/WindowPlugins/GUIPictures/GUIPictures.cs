using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Picture.Database;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Pictures
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIPictures: GUIWindow, IComparer, ISetupForm
  {
		#region MapSettings class
    [Serializable]
    public class MapSettings
    {
      protected int   _SortBy;
      protected int   _ViewAs;
      protected bool _SortAscending ;

      public MapSettings()
      {
				// Set default view
        _SortBy= (int)SortMethod.Name;
				_ViewAs = (int)View.Icons;
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
      
      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending;}
        set { _SortAscending=value;}
      }
    }
		#endregion

    #region Base variabeles
    enum SortMethod
    {
      Name=0,
      Date=1,
      Size=2
    }

    enum View
    {
      List    =       0,
      Icons    =      1,
      BigIcons  =   2,
      Filmstrip   =   3,
    }

		[SkinControlAttribute(2)]		protected GUIButtonControl btnViewAs=null;
		[SkinControlAttribute(3)]		protected GUIButtonControl btnSortBy=null;
		[SkinControlAttribute(4)]		protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(6)]		protected GUIButtonControl btnSlideShow=null;
		[SkinControlAttribute(7)]		protected GUIButtonControl btnSlideShowRecursive=null;
		[SkinControlAttribute(8)]		protected GUIButtonControl btnCreateThumbs=null;
		[SkinControlAttribute(9)]		protected GUIButtonControl btnRotate=null;
		[SkinControlAttribute(10)]		protected GUIFacadeControl facadeView=null;


    const string      ThumbsFolder=@"Thumbs\Pictures";
    int               m_iItemSelected=-1;
		GUIListItem				m_itemItemSelected=null;
    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory=String.Empty;
		string						m_strDestination=String.Empty;
    VirtualDirectory  m_directory = new VirtualDirectory();
    MapSettings       _MapSettings = new MapSettings();
		bool							m_bFileMenuEnabled=false;
		string						m_strFileMenuPinCode=String.Empty;
		    
    #endregion
    
		#region ctor/dtor
    public GUIPictures()
    {
      GetID=(int)GUIWindow.Window.WINDOW_PICTURES;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.PictureExtensions);
    }
    ~GUIPictures()
    {
      SaveSettings();
    }

		#endregion

		#region Serialisation
		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_bFileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
				m_strFileMenuPinCode = xmlreader.GetValueAsString("filemenu", "pincode", String.Empty);
				string strDefault=xmlreader.GetValueAsString("pictures", "default",String.Empty);
				m_directory.Clear();
				for (int i=0; i < 20; i++)
				{
					string strShareName=String.Format("sharename{0}",i);
					string strSharePath=String.Format("sharepath{0}",i);
					string strPincode = String.Format("pincode{0}",i);

					string shareType = String.Format("sharetype{0}", i);
					string shareServer = String.Format("shareserver{0}", i);
					string shareLogin = String.Format("sharelogin{0}", i);
					string sharePwd  = String.Format("sharepassword{0}", i);
					string sharePort = String.Format("shareport{0}", i);
					string remoteFolder = String.Format("shareremotepath{0}", i);

					Share share=new Share();
					share.Name=xmlreader.GetValueAsString("pictures", strShareName,String.Empty);
					share.Path=xmlreader.GetValueAsString("pictures", strSharePath,String.Empty);
					share.Pincode = xmlreader.GetValueAsInt("pictures", strPincode, - 1);
          
					share.IsFtpShare= xmlreader.GetValueAsBool("pictures", shareType, false);
					share.FtpServer= xmlreader.GetValueAsString("pictures", shareServer,String.Empty);
					share.FtpLoginName= xmlreader.GetValueAsString("pictures", shareLogin,String.Empty);
					share.FtpPassword= xmlreader.GetValueAsString("pictures", sharePwd,String.Empty);
					share.FtpPort= xmlreader.GetValueAsInt("pictures", sharePort,21);
					share.FtpFolder= xmlreader.GetValueAsString("pictures", remoteFolder,"/");

					if (share.Name.Length>0)
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
			}
		}

		void SaveSettings()
		{
		}
		#endregion

		#region overrides
    public override bool Init()
    {
      m_strDirectory=String.Empty;
			m_strDestination=String.Empty;
			try
			{
      System.IO.Directory.CreateDirectory(ThumbsFolder);
			}
			catch(Exception){}
			bool result= Load (GUIGraphicsContext.Skin+@"\mypics.xml");
			LoadSettings();
			return result;
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
			if (action.wID == Action.ActionType.ACTION_DELETE_ITEM)
			{
				// delete current picture
				GUIListItem item=GetSelectedItem();
				if (item!=null)
				{
					if (item.IsFolder==false)
					{
						OnDeleteItem(item);
					}
				}
			}		

      base.OnAction(action);
    }

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			GUITextureManager.CleanupThumbs();
			LoadSettings();
			LoadFolderSettings(m_strDirectory);
			ShowThumbPanel();
			LoadDirectory(m_strDirectory);
			if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,m_iItemSelected);
			}
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			m_iItemSelected=GetSelectedItemNo();
			SaveSettings();          
			SaveFolderSettings(m_strDirectory);
			base.OnPageDestroy (newWindowId);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnViewAs)
			{
				switch ((View)_MapSettings.ViewAs)
				{
				 case View.List:
					 _MapSettings.ViewAs=(int)View.Icons;
					 break;
				 case View.Icons:
					 _MapSettings.ViewAs=(int)View.BigIcons;
					 break;
				 case View.BigIcons:
					 _MapSettings.ViewAs=(int)View.Filmstrip;
					 break;
				 case View.Filmstrip:
					 _MapSettings.ViewAs=(int)View.List;
					 break;
				}
				ShowThumbPanel();
				GUIControl.FocusControl(GetID,control.GetID);
			}
			if (control==btnSortAsc)
			{
				_MapSettings.SortAscending=!_MapSettings.SortAscending;
				OnSort();
				GUIControl.FocusControl(GetID,control.GetID);
			}
			if (control==btnSortBy) // sort by
			{
				switch ((SortMethod)_MapSettings.SortBy)
				{
					case SortMethod.Name:
						_MapSettings.SortBy=(int)SortMethod.Date;
						break;
					case SortMethod.Date:
						_MapSettings.SortBy=(int)SortMethod.Size;
						break;
					case SortMethod.Size:
						_MapSettings.SortBy=(int)SortMethod.Name;
						break;
				}
				OnSort();
				GUIControl.FocusControl(GetID,control.GetID);
			}

			if (control==facadeView)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,facadeView.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SHOW_INFO) 
				{
					if (m_directory.IsRemote(m_strDirectory)) return ;
					OnInfo(iItem);
				}
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(iItem);
				}
				if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
				{
					if (m_directory.IsRemote(m_strDirectory)) return ;
					OnQueueItem(iItem);
				}
			}
			else if (control==btnSlideShow) // Slide Show
			{
				OnSlideShow();
			}
			else if (control==btnSlideShowRecursive) // Recursive Slide Show
			{
				OnSlideShowRecursive();
			}
			else if (control==btnCreateThumbs) // Create Thumbs
			{
				if (m_directory.IsRemote(m_strDirectory)) return ;
				OnCreateThumbs();
			}
			else if (control==btnRotate) // Rotate Pic
			{
				OnRotatePicture();
				return ;
			}
		}


    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_START_SLIDESHOW:
        {
          string strUrl = message.Label;
          LoadDirectory( strUrl );
          OnSlideShow();
        }
          break;

				case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
					m_strDirectory=message.Label;
					OnSlideShowRecursive();
					break;

				case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
					m_strDirectory=message.Label;
					LoadDirectory(m_strDirectory);
					break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          GUIFacadeControl pControl=(GUIFacadeControl)GetControl(facadeView.GetID);
          pControl.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          GUIFacadeControl pControl2=(GUIFacadeControl)GetControl(facadeView.GetID);
          pControl2.OnMessage(message);
          break;

				case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
				case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
					if (m_strDirectory == String.Empty || m_strDirectory.Substring(0,2)==message.Label)
					{
						m_strDirectory = String.Empty;
						LoadDirectory(m_strDirectory);
					}
					break;

      }
      return base.OnMessage(message);
    }

		protected override void OnShowContextMenu()
		{
			GUIListItem item=GetSelectedItem();
			m_itemItemSelected=item;
			int itemNo=GetSelectedItemNo();
			m_iItemSelected=itemNo;

			if (item==null) return;
			if (item.IsFolder && item.Label=="..") return;

			GUIControl cntl=GetControl(facadeView.GetID);
			if (cntl==null) return; // Control not found

			GUIDialogMenu	dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu
			if (!item.IsFolder)
			{
				dlg.AddLocalizedString(735); //rotate
				dlg.AddLocalizedString(923); //show
				dlg.AddLocalizedString(108); //start slideshow
				dlg.AddLocalizedString(940); //properties
			}

			int iPincodeCorrect;
			if (!m_directory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && m_bFileMenuEnabled)
			{
				dlg.AddLocalizedString(500); // FileMenu
			}

			dlg.DoModal(GetID);
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 735: // rotate
					OnRotatePicture();
					break;

				case 923: // show
					OnClick(itemNo);	
					break;

				case 108: // start slideshow
					OnSlideShow(itemNo);	
					break;

				case 940: // properties
					OnInfo(itemNo);	
					break;

				case 500: // File menu
				{
					// get pincode
					if (m_strFileMenuPinCode != String.Empty)
					{
						string strUserCode=String.Empty;
						if (GetUserInputString(ref strUserCode) && strUserCode==m_strFileMenuPinCode)
						{
							OnShowFileMenu();
						}
					}
					else 
						OnShowFileMenu();
				}
					break;
			}
		}
		
		#endregion

		#region listview management

    bool ViewByIcon
    {
      get 
      {
        if (_MapSettings.ViewAs != (int)View.List) return true;
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (_MapSettings.ViewAs == (int)View.BigIcons) return true;
        return false;
      }
    }

    GUIListItem GetSelectedItem()
    {
      return facadeView.SelectedListItem;
    }

    GUIListItem GetItem(int iItem)
    {
      return facadeView[iItem];
    }

    int GetSelectedItemNo()
    {
			return facadeView.SelectedListItemIndex;
    }

    int GetItemCount()
    {
      return facadeView.Count;
    }
    
		void UpdateButtonStates()
    {
      string strLine=String.Empty;
      View view=(View)_MapSettings.ViewAs;
      SortMethod method=(SortMethod )_MapSettings.SortBy;
      bool bAsc=_MapSettings.SortAscending;
			btnRotate.IsVisible=false;
      switch (view)
      {
        case View.List:
          strLine=GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          strLine=GUILocalizeStrings.Get(100);
          break;
        case View.BigIcons:
          strLine=GUILocalizeStrings.Get(417);
          break;
        case View.Filmstrip:
          strLine=GUILocalizeStrings.Get(733);
          
          btnRotate.IsVisible=true;
					btnRotate.Disabled=true;
          GUIListItem item=GetSelectedItem();
          if (item!=null)
          {
            if (!item.IsFolder)
            {
              btnRotate.Disabled=false;
            }
          }
        break;
      }
      GUIControl.SetControlLabel(GetID,btnViewAs.GetID,strLine);

      switch (method)
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
      }
      GUIControl.SetControlLabel(GetID,btnSortBy.GetID,strLine);

      if (bAsc)
				btnSortAsc.Selected=false;
			else
				btnSortAsc.Selected=true;
    }

    void ShowThumbPanel()
    {
      int iItem=GetSelectedItemNo(); 
      if ( _MapSettings.ViewAs== (int)View.BigIcons )
      {
        facadeView.View=GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.Icons)
      {
        facadeView.View=GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.List)
      {
        facadeView.View=GUIFacadeControl.ViewMode.List;
      }
      else if (_MapSettings.ViewAs== (int)View.Filmstrip)
      {
        facadeView.View=GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (iItem>-1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID,iItem);
      }
      UpdateButtonStates();
    }

		#endregion

		#region folder settings
		void LoadFolderSettings(string strDirectory)
		{
			if (strDirectory==String.Empty) strDirectory="root";
			object o;
			FolderSettings.GetFolderSetting(strDirectory,"Pictures",typeof(GUIPictures.MapSettings), out o);
			if (o!=null) _MapSettings = o as MapSettings;
			if (_MapSettings==null) _MapSettings  = new MapSettings();				
		}
    void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory==String.Empty) strDirectory="root";
      FolderSettings.AddFolderSetting(strDirectory,"Pictures",typeof(GUIPictures.MapSettings), _MapSettings);
    }
		#endregion

    #region Sort Members
    void OnSort()
    {
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

      string strSize1=String.Empty;
      string strSize2=String.Empty;
      if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);

      SortMethod method=(SortMethod )_MapSettings.SortBy;
      bool bAsc=_MapSettings.SortAscending;

      switch (method)
      {
        case SortMethod.Name:
          item1.Label2=strSize1;
          item2.Label2=strSize2;

          if (bAsc)
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
          if (bAsc)
          {
            return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
          }

        case SortMethod.Size:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          item1.Label2=strSize1;
          item2.Label2=strSize2;
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

		#region onXXX methods
		void OnRetrieveCoverArt(GUIListItem item)
		{
			if (item.IsRemote) return;
			Utils.SetDefaultIcons(item);
			Utils.SetThumbnails(ref item);
			if (!item.IsFolder)
			{
				string strThumb=GetThumbnail(item.Path) ;
				item.ThumbnailImage=strThumb;
			}
			else
			{
				if (item.Label!="..")
				{
					string strThumb=item.Path+@"\folder.jpg" ;
					if (System.IO.File.Exists(strThumb))
					{
						item.ThumbnailImage=strThumb;
					}
				}
			}
		}

		void OnDeleteItem(GUIListItem item)
		{
			if (item.IsRemote) return;

			GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (null==dlgYesNo) return;
			string strFileName=System.IO.Path.GetFileName(item.Path);
			if (!item.IsFolder) dlgYesNo.SetHeading(664);
			else dlgYesNo.SetHeading(503);
			dlgYesNo.SetLine(1,strFileName);
			dlgYesNo.SetLine(2, String.Empty);
			dlgYesNo.SetLine(3, String.Empty);
			dlgYesNo.DoModal(GetID);

			if (!dlgYesNo.IsConfirmed) return;
			DoDeleteItem(item);
						
			m_iItemSelected=GetSelectedItemNo();
			if (m_iItemSelected>0) m_iItemSelected--;
			LoadDirectory(m_strDirectory);
			if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,m_iItemSelected);
			}					
		}

		void DoDeleteItem(GUIListItem item)
		{
			if (item.IsFolder)
			{
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
			else if (!item.IsRemote)
			{  			
				Utils.FileDelete(item.Path);
			}
		}

    
		void OnInfo(int itemNumber)
		{
			GUIListItem item=GetItem(itemNumber);
			if (item==null) return;
			if (item.IsFolder || item.IsRemote) return;
			GUIDialogExif exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_EXIF);
			exifDialog.FileName=item.Path;
			exifDialog.DoModal(GetID);
		}
		void OnRotatePicture()
		{
			GUIListItem item=GetSelectedItem();
			if (item==null) return;
			if (item.IsFolder) return;
			if (item.IsRemote) return;
			int rotate=0;
			using (PictureDatabase dbs = new PictureDatabase())
			{
				rotate=dbs.GetRotation(item.Path);
				rotate++;
				if (rotate>=4)
				{
					rotate=0;
				}
				dbs.SetRotation(item.Path,rotate);
			}
			string strThumb=GetThumbnail(item.Path );
			Util.Picture.CreateThumbnail(item.Path,strThumb,128,128,rotate);

			strThumb=GetLargeThumbnail(item.Path) ;
			Util.Picture.CreateThumbnail(item.Path,strThumb,512,512,rotate);
			System.Threading.Thread.Sleep(100);
			GUIControl.RefreshControl(GetID, facadeView.GetID);      
		}

		void OnClick(int iItem)
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
          }
          return;
        }

        m_iItemSelected=GetSelectedItemNo();
        OnShowPicture(item.Path);  
      }
    }
    
    void OnQueueItem(int iItem)
    {
    }

    void OnShowPicture(string strFile)
    {
      GUISlideShow SlideShow = (GUISlideShow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow==null) return;
      

      SlideShow.Reset();
      for (int i=0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (!item.IsFolder)
        {
          if (item.IsRemote) continue;
          SlideShow.Add(item.Path);
        }
      }
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      SlideShow.Select(strFile);
    }

    void AddDir(GUISlideShow SlideShow ,string strDir)
    {
      ArrayList itemlist=m_directory.GetDirectory(strDir);
      Filter(ref itemlist);
      foreach (GUIListItem item in itemlist)
      {
        if (item.IsFolder)
        {
          if (item.Label!="..")
            AddDir(SlideShow,item.Path);
        }
        else if (!item.IsRemote)
        {
          SlideShow.Add(item.Path);
        }
      }
    }

    void OnSlideShowRecursive()
    {
      GUISlideShow SlideShow = (GUISlideShow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow==null) return;
      
      SlideShow.Reset();
      AddDir(SlideShow, m_strDirectory);
      SlideShow.StartSlideShow();
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
    }

    void OnSlideShow()
    {
      OnSlideShow(0);
    }
    void OnSlideShow(int iStartItem)
    {
      GUISlideShow SlideShow = (GUISlideShow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow==null) return;     

      SlideShow.Reset();

      if ((iStartItem<0) || (iStartItem>GetItemCount())) iStartItem=0;
      int i=iStartItem;
      do
      {
        GUIListItem item = GetItem(i);
        if (!item.IsFolder && !item.IsRemote)
        {
          SlideShow.Add(item.Path);
        }

        i++;
        if (i >= GetItemCount())
        {
          i=0;
        }
      }
      while (i != iStartItem);

      SlideShow.StartSlideShow();
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
    }

    void OnCreateThumbs()
    {
      CreateFolderThumbs();
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress!=null)
      {
        dlgProgress.SetHeading(110);
        dlgProgress.SetLine(1,String.Empty);
        dlgProgress.SetLine(2,String.Empty);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }


      using (PictureDatabase dbs = new PictureDatabase())
      {
        for (int i=0; i < GetItemCount(); ++i)
        {
          GUIListItem item = GetItem(i);
          if (item.IsRemote) continue;
          if (!item.IsFolder)
          {
            if (Utils.IsPicture(item.Path))
            {
              string strProgress=String.Format("progress:{0}/{1}", i+1, GetItemCount() );
              string strFile=String.Format("picture:{0}", item.Label);
              if (dlgProgress!=null)
              {
                dlgProgress.SetLine(1, strFile);
                dlgProgress.SetLine(2, strProgress);
                dlgProgress.Progress();
                if ( dlgProgress.IsCanceled ) break;
              }


              string strThumb=GetThumbnail(item.Path );
              int iRotate=dbs.GetRotation(item.Path);
              Util.Picture.CreateThumbnail(item.Path,strThumb,128,128,iRotate);
            }
          }
        }
      }
      if (dlgProgress!=null) dlgProgress.Close();
      GUITextureManager.CleanupThumbs();
      LoadDirectory(m_strDirectory);
    }

		void OnShowFileMenu()
		{
			GUIListItem item=m_itemItemSelected;
			if (item==null) return;
			if (item.IsFolder && item.Label=="..") return;

			// init
			GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILE);
			if (dlgFile == null) return;
			
			// File operation settings
			dlgFile.SetSourceItem(item);
			dlgFile.SetSourceDir(m_strDirectory);
			dlgFile.SetDestinationDir(m_strDestination);
			dlgFile.SetDirectoryStructure(m_directory);
			dlgFile.DoModal(GetID);
			m_strDestination = dlgFile.GetDestinationDir();

			//final		
			if (dlgFile.Reload())
			{
				LoadDirectory(m_strDirectory);
				if (m_iItemSelected>=0)
				{
					GUIControl.SelectItemControl(GetID,facadeView.GetID,m_iItemSelected);
				}
			}

			dlgFile.DeInit();
			dlgFile=null;
		}

		
		#endregion

		#region various
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
			if (strNewDirectory != m_strDirectory && _MapSettings!=null) 
			{
				SaveFolderSettings(m_strDirectory);
			}

			if (strNewDirectory != m_strDirectory || _MapSettings==null) 
			{
				LoadFolderSettings(strNewDirectory);
			}

			m_strDirectory=strNewDirectory;
			GUIControl.ClearControl(GetID,facadeView.GetID);
     
			CreateThumbnails();       
			string strObjects=String.Empty;

			ArrayList itemlist=m_directory.GetDirectory(m_strDirectory);
			Filter(ref itemlist);
      
			string strSelectedItem=m_history.Get(m_strDirectory);	
			int iItem=0;
			foreach (GUIListItem item in itemlist)
			{
				item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
				item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
				facadeView.Add(item);

			}
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
			int iTotalItems=itemlist.Count;
			if (itemlist.Count>0)
			{
				GUIListItem rootItem=(GUIListItem)itemlist[0];
				if (rootItem.Label=="..") iTotalItems--;
			}
			strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
      
			ShowThumbPanel();
		}

		void CreateFolderThumbs()
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress!=null)
      {
        dlgProgress.SetHeading(110);
        dlgProgress.SetLine(1,String.Empty);
        dlgProgress.SetLine(2,String.Empty);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      for (int i=0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label!="..")
        {
          string strProgress=String.Format("progress:{0}/{1}", i+1, GetItemCount() );
          string strFile=String.Format("folder:{0}", item.Label);
          if (dlgProgress!=null)
          {
            dlgProgress.SetLine(1, strFile);
            dlgProgress.SetLine(2, strProgress);
            dlgProgress.Progress();
            if ( dlgProgress.IsCanceled ) break;
          }

          CreateFolderThumb(item.Path);
        }//if (item.IsFolder)
      }//for (int i=0; i < GetItemCount(); ++i)
      if (dlgProgress!=null) dlgProgress.Close();
    }

    void CreateFolderThumb(string path)
    {
      // find first 4 jpegs in this subfolder
      ArrayList itemlist=m_directory.GetDirectoryUnProtected(path,true);
      Filter(ref itemlist);
      ArrayList m_pics=new ArrayList();
      foreach (GUIListItem subitem in itemlist)
      {
        if (!subitem.IsFolder)
        {
          if ( Utils.IsPicture(subitem.Path) )
          {
            m_pics.Add(subitem.Path);
            if (m_pics.Count>=4) break;
          }
        }
      }
      if (m_pics.Count>0)
      {
        using (Image imgFolder=Image.FromFile(GUIGraphicsContext.Skin+@"\media\previewbackground.png") )
        {
          int iWidth=imgFolder.Width;
          int iHeight=imgFolder.Height;
          
          int iThumbWidth=(iWidth-30)/2;
          int iThumbHeight=(iHeight-30)/2;

          using (Bitmap bmp = new Bitmap(iWidth,iHeight))
          {
            using (Graphics g = Graphics.FromImage(bmp) )
            {
              g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
              g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
              g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
  
              g.DrawImage(imgFolder,0,0,iWidth,iHeight);

              int x,y,w,h;
              x=0;y=0;w=iThumbWidth;h=iThumbHeight;
              using (Image img=LoadPicture((string)m_pics[0]))
              {
                g.DrawImage(img,x+10,y+10,w,h);
              }
              
              if (m_pics.Count>1)
              {
                using (Image img=LoadPicture((string)m_pics[1]))
                {
                  g.DrawImage(img,x+iThumbWidth+20,y+10,w,h);
                }
              }
              
              if (m_pics.Count>2)
              {
                using (Image img=LoadPicture((string)m_pics[2]))
                {
                  g.DrawImage(img,x+10,y+iThumbHeight+20,w,h);
                }
              }
              if (m_pics.Count>3)
              {
                using (Image img=LoadPicture((string)m_pics[3]))
                {
                  g.DrawImage(img,x+iThumbWidth+20,y+iThumbHeight+20,w,h);
                }
              }
            }//using (Graphics g = Graphics.FromImage(bmp) )
            try
            {
              string strThumbName=path+@"\folder.jpg";
              Utils.FileDelete(strThumbName);
              bmp.Save(strThumbName,System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception)
            {
            }
          }//using (Bitmap bmp = new Bitmap(210,210))
        }
      }//if (m_pics.Count>0)
    }

    Image LoadPicture(string strFileName)
    {
      Image img = null;
      using (PictureDatabase dbs= new PictureDatabase())
      {
        int iRotate=dbs.GetRotation(strFileName);
        img = Image.FromFile(strFileName);
        if (img!=null)
        {
          if (iRotate>0)
          {
            RotateFlipType fliptype;
            switch (iRotate)
            {
              case 1:
                fliptype=RotateFlipType.Rotate90FlipNone;
                img.RotateFlip(fliptype);
                break;
              case 2:
                fliptype=RotateFlipType.Rotate180FlipNone;
                img.RotateFlip(fliptype);
                break;
              case 3:
                fliptype=RotateFlipType.Rotate270FlipNone;
                img.RotateFlip(fliptype);
                break;
              default:
                fliptype=RotateFlipType.RotateNoneFlipNone;
                break;
            }
          }
        }
      }
      return img;
    }

    void Filter(ref ArrayList itemlist)
    {
      bool bFound;
      do
      {
        bFound=false;
        for (int i=0; i < itemlist.Count;++i)
        {
          GUIListItem item=(GUIListItem) itemlist[i];
          if (!item.IsFolder)
          {
            if ( item.Path.IndexOf("folder.jpg") > 0 )
            {
              bFound=true;
              itemlist.RemoveAt(i);
              break;
            }
          }
        }
      } while (bFound);
		}

    static public string GetThumbnail(string strPhoto)
    {
      if (strPhoto==String.Empty) return String.Empty;		
      return String.Format(@"{0}\{1}.jpg",ThumbsFolder,Utils.EncryptLine(strPhoto) );
    }
    static public string GetLargeThumbnail(string strPhoto)
    {
      if (strPhoto==String.Empty) return String.Empty;
      return String.Format(@"{0}\{1}L.jpg",ThumbsFolder,Utils.EncryptLine(strPhoto) );
    }


		void CreateThumbnails()
		{
			Thread WorkerThread = new Thread(new ThreadStart(WorkerThreadFunction));

			WorkerThread.Start();
		}
    
		void WorkerThreadFunction()
		{
			string path=m_strDirectory;
			ArrayList itemlist=m_directory.GetDirectoryUnProtected(path,true);
			using (PictureDatabase dbs = new PictureDatabase())
			{
				foreach (GUIListItem item in itemlist)
				{
					if (m_strDirectory!=path) return;
					if (GUIWindowManager.ActiveWindow!=GetID) return;
					if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;
					if (!item.IsFolder)
					{
						if (Utils.IsPicture(item.Path) )
						{
							string strThumb=GetThumbnail(item.Path) ;
							if (!System.IO.File.Exists(strThumb))
							{
								int iRotate=dbs.GetRotation(item.Path);
								Util.Picture.CreateThumbnail(item.Path,strThumb,128,128,iRotate);
								System.Threading.Thread.Sleep(100);
							}

							strThumb=GetLargeThumbnail(item.Path) ;
							if (!System.IO.File.Exists(strThumb))
							{
								int iRotate=dbs.GetRotation(item.Path);
								Util.Picture.CreateThumbnail(item.Path,strThumb,512,512,iRotate);
								System.Threading.Thread.Sleep(100);
							}
						}
					}
					else
					{
						if (item.Label!="..")
						{
							string strThumb=item.Path+@"\folder.jpg" ;
							if (!System.IO.File.Exists(strThumb))
							{
								CreateFolderThumb(item.Path);
								System.Threading.Thread.Sleep(100);
							}
						}
					}      
				} //foreach (GUIListItem item in itemlist)
			} //using (PictureDatabase dbs = new PictureDatabase())
		} //void WorkerThreadFunction()

		bool GetUserInputString(ref string sString)
		{			
			VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);			
			keyBoard.Reset();
			keyBoard.Text = sString;
			keyBoard.DoModal(GetID); // show it...
			System.GC.Collect(); // collect some garbage
			if (keyBoard.IsConfirmed) sString=keyBoard.Text;
			return keyBoard.IsConfirmed;
		}

		#endregion

		#region callback events
		public bool ThumbnailCallback()
		{
			return false;
		}

		private void item_OnItemSelected(GUIListItem item, GUIControl parent)
		{
			GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
			if (filmstrip==null) return;
			string strThumb=GetLargeThumbnail(item.Path );
			filmstrip.InfoImageFileName=strThumb;
			UpdateButtonStates();
		}

		#endregion
		
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
			return "My Pictures";
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
			// TODO:  Add GUIPictures.GetHome implementation
			strButtonText = GUILocalizeStrings.Get(1);
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
			return "Plugin to watch your photo's";
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIPictures.ShowPlugin implementation
		}

    #endregion
  }
}
