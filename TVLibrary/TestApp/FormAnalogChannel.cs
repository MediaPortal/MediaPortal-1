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
using DirectShowLib;
namespace TestApp
{
  public partial class FormAnalogChannel : Form
  {
    AnalogChannel _channel = new AnalogChannel();
    CountryCollection countries = new CountryCollection();
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
      get
      {
        return _channel;
      }
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