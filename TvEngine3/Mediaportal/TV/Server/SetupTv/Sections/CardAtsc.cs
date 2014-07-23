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
using System.Threading;
using System.Windows.Forms;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.SetupTV.Sections.CIMenu;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardAtsc : SectionSettings
  {
    [Serializable]
    public class ATSCTuning
    {
      public int frequency; // frequency
      public ATSCTuning() { }

      public ATSCTuning(int f)
      {
        frequency = f;
      }
    }

    private readonly int _cardId;
    private List<ATSCTuning> _atscChannels = new List<ATSCTuning>();
    private bool _isScanning;
    private bool _stopScanning;
    private FileFilters fileFilters;
    private CI_Menu_Dialog ciMenuDialog;

    public CardAtsc()
      : this("ATSC") { }

    public CardAtsc(string name)
      : base(name) { }

    public CardAtsc(string name, int cardId)
      : base(name)
    {
      _cardId = cardId;
      InitializeComponent();
      //insert complete ci menu dialog to tab
      Card dbCard = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId, CardIncludeRelationEnum.None);
      if (dbCard.UseConditionalAccess == true)
      {
        ciMenuDialog = new CI_Menu_Dialog(_cardId);
        tabPageCaMenu.Controls.Add(ciMenuDialog);
      }
      else
      {
        tabPageCaMenu.Dispose();
      }
      base.Text = name;
      Init();
    }

    private void Init()
    {
      UpdateQamFrequencyFieldAvailability();
      mpComboBoxFrequencies.Items.Clear();
      try
      {
        fileFilters = new FileFilters("ATSC");
        mpComboBoxFrequencies.DataSource = fileFilters.AllFiles;
        mpComboBoxFrequencies.ValueMember = "FileName";
        mpComboBoxFrequencies.DisplayMember = "DisplayName";
      }
      catch (Exception)
      {
        MessageBox.Show(@"Unable to open TuningParameters\atsc\*.xml");
        return;
      }
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      mpComboBoxTuningMode.SelectedItem = ServiceAgents.Instance.SettingServiceAgent.GetValue("atsc" + _cardId + "TuningMode", "ATSC Digital Terrestrial");

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionActivated();
      }
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("atsc" + _cardId + "TuningMode", (string)mpComboBoxTuningMode.SelectedItem);

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionDeActivated();
      }
    }

    private void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardId));
      progressBarQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardId));
    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      if (!_isScanning)
      {
        mpComboBoxTuningMode.Enabled = false;

        Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardId);
        if (!card.Enabled)
        {
          MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
          return;
        }
        if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(_cardId))
        {
          MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
          return;
        }
        // Check if the card is locked for scanning.
        IUser user;
        if (ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_cardId, out user))
        {
          MessageBox.Show(this,
                          "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid card?");
          return;
        }
        SimpleFileName tuningFile = (SimpleFileName)mpComboBoxFrequencies.SelectedItem;
        _atscChannels = (List<ATSCTuning>)fileFilters.LoadList(tuningFile.FileName, typeof(List<ATSCTuning>));
        if (_atscChannels == null)
        {
          return;
        }
        Thread scanThread = new Thread(DoScan);
        scanThread.Name = "ATSC scan thread";
        scanThread.Start();
        listViewStatus.Items.Clear();
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
      user.CardId = _cardId;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;
        if (_atscChannels.Count == 0)
          return;
        mpComboBoxFrequencies.Enabled = false;
        listViewStatus.Items.Clear();

        Card card = ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardId));
        int minchan = 2;
        int maxchan = 69 + 1;
        if ((string)mpComboBoxTuningMode.SelectedItem == "Clear QAM Cable")
        {
          minchan = 1;
          maxchan = _atscChannels.Count + 1;
        }
        else if ((string)mpComboBoxTuningMode.SelectedItem == "Digital Cable")
        {
          minchan = 0;
          maxchan = 1;
        }
        for (int index = minchan; index < maxchan; ++index)
        {
          if (_stopScanning)
            return;
          float percent = ((float)(index)) / (maxchan - minchan);
          percent *= 100f;
          if (percent > 100f)
            percent = 100f;
          progressBar1.Value = (int)percent;
          ATSCChannel tuneChannel = new ATSCChannel();
          tuneChannel.NetworkId = -1;
          tuneChannel.TransportId = -1;
          tuneChannel.ServiceId = -1;
          tuneChannel.MinorChannel = -1;
          tuneChannel.MajorChannel = -1;
          string line;
          if ((string)mpComboBoxTuningMode.SelectedItem == "Clear QAM Cable")
          {
            tuneChannel.PhysicalChannel = index;
            tuneChannel.Frequency = _atscChannels[index - 1].frequency;
            if (tuneChannel.Frequency < 10000)
            {
              continue;
            }
            tuneChannel.ModulationType = ModulationType.Mod256Qam;
            line = string.Format("physical channel = {0}, frequency = {1} kHz, modulation = 256 QAM", tuneChannel.PhysicalChannel, tuneChannel.Frequency);
            this.LogInfo("ATSC: scanning clear QAM cable, {0}, frequency plan = {1}", line, mpComboBoxFrequencies.SelectedItem);
          }
          else if ((string)mpComboBoxTuningMode.SelectedItem == "ATSC Digital Terrestrial")
          {
            tuneChannel.PhysicalChannel = index;
            tuneChannel.Frequency = -1;
            tuneChannel.ModulationType = ModulationType.Mod8Vsb;
            line = string.Format("physical channel = {0}, modulation = 8 VSB", tuneChannel.PhysicalChannel);
            this.LogInfo("ATSC: scanning ATSC over-the-air, {0}", line);
          }
          else
          {
            tuneChannel.PhysicalChannel = 0;
            tuneChannel.Frequency = 0;
            tuneChannel.ModulationType = ModulationType.Mod256Qam;
            line = "out-of-band service information";
            this.LogInfo("ATSC: scanning digital cable, {0}", line);
          }
          line += "...";
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          if (index == minchan)
          {
            ServiceAgents.Instance.ControllerServiceAgent.Scan(user.Name, user.CardId, out user, tuneChannel, -1);
          }
          IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardId, tuneChannel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (tuneChannel.PhysicalChannel > 0 && !ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardId))
            {
              line += "no signal";
            }
            else
            {
              line += "signal locked, no channels found";
            }
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
          int newChannels = 0;
          int updatedChannels = 0;
          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            ATSCChannel channel = (ATSCChannel)channels[i];
            //ATSC over-the-air channels don't generally move, but cable channels can occasionally.
            //ATSC SI has the MPEG 2 TSID and program number (SID), as well as its own source ID
            //which we currently store in the NetworkId field (TODO!). For CableCARD PSIP, source
            //ID is unique (by definition it has to be), but for clear QAM cable and ATSC over-the-air
            //it seems source ID isn't even always unique within a transport stream.
            TuningDetail currentDetail = null;
            if ((string)mpComboBoxTuningMode.SelectedItem == "Digital Cable" && channel.NetworkId != 0)
            {
              //CableCARD, support channel movement detection by source ID.
              currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetailCustom(channel, TuningDetailSearchEnum.NetworkId);
            }
            else
            {
              //No support for channel moving, or merging with existing channels here. Even include
              //the channel name as broadcasters don't always ensure TSID + SID is unique.
              TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.NetworkId | TuningDetailSearchEnum.TransportId | TuningDetailSearchEnum.ServiceId | TuningDetailSearchEnum.Name;
              currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetailCustom(channel, tuningDetailSearchEnum);
            }
            bool exists;
            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = ChannelFactory.CreateChannel(channel.Name);
              dbChannel.SortOrder = channel.LogicalChannelNumber;
              dbChannel.ChannelNumber = channel.LogicalChannelNumber;
              dbChannel.MediaType = (int)channel.MediaType;
              dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
              dbChannel.AcceptChanges();
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.Channel;
            }

            if (dbChannel.MediaType == (int)MediaTypeEnum.TV)
            {
              ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, group);
            }
            else if (dbChannel.MediaType == (int)MediaTypeEnum.Radio)
            {
              ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.AllChannels, MediaTypeEnum.Radio);
              MappingHelper.AddChannelToGroup(ref dbChannel, group);
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
          }
          line += string.Format("new = {0}, updated = {1}", newChannels, updatedChannels);
          item.Text = line;
          this.LogInfo("ATSC: scan result, new = {0}, updated = {1}", newChannels, updatedChannels);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        mpComboBoxTuningMode.Enabled = true;
        UpdateQamFrequencyFieldAvailability();
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
      }
      listViewStatus.Items.Add(
        new ListViewItem(String.Format("Total radio channels, new = {0}, updated = {1}", radioChannelsNew,
                                       radioChannelsUpdated)));
      listViewStatus.Items.Add(
        new ListViewItem(String.Format("Total TV channels, new = {0} updated = {1}", tvChannelsNew, tvChannelsUpdated)));
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done!"));
      lastItem.EnsureVisible();
      this.LogInfo("ATSC: scan summary, new TV = {0}, updated TV = {1}, new radio = {2}, updated radio = {3}", tvChannelsNew, tvChannelsUpdated, radioChannelsNew, radioChannelsUpdated);
    }

    private void mpComboBoxTuningMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateQamFrequencyFieldAvailability();
    }

    private void UpdateQamFrequencyFieldAvailability()
    {
      if ((string)mpComboBoxTuningMode.SelectedItem == "Clear QAM Cable")
      {
        mpComboBoxFrequencies.Enabled = true;
      }
      else
      {
        mpComboBoxFrequencies.Enabled = false;
      }
    }
  }
}