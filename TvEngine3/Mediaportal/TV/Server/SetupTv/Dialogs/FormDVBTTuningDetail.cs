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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormDVBTTuningDetail : SetupControls.FormTuningDetailCommon
  {
    public FormDVBTTuningDetail()
    {
      InitializeComponent();
    }


    protected ServiceDvb CreateInitialServiceDetail()
    {
      var initialServiceDetail = new ServiceDvb { TuningDetail = new TuningDetailTerrestrial() };
      return initialServiceDetail;
    }

    private void FormDVBTTuningDetail_Load(object sender, EventArgs e)
    {
      if (ServiceDetail != null)
      {
        var tuningDetail = (TuningDetailTerrestrial)ServiceDetail.TuningDetail;
        var serviceDetailDvb = ServiceDetail as ServiceDvb;

        textBoxDVBTChannel.Text = ServiceDetail.LogicalChannelNumber;
        textBoxDVBTfreq.Text = tuningDetail.Frequency.GetValueOrDefault(0).ToString();
        textBoxDVBTBandwidth.Text = tuningDetail.Bandwidth.ToString();
        textBoxNetworkId.Text = serviceDetailDvb.OriginalNetworkId.GetValueOrDefault(0).ToString();
        textBoxTransportId.Text = serviceDetailDvb.TransportStreamId.ToString();

        textBoxServiceId.Text = serviceDetailDvb.ServiceId.GetValueOrDefault(0).ToString();
        textBoxDVBTProvider.Text = serviceDetailDvb.Provider;
        checkBoxDVBTfta.Checked = ((EncryptionSchemeEnum)ServiceDetail.EncryptionScheme.GetValueOrDefault(0)) == EncryptionSchemeEnum.Free;
        textBoxPmt.Text = serviceDetailDvb.PmtPid.GetValueOrDefault(0).ToString();
      }
      else
      {
        textBoxDVBTChannel.Text = "";
        textBoxDVBTfreq.Text = "";
        textBoxDVBTBandwidth.Text = "";
        textBoxNetworkId.Text = "";
        textBoxTransportId.Text = "";
        textBoxServiceId.Text = "";
        textBoxDVBTProvider.Text = "";
        checkBoxDVBTfta.Checked = false;
        textBoxPmt.Text = "";
      }
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      if (ValidateInput())
      {
        if (ServiceDetail == null)
        {
          ServiceDetail = CreateInitialServiceDetail();
        }
        UpdateTuningDetail();
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void UpdateTuningDetail()
    {
      var tuningDetail = (TuningDetailTerrestrial)ServiceDetail.TuningDetail;
      var serviceDetailDvb = ServiceDetail as ServiceDvb;

      ServiceDetail.LogicalChannelNumber = Int32.Parse(textBoxDVBTChannel.Text).ToString();
      tuningDetail.Frequency = Int32.Parse(textBoxDVBTfreq.Text);
      tuningDetail.Bandwidth = Int32.Parse(textBoxDVBTBandwidth.Text);
      serviceDetailDvb.OriginalNetworkId = Int32.Parse(textBoxNetworkId.Text);
      serviceDetailDvb.TransportStreamId = Int32.Parse(textBoxTransportId.Text);
      serviceDetailDvb.ServiceId = Int32.Parse(textBoxServiceId.Text);
      serviceDetailDvb.Provider = textBoxDVBTProvider.Text;

      if (checkBoxDVBTfta.Checked)
      {
        ServiceDetail.EncryptionScheme = (int)EncryptionSchemeEnum.Free;
      }
      else
      {
        ServiceDetail.EncryptionScheme = (int)EncryptionSchemeEnum.Encrypted;
      }

      serviceDetailDvb.PmtPid = Int32.Parse(textBoxPmt.Text);
    }

    private bool ValidateInput()
    {
      int lcn, freq, bandwidth, onid, tsid, sid, pmt;
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
      if (textBoxDVBTBandwidth.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a bandwidth!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBTBandwidth.Text, out bandwidth))
      {
        MessageBox.Show(this, "Please enter a valid bandwidth!", "Incorrect input");
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