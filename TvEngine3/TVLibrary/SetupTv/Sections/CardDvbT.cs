/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using DirectShowLib;

using TvDatabase;

using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
namespace SetupTv.Sections
{
  public partial class CardDvbT : SectionSettings
  {

    int _cardNumber;

    public CardDvbT()
      : this("Analog")
    {
    }
    public CardDvbT(string name)
      : base(name)
    {
    }

    public CardDvbT(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }

    void Init()
    {
      mpComboBoxCountry.Items.Clear();
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(@"TuningParameters\dvbt.xml");
        XmlNodeList list = doc.SelectNodes("/dvbt/country");
        foreach (XmlNode node in list)
        {
          XmlNode attribute = node.Attributes.GetNamedItem("name");
          mpComboBoxCountry.Items.Add(attribute.Value);
        }
        mpComboBoxCountry.SelectedIndex = 0;
      }
      catch (Exception)
      {
        MessageBox.Show(@"Unable to open TuningParameters\dvbt.xml");
        return;
      }
    }

    void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));


    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      mpComboBoxCountry.SelectedIndex = Int32.Parse(layer.GetSetting("dvbt" + _cardNumber.ToString() + "Country", "0").Value);


      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      mpComboBoxCam.SelectedIndex = card.CamType;
      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbt" + _cardNumber.ToString() + "creategroups", "true").Value == "true");

    }


    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbt" + _cardNumber.ToString() + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbt" + _cardNumber.ToString() + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();
    }


    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      if (card.Enabled == false)
      {
        MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
        return;
      }
      Thread scanThread = new Thread(new ThreadStart(DoScan));
      scanThread.Start();
    }
    void DoScan()
    {
      int tvChannelsNew = 0;
      int radioChannelsNew = 0;
      int tvChannelsUpdated = 0;
      int radioChannelsUpdated = 0;
      int frequencyOffset = 0;
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        listViewStatus.Items.Clear();

        Dictionary<int, int> frequencies = new Dictionary<int, int>();
        XmlDocument doc = new XmlDocument();
        doc.Load(@"TuningParameters\dvbt.xml");
        XmlNodeList countryList = doc.SelectNodes("/dvbt/country");
        foreach (XmlNode nodeCountry in countryList)
        {
          XmlNode nodeName = nodeCountry.Attributes.GetNamedItem("name");
          if (nodeName.Value != mpComboBoxCountry.SelectedItem.ToString()) continue;
          XmlNode nodeOffset = nodeCountry.Attributes.GetNamedItem("offset");
          if (nodeOffset != null)
          {
            if (nodeOffset.Value != null)
            {
              if (Int32.TryParse(nodeOffset.Value, out frequencyOffset) == false)
              {
                frequencyOffset = 0;
              }
            }
          }
          XmlNodeList nodeFrequencyList = nodeCountry.SelectNodes("carrier");
          foreach (XmlNode nodeFrequency in nodeFrequencyList)
          {
            string frequencyText = nodeFrequency.Attributes.GetNamedItem("frequency").Value;
            string bandwidthText = "8";
            if (nodeFrequency.Attributes.GetNamedItem("bandwidth") != null)
            {
              bandwidthText = nodeFrequency.Attributes.GetNamedItem("bandwidth").Value;
            }
            int frequency = Int32.Parse(frequencyText);
            int bandWidth = Int32.Parse(bandwidthText);
            frequencies.Add(frequency, bandWidth);
          }
        }
        if (frequencies.Count == 0) return;

        mpButtonScanTv.Enabled = false;
        mpComboBoxCountry.Enabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        int index = -1;
        Dictionary<int, int>.Enumerator enumerator = frequencies.GetEnumerator();
        while (enumerator.MoveNext())
        {
          index++;
          float percent = ((float)(index)) / frequencies.Count;
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;

          KeyValuePair<int, int> values = enumerator.Current;
          DVBTChannel tuneChannel = new DVBTChannel();
          tuneChannel.Frequency = values.Key;
          tuneChannel.BandWidth = values.Value;

          string line = String.Format("{0}tp- {1} {2}", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          if (index == 0)
          {
            RemoteControl.Instance.Tune(_cardNumber, tuneChannel);
          }
          IChannel[] channels;
          channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          if (channels.Length == 0)
          {
            /// try frequency - offset
            tuneChannel.Frequency = values.Key - frequencyOffset;
            tuneChannel.BandWidth = values.Value;
            line = String.Format("{0}tp- {1} {2}", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
            item.Text = line;
            channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
            if (channels.Length == 0)
            {
              /// try frequency + offset
              tuneChannel.Frequency = values.Key + frequencyOffset;
              tuneChannel.BandWidth = values.Value;
              line = String.Format("{0}tp- {1} {2}", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
              item.Text = line;
              channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
            }
          }

          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}tp- {1} {2}:No Signal", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            else
            {
              line = String.Format("{0}tp- {1} {2}:Nothing found", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
          }

          int newChannels = 0;
          int updatedChannels = 0;

          for (int i = 0; i < channels.Length; ++i)
          {
            DVBTChannel channel = (DVBTChannel)channels[i];


            Channel dbChannel = layer.GetChannelByName(channel.Name);
            bool exists = (dbChannel != null);
            if (!exists)
            {
              dbChannel = layer.AddChannel(channel.Name);
            }

            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            if (dbChannel.IsRadio)
            {
              dbChannel.GrabEpg = false;
            }
            dbChannel.SortOrder = 10000;
            if (channel.LogicalChannelNumber >= 0)
            {
              dbChannel.SortOrder = channel.LogicalChannelNumber;
            }
            dbChannel.Persist();
            if (checkBoxCreateGroups.Checked)
            {
              layer.AddChannelToGroup(dbChannel, channel.Provider);
            }
            layer.AddTuningDetails(dbChannel, channel);

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
            layer.MapChannelToCard(card, dbChannel);
            line = String.Format("{0}tp- {1} {2}:New:{3} Updated:{4}", 1 + index, tuneChannel.Frequency, tuneChannel.BandWidth, newChannels, updatedChannels);
            item.Text = line;
          }
        }
        progressBar1.Value = 100;
        mpButtonScanTv.Enabled = true;
        mpComboBoxCountry.Enabled = true;
        //DatabaseManager.Instance.SaveChanges();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.EpgGrabberEnabled = true;
      }
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));
      lastItem.EnsureVisible();
    }

    private void CardDvbT_Load(object sender, EventArgs e)
    {

    }

    private void mpComboBoxCam_SelectedIndexChanged(object sender, EventArgs e)
    {

      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      card.CamType = mpComboBoxCam.SelectedIndex;
      card.Persist();
    }
  }
}