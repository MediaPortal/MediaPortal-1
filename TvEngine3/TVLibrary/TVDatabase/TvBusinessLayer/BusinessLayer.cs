using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
//using System.Data;
//using System.Data.OleDb;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;

using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Channels;
using TvLibrary;
using DirectShowLib;
using DirectShowLib.BDA;
namespace TvDatabase
{
  public class TvBusinessLayer
  {
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
      Card newCard = new Card(devicePath, name, 1, true, new DateTime(2000, 1, 1), "", server.IdServer, true, 0);
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
    #endregion

    #region channels

    public Channel AddChannel(string provider,string name)
    {
      Channel channel = GetChannelByName(provider,name);
      if (channel != null)
      {
        channel.Name = name;
        return channel;
      }
      Channel newChannel = new Channel(name, false, false, 0, new DateTime(2000, 1, 1), true, new DateTime(2000, 1, 1), -1, true, "");

      return newChannel;
    }

    public void AddChannelToGroup(Channel channel, string groupName)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
      sb.AddConstraint(Operator.Like, "groupName", groupName);
      SqlStatement stmt = sb.GetStatement(true);
      IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
      ChannelGroup group;
      if (groups.Count == 0)
      {
        group = new ChannelGroup(groupName);
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

    public IList GetChannelsByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "name", name);
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return channels;
    }
    public Channel GetChannelByName(string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "name", name);
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return (Channel)channels[0];
    }

    public Channel GetChannelByName(string provider, string name)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TuningDetail));
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

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Setting));
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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Setting));
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
      for (int i = 0; i < tuningDetails.Count; ++i)
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
            atscChannel.AudioPid = detail.AudioPid;
            atscChannel.VideoPid = detail.VideoPid;
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
      for (int i = 0; i < tuningDetails.Count; ++i)
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
            analogChannel.Name = detail.Name;
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
            atscChannel.Name = detail.Name;
            atscChannel.NetworkId = detail.NetworkId;
            atscChannel.PcrPid = detail.PcrPid;
            atscChannel.PmtPid = detail.PmtPid;
            atscChannel.Provider = detail.Provider;
            atscChannel.ServiceId = detail.ServiceId;
            atscChannel.SymbolRate = detail.Symbolrate;
            atscChannel.TransportId = detail.TransportId;
            atscChannel.AudioPid = detail.AudioPid;
            atscChannel.VideoPid = detail.VideoPid;
            tvChannels.Add(atscChannel);
            break;
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
            tvChannels.Add(dvbsChannel);
            break;
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
            tvChannels.Add(dvbtChannel);
            break;
        }
      }
      return tvChannels;
    }

    public ChannelMap MapChannelToCard(Card card, Channel channel)
    {
      IList channelMaps = card.ReferringChannelMap();
      for (int i = 0; i < channelMaps.Count; ++i)
      {
        ChannelMap map = (ChannelMap)channelMaps[i];
        if (map.IdChannel == channel.IdChannel && map.IdCard == card.IdCard) return map;
      }
      ChannelMap newMap = new ChannelMap(channel.IdChannel, card.IdCard);
      newMap.Persist();
      return newMap;
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
        videoPid = atscChannel.VideoPid;
        audioPid = atscChannel.AudioPid;
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
        channelType = 3;
      }

      DVBTChannel dvbtChannel = tvChannel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.BandWidth;
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
      }


      TuningDetail detail = new TuningDetail(channel.IdChannel, channelName, provider,
                              channelType, channelNumber, (int)channelFrequency, country, isRadio, isTv,
                              networkId, transportId, serviceId, pmtPid, freeToAir,
                              modulation, polarisation, symbolRate, diseqc, switchFrequency,
                              bandwidth, majorChannel, minorChannel, pcrPid, videoInputType, tunerSource, videoPid, audioPid, band,satIndex);
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
        videoPid = atscChannel.VideoPid;
        audioPid = atscChannel.AudioPid;
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
        channelType = 3;
      }

      DVBTChannel dvbtChannel = tvChannel as DVBTChannel;
      if (dvbtChannel != null)
      {
        bandwidth = dvbtChannel.BandWidth;
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
      detail.Persist();
      return detail;
    }
    #endregion

    #region programs
    public void RemoveOldPrograms()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
      DateTime dtYesterday = DateTime.Now.AddDays(-1);
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(String.Format("endTime < '{0}'", dtYesterday.ToString("yyyyMMdd HH:mm:ss", mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection(typeof(Program), stmt.Execute());

    }
    public IList GetOnairNow()
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      sb.AddConstraint(String.Format("startTime >= '{0}' and endTime <= '{1}'", DateTime.Now.ToString("yyyyMMdd HH:mm:ss", mmddFormat), DateTime.Now.ToString("yyyyMMdd HH:mm:ss", mmddFormat)));
      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public IList GetPrograms(Channel channel, DateTime startTime, DateTime endTime)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat), endTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat), endTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat), endTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat));

      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddConstraint(string.Format("({0} or {1} or {2}) ", sub1, sub2, sub3));
      sb.AddOrderByField(true, "starttime");

      SqlStatement stmt = sb.GetStatement(true);
      IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      return progs;
    }

    public IList GetPrograms(DateTime startTime, DateTime endTime)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat), endTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat), endTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat), endTime.ToString("yyyyMMdd HH:mm:ss", mmddFormat));

      sb.AddConstraint(string.Format(" ({0} or {1} or {2}) ", sub1, sub2, sub3));
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
        programs = GetPrograms(channel, startTime, endTime);
      else
        programs = GetPrograms(startTime, endTime);

      foreach (Program prog in programs)
      {
        if (prog.Title == programName)
        {
          progsReturn.Add(prog);
        }
      }
      return progsReturn;
    }

    public IList GetGenres()
    {
      /*
      List<string> genres = new List<string>();
      ICollection<IDataSourceKey> coll = DatabaseManager.Instance.DataSourceResolver.DataSourceKeys;
      IEnumerator<IDataSourceKey> enumer = coll.GetEnumerator();
      enumer.MoveNext();
      RdbKey key = (RdbKey)enumer.Current;
      string connectString = key.ConnectionString;
      using (OleDbConnection connect = new OleDbConnection(connectString))
      {
        connect.Open();
        using (OleDbCommand cmd = connect.CreateCommand())
        {
          cmd.CommandText = "select distinct(genre) from program order by genre";
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
      return genres;*/
      return null;
    }

    public IList SearchProgramsPerGenre(string currentGenre, string currentSearchCriteria)
    {
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));

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
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      if (searchCriteria.Length > 0)
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString("yyyyMMdd HH:mm:ss", mmddFormat)));
        sb.AddConstraint(Operator.Like, "title", String.Format("{0}%", searchCriteria));
        sb.AddOrderByField("title");
        sb.AddOrderByField("starttime");
      }
      else
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString("yyyyMMdd HH:mm:ss", mmddFormat)));
        sb.AddOrderByField("title");
        sb.AddOrderByField("starttime");
      }

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection(typeof(Program), stmt.Execute());

    }
    public IList SearchProgramsByDescription(string searchCriteria)
    {

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      if (searchCriteria.Length > 0)
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString("yyyyMMdd HH:mm:ss", mmddFormat)));
        sb.AddConstraint(Operator.Like, "description", String.Format("{0}%", searchCriteria));
        sb.AddOrderByField("description");
        sb.AddOrderByField("starttime");
      }
      else
      {
        sb.AddConstraint(String.Format("endTime > '{0}'", DateTime.Now.ToString("yyyyMMdd HH:mm:ss", mmddFormat)));
        sb.AddOrderByField("description");
        sb.AddOrderByField("starttime");
      }

      SqlStatement stmt = sb.GetStatement(true);
      return ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }

    #endregion
  }
}
