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

namespace TestApp
{
  public partial class FormATSCChannel : Form
  {
    ATSCChannel _channel = new ATSCChannel();
    public FormATSCChannel()
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