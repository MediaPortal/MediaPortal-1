#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Picture.Database;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.Util;

namespace MediaPortal.GUI.Pictures
{
  /// <summary>
  /// Displays pictures and offers methods for exif and rotation
  /// </summary>
  [PluginIcons("WindowPlugins.GUIPictures.Pictures.gif", "WindowPlugins.GUIPictures.PicturesDisabled.gif")]
  public class GUIPictures : GUIWindow, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {
    #region ThumbCacher class

    public class MissingThumbCacher
    {
      private VirtualDirectory vDir = new VirtualDirectory();

      private string _filepath = string.Empty;
      private bool _createLarge = true;
      private bool _recreateWithoutCheck = false;
      private Work work;

      public MissingThumbCacher(string Filepath, bool CreateLargeThumbs, bool ReCreateThumbs)
      {
        _filepath = Filepath;
        _createLarge = CreateLargeThumbs;
        _recreateWithoutCheck = ReCreateThumbs;
        //_hideFileExtensions = HideExtensions;

        work = new Work(new DoWorkHandler(this.PerformRequest));
        work.ThreadPriority = ThreadPriority.BelowNormal;
        GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
      }

      /// <summary>
      /// creates cached thumbs in MP's thumbs dir
      /// </summary>
      private void PerformRequest()
      {
        Stopwatch benchclock = new Stopwatch();
        benchclock.Start();
        string path = _filepath;
        bool autocreateLargeThumbs = _createLarge;
        bool recreateThumbs = _recreateWithoutCheck;

        vDir.SetExtensions(Util.Utils.PictureExtensions);

        if (!vDir.IsRemote(path))
        {
          List<GUIListItem> itemlist = vDir.GetDirectoryUnProtectedExt(path, true);

          foreach (GUIListItem item in itemlist)
          {
            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
            {
              return;
            }
            if (String.IsNullOrEmpty(item.Path))
            {
              return;
            }
            if (path.Length >= item.Path.Length)
            {
              Log.Warn("GUIPictures: Omitting outside path {0} from check share {1}", item.Path, path);
              continue;
            }
            Thread.Sleep(100);

            if (CheckPathForHistory(item.Path, false))
            {
              if (!item.IsFolder)
              {
                int iRotate = PictureDatabase.GetRotation(item.Path);
                Thread.Sleep(30);

                if (!item.IsRemote && Util.Utils.IsPicture(item.Path))
                {
                  string thumbnailImage = String.Format(@"{0}\{1}.jpg", Thumbs.Pictures,
                                                        Util.Utils.EncryptLine(item.Path));
                  if (recreateThumbs || !File.Exists(thumbnailImage))
                  {
                    Thread.Sleep(10);
                    if (Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution,
                                                     (int)Thumbs.ThumbResolution, iRotate, Thumbs.SpeedThumbsSmall))
                    {
                      Thread.Sleep(30);
                      Log.Debug("GUIPictures: Creation of missing thumb successful for {0}", item.Path);
                    }
                  }

                  if (autocreateLargeThumbs)
                  {
                    thumbnailImage = String.Format(@"{0}\{1}L.jpg", Thumbs.Pictures, Util.Utils.EncryptLine(item.Path));
                    if (recreateThumbs || !File.Exists(thumbnailImage))
                    {
                      Thread.Sleep(10);
                      if (Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbLargeResolution,
                                                       (int)Thumbs.ThumbLargeResolution, iRotate,
                                                       Thumbs.SpeedThumbsLarge))
                      {
                        Thread.Sleep(30);
                        //Log.Debug("GUIPictures: Creation of missing large thumb successful for {0}", item.Path);
                      }
                    }
                  }
                }
              }
              else
              {
                int pin;
                if ((item.Label != "..") && (!vDir.IsProtectedShare(item.Path, out pin)))
                {
                  string thumbnailImage = item.Path + @"\folder.jpg";
                  if (recreateThumbs || (!item.IsRemote && !File.Exists(thumbnailImage)))
                  {
                    Thread.Sleep(10);
                    if (CreateFolderThumb(item.Path, recreateThumbs))
                    {
                      Thread.Sleep(30);
                      Log.Debug("GUIPictures: Creation of missing folder preview thumb for {0}", item.Path);
                    }
                  }
                }
              }
            } //foreach (GUIListItem item in itemlist)
          }
        }
        benchclock.Stop();
        Log.Debug("GUIPictures: Creation of all thumbs for dir '{0}' took {1} seconds", _filepath,
                  benchclock.Elapsed.TotalSeconds);
      }

      private bool CreateFolderThumb(string path, bool recreateAll)
      {
        // find first 4 jpegs in this subfolder
        List<GUIListItem> itemlist = vDir.GetDirectoryUnProtectedExt(path, true);
        if (!recreateAll)
        {
          Filter(ref itemlist);
        }
        List<string> pictureList = new List<string>();
        foreach (GUIListItem subitem in itemlist)
        {
          if (!subitem.IsFolder)
          {
            if (!subitem.IsRemote && Util.Utils.IsPicture(subitem.Path))
            {
              pictureList.Add(subitem.Path);
              if (pictureList.Count >= 4)
              {
                break;
              }
            }
          }
        }
        // combine those 4 image files into one folder.jpg
        if (Util.Utils.CreateFolderPreviewThumb(pictureList, Path.Combine(path, @"Folder.jpg")))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    #endregion

    #region MapSettings class

    [Serializable]
    public class MapSettings
    {
      protected int _SortBy;
      protected int _ViewAs;
      protected bool _SortAscending;

      public MapSettings()
      {
        // Set default view
        _SortBy = (int)SortMethod.Name;
        _ViewAs = (int)View.Icons;
        _SortAscending = true;
      }

      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy; }
        set { _SortBy = value; }
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs; }
        set { _ViewAs = value; }
      }

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending; }
        set { _SortAscending = value; }
      }
    }

    #endregion

    #region Base variables

    private enum SortMethod
    {
      Name = 0,
      Date = 1,
      Size = 2
    }

    private enum View
    {
      List = 0,
      Icons = 1,
      BigIcons = 2,
      Albums = 3,
      Filmstrip = 4,
    }

    private enum Display
    {
      Files = 0,
      Date = 1
    }

    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(4)] protected GUIButtonControl btnSwitchView = null;
    [SkinControl(6)] protected GUIButtonControl btnSlideShow = null;
    [SkinControl(7)] protected GUIButtonControl btnSlideShowRecursive = null;
    [SkinControl(50)] protected GUIFacadeControl facadeView = null;

    private const int MAX_PICS_PER_DATE = 1000;

    public static List<string> thumbCreationPaths = new List<string>();
    private int selectedItemIndex = -1;
    private GUIListItem selectedListItem = null;
    private DirectoryHistory folderHistory = new DirectoryHistory();
    private string currentFolder = string.Empty;
    private string m_strDirectoryStart = string.Empty;
    private string destinationFolder = string.Empty;
    private VirtualDirectory virtualDirectory = new VirtualDirectory();
    private MapSettings mapSettings = new MapSettings();
    private bool isFileMenuEnabled = false;
    private string fileMenuPinCode = string.Empty;
    private bool _autocreateLargeThumbs = true;
    //bool _hideExtensions = true;
    private Display disp = Display.Files;
    private bool _switchRemovableDrives;
    private int CountOfNonImageItems = 0; // stores the count of items in a folder that are no images (folders etc...)

    #endregion

    #region ctor/dtor

    public GUIPictures()
    {
      GetID = (int)Window.WINDOW_PICTURES;

      virtualDirectory.AddDrives();
      virtualDirectory.SetExtensions(Util.Utils.PictureExtensions);
    }

    ~GUIPictures()
    {
      SaveSettings();
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _autocreateLargeThumbs = !xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", false);
        isFileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        fileMenuPinCode = Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));
        string strDefault = xmlreader.GetValueAsString("pictures", "default", string.Empty);
        virtualDirectory.Clear();
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
        {
          string shareName = String.Format("sharename{0}", i);
          string sharePath = String.Format("sharepath{0}", i);
          string strPincode = String.Format("pincode{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);
          string shareViewPath = String.Format("shareview{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("pictures", shareName, string.Empty);
          share.Path = xmlreader.GetValueAsString("pictures", sharePath, string.Empty);
          string pinCode = Util.Utils.DecryptPin(xmlreader.GetValueAsString("pictures", strPincode, string.Empty));
          if (pinCode != string.Empty)
          {
            share.Pincode = Convert.ToInt32(pinCode);
          }
          else
          {
            share.Pincode = -1;
          }

          share.IsFtpShare = xmlreader.GetValueAsBool("pictures", shareType, false);
          share.FtpServer = xmlreader.GetValueAsString("pictures", shareServer, string.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString("pictures", shareLogin, string.Empty);
          share.FtpPassword = xmlreader.GetValueAsString("pictures", sharePwd, string.Empty);
          share.FtpPort = xmlreader.GetValueAsInt("pictures", sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString("pictures", remoteFolder, "/");
          share.DefaultView = (Share.Views)xmlreader.GetValueAsInt("pictures", shareViewPath, (int)Share.Views.List);

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default = true;
              if (currentFolder.Length == 0)
              {
                if (share.IsFtpShare)
                {
                  //remote:hostname?port?login?password?folder
                  currentFolder = virtualDirectory.GetShareRemoteURL(share);
                  m_strDirectoryStart = currentFolder;
                }
                else
                {
                  currentFolder = share.Path;
                  m_strDirectoryStart = share.Path;
                }
              }
            }
            virtualDirectory.Add(share);
          }
          else
          {
            break;
          }
        }
        if (xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false))
        {
          string lastFolder = xmlreader.GetValueAsString("pictures", "lastfolder", currentFolder);
          if (lastFolder != "root")
          {
            currentFolder = lastFolder;
          }
        }
        _switchRemovableDrives = xmlreader.GetValueAsBool("pictures", "SwitchRemovableDrives", true);
        //_hideExtensions = xmlreader.GetValueAsBool("general", "hideextensions", true);
      }

      if (currentFolder.Length > 0)
      {
        VirtualDirectory VDir = new VirtualDirectory();
        VDir.LoadSettings("pictures");
        int pincode = 0;
        bool FolderPinProtected = VDir.IsProtectedShare(currentFolder, out pincode);
        if (FolderPinProtected)
        {
          currentFolder = string.Empty;
        }
      }
    }

    private void SaveSettings()
    {
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      currentFolder = string.Empty;
      destinationFolder = string.Empty;
      thumbCreationPaths.Clear();
      LoadSettings();
      return Load(GUIGraphicsContext.Skin + @"\mypics.xml");
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
              if (currentFolder != m_strDirectoryStart)
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
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }
      if (action.wID == Action.ActionType.ACTION_DELETE_ITEM)
      {
        // delete current picture
        GUIListItem item = GetSelectedItem();
        if (item != null)
        {
          if (item.IsFolder == false)
          {
            OnDeleteItem(item);
          }
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      if (!KeepVirtualDirectory(PreviousWindowId))
      {
        virtualDirectory.Reset();
      }
      base.OnPageLoad();
      GUITextureManager.CleanupThumbs();
      // LoadSettings();
      LoadFolderSettings(currentFolder);
      ShowThumbPanel();
      LoadDirectory(currentFolder);
      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
      }

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      selectedItemIndex = GetSelectedItemNo();
      SaveSettings();
      SaveFolderSettings(currentFolder);
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnViewAs)
      {
        switch ((View)mapSettings.ViewAs)
        {
          case View.List:
            mapSettings.ViewAs = (int)View.Icons;
            break;
          case View.Icons:
            mapSettings.ViewAs = (int)View.BigIcons;
            break;
          case View.BigIcons:
            mapSettings.ViewAs = (int)View.Filmstrip;
            break;
          case View.Albums:
            mapSettings.ViewAs = (int)View.Filmstrip;
            break;

          case View.Filmstrip:
            mapSettings.ViewAs = (int)View.List;
            break;
        }
        ShowThumbPanel();
        GUIControl.FocusControl(GetID, control.GetID);
      }
      if (control == btnSortBy) // sort by
      {
        OnShowSortMenu();
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, facadeView.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int itemIndex = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          if (virtualDirectory.IsRemote(currentFolder))
          {
            return;
          }
          OnInfo(itemIndex);
        }
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(itemIndex);
        }
        if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
        {
          if (virtualDirectory.IsRemote(currentFolder))
          {
            return;
          }
          OnQueueItem(itemIndex);
        }
      }
      else if (control == btnSlideShow) // Slide Show
      {
        OnSlideShow();
      }
      else if (control == btnSlideShowRecursive) // Recursive Slide Show
      {
        OnSlideShowRecursive();
      }
      else if (control == btnSwitchView) // Switch View
      {
        OnSwitchView();
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_START_SLIDESHOW:
          {
            string strUrl = message.Label;
            LoadDirectory(strUrl);
            OnSlideShow();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
          currentFolder = message.Label;
          OnSlideShowRecursive();
          break;

        case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
          currentFolder = message.Label;
          LoadDirectory(currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_ADD_REMOVABLE_DRIVE:
          if (_switchRemovableDrives)
          {
            currentFolder = message.Label;
            if (!Util.Utils.IsRemovable(message.Label))
            {
              virtualDirectory.AddRemovableDrive(message.Label, message.Label2);
            }
          }
          LoadDirectory(currentFolder);
          break;
        case GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE:
          if (!Util.Utils.IsRemovable(message.Label))
          {
            virtualDirectory.Remove(message.Label);
          }
          if (currentFolder.Contains(message.Label))
          {
            currentFolder = string.Empty;
          }
          LoadDirectory(currentFolder);
          break;
        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          GUIFacadeControl pControl = (GUIFacadeControl)GetControl(facadeView.GetID);
          pControl.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          GUIFacadeControl pControl2 = (GUIFacadeControl)GetControl(facadeView.GetID);
          pControl2.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
        case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
          if (currentFolder == string.Empty || currentFolder.Substring(0, 2) == message.Label)
          {
            currentFolder = string.Empty;
            LoadDirectory(currentFolder);
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = GetSelectedItem();
      selectedListItem = item;
      int itemNo = GetSelectedItemNo();
      selectedItemIndex = itemNo;

      if (item == null)
      {
        return;
      }
      if (item.IsFolder && item.Label == "..")
      {
        return;
      }

      GUIControl cntl = GetControl(facadeView.GetID);
      if (cntl == null)
      {
        return; // Control not found
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu
      if (!item.IsFolder)
      {
        dlg.AddLocalizedString(735); //rotate
        dlg.AddLocalizedString(923); //show
        dlg.AddLocalizedString(108); //start slideshow
        dlg.AddLocalizedString(940); //properties
      }
      else
      {
        //dlg.AddLocalizedString(200046); //Generate Thumbnails
        //dlg.AddLocalizedString(200047); //Recursive Generate Thumbnails
        dlg.AddLocalizedString(200048); //Regenerate Thumbnails
      }
      dlg.AddLocalizedString(457); //Switch View
      int iPincodeCorrect;
      if (!virtualDirectory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && isFileMenuEnabled)
      {
        dlg.AddLocalizedString(500); // FileMenu      
      }
      if (Util.Utils.IsRemovable(item.Path))
      {
        dlg.AddLocalizedString(831);
      }


      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }
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
          // get pincode
          if (fileMenuPinCode != string.Empty)
          {
            string strUserCode = string.Empty;
            if (GetUserInputString(ref strUserCode) && strUserCode == fileMenuPinCode)
            {
              OnShowFileMenu();
            }
          }
          else
          {
            OnShowFileMenu();
          }
          break;
        case 200046: // Generate Thumbnails
          if (item.IsFolder)
          {
            OnCreateAllThumbs(item.Path, false, false);
          }
          break;
        case 200047: // Revursive Generate Thumbnails
          if (item.IsFolder)
          {
            OnCreateAllThumbs(item.Path, false, true);
          }
          break;
        case 200048: // Regenerate Thumbnails
          if (item.IsFolder)
          {
            OnCreateAllThumbs(item.Path, true, true);
          }
          break;
        case 457: // Test change view
          OnSwitchView();
          break;
        case 831:
          string message;
          if (!RemovableDriveHelper.EjectDrive(item.Path, out message))
          {
            GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
            pDlgOK.SetHeading(831);
            pDlgOK.SetLine(1, GUILocalizeStrings.Get(832));
            pDlgOK.SetLine(2, string.Empty);
            pDlgOK.SetLine(3, message);
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          }
          else
          {
            GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
            pDlgOK.SetHeading(831);
            pDlgOK.SetLine(1, GUILocalizeStrings.Get(833));
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;
      }
    }

    #endregion

    #region listview management

    private bool ViewByIcon
    {
      get
      {
        if (mapSettings.ViewAs != (int)View.List)
        {
          return true;
        }
        return false;
      }
    }

    private bool ViewByLargeIcon
    {
      get
      {
        if (mapSettings.ViewAs == (int)View.BigIcons)
        {
          return true;
        }
        return false;
      }
    }

    private GUIListItem GetSelectedItem()
    {
      return facadeView.SelectedListItem;
    }

    private GUIListItem GetItem(int itemIndex)
    {
      if (itemIndex >= facadeView.Count || itemIndex < 0)
      {
        return null;
      }
      return facadeView[itemIndex];
    }

    private int GetSelectedItemNo()
    {
      return facadeView.SelectedListItemIndex;
    }

    private int GetItemCount()
    {
      return facadeView.Count;
    }

    private void UpdateButtonStates()
    {
      GUIControl.HideControl(GetID, facadeView.GetID);
      int iControl = facadeView.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);

      string textLine = string.Empty;
      View view = (View)mapSettings.ViewAs;
      SortMethod method = (SortMethod)mapSettings.SortBy;
      bool sortAsc = mapSettings.SortAscending;
      switch (view)
      {
        case View.List:
          textLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          textLine = GUILocalizeStrings.Get(100);
          break;
        case View.BigIcons:
          textLine = GUILocalizeStrings.Get(417);
          break;
        case View.Albums:
          textLine = GUILocalizeStrings.Get(417);
          break;
        case View.Filmstrip:
          textLine = GUILocalizeStrings.Get(733);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnViewAs.GetID, textLine);

      switch (method)
      {
        case SortMethod.Name:
          textLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.Date:
          textLine = GUILocalizeStrings.Get(104);
          break;
        case SortMethod.Size:
          textLine = GUILocalizeStrings.Get(105);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, textLine);
      btnSortBy.IsAscending = sortAsc;
    }

    private void ShowThumbPanel()
    {
      int itemIndex = GetSelectedItemNo();
      if (mapSettings.ViewAs == (int)View.BigIcons)
      {
        facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (mapSettings.ViewAs == (int)View.Albums)
      {
        facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (mapSettings.ViewAs == (int)View.Icons)
      {
        facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (mapSettings.ViewAs == (int)View.List)
      {
        facadeView.View = GUIFacadeControl.ViewMode.List;
      }
      else if (mapSettings.ViewAs == (int)View.Filmstrip)
      {
        facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (itemIndex > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);
      }
      UpdateButtonStates();
    }

    /// <summary>
    /// Set the selected item of the facadeview
    /// </summary>
    public void SetSelectedItemIndex(int index)
    {
      selectedItemIndex = CountOfNonImageItems + index;
    }

    #endregion

    #region folder settings

    /// <summary>
    /// Checks whether thumb creation had already happenend for the given path
    /// </summary>
    /// <param name="aPath">A folder with images</param>
    /// <returns>Whether the thumbnailcacher needs to proceed on this path</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private static bool CheckPathForHistory(string aPath, bool aRemoveIt)
    {
      if (!thumbCreationPaths.Contains(aPath))
      {
        if (!aRemoveIt)
        {
          thumbCreationPaths.Add(aPath);
        }
        return true;
      }
      else
      {
        if (aRemoveIt)
        {
          try
          {
            thumbCreationPaths.Remove(aPath);
          }
          catch (Exception)
          {
          }
        }
        //Log.Debug("GUIPictures: MissingThumbCacher already working on path {0}", aPath);
        return false;
      }
    }

    private void LoadFolderSettings(string folderName)
    {
      if (folderName == string.Empty)
      {
        folderName = "root";
      }
      object o;
      FolderSettings.GetFolderSetting(folderName, "Pictures", typeof(MapSettings), out o);
      if (o != null)
      {
        mapSettings = o as MapSettings;
        if (mapSettings == null)
        {
          mapSettings = new MapSettings();
        }
      }
      else
      {
        Share share = virtualDirectory.GetShare(folderName);
        if (share != null)
        {
          if (mapSettings == null)
          {
            mapSettings = new MapSettings();
          }
          mapSettings.ViewAs = (int)share.DefaultView;
        }
      }
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        if (xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false))
        {
          xmlreader.SetValue("pictures", "lastfolder", folderName);
        }
      }
    }

    private void SaveFolderSettings(string folder)
    {
      if (folder == string.Empty)
      {
        folder = "root";
      }
      FolderSettings.AddFolderSetting(folder, "Pictures", typeof(MapSettings), mapSettings);
    }

    #endregion

    #region Sort Members

    private void OnSort()
    {
      facadeView.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }

      string sizeItem1 = string.Empty;
      string sizeItem2 = string.Empty;
      if (item1.FileInfo != null && !item1.IsFolder)
      {
        sizeItem1 = Util.Utils.GetSize(item1.FileInfo.Length);
      }
      if (item2.FileInfo != null && !item1.IsFolder)
      {
        sizeItem2 = Util.Utils.GetSize(item2.FileInfo.Length);
      }

      SortMethod method = (SortMethod)mapSettings.SortBy;
      bool sortAsc = mapSettings.SortAscending;

      switch (method)
      {
        case SortMethod.Name:
          item1.Label2 = sizeItem1;
          item2.Label2 = sizeItem2;

          if (sortAsc)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }


        case SortMethod.Date:
          if (item1.FileInfo == null)
          {
            return -1;
          }
          if (item2.FileInfo == null)
          {
            return -1;
          }

          item1.Label2 = item1.FileInfo.ModificationTime.ToShortDateString() + " " +
                         item1.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.ModificationTime.ToShortDateString() + " " +
                         item2.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          if (sortAsc)
          {
            return DateTime.Compare(item1.FileInfo.ModificationTime, item2.FileInfo.ModificationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.ModificationTime, item1.FileInfo.ModificationTime);
          }

        case SortMethod.Size:
          if (item1.FileInfo == null)
          {
            return -1;
          }
          if (item2.FileInfo == null)
          {
            return -1;
          }
          item1.Label2 = sizeItem1;
          item2.Label2 = sizeItem2;
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

    private void OnRetrieveCoverArt(GUIListItem item)
    {
      if (item.IsRemote)
      {
        return;
      }
      Util.Utils.SetDefaultIcons(item);
      if (!item.IsFolder)
      {
        Util.Utils.SetThumbnails(ref item);
        string thumbnailImage = GetThumbnail(item.Path);
        item.IconImage = thumbnailImage;
        if (_autocreateLargeThumbs)
        {
          string thumbnailLargeImage = GetLargeThumbnail(item.Path);
          item.ThumbnailImage = thumbnailLargeImage;
        }
        else
        {
          item.ThumbnailImage = thumbnailImage;
        }
      }
      else
      {
        if (item.Label != "..")
        {
          int pin;
          if (!virtualDirectory.IsProtectedShare(item.Path, out pin))
          {
            Util.Utils.SetThumbnails(ref item);
          }
        }
      }
    }

    private void OnDeleteItem(GUIListItem item)
    {
      if (item.IsRemote)
      {
        return;
      }

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      string strFileName = Path.GetFileName(item.Path);
      if (!item.IsFolder)
      {
        dlgYesNo.SetHeading(664);
      }
      else
      {
        dlgYesNo.SetHeading(503);
      }
      dlgYesNo.SetLine(1, strFileName);
      dlgYesNo.SetLine(2, string.Empty);
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      DoDeleteItem(item);

      selectedItemIndex = GetSelectedItemNo();
      if (selectedItemIndex > 0)
      {
        selectedItemIndex--;
      }
      LoadDirectory(currentFolder);
      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
      }
    }

    private void DoDeleteItem(GUIListItem item)
    {
      if (item.IsFolder)
      {
        if (item.Label != "..")
        {
          List<GUIListItem> items = new List<GUIListItem>();
          items = virtualDirectory.GetDirectoryUnProtectedExt(item.Path, false);
          foreach (GUIListItem subItem in items)
          {
            DoDeleteItem(subItem);
          }
          Util.Utils.DirectoryDelete(item.Path);
        }
      }
      else if (!item.IsRemote)
      {
        Util.Utils.FileDelete(item.Path);
      }
    }

    private void OnInfo(int itemNumber)
    {
      GUIListItem item = GetItem(itemNumber);
      if (item == null)
      {
        return;
      }
      if (item.IsFolder || item.IsRemote)
      {
        return;
      }
      GUIDialogExif exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_EXIF);
      exifDialog.FileName = item.Path;
      exifDialog.DoModal(GetID);
      // Fix for Mantis issue: 0001709: Background not correct after viewing pictures properties twice
      exifDialog.Restore();
    }

    private void OnRotatePicture()
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
      {
        return;
      }
      if (item.IsFolder)
      {
        return;
      }
      if (item.IsRemote)
      {
        return;
      }
      DoRotatePicture(item.Path);
      GUIControl.RefreshControl(GetID, facadeView.GetID);
    }

    public static void DoRotatePicture(string aPicturePath)
    {
      int rotate = 0;
      rotate = PictureDatabase.GetRotation(aPicturePath);
      rotate++;
      if (rotate >= 4)
      {
        rotate = 0;
      }
      PictureDatabase.SetRotation(aPicturePath, rotate);

      try
      {
        // Delete thumbs with "old" rotation so they'll be recreated later
        string thumbnailImage = GetThumbnail(aPicturePath);
        string thumbnailImageLarge = GetLargeThumbnail(aPicturePath);
        File.Delete(thumbnailImage);
        File.Delete(thumbnailImageLarge);
        // make sure there's no conflicting access
        CheckPathForHistory(aPicturePath, true);

        if (Util.Picture.CreateThumbnail(aPicturePath, thumbnailImage, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, rotate, Thumbs.SpeedThumbsSmall))
        {
          Thread.Sleep(10);
          if (Util.Picture.CreateThumbnail(aPicturePath, thumbnailImageLarge, (int)Thumbs.ThumbLargeResolution,
                                                       (int)Thumbs.ThumbLargeResolution, rotate,
                                                       Thumbs.SpeedThumbsLarge))
            Log.Debug("GUIPictures: Recreation of thumbnails after rotation successful for {0}", aPicturePath);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: Error recreating thumbnails after rotation of {0} - {1}", aPicturePath, ex.ToString());
      }      
    }

    private void OnClick(int itemIndex)
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
      {
        return;
      }
      if (item.IsFolder)
      {
        selectedItemIndex = -1;
        LoadDirectory(item.Path);
      }
      else
      {
        if (virtualDirectory.IsRemote(item.Path))
        {
          if (!virtualDirectory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
          {
            if (!virtualDirectory.ShouldWeDownloadFile(item.Path))
            {
              return;
            }
            if (!virtualDirectory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
            {
              //show message that we are unable to download the file
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
              msg.Param1 = 916;
              msg.Param2 = 920;
              msg.Param3 = 0;
              msg.Param4 = 0;
              GUIWindowManager.SendMessage(msg);

              return;
            }
          }
          return;
        }

        selectedItemIndex = GetSelectedItemNo();
        OnShowPicture(item.Path);
      }
    }

    private void OnQueueItem(int itemIndex)
    {
    }

    private void OnShowPicture(string strFile)
    {
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
      {
        return;
      }


      SlideShow.Reset();
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (!item.IsFolder)
        {
          if (item.IsRemote)
          {
            continue;
          }
          SlideShow.Add(item.Path);
        }
      }
      if (SlideShow.Count > 0)
      {
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SLIDESHOW);
        SlideShow.Select(strFile);
      }
    }

    private void AddDir(GUISlideShow SlideShow, string strDir)
    {
      List<GUIListItem> itemlist = virtualDirectory.GetDirectoryExt(strDir);
      Filter(ref itemlist);
      foreach (GUIListItem item in itemlist)
      {
        if (item.IsFolder)
        {
          if (item.Label != "..")
          {
            AddDir(SlideShow, item.Path);
          }
        }
        else if (!item.IsRemote)
        {
          SlideShow.Add(item.Path);
        }
      }
    }

    private void OnSlideShowRecursive()
    {
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
      {
        return;
      }

      SlideShow.Reset();
      if (disp == Display.Files)
      {
        AddDir(SlideShow, currentFolder);
      }
      else
      {
        List<string> pics = new List<string>();
        int totalCount = PictureDatabase.ListPicsByDate(currentFolder.Replace("\\", "-"), ref pics);
        foreach (string pic in pics)
        {
          SlideShow.Add(pic);
        }
      }
      if (SlideShow.Count > 0)
      {
        SlideShow.StartSlideShow(currentFolder);
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SLIDESHOW);
      }
    }

    private void OnSlideShow()
    {
      OnSlideShow(0);
    }

    private void OnSlideShow(int iStartItem)
    {
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
      {
        return;
      }

      SlideShow.Reset();

      if ((iStartItem < 0) || (iStartItem > GetItemCount()))
      {
        iStartItem = 0;
      }
      int i = iStartItem;
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
          i = 0;
        }
      } while (i != iStartItem);

      if (SlideShow.Count > 0)
      {
        SlideShow.StartSlideShow(currentFolder);
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SLIDESHOW);
      }
    }

    private void CreateAllThumbs(string strDir, bool Regenerate, bool Recursive)
    {
      // int Count = 0;
      if (disp == Display.Files)
      {
        MissingThumbCacher ManualThumbBuilder = new MissingThumbCacher(strDir, _autocreateLargeThumbs, Regenerate);
      }
      else if (disp == Display.Date)
      {
        // TODO: Thumbworker alternative on file base instead of directory
      }
    }

    private void OnCreateAllThumbs(string strDir, bool Regenerate, bool Recursive)
    {
      CreateAllThumbs(strDir, Regenerate, Recursive);

      GUITextureManager.CleanupThumbs();
      GUIWaitCursor.Hide();

      LoadDirectory(currentFolder);
    }

    private void OnShowSortMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(104); // date
      dlg.AddLocalizedString(105); // size

      // set the focus to currently used sort method
      dlg.SelectedLabel = mapSettings.SortBy;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 103:
          mapSettings.SortBy = (int)SortMethod.Name;
          break;
        case 104:
          mapSettings.SortBy = (int)SortMethod.Date;
          break;
        case 105:
          mapSettings.SortBy = (int)SortMethod.Size;
          break;
        default:
          mapSettings.SortBy = (int)SortMethod.Name;
          break;
      }

      OnSort();
      GUIControl.FocusControl(GetID, btnSortBy.GetID);
    }

    private void OnShowFileMenu()
    {
      GUIListItem item = selectedListItem;
      if (item == null)
      {
        return;
      }
      if (item.IsFolder && item.Label == "..")
      {
        return;
      }

      // init
      GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_FILE);
      if (dlgFile == null)
      {
        return;
      }

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
        if (selectedItemIndex >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
        }
      }

      dlgFile.DeInit();
      dlgFile = null;
    }

    private void OnSwitchView()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(499); // Views menu

      dlg.AddLocalizedString(134); // Shares
      dlg.AddLocalizedString(636); // date

      // set the focus to currently used view
      dlg.SelectedLabel = (int)disp;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 134:
          if (disp != Display.Files)
          {
            disp = Display.Files;
            LoadDirectory(m_strDirectoryStart);
          }
          break;
        case 636:
          if (disp != Display.Date)
          {
            disp = Display.Date;
            LoadDirectory("");
          }
          break;
      }

      GUIControl.FocusControl(GetID, btnSwitchView.GetID);
    }

    #endregion

    #region various

    /// <summary>
    /// Returns true if the specified window should maintain virtual directory
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: if the specified window should maintain virtual directory
    /// false: if the specified window should not maintain virtual directory</returns>
    public static bool KeepVirtualDirectory(int windowId)
    {
      if (windowId == (int)Window.WINDOW_PICTURES)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_SLIDESHOW)
      {
        return true;
      }
      return false;
    }

    private static bool ContainsFolderThumb(GUIListItem aItem)
    {
      if (!aItem.IsFolder && aItem.Path.Contains(@"folder.jpg"))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public static void Filter(ref List<GUIListItem> itemlist)
    {
      itemlist.RemoveAll(ContainsFolderThumb);
    }

    private void LoadDirectory(string strNewDirectory)
    {
      List<GUIListItem> itemlist;
      string objectCount = string.Empty;

      GUIWaitCursor.Show();

      GUIListItem SelectedItem = GetSelectedItem();
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          folderHistory.Set(SelectedItem.Label, currentFolder);
        }
      }

      if (strNewDirectory != currentFolder && mapSettings != null)
      {
        SaveFolderSettings(currentFolder);
      }

      if (strNewDirectory != currentFolder || mapSettings == null)
      {
        LoadFolderSettings(strNewDirectory);
      }

      currentFolder = strNewDirectory;

      GUIControl.ClearControl(GetID, facadeView.GetID);

      if (disp == Display.Files)
      {
        itemlist = virtualDirectory.GetDirectoryExt(currentFolder);
        Filter(ref itemlist);
        MissingThumbCacher ThumbWorker = new MissingThumbCacher(currentFolder, _autocreateLargeThumbs, false);
        // int itemIndex = 0;
        CountOfNonImageItems = 0;
        foreach (GUIListItem item in itemlist)
        {
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeView.Add(item);

          if (item.IsFolder)
          {
            CountOfNonImageItems++; // necessary to select the right item later from the slideshow
          }
        }

        OnSort();
      }
      else
      {
        LoadDateView(strNewDirectory);
      }

      int totalItemCount = facadeView.Count;
      string strSelectedItem = folderHistory.Get(currentFolder);
      for (int i = 0; i < totalItemCount; i++)
      {
        if (facadeView[i].Label == strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, i);
          break;
        }
      }
      if (totalItemCount > 0)
      {
        GUIListItem rootItem = (GUIListItem)facadeView[0];
        if (rootItem.Label == "..")
        {
          totalItemCount--;
        }
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItemCount));

      ShowThumbPanel();

      GUIWaitCursor.Hide();
    }

    private void LoadDateView(string strNewDirectory)
    {
      CountOfNonImageItems = 0;
      if (strNewDirectory == "")
      {
        // Years
        List<string> Years = new List<string>();
        int Count = PictureDatabase.ListYears(ref Years);
        foreach (string year in Years)
        {
          GUIListItem item = new GUIListItem(year);
          item.Label = year;
          Log.Info("Load Year: " + year);
          item.Path = year;
          item.IsFolder = true;
          Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeView.Add(item);
          CountOfNonImageItems++; // necessary to select the right item later from the slideshow
        }
      }
      else if (strNewDirectory.Length == 4)
      {
        // Months
        string year = strNewDirectory.Substring(0, 4);
        GUIListItem item = new GUIListItem("..");
        item.Path = "";
        item.IsFolder = true;
        Util.Utils.SetDefaultIcons(item);
        item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
        CountOfNonImageItems++; // necessary to select the right item later from the slideshow

        List<string> Months = new List<string>();
        int Count = PictureDatabase.ListMonths(year, ref Months);
        foreach (string month in Months)
        {
          item = new GUIListItem(month);
          item.Path = year + "\\" + month;
          item.IsFolder = true;
          Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeView.Add(item);
          CountOfNonImageItems++; // necessary to select the right item later from the slideshow
        }
        List<string> pics = new List<string>();
        int PicCount = PictureDatabase.CountPicsByDate(year);
        if (PicCount <= MAX_PICS_PER_DATE)
        {
          Count += PictureDatabase.ListPicsByDate(year, ref pics);
          foreach (string pic in pics)
          {
            item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
            item.Path = pic;
            item.IsFolder = false;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            item.FileInfo = new FileInformation(pic, false);
            facadeView.Add(item);
          }
        }
      }
      else if (strNewDirectory.Length == 7)
      {
        // Days
        string year = strNewDirectory.Substring(0, 4);
        string month = strNewDirectory.Substring(5, 2);
        GUIListItem item = new GUIListItem("..");
        item.Path = year;
        item.IsFolder = true;
        Util.Utils.SetDefaultIcons(item);
        item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
        CountOfNonImageItems++; // necessary to select the right item later from the slideshow

        List<string> Days = new List<string>();
        int Count = PictureDatabase.ListDays(month, year, ref Days);
        foreach (string day in Days)
        {
          item = new GUIListItem(day);
          item.Path = year + "\\" + month + "\\" + day;
          item.IsFolder = true;
          Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeView.Add(item);
          CountOfNonImageItems++; // necessary to select the right item later from the slideshow
        }
        List<string> pics = new List<string>();
        int PicCount = PictureDatabase.CountPicsByDate(year + "-" + month);
        if (PicCount <= MAX_PICS_PER_DATE)
        {
          Count += PictureDatabase.ListPicsByDate(year + "-" + month, ref pics);
          foreach (string pic in pics)
          {
            item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
            item.Path = pic;
            item.IsFolder = false;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            item.FileInfo = new FileInformation(pic, false);
            facadeView.Add(item);
          }
        }
      }
      else if (strNewDirectory.Length == 10)
      {
        // Pics from one day
        string year = strNewDirectory.Substring(0, 4);
        string month = strNewDirectory.Substring(5, 2);
        string day = strNewDirectory.Substring(8, 2);
        GUIListItem item = new GUIListItem("..");
        item.Path = year + "\\" + month;
        item.IsFolder = true;
        Util.Utils.SetDefaultIcons(item);
        item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
        CountOfNonImageItems++; // necessary to select the right item later from the slideshow

        List<string> pics = new List<string>();
        int Count = PictureDatabase.ListPicsByDate(year + "-" + month + "-" + day, ref pics);
        foreach (string pic in pics)
        {
          item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
          item.Path = pic;
          item.IsFolder = false;
          Util.Utils.SetDefaultIcons(item);
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          item.FileInfo = new FileInformation(pic, false);
          facadeView.Add(item);
        }
      }
      if (facadeView.Count == 0 && strNewDirectory != "")
      {
        // Wrong path for date view, go back to top level
        currentFolder = "";
        LoadDateView(currentFolder);
      }
    }

    public static string GetThumbnail(string fileName)
    {
      if (fileName == string.Empty)
      {
        return string.Empty;
      }
      return String.Format(@"{0}\{1}.jpg", Thumbs.Pictures, Util.Utils.EncryptLine(fileName));
    }

    public static string GetLargeThumbnail(string fileName)
    {
      if (fileName == string.Empty)
      {
        return string.Empty;
      }
      return String.Format(@"{0}\{1}L.jpg", Thumbs.Pictures, Util.Utils.EncryptLine(fileName));
    }

    private bool GetUserInputString(ref string sString)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Text = sString;
      keyboard.DoModal(GetID); // show it...
      if (keyboard.IsConfirmed)
      {
        sString = keyboard.Text;
      }
      return keyboard.IsConfirmed;
    }

    #endregion

    #region callback events

    public bool ThumbnailCallback()
    {
      return false;
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
      {
        return;
      }
      string thumbnailImage = GetLargeThumbnail(item.Path);
      if (File.Exists(thumbnailImage))
      {
        filmstrip.InfoImageFileName = thumbnailImage;
      }
      //UpdateButtonStates();  -> fixing mantis bug 902
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      mapSettings.SortAscending = e.Order != SortOrder.Descending;

      OnSort();

      GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
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
      return "Pictures";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      // TODO:  Add GUIPictures.GetHome implementation
      strButtonText = GUILocalizeStrings.Get(1);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my pictures.png";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Watch your photos and slideshows with MediaPortal";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIPictures.ShowPlugin implementation
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}