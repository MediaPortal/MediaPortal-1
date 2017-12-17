#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Diagnostics;
using Gentle.Common;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace TvPlugin
{
  public class TvRecorded : RecordedBase, IComparer<GUIListItem>
  {
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
    private static Recording _oActiveRecording = null;
    private static bool _bIsLiveRecording = false;
    private bool _deleteWatchedShows = false;
    private int _iSelectedItem = 0;
    private string _currentLabel = string.Empty;
    private int _rootItem = 0;
    private bool _resetSMSsearch = false;
    private bool _oldStateSMSsearch;
    private DateTime _resetSMSsearchDelay;

    [SkinControl(6)]
    protected GUIButtonControl btnCleanup = null;
    [SkinControl(7)]
    protected GUIButtonControl btnCompress = null;
    

    #endregion

    #region Constructor

    public TvRecorded()
    {
      GetID = (int)Window.WINDOW_RECORDEDTV;
    }

    #endregion

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Settings xmlreader = new MPSettings())
      {
        currentLayout = (Layout)xmlreader.GetValueAsInt(SerializeName, "layout", (int)Layout.List);
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
      }
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

      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\mytvrecordedtv.xml"));
      //LoadSettings();
      GUIWindowManager.Replace((int)Window.WINDOW_RECORDEDTV, this);
      Restore();
      PreInit();
      ResetAllControls();
      return bResult;
    }

    // Make sure we get all of the ACTION_PLAY event (OnAction only receives the ACTION_PLAY event when
    // the player is not playing)...
    private void OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PLAY
           || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
          && GUIWindowManager.ActiveWindow == GetID)
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
      if (!TVHome.Connected)
      {
        RemoteControl.Clear();
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_TVENGINE);
        return;
      }

      TVHome.WaitForGentleConnection();

      if (TVHome.Navigator == null)
      {
        TVHome.OnLoaded();
      }
      else if (TVHome.Navigator.Channel == null)
      {
        TVHome.m_navigator.ReLoad();
        TVHome.LoadSettings(false);
      }

      // Create the channel navigator (it will load groups and channels)
      if (TVHome.m_navigator == null)
      {
        TVHome.m_navigator = new ChannelNavigator();
      }

      base.OnPageLoad();
      InitViewSelections();

      //DeleteInvalidRecordings();

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
    }

    protected override bool AllowLayout(Layout layout)
    {
      // Disable playlist for now as it makes no sense to move recording entries
      if (layout == Layout.Playlist)
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

      Recording rec = (Recording)pItem.TVTag;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();

      dlg.SetHeading(TVUtil.GetDisplayTitle(rec));

      if (pItem.IsFolder)
      {
        dlg.AddLocalizedString(656); //Delete recorded tv
      }
      else
      {
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
      }

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
      try {
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
        Log.Error("TvRecorded: Error in ShowViews - {0}", ex.ToString());
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
        Log.Warn("TVRecorded: Error updating button states - {0}", ex.ToString());
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

    private List<Recording> ListFolder()
    {
      List<Recording> listRecordings = new List<Recording>();

      try
      {
        // lookup radio channel ID in radio group map (smallest table that could identify a radio channel) to remove radiochannels from recording list
        IEnumerable<int> radiogroupIDs = RadioGroupMap.ListAll().Select(radiogroup => radiogroup.IdChannel).ToList();

        //List<Recording> recordings = (from r in Recording.ListAll()where !(from rad in radiogroups select rad.IdChannel).Contains(r.IdChannel)select r).ToList();
        List<Recording> recordings = Recording.ListAll().Where(rec => radiogroupIDs.All(id => rec.IdChannel != id)).ToList();

        // load the active channels only once to save multiple requests later when retrieving related channel
        List<Channel> channels = Channel.ListAll().ToList();

        // load the active recordings once to mark them later in GUI lists and groups
        List<Recording> activerecordings = Recording.ListAllActive().ToList();

        var actualLabel = _currentLabel == GUILocalizeStrings.Get(2014) ? string.Empty : _currentLabel;

        foreach (var rec in recordings)
        {
          var addToList = true;
          switch (_currentDbView)
          {
            case DBView.History:
              addToList = GetSpokenViewDate(rec.StartTime).Equals(actualLabel);
              break;
            case DBView.Recordings:
              addToList = rec.Title.Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase) ||
                          TVUtil.GetDisplayTitle(rec).Equals(actualLabel, StringComparison.InvariantCultureIgnoreCase);
              break;
            case DBView.Channel:
              // possible that recording links to a channel that no longer exists
              // make sure we pick those up if that value is selected
              Channel channel = channels.FirstOrDefault(chan => rec.IdChannel == chan.IdChannel);
              addToList = actualLabel.Equals(GUILocalizeStrings.Get(1507)) && channel == null ||
                          GetChannelRecordingDisplayName(rec, channel).Equals(actualLabel, StringComparison.InvariantCultureIgnoreCase);
              break;
            case DBView.Genre:
              addToList = rec.Genre.Equals(actualLabel, StringComparison.InvariantCultureIgnoreCase);
              break;
          }

          if (!addToList) continue;

          // Add new list item for this recording

          listRecordings.Add(rec);
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error fetching recordings from database {0}", ex.Message);
      }

      return listRecordings;
    }

    private void LoadDirectory()
    {
      var watch = new Stopwatch(); watch.Reset(); watch.Start();

      try
      {
        GUIControl.ClearControl(GetID, facadeLayout.GetID);

        SwitchLayout();

        // lookup radio channel ID in radio group map (smallest table that could identify a radio channel) to remove radiochannels from recording list
        IEnumerable<int> radiogroupIDs = RadioGroupMap.ListAll().Select(radiogroup => radiogroup.IdChannel).ToList();
        Log.Debug("LoadDirectory() - finished loading '" + radiogroupIDs.Count() + "' radiogroupIDs after '{0}' ms.", watch.ElapsedMilliseconds);

        //List<Recording> recordings = (from r in Recording.ListAll()where !(from rad in radiogroups select rad.IdChannel).Contains(r.IdChannel)select r).ToList();
        List<Recording> recordings = Recording.ListAll().Where(rec => radiogroupIDs.All(id => rec.IdChannel != id)).ToList();
        Log.Debug("LoadDirectory() - finished loading '" + recordings.Count + "' recordings after '{0}' ms.", watch.ElapsedMilliseconds);

        // load the active channels only once to save multiple requests later when retrieving related channel
        List<Channel> channels = Channel.ListAll().ToList();
        Log.Debug("LoadDirectory() - finished loading '" + channels.Count + "' channels after '{0} ms.", watch.ElapsedMilliseconds);

        // load the active recordings once to mark them later in GUI lists and groups
        List<Recording> activerecordings = Recording.ListAllActive().ToList();
        Log.Debug("LoadDirectory() - finished loading '" + activerecordings.Count + "' activerecordings after '{0} ms.", watch.ElapsedMilliseconds);

        bool singleRecording = false; // check if this is a single recording and therefore should be placed in a folder

        if (_currentLabel == string.Empty)
        {
          // we are not browsing individual records
          // first build up a list of folders based on view
          // actual program associated will be the latest one for that folder (folder could be
          // date, channel, genre etc)

          IEnumerable<Recording> groups = recordings;

          switch (_currentDbView)
          {
            case DBView.History:
              groups = recordings.GroupBy(r => GetSpokenViewDate(r.StartTime)).Select(g => g.OrderByDescending(h => h.StartTime).First());
              break;
            case DBView.Recordings:
              groups = recordings.GroupBy(r => r.Title, StringComparer.InvariantCultureIgnoreCase).Select(g => g.OrderByDescending(h => h.StartTime).First());
              break;
            case DBView.Channel:
              //recording can link to channels that no longer exist. convert these to an unknown channel (string 1507) group
              groups = recordings.GroupBy(r =>
              {
                Channel channel = channels.FirstOrDefault(chan => r.IdChannel == chan.IdChannel);
                return channel == null ? GUILocalizeStrings.Get(1507) : channel.DisplayName;
              }, StringComparer.InvariantCultureIgnoreCase).Select(g => g.OrderByDescending(h => h.StartTime).First());
              break;
            case DBView.Genre:
              groups = recordings.GroupBy(r => r.Genre, StringComparer.InvariantCultureIgnoreCase).Select(g => g.OrderByDescending(h => h.StartTime).First());
              break;
          }

          foreach (Recording folder in groups)
          {
            GUIListItem item = new GUIListItem();
            switch (_currentDbView)
            {
              case DBView.History:
                item.Label = GetSpokenViewDate(folder.StartTime);
                item.Label2 = string.Empty;
                break;
              case DBView.Recordings:
                string title = folder.Title;
                singleRecording = true;
                int count = 0;
                foreach (Recording recording in recordings)
                {
                  if (!recording.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase)) continue;
                  count++;
                  if (count <= 1) continue;
                  singleRecording = false;
                  break;
                }

                if (singleRecording)
                {
                  item = BuildItemFromRecording(folder, channels.FirstOrDefault(chan => folder.IdChannel == chan.IdChannel));
                  item.Label2 = TVUtil.GetRecordingDateString(folder);
                }
                else
                {
                  item.Label = folder.Title;
                  item.Label2 = GetSpokenViewDate(folder.StartTime);
                }

                break;
              case DBView.Channel:
                // recordings can be linked to channels that no longer exist.
                Channel channel = channels.FirstOrDefault(chan => folder.IdChannel == chan.IdChannel);
                item.Label = channel == null ? GUILocalizeStrings.Get(1507) : channel.DisplayName;
                item.Label2 = GetSpokenViewDate(folder.StartTime);
                break;
              case DBView.Genre:
                item.Label = folder.Genre;
                item.Label2 = GetSpokenViewDate(folder.StartTime);
                break;
            }
            item.Label2 = GetSpokenViewDate(folder.StartTime);
            item.TVTag = folder;
            item.IsFolder = !singleRecording;
            if (activerecordings.Contains(folder)) item.PinImage = Thumbs.TvRecordingIcon;
            Utils.SetDefaultIcons(item);
            item.ThumbnailImage = item.IconImageBig;
            facadeLayout.Add(item);
            
            if (string.IsNullOrEmpty(item.Label)) 	
            {
              item.Label = GUILocalizeStrings.Get(2014); //unknown
            }
          }
        }
        else
        {
          #region Showing a folders content

          // add parent item
          var item = new GUIListItem("..") {IsFolder = true};
          Utils.SetDefaultIcons(item);
          facadeLayout.Add(item);

          var actualLabel = _currentLabel == GUILocalizeStrings.Get(2014) ? string.Empty : _currentLabel;

          foreach (var rec in recordings)
          {
            var addToList = true;
            switch (_currentDbView)
            {
              case DBView.History:
                addToList = GetSpokenViewDate(rec.StartTime).Equals(actualLabel);
                break;
              case DBView.Recordings:
                addToList = rec.Title.Equals(_currentLabel, StringComparison.InvariantCultureIgnoreCase) ||
                            TVUtil.GetDisplayTitle(rec).Equals(actualLabel, StringComparison.InvariantCultureIgnoreCase);
                break;
              case DBView.Channel:
                // possible that recording links to a channel that no longer exists
                // make sure we pick those up if that value is selected
                Channel channel = channels.FirstOrDefault(chan => rec.IdChannel == chan.IdChannel);
                addToList = actualLabel.Equals(GUILocalizeStrings.Get(1507)) && channel == null ||
                            GetChannelRecordingDisplayName(rec, channel).Equals(actualLabel, StringComparison.InvariantCultureIgnoreCase);
                break;
              case DBView.Genre:
                addToList = rec.Genre.Equals(actualLabel, StringComparison.InvariantCultureIgnoreCase);
                break;
            }

            if (!addToList) continue;

            // Add new list item for this recording
            item = BuildItemFromRecording(rec, channels.FirstOrDefault(chan => rec.IdChannel == chan.IdChannel));
            if (activerecordings.Contains(rec)) item.PinImage = Thumbs.TvRecordingIcon;
            item.Label = TVUtil.GetDisplayTitle(rec);

            item.Label2 = TVUtil.GetRecordingDateString(rec);
            facadeLayout.Add(item);
          }
          #endregion
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error fetching recordings from database {0}", ex.Message);
      }
      Log.Debug("LoadDirectory() - finished loading facade items after '{0}' ms.", watch.ElapsedMilliseconds);

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(facadeLayout.Count - (facadeLayout.Count > 0 && facadeLayout[0].Label == ".." ? 1 : 0)));

      OnSort();
      watch.Stop();
      Log.Debug("LoadDirectory() - finished sorting facade after '{0}' ms.", watch.ElapsedMilliseconds);

      UpdateProperties();
      UpdateThumbnails(); 
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

    public static string GetChannelRecordingDisplayName(Recording rec, Channel ch)
    {
      if (rec == null || ch == null)
      {
        return "";
      }

      return ch.DisplayName;
    }

    public static bool IsRecordingActual(Recording aRecording)
    {
      return aRecording.IsRecording;
    }

    private GUIListItem BuildItemFromRecording(Recording aRecording, Channel refCh)
    {
      string strDefaultUnseenIcon = GUIGraphicsContext.GetThemedSkinFile(@"\Media\defaultVideoBig.png");
      string strDefaultSeenIcon = GUIGraphicsContext.GetThemedSkinFile(@"\Media\defaultVideoSeenBig.png");
      GUIListItem item = null;

      try
      {
        // Re-imported channels might still be valid but their channel does not need to be present anymore...
        string strChannelName = refCh != null ? refCh.DisplayName : GUILocalizeStrings.Get(1507); // unknown


        // Log.Debug("TVRecorded: BuildItemFromRecording [{0}]: {1} ({2}) on channel {3}", _currentDbView.ToString(), aRecording.Title, aRecording.Genre, strChannelName);
        item = new GUIListItem { TVTag = aRecording, IsRemote = false };

        switch (_currentDbView)
        {
          case DBView.Recordings:
            item.Label = TVUtil.GetDisplayTitle(aRecording);
            break;
          case DBView.Channel:
            item.Label = strChannelName;
            break;
          case DBView.Genre:
            item.Label = !String.IsNullOrEmpty(aRecording.Genre) ? aRecording.Genre : GUILocalizeStrings.Get(2014); // unknown
            break;
          case DBView.History:
            item.Label = GetSpokenViewDate(aRecording.StartTime);
            break;
        }

        // Set a default logo indicating the watched status
        string SmallThumb = aRecording.TimesWatched > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;

        // Get the channel logo for the small icons
        string StationLogo = Utils.GetCoverArt(Thumbs.TVChannel, strChannelName);
        if (Utils.FileExistsInCache(StationLogo))
        {
          SmallThumb = StationLogo;
        }

        string PreviewThumb = string.Format("{0}\\{1}{2}", Thumbs.TVRecorded, Path.ChangeExtension(Utils.SplitFilename(aRecording.FileName), null), Utils.GetThumbExtension());

        if (Utils.FileExistsInCache(PreviewThumb))
        {
          // Search a larger one
          string PreviewThumbLarge = Utils.ConvertToLargeCoverArt(PreviewThumb);
          if (Utils.FileExistsInCache(PreviewThumbLarge))
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
          item.IsRemote = true;  // -> will load thumbnail image later
        }
        item.IconImage = SmallThumb;
      }
      catch (Exception singleex)
      {
        item = null;
        Log.Warn("TVRecorded: Error building item from recording {0}\n{1}", aRecording.FileName, singleex.ToString());
      }

      return item;
    }

    private void SetThumbnails(GUIListItem item)
    {
      if (item == null) return;
      Recording aRecording = item.TVTag as Recording;
      if (aRecording == null) return;

      try
      {
        string PreviewThumb = string.Format("{0}\\{1}{2}", Thumbs.TVRecorded, Path.ChangeExtension(Utils.SplitFilename(aRecording.FileName), null), Utils.GetThumbExtension());

        if (!Utils.FileExistsInCache(PreviewThumb))
        {
          Log.Debug("Thumbnail {0} does not exist in local thumbs folder - get it from TV server", PreviewThumb);
          string thumbnailFilename = string.Format("{0}{1}", Path.ChangeExtension(Utils.SplitFilename(aRecording.FileName), null), Utils.GetThumbExtension());

          try
          {
            byte[] thumbData = RemoteControl.Instance.GetRecordingThumbnail(thumbnailFilename);

            if (thumbData.Length > 0)
            {
              using (FileStream fs = new FileStream(PreviewThumb, FileMode.Create))
              {
                fs.Write(thumbData, 0, thumbData.Length);
                fs.Close();
                fs.Dispose();
              }
              Utils.DoInsertExistingFileIntoCache(PreviewThumb);
            }
            else
            {
              Log.Debug("Thumbnail {0} not found on TV server", thumbnailFilename);
            }
          }
          catch (Exception ex)
          {
            Log.Error("Error fetching thumbnail {0} from TV server - {1}", thumbnailFilename, ex.Message);
          }
        }

        // Display previews only if the option to create them is active                
        if (Utils.FileExistsInCache(PreviewThumb))
        {
          // Search a larger one
          string PreviewThumbLarge = Utils.ConvertToLargeCoverArt(PreviewThumb);
          if (Utils.FileExistsInCache(PreviewThumbLarge))
          {
            PreviewThumb = PreviewThumbLarge;
          }
          item.ThumbnailImage = item.IconImageBig = PreviewThumb;
        }
      }
      catch (Exception singleex)
      {
        Log.Warn("TVRecorded - SetThumbnail: Error building item from recording {0}\n{1}", aRecording.FileName, singleex.ToString());
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
            item1.Label2 = TVUtil.GetRecordingDateString(rec);
          }
          if (currentLayout != Layout.List)
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
            catch (Exception) { }
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

    protected override bool OnSelectedRecording(int iItem)
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

          default: // from last stop time and on error
            break;
        }
      }

      if (TVHome.Card != null)
      {
        TVHome.Card.StopTimeShifting();
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
      return TVUtil.PlayRecording(rec, stoptime);
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
      
      _iSelectedItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return;
      }
      if (pItem.IsFolder)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo)
        {
          return;
        }

        dlgYesNo.SetHeading(GUILocalizeStrings.Get(2166));
        dlgYesNo.SetLine(1, pItem.Label);
        dlgYesNo.SetLine(2, string.Empty);
        dlgYesNo.SetLine(3, string.Empty);
        dlgYesNo.DoModal(GetID);
        if (!dlgYesNo.IsConfirmed)
        {
          return;
        }

        String savedCurrentLabel = _currentLabel;
        _currentLabel = pItem.Label;
        List<Recording> recItems = ListFolder();
        _currentLabel = savedCurrentLabel;

        foreach (Recording rec in recItems)
        {
          if (g_Player.Playing)
          {
            FileInfo fInfo = new FileInfo(g_Player.currentFileName);
            if (rec.FileName.IndexOf(fInfo.Name) > -1)
            {
              g_Player.Stop();
            }
          }

          bool isRec = IsRecordingActual(rec);

          if (isRec)
          {
            TvDatabase.Schedule sched = rec.ReferencedSchedule();
            if (!TVUtil.DeleteRecAndSchedWithPrompt(sched, rec.IdChannel))
            {
              continue;
            }
          }

          DeleteRecordingAndUpdateGUI(rec);
        }
      }
      else
      {
        Recording rec = (Recording)pItem.TVTag;

        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo)
        {
          return;
        }

        bool isRecPlaying = false;
        if (g_Player.currentFileName.Length > 0 && (g_Player.IsTVRecording || g_Player.Playing))
        {
          FileInfo fInfo = new FileInfo(g_Player.currentFileName);
          isRecPlaying = (rec.FileName.IndexOf(fInfo.Name) > -1);
        }

        dlgYesNo.SetDefaultToYes(false);
        bool isRec = IsRecordingActual(rec);

        bool remove = false;
        if (isRec)
        {
          TvDatabase.Schedule sched = rec.ReferencedSchedule();
          remove = TVUtil.DeleteRecAndSchedWithPrompt(sched, rec.IdChannel);
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
            Log.Info("g_Player.Stopped {0}", g_Player.Stopped);
            g_Player.Stop();
          }
          DeleteRecordingAndUpdateGUI(rec);
        }
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
      CacheManager.Clear();

      LoadDirectory();

      while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
      {
        _iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _iSelectedItem);

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
      CacheManager.Clear();
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
      RemoteControl.Instance.DeleteWatchedRecordings(currentTitle);
    }

    private bool DeleteInvalidRecordings()
    {
      Stopwatch watch = new Stopwatch(); watch.Reset(); watch.Start();
      bool deletedrecordings = RemoteControl.Instance.DeleteInvalidRecordings();
      watch.Stop();
      Log.Debug("DeleteInvalidRecordings() - finished after '" + watch.ElapsedMilliseconds + "' ms., deletedrecordings = '" + deletedrecordings + "'");
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
              if (item == null) return;
              if (item.IsRemote)
              {
                SetThumbnails(item);
                item.IsRemote = false;
                count++;
              }
            }
            Log.Debug("TvRecorded: Updated '{0}' thumbnails", count.ToString());
          }
          catch (Exception ex)
          {
            Log.Error("TvRecorded: Error updating thumbnails - {0}", ex.ToString());
          }
        }
        GUIWindowManager.SendThreadCallbackAndWait((p1, p2, data) => 0, 0, 0, null);
      }) { Name = "UpdateThumbnails", IsBackground = true, Priority = ThreadPriority.BelowNormal }.Start();
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
        else if (pItem != null && pItem.IsFolder && pItem.Label == "..")
        {
          Utils.SetDefaultIcons(pItem);
          GUIPropertyManager.SetProperty("#selectedthumb", pItem.IconImageBig);
        }
        rec = pItem.TVTag as Recording;
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
        string strTime = TVUtil.GetRecordingDateStringFull(rec);

        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", TVUtil.GetDisplayTitle(rec));
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", rec.Genre);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
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
        Log.Error("TvRecorded: Error setting item properties - {0}", ex.ToString());
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

        Recording rec1 = (Recording)item1.TVTag;
        Recording rec2 = (Recording)item2.TVTag;

        int iComp;
        SortMethod cSortMethod = _currentSortMethod;

        while (true) // starting with main sortmethod and sorting by secondary rules
        {
          switch (cSortMethod)
          {
            case SortMethod.Played:
              {
                item1.Label2 = string.Format("{0} {1}", rec1.TimesWatched, GUILocalizeStrings.Get(677)); //times
                item2.Label2 = string.Format("{0} {1}", rec2.TimesWatched, GUILocalizeStrings.Get(677)); //times
                if (rec1.TimesWatched != rec2.TimesWatched)
                {
                  return m_bSortAscending ? rec1.TimesWatched - rec2.TimesWatched : rec2.TimesWatched - rec1.TimesWatched;
                }

                cSortMethod = SortMethod.Name;
                break;
              }

            case SortMethod.Name:
              {
                iComp = string.Compare(TVUtil.GetDisplayTitle(rec1), TVUtil.GetDisplayTitle(rec2), true);
                if (iComp != 0)
                {
                  return m_bSortAscending ? iComp : -iComp;
                }

                cSortMethod = SortMethod.Channel;
                break;
              }

            case SortMethod.Channel:
              {
                // if there is no referenced channel (eg. recording that links to a channel that is now deleted)
                // set channel name to unknown channel string (1507) to avoid null reference exceptions
                Channel ch1 = rec1.ReferencedChannel();
                Channel ch2 = rec2.ReferencedChannel();
                string ch1Name = ch1 == null ? GUILocalizeStrings.Get(1507) : ch1.DisplayName;
                string ch2Name = ch2 == null ? GUILocalizeStrings.Get(1507) : ch2.DisplayName;

                iComp = string.Compare(ch1Name, ch2Name, true);
                if (iComp != 0)
                {
                  return m_bSortAscending ? iComp : -iComp;
                }

                cSortMethod = SortMethod.Date;
                break;
              }
            case SortMethod.Duration:
              {
                TimeSpan duration1 = (rec1.EndTime - rec1.StartTime);
                TimeSpan duration2 = rec2.EndTime - rec2.StartTime;
                if (duration1 != duration2)
                {
                  return duration1 > duration2 ? 1 : -1;
                }

                cSortMethod = SortMethod.Date;
                break;
              }
            case SortMethod.Date:
              {
                if (rec1.StartTime != rec2.StartTime)
                {
                  return m_bSortAscending ? (rec1.StartTime < rec2.StartTime ? 1 : -1) : (rec1.StartTime > rec2.StartTime ? 1 : -1);
                }
                return 0;
              }

            case SortMethod.Genre:
              {
                item1.Label2 = rec1.Genre;
                item2.Label2 = rec2.Genre;

                if (rec1.Genre != rec2.Genre)
                {
                  return m_bSortAscending ? string.Compare(rec1.Genre, rec2.Genre, true) : string.Compare(rec2.Genre, rec1.Genre, true);
                }

                if (rec1.StartTime != rec2.StartTime)
                {
                  return m_bSortAscending ? (rec1.StartTime - rec2.StartTime).Minutes : (rec2.StartTime - rec1.StartTime).Minutes;
                }

                if (rec1.IdChannel != rec2.IdChannel)
                {
                  // if there is no referenced channel (eg. recording that links to a channel that is now deleted)
                  // set channel name to unknown channel string (1507) to avoid null reference exceptions
                  Channel ch1 = rec1.ReferencedChannel();
                  Channel ch2 = rec2.ReferencedChannel();
                  string ch1Name = ch1 == null ? GUILocalizeStrings.Get(1507) : ch1.DisplayName;
                  string ch2Name = ch2 == null ? GUILocalizeStrings.Get(1507) : ch2.DisplayName;
                  return m_bSortAscending ? string.Compare(ch1Name, ch2Name) : string.Compare(ch2Name, ch1Name);
                }

                if (TVUtil.GetDisplayTitle(rec1) != TVUtil.GetDisplayTitle(rec2))
                {
                  return m_bSortAscending ? string.Compare(TVUtil.GetDisplayTitle(rec1), TVUtil.GetDisplayTitle(rec2)) : string.Compare(TVUtil.GetDisplayTitle(rec2), TVUtil.GetDisplayTitle(rec1));
                }

                cSortMethod = SortMethod.Date;
                break;
              }
          }
        }
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