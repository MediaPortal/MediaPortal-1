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

namespace SetupTv.Dialogs
{
  public partial class FormDVBCTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBCTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBCTuningDetail_Load(object sender, System.EventArgs e)
    {
      comboBoxDvbCModulation.Items.Clear();
      comboBoxDvbCModulation.Items.AddRange(new object[]
                                              {
                                                "Not Set",
                                                "Not Defined",
                                                "16 QAM",
                                                "32 QAM",
                                                "64 QAM",
                                                "80 QAM",
                                                "96 QAM",
                                                "112 QAM",
                                                "128 QAM",
                                                "160 QAM",
                                                "192 QAM",
                                                "224 QAM",
                                                "256 QAM",
                                                "320 QAM",
                                                "384 QAM",
                                                "448 QAM",
                                                "512 QAM",
                                                "640 QAM",
                                                "768 QAM",
                                                "896 QAM",
                                                "1024 QAM",
                                                "QPSK",
                                                "BPSK",
                                                "OQPSK",
                                                "8 VSB",
                                                "16 VSB",
                                                "Analog Amplitude",
                                                "Analog Frequency",
                                                "8 PSK",
                                                "RF",
                                                "16 APSK",
                                                "32 APSK",
                                                "QPSK2 (DVB-S2)",
                                                "8 PSK2 (DVB-S2)",
                                                "DirectTV"
                                              });

      if (TuningDetail != null)
      {
        //Editing
        textBoxChannel.Text = TuningDetail.ChannelNumber.ToString();
        textboxFreq.Text = TuningDetail.Frequency.ToString();
        textBoxONID.Text = TuningDetail.NetworkId.ToString();
        textBoxTSID.Text = TuningDetail.TransportId.ToString();
        textBoxSID.Text = TuningDetail.ServiceId.ToString();
        textBoxSymbolRate.Text = TuningDetail.Symbolrate.ToString();
        textBoxDVBCPmt.Text = TuningDetail.PmtPid.ToString();
        textBoxDVBCProvider.Text = TuningDetail.Provider;
        checkBoxDVBCfta.Checked = TuningDetail.FreeToAir;
        comboBoxDvbCModulation.SelectedIndex = TuningDetail.Modulation + 1;
      }
      else
      {
        textBoxChannel.Text = "";
        textboxFreq.Text = "";
        textBoxONID.Text = "";
        textBoxTSID.Text = "";
        textBoxSID.Text = "";
        textBoxSymbolRate.Text = "";
        textBoxDVBCPmt.Text = "";
        textBoxDVBCProvider.Text = "";
        checkBoxDVBCfta.Checked = false;
        comboBoxDvbCModulation.SelectedIndex = -1;
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
      TuningDetail.ChannelNumber = Int32.Parse(textBoxChannel.Text);
      TuningDetail.Frequency = Convert.ToInt32(textboxFreq.Text);
      TuningDetail.NetworkId = Convert.ToInt32(textBoxONID.Text);
      TuningDetail.TransportId = Convert.ToInt32(textBoxTSID.Text);
      TuningDetail.ServiceId = Convert.ToInt32(textBoxSID.Text);
      TuningDetail.Symbolrate = Convert.ToInt32(textBoxSymbolRate.Text);
      TuningDetail.PmtPid = Convert.ToInt32(textBoxDVBCPmt.Text);
      TuningDetail.Provider = textBoxDVBCProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBCfta.Checked;
      TuningDetail.Modulation = (int)(ModulationType)(comboBoxDvbCModulation.SelectedIndex - 1);
      TuningDetail.ChannelType = 2;
    }

    private bool ValidateInput()
    {
      int channel, freq, onid, tsid, sid, symbolrate, pmt;
      if (!Int32.TryParse(textBoxChannel.Text, out channel))
      {
        MessageBox.Show(this, "Please enter a valid channel number!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textboxFreq.Text, out freq))
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
        return false;
      }
      if (freq <= 0)
      {
        MessageBox.Show(this, "Please enter a valid frequency!", "Incorrect input");
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
      if (comboBoxDvbCModulation.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid modulation!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxONID.Text, out onid))
      {
        MessageBox.Show(this, "Please enter a valid network ID!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxTSID.Text, out tsid))
      {
        MessageBox.Show(this, "Please enter a valid transport ID!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxSID.Text, out sid))
      {
        MessageBox.Show(this, "Please enter a valid service ID!", "Incorrect input");
        return false;
      }
      if (onid < 0 || tsid < 0 || sid < 0)
      {
        MessageBox.Show(this, "Please enter valid network, transport and service IDs!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBCPmt.Text, out pmt))
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