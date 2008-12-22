using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Channels;
using DirectShowLib;
using DirectShowLib.BDA;
namespace TestApp
{
  public partial class FormDVBSChannel : Form
  {
    DVBSChannel _channel = new DVBSChannel();
    public FormDVBSChannel()
    {
      InitializeComponent();
      comboBoxDisEqc.SelectedIndex = 0;
      comboBoxPol.SelectedIndex = 0;
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
    private void textBox1_TextChanged(object sender, EventArgs e)
    {

    }

    public IChannel Channel
    {
      get
      {
        _channel.BandType = BandType.Universal;
        _channel.SwitchingFrequency = -1;

        return _channel;
      }
      set
      {
        _channel = (DVBSChannel)value;
        textboxFreq.Text = _channel.Frequency.ToString();
        textBoxONID.Text = _channel.NetworkId.ToString();
        textBoxTSID.Text = _channel.TransportId.ToString();
        textBoxSID.Text = _channel.ServiceId.ToString();
        textBoxSymbolRate.Text = _channel.SymbolRate.ToString();
        textBoxSwitch.Text = _channel.SwitchingFrequency.ToString();
        comboBoxDisEqc.SelectedIndex = (int)_channel.DisEqc;
        if (_channel.Polarisation == Polarisation.LinearH)
          comboBoxPol.SelectedIndex = 0;
        else
          comboBoxPol.SelectedIndex = 1;
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      _channel.Frequency = Int32.Parse(textboxFreq.Text);
      _channel.NetworkId = Int32.Parse(textBoxONID.Text);
      _channel.TransportId = Int32.Parse(textBoxTSID.Text);
      _channel.ServiceId = Int32.Parse(textBoxSID.Text);
      _channel.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);
      _channel.SwitchingFrequency = Int32.Parse(textBoxSwitch.Text);
      _channel.DisEqc = (DisEqcType)comboBoxDisEqc.SelectedIndex;
      if (comboBoxPol.SelectedIndex == 0)
        _channel.Polarisation = Polarisation.LinearH;
      else
        _channel.Polarisation = Polarisation.LinearV;

      Close();
    }
  }
}