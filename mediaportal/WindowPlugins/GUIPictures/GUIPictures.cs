#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
        int countVideos = 0;

        vDir.SetExtensions(Util.Utils.PictureExtensions);
        foreach (string ext in Util.Utils.VideoExtensions)
        {
          vDir.AddExtension(ext);
        }

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
              Log.Debug("GUIPictures: RefreshThumbnailsThread: g_Player is Playing, waiting for the end.");
            }

            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
            {
              return;
            }
            if (string.IsNullOrEmpty(item.Path))
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
                bool isVideo = Util.Utils.IsVideo(item.Path);
                bool thumbRet = false;

                if (!item.IsRemote)
                {
                  if (isPicture)
                  {
                    string thumbnailImage = Util.Utils.GetPicturesThumbPathname(item.Path);
                    string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item.Path);

                    if (recreateThumbs || !File.Exists(thumbnailImage) || !Util.Utils.FileExistsInCache(thumbnailImage) ||
                        !File.Exists(thumbnailImageL) || !Util.Utils.FileExistsInCache(thumbnailImageL))
                    {
                      Thread.Sleep(5);

                      if (autocreateLargeThumbs && (!File.Exists(thumbnailImageL) || recreateThumbs))
                      {
                        thumbRet = Util.Picture.ReCreateThumbnail(item.Path, thumbnailImageL,
                                                                (int)Thumbs.ThumbLargeResolution,
                                                                (int)Thumbs.ThumbLargeResolution, iRotate,
                                                                Thumbs.SpeedThumbsLarge,
                                                                true, false, recreateThumbs);
                      }
                      if (!File.Exists(thumbnailImage) || recreateThumbs)
                      {
                        thumbRet = Util.Picture.ReCreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution,
                                                                (int)Thumbs.ThumbResolution, iRotate,
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
                  else if (isVideo)
                  {
                    countVideos++;

                    string thumbnailImage = Util.Utils.GetPicturesThumbPathname(item.Path);
                    string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item.Path);

                    thumbRet = File.Exists(thumbnailImage) && File.Exists(thumbnailImageL);
                    if (!thumbRet)
                    {
                      thumbRet = VideoThumbCreator.CreateVideoThumb(item.Path, thumbnailImage, true, false);
                      if (thumbRet)
                      {
                        Log.Debug("GUIPictures: Creation of thumb successful for Video {0}", item.Path);
                      }
                    }

                    if (thumbRet)
                    {
                      item.IconImage = thumbnailImage;
                      item.ThumbnailImage = thumbnailImageL;
                    }
                  }
                }
              }
              else
              {
                string pin;
                if ((item.Label != "..") && (!vDir.IsProtectedShare(item.Path, out pin)))
                {
                  string thumbnailImage = Path.Combine(item.Path, @"Folder.jpg");
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
            } // if (CheckPathForHistory(item.Path, false))
          } // foreach (GUIListItem item in itemlist)

          // In folder only Video files and "..", create folder thumbs ...
          if (countVideos == itemlist.Count - 1)
          {
            string thumbnailImage = Path.Combine(path, @"Folder.jpg");
            if (!File.Exists(thumbnailImage))
            {
              List<string> pictureList = new List<string>();
              foreach (GUIListItem item in itemlist)
              {
                if (!item.IsFolder && Util.Utils.IsVideo(item.Path))
                {
                  string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item.Path);
                  if (File.Exists(thumbnailImageL))
                  {
                    pictureList.Add(thumbnailImageL);
                  }
                }
              }
              if (pictureList.Count > 0)
              {
                Util.Utils.Shuffle(pictureList);
                if (pictureList.Count > 4)
                {
                  pictureList.RemoveRange(4, pictureList.Count - 4);
                }
                // combine those 4 image files into one folder.jpg
                Util.Utils.CreateFolderPreviewThumb(pictureList, Path.Combine(path, @"Folder.jpg"));
              }
            }
          } // if (countVideos == itemlist.Count - 1)
        } // if (!vDir.IsRemote(path))
        benchclock.Stop();
        Log.Debug("GUIPictures: Creation of all thumbs for dir '{0}' took {1} seconds for {2} files", 
                                _filepath, benchclock.Elapsed.TotalSeconds, itemlist.Count);
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
        List<PictureData> aPictures = new List<PictureData>();
        if (PictureDatabase.FilterPrivate)
        {
          string SQL = "SELECT strFile FROM picturekeywords WHERE strKeyword = 'Private' AND strFile LIKE '" +
                               DatabaseUtility.RemoveInvalidChars(path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar) + "%';";
          aPictures = PictureDatabase.GetPicturesByFilter(SQL, "pictures");
          Log.Debug("GUIPictures: Load {0} private images for filter.", aPictures.Count);
        }

        foreach (GUIListItem subitem in itemlist)
        {
          if (!subitem.IsFolder)
          {
            if (!subitem.IsRemote && Util.Utils.IsPicture(subitem.Path))
            {
              if (aPictures.Count > 0)
              {
                if (aPictures.FirstOrDefault(x => x.FileName == subitem.Path) != null)
                {
                  continue;
                }
              }
              pictureList.Add(subitem.Path);
              if (pictureList.Count >= 4)
              {
                break;
              }
            }
          }
        }

        // No picture in the folder. Try to find in the subfolders.
        if (pictureList.Count == 0)
        {
          Log.Debug("GUIPictures: CreateFolderThumb: No picture in the {0}", path);
        }
        if (pictureList.Count < 4)
        {
          Log.Debug("GUIPictures: CreateFolderThumb: Try to find in the subfolders...");
          List<string> subPictureList = new List<string>();
          foreach (GUIListItem subitem in itemlist)
          {
            if (subitem.IsFolder && subitem.Path.Length > path.Length)
            {
              List<GUIListItem> subFolderList = vDir.GetDirectoryUnProtectedExt(subitem.Path, true);
              if (!recreateAll)
              {
                Filter(ref subFolderList);
              }
              int i = 0;
              foreach (GUIListItem subFolderItem in subFolderList)
              {
                if (!subFolderItem.IsFolder && !subFolderItem.IsRemote && Util.Utils.IsPicture(subFolderItem.Path))
                {
                  if (aPictures.Count > 0)
                  {
                    if (aPictures.FirstOrDefault(x => x.FileName == subFolderItem.Path) != null)
                    {
                      continue;
                    }
                  }
                  i++;
                  if (i == 1)
                  {
                    pictureList.Add(subFolderItem.Path);
                    Log.Debug("GUIPictures: CreateFolderThumb: Add file to folder.jpg {0}", subFolderItem.Path);
                  }
                  else
                  {
                    subPictureList.Add(subFolderItem.Path);
                  }
                  if (i >= 4)
                  {
                    break;
                  }
                }
              }
            }
            if (pictureList.Count >= 4)
            {
              break;
            }
          } // foreach (GUIListItem subitem in itemlist)
          if (pictureList.Count < 4)
          {
            Util.Utils.Shuffle(subPictureList);
            foreach (string strFile in subPictureList)
            {
              pictureList.Add(strFile);
              Log.Debug("GUIPictures: CreateFolderThumb: Add file to folder.jpg {0}", strFile);
              if (pictureList.Count >= 4)
              {
                break;
              }
            }
          }
        } // if (pictureList.Count < 4)

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
      foreach (string ext in Util.Utils.VideoExtensions)
      {
        vDir.AddExtension(ext);
      }

      if (!vDir.IsRemote(item))
      {
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          return;
        }
        if (string.IsNullOrEmpty(item))
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
          string thumbnailImage = Util.Utils.GetPicturesThumbPathname(item);
          string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item);

          if (recreateThumbs || !File.Exists(thumbnailImage) || !Util.Utils.FileExistsInCache(thumbnailImage) ||
              !File.Exists(thumbnailImageL) || !Util.Utils.FileExistsInCache(thumbnailImageL))
          {
            if (autocreateLargeThumbs && !File.Exists(thumbnailImageL))
            {
              thumbRet = Util.Picture.CreateThumbnail(item, thumbnailImageL, (int)Thumbs.ThumbLargeResolution,
                                                      (int)Thumbs.ThumbLargeResolution, iRotate,
                                                      Thumbs.SpeedThumbsLarge,
                                                      true, false);
            }
            if (!File.Exists(thumbnailImage))
            {
              thumbRet = Util.Picture.CreateThumbnail(item, thumbnailImage, (int)Thumbs.ThumbResolution,
                                                      (int)Thumbs.ThumbResolution, iRotate,
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
        else if (isVideo)
        {
          string thumbnailImage = Util.Utils.GetPicturesThumbPathname(item);
          string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item);

          thumbRet = File.Exists(thumbnailImage) && File.Exists(thumbnailImageL);
          if (!thumbRet)
          {
            thumbRet = VideoThumbCreator.CreateVideoThumb(item, thumbnailImage, true, false);
            if (thumbRet)
            {
              Log.Debug("GUIPictures: Creation of thumb successful for Video {0}", item);
            }
          }

          if (thumbRet)
          {
            itemObject.IconImage = thumbnailImage;
            itemObject.ThumbnailImage = thumbnailImageL;
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

    public enum Display
    {
      Files = 0,
      Date = 1,
      Keyword = 2,
      Metadata = 3
    }

    [SkinControl(6)] protected GUIButtonControl btnSlideShow = null;
    [SkinControl(7)] protected GUIButtonControl btnSlideShowRecursive = null;
    [SkinControl(8)] protected GUIButtonControl btnSearch = null;

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
    private static bool _searchMode = false;
    private static string _searchString = string.Empty;

    private ConcurrentQueue<GUIListItem> _queueItems;
    private ConcurrentQueue<GUIListItem> _queuePictures;
    private AutoResetEvent _queueItemsEvent;
    private AutoResetEvent _queuePicturesEvent;
    private Thread _threadAddPictures;
    private Thread _threadGetPicturesInfo;
    private bool _threadProcessPicturesStop = false;

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

        if (string.IsNullOrEmpty(currentFolder))
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
          disp = (Display)xmlreader.GetValueAsInt("pictures", "lastview", (int)disp);
          _searchMode = xmlreader.GetValueAsBool("pictures", "searchmode", false);
          _searchString = xmlreader.GetValueAsString("pictures", "searchstring", string.Empty);
          if (lastFolder != disp.ToString() + ":root")
          {
            currentFolder = lastFolder;
          }
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

    protected override void SaveSettings() { }

    #endregion

    #region Overrides

    protected override string SerializeName
    {
      get { return "mypicture"; }
    }

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
      btnViews.AddItem(GUILocalizeStrings.Get(134), index++);  // Shares
      btnViews.AddItem(GUILocalizeStrings.Get(636), index++);  // Date
      btnViews.AddItem(GUILocalizeStrings.Get(2167), index++); // Keyword
      btnViews.AddItem(GUILocalizeStrings.Get(2170), index++); // Metadata

      // Have the menu select the currently selected view.
      btnViews.SetSelectedItemByValue((int)disp);
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
      else if (((GUIWindow.Window)(Enum.Parse(typeof(GUIWindow.Window), GUIWindowManager.ActiveWindow.ToString())) == GUIWindow.Window.WINDOW_PICTURES) && !g_Player.Playing)
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
      {
        facadeLayout.EnableScrollLabel = method == SortMethod.Name;
      }
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
            LoadDirectory(string.Empty);
          }
          break;

        case 2: // Keyword
          if (disp != Display.Keyword)
          {
            disp = Display.Keyword;
            LoadDirectory(string.Empty);
          }
          break;

        case 3: // Metadata
          if (disp != Display.Metadata)
          {
            disp = Display.Metadata;
            LoadDirectory(string.Empty);
          }
          break;
      }
    }

    #endregion

    #region listview management

    private void ShowThumbPanel()
    {
      CurrentLayout = (Layout)mapSettings.ViewAs;
      SwitchLayout();
      UpdateButtonStates();
    }

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

    /// <summary>
    /// Set the selected item of the facadeLayout
    /// </summary>
    public void SetSelectedItemIndex(int index)
    {
      selectedItemIndex = CountOfNonImageItems + index;
    }

    protected override void SelectCurrentItem()
    {
      if (facadeLayout == null)
      {
        return;
      }

      selectedItemIndex = facadeLayout.SelectedListItemIndex;
      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
        GUIControl.FocusItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
      }
    }

    private void SelectItemByIndex(int itemIndex)
    {
      if (facadeLayout == null)
      {
        return;
      }
      if (itemIndex < 0)
      {
        itemIndex = 0;
      }
      if (itemIndex >= facadeLayout.Count)
      {
        itemIndex = facadeLayout.Count - 1;
      }

      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex);
    }

    private void SelectItemByName(string strName)
    {
      if (facadeLayout == null)
      {
        return;
      }
      if (string.IsNullOrEmpty(strName))
      {
        return;
      }

      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, 0);
      string itemName = strName.IndexOf(Path.DirectorySeparatorChar) > 0 ? Path.GetFileNameWithoutExtension(strName) : strName;
#if DEBUG
      Log.Debug("GUIPictures: Select item by name {0} - {1}", itemName, strName);
#endif
      for (int i = 0; i < facadeLayout.Count; i++)
      {
        if (facadeLayout[i].Label == itemName || facadeLayout[i].Path == strName)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, i);
          break;
        }
      }
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
          catch (Exception ex)
          {
            Log.Error("GUIPictures: CheckPathForHistory {0}", ex.Message);
          }
        }
        //Log.Debug("GUIPictures: MissingThumbCacher already working on path {0}", aPath);
        return false;
      }
    }

    private void LoadFolderSettings(string folderName)
    {
      if (string.IsNullOrEmpty(folderName))
      {
        folderName = disp.ToString() + ":root";
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

      StoreLastFolder(folderName);
      CurrentSortAsc = mapSettings.SortAscending;
      CurrentLayout = (Layout)mapSettings.ViewAs;
    }

    private void SaveFolderSettings(string folder)
    {
      if (string.IsNullOrEmpty(folder))
      {
        folder = disp.ToString() + ":root";
      }

      FolderSettings.AddFolderSetting(folder, "Pictures", typeof(MapSettings), mapSettings);
    }

    #endregion

    #region Mediaportal Load/Save Settings

    private void RestoreLastFolder()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        string lastFolder = xmlreader.GetValueAsString("pictures", "lastfolder", currentFolder);
        disp = (Display)xmlreader.GetValueAsInt("pictures", "lastview", (int)disp);
        _searchMode = xmlreader.GetValueAsBool("pictures", "searchmode", false);
        _searchString = xmlreader.GetValueAsString("pictures", "searchstring", string.Empty);
        if (lastFolder != disp.ToString() + ":root")
        {
          currentFolder = lastFolder;
        }
      }
    }

    private void StoreLastFolder(string folderName)
    {
      if (string.IsNullOrEmpty(folderName))
      {
        return;
      }

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        xmlreader.SetValue("pictures", "lastfolder", folderName);
        xmlreader.SetValue("pictures", "lastview", (int)disp);
        xmlreader.SetValueAsBool("pictures", "searchmode", _searchMode);
        xmlreader.SetValue("pictures", "searchstring", _searchString);
      }
    }

    private void ResetLeaveThumbsInFolder()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _tempLeaveThumbsInFolder = xmlreader.GetValueAsBool("thumbnails", "videosharepreview", false);
        xmlreader.SetValueAsBool("thumbnails", "videosharepreview", false);
      }
    }

    private void RestoreLeaveThumbsInFolder()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("thumbnails", "videosharepreview", _tempLeaveThumbsInFolder);
      }
    }

    private void LoadSelected()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        facadeLayout.SelectedListItemIndex = xmlreader.GetValueAsInt("pictures", "selected", -1);
      }
      SelectCurrentItem();
    }

    private void SaveSelected()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        xmlreader.SetValue("pictures", "selected", facadeLayout.SelectedListItemIndex);
      }
    }

    #endregion

    #region Sort Members

    protected virtual PictureSort.SortMethod CurrentSortMethod
    {
      get { return currentSortMethod; }
      set { currentSortMethod = value; }
    }

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
        {
          _virtualDirectory.AddExtension(ext);
        }
      }
      GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_OnNewMessage);

      _removableDrivesHandlerThread = new Thread(ListRemovableDrives);
      _removableDrivesHandlerThread.IsBackground = true;
      _removableDrivesHandlerThread.Name = "PictureRemovableDrivesHandlerThread";
      _removableDrivesHandlerThread.Start();
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
          GUIListItem item = GetItem(0);
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              _searchMode = false;
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
            _searchMode = false;
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
      else if (control == btnSearch) // Search by Keyword
      {
        OnSearch();
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
          if (string.IsNullOrEmpty(currentFolder) || currentFolder.Substring(0, 2) == message.Label)
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
          dlg.AddLocalizedString(735); // rotate
          dlg.AddLocalizedString(783); // rotate 180
          dlg.AddLocalizedString(784); // rotate 270
          dlg.AddLocalizedString(923); // show
          dlg.AddLocalizedString(108); // start slideshow
          dlg.AddLocalizedString(940); // properties
          dlg.AddLocalizedString(2168); // Update Exif
          if (disp != Display.Files)
          {
            dlg.AddLocalizedString(2169); // Go to Folder
          }
        }
        else
        {
          if (_refreshThumbnailsThread != null && _refreshThumbnailsThread.IsAlive)
          {
            dlg.AddLocalizedString(190000); // Abort thumbnail creation thread
          }
          else
          {
            dlg.AddLocalizedString(200047); // Recreate thumbnails (incl. subfolders)
            dlg.AddLocalizedString(190001); // Create missing thumbnails (incl. subfolders)
          }
          dlg.AddLocalizedString(200048); // Regenerate Thumbnails
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
        case 2168: // Update Exif
          {
            Log.Debug("GUIPictures: Update Exif {0}: {1}", PictureDatabase.UpdatePicture(item.Path, -1), item.Path);
            _queueItems.Enqueue(item);
            _queueItemsEvent.Set();
          }
          break;
        case 2169: // Go to Folder
          {
            string folder = Path.GetDirectoryName(item.Path);
            if (Directory.Exists(folder))
            {
              disp = Display.Files;
              // Have the menu select the currently selected view.
              btnViews.SetSelectedItemByValue((int)disp);
              UpdateButtonStates();
              LoadDirectory(folder);
            }
          }
          break;
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
        if (item.Label != ".." && disp == Display.Files)
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

          if (!Util.Utils.FileExistsInCache(thumbnailImage) && _autoCreateThumbs && (Util.Utils.IsPicture(item.Path) || Util.Utils.IsVideo(item.Path)))
          {
            ThreadPool.QueueUserWorkItem(delegate
                                           {
                                             try
                                             {
                                               GetThumbnailfile(ref item);
                                             }
                                             catch (Exception ex)
                                             {
                                               Log.Error("GUIPictures: Error loading next item (OnRetrieveThumbnailFiles) {0}", ex.Message);
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
        if (item.Label != ".." && disp == Display.Files)
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
      SelectItemByIndex(selectedItemIndex);
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
      if (item.IsFolder || item.IsRemote || !MediaPortal.Util.Utils.IsPicture(item.Path))
      {
        return;
      }

      if (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\PictureExifInfo.xml")))
      {
        SaveSelected();
        StoreLastFolder(currentFolder);

        GUIPicureExif pictureExif = (GUIPicureExif)GUIWindowManager.GetWindow((int)Window.WINDOW_PICTURE_EXIF);
        pictureExif.Picture = item.Path;
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_PICTURE_EXIF);
      }
      else
      {
        GUIDialogExif exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_EXIF);
        // Needed to set GUIDialogExif
        exifDialog.Restore();
        exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_EXIF);
        exifDialog.FileName = item.Path;
        exifDialog.DoModal(GetID);
        // Fix for Mantis issue: 0001709: Background not correct after viewing pictures properties twice
        exifDialog.Restore();
      }
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

    private void OnShowPicture(string strFile)
    {
      // Stop video playback before starting show picture to avoid MP freezing
      if (g_Player.MediaInfo != null && g_Player.MediaInfo.HasVideo || g_Player.IsTV || g_Player.IsVideo)
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
        if (item != null && !item.IsFolder)
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

    private void OnSearch()
    {
      bool searchByKeyword = disp == Display.Keyword;
      if (disp == Display.Files || disp == Display.Date)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

        if (dlg == null)
        {
          return;
        }

        dlg.Reset();
        dlg.SetHeading(137); // Seacrh

        // Dialog items
        dlg.AddLocalizedString(2167); // Keyword
        dlg.AddLocalizedString(2170); // Metadata

        // Show dialog menu
        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          return;
        }

        switch (dlg.SelectedId)
        {
          case 2167: // Keyword
            searchByKeyword = true;
            break;
          case 2168: // Metadata
            searchByKeyword = false;
            break;
        }
      }

      if (VirtualKeyboard.GetKeyboard(ref _searchString, GetID))
      {
        if (!string.IsNullOrEmpty(_searchString))
        {
          disp = searchByKeyword ? Display.Keyword : Display.Metadata;
          // Have the menu select the currently selected view.
          btnViews.SetSelectedItemByValue((int)disp);
          UpdateButtonStates();
          _searchMode = true;
          LoadDirectory(_searchString);
        }
      }
    }

    private void OnSlideShowRecursive()
    {
      // Stop video playback before starting show picture to avoid MP freezing
      if (g_Player.MediaInfo != null && g_Player.MediaInfo.HasVideo || g_Player.IsTV || g_Player.IsVideo)
      {
        g_Player.Stop();
      }
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
      {
        return;
      }

      StoreLastFolder(currentFolder);

      SlideShow.Reset();
      SlideShow._showRecursive = true;
      if (disp == Display.Files)
      {
        AddDir(SlideShow, currentFolder);
      }
      else if (disp == Display.Date)
      {
        if (string.IsNullOrEmpty(currentFolder))
        {
          string SQL = "SELECT strFile FROM picture";
          if (PictureDatabase.FilterPrivate)
          {
            SQL = SQL + " WHERE idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')";
          }
          List<PictureData> aPictures = PictureDatabase.GetPicturesByFilter(SQL, "pictures");
          foreach (PictureData pic in aPictures)
          {
            SlideShow.Add(pic.FileName);
          }
        }
        else
        {
          List<string> pics = new List<string>();
          int totalCount = PictureDatabase.ListPicsByDate(currentFolder.Replace(Path.DirectorySeparatorChar.ToString(), "-"), ref pics);
          foreach (string pic in pics)
          {
            SlideShow.Add(pic);
          }
        }
        if (_autoShuffle)
        {
          SlideShow.Shuffle(false, false);
        }
      }
      else if (disp == Display.Keyword)
      {
        if (string.IsNullOrEmpty(currentFolder))
        {
          string SQL = "SELECT strFile FROM picture";
          if (PictureDatabase.FilterPrivate)
          {
            SQL = SQL + " WHERE idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')";
          }
          List<PictureData> aPictures = PictureDatabase.GetPicturesByFilter(SQL, "pictures");
          foreach (PictureData pic in aPictures)
          {
            SlideShow.Add(pic.FileName);
          }
        }
        else
        {
          List<string> pics = PictureDatabase.ListPicsByKeyword(currentFolder.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty));
          foreach (string pic in pics)
          {
            SlideShow.Add(pic);
          }
        }
        if (_autoShuffle)
        {
          SlideShow.Shuffle(false, false);
        }
      }
      else if (disp == Display.Metadata)
      {
        if (string.IsNullOrEmpty(currentFolder))
        {
          string SQL = "SELECT strFile FROM picture";
          if (PictureDatabase.FilterPrivate)
          {
            SQL = SQL + " WHERE idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')";
          }
          List<PictureData> aPictures = PictureDatabase.GetPicturesByFilter(SQL, "pictures");
          foreach (PictureData pic in aPictures)
          {
            SlideShow.Add(pic.FileName);
          }
        }
        else if (!currentFolder.Contains(Path.DirectorySeparatorChar))
        {
          string SQL = "SELECT strFile FROM picturedata WHERE str" + currentFolder + " IS NOT NULL";
          if (PictureDatabase.FilterPrivate)
          {
            SQL = SQL + " AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')";
          }
          List<PictureData> aPictures = PictureDatabase.GetPicturesByFilter(SQL, "pictures");
          foreach (PictureData pic in aPictures)
          {
            SlideShow.Add(pic.FileName);
          }
        }
        else
        {
          string[] metaWhere = currentFolder.Split(Path.DirectorySeparatorChar);
          List<string> pics = PictureDatabase.ListPicsByMetadata(metaWhere[0].Trim(), metaWhere[1].Trim());
          foreach (string pic in pics)
          {
            SlideShow.Add(pic);
          }
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

    private void OnSlideShow(string strFile)
    {
      // Stop video playback before starting show picture to avoid MP freezing
      if (g_Player.MediaInfo != null && g_Player.MediaInfo.HasVideo || g_Player.IsTV || g_Player.IsVideo)
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

      StoreLastFolder(currentFolder);

      SlideShow.Reset();
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item != null && !item.IsFolder)
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
        SelectItemByIndex(selectedItemIndex);
      }

      dlgFile.DeInit();
      dlgFile = null;
    }

    private void OnCreateAllThumbs(GUIListItem item, bool Regenerate, bool Recursive)
    {
      CreateAllThumbs(item, Regenerate, Recursive);

      GUITextureManager.CleanupThumbs();

      LoadDirectory(currentFolder);
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item.IsFolder)
      {
        GUIPropertyManager.SetProperty("#pictures.filename", string.Empty);
        GUIPropertyManager.SetProperty("#pictures.path", currentFolder);
      }
      else
      {
        GUIPropertyManager.SetProperty("#pictures.filename", Path.GetFileName(item.Path));
        GUIPropertyManager.SetProperty("#pictures.path", Path.GetDirectoryName(item.Path));
      }

      int iDisp = 100002;
      if (disp == Display.Date)
      {
        if ((_useDayGrouping && currentFolder.Length <= 7) || (!_useDayGrouping && currentFolder.Length <= 4))
        {
          iDisp = 636;
        }
      }
      else if (disp == Display.Keyword)
      {
        if (string.IsNullOrEmpty(currentFolder))
        {
          iDisp = 2167;
        }
      }
      else if (disp == Display.Metadata)
      {
        if (string.IsNullOrEmpty(currentFolder) || (!_searchMode && !currentFolder.Contains(Path.DirectorySeparatorChar)))
        {
          iDisp = 2170;
        }
      }
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(iDisp));

      OnRetrieveThumbnailFiles(item);

      if (item.AlbumInfoTag == null)
      {
        if (!item.IsFolder)
        {
          SetItemExifData(item);
        }
        else
        {
          item.AlbumInfoTag = new ExifMetadata.Metadata();
        }
      }

      if (item.AlbumInfoTag is ExifMetadata.Metadata)
      {
        SetPictureProperties((ExifMetadata.Metadata)item.AlbumInfoTag);
      }

      GUIPropertyManager.SetProperty("#pictures.IsHDR", (item.AdditionalData & GUIListItemProperty.IsHDR) == GUIListItemProperty.IsHDR ? "true" : "false");
      GUIPropertyManager.SetProperty("#pictures.IsVideo", (item.AdditionalData & GUIListItemProperty.Is3D) == GUIListItemProperty.Is3D ? "true" : "false");

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
      // UpdateButtonStates();  -> fixing mantis bug 902
    }

    protected override void OnPageLoad()
    {
      ResetLeaveThumbsInFolder();
      base.OnPageLoad();

      GUIPropertyManager.SetProperty("#pictures.filename", string.Empty);
      GUIPropertyManager.SetProperty("#pictures.path", string.Empty);
      GUIPropertyManager.SetProperty("#pictures.IsHDR", string.Empty);
      GUIPropertyManager.SetProperty("#pictures.IsVideo", string.Empty);

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
      StartProcessPictures();

      GUIImageAllocator.ClearCachedAllocatorImages();
      GUITextureManager.CleanupThumbs();

      SetPictureProperties(new ExifMetadata.Metadata());

      if (!IsPictureWindow(PreviousWindowId))
      {
        _ageConfirmed = false;
        _currentPin = string.Empty;
        _currentProtectedShare.Clear();
        _protectedShares.Clear();
        GetProtectedShares(ref _protectedShares);
      }

      GUISlideShow SlideShow = null;
      string pictureFromSlideShow = string.Empty;
      returnFromSlideshow = (PreviousWindowId == (int)Window.WINDOW_SLIDESHOW || PreviousWindowId == (int)Window.WINDOW_PICTURES);
      if (returnFromSlideshow)
      {
        SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)Window.WINDOW_SLIDESHOW);
        if (SlideShow != null)
        {
          pictureFromSlideShow = SlideShow._folderCurrentItem;
          Log.Debug("GUIPictures: We return - CurrentSlideIndex {0}, File {1}", SlideShow._currentSlideIndex, pictureFromSlideShow);
          if (disp == Display.Files || disp == Display.Date)
          {
            currentFolder = GetCurrentFolderAfterReturn(pictureFromSlideShow);
          }
          else
          {
            RestoreLastFolder();
          }
        }
        else
        {
          returnFromSlideshow = false;
        }
      }

      if (PreviousWindowId == (int)Window.WINDOW_PICTURE_EXIF)
      {
        RestoreLastFolder();
      }

      LoadFolderSettings(currentFolder);
      ShowThumbPanel();

      LoadDirectory(currentFolder, true);

      if (selectedItemIndex >= 0 && returnFromSlideshow)
      {
        int direction = GUIPictureSlideShow.SlideDirection;
        Log.Debug("GUIPictures: CurrentSlideIndex: {0} Direction: {1}", SlideShow._currentSlideIndex, direction);

        GUIPictureSlideShow.SlideDirection = 0;
        g_Player.IsPicture = false;

        if (SlideShow._returnedFromVideoPlayback && !SlideShow._loadVideoPlayback)
        {
          if (direction == 0)
          {
            SlideShow.Reset();
          }
        }

        // Forward
        if (direction == 1)
        {
          selectedItemIndex++;
        }
        // Backward
        if (direction == -1)
        {
          selectedItemIndex--;
        }

        // Slide Show
        if (SlideShow._isSlideShow)
        {
          if (SlideShow._returnedFromVideoPlayback)
          {
            SlideShow._returnedFromVideoPlayback = false;
          }
          OnClickSlideShow(selectedItemIndex);
        }
        // OnClick
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
      }

      if (PreviousWindowId == (int)Window.WINDOW_PICTURE_EXIF)
      {
        LoadSelected();
        SelectItemByIndex(selectedItemIndex);
      }

      if (returnFromSlideshow && SlideShow != null)
      {
        MakeHistory(pictureFromSlideShow);
        SelectItemByName(pictureFromSlideShow);
      }
      else if (folderHistory.Count == 0 && !string.IsNullOrEmpty(currentFolder))
      {
        MakeHistory(currentFolder, true);
      }
      returnFromSlideshow = false;

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if ((_threadGetPicturesInfo != null && _threadGetPicturesInfo.IsAlive) || (_threadAddPictures != null && _threadAddPictures.IsAlive))
      {
        _threadProcessPicturesStop = true;

        _queueItemsEvent.Set();
        _queuePicturesEvent.Set();
      }

      GUIImageAllocator.ClearCachedAllocatorImages();
      GUITextureManager.CleanupThumbs();

      selectedItemIndex = GetSelectedItemNo();
      SaveSettings();
      SaveFolderSettings(currentFolder);

      if (_pictureFolderWatcher != null)
      {
        _pictureFolderWatcher.ChangeMonitoring(false);
      }

      // set back videosharepreview value
      RestoreLeaveThumbsInFolder();

      // Wait for thread ended ...
      if (_threadGetPicturesInfo != null)
      {
        _threadGetPicturesInfo.Join();
        _threadGetPicturesInfo = null;
      }
      if (_threadAddPictures != null)
      {
        _threadAddPictures.Join();
        _threadAddPictures = null;
      }

      _queueItemsEvent.Dispose();
      _queuePicturesEvent.Dispose();

      base.OnPageDestroy(newWindowId);
    }

    #endregion

    #region Page Load methods

    private string GetCurrentFolderAfterReturn(string slideName)
    {
      if (string.IsNullOrEmpty(slideName))
      {
        return string.Empty;
      }

      if (disp == Display.Files)
      {
        string folderName = Path.GetDirectoryName(slideName);
        if (!string.IsNullOrEmpty(folderName))
        {
          return folderName;
        }
      }
      else if (disp == Display.Date)
      {
        string dateTaken = PictureDatabase.GetDateTaken(slideName);
        if (!string.IsNullOrEmpty(dateTaken))
        {
          string year = dateTaken.Substring(0, 4);
          string month = dateTaken.Substring(5, 2);
          if (_useDayGrouping)
          {
            string day = dateTaken.Substring(8, 2);
            return year + "-" + month + "-" + day;
          }
          else
          {
            return year + "-" + month;
          }
        }
      }
      return string.Empty;
    }

    private void MakeHistory(string strPic)
    {
      MakeHistory(strPic, false);
    }

    private void MakeHistory(string strPic, bool isFolder)
    {
      if (string.IsNullOrEmpty(strPic))
      {
        return;
      }

      string rootFolder = string.Empty;

      if (disp == Display.Files)
      {
        string strCurrentPath = isFolder ? strPic.TrimEnd(Path.DirectorySeparatorChar) : Path.GetDirectoryName(strPic);
        rootFolder = _virtualDirectory.GetShare(strCurrentPath).Path.TrimEnd(Path.DirectorySeparatorChar);
        if (string.IsNullOrEmpty(rootFolder))
        {
          return;
        }
        strPic = strPic.Replace(rootFolder + Path.DirectorySeparatorChar, string.Empty);
      }
      else if (disp == Display.Date && !isFolder)
      {
        string dateTaken = PictureDatabase.GetDateTaken(strPic);
        if (!string.IsNullOrEmpty(dateTaken))
        {
          string strItemName = Util.Utils.GetFilename(strPic, true);
          string year = dateTaken.Substring(0, 4);
          string month = dateTaken.Substring(5, 2);
          strPic = year + Path.DirectorySeparatorChar + month;
          if (_useDayGrouping)
          {
            string day = dateTaken.Substring(8, 2);
            strPic = strPic + Path.DirectorySeparatorChar + day;
          }
          strPic = Path.Combine(strPic, strItemName);
        }
        else
        {
          return;
        }
      }

      Log.Debug("GUIPictures: Make history for {0}: {1}\{2}", disp.ToString(), rootFolder, strPic);

      string[] historyStep = strPic.Split(Path.DirectorySeparatorChar.ToString().ToArray(), StringSplitOptions.RemoveEmptyEntries);
      string historyFolder = rootFolder;
      for (int i = 0; i < historyStep.Count(); i++)
      {
        folderHistory.Set(historyStep[i], disp.ToString() + ":" + historyFolder);
#if DEBUG
        Log.Debug("GUIPictures: Make history for {0}: Folder: {1} Item: {2}", disp.ToString(), historyFolder, historyStep[i]);
#endif
        historyFolder = historyFolder + (string.IsNullOrEmpty(historyFolder) ? string.Empty : Path.DirectorySeparatorChar.ToString()) + historyStep[i];
      }
    }

    #endregion

    #region doXXX methods

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

    #endregion

    #region Thumbnails

    private void UpdateThumbnailsInFolder(GUIListItem item, bool Regenerate)
    {
      List<GUIListItem> itemlist = new List<GUIListItem>();

      ListFilesForUpdateThumbnails(item, ref itemlist);
      Log.Debug("GUIPictures: UpdateThumbnailsInFolder: Folder count {0}, regenerate all thumbs {1}", itemlist.Count, Regenerate);

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
            Log.Info("GUIPictures: RefreshThumbnailsThread: Aborted by the user.");
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
        Log.Error("GUIPictures: RefreshThumbnailsThread: {0}", ex);
      }
      finally
      {
        _progressDialogForRefreshThumbnails.Close();
      }
      Log.Info("GUIPictures: RefreshThumbnailsThread: Finished.");
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
      else if (disp == Display.Keyword)
      {
        // TODO: Thumbworker alternative on file base instead of directory
      }
      else if (disp == Display.Metadata)
      {
        // TODO: Thumbworker alternative on file base instead of directory
      }
    }

    public static string GetThumbnail(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
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
      if (string.IsNullOrEmpty(fileName))
      {
        return string.Empty;
      }

      if (Util.Utils.IsVideo(fileName))
      {
        return Util.Utils.GetVideosLargeThumbPathname(fileName);
      }
      return Util.Utils.GetPicturesLargeThumbPathname(fileName);
    }

    public static bool ThumbnailsThreadAbort
    {
      get { return _refreshThumbnailsThreadAbort; }
    }

    #endregion

    #region Various

    public Display GetDisplayMode
    {
      get { return disp; }
    }

    public static string GetCurrentFolder
    {
      get { return currentFolder; }
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

    private void ListRemovableDrives()
    {
      RemovableDrivesHandler.ListRemovableDrives(_virtualDirectory.GetDirectoryExt(string.Empty));
    }

    public void AddDir(GUISlideShow SlideShow, string strDir)
    {
      List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(strDir);
      itemlist.Sort(new PictureSort(CurrentSortMethod, CurrentSortAsc));
      Filter(ref itemlist);
      List<PictureData> aPictures = new List<PictureData>();
      if (PictureDatabase.FilterPrivate)
      {
        string SQL = "SELECT strFile FROM picturekeywords WHERE strKeyword = 'Private' AND strFile LIKE '" + DatabaseUtility.RemoveInvalidChars(strDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar) + "%';";
        aPictures = PictureDatabase.GetPicturesByFilter(SQL, "pictures");
        Log.Debug("GUIPictures: Load {0} private images for filter.", aPictures.Count);
      }
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
          if (aPictures.Count > 0)
          {
            if (aPictures.FirstOrDefault(x => x.FileName == item.Path) != null)
            {
              continue;
            }
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

        GUIListItem item = new GUIListItem(Util.Utils.GetFilename(path), string.Empty, path, true, fi);
        List<GUIListItem> itemlist = new List<GUIListItem>();
        item.IsFolder = Directory.Exists(path);

        if (ContainsFolderThumb(item))
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
        else
        {
          _queuePictures.Enqueue(item);
          _queuePicturesEvent.Set();
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
        Log.Error("GUIPictures.AddItem Exception: {0}", ex.Message);
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

    /// <summary>
    /// Returns true if the specified window should maintain virtual directory
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: if the specified window should maintain virtual directory
    /// false: if the specified window should not maintain virtual directory</returns>
    public static bool KeepVirtualDirectory(int windowId)
    {
      return (IsPictureWindow(windowId) || (windowId == (int)Window.WINDOW_FULLSCREEN_VIDEO));
    }

    private static bool ContainsFolderThumb(GUIListItem aItem)
    {
      if (!aItem.IsFolder && aItem.Path.ToLowerInvariant().Contains(@"folder.jpg"))
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

        if (serverName == _prevServerName && _wolResendTime * 60 > ts.TotalSeconds)
        {
          return true;
        }

        _prevWolTime = DateTime.Now;
        _prevServerName = serverName;

        try
        {
          Log.Debug("GUIPictures: WakeUpSrv: FolderName = {0}, ShareName = {1}, WOL enabled = {2}",
                     newFolderName, _virtualDirectory.GetShare(newFolderName).Name, wakeOnLanEnabled);
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

    public static bool IsPictureWindow(int windowId)
    {
      return (windowId == (int)Window.WINDOW_PICTURES || windowId == (int)Window.WINDOW_SLIDESHOW || windowId == (int)Window.WINDOW_PICTURE_EXIF);
    }

    #endregion

    #region Load Directory / Views

    protected override void LoadDirectory(string strNewDirectory)
    {
      LoadDirectory(strNewDirectory, false);
    }

    private void LoadDirectory(string strNewDirectory, bool waitUntilFinished)
    {
      if (strNewDirectory == null)
      {
        Log.Warn("GUIPictures: LoadDirectory called with invalid argument. newFolderName is null!");
        return;
      }

      if (facadeLayout == null)
      {
        return;
      }

      if (!WakeUpSrv(strNewDirectory))
      {
        return;
      }

      GUIWaitCursor.Show();

      SetPictureProperties(new ExifMetadata.Metadata());
      GUIListItem dummy;
      while (_queueItems.TryDequeue(out dummy));

      if (_pictureFolderWatcher != null)
      {
        _pictureFolderWatcher.ChangeMonitoring(false);
      }

      if (disp == Display.Files && !string.IsNullOrEmpty(strNewDirectory))
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
          if (SelectedItem.IsFolder)
          {
            if (SelectedItem.Label == "..")
            {
              folderHistory.Set(string.Empty, disp.ToString() + ":" + currentFolder);
            }
            else
            {
              folderHistory.Set(SelectedItem.Label, disp.ToString() + ":" + currentFolder);
            }
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

      AutoResetEvent _loadComplete = new AutoResetEvent(false);
    
      Thread worker = new Thread(() =>
      {
        try
        {
          Thread.CurrentThread.Name = "LoadPictures:" + disp.ToString();

          if (disp == Display.Files)
          {
            LoadFileView();
          }
          else if (disp == Display.Date)
          {
            LoadDateView(strNewDirectory);
          }
          else if (disp == Display.Keyword)
          {
            LoadKeywordView(strNewDirectory);
          }
          else if (disp == Display.Metadata)
          {
            LoadMetadataView(strNewDirectory);
          }

          SelectItemByName(folderHistory.Get(disp.ToString() + ":" + currentFolder));

          int totalItemCount = facadeLayout.Count;
          if (totalItemCount > 0)
          {
            GUIListItem rootItem = GetItem(0);
            if (rootItem.Label == "..")
            {
              totalItemCount--;
            }
          }

          //set object count label
          GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItemCount));

          ShowThumbPanel();
        }
        finally
        {
          GUIWaitCursor.Hide();
          _loadComplete.Set();
        }
      });
      worker.Start();
      if (waitUntilFinished)
        _loadComplete.WaitOne();
    }

    private void LoadFileView()
    {
      List<GUIListItem> itemlist;

      try
      {
        _removableDrivesHandlerThread.Join();

        itemlist = _virtualDirectory.GetDirectoryExt(currentFolder);

        if (string.IsNullOrEmpty(currentFolder))
        {
          RemovableDrivesHandler.FilterDrives(ref itemlist);
        }

        Filter(ref itemlist);

        if (_autoCreateThumbs)
        {
          MissingThumbCacher ThumbWorker = new MissingThumbCacher(currentFolder, _autocreateLargeThumbs, false, true);
        }

        List<PictureData> aPictures = new List<PictureData>();
        if (PictureDatabase.FilterPrivate)
        {
          string SQL = "SELECT strFile FROM picturekeywords WHERE strKeyword = 'Private' AND strFile LIKE '" + DatabaseUtility.RemoveInvalidChars(currentFolder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar) + "%';";
          aPictures = PictureDatabase.GetPicturesByFilter(SQL, "pictures");
          Log.Debug("GUIPictures: Load {0} private images for filter.", aPictures.Count);
        }

        // int itemIndex = 0;
        CountOfNonImageItems = 0;
        foreach (GUIListItem item in itemlist)
        {
          if (!item.IsFolder && aPictures.Count > 0)
          {
            if (aPictures.FirstOrDefault(x => x.FileName == item.Path) != null)
            {
              continue;
            }
          }

          if (!item.IsFolder)
          {
            if (Util.Utils.IsVideo(item.Path))
            {
              string thumbnailImage = Util.Utils.GetPicturesThumbPathname(item.Path);
              if (File.Exists(thumbnailImage))
              {
                item.IconImage = thumbnailImage;
              }

              string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item.Path);
              if (File.Exists(thumbnailImageL))
              {
                item.ThumbnailImage = thumbnailImageL;
              }
            }
          }

          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeLayout.Add(item);

          if (item.IsFolder)
          {
            CountOfNonImageItems++; // necessary to select the right item later from the slideshow
          }
          else
          {
            _queuePictures.Enqueue(item);
          }
        }
        OnSort();
        _queuePicturesEvent.Set();
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: Error loading file view - {0}", ex.ToString());
      }
    }

    private GUIListItem CreateAndAddFolderItem(string strLabel, string path, string thumb = null)
    {
      GUIListItem item = new GUIListItem(strLabel);
      item.Path = path;
      item.IsFolder = true;
      if (thumb == null)
      {
        Util.Utils.SetDefaultIcons(item);
      }
      else
      {
        item.IconImage = thumb;
        item.ThumbnailImage = item.IconImage;
      }
      item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
      item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
      facadeLayout.Add(item);
      CountOfNonImageItems++; // necessary to select the right item later from the slideshow
      return item;
    }

    private void LoadDateView(string strNewDirectory)
    {
      try
      {
        CountOfNonImageItems = 0;
        if (string.IsNullOrEmpty(strNewDirectory))
        {
          // Years
          List<string> Years = new List<string>();
          int Count = PictureDatabase.ListYears(ref Years);
          foreach (string year in Years)
          {
            CreateAndAddFolderItem(year, year);
          }
        }
        else if (strNewDirectory.Length == 4)
        {
          // Months
          string year = strNewDirectory.Substring(0, 4);
          CreateAndAddFolderItem("..", string.Empty);

          List<string> Months = new List<string>();
          int Count = PictureDatabase.ListMonths(year, ref Months);
          foreach (string month in Months)
          {
            // show month in a user friendly string
            CreateAndAddFolderItem(Util.Utils.GetNamedMonth(month), year + Path.DirectorySeparatorChar + month);
          }
        }

        string condition = string.Empty;

        // Check if day grouping is enabled
        if (_useDayGrouping)
        {
          if (strNewDirectory.Length == 7)
          {
            // Days
            string year = strNewDirectory.Substring(0, 4);
            string month = strNewDirectory.Substring(5, 2);
            CreateAndAddFolderItem("..", year);

            List<string> Days = new List<string>();
            int Count = PictureDatabase.ListDays(month, year, ref Days);
            foreach (string day in Days)
            {
              CreateAndAddFolderItem(day, year + Path.DirectorySeparatorChar + month + Path.DirectorySeparatorChar + day);
            }
          }
          else if (strNewDirectory.Length == 10)
          {
            // Pics from one day
            string year = strNewDirectory.Substring(0, 4);
            string month = strNewDirectory.Substring(5, 2);
            string day = strNewDirectory.Substring(8, 2);
            CreateAndAddFolderItem("..", year + Path.DirectorySeparatorChar + month);
            condition = year + "-" + month + "-" + day;
          }
        }
        else
        {
          if (strNewDirectory.Length == 7)
          {
            // Pics from one month
            string year = strNewDirectory.Substring(0, 4);
            string month = strNewDirectory.Substring(5, 2);

            CreateAndAddFolderItem("..", year);
            condition = year + "-" + month;
          }
        }

        if (!string.IsNullOrEmpty(condition))
        {
          List<string> pics = new List<string>();
          int Count = PictureDatabase.ListPicsByDate(condition, ref pics);
          AddPictureItems(pics, "GUIPictures: There is no file for this database entry: ");
        }

        if (facadeLayout.Count == 0 && !string.IsNullOrEmpty(strNewDirectory))
        {
          // Wrong path for date view, go back to top level
          currentFolder = string.Empty;
          LoadDateView(currentFolder);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: Error loading date view - {0}", ex.ToString());
      }
    }

    private void LoadKeywordView(string strNewDirectory)
    {
      try
      {
        CountOfNonImageItems = 0;
        if (string.IsNullOrEmpty(strNewDirectory)) // || strNewDirectory == "..")
        {
          _searchMode = false;

          // Keywords
          List<string> Keywords = PictureDatabase.ListKeywords();
          foreach (string keyword in Keywords)
          {
            CreateAndAddFolderItem(keyword, keyword);
          }
        }
        else
        {
          // Pics from Keyword / Search
          CreateAndAddFolderItem("..", string.Empty);

          List<string> pics;
          if (_searchMode)
          {
            pics = PictureDatabase.ListPicsByKeywordSearch(strNewDirectory);
          }
          else
          {
            pics = PictureDatabase.ListPicsByKeyword(strNewDirectory);
          }

          AddPictureItems(pics, "GUIPictures: There is no file for this keyword / search: " + strNewDirectory);
        }

        if (facadeLayout.Count == 0 && !string.IsNullOrEmpty(strNewDirectory))
        {
          _searchMode = false;
          // Wrong path for keyword view, go back to top level
          currentFolder = string.Empty;
          LoadKeywordView(string.Empty);
        }

      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: Error loading keyword view - {0}", ex.ToString());
      }
    }

    private void AddPictureItems(List<String> pics, string errorMessage)
    {
      VirtualDirectory vDir = new VirtualDirectory();
      vDir.LoadSettings("pictures");

      foreach (string pic in pics)
      {
        try
        {
          GUIListItem item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
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

          _queueItems.Enqueue(item);
          facadeLayout.Add(item);
          _queueItemsEvent.Set();
        }
        catch (Exception ex)
        {
          Log.Warn(errorMessage + " @ " + pic + ":" + ex.Message);
        }
      }
      _queueItemsEvent.Set();
    }

    private void LoadMetadataView(string strNewDirectory)
    {
      try
      {
        CountOfNonImageItems = 0;
        if (string.IsNullOrEmpty(strNewDirectory)) // || strNewDirectory == "..")
        {
          _searchMode = false;

          // Metadata
          Type type = typeof(ExifMetadata.Metadata);
          foreach (FieldInfo prop in type.GetFields())
          {
            if (prop.Name == nameof(ExifMetadata.Metadata.DatePictureTaken) ||
                prop.Name == nameof(ExifMetadata.Metadata.Keywords))
            {
              continue;
            }

            string caption = prop.Name.ToCaption() ?? prop.Name;
            CreateAndAddFolderItem(caption, prop.Name, Thumbs.Pictures + @"\exif\data\" + prop.Name + ".png");
          }
        }
        else if (!_searchMode && !strNewDirectory.Contains(Path.DirectorySeparatorChar))
        {
          // Value of Selected Metadata
          CreateAndAddFolderItem("..", string.Empty);

          List<string> metadatavalues = PictureDatabase.ListValueByMetadata(strNewDirectory.ToDBField());
          foreach (string value in metadatavalues)
          {
            string itemLabel = value.ToValue() ?? value;
            string thumbFilename = Thumbs.Pictures + @"\exif\data\" + strNewDirectory + ".png";

            if (strNewDirectory == nameof(ExifMetadata.Metadata.HDR))
            {
              string hdrValue = (value == "1" ? "Yes" : "No");
              itemLabel = hdrValue.ToValue() ?? hdrValue;
            }
            else if (strNewDirectory == nameof(ExifMetadata.Metadata.Location))
            {
              string[] geoValues = value.Split('|');
              double lat = 0;
              double lon = 0;
              if (double.TryParse(geoValues[0], NumberStyles.Any, CultureInfo.InvariantCulture, out lat) &&
                  double.TryParse(geoValues[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lon))
              {
                string latitude = lat.ToLatitudeString() ?? string.Empty;
                string longitude = lon.ToLongitudeString() ?? string.Empty;
                if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
                {
                  itemLabel = latitude + " / " + longitude;
                  string geoFile = Path.Combine(Thumbs.PicturesMaps, lat.ToFileName() + "-" + lon.ToFileName() + ".png");
                  if (File.Exists(geoFile))
                  {
                    thumbFilename = geoFile;
                  }
                }
                else
                {
                  continue;
                }
              }
              else
              {
                continue;
              }
            }
            else if (strNewDirectory == nameof(ExifMetadata.Metadata.Altitude))
            {
              double alt = 0;
              if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out alt))
              {
                itemLabel = alt.ToAltitudeString();
              }
            }

            GUIListItem item = CreateAndAddFolderItem(itemLabel, strNewDirectory + Path.DirectorySeparatorChar + value, thumbFilename);
            item.Label2 = strNewDirectory.ToCaption() ?? strNewDirectory;
          }
        }
        else
        {
          string[] metaWhere = strNewDirectory.Split('\\');

          // Pics from Metadata / Search
          CreateAndAddFolderItem("..", _searchMode ? string.Empty : metaWhere[0].Trim());

          List<string> pics;
          if (_searchMode)
          {
            pics = PictureDatabase.ListPicsBySearch(strNewDirectory);
          }
          else
          {
            pics = PictureDatabase.ListPicsByMetadata(metaWhere[0].Trim().ToDBField(), metaWhere[1].Trim());
          }

          AddPictureItems(pics, "GUIPictures: There is no file for this metadata / search:" + strNewDirectory);
        }

        if (facadeLayout.Count == 0 && !string.IsNullOrEmpty(strNewDirectory))
        {
          _searchMode = false;
          // Wrong path for keyword view, go back to top level
          currentFolder = string.Empty;
          LoadMetadataView(string.Empty);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: Error loading metadata view - {0}", ex.ToString());
      }
    }

    #endregion

    #region Fill Pictures Info

    protected void StartProcessPictures()
    {
      _threadProcessPicturesStop = false;
      _queuePicturesEvent = new AutoResetEvent(false);
      _queueItemsEvent = new AutoResetEvent(false);

      _queueItems = new ConcurrentQueue<GUIListItem>();
      _threadGetPicturesInfo = new Thread(ThreadGetPicturesInfo);
      _threadGetPicturesInfo.Priority = ThreadPriority.Lowest;
      _threadGetPicturesInfo.IsBackground = true;
      _threadGetPicturesInfo.Name = "GUIPictures GetPicturesInfo";
      _threadGetPicturesInfo.Start();

      _queuePictures = new ConcurrentQueue<GUIListItem>();
      _threadAddPictures = new Thread(ThreadAddPictures);
      _threadAddPictures.Priority = ThreadPriority.Lowest;
      _threadAddPictures.IsBackground = true;
      _threadAddPictures.Name = "GUIPictures AddPictures";
      _threadAddPictures.Start();
    }

    private void SetItemExifData(GUIListItem item)
    {
      if (item.IsFolder)
      {
        return;
      }

      VirtualDirectory vDir = new VirtualDirectory();
      vDir.SetExtensions(Util.Utils.PictureExtensions);

      string file = item.Path;

      DateTime datetime = PictureDatabase.GetDateTimeTaken(file);
      if (disp != Display.Files)
      {
        item.Label2 = datetime.ToString();
      }
      // item.Label3 = datetime.ToString();

      item.AlbumInfoTag = PictureDatabase.GetExifFromDB(file);
      if (((ExifMetadata.Metadata)item.AlbumInfoTag).HDR)
      {
        item.AdditionalData = item.AdditionalData | GUIListItemProperty.IsHDR;
      }
      if (Util.Utils.IsVideo(file))
      {
        item.AdditionalData = item.AdditionalData | GUIListItemProperty.Is3D; // Video file
      }

      if (item.FileInfo == null || string.IsNullOrEmpty(item.FileInfo.Name))
      {
        if (!vDir.IsRemote(file))
        {
          if (File.Exists(file))
          {
            item.FileInfo = new FileInformation(file, false);
          }
        }
      }
      if (item.FileInfo != null && datetime != DateTime.MinValue && item.FileInfo.CreationTime != datetime)
      {
        item.FileInfo.CreationTime = datetime;
      }
    }

    private void ThreadGetPicturesInfo()
    {
      Log.Debug("GUIPictures: ThreadGetPicturesInfo started...");
      try
      {
        while (!_threadProcessPicturesStop)
        {
          if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
          {
            break;
          }

          GUIListItem item;
          while (!_threadProcessPicturesStop && _queueItems.TryDequeue(out item))
          {
            if (string.IsNullOrEmpty(item.Path) || item.IsFolder)
            {
              continue;
            }
            SetItemExifData(item);
          }
          SelectCurrentItem();
          _queueItemsEvent.WaitOne();
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: ThreadGetPicturesInfo: {0}", ex.ToString());
      }
      Log.Debug("GUIPictures: ThreadGetPicturesInfo ended.");
    }

    private void ThreadAddPictures()
    {
      Log.Debug("GUIPictures: ThreadAddPictures started...");
      try
      {
        while (!_threadProcessPicturesStop)
        {
          GUIListItem item;
          while (!_threadProcessPicturesStop && _queuePictures.TryDequeue(out item))
          {
            if (string.IsNullOrEmpty(item.Path) || item.IsFolder)
            {
              continue;
            }

            /*
            if (PictureDatabase.AddPicture(item.Path, -1) > 0)
            {
              SetItemExifData(item);
            }
            */
            PictureDatabase.AddPicture(item.Path, -1);
          }
          SelectCurrentItem();
          _queuePicturesEvent.WaitOne();
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictures: ThreadAddPictures: {0}", ex.ToString());
      }
      Log.Debug("GUIPictures: ThreadAddPictures ended.");
    }

    #endregion

    #region Picture Properties

    private void SetPictureProperties(ExifMetadata.Metadata metadata)
    {
      metadata.SetExifProperties();

      if (metadata.IsEmpty())
      {
        GUIPropertyManager.SetProperty("#pictures.exif.images.vertical", string.Empty);
        GUIPropertyManager.SetProperty("#pictures.exif.images.horizontal", string.Empty);
        return;
      }

      int width = 96;
      int height = 0;

      List<GUIOverlayImage> exifIconImages = metadata.GetExifInfoOverlayImage(ref width, ref height);
      if (exifIconImages != null && exifIconImages.Count > 0)
      {
        GUIPropertyManager.SetProperty("#pictures.exif.images.vertical", GUIImageAllocator.BuildConcatImage("Exif:Icons:V:", string.Empty, width, height, exifIconImages));
      }
      else
      {
        GUIPropertyManager.SetProperty("#pictures.exif.images.vertical", string.Empty);
      }

      width = 0;
      height = 96;

      exifIconImages = metadata.GetExifInfoOverlayImage(ref width, ref height);
      if (exifIconImages != null && exifIconImages.Count > 0)
      {
        GUIPropertyManager.SetProperty("#pictures.exif.images.horizontal", GUIImageAllocator.BuildConcatImage("Exif:Icons:H:", string.Empty, width, height, exifIconImages));
      }
      else
      {
        GUIPropertyManager.SetProperty("#pictures.exif.images.horizontal", string.Empty);
      }
    }

    #endregion

    #region callback events

    public bool ThumbnailCallback()
    {
      return false;
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
      return "Frodo, ajs, doskabouter";
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