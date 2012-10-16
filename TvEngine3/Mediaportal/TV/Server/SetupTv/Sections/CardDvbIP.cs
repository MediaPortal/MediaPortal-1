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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.PlaylistSupport;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbIP : SectionSettings
  {
    private int _cardNumber;
    private bool _isScanning = false;
    private bool _stopScanning = false;

    public CardDvbIP()
      : this("DvbIP") {}

    public CardDvbIP(string name)
      : base(name) {}

    public CardDvbIP(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }

    private void Init()
    {
      mpComboBoxService.Items.Clear();
      mpComboBoxService.Items.Add("SAP Announcements");
      String tuningFolder = String.Format(@"{0}\TuningParameters\dvbip", PathManager.GetDataPath);
      if (Directory.Exists(tuningFolder))
      {
        string[] files = Directory.GetFiles(tuningFolder, "*.m3u");
        foreach (string f in files)
        {
          mpComboBoxService.Items.Add(Path.GetFileNameWithoutExtension(f));
        }
      }
      mpComboBoxService.SelectedIndex = 0;
    }

    private void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardNumber));
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      
      int index = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbip" + _cardNumber.ToString() + "Service", "0").Value);
      if (index < mpComboBoxService.Items.Count) mpComboBoxService.SelectedIndex = index;
      
      checkBoxCreateGroups.Checked =
        (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbip" + _cardNumber.ToString() + "creategroups", "false").Value == "true");
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbip" + _cardNumber.ToString() + "Service", mpComboBoxService.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbip" + _cardNumber.ToString() + "creategroups", checkBoxCreateGroups.Checked ? "true" : "false");
    }

    private void CardDvbIP_Load(object sender, EventArgs e) {}

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        
        Card card = ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
          return;
        }
        else if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
          return;
        }
        // Check if the card is locked for scanning.
        IUser user;
        if (ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this,
                          "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid card?");
          return;
        }
        Thread scanThread = new Thread(new ThreadStart(DoScan));
        scanThread.Name = "DVB-IP scan thread";
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }

    private void DoScan()
    {
      int tvChannelsNew = 0;
      int radioChannelsNew = 0;
      int tvChannelsUpdated = 0;
      int radioChannelsUpdated = 0;

      string buttonText = mpButtonScanTv.Text;
      IUser user = new User();
      user.CardId = _cardNumber;
      try
      {
        // First lock the card, because so that other parts of a hybrid card can't be used at the same time
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;
        listViewStatus.Items.Clear();

        PlayList playlist = new PlayList();
        if (mpComboBoxService.SelectedIndex == 0)
        {
          //TODO read SAP announcements
        }
        else
        {
          IPlayListIO playlistIO =
            PlayListFactory.CreateIO(String.Format(@"{0}\TuningParameters\dvbip\{1}.m3u", PathManager.GetDataPath,
                                                   mpComboBoxService.SelectedItem));
          playlistIO.Load(playlist,
                          String.Format(@"{0}\TuningParameters\dvbip\{1}.m3u", PathManager.GetDataPath,
                                        mpComboBoxService.SelectedItem));
        }
        if (playlist.Count == 0) return;

        mpComboBoxService.Enabled = false;
        checkBoxCreateGroups.Enabled = false;
        checkBoxEnableChannelMoveDetection.Enabled = false;
        
        Card card = ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardNumber));

        int index = -1;
        IEnumerator<PlayListItem> enumerator = playlist.GetEnumerator();

        while (enumerator.MoveNext())
        {
          if (_stopScanning) return;
          index++;
          float percent = ((float)(index)) / playlist.Count;
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;

          string url = enumerator.Current.FileName.Substring(enumerator.Current.FileName.LastIndexOf('\\') + 1);
          string name = enumerator.Current.Description;

          DVBIPChannel tuneChannel = new DVBIPChannel();
          tuneChannel.Url = url;
          tuneChannel.Name = name;
          string line = String.Format("{0}- {1} - {2}", 1 + index, tuneChannel.Name, tuneChannel.Url);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          ServiceAgents.Instance.ControllerServiceAgent.Tune(user.Name, user.CardId, out user, tuneChannel, -1);
          IChannel[] channels;
          channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardNumber, tuneChannel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}- {1} :No Signal", 1 + index, tuneChannel.Url);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            else
            {
              line = String.Format("{0}- {1} :Nothing found", 1 + index, tuneChannel.Url);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
          }

          int newChannels = 0;
          int updatedChannels = 0;

          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBIPChannel channel = (DVBIPChannel)channels[i];
            if (channels.Length > 1)
            {
              if (channel.Name.IndexOf("Unknown") == 0)
              {
                channel.Name = name + (i + 1);
              }
            }
            else
            {
              channel.Name = name;
            }
            bool exists;
            TuningDetail currentDetail;
            //Check if we already have this tuningdetail. According to DVB-IP specifications there are two ways to identify DVB-IP
            //services: one ONID + SID based, the other domain/URL based. At this time we don't fully and properly implement the DVB-IP
            //specifications, so the safest method for service identification is the URL. The user has the option to enable the use of
            //ONID + SID identification and channel move detection...
            if (checkBoxEnableChannelMoveDetection.Checked)
            {
              TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.NetworkId;
              tuningDetailSearchEnum |= TuningDetailSearchEnum.ServiceId;
              currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetailCustom(channel, tuningDetailSearchEnum);              
            }
            else
            {
              currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetailByURL(channel, channel.Url);
            }

            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = ChannelFactory.CreateChannel(channel.Name);
              dbChannel.SortOrder = 10000;
              if (channel.LogicalChannelNumber >= 1)
              {
                dbChannel.SortOrder = channel.LogicalChannelNumber;
              }
              dbChannel.MediaType = (int) channel.MediaType;
              dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
              dbChannel.AcceptChanges();
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.Channel;
            }

            ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, channel.MediaType);
            MappingHelper.AddChannelToGroup(ref dbChannel, @group);                                          

            if (checkBoxCreateGroups.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(channel.Provider, channel.MediaType);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (currentDetail == null)
            {
              ServiceAgents.Instance.ChannelServiceAgent.AddTuningDetail(dbChannel.IdChannel, channel);
            }
            else
            {
              //update tuning details...
              ServiceAgents.Instance.ChannelServiceAgent.UpdateTuningDetail(dbChannel.IdChannel, currentDetail.IdTuning, channel);            
            }

            if (channel.MediaType == MediaTypeEnum.TV)
            {
              if (exists)
              {
                tvChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                tvChannelsNew++;
                newChannels++;
              }
            }
            if (channel.MediaType == MediaTypeEnum.Radio)
            {
              if (exists)
              {
                radioChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                radioChannelsNew++;
                newChannels++;
              }
            }            
            MappingHelper.AddChannelToCard(dbChannel, card, false);
            line = String.Format("{0}- {1} :New:{2} Updated:{3}", 1 + index, tuneChannel.Name, newChannels,
                                 updatedChannels);
            item.Text = line;
          }
        }
        //DatabaseManager.Instance.SaveChanges();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        mpComboBoxService.Enabled = true;
        checkBoxCreateGroups.Enabled = true;
        checkBoxEnableChannelMoveDetection.Enabled = true;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
      }
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem =
        listViewStatus.Items.Add(
          new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew,
                                         radioChannelsUpdated)));
      lastItem =
        listViewStatus.Items.Add(
          new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));
      lastItem.EnsureVisible();
    }
  }
}