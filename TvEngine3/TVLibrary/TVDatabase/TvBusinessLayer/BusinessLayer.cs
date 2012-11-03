#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Gentle.Framework;
using MySql.Data.MySqlClient;
using TvDatabase;
using TvLibrary;
using TvLibrary.Channels;
using TvLibrary.Implementations;
using TvLibrary.Epg;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using StatementType = Gentle.Framework.StatementType;
using ThreadState = System.Threading.ThreadState;

#endregion

namespace TvDatabase
{

  #region enums

  public enum DeleteBeforeImportOption
  {
    None,
    OverlappingPrograms,
    ProgramsOnSameChannel,
    //All
  }

  public enum ChannelType
  {
    Tv,
    Radio,
    Web,
    All
  }

  #endregion

  public class TvBusinessLayer
  {
    #region delegates

    private delegate void DeleteProgramsDelegate();

    private delegate void InsertProgramsDelegate(ImportParams aImportParam);

    #endregion

    #region vars

    private static Thread _insertProgramsThread = null;

    private static Queue<ImportParams> _programInsertsQueue = new Queue<ImportParams>();
    private static AutoResetEvent _pendingProgramInserts = new AutoResetEvent(false);

    //private Thread _resetProgramStatesThread = null;

    //private readonly object SingleInsert = new object();
    //private readonly object SingleProgramStateUpdate = new object();

    // maximum hours to keep old program info
    private int _EpgKeepDuration = 0;
    //private DateTime _lastProgramUpdate = DateTime.MinValue; //when was the last time we changed anything in the program table ?

    public int EpgKeepDuration
    {
      get
      {
        if (_EpgKeepDuration == 0)
        {
          // first time query settings, caching
          Setting duration = GetSetting("epgKeepDuration", "24");
          duration.Persist();
          _EpgKeepDuration = Convert.ToInt32(duration.Value);
        }
        return _EpgKeepDuration;
      }
    }

    #endregion

    #region cards

    public Card AddCard(string name, string devicePath, Server server)
    {
      Card card = GetCardByDevicePath(devicePath);
      if (card != null)
      {
        card.Name = name;
        card.IdServer = server.IdServer;
        return card;
      }
      //
      // Card(devicePath, name, priority, grabEPG, lastEpgGrab, recordingFolder, idServer, enabled, camType, timeshiftingFolder, recordingFormat, decryptLimit)
      //
      Card newCard = new Card(devicePath, name, 1, true, new DateTime(2000, 1, 1), "", server.IdServer, true, 0, "", 0,
                              0, false, true, false, (int)TvDatabase.DbNetworkProvider.Generic);
      newCard.Persist();
      return newCard;
    }

    public IList<Card> Cards
    {
      get { return Card.ListAll(); }
    }

    public void DeleteCard(Card card)
    {
      card.Remove();
    }

    public Card GetCardByName(string name)
    {
      IList<Card> cards = Cards;
      foreach (Card card in cards)
      {
        if (card.Name == name)
        {
          return card;
        }
      }
      return null;
    }

    public Card GetCardByDevicePath(string path)
    {
      IList<Card> cards = Cards;
      foreach (Card card in cards)
      {
        if (card.DevicePath == path)
        {
          return card;
        }
      }
      return null;
    }

    public IList<Card> ListAllEnabledCardsOrderedByPriority()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Card));
      sb.AddConstraint("enabled=1");
      sb.AddOrderByField(false, "priority");
      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Card>(stmt.Execute());
    }

    #endregion

    #region channels

    // This is really needed
    public Channel AddNewChannel(string name, int channelNumber)
    {
      Channel newChannel = new Channel(false, false, 0, new DateTime(2000, 1, 1), false, new DateTime(2000, 1, 1),
                                       -1, true, "", name, channelNumber);
      return newChannel;
    }

    [System.Obsolete("use AddNewChannel(name, channelNumber)")]
    public Channel AddNewChannel(string name)
    {
      return AddNewChannel(name, 10000);
    }

    public ChannelGroup CreateGroup(string groupName)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (ChannelGroup));
      sb.AddConstraint(Operator.Like, "groupName", "%" + groupName + "%");
      SqlStatement stmt = sb.GetStatement(true);
      IList<ChannelGroup> groups = ObjectFactory.GetCollection<ChannelGroup>(stmt.Execute());
      ChannelGroup group;
      int GroupSelected = 0;
      if (groups.Count == 0)
      {
        group = new ChannelGroup(groupName, 9999);
        group.Persist();
      }
      else
      {
        for (int i = 0; i < groups.Count; ++i)
        {
          if (groups[i].GroupName == groupName)
          {
            GroupSelected = i;
          }
        }
        group = groups[GroupSelected];
      }
      return group;
    }

    public void AddChannelToGroup(Channel channel, string groupName)
    {
      bool found = false;
      ChannelGroup group = CreateGroup(groupName);
      IList<GroupMap> groupMaps = group.ReferringGroupMap();
      foreach (GroupMap map in groupMaps)
      {
        if (map.IdChannel == channel.IdChannel)
        {
          found = true;
          break;
        }
      }
      if (!found)
      {
        GroupMap map = new GroupMap(group.IdGroup, channel.IdChannel, channel.SortOrder);
        map.Persist();
      }
    }

    public void AddChannelToGroup(Channel channel, ChannelGroup group)
    {
      bool found = false;
      IList<GroupMap> groupMaps = group.ReferringGroupMap();
      foreach (GroupMap map in groupMaps)
      {
        if (map.IdChannel == channel.IdChannel)
        {
          found = true;
          break;
        }
      }
      if (!found)
      {
        GroupMap map = new GroupMap(group.IdGroup, channel.IdChannel, channel.SortOrder);
        map.Persist();
      }
    }

    /// <summary>
    /// Add a radio channel to radio group by name
    /// </summary>
    /// <param name="channel">channel to add</param>
    /// <param name="groupName">target group name</param>
    public void AddChannelToRadioGroup(Channel channel, string groupName)
    {
      RadioChannelGroup currentRadioGroup = null;
      IList<RadioChannelGroup> allRadioGroups = RadioChannelGroup.ListAll();

      // check for existing group
      foreach (RadioChannelGroup radioGroup in allRadioGroups)
      {
        if (radioGroup.GroupName == groupName)
        {
          currentRadioGroup = radioGroup;
          break;
        }
      }
      // no group found yet? then create new one
      if (currentRadioGroup == null)
      {
        currentRadioGroup = new RadioChannelGroup(groupName, allRadioGroups.Count);
        currentRadioGroup.Persist();
      }
      // add channel to group
      AddChannelToRadioGroup(channel, currentRadioGroup);
    }

    public void AddChannelToRadioGroup(Channel channel, RadioChannelGroup group)
    {
      bool found = false;
      IList<RadioGroupMap> groupMaps = group.ReferringRadioGroupMap();
      foreach (RadioGroupMap map in groupMaps)
      {
        if (map.IdChannel == channel.IdChannel)
        {
          found = true;
          break;
        }
      }
      if (!found)
      {
        RadioGroupMap map = new RadioGroupMap(group.IdGroup, channel.IdChannel, channel.SortOrder);
        map.Persist();
      }
    }

    public IList<Channel> Channels
    {
      get { return Channel.ListAll(); }
    }

    public void DeleteChannel(Channel channel)
    {
      channel.Remove();
    }

    public Channel GetChannel(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      SqlStatement stmt = sb.GetStatement(true);
      IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());
      if (channels == null)
      {
        return null;
      }
      if (channels.Count == 0)
      {
        return null;
      }
      return channels[0];
    }

    public Channel GetChannelByTuningDetail(int networkId, int transportId, int serviceId)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (TuningDetail));
      sb.AddConstraint(Operator.Equals, "networkId", networkId);
      sb.AddConstraint(Operator.Equals, "transportId", transportId);
      sb.AddConstraint(Operator.Equals, "serviceId", serviceId);

      SqlStatement stmt = sb.GetStatement(true);
      IList<TuningDetail> details = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());

      if (details == null)
      {
        return null;
      }
      if (details.Count == 0)
      {
        return null;
      }
      TuningDetail detail = details[0];
      return detail.ReferencedChannel();
    }

    public TuningDetail GetTuningDetail(int networkId, int serviceId, int channelType)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (TuningDetail));
      sb.AddConstraint(Operator.Equals, "networkId", networkId);
      sb.AddConstraint(Operator.Equals, "serviceId", serviceId);
      sb.AddConstraint(Operator.Equals, "channelType", channelType);

      SqlStatement stmt = sb.GetStatement(true);
      IList<TuningDetail> details = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());

      if (details == null)
      {
        return null;
      }
      if (details.Count == 0)
      {
        return null;
      }
      return details[0];
    }

    public TuningDetail GetTuningDetail(String url, int channelType)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "url", url);
      sb.AddConstraint(Operator.Equals, "channelType", channelType);

      SqlStatement stmt = sb.GetStatement(true);
      IList<TuningDetail> details = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());

      if (details == null)
      {
        return null;
      }
      if (details.Count == 0)
      {
        return null;
      }
      return details[0];
    }

    public TuningDetail GetTuningDetail(int networkId, int transportId, int serviceId, int channelType)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (TuningDetail));
      sb.AddConstraint(Operator.Equals, "networkId", networkId);
      sb.AddConstraint(Operator.Equals, "serviceId", serviceId);
      sb.AddConstraint(Operator.Equals, "transportId", transportId);
      sb.AddConstraint(Operator.Equals, "channelType", channelType);

      SqlStatement stmt = sb.GetStatement(true);
      IList<TuningDetail> details = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());

      if (details == null)
      {
        return null;
      }
      if (details.Count == 0)
      {
        return null;
      }
      return details[0];
    }

    public IList<TuningDetail> GetTuningDetailsByName(string name, int channelType)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", name);
      sb.AddConstraint(Operator.Equals, "channelType", channelType);

      SqlStatement stmt = sb.GetStatement(true);
      IList<TuningDetail> details = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());
      return details;
    }

    public static int GetChannelType(DVBBaseChannel channel)
    {
      int channelType;

      if (channel is DVBTChannel)
      {
        channelType = 4;
      }
      else if (channel is DVBSChannel)
      {
        channelType = 3;
      }
      else if (channel is DVBCChannel)
      {
        channelType = 2;
      }
      else if (channel is DVBIPChannel)
      {
        channelType = 7;
      }
      else // must be ATSCChannel
      {
        channelType = 1;
      }
      return channelType;
    }

    public TuningDetail GetTuningDetail(DVBBaseChannel channel)
    {
      int channelType = GetChannelType(channel);
      return GetTuningDetail(channel.NetworkId, channel.TransportId, channel.ServiceId, channelType);
    }

    public IList<Channel> GetChannelsByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
      sb.AddConstraint(Operator.Equals, "displayName", name);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());
      return channels;
    }

    public IList<Channel> GetChannelsInGroup(ChannelGroup group)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
      SqlStatement origStmt = sb.GetStatement(true);
      string sql = "select c.* from channel c inner join groupmap gm on (c.idChannel = gm.idChannel and gm.idGroup =" +
                   group.IdGroup + ") order by gm.SortOrder asc";
      SqlStatement statement = new SqlStatement(StatementType.Select, origStmt.Command, sql,
                                                typeof (Channel));
      return ObjectFactory.GetCollection<Channel>(statement.Execute());
    }

    /// <summary>
    /// Checks if a file doesnt exist and then returns the filename preceded by a CRLF combination.
    /// If the file does exist a empty string is returned.
    /// </summary>
    /// <returns>A String that contains the name of the not existing file </returns>
    private static String FileNotExistsString(String fileName)
    {
      if (!File.Exists(fileName))
      {
        return "\r\n" + fileName;
      }
      return "";
    }

    /// <summary>
    /// Checks several files that are needed for using the gentle framework for the database.
    /// If some files are not present in the working folder an System.IO.FileNotFoundException exception is thrown
    /// </summary>
    private static void checkGentleFiles()
    {
      String filesNotFound = "";
      //filesNotFound += FileNotExistsString("Gentle.config");
      filesNotFound += FileNotExistsString("Gentle.Framework.dll");
      filesNotFound += FileNotExistsString("Gentle.Provider.MySQL.dll");
      filesNotFound += FileNotExistsString("Gentle.Provider.SQLServer.dll");

      if (!filesNotFound.Equals(""))
      {
        throw new FileNotFoundException("Files not found:" + filesNotFound);
      }
    }

    /// <summary>
    /// gets a value from the database table "Setting"
    /// </summary>
    /// <returns>A Setting object with the stored value, if it doesnt exist the given default string will be the value</returns>
    public Setting GetSetting(string tagName, string defaultValue)
    {
      if (defaultValue == null)
      {
        return null;
      }
      if (tagName == null)
      {
        return null;
      }
      if (tagName == "")
      {
        return null;
      }
      SqlBuilder sb;
      try
      {
        sb = new SqlBuilder(StatementType.Select, typeof (Setting));
      }
      catch (TypeInitializationException)
      {
        checkGentleFiles(); // Try to throw a more meaningfull exception
        throw; // else re-throw the original error
      }

      sb.AddConstraint(Operator.Equals, "tag", tagName);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Setting> settingsFound = ObjectFactory.GetCollection<Setting>(stmt.Execute());
      if (settingsFound.Count == 0)
      {
        Setting set = new Setting(tagName, defaultValue);
        set.Persist();
        return set;
      }
      return settingsFound[0];
    }

    /// <summary>
    /// gets a value from the database table "Setting"
    /// </summary>
    /// <returns>A Setting object with the stored value, if it doesnt exist a empty string will be the value</returns>
    public Setting GetSetting(string tagName)
    {
      SqlBuilder sb;
      try
      {
        sb = new SqlBuilder(StatementType.Select, typeof (Setting));
      }
      catch (TypeInitializationException)
      {
        checkGentleFiles(); // Try to throw a more meaningfull exception
        throw; // else re-throw the original error
      }

      sb.AddConstraint(Operator.Equals, "tag", tagName);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Setting> settingsFound = ObjectFactory.GetCollection<Setting>(stmt.Execute());
      if (settingsFound.Count == 0)
      {
        Setting set = new Setting(tagName, "");
        set.Persist();
        return set;
      }
      return settingsFound[0];
    }

    public IChannel GetTuningChannelByType(Channel channel, int channelType)
    {
      IList<TuningDetail> tuningDetails = channel.ReferringTuningDetail();
      for (int i = 0; i < tuningDetails.Count; ++i)
      {
        TuningDetail detail = tuningDetails[i];
        if (detail.ChannelType != channelType)
        {
          continue;
        }
        return GetTuningChannel(detail);
      }
      return null;
    }

    public List<IChannel> GetTuningChannelsByDbChannel(Channel channel)
    {
      List<IChannel> tvChannels = new List<IChannel>();
      IList<TuningDetail> tuningDetails = channel.ReferringTuningDetail();
      for (int i = 0; i < tuningDetails.Count; ++i)
      {
        TuningDetail detail = tuningDetails[i];
        tvChannels.Add(GetTuningChannel(detail));
      }
      return tvChannels;
    }

    public IChannel GetTuningChannel(TuningDetail detail)
    {
      switch (detail.ChannelType)
      {
        case 0: //AnalogChannel
          AnalogChannel analogChannel = new AnalogChannel();
          analogChannel.ChannelNumber = detail.ChannelNumber;
          CountryCollection collection = new CountryCollection();
          analogChannel.Country = collection.Countries[detail.CountryId];
          analogChannel.Frequency = detail.Frequency;
          analogChannel.IsRadio = detail.IsRadio;
          analogChannel.IsTv = detail.IsTv;
          analogChannel.Name = detail.Name;
          analogChannel.TunerSource = (TunerInputType)detail.TuningSource;
          analogChannel.VideoSource = (AnalogChannel.VideoInputType)detail.VideoSource;
          analogChannel.AudioSource = (AnalogChannel.AudioInputType)detail.AudioSource;
          return analogChannel;
        case 1: //ATSCChannel
          ATSCChannel atscChannel = new ATSCChannel();
          atscChannel.MajorChannel = detail.MajorChannel;
          atscChannel.MinorChannel = detail.MinorChannel;
          atscChannel.PhysicalChannel = detail.ChannelNumber;
          atscChannel.FreeToAir = detail.FreeToAir;
          atscChannel.Frequency = detail.Frequency;
          atscChannel.IsRadio = detail.IsRadio;
          atscChannel.IsTv = detail.IsTv;
          atscChannel.Name = detail.Name;
          atscChannel.NetworkId = detail.NetworkId;
          atscChannel.PmtPid = detail.PmtPid;
          atscChannel.Provider = detail.Provider;
          atscChannel.ServiceId = detail.ServiceId;
          //atscChannel.SymbolRate = detail.Symbolrate;
          atscChannel.TransportId = detail.TransportId;
          atscChannel.ModulationType = (ModulationType)detail.Modulation;
          return atscChannel;
        case 2: //DVBCChannel
          DVBCChannel dvbcChannel = new DVBCChannel();
          dvbcChannel.ModulationType = (ModulationType)detail.Modulation;
          dvbcChannel.FreeToAir = detail.FreeToAir;
          dvbcChannel.Frequency = detail.Frequency;
          dvbcChannel.IsRadio = detail.IsRadio;
          dvbcChannel.IsTv = detail.IsTv;
          dvbcChannel.Name = detail.Name;
          dvbcChannel.NetworkId = detail.NetworkId;
          dvbcChannel.PmtPid = detail.PmtPid;
          dvbcChannel.Provider = detail.Provider;
          dvbcChannel.ServiceId = detail.ServiceId;
          dvbcChannel.SymbolRate = detail.Symbolrate;
          dvbcChannel.TransportId = detail.TransportId;
          dvbcChannel.LogicalChannelNumber = detail.ChannelNumber;
          return dvbcChannel;
        case 3: //DVBSChannel
          DVBSChannel dvbsChannel = new DVBSChannel();
          dvbsChannel.DisEqc = (DisEqcType)detail.Diseqc;
          dvbsChannel.Polarisation = (Polarisation)detail.Polarisation;
          dvbsChannel.SwitchingFrequency = detail.SwitchingFrequency;
          dvbsChannel.FreeToAir = detail.FreeToAir;
          dvbsChannel.Frequency = detail.Frequency;
          dvbsChannel.IsRadio = detail.IsRadio;
          dvbsChannel.IsTv = detail.IsTv;
          dvbsChannel.Name = detail.Name;
          dvbsChannel.NetworkId = detail.NetworkId;
          dvbsChannel.PmtPid = detail.PmtPid;
          dvbsChannel.Provider = detail.Provider;
          dvbsChannel.ServiceId = detail.ServiceId;
          dvbsChannel.SymbolRate = detail.Symbolrate;
          dvbsChannel.TransportId = detail.TransportId;
          dvbsChannel.BandType = (BandType)detail.Band;
          dvbsChannel.SatelliteIndex = detail.SatIndex;
          dvbsChannel.ModulationType = (ModulationType)detail.Modulation;
          dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate)detail.InnerFecRate;
          dvbsChannel.Pilot = (Pilot)detail.Pilot;
          dvbsChannel.Rolloff = (RollOff)detail.RollOff;
          dvbsChannel.LogicalChannelNumber = detail.ChannelNumber;
          return dvbsChannel;
        case 4: //DVBTChannel
          DVBTChannel dvbtChannel = new DVBTChannel();
          dvbtChannel.BandWidth = detail.Bandwidth;
          dvbtChannel.FreeToAir = detail.FreeToAir;
          dvbtChannel.Frequency = detail.Frequency;
          dvbtChannel.IsRadio = detail.IsRadio;
          dvbtChannel.IsTv = detail.IsTv;
          dvbtChannel.Name = detail.Name;
          dvbtChannel.NetworkId = detail.NetworkId;
          dvbtChannel.PmtPid = detail.PmtPid;
          dvbtChannel.Provider = detail.Provider;
          dvbtChannel.ServiceId = detail.ServiceId;
          dvbtChannel.TransportId = detail.TransportId;
          dvbtChannel.LogicalChannelNumber = detail.ChannelNumber;
          return dvbtChannel;
        case 7: //DVBIPChannel
          DVBIPChannel dvbipChannel = new DVBIPChannel();
          dvbipChannel.FreeToAir = detail.FreeToAir;
          dvbipChannel.IsRadio = detail.IsRadio;
          dvbipChannel.IsTv = detail.IsTv;
          dvbipChannel.Name = detail.Name;
          dvbipChannel.NetworkId = detail.NetworkId;
          dvbipChannel.PmtPid = detail.PmtPid;
          dvbipChannel.Provider = detail.Provider;
          dvbipChannel.ServiceId = detail.ServiceId;
          dvbipChannel.TransportId = detail.TransportId;
          dvbipChannel.LogicalChannelNumber = detail.ChannelNumber;
          dvbipChannel.Url = detail.Url;
          return dvbipChannel;
      }
      return null;
    }

    public ChannelMap MapChannelToCard(Card card, Channel channel, bool epgOnly)
    {
      IList<ChannelMap> channelMaps = card.ReferringChannelMap();
      for (int i = 0; i < channelMaps.Count; ++i)
      {
        ChannelMap map = channelMaps[i];
        if (map.IdChannel == channel.IdChannel && map.IdCard == card.IdCard)
        {
          return map;
        }
      }
      ChannelMap newMap = new ChannelMap(channel.IdChannel, card.IdCard, epgOnly);
      newMap.Persist();
      return newMap;
    }

    /// <summary>
    /// Gets a list of radio channels sorted by their group
    /// </summary>
    /// <returns>a list of TVDatabase Channels</returns>
    public List<Channel> GetRadioGuideChannelsForGroup(int groupID)
    {
      SqlBuilder sb1 = new SqlBuilder(StatementType.Select, typeof (Channel));
      SqlStatement stmt1 = sb1.GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(stmt1.StatementType, stmt1.Command,
                                                    String.Format(
                                                      "select c.* from Channel c inner join RadioGroupMap g on (c.idChannel=g.idChannel and g.idGroup = '{0}') where visibleInGuide = 1 and isRadio = 1 order by g.sortOrder",
                                                      groupID), typeof (Channel));
      return ObjectFactory.GetCollection<Channel>(ManualJoinSQL.Execute()) as List<Channel>;
    }

    /// <summary>
    /// Gets a list of tv channels sorted by their group
    /// </summary>
    /// <returns>a list of TVDatabase Channels</returns>
    public List<Channel> GetTVGuideChannelsForGroup(int groupID)
    {
      SqlBuilder sb1 = new SqlBuilder(StatementType.Select, typeof (Channel));
      SqlStatement stmt1 = sb1.GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(stmt1.StatementType, stmt1.Command,
                                                    String.Format(
                                                      "select c.* from Channel c inner join GroupMap g on (c.idChannel=g.idChannel and g.idGroup = '{0}') where visibleInGuide = 1 and isTv = 1 order by g.sortOrder",
                                                      groupID), typeof (Channel));
      return ObjectFactory.GetCollection<Channel>(ManualJoinSQL.Execute()) as List<Channel>;
    }

    public IList<Channel> GetAllRadioChannels()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
      sb.AddConstraint(Operator.Equals, "isRadio", 1);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Channel> radioChannels = ObjectFactory.GetCollection<Channel>(stmt.Execute());
      if (radioChannels == null)
      {
        return null;
      }
      if (radioChannels.Count == 0)
      {
        return null;
      }
      return radioChannels;
    }

    #endregion

    #region tuningdetails

    public TuningDetail AddTuningDetails(Channel channel, IChannel tvChannel)
    {
      string channelName = "";
      long channelFrequency = 0;
      int channelNumber = 0;
      int country = 31;
      bool isRadio = false;
      bool isTv = false;
      int tunerSource = 0;
      int videoInputType = 0;
      int audioInputType = 0;
      bool isVCRSignal = false;
      int symbolRate = 0;
      int modulation = 0;
      int polarisation = 0;
      int switchFrequency = 0;
      int diseqc = 0;
      int bandwidth = 8;
      bool freeToAir = true;
      int pmtPid = -1;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int band = 0;
      int satIndex = -1;
      int innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      int pilot = (int)Pilot.NotSet;
      int rollOff = (int)RollOff.NotSet;
      string url = "";

      AnalogChannel analogChannel = tvChannel as AnalogChannel;
      if (analogChannel != null)
      {
        channelName = analogChannel.Name;
        channelFrequency = analogChannel.Frequency;
        channelNumber = analogChannel.ChannelNumber;
        country = analogChannel.Country.Index;
        isRadio = analogChannel.IsRadio;
        isTv = analogChannel.IsTv;
        tunerSource = (int)analogChannel.TunerSource;
        videoInputType = (int)analogChannel.VideoSource;
        audioInputType = (int)analogChannel.AudioSource;
        isVCRSignal = analogChannel.IsVCRSignal;
        channelType = 0;
      }

      ATSCChannel atscChannel = tvChannel as ATSCChannel;
      if (atscChannel != null)
      {
        majorChannel = atscChannel.MajorChannel;
        minorChannel = atscChannel.MinorChannel;
        channelNumber = atscChannel.PhysicalChannel;
        //videoPid = atscChannel.VideoPid;
        //audioPid = atscChannel.AudioPid;
        modulation = (int)atscChannel.ModulationType;
        channelType = 1;
      }

      DVBCChannel dvbcChannel = tvChannel as DVBCChannel;
      if (dvbcChannel != null)
      {
        symbolRate = dvbcChannel.SymbolRate;
        modulation = (int)dvbcChannel.ModulationType;
        channelNumber = dvbcChannel.LogicalChannelNumber;
        channelType = 2;
      }

      DVBSChannel dvbsChannel = tvChannel as DVBSChannel;
      if (dvbsChannel != null)
      {
        symbolRate = dvbsChannel.SymbolRate;
        polarisation = (int)dvbsChannel.Polarisation;
        switchFrequency = dvbsChannel.SwitchingFrequency;
        diseqc = (int)dvbsChannel.DisEqc;
        band = (int)dvbsChannel.BandType;
        satIndex = dvbsChannel.SatelliteIndex;
        modulation = (int)dvbsChannel.ModulationType;
        innerFecRate = (int)dvbsChannel.InnerFecRate;
        pilot = (int)dvbsChannel.Pilot;
        rollOff = (int)dvbsChannel.Rolloff;
        channelNumber = dvbsChannel.LogicalChannelNumber;
        channelType = 3;
      }

      DVBTChannel dvbtChannel = tvChannel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.BandWidth;
        channelNumber = dvbtChannel.LogicalChannelNumber;
        channelType = 4;
      }

      DVBIPChannel dvbipChannel = tvChannel as DVBIPChannel;
      if (dvbipChannel != null)
      {
        url = dvbipChannel.Url;
        channelNumber = dvbipChannel.LogicalChannelNumber;
        channelType = 7;
      }

      DVBBaseChannel dvbChannel = tvChannel as DVBBaseChannel;
      if (dvbChannel != null)
      {
        pmtPid = dvbChannel.PmtPid;
        networkId = dvbChannel.NetworkId;
        serviceId = dvbChannel.ServiceId;
        transportId = dvbChannel.TransportId;
        channelName = dvbChannel.Name;
        provider = dvbChannel.Provider;
        channelFrequency = dvbChannel.Frequency;
        isRadio = dvbChannel.IsRadio;
        isTv = dvbChannel.IsTv;
        freeToAir = dvbChannel.FreeToAir;
      }

      TuningDetail detail = new TuningDetail(channel.IdChannel, channelName, provider,
                                             channelType, channelNumber, (int)channelFrequency, country, isRadio, isTv,
                                             networkId, transportId, serviceId, pmtPid, freeToAir,
                                             modulation, polarisation, symbolRate, diseqc, switchFrequency,
                                             bandwidth, majorChannel, minorChannel, videoInputType,
                                             audioInputType, isVCRSignal, tunerSource, band,
                                             satIndex,
                                             innerFecRate, pilot, rollOff, url, 0);
      detail.Persist();
      return detail;
    }

    public TuningDetail UpdateTuningDetails(Channel channel, IChannel tvChannel, TuningDetail detail)
    {
      string channelName = "";
      long channelFrequency = 0;
      int channelNumber = 0;
      int country = 31;
      bool isRadio = false;
      bool isTv = false;
      int tunerSource = 0;
      int videoInputType = 0;
      int audioInputType = 0;
      bool isVCRSignal = false;
      int symbolRate = 0;
      int modulation = 0;
      int polarisation = 0;
      int switchFrequency = 0;
      int diseqc = 0;
      int bandwidth = 8;
      bool freeToAir = true;
      int pmtPid = -1;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int band = 0;
      int satIndex = -1;
      int innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      int pilot = (int)Pilot.NotSet;
      int rollOff = (int)RollOff.NotSet;
      string url = "";

      AnalogChannel analogChannel = tvChannel as AnalogChannel;
      if (analogChannel != null)
      {
        channelName = analogChannel.Name;
        channelFrequency = analogChannel.Frequency;
        channelNumber = analogChannel.ChannelNumber;
        country = analogChannel.Country.Index;
        isRadio = analogChannel.IsRadio;
        isTv = analogChannel.IsTv;
        tunerSource = (int)analogChannel.TunerSource;
        videoInputType = (int)analogChannel.VideoSource;
        audioInputType = (int)analogChannel.AudioSource;
        isVCRSignal = analogChannel.IsVCRSignal;
        channelType = 0;
      }
      ATSCChannel atscChannel = tvChannel as ATSCChannel;
      if (atscChannel != null)
      {
        majorChannel = atscChannel.MajorChannel;
        minorChannel = atscChannel.MinorChannel;
        channelNumber = atscChannel.PhysicalChannel;
        //videoPid = atscChannel.VideoPid;
        //audioPid = atscChannel.AudioPid;
        modulation = (int)atscChannel.ModulationType;
        channelType = 1;
      }

      DVBCChannel dvbcChannel = tvChannel as DVBCChannel;
      if (dvbcChannel != null)
      {
        symbolRate = dvbcChannel.SymbolRate;
        modulation = (int)dvbcChannel.ModulationType;
        channelNumber = dvbcChannel.LogicalChannelNumber;
        channelType = 2;
      }

      DVBSChannel dvbsChannel = tvChannel as DVBSChannel;
      if (dvbsChannel != null)
      {
        symbolRate = dvbsChannel.SymbolRate;
        polarisation = (int)dvbsChannel.Polarisation;
        switchFrequency = dvbsChannel.SwitchingFrequency;
        diseqc = (int)dvbsChannel.DisEqc;
        band = (int)dvbsChannel.BandType;
        satIndex = dvbsChannel.SatelliteIndex;
        modulation = (int)dvbsChannel.ModulationType;
        innerFecRate = (int)dvbsChannel.InnerFecRate;
        pilot = (int)dvbsChannel.Pilot;
        rollOff = (int)dvbsChannel.Rolloff;
        channelNumber = dvbsChannel.LogicalChannelNumber;
        channelType = 3;
      }

      DVBTChannel dvbtChannel = tvChannel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.BandWidth;
        channelNumber = dvbtChannel.LogicalChannelNumber;
        channelType = 4;
      }

      DVBIPChannel dvbipChannel = tvChannel as DVBIPChannel;
      if (dvbipChannel != null)
      {
        url = dvbipChannel.Url;
        channelNumber = dvbipChannel.LogicalChannelNumber;
        channelType = 7;
      }

      DVBBaseChannel dvbChannel = tvChannel as DVBBaseChannel;
      if (dvbChannel != null)
      {
        pmtPid = dvbChannel.PmtPid;
        networkId = dvbChannel.NetworkId;
        serviceId = dvbChannel.ServiceId;
        transportId = dvbChannel.TransportId;
        channelName = dvbChannel.Name;
        provider = dvbChannel.Provider;
        channelFrequency = dvbChannel.Frequency;
        isRadio = dvbChannel.IsRadio;
        isTv = dvbChannel.IsTv;
        freeToAir = dvbChannel.FreeToAir;
      }

      detail.Name = channelName;
      detail.Provider = provider;
      detail.ChannelType = channelType;
      detail.ChannelNumber = channelNumber;
      detail.Frequency = (int)channelFrequency;
      detail.CountryId = country;
      detail.IsRadio = isRadio;
      detail.IsTv = isTv;
      detail.NetworkId = networkId;
      detail.TransportId = transportId;
      detail.ServiceId = serviceId;
      detail.PmtPid = pmtPid;
      detail.FreeToAir = freeToAir;
      detail.Modulation = modulation;
      detail.Polarisation = polarisation;
      detail.Symbolrate = symbolRate;
      detail.Diseqc = diseqc;
      detail.SwitchingFrequency = switchFrequency;
      detail.Bandwidth = bandwidth;
      detail.MajorChannel = majorChannel;
      detail.MinorChannel = minorChannel;
      detail.VideoSource = videoInputType;
      detail.AudioSource = audioInputType;
      detail.IsVCRSignal = isVCRSignal;
      detail.TuningSource = tunerSource;
      detail.Band = band;
      detail.SatIndex = satIndex;
      detail.InnerFecRate = innerFecRate;
      detail.Pilot = pilot;
      detail.RollOff = rollOff;
      detail.Url = url;
      return detail;
    }

    public TuningDetail AddWebStreamTuningDetails(Channel channel, string url, int bitrate)
    {
      string channelName = channel.DisplayName;
      const long channelFrequency = 0;
      const int channelNumber = 0;
      const int country = 31;
      bool isRadio = channel.IsRadio;
      bool isTv = channel.IsTv;
      const int tunerSource = 0;
      const int videoInputType = 0;
      const int audioInputType = 0;
      const bool isVCRSignal = false;
      const int symbolRate = 0;
      const int modulation = 0;
      const int polarisation = 0;
      const int switchFrequency = 0;
      const int diseqc = 0;
      const int bandwidth = 8;
      const bool freeToAir = true;
      const int pmtPid = -1;
      const int networkId = -1;
      const int serviceId = -1;
      const int transportId = -1;
      const int minorChannel = -1;
      const int majorChannel = -1;
      const string provider = "";
      const int channelType = 5;
      const int band = 0;
      const int satIndex = -1;
      const int innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      const int pilot = (int)Pilot.NotSet;
      const int rollOff = (int)RollOff.NotSet;
      if (url == null)
      {
        url = "";
      }
      TuningDetail detail = new TuningDetail(channel.IdChannel, channelName, provider,
                                             channelType, channelNumber, (int)channelFrequency, country, isRadio, isTv,
                                             networkId, transportId, serviceId, pmtPid, freeToAir,
                                             modulation, polarisation, symbolRate, diseqc, switchFrequency,
                                             bandwidth, majorChannel, minorChannel, videoInputType,
                                             audioInputType, isVCRSignal, tunerSource, band,
                                             satIndex,
                                             innerFecRate, pilot, rollOff, url, bitrate);
      detail.Persist();
      return detail;
    }

    #endregion

    #region linkage map

    public IList<ChannelLinkageMap> GetLinkagesForChannel(Channel channel)
    {
      IList<ChannelLinkageMap> pmap = channel.ReferringLinkedChannels();
      if (pmap != null)
      {
        if (pmap.Count > 0)
          return pmap;
      }
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (ChannelLinkageMap));
      sb.AddConstraint(Operator.Equals, "idLinkedChannel", channel.IdChannel);
      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<ChannelLinkageMap>(stmt.Execute());
    }

    #endregion

    #region programs

    public string GetDateTimeString()
    {
      string provider = ProviderFactory.GetDefaultProvider().Name.ToLowerInvariant();
      if (provider == "mysql")
      {
        return "yyyy-MM-dd HH:mm:ss";
      }
      return "yyyyMMdd HH:mm:ss";
    }

    public string EscapeSQLString(string original)
    {
      string provider = ProviderFactory.GetDefaultProvider().Name.ToLowerInvariant();
      if (provider == "mysql")
      {
        return original.Replace("\\", "\\\\").Replace("'", "\\'");
      }
      else
      {
        return original.Replace("\\", "\\\\").Replace("'", "''");
      }
    }

    public void RemoveOldPrograms()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof (Program));
      DateTime dtYesterday = DateTime.Now.AddHours(-EpgKeepDuration);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(String.Format("endTime < '{0}'", dtYesterday.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public void RemoveOldPrograms(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof (Program));
      DateTime dtToKeep = DateTime.Now.AddHours(-EpgKeepDuration);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddConstraint(String.Format("endTime < '{0}'", dtToKeep.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public void RemoveAllPrograms(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof (Program));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public IList<Program> GetOnairNow()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(String.Format("startTime <= '{0}' and endTime >= '{1}'",
                                     DateTime.Now.ToString(GetDateTimeString(), mmddFormat),
                                     DateTime.Now.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public IList<Program> GetPrograms(Channel channel, DateTime startTime)
    {
      //The DateTime.MinValue is lower than the min datetime value of the database
      if (startTime == DateTime.MinValue)
      {
        startTime = startTime.AddYears(1900);
      }
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(String.Format("startTime>='{0}'", startTime.ToString(GetDateTimeString(), mmddFormat)));
      sb.AddOrderByField(true, "startTime");

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public IList<Program> GetPrograms(Channel channel, DateTime startTime, DateTime endTime)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(string.Format("({0} or {1} or {2}) ", sub1, sub2, sub3));
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public IList<Program> GetProgramExists(Channel channel, DateTime startTime, DateTime endTime)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));

      string sub1 =
        String.Format("( (StartTime >= '{0}' and StartTime < '{1}') or ( EndTime > '{0}' and EndTime <= '{1}' ) )",
                      startTime.ToString(GetDateTimeString(), mmddFormat),
                      endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime < '{0}' and EndTime > '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(string.Format("({0} or {1}) ", sub1, sub2));
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    #region TV-Guide

    public Dictionary<int, List<Program>> GetProgramsForAllChannels(DateTime startTime, DateTime endTime,
                                                                    List<Channel> channelList)
    {
      MySqlConnection MySQLConnect = null;
      MySqlDataAdapter MySQLAdapter = null;
      MySqlCommand MySQLCmd = null;

      SqlDataAdapter MsSqlAdapter = null;
      SqlConnection MsSqlConnect = null;
      SqlCommand MsSqlCmd = null;
      string provider = "";
      try
      {
        string connectString;
        try
        {
          provider = ProviderFactory.GetDefaultProvider().Name.ToLowerInvariant();
          connectString = ProviderFactory.GetDefaultProvider().ConnectionString;
        }
        catch (Exception cex)
        {
          Log.Info("BusinessLayer: GetProgramsForAllChannels could not retrieve connection details - {0}", cex.Message);
          return new Dictionary<int, List<Program>>();
        }
        switch (provider)
        {
          case "sqlserver":
            MsSqlConnect = new SqlConnection(connectString);
            MsSqlAdapter = new SqlDataAdapter();
            MsSqlAdapter.TableMappings.Add("Table", "Program");
            MsSqlConnect.Open();
            MsSqlCmd = new SqlCommand(BuildEpgSelect(channelList, provider), MsSqlConnect);
            MsSqlCmd.Parameters.Add("startTime", SqlDbType.DateTime).Value = startTime;
            MsSqlCmd.Parameters.Add("endTime", SqlDbType.DateTime).Value = endTime;
            MsSqlAdapter.SelectCommand = MsSqlCmd;
            break;
          case "mysql":
            MySQLConnect = new MySqlConnection(connectString);
            MySQLAdapter = new MySqlDataAdapter(MySQLCmd);
            MySQLAdapter.TableMappings.Add("Table", "Program");
            MySQLConnect.Open();
            MySQLCmd = new MySqlCommand(BuildEpgSelect(channelList, provider), MySQLConnect);
            MySQLCmd.Parameters.Add("?startTime", MySqlDbType.DateTime).Value = startTime;
            MySQLCmd.Parameters.Add("?endTime", MySqlDbType.DateTime).Value = endTime;
            MySQLAdapter.SelectCommand = MySQLCmd;
            break;
          default:
            return new Dictionary<int, List<Program>>();
        }

        using (DataSet dataSet = new DataSet("Program"))
        {
          switch (provider)
          {
            case "sqlserver":
              if (MsSqlAdapter != null)
              {
                MsSqlAdapter.Fill(dataSet);
              }
              break;
            case "mysql":
              if (MySQLAdapter != null)
              {
                MySQLAdapter.Fill(dataSet);
              }
              break;
          }
          return FillProgramMapFromDataSet(dataSet);
        }
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: GetProgramsForAllChannels caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
        return new Dictionary<int, List<Program>>();
      }
      finally
      {
        try
        {
          switch (provider)
          {
            case "mysql":
              if (MySQLConnect != null)
              {
                MySQLConnect.Close();
              }
              if (MySQLAdapter != null)
              {
                MySQLAdapter.Dispose();
              }
              if (MySQLCmd != null)
              {
                MySQLCmd.Dispose();
              }
              if (MySQLConnect != null)
              {
                MySQLConnect.Dispose();
              }
              break;
            case "sqlserver":
              if (MsSqlConnect != null)
              {
                MsSqlConnect.Close();
              }
              if (MsSqlAdapter != null)
              {
                MsSqlAdapter.Dispose();
              }
              if (MsSqlCmd != null)
              {
                MsSqlCmd.Dispose();
              }
              if (MsSqlConnect != null)
              {
                MsSqlConnect.Dispose();
              }
              break;
          }
        }
        catch (Exception ex)
        {
          Log.Info("BusinessLayer: GetProgramsForAllChannels Exception in finally - {0}, {1}", ex.Message, ex.StackTrace);
        }
      }
    }

    private static string BuildEpgSelect(IEnumerable<Channel> channelList, string aProvider)
    {
      StringBuilder sbSelect = new StringBuilder("SELECT * FROM Program WHERE ");

      if (aProvider == "mysql")
      {
        sbSelect.Append("((EndTime > ?startTime and EndTime < ?endTime)");
        sbSelect.Append(" OR ");
        sbSelect.Append("(StartTime >= ?startTime and StartTime <= ?endTime)");
        sbSelect.Append(" OR ");
        sbSelect.Append("(StartTime <= ?startTime and EndTime >= ?endTime))");
        sbSelect.Append(" AND ");
      }
      else
      {
        sbSelect.Append("((EndTime > @startTime and EndTime < @endTime)");
        sbSelect.Append(" OR ");
        sbSelect.Append("(StartTime >= @startTime and StartTime <= @endTime)");
        sbSelect.Append(" OR ");
        sbSelect.Append("(StartTime <= @startTime and EndTime >= @endTime))");
        sbSelect.Append(" AND ");
      }

      string channelConstraint = "";
      foreach (Channel ch in channelList)
      {
        if (string.IsNullOrEmpty(channelConstraint))
        {
          channelConstraint = string.Format("(idChannel={0}", ch.IdChannel);
        }
        else
        {
          channelConstraint += string.Format(" or idChannel={0}", ch.IdChannel);
        }
      }
      if (channelConstraint.Length > 0)
      {
        channelConstraint += ") ";
        sbSelect.Append(channelConstraint);
      }

      sbSelect.Append(" ORDER BY startTime ");
      return sbSelect.ToString();
    }

    private static Dictionary<int, List<Program>> FillProgramMapFromDataSet(DataSet dataSet)
    {
      Dictionary<int, List<Program>> maps = new Dictionary<int, List<Program>>();
      int resultCount = dataSet.Tables[0].Rows.Count;
      for (int i = 0; i < resultCount; i++)
      {
        DataRow prog = dataSet.Tables[0].Rows[i];

        Program p = new Program(
          Convert.ToInt32(prog["idProgram"]),
          Convert.ToInt32(prog["idChannel"]),
          Convert.ToDateTime(prog["startTime"]),
          Convert.ToDateTime(prog["endTime"]),
          Convert.ToString(prog["title"]),
          Convert.ToString(prog["description"]),
          Convert.ToString(prog["genre"]),
          (Program.ProgramState)Convert.ToInt32(prog["state"]),
          Convert.ToDateTime(prog["originalAirDate"]),
          Convert.ToString(prog["seriesNum"]),
          Convert.ToString(prog["episodeNum"]),
          Convert.ToString(prog["episodeName"]),
          Convert.ToString(prog["episodePart"]),
          Convert.ToInt32(prog["starRating"]),
          Convert.ToString(prog["classification"]),
          Convert.ToInt32(prog["parentalRating"])
          );

        int idChannel = p.IdChannel;
        if (!maps.ContainsKey(idChannel))
        {
          maps[idChannel] = new List<Program>();
        }

        maps[idChannel].Add(p);
      }
      return maps;
    }

    #endregion

    public IList<Program> GetPrograms(DateTime startTime, DateTime endTime)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      string StartTimeString = startTime.ToString(GetDateTimeString(), mmddFormat);
      string EndTimeString = endTime.ToString(GetDateTimeString(), mmddFormat);
      StringBuilder SqlSelectCommand = new StringBuilder();
      SqlSelectCommand.Append("select p.* from Program p inner join Channel c on c.idChannel = p.idChannel ");
      SqlSelectCommand.AppendFormat(
        "where ((EndTime > '{0}' and EndTime < '{1}') or (StartTime >= '{0}' and StartTime <= '{1}') or (StartTime <= '{0}' and EndTime >= '{1}')) and c.visibleInGuide = 1 order by startTime",
        StartTimeString, EndTimeString
        );
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof (Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(),
                                                    typeof (Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    public DateTime GetNewestProgramForChannel(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddOrderByField(false, "startTime");
      sb.SetRowLimit(1);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Program> progs = ObjectFactory.GetCollection<Program>(stmt.Execute());
      return progs.Count > 0 ? progs[0].StartTime : DateTime.MinValue;
    }

    public IList<Program> GetProgramsByTitle(Channel channel, DateTime startTime, DateTime endTime, string title)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Program));

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')",
                                  startTime.ToString(GetDateTimeString(), mmddFormat),
                                  endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(string.Format("({0} or {1} or {2}) ", sub1, sub2, sub3));
      sb.AddConstraint(Operator.Equals, "title", title);
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public IList<Program> GetProgramsByTitle(DateTime startTime, DateTime endTime, string title)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      string StartTimeString = startTime.ToString(GetDateTimeString(), mmddFormat);
      string EndTimeString = endTime.ToString(GetDateTimeString(), mmddFormat);
      StringBuilder SqlSelectCommand = new StringBuilder();
      SqlSelectCommand.Append("select p.* from Program p inner join Channel c on c.idChannel = p.idChannel ");
      SqlSelectCommand.AppendFormat(
        "where ((EndTime > '{0}' and EndTime < '{1}') or (StartTime >= '{0}' and StartTime <= '{1}') or (StartTime <= '{0}' and EndTime >= '{1}')) and title = '{2}' and c.visibleInGuide = 1 order by startTime",
        StartTimeString, EndTimeString, EscapeSQLString(title)
        );
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof (Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(),
                                                    typeof (Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    public IList<Program> SearchMinimalPrograms(DateTime startTime, DateTime endTime, string programName,
                                                Channel channel)
    {
      return channel != null
               ? GetProgramsByTitle(channel, startTime, endTime, programName)
               : GetProgramsByTitle(startTime, endTime, programName);
    }

    // Maintained for backward compatibility.
    public List<string> GetGenres()
    {
      return GetProgramGenres();
    }

    public List<string> GetProgramGenres()
    {
      List<string> genres = new List<string>();
      string connectString = ProviderFactory.GetDefaultProvider().ConnectionString;

      string provider = ProviderFactory.GetDefaultProvider().Name.ToLowerInvariant();
      if (provider == "mysql")
      {
        using (MySqlConnection connect = new MySqlConnection(connectString))
        {
          connect.Open();
          using (MySqlCommand cmd = connect.CreateCommand())
          {
            cmd.CommandText = "select distinct(genre) from Program order by genre";
            cmd.CommandType = CommandType.Text;
            using (IDataReader reader = cmd.ExecuteReader())
            {
              while (reader.Read())
              {
                genres.Add((string)reader[0]);
              }
              reader.Close();
            }
          }
          connect.Close();
        }
      }
      else
      {
        using (OleDbConnection connect = new OleDbConnection("Provider=SQLOLEDB;" + connectString))
        {
          connect.Open();
          using (OleDbCommand cmd = connect.CreateCommand())
          {
            cmd.CommandText = "select distinct(genre) from Program order by genre";
            cmd.CommandType = CommandType.Text;
            using (IDataReader reader = cmd.ExecuteReader())
            {
              while (reader.Read())
              {
                genres.Add((string)reader[0]);
              }
              reader.Close();
            }
          }
          connect.Close();
        }
      }
      return genres;
    }

    private IList<string> GetDefaultMpGenreNames()
    {
      // Return a list of default names of MP genres.
      List<string> defaultGenreList = new List<string>();
      defaultGenreList.Add("Documentary");
      defaultGenreList.Add("Kids");
      defaultGenreList.Add("Movie");
      defaultGenreList.Add("Music");
      defaultGenreList.Add("News");
      defaultGenreList.Add("Special");
      defaultGenreList.Add("Sports");
      return defaultGenreList;
    }

    /// <summary>
    /// Returns a list of MediaPortal genres.
    /// </summary>
    /// <returns>A list of MediaPortal genres</returns>
    public List<MpGenre> GetMpGenres()
    {
      string genre;
      bool enabled;
      List<MpGenre> mpGenres = new List<MpGenre>();
      List<string> mappedProgramGenres;
      List<string> defaultGenreNames = (List<string>)GetDefaultMpGenreNames();

      // Get the id of the mp genre identified as the movie genre.
      int genreMapMovieGenreId;
      if (!int.TryParse(GetSetting("genreMapMovieGenreId", "-1").Value, out genreMapMovieGenreId))
      {
        genreMapMovieGenreId = -1;
      }

      // Each genre map value is a '{' delimited list of "program" genre names (those that may be compared with the genre from the program listings).
      // It is an error if a single "program" genre is mapped to more than one guide genre; behavior is undefined for this condition.
      for (int i = 0; i < defaultGenreNames.Count; i++)
      {
        // The genremap key is an integer value that is added to a base value in order to locate the correct localized genre name string.
        genre = GetSetting("genreMapName" + i, defaultGenreNames[i]).Value;

        // Get the status of the mp genre.
        if (!bool.TryParse(GetSetting("genreMapNameEnabled" + i, "True").Value, out enabled))
        {
          enabled = true;
        }

        // Create a mp genre object.
        MpGenre mpg = new MpGenre(genre, i);
        mpg.IsMovie = (i == genreMapMovieGenreId);
        mpg.Enabled = enabled;

        string genreMapEntry = GetSetting("genreMapEntry" + i, "").Value;
        mappedProgramGenres = new List<string>(genreMapEntry.Split(new char[] { '{' }, StringSplitOptions.RemoveEmptyEntries));

        foreach (string programGenre in mappedProgramGenres)
        {
          mpg.MapToProgramGenre(programGenre);
        }

        mpGenres.Add((MpGenre)mpg);
      }

      return mpGenres;
    }

    /// <summary>
    /// Save the specified list of MediaPortal genres to the database.
    /// </summary>
    /// <param name="mpGenres">A list of MediaPortal genre objects</param>
    public void SaveMpGenres(List<MpGenre> mpGenres)
    {
      Setting setting;
      foreach (var genre in mpGenres)
      {
        setting = GetSetting("genreMapName" + genre.Id, "");
        setting.Value = genre.Name;
        setting.Persist();

        string mappedProgramGenres = "";
        foreach (var programGenre in genre.MappedProgramGenres)
        {
          mappedProgramGenres += programGenre + '{';
        }

        setting = GetSetting("genreMapEntry" + genre.Id, "");
        setting.Value = mappedProgramGenres.TrimEnd('{');
        setting.Persist();

        setting = GetSetting("genreMapNameEnabled" + genre.Id, "true");
        setting.Value = genre.Enabled.ToString();
        setting.Persist();

        if (genre.IsMovie)
        {
          setting = GetSetting("genreMapMovieGenreId", "-1");
          setting.Value = genre.Id.ToString();
          setting.Persist();
        }
      }
    }

    //GEMX: for downwards compatibility
    public IList<Program> SearchProgramsPerGenre(string currentGenre, string searchCriteria)
    {
      return SearchProgramsPerGenre(currentGenre, searchCriteria, ChannelType.All);
    }

    public IList<Program> SearchProgramsPerGenre(string currentGenre, string searchCriteria, ChannelType channelType)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      StringBuilder SqlSelectCommand = new StringBuilder();
      SqlSelectCommand.Append("select p.* from Program p inner join Channel c on c.idChannel = p.idChannel ");
      SqlSelectCommand.AppendFormat("where endTime > '{0}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));
      if (currentGenre.Length > 0)
      {
        SqlSelectCommand.AppendFormat("and genre like '%{0}%' ", currentGenre);
      }
      if (searchCriteria.Length > 0)
      {
        SqlSelectCommand.AppendFormat("and title like '{0}%' ", searchCriteria);
      }
      switch (channelType)
      {
        case ChannelType.Radio:
          SqlSelectCommand.Append("and c.isTv=0 ");
          break;
        case ChannelType.Tv:
          SqlSelectCommand.Append("and c.isTv=1 ");
          break;
      }
      SqlSelectCommand.Append("and c.visibleInGuide = 1 order by title, startTime");
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof (Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(),
                                                    typeof (Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    //GEMX: for downwards compatibility
    public IList<Program> SearchPrograms(string searchCriteria)
    {
      return SearchPrograms(searchCriteria, ChannelType.All);
    }

    public IList<Program> SearchPrograms(string searchCriteria, ChannelType channelType)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      StringBuilder SqlSelectCommand = new StringBuilder();
      SqlSelectCommand.Append("select p.* from Program p inner join Channel c on c.idChannel = p.idChannel ");
      SqlSelectCommand.AppendFormat("where endTime > '{0}' ", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));

      if (searchCriteria.Length > 0)
      {
        SqlSelectCommand.AppendFormat("and title like '{0}%' ", EscapeSQLString(searchCriteria));
      }
      switch (channelType)
      {
        case ChannelType.Radio:
          SqlSelectCommand.Append("and c.isTv=0 ");
          break;
        case ChannelType.Tv:
          SqlSelectCommand.Append("and c.isTv=1 ");
          break;
      }
      SqlSelectCommand.Append("and c.visibleInGuide = 1 order by title,startTime");
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof (Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(),
                                                    typeof (Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    //GEMX: for downwards compatibility
    public IList<Program> SearchProgramsByDescription(string searchCriteria)
    {
      return SearchProgramsByDescription(searchCriteria, ChannelType.All);
    }

    public IList<Program> SearchProgramsByDescription(string searchCriteria, ChannelType channelType)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      StringBuilder SqlSelectCommand = new StringBuilder();
      SqlSelectCommand.Append("select p.* from Program p inner join Channel c on c.idChannel = p.idChannel ");
      SqlSelectCommand.AppendFormat("where endTime > '{0}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));
      if (searchCriteria.Length > 0)
      {
        SqlSelectCommand.AppendFormat("and description like '{0}%' ", EscapeSQLString(searchCriteria));
      }
      switch (channelType)
      {
        case ChannelType.Radio:
          SqlSelectCommand.Append("and c.isTv=0 ");
          break;
        case ChannelType.Tv:
          SqlSelectCommand.Append("and c.isTv=1 ");
          break;
      }
      SqlSelectCommand.Append("and c.visibleInGuide = 1 order by description, startTime");
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof (Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(),
                                                    typeof (Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    private static string BuildCommandTextMiniGuide(string aProvider, ICollection<Channel> aEpgChannelList)
    {
      string completeStatement;

      // no channel = no EPG but we need a valid command text
      if (aEpgChannelList.Count < 1)
      {
        completeStatement = "SELECT * FROM Program WHERE 0=1";
      }
      else
      {
        StringBuilder sbSelect = new StringBuilder();
        if (aProvider == "mysql")
        {
          foreach (Channel ch in aEpgChannelList)
          {
            sbSelect.AppendFormat(
              "(SELECT idChannel,idProgram,starttime,endtime,title,episodeName,seriesNum,episodeNum,episodePart FROM Program WHERE idChannel={0} AND (Program.endtime >= NOW()) order by starttime limit 2)  UNION  ",
              ch.IdChannel);
          }

          completeStatement = sbSelect.ToString();
          completeStatement = completeStatement.Remove(completeStatement.Length - 8); // Remove trailing UNION
        }
        else
        {
          //foreach (Channel ch in aEpgChannelList)
          //  sbSelect.AppendFormat("(SELECT TOP 2 idChannel,idProgram,starttime,endtime,title FROM Program WHERE idChannel={0} AND (Program.endtime >= getdate()))  UNION ALL  ", ch.IdChannel);

          //completeStatement = sbSelect.ToString();
          //completeStatement = completeStatement.Remove(completeStatement.Length - 12); // Remove trailing UNION ALL
          //completeStatement = completeStatement + " ORDER BY idChannel, startTime";   // MSSQL does not support order by in single UNION selects

          sbSelect.Append(
            "SELECT idChannel,idProgram,starttime,endtime,title,episodeName,seriesNum,episodeNum,episodePart FROM Program ");
          sbSelect.Append("WHERE (Program.endtime >= getdate() AND Program.endtime < DATEADD(day, 1, getdate()))");

          StringBuilder whereChannel = new StringBuilder(" AND (");
          foreach (Channel ch in aEpgChannelList)
          {
            whereChannel.AppendFormat("idChannel={0} OR ", ch.IdChannel);
          }

          string channelClause = whereChannel.ToString();
          // remove trailing "OR "
          channelClause = channelClause.Remove(channelClause.Length - 3);
          sbSelect.Append(channelClause);
          sbSelect.Append(")");

          sbSelect.Append(" ORDER BY idchannel,starttime");
          completeStatement = sbSelect.ToString();
        }
      }

      // Log.Info("BusinessLayer: mini-guide command: {0}", completeStatement);
      return completeStatement;
    }

    public Dictionary<int, NowAndNext> GetNowAndNext(List<Channel> aEpgChannelList)
    {
      Dictionary<int, NowAndNext> nowNextList = new Dictionary<int, NowAndNext>();
      string provider = ProviderFactory.GetDefaultProvider().Name.ToLowerInvariant();
      string connectString = ProviderFactory.GetDefaultProvider().ConnectionString;
      MySqlConnection MySQLConnect = null;
      MySqlDataAdapter MySQLAdapter = null;
      MySqlCommand MySQLCmd = null;

      SqlDataAdapter MsSqlAdapter = null;
      SqlConnection MsSqlConnect = null;
      SqlCommand MsSqlCmd = null;

      try
      {
        switch (provider)
        {
          case "mysql":
            MySQLConnect = new MySqlConnection(connectString);
            MySQLAdapter = new MySqlDataAdapter();
            MySQLAdapter.TableMappings.Add("Table", "Program");
            MySQLConnect.Open();
            MySQLCmd = new MySqlCommand(BuildCommandTextMiniGuide(provider, aEpgChannelList), MySQLConnect);
            MySQLAdapter.SelectCommand = MySQLCmd;
            break;
          case "sqlserver":
            //MSSQLConnect = new System.Data.OleDb.OleDbConnection("Provider=SQLOLEDB;" + connectString);
            MsSqlConnect = new SqlConnection(connectString);
            MsSqlAdapter = new SqlDataAdapter();
            MsSqlAdapter.TableMappings.Add("Table", "Program");
            MsSqlConnect.Open();
            MsSqlCmd = new SqlCommand(BuildCommandTextMiniGuide(provider, aEpgChannelList), MsSqlConnect);
            MsSqlAdapter.SelectCommand = MsSqlCmd;
            break;
          default:
            //MSSQLConnect = new System.Data.OleDb.OleDbConnection("Provider=SQLOLEDB;" + connectString);
            Log.Info("BusinessLayer: No connect info for provider {0} - aborting", provider);
            return nowNextList;
        }

        using (DataSet dataSet = new DataSet("Program"))
        {
          // ToDo: check if column fetching wastes performance
          switch (provider)
          {
            case "sqlserver":
              if (MsSqlAdapter != null)
              {
                MsSqlAdapter.Fill(dataSet);
              }
              break;
            case "mysql":
              if (MySQLAdapter != null)
              {
                MySQLAdapter.Fill(dataSet);
              }
              break;
          }

          nowNextList = BuildNowNextFromDataSet(dataSet);
        }
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: GetNowNext failed {0}", ex.Message);
      }
      finally
      {
        switch (provider)
        {
          case "mysql":
            if (MySQLConnect != null)
            {
              MySQLConnect.Close();
            }
            if (MySQLAdapter != null)
            {
              MySQLAdapter.Dispose();
            }
            if (MySQLCmd != null)
            {
              MySQLCmd.Dispose();
            }
            if (MySQLConnect != null)
            {
              MySQLConnect.Dispose();
            }
            break;
          case "sqlserver":
            if (MsSqlConnect != null)
            {
              MsSqlConnect.Close();
            }
            if (MsSqlAdapter != null)
            {
              MsSqlAdapter.Dispose();
            }
            if (MsSqlCmd != null)
            {
              MsSqlCmd.Dispose();
            }
            if (MsSqlConnect != null)
            {
              MsSqlConnect.Dispose();
            }
            break;
        }
      }
      return nowNextList;
    }

    private static Dictionary<int, NowAndNext> BuildNowNextFromDataSet(DataSet dataSet)
    {
      Dictionary<int, NowAndNext> progList = new Dictionary<int, NowAndNext>();

      int programsCount = dataSet.Tables[0].Rows.Count;
      List<int> lastChannelIDs = new List<int>();

      // for-loops are faster than foreach-loops
      for (int j = 0; j < programsCount; j++)
      {
        int idChannel = (int)dataSet.Tables[0].Rows[j]["idChannel"];
        // Only get the Now-Next-Data _once_ per channel
        if (!lastChannelIDs.Contains(idChannel))
        {
          lastChannelIDs.Add(idChannel);

          int nowidProgram = (int)dataSet.Tables[0].Rows[j]["idProgram"];
          DateTime nowStart = (DateTime)dataSet.Tables[0].Rows[j]["startTime"];
          DateTime nowEnd = (DateTime)dataSet.Tables[0].Rows[j]["endTime"];
          string nowTitle = (string)dataSet.Tables[0].Rows[j]["title"];
          string episodeName = (string)dataSet.Tables[0].Rows[j]["episodeName"];
          string seriesNum = (string)dataSet.Tables[0].Rows[j]["seriesNum"];
          string episodeNum = (string)dataSet.Tables[0].Rows[j]["episodeNum"];
          string episodePart = (string)dataSet.Tables[0].Rows[j]["episodePart"];
          // if the first entry is not valid for the "Now" entry - use if for "Next" info
          if (nowStart > DateTime.Now)
          {
            NowAndNext p = new NowAndNext(idChannel, SqlDateTime.MinValue.Value, SqlDateTime.MinValue.Value,
                                          string.Empty, nowTitle, -1, nowidProgram, episodeName, string.Empty,
                                          seriesNum, string.Empty, episodeNum, string.Empty,
                                          episodePart, string.Empty);
            progList[idChannel] = p;
            continue;
          }

          if (j < programsCount - 1)
          {
            // get the the "Next" info if it belongs to the same channel.
            if (idChannel == (int)dataSet.Tables[0].Rows[j + 1]["idChannel"])
            {
              int nextidProgram = (int)dataSet.Tables[0].Rows[j + 1]["idProgram"];
              string nextTitle = (string)dataSet.Tables[0].Rows[j + 1]["title"];
              string nextEpisodeName = (string)dataSet.Tables[0].Rows[j + 1]["episodeName"];
              string nextSeriesNum = (string)dataSet.Tables[0].Rows[j + 1]["seriesNum"];
              string nextEpisodeNum = (string)dataSet.Tables[0].Rows[j + 1]["episodeNum"];
              string nextEpisodePart = (string)dataSet.Tables[0].Rows[j + 1]["episodePart"];
              NowAndNext p = new NowAndNext(idChannel, nowStart, nowEnd, nowTitle, nextTitle, nowidProgram,
                                            nextidProgram, episodeName, nextEpisodeName, seriesNum, nextSeriesNum,
                                            episodeNum, nextEpisodeNum, episodePart, nextEpisodePart);
              progList[idChannel] = p;
            }
            else
            {
              // no "next" info because of holes in EPG data - we want the "now" info nevertheless
              NowAndNext p = new NowAndNext(idChannel, nowStart, nowEnd, nowTitle, string.Empty, nowidProgram, -1,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty);
              progList[idChannel] = p;
            }
          }
        }
      }
      return progList;
    }

    #region EPG Insert

    private class ImportParams
    {
      public ProgramList ProgramList;
      public DeleteBeforeImportOption ProgamsToDelete;
      public string ConnectString;
      public ThreadPriority Priority;
      public int SleepTime;
    } ;

    /// <summary>
    /// Batch inserts programs - intended for faster EPG import. You must make sure before that there are no duplicates 
    /// (e.g. delete all program data of the current channel).
    /// Also you MUST provide a true copy of "aProgramList". If you update it's reference in your own code the values will get overwritten
    /// (possibly before they are written to disk)!
    /// </summary>
    /// <param name="aProgramList">A list of persistable gentle.NET Program objects mapping to the Programs table</param>
    /// <param name="aThreadPriority">Use "Lowest" for Background imports allowing LiveTV, AboveNormal for full speed</param>
    /// <returns>The record count of programs if successful, 0 on errors</returns>
    public int InsertPrograms(List<Program> aProgramList, ThreadPriority aThreadPriority)
    {
      return InsertPrograms(aProgramList, DeleteBeforeImportOption.None, aThreadPriority);
    }

    /// <summary>
    /// Batch inserts programs - intended for faster EPG import. You must make sure before that there are no duplicates 
    /// (e.g. delete all program data of the current channel).
    /// Also you MUST provide a true copy of "aProgramList". If you update it's reference in your own code the values will get overwritten
    /// (possibly before they are written to disk)!
    /// </summary>
    /// <param name="aProgramList">A list of persistable gentle.NET Program objects mapping to the Programs table</param>
    /// <param name="progamsToDelete">Flag specifying which existing programs to delete before the insert</param>
    /// <param name="aThreadPriority">Use "Lowest" for Background imports allowing LiveTV, AboveNormal for full speed</param>
    /// <returns>The record count of programs if successful, 0 on errors</returns>
    /// <remarks><para>Inserts are queued to be performed in the background. Each batch of inserts is executed in a single transaction.
    /// You may also optionally specify to delete either all existing programs in the same channel(s) as the programs to be inserted 
    /// (<see cref="DeleteBeforeImportOption.ProgramsOnSameChannel"/>), or existing programs that would otherwise overlap new programs
    /// (<see cref="DeleteBeforeImportOption.OverlappingPrograms"/>), or none (<see cref="DeleteBeforeImportOption.None"/>).
    /// The deletion is also performed in the same transaction as the inserts so that EPG will not be at any time empty.</para>
    /// <para>After all insert have completed and the background thread is idle for 60 seconds, the program states are
    /// automatically updated to reflect the changes.</para></remarks>
    public int InsertPrograms(List<Program> aProgramList, DeleteBeforeImportOption progamsToDelete,
                              ThreadPriority aThreadPriority)
    {
      try
      {
        int sleepTime = 10;

        switch (aThreadPriority)
        {
          case ThreadPriority.Highest:
          case ThreadPriority.AboveNormal:
            aThreadPriority = ThreadPriority.Normal;
            sleepTime = 0;
            break;
          case ThreadPriority.Normal:
            // this is almost enough on dualcore systems for one cpu to gather epg and the other to insert it
            sleepTime = 10;
            break;
          case ThreadPriority.BelowNormal: // on faster systems this might be enough for background importing
            sleepTime = 20;
            break;
          case ThreadPriority.Lowest: // even a single core system is enough to use MP while importing.
            sleepTime = 40;
            break;
        }

        ImportParams param = new ImportParams();
        param.ProgramList = new ProgramList(aProgramList);
        param.ProgamsToDelete = progamsToDelete;
        param.SleepTime = sleepTime;
        param.Priority = aThreadPriority;

        lock (_programInsertsQueue)
        {
          _programInsertsQueue.Enqueue(param);
          _pendingProgramInserts.Set();

          if (_insertProgramsThread == null)
          {
            _insertProgramsThread = new Thread(InsertProgramsThreadStart)
                                      {
                                        Priority = ThreadPriority.Lowest,
                                        Name = "SQL EPG importer",
                                        IsBackground = true
                                      };
            _insertProgramsThread.Start();
          }
        }

        return aProgramList.Count;
      }
      catch (Exception ex)
      {
        Log.Error("BusinessLayer: InsertPrograms error - {0}, {1}", ex.Message, ex.StackTrace);
        return 0;
      }
    }

    public void WaitForInsertPrograms(int millisecondsTimeout)
    {
      Thread currentInsertThread = _insertProgramsThread;
      if (currentInsertThread != null &&
          (currentInsertThread.ThreadState & ThreadState.Unstarted) != ThreadState.Unstarted)
        currentInsertThread.Join(millisecondsTimeout);
    }

    public void WaitForInsertPrograms()
    {
      Thread currentInsertThread = _insertProgramsThread;
      if (currentInsertThread != null &&
          (currentInsertThread.ThreadState & ThreadState.Unstarted) != ThreadState.Unstarted)
        currentInsertThread.Join();
    }

    //public void StartResetProgramStatesThread(ThreadPriority aThreadPriority)
    //{
    //  if (_resetProgramStatesThread == null || !_resetProgramStatesThread.IsAlive)
    //  {
    //    _resetProgramStatesThread = new Thread(ProgramStatesThread);
    //    _resetProgramStatesThread.Priority = aThreadPriority;
    //    _resetProgramStatesThread.Name = "Program states thread";
    //    _resetProgramStatesThread.IsBackground = true;
    //    _resetProgramStatesThread.Start();
    //  }
    //}

    //private void ProgramStatesThread()
    //{
    //  lock (SingleProgramStateUpdate)
    //  {
    //    if (_lastProgramUpdate == DateTime.MinValue)
    //    {
    //      return;
    //    }

    //    while (true)
    //    {
    //      System.TimeSpan ts = DateTime.Now - _lastProgramUpdate;

    //      Log.Info("BusinessLayer: ProgramStatesThread waiting...{0} sec", ts.TotalSeconds);

    //      if (ts.TotalSeconds >= 60) //if more than 60 secs. has passed since last update to the program table, then lets do the program states
    //      {
    //        Log.Info("BusinessLayer: ProgramStatesThread - done waiting. calling SynchProgramStatesForAll");
    //        _lastProgramUpdate = DateTime.MinValue;
    //        Schedule.SynchProgramStatesForAll();
    //        return;
    //      }
    //      else
    //      {
    //        Thread.Sleep(1000);
    //      }
    //    }
    //  }
    //}

    //private void ImportMySqlThread(object aImportParam)
    //{
    //  lock (SingleInsert)
    //  {
    //    ImportParams MyParams = (ImportParams)aImportParam;
    //    InsertMySql(MyParams);
    //    _lastProgramUpdate = DateTime.Now;
    //    StartResetProgramStatesThread(ThreadPriority.BelowNormal);
    //  }
    //}

    //private void ImportSqlServerThread(object aImportParam)
    //{
    //  lock (SingleInsert)
    //  {
    //    ImportParams MyParams = (ImportParams)aImportParam;
    //    InsertSqlServer(MyParams);
    //    _lastProgramUpdate = DateTime.Now;
    //    StartResetProgramStatesThread(ThreadPriority.BelowNormal);
    //  }
    //}

    private static void InsertProgramsThreadStart()
    {
      try
      {
        Log.Debug("BusinessLayer: InsertProgramsThread started");

        IGentleProvider prov = ProviderFactory.GetDefaultProvider();
        string provider = prov.Name.ToLowerInvariant();
        string defaultConnectString = prov.ConnectionString;
        DateTime lastImport = DateTime.Now;
        InsertProgramsDelegate insertProgams;

        switch (provider)
        {
          case "mysql":
            insertProgams = InsertProgramsMySql;
            break;
          case "sqlserver":
            insertProgams = InsertProgramsSqlServer;
            break;
          default:
            Log.Info("BusinessLayer: InsertPrograms unknown provider - {0}", provider);
            return;
        }

        while (true)
        {
          if (lastImport.AddSeconds(60) < DateTime.Now)
          {
            // Done importing and 60 seconds since last import
            // Remove old programs
            TvBusinessLayer layer = new TvBusinessLayer();
            layer.RemoveOldPrograms();
            // Let's update states
            Schedule.SynchProgramStatesForAll();
            // and exit
            lock (_programInsertsQueue)
            {
              //  Has new work been queued in the meantime?
              if (_programInsertsQueue.Count == 0)
              {
                Log.Debug("BusinessLayer: InsertProgramsThread exiting");
                _insertProgramsThread = null;
                break;
              }
            }
          }

          _pendingProgramInserts.WaitOne(10000); // Check every 10 secs
          while (_programInsertsQueue.Count > 0)
          {
            try
            {
              ImportParams importParams;
              lock (_programInsertsQueue)
              {
                importParams = _programInsertsQueue.Dequeue();
              }
              importParams.ConnectString = defaultConnectString;
              Thread.CurrentThread.Priority = importParams.Priority;
              insertProgams(importParams);
              Log.Debug("BusinessLayer: Inserted {0} programs to the database", importParams.ProgramList.Count);
              lastImport = DateTime.Now;
              Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            }
            catch (Exception ex)
            {
              Log.Error("BusinessLayer: InsertMySQL/InsertMSSQL caused an exception:");
              Log.Write(ex);
            }
          }
          // Now all queued inserts have been processed, clear Gentle cache
          Gentle.Common.CacheManager.ClearQueryResultsByType(typeof (Program));
        }
      }
      catch (Exception ex)
      {
        Log.Error("BusinessLayer: InsertProgramsThread error - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    #region SQL methods

    /// <summary>
    /// Checks a MySQL table for corruption and optimizes / compacts it. Use this after lots of insert / delete operations.
    /// </summary>
    /// <param name="aTable">The table which should be optimized. During the check process it will be locked.</param>
    public void OptimizeMySql(string aTable)
    {
      Stopwatch benchClock = new Stopwatch();
      benchClock.Start();
      MySqlTransaction transact = null;
      try
      {
        string connectString = (ProviderFactory.GetDefaultProvider()).ConnectionString;
        if (string.IsNullOrEmpty(connectString))
        {
          return;
        }

        using (MySqlConnection connection = new MySqlConnection(connectString))
        {
          connection.Open();
          transact = connection.BeginTransaction();
          using (MySqlCommand cmd = new MySqlCommand(string.Format("LOCK TABLES {0} READ;", aTable), connection))
          {
            cmd.ExecuteNonQuery();
          }

          using (MySqlCommand cmd = new MySqlCommand(string.Format("CHECK TABLE {0}", aTable), connection))
          {
            cmd.ExecuteNonQuery();
          }

          using (MySqlCommand cmd = new MySqlCommand(string.Format("UNLOCK TABLES"), connection))
          {
            cmd.ExecuteNonQuery();
          }

          using (MySqlCommand cmd = new MySqlCommand(string.Format("OPTIMIZE TABLE {0}", aTable), connection))
          {
            cmd.ExecuteNonQuery();
          }

          transact.Commit();
          benchClock.Stop();
          Log.Info("BusinessLayer: OptimizeMySql successful - duration: {0}ms",
                   benchClock.ElapsedMilliseconds.ToString());
        }
      }
      catch (Exception ex)
      {
        try
        {
          if (transact != null)
          {
            transact.Rollback();
          }
        }
        catch (Exception ex2)
        {
          Log.Info("BusinessLayer: OptimizeMySql unsuccessful - ROLLBACK - {0}, {1}", ex2.Message, ex2.StackTrace);
        }
        Log.Info("BusinessLayer: OptimizeMySql unsuccessful - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    private static void InsertProgramsMySql(ImportParams aImportParam)
    {
      MySqlTransaction transact = null;
      try
      {
        using (MySqlConnection connection = new MySqlConnection(aImportParam.ConnectString))
        {
          DeleteProgramsDelegate deletePrograms = null;

          switch (aImportParam.ProgamsToDelete)
          {
            case DeleteBeforeImportOption.OverlappingPrograms:
              IEnumerable<ProgramListPartition> partitions = aImportParam.ProgramList.GetPartitions();
              deletePrograms =
                () => ExecuteDeleteProgramsMySqlCommand(partitions, connection, transact, aImportParam.SleepTime);
              break;
            case DeleteBeforeImportOption.ProgramsOnSameChannel:
              IEnumerable<int> channelIds = aImportParam.ProgramList.GetChannelIds();
              deletePrograms =
                () => ExecuteDeleteProgramsMySqlCommand(channelIds, connection, transact, aImportParam.SleepTime);
              break;
          }
          connection.Open();
          transact = connection.BeginTransaction();
          if (deletePrograms != null)
          {
            deletePrograms();
          }
          ExecuteInsertProgramsMySqlCommand(aImportParam.ProgramList, connection, transact, aImportParam.SleepTime);
          transact.Commit();
          //OptimizeMySql("Program");
        }
      }
      catch (Exception ex)
      {
        try
        {
          if (transact != null)
          {
            transact.Rollback();
          }
        }
        catch (Exception ex2)
        {
          Log.Info("BusinessLayer: InsertSqlServer unsuccessful - ROLLBACK - {0}, {1}", ex2.Message, ex2.StackTrace);
        }
        Log.Info("BusinessLayer: InsertMySql caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    private static void InsertProgramsSqlServer(ImportParams aImportParam)
    {
      SqlTransaction transact = null;
      try
      {
        using (SqlConnection connection = new SqlConnection(aImportParam.ConnectString))
        {
          DeleteProgramsDelegate deletePrograms = null;

          switch (aImportParam.ProgamsToDelete)
          {
            case DeleteBeforeImportOption.OverlappingPrograms:
              IEnumerable<ProgramListPartition> partitions = aImportParam.ProgramList.GetPartitions();
              deletePrograms =
                () => ExecuteDeleteProgramsSqlServerCommand(partitions, connection, transact, aImportParam.SleepTime);
              break;
            case DeleteBeforeImportOption.ProgramsOnSameChannel:
              IEnumerable<int> channelIds = aImportParam.ProgramList.GetChannelIds();
              deletePrograms =
                () => ExecuteDeleteProgramsSqlServerCommand(channelIds, connection, transact, aImportParam.SleepTime);
              break;
          }
          connection.Open();
          transact = connection.BeginTransaction();
          if (deletePrograms != null)
          {
            deletePrograms();
          }
          ExecuteInsertProgramsSqlServerCommand(aImportParam.ProgramList, connection, transact, aImportParam.SleepTime);
          transact.Commit();
        }
      }
      catch (Exception ex)
      {
        try
        {
          if (transact != null)
          {
            transact.Rollback();
          }
        }
        catch (Exception ex2)
        {
          Log.Info("BusinessLayer: InsertSqlServer unsuccessful - ROLLBACK - {0}, {1}", ex2.Message, ex2.StackTrace);
        }
        Log.Info("BusinessLayer: InsertSqlServer caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    #endregion

    #region SQL Builder

    private static void ExecuteDeleteProgramsMySqlCommand(IEnumerable<ProgramListPartition> deleteProgramRanges,
                                                          MySqlConnection aConnection,
                                                          MySqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      MySqlCommand sqlCmd = new MySqlCommand();

      sqlCmd.CommandText =
        "DELETE FROM Program WHERE idChannel = ?idChannel AND ((endTime > ?rangeStart AND startTime < ?rangeEnd) OR (startTime = endTime AND startTime BETWEEN ?rangeStart AND ?rangeEnd))";

      sqlCmd.Parameters.Add("?idChannel", MySqlDbType.Int32);
      sqlCmd.Parameters.Add("?rangeStart", MySqlDbType.DateTime);
      sqlCmd.Parameters.Add("?rangeEnd", MySqlDbType.DateTime);

      try
      {
        sqlCmd.Connection = aConnection;
        sqlCmd.Transaction = aTransaction;
        // Prepare the command since we will reuse it quite often
        sqlCmd.Prepare();
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: ExecuteDeleteProgramsMySqlCommand - Prepare caused an Exception - {0}", ex.Message);
        throw;
      }

      foreach (ProgramListPartition partition in deleteProgramRanges)
      {
        sqlCmd.Parameters["?idChannel"].Value = partition.IdChannel;
        sqlCmd.Parameters["?rangeStart"].Value = partition.Start;
        sqlCmd.Parameters["?rangeEnd"].Value = partition.End;
        try
        {
          // Finally insert all our data
          sqlCmd.ExecuteNonQuery();
          aCounter++;
          // Avoid I/O starving
          if (aCounter % 3 == 0)
          {
            Thread.Sleep(aDelay);
          }
        }
        catch (Exception ex)
        {
          Log.Info("BusinessLayer: ExecuteDeleteProgramsMySqlCommand caused an Exception - {0}, {1}", ex.Message,
                   ex.StackTrace);
          throw;
        }
      }
      return;
    }

    private static void ExecuteDeleteProgramsMySqlCommand(IEnumerable<int> channelIds, MySqlConnection aConnection,
                                                          MySqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      MySqlCommand sqlCmd = new MySqlCommand();

      sqlCmd.CommandText =
        "DELETE FROM Program WHERE idChannel = ?idChannel";

      sqlCmd.Parameters.Add("?idChannel", MySqlDbType.Int32);

      try
      {
        sqlCmd.Connection = aConnection;
        sqlCmd.Transaction = aTransaction;
        // Prepare the command since we will reuse it quite often
        sqlCmd.Prepare();
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: ExecuteDeleteProgramsMySqlCommand - Prepare caused an Exception - {0}", ex.Message);
        throw;
      }

      foreach (int idChannel in channelIds)
      {
        sqlCmd.Parameters["?idChannel"].Value = idChannel;
        try
        {
          // Finally insert all our data
          sqlCmd.ExecuteNonQuery();
          aCounter++;
          // Avoid I/O starving
          if (aCounter % 3 == 0)
          {
            Thread.Sleep(aDelay);
          }
        }
        catch (Exception ex)
        {
          Log.Info("BusinessLayer: ExecuteDeleteProgramsMySqlCommand caused an Exception - {0}, {1}", ex.Message,
                   ex.StackTrace);
          throw;
        }
      }
      return;
    }

    private static void ExecuteInsertProgramsMySqlCommand(IEnumerable<Program> aProgramList, MySqlConnection aConnection,
                                                          MySqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      MySqlCommand sqlCmd = new MySqlCommand();
      List<Program> currentInserts = new List<Program>(aProgramList);

      sqlCmd.CommandText =
        "INSERT INTO Program (idChannel, startTime, endTime, title, description, seriesNum, episodeNum, genre, originalAirDate, classification, starRating, state, parentalRating, episodeName, episodePart) VALUES (?idChannel, ?startTime, ?endTime, ?title, ?description, ?seriesNum, ?episodeNum, ?genre, ?originalAirDate, ?classification, ?starRating, ?state, ?parentalRating, ?episodeName, ?episodePart)";

      sqlCmd.Parameters.Add("?idChannel", MySqlDbType.Int32);
      sqlCmd.Parameters.Add("?startTime", MySqlDbType.DateTime);
      sqlCmd.Parameters.Add("?endTime", MySqlDbType.DateTime);
      sqlCmd.Parameters.Add("?title", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?description", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?seriesNum", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?episodeNum", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?genre", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?originalAirDate", MySqlDbType.DateTime);
      sqlCmd.Parameters.Add("?classification", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?starRating", MySqlDbType.Int32);
      sqlCmd.Parameters.Add("?state", MySqlDbType.Int32);
      sqlCmd.Parameters.Add("?parentalRating", MySqlDbType.Int32);
      sqlCmd.Parameters.Add("?episodeName", MySqlDbType.Text);
      sqlCmd.Parameters.Add("?episodePart", MySqlDbType.Text);

      try
      {
        sqlCmd.Connection = aConnection;
        sqlCmd.Transaction = aTransaction;
        // Prepare the command since we will reuse it quite often
        sqlCmd.Prepare();
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: ExecuteInsertProgramsMySqlCommand - Prepare caused an Exception - {0}", ex.Message);
      }
      foreach (Program prog in currentInserts)
      {
        sqlCmd.Parameters["?idChannel"].Value = prog.IdChannel;
        sqlCmd.Parameters["?startTime"].Value = prog.StartTime;
        sqlCmd.Parameters["?endTime"].Value = prog.EndTime;
        sqlCmd.Parameters["?title"].Value = prog.Title;
        sqlCmd.Parameters["?description"].Value = prog.Description;
        sqlCmd.Parameters["?seriesNum"].Value = prog.SeriesNum;
        sqlCmd.Parameters["?episodeNum"].Value = prog.EpisodeNum;
        sqlCmd.Parameters["?genre"].Value = prog.Genre;
        sqlCmd.Parameters["?originalAirDate"].Value = prog.OriginalAirDate;
        sqlCmd.Parameters["?classification"].Value = prog.Classification;
        sqlCmd.Parameters["?starRating"].Value = prog.StarRating;
        sqlCmd.Parameters["?state"].Value = 0; // prog.Notify;
        sqlCmd.Parameters["?parentalRating"].Value = prog.ParentalRating;
        sqlCmd.Parameters["?episodeName"].Value = prog.EpisodeName;
        sqlCmd.Parameters["?episodePart"].Value = prog.EpisodePart;
        try
        {
          // Finally insert all our data
          sqlCmd.ExecuteNonQuery();
          aCounter++;
          // Avoid I/O starving
          if (aCounter % 3 == 0)
          {
            Thread.Sleep(aDelay);
          }
        }
        catch (MySqlException myex)
        {
          string errorRow = sqlCmd.Parameters["?idChannel"].Value + ", " + sqlCmd.Parameters["?title"].Value + " : " +
                            sqlCmd.Parameters["?startTime"].Value + "-" + sqlCmd.Parameters["?endTime"].Value;
          switch (myex.Number)
          {
            case 1062:
              Log.Info("BusinessLayer: Your importer tried to add a duplicate entry: {0}", errorRow);
              break;
            case 1406:
              Log.Info("BusinessLayer: Your importer tried to add a too much info: {0}, {1}", errorRow, myex.Message);
              break;
            default:
              Log.Info("BusinessLayer: ExecuteInsertProgramsMySqlCommand caused a MySqlException - {0}, {1} {2}",
                       myex.Message,
                       myex.Number, myex.HelpLink);
              break;
          }
        }
        catch (Exception ex)
        {
          Log.Info("BusinessLayer: ExecuteInsertProgramsMySqlCommand caused an Exception - {0}, {1}", ex.Message,
                   ex.StackTrace);
        }
      }
      return;
    }

    private static void ExecuteDeleteProgramsSqlServerCommand(IEnumerable<ProgramListPartition> deleteProgramRanges,
                                                              SqlConnection aConnection,
                                                              SqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      SqlCommand sqlCmd = new SqlCommand();

      sqlCmd.CommandText =
        "DELETE FROM Program WHERE idChannel = @idChannel AND ((endTime > @rangeStart AND startTime < @rangeEnd) OR (startTime = endTime AND startTime BETWEEN @rangeStart AND @rangeEnd))";

      sqlCmd.Parameters.Add("idChannel", SqlDbType.Int);
      sqlCmd.Parameters.Add("rangeStart", SqlDbType.DateTime);
      sqlCmd.Parameters.Add("rangeEnd", SqlDbType.DateTime);

      try
      {
        sqlCmd.Connection = aConnection;
        sqlCmd.Transaction = aTransaction;
        // Prepare the command since we will reuse it quite often
        // sqlCmd.Prepare(); <-- this would need exact param field length definitions
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: ExecuteDeleteProgramsSqlServerCommand - Prepare caused an Exception - {0}", ex.Message);
      }
      foreach (ProgramListPartition partition in deleteProgramRanges)
      {
        sqlCmd.Parameters["idChannel"].Value = partition.IdChannel;
        sqlCmd.Parameters["rangeStart"].Value = partition.Start;
        sqlCmd.Parameters["rangeEnd"].Value = partition.End;

        try
        {
          // Finally insert all our data
          sqlCmd.ExecuteNonQuery();
          aCounter++;
          // Avoid I/O starving
          if (aCounter % 2 == 0)
          {
            Thread.Sleep(aDelay);
          }
        }
        catch (Exception ex)
        {
          Log.Error("BusinessLayer: ExecuteDeleteProgramsSqlServerCommand error - {0}, {1}", ex.Message, ex.StackTrace);
        }
      }
      return;
    }

    private static void ExecuteDeleteProgramsSqlServerCommand(IEnumerable<int> channelIds, SqlConnection aConnection,
                                                              SqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      SqlCommand sqlCmd = new SqlCommand();

      sqlCmd.CommandText =
        "DELETE FROM Program WHERE idChannel = @idChannel";

      sqlCmd.Parameters.Add("idChannel", SqlDbType.Int);

      try
      {
        sqlCmd.Connection = aConnection;
        sqlCmd.Transaction = aTransaction;
        // Prepare the command since we will reuse it quite often
        // sqlCmd.Prepare(); <-- this would need exact param field length definitions
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: ExecuteDeleteProgramsSqlServerCommand - Prepare caused an Exception - {0}", ex.Message);
      }
      foreach (int idChannel in channelIds)
      {
        sqlCmd.Parameters["idChannel"].Value = idChannel;

        try
        {
          // Finally insert all our data
          sqlCmd.ExecuteNonQuery();
          aCounter++;
          // Avoid I/O starving
          if (aCounter % 2 == 0)
          {
            Thread.Sleep(aDelay);
          }
        }
        catch (Exception ex)
        {
          Log.Error("BusinessLayer: ExecuteDeleteProgramsSqlServerCommand error - {0}, {1}", ex.Message, ex.StackTrace);
        }
      }
      return;
    }

    private static void ExecuteInsertProgramsSqlServerCommand(IEnumerable<Program> aProgramList,
                                                              SqlConnection aConnection,
                                                              SqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      SqlCommand sqlCmd = new SqlCommand();
      List<Program> currentInserts = new List<Program>(aProgramList);

      sqlCmd.CommandText =
        "INSERT INTO Program (idChannel, startTime, endTime, title, description, seriesNum, episodeNum, genre, originalAirDate, classification, starRating, state, parentalRating, episodeName, episodePart) VALUES (@idChannel, @startTime, @endTime, @title, @description, @seriesNum, @episodeNum, @genre, @originalAirDate, @classification, @starRating, @state, @parentalRating, @episodeName, @episodePart)";

      sqlCmd.Parameters.Add("idChannel", SqlDbType.Int);
      sqlCmd.Parameters.Add("startTime", SqlDbType.DateTime);
      sqlCmd.Parameters.Add("endTime", SqlDbType.DateTime);
      sqlCmd.Parameters.Add("title", SqlDbType.VarChar);
      sqlCmd.Parameters.Add("description", SqlDbType.VarChar);
      sqlCmd.Parameters.Add("seriesNum", SqlDbType.VarChar);
      sqlCmd.Parameters.Add("episodeNum", SqlDbType.VarChar);
      sqlCmd.Parameters.Add("genre", SqlDbType.VarChar);
      sqlCmd.Parameters.Add("originalAirDate", SqlDbType.DateTime);
      sqlCmd.Parameters.Add("classification", SqlDbType.VarChar);
      sqlCmd.Parameters.Add("starRating", SqlDbType.Int);
      sqlCmd.Parameters.Add("state", SqlDbType.Int);
      sqlCmd.Parameters.Add("parentalRating", SqlDbType.Int);
      sqlCmd.Parameters.Add("episodeName", SqlDbType.VarChar);
      sqlCmd.Parameters.Add("episodePart", SqlDbType.VarChar);

      try
      {
        sqlCmd.Connection = aConnection;
        sqlCmd.Transaction = aTransaction;
        // Prepare the command since we will reuse it quite often
        // sqlCmd.Prepare(); <-- this would need exact param field length definitions
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: ExecuteInsertProgramsSqlServerCommand - Prepare caused an Exception - {0}", ex.Message);
      }
      foreach (Program prog in currentInserts)
      {
        sqlCmd.Parameters["idChannel"].Value = prog.IdChannel;
        sqlCmd.Parameters["startTime"].Value = prog.StartTime;
        sqlCmd.Parameters["endTime"].Value = prog.EndTime;
        sqlCmd.Parameters["title"].Value = prog.Title;
        sqlCmd.Parameters["description"].Value = prog.Description;
        sqlCmd.Parameters["seriesNum"].Value = prog.SeriesNum;
        sqlCmd.Parameters["episodeNum"].Value = prog.EpisodeNum;
        sqlCmd.Parameters["genre"].Value = prog.Genre;
        sqlCmd.Parameters["originalAirDate"].Value = prog.OriginalAirDate;
        sqlCmd.Parameters["classification"].Value = prog.Classification;
        sqlCmd.Parameters["starRating"].Value = prog.StarRating;
        sqlCmd.Parameters["state"].Value = 0; // prog.Notify;
        sqlCmd.Parameters["parentalRating"].Value = prog.ParentalRating;
        sqlCmd.Parameters["episodeName"].Value = prog.EpisodeName;
        sqlCmd.Parameters["episodePart"].Value = prog.EpisodePart;

        try
        {
          // Finally insert all our data
          sqlCmd.ExecuteNonQuery();
          aCounter++;
          // Avoid I/O starving
          if (aCounter % 2 == 0)
          {
            Thread.Sleep(aDelay);
          }
        }
        catch (SqlException msex)
        {
          string errorRow = sqlCmd.Parameters["idChannel"].Value + ", " + sqlCmd.Parameters["title"].Value + " : " +
                            sqlCmd.Parameters["startTime"].Value + "-" + sqlCmd.Parameters["endTime"].Value;
          switch (msex.Number)
          {
            case 2601:
              Log.Info("BusinessLayer: Your importer tried to add a duplicate entry: {0}", errorRow);
              break;
            case 8152:
              Log.Info("BusinessLayer: Your importer tried to add a too much info: {0}, {1}", errorRow, msex.Message);
              break;
            default:
              Log.Info("BusinessLayer: ExecuteInsertProgramsSqlServerCommand caused a SqlException - {0}, {1} {2}",
                       msex.Message, msex.Number,
                       msex.HelpLink);
              break;
          }
        }
        catch (Exception ex)
        {
          Log.Error("BusinessLayer: ExecuteInsertProgramsSqlServerCommand error - {0}, {1}", ex.Message, ex.StackTrace);
        }
      }
      return;
    }

    #endregion // SQL builder

    #endregion // EPG Insert

    #endregion // programs

    #region schedules

    public List<Schedule> GetConflictingSchedules(Schedule rec)
    {
      Log.Info("GetConflictingSchedules: Schedule = " + rec);
      List<Schedule> conflicts = new List<Schedule>();
      IList<Schedule> schedulesList = Schedule.ListAll();

      IList<Card> cards = Card.ListAll();
      if (cards.Count == 0)
      {
        return conflicts;
      }
      Log.Info("GetConflictingSchedules: Cards.Count = {0}", cards.Count);

      List<Schedule>[] cardSchedules = new List<Schedule>[cards.Count];
      for (int i = 0; i < cards.Count; i++)
      {
        cardSchedules[i] = new List<Schedule>();
      }

      // GEMX: Assign all already scheduled timers to cards. Assume that even possibly overlapping schedulues are ok to the user,
      // as he decided to keep them before. That's why they are in the db
      foreach (Schedule schedule in schedulesList)
      {
        List<Schedule> episodes = GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.EndTime)
          {
            continue;
          }
          if (episode.IsSerieIsCanceled(episode.StartTime))
          {
            continue;
          }
          List<Schedule> overlapping;
          AssignSchedulesToCard(episode, cardSchedules, out overlapping);
        }
      }

      List<Schedule> newEpisodes = GetRecordingTimes(rec);
      foreach (Schedule newEpisode in newEpisodes)
      {
        if (DateTime.Now > newEpisode.EndTime)
        {
          continue;
        }
        if (newEpisode.IsSerieIsCanceled(newEpisode.StartTime))
        {
          continue;
        }
        List<Schedule> overlapping;
        if (!AssignSchedulesToCard(newEpisode, cardSchedules, out overlapping))
        {
          Log.Info("GetConflictingSchedules: newEpisode can not be assigned to a card = " + newEpisode);
          conflicts.AddRange(overlapping);
        }
      }
      return conflicts;
    }

    private static bool AssignSchedulesToCard(Schedule schedule, List<Schedule>[] cardSchedules,
                                              out List<Schedule> overlappingSchedules)
    {
      overlappingSchedules = new List<Schedule>();
      Log.Info("AssignSchedulesToCard: schedule = " + schedule);
      IList<Card> cards = Card.ListAll();
      bool assigned = false;
      int count = 0;
      foreach (Card card in cards)
      {
        if (card.Enabled && card.canViewTvChannel(schedule.IdChannel))
        {
          // checks if any schedule assigned to this cards overlaps current parsed schedule
          bool free = true;
          foreach (Schedule assignedSchedule in cardSchedules[count])
          {
            Log.Info("AssignSchedulesToCard: card {0}, ID = {1} has schedule = " + assignedSchedule, count, card.IdCard);
            bool hasOverlappingSchedule = schedule.IsOverlapping(assignedSchedule);
            if (hasOverlappingSchedule)
            {
              bool isSameTransponder = (schedule.isSameTransponder(assignedSchedule) && card.supportSubChannels);
              if (!isSameTransponder)
              {
                overlappingSchedules.Add(assignedSchedule);
                Log.Info("AssignSchedulesToCard: overlapping with " + assignedSchedule + " on card {0}, ID = {1}", count,
                         card.IdCard);
                free = false;
                break;
              }
            }
          }
          if (free)
          {
            Log.Info("AssignSchedulesToCard: free on card {0}, ID = {1}", count, card.IdCard);
            cardSchedules[count].Add(schedule);                                                
            assigned = true;
            break;
          }
        }
        count++;
      }
      if (!assigned)
      {
        return false;
      }

      return true;
    }

    public List<Schedule> GetRecordingTimes(Schedule rec)
    {
      return GetRecordingTimes(rec, 10);
    }


    public List<Schedule> GetRecordingTimes(Schedule rec, int days)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      List<Schedule> recordings = new List<Schedule>();

      DateTime dtDay = DateTime.Now;
      if (rec.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        recordings.Add(rec);
        return recordings;
      }

      if (rec.ScheduleType == (int)ScheduleRecordingType.Daily)
      {
        for (int i = 0; i < days; ++i)
        {
          Schedule recNew = rec.Clone();
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                          0);
          if (rec.EndTime.Day > rec.StartTime.Day)
          {
            dtDay = dtDay.AddDays(1);
          }
          recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
          if (rec.EndTime.Day > rec.StartTime.Day)
          {
            dtDay = dtDay.AddDays(-1);
          }
          recNew.Series = true;
          if (recNew.StartTime >= DateTime.Now)
          {
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
      {
        for (int i = 0; i < days; ++i)
        {
          if (WeekEndTool.IsWorkingDay(dtDay.DayOfWeek))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                            0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.ScheduleType == (int)ScheduleRecordingType.Weekends)
      {
        IList<Program> progList = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName,
                                                              rec.ReferencedChannel());

        foreach (Program prog in progList)
        {
          if ((rec.IsRecordingProgram(prog, false)) &&
              (WeekEndTool.IsWeekend(prog.StartTime.DayOfWeek)))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;

            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);
          }
        }
        return recordings;
      }
      if (rec.ScheduleType == (int)ScheduleRecordingType.Weekly)
      {
        for (int i = 0; i < days; ++i)
        {
          if ((dtDay.DayOfWeek == rec.StartTime.DayOfWeek) && (dtDay.Date >= rec.StartTime.Date))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                            0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(1);
            }
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
            {
              dtDay = dtDay.AddDays(-1);
            }
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      IList<Program> programs;
      if (rec.ScheduleType == (int)ScheduleRecordingType.WeeklyEveryTimeOnThisChannel)
      {
        //Log.Debug("get {0} {1} EveryTimeOnThisChannel", rec.ProgramName, rec.ReferencedChannel().Name);
        programs = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, rec.ReferencedChannel());
        foreach (Program prog in programs)
        {
          // dtDay.DayOfWeek == rec.StartTime.DayOfWeek
          // Log.Debug("BusinessLayer.cs Program prog in programs WeeklyEveryTimeOnThisChannel: {0} {1} prog.StartTime.DayOfWeek == rec.StartTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.StartTime.DayOfWeek, rec.StartTime.DayOfWeek);
          if (prog.StartTime.DayOfWeek == rec.StartTime.DayOfWeek && rec.IsRecordingProgram(prog, false))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.IdChannel = prog.IdChannel;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
            {
              recNew.Canceled = recNew.StartTime;
            }
            recordings.Add(recNew);

            //Log.Debug("BusinessLayer.cs Added Recording WeeklyEveryTimeOnThisChannel: {0} {1} prog.StartTime.DayOfWeek == rec.StartTime.DayOfWeek {2} == {3}", rec.ProgramName, rec.ReferencedChannel().Name, prog.StartTime.DayOfWeek, rec.StartTime.DayOfWeek);
          }
        }
        return recordings;
      }

      programs = rec.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel
                   ? layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName,
                                                 rec.ReferencedChannel())
                   : layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, null);
      foreach (Program prog in programs)
      {
        if (rec.IsRecordingProgram(prog, false))
        {
          Schedule recNew = rec.Clone();
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.IdChannel = prog.IdChannel;
          recNew.StartTime = prog.StartTime;
          recNew.EndTime = prog.EndTime;
          recNew.Series = true;
          if (rec.IsSerieIsCanceled(rec.GetSchedStartTimeForProg(prog), prog.IdChannel))
          {
            recNew.Canceled = recNew.StartTime;
          }
          recordings.Add(recNew);
        }
      }
      return recordings;
    }

    // Add schedules for importing from xml
    public Schedule AddSchedule(int idChannel, string programName, DateTime startTime, DateTime endTime,
                                int scheduleType)
    {
      Schedule schedule = GetSchedule(idChannel, programName, startTime, endTime, scheduleType);
      if (schedule != null)
      {
        return schedule;
      }
      Schedule newSchedule = new Schedule(idChannel, programName, startTime, endTime);
      return newSchedule;
    }

    // Get schedules to import from xml
    public Schedule GetSchedule(int idChannel, string programName, DateTime startTime, DateTime endTime,
                                int scheduleType)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Schedule));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddConstraint(Operator.Equals, "programName", programName);
      sb.AddConstraint(Operator.Equals, "startTime", startTime);
      sb.AddConstraint(Operator.Equals, "endTime", endTime);
      sb.AddConstraint(Operator.Equals, "scheduleType", scheduleType);
      SqlStatement stmt = sb.GetStatement(true);
      Log.Info(stmt.Sql);
      IList<Schedule> schedules = ObjectFactory.GetCollection<Schedule>(stmt.Execute());
      if (schedules == null)
      {
        return null;
      }
      if (schedules.Count == 0)
      {
        return null;
      }
      return schedules[0];
    }

    #endregion

    #region recordings

    public Recording GetRecordingByFileName(string fileName)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Recording));
      sb.AddConstraint(Operator.Equals, "fileName", fileName);
      sb.SetRowLimit(1);
      SqlStatement stmt = sb.GetStatement(true);
      IList<Recording> recordings = ObjectFactory.GetCollection<Recording>(stmt.Execute());
      if (recordings.Count == 0)
      {
        return null;
      }
      return recordings[0];
    }

    #endregion

    #region channelgroups

    public RadioChannelGroup GetRadioChannelGroupByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (RadioChannelGroup));
      sb.AddConstraint(Operator.Equals, "groupName", name);
      SqlStatement stmt = sb.GetStatement(true);
      IList<RadioChannelGroup> groups = ObjectFactory.GetCollection<RadioChannelGroup>(stmt.Execute());
      if (groups == null)
      {
        return null;
      }
      if (groups.Count == 0)
      {
        return null;
      }
      return groups[0];
    }

    // Get group to import from xml
    public ChannelGroup GetGroupByName(string aGroupName)
    {
      return GetGroupByName(aGroupName, -1);
    }

    public ChannelGroup GetGroupByName(string aGroupName, int aSortOrder)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (ChannelGroup));
      sb.AddConstraint(Operator.Like, "groupName", "%" + aGroupName + "%");
      // use like here since the user might have changed the casing
      if (aSortOrder > -1)
      {
        sb.AddConstraint(Operator.Equals, "sortOrder", aSortOrder);
      }
      SqlStatement stmt = sb.GetStatement(true);
      Log.Debug(stmt.Sql);
      IList<ChannelGroup> groups = ObjectFactory.GetCollection<ChannelGroup>(stmt.Execute());
      if (groups == null)
      {
        return null;
      }
      if (groups.Count == 0)
      {
        return null;
      }
      return groups[0];
    }

    #endregion

    #region EPG Updating

    //string _titleTemplate;
    //string _descriptionTemplate;
    //string _epgLanguages;

    #endregion

    #region SoftwareEncoders

    public IList<SoftwareEncoder> GetSofwareEncodersVideo()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (SoftwareEncoder));
      sb.AddConstraint(Operator.Equals, "type", 0);
      sb.AddOrderByField(true, "priority");
      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<SoftwareEncoder>(stmt.Execute());
    }

    public IList<SoftwareEncoder> GetSofwareEncodersAudio()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (SoftwareEncoder));
      sb.AddConstraint(Operator.Equals, "type", 1);
      sb.AddOrderByField(true, "priority");
      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<SoftwareEncoder>(stmt.Execute());
    }

    #endregion

    public bool IsChannelMappedToCard(Channel dbChannel, Card card, bool forEpg)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (ChannelMap));
      SqlStatement origStmt = sb.GetStatement(true);
      string sql = "select cm.* from ChannelMap cm where cm.idChannel =" +
                   dbChannel.IdChannel + " and cm.idCard=" + card.IdCard + (forEpg ? "" : " and cm.epgOnly=0");
      SqlStatement statement = new SqlStatement(StatementType.Select, origStmt.Command, sql,
                                                typeof (Channel));
      IList<ChannelMap> maps = ObjectFactory.GetCollection<ChannelMap>(statement.Execute());
      return maps != null && maps.Count > 0;
    }
  }
}