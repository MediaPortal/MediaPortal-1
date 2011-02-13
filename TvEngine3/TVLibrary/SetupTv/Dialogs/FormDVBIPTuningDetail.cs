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
  public partial class FormDVBIPTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBIPTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBIPTuningDetail_Load(object sender, EventArgs e)
    {
      if (TuningDetail != null)
      {
        textBoxDVBIPChannel.Text = TuningDetail.ChannelNumber.ToString();
        textBoxDVBIPUrl.Text = TuningDetail.Url;
        textBoxDVBIPNetworkId.Text = TuningDetail.NetworkId.ToString();
        textBoxDVBIPTransportId.Text = TuningDetail.TransportId.ToString();
        textBoxDVBIPServiceId.Text = TuningDetail.ServiceId.ToString();
        textBoxDVBIPPmtPid.Text = TuningDetail.PmtPid.ToString();
        textBoxDVBIPProvider.Text = TuningDetail.Provider;
        checkBoxDVBIPfta.Checked = TuningDetail.FreeToAir;
      }
      else
      {
        textBoxDVBIPChannel.Text = "";
        textBoxDVBIPUrl.Text = "";
        textBoxDVBIPNetworkId.Text = "";
        textBoxDVBIPTransportId.Text = "";
        textBoxDVBIPServiceId.Text = "";
        textBoxDVBIPPmtPid.Text = "";
        textBoxDVBIPProvider.Text = "";
        checkBoxDVBIPfta.Checked = false;
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
      TuningDetail.ChannelType = 7;
      TuningDetail.ChannelNumber = Int32.Parse(textBoxDVBIPChannel.Text);
      TuningDetail.Url = textBoxDVBIPUrl.Text;
      TuningDetail.NetworkId = Int32.Parse(textBoxDVBIPNetworkId.Text);
      TuningDetail.TransportId = Int32.Parse(textBoxDVBIPTransportId.Text);
      TuningDetail.ServiceId = Int32.Parse(textBoxDVBIPServiceId.Text);
      TuningDetail.PmtPid = Int32.Parse(textBoxDVBIPPmtPid.Text);
      TuningDetail.Provider = textBoxDVBIPProvider.Text;
      TuningDetail.FreeToAir = checkBoxDVBIPfta.Checked;
    }

    private bool ValidateInput()
    {
      if (textBoxDVBIPChannel.Text.Length == 0)
      {
        return false;
      }
      int lcn, onid, tsid, sid, pmt;
      if (!Int32.TryParse(textBoxDVBIPChannel.Text, out lcn))
      {
        MessageBox.Show(this, "Please enter a valid channel!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPNetworkId.Text, out onid))
      {
        MessageBox.Show(this, "Please enter a valid network id!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPTransportId.Text, out tsid))
      {
        MessageBox.Show(this, "Please enter a valid transport id!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPServiceId.Text, out sid))
      {
        MessageBox.Show(this, "Please enter a valid service id!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPPmtPid.Text, out pmt))
      {
        MessageBox.Show(this, "Please enter a valid network, transport and service id!", "Incorrect input");
        return false;
      }
      if (onid <= 0 && tsid < 0 && sid < 0)
      {
        return false;
      }
      return true;
    }
  }
}