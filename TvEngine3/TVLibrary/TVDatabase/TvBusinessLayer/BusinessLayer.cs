using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using MySql.Data.MySqlClient;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Channels;
using TvLibrary.Log;
using TvLibrary;
using DirectShowLib;
using DirectShowLib.BDA;
using System.Threading;
using System.Diagnostics;

namespace TvDatabase
{
  public class TvBusinessLayer
  {
    #region vars
    object SingleInsert = new object();
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
      Card newCard = new Card(devicePath, name, 1, true, new DateTime(2000, 1, 1), "", server.IdServer, true, 0, "", 0, 1);
      newCard.Persist();
      return newCard;
    }

    public IList Cards
    {
      get
      {
        return Card.ListAll();
      }
    }

    public void DeleteCard(Card card)
    {
      card.Remove();
    }

    public Card GetCardByName(string name)
    {
      IList cards = Cards;
      foreach (Card card in cards)
      {
        if (card.Name == name) return card;
      }
      return null;
    }

    public Card GetCardByDevicePath(string path)
    {
      IList cards = Cards;
      foreach (Card card in cards)
      {
        if (card.DevicePath == path) return card;
      }
      return null;
    }

    public IList ListAllEnabledCardsOrderedByPriority()
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Card));
      sb.AddConstraint("enabled=1");
      sb.AddOrderByField(false, "priority");
      SqlStatement stmt = sb.GetStatement(true);
      IList cards = ObjectFactory.GetCollection(typeof(Card), stmt.Execute());
      return cards;
    }
    #endregion

    #region channels
    // This is really needed
    public Channel AddNewChannel(string name)
    {
      Channel newChannel = new Channel(name, false, false, 0, new DateTime(2000, 1, 1), true, new DateTime(2000, 1, 1), -1, true, "", true, name);
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
      Channel newChannel = new Channel(name, false, false, 0, new DateTime(2000, 1, 1), true, new DateTime(2000, 1, 1), -1, true, "", true, name);
      return newChannel;
    }

    public void AddChannelToGroup(Channel channel, string groupName)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(ChannelGroup));
      sb.AddConstraint(Operator.Like, "groupName", groupName);
      SqlStatement stmt = sb.GetStatement(true);
      IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
      ChannelGroup group;
      if (groups.Count == 0)
      {
        group = new ChannelGroup(groupName, 9999);
        group.Persist();
      }
      else
      {
        group = (ChannelGroup)groups[0];
      }
      bool found = false;
      IList groupMaps = group.ReferringGroupMap();
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

    public void AddChannelToRadioGroup(Channel channel, RadioChannelGroup group)
    {
      bool found = false;
      IList groupMaps = group.ReferringRadioGroupMap();
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

    public IList Channels
    {
      get
      {
        return Channel.ListAll();
      }
    }

    public void DeleteChannel(Channel channel)
    {
      channel.Remove();
    }

    public TuningDetail GetChannel(DVBBaseChannel channel)
    {
      int channelType = 0;

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
      else // must be ATSCChannel  or AnalogChannel
      {
        channelType = 1;
      }

      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", channel.Name);
      sb.AddConstraint(Operator.Equals, "provider", channel.Provider);
      sb.AddConstraint(Operator.Equals, "networkId", channel.NetworkId);
      sb.AddConstraint(Operator.Equals, "transportId", channel.TransportId);
      sb.AddConstraint(Operator.Equals, "serviceId", channel.ServiceId);
      sb.AddConstraint(Operator.Equals, "channelType", channelType);

      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(TuningDetail), stmt.Execute());
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return (TuningDetail)channels[0];
    }

    public Channel GetChannel(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);

      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return (Channel)channels[0];
    }

    public Channel GetChannelByTuningDetail(int networkId, int transportId, int serviceId)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "networkId", networkId);
      sb.AddConstraint(Operator.Equals, "transportId", transportId);
      sb.AddConstraint(Operator.Equals, "serviceId", serviceId);

      SqlStatement stmt = sb.GetStatement(true);
      IList details = ObjectFactory.GetCollection(typeof(TuningDetail), stmt.Execute());

      if (details == null) return null;
      if (details.Count == 0) return null;
      TuningDetail detail = (TuningDetail)details[0];
      return detail.ReferencedChannel();
    }

    public TuningDetail GetAtscChannel(ATSCChannel channel)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", channel.Name);
      sb.AddConstraint(Operator.Equals, "provider", channel.Provider);
      sb.AddConstraint(Operator.Equals, "networkId", channel.MajorChannel);
      sb.AddConstraint(Operator.Equals, "transportId", channel.MinorChannel);
      sb.AddConstraint(Operator.Equals, "serviceId", channel.ServiceId);

      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(TuningDetail), stmt.Execute());
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return (TuningDetail)channels[0];
    }

    public IList GetChannelsByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "name", name);
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return channels;
    }

    public Channel GetChannelByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "name", name);
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return (Channel)channels[0];
    }

    public Channel GetChannelByName(string provider, string name)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(TuningDetail));
      sb.AddConstraint(Operator.Equals, "name", name);
      sb.AddConstraint(Operator.Equals, "provider", provider);
      SqlStatement stmt = sb.GetStatement(true);
      IList details = ObjectFactory.GetCollection(typeof(TuningDetail), stmt.Execute());
      if (details == null) return null;
      if (details.Count == 0) return null;
      TuningDetail detail = (TuningDetail)details[0];
      return detail.ReferencedChannel();
    }

    public Setting GetSetting(string tagName, string defaultValue)
    {
      if (defaultValue == null) return null;
      if (tagName == null) return null;
      if (tagName == "") return null;

      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Setting));
      sb.AddConstraint(Operator.Equals, "tag", tagName);
      SqlStatement stmt = sb.GetStatement(true);
      IList settingsFound = ObjectFactory.GetCollection(typeof(Setting), stmt.Execute());
      if (settingsFound.Count == 0)
      {
        Setting set = new Setting(tagName, defaultValue);
        set.Persist();
        return set;
      }
      return (Setting)settingsFound[0];
    }

    public Setting GetSetting(string tagName)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Setting));
      sb.AddConstraint(Operator.Equals, "tag", tagName);
      SqlStatement stmt = sb.GetStatement(true);
      IList settingsFound = ObjectFactory.GetCollection(typeof(Setting), stmt.Execute());
      if (settingsFound.Count == 0)
      {
        Setting set = new Setting(tagName, "");
        set.Persist();
        return set;
      }
      return (Setting)settingsFound[0];
    }

    public IChannel GetTuningChannelByType(Channel channel, int channelType)
    {
      CountryCollection collection = new CountryCollection();
      IList tuningDetails = channel.ReferringTuningDetail();
      for (int i = 0 ; i < tuningDetails.Count ; ++i)
      {
        TuningDetail detail = (TuningDetail)tuningDetails[i];
        if (detail.ChannelType != channelType) continue;
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
        }
      }
      return null;
    }

    public List<IChannel> GetTuningChannelByName(Channel channel)
    {
      List<IChannel> tvChannels = new List<IChannel>();
      CountryCollection collection = new CountryCollection();
      IList tuningDetails = channel.ReferringTuningDetail();
      for (int i = 0 ; i < tuningDetails.Count ; ++i)
      {
        TuningDetail detail = (TuningDetail)tuningDetails[i];
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
        }
      }
      return tvChannels;
    }

    public ChannelMap MapChannelToCard(Card card, Channel channel)
    {
      IList channelMaps = card.ReferringChannelMap();
      for (int i = 0 ; i < channelMaps.Count ; ++i)
      {
        ChannelMap map = (ChannelMap)channelMaps[i];
        if (map.IdChannel == channel.IdChannel && map.IdCard == card.IdCard) return map;
      }
      ChannelMap newMap = new ChannelMap(channel.IdChannel, card.IdCard);
      newMap.Persist();
      return newMap;
    }

    /// <summary>
    /// Gets a list of tv channels sorted by their group
    /// </summary>
    /// <returns>a list of TVDatabase Channels</returns>
    public List<Channel> GetTVGuideChannelsForGroup(int groupID)
    {
      List<Channel> ResultingChannelList = new List<Channel>();

      SqlBuilder sb1 = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
      SqlStatement stmt1 = sb1.GetStatement(true);
      SqlStatement ManualJoinSQL = new SqlStatement(stmt1.StatementType, stmt1.Command, String.Format("select c.* from Channel c join GroupMap g on c.idChannel=g.idChannel where visibleInGuide = 1 and isTv = 1 and idGroup = '{0}' order by g.idGroup, g.sortOrder", groupID), typeof(Channel));
      IList ChanList = ObjectFactory.GetCollection(typeof(Channel), ManualJoinSQL.Execute());

      foreach (Channel SingleChannel in ChanList)
      {
        ResultingChannelList.Add(SingleChannel);
      }

      return ResultingChannelList;
    }

    public IList GetAllRadioChannels()
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "isRadio", 1);
      SqlStatement stmt = sb.GetStatement(true);
      IList radioChannels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      if (radioChannels == null) return null;
      if (radioChannels.Count == 0) return null;
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
        if (dvbsChannel.LogicalChannelNumber > 999)
        {
          channelNumber = channel.IdChannel;
        }
        else channelNumber = dvbsChannel.LogicalChannelNumber;
        channelType = 3;
      }

      DVBTChannel dvbtChannel = tvChannel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.BandWidth;
        channelNumber = dvbtChannel.LogicalChannelNumber;
        channelType = 4;
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
                              bandwidth, majorChannel, minorChannel, pcrPid, videoInputType, tunerSource, videoPid, audioPid, band, satIndex, innerFecRate, pilot, rollOff, "", 0);
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
        if (dvbsChannel.LogicalChannelNumber > 999)
        {
          channelNumber = channel.IdChannel;
        }
        else channelNumber = dvbsChannel.LogicalChannelNumber;
        channelType = 3;
      }

      DVBTChannel dvbtChannel = tvChannel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.BandWidth;
        channelNumber = dvbtChannel.LogicalChannelNumber;
        channelType = 4;
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
      detail.TuningSource = tunerSource;
      detail.VideoPid = videoPid;
      detail.AudioPid = audioPid;
      detail.Band = band;
      detail.SatIndex = satIndex;
      detail.InnerFecRate = innerFecRate;
      detail.Pilot = pilot;
      detail.RollOff = rollOff;
      detail.Persist();
      return detail;
    }

    public TuningDetail AddWebStreamTuningDetails(Channel channel, string url, int bitrate)
    {
      string channelName = channel.Name;
      long channelFrequency = 0;
      int channelNumber = 0;
      int country = 31;
      bool isRadio = channel.IsRadio;
      bool isTv = channel.IsTv;
      int tunerSource = 0;
      int videoInputType = 0;
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
      int channelType = 5;
      int videoPid = -1;
      int audioPid = -1;
      int band = 0;
      int satIndex = -1;
      int innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      int pilot = (int)Pilot.NotSet;
      int rollOff = (int)RollOff.NotSet;
      if (url == null)
        url = "";
      TuningDetail detail = new TuningDetail(channel.IdChannel, channelName, provider,
                              channelType, channelNumber, (int)channelFrequency, country, isRadio, isTv,
                              networkId, transportId, serviceId, pmtPid, freeToAir,
                              modulation, polarisation, symbolRate, diseqc, switchFrequency,
                              bandwidth, majorChannel, minorChannel, pcrPid, videoInputType, tunerSource, videoPid, audioPid, band, satIndex, innerFecRate, pilot, rollOff, url, bitrate);
      detail.Persist();
      return detail;
    }

    public TuningDetail AddFMRadioTuningDetails(Channel channel, int frequency)
    {
      string channelName = channel.Name;
      long channelFrequency = frequency;
      int channelNumber = 0;
      int country = 31;
      bool isRadio = channel.IsRadio;
      bool isTv = channel.IsTv;
      int tunerSource = 0;
      int videoInputType = 0;
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
      int channelType = 6;
      int videoPid = -1;
      int audioPid = -1;
      int band = 0;
      int satIndex = -1;
      int innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      int pilot = (int)Pilot.NotSet;
      int rollOff = (int)RollOff.NotSet;
      string url = "";
      int bitrate = 0;
      TuningDetail detail = new TuningDetail(channel.IdChannel, channelName, provider,
                              channelType, channelNumber, (int)channelFrequency, country, isRadio, isTv,
                              networkId, transportId, serviceId, pmtPid, freeToAir,
                              modulation, polarisation, symbolRate, diseqc, switchFrequency,
                              bandwidth, majorChannel, minorChannel, pcrPid, videoInputType, tunerSource, videoPid, audioPid, band, satIndex, innerFecRate, pilot, rollOff, url, bitrate);
      detail.Persist();
      return detail;
    }
    #endregion

    #region linkage map
    public IList GetLinkagesForChannel(Channel channel)
    {
      int idChannel = -1;
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(ChannelLinkageMap));
      sb.AddConstraint(Operator.Equals, "idLinkedChannel", channel.IdChannel);
      SqlStatement stmt = sb.GetStatement(true);
      IList links = ObjectFactory.GetCollection(typeof(ChannelLinkageMap), stmt.Execute());
      if (links != null)
      {
        if (links.Count > 0)
        {
          ChannelLinkageMap map = (ChannelLinkageMap)links[0];
          idChannel = map.ReferringPortalChannel().IdChannel;
        }
      }
      if (idChannel == -1)
        idChannel = channel.IdChannel;
      sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(ChannelLinkageMap));
      sb.AddConstraint(Operator.Equals, "idPortalChannel", idChannel);
      stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection(typeof(ChannelLinkageMap), stmt.Execute());
    }
    #endregion

    #region programs

    public string GetDateTimeString()
    {
      string provider = Gentle.Framework.ProviderFactory.GetDefaultProvider().Name.ToLower();
      if (provider == "mysql") return "yyyy-MM-dd HH:mm:ss";
      return "yyyyMMdd HH:mm:ss";
    }

    public void RemoveOldPrograms()
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Delete, typeof(Program));
      DateTime dtYesterday = DateTime.Now.AddDays(-1);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(String.Format("endTime < '{0}'", dtYesterday.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }

    public void RemoveOldPrograms(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Delete, typeof(Program));
      DateTime dtToKeep = DateTime.Now.AddHours(-4.0);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddConstraint(String.Format("startTime < '{0}'", dtToKeep.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }

    public void RemoveAllPrograms(int idChannel)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Delete, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }

    public IList GetOnairNow()
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(String.Format("startTime <= '{0}' and endTime >= '{1}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat), DateTime.Now.ToString(GetDateTimeString(), mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public IList GetPrograms(Channel channel, DateTime startTime)
    {
      //The DateTime.MinValue is lower than the min datetime value of the database
      if (startTime == DateTime.MinValue)
        startTime = startTime.AddYears(1900);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(String.Format("startTime>='{0}'", startTime.ToString(GetDateTimeString(), mmddFormat)));
      sb.AddOrderByField(true, "startTime");

      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public IList GetPrograms(Channel channel, DateTime startTime, DateTime endTime)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(string.Format("({0} or {1} or {2}) ", sub1, sub2, sub3));
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public IList GetProgramExists(Channel channel, DateTime startTime, DateTime endTime)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));

      string sub = String.Format("(StartTime = '{0}' and EndTime = '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(string.Format("({0}) ", sub));
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public Dictionary<int, List<Program>> GetProgramsForAllChannels(DateTime startTime, DateTime endTime, List<Channel> channelList)
    {
      //Stopwatch bench = new Stopwatch();
      //bench.Start();
      Dictionary<int, List<Program>> maps = new Dictionary<int, List<Program>>();
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(string.Format("({0} or {1} or {2}) ", sub1, sub2, sub3));
      string channelConstraint = "";
      foreach (Channel ch in channelList)
      {
        if (channelConstraint == "")
        {
          channelConstraint = String.Format("(idChannel={0}", ch.IdChannel);
        }
        else
        {
          channelConstraint += String.Format(" or idChannel={0}", ch.IdChannel);
        }
      }
      if (channelConstraint.Length > 0)
      {
        channelConstraint += ")";
        sb.AddConstraint(channelConstraint);
      }
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      foreach (Program p in progs)
      {
        int idChannel = p.IdChannel;
        if (!maps.ContainsKey(idChannel))
        {
          maps[idChannel] = new List<Program>();
        }
        maps[idChannel].Add(p);
      }
      //bench.Stop();
      //Log.Info("BL: GetProgsForAllChans: Start: {0}, End: {1}, Channels: {2}, Results: {3}, Time: {4} ms", startTime.ToString(), endTime.ToString(), channelList.Count.ToString(), maps.Count.ToString(), bench.ElapsedMilliseconds.ToString());
      return maps;
    }

    public IList GetPrograms(DateTime startTime, DateTime endTime)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(string.Format(" ({0} or {1} or {2}) ", sub1, sub2, sub3));
      sb.AddOrderByField(true, "starttime");
      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public DateTime GetNewestProgramForChannel(int idChannel)
    {
      DateTime dtNewestEntry = DateTime.MinValue;
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddOrderByField(false, "startTime");
      sb.SetRowLimit(1);
      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      if (progs.Count > 0)
        return ((Program)progs[0]).StartTime;
      else
        return DateTime.MinValue;
    }

    public IList GetProgramsByTitle(Channel channel, DateTime startTime, DateTime endTime, string title)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(string.Format("({0} or {1} or {2}) ", sub1, sub2, sub3));
      sb.AddConstraint(Operator.Like, "title", title);
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public IList GetProgramsByTitle(DateTime startTime, DateTime endTime, string title)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString(GetDateTimeString(), mmddFormat), endTime.ToString(GetDateTimeString(), mmddFormat));

      sb.AddConstraint(string.Format(" ({0} or {1} or {2}) ", sub1, sub2, sub3));
      sb.AddConstraint(Operator.Like, "title", title);
      sb.AddOrderByField(true, "starttime");
      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public IList SearchMinimalPrograms(DateTime startTime, DateTime endTime, string programName, Channel channel)
    {
      IList programs;
      IList progsReturn = new List<Program>();
      if (channel != null)
        programs = GetProgramsByTitle(channel, startTime, endTime, programName);
      else
        programs = GetProgramsByTitle(startTime, endTime, programName);

      foreach (Program prog in programs)
      {
        progsReturn.Add(prog);
      }
      return progsReturn;
    }

    public IList GetGenres()
    {
      List<string> genres = new List<string>();
      string connectString = Gentle.Framework.ProviderFactory.GetDefaultProvider().ConnectionString;

      string provider = Gentle.Framework.ProviderFactory.GetDefaultProvider().Name.ToLower();
      if (provider == "mysql")
      {
        using (MySqlConnection connect = new MySqlConnection(connectString))
        {
          connect.Open();
          using (MySqlCommand cmd = connect.CreateCommand())
          {
            cmd.CommandText = "select distinct(genre) from Program order by genre";
            cmd.CommandType = System.Data.CommandType.Text;
            using (System.Data.IDataReader reader = cmd.ExecuteReader())
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
        using (System.Data.OleDb.OleDbConnection connect = new System.Data.OleDb.OleDbConnection("Provider=SQLOLEDB;" + connectString))
        {
          connect.Open();
          using (System.Data.OleDb.OleDbCommand cmd = connect.CreateCommand())
          {
            cmd.CommandText = "select distinct(genre) from Program order by genre";
            cmd.CommandType = System.Data.CommandType.Text;
            using (System.Data.IDataReader reader = cmd.ExecuteReader())
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

    public IList SearchProgramsPerGenre(string currentGenre, string currentSearchCriteria)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));

      if (currentSearchCriteria.Length == 0)
      {
        sb.AddConstraint(Operator.Like, "genre", currentGenre);
        sb.AddOrderByField("title");
        sb.AddOrderByField("starttime");
      }
      else
      {
        sb.AddConstraint(Operator.Like, "genre", currentGenre);
        sb.AddConstraint(Operator.Like, "title", String.Format("{0}%", currentSearchCriteria));
        sb.AddOrderByField("title");
        sb.AddOrderByField("starttime");
      }

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }

    public IList SearchPrograms(string searchCriteria)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      if (searchCriteria.Length > 0)
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat)));
        sb.AddConstraint(Operator.Like, "title", String.Format("{0}%", searchCriteria));
        sb.AddOrderByField("title");
        sb.AddOrderByField("starttime");
      }
      else
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat)));
        sb.AddOrderByField("title");
        sb.AddOrderByField("starttime");
      }

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection(typeof(Program), stmt.Execute());

    }

    public IList SearchProgramsByDescription(string searchCriteria)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      if (searchCriteria.Length > 0)
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat)));
        sb.AddConstraint(Operator.Like, "description", String.Format("{0}%", searchCriteria));
        sb.AddOrderByField("description");
        sb.AddOrderByField("starttime");
      }
      else
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString(GetDateTimeString(), mmddFormat)));
        sb.AddOrderByField("description");
        sb.AddOrderByField("starttime");
      }

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }

    public Dictionary<int, NowAndNext> GetNowAndNext()
    {
      Dictionary<int, NowAndNext> nowNextList = new Dictionary<int, NowAndNext>();
      string provider = Gentle.Framework.ProviderFactory.GetDefaultProvider().Name.ToLower();
      string connectString = Gentle.Framework.ProviderFactory.GetDefaultProvider().ConnectionString;
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
            MySQLCmd = MySQLConnect.CreateCommand();
            MySQLCmd.CommandType = CommandType.Text;
            MySQLCmd.CommandText = "SELECT idChannel,idProgram,starttime,endtime,title FROM Program WHERE Program.endtime >= NOW() AND Program.endtime < DATE_ADD(SYSDATE(),INTERVAL 24 HOUR) ORDER BY idchannel,starttime";
            MySQLAdapter.SelectCommand = MySQLCmd;
            break;
          case "sqlserver":
            //MSSQLConnect = new System.Data.OleDb.OleDbConnection("Provider=SQLOLEDB;" + connectString);
            MsSqlConnect = new SqlConnection(connectString);
            MsSqlAdapter = new SqlDataAdapter();
            MsSqlAdapter.TableMappings.Add("Table", "Program");
            MsSqlConnect.Open();
            MsSqlCmd = MsSqlConnect.CreateCommand();
            MsSqlCmd.CommandType = CommandType.Text;
            // "select idChannel,idProgram,starttime,endtime,title from Program where Program.endtime >= now() and Program.idProgram in (select idProgram from Program as p3 where p3.idchannel=Program.idchannel and p3.endtime >= now() order by starttime) order by idchannel,starttime desc";
            MsSqlCmd.CommandText = "SELECT idChannel,idProgram,starttime,endtime,title FROM Program WHERE Program.endtime >= getdate() AND Program.endtime < DATEADD(day, 1, getdate()) ORDER BY idchannel,starttime";
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
          if (provider == "sqlserver")
            MsSqlAdapter.Fill(dataSet);
          else
            if (provider == "mysql")
              MySQLAdapter.Fill(dataSet);

          int resultCount = dataSet.Tables[0].Rows.Count;
          List<int> lastChannelIDs = new List<int>();

          // for-loops are faster than foreach-loops
          for (int j = 0 ; j < resultCount ; j++)
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

              if (j < resultCount - 1)
              {
                // get the the "Next" info if it belongs to the same channel.
                if (idChannel == (int)dataSet.Tables[0].Rows[j + 1]["idChannel"])
                {
                  int nextidProgram = (int)dataSet.Tables[0].Rows[j + 1]["idProgram"];
                  DateTime nextStart = (DateTime)dataSet.Tables[0].Rows[j + 1]["startTime"];
                  DateTime nextEnd = (DateTime)dataSet.Tables[0].Rows[j + 1]["endTime"];
                  string nextTitle = (string)dataSet.Tables[0].Rows[j + 1]["title"];

                  NowAndNext p = new NowAndNext(idChannel, nowStart, nowEnd, nextStart, nextEnd, nowTitle, nextTitle, nowidProgram, nextidProgram);
                  nowNextList[idChannel] = p;
                }
              }
            }
          }
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
            MySQLConnect.Close();
            MySQLAdapter.Dispose();
            MySQLCmd.Dispose();
            MySQLConnect.Dispose();
            break;
          case "sqlserver":
            MsSqlConnect.Close();
            MsSqlAdapter.Dispose();
            MsSqlCmd.Dispose();
            MsSqlConnect.Dispose();
            break;
        }
      }

      return nowNextList;
    }

    #region EPG Insert

    private class ImportParams
    {
      public List<Program> ProgramList;
      public string ConnectString;
      public int SleepTime;
    };

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
        IGentleProvider prov = Gentle.Framework.ProviderFactory.GetDefaultProvider();
        string provider = prov.Name.ToLower();
        string defaultConnectString = prov.ConnectionString; // Gentle.Framework.ProviderFactory.GetDefaultProvider().ConnectionString;
        int sleepTime = 8;

        switch (aThreadPriority)
        {
          case ThreadPriority.Highest:
          case ThreadPriority.AboveNormal:
            aThreadPriority = ThreadPriority.Normal;
            sleepTime = 0;
            break;
          case ThreadPriority.Normal:      // this is almost enough on dualcore systems for one cpu to gather epg and the other to insert it
            sleepTime = 8;
            break;
          case ThreadPriority.BelowNormal: // on faster systems this might be enough for background importing
            sleepTime = 32;
            break;
          case ThreadPriority.Lowest:      // even a single core system is enough to use MP while importing.
            sleepTime = 64;
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
            importThread = new Thread(new ParameterizedThreadStart(ExecuteMySqlCommand));
            importThread.Priority = aThreadPriority;
            importThread.Start(param);
            break;
          case "sqlserver":
            importThread = new Thread(new ParameterizedThreadStart(ExecuteSqlServerCommand));
            importThread.Priority = aThreadPriority;
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

    private void ExecuteMySqlCommand(object aImportParam)
    {
      lock (SingleInsert)
      {
        ImportParams MyParams = (ImportParams)aImportParam;
        InsertMySql(MyParams);
      }
    }

    #region SQL methods

    private void InsertMySql(ImportParams aImportParam)
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
        }
      }
      catch (Exception ex)
      {
        try
        {
          transact.Rollback();
        }
        catch (Exception) { }
        Log.Info("BusinessLayer: InsertMySql caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
      }
      transact = null;
      GC.Collect();
    }

    private void InsertSqlServer(string aConnectString, SqlCommand aSqlCommand)
    {
      if (aSqlCommand == null || string.IsNullOrEmpty(aConnectString))
        return;

      SqlTransaction transact = null;
      try
      {
        using (SqlConnection connection = new SqlConnection(aConnectString))
        {
          connection.Open();
          transact = connection.BeginTransaction();
          aSqlCommand.Connection = connection;
          aSqlCommand.Transaction = transact;
          aSqlCommand.ExecuteNonQuery();
          transact.Commit();
        }
      }
      catch (SqlException msex)
      {
        string errorRow = aSqlCommand.Parameters["idChannel"].Value + ", " + aSqlCommand.Parameters["title"].Value + " : " + aSqlCommand.Parameters["startTime"].Value + "-" + aSqlCommand.Parameters["endTime"].Value;
        switch (msex.Number)
        {
          //case 1062:
          //  Log.Info("BusinessLayer: Your importer tried to add a duplicate entry: {0}", errorRow);
          //case 1406:
          //  Log.Info("BusinessLayer: Your importer tried to add a too much info: {0}, {1}", errorRow, msex.Message);
          default:
            Log.Info("BusinessLayer: InsertSqlServer caused a SqlException - {0}, {1}, {2}", msex.Message, msex.Number, msex.HelpLink);
            break;
        }
        try
        {
          transact.Rollback();
        }
        catch (Exception) { }
      }
      catch (Exception ex)
      {
        Log.Error("BusinessLayer: InsertSqlServer error - {0}, {1}", ex.Message, ex.StackTrace);
      }
    }

    #endregion

    #region SQL Builder

    private void ExecuteMySqlCommand(List<Program> aProgramList, MySqlConnection aConnection, MySqlTransaction aTransaction, int aDelay)
    {
      MySqlCommand sqlcmd = new MySqlCommand();
      List<Program> currentInserts = new List<Program>(aProgramList);

      sqlcmd.CommandText = "INSERT INTO Program (idChannel, startTime, endTime, title, description, seriesNum, episodeNum, genre, originalAirDate, classification, starRating, notify, parentalRating) VALUES (?idChannel, ?startTime, ?endTime, ?title, ?description, ?seriesNum, ?episodeNum, ?genre, ?originalAirDate, ?classification, ?starRating, ?notify, ?parentalRating)";

      sqlcmd.Parameters.Add("?idChannel", MySqlDbType.Int32);
      sqlcmd.Parameters.Add("?startTime", MySqlDbType.Datetime);
      sqlcmd.Parameters.Add("?endTime", MySqlDbType.Datetime);
      sqlcmd.Parameters.Add("?title", MySqlDbType.VarChar);
      sqlcmd.Parameters.Add("?description", MySqlDbType.VarChar);
      sqlcmd.Parameters.Add("?seriesNum", MySqlDbType.VarChar);
      sqlcmd.Parameters.Add("?episodeNum", MySqlDbType.VarChar);
      sqlcmd.Parameters.Add("?genre", MySqlDbType.VarChar);
      sqlcmd.Parameters.Add("?originalAirDate", MySqlDbType.Datetime);
      sqlcmd.Parameters.Add("?classification", MySqlDbType.VarChar);
      sqlcmd.Parameters.Add("?starRating", MySqlDbType.Int32);
      sqlcmd.Parameters.Add("?notify", MySqlDbType.Bit);
      sqlcmd.Parameters.Add("?parentalRating", MySqlDbType.Int32);

      try
      {
        sqlcmd.Connection = aConnection;
        sqlcmd.Transaction = aTransaction;
        // Prepare the command since we will reuse it quite often
        sqlcmd.Prepare();
      }
      catch (Exception ex)
      {
        Log.Info("BusinessLayer: ExecuteMySqlCommand - Prepare caused an Exception - {0}", ex.Message);
      }

      foreach (Program prog in currentInserts)
      {
        sqlcmd.Parameters["?idChannel"].Value = prog.IdChannel;
        sqlcmd.Parameters["?startTime"].Value = prog.StartTime;
        sqlcmd.Parameters["?endTime"].Value = prog.EndTime;
        sqlcmd.Parameters["?title"].Value = prog.Title;
        sqlcmd.Parameters["?description"].Value = prog.Description;
        sqlcmd.Parameters["?seriesNum"].Value = prog.SeriesNum;
        sqlcmd.Parameters["?episodeNum"].Value = prog.EpisodeNum;
        sqlcmd.Parameters["?genre"].Value = prog.Genre;
        sqlcmd.Parameters["?originalAirDate"].Value = prog.OriginalAirDate;
        sqlcmd.Parameters["?classification"].Value = prog.Classification;
        sqlcmd.Parameters["?starRating"].Value = prog.StarRating;
        sqlcmd.Parameters["?notify"].Value = prog.Notify;
        sqlcmd.Parameters["?parentalRating"].Value = prog.ParentalRating;
        try
        {
          // Finally insert all our data
          sqlcmd.ExecuteNonQuery();
          // Avoid I/O starving
          Thread.Sleep(aDelay / 4);
        }
        catch (MySqlException myex)
        {
          string errorRow = sqlcmd.Parameters["?idChannel"].Value + ", " + sqlcmd.Parameters["?title"].Value + " : " + sqlcmd.Parameters["?startTime"].Value + "-" + sqlcmd.Parameters["?endTime"].Value;
          switch (myex.Number)
          {
            case 1062:
              Log.Info("BusinessLayer: Your importer tried to add a duplicate entry: {0}", errorRow);
              break;
            case 1406:
              Log.Info("BusinessLayer: Your importer tried to add a too much info: {0}, {1}", errorRow, myex.Message);
              break;
            default:
              Log.Info("BusinessLayer: ExecuteMySqlCommand caused a MySqlException - {0}, {1}, {2}", myex.Message, myex.Number, myex.HelpLink);
              break;
          }
        }
        catch (Exception ex)
        {
          Log.Info("BusinessLayer: ExecuteMySqlCommand caused an Exception - {0}, {1}", ex.Message, ex.StackTrace);
        }
      }
    }

    private void ExecuteSqlServerCommand(object aImportParam)
    {
      lock (SingleInsert)
      {
        ImportParams MyParams = (ImportParams)aImportParam;
        SqlCommand sqlInsert = new SqlCommand();
        List<Program> currentInserts = new List<Program>(MyParams.ProgramList);

        sqlInsert.CommandText = "INSERT INTO Program (idChannel, startTime, endTime, title, description, seriesNum, episodeNum, genre, originalAirDate, classification, starRating, notify, parentalRating) VALUES (@idChannel, @startTime, @endTime, @title, @description, @seriesNum, @episodeNum, @genre, @originalAirDate, @classification, @starRating, @notify, @parentalRating)";

        sqlInsert.Parameters.Add("idChannel", SqlDbType.Int);
        sqlInsert.Parameters.Add("startTime", SqlDbType.DateTime);
        sqlInsert.Parameters.Add("endTime", SqlDbType.DateTime);
        sqlInsert.Parameters.Add("title", SqlDbType.VarChar);
        sqlInsert.Parameters.Add("description", SqlDbType.VarChar);
        sqlInsert.Parameters.Add("seriesNum", SqlDbType.VarChar);
        sqlInsert.Parameters.Add("episodeNum", SqlDbType.VarChar);
        sqlInsert.Parameters.Add("genre", SqlDbType.VarChar);
        sqlInsert.Parameters.Add("originalAirDate", SqlDbType.DateTime);
        sqlInsert.Parameters.Add("classification", SqlDbType.VarChar);
        sqlInsert.Parameters.Add("starRating", SqlDbType.Int);
        sqlInsert.Parameters.Add("notify", SqlDbType.Bit);
        sqlInsert.Parameters.Add("parentalRating", SqlDbType.Int);

        if (currentInserts.Count > 0)
        {
          foreach (Program prog in currentInserts)
          {
            sqlInsert.Parameters["idChannel"].Value = prog.IdChannel;
            sqlInsert.Parameters["startTime"].Value = prog.StartTime;
            sqlInsert.Parameters["endTime"].Value = prog.EndTime;
            sqlInsert.Parameters["title"].Value = prog.Title;
            sqlInsert.Parameters["description"].Value = prog.Description;
            sqlInsert.Parameters["seriesNum"].Value = prog.SeriesNum;
            sqlInsert.Parameters["episodeNum"].Value = prog.EpisodeNum;
            sqlInsert.Parameters["genre"].Value = prog.Genre;
            sqlInsert.Parameters["originalAirDate"].Value = prog.OriginalAirDate;
            sqlInsert.Parameters["classification"].Value = prog.Classification;
            sqlInsert.Parameters["starRating"].Value = prog.StarRating;
            sqlInsert.Parameters["notify"].Value = prog.Notify;
            sqlInsert.Parameters["parentalRating"].Value = prog.ParentalRating;

            InsertSqlServer(MyParams.ConnectString, sqlInsert);
            Thread.Sleep(MyParams.SleepTime / 2);
          }
        }
        Thread.Sleep(MyParams.SleepTime);
      }
    }

    #endregion // SQL builder

    #endregion // EPG Insert

    #endregion // programs

    #region schedules
    public List<Schedule> GetConflictingSchedules(Schedule rec)
    {
      Log.Info("GetConflictingSchedules: Schedule = " + rec.ToString());
      List<Schedule> conflicts = new List<Schedule>();
      IList schedulesList = Schedule.ListAll();
      IList cards = Card.ListAll();
      if (cards.Count == 0) return conflicts;
      Log.Info("GetConflictingSchedules: Cards.Count = {0}", cards.Count);

      List<Schedule>[] cardSchedules = new List<Schedule>[cards.Count];
      for (int i = 0 ; i < cards.Count ; i++) cardSchedules[i] = new List<Schedule>();

      // GEMX: Assign all already scheduled timers to cards. Assume that even possibly overlapping schedulues are ok to the user,
      // as he decided to keep them before. That's why they are in the db
      foreach (Schedule schedule in schedulesList)
      {
        List<Schedule> episodes = GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.EndTime) continue;
          if (episode.IsSerieIsCanceled(episode.StartTime)) continue;
          Schedule overlapping;
          AssignSchedulesToCard(episode, cardSchedules, out overlapping);
        }
      }

      List<Schedule> newEpisodes = GetRecordingTimes(rec);
      foreach (Schedule newEpisode in newEpisodes)
      {
        if (DateTime.Now > newEpisode.EndTime) continue;
        if (newEpisode.IsSerieIsCanceled(newEpisode.StartTime)) continue;
        Schedule overlapping;
        if (!AssignSchedulesToCard(newEpisode, cardSchedules, out overlapping))
        {
          Log.Info("GetConflictingSchedules: newEpisode can not be assigned to a card = " + newEpisode.ToString());
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

    private bool AssignSchedulesToCard(Schedule schedule, List<Schedule>[] cardSchedules, out Schedule overlappingSchedule)
    {
      overlappingSchedule = null;
      Log.Info("AssignSchedulesToCard: schedule = " + schedule.ToString());
      IList cards = Card.ListAll();
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
            Log.Info("AssignSchedulesToCard: card {0}, ID = {1} has schedule = " + assignedSchedule.ToString(), count, card.IdCard);
            if (schedule.IsOverlapping(assignedSchedule))
            {
              if (!(schedule.isSameTransponder(assignedSchedule) && card.supportSubChannels))
              {
                overlappingSchedule = assignedSchedule;
                Log.Info("AssignSchedulesToCard: overlapping with " + assignedSchedule.ToString() + " on card {0}, ID = {1}", count, card.IdCard);
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
      if (!assigned) return false;

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
        for (int i = 0 ; i < days ; ++i)
        {
          Schedule recNew = rec.Clone();
          recNew.ScheduleType = (int)ScheduleRecordingType.Once;
          recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute, 0);
          if (rec.EndTime.Day > rec.StartTime.Day)
            dtDay = dtDay.AddDays(1);
          recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
          if (rec.EndTime.Day > rec.StartTime.Day)
            dtDay = dtDay.AddDays(-1);
          recNew.Series = true;
          if (recNew.StartTime >= DateTime.Now)
          {
            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
            recordings.Add(recNew);
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }

      if (rec.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
      {
        for (int i = 0 ; i < days ; ++i)
        {
          if (dtDay.DayOfWeek != DayOfWeek.Saturday && dtDay.DayOfWeek != DayOfWeek.Sunday)
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(1);
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(-1);
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
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
        IList progList;
        progList = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, rec.ReferencedChannel());

        foreach (Program prog in progList)
        {
          if ((rec.IsRecordingProgram(prog, false)) &&
                      (prog.StartTime.DayOfWeek == DayOfWeek.Saturday || prog.StartTime.DayOfWeek == DayOfWeek.Sunday))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = prog.StartTime;
            recNew.EndTime = prog.EndTime;
            recNew.Series = true;

            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
            recordings.Add(recNew);
          }

        }
        return recordings;
      }
      if (rec.ScheduleType == (int)ScheduleRecordingType.Weekly)
      {
        for (int i = 0 ; i < days ; ++i)
        {
          if ((dtDay.DayOfWeek == rec.StartTime.DayOfWeek) && (dtDay.Date >= rec.StartTime.Date))
          {
            Schedule recNew = rec.Clone();
            recNew.ScheduleType = (int)ScheduleRecordingType.Once;
            recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(1);
            recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
            if (rec.EndTime.Day > rec.StartTime.Day)
              dtDay = dtDay.AddDays(-1);
            recNew.Series = true;
            if (rec.IsSerieIsCanceled(recNew.StartTime))
              recNew.Canceled = recNew.StartTime;
            if (recNew.StartTime >= DateTime.Now)
            {
              recordings.Add(recNew);
            }
          }
          dtDay = dtDay.AddDays(1);
        }
        return recordings;
      }


      IList programs;
      if (rec.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel)
      {
        //Log.Debug("get {0} {1} EveryTimeOnThisChannel", rec.ProgramName, rec.ReferencedChannel().Name);
        programs = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, rec.ReferencedChannel());
      }
      else
      {
        //Log.Debug("get {0} EveryTimeOnAllChannels", rec.ProgramName);

        programs = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, null);
      }
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
            recNew.Canceled = recNew.StartTime;
          recordings.Add(recNew);
        }
      }
      return recordings;
    }

    // Add schedules for importing from xml
    public Schedule AddSchedule(int idChannel, string programName, DateTime startTime, DateTime endTime, int scheduleType)
    {
      Schedule schedule = GetSchedule(idChannel, programName, startTime, endTime, scheduleType);
      if (schedule != null)
        return schedule;
      else
      {
        Schedule newSchedule = new Schedule(idChannel, programName, startTime, endTime);
        return newSchedule;
      }
    }

    // Get schedules to import from xml
    public Schedule GetSchedule(int idChannel, string programName, DateTime startTime, DateTime endTime, int scheduleType)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Schedule));
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddConstraint(Operator.Equals, "programName", programName);
      sb.AddConstraint(Operator.Equals, "startTime", startTime);
      sb.AddConstraint(Operator.Equals, "endTime", endTime);
      sb.AddConstraint(Operator.Equals, "scheduleType", scheduleType);
      SqlStatement stmt = sb.GetStatement(true);
      Log.Info(stmt.Sql);
      IList schedules = ObjectFactory.GetCollection(typeof(Schedule), stmt.Execute());
      if (schedules == null) return null;
      if (schedules.Count == 0) return null;
      return (Schedule)schedules[0];
    }
    #endregion

    #region recordings
    public Recording GetRecordingByFileName(string fileName)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Recording));
      sb.AddConstraint(Operator.Equals, "fileName", fileName);
      sb.SetRowLimit(1);
      SqlStatement stmt = sb.GetStatement(true);
      IList recordings = ObjectFactory.GetCollection(typeof(Recording), stmt.Execute());
      if (recordings.Count == 0)
        return null;
      return (Recording)recordings[0];
    }
    #endregion

    #region channelgroups

    public RadioChannelGroup GetRadioChannelGroupByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(RadioChannelGroup));
      sb.AddConstraint(Operator.Equals, "groupName", name);
      SqlStatement stmt = sb.GetStatement(true);
      IList groups = ObjectFactory.GetCollection(typeof(RadioChannelGroup), stmt.Execute());
      if (groups == null) return null;
      if (groups.Count == 0) return null;
      return (RadioChannelGroup)groups[0];
    }
    #endregion

    #region EPG Updating
    string _titleTemplate;
    string _descriptionTemplate;
    string _epgLanguages;
    #endregion
  }
}
