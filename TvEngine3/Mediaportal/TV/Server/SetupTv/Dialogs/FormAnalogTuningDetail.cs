#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using DirectShowLib;
using TvLibrary;

namespace SetupTv.Dialogs
{
  public partial class FormAnalogTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormAnalogTuningDetail()
    {
      InitializeComponent();
    }

    private void FormAnalogTuningDetail_Load(object sender, EventArgs e)
    {
      CountryCollection countries = new CountryCollection();

      comboBoxCountry.Items.AddRange(countries.Countries);

      if (TuningDetail != null)
      {
        //Editing
        textBoxChannel.Text = TuningDetail.ChannelNumber.ToString();
        comboBoxInput.SelectedIndex = 0;
        if (TuningDetail.TuningSource == (int)TunerInputType.Cable)
          comboBoxInput.SelectedIndex = 1;
        comboBoxCountry.SelectedIndex = TuningDetail.CountryId;
        comboBoxVideoSource.SelectedIndex = TuningDetail.VideoSource;
        comboBoxAudioSource.SelectedIndex = TuningDetail.AudioSource;
        textBoxAnalogFrequency.Text = SetFrequency(TuningDetail.Frequency, "2");
        checkBoxVCR.Checked = TuningDetail.IsVCRSignal;
      }
      else
      {
        //Editing
        textBoxChannel.Text = "";
        comboBoxInput.SelectedIndex = -1;
        comboBoxCountry.SelectedIndex = -1;
        comboBoxVideoSource.SelectedIndex = -1;
        comboBoxAudioSource.SelectedIndex = -1;
        textBoxAnalogFrequency.Text = "0";
        checkBoxVCR.Checked = false;
      }
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      if (ValidateInput())
      {
        if (TuningDetail == null)
        {
          TuningDetail = CreateInitialTuningDetail();
        }
        UpdateTuningDetail();
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void UpdateTuningDetail()
    {
      TuningDetail.ChannelNumber = Convert.ToInt32(textBoxChannel.Text);
      TuningDetail.CountryId = ((Country)comboBoxCountry.SelectedItem).Index;
      TuningDetail.TuningSource = (int)(comboBoxInput.SelectedIndex == 1
                                          ? TunerInputType.Cable
                                          : TunerInputType.Antenna);
      TuningDetail.VideoSource = comboBoxVideoSource.SelectedIndex;
      TuningDetail.AudioSource = comboBoxAudioSource.SelectedIndex;
      TuningDetail.Frequency = (int)GetFrequency(textBoxAnalogFrequency.Text, "2");
      TuningDetail.IsVCRSignal = checkBoxVCR.Checked;
      TuningDetail.ChannelType = 0;
    }

    private bool ValidateInput()
    {
      if (textBoxChannel.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a channel number!", "Incorrect input");
        return false;
      }
      int channelNumber;
      if (!Int32.TryParse(textBoxChannel.Text, out channelNumber))
      {
        MessageBox.Show(this, "Please enter a valid channel number!", "Incorrect input");
        return false;
      }

      if (textBoxAnalogFrequency.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a frequency!", "Incorrect input");
        return false;
      }

      try
      {
        GetFrequency(textBoxAnalogFrequency.Text, "2");
      }
      catch (Exception)
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
        return false;
      }

      if (comboBoxInput.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select an input type!", "Incorrect input");
        return false;
      }

      if (comboBoxCountry.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a country!", "Incorrect input");
        return false;
      }

      if (comboBoxVideoSource.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a video source!", "Incorrect input");
        return false;
      }

      if (comboBoxAudioSource.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a audio source!", "Incorrect input");
        return false;
      }

      return true;
    }

    private static long GetFrequency(string text, string precision)
    {
      float tmp = 123.25f;
      if (tmp.ToString("f" + precision).IndexOf(',') > 0)
      {
        text = text.Replace('.', ',');
      }
      else
      {
        text = text.Replace(',', '.');
      }
      float freq = float.Parse(text);
      freq *= 1000000f;
      return (long)freq;
    }

    private static string SetFrequency(long frequency, string precision)
    {
      float freq = frequency;
      freq /= 1000000f;
      return freq.ToString("f" + precision);
    }
  }
}