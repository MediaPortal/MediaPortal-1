#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Music.Amazon;
using System.Threading;

namespace MediaPortal.GUI.Music
{
  public class GUICoverArtGrabberProgress : GUIWindow
  {
    public delegate void CoverArtSelectedHandler(AlbumInfo albumInfo, string albumPath, bool bSaveToAlbumFolder, bool bSaveToThumbsFolder);
    public event CoverArtSelectedHandler CoverArtSelected;

    public delegate void CoverArtGrabDoneHandler(GUICoverArtGrabberProgress coverArtGrabberProgress);
    public event CoverArtGrabDoneHandler CoverArtGrabDone;

    private enum ControlIDs
    {
      LBL_OVERALL_PROGRESS = 10,
      LBL_CURRENT_PROGRESS = 11,
      LBL_FOLDERNAME = 12,
      LBL_CURRENT_ALBUM = 13,
      LBL_ALBUM_FILTERED_SEARCH = 14,
      PROG_OVERALL = 20,
      PROG_CURRENT = 21,
      BTN_START = 30,
      BTN_CANCEL = 31,
      CHECK_SKIP_IF_EXISTS = 40,
      CHECK_SAVE_TO_ALBUM_FOLDER = 41,
      CHECK_SAVE_TO_THUMBS_FOLDER = 42,
      IMG_OVERALL_PROG_BG = 100,
      IMG_CURRENT_PROG_BG = 101,
    }

    [SkinControlAttribute((int)ControlIDs.LBL_OVERALL_PROGRESS)]
    protected GUIFadeLabel lblOverallProgress = null;

    [SkinControlAttribute((int)ControlIDs.LBL_CURRENT_PROGRESS)]
    protected GUIFadeLabel lblCurrentProgress = null;

    [SkinControlAttribute((int)ControlIDs.LBL_FOLDERNAME)]
    protected GUILabelControl lblFolderName = null;

    [SkinControlAttribute((int)ControlIDs.LBL_CURRENT_ALBUM)]
    protected GUIFadeLabel lblCurrentAlbum = null;

    [SkinControlAttribute((int)ControlIDs.LBL_ALBUM_FILTERED_SEARCH)]
    protected GUIFadeLabel lblFilteredSearch = null;

    [SkinControlAttribute((int)ControlIDs.BTN_START)]
    protected GUIButtonControl btnStart = null;

    [SkinControlAttribute((int)ControlIDs.BTN_CANCEL)]
    protected GUIButtonControl btnCancel = null;

    [SkinControlAttribute((int)ControlIDs.PROG_OVERALL)]
    protected GUIProgressControl progOverall = null;

    [SkinControlAttribute((int)ControlIDs.PROG_CURRENT)]
    protected GUIProgressControl progCurrent = null;

    [SkinControlAttribute((int)ControlIDs.CHECK_SKIP_IF_EXISTS)]
    protected GUICheckMarkControl checkSkipExisting = null;

    [SkinControlAttribute((int)ControlIDs.CHECK_SAVE_TO_ALBUM_FOLDER)]
    protected GUICheckMarkControl checkSaveInAlbumFolder = null;

    [SkinControlAttribute((int)ControlIDs.CHECK_SAVE_TO_THUMBS_FOLDER)]
    protected GUICheckMarkControl checkSaveInThumbsFolder = null;

    [SkinControlAttribute((int)ControlIDs.IMG_OVERALL_PROG_BG)]
    protected GUIImage imgOverallProgBG = null;

    [SkinControlAttribute((int)ControlIDs.IMG_CURRENT_PROG_BG)]
    protected GUIImage imgCurrentProgBG = null;

    #region Base Dialog Variables

    //private bool m_bRunning = false;
    private bool m_bRefresh = false;
    private int m_dwParentWindowID = 0;
    private GUIWindow m_pParentWindow = null;

    #endregion

    #region Variables

    private bool m_bOverlay = false;
    private GUICoverArtGrabberResults.SearchDepthMode _SearchMode = GUICoverArtGrabberResults.SearchDepthMode.Album;

    private static bool _IsCancelledByUser = false;
    private int _AlbumCount = 0;
    private int _CurrentCoverArtIndex = 0;
    private int _OverallProgressPercent = 0;
    //private int _CurrentCoverArtProgress = 0;

    private string _TopLevelFolderName = string.Empty;
    private string SearchFolderProgressFormatString = string.Empty;
    private string SearchFolderNameFormatString = string.Empty;
    private string GrabbingAlbumNameFormatString = string.Empty;

    private int _TotalAlbumCount = 0;
    private List<Song> songs = new List<Song>();

    private bool _SkipIfCoverArtExists = true;
    private bool _SaveImageToAlbumFolder = true;
    private bool _SaveImageToThumbsFolder = true;
    private bool GrabInProgress = false;
    private bool _Abort = false;
    private bool _AbortedByUser = false;
    private bool _GrabCompletedSuccessfully = true;
    private bool _UseID3 = false;

    private int _CoversGrabbed = 0;
    private long ControlColorDisabled;
    private long ControlColorUnfocused;

    private MusicDatabase _MusicDatabase = null;
    private GUICoverArtGrabberResults GuiCoverArtResults = null;

    private System.Threading.Thread WaitCursorThread = null;
    private bool WaitCursorActive = false;
    private WaitCursor _WaitCursor = null;

    #endregion

    #region Properties

    public bool NeedsRefresh
    {
      get { return m_bRefresh; }
    }

    public GUICoverArtGrabberResults.SearchDepthMode SearchMode
    {
      get { return _SearchMode; }
      set { _SearchMode = value; }
    }

    public static bool IsCancelledByUser
    {
      get { return _IsCancelledByUser; }
    }

    public int CurrentCoverArtIndex
    {
      get { return _CurrentCoverArtIndex; }
      set
      {
        if (value < 0)
          value = 0;

        //if (value > 100)
        //    value = 100;

        if (_AlbumCount >= 0 && value > _AlbumCount)
          value = _AlbumCount;

        _CurrentCoverArtIndex = value;
        int progPrecent = 0;

        if (_AlbumCount > 0 && _CurrentCoverArtIndex > 0)
        {
          progPrecent = (int)(((float)_CurrentCoverArtIndex / (float)_AlbumCount) * 100f);
        }

        SetTotalProgressPercentage(progPrecent);
        SetOverallProgressLabel();
      }
    }

    public int OverallProgressPercent
    {
      get { return _OverallProgressPercent; }
      set
      {
        if (value < 0)
          value = 0;

        if (value > 100)
          value = 100;

        _OverallProgressPercent = value;
        progOverall.Percentage = _OverallProgressPercent;
        SetOverallProgressLabel();
      }
    }

    public List<Song> Songs
    {
      set { songs = value; }
    }

    public string TopLevelFolderName
    {
      get { return _TopLevelFolderName; }
      set { _TopLevelFolderName = value; }
    }

    public int TotalAlbumCount
    {
      get { return _TotalAlbumCount; }
    }

    public int CoversGrabbed
    {
      get { return _CoversGrabbed; }
    }

    public bool AbortedByUser
    {
      get { return _AbortedByUser; }
    }

    public bool GrabCompletedSuccessfully
    {
      get { return _GrabCompletedSuccessfully; }
    }

    public bool UseID3
    {
      get { return _UseID3; }
      set { _UseID3 = value; }
    }

    #endregion

    public GUICoverArtGrabberProgress()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_COVERART_GRABBER_PROGRESS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\MyMusicCoverArtGrabberProgress.xml");
    }

    public override void DeInit()
    {
      base.DeInit();

      if (GuiCoverArtResults != null)
        GuiCoverArtResults.AmazonWebService.AbortGrab = true;
    }

    public void Reset()
    {
      _CurrentCoverArtIndex = 0;
      _AlbumCount = 0;
      _CoversGrabbed = 0;
      _Abort = false;
      _AbortedByUser = false;
      _GrabCompletedSuccessfully = true;

      lblCurrentProgress.Label = "";
      lblOverallProgress.Label = "";

      lblOverallProgress.Visible = false;
      lblCurrentProgress.Visible = false;
      lblCurrentAlbum.Visible = false;

      lblCurrentAlbum.Label = "";
      lblFolderName.Label = "";
      lblFilteredSearch.Label = "";

      checkSkipExisting.TextAlignment = GUIControl.Alignment.ALIGN_RIGHT;
      checkSaveInAlbumFolder.TextAlignment = GUIControl.Alignment.ALIGN_RIGHT;
      checkSaveInThumbsFolder.TextAlignment = GUIControl.Alignment.ALIGN_RIGHT;

      checkSkipExisting.Selected = _SkipIfCoverArtExists;
      checkSaveInAlbumFolder.Selected = _SaveImageToAlbumFolder;
      checkSaveInThumbsFolder.Selected = _SaveImageToThumbsFolder;

      SetTotalProgressPercentage(0);
      SetCurrentProgressPercentage(0);
      ShowTotalProgressBar(false);
      ShowCurrentProgressBar(false);

      GUICoverArtGrabberResults.CancelledByUser = false;

      SearchFolderProgressFormatString = GUILocalizeStrings.Get(4504);

      if (SearchFolderProgressFormatString.Length == 0
          || SearchFolderProgressFormatString.IndexOf("{0}") == -1
          || SearchFolderProgressFormatString.IndexOf("{1}") == -1
          || SearchFolderProgressFormatString.IndexOf("{2}") == -1)
      {
        SearchFolderProgressFormatString = "{0}% - {1} of {2}";
      }

      SearchFolderNameFormatString = GUILocalizeStrings.Get(4505);

      if (SearchFolderNameFormatString.Length == 0
          || SearchFolderNameFormatString.IndexOf("{0}") == -1)
      {
        SearchFolderNameFormatString = "Searching {0}...";
      }

      GrabbingAlbumNameFormatString = GUILocalizeStrings.Get(4522);

      if (GrabbingAlbumNameFormatString.Length == 0
          || GrabbingAlbumNameFormatString.IndexOf("{0}") == -1)
      {
        GrabbingAlbumNameFormatString = "Grabbing {0}...";
      }

      string cancelBtnText = GUILocalizeStrings.Get(4517);

      if (cancelBtnText.Length == 0)
        cancelBtnText = "Close";

      btnCancel.Label = cancelBtnText;

      lblOverallProgress.AllowScrolling = false;
      lblCurrentProgress.AllowScrolling = false;
      lblCurrentAlbum.AllowScrolling = false;
      lblFilteredSearch.AllowScrolling = false;
    }

    private void EnableControls(bool enabled)
    {
      checkSkipExisting.Disabled = !enabled;
      checkSaveInAlbumFolder.Disabled = !enabled;

      // At a minumum the cover art needs to be saved to the thumbs folder
      // so we'll make sure the user can't uncheck this option...
      checkSaveInThumbsFolder.Disabled = true;
      btnStart.Disabled = !enabled;
      checkSaveInThumbsFolder.DisabledColor = ControlColorDisabled;

      if (enabled)
      {
        checkSkipExisting.DisabledColor = ControlColorUnfocused;
        checkSaveInAlbumFolder.DisabledColor = ControlColorUnfocused;

        string cancelBtnText = GUILocalizeStrings.Get(4517);

        if (cancelBtnText.Length == 0)
          cancelBtnText = "Close";

        btnCancel.Label = cancelBtnText;
      }

      else
      {
        checkSkipExisting.DisabledColor = ControlColorDisabled;
        checkSaveInAlbumFolder.DisabledColor = ControlColorDisabled;
        //checkSaveInThumbsFolder.DisabledColor = ControlColorDisabled;

        btnCancel.Label = GUILocalizeStrings.Get(222);
      }
    }

    #region Base Dialog Members

    void Close()
    {
      Console.WriteLine("Closing Cover Art Grabber Progress Window...");
      _Abort = true;

      if (GuiCoverArtResults != null)
        GuiCoverArtResults.AmazonWebService.AbortGrab = true;

      HideWaitCursor();
      EnableControls(true);

      if (!GrabInProgress)
        GUIWindowManager.ReplaceWindow(m_dwParentWindowID);
    }

    public void Show(int dwParentId)
    {
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, GetID, 0, null);
      GUIWindowManager.SendThreadMessage(msg);
    }

    #endregion

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      m_bOverlay = GUIGraphicsContext.Overlay;

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      Reset();

      SetSummaryInfo();
      SetOverallProgressLabel();

      ControlColorDisabled = btnStart.DisabledColor;
      ControlColorUnfocused = checkSaveInAlbumFolder.DisabledColor;
      EnableControls(true);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        //_Abort = true;
        Close();
        return;
      }

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(4514));
          return true;
        //break;

        default:
          return base.OnMessage(message);
      }

      //return base.OnMessage(message);
    }

    private void CheckForAppShutdown()
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      {
        GrabInProgress = false;
        _Abort = true;

        if (GuiCoverArtResults != null)
          GuiCoverArtResults.AmazonWebService.AbortGrab = true;
      }
    }

    void OnFindCoverArtDone(AmazonWebservice aws, EventArgs e)
    {
      string progressText = string.Format("{0}% -- Done", 100);
      SetCurrentCoverArtProgressLabel(progressText, 100);
      progCurrent.Percentage = 100;
      System.Windows.Forms.Application.DoEvents();
      GUIWindowManager.Process();
    }


    void OnFindCoverArtProgress(AmazonWebservice aws, int progressPercent)
    {
      string albumName = aws.AlbumName;

      if (albumName.Length == 0)
        albumName = GUILocalizeStrings.Get(4506);

      string progressText = GetCurrentProgressString(progressPercent, albumName);
      SetCurrentProgressPercentage(progressPercent);
      SetCurrentCoverArtProgressLabel(progressText, progressPercent);
      System.Windows.Forms.Application.DoEvents();
      GUIWindowManager.Process();
    }

    void OnAlbumNotFoundRetryingFiltered(AmazonWebservice aws, string origAlbumName, string filteredAlbumName)
    {
      lblCurrentAlbum.Label = string.Format(GrabbingAlbumNameFormatString, filteredAlbumName);
      lblFilteredSearch.Label = string.Format("{0} not found", origAlbumName);
      lblFilteredSearch.Visible = true;
      string progressText = GetCurrentProgressString(0, filteredAlbumName);
      SetCurrentProgressPercentage(0);
      SetCurrentCoverArtProgressLabel(progressText, 0);
      GUIWindowManager.Process();
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      switch (controlId)
      {
        case (int)ControlIDs.BTN_CANCEL:
          {
            Close();
            break;
          }

        case (int)ControlIDs.BTN_START:
          {
            GetCoverArt();
            break;
          }

        case (int)ControlIDs.CHECK_SAVE_TO_ALBUM_FOLDER:
          {
            _SaveImageToAlbumFolder = checkSaveInAlbumFolder.Selected;
            break;
          }

        case (int)ControlIDs.CHECK_SAVE_TO_THUMBS_FOLDER:
          {
            _SaveImageToThumbsFolder = checkSaveInThumbsFolder.Selected;
            break;
          }

        case (int)ControlIDs.CHECK_SKIP_IF_EXISTS:
          {
            _SkipIfCoverArtExists = checkSkipExisting.Selected;
            break;
          }
      }
    }

    private void ShowTotalProgressBar(bool bShow)
    {
      imgOverallProgBG.Visible = bShow;
      progOverall.Visible = progOverall.Percentage > 0 && bShow;
    }

    private void ShowCurrentProgressBar(bool bShow)
    {
      imgCurrentProgBG.Visible = bShow;
      progCurrent.Visible = progCurrent.Percentage > 0 && bShow;
    }

    public void SetTotalProgressPercentage(int iPercent)
    {
      progOverall.Visible = iPercent > 0;
      progOverall.Percentage = iPercent;
    }

    public void SetCurrentProgressPercentage(int iPercent)
    {
      progCurrent.Visible = iPercent > 0;
      progCurrent.Percentage = iPercent;
    }

    private void SetSummaryInfo()
    {
      lblFolderName.Label = new System.IO.DirectoryInfo(_TopLevelFolderName).Name;
    }

    private void SetOverallProgressLabel()
    {
      int percent = progOverall.Percentage;
      string progressText = string.Format("{0}% - Getting cover art {1} of {2}", percent, _CurrentCoverArtIndex, _AlbumCount);
      lblOverallProgress.Label = progressText;
      GUIControl.RefreshControl(GetID, (int)ControlIDs.PROG_OVERALL);
      System.Windows.Forms.Application.DoEvents();
    }

    private void SetCurrentCoverArtProgressLabel(string progressText, int progressPercent)
    {
      lblCurrentProgress.Label = progressText;
    }

    private string GetCurrentProgressString(int percent, string albumName)
    {
      string progressText = string.Format("{0}% - {1}", percent, albumName);
      return progressText;
    }

    private void GetAlbumCount(string folderPath, ref int count)
    {
      System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(folderPath);
      System.IO.DirectoryInfo[] dirs = dirInfo.GetDirectories();
      //count += dirs.Length;

      foreach (System.IO.DirectoryInfo di in dirs)
      {
        try
        {
          CheckForAppShutdown();

          if (_Abort || _AbortedByUser || GUICoverArtGrabberResults.CancelledByUser)
            break;

          System.IO.FileInfo[] files = di.GetFiles();
          bool isMusicFolder = false;

          foreach (System.IO.FileInfo fi in files)
          {
            if (MediaPortal.Util.Utils.IsAudio(fi.FullName))
            {
              // This appears to be a music folder
              isMusicFolder = true;

              if (_SkipIfCoverArtExists)
              {
              }

              break;
            }
          }

          if (isMusicFolder)
            count++;

          GetAlbumCount(di.FullName, ref count);
          System.Windows.Forms.Application.DoEvents();
        }

        catch (Exception ex)
        {
          Log.Info("Cover art grabber exception:{0}", ex.ToString());
          continue;
        }

        GUIWindowManager.Process();
      }
    }

    private void GetCoverArtList(string folderPath, ref int albumCount, ref int curCount, bool skipIfCoverartExist, ref List<Song> songs)
    {
      System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(folderPath);
      System.IO.FileSystemInfo[] fsInfos = dirInfo.GetFileSystemInfos();
      bool foundTrackForThisDir = false;
      bool skippingAlbum = false;

      foreach (System.IO.FileSystemInfo fsi in fsInfos)
      {
        try
        {
          CheckForAppShutdown();

          if (_Abort || _AbortedByUser || GUICoverArtGrabberResults.CancelledByUser)
            break;

          // Is it a DirectoryInfo object...
          if (fsi is System.IO.DirectoryInfo)
          {
            //curCount++;
            foundTrackForThisDir = false;

            // Iterate through all sub-directories.
            GetCoverArtList(fsi.FullName, ref albumCount, ref curCount, skipIfCoverartExist, ref songs);
          }

        // ...or a FileInfo object?
          else if (fsi is System.IO.FileInfo)
          {
            // Have we already processed a track for this folder
            // and if so, is it an audio file?
            if (foundTrackForThisDir || !MediaPortal.Util.Utils.IsAudio(fsi.FullName))
              continue;

            string path = System.IO.Path.GetDirectoryName(fsi.FullName);
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
            System.IO.FileInfo[] files = null;

            if (di != null)
              files = di.GetFiles();

            string artist = string.Empty;
            bool isCompilation = false;
            int difArtistCount = 0;
            int foundAudioFileCount = 0;

            MusicTag tag = null;
            Song song = null;

            for (int i = 0; i < files.Length; i++)
            {
              if (foundAudioFileCount > 0)
                break;

              song = null;
              tag = null;

              string curTrackPath = files[i].FullName;

              if (!MediaPortal.Util.Utils.IsAudio(curTrackPath))
                continue;

              foundAudioFileCount++;
              song = new Song();
              if (_MusicDatabase.GetSongByFileName(curTrackPath, ref song) && UseID3)
              {
                // Make sure the the returned song has a valid file path
                if (song.FileName.Length == 0)
                  song.FileName = curTrackPath;

                if (artist == string.Empty)
                {
                  artist = song.Artist;
                  continue;
                }

                if (artist.ToLower().CompareTo(song.Artist.ToLower()) != 0)
                  difArtistCount++;
              }

              else
              {
                song = null;
                tag = TagReader.TagReader.ReadTag(curTrackPath);

                if (tag != null)
                {
                  if (artist == string.Empty)
                  {
                    artist = tag.Artist;
                    continue;
                  }

                  if (artist.ToLower().CompareTo(tag.Artist.ToLower()) != 0)
                    difArtistCount++;
                }
              }
            }

            if (difArtistCount > 0)
              isCompilation = true;

            if (song == null)
            {
              if (tag != null)
              {
                song = new Song();
                song.FileName = fsi.FullName;
                song.Album = tag.Album;

                if (isCompilation)
                  song.Artist = "";

                song.Artist = tag.Artist;
                song.Title = tag.Title;
                song.Track = tag.Track;
              }
            }

            if (song != null)
            {
              curCount++;

              //Log.Info("Cover art grabber:updating status for {0}", song.Album);
              UpdateAlbumScanProgress(song.Album, albumCount, curCount);
              foundTrackForThisDir = true;

              if (_SkipIfCoverArtExists && GUIMusicBaseWindow.CoverArtExists(song.Artist, song.Album, song.FileName, _SaveImageToAlbumFolder))
              {
                continue;
              }

              if (skippingAlbum)
                continue;

              songs.Add(song);
            }
          }
        }

        catch (Exception ex)
        {
          Log.Info("Cover art grabber exception:{0}", ex.ToString());
          continue;
        }
      }
    }

    private void UpdateAlbumScanProgress(string album, int albumCount, int curCount)
    {
      int progPrecent = (int)(((float)curCount / (float)albumCount) * 100f);
      string statusText = string.Format(SearchFolderNameFormatString, album);
      string progressText = string.Format(SearchFolderProgressFormatString, progPrecent, curCount, albumCount);

      SetCurrentProgressPercentage(progPrecent);
      lblCurrentProgress.Label = progressText;
      lblCurrentAlbum.Label = statusText;
      GUIWindowManager.Process();
    }

    private void ShowWaitCursorAsync()
    {
      _WaitCursor = new WaitCursor();

      while (WaitCursorActive)
      {
        System.Threading.Thread.Sleep(800);
        GUIWindowManager.Process();
      }

      if (_WaitCursor != null)
      {
        _WaitCursor.Dispose();
        _WaitCursor = null;
      }
    }

    private void ShowWaitCursor()
    {
      HideWaitCursor();

      if (WaitCursorThread == null)
      {
        ThreadStart ts = new ThreadStart(ShowWaitCursorAsync);
        WaitCursorThread = new Thread(ts);
      }
      else
      {
        if (WaitCursorThread.IsAlive)
        {
          WaitCursorThread.Abort();
        }

        WaitCursorThread = null;
        ThreadStart ts = new ThreadStart(ShowWaitCursorAsync);
        WaitCursorThread = new Thread(ts);
      }

      WaitCursorActive = true;
      WaitCursorThread.IsBackground = true;
      WaitCursorThread.Name = "WaitCursorAsync";
      WaitCursorThread.Start();

      GUIWindowManager.Process();
    }

    private void HideWaitCursor()
    {
      if (!WaitCursorActive && _WaitCursor == null && WaitCursorThread == null)
        return;

      WaitCursorActive = false;

      // Dispose of the WaitCursor object
      if (_WaitCursor != null)
      {
        _WaitCursor.Dispose();
        _WaitCursor = null;
      }

      // Make sure the thread is dead
      if (WaitCursorThread != null)
      {
        if (WaitCursorThread.IsAlive)
          WaitCursorThread.Abort();

        WaitCursorThread = null;
      }
    }

    private void GetCoverArt()
    {
      if (!Util.Win32API.IsConnectedToInternet())
        return;

      GrabInProgress = true;
      GUICoverArtGrabberResults.CancelledByUser = false;
      _AbortedByUser = false;
      _Abort = false;

      btnStart.Focus = false;
      btnCancel.Focus = true;

      progCurrent.Percentage = 0;
      progOverall.Percentage = 0;

      lblCurrentAlbum.Label = "";
      lblCurrentProgress.Label = "";
      lblFilteredSearch.Label = "";
      lblFolderName.Label = "";
      lblOverallProgress.Label = "";

      EnableControls(false);

      // Force a redraw...
      GUIWindowManager.Process();

      if (_MusicDatabase == null)
        _MusicDatabase = MusicDatabase.Instance;

      int albumCount = 0;
      int curCount = 0;
      songs.Clear();

      try
      {
        ShowWaitCursor();
        string status = GUILocalizeStrings.Get(4503);

        if (status.Length == 0)
          status = "Getting album count. Please wait...";

        lblCurrentAlbum.Label = status;
        lblCurrentAlbum.Visible = true;
        lblFilteredSearch.Visible = true;

        GUIWindowManager.Process();

        Log.Info("Cover art grabber:getting folder count for {0}...", _TopLevelFolderName);
        GetAlbumCount(_TopLevelFolderName, ref albumCount);
        Log.Info("Cover art grabber:{0} folders found", albumCount);
      }

      finally
      {
        HideWaitCursor();
      }

      lblFilteredSearch.Label = "";
      lblCurrentProgress.Label = "";
      lblCurrentProgress.Visible = true;
      ShowTotalProgressBar(true);
      GUIWindowManager.Process();

      Log.Info("Cover art grabber:getting pending cover count...");
      GetCoverArtList(_TopLevelFolderName, ref albumCount, ref curCount, _SkipIfCoverArtExists, ref songs);
      Log.Info("Cover art grabber:{0} covers queued for update", albumCount);

      if (_Abort)
      {
        Cleanup();
        return;
      }

      _CoversGrabbed = 0;
      _AbortedByUser = false;
      _AlbumCount = songs.Count;

      try
      {
        if (_AlbumCount > 0)
        {
          GuiCoverArtResults = (GUICoverArtGrabberResults)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS);

          if (null == GuiCoverArtResults)
            return;

          GuiCoverArtResults.SearchMode = GUICoverArtGrabberResults.SearchDepthMode.Share;
          GuiCoverArtResults.FindCoverArtProgress += new GUICoverArtGrabberResults.FindCoverArtProgressHandler(OnFindCoverArtProgress);
          GuiCoverArtResults.FindCoverArtDone += new GUICoverArtGrabberResults.FindCoverArtDoneHandler(OnFindCoverArtDone);
          GuiCoverArtResults.AlbumNotFoundRetryingFiltered += new GUICoverArtGrabberResults.AlbumNotFoundRetryingFilteredHandler(OnAlbumNotFoundRetryingFiltered);

          ShowTotalProgressBar(true);
          ShowCurrentProgressBar(true);

          lblOverallProgress.Visible = true;
          lblCurrentProgress.Visible = true;

          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REFRESH, 0, 0, 0, GetID, 0, null);
          GUIWindowManager.SendMessage(msg);

          for (int i = 0; i < songs.Count; i++)
          {
            if (_Abort)
              break;

            lblFilteredSearch.Clear();
            lblFilteredSearch.Visible = false;

            Song curSong = songs[i];
            CurrentCoverArtIndex = i + 1;
            SetCurrentCoverArtProgressLabel(curSong.Album, 0);
            System.Windows.Forms.Application.DoEvents();

            lblCurrentAlbum.Label = string.Format(GrabbingAlbumNameFormatString, curSong.Album);
            string progressText = GetCurrentProgressString(0, curSong.Album);
            SetCurrentProgressPercentage(0);
            SetCurrentCoverArtProgressLabel(progressText, 0);
            GUIWindowManager.Process();

            if (curSong.FileName.Length == 0)
              continue;

            string albumPath = System.IO.Path.GetDirectoryName(curSong.FileName);
            GuiCoverArtResults.GetAlbumCovers(curSong.Artist, curSong.Album, albumPath, GetID, true);

            if (IsAbortedByUser())
              break;

            GuiCoverArtResults.DoModal(GetID);

            if (IsAbortedByUser())
              break;

            if (GuiCoverArtResults.SelectedAlbum != null)
            {
              _CoversGrabbed++;

              if (CoverArtSelected != null)
              {
                GuiCoverArtResults.SelectedAlbum.Artist = curSong.Artist;
                GuiCoverArtResults.SelectedAlbum.Album = curSong.Album;
                CoverArtSelected(GuiCoverArtResults.SelectedAlbum, albumPath, _SaveImageToAlbumFolder, _SaveImageToThumbsFolder);
              }
            }
          }
        }
      }

      catch (Exception ex)
      {
        Log.Info("Cover art grabber exception:{0}", ex.ToString());
        _GrabCompletedSuccessfully = false;
      }

      finally
      {
        Cleanup();
      }

      if (CoverArtGrabDone != null)
        CoverArtGrabDone(this);

      ShowResultsDialog(_AbortedByUser, _GrabCompletedSuccessfully, _CoversGrabbed, GetID);
    }

    private bool IsAbortedByUser()
    {
      if (GUICoverArtGrabberResults.CancelledByUser || GuiCoverArtResults.AmazonWebService.AbortGrab)
      {
        Log.Info("Cover art grabber:user aborted grab");

        _AbortedByUser = true;
        return true;
      }

      return false;
    }

    public void Cleanup()
    {
      if (GuiCoverArtResults != null)
      {
        GuiCoverArtResults.AmazonWebService.AbortGrab = true;
        GuiCoverArtResults.FindCoverArtProgress -= new GUICoverArtGrabberResults.FindCoverArtProgressHandler(OnFindCoverArtProgress);
        GuiCoverArtResults.FindCoverArtDone -= new GUICoverArtGrabberResults.FindCoverArtDoneHandler(OnFindCoverArtDone);
        GuiCoverArtResults.AlbumNotFoundRetryingFiltered -= new GUICoverArtGrabberResults.AlbumNotFoundRetryingFilteredHandler(OnAlbumNotFoundRetryingFiltered);
      }

      EnableControls(true);
      ShowTotalProgressBar(false);
      ShowCurrentProgressBar(false);

      lblOverallProgress.Visible = false;
      lblCurrentProgress.Visible = false;
      lblCurrentAlbum.Visible = false;
      lblFilteredSearch.Label = "";
      lblFilteredSearch.Visible = false;

      GrabInProgress = false;
    }

    public static void ShowResultsDialog(bool abortedByUser, bool completedSuccessfully, int coversGrabbed, int parentWindowID)
    {
      GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);

      if (dlg != null)
      {
        string line1Text = "";
        string line2Text = "";

        // The user cancelled the grab
        if (abortedByUser)
        {
          line1Text = GUILocalizeStrings.Get(4507);

          if (line1Text.Length == 0)
            line1Text = "Cover art grabber aborted by user";
        }

        else
        {
          if (completedSuccessfully)
          {
            if (coversGrabbed > 0)
            {
              line1Text = GUILocalizeStrings.Get(4508);

              if (line1Text.Length == 0)
                line1Text = "Cover art grab completed successfuly";
            }

        // The grab succeeded but all of the covers were up to date
            else
            {
              line1Text = GUILocalizeStrings.Get(4512);

              if (line1Text.Length == 0)
                line1Text = "All covers up to date";
            }
          }

          else
          {
            line1Text = GUILocalizeStrings.Get(4509);

            if (line1Text.Length == 0)
              line1Text = "Cover art grab completed with errors";
          }

          string fmtString = GUILocalizeStrings.Get(4510);

          if (fmtString.Length == 0 || fmtString.IndexOf("{0}") == -1)
            fmtString = "{0} cover art images updated";

          line2Text = string.Format(fmtString, coversGrabbed);
        }

        string caption = GUILocalizeStrings.Get(4511);

        if (caption.Length == 0)
          caption = "Cover Art Grabber Done";

        dlg.SetHeading(caption);
        dlg.SetLine(1, line1Text);
        dlg.SetLine(2, line2Text);
        dlg.DoModal(parentWindowID);
      }
    }
  }
}
