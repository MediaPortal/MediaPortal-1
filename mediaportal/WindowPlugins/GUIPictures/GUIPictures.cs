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
			Albums=3,
      Filmstrip   =   4,
    }

		[SkinControlAttribute(2)]		protected GUIButtonControl btnViewAs=null;
		[SkinControlAttribute(3)]		protected GUIButtonControl btnSortBy=null;
		[SkinControlAttribute(4)]		protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(6)]		protected GUIButtonControl btnSlideShow=null;
		[SkinControlAttribute(7)]		protected GUIButtonControl btnSlideShowRecursive=null;
		[SkinControlAttribute(8)]		protected GUIButtonControl btnCreateThumbs=null;
		[SkinControlAttribute(9)]		protected GUIButtonControl btnRotate=null;
		[SkinControlAttribute(10)]		protected GUIFacadeControl facadeView=null;


    int               selectedItemIndex=-1;
		GUIListItem				selectedListItem=null;
    DirectoryHistory  folderHistory = new DirectoryHistory();
    string            currentFolder=String.Empty;
		string            m_strDirectoryStart=String.Empty;
		string						destinationFolder=String.Empty;
    VirtualDirectory  virtualDirectory = new VirtualDirectory();
    MapSettings       mapSettings = new MapSettings();
		bool							isFileMenuEnabled=false;
		string						fileMenuPinCode=String.Empty;
		    
    #endregion
    
		#region ctor/dtor
    public GUIPictures()
    {
      GetID=(int)GUIWindow.Window.WINDOW_PICTURES;
      
      virtualDirectory.AddDrives();
      virtualDirectory.SetExtensions (Utils.PictureExtensions);
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
				isFileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
				fileMenuPinCode = xmlreader.GetValueAsString("filemenu", "pincode", String.Empty);
				string strDefault=xmlreader.GetValueAsString("pictures", "default",String.Empty);
				virtualDirectory.Clear();
				for (int i=0; i < 20; i++)
				{
					string shareName=String.Format("sharename{0}",i);
					string sharePath=String.Format("sharepath{0}",i);
					string strPincode = String.Format("pincode{0}",i);

					string shareType = String.Format("sharetype{0}", i);
					string shareServer = String.Format("shareserver{0}", i);
					string shareLogin = String.Format("sharelogin{0}", i);
					string sharePwd  = String.Format("sharepassword{0}", i);
					string sharePort = String.Format("shareport{0}", i);
					string remoteFolder = String.Format("shareremotepath{0}", i);
					string shareViewPath = String.Format("shareview{0}", i);

					Share share=new Share();
					share.Name=xmlreader.GetValueAsString("pictures", shareName,String.Empty);
					share.Path=xmlreader.GetValueAsString("pictures", sharePath,String.Empty);
					share.Pincode = xmlreader.GetValueAsInt("pictures", strPincode, - 1);
          
					share.IsFtpShare= xmlreader.GetValueAsBool("pictures", shareType, false);
					share.FtpServer= xmlreader.GetValueAsString("pictures", shareServer,String.Empty);
					share.FtpLoginName= xmlreader.GetValueAsString("pictures", shareLogin,String.Empty);
					share.FtpPassword= xmlreader.GetValueAsString("pictures", sharePwd,String.Empty);
					share.FtpPort= xmlreader.GetValueAsInt("pictures", sharePort,21);
					share.FtpFolder= xmlreader.GetValueAsString("pictures", remoteFolder,"/");
					share.DefaultView= (Share.Views)xmlreader.GetValueAsInt("pictures", shareViewPath, (int)Share.Views.List);

					if (share.Name.Length>0)
					{

						if (strDefault == share.Name)
						{
							share.Default=true;
							if (currentFolder.Length==0) 
							{
								currentFolder = share.Path;
								m_strDirectoryStart=share.Path;
							}
						}
						virtualDirectory.Add(share);
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
      currentFolder=String.Empty;
			destinationFolder=String.Empty;

			bool result= Load (GUIGraphicsContext.Skin+@"\mypics.xml");
			LoadSettings();
			return result;
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
							if (currentFolder!=m_strDirectoryStart)
							{
								LoadDirectory(item.Path);
								return;
							}
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
			LoadFolderSettings(currentFolder);
			ShowThumbPanel();
			LoadDirectory(currentFolder);
			if (selectedItemIndex>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,selectedItemIndex);
			}
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			selectedItemIndex=GetSelectedItemNo();
			SaveSettings();          
			SaveFolderSettings(currentFolder);
			base.OnPageDestroy (newWindowId);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnViewAs)
			{
				switch ((View)mapSettings.ViewAs)
				{
				 case View.List:
					 mapSettings.ViewAs=(int)View.Icons;
					 break;
				 case View.Icons:
					 mapSettings.ViewAs=(int)View.BigIcons;
					 break;
				 case View.BigIcons:
					 mapSettings.ViewAs=(int)View.Filmstrip;
					 break;
					case View.Albums:
						mapSettings.ViewAs=(int)View.Filmstrip;
						break;

				 case View.Filmstrip:
					 mapSettings.ViewAs=(int)View.List;
					 break;
				}
				ShowThumbPanel();
				GUIControl.FocusControl(GetID,control.GetID);
			}
			if (control==btnSortAsc)
			{
				mapSettings.SortAscending=!mapSettings.SortAscending;
				OnSort();
				GUIControl.FocusControl(GetID,control.GetID);
			}
			if (control==btnSortBy) // sort by
			{
				switch ((SortMethod)mapSettings.SortBy)
				{
					case SortMethod.Name:
						mapSettings.SortBy=(int)SortMethod.Date;
						break;
					case SortMethod.Date:
						mapSettings.SortBy=(int)SortMethod.Size;
						break;
					case SortMethod.Size:
						mapSettings.SortBy=(int)SortMethod.Name;
						break;
				}
				OnSort();
				GUIControl.FocusControl(GetID,control.GetID);
			}

			if (control==facadeView)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,facadeView.GetID,0,0,null);
				OnMessage(msg);         
				int itemIndex=(int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SHOW_INFO) 
				{
					if (virtualDirectory.IsRemote(currentFolder)) return ;
					OnInfo(itemIndex);
				}
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(itemIndex);
				}
				if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
				{
					if (virtualDirectory.IsRemote(currentFolder)) return ;
					OnQueueItem(itemIndex);
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
				if (virtualDirectory.IsRemote(currentFolder)) return ;
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
					currentFolder=message.Label;
					OnSlideShowRecursive();
					break;

				case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
					currentFolder=message.Label;
					LoadDirectory(currentFolder);
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
					if (currentFolder == String.Empty || currentFolder.Substring(0,2)==message.Label)
					{
						currentFolder = String.Empty;
						LoadDirectory(currentFolder);
					}
					break;

      }
      return base.OnMessage(message);
    }

		protected override void OnShowContextMenu()
		{
			GUIListItem item=GetSelectedItem();
			selectedListItem=item;
			int itemNo=GetSelectedItemNo();
			selectedItemIndex=itemNo;

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
			if (!virtualDirectory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && isFileMenuEnabled)
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
					if (fileMenuPinCode != String.Empty)
					{
						string strUserCode=String.Empty;
						if (GetUserInputString(ref strUserCode) && strUserCode==fileMenuPinCode)
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
        if (mapSettings.ViewAs != (int)View.List) return true;
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (mapSettings.ViewAs == (int)View.BigIcons) return true;
        return false;
      }
    }

    GUIListItem GetSelectedItem()
    {
      return facadeView.SelectedListItem;
    }

    GUIListItem GetItem(int itemIndex)
    {
			if (itemIndex>=facadeView.Count || itemIndex<0) return null;
      return facadeView[itemIndex];
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
      string textLine=String.Empty;
      View view=(View)mapSettings.ViewAs;
      SortMethod method=(SortMethod )mapSettings.SortBy;
      bool sortAsc=mapSettings.SortAscending;
			btnRotate.IsVisible=false;
      switch (view)
      {
        case View.List:
          textLine=GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          textLine=GUILocalizeStrings.Get(100);
          break;
        case View.BigIcons:
          textLine=GUILocalizeStrings.Get(417);
					break;
				case View.Albums:
					textLine=GUILocalizeStrings.Get(417);
					break;
        case View.Filmstrip:
          textLine=GUILocalizeStrings.Get(733);
          
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
      GUIControl.SetControlLabel(GetID,btnViewAs.GetID,textLine);

      switch (method)
      {
        case SortMethod.Name:
          textLine=GUILocalizeStrings.Get(103);
          break;
        case SortMethod.Date:
          textLine=GUILocalizeStrings.Get(104);
          break;
        case SortMethod.Size:
          textLine=GUILocalizeStrings.Get(105);
          break;
      }
      GUIControl.SetControlLabel(GetID,btnSortBy.GetID,textLine);

      if (sortAsc)
				btnSortAsc.Selected=false;
			else
				btnSortAsc.Selected=true;
    }

    void ShowThumbPanel()
    {
      int itemIndex=GetSelectedItemNo(); 
      if ( mapSettings.ViewAs== (int)View.BigIcons )
      {
        facadeView.View=GUIFacadeControl.ViewMode.LargeIcons;
      }
			else if ( mapSettings.ViewAs== (int)View.Albums )
      {
        facadeView.View=GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (mapSettings.ViewAs== (int)View.Icons)
      {
        facadeView.View=GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (mapSettings.ViewAs== (int)View.List)
      {
        facadeView.View=GUIFacadeControl.ViewMode.List;
      }
      else if (mapSettings.ViewAs== (int)View.Filmstrip)
      {
        facadeView.View=GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (itemIndex>-1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID,itemIndex);
      }
      UpdateButtonStates();
    }

		#endregion

		#region folder settings
		void LoadFolderSettings(string folder)
		{
			if (folder==String.Empty) folder="root";
			object o;
			FolderSettings.GetFolderSetting(folder,"Pictures",typeof(GUIPictures.MapSettings), out o);
			if (o!=null) 
			{
				mapSettings = o as MapSettings;
				if (mapSettings==null) mapSettings  = new MapSettings();
			}
			else
			{
				Share share=virtualDirectory.GetShare(folder);
				if (share!=null)
				{
					if (mapSettings==null) mapSettings  = new MapSettings();
					mapSettings.ViewAs=(int)share.DefaultView;
				}
			}


		}
    void SaveFolderSettings(string folder)
    {
      if (folder==String.Empty) folder="root";
      FolderSettings.AddFolderSetting(folder,"Pictures",typeof(GUIPictures.MapSettings), mapSettings);
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

      string sizeItem1=String.Empty;
      string sizeItem2=String.Empty;
      if (item1.FileInfo!=null) sizeItem1=Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo!=null) sizeItem2=Utils.GetSize(item2.FileInfo.Length);

      SortMethod method=(SortMethod )mapSettings.SortBy;
      bool sortAsc=mapSettings.SortAscending;

      switch (method)
      {
        case SortMethod.Name:
          item1.Label2=sizeItem1;
          item2.Label2=sizeItem2;

          if (sortAsc)
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
          
          item1.Label2 =item1.FileInfo.ModificationTime.ToShortDateString() + " "+item1.FileInfo.ModificationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 =item2.FileInfo.ModificationTime.ToShortDateString() + " "+item2.FileInfo.ModificationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          if (sortAsc)
          {
            return DateTime.Compare(item1.FileInfo.ModificationTime,item2.FileInfo.ModificationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.ModificationTime,item1.FileInfo.ModificationTime);
          }

        case SortMethod.Size:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          item1.Label2=sizeItem1;
          item2.Label2=sizeItem2;
          if (sortAsc)
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
				string thumbnailImage=GetThumbnail(item.Path) ;
				item.ThumbnailImage=thumbnailImage;
			}
			else
			{
				if (item.Label!="..")
				{
					string thumbnailImage=item.Path+@"\folder.jpg" ;
					if (System.IO.File.Exists(thumbnailImage))
					{
						item.ThumbnailImage=thumbnailImage;
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
						
			selectedItemIndex=GetSelectedItemNo();
			if (selectedItemIndex>0) selectedItemIndex--;
			LoadDirectory(currentFolder);
			if (selectedItemIndex>=0)
			{
				GUIControl.SelectItemControl(GetID,facadeView.GetID,selectedItemIndex);
			}					
		}

		void DoDeleteItem(GUIListItem item)
		{
			if (item.IsFolder)
			{
				if (item.Label != "..")
				{
					ArrayList items = new ArrayList();
					items=virtualDirectory.GetDirectoryUnProtected(item.Path,false);
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
			string thumbnailImage=GetThumbnail(item.Path );
			Util.Picture.CreateThumbnail(item.Path,thumbnailImage,128,128,rotate);

			thumbnailImage=GetLargeThumbnail(item.Path) ;
			Util.Picture.CreateThumbnail(item.Path,thumbnailImage,512,512,rotate);
			System.Threading.Thread.Sleep(100);
			GUIControl.RefreshControl(GetID, facadeView.GetID);      
		}

		void OnClick(int itemIndex)
    {
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      if (item.IsFolder)
      {
        selectedItemIndex=-1;
        LoadDirectory(item.Path);
      }
      else
      {
        if (virtualDirectory.IsRemote(item.Path) )
        {
          if (!virtualDirectory.IsRemoteFileDownloaded(item.Path,item.FileInfo.Length) )
          {
            if (!virtualDirectory.ShouldWeDownloadFile(item.Path)) return;
            if (!virtualDirectory.DownloadRemoteFile(item.Path,item.FileInfo.Length))
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

        selectedItemIndex=GetSelectedItemNo();
        OnShowPicture(item.Path);  
      }
    }
    
    void OnQueueItem(int itemIndex)
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
      ArrayList itemlist=virtualDirectory.GetDirectory(strDir);
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
      AddDir(SlideShow, currentFolder);
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
				dlgProgress.ShowProgressBar(true);
        dlgProgress.SetLine(1,String.Empty);
        dlgProgress.SetLine(2,String.Empty);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }


      using (PictureDatabase dbs = new PictureDatabase())
      {
        for (int i=0; i < GetItemCount(); ++i)
        {
					int percent=(i*100) / (GetItemCount()+1);
          GUIListItem item = GetItem(i);
					if (item.IsRemote) continue;
					if (dlgProgress!=null)
						dlgProgress.SetPercentage(percent);
          if (!item.IsFolder)
          {
            if (Utils.IsPicture(item.Path))
            {
              string progressLine=String.Format("progress:{0}/{1}", i+1, GetItemCount() );
              string strFile=String.Format("picture:{0}", item.Label);
              if (dlgProgress!=null)
              {
                dlgProgress.SetLine(1, strFile);
                dlgProgress.SetLine(2, progressLine);
                dlgProgress.Progress();
                if ( dlgProgress.IsCanceled ) break;
              }


              string thumbnailImage=GetThumbnail(item.Path );
              int iRotate=dbs.GetRotation(item.Path);
              Util.Picture.CreateThumbnail(item.Path,thumbnailImage,128,128,iRotate);
            }
          }
        }
      }
      if (dlgProgress!=null) dlgProgress.Close();
      GUITextureManager.CleanupThumbs();
      LoadDirectory(currentFolder);
    }

		void OnShowFileMenu()
		{
			GUIListItem item=selectedListItem;
			if (item==null) return;
			if (item.IsFolder && item.Label=="..") return;

			// init
			GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILE);
			if (dlgFile == null) return;
			
			// File operation settings
			dlgFile.SetSourceItem(item);
			dlgFile.SetSourceDir(currentFolder);
			dlgFile.SetDestinationDir(destinationFolder);
			dlgFile.SetDirectoryStructure(virtualDirectory);
			dlgFile.DoModal(GetID);
			destinationFolder = dlgFile.GetDestinationDir();

			//final		
			if (dlgFile.Reload())
			{
				LoadDirectory(currentFolder);
				if (selectedItemIndex>=0)
				{
					GUIControl.SelectItemControl(GetID,facadeView.GetID,selectedItemIndex);
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
					folderHistory.Set(SelectedItem.Label, currentFolder);
				}
			}
			if (strNewDirectory != currentFolder && mapSettings!=null) 
			{
				SaveFolderSettings(currentFolder);
			}

			if (strNewDirectory != currentFolder || mapSettings==null) 
			{
				LoadFolderSettings(strNewDirectory);
			}

			currentFolder=strNewDirectory;
			GUIControl.ClearControl(GetID,facadeView.GetID);
     
			CreateThumbnails();       
			string objectCount=String.Empty;

			ArrayList itemlist=virtualDirectory.GetDirectory(currentFolder);
			Filter(ref itemlist);
      
			string strSelectedItem=folderHistory.Get(currentFolder);	
			int itemIndex=0;
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
					GUIControl.SelectItemControl(GetID,facadeView.GetID,itemIndex);
					break;
				}
				itemIndex++;
			}
			int totalItemCount=itemlist.Count;
			if (itemlist.Count>0)
			{
				GUIListItem rootItem=(GUIListItem)itemlist[0];
				if (rootItem.Label=="..") totalItemCount--;
			}
			objectCount=String.Format("{0} {1}", totalItemCount, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",objectCount);
      
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
				dlgProgress.ShowProgressBar(true);
				dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      for (int i=0; i < GetItemCount(); ++i)
      {
				int percent=(i*100) / (GetItemCount()+1);
				if (dlgProgress!=null)
					dlgProgress.SetPercentage(percent);
				GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label!="..")
        {
          string progressLine=String.Format("progress:{0}/{1}", i+1, GetItemCount() );
          string fileName=String.Format("folder:{0}", item.Label);
          if (dlgProgress!=null)
          {
            dlgProgress.SetLine(1, fileName);
            dlgProgress.SetLine(2, progressLine);
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
      ArrayList itemlist=virtualDirectory.GetDirectoryUnProtected(path,true);
      Filter(ref itemlist);
      ArrayList pictureList=new ArrayList();
      foreach (GUIListItem subitem in itemlist)
      {
        if (!subitem.IsFolder)
        {
          if ( Utils.IsPicture(subitem.Path) )
          {
            pictureList.Add(subitem.Path);
            if (pictureList.Count>=4) break;
          }
        }
      }
      if (pictureList.Count>0)
      {
        using (Image imgFolder=Image.FromFile(GUIGraphicsContext.Skin+@"\media\previewbackground.png") )
        {
          int width=imgFolder.Width;
          int height=imgFolder.Height;
          
          int thumbnailWidth=(width-30)/2;
          int thumbnailHeight=(height-30)/2;

          using (Bitmap bmp = new Bitmap(width,height))
          {
            using (Graphics g = Graphics.FromImage(bmp) )
            {
              g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
              g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
              g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
  
              g.DrawImage(imgFolder,0,0,width,height);

              int x,y,w,h;
              x=0;y=0;w=thumbnailWidth;h=thumbnailHeight;
              using (Image img=LoadPicture((string)pictureList[0]))
              {
                g.DrawImage(img,x+10,y+10,w,h);
              }
              
              if (pictureList.Count>1)
              {
                using (Image img=LoadPicture((string)pictureList[1]))
                {
                  g.DrawImage(img,x+thumbnailWidth+20,y+10,w,h);
                }
              }
              
              if (pictureList.Count>2)
              {
                using (Image img=LoadPicture((string)pictureList[2]))
                {
                  g.DrawImage(img,x+10,y+thumbnailHeight+20,w,h);
                }
              }
              if (pictureList.Count>3)
              {
                using (Image img=LoadPicture((string)pictureList[3]))
                {
                  g.DrawImage(img,x+thumbnailWidth+20,y+thumbnailHeight+20,w,h);
                }
              }
            }//using (Graphics g = Graphics.FromImage(bmp) )
            try
            {
              string thumbnailImageName=path+@"\folder.jpg";
              Utils.FileDelete(thumbnailImageName);
              bmp.Save(thumbnailImageName,System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception)
            {
            }
          }//using (Bitmap bmp = new Bitmap(210,210))
        }
      }//if (pictureList.Count>0)
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
            RotateFlipType flipType;
            switch (iRotate)
            {
              case 1:
                flipType=RotateFlipType.Rotate90FlipNone;
                img.RotateFlip(flipType);
                break;
              case 2:
                flipType=RotateFlipType.Rotate180FlipNone;
                img.RotateFlip(flipType);
                break;
              case 3:
                flipType=RotateFlipType.Rotate270FlipNone;
                img.RotateFlip(flipType);
                break;
              default:
                flipType=RotateFlipType.RotateNoneFlipNone;
                break;
            }
          }
        }
      }
      return img;
    }

    void Filter(ref ArrayList itemlist)
    {
      bool isFound;
      do
      {
        isFound=false;
        for (int i=0; i < itemlist.Count;++i)
        {
          GUIListItem item=(GUIListItem) itemlist[i];
          if (!item.IsFolder)
          {
            if ( item.Path.IndexOf("folder.jpg") > 0 )
            {
              isFound=true;
              itemlist.RemoveAt(i);
              break;
            }
          }
        }
      } while (isFound);
		}

    static public string GetThumbnail(string fileName)
    {
      if (fileName==String.Empty) return String.Empty;		
      return String.Format(@"{0}\{1}.jpg",Thumbs.Pictures,Utils.EncryptLine(fileName) );
    }
    static public string GetLargeThumbnail(string fileName)
    {
      if (fileName==String.Empty) return String.Empty;
      return String.Format(@"{0}\{1}L.jpg",Thumbs.Pictures,Utils.EncryptLine(fileName) );
    }


		void CreateThumbnails()
		{
			Thread WorkerThread = new Thread(new ThreadStart(WorkerThreadFunction));

			WorkerThread.Start();
		}
    
		void WorkerThreadFunction()
		{
			string path=currentFolder;
			ArrayList itemlist=virtualDirectory.GetDirectoryUnProtected(path,true);
			using (PictureDatabase dbs = new PictureDatabase())
			{
				foreach (GUIListItem item in itemlist)
				{
					if (currentFolder!=path) return;
					if (GUIWindowManager.ActiveWindow!=GetID) return;
					if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;
					if (!item.IsFolder)
					{
						if (Utils.IsPicture(item.Path) )
						{
							string thumbnailImage=GetThumbnail(item.Path) ;
							if (!System.IO.File.Exists(thumbnailImage))
							{
								int iRotate=dbs.GetRotation(item.Path);
								Util.Picture.CreateThumbnail(item.Path,thumbnailImage,128,128,iRotate);
								System.Threading.Thread.Sleep(100);
							}

							thumbnailImage=GetLargeThumbnail(item.Path) ;
							if (!System.IO.File.Exists(thumbnailImage))
							{
								int iRotate=dbs.GetRotation(item.Path);
								Util.Picture.CreateThumbnail(item.Path,thumbnailImage,512,512,iRotate);
								System.Threading.Thread.Sleep(100);
							}
						}
					}
					else
					{
						if (item.Label!="..")
						{
							string thumbnailImage=item.Path+@"\folder.jpg" ;
							if (!System.IO.File.Exists(thumbnailImage))
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
			string thumbnailImage=GetLargeThumbnail(item.Path );
			filmstrip.InfoImageFileName=thumbnailImage;
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
