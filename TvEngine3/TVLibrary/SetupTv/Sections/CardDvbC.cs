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

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

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
  public partial class CardDvbC : SectionSettings
  {
    struct DVBCList
    {
      public int frequency;		 // frequency
      public ModulationType modulation;	 // modulation
      public int symbolrate;	 // symbol rate
    }

    int _cardNumber;
    DVBCList[] _dvbcChannels = new DVBCList[1000];
    int _channelCount = 0;

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
      int LineNr = 0;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
        {
          LineNr++;
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
              }
              catch
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
          string ext = System.IO.Path.GetExtension(files[i]).ToLower();
          if (ext != ".dvbc") continue;
          string fileName = System.IO.Path.GetFileNameWithoutExtension(files[i]);
          mpComboBoxCountry.Items.Add(fileName);
        }
        mpComboBoxCountry.SelectedIndex = 0;
      }
      catch (Exception)
      {
        return;
      }
    }

    void UpdateStatus()
    {
      mpLabelTunerLocked.Text = "No";
      if (RemoteControl.Instance.TunerLocked(_cardNumber))
        mpLabelTunerLocked.Text = "Yes";
      progressBarLevel.Value = RemoteControl.Instance.SignalLevel(_cardNumber);
      progressBarQuality.Value = RemoteControl.Instance.SignalQuality(_cardNumber);

      DVBCChannel channel = RemoteControl.Instance.CurrentChannel(_cardNumber) as DVBCChannel;
      if (channel == null)
        mpLabelChannel.Text = "none";
      else
        mpLabelChannel.Text = String.Format("Frequency:{0} Modulation:{1}", channel.Frequency, channel.ModulationType);
      
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      labelScan1.Text = "";
      labelScan2.Text = "";
      TvBusinessLayer layer = new TvBusinessLayer();
      mpComboBoxCountry.SelectedIndex = Int32.Parse(layer.GetSetting("dvbc" + _cardNumber.ToString() + "Country", "0").Value);

    }
    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.GetSetting("dvbc" + _cardNumber.ToString() + "Country", "0").Value = mpComboBoxCountry.SelectedIndex.ToString();
      DatabaseManager.Instance.SaveChanges();
    }



    private void mpButtonScanTv_Click_1(object sender, EventArgs e)
    {
      Thread scanThread = new Thread(new ThreadStart(DoScan));
      scanThread.Start();
    }
    void DoScan()
    {
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        labelScan1.Text = "";
        labelScan2.Text = "";
        int tvChannelsNew = 0;
        int radioChannelsNew = 0;
        int tvChannelsUpdated = 0;
        int radioChannelsUpdated = 0;
        LoadList(String.Format(@"Tuningparameters\{0}.dvbc", mpComboBoxCountry.SelectedItem));
        if (_channelCount == 0) return;

        mpButtonScanTv.Enabled = false;
        mpComboBoxCountry.Enabled = false;

        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        for (int index = 0; index < _channelCount; ++index)
        {
          float percent = ((float)(index)) / _channelCount;
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;

          
          DVBCChannel tuneChannel = new DVBCChannel();
          tuneChannel.Frequency = _dvbcChannels[index].frequency;
          tuneChannel.ModulationType = _dvbcChannels[index].modulation;
          tuneChannel.SymbolRate = _dvbcChannels[index].symbolrate;

          if (index == 0)
          {
            RemoteControl.Instance.Tune(_cardNumber, tuneChannel);
          }
          
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();
          
          if (channels == null) continue;
          if (channels.Length == 0) continue;

          for (int i = 0; i < channels.Length; ++i)
          {
            DVBCChannel channel = (DVBCChannel)channels[i];

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
            layer.AddTuningDetails(dbChannel, channel);
            if (channel.IsTv)
            {
              if (exists)
                tvChannelsUpdated++;
              else
                tvChannelsNew++;
            }
            if (channel.IsRadio)
            {
              if (exists)
                radioChannelsUpdated++;
              else
                radioChannelsNew++;
            }
            layer.MapChannelToCard(card, dbChannel);

            labelScan1.Text = String.Format("Tv channels New:{0} Updated:{1}", tvChannelsNew, tvChannelsUpdated);
            labelScan2.Text = String.Format("Radio channels New:{0} Updated:{1}", radioChannelsNew, radioChannelsUpdated);
            
          }
        }

        progressBar1.Value = 100;
        mpButtonScanTv.Enabled = true;
        mpComboBoxCountry.Enabled = true;
        DatabaseManager.Instance.SaveChanges();
        
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.EpgGrabberEnabled = true;
      }
    }

  }
}