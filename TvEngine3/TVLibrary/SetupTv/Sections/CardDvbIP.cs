/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Playlists;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class CardDvbIP : SectionSettings
  {
    int _cardNumber;
    bool _isScanning = false;
    bool _stopScanning = false;

    public CardDvbIP()
      : this("DvbIP")
    {
    }
    public CardDvbIP(string name)
      : base(name)
    {
    }

    public CardDvbIP(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }

    void Init()
    {
      mpComboBoxService.Items.Clear();
      mpComboBoxService.Items.Add("SAP Announcements");
      string[] files = Directory.GetFiles(Utils.ApplicationDirectory + @"\TuningParameters\iptv", "*.m3u");
      foreach (string f in files)
      {
        mpComboBoxService.Items.Add(Path.GetFileNameWithoutExtension(f));
      }
      mpComboBoxService.SelectedIndex = 0;
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
      int index = Int32.Parse(layer.GetSetting("dvbip" + _cardNumber.ToString() + "Service", "0").Value);
      if (index < mpComboBoxService.Items.Count) mpComboBoxService.SelectedIndex = index;


      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbip" + _cardNumber.ToString() + "creategroups", "false").Value == "true");

    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbip" + _cardNumber.ToString() + "Service", "0");
      setting.Value = mpComboBoxService.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbip" + _cardNumber.ToString() + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();
    }

    private void CardDvbIP_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        else if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning another part of a hybrid card.");
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

    void DoScan()
    {
      int tvChannelsNew = 0;
      int radioChannelsNew = 0;
      int tvChannelsUpdated = 0;
      int radioChannelsUpdated = 0;

      string buttonText = mpButtonScanTv.Text;
      User user = new User();
      user.CardId = _cardNumber;
      try
      {
        // First lock the card, because so that other parts of a hybrid card can't be used at the same time
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        listViewStatus.Items.Clear();

        PlayList playlist = new PlayList();
        if (mpComboBoxService.SelectedIndex == 0)
        {
          //TODO read SAP announcements
        }
        else
        {
          IPlayListIO playlistIO =
            PlayListFactory.CreateIO(Utils.ApplicationDirectory + @"\TuningParameters\iptv\" + mpComboBoxService.SelectedItem + ".m3u");
          playlistIO.Load(playlist, Utils.ApplicationDirectory + @"\TuningParameters\iptv\" + mpComboBoxService.SelectedItem + ".m3u");
        }
        if (playlist.Count == 0) return;

        mpComboBoxService.Enabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

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
          RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          IChannel[] channels;
          channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();
          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
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

          bool exists;
          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBIPChannel channel = (DVBIPChannel)channels[i];
            //TuningDetail currentDetail = layer.GetChannel(channel);
//            if (channel.Name.IndexOf("Unknown") == 0)
//            {
            if (channels.Length > 1)
            {
                if (channel.Name.IndexOf("Unknown") == 0)
                {
                    channel.Name = name + (i + 1);
                }
            } else
            {
                channel.Name = name;
            }
//            }
            TuningDetail currentDetail = layer.GetChannel(channel.Provider, channel.Name, channel.ServiceId);
            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = layer.AddChannel(channel.Provider, channel.Name);
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
            dbChannel.FreeToAir = channel.FreeToAir;
            dbChannel.Persist();

            if (checkBoxCreateGroups.Checked)
            {
              layer.AddChannelToGroup(dbChannel, channel.Provider);
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
            line = String.Format("{0}- {1} :New:{2} Updated:{3}", 1 + index, tuneChannel.Name, newChannels, updatedChannels);
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
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        mpComboBoxService.Enabled = true;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
      }
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));
      lastItem.EnsureVisible();
    }
  }
}
