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
using DirectShowLib.BDA;

namespace SetupTv.Sections
{
  public partial class CardAtsc : SectionSettings
  {

    int _cardNumber;
    public CardAtsc()
      : this("DVBC")
    {
    }
    public CardAtsc(string name)
      : base(name)
    {
    }

    public CardAtsc(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
    }

    void UpdateStatus()
    {
      mpLabelTunerLocked.Text = "No";
      if (RemoteControl.Instance.TunerLocked(_cardNumber))
        mpLabelTunerLocked.Text = "Yes";
      progressBarLevel.Value = Math.Min(100,RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));

      ATSCChannel channel = RemoteControl.Instance.CurrentChannel(_cardNumber) as ATSCChannel;
      if (channel == null)
        mpLabelChannel.Text = "none";
      else
        mpLabelChannel.Text = String.Format("{0}", channel.PhysicalChannel);
    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      Thread scanThread = new Thread(new ThreadStart(DoScan));
      scanThread.Start();
    }
    void DoScan()
    {
      int tvChannelsNew = 0;
      int radioChannelsNew = 0;
      int tvChannelsUpdated = 0;
      int radioChannelsUpdated = 0;
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        listViewStatus.Items.Clear();

        mpButtonScanTv.Enabled = false;

        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        for (int index = 2; index <= 69; ++index)
        {
          float percent = ((float)(index)) / (69 - 2);
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;

          ATSCChannel tuneChannel = new ATSCChannel();
          tuneChannel.NetworkId = -1;
          tuneChannel.TransportId = -1;
          tuneChannel.ServiceId = -1;
          tuneChannel.MinorChannel = -1;
          tuneChannel.MajorChannel = -1;
          tuneChannel.Frequency = -1;
          tuneChannel.SymbolRate = -1;
          tuneChannel.PhysicalChannel = index;
          tuneChannel.ModulationType = ModulationType.ModNotSet;

          string line = String.Format("{0}tp- channel:{1}", 1 + index, tuneChannel.PhysicalChannel);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          if (index == 2)
          {
            RemoteControl.Instance.TuneScan(_cardNumber, tuneChannel);
          }
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();

          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = line = String.Format("{0}tp- channel:{1}:No signal", 1 + index, tuneChannel.PhysicalChannel);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            else
            {
              line = line = String.Format("{0}tp- channel:{1}:Nothing found", 1 + index, tuneChannel.PhysicalChannel);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
          }

          int newChannels = 0;
          int updatedChannels = 0;
          for (int i = 0; i < channels.Length; ++i)
          {
            ATSCChannel channel = (ATSCChannel)channels[i];

            bool exists = (layer.GetChannelByName(channel.Name) != null);
            Channel dbChannel = layer.AddChannel(channel.Name);
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
            layer.AddChannelToGroup(dbChannel, "ATSC");
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

            line = line = String.Format("{0}tp- channel:{1}:New:{2} Updated:{3}", 1 + index, tuneChannel.PhysicalChannel, newChannels, updatedChannels);
            item.Text = line;
          }
        }

        progressBar1.Value = 100;
        mpButtonScanTv.Enabled = true;
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
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));

      lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem.EnsureVisible();
    }
  }
}