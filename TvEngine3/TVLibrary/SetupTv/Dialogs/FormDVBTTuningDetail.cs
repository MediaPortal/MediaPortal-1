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

namespace SetupTv.Dialogs
{
  public partial class FormDVBTTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBTTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBTTuningDetail_Load(object sender, EventArgs e)
    {
      if (TuningDetail != null)
      {
        textBoxDVBTChannel.Text = TuningDetail.ChannelNumber.ToString();
        textBoxDVBTfreq.Text = TuningDetail.Frequency.ToString();
        textBoxNetworkId.Text = TuningDetail.NetworkId.ToString();
        textBoxTransportId.Text = TuningDetail.TransportId.ToString();
        textBoxServiceId.Text = TuningDetail.ServiceId.ToString();
        textBoxDVBTProvider.Text = TuningDetail.Provider;
        checkBoxDVBTfta.Checked = TuningDetail.FreeToAir;
        comboBoxBandWidth.SelectedIndex = TuningDetail.Bandwidth == 7 ? 0 : 1;
        textBoxPmt.Text = TuningDetail.PmtPid.ToString();
      }
      else
      {
        textBoxDVBTChannel.Text = "";
        textBoxDVBTfreq.Text = "";
        textBoxNetworkId.Text = "";
        textBoxTransportId.Text = "";
        textBoxServiceId.Text = "";
        textBoxDVBTProvider.Text = "";
        checkBoxDVBTfta.Checked = false;
        comboBoxBandWidth.SelectedIndex = -1;
        textBoxPmt.Text = "";
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
      TuningDetail.ChannelType = 4;
      TuningDetail.ChannelNumber = Int32.Parse(textBoxDVBTChannel.Text);
      TuningDetail.Frequency = Int32.Parse(textBoxDVBTfreq.Text);
      TuningDetail.NetworkId = Int32.Parse(textBoxNetworkId.Text);
      TuningDetail.TransportId = Int32.Parse(textBoxTransportId.Text);
      TuningDetail.ServiceId = Int32.Parse(textBoxServiceId.Text);
      TuningDetail.Provider = textBoxDVBTProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBTfta.Checked;
      TuningDetail.Bandwidth = comboBoxBandWidth.SelectedIndex == 0 ? 7 : 8;
      TuningDetail.PmtPid = Int32.Parse(textBoxPmt.Text);
    }

    private bool ValidateInput()
    {
      int lcn, freq, onid, tsid, sid, pmt;
      if (textBoxDVBTChannel.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a channel number!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBTChannel.Text, out lcn))
      {
        MessageBox.Show(this, "Please enter a valid channel number!", "Incorrect input");
        return false;
      }
      if (textBoxDVBTfreq.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a frequency!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBTfreq.Text, out freq))
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
        return false;
      }
      if (comboBoxBandWidth.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid bandwidth!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxNetworkId.Text, out onid))
      {
        MessageBox.Show(this, "Please enter a valid network ID!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxTransportId.Text, out tsid))
      {
        MessageBox.Show(this, "Please enter a valid transport ID!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxServiceId.Text, out sid))
      {
        MessageBox.Show(this, "Please enter a valid service ID!", "Incorrect input");
        return false;
      }
      if (onid < 0 || tsid < 0 || sid < 0)
      {
        MessageBox.Show(this, "Please enter valid network, transport and service IDs!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxPmt.Text, out pmt))
      {
        MessageBox.Show(this, "Please enter a valid PMT PID!", "Incorrect input");
        return false;
      }
      if (pmt < 0)
      {
        MessageBox.Show(this, "Please enter a valid PMT PID!", "Incorrect input");
        return false;
      }
      return true;
    }
  }
}