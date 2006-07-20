using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
using TvDatabase;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;
using DirectShowLib;
using DirectShowLib.BDA;

namespace SetupTv.Sections
{
  public partial class FormEditChannel : Form
  {
    bool _analog = false;
    bool _dvbt = false;
    bool _dvbc = false;
    bool _dvbs = false;
    bool _atsc = false;
    Channel _channel;
    public FormEditChannel()
    {
      InitializeComponent();
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

      comboBoxBandWidth.SelectedIndex = 1;

      //general tab
      _channel.Name = textBoxName.Text;
      _channel.VisibleInGuide = checkBoxVisibleInTvGuide.Checked;

      foreach (TuningDetail detail in _channel.TuningDetails)
      {
        //analog tab
        if (detail.ChannelType == 0)
        {
          detail.ChannelNumber = Int32.Parse(textBoxChannel.Text);
          detail.CountryId = countries.Countries[comboBoxCountry.SelectedIndex].Id;
          if (comboBoxInput.SelectedIndex == 1)
            detail.TuningSource = (int)TunerInputType.Cable;
          else
            detail.TuningSource = (int)TunerInputType.Antenna;
        }

        //ATSC tab
        if (detail.ChannelType == 1)
        {
          detail.ChannelNumber = Int32.Parse(textBox12.Text);
          detail.MajorChannel = Int32.Parse(textBox11.Text);
          detail.MinorChannel = Int32.Parse(textBox10.Text);
        }

        //DVBC tab
        if (detail.ChannelType == 2)
        {
          detail.Frequency = Int32.Parse(textboxFreq.Text);
          detail.NetworkId = Int32.Parse(textBoxONID.Text);
          detail.TransportId = Int32.Parse(textBoxTSID.Text);
          detail.ServiceId = Int32.Parse(textBoxSID.Text);
          detail.Symbolrate = Int32.Parse(textBoxSymbolRate.Text);
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

          if (comboBoxPol.SelectedIndex == 1)
            detail.Polarisation = (int)Polarisation.LinearH;
          else
            detail.Polarisation = (int)Polarisation.LinearV;

          detail.Diseqc = comboBoxDisEqc.SelectedIndex;
        }

        //dvbt tab
        if (detail.ChannelType == 4)
        {
          detail.Frequency = Int32.Parse(textBox9.Text);
          detail.NetworkId=Int32.Parse(textBox8.Text );
          detail.TransportId=Int32.Parse(textBox7.Text );
          detail.ServiceId=Int32.Parse(textBox6.Text );
          if (comboBoxBandWidth.SelectedIndex ==0)
            detail.Bandwidth =7;
          else
            detail.Bandwidth = 8;
        }
      }
      this.Close();
    }

    private void FormEditChannel_Load(object sender, EventArgs e)
    {
      CountryCollection countries = new CountryCollection();
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        comboBoxCountry.Items.Add(countries.Countries[i].Name);
      }


      comboBoxInput.SelectedIndex = 0;
      comboBoxCountry.SelectedIndex = 0;
      comboBoxDisEqc.SelectedIndex = 0;
      comboBoxPol.SelectedIndex = 0;
      comboBoxBandWidth.SelectedIndex = 1;

      //general tab
      textBoxName.Text = _channel.Name;
      checkBoxVisibleInTvGuide.Checked = _channel.VisibleInGuide;

      foreach (TuningDetail detail in _channel.TuningDetails)
      {

        //analog tab
        if (detail.ChannelType == 0)
        {
          _analog = true;
          textBoxChannel.Text = detail.ChannelNumber.ToString();
          for (int i = 0; i < countries.Countries.Length; ++i)
          {
            if (detail.CountryId == countries.Countries[i].Id)
            {
              comboBoxCountry.SelectedIndex = i;
              break;
            }
            if (detail.TuningSource == (int)TunerInputType.Cable)
              comboBoxInput.SelectedIndex = 1;
          }
        }

        //ATSC tab
        if (detail.ChannelType == 1)
        {
          _atsc = true;
          textBox12.Text = detail.ChannelNumber.ToString();
          textBox11.Text = detail.MajorChannel.ToString();
          textBox10.Text = detail.MinorChannel.ToString();
        }

        //DVBC tab
        if (detail.ChannelType == 2)
        {
          _dvbc = true;
          textboxFreq.Text = detail.Frequency.ToString();
          textBoxONID.Text = detail.NetworkId.ToString();
          textBoxTSID.Text = detail.TransportId.ToString();
          textBoxSID.Text = detail.ServiceId.ToString();
          textBoxSymbolRate.Text = detail.Symbolrate.ToString();
        }

        //dvbs tab
        if (detail.ChannelType == 3)
        {
          _dvbs = true;
          textBox5.Text = detail.Frequency.ToString();
          textBox4.Text = detail.NetworkId.ToString();
          textBox3.Text = detail.TransportId.ToString();
          textBox2.Text = detail.ServiceId.ToString();
          textBox1.Text = detail.Symbolrate.ToString();
          textBoxSwitch.Text = detail.SwitchingFrequency.ToString();

          if (detail.Polarisation != (int)Polarisation.LinearH)
            comboBoxPol.SelectedIndex = 1;

          comboBoxDisEqc.SelectedIndex = (int)detail.Diseqc;
        }

        //dvbt tab
        if (detail.ChannelType == 4)
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
          if (_analog == false) tabControl1.SelectedIndex = 0;
          break;
        case 2:
          if (_dvbc == false) tabControl1.SelectedIndex = 0;
          break;
        case 3:
          if (_dvbs == false) tabControl1.SelectedIndex = 0;
          break;
        case 4:
          if (_dvbt == false) tabControl1.SelectedIndex = 0;
          break;
        case 5:
          if (_atsc == false) tabControl1.SelectedIndex = 0;
          break;
      }
    }
  }
}