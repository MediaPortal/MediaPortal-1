using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using TvDatabase;
using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
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
        card.Server = server;
        return card;
      }
      Card newCard = Card.Create();
      newCard.Name = name;
      newCard.Priority = 1;
      newCard.DevicePath = devicePath;
      newCard.Server = server;
      return newCard;
    }

    public EntityList<Card> Cards
    {
      get
      {
        EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
        return cards;
      }
    }
    public void DeleteCard(Card card)
    {
      card.Delete();
    }

    public Card GetCardByName(string name)
    {
      EntityList<Card> cards = Cards;
      if (cards == null) return null;
      if (cards.Count == 0) return null;

      foreach (Card card in cards)
      {
        if (card.Name == name) return card;
      }
      return null;
    }

    public Card GetCardByDevicePath(string path)
    {
      EntityList<Card> cards = Cards;
      if (cards == null) return null;
      if (cards.Count == 0) return null;
      foreach (Card card in cards)
      {
        if (card.DevicePath == path) return card;
      }
      return null;
    }
    #endregion

    #region channels

    public Channel AddChannel(string name)
    {
      Channel channel = GetChannelByName(name);
      if (channel != null)
      {
        channel.Name = name;
        return channel;
      }
      Channel newChannel = Channel.Create();
      newChannel.GrabEpg = true;
      newChannel.IsRadio = false;
      newChannel.IsTv = false;
      newChannel.LastGrabTime = new DateTime(2000, 1, 1, 0, 0, 0);
      newChannel.SortOrder = -1;
      newChannel.TimesWatched = 0;
      newChannel.TotalTimeWatched = new DateTime(2000, 1, 1, 0, 0, 0);
      newChannel.Name = name;

      return newChannel;
    }
    public void AddChannelToGroup(Channel channel, string groupName)
    {
      ChannelGroup nullGroup = DatabaseManager.Instance.GetNullEntity<ChannelGroup>();
      EntityQuery query = new EntityQuery(typeof(ChannelGroup));
      query.AddClause(ChannelGroup.GroupNameEntityColumn, EntityQueryOp.EQ, groupName);
      EntityList<ChannelGroup> groups = DatabaseManager.Instance.GetEntities<ChannelGroup>(query);

      ChannelGroup group;
      if (groups.Count == 0)
      {
        group = ChannelGroup.Create();
        group.GroupName = groupName;
      }
      else
      {
        group = groups[0];
      }
      bool found = false;
      foreach (GroupMap map in group.GroupMaps)
      {
        if (map.Channel.Name == channel.Name) found = true;
      }
      if (!found)
      {
        GroupMap map = GroupMap.Create();
        map.Channel = channel;
        map.ChannelGroup = group;
      }
    }

    public EntityList<Channel> Channels
    {
      get
      {
        EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>();
        return channels;
      }
    }

    public void DeleteChannel(Channel channel)
    {
      channel.Delete();
    }
    public Channel GetChannelByName(string name)
    {
      Channel channel = DatabaseManager.Instance.GetNullEntity<Channel>();
      EntityQuery query = new EntityQuery(typeof(Channel));
      query.AddClause(channel.NameColumn.ColumnName, EntityQueryOp.EQ, name);
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>(query);
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      return channels[0];
    }

    public Setting GetSetting(string tagName, string defaultValue)
    {
      Setting Setting = DatabaseManager.Instance.GetNullEntity<Setting>();
      EntityQuery query = new EntityQuery(typeof(Setting));
      query.AddClause(Setting.TagColumn.ColumnName, EntityQueryOp.EQ, tagName);
      EntityList<Setting> Settings = DatabaseManager.Instance.GetEntities<Setting>(query);
      if (Settings == null)
      {
        Setting set = Setting.Create();
        set.Tag = tagName;
        set.Value = defaultValue;
        return set;
      }
      if (Settings.Count == 0)
      {
        Setting set = Setting.Create();
        set.Tag = tagName;
        set.Value = defaultValue;
        return set;
      }
      return Settings[0];
    }
    public Setting GetSetting(string tagName)
    {
      Setting Setting = DatabaseManager.Instance.GetNullEntity<Setting>();
      EntityQuery query = new EntityQuery(typeof(Setting));
      query.AddClause(Setting.TagColumn.ColumnName, EntityQueryOp.EQ, tagName);
      EntityList<Setting> Settings = DatabaseManager.Instance.GetEntities<Setting>(query);
      if (Settings == null)
      {
        Setting set = Setting.Create();
        set.Tag = tagName;
        return set;
      }
      if (Settings.Count == 0)
      {
        Setting set = Setting.Create();
        set.Tag = tagName;
        return set;
      }
      return Settings[0];
    }

    public List<IChannel> GetTuningChannelByName(string name)
    {
      Channel channel = DatabaseManager.Instance.GetNullEntity<Channel>();
      EntityQuery query = new EntityQuery(typeof(Channel));
      query.AddClause(channel.NameColumn.ColumnName, EntityQueryOp.EQ, name);
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>(query);
      if (channels == null) return null;
      if (channels.Count == 0) return null;
      List<IChannel> tvChannels = new List<IChannel>();
      CountryCollection collection = new CountryCollection();
      for (int i = 0; i < channels[0].TuningDetails.Count; ++i)
      {
        TuningDetail detail = channels[0].TuningDetails[i];
        switch (detail.ChannelType)
        {
          case 0: //AnalogChannel
            AnalogChannel analogChannel = new AnalogChannel();
            analogChannel.ChannelNumber = detail.ChannelNumber;
            analogChannel.Country = collection.GetTunerCountryFromID(detail.CountryId);
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
      for (int i = 0; i < card.ChannelMaps.Count; ++i)
      {
        ChannelMap map = card.ChannelMaps[i];
        if (map.Channel.IdChannel == channel.IdChannel) return map;
      }
      ChannelMap newMap = ChannelMap.Create();
      newMap.Card = card;
      newMap.Channel = channel;
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

      AnalogChannel analogChannel = tvChannel as AnalogChannel;
      if (analogChannel != null)
      {
        channelName = analogChannel.Name;
        channelFrequency = analogChannel.Frequency;
        channelNumber = analogChannel.ChannelNumber;
        country = analogChannel.Country.Id;
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

      for (int i = 0; i < channel.TuningDetails.Count; ++i)
      {
        if (channel.TuningDetails[i].Name == channelName &&
            channel.TuningDetails[i].Provider == provider &&
            channel.TuningDetails[i].NetworkId == networkId &&
            channel.TuningDetails[i].TransportId == transportId &&
            channel.TuningDetails[i].ServiceId == serviceId &&
            channel.TuningDetails[i].Frequency == channelFrequency &&
            channel.TuningDetails[i].MinorChannel == minorChannel &&
            channel.TuningDetails[i].MajorChannel == majorChannel &&
            channel.TuningDetails[i].ChannelNumber == channelNumber &&
            channel.TuningDetails[i].IsTv == isTv &&
            channel.TuningDetails[i].ChannelType == channelType &&
            channel.TuningDetails[i].IsRadio == isRadio)
        {
          channel.TuningDetails[i].Bandwidth = bandwidth;
          channel.TuningDetails[i].CountryId = country;
          channel.TuningDetails[i].Diseqc = diseqc;
          channel.TuningDetails[i].FreeToAir = freeToAir;
          channel.TuningDetails[i].Modulation = modulation;
          channel.TuningDetails[i].PcrPid = pcrPid;
          channel.TuningDetails[i].PmtPid = pmtPid;
          channel.TuningDetails[i].Polarisation = polarisation;
          channel.TuningDetails[i].SwitchingFrequency = switchFrequency;
          channel.TuningDetails[i].Symbolrate = symbolRate;
          channel.TuningDetails[i].VideoSource = videoInputType;
          channel.TuningDetails[i].TuningSource = tunerSource;
          channel.TuningDetails[i].ChannelType = channelType;
          return channel.TuningDetails[i];
        }
      }

      TuningDetail detail = TuningDetail.Create();
      detail.Channel = channel;
      detail.IdChannel = channel.IdChannel;
      detail.Name = channelName;
      detail.Provider = provider;
      detail.NetworkId = networkId;
      detail.TransportId = transportId;
      detail.ServiceId = serviceId;
      detail.Frequency = (int)channelFrequency;
      detail.MinorChannel = minorChannel;
      detail.MajorChannel = majorChannel;
      detail.ChannelNumber = channelNumber;
      detail.IsTv = isTv;
      detail.IsRadio = isRadio;

      detail.Bandwidth = bandwidth;
      detail.CountryId = country;
      detail.Diseqc = diseqc;
      detail.FreeToAir = freeToAir;
      detail.Modulation = modulation;
      detail.PcrPid = pcrPid;
      detail.PmtPid = pmtPid;
      detail.Polarisation = polarisation;
      detail.SwitchingFrequency = switchFrequency;
      detail.Symbolrate = symbolRate;
      detail.VideoSource = videoInputType;
      detail.TuningSource = tunerSource;
      detail.ChannelType = channelType;
      return detail;
    }
    #endregion

    #region programs
    public void RemoveOldPrograms()
    {
      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program), "delete from program where (dateadd(day,-1,getdate()) >= endTime)");
      DatabaseManager.Instance.GetEntities<Program>(query);
    }
    public List<Program> GetOnairNow()
    {
      List<Program> returnProgs = new List<Program>();
      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program), "select * from program where (getdate() >= startTime and getdate() <=endTime) ");
      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);
      foreach (Program prog in programs)
      {
        returnProgs.Add(prog);
      }
      return returnProgs;
    }

    public List<Program> GetPrograms(Channel channel, DateTime startTime, DateTime endTime)
    {
      Program program = DatabaseManager.Instance.GetNullEntity<Program>();

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString("MM/dd/yyyy HH:mm:ss"), endTime.ToString("MM/dd/yyyy HH:mm:ss"));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString("MM/dd/yyyy HH:mm:ss"), endTime.ToString("MM/dd/yyyy HH:mm:ss"));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString("MM/dd/yyyy HH:mm:ss"), endTime.ToString("MM/dd/yyyy HH:mm:ss"));
      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program),
        String.Format(
            "select * from program where program.idChannel={0} and ( {1} or {2} or {3})  order by starttime asc"
        , channel.IdChannel, sub1, sub2, sub3));

      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);
      List<Program> filteredProgs = new List<Program>();
      if (programs == null) return filteredProgs;
      if (programs.Count == 0) return filteredProgs;
      foreach (Program prog in programs)
      {
        bool add = false;
        if (prog.EndTime > startTime && prog.EndTime < endTime) add = true;
        if (prog.StartTime >= startTime && prog.StartTime <= endTime) add = true;
        if (prog.StartTime <= startTime && prog.EndTime >= endTime) add = true;
        if (add)
        {
          filteredProgs.Add(prog);
        }
      }
      return filteredProgs;
    }

    public List<Program> GetPrograms(DateTime startTime, DateTime endTime)
    {
      Program program = DatabaseManager.Instance.GetNullEntity<Program>();

      string sub1 = String.Format("(EndTime > '{0}' and EndTime < '{1}')", startTime.ToString("MM/dd/yyyy HH:mm:ss"), endTime.ToString("MM/dd/yyyy HH:mm:ss"));
      string sub2 = String.Format("(StartTime >= '{0}' and StartTime <= '{1}')", startTime.ToString("MM/dd/yyyy HH:mm:ss"), endTime.ToString("MM/dd/yyyy HH:mm:ss"));
      string sub3 = String.Format("(StartTime <= '{0}' and EndTime >= '{1}')", startTime.ToString("MM/dd/yyyy HH:mm:ss"), endTime.ToString("MM/dd/yyyy HH:mm:ss"));
      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program),
        String.Format(
            "select * from program where  ( {0} or {1} or {2})  order by starttime asc"
        , sub1, sub2, sub3));

      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);
      List<Program> filteredProgs = new List<Program>();
      if (programs == null) return filteredProgs;
      if (programs.Count == 0) return filteredProgs;
      foreach (Program prog in programs)
      {
        bool add = false;
        if (prog.EndTime > startTime && prog.EndTime < endTime) add = true;
        if (prog.StartTime >= startTime && prog.StartTime <= endTime) add = true;
        if (prog.StartTime <= startTime && prog.EndTime >= endTime) add = true;
        if (add)
        {
          filteredProgs.Add(prog);
        }
      }
      return filteredProgs;
    }
    public List<Program> SearchMinimalPrograms(DateTime startTime, DateTime endTime, string programName, Channel channel)
    {
      List<Program> programs;
      List<Program> progsReturn = new List<Program>();
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
    public List<string> GetGenres()
    {
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
      return genres;
    }
    public EntityList<Program> SearchProgramsPerGenre(string currentGenre, string currentSearchCriteria)
    {
      string sql = String.Format("select * from program where genre like '{0}' and title like '{1}%' order by title,starttime",
            currentGenre, currentSearchCriteria);
      if (currentSearchCriteria.Length == 0)
      {
        sql = String.Format("select * from program where genre like '{0}' order by title,starttime", currentGenre);
      }
      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program), sql);

      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);

      return programs;
    }

    public List<Program> SearchPrograms(string searchCriteria)
    {
      string sql;
      if (searchCriteria.Length > 0)
      {
        sql = String.Format("select * from program where endtime>=getdate() and title like '{0}%' order  by title,starttime asc", searchCriteria);
      }
      else
      {
        sql = String.Format("select * from program where endtime>=getdate() order  by title,starttime asc");
      }

      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program), sql);

      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);
      List<Program> filteredProgs = new List<Program>();
      if (programs == null) return filteredProgs;
      if (programs.Count == 0) return filteredProgs;
      foreach (Program prog in programs)
      {
        filteredProgs.Add(prog);
      }
      return filteredProgs;
    }
    public List<Program> SearchProgramsByDescription(string searchCriteria)
    {

      string sql;
      if (searchCriteria.Length > 0)
      {
        sql = String.Format("select * from program where endtime>=getdate() and description like '{0}%' order  by description,starttime asc", searchCriteria);
      }
      else
      {
        sql = String.Format("select * from program where endtime>=getdate() order  by description,starttime asc");
      }

      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program), sql);
      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);

      List<Program> filteredProgs = new List<Program>();
      if (programs == null) return filteredProgs;
      if (programs.Count == 0) return filteredProgs;
      foreach (Program prog in programs)
      {
        filteredProgs.Add(prog);
      }
      return filteredProgs;
    }

    #endregion
  }
}
