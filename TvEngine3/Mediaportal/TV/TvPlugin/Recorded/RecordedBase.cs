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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Common.GUIPlugins;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.TvPlugin.Helper;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;
using Log = Mediaportal.TV.Server.TVLibrary.Interfaces.Logging.Log;

namespace Mediaportal.TV.TvPlugin.Recorded
{
  public abstract class RecordedBase : WindowPluginBase, IComparer<GUIListItem>
  {
    #region Enums

    private enum Controls
    {
      LABEL_PROGRAMTITLE = 13,
      LABEL_PROGRAMTIME = 14,
      LABEL_PROGRAMDESCRIPTION = 15,
      LABEL_PROGRAMGENRE = 17
    }

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
      History
    }

    #endregion

    #region Variables

    private static Recording _activeRecording = null;
    private static bool _isLiveRecording = false;

    private SortMethod _currentSortMethod = SortMethod.Date;
    private DBView _currentDbView = DBView.Recordings;
    private bool _deleteWatchedShows = false;
    private int _selectedItem = 0;
    private string _currentLabel = string.Empty;
    private int _rootItem = 0;
    private bool _resetSMSsearch = false;
    private bool _oldStateSMSsearch;
    private DateTime _resetSMSsearchDelay;

    [SkinControl(6)] protected GUIButtonControl btnCleanup = null;
    [SkinControl(7)] protected GUIButtonControl btnCompress = null;

    #endregion

    #region Constructor

    public RecordedBase()
    {
      IntegrationProviderHelper.Register();
      GUIWindowManager.OnNewAction += OnNewAction;
    }

    #endregion

    #region abstract

    protected abstract MediaType MediaType
    {
      get;
    }

    protected abstract string ThumbsType
    {
      get;
    }

    protected abstract string SettingsSection
    {
      get;
    }

    protected abstract string SkinFileName
    {
      get;
    }

    protected abstract string SkinPropertyPrefix
    {
      get;
    }

    protected abstract int ChannelViewOptionStringId
    {
      get;
    }

    protected abstract bool OnSelectedRecording(int iItem);

    protected abstract string GetCachedRecordingFileName(Recording recording);

    #endregion

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Settings xmlreader = new MPSettings())
      {
        currentLayout = (GUIFacadeControl.Layout)xmlreader.GetValueAsInt(SettingsSection, "layout", (int)GUIFacadeControl.Layout.List);
        m_bSortAscending = xmlreader.GetValueAsBool(SettingsSection, "sortasc", true);

        string strTmp = xmlreader.GetValueAsString(SettingsSection, "sort", "channel");

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
      }
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(SettingsSection, "layout", (int)currentLayout);
        xmlwriter.SetValueAsBool(SettingsSection, "sortasc", m_bSortAscending);

        switch (_currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue(SettingsSection, "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue(SettingsSection, "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue(SettingsSection, "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue(SettingsSection, "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue(SettingsSection, "sort", "played");
            break;
          case SortMethod.Duration:
            xmlwriter.SetValue(SettingsSection, "sort", "duration");
            break;
        }
      }
    }

    #endregion

    #region Static

    /// <summary>
    /// Convert how long ago a recording took place into a meaningful description
    /// </summary>
    /// <param name="aStartTime">A recordings start time</param>
    /// <returns>The spoken date label</returns>
    protected static string GetSpokenViewDate(DateTime aStartTime)
    {
      DateTime now = DateTime.Now;
      DateTime today = now.Date;

      var thisMonth = new DateTime(today.Year, today.Month, 1);
      var lastMonth = thisMonth.AddMonths(-1);

      DayOfWeek firstDayOfWeek = WeekEndTool.GetFirstWorkingDay();
      DateTime firstDayOfThisWeek = today;
      while (firstDayOfThisWeek.DayOfWeek != firstDayOfWeek)
        firstDayOfThisWeek = firstDayOfThisWeek.AddDays(-1);
      int daysToStartOfWeek = (aStartTime.Date - firstDayOfThisWeek).Days;
      int daysToStartOfLastWeek = daysToStartOfWeek + 7;

      if (now < aStartTime)
        return GUILocalizeStrings.Get(6095); // "Future"
      else if (today.Equals(aStartTime.Date))
        return GUILocalizeStrings.Get(6030); // "Today"
      else if (today.Equals(aStartTime.AddDays(1).Date))
        return GUILocalizeStrings.Get(6040); // "Yesterday"
      else if (0 <= daysToStartOfWeek && daysToStartOfWeek < 5) // current week excluding today and yesterday
        return GUILocalizeStrings.Get(6055); // "Earlier this week";
      else if (0 <= daysToStartOfLastWeek && daysToStartOfLastWeek < 7)
        return GUILocalizeStrings.Get(6056); // "Last week"
      else if (thisMonth.Equals(new DateTime(aStartTime.Year, aStartTime.Month, 1)))
        return GUILocalizeStrings.Get(6062); //"Earlier this month"
      else if (lastMonth.Equals(new DateTime(aStartTime.Year, aStartTime.Month, 1)))
        return GUILocalizeStrings.Get(6065); // "Last month"
      else if (today.Year.Equals(aStartTime.Year))
        return GUILocalizeStrings.Get(6075); // "Earlier this year"
      else if (today.Year.Equals(aStartTime.AddYears(1).Year))
        return GUILocalizeStrings.Get(6080); // "Last year";
      else
        return GUILocalizeStrings.Get(6090); // "Older"
    }

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
        Log.Info("RecordedBase: ShowFullScreenWindow switching to fullscreen video");
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
        var prog = ProgramFactory.CreateEmptyProgram();
        prog.IdChannel = rec.IdChannel.GetValueOrDefault();
        prog.StartTime = rec.StartTime;
        prog.EndTime = rec.EndTime;
        prog.Title = rec.Title;
        prog.Description = rec.Description;
        TVProgramInfo.CurrentProgram = prog;
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV_PROGRAM_INFO);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "RecordedBase: Error in ShowUpcomingEpisodes");
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

      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(SkinFileName));
      //LoadSettings();
      GUIWindowManager.Replace(GetID, this);
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

      _selectedItem = GetSelectedItemNo();
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      TVHome.ShowTvEngineSettingsUIIfConnectionDown();

      base.OnPageLoad();
      InitViewSelections();

      if (btnCompress != null)
      {
        btnCompress.Visible = false;
      }

      LoadSettings();
      LoadDirectory();

      while (_selectedItem >= GetItemCount() && _selectedItem > 0)
      {
        _selectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItem);

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
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
      dlg.AddLocalizedString(621); // date
      dlg.AddLocalizedString(268); // title
      dlg.AddLocalizedString(678); // genre
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

        case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
        case GUIMessage.MessageType.GUI_MSG_CLICKED:

          // Depending on the mode, handle the GUI_MSG_ITEM_SELECT message from the dialog menu and
          // the GUI_MSG_CLICKED message from the spin control.
          // Respond to the correct control.  The value is retrived directly from the control by the called handler.
          if (message.TargetControlId == btnViews.GetID)
          {
            // Set the new view.
            SetView(btnViews.SelectedItemValue);
          }
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
          _selectedItem = GetSelectedItemNo();
          ResetWatchedStatus(rec);
          LoadDirectory();
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItem);
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

    protected override void InitViewSelections()
    {
      btnViews.ClearMenu();

      // Add the view options to the menu.
      int index = 0;
      btnViews.AddItem(GUILocalizeStrings.Get(914), index++); // Recordings
      btnViews.AddItem(GUILocalizeStrings.Get(135), index++); // Genres
      btnViews.AddItem(GUILocalizeStrings.Get(915), index++); // TV Channels
      btnViews.AddItem(GUILocalizeStrings.Get(636), index++); // Date

      // Have the menu select the currently selected view.
      switch (_currentDbView)
      {
        case DBView.Recordings:
          btnViews.SetSelectedItemByValue(0);
          break;
        case DBView.Genre:
          btnViews.SetSelectedItemByValue(1);
          break;
        case DBView.Channel:
          btnViews.SetSelectedItemByValue(2);
          break;
        case DBView.History:
          btnViews.SetSelectedItemByValue(3);
          break;
      }
    }

    protected override void SetView(int selectedViewId)
    {
      try
      {
        switch (selectedViewId)
        {
          case 0:
            _currentDbView = DBView.Recordings;
            break;
          case 1:
            _currentDbView = DBView.Genre;
            break;
          case 2:
            _currentDbView = DBView.Channel;
            break;
          case 3:
            _currentDbView = DBView.History;
            break;
        }

        // If we had been in 2nd group level - go up to root again.
        _currentLabel = String.Empty;
        LoadDirectory();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RecordedBase: Error in SetView");
      }
    }

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
        this.LogError(ex, "RecordedBase: Error updating button states");
      }
    }

    #endregion

    protected bool GetUserPasswordString(ref string sString)
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

    protected bool OnSelectedRecording(int iItem, MediaType tveMediaType, g_Player.MediaType mpMediaType, out GUIListItem pItem)
    {
      pItem = GetItem(iItem);
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
      _activeRecording = rec;
      _isLiveRecording = false;
      if (rec.IsRecording)
      {
        IEnumerable<Recording> itemlist = ServiceAgents.Instance.RecordingServiceAgent.ListAllRecordingsByMediaType(tveMediaType);
        foreach (Recording recItem in itemlist)
        {
          if (rec.IdRecording == recItem.IdRecording)
          {
            _isLiveRecording = true;
            break;
          }
        }
      }

      int stoptime = rec.StopTime;
      if (_isLiveRecording || stoptime > 0)
      {
        GUIResumeDialog.MediaType mediaType = GUIResumeDialog.MediaType.Recording;
        if (_isLiveRecording)
        {
          mediaType = GUIResumeDialog.MediaType.LiveRecording;
        }

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
      return TVUtil.PlayRecording(rec, stoptime, mpMediaType);
    }

    #region Public methods

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    public static Recording ActiveRecording()
    {
      return _activeRecording;
    }

    public static void SetActiveRecording(Recording rec)
    {
      _activeRecording = rec;
      _isLiveRecording = rec.IsRecording;
    }

    public static bool IsLiveRecording()
    {
      return _isLiveRecording;
    }

    public static string GetChannelDisplayName(Recording rec)
    {
      if (rec == null)
      {
        return GUILocalizeStrings.Get(2014);
      }

      // Re-imported channels might still be valid but their channel does not need to be present anymore...
      Channel ch = rec.Channel;
      if (ch == null)
      {
        return GUILocalizeStrings.Get(2014);
      }

      return ch.Name;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// OnAction only receives ACTION_PLAY event when the player is not playing. Ensure all actions are processed
    /// </summary>
    /// <param name="action">Action command</param>
    private void OnNewAction(Action action)
    {
      if (GUIWindowManager.ActiveWindow == GetID &&
        (action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
      )
      {
        GUIListItem item = facadeLayout.SelectedListItem;

        if (item == null || item.Label == ".." || item.IsFolder)
        {
          return;
        }

        if (GetFocusControlId() == facadeLayout.GetID)
        {
          // only start something is facade is focused
          OnSelectedRecording(facadeLayout.SelectedListItemIndex);
        }
      }
    }

    private void LoadDirectory()
    {
      var watch = new Stopwatch();
      watch.Reset();
      watch.Start();

      try
      {
        GUIControl.ClearControl(GetID, facadeLayout.GetID);
        IEnumerable<Recording> recordings = ServiceAgents.Instance.RecordingServiceAgent.ListAllRecordingsByMediaType(MediaType);

        if (_currentLabel == string.Empty)
        {
          Func<Recording, string> keySelector = (rec => rec.Title);
          switch (_currentDbView)
          {
            case DBView.History:
              keySelector = (rec => GetSpokenViewDate(rec.StartTime));
              break;
            case DBView.Recordings:
              keySelector = (rec => rec.Title.ToUpperInvariant());
              break;
            case DBView.Channel:
              keySelector = (rec => GetChannelDisplayName(rec));
              break;
            case DBView.Genre:
              keySelector = (rec => TVUtil.GetCategory(rec.ProgramCategory));
              break;
          }

          ILookup<string, Recording> recItems = recordings.ToLookup(keySelector);
          recItems.Select(recs =>
          {
            GUIListItem item;
            // Is this item going to be displayed as a folder or a single recording?
            if (_currentDbView != DBView.Recordings || recs.Count<Recording>() > 1)
            {
              item = BuildFolderItemFromRecording(
                recs.Aggregate(
                  (r1, r2) =>
                    r1 == null ? r2 :
                    r2.IsRecording ? r2 :
                    r2.StartTime > r1.StartTime ? r2 : r1
                )
              );
            }
            else
            {
              item = BuildItemFromRecording(recs.First<Recording>());
            }
            facadeLayout.Add(item);
            return item;
          }).ToList();
        }
        else
        {
          // Showing a merged folders content.
          // All items except the top ".." link are recordings, not folders.
          GUIListItem item = new GUIListItem("..");
          item.IsFolder = true;
          Utils.SetDefaultIcons(item);
          facadeLayout.Add(item);

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
                            TVUtil.GetDisplayTitle(rec).Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase);
                break;
              case DBView.Channel:
                addToList = GetChannelDisplayName(rec).Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase);
                break;
              case DBView.Genre:
                addToList = TVUtil.GetCategory(rec.ProgramCategory).Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase);
                break;
            }

            if (addToList)
            {
              // Add new list item for this recording
              item = BuildItemFromRecording(rec);
              if (item != null)
              {
                item.Label = TVUtil.GetDisplayTitle(rec);
                facadeLayout.Add(item);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RecordedBase: Error fetching recordings from database");
      }

      this.LogDebug("RecordedBase: LoadDirectory() - finished loading facade items after '{0}' ms.", watch.ElapsedMilliseconds);

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(facadeLayout.Count - (facadeLayout.Count > 0 && facadeLayout[0].Label == ".." ? 1 : 0)));

      SwitchLayout();
      OnSort();

      watch.Stop();
      this.LogDebug("RecordedBase: LoadDirectory() - finished sorting facade after '{0}' ms.", watch.ElapsedMilliseconds);

      UpdateProperties();
      UpdateThumbnails();
    }

    private GUIListItem BuildItemFromRecording(Recording aRecording)
    {
      GUIListItem item = null;

      try
      {
        // this.LogDebug("RecordedBase: BuildItemFromRecording [{0}]: {1} ({2}) on channel {3}", _currentDbView.ToString(), aRecording.title, aRecording.ProgramCategory.category, strChannelName);
        item = new GUIListItem { Label = TVUtil.GetDisplayTitle(aRecording), TVTag = aRecording, IsRemote = false };
        if (aRecording.IsRecording)
        {
          item.PinImage = Thumbs.TvRecordingIcon;
        }

        // Set a default logo indicating the watched status
        string smallThumb;
        if (aRecording.TimesWatched > 0)
        {
          smallThumb = GUIGraphicsContext.GetThemedSkinFile(@"\Media\defaultVideoSeenBig.png");
        }
        else
        {
          smallThumb = GUIGraphicsContext.GetThemedSkinFile(@"\Media\defaultVideoBig.png");
        }

        // Get the channel logo for the small icons
        string stationLogo = Utils.GetCoverArt(ThumbsType, GetChannelDisplayName(aRecording));
        if (Utils.FileExistsInCache(stationLogo))
        {
          smallThumb = stationLogo;
        }

        string previewThumb = GetCachedRecordingFileName(aRecording);
        if (Utils.FileExistsInCache(previewThumb))
        {
          // Search a larger one
          string previewThumbLarge = Utils.ConvertToLargeCoverArt(previewThumb);
          if (Utils.FileExistsInCache(previewThumbLarge))
          {
            previewThumb = previewThumbLarge;
          }
          item.ThumbnailImage = item.IconImageBig = previewThumb;
        }
        else
        {
          // Fallback to Logo/Default icon
          item.IconImageBig = smallThumb;
          item.ThumbnailImage = string.Empty;
          item.IsRemote = true; // -> will load thumbnail image later
        }
        item.IconImage = smallThumb;
      }
      catch (Exception singleex)
      {
        item = null;
        this.LogError(singleex, "RecordedBase: Error building item from recording {0}", aRecording.FileName);
      }

      return item;
    }

    private GUIListItem BuildFolderItemFromRecording(Recording aRecording)
    {
      GUIListItem item = null;

      try
      {
        item = new GUIListItem { IsFolder = true, TVTag = aRecording, IsRemote = false };
        if (aRecording.IsRecording)
        {
          item.PinImage = Thumbs.TvRecordingIcon;
        }

        Utils.SetDefaultIcons(item);
        item.ThumbnailImage = item.IconImageBig;
        switch (_currentDbView)
        {
          case DBView.History:
            item.Label = GetSpokenViewDate(aRecording.StartTime);
            break;
          case DBView.Recordings:
            item.Label = aRecording.Title;
            break;
          case DBView.Channel:
            item.Label = GetChannelDisplayName(aRecording);
            string stationLogo = Utils.GetCoverArt(ThumbsType, GetChannelDisplayName(aRecording));
            if (Utils.FileExistsInCache(stationLogo))
            {
              item.IconImage = stationLogo;
              item.IconImageBig = stationLogo;
            }
            break;
          case DBView.Genre:
            item.Label = TVUtil.GetCategory(aRecording.ProgramCategory);
            break;
        }
      }
      catch (Exception singleex)
      {
        item = null;
        this.LogError(singleex, "RecordedBase: Error building folder item from recording {0}", aRecording.FileName);
      }

      return item;
    }

    private void SetThumbnails(GUIListItem item)
    {
      if (item == null)
        return;
      Recording aRecording = item.TVTag as Recording;
      if (aRecording == null)
      {
        return;
      }

      try
      {
        string previewThumb = GetCachedRecordingFileName(aRecording);
        if (!Utils.FileExistsInCache(previewThumb))
        {
          this.LogDebug("RecordedBase: thumbnail {0} does not exist in local thumbs folder - get it from TV server", previewThumb);
          try
          {
            byte[] thumbData = ServiceAgents.Instance.ControllerServiceAgent.GetThumbnailForRecording(aRecording.FileName);
            if (thumbData.Length > 0)
            {
              using (FileStream fs = new FileStream(previewThumb, FileMode.Create))
              {
                fs.Write(thumbData, 0, thumbData.Length);
                fs.Close();
                fs.Dispose();
              }
              Utils.DoInsertExistingFileIntoCache(previewThumb);
            }
            else
            {
              this.LogDebug("RecordedBase: thumbnail {0} not found on TV server", aRecording.FileName);
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "RecordedBase: error fetching thumbnail {0} from TV server", aRecording.FileName);
          }
        }

        // Display previews only if the option to create them is active
        if (Utils.FileExistsInCache(previewThumb))
        {
          // Search a larger one
          string previewThumbLarge = Utils.ConvertToLargeCoverArt(previewThumb);
          if (Utils.FileExistsInCache(previewThumbLarge))
          {
            previewThumb = previewThumbLarge;
          }
          item.ThumbnailImage = item.IconImageBig = previewThumb;
        }
      }
      catch (Exception singleex)
      {
        this.LogWarn(singleex, "RecordedBase - SetThumbnail: Error building item from recording {0}", aRecording.FileName);
      }
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
            item1.Label2 = string.Format("{0} ({1})", Utils.GetNamedDate(rec.StartTime), Utils.SecondsToHMString((int)(rec.EndTime - rec.StartTime).TotalSeconds));
          }
          if (currentLayout != GUIFacadeControl.Layout.List)
          {
            item1.Label3 = TVUtil.GetCategory(rec.ProgramCategory);
          }

          if (_currentSortMethod == SortMethod.Channel)
          {
            item1.Label2 = GetChannelDisplayName(rec);
          }

          if (rec.TimesWatched > 0)
          {
            if (!item1.IsFolder)
            {
              item1.IsPlayed = true;
            }
          }
          //this.LogDebug("RecordedBase: SetLabels - 1: {0}, 2: {1}, 3: {2}", item1.Label, item1.Label2, item1.Label3);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "RecordedBase: error in SetLabels");
        }
      }
    }

    private void OnDeleteRecording(int iItem)
    {
      string userCode = string.Empty;
      string _fileMenuPinCode = string.Empty;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        _fileMenuPinCode = Utils.DecryptPassword(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));
      }

      if (!string.IsNullOrEmpty(_fileMenuPinCode))
      {
        GetUserPasswordString(ref userCode);
        if (userCode != _fileMenuPinCode)
        {
          return;
        }
      }
      
      _selectedItem = GetSelectedItemNo();
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
      bool remove = false;
      if (rec.IsRecording)
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

        dlgYesNo.SetLine(1, GetChannelDisplayName(rec));
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

      UpdateGUI();

      _resetSMSsearchDelay = DateTime.Now;
      _resetSMSsearch = true;
    }

    private void UpdateGUI()
    {
      LoadDirectory();

      while (_selectedItem >= GetItemCount() && _selectedItem > 0)
      {
        _selectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItem);

      if (facadeLayout != null && facadeLayout.SelectedListItem != null && facadeLayout.SelectedListItem.Label == "..")
      {
        _currentLabel = string.Empty;
        LoadDirectory();
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _rootItem);
        _rootItem = 0;
      }
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
      _selectedItem = GetSelectedItemNo();
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
      while (_selectedItem >= GetItemCount() && _selectedItem > 0)
      {
        _selectedItem--;
      }

      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItem);
    }

    private void DeleteWatchedRecordings(string currentTitle)
    {
      ServiceAgents.Instance.ControllerServiceAgent.DeleteWatchedRecordings(currentTitle);
    }

    private bool DeleteInvalidRecordings()
    {
      Stopwatch watch = new Stopwatch();
      watch.Reset();
      watch.Start();
      bool deletedrecordings = ServiceAgents.Instance.ControllerServiceAgent.DeleteInvalidRecordings();
      watch.Stop();
      this.LogDebug("DeleteInvalidRecordings() - finished after {0} ms., deletedrecordings = {1}", watch.ElapsedMilliseconds, deletedrecordings);
      return deletedrecordings;
    }

    private void UpdateThumbnails()
    {
      new Thread(delegate()
      {
        {
          try
          {
            int count = 0;
            for (int i = 0; i < facadeLayout.Count; i++)
            {
              GUIListItem item = facadeLayout[i];
              if (item != null && item.IsRemote)
              {
                SetThumbnails(item);
                item.IsRemote = false;
                count++;
              }
            }
            this.LogDebug("RecordedBase: Updated '{0}' thumbnails", count);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "RecordedBase: Error updating thumbnails");
          }
        }
        GUIWindowManager.SendThreadCallbackAndWait((p1, p2, data) => 0, 0, 0, null);
      }) { Name = "UpdateThumbnails", IsBackground = true, Priority = ThreadPriority.BelowNormal }.Start();
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
        else if (pItem != null && pItem.IsFolder && pItem.Label == "..")
        {
          Utils.SetDefaultIcons(pItem);
          GUIPropertyManager.SetProperty("#selectedthumb", pItem.IconImageBig);
        }
        Recording rec = pItem.TVTag as Recording;
        if (rec == null)
        {
          SetProperties(null);
          if (pItem != null && pItem.IsFolder && pItem.Label == "..")
          {
            Utils.SetDefaultIcons(pItem);
            GUIPropertyManager.SetProperty("#selectedthumb", pItem.IconImageBig);
          }
          return;
        }
        SetProperties(rec);
        if (!pItem.IsFolder)
        {
          if (string.IsNullOrEmpty(pItem.ThumbnailImage))
          {
            MediaPortal.Util.Utils.SetDefaultIcons(pItem);
            GUIPropertyManager.SetProperty("#selectedthumb", pItem.IconImageBig);
          }
          else
          {
            GUIPropertyManager.SetProperty("#selectedthumb", pItem.ThumbnailImage);
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RecordedBase: Error updating properties");
      }
    }

    private void SetProperties(Recording rec)
    {
      try
      {
        if (rec == null)
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Title", string.Empty);
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Genre", string.Empty);
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Time", string.Empty);
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Channel", string.Empty);
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Description", string.Empty);
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".thumb", string.Empty);
          return;
        }
        string strTime = string.Format("{0} {1} - {2}",
                                       Utils.GetShortDayString(rec.StartTime),
                                       rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Title", rec.Title);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Genre", TVUtil.GetCategory(rec.ProgramCategory));
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Time", strTime);
        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Description", rec.Description);

        string strLogo = string.Empty;

        GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".Channel", GetChannelDisplayName(rec));
        strLogo = Utils.GetCoverArt(ThumbsType, GetChannelDisplayName(rec));

        if (!string.IsNullOrEmpty(strLogo) && Utils.FileExistsInCache(strLogo))
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".thumb", strLogo);
        }
        else
        {
          GUIPropertyManager.SetProperty(SkinPropertyPrefix + ".thumb", "defaultVideoBig.png");
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RecordedBase: Error setting item properties");
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
        this.LogError(ex, "RecordedBase: Error sorting items");
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
        if (item1 == null || item2 == null)
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

        SortMethod cSortMethod = _currentSortMethod;
        int iComp = 0;
        Recording rec1 = (Recording)item1.TVTag;
        Recording rec2 = (Recording)item2.TVTag;

        string rec1DisplayName = "";
        string rec2DisplayName = "";

        if (rec1.Channel != null)
        {
          rec1DisplayName = rec1.Channel.Name;
        }
        if (rec2.Channel != null)
        {
          rec2DisplayName = rec2.Channel.Name;
        }
        while (true) // starting with main sortmethod and sorting by secondary rules
        {
          switch (_currentSortMethod)
          {
            case SortMethod.Played:
              item1.Label2 = string.Format("{0} {1}", rec1.TimesWatched, GUILocalizeStrings.Get(677)); //times
              item2.Label2 = string.Format("{0} {1}", rec2.TimesWatched, GUILocalizeStrings.Get(677)); //times
              if (rec1.TimesWatched != rec2.TimesWatched)
              {
                int x = rec1.TimesWatched - rec2.TimesWatched;
                return m_bSortAscending ? x : -x;
              }
              _currentSortMethod = SortMethod.Name;
              break;
            case SortMethod.Name:
              iComp = string.Compare(TVUtil.GetDisplayTitle(rec1), TVUtil.GetDisplayTitle(rec2), true);
              if (iComp != 0)
              {
                return m_bSortAscending ? iComp : -iComp;
              }
              _currentSortMethod = SortMethod.Channel;
              break;
            case SortMethod.Channel:
              iComp = string.Compare(rec1DisplayName, rec2DisplayName, true);
              if (iComp != 0)
              {
                return m_bSortAscending ? iComp : -iComp;
              }
              _currentSortMethod = SortMethod.Date;
              break;
            case SortMethod.Duration:
              {
                TimeSpan duration1 = (rec1.EndTime - rec1.StartTime);
                TimeSpan duration2 = rec2.EndTime - rec2.StartTime;
                if (!m_bSortAscending)
                {
                  TimeSpan duration3 = duration1;
                  duration1 = duration2;
                  duration2 = duration3;
                }
                if (duration1 != duration2)
                {
                  return duration1 < duration2 ? 1 : -1;
                }
                _currentSortMethod = SortMethod.Date;
                break;
              }
            case SortMethod.Date:
              DateTime t1 = rec1.StartTime;
              DateTime t2 = rec2.StartTime;
              if (!m_bSortAscending)
              {
                DateTime t3 = t1;
                t1 = t2;
                t2 = t3;
              }
              if (t1 != t2)
              {
                return rec1.StartTime < rec2.StartTime ? 1 : -1;
              }
              return 0;
            case SortMethod.Genre:
              var categoryRec1 = TVUtil.GetCategory(rec1.ProgramCategory);
              var categoryRec2 = TVUtil.GetCategory(rec2.ProgramCategory);
              item1.Label2 = categoryRec1;
              item2.Label2 = categoryRec2;
              iComp = string.Compare(categoryRec1, categoryRec2, true);
              if (iComp != 0)
              {
                return m_bSortAscending ? iComp : -iComp;
              }
              _currentSortMethod = SortMethod.Date;
              break;
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RecordedBase: Error comparing files");
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

    protected virtual void DoOnPlayBackStoppedOrChanged(g_Player.MediaType type, int stoptime, string filename, string caller)
    {
      this.LogInfo("RecordedBase:{0} {1} {2}", caller, type, filename);

      Recording rec = ServiceAgents.Instance.RecordingServiceAgent.GetRecordingByFileName(filename);
      if (rec != null)
      {
        if (stoptime >= g_Player.Duration)
        {
          stoptime = 0;
        }
        // temporary workaround before end of stream get's properly implemented        
        rec.StopTime = stoptime;
        ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(rec);
      }
      else
      {
        this.LogInfo("RecordedBase:{0} no recording found with filename {1}", caller, filename);
      }
    }

    private void OnPlayRecordingBackChanged(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
    {
      DoOnPlayBackStoppedOrChanged(type, stoptime, filename, "OnPlayRecordingBackChanged");
    }

    private void OnPlayRecordingBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      DoOnPlayBackStoppedOrChanged(type, stoptime, filename, "OnPlayRecordingBackStopped");
    }

    protected virtual void OnPlayRecordingBackEnded(g_Player.MediaType type, string filename)
    {
      g_Player.Stop();

      Recording rec = ServiceAgents.Instance.RecordingServiceAgent.GetRecordingByFileName(filename);
      if (rec != null)
      {
        if (_deleteWatchedShows || rec.KeepUntil == (int)RecordingKeepMethod.UntilWatched)
        {
          ServiceAgents.Instance.ControllerServiceAgent.DeleteRecording(rec.IdRecording);
        }
        else
        {
          rec.StopTime = 0;
          ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(rec);
        }
      }
    }

    protected virtual void OnPlayRecordingBackStarted(g_Player.MediaType type, string filename)
    {
      // set audio track based on user prefs.
      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int prefLangIdx = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      this.LogDebug("RecordedBase.OnPlayRecordingBackStarted(): setting audioIndex on tsreader {0}", prefLangIdx);
      g_Player.CurrentAudioStream = prefLangIdx;

      if (dualMonoMode != eAudioDualMonoMode.UNSUPPORTED)
      {
        g_Player.SetAudioDualMonoMode(dualMonoMode);
      }
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