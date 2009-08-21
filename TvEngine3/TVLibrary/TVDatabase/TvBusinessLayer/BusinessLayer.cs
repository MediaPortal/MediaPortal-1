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
using TvLibrary.Interfaces;
using TvLibrary.Log;
using StatementType = Gentle.Framework.StatementType;

#endregion

namespace TvDatabase
{
  public class TvBusinessLayer
  {
    #region vars

    private readonly object SingleInsert = new object();

    // maximum hours to keep old program info
    private int _EpgKeepDuration = 0;

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
                              0);
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Card));
      sb.AddConstraint("enabled=1");
      sb.AddOrderByField(false, "priority");
      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Card>(stmt.Execute());
    }

    #endregion

    #region channels

    // This is really needed
    public Channel AddNewChannel(string name)
    {
      Channel newChannel = new Channel(name, false, false, 0, new DateTime(2000, 1, 1), false, new DateTime(2000, 1, 1),
                                       -1, true, "", true, name);
      return newChannel;
    }

    public Channel AddChannel(string provider, string name)
    {
      Channel channel = GetChannelByName(provider, name);
      if (channel != null)
      {
        channel.Name = name;
        return channel;
      }

      return AddNewChannel(name);
    }

    public ChannelGroup CreateGroup(string groupName)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
      sb.AddConstraint(Operator.Like, "groupName", "%" + groupName + "%");
      SqlStatement stmt = sb.GetStatement(true);
      IList<ChannelGroup> groups = ObjectFactory.GetCollection<ChannelGroup>(stmt.Execute());
      ChannelGroup group;
      if (groups.Count == 0)
      {
        group = new ChannelGroup(groupName, 9999);
        group.Persist();
      }
      else
      {
        group = groups[0];
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

    public TuningDetail GetChannel(DVBBaseChannel channel)
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
      else // must be ATSCChannel  or AnalogChannel
      {
        channelType = 1;
      }

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", channel.Name);
      sb.AddConstraint(Operator.Equals, "provider", channel.Provider);
      sb.AddConstraint(Operator.Equals, "networkId", channel.NetworkId);
      sb.AddConstraint(Operator.Equals, "transportId", channel.TransportId);
      sb.AddConstraint(Operator.Equals, "serviceId", channel.ServiceId);
      sb.AddConstraint(Operator.Equals, "channelType", channelType);

      SqlStatement stmt = sb.GetStatement(true);
      IList<TuningDetail> channels = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());
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

    public TuningDetail GetChannel(string provider, string name, int serviceId)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", name);
      sb.AddConstraint(Operator.Equals, "provider", provider);
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
      return detail;
    }


    public Channel GetChannel(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TuningDetail));
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

    public TuningDetail GetAtscChannel(ATSCChannel channel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", channel.Name);
      sb.AddConstraint(Operator.Equals, "provider", channel.Provider);
      sb.AddConstraint(Operator.Equals, "networkId", channel.MajorChannel);
      sb.AddConstraint(Operator.Equals, "transportId", channel.MinorChannel);
      sb.AddConstraint(Operator.Equals, "serviceId", channel.ServiceId);

      SqlStatement stmt = sb.GetStatement(true);
      IList<TuningDetail> channels = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());
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

    public IList<Channel> GetChannelsByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "name", name);
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
      return channels;
    }

    public Channel GetChannelByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "name", name);
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

    public Channel GetChannelByName(string provider, string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", name);
      sb.AddConstraint(Operator.Equals, "provider", provider);
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
        sb = new SqlBuilder(StatementType.Select, typeof(Setting));
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
        sb = new SqlBuilder(StatementType.Select, typeof(Setting));
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
      CountryCollection collection = new CountryCollection();
      IList<TuningDetail> tuningDetails = channel.ReferringTuningDetail();
      for (int i = 0; i < tuningDetails.Count; ++i)
      {
        TuningDetail detail = tuningDetails[i];
        if (detail.ChannelType != channelType)
        {
          continue;
        }
        switch (detail.ChannelType)
        {
          case 0: //AnalogChannel
            AnalogChannel analogChannel = new AnalogChannel();
            analogChannel.ChannelNumber = detail.ChannelNumber;
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
            atscChannel.PcrPid = detail.PcrPid;
            atscChannel.PmtPid = detail.PmtPid;
            atscChannel.Provider = detail.Provider;
            atscChannel.ServiceId = detail.ServiceId;
            atscChannel.SymbolRate = detail.Symbolrate;
            atscChannel.TransportId = detail.TransportId;
            atscChannel.VideoPid = detail.VideoPid;
            atscChannel.AudioPid = detail.AudioPid;
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
            dvbcChannel.PcrPid = detail.PcrPid;
            dvbcChannel.PmtPid = detail.PmtPid;
            dvbcChannel.Provider = detail.Provider;
            dvbcChannel.ServiceId = detail.ServiceId;
            dvbcChannel.SymbolRate = detail.Symbolrate;
            dvbcChannel.TransportId = detail.TransportId;
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
            dvbsChannel.PcrPid = detail.PcrPid;
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
            dvbsChannel.VideoPid = detail.VideoPid;
            dvbsChannel.AudioPid = detail.AudioPid;
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
            dvbtChannel.PcrPid = detail.PcrPid;
            dvbtChannel.PmtPid = detail.PmtPid;
            dvbtChannel.Provider = detail.Provider;
            dvbtChannel.ServiceId = detail.ServiceId;
            dvbtChannel.TransportId = detail.TransportId;
            dvbtChannel.VideoPid = detail.VideoPid;
            dvbtChannel.AudioPid = detail.AudioPid;
            dvbtChannel.LogicalChannelNumber = detail.ChannelNumber;
            return dvbtChannel;
          case 7: //DVBIPChannel
            DVBIPChannel dvbipChannel = new DVBIPChannel();
            dvbipChannel.FreeToAir = detail.FreeToAir;
            dvbipChannel.IsRadio = detail.IsRadio;
            dvbipChannel.IsTv = detail.IsTv;
            dvbipChannel.Name = detail.Name;
            dvbipChannel.NetworkId = detail.NetworkId;
            dvbipChannel.PcrPid = detail.PcrPid;
            dvbipChannel.PmtPid = detail.PmtPid;
            dvbipChannel.Provider = detail.Provider;
            dvbipChannel.ServiceId = detail.ServiceId;
            dvbipChannel.TransportId = detail.TransportId;
            dvbipChannel.VideoPid = detail.VideoPid;
            dvbipChannel.AudioPid = detail.AudioPid;
            dvbipChannel.LogicalChannelNumber = detail.ChannelNumber;
            dvbipChannel.Url = detail.Url;
            return dvbipChannel;
        }
      }
      return null;
    }

    public List<IChannel> GetTuningChannelByName(Channel channel)
    {
      List<IChannel> tvChannels = new List<IChannel>();
      CountryCollection collection = new CountryCollection();
      IList<TuningDetail> tuningDetails = channel.ReferringTuningDetail();
      for (int i = 0; i < tuningDetails.Count; ++i)
      {
        TuningDetail detail = tuningDetails[i];
        switch (detail.ChannelType)
        {
          case 0: //AnalogChannel
            AnalogChannel analogChannel = new AnalogChannel();
            analogChannel.ChannelNumber = detail.ChannelNumber;
            analogChannel.Country = collection.Countries[detail.CountryId];
            analogChannel.Frequency = detail.Frequency;
            analogChannel.IsRadio = detail.IsRadio;
            analogChannel.IsTv = detail.IsTv;
            analogChannel.Name = channel.Name; //detail.Name;
            analogChannel.TunerSource = (TunerInputType)detail.TuningSource;
            analogChannel.VideoSource = (AnalogChannel.VideoInputType)detail.VideoSource;
            analogChannel.AudioSource = (AnalogChannel.AudioInputType)detail.AudioSource;
            tvChannels.Add(analogChannel);
            break;
          case 1: //ATSCChannel
            ATSCChannel atscChannel = new ATSCChannel();
            atscChannel.MajorChannel = detail.MajorChannel;
            atscChannel.MinorChannel = detail.MinorChannel;
            atscChannel.PhysicalChannel = detail.ChannelNumber;
            atscChannel.FreeToAir = detail.FreeToAir;
            atscChannel.Frequency = detail.Frequency;
            atscChannel.IsRadio = detail.IsRadio;
            atscChannel.IsTv = detail.IsTv;
            atscChannel.Name = channel.Name; //detail.Name;
            atscChannel.NetworkId = detail.NetworkId;
            atscChannel.PcrPid = detail.PcrPid;
            atscChannel.PmtPid = detail.PmtPid;
            atscChannel.Provider = detail.Provider;
            atscChannel.ServiceId = detail.ServiceId;
            atscChannel.SymbolRate = detail.Symbolrate;
            atscChannel.TransportId = detail.TransportId;
            atscChannel.AudioPid = detail.AudioPid;
            atscChannel.VideoPid = detail.VideoPid;
            atscChannel.ModulationType = (ModulationType)detail.Modulation;
            tvChannels.Add(atscChannel);
            break;
          case 2: //DVBCChannel
            DVBCChannel dvbcChannel = new DVBCChannel();
            dvbcChannel.ModulationType = (ModulationType)detail.Modulation;
            dvbcChannel.FreeToAir = detail.FreeToAir;
            dvbcChannel.Frequency = detail.Frequency;
            dvbcChannel.IsRadio = detail.IsRadio;
            dvbcChannel.IsTv = detail.IsTv;
            dvbcChannel.Name = channel.Name; //detail.Name;
            dvbcChannel.NetworkId = detail.NetworkId;
            dvbcChannel.PcrPid = detail.PcrPid;
            dvbcChannel.PmtPid = detail.PmtPid;
            dvbcChannel.Provider = detail.Provider;
            dvbcChannel.ServiceId = detail.ServiceId;
            dvbcChannel.SymbolRate = detail.Symbolrate;
            dvbcChannel.TransportId = detail.TransportId;
            tvChannels.Add(dvbcChannel);
            break;
          case 3: //DVBSChannel
            DVBSChannel dvbsChannel = new DVBSChannel();
            dvbsChannel.DisEqc = (DisEqcType)detail.Diseqc;
            dvbsChannel.Polarisation = (Polarisation)detail.Polarisation;
            dvbsChannel.SwitchingFrequency = detail.SwitchingFrequency;
            dvbsChannel.FreeToAir = detail.FreeToAir;
            dvbsChannel.Frequency = detail.Frequency;
            dvbsChannel.IsRadio = detail.IsRadio;
            dvbsChannel.IsTv = detail.IsTv;
            dvbsChannel.Name = channel.Name; //detail.Name;
            dvbsChannel.NetworkId = detail.NetworkId;
            dvbsChannel.PcrPid = detail.PcrPid;
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
            dvbsChannel.VideoPid = detail.VideoPid;
            dvbsChannel.AudioPid = detail.AudioPid;
            tvChannels.Add(dvbsChannel);
            break;
          case 4: //DVBTChannel
            DVBTChannel dvbtChannel = new DVBTChannel();
            dvbtChannel.BandWidth = detail.Bandwidth;
            dvbtChannel.FreeToAir = detail.FreeToAir;
            dvbtChannel.Frequency = detail.Frequency;
            dvbtChannel.IsRadio = detail.IsRadio;
            dvbtChannel.IsTv = detail.IsTv;
            dvbtChannel.Name = channel.Name; //detail.Name;
            dvbtChannel.NetworkId = detail.NetworkId;
            dvbtChannel.PcrPid = detail.PcrPid;
            dvbtChannel.PmtPid = detail.PmtPid;
            dvbtChannel.Provider = detail.Provider;
            dvbtChannel.ServiceId = detail.ServiceId;
            dvbtChannel.TransportId = detail.TransportId;
            dvbtChannel.LogicalChannelNumber = detail.ChannelNumber;
            dvbtChannel.VideoPid = detail.VideoPid;
            dvbtChannel.AudioPid = detail.AudioPid;
            tvChannels.Add(dvbtChannel);
            break;
          case 7: //DVBIPChannel
            DVBIPChannel dvbipChannel = new DVBIPChannel();
            dvbipChannel.FreeToAir = detail.FreeToAir;
            dvbipChannel.IsRadio = detail.IsRadio;
            dvbipChannel.IsTv = detail.IsTv;
            dvbipChannel.Name = detail.Name;
            dvbipChannel.NetworkId = detail.NetworkId;
            dvbipChannel.PcrPid = detail.PcrPid;
            dvbipChannel.PmtPid = detail.PmtPid;
            dvbipChannel.Provider = detail.Provider;
            dvbipChannel.ServiceId = detail.ServiceId;
            dvbipChannel.TransportId = detail.TransportId;
            dvbipChannel.VideoPid = detail.VideoPid;
            dvbipChannel.AudioPid = detail.AudioPid;
            dvbipChannel.LogicalChannelNumber = detail.ChannelNumber;
            dvbipChannel.Url = detail.Url;
            tvChannels.Add(dvbipChannel);
            break;
        }
      }
      return tvChannels;
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
    /// Gets a list of tv channels sorted by their group
    /// </summary>
    /// <returns>a list of TVDatabase Channels</returns>
    public List<Channel> GetTVGuideChannelsForGroup(int groupID)
    {
      SqlBuilder sb1 = new SqlBuilder(StatementType.Select, typeof(Channel));
      SqlStatement stmt1 = sb1.GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(stmt1.StatementType, stmt1.Command,
                                                    String.Format(
                                                      "select c.* from Channel c join GroupMap g on c.idChannel=g.idChannel where visibleInGuide = 1 and isTv = 1 and idGroup = '{0}' order by g.idGroup, g.sortOrder",
                                                      groupID), typeof(Channel));
      return ObjectFactory.GetCollection<Channel>(ManualJoinSQL.Execute()) as List<Channel>;
    }

    public IList<Channel> GetAllRadioChannels()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
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
      int pcrPid = -1;
      int pmtPid = -1;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int videoPid = -1;
      int audioPid = -1;
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
        channelNumber = dvbsChannel.LogicalChannelNumber > 999 ? channel.IdChannel : dvbsChannel.LogicalChannelNumber;
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
        pcrPid = dvbChannel.PcrPid;
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
        videoPid = dvbChannel.VideoPid;
        audioPid = dvbChannel.AudioPid;
      }

      TuningDetail detail = new TuningDetail(channel.IdChannel, channelName, provider,
                                             channelType, channelNumber, (int)channelFrequency, country, isRadio, isTv,
                                             networkId, transportId, serviceId, pmtPid, freeToAir,
                                             modulation, polarisation, symbolRate, diseqc, switchFrequency,
                                             bandwidth, majorChannel, minorChannel, pcrPid, videoInputType,
                                             audioInputType, isVCRSignal, tunerSource, videoPid, audioPid, band, satIndex,
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
      int pcrPid = -1;
      int pmtPid = -1;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int videoPid = -1;
      int audioPid = -1;
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
        channelNumber = dvbsChannel.LogicalChannelNumber > 999 ? channel.IdChannel : dvbsChannel.LogicalChannelNumber;
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
        pcrPid = dvbChannel.PcrPid;
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
        videoPid = dvbChannel.VideoPid;
        audioPid = dvbChannel.AudioPid;
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
      detail.PcrPid = pcrPid;
      detail.VideoSource = videoInputType;
      detail.AudioSource = audioInputType;
      detail.IsVCRSignal = isVCRSignal;
      detail.TuningSource = tunerSource;
      detail.VideoPid = videoPid;
      detail.AudioPid = audioPid;
      detail.Band = band;
      detail.SatIndex = satIndex;
      detail.InnerFecRate = innerFecRate;
      detail.Pilot = pilot;
      detail.RollOff = rollOff;
      detail.Url = url;
      detail.Persist();
      return detail;
    }

    public TuningDetail AddWebStreamTuningDetails(Channel channel, string url, int bitrate)
    {
      string channelName = channel.Name;
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
      const int pcrPid = -1;
      const int pmtPid = -1;
      const int networkId = -1;
      const int serviceId = -1;
      const int transportId = -1;
      const int minorChannel = -1;
      const int majorChannel = -1;
      const string provider = "";
      const int channelType = 5;
      const int videoPid = -1;
      const int audioPid = -1;
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
                                             bandwidth, majorChannel, minorChannel, pcrPid, videoInputType,
                                             audioInputType, isVCRSignal, tunerSource, videoPid, audioPid, band, satIndex,
                                             innerFecRate, pilot, rollOff, url, bitrate);
      detail.Persist();
      return detail;
    }

    #endregion

    #region linkage map

    public IList<ChannelLinkageMap> GetLinkagesForChannel(Channel channel)
    {
      int idChannel = -1;
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(ChannelLinkageMap));
      sb.AddConstraint(Operator.Equals, "idLinkedChannel", channel.IdChannel);
      SqlStatement stmt = sb.GetStatement(true);
      IList<ChannelLinkageMap> links = ObjectFactory.GetCollection<ChannelLinkageMap>(stmt.Execute());
      if (links != null)
      {
        if (links.Count > 0)
        {
          ChannelLinkageMap map = links[0];
          idChannel = map.ReferringPortalChannel().IdChannel;
        }
      }
      if (idChannel == -1)
      {
        idChannel = channel.IdChannel;
      }
      sb = new SqlBuilder(StatementType.Select, typeof(ChannelLinkageMap));
      sb.AddConstraint(Operator.Equals, "idPortalChannel", idChannel);
      stmt = sb.GetStatement(true);
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
            return original.Replace("'", "\\'");
        }
        else
        {
            return original.Replace("'", "''");
        }
    }

    public void RemoveOldPrograms()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
      DateTime dtYesterday = DateTime.Now.AddHours(-EpgKeepDuration);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(String.Format("endTime < '{0}'", dtYesterday.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public void RemoveOldPrograms(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
      DateTime dtToKeep = DateTime.Now.AddHours(-EpgKeepDuration);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddConstraint(String.Format("endTime < '{0}'", dtToKeep.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public void RemoveAllPrograms(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public IList<Program> GetOnairNow()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(String.Format("startTime>='{0}'", startTime.ToString(GetDateTimeString(), mmddFormat)));
      sb.AddOrderByField(true, "startTime");

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection<Program>(stmt.Execute());
    }

    public IList<Program> GetPrograms(Channel channel, DateTime startTime, DateTime endTime)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));

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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));

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
            MySQLCmd.Parameters.Add("?startTime", MySqlDbType.Datetime).Value = startTime;
            MySQLCmd.Parameters.Add("?endTime", MySqlDbType.Datetime).Value = endTime;
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

        Program p = new Program(Convert.ToInt32(prog["idChannel"]),
                                Convert.ToDateTime(prog["startTime"]),
                                Convert.ToDateTime(prog["endTime"]),
                                Convert.ToString(prog["title"]),
                                Convert.ToString(prog["description"]),
                                Convert.ToString(prog["genre"]),
                                Convert.ToBoolean(prog["notify"]),
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
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof(Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(), typeof(Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    public DateTime GetNewestProgramForChannel(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));

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
      sb.AddConstraint(Operator.Like, "title", "%" + title + "%");
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
        "where ((EndTime > '{0}' and EndTime < '{1}') or (StartTime >= '{0}' and StartTime <= '{1}') or (StartTime <= '{0}' and EndTime >= '{1}')) and title like '%{2}%' and c.visibleInGuide = 1 order by startTime",
        StartTimeString, EndTimeString, EscapeSQLString(title)
      );
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof(Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(), typeof(Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    public IList<Program> SearchMinimalPrograms(DateTime startTime, DateTime endTime, string programName,
                                                Channel channel)
    {
      return channel != null
               ? GetProgramsByTitle(channel, startTime, endTime, programName)
               : GetProgramsByTitle(startTime, endTime, programName);
    }

    public IList<string> GetGenres()
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

    public IList<Program> SearchProgramsPerGenre(string currentGenre, string searchCriteria)
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
      SqlSelectCommand.Append("and c.visibleInGuide = 1 order by title, startTime");
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof(Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(), typeof(Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    public IList<Program> SearchPrograms(string searchCriteria)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      StringBuilder SqlSelectCommand = new StringBuilder();
      SqlSelectCommand.Append("select p.* from Program p inner join Channel c on c.idChannel = p.idChannel ");
      SqlSelectCommand.AppendFormat("where endTime > '{0}' ", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));
      if (searchCriteria.Length > 0)
      {
        SqlSelectCommand.AppendFormat("and title like '{0}%' ", EscapeSQLString(searchCriteria));
      }
      SqlSelectCommand.Append("and c.visibleInGuide = 1 order by title, startTime");
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof(Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(), typeof(Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    public IList<Program> SearchProgramsByDescription(string searchCriteria)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      StringBuilder SqlSelectCommand = new StringBuilder();
      SqlSelectCommand.Append("select p.* from Program p inner join Channel c on c.idChannel = p.idChannel ");
      SqlSelectCommand.AppendFormat("where endTime > '{0}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));
      if (searchCriteria.Length > 0)
      {
          SqlSelectCommand.AppendFormat("and description like '{0}%' ", EscapeSQLString(searchCriteria));
      }
      SqlSelectCommand.Append("and c.visibleInGuide = 1 order by description, startTime");
      SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof(Program)).GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(), typeof(Program));
      return ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
    }

    private static string BuildCommandTextMiniGuide(string aProvider, ICollection<Channel> aEpgChannelList)
    {
      string completeStatement;

      // no channel = no EPG but we need a valid command text
      if (aEpgChannelList.Count < 1)
      {
        completeStatement = "SELECT * FROM program WHERE 0=1";
      }
      else
      {
        StringBuilder sbSelect = new StringBuilder();
        if (aProvider == "mysql")
        {
          foreach (Channel ch in aEpgChannelList)
          {
            sbSelect.AppendFormat(
              "(SELECT idChannel,idProgram,starttime,endtime,title,episodeName,seriesNum,episodeNum,episodePart FROM program WHERE idChannel={0} AND (Program.endtime >= NOW()) order by starttime limit 2)  UNION  ",
              ch.IdChannel);
          }

          completeStatement = sbSelect.ToString();
          completeStatement = completeStatement.Remove(completeStatement.Length - 8); // Remove trailing UNION
        }
        else
        {
          //foreach (Channel ch in aEpgChannelList)
          //  sbSelect.AppendFormat("(SELECT TOP 2 idChannel,idProgram,starttime,endtime,title FROM program WHERE idChannel={0} AND (Program.endtime >= getdate()))  UNION ALL  ", ch.IdChannel);

          //completeStatement = sbSelect.ToString();
          //completeStatement = completeStatement.Remove(completeStatement.Length - 12); // Remove trailing UNION ALL
          //completeStatement = completeStatement + " ORDER BY idChannel, startTime";   // MSSQL does not support order by in single UNION selects

          sbSelect.Append("SELECT idChannel,idProgram,starttime,endtime,title,episodeName,seriesNum,episodeNum,episodePart FROM Program ");
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
                                          string.Empty, nowTitle, -1, nowidProgram,episodeName,string.Empty,
                                          seriesNum,string.Empty,episodeNum,string.Empty,
                                          episodePart,string.Empty);
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
                                            nextidProgram,episodeName,nextEpisodeName,seriesNum,nextSeriesNum,
                                            episodeNum,nextEpisodeNum,episodePart,nextEpisodePart);
              progList[idChannel] = p;
            }
            else
            {
              // no "next" info because of holes in EPG data - we want the "now" info nevertheless
              NowAndNext p = new NowAndNext(idChannel, nowStart, nowEnd, nowTitle, string.Empty, nowidProgram, -1,
                                            string.Empty,string.Empty,string.Empty,string.Empty,string.Empty,
                                            string.Empty,string.Empty,string.Empty);
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
      public List<Program> ProgramList;
      public string ConnectString;
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
      try
      {
        IGentleProvider prov = ProviderFactory.GetDefaultProvider();
        string provider = prov.Name.ToLowerInvariant();
        string defaultConnectString = prov.ConnectionString;
        // Gentle.Framework.ProviderFactory.GetDefaultProvider().ConnectionString;
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
        param.ProgramList = aProgramList;
        param.ConnectString = defaultConnectString;
        param.SleepTime = sleepTime;
        Thread importThread;

        // TODO: /!\ Temporarily turn of index rebuilding and other stuff that would speed up bulk inserts
        switch (provider)
        {
          case "mysql":
            importThread = new Thread(ImportMySqlThread);
            importThread.Priority = aThreadPriority;
            importThread.Name = "MySQL EPG importer";
            importThread.Start(param);
            break;
          case "sqlserver":
            importThread = new Thread(ImportSqlServerThread);
            importThread.Priority = aThreadPriority;
            importThread.Name = "MSSQL EPG importer";
            importThread.Start(param);
            break;
          default:
            Log.Info("BusinessLayer: InsertPrograms unknown provider - {0}", provider);
            break;
        }

        return aProgramList.Count;
      }
      catch (Exception ex)
      {
        Log.Error("BusinessLayer: InsertPrograms error - {0}, {1}", ex.Message, ex.StackTrace);
        return 0;
      }
    }

    private void ImportMySqlThread(object aImportParam)
    {
      lock (SingleInsert)
      {
        ImportParams MyParams = (ImportParams)aImportParam;
        InsertMySql(MyParams);
      }
    }

    private void ImportSqlServerThread(object aImportParam)
    {
      lock (SingleInsert)
      {
        ImportParams MyParams = (ImportParams)aImportParam;
        InsertSqlServer(MyParams);
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

    private static void InsertMySql(ImportParams aImportParam)
    {
      MySqlTransaction transact = null;
      try
      {
        using (MySqlConnection connection = new MySqlConnection(aImportParam.ConnectString))
        {
          connection.Open();
          transact = connection.BeginTransaction();
          ExecuteMySqlCommand(aImportParam.ProgramList, connection, transact, aImportParam.SleepTime);
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
          Log.Info("BusinessLayer: OptimizeMySql unsuccessful - ROLLBACK - {0}, {1}", ex2.Message, ex2.StackTrace);
        }
        Log.Info("BusinessLayer: InsertMySql caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    private static void InsertSqlServer(ImportParams aImportParam)
    {
      SqlTransaction transact = null;
      try
      {
        using (SqlConnection connection = new SqlConnection(aImportParam.ConnectString))
        {
          connection.Open();
          transact = connection.BeginTransaction();
          ExecuteSqlServerCommand(aImportParam.ProgramList, connection, transact, aImportParam.SleepTime);
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
          Log.Info("BusinessLayer: OptimizeMySql unsuccessful - ROLLBACK - {0}, {1}", ex2.Message, ex2.StackTrace);
        }
        Log.Info("BusinessLayer: InsertSqlServer caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    #endregion

    #region SQL Builder

    private static void ExecuteMySqlCommand(IEnumerable<Program> aProgramList, MySqlConnection aConnection,
                                            MySqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      MySqlCommand sqlCmd = new MySqlCommand();
      List<Program> currentInserts = new List<Program>(aProgramList);

      sqlCmd.CommandText =
        "INSERT INTO Program (idChannel, startTime, endTime, title, description, seriesNum, episodeNum, genre, originalAirDate, classification, starRating, notify, parentalRating, episodeName, episodePart) VALUES (?idChannel, ?startTime, ?endTime, ?title, ?description, ?seriesNum, ?episodeNum, ?genre, ?originalAirDate, ?classification, ?starRating, ?notify, ?parentalRating, ?episodeName, ?episodePart)";

      sqlCmd.Parameters.Add("?idChannel", MySqlDbType.Int32);
      sqlCmd.Parameters.Add("?startTime", MySqlDbType.Datetime);
      sqlCmd.Parameters.Add("?endTime", MySqlDbType.Datetime);
      sqlCmd.Parameters.Add("?title", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?description", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?seriesNum", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?episodeNum", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?genre", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?originalAirDate", MySqlDbType.Datetime);
      sqlCmd.Parameters.Add("?classification", MySqlDbType.VarChar);
      sqlCmd.Parameters.Add("?starRating", MySqlDbType.Int32);
      sqlCmd.Parameters.Add("?notify", MySqlDbType.Bit);
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
        Log.Info("BusinessLayer: ExecuteMySqlCommand - Prepare caused an Exception - {0}", ex.Message);
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
        sqlCmd.Parameters["?notify"].Value = prog.Notify;
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
              Log.Info("BusinessLayer: ExecuteMySqlCommand caused a MySqlException - {0}, {1} {2}", myex.Message,
                       myex.Number, myex.HelpLink);
              break;
          }
        }
        catch (Exception ex)
        {
          Log.Info("BusinessLayer: ExecuteMySqlCommand caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
        }
      }
      return;
    }

    private static void ExecuteSqlServerCommand(IEnumerable<Program> aProgramList, SqlConnection aConnection,
                                                SqlTransaction aTransaction, int aDelay)
    {
      int aCounter = 0;
      SqlCommand sqlCmd = new SqlCommand();
      List<Program> currentInserts = new List<Program>(aProgramList);

      sqlCmd.CommandText =
        "INSERT INTO Program (idChannel, startTime, endTime, title, description, seriesNum, episodeNum, genre, originalAirDate, classification, starRating, notify, parentalRating, episodeName, episodePart) VALUES (@idChannel, @startTime, @endTime, @title, @description, @seriesNum, @episodeNum, @genre, @originalAirDate, @classification, @starRating, @notify, @parentalRating, @episodeName, @episodePart)";

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
      sqlCmd.Parameters.Add("notify", SqlDbType.Bit);
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
        Log.Info("BusinessLayer: ExecuteSqlServerCommand - Prepare caused an Exception - {0}", ex.Message);
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
        sqlCmd.Parameters["notify"].Value = prog.Notify;
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
              Log.Info("BusinessLayer: InsertSqlServer caused a SqlException - {0}, {1} {2}", msex.Message, msex.Number,
                       msex.HelpLink);
              break;
          }
        }
        catch (Exception ex)
        {
          Log.Error("BusinessLayer: InsertSqlServer error - {0}, {1}", ex.Message, ex.StackTrace);
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
          Schedule overlapping;
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
        Schedule overlapping;
        if (!AssignSchedulesToCard(newEpisode, cardSchedules, out overlapping))
        {
          Log.Info("GetConflictingSchedules: newEpisode can not be assigned to a card = " + newEpisode);
          conflicts.Add(overlapping);
        }

        /*Log.Info("GetConflictingSchedules: newEpisode = " + newEpisode.ToString());
        foreach (Schedule schedule in schedulesList)
        {
          if (DateTime.Now > schedule.EndTime) continue;
          if (schedule.Canceled != Schedule.MinSchedule) continue;
          if (newEpisode.IdSchedule == schedule.IdSchedule) continue;

          List<Schedule> otherEpisodes = GetRecordingTimes(schedule);
          foreach (Schedule otherEpisode in otherEpisodes)
          {
            if (DateTime.Now > otherEpisode.EndTime) continue;
            if (otherEpisode.Canceled != Schedule.MinSchedule) continue;
            if (newEpisode.IdSchedule == otherEpisode.IdSchedule) continue;

            if (newEpisode.IsOverlapping(otherEpisode))
            {
              Log.Info("GetConflictingSchedules: overlapping -> " + newEpisode.ToString() + "   with   " + otherEpisode.ToString());
              if (!AssignSchedulesToCard(otherEpisode, cardSchedules))
              {
                Log.Info("GetConflictingSchedules: conflicts.Add = " + otherEpisode.ToString());
                conflicts.Add(otherEpisode);
              }
            }
          }
        }*/
      }
      return conflicts;
    }

    private static bool AssignSchedulesToCard(Schedule schedule, List<Schedule>[] cardSchedules,
                                              out Schedule overlappingSchedule)
    {
      overlappingSchedule = null;
      Log.Info("AssignSchedulesToCard: schedule = " + schedule);
      IList<Card> cards = Card.ListAll();
      bool assigned = false;
      int count = 0;
      foreach (Card card in cards)
      {
        if (card.canViewTvChannel(schedule.IdChannel))
        {
          // checks if any schedule assigned to this cards overlaps current parsed schedule
          bool free = true;
          foreach (Schedule assignedSchedule in cardSchedules[count])
          {
            Log.Info("AssignSchedulesToCard: card {0}, ID = {1} has schedule = " + assignedSchedule, count, card.IdCard);
            if (schedule.IsOverlapping(assignedSchedule))
            {
              if (!(schedule.isSameTransponder(assignedSchedule) && card.supportSubChannels))
              {
                overlappingSchedule = assignedSchedule;
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

      if (rec.ScheduleType == (int) ScheduleRecordingType.WorkingDays)
      {
        WeekEndTool weekEndTool = Setting.GetWeekEndTool();
        for (int i = 0; i < days; ++i)
        {
          if (weekEndTool.IsWorkingDay(dtDay.DayOfWeek))
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

        WeekEndTool weekEndTool = Setting.GetWeekEndTool();
        foreach (Program prog in progList)
        {
          if ((rec.IsRecordingProgram(prog, false)) &&
              (weekEndTool.IsWeekend(prog.StartTime.DayOfWeek)) )
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


      IList<Program> programs = rec.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel
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
          if (rec.IsSerieIsCanceled(recNew.StartTime))
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Schedule));
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Recording));
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(RadioChannelGroup));
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
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
  }
}