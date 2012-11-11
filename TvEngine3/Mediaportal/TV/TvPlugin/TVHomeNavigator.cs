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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Mediaportal.Common.Utils;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

#endregion

namespace Mediaportal.TV.TvPlugin
{
  /// <summary>
  /// Handles the logic for channel zapping. This is used by the different GUI modules in the TV section.
  /// </summary>
  public class ChannelNavigator
  {

    #region config xml file

    private const string ConfigFileXml =
      @"<?xml version=|1.0| encoding=|utf-8|?> 
<ideaBlade xmlns:xsi=|http://www.w3.org/2001/XMLSchema-instance| xmlns:xsd=|http://www.w3.org/2001/XMLSchema| useDeclarativeTransactions=|false| version=|1.03|> 
  <useDTC>false</useDTC>
  <copyLocal>false</copyLocal>
  <logging>
    <archiveLogs>false</archiveLogs>
    <logFile>DebugLog.xml</logFile>
    <usesSeparateAppDomain>false</usesSeparateAppDomain>
    <port>0</port>
  </logging>
  <rdbKey name=|default| databaseProduct=|Unknown|>
    <connection>[CONNECTION]</connection>
    <probeAssemblyName>TVDatabase</probeAssemblyName>
  </rdbKey>
  <remoting>
    <remotePersistenceEnabled>false</remotePersistenceEnabled>
    <remoteBaseURL>http://localhost</remoteBaseURL>
    <serverPort>9009</serverPort>
    <serviceName>PersistenceServer</serviceName>
    <serverDetectTimeoutMilliseconds>-1</serverDetectTimeoutMilliseconds>
    <proxyPort>0</proxyPort>
  </remoting>
  <appUpdater/>
</ideaBlade>
";

    #endregion

    #region Private members

    private List<ChannelGroup> _groups = new List<ChannelGroup>();
    // Contains all channel groups (including an "all _channels" group)

    private int m_currentgroup = 0;
    private DateTime m_zaptime;
    private long m_zapdelay;
    private ChannelBLL m_zapchannel = null;
    private int m_zapChannelNr = -1;
    private int m_zapgroup = -1;
    private Channel _lastViewedChannel = null; // saves the last viewed Channel  // mPod    
    private ChannelBLL m_currentChannel = null;
    private IDictionary<int, Channel> _channels = new Dictionary<int, Channel>();
    private bool reentrant = false;    

    #endregion

    #region events & delegates

    internal event OnZapChannelDelegate OnZapChannel;
    internal delegate void OnZapChannelDelegate();

    #endregion

    #region Constructors

    public ChannelNavigator()
    {
      // Load all groups
      //ServiceProvider services = GlobalServiceProvider.Instance;
      this.LogDebug("ChannelNavigator: ctor()");

      ReLoad();
    }

    public void SetupDatabaseConnection()
    {
      /*string connectionString, provider;
      if (!TVHome.Connected)
      {
        return;
      }

      ServiceAgents.Instance.ControllerService.GetDatabaseConnectionString(out connectionString, out provider);

      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(Config.GetFile(Config.Dir.Config, "gentle.config"));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        node.InnerText = connectionString;
        nodeProvider.InnerText = provider;
        doc.Save(Config.GetFile(Config.Dir.Config, "gentle.config"));
      }
      catch (Exception ex)
      {
        this.LogError("Unable to create/modify gentle.config {0},{1}", ex.Message, ex.StackTrace);
      }
      this.LogInfo("ChannelNavigator::Reload()");*/
    }   
    
    public void ReLoad()
    {
      //System.Diagnostics.Debugger.Launch();
      try
      {
        _groups.Clear();
        SetupDatabaseConnection();
        Task taskGetAllChannels = Task.Factory.StartNew(GetAllChannels);
        Task taskGetOrCreateGroup = Task.Factory.StartNew(delegate
        {
          ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);
        });
        
        Task taskGetAllGroups = Task.Factory.StartNew(GetAllGroups);                
        taskGetOrCreateGroup.WaitAndHandleExceptions();
        taskGetAllChannels.WaitAndHandleExceptions();
        taskGetAllGroups.WaitAndHandleExceptions();

        TvNotifyManager.OnNotifiesChanged();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TVHome: Error in Reload");        
      }      
    }

    private void GetAllChannels()
    {
      if (_channels.Count == 0)
      {
        this.LogInfo("get _channels from database");
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannelsByMediaType(MediaTypeEnum.TV,
                                                                                                        ChannelIncludeRelationEnum
                                                                                                          .None);
        _channels = channels.Distinct().ToDictionary(c => c.IdChannel);
        this.LogInfo("found:{0} tv channels", _channels.Count); 
      }
    }

    private void GetAllGroups()
    {      
      if (_groups.Count == 0)
      {
        bool hideAllChannelsGroup;
        using (Settings xmlreader = new MPSettings())
        {
          hideAllChannelsGroup = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
        }

        this.LogInfo("get all groups from database");
        ChannelGroupIncludeRelationEnum include = ChannelGroupIncludeRelationEnum.GroupMaps;
        include |= ChannelGroupIncludeRelationEnum.GroupMapsChannel;

        if (hideAllChannelsGroup)
        {
          _groups =
            ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllCustomChannelGroups(include, MediaTypeEnum.TV).OrderBy(g => g.GroupName).
              ToList();
        }
        else
        {
          _groups =
            ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(MediaTypeEnum.TV, include).OrderBy(g => g.GroupName).ToList();
        }
        this.LogInfo("loaded {0} tv groups", _groups.Count);
      }
    }    

    #endregion

    #region Public properties

    /// <summary>
    /// Gets the channel that we currently watch.
    /// Returns empty string if there is no current channel.
    /// </summary>
    public string CurrentChannel
    {
      get
      {
        if (m_currentChannel.Entity == null)
        {
          return null;
        }
        return m_currentChannel.Entity.DisplayName;
      }
    }

    public ChannelBLL Channel
    {
      get { return m_currentChannel; }
    }

    /// <summary>
    /// Gets and sets the last viewed channel
    /// Returns empty string if no zap occurred before
    /// </summary>
    public Channel LastViewedChannel
    {
      get { return _lastViewedChannel; }
      set { _lastViewedChannel = value; }
    }

    /// <summary>
    /// Gets the currently active tv channel group.
    /// </summary>
    public ChannelGroup CurrentGroup
    {
      get
      {
        if (_groups.Count == 0)
        {
          return null;
        }
        return _groups[m_currentgroup];
      }
    }

    /// <summary>
    /// Gets the index of currently active tv channel group.
    /// </summary>
    public int CurrentGroupIndex
    {
      get { return m_currentgroup; }
    }

    /// <summary>
    /// Gets the list of tv channel groups.
    /// </summary>
    public List<ChannelGroup> Groups
    {
      get { return _groups; }
    }

    /// <summary>
    /// Gets the channel that we will zap to. Contains the current channel if not zapping to anything.
    /// </summary>
    public Channel ZapChannel
    {
      get
      {
        if (m_zapchannel == null)
        {
          return m_currentChannel.Entity;
        }
        return m_zapchannel.Entity;
      }
    }

    /// <summary>
    /// Gets the channel number that we will zap to. If not zapping by number or not zapping to anything, returns -1.
    /// </summary>
    public int ZapChannelNr
    {
      get
      {
        return m_zapChannelNr;
      }
      set
      {
        m_zapChannelNr = value;
      }
    }

    /// <summary>
    /// Gets the configured zap delay (in milliseconds).
    /// </summary>
    public long ZapDelay
    {
      get { return m_zapdelay; }
    }

    /// <summary>
    /// Gets the group that we will zap to. Contains the current group name if not zapping to anything.
    /// </summary>
    public string ZapGroupName
    {
      get
      {
        if (m_zapgroup == -1)
        {
          return CurrentGroup.GroupName;
        }
        return _groups[m_zapgroup].GroupName;
      }
    }

    #endregion

    #region Public methods

    public void ZapNow()
    {
      m_zaptime = DateTime.Now.AddSeconds(-1);
      RaiseOnZapChannelEvent();
      // this.LogInfo(Log.LogType.Error, "zapnow group:{0} current group:{0}", m_zapgroup, m_currentgroup);
      //if (m_zapchannel == null)
      //   this.LogInfo(Log.LogType.Error, "zapchannel==null");
      //else
      //   this.LogInfo(Log.LogType.Error, "zapchannel=={0}",m_zapchannel);
    }

    /// <summary>
    /// Checks if it is time to zap to a different channel. This is called during Process().
    /// </summary>
    public bool CheckChannelChange()
    {
      if (reentrant)
      {
        return false;
      }      
      try
      {
        reentrant = true;        
        // Zapping to another group or channel?
        if (m_zapgroup != -1 || m_zapchannel != null)
        {
          // Time to zap?
          if (DateTime.Now >= m_zaptime)
          {
            // Zapping to another group?
            if (m_zapgroup != -1 && m_zapgroup != m_currentgroup)
            {
              // Change current group and zap to the first channel of the group
              m_currentgroup = m_zapgroup;
              if (CurrentGroup != null && CurrentGroup.GroupMaps.Count > 0)
              {
                GroupMap gm = CurrentGroup.GroupMaps[0];
                Channel chan = gm.Channel;
                m_zapchannel.Entity = chan;
              }
            }
            m_zapgroup = -1;

            //if (m_zapchannel != m_currentchannel)
            //  lastViewedChannel = m_currentchannel;
            // Zap to desired channel
            if (m_zapchannel != null) // might be NULL after tuning failed
            {
              Channel zappingTo = m_zapchannel.Entity;

              //remember to apply the new group also.
              if (m_zapchannel.CurrentGroup != null)
              {
                m_currentgroup = GetGroupIndex(m_zapchannel.CurrentGroup.GroupName);
                this.LogInfo("Channel change:{0} on group {1}", zappingTo.DisplayName, m_zapchannel.CurrentGroup.GroupName);
              }
              else
              {
                this.LogInfo("Channel change:{0}", zappingTo.DisplayName);
              }
              m_zapchannel = null;
              TVHome.ViewChannel(zappingTo);
            }
            m_zapChannelNr = -1;
            reentrant = false;

            return true;
          }
        }
      }
      finally
      {
        reentrant = false;        
      }      
      return false;
    }

    /// <summary>
    /// Changes the current channel group.
    /// </summary>
    /// <param name="groupname">The name of the group to change to.</param>
    public void SetCurrentGroup(string groupname)
    {
      m_currentgroup = GetGroupIndex(groupname);
    }

    /// <summary>
    /// Changes the current channel group.
    /// </summary>
    /// <param name="groupIndex">The id of the group to change to.</param>
    public void SetCurrentGroup(int groupIndex)
    {
      m_currentgroup = groupIndex;
    }


    /// <summary>
    /// Ensures that the navigator has the correct current channel (retrieved from the Recorder).
    /// </summary>
    public void UpdateCurrentChannel()
    {
      Channel newChannel = null;
      //if current card is watching tv then use that channel
      int id;
      if (TVHome.Connected)
      {
        if (TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording)
        {
          id = TVHome.Card.IdChannel;
          if (id >= 0)
          {
            newChannel = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(id);
          }
        }
        else
        {
          // else if any card is recording
          // then get & use that channel
          if (TVHome.IsAnyCardRecording)
          {
            IUser user = new User();
            IVirtualCard card = new VirtualCard(user);
            if (card.IsRecording)
            {
              id = card.IdChannel;
              if (id >= 0)
              {
                newChannel = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(id);                
              }
            }
          }
        }
        if (newChannel == null)
        {
          newChannel = m_currentChannel.Entity;
        }

        int currentChannelId = 0;
        int newChannelId = 0;

        if (m_currentChannel.Entity != null)
        {
          currentChannelId = m_currentChannel.Entity.IdChannel;
        }

        if (newChannel != null)
        {
          newChannelId = newChannel.IdChannel;
        }

        if (currentChannelId != newChannelId)
        {
          m_currentChannel = new ChannelBLL(newChannel) {CurrentGroup = CurrentGroup};
        }
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelName">The channel to switch to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(Channel channel, bool useZapDelay)
    {
      this.LogDebug("ChannelNavigator.ZapToChannel {0} - zapdelay {1}", channel.DisplayName, useZapDelay);
      TVHome.UserChannelChanged = true;
      m_zapchannel = new ChannelBLL(channel) {CurrentGroup = null};

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
      RaiseOnZapChannelEvent();
    }

    /// <summary>
    /// Changes the current channel (based on channel number) after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannelNumber(int channelNr, bool useZapDelay)
    {
      IList<GroupMap> channels = CurrentGroup.GroupMaps;
      if (channelNr >= 0)
      {
        this.LogDebug("_channels.Count {0}", channels.Count);

        bool found = false;
        int iCounter = 0;
        Channel chan;
        while (iCounter < channels.Count && found == false)
        {
          chan = ((GroupMap)channels[iCounter]).Channel;

          this.LogDebug("chan {0}", chan.DisplayName);
          if (chan.VisibleInGuide)
          {
            foreach (TuningDetail detail in chan.TuningDetails)
            {
              this.LogDebug("detail nr {0} id{1}", detail.ChannelNumber, detail.IdChannel);

              if (detail.ChannelNumber == channelNr)
              {
                this.LogDebug("find channel: iCounter {0}, detail.channelNumber {1}, detail.name {2}, _channels.Count {3}",
                          iCounter, detail.ChannelNumber, detail.Name, channels.Count);
                found = true;
                ZapToChannel(iCounter + 1, useZapDelay);
              }
            }
          }
          iCounter++;
        }
        if (found)
        {
          m_zapChannelNr = channelNr;
        }
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(int channelNr, bool useZapDelay)
    {
      IList<GroupMap> groupMaps = CurrentGroup.GroupMaps;
      m_zapChannelNr = channelNr;
      channelNr--;
      if (channelNr >= 0 && channelNr < groupMaps.Count)
      {
        GroupMap gm = groupMaps[channelNr];
        Channel chan = gm.Channel;
        TVHome.UserChannelChanged = true;
        ZapToChannel(chan, useZapDelay);
      }
    }

    /// <summary>
    /// Changes to the next channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToNextChannel(bool useZapDelay)
    {
      Channel currentChan = null;
      int currindex;
      if (m_zapchannel == null)
      {
        currindex = GetChannelIndex(Channel.Entity);
        currentChan = Channel.Entity;
      }
      else
      {
        currindex = GetChannelIndex(m_zapchannel.Entity); // Zap from last zap channel 
        currentChan = Channel.Entity;
      }
      ChannelBLL chan;
      //check if channel is visible 
      //if not find next visible 
      do
      {
        // Step to next channel 
        currindex++;
        if (currindex >= CurrentGroup.GroupMaps.Count)
        {
          currindex = 0;
        }
        GroupMap gm = CurrentGroup.GroupMaps[currindex];
        chan = new ChannelBLL(gm.Channel);
      } while (!chan.Entity.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = chan;
      m_zapchannel.CurrentGroup = null;
      m_zapChannelNr = -1;
      this.LogInfo("Navigator:ZapNext {0}->{1}", currentChan.DisplayName, m_zapchannel.Entity.DisplayName);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
        {
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        }
        else
        {
          m_zaptime = DateTime.Now;
        }
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
      RaiseOnZapChannelEvent();
    }    

    private void RaiseOnZapChannelEvent ()
    {
      if (OnZapChannel != null)
      {
        OnZapChannel();
      }
    }

    /// <summary>
    /// Changes to the previous channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToPreviousChannel(bool useZapDelay)
    {
      Channel currentChan = null;
      int currindex;
      if (m_zapchannel == null)
      {
        currentChan = Channel.Entity;
        currindex = GetChannelIndex(Channel.Entity);
      }
      else
      {
        currentChan = m_zapchannel.Entity;
        currindex = GetChannelIndex(m_zapchannel.Entity); // Zap from last zap channel 
      }
      GroupMap gm;
      Channel chan;
      //check if channel is visible 
      //if not find next visible 
      do
      {
        // Step to prev channel 
        currindex--;
        if (currindex < 0)
        {
          currindex = CurrentGroup.GroupMaps.Count - 1;
        }
        gm = CurrentGroup.GroupMaps[currindex];
        chan = gm.Channel;
      } while (!chan.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = new ChannelBLL(chan) {CurrentGroup = null};
      m_zapChannelNr = -1;
      this.LogInfo("Navigator:ZapPrevious {0}->{1}",
               currentChan.DisplayName, m_zapchannel.Entity.DisplayName);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
        {
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        }
        else
        {
          m_zaptime = DateTime.Now;
        }
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
      RaiseOnZapChannelEvent();
    }

    /// <summary>
    /// Changes to the next channel group.
    /// </summary>
    public void ZapToNextGroup(bool useZapDelay)
    {
      if (m_zapgroup == -1)
      {
        m_zapgroup = m_currentgroup + 1;
      }
      else
      {
        m_zapgroup = m_zapgroup + 1; // Zap from last zap group
      }

      if (m_zapgroup >= _groups.Count)
      {
        m_zapgroup = 0;
      }

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
      RaiseOnZapChannelEvent();
    }

    /// <summary>
    /// Changes to the previous channel group.
    /// </summary>
    public void ZapToPreviousGroup(bool useZapDelay)
    {
      if (m_zapgroup == -1)
      {
        m_zapgroup = m_currentgroup - 1;
      }
      else
      {
        m_zapgroup = m_zapgroup - 1;
      }

      if (m_zapgroup < 0)
      {
        m_zapgroup = _groups.Count - 1;
      }

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
      RaiseOnZapChannelEvent();
    }

    /// <summary>
    /// Zaps to the last viewed Channel (without ZapDelay).  // mPod
    /// </summary>
    public void ZapToLastViewedChannel()
    {
      if (_lastViewedChannel != null)
      {
        TVHome.UserChannelChanged = true;
        m_zapchannel = new ChannelBLL(_lastViewedChannel);
        m_zapChannelNr = -1;
        m_zaptime = DateTime.Now;
        RaiseOnZapChannelEvent();
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Retrieves the index of the current channel.
    /// </summary>
    /// <returns></returns>
    private int GetChannelIndex(Channel ch)
    {
      IList<GroupMap> groupMaps = CurrentGroup.GroupMaps;
      for (int i = 0; i < groupMaps.Count; i++)
      {
        GroupMap gm = groupMaps[i];
        Channel chan = gm.Channel;
        if (chan.IdChannel == ch.IdChannel)
        {
          return i;
        }
      }
      return 0; // Not found, return first channel index
    }

    /// <summary>
    /// Retrieves the index of the group with the specified name.
    /// </summary>
    /// <param name="groupname"></param>
    /// <returns></returns>
    private int GetGroupIndex(string groupname)
    {
      for (int i = 0; i < _groups.Count; i++)
      {
        ChannelGroup group = _groups[i];
        if (group.GroupName == groupname)
        {
          return i;
        }
      }
      return -1;
    }

    public ChannelBLL GetChannel(int channelId)
    {
      return GetChannel(channelId, false);
    }

    public ChannelBLL GetChannel(int channelId, bool allChannels)
    {
      Channel channel;
      bool found = _channels.TryGetValue(channelId, out channel);
      if (found)
      {
        if (!allChannels && !channel.VisibleInGuide)
        {
          channel = null;
        }
      }
      return new ChannelBLL(channel);      
    }

    public Channel GetChannel(string channelName)
    {
      return _channels.Values.FirstOrDefault(chan => chan.DisplayName == channelName && chan.VisibleInGuide);
    }

    #endregion

    #region Serialization

    public void LoadSettings(Settings xmlreader)
    {
      this.LogInfo("ChannelNavigator::LoadSettings()");
      string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
      m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
      string groupname = xmlreader.GetValueAsString("mytv", "group", TvConstants.TvGroupNames.AllChannels);
      m_currentgroup = GetGroupIndex(groupname);
      if (m_currentgroup < 0 || m_currentgroup >= _groups.Count) // Group no longer exists?
      {
        m_currentgroup = 0;
      }

      m_currentChannel = new ChannelBLL(GetChannel(currentchannelName));

      if (m_currentChannel.Entity == null)
      {
        if (m_currentgroup < _groups.Count)
        {
          ChannelGroup group = _groups[m_currentgroup];
          if (group.GroupMaps.Count > 0)
          {
            GroupMap gm = group.GroupMaps[0];
            m_currentChannel = new ChannelBLL(gm.Channel);
          }
        }
      }

      //check if the channel does indeed belong to the group read from the XML setup file ?


      bool foundMatchingGroupName = false;

      if (m_currentChannel.Entity != null)
      {
        foreach (GroupMap groupMap in m_currentChannel.Entity.GroupMaps)
        {
          foundMatchingGroupName = DoesGroupNameContainGroupName(groupMap, groupname);
          if (foundMatchingGroupName)
          {
            break;
          }          
        }
      }

      //if we still havent found the right group, then iterate through the selected group and find the channelname.      
      if (!foundMatchingGroupName && m_currentChannel.Entity != null && _groups != null)
      {
        foreach (GroupMap groupMap in ((ChannelGroup)_groups[m_currentgroup]).GroupMaps)
        {
          if (groupMap.Channel.DisplayName == currentchannelName)
          {
            foundMatchingGroupName = true;
            m_currentChannel = GetChannel(groupMap.IdChannel);
            break;
          }
        }
      }


      // if the groupname does not match any of the groups assigned to the channel, then find the last group avail. (avoiding the all "_channels group") for that channel and set is as the new currentgroup
      if (!foundMatchingGroupName && m_currentChannel.Entity != null && m_currentChannel.Entity.GroupMaps.Count > 0)
      {
        GroupMap groupMap = m_currentChannel.Entity.GroupMaps[m_currentChannel.Entity.GroupMaps.Count - 1];

        ChannelGroup group = groupMap.ChannelGroup;        

        if (group != null)
        {
          m_currentgroup = GetGroupIndex(group.GroupName);

          if (m_currentgroup < 0 || m_currentgroup >= _groups.Count) // Group no longer exists?
          {
            m_currentgroup = 0;
          }
        }
        else
        {
          m_currentgroup = 0;
        }        
      }

      if (m_currentChannel.Entity != null)
      {
        m_currentChannel.CurrentGroup = CurrentGroup;
      }
    }

    public void SaveSettings(Settings xmlwriter)
    {
      string groupName = "";
      if (CurrentGroup != null)
      {
        groupName = CurrentGroup.GroupName.Trim();
        try
        {
          if (groupName != String.Empty)
          {
            if (m_currentgroup > -1)
            {
              groupName = _groups[m_currentgroup].GroupName;
            }
            else if (m_currentChannel.Entity != null)
            {
              groupName = m_currentChannel.CurrentGroup.GroupName;
            }

            if (groupName.Length > 0)
            {
              xmlwriter.SetValue("mytv", "group", groupName);
            }
          }
        }
        catch (Exception) {}
      }

      if (m_currentChannel.Entity != null)
      {
        try
        {
          if (m_currentChannel.Entity.MediaType == (int)MediaTypeEnum.TV)
          {
            bool foundMatchingGroupName = false;

            foreach (GroupMap groupMap in m_currentChannel.Entity.GroupMaps)
            {
              foundMatchingGroupName = DoesGroupNameContainGroupName(groupMap, groupName);
              if (foundMatchingGroupName)
              {
                break;
              }                        
            }
            if (foundMatchingGroupName)
            {
              xmlwriter.SetValue("mytv", "channel", m_currentChannel.Entity.DisplayName);
            }
            else
              //the channel did not belong to the group, then pick the first channel avail in the group and set this as the last channel.
            {
              if (m_currentgroup > -1)
              {
                ChannelGroup cg = _groups[m_currentgroup];
                if (cg.GroupMaps.Count > 0)
                {
                  GroupMap gm = cg.GroupMaps[0];
                  xmlwriter.SetValue("mytv", "channel", gm.Channel.DisplayName);
                }
              }
            }
          }
        }
        catch (Exception) {}
      }
    }

    private static bool DoesGroupNameContainGroupName (GroupMap groupMap, string groupname)
    {
      bool foundMatchingGroupName = false;
      ChannelGroup group = groupMap.ChannelGroup;
      if (group != null && group.GroupName == groupname)
      {
        foundMatchingGroupName = true;
      }

      return foundMatchingGroupName;
    }

    #endregion
  }
}