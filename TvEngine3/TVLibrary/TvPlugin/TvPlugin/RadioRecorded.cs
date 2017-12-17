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
using System.Threading;
using System.Windows.Forms;
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
  public class RadioRecorded : RecordedBase, IComparer<GUIListItem>
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

    #endregion

    #region Skin variables

    [SkinControl(6)] protected GUIButtonControl btnCleanup = null;
    [SkinControl(7)] protected GUIButtonControl btnCompress = null;

    #endregion

    #region Constructor

    public RadioRecorded()
    {
      GetID = (int)Window.WINDOW_RECORDEDRADIO;
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
        
        string strTmp = xmlreader.GetValueAsString("radiorecorded", "sort", "channel");

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
            xmlwriter.SetValue("radiorecorded", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("radiorecorded", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("radiorecorded", "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("radiorecorded", "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("radiorecorded", "sort", "played");
            break;
          case SortMethod.Duration:
            xmlwriter.SetValue("radiorecorded", "sort", "duration");
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
        return "radiorecorded";
      }
    }

    public override bool Init()
    {
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayRecordingBackStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayRecordingBackEnded);
      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayRecordingBackStarted);
      g_Player.PlayBackChanged += new g_Player.ChangedHandler(OnPlayRecordingBackChanged);

      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\myradiorecorded.xml"));
      //GUIWindowManager.Replace((int)Window.WINDOW_RECORDEDRADIO, this);
      //Restore();
      //PreInit();
      //ResetAllControls();
      return bResult;
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

      // Reload ChannelGroups
      Radio radioLoad = (Radio)GUIWindowManager.GetWindow((int)Window.WINDOW_RADIO);
      radioLoad.OnAdded();

      base.OnPageLoad();
      InitViewSelections();
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

    protected override void OnClick(int iItem)
    {
      OnSelectedRecording(iItem);
    }

    protected override void OnInfo(int iItem)
    {
      OnShowContextMenu();
    }

    protected override void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(495); //Sort Options
      dlg.AddLocalizedString(620); //channel
      dlg.AddLocalizedString(621); //date
      dlg.AddLocalizedString(268); //title
      dlg.AddLocalizedString(678); //genre
      dlg.AddLocalizedString(671); //watched
      dlg.AddLocalizedString(1017); //duration


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

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();

      Recording rec = (Recording)pItem.TVTag;

      dlg.SetHeading(TVUtil.GetDisplayTitle(rec));

      if (pItem.IsFolder)
      {
        dlg.AddLocalizedString(618); //Delete recorded tv
      }
      else
      {
        dlg.AddLocalizedString(208); //Play
        dlg.AddLocalizedString(618); //Delete
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
        case 618: // delete
          OnDeleteRecording(iItem);
          break;

        case 208: // play
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
      btnViews.AddItem(GUILocalizeStrings.Get(812), index++); // Radio stations
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
        Log.Error("RadioRecorded: Error in ShowViews - {0}", ex.ToString());
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
        Log.Warn("RadioRecorded: Error updating button states - {0}", ex.ToString());
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
        Log.Info("RadioRecorded: ShowFullScreenWindow switching to fullscreen video");
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
        Log.Error("RadioRecorded: Error in ShowUpcomingEpisodes - {0}", ex.ToString());
      }
    }

    private List<Recording> ListFolder()
    {
      List<Recording> listRecordings = new List<Recording>();

      try
      {
        IList<Recording> recordings = Recording.ListAll();

        var actualLabel = _currentLabel == GUILocalizeStrings.Get(2014) ? string.Empty : _currentLabel;

        foreach (var rec in recordings)
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

          if (!addToList) continue;

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
      List<GUIListItem> itemlist = new List<GUIListItem>();
      try
      {
        GUIControl.ClearControl(GetID, facadeLayout.GetID);

        IList<RadioGroupMap> radiogroups = RadioGroupMap.ListAll();
        IList<Recording> recordings = Recording.ListAll();
        if (_currentLabel == string.Empty)
        {
          foreach (Recording rec in recordings)
          {
            // catch exceptions here so MP will go on and list further recs
            try
            {
              bool isRadioChannel = false;
              foreach (RadioGroupMap radiogroup in radiogroups)
              {
                if (rec.IdChannel == radiogroup.IdChannel)
                {
                  isRadioChannel = true;
                  break;
                }
              }
              if (isRadioChannel == false) continue;  // only RadioChannels are allowed 
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
              Log.Error("RadioRecorded: error processing recordings - {0}", recex.Message);
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

          // Log.Debug("RadioRecorded: Currently showing the virtual folder contents of {0}", _currentLabel);
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
        Log.Error("RadioRecorded: Error fetching recordings from database {0}", ex.Message);
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
        Log.Error("RadioRecorded: Error adding recordings to list - {0}", ex2.Message);
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
      string strDefaultUnseenIcon = GUIGraphicsContext.GetThemedSkinFile(@"\Media\defaultVideoBig.png");
      string strDefaultSeenIcon = GUIGraphicsContext.GetThemedSkinFile(@"\Media\defaultVideoSeenBig.png");
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
        catch (Exception) { }
        if (refCh != null)
        {
          strChannelName = refCh.DisplayName;
        }
        if (!String.IsNullOrEmpty(aRecording.Genre))
        {
          strGenre = aRecording.Genre;
        }

        // Log.Debug("RadioRecorded: BuildItemFromRecording [{0}]: {1} ({2}) on channel {3}", _currentDbView.ToString(), aRecording.Title, aRecording.Genre, strChannelName);
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
        string StationLogo = Utils.GetCoverArt(Thumbs.Radio, strChannelName);
        if (!string.IsNullOrEmpty(StationLogo))            
        {
          SmallThumb = StationLogo;
        }

        item.IconImage = item.ThumbnailImage = item.IconImageBig = SmallThumb;
        // Display previews only if the option to create them is active
        if (string.IsNullOrEmpty(PreviewThumb))                              
        {
          item.ThumbnailImage = String.Empty;
        }
        
        //Mark the recording with a "rec. symbol" if it is an active recording.
        if (IsRecordingActual(aRecording))
        {
          item.PinImage = Thumbs.TvRecordingIcon;
        }
      }
      catch (Exception singleex)
      {
        item = null;
        Log.Warn("RadioRecorded: Error building item from recording {0}\n{1}", aRecording.FileName, singleex.ToString());
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
          //Log.Debug("RadioRecorded: SetLabels - 1: {0}, 2: {1}, 3: {2}", item1.Label, item1.Label2, item1.Label3);
        }
        catch (Exception ex)
        {
          Log.Warn("RadioRecorded: error in SetLabels - {0}", ex.Message);
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

      // We have the Station Name in there to retrieve the correct Coverart for the station in the Vis Window
      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", pItem.Label);
      GUIPropertyManager.SetProperty("#Play.Current.Album", pItem.Label);
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", pItem.ThumbnailImage);
      
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
          GUIResumeDialog.ShowResumeDialog(rec.Title, rec.StopTime, mediaType);

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

      return TVUtil.PlayRecording(rec, stoptime, g_Player.MediaType.RadioRecording);
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
          dlgYesNo.SetLine(2, rec.Title);
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

      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _iSelectedItem);
    }

    private void DeleteWatchedRecordings(string currentTitle)
    {
      RemoteControl.Instance.DeleteWatchedRecordings(currentTitle);
    }

    private void DeleteInvalidRecordings()
    {
      if (RemoteControl.Instance.DeleteInvalidRecordings())
      {
        CacheManager.Clear();
        LoadDirectory();
        while (_iSelectedItem >= GetItemCount() && _iSelectedItem > 0)
        {
          _iSelectedItem--;
        }

        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _iSelectedItem);
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
          if (pItem.IsFolder && pItem.Label == "..")
          {
            MediaPortal.Util.Utils.SetDefaultIcons(pItem);
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
        Log.Error("RadioRecorded: Error updating properties - {0}", ex.ToString());
      }
    }

    private void SetProperties(Recording rec)
    {
      try
      {
        if (rec == null)
        {
          GUIPropertyManager.SetProperty("#Radio.Recorded.Title", "");
          GUIPropertyManager.SetProperty("#Radio.Recorded.Genre", "");
          GUIPropertyManager.SetProperty("#Radio.Recorded.Time", "");
          GUIPropertyManager.SetProperty("#Radio.Recorded.Channel", "");
          GUIPropertyManager.SetProperty("#Radio.Recorded.Description", "");
          GUIPropertyManager.SetProperty("#Radio.Recorded.thumb", "");
          return;
        }

        GUIPropertyManager.SetProperty("#Radio.Recorded.Title", rec.Title);
        GUIPropertyManager.SetProperty("#Radio.Recorded.Genre", rec.Genre);
        GUIPropertyManager.SetProperty("#Radio.Recorded.Time", TVUtil.GetRecordingDateStringFull(rec));
        GUIPropertyManager.SetProperty("#Radio.Recorded.Description", rec.Description);

        string strLogo = "";

        GUIPropertyManager.SetProperty("#Radio.Recorded.Channel", GetRecordingDisplayName(rec));
        strLogo = Utils.GetCoverArt(Thumbs.Radio, GetRecordingDisplayName(rec));

        if (!string.IsNullOrEmpty(strLogo))                                                
        {
          GUIPropertyManager.SetProperty("#Radio.Recorded.thumb", strLogo);
        }
        else
        {
          GUIPropertyManager.SetProperty("#Radio.Recorded.thumb", "defaultVideoBig.png");
        }
      }
      catch (Exception ex)
      {
        Log.Error("RadioRecorded: Error setting item properties - {0}", ex.ToString());
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
        Log.Error("RadioRecorded: Error sorting items - {0}", ex.ToString());
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
        Log.Error("RadioRecorded: Error comparing files - {0}", ex.ToString());
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
      Log.Info("RadioRecorded:{0} {1} {2}", caller, type, filename);
      if (type != g_Player.MediaType.Radio)
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
        Log.Info("RadioRecorded:{0} no recording found with filename {1}", caller, filename);
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
      if (type != g_Player.MediaType.Radio)
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
      if (type != g_Player.MediaType.Radio)
      {
        return;
      }

      // set audio track based on user prefs.
      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int prefLangIdx = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Log.Debug("RadioRecorded.OnPlayRecordingBackStarted(): setting audioIndex on tsreader {0}", prefLangIdx);
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
