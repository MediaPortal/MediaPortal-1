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
using DirectShowLib.BDA;
using TvDatabase;

namespace SetupTv.Dialogs
{
  public partial class FormDVBSTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBSTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBSTuningDetail_Load(object sender, EventArgs e)
    {
      if (TuningDetail != null)
      {
        textBoxFrequency.Text = TuningDetail.Frequency.ToString();
        textBoxNetworkId.Text = TuningDetail.NetworkId.ToString();
        textBoxTransportId.Text = TuningDetail.TransportId.ToString();
        textBoxServiceId.Text = TuningDetail.ServiceId.ToString();
        textBoxSymbolRate.Text = TuningDetail.Symbolrate.ToString();
        textBoxDVBSChannel.Text = TuningDetail.ChannelNumber.ToString();
        textBoxDVBSPmt.Text = TuningDetail.PmtPid.ToString();
        textBoxDVBSProvider.Text = TuningDetail.Provider;
        checkBoxDVBSfta.Checked = TuningDetail.FreeToAir;
        comboBoxPol.SelectedIndex = TuningDetail.Polarisation + 1;
        comboBoxModulation.SelectedIndex = TuningDetail.Modulation + 1;
        comboBoxInnerFecRate.SelectedIndex = TuningDetail.InnerFecRate + 1;
        comboBoxPilot.SelectedIndex = TuningDetail.Pilot + 1;
        comboBoxRollOff.SelectedIndex = TuningDetail.RollOff + 1;
        comboBoxDisEqc.SelectedIndex = TuningDetail.Diseqc;
      }
      else
      {
        textBoxFrequency.Text = "";
        textBoxNetworkId.Text = "";
        textBoxTransportId.Text = "";
        textBoxServiceId.Text = "";
        textBoxSymbolRate.Text = "";
        textBoxDVBSChannel.Text = "";
        textBoxDVBSPmt.Text = "";
        textBoxDVBSProvider.Text = "";
        checkBoxDVBSfta.Checked = false;
        comboBoxPol.SelectedIndex = -1;
        comboBoxModulation.SelectedIndex = -1;
        comboBoxInnerFecRate.SelectedIndex = -1;
        comboBoxPilot.SelectedIndex = -1;
        comboBoxRollOff.SelectedIndex = -1;
        comboBoxDisEqc.SelectedIndex = -1;
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
      TuningDetail.ChannelType = 3;
      TuningDetail.Frequency = Int32.Parse(textBoxFrequency.Text);
      TuningDetail.NetworkId = Int32.Parse(textBoxNetworkId.Text);
      TuningDetail.TransportId = Int32.Parse(textBoxTransportId.Text);
      TuningDetail.ServiceId = Int32.Parse(textBoxServiceId.Text);
      TuningDetail.Symbolrate = Int32.Parse(textBoxSymbolRate.Text);
      TuningDetail.Polarisation = (int)(Polarisation)(comboBoxPol.SelectedIndex - 1);
      TuningDetail.InnerFecRate = (int)(BinaryConvolutionCodeRate)(comboBoxInnerFecRate.SelectedIndex - 1);
      TuningDetail.Pilot = (int)(Pilot)(comboBoxPilot.SelectedIndex - 1);
      TuningDetail.RollOff = (int)(RollOff)(comboBoxRollOff.SelectedIndex - 1);
      TuningDetail.Modulation = (int)(ModulationType)(comboBoxModulation.SelectedIndex - 1);
      TuningDetail.ChannelNumber = Int32.Parse(textBoxDVBSChannel.Text);
      TuningDetail.PmtPid = Int32.Parse(textBoxDVBSPmt.Text);
      TuningDetail.Provider = textBoxDVBSProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBSfta.Checked;
      TuningDetail.Diseqc = comboBoxDisEqc.SelectedIndex;
    }

    private bool ValidateInput()
    {
      int lcn, freq, onid, tsid, sid, symbolrate, pmt;
      if (textBoxDVBSChannel.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a channel number!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBSChannel.Text, out lcn))
      {
        MessageBox.Show(this, "Please enter a valid channel number!", "Incorrect input");
        return false;
      }
      if (comboBoxDisEqc.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid DiSEqC port!", "Incorrect input");
        return false;
      }
      if (textBoxFrequency.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a frequency!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxFrequency.Text, out freq))
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
        return false;
      }
      if (freq <= 0)
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
        return false;
      }
      if (textBoxSymbolRate.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a symbol rate!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxSymbolRate.Text, out symbolrate))
      {
        MessageBox.Show(this, "Please enter a valid symbol rate!", "Incorrect input");
        return false;
      }
      if (symbolrate <= 0)
      {
        MessageBox.Show(this, "Please enter a valid symbol rate!", "Incorrect input");
        return false;
      }
      if (comboBoxPol.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid polarisation!", "Incorrect input");
        return false;
      }
      if (comboBoxModulation.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid modulation!", "Incorrect input");
        return false;
      }
      if (comboBoxInnerFecRate.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid inner FEC rate!", "Incorrect input");
        return false;
      }
      if (comboBoxPilot.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid pilot!", "Incorrect input");
        return false;
      }
      if (comboBoxRollOff.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid roll-off!", "Incorrect input");
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
      if (!Int32.TryParse(textBoxDVBSPmt.Text, out pmt))
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