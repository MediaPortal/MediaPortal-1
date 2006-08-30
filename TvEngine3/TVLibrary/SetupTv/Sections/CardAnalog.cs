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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DirectShowLib;
using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;

using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;

namespace SetupTv.Sections
{
  public partial class CardAnalog : SectionSettings
  {
    int _cardNumber;

    public CardAnalog()
      : this("Analog")
    {
    }
    public CardAnalog(string name)
      : base(name)
    {
    }

    public CardAnalog(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();

    }
    void Init()
    {
      CountryCollection countries = new CountryCollection();
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        mpComboBoxCountry.Items.Add(countries.Countries[i]);
      }
      mpComboBoxCountry.SelectedIndex = 0;
      mpComboBoxSource.Items.Add(TunerInputType.Antenna);
      mpComboBoxSource.Items.Add(TunerInputType.Cable);
      mpComboBoxSource.SelectedIndex = 0;
    }

    void UpdateStatus()
    {
      mpLabelTunerLocked.Text = "No";
      if (RemoteControl.Instance.TunerLocked(_cardNumber))
        mpLabelTunerLocked.Text = "Yes";

      AnalogChannel channel = RemoteControl.Instance.CurrentChannel(_cardNumber) as AnalogChannel;
      if (channel == null)
        mpLabelChannel.Text = "none";
      else
        mpLabelChannel.Text = String.Format("#{0} {1}", channel.ChannelNumber, channel.Name);
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpComboBoxSensitivity.SelectedIndex = 1;
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      mpComboBoxCountry.SelectedIndex = Int32.Parse(layer.GetSetting("analog" + _cardNumber.ToString() + "Country", "0").Value);
      mpComboBoxSource.SelectedIndex = Int32.Parse(layer.GetSetting("analog" + _cardNumber.ToString() + "Source", "0").Value);
    }
    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.GetSetting("analog" + _cardNumber.ToString() + "Country", "0").Value = mpComboBoxCountry.SelectedIndex.ToString();
      layer.GetSetting("analog" + _cardNumber.ToString() + "Source", "0").Value = mpComboBoxSource.SelectedIndex.ToString();
      DatabaseManager.Instance.SaveChanges();
    }

    private void mpButtonScan_Click(object sender, EventArgs e)
    {
      Thread scanThread = new Thread(new ThreadStart(DoTvScan));
      scanThread.Start();
    }
    void DoTvScan()
    {
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        mpButtonScanRadio.Enabled = false;
        mpButtonScanTv.Enabled = false;
        UpdateStatus();
        mpListView1.Items.Clear();
        CountryCollection countries = new CountryCollection();
        RemoteControl.Instance.Tune(_cardNumber, new AnalogChannel());
        int minChannel = RemoteControl.Instance.MinChannel(_cardNumber);
        int maxChannel = RemoteControl.Instance.MaxChannel(_cardNumber);
        for (int channelNr = minChannel; channelNr <= maxChannel; channelNr++)
        {
          float percent = ((float)((channelNr - minChannel)) / (maxChannel-minChannel));
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;
          AnalogChannel channel = new AnalogChannel();
          if (mpComboBoxSource.SelectedIndex == 0)
            channel.TunerSource = TunerInputType.Antenna;
          else
            channel.TunerSource = TunerInputType.Cable;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.ChannelNumber = channelNr;
          channel.IsTv = true;
          channel.IsRadio = false;

          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, channel);
          UpdateStatus();
          if (channels == null) continue;
          if (channels.Length == 0) continue;

          channel = (AnalogChannel)channels[0];
          if (channel.Name == "") channel.Name = String.Format(channel.ChannelNumber.ToString());
          ListViewItem item = mpListView1.Items.Add(channel.ChannelNumber.ToString());
          item.SubItems.Add(channel.Name);
          mpListView1.EnsureVisible(mpListView1.Items.Count - 1);

          Channel dbChannel = layer.AddChannel(channel.Name);
          dbChannel.IsTv = channel.IsTv;
          dbChannel.IsRadio = channel.IsRadio;
          layer.AddTuningDetails(dbChannel, channel);


          layer.MapChannelToCard(card, dbChannel);
        }

        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        mpButtonScanTv.Enabled = true;
        DatabaseManager.Instance.SaveChanges();

      }
      finally
      {
        RemoteControl.Instance.EpgGrabberEnabled = true;
      }
    }

    private void mpButtonScanRadio_Click(object sender, EventArgs e)
    {
      Thread scanThread = new Thread(new ThreadStart(DoRadioScan));
      scanThread.Start();
    }

    int SignalStrength(int sensitivity)
    {
      int i = 0;
      for (i = 0; i < sensitivity * 2; i++)
      {
        if (!RemoteControl.Instance.TunerLocked(_cardNumber))
        {
          break;
        }
        System.Threading.Thread.Sleep(50);
      }
      return ((i * 50) / sensitivity);
    }
    void DoRadioScan()
    {
      int sensitivity = 1;
      switch (mpComboBoxSensitivity.Text)
      {
        case "High":
          sensitivity = 10;
          break;

        case "Medium":
          sensitivity = 2;
          break;

        case "Low":
          sensitivity = 1;
          break;
      }
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        mpComboBoxCountry.Enabled = false;
        mpComboBoxSource.Enabled = false;
        mpButtonScanRadio.Enabled = false;
        mpComboBoxSensitivity.Enabled = false;
        mpButtonScanTv.Enabled = false;
        UpdateStatus();
        mpListView1.Items.Clear();
        CountryCollection countries = new CountryCollection();

        for (int freq = 87500000; freq < 108000000; freq += 100000)
        {
          float percent = ((float)(freq - 87500000)) / (108000000f - 87500000f);
          percent *= 100f;
          if (percent > 100f) percent = 100f;
          progressBar1.Value = (int)percent;
          AnalogChannel channel = new AnalogChannel();
          channel.IsRadio = true;
          if (mpComboBoxSource.SelectedIndex == 0)
            channel.TunerSource = TunerInputType.Antenna;
          else
            channel.TunerSource = TunerInputType.Cable;
          channel.Country = countries.Countries[mpComboBoxCountry.SelectedIndex];
          channel.Frequency = freq;
          channel.IsTv = false;
          channel.IsRadio = true;

          RemoteControl.Instance.Tune(_cardNumber, channel);
          UpdateStatus();
          if (SignalStrength(sensitivity) == 100)
          {
            ListViewItem item = mpListView1.Items.Add(channel.Frequency.ToString());
            mpListView1.EnsureVisible(mpListView1.Items.Count - 1);


            channel.Name = String.Format("{0}", freq);
            Channel dbChannel = layer.AddChannel(channel.Name);
            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            layer.AddTuningDetails(dbChannel, channel);

            layer.MapChannelToCard(card, dbChannel);
          }
        }

        progressBar1.Value = 100;
        mpComboBoxCountry.Enabled = true;
        mpComboBoxSource.Enabled = true;
        mpButtonScanRadio.Enabled = true;
        mpButtonScanTv.Enabled = true;
        mpComboBoxSensitivity.Enabled = true;
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

    private void mpBeveledLine1_Load(object sender, EventArgs e)
    {

    }
  }
}