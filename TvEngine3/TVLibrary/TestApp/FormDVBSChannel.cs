using System;
using System.Windows.Forms;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
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
        comboBoxPol.SelectedIndex = _channel.Polarisation == Polarisation.LinearH ? 0 : 1;
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
      _channel.Polarisation = comboBoxPol.SelectedIndex == 0 ? Polarisation.LinearH : Polarisation.LinearV;

      Close();
    }
  }
}