using System;
using System.Windows.Forms;
using TvLibrary.Interfaces;
using TvLibrary.Channels;

namespace TestApp
{
  public partial class FormATSCChannel : Form
  {
    private ATSCChannel _channel = new ATSCChannel();

    public FormATSCChannel()
    {
      InitializeComponent();
    }

    public IChannel Channel
    {
      get { return _channel; }
      set
      {
        _channel = (ATSCChannel)value;
        textboxFreq.Text = _channel.PhysicalChannel.ToString();
        textBoxONID.Text = _channel.MajorChannel.ToString();
        textBoxTSID.Text = _channel.MinorChannel.ToString();
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      _channel.Frequency = -1;
      _channel.SymbolRate = -1;
      _channel.TransportId = -1;
      _channel.ModulationType = DirectShowLib.BDA.ModulationType.ModNotSet;
      _channel.PhysicalChannel = Int32.Parse(textboxFreq.Text);
      _channel.MajorChannel = Int32.Parse(textBoxONID.Text);
      _channel.MinorChannel = Int32.Parse(textBoxTSID.Text);
      Close();
    }
  }
}