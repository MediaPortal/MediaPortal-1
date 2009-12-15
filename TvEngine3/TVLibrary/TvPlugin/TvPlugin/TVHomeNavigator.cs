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
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using Gentle.Framework;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;


#endregion

namespace TvPlugin
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
    <logFile>DebugMediaPortal.GUI.Library.Log.xml</logFile>
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

    private List<Channel> _channelList = new List<Channel>();

    private List<ChannelGroup> m_groups = new List<ChannelGroup>();
    // Contains all channel groups (including an "all channels" group)

    private int m_currentgroup = 0;
    private DateTime m_zaptime;
    private long m_zapdelay;
    private Channel m_zapchannel = null;
    private int m_zapgroup = -1;
    private Channel _lastViewedChannel = null; // saves the last viewed Channel  // mPod    
    private Channel m_currentChannel = null;
    private IList channels = new ArrayList();
    private bool reentrant = false;

    #endregion

    #region Constructors

    public ChannelNavigator()
    {
      // Load all groups
      //ServiceProvider services = GlobalServiceProvider.Instance;
      Log.Debug("ChannelNavigator: ctor()");

      ReLoad();
    }

    public void ReLoad()
    {
      //System.Diagnostics.Debugger.Launch();
      try
      {        
        string connectionString, provider;        
        if (!TVHome.Connected)
        {
          return;
        }

        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);

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
          Log.Error("Unable to create/modify gentle.config {0},{1}", ex.Message, ex.StackTrace);
        }

        Log.Info("ChannelNavigator::Reload()");
        ProviderFactory.ResetGentle(true);
        ProviderFactory.SetDefaultProviderConnectionString(connectionString);
        Log.Info("get channels from database");
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddConstraint(Operator.Equals, "isTv", 1);
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        Log.Info("found:{0} tv channels", channels.Count);
        TvNotifyManager.OnNotifiesChanged();
        m_groups.Clear();

        TvBusinessLayer layer = new TvBusinessLayer();
        RadioChannelGroup allRadioChannelsGroup = layer.GetRadioChannelGroupByName(TvConstants.RadioGroupNames.AllChannels);
        IList<Channel> radioChannels = layer.GetAllRadioChannels();
        if (radioChannels != null)
        {
          if (radioChannels.Count > allRadioChannelsGroup.ReferringRadioGroupMap().Count)
          {
            foreach (Channel radioChannel in radioChannels)
            {
              layer.AddChannelToRadioGroup(radioChannel, allRadioChannelsGroup);
            }
          }
        }
        Log.Info("Done.");

        Log.Info("get all groups from database");
        sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
        sb.AddOrderByField(true, "groupName");
        stmt = sb.GetStatement(true);
        IList<ChannelGroup> groups = ObjectFactory.GetCollection<ChannelGroup>(stmt.Execute());
        IList<GroupMap> allgroupMaps = GroupMap.ListAll();

        bool hideAllChannelsGroup = false;
        using (
          Settings xmlreader =
            new MPSettings())
        {
          hideAllChannelsGroup = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
        }


        foreach (ChannelGroup group in groups)
        {
          if (group.GroupName == TvConstants.TvGroupNames.AllChannels)
          {
            foreach (Channel channel in channels)
            {
              if (channel.IsTv == false)
              {
                continue;
              }
              bool groupContainsChannel = false;
              foreach (GroupMap map in allgroupMaps)
              {
                if (map.IdGroup != group.IdGroup)
                {
                  continue;
                }
                if (map.IdChannel == channel.IdChannel)
                {
                  groupContainsChannel = true;
                  break;
                }
              }
              if (!groupContainsChannel)
              {
                layer.AddChannelToGroup(channel, TvConstants.TvGroupNames.AllChannels);
              }
            }
            break;
          }
        }

        groups = ChannelGroup.ListAll();
        foreach (ChannelGroup group in groups)
        {
          //group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);
          if (hideAllChannelsGroup && group.GroupName.Equals(TvConstants.TvGroupNames.AllChannels) && groups.Count > 1)
          {
            continue;
          }
          m_groups.Add(group);
        }
        Log.Info("loaded {0} tv groups", m_groups.Count);

        //TVHome.Connected = true;
      }
      catch (Exception ex)
      {
        Log.Error("TVHome: Error in Reload");
        Log.Error(ex);
        //TVHome.Connected = false;
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
        if (m_currentChannel == null)
        {
          return null;
        }
        return m_currentChannel.DisplayName;
      }
    }

    public Channel Channel
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
        if (m_groups.Count == 0)
        {
          return null;
        }
        return (ChannelGroup)m_groups[m_currentgroup];
      }
    }
    /// <summary>
    /// Gets the index of currently active tv channel group.
    /// </summary>
    public int CurrentGroupIndex
    {
      get
      {
        return m_currentgroup;
      }
    }
    /// <summary>
    /// Gets the list of tv channel groups.
    /// </summary>
    public List<ChannelGroup> Groups
    {
      get { return m_groups; }
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
          return m_currentChannel;
        }
        return m_zapchannel;
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
        return ((ChannelGroup)m_groups[m_zapgroup]).GroupName;
      }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Sets last failed channel
    /// </summary>
    /// <param name="failedChannel"></param>
    public void SetFailingChannel(Channel failedChannel)
    {
      m_currentChannel = failedChannel;
    }

    public void ZapNow()
    {
      m_zaptime = DateTime.Now.AddSeconds(-1);
      // MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapnow group:{0} current group:{0}", m_zapgroup, m_currentgroup);
      //if (m_zapchannel == null)
      //   MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapchannel==null");
      //else
      //   MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapchannel=={0}",m_zapchannel);
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
      // BAV, 02.03.08: a channel change should not be delayed by rendering.
      //                by scipping this => 1 min delays in zapping should be avoided 
      //if (GUIGraphicsContext.InVmr9Render) return false;
      reentrant = true;
      UpdateCurrentChannel();

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
            if (CurrentGroup != null && CurrentGroup.ReferringGroupMap().Count > 0)
            {
              GroupMap gm = (GroupMap)CurrentGroup.ReferringGroupMap()[0];
              Channel chan = (Channel)gm.ReferencedChannel();
              m_zapchannel = chan;
            }
          }
          m_zapgroup = -1;

          //if (m_zapchannel != m_currentchannel)
          //  lastViewedChannel = m_currentchannel;
          // Zap to desired channel
          if (m_zapchannel != null) // might be NULL after tuning failed
          {
            Channel zappingTo = m_zapchannel;

            //remember to apply the new group also.
            if (m_zapchannel.CurrentGroup != null)
            {
              m_currentgroup = GetGroupIndex(m_zapchannel.CurrentGroup.GroupName);
              Log.Info("Channel change:{0} on group {1}", zappingTo.DisplayName, m_zapchannel.CurrentGroup.GroupName);
            }
            else
            {
              Log.Info("Channel change:{0}", zappingTo.DisplayName);
            }
            m_zapchannel = null;

            TVHome.ViewChannel(zappingTo);
          }
          reentrant = false;

          return true;
        }
      }

      reentrant = false;
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
            newChannel = Channel.Retrieve(id);
          }
        }
        else
        {
          // else if any card is recording
          // then get & use that channel
          TvServer server = new TvServer();
          if (server.IsAnyCardRecording())
          {
            for (int i = 0; i < server.Count; ++i)
            {
              User user = new User();
              VirtualCard card = server.CardByIndex(user, i);
              if (card.IsRecording)
              {
                id = card.IdChannel;
                if (id >= 0)
                {
                  newChannel = Channel.Retrieve(id);
                  break;
                }
              }
            }
          }
        }
        if (newChannel == null)
        {
          newChannel = m_currentChannel;
        }

        int currentChannelId = 0;
        int newChannelId = 0;

        if (m_currentChannel != null)
        {
          currentChannelId = m_currentChannel.IdChannel;
        }

        if (newChannel != null)
        {
          newChannelId = newChannel.IdChannel;
        }

        if (currentChannelId != newChannelId)
        {
          m_currentChannel = newChannel;
          m_currentChannel.CurrentGroup = CurrentGroup;
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
      Log.Debug("ChannelNavigator.ZapToChannel {0} - zapdelay {1}", channel.DisplayName, useZapDelay);
      TVHome.UserChannelChanged = true;
      m_zapchannel = channel;

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    private void GetChannels(bool refresh)
    {
      if (refresh)
      {
        _channelList = new List<Channel>();
      }
      if (_channelList == null)
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
    /// Changes the current channel (based on channel number) after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannelNumber(int channelNr, bool useZapDelay)
    {
      IList<GroupMap> channels = CurrentGroup.ReferringGroupMap();
      if (channelNr >= 0)
      {
        Log.Debug("channels.Count {0}", channels.Count);

        bool found = false;
        int iCounter = 0;
        Channel chan;
        GetChannels(true);
        while (iCounter < channels.Count && found == false)
        {
          chan = (Channel)_channelList[iCounter];

          Log.Debug("chan {0}", chan.DisplayName);

          foreach (TuningDetail detail in chan.ReferringTuningDetail())
          {
            Log.Debug("detail nr {0} id{1}", detail.ChannelNumber, detail.IdChannel);

            if (detail.ChannelNumber == channelNr)
            {
              Log.Debug("find channel: iCounter {0}, detail.ChannelNumber {1}, detail.name {2}, channels.Count {3}",
                        iCounter, detail.ChannelNumber, detail.Name, channels.Count);
              found = true;
              ZapToChannel(iCounter + 1, useZapDelay);
            }
          }
          iCounter++;
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
      IList<GroupMap> channels = CurrentGroup.ReferringGroupMap();
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        GroupMap gm = (GroupMap)channels[channelNr];
        Channel chan = gm.ReferencedChannel();
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
        currindex = GetChannelIndex(Channel);
        currentChan = Channel;
      }
      else
      {
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel 
        currentChan = Channel;
      }
      GroupMap gm;
      Channel chan;
      //check if channel is visible 
      //if not find next visible 
      do
      {
        // Step to next channel 
        currindex++;
        if (currindex >= CurrentGroup.ReferringGroupMap().Count)
        {
          currindex = 0;
        }
        gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
        chan = (Channel)gm.ReferencedChannel();
      } while (!chan.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = chan;
      Log.Info("Navigator:ZapNext {0}->{1}", currentChan.DisplayName, m_zapchannel.DisplayName);
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
        currentChan = Channel;
        currindex = GetChannelIndex(Channel);
      }
      else
      {
        currentChan = m_zapchannel;
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel 
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
          currindex = CurrentGroup.ReferringGroupMap().Count - 1;
        }
        gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
        chan = (Channel)gm.ReferencedChannel();
      } while (!chan.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = chan;
      Log.Info("Navigator:ZapPrevious {0}->{1}",
               currentChan.DisplayName, m_zapchannel.DisplayName);
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

      if (m_zapgroup >= m_groups.Count)
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
        m_zapgroup = m_groups.Count - 1;
      }

      if (useZapDelay)
      {
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    /// <summary>
    /// Zaps to the last viewed Channel (without ZapDelay).  // mPod
    /// </summary>
    public void ZapToLastViewedChannel()
    {
      if (_lastViewedChannel != null)
      {
        TVHome.UserChannelChanged = true;
        m_zapchannel = _lastViewedChannel;
        m_zaptime = DateTime.Now;
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
      IList<GroupMap> groupMaps = CurrentGroup.ReferringGroupMap();
      for (int i = 0; i < groupMaps.Count; i++)
      {
        GroupMap gm = (GroupMap)groupMaps[i];
        Channel chan = (Channel)gm.ReferencedChannel();
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
      for (int i = 0; i < m_groups.Count; i++)
      {
        ChannelGroup group = (ChannelGroup)m_groups[i];
        if (group.GroupName == groupname)
        {
          return i;
        }
      }
      return -1;
    }

    public Channel GetChannel(int channelId)
    {
      foreach (Channel chan in channels)
      {
        if (chan.IdChannel == channelId && chan.VisibleInGuide)
        {
          return chan;
        }
      }
      return null;
    }

    public Channel GetChannel(string channelName)
    {
      foreach (Channel chan in channels)
      {
        if (chan.DisplayName == channelName && chan.VisibleInGuide)
        {
          return chan;
        }
      }
      return null;
    }

    #endregion

    #region Serialization

    public void LoadSettings(Settings xmlreader)
    {
      Log.Info("ChannelNavigator::LoadSettings()");
      string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
      m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
      string groupname = xmlreader.GetValueAsString("mytv", "group", TvConstants.TvGroupNames.AllChannels);
      m_currentgroup = GetGroupIndex(groupname);
      if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count) // Group no longer exists?
      {
        m_currentgroup = 0;
      }

      m_currentChannel = GetChannel(currentchannelName);

      if (m_currentChannel == null)
      {
        if (m_currentgroup < m_groups.Count)
        {
          ChannelGroup group = (ChannelGroup)m_groups[m_currentgroup];
          if (group.ReferringGroupMap().Count > 0)
          {
            GroupMap gm = (GroupMap)group.ReferringGroupMap()[0];
            m_currentChannel = gm.ReferencedChannel();
          }
        }
      }

      //check if the channel does indeed belong to the group read from the XML setup file ?


      bool foundMatchingGroupName = false;

      if (m_currentChannel != null)
      {
        foreach (GroupMap groupMap in m_currentChannel.ReferringGroupMap())
        {
          if (groupMap.ReferencedChannelGroup().GroupName == groupname)
          {
            foundMatchingGroupName = true;
            break;
          }
        }
      }

      //if we still havent found the right group, then iterate through the selected group and find the channelname.      
      if (!foundMatchingGroupName && m_currentChannel != null && m_groups != null)
      {
        foreach (GroupMap groupMap in ((ChannelGroup)m_groups[m_currentgroup]).ReferringGroupMap())
        {
          if (groupMap.ReferencedChannel().DisplayName == currentchannelName)
          {
            foundMatchingGroupName = true;
            m_currentChannel = GetChannel(groupMap.ReferencedChannel().IdChannel);
            break;
          }
        }
      }


      // if the groupname does not match any of the groups assigned to the channel, then find the last group avail. (avoiding the all "channels group") for that channel and set is as the new currentgroup
      if (!foundMatchingGroupName && m_currentChannel != null && m_currentChannel.ReferringGroupMap().Count > 0)
      {
        GroupMap groupMap =
          (GroupMap)m_currentChannel.ReferringGroupMap()[m_currentChannel.ReferringGroupMap().Count - 1];
        m_currentgroup = GetGroupIndex(groupMap.ReferencedChannelGroup().GroupName);
        if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count) // Group no longer exists?
        {
          m_currentgroup = 0;
        }
      }

      if (m_currentChannel != null)
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
              groupName = ((ChannelGroup)m_groups[m_currentgroup]).GroupName;
            }
            else if (m_currentChannel != null)
            {
              groupName = m_currentChannel.CurrentGroup.GroupName;
            }

            if (groupName.Length > 0)
            {
              xmlwriter.SetValue("mytv", "group", groupName);
            }
          }
        }
        catch (Exception)
        {
        }
      }

      if (m_currentChannel != null)
      {
        try
        {
          if (m_currentChannel.IsTv)
          {
            bool foundMatchingGroupName = false;

            foreach (GroupMap groupMap in m_currentChannel.ReferringGroupMap())
            {
              if (groupMap.ReferencedChannelGroup().GroupName == groupName)
              {
                foundMatchingGroupName = true;
                break;
              }
            }
            if (foundMatchingGroupName)
            {
              xmlwriter.SetValue("mytv", "channel", m_currentChannel.DisplayName);
            }
            else
            //the channel did not belong to the group, then pick the first channel avail in the group and set this as the last channel.
            {
              if (m_currentgroup > -1)
              {
                ChannelGroup cg = (ChannelGroup)m_groups[m_currentgroup];
                if (cg.ReferringGroupMap().Count > 0)
                {
                  GroupMap gm = (GroupMap)cg.ReferringGroupMap()[0];
                  xmlwriter.SetValue("mytv", "channel", gm.ReferencedChannel().DisplayName);
                }
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    #endregion
  }
}