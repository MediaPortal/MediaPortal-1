using System;
using System.Windows.Forms;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using DirectShowLib.BDA;
namespace TestApp
{
  public partial class FormDVBCChannel : Form
  {
    DVBCChannel _channel = new DVBCChannel();
    public FormDVBCChannel()
    {
      InitializeComponent();
    }


    public IChannel Channel
    {
      get
      {
        return _channel;
      }
      set
      {
        _channel = (DVBCChannel)value;
        textboxFreq.Text = _channel.Frequency.ToString();
        textBoxONID.Text = _channel.NetworkId.ToString();
        textBoxTSID.Text = _channel.TransportId.ToString();
        textBoxSID.Text = _channel.ServiceId.ToString();
        textBoxSymbolRate.Text = _channel.SymbolRate.ToString();
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      _channel.Frequency = Int32.Parse(textboxFreq.Text);
      _channel.NetworkId = Int32.Parse(textBoxONID.Text);
      _channel.TransportId = Int32.Parse(textBoxTSID.Text);
      _channel.ServiceId = Int32.Parse(textBoxSID.Text);
      _channel.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);
      _channel.ModulationType = ModulationType.Mod64Qam;
      Close();
    }
  }
}