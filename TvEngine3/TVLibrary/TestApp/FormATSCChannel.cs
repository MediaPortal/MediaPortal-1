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