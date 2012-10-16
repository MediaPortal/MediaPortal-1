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

#region usings

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mediaportal.Common.Utils;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.TvPlugin.Helper;
using Action = MediaPortal.GUI.Library.Action;
using TuningDetail = Mediaportal.TV.Server.TVDatabase.Entities.TuningDetail;

#endregion

namespace Mediaportal.TV.TvPlugin
{
  /// <summary>
  /// GUIMiniGuide
  /// </summary>
  /// 
  public class TvMiniGuide : GUIDialogWindow
  {
    // Member variables                                  
    [SkinControl(34)]
    protected GUIButtonControl cmdExit = null;

    [SkinControl(35)]
    protected GUIListControl lstChannelsNoStateIcons = null;

    [SkinControl(36)]
    protected GUISpinControl spinGroup = null;

    [SkinControl(37)]
    protected GUIListControl lstChannelsWithStateIcons = null;

    protected GUIListControl lstChannels = null;

    private bool _canceled = false;
    /*
    private bool _running = false;
    private int _parentWindowID = 0;
    private GUIWindow _parentWindow = null;
    */
    private Dictionary<int, List<Channel>> _tvGroupChannelListCache = null;    


    private List<ChannelGroup> _channelGroupList = null;
    private Channel _selectedChannel;
    private bool _zap = true;
    private Stopwatch benchClock = null;
    private List<Channel> _channelList = new List<Channel>();

    private bool _byIndex = false;
    private bool _showChannelNumber = false;
    private int _channelNumberMaxLength = 3;

    private Dictionary<int, DateTime> _nextEPGupdate = new Dictionary<int, DateTime>();
    private IDictionary<int, IDictionary<int, NowAndNext>> _listNowNext = new Dictionary<int, IDictionary<int, NowAndNext>>();

    private readonly string PathIconNoTune = GUIGraphicsContext.Skin + @"\Media\remote_blue.png";
    private readonly string PathIconTimeshift = GUIGraphicsContext.Skin + @"\Media\remote_yellow.png";
    private readonly string PathIconRecord = GUIGraphicsContext.Skin + @"\Media\remote_red.png";
    // fetch localized ID's only once from XML file
    private readonly string local736 = GUILocalizeStrings.Get(736); // No data available
    private readonly string local789 = GUILocalizeStrings.Get(789); // Now:
    private readonly string local790 = GUILocalizeStrings.Get(790); // Next:
    private readonly string local1054 = GUILocalizeStrings.Get(1054); // (recording)
    private readonly string local1055 = GUILocalizeStrings.Get(1055); // (timeshifting)
    private readonly string local1056 = GUILocalizeStrings.Get(1056); // (unavailable)    

    private StringBuilder sb = new StringBuilder();
    private StringBuilder sbTmp = new StringBuilder();

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _byIndex = xmlreader.GetValueAsBool("mytv", "byindex", true);
        _showChannelNumber = xmlreader.GetValueAsBool("mytv", "showchannelnumber", false);
        _channelNumberMaxLength = xmlreader.GetValueAsInt("mytv", "channelnumbermaxlength", 3);
      }
    }

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public TvMiniGuide()
    {
      GetID = (int)Window.WINDOW_MINI_GUIDE;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is tv.
    /// </summary>
    /// <value><c>true</c> if this instance is tv; otherwise, <c>false</c>.</value>
    public override bool IsTv
    {
      get { return true; }
    }

    /// <summary>
    /// Gets a value indicating whether the dialog was canceled. 
    /// </summary>
    /// <value><c>true</c> if dialog was canceled without a selection</value>
    public bool Canceled
    {
      get { return _canceled; }
    }

    /// <summary>
    /// Gets or sets the selected channel.
    /// </summary>
    /// <value>The selected channel.</value>
    public Channel SelectedChannel
    {
      get { return _selectedChannel; }
      set { _selectedChannel = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [auto zap].
    /// </summary>
    /// <value><c>true</c> if [auto zap]; otherwise, <c>false</c>.</value>
    public bool AutoZap
    {
      get { return _zap; }
      set { _zap = value; }
    }

    /// <summary>
    /// Init method
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\TVMiniGuide.xml");

      GetID = (int)Window.WINDOW_MINI_GUIDE;
      //GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      _canceled = true;
      LoadSettings();
      return bResult;
    }

    /*
    /// <summary>
    /// Renderer
    /// </summary>
    /// <param name="timePassed"></param>
    public override void Render(float timePassed)
    {
      base.Render(timePassed); // render our controls to the screen
    }
    */


    /// <summary>
    /// On Message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            if (message.SenderControlId == 35 || message.SenderControlId == 37) // listbox
            {
              if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                // switching logic
                SelectedChannel = (Channel)lstChannels.SelectedListItem.TVTag;

                Server.TVDatabase.Entities.Channel changeChannel = null;
                if (AutoZap)
                {
                  if ((TVHome.Navigator.Channel.Entity.IdChannel != SelectedChannel.IdChannel) || g_Player.IsTVRecording)
                  {
                    List<Server.TVDatabase.Entities.Channel> tvChannelList = GetChannelListByGroup();
                    if (tvChannelList != null)
                    {
                      changeChannel = tvChannelList[lstChannels.SelectedListItemIndex] as Server.TVDatabase.Entities.Channel;
                    }
                  }
                }
                _canceled = false;
                PageDestroy();

                TVHome.UserChannelChanged = true;

                if (changeChannel != null)
                {
                  //todo: remove gentle
                  Channel ch = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(changeChannel.IdChannel);
                  TVHome.ViewChannel(ch);
                }
              }
            }
            else if (message.SenderControlId == 36) // spincontrol
            {
              // switch group              
              OnGroupChanged();
            }
            else if (message.SenderControlId == 34) // exit button
            {
              // exit
              _canceled = true;
              PageDestroy();
            }
            break;
          }
      }
      return base.OnMessage(message);
    }

    /// <summary>
    /// On action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_CONTEXT_MENU:
          //_running = false;
          PageDestroy();
          return;
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          //_running = false;
          _canceled = true;
          PageDestroy();
          return;
        case Action.ActionType.ACTION_MOVE_LEFT:
        case Action.ActionType.ACTION_TVGUIDE_PREV_GROUP:
          // switch group
          spinGroup.MoveUp();
          return;
        case Action.ActionType.ACTION_MOVE_RIGHT:
        case Action.ActionType.ACTION_TVGUIDE_NEXT_GROUP:
          // switch group
          spinGroup.MoveDown();
          return;
      }
      base.OnAction(action);
    }

    /// <summary>
    /// Page gets destroyed
    /// </summary>
    /// <param name="new_windowId"></param>
    protected override void OnPageDestroy(int new_windowId)
    {
      //Log.Debug("TvMiniGuide: OnPageDestroy");
      base.OnPageDestroy(new_windowId);
      //_running = false;
    }

    /// <summary>
    /// Page gets loaded
    /// </summary>
    protected override void OnPageLoad()
    {
      AllocResources();
      ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset            

      lstChannels = getChannelList();

      if (lstChannelsWithStateIcons != null)
      {
        lstChannelsWithStateIcons.Visible = false;
      }
      lstChannels.Visible = true;

      spinGroup.CycleItems = true;

      FillChannelList();
      FillGroupList();
      base.OnPageLoad();
    }

    private GUIListControl getChannelList()
    {
      GUIListControl lstChannels = null;

      if (TVHome.ShowChannelStateIcons() && lstChannelsWithStateIcons != null)
      {
        lstChannels = lstChannelsWithStateIcons;
      }
      else
      {
        lstChannels = lstChannelsNoStateIcons;
      }

      return lstChannels;
    }

    private void OnGroupChanged()
    {
      Stopwatch bClock = Stopwatch.StartNew();

      GUIWaitCursor.Show();
      TVHome.Navigator.SetCurrentGroup(spinGroup.Value);
      GUIPropertyManager.SetProperty("#TV.Guide.Group", spinGroup.GetLabel());
      lstChannels.Clear();
      FillChannelList();
      GUIWaitCursor.Hide();

      Log.Debug("OnGroupChanged {0} took {1} msec", spinGroup.Value, bClock.ElapsedMilliseconds);

    }

    /// <summary>
    /// Fill up the list with groups
    /// </summary>
    public void FillGroupList()
    {
      benchClock = Stopwatch.StartNew();

      ChannelGroup current = null;
      _channelGroupList = TVHome.Navigator.Groups;
      // empty list of channels currently in the 
      // spin control
      spinGroup.Reset();
      // start to fill them up again
      for (int i = 0; i < _channelGroupList.Count; i++)
      {
        current = _channelGroupList[i];
        spinGroup.AddLabel(current.GroupName, i);
        // set selected
        if (current.GroupName.CompareTo(TVHome.Navigator.CurrentGroup.GroupName) == 0)
        {
          spinGroup.Value = i;
        }
      }

      if (_channelGroupList.Count < 2)
      {
        spinGroup.Visible = false;
      }

      benchClock.Stop();
      Log.Debug("TvMiniGuide: FillGroupList finished after {0} ms", benchClock.ElapsedMilliseconds.ToString());
    }

    private List<Server.TVDatabase.Entities.Channel> GetChannelListByGroup()
    {
      int idGroup = TVHome.Navigator.CurrentGroup.IdGroup;

      if (_tvGroupChannelListCache == null)
      {
        _tvGroupChannelListCache = new Dictionary<int, List<Server.TVDatabase.Entities.Channel>>();
      }

      List<Server.TVDatabase.Entities.Channel> channels = null;
      if (_tvGroupChannelListCache.TryGetValue(idGroup, out channels))  //already in cache ? then return it.      
      {
        Log.Debug("TvMiniGuide: GetChannelListByGroup returning cached version of channels.");
        return channels;
      }
      else //not in cache, fetch it and update cache, then return.
      {
        List<Server.TVDatabase.Entities.Channel> tvChannelList =
          ServiceAgents.Instance.ChannelServiceAgent.GetAllChannelsByGroupIdAndMediaType(
            TVHome.Navigator.CurrentGroup.IdGroup, MediaTypeEnum.TV, ChannelIncludeRelationEnum.TuningDetails).ToList();

        if (tvChannelList != null)
        {
          Log.Debug("TvMiniGuide: GetChannelListByGroup caching channels from DB.");
          _tvGroupChannelListCache.Add(idGroup, tvChannelList);
          return tvChannelList;
        }
        else
        {
          return new List<Server.TVDatabase.Entities.Channel>();
        }
      }
    }

    /// <summary>
    /// Fill the list with channels
    /// </summary>
    public void FillChannelList()
    {
      benchClock = Stopwatch.StartNew();
      DateTime nextEPGupdate = GetNextEpgUpdate();

      IList<Channel> tvChannelList = null;
      IDictionary<int, NowAndNext> listNowNext = null;
      ThreadHelper.ParallelInvoke(
        () => tvChannelList = GetChannelListByGroup(),
        () => listNowNext = GetNowAndNext(nextEPGupdate)
      );            

      benchClock.Stop();
      Log.Debug("TvMiniGuide: FillChannelList retrieved {0} programs for {1} channels in {2} ms", listNowNext.Count,
                tvChannelList.Count, benchClock.ElapsedMilliseconds.ToString());

      GUIListItem item = null;
      string ChannelLogo = "";
      //List<int> RecChannels = null;
      //List<int> TSChannels = null;
      int SelectedID = 0;
      int channelID = 0;
      bool DisplayStatusInfo = true;


      Dictionary<int, ChannelState> tvChannelStatesList = null;

      if (TVHome.ShowChannelStateIcons())
      {
        benchClock.Reset();
        benchClock.Start();

        tvChannelStatesList = TVHome.TvChannelStatesList;        

        benchClock.Stop();
        if (tvChannelStatesList != null)
        {
          Log.Debug("TvMiniGuide: FillChannelList - {0} channel states for group retrieved in {1} ms",
                    Convert.ToString(tvChannelStatesList.Count), benchClock.ElapsedMilliseconds.ToString());
        }
      }

      for (int i = 0; i < tvChannelList.Count; i++)
      {
        Channel currentChan = tvChannelList[i];

        if (currentChan.VisibleInGuide)
        {
          ChannelState currentChanState = ChannelState.tunable;
          channelID = currentChan.IdChannel;
          if (TVHome.ShowChannelStateIcons())
          {
            if (!tvChannelStatesList.TryGetValue(channelID, out currentChanState))
            {
              currentChanState = ChannelState.tunable;
            }
          }

          //StringBuilder sb = new StringBuilder();
          sb.Length = 0;
          item = new GUIListItem("");
          // store here as it is not needed right now - please beat me later..
          item.TVTag = currentChan;

          sb.Append(currentChan.DisplayName);
          ChannelLogo = Utils.GetCoverArt(Thumbs.TVChannel, currentChan.DisplayName);

          // if we are watching this channel mark it
          if (TVHome.Navigator != null && TVHome.Navigator.Channel.Entity != null &&
              TVHome.Navigator.Channel.Entity.IdChannel == channelID)
          {
            item.IsRemote = true;
            SelectedID = lstChannels.Count;
          }

          if (!string.IsNullOrEmpty(ChannelLogo))
          {
            item.IconImageBig = ChannelLogo;
            item.IconImage = ChannelLogo;
          }
          else
          {
            item.IconImageBig = string.Empty;
            item.IconImage = string.Empty;
          }

          if (DisplayStatusInfo)
          {
            bool showChannelStateIcons = (TVHome.ShowChannelStateIcons() && lstChannelsWithStateIcons != null);

            switch (currentChanState)
            {
              case ChannelState.nottunable:
                item.IsPlayed = true;
                if (showChannelStateIcons)
                {
                  item.PinImage = Thumbs.TvIsUnavailableIcon;
                }
                else
                {
                  sb.Append(" ");
                  sb.Append(local1056);
                }
                break;
              case ChannelState.timeshifting:
                if (showChannelStateIcons)
                {
                  item.PinImage = Thumbs.TvIsTimeshiftingIcon;
                }
                else
                {
                  sb.Append(" ");
                  sb.Append(local1055);
                }
                break;
              case ChannelState.recording:
                if (showChannelStateIcons)
                {
                  item.PinImage = Thumbs.TvIsRecordingIcon;
                }
                else
                {
                  sb.Append(" ");
                  sb.Append(local1054);
                }
                break;
              default:
                item.IsPlayed = false;
                if (showChannelStateIcons)
                {
                  item.PinImage = Thumbs.TvIsAvailableIcon;
                }
                break;
            }
          }
          
          sbTmp.Length = 0;

          NowAndNext currentNowAndNext;
          bool hasNowNext = listNowNext.TryGetValue(channelID, out currentNowAndNext);

          if (hasNowNext)
          {
            if (!string.IsNullOrEmpty(currentNowAndNext.TitleNow))
            {
              TVUtil.TitleDisplay(sbTmp, currentNowAndNext.TitleNow, currentNowAndNext.EpisodeName,
                                              currentNowAndNext.SeriesNum,
                                              currentNowAndNext.EpisodeNum, currentNowAndNext.EpisodePart);
            }
            else
            {
              sbTmp.Append(local736);
            }
          }
          else
          {
            sbTmp.Append(local736);
          }

          item.Label2 = sbTmp.ToString();
          sbTmp.Insert(0, local789);
          item.Label3 = sbTmp.ToString();

          sbTmp.Length = 0;

          if (_showChannelNumber == true)
          {
            sb.Append(" - ");
            if (!_byIndex)
            {              
              foreach (TuningDetail detail in tvChannelList[i].TuningDetails)
              {
                sb.Append(detail.ChannelNumber);
              }
            }
            else
            {
              sb.Append(i + 1);
            }
          }

          if (hasNowNext)
          {
            // if the "Now" DB entry is in the future we set MinValue intentionally to avoid wrong percentage calculations
            DateTime startTime = currentNowAndNext.NowStartTime;
            if (startTime != SqlDateTime.MinValue.Value)
            {
              DateTime endTime = currentNowAndNext.NowEndTime;
              sb.Append(" - ");
              sb.Append(
                CalculateProgress(startTime, endTime).ToString());
              sb.Append("%");

              if (endTime < nextEPGupdate || nextEPGupdate == DateTime.MinValue)
              {
                nextEPGupdate = endTime;
                SetNextEpgUpdate(endTime);
              }
            }
          }



          if (hasNowNext && listNowNext[channelID].IdProgramNext != -1)
          {
            TVUtil.TitleDisplay(sbTmp, currentNowAndNext.TitleNext, currentNowAndNext.EpisodeNameNext,
                                            currentNowAndNext.SeriesNumNext,
                                            currentNowAndNext.EpisodeNumNext,
                                            currentNowAndNext.EpisodePartNext);
          }
          else
          {
            sbTmp.Append(local736);
          }

          item.Label2 = sb.ToString();

          sbTmp.Insert(0, local790);

          item.Label = sbTmp.ToString();

          lstChannels.Add(item);
        }
      }
      benchClock.Stop();
      Log.Debug("TvMiniGuide: State check + filling completed after {0} ms", benchClock.ElapsedMilliseconds.ToString());
      lstChannels.SelectedListItemIndex = SelectedID;

      if (lstChannels.GetID == 37)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, 37, 0, 0, null);
        OnMessage(msg);
      }

      sb.Length = 0;
      sbTmp.Length = 0;
    }

    private IDictionary<int, NowAndNext> GetNowAndNext(DateTime nextEPGupdate)
    {
      IDictionary<int, NowAndNext> getNowAndNext = new Dictionary<int, NowAndNext>();
      int idGroup = TVHome.Navigator.CurrentGroup.IdGroup;
      if (_listNowNext.TryGetValue(idGroup, out getNowAndNext))
      {
        bool updateNow = (DateTime.Now >= nextEPGupdate);
        if (updateNow)
        {
          getNowAndNext = ServiceAgents.Instance.ProgramServiceAgent.GetNowAndNextForChannelGroup(TVHome.Navigator.CurrentGroup.IdGroup);
          _listNowNext[idGroup] = getNowAndNext;
        }
      }
      else
      {
        getNowAndNext = ServiceAgents.Instance.ProgramServiceAgent.GetNowAndNextForChannelGroup(TVHome.Navigator.CurrentGroup.IdGroup);
        _listNowNext.Add(idGroup, getNowAndNext);
      }
      return getNowAndNext;
    }

    private void SetNextEpgUpdate(DateTime nextEPGupdate)
    {
      int idGroup = TVHome.Navigator.CurrentGroup.IdGroup;
      if (_nextEPGupdate.ContainsKey(idGroup))
      {
        _nextEPGupdate[idGroup] = nextEPGupdate;
      }
      else
      {
        _nextEPGupdate.Add(idGroup, nextEPGupdate);
      }
    }

    private DateTime GetNextEpgUpdate()
    {
      DateTime nextEPGupdate = DateTime.MinValue;      
      int idGroup = TVHome.Navigator.CurrentGroup.IdGroup;

      _nextEPGupdate.TryGetValue(idGroup, out nextEPGupdate);
      return nextEPGupdate;
    }

    /// <summary>
    /// Get current tv program
    /// </summary>
    /// <param name="prog"></param>
    /// <returns></returns>
    private double CalculateProgress(DateTime start, DateTime end)
    {
      TimeSpan length = end - start;
      TimeSpan passed = DateTime.Now - start;
      double fprogress = 0;
      if (length.TotalMinutes > 0)
      {
        fprogress = (passed.TotalMinutes / length.TotalMinutes) * 100;
        fprogress = Math.Floor(fprogress);
        if (fprogress > 100.0f)
        {
          fprogress = 100.0f;
        }
        if (fprogress < 1.0f)
        {
          fprogress = 0;
        }
      }
      return fprogress;
    }
  }
}