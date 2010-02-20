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
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using DirectShowLib;

namespace TestApp
{
  public partial class FormAnalogChannel : Form
  {
    private AnalogChannel _channel = new AnalogChannel();
    private readonly CountryCollection countries = new CountryCollection();

    public FormAnalogChannel()
    {
      _channel.Country = countries.GetTunerCountryFromID(31);
      _channel.ChannelNumber = 25;
      _channel.TunerSource = TunerInputType.Antenna;
      InitializeComponent();
      comboBoxInput.SelectedIndex = 0;
      for (int i = 0; i < countries.Countries.Length; ++i)
      {
        comboBoxCountry.Items.Add(countries.Countries[i].Name);
      }
      comboBoxCountry.SelectedIndex = 0;
    }

    public IChannel Channel
    {
      get { return _channel; }
      set
      {
        _channel = (AnalogChannel)value;
        comboBoxInput.SelectedIndex = 0;
        if (_channel.TunerSource == TunerInputType.Cable)
          comboBoxInput.SelectedIndex = 1;

        if (_channel.Country == null)
          comboBoxCountry.SelectedIndex = 0;
        else
        {
          for (int i = 0; i < countries.Countries.Length; ++i)
          {
            if ((_channel.Country.Id == countries.Countries[i].Id))
            {
              comboBoxCountry.SelectedIndex = i;
              break;
            }
          }
        }
        textBoxChannel.Text = _channel.ChannelNumber.ToString();
      }
    }

    private void FormAnalogChannel_Load(object sender, EventArgs e)
    {
      Channel = _channel;
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      _channel.ChannelNumber = Int32.Parse(textBoxChannel.Text);
      _channel.Country = countries.Countries[comboBoxCountry.SelectedIndex];
      _channel.TunerSource = TunerInputType.Antenna;
      if (comboBoxInput.SelectedIndex == 1)
        _channel.TunerSource = TunerInputType.Cable;
      Close();
    }
  }
}