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
      mpLabelTunerLocked.Text = "No";
      if (RemoteControl.Instance.TunerLocked(_cardNumber))
        mpLabelTunerLocked.Text = "Yes";
      progressBarLevel.Value = RemoteControl.Instance.SignalLevel(_cardNumber);
      progressBarQuality.Value = RemoteControl.Instance.SignalQuality(_cardNumber);

      DVBTChannel channel = RemoteControl.Instance.CurrentChannel(_cardNumber) as DVBTChannel;
      if (channel == null)
        mpLabelChannel.Text = "none";
      else
        mpLabelChannel.Text = String.Format("Frequency:{0} Bandwidth:{1}", channel.Frequency, channel.BandWidth, channel.NetworkId);
      
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      labelScan1.Text = "";
      labelScan2.Text = "";
      TvBusinessLayer layer = new TvBusinessLayer();
      mpComboBoxCountry.SelectedIndex = Int32.Parse(layer.GetSetting("dvbt" + _cardNumber.ToString() + "Country", "0").Value);
    }


    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.GetSetting("dvbt" + _cardNumber.ToString() + "Country", "0").Value = mpComboBoxCountry.SelectedIndex.ToString();
      DatabaseManager.Instance.SaveChanges();
    }


    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      Thread scanThread = new Thread(new ThreadStart(DoScan));
      scanThread.Start();
    }
    void DoScan()
    {
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        int tvChannelsNew = 0;
        int radioChannelsNew = 0;
        int tvChannelsUpdated = 0;
        int radioChannelsUpdated = 0;
        labelScan1.Text = "";
        labelScan2.Text = "";

        Dictionary<int, int> frequencies = new Dictionary<int, int>();
        XmlDocument doc = new XmlDocument();
        doc.Load(@"TuningParameters\dvbt.xml");
        XmlNodeList list = doc.SelectNodes("/dvbt/country");
        foreach (XmlNode node in list)
        {
          XmlNode attribute = node.Attributes.GetNamedItem("name");
          if (attribute.Value != mpComboBoxCountry.SelectedItem.ToString()) continue;
          XmlNodeList nodesFreqs = node.SelectNodes("carrier");
          foreach (XmlNode nodeFreq in nodesFreqs)
          {
            string frequency = nodeFreq.Attributes.GetNamedItem("frequency").Value;
            string bandwidth = "8";
            if (nodeFreq.Attributes.GetNamedItem("bandwidth") != null)
            {
              bandwidth = nodeFreq.Attributes.GetNamedItem("bandwidth").Value;
            }
            frequencies.Add(Int32.Parse(frequency), Int32.Parse(bandwidth));
          }
        }
        if (frequencies.Count == 0) return;

        mpButtonScanTv.Enabled = false;
        mpComboBoxCountry.Enabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        int index = 0;
        Dictionary<int, int>.Enumerator enumerator = frequencies.GetEnumerator();
        while (enumerator.MoveNext())
        {
          float percent = ((float)(index)) / frequencies.Count;
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;

          KeyValuePair<int, int> values = enumerator.Current;
          DVBTChannel tuneChannel = new DVBTChannel();
          tuneChannel.Frequency = values.Key;
          tuneChannel.BandWidth = values.Value;
          if (index == 0)
          {
            RemoteControl.Instance.Tune(_cardNumber, tuneChannel);
          }
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);

          UpdateStatus();
          index++;
          if (channels == null) continue;
          if (channels.Length == 0) continue;

          for (int i = 0; i < channels.Length; ++i)
          {
            DVBTChannel channel = (DVBTChannel)channels[i];

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
            labelScan1.Text = String.Format("Tv channels New:{0} Updated:{1}", tvChannelsNew, tvChannelsUpdated);
            labelScan2.Text = String.Format("Radio channels New:{0} Updated:{1}", radioChannelsNew, radioChannelsUpdated);
            layer.MapChannelToCard(card, dbChannel);

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

    private void CardDvbT_Load(object sender, EventArgs e)
    {

    }
  }
}