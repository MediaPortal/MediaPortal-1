#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Windows.Forms;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using DirectShowLib.BDA;

namespace TestApp
{
  public partial class FormDVBSChannel : Form
  {
    private DVBSChannel _channel = new DVBSChannel();

    public FormDVBSChannel()
    {
      InitializeComponent();
      comboBoxDisEqc.SelectedIndex = 0;
      comboBoxPol.SelectedIndex = 0;
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {}
    private void textBox1_TextChanged(object sender, EventArgs e) {}

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