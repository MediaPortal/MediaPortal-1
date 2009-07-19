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
using System.Windows.Forms;
using TvDatabase;
using TvLibrary;
using TvLibrary.Implementations;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace SetupTv.Sections
{
  public partial class FormEditChannel : Form
  {
    bool _analog;
    bool _dvbt;
    bool _dvbc;
    bool _dvbs;
    bool _atsc;
    bool _dvbip;
    bool _newChannel;
    bool _isTv = true;
    bool _webstream;
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
        _channel.DisplayName = textBoxName.Text;
        _channel.VisibleInGuide = checkBoxVisibleInTvGuide.Checked;
        _channel.IsTv = _isTv;
        _channel.IsRadio = !_isTv;
        _channel.Persist();

        //Analog
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
              analogChannel.TunerSource = comboBoxInput.SelectedIndex == 1 ? TunerInputType.Cable : TunerInputType.Antenna;
              analogChannel.VideoSource = (AnalogChannel.VideoInputType)comboBoxVideoSource.SelectedIndex;
              analogChannel.AudioSource = (AnalogChannel.AudioInputType)comboBoxAudioSource.SelectedIndex;
              analogChannel.Frequency = (int)GetFrequency(textBoxAnalogFrequency.Text, "2");
              analogChannel.IsVCRSignal = checkBoxVCR.Checked;
              layer.AddTuningDetails(_channel, analogChannel);
            }
          }
        }

        //ATSC
        if (textBoxProgram.Text.Length != 0)
        {
          int physical, frequency, major, minor, onid, tsid, sid, pmt;
          if (Int32.TryParse(textBoxProgram.Text, out physical))
          {
            if (Int32.TryParse(textBoxFrequency.Text, out frequency))
            {
              if (Int32.TryParse(textBoxMajor.Text, out major))
              {
                if (Int32.TryParse(textBoxMinor.Text, out minor))
                {
                  if (Int32.TryParse(textBoxQamONID.Text, out onid))
                  {
                    if (Int32.TryParse(textBoxQamTSID.Text, out tsid))
                    {
                      if (Int32.TryParse(textBoxQamSID.Text, out sid))
                      {
                        if (Int32.TryParse(textBoxQamPmt.Text, out pmt))
                        {
                          if (physical > 0 && major >= 0 && minor >= 0)
                          {
                            ATSCChannel atscChannel = new ATSCChannel();
                            atscChannel.IsTv = _isTv;
                            atscChannel.IsRadio = !_isTv;
                            atscChannel.Name = _channel.Name;
                            atscChannel.PhysicalChannel = physical;
                            atscChannel.Frequency = frequency;
                            atscChannel.MajorChannel = major;
                            atscChannel.MinorChannel = minor;
                            atscChannel.Provider = textBoxQamProvider.Text;
                            atscChannel.FreeToAir = checkBoxQamfta.Checked;
                            switch (comboBoxQAMModulation.SelectedIndex)
                            {
                              case 0:
                                atscChannel.ModulationType = ModulationType.ModNotSet;
                                break;
                              case 1:
                                atscChannel.ModulationType = ModulationType.Mod8Vsb;
                                break;
                              case 2:
                                atscChannel.ModulationType = ModulationType.Mod64Qam;
                                break;
                              case 3:
                                atscChannel.ModulationType = ModulationType.Mod256Qam;
                                break;
                            }
                            atscChannel.NetworkId = onid;
                            atscChannel.TransportId = tsid;
                            atscChannel.ServiceId = sid;
                            atscChannel.PmtPid = pmt;
                            layer.AddTuningDetails(_channel, atscChannel);
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }

        //DVB-C
        if (textboxFreq.Text.Length != 0)
        {
          int freq, onid, tsid, sid, symbolrate, pmt;
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
                    if (Int32.TryParse(textBoxDVBCPmt.Text, out pmt))
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
                        dvbcChannel.PmtPid = pmt;
                        dvbcChannel.Provider = textBoxDVBCProvider.Text;
                        dvbcChannel.FreeToAir = checkBoxDVBCfta.Checked;
                        layer.AddTuningDetails(_channel, dvbcChannel);
                      }
                    }
                  }
                }
              }
            }
          }
        }

        //DVB-S
        if (textBox5.Text.Length != 0)
        {
          int lcn, freq, onid, tsid, sid, symbolrate, switchfreq, pmt;
          if (Int32.TryParse(textBoxDVBSChannel.Text, out lcn))
          {
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
                        if (Int32.TryParse(textBoxDVBSPmt.Text, out pmt))
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
                            dvbsChannel.Pilot = (Pilot)(comboBoxPilot.SelectedIndex - 1);
                            dvbsChannel.Rolloff = (RollOff)(comboBoxRollOff.SelectedIndex - 1);
                            dvbsChannel.ModulationType = (ModulationType)(comboBoxModulation.SelectedIndex - 1);
                            dvbsChannel.LogicalChannelNumber = lcn;
                            dvbsChannel.PmtPid = pmt;
                            dvbsChannel.Provider = textBoxDVBSProvider.Text;
                            dvbsChannel.FreeToAir = checkBoxDVBSfta.Checked;
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
                            /*IList satellites = Satellite.ListAll();
                            foreach (Satellite sat in satellites)
                            {
                              if (sat.SatelliteName == comboBoxSatellite.SelectedItem.ToString())
                              {
                                dvbsChannel.SatelliteIndex = sat.IdSatellite;
                                break;
                              }
                            }*/
                            layer.AddTuningDetails(_channel, dvbsChannel);
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }

        //DVB-T
        if (textBoxDVBTfreq.Text.Length != 0)
        {
          int lcn, freq, onid, tsid, sid, pmt;
          if (Int32.TryParse(textBoxDVBTChannel.Text, out lcn))
          {
            if (Int32.TryParse(textBoxDVBTfreq.Text, out freq))
            {
              if (Int32.TryParse(textBox8.Text, out onid))
              {
                if (Int32.TryParse(textBox7.Text, out tsid))
                {
                  if (Int32.TryParse(textBox6.Text, out sid))
                  {
                    if (Int32.TryParse(textBoxPmt.Text, out pmt))
                    {
                      if (onid > 0 && tsid >= 0 && sid >= 0)
                      {
                        DVBTChannel dvbtChannel = new DVBTChannel();
                        dvbtChannel.IsTv = _isTv;
                        dvbtChannel.IsRadio = !_isTv;
                        dvbtChannel.Name = _channel.Name;
                        dvbtChannel.LogicalChannelNumber = lcn;
                        dvbtChannel.Frequency = freq;
                        dvbtChannel.NetworkId = onid;
                        dvbtChannel.TransportId = tsid;
                        dvbtChannel.ServiceId = sid;
                        dvbtChannel.Provider = textBoxDVBTProvider.Text;
                        dvbtChannel.FreeToAir = checkBoxDVBTfta.Checked;
                        dvbtChannel.BandWidth = comboBoxBandWidth.SelectedIndex == 0 ? 7 : 8;
                        dvbtChannel.PmtPid = pmt;
                        layer.AddTuningDetails(_channel, dvbtChannel);
                      }
                    }
                  }
                }
              }
            }
          }
        }

        //DVB-IP
        if (textBoxDVBIPChannel.Text.Length != 0)
        {
          string url = textBoxDVBIPChannel.Text;
          int lcn, onid, tsid, sid, pmt;
          if (Int32.TryParse(textBoxDVBIPChannel.Text, out lcn))
          {
            if (Int32.TryParse(textBoxDVBIPNetworkId.Text, out onid))
            {
              if (Int32.TryParse(textBoxDVBIPTransportId.Text, out tsid))
              {
                if (Int32.TryParse(textBoxDVBIPServiceId.Text, out sid))
                {
                  if (Int32.TryParse(textBoxDVBIPPmtPid.Text, out pmt))
                  {
                    if (onid > 0 && tsid >= 0 && sid >= 0)
                    {
                      DVBIPChannel dvbipChannel = new DVBIPChannel();
                      dvbipChannel.IsTv = _isTv;
                      dvbipChannel.IsRadio = !_isTv;
                      dvbipChannel.Name = _channel.Name;
                      dvbipChannel.LogicalChannelNumber = lcn;
                      dvbipChannel.NetworkId = onid;
                      dvbipChannel.TransportId = tsid;
                      dvbipChannel.ServiceId = sid;
                      dvbipChannel.Provider = textBoxDVBIPProvider.Text;
                      dvbipChannel.FreeToAir = checkBoxDVBIPfta.Checked;
                      dvbipChannel.PmtPid = pmt;
                      dvbipChannel.Url = textBoxDVBIPUrl.Text;
                      layer.AddTuningDetails(_channel, dvbipChannel);
                    }
                  }
                }
              }
            }
          }
        }

        //Webstream
        if (edStreamURL.Text != "")
        {
          _channel.GrabEpg = false;
          _channel.Persist();
          layer.AddWebStreamTuningDetails(_channel, edStreamURL.Text, (int)nudStreamBitrate.Value);
        }

        DialogResult = DialogResult.OK;
        Close();
        return;
      }

      //General Tab
      _channel.DisplayName = textBoxName.Text;
      _channel.VisibleInGuide = checkBoxVisibleInTvGuide.Checked;

      foreach (TuningDetail detail in _channel.ReferringTuningDetail())
      {
        //Analog Tab
        if (detail.ChannelType == 0)
        {
          detail.ChannelNumber = Int32.Parse(textBoxChannel.Text);
          detail.CountryId = comboBoxCountry.SelectedIndex;
          if (comboBoxInput.SelectedIndex == 1)
            detail.TuningSource = (int)TunerInputType.Cable;
          else
            detail.TuningSource = (int)TunerInputType.Antenna;
          detail.VideoSource = comboBoxVideoSource.SelectedIndex;
          detail.AudioSource = comboBoxAudioSource.SelectedIndex;
          detail.Frequency = (int)GetFrequency(textBoxAnalogFrequency.Text, "2");
          detail.Persist();
        }

        //ATSC Tab
        if (detail.ChannelType == 1)
        {
          detail.ChannelNumber = Int32.Parse(textBoxProgram.Text);
          detail.Frequency = Int32.Parse(textBoxFrequency.Text);
          detail.MajorChannel = Int32.Parse(textBoxMajor.Text);
          detail.MinorChannel = Int32.Parse(textBoxMinor.Text);
          switch (comboBoxQAMModulation.SelectedIndex)
          {
            case 0:
              detail.Modulation = (int)ModulationType.ModNotSet;
              break;
            case 1:
              detail.Modulation = (int)ModulationType.Mod8Vsb;
              break;
            case 2:
              detail.Modulation = (int)ModulationType.Mod64Qam;
              break;
            case 3:
              detail.Modulation = (int)ModulationType.Mod256Qam;
              break;
          }
          detail.NetworkId = Int32.Parse(textBoxQamONID.Text);
          detail.TransportId = Int32.Parse(textBoxQamTSID.Text);
          detail.ServiceId = Int32.Parse(textBoxQamSID.Text);
          detail.PmtPid = Int32.Parse(textBoxQamPmt.Text);
          detail.Provider = textBoxQamProvider.Text;
          detail.FreeToAir = checkBoxQamfta.Checked;
          detail.Persist();
        }

        //DVB-C tab
        if (detail.ChannelType == 2)
        {
          detail.Frequency = Int32.Parse(textboxFreq.Text);
          detail.NetworkId = Int32.Parse(textBoxONID.Text);
          detail.TransportId = Int32.Parse(textBoxTSID.Text);
          detail.ServiceId = Int32.Parse(textBoxSID.Text);
          detail.Symbolrate = Int32.Parse(textBoxSymbolRate.Text);
          detail.PmtPid = Int32.Parse(textBoxDVBCPmt.Text);
          detail.Provider = textBoxDVBCProvider.Text;
          detail.FreeToAir = checkBoxDVBCfta.Checked;
          detail.Persist();
        }

        //DVB-S tab
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
          detail.Pilot = (int)(Pilot)(comboBoxPilot.SelectedIndex - 1);
          detail.RollOff = (int)(RollOff)(comboBoxRollOff.SelectedIndex - 1);
          detail.Modulation = (int)(ModulationType)(comboBoxModulation.SelectedIndex - 1);
          detail.ChannelNumber = Int32.Parse(textBoxDVBSChannel.Text);
          detail.PmtPid = Int32.Parse(textBoxDVBSPmt.Text);
          detail.Provider = textBoxDVBSProvider.Text;
          detail.FreeToAir = checkBoxDVBSfta.Checked;
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
          /*IList satellites = Satellite.ListAll();
          foreach (Satellite sat in satellites)
          {
            if (sat.SatelliteName == comboBoxSatellite.SelectedItem.ToString())
            {
              foreach (DiSEqCMotor motor in DiSEqCMotor.ListAll())
              {
                if (motor.IdSatellite == sat.IdSatellite)
                {
                  detail.SatIndex = motor.Position;
                }
              }
              break;
            }
          }*/
          detail.Diseqc = comboBoxDisEqc.SelectedIndex;
          detail.Persist();
        }

        //DVB-T tab
        if (detail.ChannelType == 4)
        {
          detail.ChannelNumber = Int32.Parse(textBoxDVBTChannel.Text);
          detail.Frequency = Int32.Parse(textBoxDVBTfreq.Text);
          detail.NetworkId = Int32.Parse(textBox8.Text);
          detail.TransportId = Int32.Parse(textBox7.Text);
          detail.ServiceId = Int32.Parse(textBox6.Text);
          detail.Provider = textBoxDVBTProvider.Text;
          detail.FreeToAir = checkBoxDVBTfta.Checked;
          detail.Bandwidth = comboBoxBandWidth.SelectedIndex == 0 ? 7 : 8;
          detail.PmtPid = Int32.Parse(textBoxPmt.Text);
          detail.Persist();
        }

        //DVB-IP tab
        if (detail.ChannelType == 7)
        {
          detail.ChannelNumber = Int32.Parse(textBoxDVBIPChannel.Text);
          detail.Url = textBoxDVBIPUrl.Text;
          detail.NetworkId = Int32.Parse(textBoxDVBIPNetworkId.Text);
          detail.TransportId = Int32.Parse(textBoxDVBIPTransportId.Text);
          detail.ServiceId = Int32.Parse(textBoxDVBIPServiceId.Text);
          detail.PmtPid = Int32.Parse(textBoxDVBIPPmtPid.Text);
          detail.Provider = textBoxDVBIPProvider.Text;
          detail.FreeToAir = checkBoxDVBIPfta.Checked;
          detail.Persist();
        }

        //Webstream tab
        if (detail.ChannelType == 5)
        {
          _webstream = true;
          detail.Url = edStreamURL.Text;
          detail.Bitrate = (int)nudStreamBitrate.Value;
          detail.Persist();
        }
      }
      DialogResult = DialogResult.OK;
      Close();
    }

    private void FormEditChannel_Load(object sender, EventArgs e)
    {
      if (_isTv)
        tabControl1.Controls.Remove(tabSHOUTcast);
      _newChannel = false;
      if (_channel == null)
      {
        _newChannel = true;
        Channel = new Channel("", false, true, 0, Schedule.MinSchedule, true, Schedule.MinSchedule, 10000, true, "", true, "");
      }
      CountryCollection countries = new CountryCollection();
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        comboBoxCountry.Items.Add(countries.Countries[i].Name);
      }

      /*comboBoxSatellite.Items.Clear();
      IList satellites = Satellite.ListAll();
      foreach (Satellite sat in satellites)
      {
        comboBoxSatellite.Items.Add(sat.SatelliteName);
      }
      if (comboBoxSatellite.Items.Count > 0)
        comboBoxSatellite.SelectedIndex = 0;*/
      comboBoxInput.SelectedIndex = 0;
      comboBoxCountry.SelectedIndex = 0;
      comboBoxDisEqc.SelectedIndex = 0;
      comboBoxPol.SelectedIndex = 0;
      comboBoxBandWidth.SelectedIndex = 1;
      comboBoxModulation.SelectedIndex = 0;
      comboBoxInnerFecRate.SelectedIndex = 0;
      comboBoxPilot.SelectedIndex = 0;
      comboBoxRollOff.SelectedIndex = 0;
      //General Tab
      if (_channel != null)
      {
        textBoxName.Text = _channel.DisplayName;
        checkBoxVisibleInTvGuide.Checked = _channel.VisibleInGuide;
      }
      if (_newChannel)
      {
        _analog = true;
        _dvbt = true;
        _dvbc = true;
        _dvbs = true;
        _atsc = true;
        _dvbip = true;
        _webstream = true;
        textBoxChannel.Text = "";
        textBoxProgram.Text = "";
        textboxFreq.Text = "";
        textBox5.Text = "";
        textBoxDVBTfreq.Text = "";
        return;
      }
      if (_channel != null)
        foreach (TuningDetail detail in _channel.ReferringTuningDetail())
        {
          //Analog Tab
          if (detail.ChannelType == 0 || _newChannel)
          {
            _analog = true;
            textBoxChannel.Text = detail.ChannelNumber.ToString();
            if (detail.TuningSource == (int)TunerInputType.Cable)
              comboBoxInput.SelectedIndex = 1;
            comboBoxCountry.SelectedIndex = detail.CountryId;
            comboBoxVideoSource.SelectedIndex = detail.VideoSource;
            comboBoxAudioSource.SelectedIndex = detail.AudioSource;
            textBoxAnalogFrequency.Text = SetFrequency(detail.Frequency, "2");
            checkBoxVCR.Checked = detail.IsVCRSignal;
          }

          //ATSC Tab
          if (detail.ChannelType == 1 || _newChannel)
          {
            _atsc = true;
            textBoxProgram.Text = detail.ChannelNumber.ToString();
            textBoxFrequency.Text = detail.Frequency.ToString();
            textBoxMajor.Text = detail.MajorChannel.ToString();
            textBoxMinor.Text = detail.MinorChannel.ToString();
            switch ((ModulationType)detail.Modulation)
            {
              case ModulationType.ModNotSet:
                comboBoxQAMModulation.SelectedIndex = 0;
                break;
              case ModulationType.Mod8Vsb:
                comboBoxQAMModulation.SelectedIndex = 1;
                break;
              case ModulationType.Mod64Qam:
                comboBoxQAMModulation.SelectedIndex = 2;
                break;
              case ModulationType.Mod256Qam:
                comboBoxQAMModulation.SelectedIndex = 3;
                break;
            }
            textBoxQamONID.Text = detail.NetworkId.ToString();
            textBoxQamTSID.Text = detail.TransportId.ToString();
            textBoxQamSID.Text = detail.ServiceId.ToString();
            textBoxQamPmt.Text = detail.PmtPid.ToString();
            textBoxQamProvider.Text = detail.Provider;
            checkBoxQamfta.Checked = detail.FreeToAir;
          }

          //DVB-C Tab
          if (detail.ChannelType == 2 || _newChannel)
          {
            _dvbc = true;
            textboxFreq.Text = detail.Frequency.ToString();
            textBoxONID.Text = detail.NetworkId.ToString();
            textBoxTSID.Text = detail.TransportId.ToString();
            textBoxSID.Text = detail.ServiceId.ToString();
            textBoxSymbolRate.Text = detail.Symbolrate.ToString();
            textBoxDVBCPmt.Text = detail.PmtPid.ToString();
            textBoxDVBCProvider.Text = detail.Provider;
            checkBoxDVBCfta.Checked = detail.FreeToAir;
          }

          //DVB-S Tab
          if (detail.ChannelType == 3 || _newChannel)
          {
            _dvbs = true;
            textBox5.Text = detail.Frequency.ToString();
            textBox4.Text = detail.NetworkId.ToString();
            textBox3.Text = detail.TransportId.ToString();
            textBox2.Text = detail.ServiceId.ToString();
            textBox1.Text = detail.Symbolrate.ToString();
            textBoxSwitch.Text = detail.SwitchingFrequency.ToString();
            textBoxDVBSChannel.Text = detail.ChannelNumber.ToString();
            textBoxDVBSPmt.Text = detail.PmtPid.ToString();
            textBoxDVBSProvider.Text = detail.Provider;
            checkBoxDVBSfta.Checked = detail.FreeToAir;
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
            comboBoxModulation.SelectedIndex = detail.Modulation + 1;
            comboBoxInnerFecRate.SelectedIndex = detail.InnerFecRate + 1;
            comboBoxPilot.SelectedIndex = detail.Pilot + 1;
            comboBoxRollOff.SelectedIndex = detail.RollOff + 1;
            comboBoxDisEqc.SelectedIndex = detail.Diseqc;
            foreach (DiSEqCMotor motor in DiSEqCMotor.ListAll())
            {
              if (detail.SatIndex == motor.Position)
              {
                Satellite.Retrieve(motor.IdSatellite);
                break;
              }
            }
            /*if (sat != null)
          {
            for (int i = 0; i < comboBoxSatellite.Items.Count; ++i)
            {
              if (comboBoxSatellite.Items[i].ToString() == sat.SatelliteName)
              {
                comboBoxSatellite.SelectedIndex = i;
                break;
              }
            }
          }*/
          }

          //DVB-T Tab
          if (detail.ChannelType == 4 || _newChannel)
          {
            _dvbt = true;
            textBoxDVBTChannel.Text = detail.ChannelNumber.ToString();
            textBoxDVBTfreq.Text = detail.Frequency.ToString();
            textBox8.Text = detail.NetworkId.ToString();
            textBox7.Text = detail.TransportId.ToString();
            textBox6.Text = detail.ServiceId.ToString();
            textBoxDVBTProvider.Text = detail.Provider;
            checkBoxDVBTfta.Checked = detail.FreeToAir;
            comboBoxBandWidth.SelectedIndex = detail.Bandwidth == 7 ? 0 : 1;
            textBoxPmt.Text = detail.PmtPid.ToString();
          }

          //DVB-IP Tab
          if (detail.ChannelType == 7 || _newChannel)
          {
            _dvbip = true;
            textBoxDVBIPChannel.Text = detail.ChannelNumber.ToString();
            textBoxDVBIPUrl.Text = detail.Url;
            textBoxDVBIPNetworkId.Text = detail.NetworkId.ToString();
            textBoxDVBIPTransportId.Text = detail.TransportId.ToString();
            textBoxDVBIPServiceId.Text = detail.ServiceId.ToString();
            textBoxDVBIPPmtPid.Text = detail.PmtPid.ToString();
            textBoxDVBIPProvider.Text = detail.Provider;
            checkBoxDVBIPfta.Checked = detail.FreeToAir;
          }

          //Webstream Tab
          if (detail.ChannelType == 5 || _newChannel)
          {
            _webstream = true;
            edStreamURL.Text = detail.Url;
            nudStreamBitrate.Value = detail.Bitrate;
          }

        }
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
        case 6:
          if (_dvbip == false)
          {
            tabControl1.SelectedIndex = 0;
            MessageBox.Show(this, "No DVB-IP tuning details available for this channel");
          }
          break;
        case 7:
          if (_webstream == false)
          {
            tabControl1.SelectedIndex = 0;
            MessageBox.Show(this, "No Webstream details available for this channel");
          }
          break;
      }
    }

    private void comboBoxCountry_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    static long GetFrequency(string text, string precision)
    {
      float tmp = 123.25f;
      if (tmp.ToString("f" + precision).IndexOf(',') > 0)
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

    static string SetFrequency(long frequency, string precision)
    {
      float freq = frequency;
      freq /= 1000000f;
      return freq.ToString("f" + precision);
    }

    private void btnSearchSHOUTcast_Click(object sender, EventArgs e)
    {
      SearchSHOUTcast dlg = new SearchSHOUTcast();
      dlg.ShowDialog(this);
      if (dlg.Station == null) return;
      textBoxName.Text = dlg.Station.name;
      edStreamURL.Text = dlg.Station.url;
      nudStreamBitrate.Value = dlg.Station.bitrate;
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }
  }
}
