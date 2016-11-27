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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.TvPlugin.Helper;
using WindowPlugins;
using Action = MediaPortal.GUI.Library.Action;
using Log = Mediaportal.TV.Server.TVLibrary.Interfaces.Logging.Log;

//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using MediaPortal.Video.Database;
//using Toub.MediaCenter.Dvrms.Metadata;
///@using MediaPortal.TV.DiskSpace;

namespace Mediaportal.TV.TvPlugin
{
  public class TvRecorded : RecordedBase, IComparer<GUIListItem>
  {


    #region ThumbCacher

    public class RecordingThumbCacher
    {


      private Work work;

      public RecordingThumbCacher()
      {
        work = new Work(new DoWorkHandler(PerformRequest));
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

          IList<Recording> recordings = ServiceAgents.Instance.RecordingServiceAgent.ListAllRecordingsByMediaType(MediaTypeEnum.TV).ToList();
          for (int i = recordings.Count - 1; i >= 0; i--)
          {
            string recFileName = TVUtil.GetFileNameForRecording(recordings[i]);
            string thumbNail = string.Format("{0}\\{1}{2}", Thumbs.TVRecorded,
                                             Path.ChangeExtension(Utils.SplitFilename(recFileName), null),
                                             Utils.GetThumbExtension());

            if ((!TVHome.UseRTSP()) && !Utils.FileExistsInCache(thumbNail))
            {
              try
              {
                Thread.Sleep(250);
                if (VideoThumbCreator.CreateVideoThumb(recFileName, thumbNail, true, true))
                {
                  this.LogInfo("RecordedTV: Thumbnail successfully created for - {0}", Utils.SplitFilename(recFileName));
                }
                else
                {
                  this.LogInfo("RecordedTV: No thumbnail created for - {0}", Utils.SplitFilename(recFileName));
                }
                Thread.Sleep(250);
              }
              catch (Exception ex)
              {
                this.LogError(ex, "RecordedTV: No thumbnail created for {0}", Utils.SplitFilename(recFileName));
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

    private SortMethod _currentSortMethod = SortMethod.Date;
    private DBView _currentDbView = DBView.Recordings;
    private static Recording _activeRecording = null;
    private static bool _bIsLiveRecording = false;
    private static bool _thumbCreationActive = false;
    private static bool _createRecordedThumbs = true;
    private bool _deleteWatchedShows = false;
    private int _iSelectedItem = 0;
    private string _currentLabel = string.Empty;
    private int _rootItem = 0;
    private bool _resetSMSsearch = false;
    private bool _oldStateSMSsearch;
    private DateTime _resetSMSsearchDelay;

    private RecordingThumbCacher thumbworker = null;

    [SkinControl(6)]
    protected GUIButtonControl btnCleanup = null;
    [SkinControl(7)]
    protected GUIButtonControl btnCompress = null;


    #endregion

    #region Constructor

    public TvRecorded()
    {
      IntegrationProviderHelper.Register();
      GetID = (int)Window.WINDOW_RECORDEDTV;
    }

    #endregion

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Settings xmlreader = new MPSettings())
      {
        currentLayout = (GUIFacadeControl.Layout)xmlreader.GetValueAsInt(SerializeName, "layout", (int)GUIFacadeControl.Layout.List);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);

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

        _deleteWatchedShows = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
        _createRecordedThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
      }

      thumbworker = null;
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "layout", (int)currentLayout);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);

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
      }
    }

    #endregion

    #region Overrides

    protected override string SerializeName
    {
      get
      {
        return "tvrecorded";
      }
    }

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
          if (facadeLayout != null)
          {
            if (facadeLayout.Focus)
            {
              GUIListItem item = GetItem(0);
              if (item != null)
              {
                if (item.IsFolder && item.Label == "..")
                {
                  _currentLabel = string.Empty;
                  LoadDirectory();
                  GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _rootItem);
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
      /*g_Player.PlayBackStopped -= new g_Player.StoppedHandler(OnPlayRecordingBackStopped);
      g_Player.PlayBackEnded -= new g_Player.EndedHandler(OnPlayRecordingBackEnded);
      g_Player.PlayBackStarted -= new g_Player.StartedHandler(OnPlayRecordingBackStarted);
      g_Player.PlayBackChanged -= new g_Player.ChangedHandler(OnPlayRecordingBackChanged);*/

      _iSelectedItem = GetSelectedItemNo();
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      TVHome.ShowTvEngineSettingsUIIfConnectionDown();

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
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _iSelectedItem);

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      if (thumbworker == null)
      {
        if (_createRecordedThumbs)
        {
          _createRecordedThumbs = (!TVHome.UseRTSP());

          if (!_createRecordedThumbs)
          {
            this.LogInfo("GUIRecordedTV: skipping thumbworker thread - RTSP mode is in use");
          }
          else
          {
            thumbworker = new RecordingThumbCacher();
          }
        }
      }
      else
      {
        this.LogInfo("GUIRecordedTV: thumbworker already running - didn't start another one");
      }
    }

    protected override bool AllowLayout(GUIFacadeControl.Layout layout)
    {
      // Disable playlist for now as it makes no sense to move recording entries
      if (layout == GUIFacadeControl.Layout.Playlist)
      {
        return false;
      }
      return true;
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnLayouts)
      {
        LoadDirectory();
      }

      if (control == btnCleanup)
      {
        OnCleanup();
      }
    }

    protected override void OnInfo(int iItem)
    {
      OnShowContextMenu();
    }

    protected override void OnClick(int iItem)
    {
      OnSelectedRecording(iItem);
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
      dlg.AddLocalizedString(620); // channel
      dlg.AddLocalizedString(104); // date
      dlg.AddLocalizedString(268); // title
      dlg.AddLocalizedString(669); // genre
      dlg.AddLocalizedString(671); // watched
      dlg.AddLocalizedString(1017); // duration


      // set the focus to currently used sort method
      dlg.SelectedLabel = (int)_currentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      _currentSortMethod = (SortMethod)dlg.SelectedLabel;

      OnSort();
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
        dlg.AddLocalizedString(200072); //Upcoming episodes      
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

        case 200072:
          ShowUpcomingEpisodes(rec);
          break;

        case 830: // Reset watched status
          _iSelectedItem = GetSelectedItemNo();
          ResetWatchedStatus(rec);
          LoadDirectory();
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _iSelectedItem);
          break;
      }
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
      if ((_resetSMSsearch == true) && (_resetSMSsearchDelay.Subtract(DateTime.Now).Seconds < -2))
      {
        _resetSMSsearchDelay = DateTime.Now;
        _resetSMSsearch = true;
        facadeLayout.EnableSMSsearch = _oldStateSMSsearch;
      }

      base.Process();
    }

    /*protected override void  OnShowViews()
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
        this.LogError("TvRecorded: Error in ShowViews - {0}", ex.ToString());
      }
    }*/

    protected override void UpdateButtonStates()
    {
      base.UpdateButtonStates();
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
        }

        if (null != facadeLayout)
          facadeLayout.EnableScrollLabel = _currentSortMethod == SortMethod.Name;

        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTITLE);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMDESCRIPTION);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMGENRE);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTIME);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TVRecorded: Error updating button states");
      }
    }

    #endregion

    #region Public methods

    public override bool IsTv
    {
      get { return true; }
    }

    public static Recording ActiveRecording()
    {
      return _activeRecording;
    }

    public static void SetActiveRecording(Recording rec)
    {
      _activeRecording = rec;
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
        Program paramProg = ProgramFactory.CreateProgram(rec.IdChannel.GetValueOrDefault(), rec.StartTime, rec.EndTime, rec.Title, rec.Description, rec.ProgramCategory,
                                        ProgramState.None, DateTime.MinValue, String.Empty, String.Empty,
                                        String.Empty, String.Empty, 0, String.Empty, 0);
        TVProgramInfo.CurrentProgram = paramProg;
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_PROGRAM_INFO);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "TvRecorded: Error in ShowUpcomingEpisodes");
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
        GUIControl.ClearControl(GetID, facadeLayout.GetID);
        IEnumerable<Recording> recordings = ServiceAgents.Instance.RecordingServiceAgent.ListAllRecordingsByMediaType(MediaTypeEnum.TV);

        if (_currentLabel == string.Empty)
        {
          Func<Recording, string> keySelector = (rec => rec.Title);
          switch (_currentDbView)
          {
            case DBView.History:
              keySelector = (rec => GetSpokenViewDate(rec.StartTime).ToUpperInvariant());
              break;
            case DBView.Recordings:
              keySelector = (rec => rec.Title.ToUpperInvariant());
              break;
            case DBView.Channel:
              keySelector = (rec => rec.IdChannel.ToString());
              break;
            case DBView.Genre:
              keySelector = (rec => TVUtil.GetCategory(rec.ProgramCategory).ToUpperInvariant());
              break;
          }

          ILookup<string, Recording> recItems = recordings.ToLookup(keySelector);

          itemlist = recItems.Select(recs =>
          {
            var item =
              BuildItemFromRecording(
                recs.Aggregate(
                  (ra, r) =>
                    ra == null ? r :
                    IsRecordingActual(r) ? r :
                    r.StartTime > ra.StartTime ? r : ra));
            if (recs.Count() > 1)
            {
              item.IsFolder = true;
              Utils.SetDefaultIcons(item);
              item.ThumbnailImage = item.IconImageBig;
            }
            return item;
          }).ToList();
        }
        else
        {
          // Showing a merged folders content
          GUIListItem item = new GUIListItem("..");
          item.IsFolder = true;
          Utils.SetDefaultIcons(item);
          itemlist.Add(item);

          // this.LogDebug("TVRecorded: Currently showing the virtual folder contents of {0}", _currentLabel);
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
                addToList = TVUtil.GetCategory(rec.ProgramCategory).Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase);
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
        this.LogError(ex, "TvRecorded: Error fetching recordings from database");
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

          facadeLayout.Add(item);
        }
      }
      catch (Exception ex2)
      {
        this.LogError("TvRecorded: Error adding recordings to list - {0}", ex2.Message);
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(itemlist.Count - (itemlist.Count > 0 && itemlist[0].Label == ".." ? 1 : 0)));

      SwitchLayout();
      OnSort();
      UpdateProperties();
    }

    public static string GetRecordingDisplayName(Recording rec)
    {
      if (rec == null)
      {
        return "";
      }

      Channel ch = rec.Channel;

      if (ch == null)
      {
        return "";
      }

      return ch.DisplayName;
    }

    private static bool IsRecordingActual(Recording aRecording)
    {
      return aRecording.IsRecording;
    }

    private GUIListItem BuildItemFromRecording(Recording aRecording)
    {
      string strDefaultUnseenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoBig.png";
      string strDefaultSeenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoSeenBig.png";
      GUIListItem item = null;
      string strChannelName = GUILocalizeStrings.Get(2014); // unknown      

      try
      {
        Channel refCh = null;
        try
        {
          // Re-imported channels might still be valid but their channel does not need to be present anymore...
          refCh = aRecording.Channel;
        }
        catch (Exception) { }
        if (refCh != null)
        {
          strChannelName = refCh.DisplayName;
        }

        // this.LogDebug("TVRecorded: BuildItemFromRecording [{0}]: {1} ({2}) on channel {3}", _currentDbView.ToString(), aRecording.title, aRecording.ProgramCategory.category, strChannelName);
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
            item.Label = TVUtil.GetCategory(aRecording.ProgramCategory);
            break;
          case DBView.History:
            item.Label = GetSpokenViewDate(aRecording.StartTime);
            break;
        }

        item.TVTag = aRecording;

        // Set a default logo indicating the watched status
        string SmallThumb = aRecording.TimesWatched > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;
        string previewThumb = string.Format("{0}\\{1}{2}", Thumbs.TVRecorded,
                                            Path.ChangeExtension(Utils.SplitFilename(aRecording.FileName), null),
                                            Utils.GetThumbExtension());

        // Get the channel logo for the small icons
        string StationLogo = Utils.GetCoverArt(Thumbs.TVChannel, strChannelName);
        if (Utils.FileExistsInCache(StationLogo))
        {
          SmallThumb = StationLogo;
        }

        // Display previews only if the option to create them is active                
        if (Utils.FileExistsInCache(previewThumb))
        {
          // Search a larger one
          string PreviewThumbLarge = Utils.ConvertToLargeCoverArt(previewThumb);
          if (Utils.FileExistsInCache(PreviewThumbLarge))
          {
            previewThumb = PreviewThumbLarge;
          }
          item.ThumbnailImage = item.IconImageBig = previewThumb;
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
        this.LogError(singleex, "TVRecorded: Error building item from recording {0}", aRecording.FileName);
      }

      return item;
    }

    private void SetLabels()
    {
      SortMethod method = _currentSortMethod;
      bool bAscending = m_bSortAscending;

      for (int i = 0; i < facadeLayout.Count; ++i)
      {
        try
        {
          GUIListItem item1 = facadeLayout[i];
          if (item1.Label == "..")
          {
            continue;
          }
          Recording rec = (Recording)item1.TVTag;

          // Do not display a duration in top level of History view
          if (_currentDbView != DBView.History || _currentLabel != String.Empty)
          {
            item1.Label2 = TVUtil.GetRecordingDateString(rec);
          }
          if (currentLayout != GUIFacadeControl.Layout.List)
          {
            item1.Label3 = GUILocalizeStrings.Get(2014); // unknown
            if (!String.IsNullOrEmpty(TVUtil.GetCategory(rec.ProgramCategory)))
            {
              item1.Label3 = TVUtil.GetCategory(rec.ProgramCategory);
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
            catch (Exception) { }
          }

          if (rec.TimesWatched > 0)
          {
            if (!item1.IsFolder)
            {
              item1.IsPlayed = true;
            }
          }
          //this.LogDebug("TvRecorded: SetLabels - 1: {0}, 2: {1}, 3: {2}", item1.Label, item1.Label2, item1.Label3);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "TVRecorded: error in SetLabels");
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
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _rootItem);
          _rootItem = 0;
        }
        return false;
      }

      Recording rec = (Recording)pItem.TVTag;
      IEnumerable<Recording> itemlist = ServiceAgents.Instance.RecordingServiceAgent.ListAllRecordingsByMediaType(MediaTypeEnum.TV);

      _activeRecording = rec;
      _bIsLiveRecording = false;
      foreach (Recording recItem in itemlist)
      {
        if (rec.IdRecording == recItem.IdRecording && IsRecordingActual(recItem))
        {
          _bIsLiveRecording = true;
          break;
        }
      }

      int stoptime = rec.StopTime;
      if (_bIsLiveRecording || stoptime > 0)
      {
        GUIResumeDialog.MediaType mediaType = GUIResumeDialog.MediaType.Recording;
        if (_bIsLiveRecording)
          mediaType = GUIResumeDialog.MediaType.LiveRecording;

        GUIResumeDialog.Result result =
          GUIResumeDialog.ShowResumeDialog(TVUtil.GetDisplayTitle(rec), rec.StopTime, mediaType);

        switch (result)
        {
          case GUIResumeDialog.Result.Abort:
            return false;

          case GUIResumeDialog.Result.PlayFromBeginning:
            stoptime = 0;
            break;

          case GUIResumeDialog.Result.PlayFromLivePoint:
            stoptime = -1; // magic -1 is used for the live point
            break;
        }
      }

      if (TVHome.Card != null)
      {
        TVHome.Card.StopTimeShifting();
      }
      return TVUtil.PlayRecording(rec, stoptime);
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
      bool remove;
      if (isRec)
      {
        Schedule sched = rec.Schedule;
        remove = TVUtil.DeleteRecAndSchedWithPrompt(sched);
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
        dlgYesNo.SetLine(2, TVUtil.GetDisplayTitle(rec));
        dlgYesNo.SetLine(3, string.Empty);
        dlgYesNo.DoModal(GetID);
        if (!dlgYesNo.IsConfirmed)
        {
          return;
        }
        remove = true;

      }

      if (remove)
      {
        if (isRecPlaying)
        {
          this.LogInfo("g_Player.Stopped {0}", g_Player.Stopped);
          g_Player.Stop();
        }
        DeleteRecordingAndUpdateGUI(rec);
      }
    }

    private void DeleteRecordingAndUpdateGUI(Recording rec)
    {
      _oldStateSMSsearch = facadeLayout.EnableSMSsearch;
      facadeLayout.EnableSMSsearch = false;

      TryDeleteRecordingAndNotifyUser(rec);


      LoadDirectory();
      while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
      {
        _iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _iSelectedItem);

      _resetSMSsearchDelay = DateTime.Now;
      _resetSMSsearch = true;
    }


    private void TryDeleteRecordingAndNotifyUser(Recording rec)
    {

      int timeout = 0;
      bool deleteRecording = false;

      while (!deleteRecording && timeout < 5)
      {
        deleteRecording = ServiceAgents.Instance.ControllerServiceAgent.DeleteRecording(rec.IdRecording);
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
          dlgOk.SetLine(2, TVUtil.GetDisplayTitle(rec));
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
      dlg.Reset();
      LoadDirectory();
      while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
      {
        _iSelectedItem--;
      }

      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _iSelectedItem);
    }

    private void DeleteWatchedRecordings(string currentTitle)
    {
      ServiceAgents.Instance.ControllerServiceAgent.DeleteWatchedRecordings(currentTitle);
    }

    private void DeleteInvalidRecordings()
    {
      ServiceAgents.Instance.ControllerServiceAgent.DeleteInvalidRecordings();
    }

    private void UpdateProperties()
    {
      try
      {
        GUIListItem pItem = GetItem(GetSelectedItemNo());
        if (pItem == null)
        {
          GUIPropertyManager.SetProperty("#selectedthumb", String.Empty);
          SetProperties(null);
          return;
        }
        Recording rec = pItem.TVTag as Recording;
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
        this.LogError(ex, "TvRecorded: Error updating properties");
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

        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", TVUtil.GetDisplayTitle(rec));
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", TVUtil.GetCategory(rec.ProgramCategory));
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", TVUtil.GetRecordingDateStringFull(rec));
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", rec.Description);

        string strLogo = "";

        GUIPropertyManager.SetProperty("#TV.RecordedTV.Channel", GetRecordingDisplayName(rec));
        strLogo = Utils.GetCoverArt(Thumbs.TVChannel, GetRecordingDisplayName(rec));

        if (Utils.FileExistsInCache(strLogo))
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
        this.LogError(ex, "TvRecorded: Error setting item properties");
      }
    }

    #endregion

    #region View management

    private GUIListItem GetSelectedItem()
    {
      return facadeLayout.SelectedListItem;
    }

    private GUIListItem GetItem(int iItem)
    {
      if (iItem < 0 || iItem >= facadeLayout.Count)
      {
        return null;
      }
      return facadeLayout[iItem];
    }

    private int GetSelectedItemNo()
    {
      if (facadeLayout.Count > 0)
      {
        return facadeLayout.SelectedListItemIndex;
      }
      else
      {
        return -1;
      }
    }

    private int GetItemCount()
    {
      return facadeLayout.Count;
    }

    #endregion

    #region Sort Members

    private void OnSort()
    {
      try
      {
        SetLabels();
        facadeLayout.Sort(this);
        UpdateButtonStates();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TvRecorded: Error sorting items");
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

        string rec1DisplayName = "";
        string rec2DisplayName = "";

        if (rec1.Channel != null)
        {
          rec1DisplayName = rec1.Channel.DisplayName;
        }
        if (rec2.Channel != null)
        {
          rec2DisplayName = rec2.Channel.DisplayName;
        }
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
              iComp = string.Compare(rec1DisplayName, rec2DisplayName, true);
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
              iComp = string.Compare(rec2DisplayName, rec1DisplayName, true);
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
            var categoryRec1 = TVUtil.GetCategory(rec1.ProgramCategory);
            var categoryRec2 = TVUtil.GetCategory(rec2.ProgramCategory);
            item1.Label2 = categoryRec1;
            item2.Label2 = categoryRec2;
            if (categoryRec1 != categoryRec2)
            {
              if (m_bSortAscending)
              {
                return string.Compare(categoryRec1, categoryRec2, true);
              }
              else
              {
                return string.Compare(categoryRec2, categoryRec1, true);
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
                return string.Compare(rec1DisplayName, rec2DisplayName);
              }
              else
              {
                return string.Compare(rec2DisplayName, rec1DisplayName);
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
        this.LogError(ex, "TvRecorded: Error comparing files");
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
      this.LogInfo("TvRecorded:{0} {1} {2}", caller, type, filename);
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }

      Recording rec = ServiceAgents.Instance.RecordingServiceAgent.GetRecordingByFileName(filename);
      if (rec != null)
      {
        if (stoptime >= g_Player.Duration)
        {
          stoptime = 0;
        }
        ; //temporary workaround before end of stream get's properly implemented        
        rec = ServiceAgents.Instance.RecordingServiceAgent.GetRecording(rec.IdRecording);
        rec.StopTime = stoptime;
        ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(rec);
      }
      else
      {
        this.LogInfo("TvRecorded:{0} no recording found with filename {1}", caller, filename);
      }
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

      Recording rec = ServiceAgents.Instance.RecordingServiceAgent.GetRecordingByFileName(filename);
      if (rec != null)
      {
        if (_deleteWatchedShows || rec.KeepUntil == (int)KeepMethodType.UntilWatched)
        {

          ServiceAgents.Instance.ControllerServiceAgent.DeleteRecording(rec.IdRecording);
        }
        else
        {
          rec.StopTime = 0;
          ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(rec);
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

      this.LogDebug("TVRecorded.OnPlayRecordingBackStarted(): setting audioIndex on tsreader {0}", prefLangIdx);
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
      ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(aRecording);
    }

    #endregion
  }
}