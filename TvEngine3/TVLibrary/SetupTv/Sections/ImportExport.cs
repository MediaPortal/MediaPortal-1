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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;
using Gentle.Framework;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Channels;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class ImportExport : SectionSettings
  {
    public ImportExport()
      : this("Import/Export")
    {
    }

    public ImportExport(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
    }

    private static void AddAttribute(XmlNode node, string tagName, string tagValue)
    {
      XmlAttribute attr = node.OwnerDocument.CreateAttribute(tagName);
      attr.InnerText = tagValue;
      node.Attributes.Append(attr);
    }

    private static void AddAttribute(XmlNode node, string tagName, int tagValue)
    {
      AddAttribute(node, tagName, tagValue.ToString());
    }

    // store DateTime Values as strings. Improves readability
    private static void AddAttribute(XmlNode node, string tagName, DateTime tagValue)
    {
      AddAttribute(node, tagName, String.Format("{0}-{1}-{2} {3}:{4}:{5}", tagValue.Year, tagValue.Month, tagValue.Day, tagValue.Hour, tagValue.Minute, tagValue.Second));
    }

    private static void AddAttribute(XmlNode node, string tagName, bool tagValue)
    {
      AddAttribute(node, tagName, tagValue.ToString());
    }

    private void Export(string fileName, bool exporttv, bool exporttvgroups, bool exportradio, bool exportradiogroups, bool exportschedules)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlNode rootElement = xmlDoc.CreateElement("tvserver");
      AddAttribute(rootElement, "version", "1.0");

      XmlNode nodeServers = xmlDoc.CreateElement("servers");
      IList<Server> servers = Server.ListAll();
      foreach (Server server in servers)
      {
        XmlNode nodeServer = xmlDoc.CreateElement("server");
        AddAttribute(nodeServer, "HostName", server.HostName);
        AddAttribute(nodeServer, "IdServer", server.IdServer);
        AddAttribute(nodeServer, "IsMaster", server.IsMaster);

        XmlNode nodeCards = xmlDoc.CreateElement("cards");
        IList<Card> cards = Card.ListAll();
        foreach (Card card in cards)
        {
          XmlNode nodeCard = xmlDoc.CreateElement("card");
          AddAttribute(nodeCard, "IdCard", card.IdCard);
          AddAttribute(nodeCard, "DevicePath", card.DevicePath);
          AddAttribute(nodeCard, "Enabled", card.Enabled);
          AddAttribute(nodeCard, "CamType", card.CamType);
          AddAttribute(nodeCard, "GrabEPG", card.GrabEPG);
          AddAttribute(nodeCard, "LastEpgGrab", card.LastEpgGrab);
          AddAttribute(nodeCard, "Name", card.Name);
          AddAttribute(nodeCard, "Priority", card.Priority);
          AddAttribute(nodeCard, "RecordingFolder", card.RecordingFolder);
          nodeCards.AppendChild(nodeCard);
        }
        nodeServer.AppendChild(nodeCards);
        nodeServers.AppendChild(nodeServer);
      }
      rootElement.AppendChild(nodeServers);

      XmlNode nodechannels = xmlDoc.CreateElement("channels");
      IList<Channel> channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        // Only export TV or radio channels if the corresponding checkbox was checked
        if ((channel.IsTv && !exporttv) || (!channel.IsRadio && !exportradio))
          continue;

        XmlNode nodechannel = xmlDoc.CreateElement("channel");
        AddAttribute(nodechannel, "Name", channel.Name);
        AddAttribute(nodechannel, "GrabEpg", channel.GrabEpg);
        AddAttribute(nodechannel, "IdChannel", channel.IdChannel);
        AddAttribute(nodechannel, "IsRadio", channel.IsRadio);
        AddAttribute(nodechannel, "IsTv", channel.IsTv);
        AddAttribute(nodechannel, "LastGrabTime", channel.LastGrabTime);
        AddAttribute(nodechannel, "SortOrder", channel.SortOrder);
        AddAttribute(nodechannel, "TimesWatched", channel.TimesWatched);
        AddAttribute(nodechannel, "TotalTimeWatched", channel.TotalTimeWatched);
        AddAttribute(nodechannel, "VisibleInGuide", channel.VisibleInGuide);
        AddAttribute(nodechannel, "FreeToAir", channel.FreeToAir);
        AddAttribute(nodechannel, "DisplayName", channel.DisplayName);

        XmlNode nodeMaps = xmlDoc.CreateElement("mappings");
        foreach (ChannelMap map in channel.ReferringChannelMap())
        {
          XmlNode nodeMap = xmlDoc.CreateElement("map");
          AddAttribute(nodeMap, "IdCard", map.IdCard);
          AddAttribute(nodeMap, "IdChannel", map.IdChannel);
          AddAttribute(nodeMap, "IdChannelMap", map.IdChannelMap);
          nodeMaps.AppendChild(nodeMap);
        }
        nodechannel.AppendChild(nodeMaps);

        XmlNode nodeTuningDetails = xmlDoc.CreateElement("TuningDetails");
        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          XmlNode nodeTune = xmlDoc.CreateElement("tune");
          AddAttribute(nodeTune, "IdChannel", detail.IdChannel);
          AddAttribute(nodeTune, "IdTuning", detail.IdTuning);
          AddAttribute(nodeTune, "AudioPid", detail.AudioPid);
          AddAttribute(nodeTune, "Bandwidth", detail.Bandwidth);
          AddAttribute(nodeTune, "ChannelNumber", detail.ChannelNumber);
          AddAttribute(nodeTune, "ChannelType", detail.ChannelType);
          AddAttribute(nodeTune, "CountryId", detail.CountryId);
          AddAttribute(nodeTune, "Diseqc", detail.Diseqc);
          AddAttribute(nodeTune, "FreeToAir", detail.FreeToAir);
          AddAttribute(nodeTune, "Frequency", detail.Frequency);
          AddAttribute(nodeTune, "MajorChannel", detail.MajorChannel);
          AddAttribute(nodeTune, "MinorChannel", detail.MinorChannel);
          AddAttribute(nodeTune, "Modulation", detail.Modulation);
          AddAttribute(nodeTune, "Name", detail.Name);
          AddAttribute(nodeTune, "NetworkId", detail.NetworkId);
          AddAttribute(nodeTune, "PcrPid", detail.PcrPid);
          AddAttribute(nodeTune, "PmtPid", detail.PmtPid);
          AddAttribute(nodeTune, "Polarisation", detail.Polarisation);
          AddAttribute(nodeTune, "Provider", detail.Provider);
          AddAttribute(nodeTune, "ServiceId", detail.ServiceId);
          AddAttribute(nodeTune, "SwitchingFrequency", detail.SwitchingFrequency);
          AddAttribute(nodeTune, "Symbolrate", detail.Symbolrate);
          AddAttribute(nodeTune, "TransportId", detail.TransportId);
          AddAttribute(nodeTune, "TuningSource", detail.TuningSource);
          AddAttribute(nodeTune, "VideoPid", detail.VideoPid);
          AddAttribute(nodeTune, "VideoSource", detail.VideoSource);
          AddAttribute(nodeTune, "AudioSource", detail.AudioSource);
          AddAttribute(nodeTune, "IsVCRSignal", detail.IsVCRSignal);
          AddAttribute(nodeTune, "SatIndex", detail.SatIndex);
          AddAttribute(nodeTune, "InnerFecRate", detail.InnerFecRate);
          AddAttribute(nodeTune, "Band", detail.Band);
          AddAttribute(nodeTune, "Pilot", detail.Pilot);
          AddAttribute(nodeTune, "RollOff", detail.RollOff);
          AddAttribute(nodeTune, "Url", detail.Url);
          AddAttribute(nodeTune, "Bitrate", detail.Bitrate);
          nodeTuningDetails.AppendChild(nodeTune);
        }
        nodechannel.AppendChild(nodeTuningDetails);

        nodechannels.AppendChild(nodechannel);
      }
      rootElement.AppendChild(nodechannels);

      // exporting the schedules
      if (exportschedules)
      {
        XmlNode nodeSchedules = xmlDoc.CreateElement("schedules");
        IList<Schedule> schedules = Schedule.ListAll();
        foreach (Schedule schedule in schedules)
        {
          XmlNode nodeSchedule = xmlDoc.CreateElement("schedule");
          AddAttribute(nodeSchedule, "ChannelName", schedule.ReferencedChannel().Name);
          AddAttribute(nodeSchedule, "ProgramName", schedule.ProgramName);
          AddAttribute(nodeSchedule, "StartTime", schedule.StartTime);
          AddAttribute(nodeSchedule, "EndTime", schedule.EndTime);
          AddAttribute(nodeSchedule, "KeepDate", schedule.KeepDate);
          AddAttribute(nodeSchedule, "PreRecordInterval", schedule.PreRecordInterval);
          AddAttribute(nodeSchedule, "PostRecordInterval", schedule.PostRecordInterval);
          AddAttribute(nodeSchedule, "Priority", schedule.Priority);
          AddAttribute(nodeSchedule, "Quality", schedule.Quality);
          AddAttribute(nodeSchedule, "Directory", schedule.Directory);
          AddAttribute(nodeSchedule, "KeepMethod", schedule.KeepMethod);
          AddAttribute(nodeSchedule, "MaxAirings", schedule.MaxAirings);
          AddAttribute(nodeSchedule, "RecommendedCard", schedule.RecommendedCard);
          AddAttribute(nodeSchedule, "ScheduleType", schedule.ScheduleType);
          AddAttribute(nodeSchedule, "Series", schedule.Series);
          nodeSchedules.AppendChild(nodeSchedule);
        }
        rootElement.AppendChild(nodeSchedules);
      }

      // exporting tv channel groups
      if (exporttvgroups)
      {
        XmlNode nodeChannelGroups = xmlDoc.CreateElement("channelgroups");
        IList<ChannelGroup> channelgroups = ChannelGroup.ListAll();
        foreach (ChannelGroup group in channelgroups)
        {
          XmlNode nodeChannelGroup = xmlDoc.CreateElement("channelgroup");
          AddAttribute(nodeChannelGroup, "GroupName", group.GroupName);
          AddAttribute(nodeChannelGroup, "SortOrder", group.SortOrder.ToString());
          XmlNode nodeGroupMap = xmlDoc.CreateElement("mappings");
          IList<GroupMap> maps = group.ReferringGroupMap();
          foreach (GroupMap map in maps)
          {
            XmlNode nodeMap = xmlDoc.CreateElement("map");
            AddAttribute(nodeMap, "ChannelName", map.ReferencedChannel().Name);
            AddAttribute(nodeMap, "SortOrder", map.SortOrder.ToString());
            nodeGroupMap.AppendChild(nodeMap);
          }
          nodeChannelGroup.AppendChild(nodeGroupMap);
          nodeChannelGroups.AppendChild(nodeChannelGroup);
        }
        rootElement.AppendChild(nodeChannelGroups);
      }

      // exporting radio channel groups
      if (exportradiogroups)
      {
        XmlNode nodeRadioChannelGroups = xmlDoc.CreateElement("radiochannelgroups");
        IList<RadioChannelGroup> radiochannelgroups = RadioChannelGroup.ListAll();
        foreach (RadioChannelGroup radiogroup in radiochannelgroups)
        {
          XmlNode nodeRadioChannelGroup = xmlDoc.CreateElement("radiochannelgroup");
          AddAttribute(nodeRadioChannelGroup, "GroupName", radiogroup.GroupName);
          AddAttribute(nodeRadioChannelGroup, "SortOrder", radiogroup.SortOrder.ToString());
          XmlNode nodeRadioGroupMap = xmlDoc.CreateElement("mappings");
          IList<RadioGroupMap> maps = radiogroup.ReferringRadioGroupMap();
          foreach (RadioGroupMap map in maps)
          {
            XmlNode nodeMap = xmlDoc.CreateElement("map");
            AddAttribute(nodeMap, "ChannelName", map.ReferencedChannel().Name);
            AddAttribute(nodeMap, "SortOrder", map.SortOrder.ToString());
            nodeRadioGroupMap.AppendChild(nodeMap);
          }
          nodeRadioChannelGroup.AppendChild(nodeRadioGroupMap);
          nodeRadioChannelGroups.AppendChild(nodeRadioChannelGroup);
        }
        rootElement.AppendChild(nodeRadioChannelGroups);
      }

      xmlDoc.AppendChild(rootElement);
      xmlDoc.Save(fileName);
      MessageBox.Show(this, "The selected items have been exported to " + fileName);
    }

    private static string GetNodeAttribute(XmlNode node, string attribute, string defaultValue)
    {
      if (node.Attributes[attribute] == null)
        return defaultValue;
      return node.Attributes[attribute].Value;
    }

    private void importButton_Click(object sender, EventArgs e)
    {
      bool importtv = imCheckTvChannels.Checked;
      bool importtvgroups = imCheckTvGroups.Checked;
      bool importradio = imCheckRadioChannels.Checked;
      bool importradiogroups = imCheckRadioGroups.Checked;
      bool importschedules = imCheckSchedules.Checked;

      openFileDialog1.CheckFileExists = true;
      openFileDialog1.DefaultExt = "xml";
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.Title = "Load channels, channel groups and schedules";
      openFileDialog1.InitialDirectory = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
      openFileDialog1.FileName = "export.xml";
      openFileDialog1.AddExtension = true;
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
        return;
      NotifyForm dlg = new NotifyForm("Importing tv channels...", "This can take some time\n\nPlease be patient...");
      try
      {
        dlg.Show();
        dlg.WaitForDisplay();
        CountryCollection collection = new CountryCollection();
        TvBusinessLayer layer = new TvBusinessLayer();
        bool mergeChannels = false; // every exported channel will be imported on its own.
        int channelCount = 0;
        int scheduleCount = 0;
        int tvChannelGroupCount = 0;
        int radioChannelGroupCount = 0;

        if (layer.Channels.Count > 0 && (importtv || importradio))
        {
          // rtv: we could offer to set a "merge" property here so tuningdetails would be updated for existing channels.
          if (MessageBox.Show("Existing channels detected! \nIf you continue to import your old backup then all identically named channels will be treated equal - there is a risk of duplicate entries. \nDo you really want to go on?", "Channels found", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
            return;
          else
            mergeChannels = true;
        }

        XmlDocument doc = new XmlDocument();
        Log.Info("TvChannels: Trying to import channels from {0}", openFileDialog1.FileName);
        doc.Load(openFileDialog1.FileName);
        XmlNodeList channelList = doc.SelectNodes("/tvserver/channels/channel");
        XmlNodeList tvChannelGroupList = doc.SelectNodes("/tvserver/channelgroups/channelgroup");
        XmlNodeList radioChannelGroupList = doc.SelectNodes("/tvserver/radiochannelgroups/radiochannelgroup");
        XmlNodeList scheduleList = doc.SelectNodes("/tvserver/schedules/schedule");
        if (channelList != null)
        {
          foreach (XmlNode nodeChannel in channelList)
          {
            try
            {
              Channel dbChannel;
              XmlNodeList tuningList = nodeChannel.SelectNodes("TuningDetails/tune");
              XmlNodeList mappingList = nodeChannel.SelectNodes("mappings/map");
              string name = nodeChannel.Attributes["Name"].Value;
              bool grabEpg = (GetNodeAttribute(nodeChannel, "GrabEpg", "True") == "True");
              bool isRadio = (GetNodeAttribute(nodeChannel, "IsRadio", "False") == "True");
              bool isTv = (GetNodeAttribute(nodeChannel, "IsTv", "True") == "True");
              DateTime lastGrabTime = DateTime.ParseExact(GetNodeAttribute(nodeChannel, "LastGrabTime", "01.01.1900"), "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
              int sortOrder = Int32.Parse(GetNodeAttribute(nodeChannel, "SortOrder", "0"));
              int timesWatched = Int32.Parse(GetNodeAttribute(nodeChannel, "TimesWatched", "0"));
              DateTime totalTimeWatched = DateTime.ParseExact(GetNodeAttribute(nodeChannel, "TotalTimeWatched", "01.01.1900"), "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
              bool visibileInGuide = (GetNodeAttribute(nodeChannel, "VisibleInGuide", "True") == "True");
              bool FreeToAir = (GetNodeAttribute(nodeChannel, "FreeToAir", "True") == "True");
              string displayName = GetNodeAttribute(nodeChannel, "DisplayName", name);

              // Only import TV or radio channels if the corresponding checkbox was checked
              if ((isTv && !importtv) || (isRadio && !importradio))
                continue;

              channelCount++;

              // rtv: since analog allows NOT to merge channels we need to take care of this. US users e.g. have multiple stations named "Sport" with different tuningdetails.
              // using AddChannel would incorrectly "merge" these totally different channels.
              // see this: http://forum.team-mediaportal.com/1-0-rc1-svn-builds-271/importing-exported-channel-list-groups-channels-39368/
              Log.Info("TvChannels: Adding {0}. channel: {1} ({2})", channelCount, name, displayName);
              if (mergeChannels)
              {
                dbChannel = layer.GetChannelByName(name) ?? layer.AddChannel("", name);
              }
              else
                dbChannel = layer.AddNewChannel(name);

              dbChannel.GrabEpg = grabEpg;
              dbChannel.IsRadio = isRadio;
              dbChannel.IsTv = isTv;
              dbChannel.LastGrabTime = lastGrabTime;
              dbChannel.SortOrder = sortOrder;
              dbChannel.TimesWatched = timesWatched;
              dbChannel.TotalTimeWatched = totalTimeWatched;
              dbChannel.VisibleInGuide = visibileInGuide;
              dbChannel.FreeToAir = FreeToAir;
              dbChannel.DisplayName = displayName;
              dbChannel.Persist();
              foreach (XmlNode nodeMap in mappingList)
              {
                int idCard = Int32.Parse(nodeMap.Attributes["IdCard"].Value);
                XmlNode nodeCard = doc.SelectSingleNode(String.Format("/tvserver/servers/server/cards/card[@IdCard={0}]", idCard));
                Card dbCard = layer.GetCardByDevicePath(nodeCard.Attributes["DevicePath"].Value);
                if (dbCard != null)
                {
                  layer.MapChannelToCard(dbCard, dbChannel, false);
                }
              }
              foreach (XmlNode nodeTune in tuningList)
              {
                int audioPid = Int32.Parse(nodeTune.Attributes["AudioPid"].Value);
                int bandwidth = Int32.Parse(nodeTune.Attributes["Bandwidth"].Value);
                int channelNumber = Int32.Parse(nodeTune.Attributes["ChannelNumber"].Value);
                int channelType = Int32.Parse(nodeTune.Attributes["ChannelType"].Value);
                int countryId = Int32.Parse(nodeTune.Attributes["CountryId"].Value);
                int diseqc = Int32.Parse(nodeTune.Attributes["Diseqc"].Value);
                bool fta = (nodeTune.Attributes["FreeToAir"].Value == "True");
                int frequency = Int32.Parse(nodeTune.Attributes["Frequency"].Value);
                int majorChannel = Int32.Parse(nodeTune.Attributes["MajorChannel"].Value);
                int minorChannel = Int32.Parse(nodeTune.Attributes["MinorChannel"].Value);
                int modulation = Int32.Parse(nodeTune.Attributes["Modulation"].Value);
                name = nodeTune.Attributes["Name"].Value;
                int networkId = Int32.Parse(nodeTune.Attributes["NetworkId"].Value);
                int pcrPid = Int32.Parse(nodeTune.Attributes["PcrPid"].Value);
                int pmtPid = Int32.Parse(nodeTune.Attributes["PmtPid"].Value);
                int polarisation = Int32.Parse(nodeTune.Attributes["Polarisation"].Value);
                string provider = GetNodeAttribute(nodeTune, "Provider", "");
                int serviceId = Int32.Parse(nodeTune.Attributes["ServiceId"].Value);
                int switchingFrequency = Int32.Parse(nodeTune.Attributes["SwitchingFrequency"].Value);
                int symbolrate = Int32.Parse(nodeTune.Attributes["Symbolrate"].Value);
                int transportId = Int32.Parse(nodeTune.Attributes["TransportId"].Value);
                int tuningSource = Int32.Parse(GetNodeAttribute(nodeTune, "TuningSource", "0"));
                int videoPid = Int32.Parse(GetNodeAttribute(nodeTune, "VideoPid", "-1"));
                int videoSource = Int32.Parse(GetNodeAttribute(nodeTune, "VideoSource", "0"));
                int audioSource = Int32.Parse(GetNodeAttribute(nodeTune, "AudioSource", "0"));
                bool isVCRSignal = (GetNodeAttribute(nodeChannel, "IsVCRSignal", "False") == "True");
                int SatIndex = Int32.Parse(GetNodeAttribute(nodeTune, "SatIndex", "-1"));
                int InnerFecRate = Int32.Parse(GetNodeAttribute(nodeTune, "InnerFecRate", "-1"));
                int band = Int32.Parse(GetNodeAttribute(nodeTune, "Band", "0"));
                int pilot = Int32.Parse(GetNodeAttribute(nodeTune, "Pilot", "-1"));
                int rollOff = Int32.Parse(GetNodeAttribute(nodeTune, "RollOff", "-1"));
                string url = GetNodeAttribute(nodeTune, "Url", "");
                int bitrate = Int32.Parse(GetNodeAttribute(nodeTune, "Bitrate", "0"));

                switch (channelType)
                {
                  case 0: //AnalogChannel
                    AnalogChannel analogChannel = new AnalogChannel();
                    analogChannel.ChannelNumber = channelNumber;
                    analogChannel.Country = collection.Countries[countryId];
                    analogChannel.Frequency = frequency;
                    analogChannel.IsRadio = isRadio;
                    analogChannel.IsTv = isTv;
                    analogChannel.Name = name;
                    analogChannel.TunerSource = (TunerInputType)tuningSource;
                    analogChannel.AudioSource = (AnalogChannel.AudioInputType)audioSource;
                    analogChannel.VideoSource = (AnalogChannel.VideoInputType)videoSource;
                    analogChannel.IsVCRSignal = isVCRSignal;
                    layer.AddTuningDetails(dbChannel, analogChannel);
                    Log.Info("TvChannels: Added tuning details for analog channel: {0} number: {1}", name, channelNumber);
                    break;
                  case 1: //ATSCChannel
                    ATSCChannel atscChannel = new ATSCChannel();
                    atscChannel.MajorChannel = majorChannel;
                    atscChannel.MinorChannel = minorChannel;
                    atscChannel.PhysicalChannel = channelNumber;
                    atscChannel.FreeToAir = fta;
                    atscChannel.Frequency = frequency;
                    atscChannel.IsRadio = isRadio;
                    atscChannel.IsTv = isTv;
                    atscChannel.Name = name;
                    atscChannel.NetworkId = networkId;
                    atscChannel.PcrPid = pcrPid;
                    atscChannel.PmtPid = pmtPid;
                    atscChannel.Provider = provider;
                    atscChannel.ServiceId = serviceId;
                    atscChannel.SymbolRate = symbolrate;
                    atscChannel.TransportId = transportId;
                    atscChannel.AudioPid = audioPid;
                    atscChannel.VideoPid = videoPid;
                    atscChannel.ModulationType = (ModulationType)modulation;
                    layer.AddTuningDetails(dbChannel, atscChannel);
                    Log.Info("TvChannels: Added tuning details for ATSC channel: {0} number: {1} provider: {2}", name, channelNumber, provider);
                    break;
                  case 2: //DVBCChannel
                    DVBCChannel dvbcChannel = new DVBCChannel();
                    dvbcChannel.ModulationType = (ModulationType)modulation;
                    dvbcChannel.FreeToAir = fta;
                    dvbcChannel.Frequency = frequency;
                    dvbcChannel.IsRadio = isRadio;
                    dvbcChannel.IsTv = isTv;
                    dvbcChannel.Name = name;
                    dvbcChannel.NetworkId = networkId;
                    dvbcChannel.PcrPid = pcrPid;
                    dvbcChannel.PmtPid = pmtPid;
                    dvbcChannel.Provider = provider;
                    dvbcChannel.ServiceId = serviceId;
                    dvbcChannel.SymbolRate = symbolrate;
                    dvbcChannel.TransportId = transportId;
                    layer.AddTuningDetails(dbChannel, dvbcChannel);
                    Log.Info("TvChannels: Added tuning details for DVB-C channel: {0} provider: {1}", name, provider);
                    break;
                  case 3: //DVBSChannel
                    DVBSChannel dvbsChannel = new DVBSChannel();
                    dvbsChannel.DisEqc = (DisEqcType)diseqc;
                    dvbsChannel.Polarisation = (Polarisation)polarisation;
                    dvbsChannel.SwitchingFrequency = switchingFrequency;
                    dvbsChannel.FreeToAir = fta;
                    dvbsChannel.Frequency = frequency;
                    dvbsChannel.IsRadio = isRadio;
                    dvbsChannel.IsTv = isTv;
                    dvbsChannel.Name = name;
                    dvbsChannel.NetworkId = networkId;
                    dvbsChannel.PcrPid = pcrPid;
                    dvbsChannel.PmtPid = pmtPid;
                    dvbsChannel.Provider = provider;
                    dvbsChannel.ServiceId = serviceId;
                    dvbsChannel.SymbolRate = symbolrate;
                    dvbsChannel.TransportId = transportId;
                    dvbsChannel.SatelliteIndex = SatIndex;
                    dvbsChannel.ModulationType = (ModulationType)modulation;
                    dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate)InnerFecRate;
                    dvbsChannel.BandType = (BandType)band;
                    dvbsChannel.Pilot = (Pilot)pilot;
                    dvbsChannel.Rolloff = (RollOff)rollOff;
                    dvbsChannel.LogicalChannelNumber = channelNumber;
                    dvbsChannel.VideoPid = videoPid;
                    dvbsChannel.AudioPid = audioPid;
                    layer.AddTuningDetails(dbChannel, dvbsChannel);
                    Log.Info("TvChannels: Added tuning details for DVB-S channel: {0} provider: {1}", name, provider);
                    break;
                  case 4: //DVBTChannel
                    DVBTChannel dvbtChannel = new DVBTChannel();
                    dvbtChannel.BandWidth = bandwidth;
                    dvbtChannel.FreeToAir = fta;
                    dvbtChannel.Frequency = frequency;
                    dvbtChannel.IsRadio = isRadio;
                    dvbtChannel.IsTv = isTv;
                    dvbtChannel.Name = name;
                    dvbtChannel.NetworkId = networkId;
                    dvbtChannel.PcrPid = pcrPid;
                    dvbtChannel.PmtPid = pmtPid;
                    dvbtChannel.Provider = provider;
                    dvbtChannel.ServiceId = serviceId;
                    dvbtChannel.TransportId = transportId;
                    dvbtChannel.VideoPid = videoPid;
                    dvbtChannel.AudioPid = audioPid;
                    dvbtChannel.LogicalChannelNumber = channelNumber;
                    layer.AddTuningDetails(dbChannel, dvbtChannel);
                    Log.Info("TvChannels: Added tuning details for DVB-T channel: {0} provider: {1}", name, provider);
                    break;
                  case 5: //Webstream
                    layer.AddWebStreamTuningDetails(dbChannel, url, bitrate);
                    break;
                }
              }
            }
            catch (Exception exc)
            {
              Log.Error("TvChannels: Failed to add channel - {0}", exc.Message);
            }
          }
        }

        if (tvChannelGroupList != null && importtvgroups)
        {
          // Import tv channel groups
          foreach (XmlNode nodeChannelGroup in tvChannelGroupList)
          {
            try
            {
              tvChannelGroupCount++;
              string groupName = nodeChannelGroup.Attributes["GroupName"].Value;
              int groupSortOrder = Int32.Parse(nodeChannelGroup.Attributes["SortOrder"].Value);
              ChannelGroup group = layer.GetGroupByName(groupName, groupSortOrder) ??
                                   new ChannelGroup(groupName, groupSortOrder);
              group.Persist();
              XmlNodeList mappingList = nodeChannelGroup.SelectNodes("mappings/map");
              foreach (XmlNode nodeMap in mappingList)
              {
                Channel channel = layer.GetChannelByName(nodeMap.Attributes["ChannelName"].Value);
                int sortOrder = Int32.Parse(GetNodeAttribute(nodeMap, "SortOrder", "9999"));
                if (channel != null)
                {
                  GroupMap map = new GroupMap(group.IdGroup, channel.IdChannel, sortOrder);
                  map.Persist();
                }
              }
            }
            catch (Exception exg)
            {
              Log.Error("TvChannels: Failed to add group - {0}", exg.Message);
            }
          }
        }

        if (radioChannelGroupList != null && importradiogroups)
        {
          // Import radio channel groups
          foreach (XmlNode nodeChannelGroup in radioChannelGroupList)
          {
            try
            {
              radioChannelGroupCount++;
              string groupName = nodeChannelGroup.Attributes["GroupName"].Value;
              int groupSortOrder = Int32.Parse(nodeChannelGroup.Attributes["SortOrder"].Value);
              RadioChannelGroup group = layer.GetRadioChannelGroupByName(groupName) ??
                                   new RadioChannelGroup(groupName, groupSortOrder);
              group.Persist();
              XmlNodeList mappingList = nodeChannelGroup.SelectNodes("mappings/map");
              foreach (XmlNode nodeMap in mappingList)
              {
                Channel channel = layer.GetChannelByName(nodeMap.Attributes["ChannelName"].Value);
                int sortOrder = Int32.Parse(GetNodeAttribute(nodeMap, "SortOrder", "9999"));
                if (channel != null)
                {
                  RadioGroupMap map = new RadioGroupMap(group.IdGroup, channel.IdChannel, sortOrder);
                  map.Persist();
                }
              }
            }
            catch (Exception exg)
            {
              Log.Error("Radio Channels: Failed to add group - {0}", exg.Message);
            }
          }
        }

        if (scheduleList != null && importschedules)
        {
          // Import schedules
          foreach (XmlNode nodeSchedule in scheduleList)
          {
            try
            {
              int idChannel = -1;

              string programName = nodeSchedule.Attributes["ProgramName"].Value;
              string channel = nodeSchedule.Attributes["ChannelName"].Value;
              if (!string.IsNullOrEmpty(channel))
              {
                idChannel = layer.GetChannelByName(channel).IdChannel;
              }
              DateTime startTime = DateTime.ParseExact(nodeSchedule.Attributes["StartTime"].Value, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
              DateTime endTime = DateTime.ParseExact(nodeSchedule.Attributes["EndTime"].Value, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
              int scheduleType = Int32.Parse(nodeSchedule.Attributes["ScheduleType"].Value);
              Schedule schedule = layer.AddSchedule(idChannel, programName, startTime, endTime, scheduleType);

              schedule.ScheduleType = scheduleType;
              schedule.KeepDate = DateTime.ParseExact(nodeSchedule.Attributes["KeepDate"].Value, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
              schedule.PreRecordInterval = Int32.Parse(nodeSchedule.Attributes["PreRecordInterval"].Value);
              schedule.PostRecordInterval = Int32.Parse(nodeSchedule.Attributes["PostRecordInterval"].Value);
              schedule.Priority = Int32.Parse(nodeSchedule.Attributes["Priority"].Value);
              schedule.Quality = Int32.Parse(nodeSchedule.Attributes["Quality"].Value);
              schedule.Directory = nodeSchedule.Attributes["Directory"].Value;
              schedule.KeepMethod = Int32.Parse(nodeSchedule.Attributes["KeepMethod"].Value);
              schedule.MaxAirings = Int32.Parse(nodeSchedule.Attributes["MaxAirings"].Value);
              schedule.RecommendedCard = Int32.Parse(nodeSchedule.Attributes["RecommendedCard"].Value);
              schedule.ScheduleType = Int32.Parse(nodeSchedule.Attributes["ScheduleType"].Value);
              schedule.Series = (GetNodeAttribute(nodeSchedule, "Series", "False") == "True");
              if (idChannel > -1)
              {
                schedule.Persist();
                scheduleCount++;
                Log.Info("TvChannels: Added schedule: {0} on channel: {1}", programName, channel);
              }
              else
                Log.Info("TvChannels: Skipped schedule: {0} because the channel was unknown: {1}", programName, channel);
            }
            catch (Exception ex)
            {
              Log.Error("TvChannels: Failed to add schedule - {0}", ex.Message);
            }
          }
        }

        dlg.Close();
        Log.Info("TvChannels: Imported {0} channels, {1} tv channel groups, {2} radio channel groups and {3} schedules", channelCount, tvChannelGroupCount, radioChannelGroupCount, scheduleCount);
        MessageBox.Show(String.Format("Imported {0} channels, {1} tv channel groups, {2} radio channel groups and {3} schedules", channelCount, tvChannelGroupCount, radioChannelGroupCount, scheduleCount));
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, "Error while importing:\n\n" + ex + " " + ex.StackTrace);
      }
      finally
      {
        dlg.Close();
        OnSectionActivated();
      }
    }

    private void exportButton_Click(object sender, EventArgs e)
    {
      saveFileDialog1.CheckFileExists = false;
      saveFileDialog1.DefaultExt = "xml";
      saveFileDialog1.RestoreDirectory = true;
      saveFileDialog1.Title = "Save channels, channel groups and schedules";
      saveFileDialog1.InitialDirectory = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
      saveFileDialog1.FileName = "export.xml";
      saveFileDialog1.AddExtension = true;
      if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        NotifyForm dlg = new NotifyForm("Exporting tv channels...", "This can take some time\n\nPlease be patient...");
        dlg.Show();
        dlg.WaitForDisplay();
        Export(saveFileDialog1.FileName, exCheckTVChannels.Checked, exCheckTVGroups.Checked, exCheckRadioChannels.Checked, exCheckRadioGroups.Checked, exCheckSchedules.Checked);
        dlg.Close();
      }
    }

  }
}
