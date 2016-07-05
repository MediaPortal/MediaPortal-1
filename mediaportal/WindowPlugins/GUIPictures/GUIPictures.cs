#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Common.GUIPlugins;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Picture.Database;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.Util;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using ThreadPool = System.Threading.ThreadPool;

namespace MediaPortal.GUI.Pictures
{
  /// <summary>
  /// Displays pictures and offers methods for exif and rotation
  /// </summary>
  [PluginIcons("GUIPictures.Pictures.gif", "GUIPictures.PicturesDisabled.gif")]
  public class GUIPictures : WindowPluginBase, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {
    #region ThumbCacher class

    public class MissingThumbCacher
    {
      private VirtualDirectory vDir = new VirtualDirectory();

      private string _filepath = string.Empty;
      private bool _createLarge = true;
      private bool _recreateWithoutCheck = false;
      private Work work;

      public MissingThumbCacher(string Filepath, bool CreateLargeThumbs, bool ReCreateThumbs, bool Thread)
      {
        _filepath = Filepath;
        _createLarge = CreateLargeThumbs;
        _recreateWithoutCheck = ReCreateThumbs;

        if (Thread)
        {
          work = new Work(new DoWorkHandler(this.PerformRequest));
          work.ThreadPriority = ThreadPriority.Lowest;
          GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
        }
        else
        {
          PerformRequest();
        }
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
        List<GUIListItem> itemlist = null;

        vDir.SetExtensions(Util.Utils.PictureExtensions);

        if (!vDir.IsRemote(path))
        {
          itemlist = vDir.GetDirectoryUnProtectedExt(path, true);

          itemlist.Sort(new PictureSort(PictureSort.SortMethod.Name, true));

          foreach (GUIListItem item in itemlist)
          {
            if (ThumbnailsThreadAbort)
            {
              return;
            }

            while (g_Player.Playing || g_Player.Starting)
            {
              Thread.Sleep(5000);
              Log.Debug("RefreshThumbnailsThread: g_Player is Playing, waiting for the end.");
            }

            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
            {
              return;
            }
            if (String.IsNullOrEmpty(item.Path))
            {
              continue;
            }
            if (path.Length >= item.Path.Length)
            {
              Log.Warn("GUIPictures: Omitting outside path {0} from check share {1}", item.Path, path);
              continue;
            }
            Thread.Sleep(5);

            if (CheckPathForHistory(item.Path, false))
            {
              if (!item.IsFolder)
              {
                int iRotate = PictureDatabase.GetRotation(item.Path);
                Thread.Sleep(5);

                bool isPicture = Util.Utils.IsPicture(item.Path);
                bool thumbRet = false;

                if (!item.IsRemote && isPicture)
                {
                  string thumbnailImage = null;
                  string thumbnailImageL = null;

                  if (isPicture)
                  {
                    thumbnailImage = Util.Utils.GetPicturesThumbPathname(item.Path);
                    thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item.Path);

                    if (recreateThumbs || !File.Exists(thumbnailImage) || !Util.Utils.FileExistsInCache(thumbnailImage) ||
                        !File.Exists(thumbnailImageL) || !Util.Utils.FileExistsInCache(thumbnailImageL))
                    {
                      Thread.Sleep(5);

                      if (autocreateLargeThumbs && (!File.Exists(thumbnailImageL) || recreateThumbs))
                      {
                        thumbRet = Util.Picture.ReCreateThumbnail(item.Path, thumbnailImageL,
                                                                (int) Thumbs.ThumbLargeResolution,
                                                                (int) Thumbs.ThumbLargeResolution, iRotate,
                                                                Thumbs.SpeedThumbsLarge,
                                                                true, false, recreateThumbs);
                      }
                      if (!File.Exists(thumbnailImage) || recreateThumbs)
                      {
                        thumbRet = Util.Picture.ReCreateThumbnail(item.Path, thumbnailImage, (int) Thumbs.ThumbResolution,
                                                                (int) Thumbs.ThumbResolution, iRotate,
                                                                Thumbs.SpeedThumbsSmall,
                                                                false, false, recreateThumbs);
                      }
                    }

                    if (thumbRet && (autocreateLargeThumbs || recreateThumbs))
                    {
                      item.ThumbnailImage = thumbnailImageL;
                      item.IconImage = thumbnailImage;
                      Log.Debug("GUIPictures: Creation of large thumb successful for {0}", item.Path);
                    }
                    else if (thumbRet || recreateThumbs)
                    {
                      item.ThumbnailImage = thumbnailImage;
                      item.IconImage = thumbnailImage;
                      Log.Debug("GUIPictures: Creation of thumb successful for {0}", item.Path);
                    }
                  }
                }
              }
              else
              {
                string pin;
                if ((item.Label != "..") && (!vDir.IsProtectedShare(item.Path, out pin)))
                {
                  string thumbnailImage = item.Path + @"\folder.jpg";
                  if (recreateThumbs || (!item.IsRemote && !Util.Utils.FileExistsInCache(thumbnailImage)))
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
            }
          }
        }
        benchclock.Stop();
        Log.Debug("GUIPictures: Creation of all thumbs for dir '{0}' took {1} seconds for {2} files", _filepath,
                  benchclock.Elapsed.TotalSeconds, itemlist.Count);
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

        // No picture in the folder. Try to find in the subfolders.
        Log.Debug("CreateFolderThumb: No picture in the {0}", path);
        if (pictureList.Count == 0)
        {
          foreach (GUIListItem subitem in itemlist)
          {
            if (subitem.IsFolder && subitem.Path.Length > path.Length)
            {
              List<GUIListItem> subFolderList = vDir.GetDirectoryUnProtectedExt(subitem.Path, true);
              if (!recreateAll)
              {
                Filter(ref subFolderList);
              }
              foreach (GUIListItem subFolderItem in subFolderList)
              {
                if (!subFolderItem.IsFolder && !subFolderItem.IsRemote && Util.Utils.IsPicture(subFolderItem.Path))
                {
                  pictureList.Add(subFolderItem.Path);
                  Log.Debug("CreateFolderThumb: Add file to folder.jpg {0}", subFolderItem.Path);
                  break;
                }
              }
            }
            if (pictureList.Count >= 4)
            {
              break;
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

    /// <summary>
    /// creates cached thumbs in MP's thumbs dir
    /// </summary>
    private void GetThumbnailfile(ref GUIListItem itemObject)
    {
      Thread.CurrentThread.Name = "GUIPictures Thumbnail";
      Stopwatch benchclockfile = new Stopwatch();
      VirtualDirectory vDir = new VirtualDirectory();
      benchclockfile.Start();
      string item = itemObject.Path;
      bool autocreateLargeThumbs = _autocreateLargeThumbs;
      bool recreateThumbs = false;

      vDir.SetExtensions(Util.Utils.PictureExtensions);

      if (!vDir.IsRemote(item))
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          return;
        }
        if (String.IsNullOrEmpty(item))
        {
          return;
        }

        int iRotate = PictureDatabase.GetRotation(item);
        Thread.Sleep(5);

        bool isVideo = Util.Utils.IsVideo(item);
        bool isPicture = Util.Utils.IsPicture(item);
        bool thumbRet = false;

        if (isPicture)
        {
          string thumbnailImage;
          string thumbnailImageL = null;
          thumbnailImage = Util.Utils.GetPicturesThumbPathname(item);
          thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item);

          if (recreateThumbs || !File.Exists(thumbnailImage) || !Util.Utils.FileExistsInCache(thumbnailImage) ||
              !File.Exists(thumbnailImageL) || !Util.Utils.FileExistsInCache(thumbnailImageL))
          {
            if (autocreateLargeThumbs && !File.Exists(thumbnailImageL))
            {
              thumbRet = Util.Picture.CreateThumbnail(item, thumbnailImageL, (int) Thumbs.ThumbLargeResolution,
                                                      (int) Thumbs.ThumbLargeResolution, iRotate,
                                                      Thumbs.SpeedThumbsLarge,
                                                      true, false);
            }
            if (!File.Exists(thumbnailImage))
            {
              thumbRet = Util.Picture.CreateThumbnail(item, thumbnailImage, (int) Thumbs.ThumbResolution,
                                                      (int) Thumbs.ThumbResolution, iRotate,
                                                      Thumbs.SpeedThumbsSmall,
                                                      false, false);
            }

            if (thumbRet && autocreateLargeThumbs)
            {
              itemObject.ThumbnailImage = thumbnailImageL;
              itemObject.IconImage = thumbnailImage;
              Log.Debug("GUIPictures: Creation of missing large thumb successful for {0}", item);
            }
            else if (thumbRet)
            {
              itemObject.ThumbnailImage = thumbnailImage;
              itemObject.IconImage = thumbnailImage;
              Log.Debug("GUIPictures: Creation of missing thumb successful for {0}", item);
            }
          }
        }
        if (thumbRet)
        {
          benchclockfile.Stop();
          Log.Debug("GUIPictures: Creation of selected thumb process file for '{0}' took {1} seconds", item,
                    benchclockfile.Elapsed.TotalSeconds);
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
        _ViewAs = (int)Layout.SmallIcons;
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
      Modified = 1,
      Created = 2,
      Size = 3
    }

    private enum Display
    {
      Files = 0,
      Date = 1
    }

    [SkinControl(6)] protected GUIButtonControl btnSlideShow = null;
    [SkinControl(7)] protected GUIButtonControl btnSlideShowRecursive = null;

    private const int MAX_PICS_PER_DATE = 1000;
    private PicturesFolderWatcherHelper _pictureFolderWatcher;
    public static HashSet<string> thumbCreationPaths = new HashSet<string>();
    private int selectedItemIndex = -1;
    private GUIListItem selectedListItem = null;
    private DirectoryHistory folderHistory = new DirectoryHistory();
    private static string currentFolder = string.Empty;
    private string m_strDirectoryStart = string.Empty;
    private string destinationFolder = string.Empty;
    private static VirtualDirectory _virtualDirectory;
    private MapSettings mapSettings = new MapSettings();
    private bool isFileMenuEnabled = false;
    private string fileMenuPinCode = string.Empty;
    private bool _autocreateLargeThumbs = true;
    private bool _useDayGrouping = false;
    private bool _enableVideoPlayback = false;
    public bool _playVideosInSlideshows = false;
    public bool _tempLeaveThumbsInFolder = false;
    //bool _hideExtensions = true;
    private Display disp = Display.Files;
    private bool _switchRemovableDrives;
    private int CountOfNonImageItems = 0; // stores the count of items in a folder that are no images (folders etc...)
    public static string fileNameCheck = string.Empty;
    protected PictureSort.SortMethod currentSortMethod = PictureSort.SortMethod.Name;
    public static List<string> _thumbnailFolderItem = new List<string>();
    private static string _prevServerName = string.Empty;
    private static DateTime _prevWolTime;
    private static int _wolTimeout;
    private static int _wolResendTime;
    private static bool returnFromSlideshow = false;
    private bool _autoShuffle = false;
    private bool _ageConfirmed = false;
    private ArrayList _protectedShares = new ArrayList();
    private string _currentPin = string.Empty;
    private ArrayList _currentProtectedShare = new ArrayList();
    private Thread _refreshThumbnailsThread;
    private GUIDialogProgress _progressDialogForRefreshThumbnails;
    private static bool _refreshThumbnailsThreadAbort = false;
    private static Thread _removableDrivesHandlerThread;
    private static bool _autoCreateThumbs = true;

    #endregion

    #region ctor/dtor

    public GUIPictures()
    {
      GetID = (int)Window.WINDOW_PICTURES;
    }

    #endregion

    #region Serialisation

    protected override void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _autocreateLargeThumbs = !xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", false);
        _autoCreateThumbs = xmlreader.GetValueAsBool("thumbnails", "pictureAutoCreateThumbs", true);
        _useDayGrouping = xmlreader.GetValueAsBool("pictures", "useDayGrouping", false);
        _enableVideoPlayback = xmlreader.GetValueAsBool("pictures", "enableVideoPlayback", false);
        _playVideosInSlideshows = xmlreader.GetValueAsBool("pictures", "playVideosInSlideshows", false);
        isFileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        fileMenuPinCode = Util.Utils.DecryptPassword(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));
        //string strDefault = xmlreader.GetValueAsString("pictures", "default", string.Empty);
        _wolTimeout = xmlreader.GetValueAsInt("WOL", "WolTimeout", 10);
        _wolResendTime = xmlreader.GetValueAsInt("WOL", "WolResendTime", 1);
        _virtualDirectory = VirtualDirectories.Instance.Pictures;
        _autoShuffle = xmlreader.GetValueAsBool("pictures", "autoShuffle", false);
        
        if (currentFolder == string.Empty)
        {
          if (_virtualDirectory.DefaultShare != null)
          {
            if (_virtualDirectory.DefaultShare.IsFtpShare)
            {
              //remote:hostname?port?login?password?folder
              currentFolder = _virtualDirectory.GetShareRemoteURL(_virtualDirectory.DefaultShare);
              m_strDirectoryStart = currentFolder;
            }
            else
            {
              currentFolder = _virtualDirectory.DefaultShare.Path;
              m_strDirectoryStart = _virtualDirectory.DefaultShare.Path;
            }
          }
        }

        if (xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false))
        {
          string lastFolder = xmlreader.GetValueAsString("pictures", "lastfolder", currentFolder);
          if (lastFolder != "root")
          {
            currentFolder = lastFolder;
          }
          disp = (Display)xmlreader.GetValueAsInt("pictures", "lastview", (int)disp);
        }
        _switchRemovableDrives = xmlreader.GetValueAsBool("pictures", "SwitchRemovableDrives", true);
        //_hideExtensions = xmlreader.GetValueAsBool("gui", "hideextensions", true);
      }

      if (currentFolder.Length > 0 && currentFolder == m_strDirectoryStart)
      {
        VirtualDirectory vDir = new VirtualDirectory();
        vDir.LoadSettings("pictures");
        string pincode = string.Empty;
        bool FolderPinProtected = vDir.IsProtectedShare(currentFolder, out pincode);
        if (FolderPinProtected)
        {
          currentFolder = string.Empty;
        }
      }
    }

    protected override void SaveSettings() {}

    #endregion

    #region overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\mypics.xml"));
    }

    public override void DeInit()
    {
      base.DeInit();
      SaveSettings();
    }

    protected override void InitViewSelections()
    {
      btnViews.ClearMenu();

      // Add the view options to the menu.
      int index = 0;
      btnViews.AddItem(GUILocalizeStrings.Get(134), index++); // Shares
      btnViews.AddItem(GUILocalizeStrings.Get(636), index++); // Date

      // Have the menu select the currently selected view.
      btnViews.SetSelectedItemByValue((int)disp);
    }

    public override void OnAdded()
    {
      base.OnAdded();
      currentFolder = string.Empty;
      LoadSettings();
      _virtualDirectory.AddDrives();
      _virtualDirectory.SetExtensions(Util.Utils.PictureExtensions);
      destinationFolder = string.Empty;
      thumbCreationPaths.Clear();
      if (_enableVideoPlayback)
      {
        foreach (string ext in Util.Utils.VideoExtensions)
          _virtualDirectory.AddExtension(ext);
      }
      GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_OnNewMessage);

      _removableDrivesHandlerThread = new Thread(ListRemovableDrives);
      _removableDrivesHandlerThread.IsBackground = true;
      _removableDrivesHandlerThread.Name = "PictureRemovableDrivesHandlerThread";
      _removableDrivesHandlerThread.Start();
    }

    private void ListRemovableDrives()
    {
      RemovableDrivesHandler.ListRemovableDrives(_virtualDirectory.GetDirectoryExt(string.Empty));
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_STOP)
      {
        if (g_Player.IsPicture)
        {
          GUIPictureSlideShow._slideDirection = 0;
        }
      }
      if (action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        if (g_Player.IsPicture)
        {
          GUIPictureSlideShow._slideDirection = 0;
          g_Player.Stop();
        }
      }
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeLayout.Focus)
        {
          GUIListItem item = facadeLayout[0];
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
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
      if (action.wID == Action.ActionType.ACTION_EJECTCD)
      {
        GUIListItem item = facadeLayout.SelectedListItem;
        if (item == null || item.Path == null || Util.Utils.getDriveType(item.Path) != 5)
        {
          Util.Utils.EjectCDROM();
        }
        else
        {
          Util.Utils.EjectCDROM(Path.GetPathRoot(item.Path));
        }

        LoadDirectory(string.Empty);
      }

      base.OnAction(action);
    }

    public override void Process()
    {
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow._enableResumeMusic)
      {
        // Enable only once to deinit stopped Fullscreen Video 
        base.Process();
        SlideShow._enableResumeMusic = false;
      }
      else if (((GUIWindow.Window)(Enum.Parse(typeof(GUIWindow.Window), GUIWindowManager.ActiveWindow.ToString())) == GUIWindow.Window.WINDOW_PICTURES) && !g_Player.Playing )
      {
        if (SlideShow.pausedMusic && SlideShow._returnedFromVideoPlayback && !SlideShow._isBackgroundMusicPlaying)
        {
          SlideShow.resumePausedMusic();
        }
        if (SlideShow._returnedFromVideoPlayback)
        {
          SlideShow._returnedFromVideoPlayback = false;
        }
        if (SlideShow.pausedMusic)
        {
          SlideShow.pausedMusic = false;
        }
        if (SlideShow._isBackgroundMusicPlaying)
        {
          SlideShow._isBackgroundMusicPlaying = false;
        }
      }
    }

    // Get all shares and pins for protected pictures folders
    private void GetProtectedShares(ref ArrayList shares)
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        shares = new ArrayList();

        for (int index = 0; index < 128; index++)
        {
          string sharePin = String.Format("pincode{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePinData = Util.Utils.DecryptPassword(xmlreader.GetValueAsString("pictures", sharePin, string.Empty));
          string sharePathData = xmlreader.GetValueAsString("pictures", sharePath, string.Empty);

          if (!string.IsNullOrEmpty(sharePinData))
          {
            shares.Add(sharePinData + "|" + sharePathData);
          }
        }
      }
    }

    protected override void OnPageLoad()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _tempLeaveThumbsInFolder = xmlreader.GetValueAsBool("thumbnails", "videosharepreview", false);
        xmlreader.SetValueAsBool("thumbnails", "videosharepreview", false);
      }
      base.OnPageLoad();

      if (!PictureDatabase.DbHealth)
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        pDlgOK.SetHeading(315);
        pDlgOK.SetLine(1, string.Empty);
        pDlgOK.SetLine(2, GUILocalizeStrings.Get(190010, new object[] { GUILocalizeStrings.Get(1) }));
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      }
      if (!FolderSettings.DbHealth)
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        pDlgOK.SetHeading(315);
        pDlgOK.SetLine(1, string.Empty);
        pDlgOK.SetLine(2, GUILocalizeStrings.Get(190010, new object[] { GUILocalizeStrings.Get(190011) }));
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
      }

      if (!KeepVirtualDirectory(PreviousWindowId))
      {
        _virtualDirectory.Reset();
      }
      InitViewSelections();
      UpdateButtonStates();

      GUITextureManager.CleanupThumbs();
      // LoadSettings();

      if (!IsPictureWindow(PreviousWindowId))
      {
        _ageConfirmed = false;
        _currentPin = string.Empty;
        _currentProtectedShare.Clear();
        _protectedShares.Clear();
        GetProtectedShares(ref _protectedShares);
      }

      LoadFolderSettings(currentFolder);
      ShowThumbPanel();
      LoadDirectory(currentFolder);
      if (selectedItemIndex >= 0 && (PreviousWindowId == (int)Window.WINDOW_SLIDESHOW || PreviousWindowId == (int)Window.WINDOW_PICTURES))
      {
        GUISlideShow SlideShow = (GUISlideShow) GUIWindowManager.GetWindow((int) Window.WINDOW_SLIDESHOW);
        Log.Debug("GUIPictures: currentSlideIndex {0}", SlideShow._currentSlideIndex);
        /*if (SlideShow._currentSlideIndex != -1)
          selectedItemIndex += SlideShow._currentSlideIndex+1;*/
        int direction = GUIPictureSlideShow.SlideDirection;
        GUIPictureSlideShow.SlideDirection = 0;
        g_Player.IsPicture = false;

        if (SlideShow._returnedFromVideoPlayback && !SlideShow._loadVideoPlayback)
        {
          if (direction == 0)
          {
            SlideShow.Reset();
          }
        }

        //forward
        if (direction == 1)
        {
          selectedItemIndex++;
        }
        //Backward
        if (direction == -1)
        {
          selectedItemIndex--;
        }

        //Slide Show 
        if (SlideShow._isSlideShow)
        {
          if (SlideShow._returnedFromVideoPlayback)
          {
            SlideShow._returnedFromVideoPlayback = false;
          }
          OnClickSlideShow(selectedItemIndex);
        }
        //OnClick
        else if (direction != 0)
        {
          if (SlideShow._returnedFromVideoPlayback)
          {
            SlideShow._returnedFromVideoPlayback = false;
          }
          OnClickSlide(selectedItemIndex);
        }

        if (SlideShow._showRecursive)
        {
          SlideShow._showRecursive = false;
        }

        returnFromSlideshow = true;

        // Select latest item played from slideshow/slideshow recursive (random or not)
        if (disp == Display.Files)
        {
          string strSelectedItemExt = Util.Utils.GetFileNameWithExtension(SlideShow._folderCurrentItem);
          string strSelectedItem = Util.Utils.GetFilename(SlideShow._folderCurrentItem, true);
          string directoryName = Path.GetDirectoryName(SlideShow._folderCurrentItem);
          string directoryNameCheck = Path.GetDirectoryName(SlideShow._folderCurrentItem);
          if (selectedItemIndex >= 0 && !String.IsNullOrEmpty(directoryName))
          {
            // Set root directory
            string rootFolder = _virtualDirectory.GetShare(directoryName).Name;
            string rootFolderPath = _virtualDirectory.GetShare(directoryName).Path;
            currentFolder = directoryName;

            while (true)
            {
              if (!string.IsNullOrEmpty(rootFolderPath) && !string.IsNullOrEmpty(directoryNameCheck))
              {
                if (rootFolderPath == directoryNameCheck)
                {
                  folderHistory.Set(strSelectedItem, directoryName);
                  break;
                }
                string sourceFolder = directoryNameCheck;
                if (rootFolderPath != directoryNameCheck)
                {
                  string sourceCurrentFolder = Path.GetDirectoryName(sourceFolder);
                  string destinationItem = Path.GetFileName(sourceFolder);
                  folderHistory.Set(destinationItem, sourceCurrentFolder);
                }
                else
                {
                  string destinationItem = Path.GetFileName(sourceFolder);
                  folderHistory.Set(destinationItem, sourceFolder);
                }
                directoryNameCheck = Path.GetDirectoryName(sourceFolder);
              }
              break;
            }

            folderHistory.Set(rootFolder, "");

            LoadFolderSettings(directoryName);
            LoadDirectory(directoryName);
            int totalItemCount = facadeLayout.Count;
            for (int i = 0; i < totalItemCount; i++)
            {
              if (facadeLayout[i].Label == strSelectedItemExt || facadeLayout[i].Label == strSelectedItem)
              {
                GUIControl.SelectItemControl(GetID, facadeLayout.GetID, i);
                SlideShow._folderCurrentItem = null;
                break;
              }
            }
          }
          else
          {
            GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
          }
          returnFromSlideshow = false;
        }
        else
        {
          bool foundSelectedItem = false;
          int totalItemCount = facadeLayout.Count;
          string strSelectedItem = Util.Utils.GetFilename(SlideShow._folderCurrentItem, true);
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
          for (int i = 0; i < totalItemCount; i++)
          {
            string strSelectedItemcheck = LoadDateViewSelect(facadeLayout[i].Path, SlideShow._folderCurrentItem);
            if (strSelectedItemcheck == SlideShow._folderCurrentItem)
            {
              foundSelectedItem = true;
              break;
            }
          }
          if (foundSelectedItem)
          {
            for (int i = 0; i < facadeLayout.Count; i++)
            {
              if (facadeLayout[i].Label == strSelectedItem)
              {
                GUIControl.SelectItemControl(GetID, facadeLayout.GetID, i);
                break;
              }
            }
          }
          else
          {
            GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
          }
        }
      }
      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      selectedItemIndex = GetSelectedItemNo();
      SaveSettings();
      SaveFolderSettings(currentFolder);

      if (_pictureFolderWatcher != null)
      {
        _pictureFolderWatcher.ChangeMonitoring(false);
      }

      // set back videosharepreview value
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("thumbnails", "videosharepreview", _tempLeaveThumbsInFolder);
      }
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnLayouts)
      {
        mapSettings.ViewAs = (int)currentLayout;
        //ShowThumbPanel();
      }
      else if (control == btnSlideShow) // Slide Show
      {
        OnSlideShow();
      }
      else if (control == btnSlideShowRecursive) // Recursive Slide Show
      {
        OnSlideShowRecursive();
      }
    }

    private void GUIWindowManager_OnNewMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
          if (message.Param1 == (int)Ripper.AutoPlay.MediaType.PHOTO)
          {
            if (message.Param2 == (int)Ripper.AutoPlay.MediaSubType.FILES)
            {
              currentFolder = message.Label;
              OnSlideShowRecursive();
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_ONRESUME:
          using (Settings xmlreader = new MPSettings())
          {
            if (!xmlreader.GetValueAsBool("general", "showlastactivemodule", false))
            {
              currentFolder = string.Empty;
      }
    }

          Log.Debug("{0}:{1}", SerializeName, message.Message);
          break;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_START_SLIDESHOW:
          string strUrl = message.Label;
          LoadDirectory(strUrl);
          OnSlideShow();
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
              _virtualDirectory.AddRemovableDrive(message.Label, message.Label2);
            }
          }
          if (_removableDrivesHandlerThread != null)
          {
            _removableDrivesHandlerThread.Join();
          }
          RemovableDrivesHandler.ListRemovableDrives(_virtualDirectory.GetDirectoryExt(string.Empty));
          LoadDirectory(currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE:
          if (!Util.Utils.IsRemovable(message.Label))
          {
            _virtualDirectory.Remove(message.Label);
          }
          if (currentFolder.Contains(message.Label))
          {
            currentFolder = string.Empty;
          }
          LoadDirectory(currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          GUIFacadeControl pControl = (GUIFacadeControl)GetControl(facadeLayout.GetID);
          pControl.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          GUIFacadeControl pControl2 = (GUIFacadeControl)GetControl(facadeLayout.GetID);
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

        case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
        case GUIMessage.MessageType.GUI_MSG_CLICKED:

          // Respond to the correct control.  The value is retrived directly from the control by the called handler.
          if (message.TargetControlId == btnViews.GetID)
          {
            SetView(btnViews.SelectedItemValue);
            GUIControl.FocusControl(GetID, btnViews.GetID);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_LAYOUT_CHANGED:
          FolderSetting folderSetting = new FolderSetting();
          folderSetting.UpdateFolders(-1, CurrentSortAsc, (int)currentLayout);
          break;

        case GUIMessage.MessageType.GUI_MSG_PICTURESFILE_CREATED:
          if (disp == Display.Files)
          {
            AddItem(message.Label, -1);
      }
          break;

        case GUIMessage.MessageType.GUI_MSG_PICTURESFILE_DELETED:
          if (disp == Display.Files)
          {
            DeleteItem(message.Label);
            SelectCurrentItem();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PICTURESFILE_RENAMED:
          if (disp == Display.Files)
          {
            ReplaceItem(message.Label2, message.Label);
            SelectCurrentItem();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PICTURESDIRECTORY_CREATED:
          if (disp == Display.Files)
          {
            AddItem(message.Label, -1);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PICTURESDIRECTORY_DELETED:
          if (disp == Display.Files)
          {
            DeleteItem(message.Label);
            SelectCurrentItem();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PICTURESDIRECTORY_RENAMED:
          if (disp == Display.Files)
          {
            ReplaceItem(message.Label2, message.Label);
            SelectCurrentItem();
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void SetView(int selectedViewId)
    {
      switch (selectedViewId)
      {
        case 0: // Shares
          if (disp != Display.Files)
          {
            disp = Display.Files;
            LoadDirectory(m_strDirectoryStart);
          }
          break;

        case 1: // Date
          if (disp != Display.Date)
          {
            disp = Display.Date;
            LoadDirectory("");
          }
          break;
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = GetSelectedItem();
      selectedListItem = item;
      int itemNo = GetSelectedItemNo();
      selectedItemIndex = itemNo;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (item == null)
      {
        dlg.AddLocalizedString(868); // Force reset virtual directory if user want to refresh offline share
      }
      else if (item.IsFolder && item.Label == ".." && _virtualDirectory.IsShareOfflineDetected())
      {
        dlg.AddLocalizedString(868); // Force reset virtual directory if user want to refresh offline share
      }
      else if (item.IsFolder && item.Label == "..")
      {
        return;
      }
      else
      {
        GUIControl cntl = GetControl(facadeLayout.GetID);
        if (cntl == null)
        {
          return; // Control not found
        }

        if (!item.IsFolder)
        {
          dlg.AddLocalizedString(735); //rotate
          dlg.AddLocalizedString(783); //rotate 180
          dlg.AddLocalizedString(784); //rotate 270 
          dlg.AddLocalizedString(923); //show
          dlg.AddLocalizedString(108); //start slideshow
          dlg.AddLocalizedString(940); //properties
        }
        else
        {
          if (_refreshThumbnailsThread != null && _refreshThumbnailsThread.IsAlive)
          {
            dlg.AddLocalizedString(190000); //Abort thumbnail creation thread
          }
          else
          {
            dlg.AddLocalizedString(200047); //Recreate thumbnails (incl. subfolders)
            dlg.AddLocalizedString(190001); //Create missing thumbnails (incl. subfolders)
          }
          dlg.AddLocalizedString(200048); //Regenerate Thumbnails
        }

        string iPincodeCorrect;

        if (!_virtualDirectory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && isFileMenuEnabled)
        {
          dlg.AddLocalizedString(500); // FileMenu      
        }

        #region Eject/Load

        // CD/DVD/BD
        if (Util.Utils.getDriveType(item.Path) == 5)
        {
          if (item.Path != null)
          {
            var driveInfo = new DriveInfo(Path.GetPathRoot(item.Path));

            // There is no easy way in NET to detect open tray so we will check
            // if media is inside (load will be visible also in case that tray is closed but
            // media is not loaded)
            if (!driveInfo.IsReady)
            {
              dlg.AddLocalizedString(607); //Load  
            }

            dlg.AddLocalizedString(654); //Eject  
          }
        }

        if (Util.Utils.IsRemovable(item.Path) || Util.Utils.IsUsbHdd(item.Path))
        {
          dlg.AddLocalizedString(831);
        }

        if (_virtualDirectory.IsRootShare(item.Path) || _virtualDirectory.IsShareOfflineDetected())
        {
          dlg.AddLocalizedString(868); // Force reset virtual directory if user want to refresh offline share
        }

        #endregion
      }

      if (_protectedShares.Count > 0)
      {
        if (_ageConfirmed)
        {
          dlg.AddLocalizedString(1240); //Lock content
        }
        else
        {
          dlg.AddLocalizedString(1241); //Unlock content
        }
      }

      dlg.AddLocalizedString(1299); // Refresh current directory

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 457: //Switch View
          ;
          break;
        
        case 735: // rotate
          OnRotatePicture(90);
          break;

        case 783: // rotate 180
          OnRotatePicture(180);
          break;

        case 784: // rotate 270
          OnRotatePicture(270);
          break;

        case 923: // show
          OnClick(itemNo);
          break;

        case 108: // start slideshow
          OnClickSlideShow(itemNo);
          break;

        case 940: // properties
          OnInfo(itemNo);
          break;

        case 500: // File menu
          // get pincode
          if (fileMenuPinCode != string.Empty)
          {
            string strUserCode = string.Empty;
            if (GetUserPasswordString(ref strUserCode) && strUserCode == fileMenuPinCode)
            {
              OnShowFileMenu();
            }
          }
          else
          {
            OnShowFileMenu();
          }
          break;

        case 1240: // Protected content
        case 1241: // Protected content
          OnContentLock();
          break;

        case 190000: // Abort thumbnail creation thread
          if (_refreshThumbnailsThread != null && _refreshThumbnailsThread.IsAlive)
          {
            _refreshThumbnailsThreadAbort = true;
          }
          break;
        case 190001: // Create missing thumbnails (incl. subfolders)
          if (item != null && item.IsFolder)
          {
            OnCreateAllThumbs(item, false, true);
          }
          break;
        case 200047: // Recreate all thumbnails (incl. subfolders)
          if (item != null && item.IsFolder)
          {
            OnCreateAllThumbs(item, true, true);
          }
          break;
        case 200048: // Regenerate Thumbnails
          if (item != null && item.IsFolder)
          {
            OnCreateAllThumbs(item, true, false);
          }
          break;
        case 831:
          string message;

          if (item != null && (Util.Utils.IsUsbHdd(item.Path) || Util.Utils.IsRemovableUsbDisk(item.Path)))
          {
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
          }
          else if (item != null && !RemovableDriveHelper.EjectMedia(item.Path, out message))
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
        case 607: // Load (only CDROM)
          if (item != null)
          {
            Util.Utils.CloseCDROM(Path.GetPathRoot(item.Path));
          }
          break;
        case 654: // Eject
          if (item == null || item.Path == null || Util.Utils.getDriveType(item.Path) != 5)
          {
            Util.Utils.EjectCDROM();
          }
          else
          {
            Util.Utils.EjectCDROM(Path.GetPathRoot(item.Path));
          }
          LoadDirectory(string.Empty);
          break;

        case 868: // Reset V.directory
          {
            ResetShares();

            if (_virtualDirectory.DefaultShare != null && _virtualDirectory.DefaultShare.Path != string.Empty)
            {
              LoadDirectory(_virtualDirectory.DefaultShare.Path);
            }
            else
            {
              LoadDirectory(string.Empty);
            }
          }
          break;

        case 1299: // Refresh current directory
          {
            if (facadeLayout.ListLayout.ListItems.Count > 0 && !string.IsNullOrEmpty(currentFolder))
            {
              facadeLayout.SelectedListItemIndex = 0;
              LoadDirectory(currentFolder);
      }
    }
          break;
      }
    }

    private void ReplaceItem(string oldPath, string newPath)
    {
      if (Directory.Exists(newPath) || (Util.Utils.IsPicture(oldPath) && Util.Utils.IsPicture(newPath)))
      {
        for (int i = 0; i < facadeLayout.Count; i++)
        {
          if (facadeLayout[i].Path == oldPath)
          {
            AddItem(newPath, i);
            return;
          }
        }
      }

      if (Util.Utils.IsPicture(newPath))
      {
        AddItem(newPath, -1);
      }

      if (Util.Utils.IsPicture(oldPath))
      {
        DeleteItem(oldPath);
      }
    }

    private int DeleteItem(string path)
    {
      int oldItem = -1;
      try
      {
        selectedItemIndex = facadeLayout.SelectedListItemIndex;
        for (int i = 0; i < facadeLayout.Count; i++)
        {
          if (facadeLayout[i].Path == path)
          {
            facadeLayout.RemoveItem(i);
            if (selectedItemIndex >= i)
            {
              selectedItemIndex--;
            }
            oldItem = i;
            break;
          }
        }
        int totalItems = facadeLayout.Count;

        if (totalItems > 0)
        {
          GUIListItem rootItem = facadeLayout[0];
          if (rootItem.Label == "..")
          {
            totalItems--;
          }
        }

        GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItems));
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures.DeleteItem Exception: {0}", ex.Message);
      }
      return oldItem;
    }

    private void AddItem(string path, int index)
    {
      try
      {
        for (int i = 0; i < facadeLayout.Count; i++)
        {
          if (facadeLayout[i].Path == path)
          {
            Log.Debug("GUIPictures.AddItem Duplicated item found: {0}", path);
            return;
          }
        }
        
        FileInformation fi = new FileInformation();
        if (File.Exists(path))
        {
          FileInfo f = new FileInfo(path);
          fi.CreationTime = File.GetCreationTime(path);
          fi.Length = f.Length;
        }
        else
        {
          fi = new FileInformation();
          fi.CreationTime = DateTime.Now;
          fi.Length = 0;
        }

        GUIListItem item = new GUIListItem(Util.Utils.GetFilename(path), "", path, true, fi);
        List<GUIListItem> itemlist = new List<GUIListItem>();
        item.IsFolder = Directory.Exists(path);

        if (!item.IsFolder && path.ToLower().Contains(@"folder.jpg"))
        {
          return;
        }

        if (!item.IsFolder)
        {
          SortMethod method = (SortMethod)mapSettings.SortBy;
          bool sortAsc = mapSettings.SortAscending;

          switch (method)
          {
            case SortMethod.Name:
              item.Label2 = Util.Utils.GetSize(item.FileInfo.Length);
              break;
            case SortMethod.Modified:
            case SortMethod.Created:
              if (method == SortMethod.Modified)
              {
                item.Label2 = item.FileInfo.ModificationTime.ToShortDateString() + " " +
                               item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
              }
              else
              {
                item.Label2 = item.FileInfo.CreationTime.ToShortDateString() + " " +
                               item.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
              }
              break;
            case SortMethod.Size:
              item.Label2 = Util.Utils.GetSize(item.FileInfo.Length);
              break;
          }
        }

        MissingThumbCacher ThumbWorker = new MissingThumbCacher(currentFolder, _autocreateLargeThumbs, false, true);

        item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

        if (item.IsFolder)
        {
          CountOfNonImageItems++; // necessary to select the right item later from the slideshow
        }

        if (index == -1)
        {
          facadeLayout.Add(item);
        }
        else
        {
          facadeLayout.Replace(index, item);
        }

        int totalItems = facadeLayout.Count;

        if (totalItems > 0)
        {
          GUIListItem rootItem = facadeLayout[0];
          if (rootItem.Label == "..")
          {
            totalItems--;
          }
        }
        GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItems));
      }
      catch (Exception ex)
      {
        Log.Error("GUIPicturesFiles.AddItem Exception: {0}", ex.Message);
      }
    }

    // Show or hide protected content
    private void OnContentLock()
    {
      if (!_ageConfirmed)
      {
        if (RequestPin())
        {
          _ageConfirmed = true;
          LoadDirectory(currentFolder);
        }
        return;
      }
      _ageConfirmed = false;
      LoadDirectory(currentFolder);
    }

    // Protected content PIN validation (any PIN from pics protected folders is valid)
    private bool RequestPin()
    {
      bool retry = true;
      bool sucess = false;
      _currentProtectedShare.Clear();

      while (retry)
      {
        GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
        GUIWindowManager.SendMessage(msgGetPassword);
        string iPincode = string.Empty;

        iPincode = msgGetPassword.Label;

        foreach (string p in _protectedShares)
        {
          char[] splitter = { '|' };
          string[] pin = p.Split(splitter);

          if (iPincode != pin[0])
          {
            _currentPin = iPincode;
            continue;
          }

          if (iPincode == pin[0])
          {
            _currentPin = iPincode;
            _currentProtectedShare.Add(pin[1]);
            sucess = true;
          }
        }

        if (sucess)
          return true;

        GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0,
                                                     0);
        GUIWindowManager.SendMessage(msgWrongPassword);

        if (!(bool)msgWrongPassword.Object)
        {
          retry = false;
        }
        else
        {
          retry = true;
        }
      }

      _currentPin = string.Empty;
      return false;
    }

    protected virtual PictureSort.SortMethod CurrentSortMethod
    {
      get { return currentSortMethod; }
      set { currentSortMethod = value; }
    }

    #endregion

    #region listview management

    protected bool ViewByIcon
    {
      get
      {
        if (mapSettings.ViewAs != (int)Layout.List)
        {
          return true;
        }
        return false;
      }
    }

    protected bool ViewByLargeIcon
    {
      get
      {
        if (mapSettings.ViewAs == (int)Layout.LargeIcons)
        {
          return true;
        }
        return false;
      }
    }

    private GUIListItem GetSelectedItem()
    {
      return facadeLayout.SelectedListItem;
    }

    private GUIListItem GetItem(int itemIndex)
    {
      if (itemIndex >= facadeLayout.Count || itemIndex < 0)
      {
        return null;
      }
      return facadeLayout[itemIndex];
    }

    private int GetSelectedItemNo()
    {
      return facadeLayout.SelectedListItemIndex;
    }

    private int GetItemCount()
    {
      return facadeLayout.Count;
    }

    protected override string SerializeName
    {
      get { return "mypicture"; }
    }

    protected override void UpdateButtonStates()
    {
      CurrentSortAsc = mapSettings.SortAscending;

      base.UpdateButtonStates();

      string textLine = string.Empty;

      SortMethod method = (SortMethod)mapSettings.SortBy;

      switch (method)
      {
        case SortMethod.Name:
          textLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.Modified:
          textLine = GUILocalizeStrings.Get(1221);
          break;
        case SortMethod.Created:
          textLine = GUILocalizeStrings.Get(1220);
          break;
        case SortMethod.Size:
          textLine = GUILocalizeStrings.Get(105);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, GUILocalizeStrings.Get(96) + textLine);

      if (null != facadeLayout)
        facadeLayout.EnableScrollLabel = method == SortMethod.Name;
    }

    private void ShowThumbPanel()
    {
      CurrentLayout = (Layout)mapSettings.ViewAs;
      SwitchLayout();
      UpdateButtonStates();
    }

    /// <summary>
    /// Set the selected item of the facadeLayout
    /// </summary>
    public void SetSelectedItemIndex(int index)
    {
      selectedItemIndex = CountOfNonImageItems + index;
    }

    public void IncSelectedItemIndex()
    {
      Log.Debug("GUIPictures: INC selectedItemIndex {0}", selectedItemIndex);
      selectedItemIndex ++;
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
          catch (Exception) {}
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
      FolderSettings.GetFolderSetting(folderName, "Pictures", typeof (MapSettings), out o);
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
        Share share = _virtualDirectory.GetShare(folderName);
        if (share != null)
        {
          if (mapSettings == null)
          {
            mapSettings = new MapSettings();
          }
          mapSettings.ViewAs = (int)share.DefaultLayout;
          CurrentLayout = (Layout)mapSettings.ViewAs;
        }
      }
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        if (xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false))
        {
          xmlreader.SetValue("pictures", "lastfolder", folderName);
          xmlreader.SetValue("pictures", "lastview", (int)disp);
        }
      }
      CurrentSortAsc = mapSettings.SortAscending;
      CurrentLayout = (Layout)mapSettings.ViewAs;
    }

    private void SaveFolderSettings(string folder)
    {
      if (folder == string.Empty)
      {
        folder = "root";
      }
      FolderSettings.AddFolderSetting(folder, "Pictures", typeof (MapSettings), mapSettings);
    }

    #endregion

    #region Sort Members

    private void OnSort()
    {
      facadeLayout.Sort(this);
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
        return 1;
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
            return Util.StringLogicalComparer.Compare(item1.Label, item2.Label);
          }
          else
          {
            return Util.StringLogicalComparer.Compare(item2.Label, item1.Label);
          }


        case SortMethod.Modified:
        case SortMethod.Created:
          if (item1.FileInfo == null)
          {
            return -1;
          }
          if (item2.FileInfo == null)
          {
            return -1;
          }

          if (method == SortMethod.Modified)
          {
            item1.Label2 = item1.FileInfo.ModificationTime.ToShortDateString() + " " +
                           item1.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
            item2.Label2 = item2.FileInfo.ModificationTime.ToShortDateString() + " " +
                           item2.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          }
          else
          {
            item1.Label2 = item1.FileInfo.CreationTime.ToShortDateString() + " " +
                           item1.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
            item2.Label2 = item2.FileInfo.CreationTime.ToShortDateString() + " " +
                           item2.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          }

          if (sortAsc)
          {
            if (method == SortMethod.Modified)
              return DateTime.Compare(item1.FileInfo.ModificationTime, item2.FileInfo.ModificationTime);
            else
              return DateTime.Compare(item1.FileInfo.CreationTime, item2.FileInfo.CreationTime);
          }
          else
          {
            if (method == SortMethod.Modified)
              return DateTime.Compare(item2.FileInfo.ModificationTime, item1.FileInfo.ModificationTime);
            else
              return DateTime.Compare(item2.FileInfo.CreationTime, item1.FileInfo.CreationTime);
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
            long compare = (item1.FileInfo.Length - item2.FileInfo.Length);
            return compare == 0 ? 0 : compare < 0 ? -1 : 1;
          }
          else
          {
            long compare = (item2.FileInfo.Length - item1.FileInfo.Length);
            return compare == 0 ? 0 : compare < 0 ? -1 : 1;
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
      if (!item.IsFolder)
      {
        if (item.HasThumbnail)
        {
          string thumbnailImage = GetThumbnail(item.Path);
          string thumbnailLargeImage = GetLargeThumbnail(item.Path);
          if (File.Exists(thumbnailImage) || Util.Utils.FileExistsInCache(thumbnailImage))
          {
            item.IconImage = thumbnailImage;

            if (_autocreateLargeThumbs)
            {
              item.ThumbnailImage = thumbnailLargeImage;
              item.IconImage = thumbnailImage;
            }
            else
            {
              item.ThumbnailImage = thumbnailImage;
              item.IconImage = thumbnailImage;
            }
          }
        }
        else
        {
          OnRetrieveThumbnailFiles(item);
        }
        Util.Utils.SetThumbnails(ref item);
      }
      else
      {
        if (item.Label != "..")
        {
          string pin;
          if (!_virtualDirectory.IsProtectedShare(item.Path, out pin))
          {
            Util.Utils.SetThumbnails(ref item);
          }
        }
      }
    }

    private void OnRetrieveThumbnailFiles(GUIListItem item)
    {
      if (item.IsRemote)
      {
        return;
      }
      if (!item.IsFolder)
      {
        if (!item.HasThumbnail)
        {
          string thumbnailImage = GetThumbnail(item.Path);
          string thumbnailLargeImage = GetLargeThumbnail(item.Path);
          MediaPortal.Util.Utils.SetDefaultIcons(item);

          if (!Util.Utils.FileExistsInCache(thumbnailImage) && Util.Utils.IsPicture(item.Path) && _autoCreateThumbs)
          {
            ThreadPool.QueueUserWorkItem(delegate
                                           {
                                             try
                                             {
                                               GetThumbnailfile(ref item);
                                             }
                                             catch (Exception)
                                             {
                                               Log.Error(
                                                 "GUIPictures - Error loading next item (OnRetrieveThumbnailFiles)");
                                             }
                                           });
          }
          else
          {
            if (Util.Utils.FileExistsInCache(thumbnailImage))
            {
              if (_autocreateLargeThumbs && Util.Utils.FileExistsInCache(thumbnailLargeImage))
              {
                item.ThumbnailImage = thumbnailLargeImage;
                item.IconImage = thumbnailImage;
              }
              else
              {
                item.ThumbnailImage = thumbnailImage;
                item.IconImage = thumbnailImage;
              }
            }
          }
          Util.Utils.SetThumbnails(ref item);
        }
      }
      else
      {
        if (item.Label != "..")
        {
          string pin;
          if (!_virtualDirectory.IsProtectedShare(item.Path, out pin))
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
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
      }
    }

    private void DoDeleteItem(GUIListItem item)
    {
      if (item.IsFolder)
      {
        if (item.Label != "..")
        {
          List<GUIListItem> items = new List<GUIListItem>();
          items = _virtualDirectory.GetDirectoryUnProtectedExt(item.Path, false);
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

    protected override void OnInfo(int itemNumber)
    {
      if (_virtualDirectory.IsRemote(currentFolder))
      {
        return;
      }
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
      // Needed to set GUIDialogExif
      exifDialog.Restore();
      exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_EXIF);
      exifDialog.FileName = item.Path;
      exifDialog.DoModal(GetID);
      // Fix for Mantis issue: 0001709: Background not correct after viewing pictures properties twice
      exifDialog.Restore();
    }

    private void OnRotatePicture(int degrees)
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
      DoRotatePicture(item.Path, degrees);
      GUIControl.RefreshControl(GetID, facadeLayout.GetID);
    }

    public static void DoRotatePicture(string aPicturePath)
    {
      DoRotatePicture(aPicturePath, 90);
    }

    public static void DoRotatePicture(string aPicturePath, int degrees)
    {
      int rotate = 0;
      rotate = PictureDatabase.GetRotation(aPicturePath);

      if (degrees == 90)
      {
        rotate++;
      }
      else if (degrees == 180)
      {
        rotate = rotate + 2;
      }
      else if (degrees == 270)
      {
        rotate = rotate + 3;
      }

      if (rotate >= 4)
      {
        rotate = rotate - 4;
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

        if (Util.Picture.CreateThumbnail(aPicturePath, thumbnailImage, (int)Thumbs.ThumbResolution,
                                         (int)Thumbs.ThumbResolution, rotate, Thumbs.SpeedThumbsSmall))
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

    protected void OnClickSlide(int itemIndex)
    {
      if ((itemIndex < 0) || (itemIndex > GetSelectedItemNo()))
      {
        itemIndex = 0;
      }
      int i = itemIndex;

      GUIListItem item = GetItem(i);

      if (item == null)
      {
        return;
      }
      if (item.IsFolder)
      {
        selectedItemIndex = GetSelectedItemNo();
        OnShowPicture(item.Path);
      }
      else
      {
        if (_virtualDirectory.IsRemote(item.Path))
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
          {
            if (!_virtualDirectory.ShouldWeDownloadFile(item.Path))
            {
              return;
            }
            if (!_virtualDirectory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
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

    protected override void OnClick(int itemIndex)
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
      {
        return;
      }

      if (!WakeUpSrv(item.Path))
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
        if (_virtualDirectory.IsRemote(item.Path))
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
          {
            if (!_virtualDirectory.ShouldWeDownloadFile(item.Path))
            {
              return;
            }
            if (!_virtualDirectory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
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

    protected void OnClickSlideShow(int itemIndex)
    {
      if ((itemIndex < 0) || (itemIndex > GetSelectedItemNo()))
      {
        itemIndex = 0;
      }
      int i = itemIndex;

      GUIListItem item = GetItem(i);

      if (item == null)
      {
        return;
      }

      if (!WakeUpSrv(item.Path))
      {
        return;
      }

      if (item.IsFolder)
      {
        i++;
        GUIListItem itemSelect = GetItem(i);
        selectedItemIndex = i;
        OnSlideShow(itemSelect.Path);
      }
      else
      {
        if (_virtualDirectory.IsRemote(item.Path))
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
          {
            if (!_virtualDirectory.ShouldWeDownloadFile(item.Path))
            {
              return;
            }
            if (!_virtualDirectory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
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
        OnSlideShow(item.Path);
      }
    }

    private void OnShowPicture(string strFile)
    {
      // Stop video playback before starting show picture to avoid MP freezing
      if (g_Player.MediaInfo != null && g_Player.MediaInfo.hasVideo || g_Player.IsTV || g_Player.IsVideo)
      {
        g_Player.Stop();
      }
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
      {
        return;
      }
      if (SlideShow._returnedFromVideoPlayback)
      {
        SlideShow._returnedFromVideoPlayback = false;
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

    public void AddDir(GUISlideShow SlideShow, string strDir)
    {
      List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(strDir);
      itemlist.Sort(new PictureSort(CurrentSortMethod, CurrentSortAsc));
      Filter(ref itemlist);
      foreach (GUIListItem item in itemlist)
      {
        if (item.IsFolder)
        {
          if (item.Label != ".." && !SlideShow._slideFolder.Contains(item.Label))
          {
            SlideShow._slideFolder.Add(item.Path);
            if (_playVideosInSlideshows)
            {
              SlideShow.Add(item.Path);
            }
            else if (!Util.Utils.IsVideo(item.Path))
            {
              SlideShow.Add(item.Path);
            }
          }
        }
        else if (!item.IsRemote)
        {
          if (_playVideosInSlideshows)
          {
            SlideShow.Add(item.Path);
          }
          else if (!Util.Utils.IsVideo(item.Path))
          {
            SlideShow.Add(item.Path);
          }
        }
      }
    }

    private void OnSlideShowRecursive()
    {
      // Stop video playback before starting show picture to avoid MP freezing
      if (g_Player.MediaInfo != null && g_Player.MediaInfo.hasVideo || g_Player.IsTV || g_Player.IsVideo)
      {
        g_Player.Stop();
      }
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
      {
        return;
      }

      SlideShow.Reset();
      SlideShow._showRecursive = true;
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
        if (_autoShuffle)
        {
          SlideShow.Shuffle(false, false);
        }
      }
      if (SlideShow.Count > 0 || SlideShow._slideFolder.Count > 0 && SlideShow._slideList.Count > 0)
      {
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SLIDESHOW);
        SlideShow.StartSlideShow(currentFolder);
      }
    }

    private void OnSlideShow()
    {
      OnClickSlideShow(0);
    }

    private void OnSlideShow (string strFile)
    {
      // Stop video playback before starting show picture to avoid MP freezing
      if (g_Player.MediaInfo != null && g_Player.MediaInfo.hasVideo || g_Player.IsTV || g_Player.IsVideo)
      {
        g_Player.Stop();
      }
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
      {
        return;
      }
      if (SlideShow._returnedFromVideoPlayback)
      {
        SlideShow._returnedFromVideoPlayback = false;
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
          if (_playVideosInSlideshows)
          {
            SlideShow.Add(item.Path);
          }
          else if (!Util.Utils.IsVideo(item.Path))
          {
            SlideShow.Add(item.Path);
          }
        }
      }
      if (SlideShow.Count > 0)
      {
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SLIDESHOW);
        SlideShow.Select(strFile);
        SlideShow.StartSlideShow(currentFolder);
      }
    }

    private void UpdateThumbnailsInFolder(GUIListItem item, bool Regenerate)
    {
      List<GUIListItem> itemlist = new List<GUIListItem>();

      ListFilesForUpdateThumbnails(item, ref itemlist);
      Log.Debug("UpdateThumbnailsInFolder: Folder count {0}, regenerate all thumbs {1}", itemlist.Count, Regenerate);

      _progressDialogForRefreshThumbnails =
       (GUIDialogProgress)GUIWindowManager.GetWindow(101); //(int)Window.WINDOW_DIALOG_PROGRESS
      _progressDialogForRefreshThumbnails.Reset();
      _progressDialogForRefreshThumbnails.SetHeading(GUILocalizeStrings.Get(200047));
      _progressDialogForRefreshThumbnails.ShowProgressBar(true);
      _progressDialogForRefreshThumbnails.SetLine(1, itemlist[0].Path.Replace("\\", "/"));
      _progressDialogForRefreshThumbnails.SetLine(2, string.Empty);
      _progressDialogForRefreshThumbnails.SetLine(3, "Folder count: " + itemlist.Count + "/0");
      _progressDialogForRefreshThumbnails.StartModal(GUIWindowManager.ActiveWindow);


      if (_refreshThumbnailsThread != null && _refreshThumbnailsThread.IsAlive)
      {
        _refreshThumbnailsThread.Abort();
      }

      _refreshThumbnailsThreadAbort = false;
      _refreshThumbnailsThread = new Thread(() => RefreshThumbnailsThread(itemlist, Regenerate));
      _refreshThumbnailsThread.Priority = ThreadPriority.Lowest;
      _refreshThumbnailsThread.IsBackground = true;
      _refreshThumbnailsThread.Name = "RefreshThumbnailsThread";
      _refreshThumbnailsThread.Start();
    }

    private void RefreshThumbnailsThread(List<GUIListItem> itemlist, bool Regenerate)
    {
      try
      {
        for (int i = 0; i < itemlist.Count; i++)
        {
          if (_refreshThumbnailsThreadAbort)
          {
            Log.Info("RefreshThumbnailsThread: Aborted by the user.");
            return;
          }
          int perc = (i * 100) / itemlist.Count;
          _progressDialogForRefreshThumbnails.SetPercentage(perc);
          _progressDialogForRefreshThumbnails.SetLine(1, itemlist[i].Path.Replace("\\", "/"));
          _progressDialogForRefreshThumbnails.SetLine(3, "Folder count: " + itemlist.Count + "/" + i);
          _progressDialogForRefreshThumbnails.Progress();

          Thread.Sleep(50);

          MissingThumbCacher ManualThumbBuilder = new MissingThumbCacher(itemlist[i].Path, _autocreateLargeThumbs, Regenerate, false);
        }
      }
      catch (Exception ex)
      {
        Log.Error("RefreshThumbnailsThread: {0}", ex);
      }
      finally
      {
        _progressDialogForRefreshThumbnails.Close();
      }
      Log.Info("RefreshThumbnailsThread: Finished.");
    }

    private void ListFilesForUpdateThumbnails(GUIListItem item, ref List<GUIListItem> Itemist)
    {
      if (item != null)
      {
        Itemist.Add(item);

        VirtualDirectory virtualDirectory = new VirtualDirectory();
        virtualDirectory.SetExtensions(Util.Utils.VideoExtensions);
        List<GUIListItem> inertItemlist = virtualDirectory.GetDirectoryUnProtectedExt(item.Path, true);
        foreach (GUIListItem subItem in inertItemlist)
        {
          if (subItem.IsFolder && subItem.Label != "..")
          {
            ListFilesForUpdateThumbnails(subItem, ref Itemist);
          }
        }
      }
    }

    private void CreateAllThumbs(GUIListItem item, bool Regenerate, bool Recursive)
    {
      if (disp == Display.Files)
      {
        if (Recursive)
        {
          UpdateThumbnailsInFolder(item, Regenerate);
          return;
        }
        MissingThumbCacher ManualThumbBuilder = new MissingThumbCacher(item.Path, _autocreateLargeThumbs, Regenerate, true);
      }
      else if (disp == Display.Date)
      {
        // TODO: Thumbworker alternative on file base instead of directory
      }
    }

    private void OnCreateAllThumbs(GUIListItem item, bool Regenerate, bool Recursive)
    {
      CreateAllThumbs(item, Regenerate, Recursive);

      GUITextureManager.CleanupThumbs();
      GUIWaitCursor.Hide();

      LoadDirectory(currentFolder);
    }

    protected override void SelectCurrentItem()
    {
      selectedItemIndex = facadeLayout.SelectedListItemIndex;
      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
      }
    }

    protected override void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(1221); // date modified
      dlg.AddLocalizedString(1220); // date created
      dlg.AddLocalizedString(105); // size

      // set the focus to currently used sort method
      dlg.SelectedLabel = mapSettings.SortBy;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      CurrentSortAsc = true;
      switch (dlg.SelectedId)
      {
        case 103:
          mapSettings.SortBy = (int)SortMethod.Name;
          break;
        case 1220:
          mapSettings.SortBy = (int)SortMethod.Created;
          break;
        case 1221:
          mapSettings.SortBy = (int)SortMethod.Modified;
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
      FolderSetting folderSetting = new FolderSetting();
      folderSetting.UpdateFolders(mapSettings.SortBy, CurrentSortAsc, -1);
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
      dlgFile.SetDirectoryStructure(_virtualDirectory);
      dlgFile.DoModal(GetID);
      destinationFolder = dlgFile.GetDestinationDir();

      //final		
      if (dlgFile.Reload())
      {
        LoadDirectory(currentFolder);
        if (selectedItemIndex >= 0)
        {
          if (selectedItemIndex >= facadeLayout.Count)
            selectedItemIndex = facadeLayout.Count - 1;
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
        }
      }

      dlgFile.DeInit();
      dlgFile = null;
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
      if (windowId == (int)Window.WINDOW_FULLSCREEN_VIDEO)
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

    private bool WakeUpSrv(string newFolderName)
    {
      bool wakeOnLanEnabled;
      if (!Util.Utils.IsUNCNetwork(newFolderName))
      {
        // Check if letter drive is a network drive
        string detectedFolderName = Util.Utils.FindUNCPaths(newFolderName);
        if (Util.Utils.IsUNCNetwork(detectedFolderName))
        {
          wakeOnLanEnabled = _virtualDirectory.IsWakeOnLanEnabled(_virtualDirectory.GetShare(newFolderName));
          newFolderName = detectedFolderName;
        }
        else
        {
          return true;
        }
      }
      else
      {
        wakeOnLanEnabled = _virtualDirectory.IsWakeOnLanEnabled(_virtualDirectory.GetShare(newFolderName));
      }

      string serverName = string.Empty;

      if (wakeOnLanEnabled)
      {
        serverName = Util.Utils.GetServerNameFromUNCPath(newFolderName);

        DateTime now = DateTime.Now;
        TimeSpan ts = now - _prevWolTime;

        if (serverName == _prevServerName && _wolResendTime*60 > ts.TotalSeconds)
        {
          return true;
        }

        _prevWolTime = DateTime.Now;
        _prevServerName = serverName;

        try
        {
          Log.Debug("WakeUpSrv: FolderName = {0}, ShareName = {1}, WOL enabled = {2}", newFolderName,
                    _virtualDirectory.GetShare(newFolderName).Name, wakeOnLanEnabled);
        }
        catch
        {
        }

        if (!string.IsNullOrEmpty(serverName))
        {
          return WakeupUtils.HandleWakeUpServer(serverName, _wolTimeout);
        }
      }
      return true;
    }

    public static void Filter(ref List<GUIListItem> itemlist)
    {
      itemlist.RemoveAll(ContainsFolderThumb);
    }

    protected override void LoadDirectory(string strNewDirectory)
    {
      if (strNewDirectory == null)
      {
        Log.Warn("GUIPictures::LoadDirectory called with invalid argument. newFolderName is null!");
        return;
      }

      if (facadeLayout == null)
      {
        return;
      }
      
      List<GUIListItem> itemlist;
      string objectCount = string.Empty;

      if (!WakeUpSrv(strNewDirectory))
      {
        return;
      }

      GUIWaitCursor.Show();

      if (_pictureFolderWatcher != null)
      {
        _pictureFolderWatcher.ChangeMonitoring(false);
      }

      if (!string.IsNullOrEmpty(strNewDirectory))
      {
        _pictureFolderWatcher = new PicturesFolderWatcherHelper(strNewDirectory);
        _pictureFolderWatcher.SetMonitoring(true);
        _pictureFolderWatcher.StartMonitor();
      }

      if (!returnFromSlideshow)
      {
        GUIListItem SelectedItem = GetSelectedItem();
        if (SelectedItem != null)
        {
          if (SelectedItem.IsFolder && SelectedItem.Label != "..")
          {
            folderHistory.Set(SelectedItem.Label, currentFolder);
          }
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

      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      if (disp == Display.Files)
      {
        _removableDrivesHandlerThread.Join();
        
        itemlist = _virtualDirectory.GetDirectoryExt(currentFolder);

        if (currentFolder == string.Empty)
        {
          RemovableDrivesHandler.FilterDrives(ref itemlist);
        }

        Filter(ref itemlist);

        if (_autoCreateThumbs)
        {
          MissingThumbCacher ThumbWorker = new MissingThumbCacher(currentFolder, _autocreateLargeThumbs, false, true);
        }

        // int itemIndex = 0;
        CountOfNonImageItems = 0;
        foreach (GUIListItem item in itemlist)
        {
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeLayout.Add(item);

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

      int totalItemCount = facadeLayout.Count;
      string strSelectedItem = folderHistory.Get(currentFolder);
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
      for (int i = 0; i < totalItemCount; i++)
      {
        if (facadeLayout[i].Label == strSelectedItem || facadeLayout[i].Path == strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, i);
          break;
        }
      }
      if (totalItemCount > 0)
      {
        GUIListItem rootItem = (GUIListItem)facadeLayout[0];
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

    // Check if item is pin protected and if it exists within unlocked shares
    // Returns true if item is valid or if item is not within protected shares
    private bool IsItemPinProtected(GUIListItem item, VirtualDirectory vDir)
    {
      string directory = Path.GetDirectoryName(item.Path); // item path

      if (directory != null)
      {
        // Check if item belongs to protected shares
        string pincode = string.Empty;
        bool folderPinProtected = vDir.IsProtectedShare(directory, out pincode);

        bool success = false;

        // User unlocked share/shares with PIN and item is within protected shares
        if (folderPinProtected && _ageConfirmed)
        {
          // Iterate unlocked shares against current item path
          foreach (string share in _currentProtectedShare)
          {
            if (!directory.ToUpperInvariant().Contains(share.ToUpperInvariant()))
            {
              continue;
            }
            success = true;
            break;
          }

          // current item is not within unlocked shares, 
          // don't show item and go to the next item
          if (!success)
          {
            return false;
          }
          return true;
        }

        // Nothing unlocked and item belongs to protected shares,
        // don't show item and go to the next item
        if (folderPinProtected && !_ageConfirmed)
        {
          return false;
        }
      }

      // Item is not inside protected shares, show it
      return true;
    }

    private void LoadDateView(string strNewDirectory)
    {
      try
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
            //Log.Debug("Load Year: " + year);
            item.Path = year;
            item.IsFolder = true;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            facadeLayout.Add(item);
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
          facadeLayout.Add(item);
          CountOfNonImageItems++; // necessary to select the right item later from the slideshow

          List<string> Months = new List<string>();
          int Count = PictureDatabase.ListMonths(year, ref Months);
          foreach (string month in Months)
          {
            // show month in a user friendly string
            item = new GUIListItem(Util.Utils.GetNamedMonth(month));
            item.Path = year + "\\" + month;
            item.IsFolder = true;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            facadeLayout.Add(item);
            CountOfNonImageItems++; // necessary to select the right item later from the slideshow
          }
        }

        // check if day grouping is enabled
        if (_useDayGrouping)
        {
          if (strNewDirectory.Length == 7)
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
            facadeLayout.Add(item);
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
              facadeLayout.Add(item);
              CountOfNonImageItems++; // necessary to select the right item later from the slideshow
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
            facadeLayout.Add(item);
            CountOfNonImageItems++; // necessary to select the right item later from the slideshow

            List<string> pics = new List<string>();
            int Count = PictureDatabase.ListPicsByDate(year + "-" + month + "-" + day, ref pics);

            VirtualDirectory vDir = new VirtualDirectory();
            vDir.LoadSettings("pictures");

            foreach (string pic in pics)
            {
              item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
              item.Path = pic;
              item.IsFolder = false;

              if (!IsItemPinProtected(item, vDir))
                continue;
                
              Util.Utils.SetDefaultIcons(item);
              item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
              item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

              if (!WakeUpSrv(pic))
              {
                return;
              }

              item.FileInfo = new FileInformation(pic, false);
              facadeLayout.Add(item);
            }
          }
        }
        else
        {
          if (strNewDirectory.Length == 7)
          {
            // Pics from one month
            string year = strNewDirectory.Substring(0, 4);
            string month = strNewDirectory.Substring(5, 2);

            GUIListItem item = new GUIListItem("..");
            item.Path = year;
            item.IsFolder = true;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            facadeLayout.Add(item);
            CountOfNonImageItems++; // necessary to select the right item later from the slideshow

            List<string> pics = new List<string>();
            int Count = PictureDatabase.ListPicsByDate(year + "-" + month, ref pics);

            VirtualDirectory vDir = new VirtualDirectory();
            vDir.LoadSettings("pictures");

            foreach (string pic in pics)
            {
              try
              {
                item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
                item.Path = pic;
                item.IsFolder = false;

                if (!IsItemPinProtected(item, vDir))
                  continue;
               
                Util.Utils.SetDefaultIcons(item);
                item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
                item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

                if (!WakeUpSrv(pic))
                {
                  return;
                }

                item.FileInfo = new FileInformation(pic, false);
                facadeLayout.Add(item);
              }
              catch (Exception)
              {
                Log.Warn("GUIPictures: There is no file for this database entry: {0}", item.Path);
              }
            }
          }
        }

        if (facadeLayout.Count == 0 && strNewDirectory != "")
        {
          // Wrong path for date view, go back to top level
          currentFolder = "";
          LoadDateView(currentFolder);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: Error loading date view - {0}", ex.ToString());
      }
    }

    private string LoadDateViewSelect(string strNewDirectory, string selectedItem)
    {
      try
      {
        returnFromSlideshow = true;
        string strSelectedItem = Util.Utils.GetFilename(selectedItem, true);
        if (strNewDirectory.Length == 4 && !_useDayGrouping)
        {
          // Months
          string year = strNewDirectory.Substring(0, 4);
          List<string> Months = new List<string>();
          int CountMonths = PictureDatabase.ListMonths(year, ref Months);
          foreach (string month in Months)
          {
            List<string> pics = new List<string>();
            int CountPics = PictureDatabase.ListPicsByDate(year + "-" + month, ref pics);
            foreach (string pic in pics)
            {
              try
              {
                if (pic == selectedItem)
                {
                  string strFreshNewDirectory = strNewDirectory + "\\" + month;
                  // Set root directory
                  folderHistory.Set(year, "");
                  folderHistory.Set(strFreshNewDirectory, year);
                  folderHistory.Set(strSelectedItem, strFreshNewDirectory);
                  // Reset facadeLayout
                  facadeLayout.Clear();
                  // Reload fresh view
                  LoadDateView(strFreshNewDirectory);
                  returnFromSlideshow = false;
                  return selectedItem;
                }
              }
              catch (Exception)
              {
                Log.Warn("GUIPictures: can't match item in date view");
              }
            }
          }
        }

        // check if day grouping is enabled
        if (_useDayGrouping)
        {
          if (strNewDirectory.Length == 4)
          {
            // Months
            string year = strNewDirectory.Substring(0, 4);
            List<string> Months = new List<string>();
            int CountMonths = PictureDatabase.ListMonths(year, ref Months);
            foreach (string month in Months)
            {
              List<string> Days = new List<string>();
              int CountDays = PictureDatabase.ListDays(month, year, ref Days);
              foreach (string day in Days)
              {
                List<string> pics = new List<string>();
                int CountPics = PictureDatabase.ListPicsByDate(year + "-" + month + "-" + day, ref pics);
                foreach (string pic in pics)
                {
                  try
                  {
                    if (pic == selectedItem)
                    {
                      // Set root directory
                      string strFreshNewDirectory = strNewDirectory + "\\" + month + "\\" + day;
                      // Set root directory
                      folderHistory.Set(year, "");
                      folderHistory.Set(strFreshNewDirectory.Substring(0, 7), year);
                      folderHistory.Set(strFreshNewDirectory, strFreshNewDirectory.Substring(0, 7));
                      folderHistory.Set(strSelectedItem, strFreshNewDirectory);
                      // Reset facadeLayout
                      facadeLayout.Clear();
                      // Reload fresh view
                      LoadDateView(strFreshNewDirectory);
                      returnFromSlideshow = false;
                      return selectedItem;
                    }
                  }
                  catch (Exception)
                  {
                    Log.Warn("GUIPictures: can't match item in date view");
                    returnFromSlideshow = false;
                  }
                }
              }
            }
          }
          else if (strNewDirectory.Length == 7)
          {
            // Days
            string year = strNewDirectory.Substring(0, 4);
            string month = strNewDirectory.Substring(5, 2);

            List<string> Days = new List<string>();
            int CountDays = PictureDatabase.ListDays(month, year, ref Days);
            foreach (string day in Days)
            {
              List<string> pics = new List<string>();
              int CountPics = PictureDatabase.ListPicsByDate(year + "-" + month + "-" + day, ref pics);
              foreach (string pic in pics)
              {
                try
                {
                  if (pic == selectedItem)
                  {
                    string strFreshNewDirectory = strNewDirectory + "\\" + day;
                    // Set root directory
                    folderHistory.Set(year, "");
                    folderHistory.Set(strFreshNewDirectory.Substring(0, 7), year);
                    folderHistory.Set(strFreshNewDirectory, strFreshNewDirectory.Substring(0, 7));
                    folderHistory.Set(strSelectedItem, strFreshNewDirectory);
                    // Reset facadeLayout
                    facadeLayout.Clear();
                    // Reload fresh view
                    LoadDateView(strFreshNewDirectory);
                    returnFromSlideshow = false;
                    return selectedItem;
                  }
                }
                catch (Exception)
                {
                  Log.Warn("GUIPictures: can't match item in date view");
                  returnFromSlideshow = false;
                }
              }
            }
          }
          else if (strNewDirectory.Length == 10)
          {
            // Pics from one day
            string year = strNewDirectory.Substring(0, 4);
            string month = strNewDirectory.Substring(5, 2);
            string day = strNewDirectory.Substring(8, 2);

            List<string> pics = new List<string>();
            int CountPics = PictureDatabase.ListPicsByDate(year + "-" + month + "-" + day, ref pics);
            foreach (string pic in pics)
            {
              if (pic == selectedItem)
              {
                // Set root directory
                folderHistory.Set(year, "");
                folderHistory.Set(strNewDirectory, year);
                folderHistory.Set(day, strNewDirectory);
                folderHistory.Set(strSelectedItem, strNewDirectory);

                //folderHistory.Set(strFreshNewDirectory.Substring(0, 7), year);
                //folderHistory.Set(strFreshNewDirectory, strFreshNewDirectory.Substring(0, 7));
                //folderHistory.Set(strSelectedItem, strFreshNewDirectory);

                // Reset facadeLayout
                facadeLayout.Clear();
                // Reload fresh view
                LoadDateView(strNewDirectory);
                returnFromSlideshow = false;
                return selectedItem;
              }
            }
          }
        }
        else
        {
          if (strNewDirectory.Length == 7)
          {
            // Pics from one month
            string year = strNewDirectory.Substring(0, 4);
            string month = strNewDirectory.Substring(5, 2);

            List<string> pics = new List<string>();
            int CountPics = PictureDatabase.ListPicsByDate(year + "-" + month, ref pics);
            foreach (string pic in pics)
            {
              try
              {
                if (pic == selectedItem)
                {
                  // Set root directory
                  folderHistory.Set(year, "");
                  folderHistory.Set(strNewDirectory, year);
                  folderHistory.Set(strSelectedItem, strNewDirectory);
                  // Reset facadeLayout
                  facadeLayout.Clear();
                  // Reload fresh view
                  LoadDateView(strNewDirectory);
                  returnFromSlideshow = false;
                  return selectedItem;
                }
              }
              catch (Exception)
              {
                Log.Warn("GUIPictures: can't match item in date view");
                returnFromSlideshow = false;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: Error loading date view - {0}", ex.ToString());
        returnFromSlideshow = false;
      }
      returnFromSlideshow = false;
      return String.Empty;
    }

    public static string GetThumbnail(string fileName)
    {
      if (fileName == string.Empty)
      {
        return string.Empty;
      }
      if (Util.Utils.IsVideo(fileName))
      {
        return Util.Utils.GetVideosThumbPathname(fileName);
      }

      return Util.Utils.GetPicturesThumbPathname(fileName);
    }

    public static string GetLargeThumbnail(string fileName)
    {
      if (fileName == string.Empty)
      {
        return string.Empty;
      }
      if (Util.Utils.IsVideo(fileName))
      {
        return Util.Utils.GetVideosLargeThumbPathname(fileName);
      }

      return Util.Utils.GetPicturesLargeThumbPathname(fileName);
    }

    private bool GetUserPasswordString(ref string sString)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Password = true;
      keyboard.Text = sString;
      keyboard.DoModal(GetID); // show it...
      if (keyboard.IsConfirmed)
      {
        sString = keyboard.Text;
      }
      return keyboard.IsConfirmed;
    }

    public static void ResetShares()
    {
      _virtualDirectory.Reset();
      _virtualDirectory.DefaultShare = null;
      _virtualDirectory.LoadSettings("pictures");

      if (_virtualDirectory.DefaultShare != null)
      {
        string pincode;
        bool folderPinProtected = _virtualDirectory.IsProtectedShare(_virtualDirectory.DefaultShare.Path, out pincode);
        if (folderPinProtected)
        {
          currentFolder = string.Empty;
        }
        else
        {
          currentFolder = _virtualDirectory.DefaultShare.Path;
        }
      }
    }

    public static void ResetExtensions(ArrayList extensions)
    {
      _virtualDirectory.SetExtensions(extensions);
    }

    #endregion

    public static bool ThumbnailsThreadAbort
    {
      get { return _refreshThumbnailsThreadAbort; }
    }

    public static bool IsPictureWindow(int windowId)
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

    #region callback events

    public bool ThumbnailCallback()
    {
      return false;
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      OnRetrieveThumbnailFiles(item);
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
      {
        return;
      }
      string thumbnailImage = GetLargeThumbnail(item.Path);
      if (Util.Utils.FileExistsInCache(thumbnailImage))
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

    public static string GetCurrentFolder
    {
      get { return currentFolder; }
    }
  }
}