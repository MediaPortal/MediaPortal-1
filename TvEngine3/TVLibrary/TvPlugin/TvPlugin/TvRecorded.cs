#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Gentle.Common;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.Util;
using TvControl;
using TvDatabase;

//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using MediaPortal.Video.Database;
//using Toub.MediaCenter.Dvrms.Metadata;
///@using MediaPortal.TV.DiskSpace;

namespace TvPlugin
{
  public class TvRecorded : GUIInternalWindow, IComparer<GUIListItem>
  {
    #region ThumbCacher

    public class RecordingThumbCacher
    {
      private Work work;

      public RecordingThumbCacher()
      {
        work = new Work(new DoWorkHandler(this.PerformRequest));
        work.ThreadPriority = ThreadPriority.BelowNormal;
        GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
      }

      private void PerformRequest()
      {
        if (_thumbCreationActive)
        {
          return;
        }
        try
        {
          _thumbCreationActive = true;

          IList<Recording> recordings = Recording.ListAll();
          for (int i = recordings.Count - 1; i >= 0; i--)
          {
            string recFileName = TVUtil.GetFileNameForRecording(recordings[i]);
            string thumbNail = string.Format("{0}\\{1}{2}", Thumbs.TVRecorded,
                                             Path.ChangeExtension(Utils.SplitFilename(recFileName), null),
                                             Utils.GetThumbExtension());
            if ((!TVHome.UseRTSP()) && !File.Exists(thumbNail))
            {
              //Log.Info("RecordedTV: No thumbnail found at {0} for recording {1} - grabbing from file now", thumbNail, rec.FileName);

              //if (!DvrMsImageGrabber.GrabFrame(rec.FileName, thumbNail))
              //  Log.Info("GUIRecordedTV: No thumbnail created for {0}", Utils.SplitFilename(rec.FileName));
              try
              {
                Thread.Sleep(250);
                //MediaInfoWrapper recinfo = new MediaInfoWrapper(recFileName);
                //if (recinfo.IsH264)
                //{
                //  Log.Info("RecordedTV: Thumbnail creation not supported for h.264 file - {0}", Utils.SplitFilename(recFileName));
                //}
                //else
                //{
                if (VideoThumbCreator.CreateVideoThumb(recFileName, thumbNail, true, true))
                {
                  Log.Info("RecordedTV: Thumbnail successfully created for - {0}", Utils.SplitFilename(recFileName));
                }
                else
                {
                  Log.Info("RecordedTV: No thumbnail created for - {0}", Utils.SplitFilename(recFileName));
                }
                Thread.Sleep(250);
                //}

                // The .NET3 way....
                //
                //MediaPlayer player = new MediaPlayer();
                //player.Open(new Uri(rec.FileName, UriKind.Absolute));
                //player.ScrubbingEnabled = true;
                //player.Play();
                //player.Pause();
                //// Grab the frame 10 minutes after start to respect pre-recording times.
                //player.Position = new TimeSpan(0, 10, 0);
                //System.Threading.Thread.Sleep(5000);
                //RenderTargetBitmap rtb = new RenderTargetBitmap(720, 576, 1 / 200, 1 / 200, PixelFormats.Pbgra32);
                //DrawingVisual dv = new DrawingVisual();
                //DrawingContext dc = dv.RenderOpen();
                //dc.DrawVideo(player, new Rect(0, 0, 720, 576));
                //dc.Close();
                //rtb.Render(dv);
                //PngBitmapEncoder encoder = new PngBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(rtb));
                //using (FileStream stream = new FileStream(thumbNail, FileMode.OpenOrCreate))
                //{
                //  encoder.Save(stream);
                //}
                //player.Stop();
                //player.Close();
              }
              catch (Exception ex)
              {
                Log.Error("RecordedTV: No thumbnail created for {0} - {1}", Utils.SplitFilename(recFileName),
                          ex.Message);
              }
            }
          }
        }
        finally
        {
          _thumbCreationActive = false;
        }
      }
    }

    #endregion

    #region Variables

    private enum Controls
    {
      LABEL_PROGRAMTITLE = 13,
      LABEL_PROGRAMTIME = 14,
      LABEL_PROGRAMDESCRIPTION = 15,
      LABEL_PROGRAMGENRE = 17,
    } ;

    private enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
      Genre = 3,
      Played = 4,
      Duration = 5
    }

    private enum DBView
    {
      Recordings,
      Channel,
      Genre,
      History,
    }

    private GUIFacadeControl.ViewMode _currentViewMethod = GUIFacadeControl.ViewMode.List;
    private SortMethod _currentSortMethod = SortMethod.Date;
    private DBView _currentDbView = DBView.Recordings;
    private static Recording _oActiveRecording = null;
    private static bool _bIsLiveRecording = false;
    private static bool _thumbCreationActive = false;
    private static bool _createRecordedThumbs = true;
    private bool m_bSortAscending = true;
    private bool _deleteWatchedShows = false;
    private int _iSelectedItem = 0;
    private string _currentLabel = string.Empty;
    private int _rootItem = 0;

    private RecordingThumbCacher thumbworker = null;

    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIButtonControl btnView = null;
    [SkinControl(6)] protected GUIButtonControl btnCleanup = null;
    [SkinControl(7)] protected GUIButtonControl btnCompress = null;
    [SkinControl(50)] protected GUIFacadeControl facadeView = null;

    #endregion

    #region Constructor

    public TvRecorded()
    {
      GetID = (int)Window.WINDOW_RECORDEDTV;
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strTmp = xmlreader.GetValueAsString("tvrecorded", "sort", "channel");

        if (strTmp == "channel")
        {
          _currentSortMethod = SortMethod.Channel;
        }
        else if (strTmp == "date")
        {
          _currentSortMethod = SortMethod.Date;
        }
        else if (strTmp == "name")
        {
          _currentSortMethod = SortMethod.Name;
        }
        else if (strTmp == "type")
        {
          _currentSortMethod = SortMethod.Genre;
        }
        else if (strTmp == "played")
        {
          _currentSortMethod = SortMethod.Played;
        }
        else if (strTmp == "duration")
        {
          _currentSortMethod = SortMethod.Duration;
        }

        strTmp = xmlreader.GetValueAsString("tvrecorded", "view", "list");
        if (strTmp == "List")
        {
          _currentViewMethod = GUIFacadeControl.ViewMode.List;
        }
        else if (strTmp == "AlbumView")
        {
          _currentViewMethod = GUIFacadeControl.ViewMode.AlbumView;
        }
        else if (strTmp == "SmallIcons")
        {
          _currentViewMethod = GUIFacadeControl.ViewMode.SmallIcons;
        }
        else if (strTmp == "LargeIcons")
        {
          _currentViewMethod = GUIFacadeControl.ViewMode.LargeIcons;
        }
        else if (strTmp == "Filmstrip")
        {
          _currentViewMethod = GUIFacadeControl.ViewMode.Filmstrip;
        }

        facadeView.View = _currentViewMethod;
        facadeView.Focus = true;

        m_bSortAscending = xmlreader.GetValueAsBool("tvrecorded", "sortascending", true);
        _deleteWatchedShows = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
        _createRecordedThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
      }

      thumbworker = null;
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (_currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvrecorded", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvrecorded", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvrecorded", "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvrecorded", "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvrecorded", "sort", "played");
            break;
          case SortMethod.Duration:
            xmlwriter.SetValue("tvrecorded", "sort", "duration");
            break;
        }

        switch (_currentViewMethod)
        {
          case GUIFacadeControl.ViewMode.AlbumView:
            xmlwriter.SetValue("tvrecorded", "view", "AlbumView");
            break;
          case GUIFacadeControl.ViewMode.Filmstrip:
            xmlwriter.SetValue("tvrecorded", "view", "Filmstrip");
            break;
          case GUIFacadeControl.ViewMode.LargeIcons:
            xmlwriter.SetValue("tvrecorded", "view", "LargeIcons");
            break;
          case GUIFacadeControl.ViewMode.List:
            xmlwriter.SetValue("tvrecorded", "view", "List");
            break;
          case GUIFacadeControl.ViewMode.Playlist:
            xmlwriter.SetValue("tvrecorded", "view", "Playlist");
            break;
          case GUIFacadeControl.ViewMode.SmallIcons:
            xmlwriter.SetValue("tvrecorded", "view", "SmallIcons");
            break;
          default:
            xmlwriter.SetValue("tvrecorded", "view", "List");
            break;
        }

        xmlwriter.SetValueAsBool("tvrecorded", "sortascending", m_bSortAscending);
      }
    }

    #endregion

    #region Overrides

    public override bool Init()
    {
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayRecordingBackStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayRecordingBackEnded);
      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayRecordingBackStarted);
      g_Player.PlayBackChanged += new g_Player.ChangedHandler(OnPlayRecordingBackChanged);

      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvrecordedtv.xml");
      //LoadSettings();
      GUIWindowManager.Replace((int)Window.WINDOW_RECORDEDTV, this);
      Restore();
      PreInit();
      ResetAllControls();
      return bResult;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_DELETE_ITEM:
          {
            int item = GetSelectedItemNo();
            if (item >= 0)
            {
              OnDeleteRecording(item);
            }
            UpdateProperties();
          }
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
          if (facadeView != null)
          {
            if (facadeView.Focus)
            {
              GUIListItem item = GetItem(0);
              if (item != null)
              {
                if (item.IsFolder && item.Label == "..")
                {
                  _currentLabel = string.Empty;
                  LoadDirectory();
                  GUIControl.SelectItemControl(GetID, facadeView.GetID, _rootItem);
                  _rootItem = 0;
                  return;
                }
              }
            }
          }
          break;
      }
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      _iSelectedItem = GetSelectedItemNo();
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      TVHome.WaitForGentleConnection();

      base.OnPageLoad();
      DeleteInvalidRecordings();

      if (btnCompress != null)
      {
        btnCompress.Visible = false;
      }

      LoadSettings();
      LoadDirectory();

      while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
      {
        _iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeView.GetID, _iSelectedItem);

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      if (thumbworker == null)
      {
        if (_createRecordedThumbs)
        {
          _createRecordedThumbs = (!TVHome.UseRTSP());

          if (!_createRecordedThumbs)
          {
            Log.Info("GUIRecordedTV: skipping thumbworker thread - RTSP mode is in use");
          }
          else
          {
            thumbworker = new RecordingThumbCacher();
          }
        }
      }
      else
      {
        Log.Info("GUIRecordedTV: thumbworker already running - didn't start another one");
      }
    }

    protected bool AllowView(GUIFacadeControl.ViewMode view)
    {
      // Disable playlist for now as it makes no sense to move recording entries
      if (view == GUIFacadeControl.ViewMode.Playlist)
      {
        return false;
      }
      return true;
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnView)
      {
        ShowViews();
        return;
      }

      if (control == btnViewAs)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (_currentViewMethod)
          {
            case GUIFacadeControl.ViewMode.List:
              _currentViewMethod = GUIFacadeControl.ViewMode.Playlist;
              if (!AllowView(_currentViewMethod) || facadeView.PlayListView == null)
              {
                shouldContinue = true;
              }
              break;
            case GUIFacadeControl.ViewMode.Playlist:
              _currentViewMethod = GUIFacadeControl.ViewMode.SmallIcons;
              if (!AllowView(_currentViewMethod) || facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              break;
            case GUIFacadeControl.ViewMode.SmallIcons:
              _currentViewMethod = GUIFacadeControl.ViewMode.LargeIcons;
              if (!AllowView(_currentViewMethod) || facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              break;
            case GUIFacadeControl.ViewMode.LargeIcons:
              _currentViewMethod = GUIFacadeControl.ViewMode.AlbumView;
              if (!AllowView(_currentViewMethod) || facadeView.AlbumListView == null)
              {
                shouldContinue = true;
              }
              break;
            case GUIFacadeControl.ViewMode.AlbumView:
              _currentViewMethod = GUIFacadeControl.ViewMode.Filmstrip;
              if (!AllowView(_currentViewMethod) || facadeView.FilmstripView == null)
              {
                shouldContinue = true;
              }
              break;
            case GUIFacadeControl.ViewMode.Filmstrip:
              _currentViewMethod = GUIFacadeControl.ViewMode.List;
              if (!AllowView(_currentViewMethod) || facadeView.ListView == null)
              {
                shouldContinue = true;
              }
              break;
          }
        } while (shouldContinue);

        facadeView.View = _currentViewMethod;

        LoadDirectory();
      }

      if (control == btnSortBy) // sort by
      {
        switch (_currentSortMethod)
        {
          case SortMethod.Channel:
            _currentSortMethod = SortMethod.Date;
            break;
          case SortMethod.Date:
            _currentSortMethod = SortMethod.Name;
            break;
          case SortMethod.Name:
            _currentSortMethod = SortMethod.Genre;
            break;
          case SortMethod.Genre:
            _currentSortMethod = SortMethod.Played;
            break;
          case SortMethod.Played:
            _currentSortMethod = SortMethod.Duration;
            break;
          case SortMethod.Duration:
            _currentSortMethod = SortMethod.Channel;
            break;
        }
        OnSort();
      }

      if (control == btnCleanup)
      {
        OnCleanup();
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnSelectedRecording(iItem);
        }
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnShowContextMenu();
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          UpdateProperties();
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      int iItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return;
      }
      if (pItem.IsFolder)
      {
        return;
      }
      Recording rec = (Recording)pItem.TVTag;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();

      dlg.SetHeading(TVUtil.GetDisplayTitle(rec));

      dlg.AddLocalizedString(655); //Play recorded tv
      dlg.AddLocalizedString(656); //Delete recorded tv
      if (rec.TimesWatched > 0)
      {
        dlg.AddLocalizedString(830); //Reset watched status
      }
      if (!rec.Title.Equals("manual", StringComparison.CurrentCultureIgnoreCase))
      {
        dlg.AddLocalizedString(1041); //Upcoming episodes      
      }
      dlg.AddLocalizedString(1048); //Settings

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 656: // delete
          OnDeleteRecording(iItem);
          break;

        case 655: // play
          if (OnSelectedRecording(iItem))
          {
            return;
          }
          break;

        case 1048: // Settings
          TvRecordedInfo.CurrentProgram = rec;
          GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_RECORDED_INFO);
          break;

        case 1041:
          ShowUpcomingEpisodes(rec);
          break;

        case 830: // Reset watched status
          _iSelectedItem = GetSelectedItemNo();
          ResetWatchedStatus(rec);
          LoadDirectory();
          GUIControl.SelectItemControl(GetID, facadeView.GetID, _iSelectedItem);
          break;
      }
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }

    #endregion

    #region Public methods

    public override bool IsTv
    {
      get { return true; }
    }

    public static Recording ActiveRecording()
    {
      return _oActiveRecording;
    }

    public static void SetActiveRecording(Recording rec)
    {
      _oActiveRecording = rec;
      _bIsLiveRecording = IsRecordingActual(rec);
    }

    public static bool IsLiveRecording()
    {
      return _bIsLiveRecording;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowVideo
    /// </summary>
    ///<returns></returns>
    private static bool ShowFullScreenWindowVideoHandler()
    {
      if (g_Player.IsTVRecording)
      {
        // watching TV
        if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_TVFULLSCREEN)
        {
          return true;
        }
        Log.Info("TVRecorded: ShowFullScreenWindow switching to fullscreen video");
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_TVFULLSCREEN);
        GUIGraphicsContext.IsFullScreenVideo = true;
        return true;
      }
      return g_Player.ShowFullScreenWindowVideoDefault();
    }

    private static void ShowUpcomingEpisodes(Recording rec)
    {
      try
      {
        Program ParamProg = new Program(rec.IdChannel, rec.StartTime, rec.EndTime, rec.Title, rec.Description, rec.Genre,
                                        Program.ProgramState.None, DateTime.MinValue, String.Empty, String.Empty,
                                        String.Empty, String.Empty, 0, String.Empty, 0);
        TVProgramInfo.CurrentProgram = ParamProg;
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_PROGRAM_INFO);
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error in ShowUpcomingEpisodes - {0}", ex.ToString());
      }
    }

    private void ShowViews()
    {
      try
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(652); // my recorded tv

        dlg.AddLocalizedString(914);
        dlg.AddLocalizedString(135);
        dlg.AddLocalizedString(915);
        dlg.AddLocalizedString(636); //TODO: Implement proper view
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedId)
        {
          case 914: //	all
            _currentDbView = DBView.Recordings;
            break;
          case 135: //	genres
            _currentDbView = DBView.Genre;
            break;
          case 915: //	channel
            _currentDbView = DBView.Channel;
            break;
          case 636: //	date
            _currentDbView = DBView.History;
            break;
        }
        // If we had been in 2nd group level - go up to root again
        _currentLabel = String.Empty;
        LoadDirectory();
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error in ShowViews - {0}", ex.ToString());
      }
    }

    /// <summary>
    /// Build an Outlook / Thunderbird like view grouped by date
    /// </summary>
    /// <param name="aStartTime">A recordings start time</param>
    /// <returns>The spoken date label</returns>
    private string GetSpokenViewDate(DateTime aStartTime)
    {
      DateTime compareDate = DateTime.Now.Subtract(DateTime.Now.Subtract(aStartTime));
      if (DateTime.Now.Subtract(aStartTime) < new TimeSpan(24, 0, 0))
        return GUILocalizeStrings.Get(6030); // "Today"
      else if (DateTime.Now.Subtract(aStartTime) < new TimeSpan(48, 0, 0))
        return GUILocalizeStrings.Get(6040); // "Yesterday"
      else if (DateTime.Now.Subtract(aStartTime) < new TimeSpan(72, 0, 0))
        return GUILocalizeStrings.Get(6041); // "Two days ago"
        //else if (DateTime.Now.Subtract(aStartTime) < new TimeSpan(168, 0, 0)) // current week
        //{
        //  switch (compareDate.DayOfWeek)
        //  {
        //    case DayOfWeek.Monday: return GUILocalizeStrings.Get(11);
        //    case DayOfWeek.Tuesday: return GUILocalizeStrings.Get(12);
        //    case DayOfWeek.Wednesday: return GUILocalizeStrings.Get(13);
        //    case DayOfWeek.Thursday: return GUILocalizeStrings.Get(14);
        //    case DayOfWeek.Friday: return GUILocalizeStrings.Get(15);
        //    case DayOfWeek.Saturday: return GUILocalizeStrings.Get(16);
        //    case DayOfWeek.Sunday: return GUILocalizeStrings.Get(12);
        //    default: return "Current week";
        //  }
        //}
        //else if (DateTime.Now.Subtract(aStartTime) < new TimeSpan(336, 0, 0)) // last week
        //  return "Last week";
      else if (DateTime.Now.Subtract(aStartTime) < new TimeSpan(672, 0, 0)) // current month
        return GUILocalizeStrings.Get(6060); // "Current month";
      else if (DateTime.Now.Year.Equals(compareDate.Year))
        return GUILocalizeStrings.Get(6070); // "Current year";
      else if (DateTime.Now.Year.Equals(compareDate.AddYears(1).Year))
        return GUILocalizeStrings.Get(6080); // "Last year";
      else return GUILocalizeStrings.Get(6090); // "Older";
    }

    private void LoadDirectory()
    {
      List<GUIListItem> itemlist = new List<GUIListItem>();
      try
      {
        GUIControl.ClearControl(GetID, facadeView.GetID);

        IList<Recording> recordings = Recording.ListAll();
        if (_currentLabel == string.Empty)
        {
          foreach (Recording rec in recordings)
          {
            // catch exceptions here so MP will go on and list further recs
            try
            {
              bool add = true;

              // combine recordings with the same name to a folder located on top
              foreach (GUIListItem item in itemlist)
              {
                if (item.TVTag != null)
                {
                  bool merge = false;
                  Recording listRec = item.TVTag as Recording;
                  if (listRec != null)
                  {
                    switch (_currentDbView)
                    {
                      case DBView.History:
                        merge = GetSpokenViewDate(rec.StartTime).Equals(GetSpokenViewDate(listRec.StartTime));
                        break;
                      case DBView.Recordings:
                        merge = rec.Title.Equals(listRec.Title, StringComparison.InvariantCultureIgnoreCase);
                        //merge = TVUtil.GetDisplayTitle(rec).Equals(listRec.Title, StringComparison.InvariantCultureIgnoreCase);
                        break;
                      case DBView.Channel:
                        merge = rec.IdChannel == listRec.IdChannel;
                        break;
                      case DBView.Genre:
                        merge = rec.Genre.Equals(listRec.Genre, StringComparison.InvariantCultureIgnoreCase);
                        break;
                    }
                    if (merge)
                    {
                      if (listRec.StartTime < rec.StartTime)
                      {
                        // Make sure that the folder items shows the information of the most recent subitem
                        // e.g. the Start time might be relevant for sorting the folders correctly.
                        item.TVTag = (BuildItemFromRecording(rec)).TVTag;
                      }

                      item.IsFolder = true;
                      // NO thumbnails for folders please so we can distinguish between single files and folders
                      Utils.SetDefaultIcons(item);
                      item.ThumbnailImage = item.IconImageBig;
                      add = false;
                      break;
                    }
                  }
                }
              }

              GUIListItem it = BuildItemFromRecording(rec);
              if (it != null)
              {
                if (add)
                {
                  // Add new list item for this recording
                  itemlist.Add(it);
                }
                else
                {
                  if (IsRecordingActual(rec))
                  {
                    for (int i = 0; i < itemlist.Count; i++)
                    {
                      if (itemlist[i].IsFolder &&
                          (TVUtil.GetDisplayTitle(rec).Equals(itemlist[i].Label,
                                                              StringComparison.InvariantCultureIgnoreCase) ||
                           (itemlist[i].Label.Equals(rec.Title, StringComparison.InvariantCultureIgnoreCase))))
                      {
                        it.IsFolder = true;
                        Utils.SetDefaultIcons(it);
                        itemlist.RemoveAt(i);
                        itemlist.Insert(i, it);
                        break;
                      }
                    }
                  }
                }
              }
            }
            catch (Exception recex)
            {
              Log.Error("TVRecorded: error processing recordings - {0}", recex.Message);
            }
          }
        }
        else
        {
          // Showing a merged folders content
          GUIListItem item = new GUIListItem("..");
          item.IsFolder = true;
          Utils.SetDefaultIcons(item);
          itemlist.Add(item);

          // Log.Debug("TVRecorded: Currently showing the virtual folder contents of {0}", _currentLabel);
          foreach (Recording rec in recordings)
          {
            bool addToList = true;
            switch (_currentDbView)
            {
              case DBView.History:
                addToList = GetSpokenViewDate(rec.StartTime).Equals(_currentLabel);
                break;
              case DBView.Recordings:
                addToList = rec.Title.Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase) ||
                            TVUtil.GetDisplayTitle(rec).Equals(_currentLabel,
                                                               StringComparison.InvariantCultureIgnoreCase);
                break;
              case DBView.Channel:
                addToList = GetRecordingDisplayName(rec).Equals(_currentLabel,
                                                                StringComparison.InvariantCultureIgnoreCase);
                break;
              case DBView.Genre:
                addToList = rec.Genre.Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase);
                break;
            }

            if (addToList)
            {
              // Add new list item for this recording
              item = BuildItemFromRecording(rec);
              item.Label = TVUtil.GetDisplayTitle(rec);
              if (item != null)
              {
                itemlist.Add(item);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error fetching recordings from database {0}", ex.Message);
      }

      try
      {
        foreach (GUIListItem item in itemlist)
        {
          // Ugly hack to remove the episode info from merged folders 
          // Since the first item above does not "known" it will be a folder's start item later
          // we need to do this here...
          if (item.IsFolder && _currentDbView == DBView.Recordings)
          {
            Recording listRec = item.TVTag as Recording;
            if (listRec != null)
            {
              item.Label = listRec.Title;
            }
          }

          facadeView.Add(item);
        }
      }
      catch (Exception ex2)
      {
        Log.Error("TvRecorded: Error adding recordings to list - {0}", ex2.Message);
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(itemlist.Count));

      OnSort();
      UpdateProperties();
    }

    public static string GetRecordingDisplayName(Recording rec)
    {
      if (rec == null)
      {
        return "";
      }

      Channel ch = rec.ReferencedChannel();

      if (ch == null)
      {
        return "";
      }

      return ch.DisplayName;
    }

    public static bool IsRecordingActual(Recording aRecording)
    {
      return aRecording.IsRecording;
    }

    private GUIListItem BuildItemFromRecording(Recording aRecording)
    {
      string strDefaultUnseenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoBig.png";
      string strDefaultSeenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoSeenBig.png";
      GUIListItem item = null;
      string strChannelName = GUILocalizeStrings.Get(2014); // unknown
      string strGenre = GUILocalizeStrings.Get(2014); // unknown

      try
      {
        Channel refCh = null;
        try
        {
          // Re-imported channels might still be valid but their channel does not need to be present anymore...
          refCh = aRecording.ReferencedChannel();
        }
        catch (Exception) {}
        if (refCh != null)
        {
          strChannelName = refCh.DisplayName;
        }
        if (!String.IsNullOrEmpty(aRecording.Genre))
        {
          strGenre = aRecording.Genre;
        }

        // Log.Debug("TVRecorded: BuildItemFromRecording [{0}]: {1} ({2}) on channel {3}", _currentDbView.ToString(), aRecording.Title, aRecording.Genre, strChannelName);
        item = new GUIListItem();
        switch (_currentDbView)
        {
          case DBView.Recordings:
            item.Label = TVUtil.GetDisplayTitle(aRecording);
            break;
          case DBView.Channel:
            item.Label = strChannelName;
            break;
          case DBView.Genre:
            item.Label = aRecording.Genre;
            break;
          case DBView.History:
            item.Label = GetSpokenViewDate(aRecording.StartTime);
            break;
        }

        item.TVTag = aRecording;

        // Set a default logo indicating the watched status
        string SmallThumb = aRecording.TimesWatched > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;
        string PreviewThumb = string.Format("{0}\\{1}{2}", Thumbs.TVRecorded,
                                            Path.ChangeExtension(Utils.SplitFilename(aRecording.FileName), null),
                                            Utils.GetThumbExtension());

        // Get the channel logo for the small icons
        string StationLogo = Utils.GetCoverArt(Thumbs.TVChannel, strChannelName);
        if (File.Exists(StationLogo))
        {
          SmallThumb = StationLogo;
        }

        // Display previews only if the option to create them is active
        if (File.Exists(PreviewThumb) /*&& _createRecordedThumbs*/)
        {
          // Search a larger one
          string PreviewThumbLarge = Utils.ConvertToLargeCoverArt(PreviewThumb);
          if (File.Exists(PreviewThumbLarge))
          {
            PreviewThumb = PreviewThumbLarge;
          }
          item.ThumbnailImage = item.IconImageBig = PreviewThumb;
        }
        else
        {
          // Fallback to Logo/Default icon
          item.IconImageBig = SmallThumb;
          item.ThumbnailImage = String.Empty;
        }
        item.IconImage = SmallThumb;

        //Mark the recording with a "rec. symbol" if it is an active recording.
        if (IsRecordingActual(aRecording))
        {
          item.PinImage = Thumbs.TvRecordingIcon;
        }
      }
      catch (Exception singleex)
      {
        item = null;
        Log.Warn("TVRecorded: Error building item from recording {0}\n{1}", aRecording.FileName, singleex.ToString());
      }

      return item;
    }

    private void UpdateButtonStates()
    {
      try
      {
        string strLine = string.Empty;
        if (btnSortBy != null)
        {
          switch (_currentSortMethod)
          {
            case SortMethod.Channel:
              strLine = GUILocalizeStrings.Get(620); //Sort by: Channel
              break;
            case SortMethod.Date:
              strLine = GUILocalizeStrings.Get(621); //Sort by: Date
              break;
            case SortMethod.Name:
              strLine = GUILocalizeStrings.Get(268); //Sort by: Title
              break;
            case SortMethod.Genre:
              strLine = GUILocalizeStrings.Get(678); //Sort by: Genre
              break;
            case SortMethod.Played:
              strLine = GUILocalizeStrings.Get(671); //Sort by: Watched
              break;
            case SortMethod.Duration:
              strLine = GUILocalizeStrings.Get(1017); //Sort by: Duration
              break;
          }
          GUIControl.SetControlLabel(GetID, btnSortBy.GetID, strLine);
          btnSortBy.IsAscending = m_bSortAscending;
        }
        if (btnViewAs != null)
        {
          switch (_currentViewMethod)
          {
            case GUIFacadeControl.ViewMode.AlbumView:
              strLine = GUILocalizeStrings.Get(529);
              break;
            case GUIFacadeControl.ViewMode.Filmstrip:
              strLine = GUILocalizeStrings.Get(733);
              break;
            case GUIFacadeControl.ViewMode.LargeIcons:
              strLine = GUILocalizeStrings.Get(417);
              break;
            case GUIFacadeControl.ViewMode.List:
              strLine = GUILocalizeStrings.Get(101);
              break;
              //case GUIFacadeControl.ViewMode.Playlist:
              //  break;
            case GUIFacadeControl.ViewMode.SmallIcons:
              strLine = GUILocalizeStrings.Get(100);
              break;
            default:
              strLine = GUILocalizeStrings.Get(101);
              break;
          }
          GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);
        }

        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTITLE);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMDESCRIPTION);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMGENRE);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTIME);
      }
      catch (Exception ex)
      {
        Log.Warn("TVRecorded: Error updating button states - {0}", ex.ToString());
      }
    }

    private void SetLabels()
    {
      SortMethod method = _currentSortMethod;
      bool bAscending = m_bSortAscending;

      for (int i = 0; i < facadeView.Count; ++i)
      {
        try
        {
          GUIListItem item1 = facadeView[i];
          if (item1.Label == "..")
          {
            continue;
          }
          Recording rec = (Recording)item1.TVTag;
          TimeSpan ts = rec.EndTime - rec.StartTime;

          string strTime = String.Format("{0} ({1})",
                                         Utils.GetNamedDate(rec.StartTime),
                                         Utils.SecondsToHMString((int)ts.TotalSeconds));

          // Do not display a duration in top level of History view
          if (_currentDbView != DBView.History || _currentLabel != String.Empty)
          {
            item1.Label2 = strTime;
          }
          if (_currentViewMethod != GUIFacadeControl.ViewMode.List)
          {
            item1.Label3 = GUILocalizeStrings.Get(2014); // unknown
            if (!String.IsNullOrEmpty(rec.Genre))
            {
              item1.Label3 = rec.Genre;
            }
          }

          if (_currentSortMethod == SortMethod.Channel)
          {
            item1.Label2 = GUILocalizeStrings.Get(2014); // unknown
            try
            {
              string Channel = GetRecordingDisplayName(rec);
              if (!String.IsNullOrEmpty(Channel))
              {
                item1.Label2 = Channel;
              }
            }
            catch (Exception) {}
          }

          if (rec.TimesWatched > 0)
          {
            if (!item1.IsFolder)
            {
              item1.IsPlayed = true;
            }
          }
          //Log.Debug("TvRecorded: SetLabels - 1: {0}, 2: {1}, 3: {2}", item1.Label, item1.Label2, item1.Label3);
        }
        catch (Exception ex)
        {
          Log.Warn("TVRecorded: error in SetLabels - {0}", ex.Message);
        }
      }
    }

    private bool OnSelectedRecording(int iItem)
    {
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return false;
      }
      if (pItem.IsFolder)
      {
        if (pItem.Label.Equals(".."))
        {
          _currentLabel = string.Empty;
        }
        else
        {
          _currentLabel = pItem.Label;
          _rootItem = iItem;
        }
        LoadDirectory();
        if (pItem.Label.Equals(".."))
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, _rootItem);
          _rootItem = 0;
        }
        return false;
      }

      Recording rec = (Recording)pItem.TVTag;
      IList<Recording> itemlist = Recording.ListAll();

      _oActiveRecording = rec;
      _bIsLiveRecording = false;
      TvServer server = new TvServer();
      foreach (Recording recItem in itemlist)
      {
        if (rec.IdRecording == recItem.IdRecording && IsRecordingActual(recItem))
        {
          _bIsLiveRecording = true;
          break;
        }
      }

      if (TVHome.Card != null)
      {
        TVHome.Card.StopTimeShifting();
      }

      int stoptime = rec.StopTime;
      if (stoptime > 0)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo)
        {
          return false;
        }
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
        string chName = GetRecordingDisplayName(rec);
        dlgYesNo.SetLine(1, chName);
        dlgYesNo.SetLine(2, rec.Title);
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936) + " " + Utils.SecondsToHMSString(rec.StopTime));
        dlgYesNo.SetDefaultToYes(true);
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
        if (!dlgYesNo.IsConfirmed)
        {
          stoptime = 0;
        }
      }
      else if (_bIsLiveRecording)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(rec.Title);
          dlg.AddLocalizedString(979); //Play recording from beginning
          dlg.AddLocalizedString(980); //Play recording from live point
          dlg.DoModal(GetID);
          if (dlg.SelectedId == 979) {}
          else if (dlg.SelectedId == 980)
          {
            TVHome.ViewChannelAndCheck(rec.ReferencedChannel());
            if (g_Player.Playing)
            {
              g_Player.ShowFullScreenWindow();
            }
            return true;
          }
        }
      }


      /*
              IMDBMovie movieDetails = new IMDBMovie();
              VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
              int idMovie = VideoDatabase.GetMovieId(rec.FileName);
              int idFile = VideoDatabase.GetFileId(rec.FileName);
              if (idMovie >= 0 && idFile >= 0 )
              {
                Log.Info("play got movie id:{0} for {1}", idMovie, rec.FileName);
                stoptime = VideoDatabase.GetMovieStopTime(idMovie);
                if (stoptime > 0)
                {
                  string title = System.IO.Path.GetFileName(rec.FileName);
                  if (movieDetails.Title != string.Empty) title = movieDetails.Title;

                  GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                  if (null == dlgYesNo) return false;
                  dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                  dlgYesNo.SetLine(1, rec.Channel);
                  dlgYesNo.SetLine(2, title);
                  dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936) + Utils.SecondsToHMSstring(stoptime));
                  dlgYesNo.SetDefaultToYes(true);
                  dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                  if (!dlgYesNo.IsConfirmed) stoptime = 0;
                }
              }
            */
      string fileName = TVUtil.GetFileNameForRecording(rec);

      bool useRTSP = TVHome.UseRTSP();

      Log.Info("TvRecorded Play:{0} - using rtsp mode:{1}", fileName, useRTSP);
      if (g_Player.Play(fileName, g_Player.MediaType.Recording))
      {
        if (Utils.IsVideo(fileName))
        {
          //g_Player.SeekAbsolute(0); //this seek sometimes causes a deadlock in tsreader. original problem still present.
          g_Player.ShowFullScreenWindow();
        }
        if (stoptime > 0)
        {
          g_Player.SeekAbsolute(stoptime);
        }

        //populates recording metadata to g_player;
        g_Player.currentFileName = rec.FileName;
        g_Player.currentTitle = rec.Title;
        g_Player.currentDescription = rec.Description;

        rec.TimesWatched++;
        rec.Persist();

        return true;
      }
      return false;
    }

    private void OnDeleteRecording(int iItem)
    {
      _iSelectedItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return;
      }
      if (pItem.IsFolder)
      {
        return;
      }
      Recording rec = (Recording)pItem.TVTag;

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }

      bool isRecPlaying = false;
      if (g_Player.currentFileName.Length > 0 && g_Player.IsTVRecording && g_Player.Playing)
      {
        FileInfo fInfo = new FileInfo(g_Player.currentFileName);
        isRecPlaying = (rec.FileName.IndexOf(fInfo.Name) > -1);
      }

      dlgYesNo.SetDefaultToYes(false);
      bool isRec = IsRecordingActual(rec);      
      TvServer server = new TvServer();
      if (isRec)
      {
        TvDatabase.Schedule sched = rec.ReferencedSchedule();
        TVUtil.DeleteRecAndSchedWithPrompt(sched);
      }
      else
      {
        if (rec.TimesWatched > 0)
        {
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));
        }
        else
        {
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(820));
        }
        string chName = GetRecordingDisplayName(rec);

        dlgYesNo.SetLine(1, chName);
        dlgYesNo.SetLine(2, rec.Title);
        dlgYesNo.SetLine(3, string.Empty);
        dlgYesNo.DoModal(GetID);
        if (!dlgYesNo.IsConfirmed)
        {
          return;
        }
        if (isRecPlaying)
        {
          g_Player.Stop();
        }
      }    

      TryDeleteRecordingAndNotifyUser(rec);

      CacheManager.Clear();

      LoadDirectory();
      while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
      {
        _iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeView.GetID, _iSelectedItem);
    }

    private void TryDeleteRecordingAndNotifyUser(Recording rec)
    {
      TvServer server = new TvServer();
      int timeout = 0;
      bool deleteRecording = false;      

      while (!deleteRecording && timeout < 5)      
      {
        deleteRecording = server.DeleteRecording(rec.IdRecording);        
        if (!deleteRecording)
        {
          timeout++;          
          Thread.Sleep(1000);
        }        
      }

      if (!deleteRecording)
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

        if (dlgOk != null)
        {
          dlgOk.SetHeading(257);
          dlgOk.SetLine(1, GUILocalizeStrings.Get(200054));
          dlgOk.SetLine(2, rec.Title);
          dlgOk.DoModal(GetID);
        }
      }
    }

    private void OnCleanup()
    {
      _iSelectedItem = GetSelectedItemNo();
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(200043)); //Cleanup recordings?
      dlgYesNo.SetLine(1, GUILocalizeStrings.Get(200050)); //This will delete your recordings from harddisc
      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(506)); // Are you sure?
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(200043)); //Cleanup recordings?

      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(676))); // Only watched recordings?
      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(200044))); // Only invalid recordings?
      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(200045))); // Both?
      if (_currentLabel != "")
      {
        dlg.Add(new GUIListItem(GUILocalizeStrings.Get(200049))); // Only watched recordings from this folder.
      }
      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(222))); // Cancel?
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      if (dlg.SelectedLabel > 4)
      {
        return;
      }

      if ((dlg.SelectedLabel == 0) || (dlg.SelectedLabel == 2))
      {
        DeleteWatchedRecordings(null);
      }
      if ((dlg.SelectedLabel == 1) || (dlg.SelectedLabel == 2))
      {
        DeleteInvalidRecordings();
      }
      if (dlg.SelectedLabel == 3 && _currentLabel != "")
      {
        DeleteWatchedRecordings(_currentLabel);
      }
      CacheManager.Clear();
      dlg.Reset();
      LoadDirectory();
      while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
      {
        _iSelectedItem--;
      }

      GUIControl.SelectItemControl(GetID, facadeView.GetID, _iSelectedItem);
    }

    private void DeleteWatchedRecordings(string _currentTitle)
    {
      IList<Recording> itemlist = Recording.ListAll();
      TvServer server = new TvServer();
      foreach (Recording rec in itemlist)
      {
        if (rec.TimesWatched > 0)
        {
          if (_currentTitle == null || _currentTitle == rec.Title)
          {
            server.DeleteRecording(rec.IdRecording);
          }
        }
      }
    }

    private void DeleteInvalidRecordings()
    {
      IList<Recording> itemlist = Recording.ListAllActive();
      TvServer server = new TvServer();
      bool foundInvalidRecording = false;
      foreach (Recording rec in itemlist)
      {
        server.DeleteRecording(rec.IdRecording);
        foundInvalidRecording = true;
      }
      if (foundInvalidRecording)
      {
        CacheManager.Clear();
        LoadDirectory();
        while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
        {
          _iSelectedItem--;
        }

        GUIControl.SelectItemControl(GetID, facadeView.GetID, _iSelectedItem);
      }
    }

    private void UpdateProperties()
    {
      try
      {
        Recording rec;
        GUIListItem pItem = GetItem(GetSelectedItemNo());
        if (pItem == null)
        {
          GUIPropertyManager.SetProperty("#selectedthumb", String.Empty);
          SetProperties(null);
          return;
        }
        rec = pItem.TVTag as Recording;
        if (rec == null)
        {
          SetProperties(null);
          return;
        }
        SetProperties(rec);
        if (!pItem.IsFolder)
        {
          GUIPropertyManager.SetProperty("#selectedthumb", pItem.ThumbnailImage);
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error updating properties - {0}", ex.ToString());
      }
    }

    private void SetProperties(Recording rec)
    {
      try
      {
        if (rec == null)
        {
          GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", "");
          GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", "");
          GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", "");
          GUIPropertyManager.SetProperty("#TV.RecordedTV.Channel", "");
          GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", "");
          GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "");
          return;
        }
        string strTime = string.Format("{0} {1} - {2}",
                                       Utils.GetShortDayString(rec.StartTime),
                                       rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.Title);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", rec.Genre);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", rec.Description);

        string strLogo = "";

        GUIPropertyManager.SetProperty("#TV.RecordedTV.Channel", GetRecordingDisplayName(rec));
        strLogo = Utils.GetCoverArt(Thumbs.TVChannel, GetRecordingDisplayName(rec));

        if (File.Exists(strLogo))
        {
          GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", strLogo);
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error setting item properties - {0}", ex.ToString());
      }
    }

    #endregion

    #region View management

    private GUIListItem GetSelectedItem()
    {
      return facadeView.SelectedListItem;
    }

    private GUIListItem GetItem(int iItem)
    {
      if (iItem < 0 || iItem >= facadeView.Count)
      {
        return null;
      }
      return facadeView[iItem];
    }

    private int GetSelectedItemNo()
    {
      if (facadeView.Count > 0)
      {
        return facadeView.SelectedListItemIndex;
      }
      else
      {
        return -1;
      }
    }

    private int GetItemCount()
    {
      return facadeView.Count;
    }

    #endregion

    #region Sort Members

    private void OnSort()
    {
      try
      {
        SetLabels();
        facadeView.Sort(this);
        UpdateButtonStates();
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error sorting items - {0}", ex.ToString());
      }
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      try
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

        int iComp = 0;
        TimeSpan ts;
        Recording rec1 = (Recording)item1.TVTag;
        Recording rec2 = (Recording)item2.TVTag;

        switch (_currentSortMethod)
        {
          case SortMethod.Played:
            item1.Label2 = string.Format("{0} {1}", rec1.TimesWatched, GUILocalizeStrings.Get(677)); //times
            item2.Label2 = string.Format("{0} {1}", rec2.TimesWatched, GUILocalizeStrings.Get(677)); //times
            if (rec1.TimesWatched == rec2.TimesWatched)
            {
              goto case SortMethod.Name;
            }
            else
            {
              if (m_bSortAscending)
              {
                return rec1.TimesWatched - rec2.TimesWatched;
              }
              else
              {
                return rec2.TimesWatched - rec1.TimesWatched;
              }
            }
          case SortMethod.Name:
            if (m_bSortAscending)
            {
              iComp = string.Compare(TVUtil.GetDisplayTitle(rec1), TVUtil.GetDisplayTitle(rec2), true);
              if (iComp == 0)
              {
                goto case SortMethod.Channel;
              }
              else
              {
                return iComp;
              }
            }
            else
            {
              iComp = string.Compare(TVUtil.GetDisplayTitle(rec2), TVUtil.GetDisplayTitle(rec1), true);
              if (iComp == 0)
              {
                goto case SortMethod.Channel;
              }
              else
              {
                return iComp;
              }
            }
          case SortMethod.Channel:
            if (m_bSortAscending)
            {
              iComp = string.Compare(rec1.ReferencedChannel().DisplayName, rec2.ReferencedChannel().DisplayName, true);
              if (iComp == 0)
              {
                goto case SortMethod.Date;
              }
              else
              {
                return iComp;
              }
            }
            else
            {
              iComp = string.Compare(rec2.ReferencedChannel().DisplayName, rec1.ReferencedChannel().DisplayName, true);
              if (iComp == 0)
              {
                goto case SortMethod.Date;
              }
              else
              {
                return iComp;
              }
            }
          case SortMethod.Duration:
            {
              TimeSpan duration1 = (rec1.EndTime - rec1.StartTime);
              TimeSpan duration2 = rec2.EndTime - rec2.StartTime;
              if (m_bSortAscending)
              {
                if (duration1 == duration2)
                {
                  goto case SortMethod.Date;
                }
                if (duration1 > duration2)
                {
                  return 1;
                }
                return -1;
              }
              else
              {
                if (duration1 == duration2)
                {
                  goto case SortMethod.Date;
                }
                if (duration1 < duration2)
                {
                  return 1;
                }
                return -1;
              }
            }
          case SortMethod.Date:
            if (m_bSortAscending)
            {
              if (rec1.StartTime == rec2.StartTime)
              {
                return 0;
              }
              if (rec1.StartTime < rec2.StartTime)
              {
                return 1;
              }
              return -1;
            }
            else
            {
              if (rec1.StartTime == rec2.StartTime)
              {
                return 0;
              }
              if (rec1.StartTime > rec2.StartTime)
              {
                return 1;
              }
              return -1;
            }
          case SortMethod.Genre:
            item1.Label2 = rec1.Genre;
            item2.Label2 = rec2.Genre;
            if (rec1.Genre != rec2.Genre)
            {
              if (m_bSortAscending)
              {
                return string.Compare(rec1.Genre, rec2.Genre, true);
              }
              else
              {
                return string.Compare(rec2.Genre, rec1.Genre, true);
              }
            }
            if (rec1.StartTime != rec2.StartTime)
            {
              if (m_bSortAscending)
              {
                ts = rec1.StartTime - rec2.StartTime;
                return (int)(ts.Minutes);
              }
              else
              {
                ts = rec2.StartTime - rec1.StartTime;
                return (int)(ts.Minutes);
              }
            }
            if (rec1.IdChannel != rec2.IdChannel)
            {
              if (m_bSortAscending)
              {
                return string.Compare(rec1.ReferencedChannel().DisplayName, rec2.ReferencedChannel().DisplayName);
              }
              else
              {
                return string.Compare(rec2.ReferencedChannel().DisplayName, rec1.ReferencedChannel().DisplayName);
              }
            }
            if (TVUtil.GetDisplayTitle(rec1) != TVUtil.GetDisplayTitle(rec2))
            {
              if (m_bSortAscending)
              {
                return string.Compare(TVUtil.GetDisplayTitle(rec1), TVUtil.GetDisplayTitle(rec2));
              }
              else
              {
                return string.Compare(TVUtil.GetDisplayTitle(rec2), TVUtil.GetDisplayTitle(rec1));
              }
            }
            return 0;
        }
        return 0;
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error comparing files - {0}", ex.ToString());
        return 0;
      }
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != SortOrder.Descending;
      OnSort();
    }

    #endregion

    #region playback events

    private void doOnPlayBackStoppedOrChanged(g_Player.MediaType type, int stoptime, string filename, string caller)
    {
      Log.Info("TvRecorded:{0} {1} {2}", caller, type, filename);
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }

      TvBusinessLayer layer = new TvBusinessLayer();
      Recording rec = layer.GetRecordingByFileName(filename);
      if (rec != null)
      {
        if (stoptime >= g_Player.Duration)
        {
          stoptime = 0;
        }
        ; //temporary workaround before end of stream get's properly implemented
        rec.Refresh();
        rec.StopTime = stoptime;
        rec.Persist();
      }
      else
      {
        Log.Info("TvRecorded:{0} no recording found with filename {1}", caller, filename);
      }

      /*
            if (GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV, GetID, 0, 0, 0, null);
              msg.SendToTargetWindow = true;
              GUIWindowManager.SendThreadMessage(msg);
            }
            */
    }

    private void OnPlayRecordingBackChanged(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
    {
      doOnPlayBackStoppedOrChanged(type, stoptime, filename, "OnPlayRecordingBackChanged");
    }

    private void OnPlayRecordingBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      doOnPlayBackStoppedOrChanged(type, stoptime, filename, "OnPlayRecordingBackStopped");
    }

    private void OnPlayRecordingBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }

      g_Player.Stop();

      TvBusinessLayer layer = new TvBusinessLayer();
      Recording rec = layer.GetRecordingByFileName(filename);
      if (rec != null)
      {
        if (_deleteWatchedShows || rec.KeepUntil == (int)KeepMethodType.UntilWatched)
        {
          TvServer server = new TvServer();
          server.DeleteRecording(rec.IdRecording);
        }
        else
        {
          rec.Refresh();
          rec.StopTime = 0;
          rec.Persist();
        }
      }

      //@int movieid = VideoDatabase.GetMovieId(filename);
      //@if (movieid < 0) return;

      //@VideoDatabase.DeleteMovieStopTime(movieid);

      //@IMDBMovie details = new IMDBMovie();
      //@VideoDatabase.GetMovieInfoById(movieid, ref details);
      //@details.Watched++;
      //@VideoDatabase.SetWatched(details);
    }

    private void OnPlayRecordingBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }

      // set audio track based on user prefs.
      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int prefLangIdx = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Log.Debug("TVRecorded.OnPlayRecordingBackStarted(): setting audioIndex on tsreader {0}", prefLangIdx);
      g_Player.CurrentAudioStream = prefLangIdx;

      if (dualMonoMode != eAudioDualMonoMode.UNSUPPORTED)
      {
        g_Player.SetAudioDualMonoMode(dualMonoMode);
      }
      //@VideoDatabase.AddMovieFile(filename);
    }

    private void ResetWatchedStatus(Recording aRecording)
    {
      aRecording.TimesWatched = 0;
      aRecording.StopTime = 0;
      aRecording.Persist();
    }

    #endregion
  }
}