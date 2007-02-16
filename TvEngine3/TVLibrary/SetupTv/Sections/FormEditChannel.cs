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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


using TvDatabase;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace SetupTv.Sections
{
  public partial class FormEditChannel : Form
  {
    bool _analog = false;
    bool _dvbt = false;
    bool _dvbc = false;
    bool _dvbs = false;
    bool _atsc = false;
    bool _newChannel = false;
    bool _isTv = true;
    Channel _channel;
    public FormEditChannel()
    {
      InitializeComponent();
    }

    public bool IsTv
    {
      get
      {
        return _isTv;
      }
      set
      {
        _isTv = value;
      }
    }


    public Channel Channel
    {
      get
      {
        return _channel;
      }
      set
      {
        _channel = value;
      }
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      CountryCollection countries = new CountryCollection();
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        comboBoxCountry.Items.Add(countries.Countries[i].Name);
      }


      if (_newChannel)
      {
        if (textBoxName.Text.Length == 0)
        {
          MessageBox.Show("Please enter a name for this channel");
          return;
        }
        _channel.Name = textBoxName.Text;
        _channel.VisibleInGuide = checkBoxVisibleInTvGuide.Checked;
        _channel.IsTv = _isTv;
        _channel.IsRadio = !_isTv;
        _channel.Persist();

        //analog
        TvBusinessLayer layer = new TvBusinessLayer();
        if (textBoxChannel.Text.Length != 0)
        {
          if (comboBoxCountry.SelectedIndex >= 0 && comboBoxVideoSource.SelectedIndex >= 0)
          {
            int channelNumber;
            if (Int32.TryParse(textBoxChannel.Text, out channelNumber))
            {
              AnalogChannel analogChannel = new AnalogChannel();
              analogChannel.IsTv = _isTv;
              analogChannel.IsRadio = !_isTv;
              analogChannel.Name = _channel.Name;
              analogChannel.ChannelNumber = channelNumber;
              analogChannel.Country = countries.Countries[comboBoxCountry.SelectedIndex];
              if (comboBoxInput.SelectedIndex == 1)
                analogChannel.TunerSource = TunerInputType.Cable;
              else
                analogChannel.TunerSource = TunerInputType.Antenna;
              analogChannel.VideoSource = (AnalogChannel.VideoInputType)comboBoxVideoSource.SelectedIndex;
              analogChannel.Frequency = (int)GetFrequency(textBoxAnalogFrequency.Text);

              layer.AddTuningDetails(_channel, analogChannel);
            }
          }
        }
        //atsc

        if (textBoxProgram.Text.Length != 0)
        {
          int physical, major, minor, audio, video;
          if (Int32.TryParse(textBoxProgram.Text, out physical))
          {
            if (Int32.TryParse(textBoxMajor.Text, out major))
            {
              if (Int32.TryParse(textBoxMinor.Text, out minor))
              {
                if (Int32.TryParse(textBoxAudioPid.Text, out audio))
                {
                  if (Int32.TryParse(textBoxVideoPid.Text, out video))
                  {
                    if (physical > 0 && major >= 0 && minor >= 0)
                    {
                      ATSCChannel atscChannel = new ATSCChannel();
                      atscChannel.IsTv = _isTv;
                      atscChannel.IsRadio = !_isTv;
                      atscChannel.Name = _channel.Name;
                      atscChannel.PhysicalChannel = physical;
                      atscChannel.MajorChannel = major;
                      atscChannel.MinorChannel = minor;
                      atscChannel.AudioPid = audio;
                      atscChannel.VideoPid = video;
                      layer.AddTuningDetails(_channel, atscChannel);
                    }
                  }
                }
              }
            }
          }
        }

        //dvbc
        if (textboxFreq.Text.Length != 0)
        {
          int freq, onid, tsid, sid, symbolrate;
          if (Int32.TryParse(textboxFreq.Text, out freq))
          {
            if (Int32.TryParse(textBoxONID.Text, out onid))
            {
              if (Int32.TryParse(textBoxTSID.Text, out tsid))
              {
                if (Int32.TryParse(textBoxSID.Text, out sid))
                {
                  if (Int32.TryParse(textBoxSymbolRate.Text, out symbolrate))
                  {
                    if (onid > 0 && tsid >= 0 && sid >= 0)
                    {
                      DVBCChannel dvbcChannel = new DVBCChannel();
                      dvbcChannel.IsTv = _isTv;
                      dvbcChannel.IsRadio = !_isTv;
                      dvbcChannel.Name = _channel.Name;
                      dvbcChannel.Frequency = freq;
                      dvbcChannel.NetworkId = onid;
                      dvbcChannel.TransportId = tsid;
                      dvbcChannel.ServiceId = sid;
                      dvbcChannel.SymbolRate = symbolrate;
                      layer.AddTuningDetails(_channel, dvbcChannel);
                    }
                  }
                }
              }
            }
          }
        }
        //dvbs

        if (textBox5.Text.Length != 0)
        {
          int freq, onid, tsid, sid, symbolrate, switchfreq;
          if (Int32.TryParse(textBox5.Text, out freq))
          {
            if (Int32.TryParse(textBox4.Text, out onid))
            {
              if (Int32.TryParse(textBox3.Text, out tsid))
              {
                if (Int32.TryParse(textBox2.Text, out sid))
                {
                  if (Int32.TryParse(textBox1.Text, out symbolrate))
                  {
                    if (Int32.TryParse(textBoxSwitch.Text, out switchfreq))
                    {
                      if (onid > 0 && tsid >= 0 && sid >= 0)
                      {
                        DVBSChannel dvbsChannel = new DVBSChannel();
                        dvbsChannel.IsTv = _isTv;
                        dvbsChannel.IsRadio = !_isTv;
                        dvbsChannel.Name = _channel.Name;
                        dvbsChannel.Frequency = freq;
                        dvbsChannel.NetworkId = onid;
                        dvbsChannel.TransportId = tsid;
                        dvbsChannel.ServiceId = sid;
                        dvbsChannel.SymbolRate = symbolrate;
                        dvbsChannel.SwitchingFrequency = switchfreq;
                        dvbsChannel.InnerFecRate = (BinaryConvolutionCodeRate)(comboBoxInnerFecRate.SelectedIndex - 1);
                        if (comboBoxModulation.SelectedIndex == 1)
                          dvbsChannel.ModulationType = ModulationType.Mod8Vsb;
                        else
                          dvbsChannel.ModulationType = ModulationType.ModQpsk;
                        switch (comboBoxPol.SelectedIndex)
                        {
                          case 0:
                            dvbsChannel.Polarisation = Polarisation.LinearH;
                            break;
                          case 1:
                            dvbsChannel.Polarisation = Polarisation.LinearV;
                            break;
                          case 2:
                            dvbsChannel.Polarisation = Polarisation.CircularL;
                            break;
                          case 3:
                            dvbsChannel.Polarisation = Polarisation.CircularR;
                            break;
                        }
                        dvbsChannel.DisEqc = (DisEqcType)comboBoxDisEqc.SelectedIndex;
                        
                        IList satellites = Satellite.ListAll();
                        foreach (Satellite sat in satellites)
                        {
                          if (sat.SatelliteName == comboBoxSatellite.SelectedItem.ToString())
                          {
                            dvbsChannel.SatelliteIndex = sat.IdSatellite;
                            break;
                          }
                        }
                        layer.AddTuningDetails(_channel, dvbsChannel);
                      }
                    }
                  }
                }
              }
            }
          }
        }

        //dvbt
        if (textBox9.Text.Length != 0)
        {
          int freq, onid, tsid, sid;
          if (Int32.TryParse(textBox9.Text, out freq))
          {
            if (Int32.TryParse(textBox8.Text, out onid))
            {
              if (Int32.TryParse(textBox7.Text, out tsid))
              {
                if (Int32.TryParse(textBox6.Text, out sid))
                {
                  if (onid > 0 && tsid >= 0 && sid >= 0)
                  {
                    DVBTChannel dvbtChannel = new DVBTChannel();
                    dvbtChannel.IsTv = _isTv;
                    dvbtChannel.IsRadio = !_isTv;
                    dvbtChannel.Name = _channel.Name;
                    dvbtChannel.Frequency = freq;
                    dvbtChannel.NetworkId = onid;
                    dvbtChannel.TransportId = tsid;
                    dvbtChannel.ServiceId = sid;
                    if (comboBoxBandWidth.SelectedIndex == 0)
                      dvbtChannel.BandWidth = 7;
                    else
                      dvbtChannel.BandWidth = 8;
                    layer.AddTuningDetails(_channel, dvbtChannel);
                  }
                }
              }
            }
          }
        }
        this.Close();
        return;
      }


      //general tab
      _channel.Name = textBoxName.Text;
      _channel.VisibleInGuide = checkBoxVisibleInTvGuide.Checked;

      foreach (TuningDetail detail in _channel.ReferringTuningDetail())
      {
        //analog tab
        if (detail.ChannelType == 0)
        {
          detail.ChannelNumber = Int32.Parse(textBoxChannel.Text);
          detail.CountryId = comboBoxCountry.SelectedIndex;
          if (comboBoxInput.SelectedIndex == 1)
            detail.TuningSource = (int)TunerInputType.Cable;
          else
            detail.TuningSource = (int)TunerInputType.Antenna;
          detail.VideoSource = comboBoxVideoSource.SelectedIndex;
          detail.Frequency = (int)GetFrequency(textBoxAnalogFrequency.Text);
          detail.Persist();
        }

        //ATSC tab
        if (detail.ChannelType == 1)
        {
          detail.ChannelNumber = Int32.Parse(textBoxProgram.Text);
          detail.MajorChannel = Int32.Parse(textBoxMajor.Text);
          detail.MinorChannel = Int32.Parse(textBoxMinor.Text);
          detail.AudioPid = Int32.Parse(textBoxAudioPid.Text);
          detail.VideoPid = Int32.Parse(textBoxVideoPid.Text);
          detail.Persist();
        }

        //DVBC tab
        if (detail.ChannelType == 2)
        {
          detail.Frequency = Int32.Parse(textboxFreq.Text);
          detail.NetworkId = Int32.Parse(textBoxONID.Text);
          detail.TransportId = Int32.Parse(textBoxTSID.Text);
          detail.ServiceId = Int32.Parse(textBoxSID.Text);
          detail.Symbolrate = Int32.Parse(textBoxSymbolRate.Text);
          detail.Persist();
        }

        //dvbs tab
        if (detail.ChannelType == 3)
        {
          _dvbs = true;
          detail.Frequency = Int32.Parse(textBox5.Text);
          detail.NetworkId = Int32.Parse(textBox4.Text);
          detail.TransportId = Int32.Parse(textBox3.Text);
          detail.ServiceId = Int32.Parse(textBox2.Text);
          detail.Symbolrate = Int32.Parse(textBox1.Text);
          detail.SwitchingFrequency = Int32.Parse(textBoxSwitch.Text);
          detail.InnerFecRate = (int)(BinaryConvolutionCodeRate)(comboBoxInnerFecRate.SelectedIndex - 1);
          if (comboBoxModulation.SelectedIndex == 1)
            detail.Modulation = (int)ModulationType.Mod8Vsb;
          else
            detail.Modulation = (int)ModulationType.ModQpsk;
          switch (comboBoxPol.SelectedIndex)
          {
            case 0:
              detail.Polarisation = (int)Polarisation.LinearH;
              break;
            case 1:
              detail.Polarisation = (int)Polarisation.LinearV;
              break;
            case 2:
              detail.Polarisation = (int)Polarisation.CircularL;
              break;
            case 3:
              detail.Polarisation = (int)Polarisation.CircularR;
              break;
          }

          IList satellites = Satellite.ListAll();
          foreach (Satellite sat in satellites)
          {
            if (sat.SatelliteName == comboBoxSatellite.SelectedItem.ToString())
            {
              detail.SatIndex = sat.IdSatellite;
              break;
            }
          }
          detail.Diseqc = comboBoxDisEqc.SelectedIndex;
          detail.Persist();
        }

        //dvbt tab
        if (detail.ChannelType == 4)
        {
          detail.Frequency = Int32.Parse(textBox9.Text);
          detail.NetworkId = Int32.Parse(textBox8.Text);
          detail.TransportId = Int32.Parse(textBox7.Text);
          detail.ServiceId = Int32.Parse(textBox6.Text);
          if (comboBoxBandWidth.SelectedIndex == 0)
            detail.Bandwidth = 7;
          else
            detail.Bandwidth = 8;
          detail.Persist();
        }
      }
      this.Close();
    }

    private void FormEditChannel_Load(object sender, EventArgs e)
    {
      _newChannel = false;
      if (Channel == null)
      {
        _newChannel = true;
        Channel = new Channel("", false, true, 0, Schedule.MinSchedule, true, Schedule.MinSchedule, 10000, true, "", true);
      }
      CountryCollection countries = new CountryCollection();
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        comboBoxCountry.Items.Add(countries.Countries[i].Name);
      }

      comboBoxSatellite.Items.Clear();
      IList satellites = Satellite.ListAll();
      foreach (Satellite sat in satellites)
      {
        comboBoxSatellite.Items.Add(sat.SatelliteName);
      }
      if (comboBoxSatellite.Items.Count > 0)
        comboBoxSatellite.SelectedIndex = 0;
      comboBoxInput.SelectedIndex = 0;
      comboBoxCountry.SelectedIndex = 0;
      comboBoxDisEqc.SelectedIndex = 0;
      comboBoxPol.SelectedIndex = 0;
      comboBoxBandWidth.SelectedIndex = 1;
      comboBoxModulation.SelectedIndex = 0;
      comboBoxInnerFecRate.SelectedIndex = 0;
      //general tab
      textBoxName.Text = _channel.Name;
      checkBoxVisibleInTvGuide.Checked = _channel.VisibleInGuide;

      if (_newChannel)
      {
        _analog = true;
        _dvbt = true;
        _dvbc = true;
        _dvbs = true;
        _atsc = true;
        textBoxChannel.Text = "";
        textBoxProgram.Text = "";
        textboxFreq.Text = "";
        textBox5.Text = "";
        textBox9.Text = "";
        return;
      }
      foreach (TuningDetail detail in _channel.ReferringTuningDetail())
      {

        //analog tab
        if (detail.ChannelType == 0 || _newChannel)
        {
          _analog = true;
          textBoxChannel.Text = detail.ChannelNumber.ToString();
          if (detail.TuningSource == (int)TunerInputType.Cable)
            comboBoxInput.SelectedIndex = 1;
          CountryCollection collection = new CountryCollection();
          comboBoxCountry.SelectedIndex = detail.CountryId;
          comboBoxVideoSource.SelectedIndex = detail.VideoSource;
          textBoxAnalogFrequency.Text = SetFrequency(detail.Frequency);
        }

        //ATSC tab
        if (detail.ChannelType == 1 || _newChannel)
        {
          _atsc = true;
          textBoxProgram.Text = detail.ChannelNumber.ToString();
          textBoxMajor.Text = detail.MajorChannel.ToString();
          textBoxMinor.Text = detail.MinorChannel.ToString();
          textBoxAudioPid.Text = detail.AudioPid.ToString();
          textBoxVideoPid.Text = detail.VideoPid.ToString();
        }

        //DVBC tab
        if (detail.ChannelType == 2 || _newChannel)
        {
          _dvbc = true;
          textboxFreq.Text = detail.Frequency.ToString();
          textBoxONID.Text = detail.NetworkId.ToString();
          textBoxTSID.Text = detail.TransportId.ToString();
          textBoxSID.Text = detail.ServiceId.ToString();
          textBoxSymbolRate.Text = detail.Symbolrate.ToString();
        }

        //dvbs tab
        if (detail.ChannelType == 3 || _newChannel)
        {
          _dvbs = true;
          textBox5.Text = detail.Frequency.ToString();
          textBox4.Text = detail.NetworkId.ToString();
          textBox3.Text = detail.TransportId.ToString();
          textBox2.Text = detail.ServiceId.ToString();
          textBox1.Text = detail.Symbolrate.ToString();
          textBoxSwitch.Text = detail.SwitchingFrequency.ToString();
          switch ((Polarisation)detail.Polarisation)
          {
            case Polarisation.LinearH:
              comboBoxPol.SelectedIndex = 0;
              break;
            case Polarisation.LinearV:
              comboBoxPol.SelectedIndex = 1;
              break;
            case Polarisation.CircularL:
              comboBoxPol.SelectedIndex = 2;
              break;
            case Polarisation.CircularR:
              comboBoxPol.SelectedIndex = 2;
              break;
          }
          if (((ModulationType)detail.Modulation) == ModulationType.Mod8Vsb)
            comboBoxModulation.SelectedIndex = 1;

          comboBoxInnerFecRate.SelectedIndex = 1 + detail.InnerFecRate;
          comboBoxDisEqc.SelectedIndex = (int)detail.Diseqc;
          Satellite sat=Satellite.Retrieve( detail.SatIndex);
          for (int i = 0; i < comboBoxSatellite.Items.Count; ++i)
          {
            if (comboBoxSatellite.Items[i].ToString() == sat.SatelliteName)
            {
              comboBoxSatellite.SelectedIndex = i;
              break;
            }
          }
        }

        //dvbt tab
        if (detail.ChannelType == 4 || _newChannel)
        {
          _dvbt = true;
          textBox9.Text = detail.Frequency.ToString();
          textBox8.Text = detail.NetworkId.ToString();
          textBox7.Text = detail.TransportId.ToString();
          textBox6.Text = detail.ServiceId.ToString();
          if (detail.Bandwidth == 7)
            comboBoxBandWidth.SelectedIndex = 0;
          else
            comboBoxBandWidth.SelectedIndex = 1;
        }
      }

    }

    private void comboBoxInput_TabIndexChanged(object sender, EventArgs e)
    {
    }
    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (tabControl1.SelectedIndex)
      {
        case 1:
          if (_analog == false)
          {
            tabControl1.SelectedIndex = 0;
            MessageBox.Show(this, "No analog tuning details available for this channel");
          }
          break;
        case 2:
          if (_dvbc == false)
          {
            tabControl1.SelectedIndex = 0;
            MessageBox.Show(this, "No DVB-C tuning details available for this channel");
          }
          break;
        case 3:
          if (_dvbs == false)
          {
            tabControl1.SelectedIndex = 0;
            MessageBox.Show(this, "No DVB-S tuning details available for this channel");
          }
          break;
        case 4:
          if (_dvbt == false)
          {
            tabControl1.SelectedIndex = 0;
            MessageBox.Show(this, "No DVB-T tuning details available for this channel");
          }
          break;
        case 5:
          if (_atsc == false)
          {
            tabControl1.SelectedIndex = 0;
            MessageBox.Show(this, "No ATSC tuning details available for this channel");
          }
          break;
      }
    }

    private void label3_Click(object sender, EventArgs e)
    {

    }

    private void comboBoxCountry_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void textBox10_TextChanged(object sender, EventArgs e)
    {

    }
    long GetFrequency(string text)
    {
      float tmp = 123.25f;
      if (tmp.ToString("f2").IndexOf(',') > 0)
      {
        text = text.Replace('.', ',');
      }
      else
      {
        text = text.Replace(',', '.');
      }
      float freq = float.Parse(text);
      freq *= 1000000f;
      return (long)freq;
    }

    string SetFrequency(long frequency)
    {
      float freq = frequency;
      freq /= 1000000f;
      return freq.ToString("f2");
    }

  }
}