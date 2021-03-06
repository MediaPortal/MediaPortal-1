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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using TvDatabase;
using TvControl;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using DirectShowLib.BDA;
using System.Collections.Generic;

namespace SetupTv.Sections
{
  public partial class CardAtsc : SectionSettings
  {
    [Serializable]
    public class ATSCTuning
    {
      public int frequency; // frequency
      public ATSCTuning() {}

      public ATSCTuning(int f)
      {
        frequency = f;
      }
    }

    private readonly int _cardNumber;
    private List<ATSCTuning> _atscChannels = new List<ATSCTuning>();
    private bool _isScanning;
    private bool _stopScanning;
    private FileFilters fileFilters;

    public CardAtsc()
      : this("ATSC") {}

    public CardAtsc(string name)
      : base(name) {}

    public CardAtsc(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
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
      TvBusinessLayer layer = new TvBusinessLayer();
      mpComboBoxTuningMode.SelectedItem = layer.GetSetting("atsc" + _cardNumber + "TuningMode", "ATSC Digital Terrestrial").Value;
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("atsc" + _cardNumber + "TuningMode", "ATSC Digital Terrestrial");
      setting.Value = (string)mpComboBoxTuningMode.SelectedItem;
      setting.Persist();
    }

    private void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        mpComboBoxTuningMode.Enabled = false;

        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
          return;
        }
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
          return;
        }
        // Check if the card is locked for scanning.
        IUser user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this,
                          "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid card?");
          return;
        }
        SimpleFileName tuningFile = (SimpleFileName)mpComboBoxFrequencies.SelectedItem;
        _atscChannels = (List<ATSCTuning>)fileFilters.LoadList(tuningFile.FileName, typeof (List<ATSCTuning>));
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
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        if (_atscChannels.Count == 0)
          return;
        mpComboBoxFrequencies.Enabled = false;
        listViewStatus.Items.Clear();
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        IUser user = new User();
        user.CardId = _cardNumber;
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
            Log.Info("ATSC: scanning clear QAM cable, {0}, frequency plan = {1}", line, mpComboBoxFrequencies.SelectedItem);
          }
          else if ((string)mpComboBoxTuningMode.SelectedItem == "ATSC Digital Terrestrial")
          {
            tuneChannel.PhysicalChannel = index;
            tuneChannel.Frequency = -1;
            tuneChannel.ModulationType = ModulationType.Mod8Vsb;
            line = string.Format("physical channel = {0}, modulation = 8 VSB", tuneChannel.PhysicalChannel);
            Log.Info("ATSC: scanning ATSC over-the-air, {0}", line);
          }
          else
          {
            tuneChannel.PhysicalChannel = 0;
            tuneChannel.ModulationType = ModulationType.Mod256Qam;
            line = "out-of-band service information";
            Log.Info("ATSC: scanning digital cable, {0}", line);
          }
          line += "... ";
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          if (index == minchan)
          {
            RemoteControl.Instance.Scan(ref user, tuneChannel, -1);
          }
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (tuneChannel.PhysicalChannel > 0 && !RemoteControl.Instance.TunerLocked(_cardNumber))
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
            //No support for channel moving, or merging with existing channels here.
            //We do not know how ATSC works to correctly implement this.
            TuningDetail currentDetail = layer.GetTuningDetail(channel);
            if (currentDetail != null)
            {
              if (channel.IsDifferentTransponder(layer.GetTuningChannel(currentDetail)))
              {
                currentDetail = null;
              }
            }
            bool exists;
            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = layer.AddNewChannel(channel.Name, channel.LogicalChannelNumber);
              dbChannel.SortOrder = 10000;
              if (channel.LogicalChannelNumber >= 1)
              {
                dbChannel.SortOrder = channel.LogicalChannelNumber;
              }
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.ReferencedChannel();
            }
            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            dbChannel.Persist();
            if (dbChannel.IsTv)
            {
              layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.AllChannels);
            }
            if (dbChannel.IsRadio)
            {
              layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.AllChannels);
            }
            if (currentDetail == null)
            {
              layer.AddTuningDetails(dbChannel, channel);
            }
            else
            {
              //update tuning details...
              TuningDetail td = layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
              td.Persist();
            }
            if (channel.IsTv)
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
            if (channel.IsRadio)
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
            layer.MapChannelToCard(card, dbChannel, false);
          }
          line += string.Format("new = {0}, updated = {1}", newChannels, updatedChannels);
          item.Text = line;
          Log.Info("ATSC: scan result, new = {0}, updated = {1}", newChannels, updatedChannels);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        IUser user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
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
      Log.Info("ATSC: scan summary, new TV = {0}, updated TV = {1}, new radio = {2}, updated radio = {3}", tvChannelsNew, tvChannelsUpdated, radioChannelsNew, radioChannelsUpdated);
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