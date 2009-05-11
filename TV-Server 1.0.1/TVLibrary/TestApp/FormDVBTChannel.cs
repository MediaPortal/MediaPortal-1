using System;
using System.Windows.Forms;
using TvLibrary.Interfaces;
using TvLibrary.Channels;

namespace TestApp
{
  public partial class FormDVBTChannel : Form
  {
    DVBTChannel _channel = new DVBTChannel();
    public FormDVBTChannel()
    {
      InitializeComponent();
      comboBoxBandWidth.SelectedIndex = 1;
    }

    public IChannel Channel
    {
      get
      {
        return _channel;
      }
      set
      {
        _channel = (DVBTChannel)value;
        textboxFreq.Text = _channel.Frequency.ToString();
        textBoxONID.Text = _channel.NetworkId.ToString();
        textBoxTSID.Text = _channel.TransportId.ToString();
        textBoxSID.Text = _channel.ServiceId.ToString();
        comboBoxBandWidth.SelectedIndex = _channel.BandWidth == 7 ? 0 : 1;
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      _channel.Frequency = Int32.Parse(textboxFreq.Text);
      _channel.NetworkId = Int32.Parse(textBoxONID.Text);
      _channel.TransportId = Int32.Parse(textBoxTSID.Text);
      _channel.ServiceId = Int32.Parse(textBoxSID.Text);
      _channel.BandWidth = comboBoxBandWidth.SelectedIndex == 0 ? 7 : 8;
      Close();
    }

  }
}