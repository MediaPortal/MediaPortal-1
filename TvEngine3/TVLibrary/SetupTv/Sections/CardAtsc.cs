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
      progressBarLevel.Value = RemoteControl.Instance.SignalLevel(_cardNumber);
      progressBarQuality.Value = RemoteControl.Instance.SignalQuality(_cardNumber);

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
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        labelScan1.Text = "";
        labelScan2.Text = "";
        int tvChannelsNew = 0;
        int radioChannelsNew = 0;
        int tvChannelsUpdated = 0;
        int radioChannelsUpdated = 0;

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
          tuneChannel.PhysicalChannel = index;
          if (index == 2)
          {
            RemoteControl.Instance.Tune(_cardNumber, tuneChannel);
          }
          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();
          if (channels == null) continue;
          if (channels.Length == 0) continue;

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