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
        _channel=(DVBTChannel)value;
        textboxFreq.Text=_channel.Frequency.ToString();
        textBoxONID.Text=_channel.NetworkId.ToString();
        textBoxTSID.Text=_channel.TransportId.ToString();
        textBoxSID.Text=_channel.ServiceId.ToString();
        if (_channel.BandWidth==7)
          comboBoxBandWidth.SelectedIndex=0;
        else
          comboBoxBandWidth.SelectedIndex=1;
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      _channel.Frequency=Int32.Parse(textboxFreq.Text);
      _channel.NetworkId=Int32.Parse(textBoxONID.Text);
      _channel.TransportId=Int32.Parse(textBoxTSID.Text);
      _channel.ServiceId=Int32.Parse(textBoxSID.Text);
      if (comboBoxBandWidth.SelectedIndex==0)
        _channel.BandWidth=7;
      else 
        _channel.BandWidth=8;
      Close();
    }

  }
}