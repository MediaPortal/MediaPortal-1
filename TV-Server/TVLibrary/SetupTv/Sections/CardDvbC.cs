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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using TvDatabase;

using TvControl;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using DirectShowLib.BDA;

namespace SetupTv.Sections
{
  public partial class CardDvbC : SectionSettings
  {
    struct DVBCList
    {
      public int frequency;		 // frequency
      public ModulationType modulation;	 // modulation
      public int symbolrate;	 // symbol rate
    }

    readonly int _cardNumber;
    readonly DVBCList[] _dvbcChannels = new DVBCList[1000];
    int _channelCount;
    bool _isScanning;
    bool _stopScanning;

    public CardDvbC()
      : this("DVBC")
    {
    }
    public CardDvbC(string name)
      : base(name)
    {
    }

    public CardDvbC(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }
    void LoadList(string fileName)
    {

      _channelCount = 0;
      string line;
      string[] tpdata;
      System.IO.TextReader tin = System.IO.File.OpenText(fileName);
      do
      {
        line = tin.ReadLine();
        if (line != null)
        {
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length != 3)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 3)
            {
              try
              {
                _dvbcChannels[_channelCount].frequency = Int32.Parse(tpdata[0]);
                string mod = tpdata[1].ToUpper();
                switch (mod)
                {
                  case "1024QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod1024Qam;
                    break;
                  case "112QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod112Qam;
                    break;
                  case "128QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod128Qam;
                    break;
                  case "160QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod160Qam;
                    break;
                  case "16QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod16Qam;
                    break;
                  case "16VSB":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod16Vsb;
                    break;
                  case "192QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod192Qam;
                    break;
                  case "224QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod224Qam;
                    break;
                  case "256QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod256Qam;
                    break;
                  case "320QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod320Qam;
                    break;
                  case "384QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod384Qam;
                    break;
                  case "448QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod448Qam;
                    break;
                  case "512QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod512Qam;
                    break;
                  case "640QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod640Qam;
                    break;
                  case "64QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod64Qam;
                    break;
                  case "768QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod768Qam;
                    break;
                  case "80QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod80Qam;
                    break;
                  case "896QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod896Qam;
                    break;
                  case "8VSB":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod8Vsb;
                    break;
                  case "96QAM":
                    _dvbcChannels[_channelCount].modulation = ModulationType.Mod96Qam;
                    break;
                  case "AMPLITUDE":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModAnalogAmplitude;
                    break;
                  case "FREQUENCY":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModAnalogFrequency;
                    break;
                  case "BPSK":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModBpsk;
                    break;
                  case "OQPSK":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModOqpsk;
                    break;
                  case "QPSK":
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModQpsk;
                    break;
                  default:
                    _dvbcChannels[_channelCount].modulation = ModulationType.ModNotSet;
                    break;
                }
                _dvbcChannels[_channelCount].symbolrate = Int32.Parse(tpdata[2]) / 1000;
                _channelCount += 1;
              } catch
              {
              }
            }
          }
        }
      } while (!(line == null));
      tin.Close();
    }

    void Init()
    {
      mpComboBoxCountry.Items.Clear();
      try
      {
        string[] files = System.IO.Directory.GetFiles("TuningParameters");
        for (int i = 0; i < files.Length; ++i)
        {
          string ext = System.IO.Path.GetExtension(files[i]).ToLowerInvariant();
          if (ext != ".dvbc")
            continue;
          string fileName = System.IO.Path.GetFileNameWithoutExtension(files[i]);
          mpComboBoxCountry.Items.Add(fileName);
        }
        mpComboBoxCountry.SelectedIndex = 0;
      } catch (Exception)
      {
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
      mpComboBoxMod.SelectedIndex = 3;
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      int index = Int32.Parse(layer.GetSetting("dvbc" + _cardNumber + "Country", "0").Value);
      if (index < mpComboBoxCountry.Items.Count)
        mpComboBoxCountry.SelectedIndex = index;


      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbc" + _cardNumber + "creategroups", "false").Value == "true");


    }
    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbc" + _cardNumber + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();
      //DatabaseManager.Instance.SaveChanges();
    }



    private void mpButtonScanTv_Click_1(object sender, EventArgs e)
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
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        LoadList(String.Format(@"Tuningparameters\{0}.dvbc", mpComboBoxCountry.SelectedItem));
        Thread scanThread = new Thread(DoScan);
        scanThread.Name = "DVB-C scan thread";
        scanThread.Start();
        listViewStatus.Items.Clear();
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
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        if (_channelCount == 0)
          return;

        mpComboBoxCountry.Enabled = false;
        mpButton1.Enabled = false;

        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        for (int index = 0; index < _channelCount; ++index)
        {
          if (_stopScanning)
            return;
          float percent = ((float)(index)) / _channelCount;
          percent *= 100f;
          if (percent > 100f)
            percent = 100f;
          progressBar1.Value = (int)percent;


          DVBCChannel tuneChannel = new DVBCChannel();
          tuneChannel.Frequency = _dvbcChannels[index].frequency;
          tuneChannel.ModulationType = _dvbcChannels[index].modulation;
          tuneChannel.SymbolRate = _dvbcChannels[index].symbolrate;
          string line = String.Format("{0}tp- {1} {2} {3}", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          if (index == 0)
          {
            RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          }

          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();

          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}tp- {1} {2} {3}:No signal", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            line = String.Format("{0}tp- {1} {2} {3}:Nothing found", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }

          int newChannels = 0;
          int updatedChannels = 0;
          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBCChannel channel = (DVBCChannel)channels[i];
            //TuningDetail currentDetail = layer.GetChannel(channel);
            TuningDetail currentDetail = layer.GetChannel(channel.Provider, channel.Name, channel.ServiceId);
            bool exists;
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

            layer.AddChannelToGroup(dbChannel, "All Channels");

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
            line = String.Format("{0}tp- {1} {2} {3}:New:{4} Updated:{5}", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate, newChannels, updatedChannels);
            item.Text = line;
          }
        }

        //DatabaseManager.Instance.SaveChanges();

      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpButton1.Enabled = true;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
      }
      listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radioChannelsNew, radioChannelsUpdated)));
      listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tvChannelsNew, tvChannelsUpdated)));
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem.EnsureVisible();
    }

    private void mpComboBoxCountry_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpBeveledLine1_Load(object sender, EventArgs e)
    {

    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      if (card.Enabled == false)
      {
        MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
        return;
      }
      if (!RemoteControl.Instance.CardPresent(card.IdCard))
      {
        MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
        return;
      }
      RemoteControl.Instance.EpgGrabberEnabled = false;
      mpButton1.Enabled = false;
      DVBCChannel tuneChannel = new DVBCChannel();
      tuneChannel.Frequency = Int32.Parse(textBoxFreq.Text);
      tuneChannel.ModulationType = (ModulationType)mpComboBoxMod.SelectedIndex;
      tuneChannel.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);

      listViewStatus.Items.Clear();
      string line = String.Format("Scan freq:{0} {1} symbolrate:{2} ...", tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
      item.EnsureVisible();
      Application.DoEvents();
      IChannel[] channels = RemoteControl.Instance.ScanNIT(_cardNumber, tuneChannel);
      if (channels != null)
      {
        for (int i = 0; i < channels.Length; ++i)
        {
          DVBCChannel ch = (DVBCChannel)channels[i];
          line = String.Format("{0}) {1} freq:{2} mod:{3} symbolrate:{4}", i, ch.Name, ch.Frequency, ch.ModulationType, ch.SymbolRate);
          item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();
          _dvbcChannels[i] = new DVBCList();
          _dvbcChannels[i].frequency = (int)ch.Frequency / 10;
          _dvbcChannels[i].modulation = ch.ModulationType;
          _dvbcChannels[i].symbolrate = ch.SymbolRate / 10;

        }
        _channelCount = channels.Length;
      }

      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Scan done, found {0} transponders...", _channelCount)));
      lastItem.EnsureVisible();
      mpButton1.Enabled = true;

      RemoteControl.Instance.EpgGrabberEnabled = true;
      if (_channelCount != 0)
      {
        if (DialogResult.Yes == MessageBox.Show(String.Format("Found {0} transponders. Would you like to scan those?", _channelCount), "Manual scan results", MessageBoxButtons.YesNo))
        {
          Thread scanThread = new Thread(DoScan);
          scanThread.Name = "DVB-C scan thread";
          scanThread.Start();
        }
      }
    }

    private void CardDvbC_Load(object sender, EventArgs e)
    {

    }

  }
}
