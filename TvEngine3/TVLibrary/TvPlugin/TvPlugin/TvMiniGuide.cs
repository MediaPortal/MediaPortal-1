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
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Text;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;

namespace TvPlugin
{
  /// <summary>
  /// GUIMiniGuide
  /// </summary>
  /// 
  public class TvMiniGuide : GUIWindow, IRenderLayer
  {
    // Member variables                                  
    [SkinControl(34)] protected GUIButtonControl cmdExit = null;
    [SkinControl(35)] protected GUIListControl lstChannelsNoStateIcons = null;
    [SkinControl(36)] protected GUISpinControl spinGroup = null;
    [SkinControl(37)] protected GUIListControl lstChannelsWithStateIcons = null;

    protected GUIListControl lstChannels = null;

    private bool _canceled = false;
    private bool _running = false;
    private int _parentWindowID = 0;
    private GUIWindow _parentWindow = null;
    private Dictionary<int, List<Channel>> _tvGroupChannelListCache = null;

    private List<ChannelGroup> _channelGroupList = null;
    private Channel _selectedChannel;
    private bool _zap = true;
    private Stopwatch benchClock = null;
    private List<Channel> _channelList = new List<Channel>();

    private bool _byIndex = false;
    private bool _showChannelNumber = false;
    private int _channelNumberMaxLength = 3;

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
      GetID = (int) Window.WINDOW_MINI_GUIDE;
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

      GetID = (int) Window.WINDOW_MINI_GUIDE;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      _canceled = true;
      LoadSettings();
      return bResult;
    }


    /// <summary>
    /// Renderer
    /// </summary>
    /// <param name="timePassed"></param>
    public override void Render(float timePassed)
    {
      base.Render(timePassed); // render our controls to the screen
    }

    private void GetChannels(bool refresh)
    {
      if (refresh || _channelList == null)
      {
        _channelList = new List<Channel>();
      }

      if (_channelList.Count == 0)
      {
        try
        {
          if (TVHome.Navigator.CurrentGroup != null)
          {
            foreach (GroupMap chan in TVHome.Navigator.CurrentGroup.ReferringGroupMap())
            {
              Channel ch = chan.ReferencedChannel();
              if (ch.VisibleInGuide && ch.IsTv)
              {
                _channelList.Add(ch);
              }
            }
          }
        }
        catch
        {
        }

        if (_channelList.Count == 0)
        {
          Channel newChannel = new Channel(GUILocalizeStrings.Get(911), false, true, 0, DateTime.MinValue, false,
                                           DateTime.MinValue, 0, true, "", true, GUILocalizeStrings.Get(911));
          for (int i = 0; i < 10; ++i)
          {
            _channelList.Add(newChannel);
          }
        }
      }
    }

    /// <summary>
    /// On close
    /// </summary>
    private void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        _running = false;
        _parentWindow = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
      GUILayerManager.UnRegisterLayer(this);
    }

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
              if ((int) Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                // switching logic
                SelectedChannel = (Channel) lstChannels.SelectedListItem.MusicTag;

                Channel changeChannel = null;
                if (AutoZap)
                {
                  string selectedChan = (string) lstChannels.SelectedListItem.TVTag;
                  if ((TVHome.Navigator.CurrentChannel != selectedChan) || g_Player.IsTVRecording)
                  {
                    List<Channel> tvChannelList = GetChannelListByGroup();
                    if (tvChannelList != null)
                    {
                      changeChannel = (Channel) tvChannelList[lstChannels.SelectedListItemIndex];
                    }
                  }
                }
                _canceled = false;
                Close();

                //This one shows the zapOSD when changing channel from mini GUIDE, this is currently unwanted.
                /*
                TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
                if (TVWindow != null) TVWindow.UpdateOSD(changeChannel.Name);                
                */

                TVHome.UserChannelChanged = true;

                if (changeChannel != null)
                {
                  TVHome.ViewChannel(changeChannel);
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
              Close();
              _canceled = true;
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
          Close();
          return;
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          //_running = false;
          _canceled = true;
          Close();
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
      _running = false;
    }

    /// <summary>
    /// Page gets loaded
    /// </summary>
    protected override void OnPageLoad()
    {
      benchClock = Stopwatch.StartNew();
      //Log.Debug("TvMiniGuide: onpageload");


      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      AllocResources();
      ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
      benchClock.Stop();
      Log.Debug("TvMiniGuide: All controls are reset after {0}ms", benchClock.ElapsedMilliseconds.ToString());

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
      GUIWaitCursor.Show();
      TVHome.Navigator.SetCurrentGroup(spinGroup.Value);
      GUIPropertyManager.SetProperty("#TV.Guide.Group", spinGroup.GetLabel());
      lstChannels.Clear();
      FillChannelList();
      GUIWaitCursor.Hide();
    }

    /// <summary>
    /// Fill up the list with groups
    /// </summary>
    public void FillGroupList()
    {
      benchClock.Reset();
      benchClock.Start();
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

    private List<Channel> GetChannelListByGroup()
    {
      int idGroup = TVHome.Navigator.CurrentGroup.IdGroup;

      if (_tvGroupChannelListCache == null)
      {
        _tvGroupChannelListCache = new Dictionary<int, List<Channel>>();
      }

      if (_tvGroupChannelListCache.ContainsKey(idGroup)) //already in cache ? then return it.
      {
        Log.Debug("TvMiniGuide: GetChannelListByGroup returning cached version of channels.");
        return _tvGroupChannelListCache[idGroup];
      }
      else //not in cache, fetch it and update cache, then return.
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        List<Channel> tvChannelList = layer.GetTVGuideChannelsForGroup(idGroup);

        if (tvChannelList != null)
        {
          Log.Debug("TvMiniGuide: GetChannelListByGroup caching channels from DB.");
          _tvGroupChannelListCache.Add(idGroup, tvChannelList);
          return tvChannelList;
        }
      }
      return new List<Channel>();
    }


    /// <summary>
    /// Fill the list with channels
    /// </summary>
    public void FillChannelList()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      List<Channel> tvChannelList = GetChannelListByGroup();

      benchClock.Reset();
      benchClock.Start();
      Dictionary<int, NowAndNext> listNowNext = layer.GetNowAndNext(tvChannelList);
      benchClock.Stop();
      Log.Debug("TvMiniGuide: FillChannelList retrieved {0} programs for {1} channels in {2} ms", listNowNext.Count,
                tvChannelList.Count, benchClock.ElapsedMilliseconds.ToString());
      Channel CurrentChan = null;
      GUIListItem item = null;
      string ChannelLogo = "";
      //List<int> RecChannels = null;
      //List<int> TSChannels = null;
      int SelectedID = 0;
      int CurrentChanState = 0;
      int CurrentId = 0;
      bool DisplayStatusInfo = true;
      string PathIconNoTune = GUIGraphicsContext.Skin + @"\Media\remote_blue.png";
      string PathIconTimeshift = GUIGraphicsContext.Skin + @"\Media\remote_yellow.png";
      string PathIconRecord = GUIGraphicsContext.Skin + @"\Media\remote_red.png";
      // fetch localized ID's only once from XML file
      string local736 = GUILocalizeStrings.Get(736); // No data available
      string local789 = GUILocalizeStrings.Get(789); // Now:
      string local790 = GUILocalizeStrings.Get(790); // Next:
      string local1054 = GUILocalizeStrings.Get(1054); // (recording)
      string local1055 = GUILocalizeStrings.Get(1055); // (timeshifting)
      string local1056 = GUILocalizeStrings.Get(1056); // (unavailable)    

      Dictionary<int, ChannelState> tvChannelStatesList = null;

      benchClock.Reset();
      benchClock.Start();

      if (TVHome.Navigator.CurrentGroup.GroupName.Equals(TvConstants.TvGroupNames.AllChannels) || (!g_Player.IsTV && !g_Player.Playing))
      {
        //we have no way of using the cached channelstates on the server in the following situations.
        // 1) when the "all channels" group is selected - too many channels.
        // 2) when user is not timeshifting - no user object on the server.
        User currentUser = new User();
        tvChannelStatesList = TVHome.TvServer.GetAllChannelStatesForGroup(TVHome.Navigator.CurrentGroup.IdGroup,
                                                                          currentUser);
      }
      else
      {
        // use the more speedy approach
        // ask the server of the cached list of channel states corresponding to the user.
        tvChannelStatesList = TVHome.TvServer.GetAllChannelStatesCached(TVHome.Card.User);

        if (tvChannelStatesList == null)
        {
          //slow approach.
          tvChannelStatesList = TVHome.TvServer.GetAllChannelStatesForGroup(TVHome.Navigator.CurrentGroup.IdGroup,
                                                                            TVHome.Card.User);
        }
      }

      benchClock.Stop();
      if (tvChannelStatesList != null)
      {
        Log.Debug("TvMiniGuide: FillChannelList - {0} channel states for group retrieved in {1} ms",
                  Convert.ToString(tvChannelStatesList.Count), benchClock.ElapsedMilliseconds.ToString());
      }

      for (int i = 0; i < tvChannelList.Count; i++)
      {
        CurrentChan = tvChannelList[i];
        CurrentId = CurrentChan.IdChannel;

        if (tvChannelStatesList != null && tvChannelStatesList.ContainsKey(CurrentId))
        {
          CurrentChanState = (int) tvChannelStatesList[CurrentId];
        }
        else
        {
          CurrentChanState = (int) ChannelState.tunable;
        }

        if (CurrentChan.VisibleInGuide)
        {
          StringBuilder sb = new StringBuilder();
          item = new GUIListItem("");
          // store here as it is not needed right now - please beat me later..
          item.TVTag = CurrentChan.DisplayName;
          item.MusicTag = CurrentChan;

          sb.Append(CurrentChan.DisplayName);
          ChannelLogo = Utils.GetCoverArt(Thumbs.TVChannel, CurrentChan.DisplayName);

          // if we are watching this channel mark it
          if (TVHome.Navigator.Channel.IdChannel == CurrentId)
          {
            item.IsRemote = true;
            SelectedID = lstChannels.Count;
          }

          if (File.Exists(ChannelLogo))
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

            switch (CurrentChanState)
            {
              case (int) ChannelState.nottunable: //not avail.                
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
              case (int) ChannelState.timeshifting: // timeshifting                
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
              case (int) ChannelState.recording: // recording                
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

          string tmpString = local736;

          if (listNowNext.ContainsKey(CurrentId))
          {
            //tmpString = CurrentChan.CurrentProgram.Title; <-- this would be SLOW
            if (!string.IsNullOrEmpty(listNowNext[CurrentId].TitleNow))
            {
              tmpString = listNowNext[CurrentId].TitleNow;
            }
          }
          item.Label2 = tmpString;
          item.Label3 = local789 + tmpString;

          if (_showChannelNumber == true)
          {
            string chanNumbers = " - ";
            foreach (TuningDetail detail in tvChannelList[i].ReferringTuningDetail())
            {
              chanNumbers = chanNumbers + detail.ChannelNumber + " - ";
            }
            // strip trailing " - "
            chanNumbers = chanNumbers.Remove(chanNumbers.Length - 3);
            sb.Append(chanNumbers);
          }

          if (listNowNext.ContainsKey(CurrentId))
          {
            // if the "Now" DB entry is in the future we set MinValue intentionally to avoid wrong percentage calculations
            if (listNowNext[CurrentId].NowStartTime != SqlDateTime.MinValue.Value)
            {
              sb.Append(" - ");
              sb.Append(
                CalculateProgress(listNowNext[CurrentId].NowStartTime, listNowNext[CurrentId].NowEndTime).ToString());
              sb.Append("%");
            }
          }
          //else
          //  sb.Append(CalculateProgress(DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1)).ToString());

          tmpString = local736;
          if ((listNowNext.ContainsKey(CurrentId)) && (listNowNext[CurrentId].IdProgramNext != -1))
          {
            tmpString = listNowNext[CurrentId].TitleNext;
          }

          item.Label2 = sb.ToString();
          item.Label = local790 + tmpString;

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
        fprogress = (passed.TotalMinutes/length.TotalMinutes)*100;
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

    /// <summary>
    /// Do this modal
    /// </summary>
    /// <param name="dwParentId"></param>
    public void DoModal(int dwParentId)
    {
      //Log.Debug("TvMiniGuide: domodal");
      _parentWindowID = dwParentId;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowID);
      if (null == _parentWindow)
      {
        //Log.Debug("TvMiniGuide: parentwindow = null");
        _parentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // activate this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      _running = true;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      while (_running && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }

      Close();
    }

    // Overlay IRenderLayer members

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      //TVHome.SendHeartBeat(); //not needed, now sent from tvoverlay.cs
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      if (_running)
      {
        Render(timePassed);
      }
    }

    #endregion
  }
}