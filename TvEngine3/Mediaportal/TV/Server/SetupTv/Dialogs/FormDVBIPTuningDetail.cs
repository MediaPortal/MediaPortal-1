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
  public partial class FormDVBIPTuningDetail : SetupControls.FormTuningDetailCommon
  {

    protected ServiceDvb CreateInitialServiceDetail()
    {
      var initialServiceDetail = new ServiceDvb { TuningDetail = new TuningDetailStream() };
      return initialServiceDetail;
    }


    public FormDVBIPTuningDetail()
    {
      InitializeComponent();
    }

    private void FormDVBIPTuningDetail_Load(object sender, EventArgs e)
    {
      if (ServiceDetail != null)
      {
        var tuningDetail = (TuningDetailStream)ServiceDetail.TuningDetail;

        textBoxDVBIPChannel.Text = ServiceDetail.LogicalChannelNumber.ToString();
        textBoxDVBIPUrl.Text = tuningDetail.Url;

        var serviceDvb = ServiceDetail as ServiceDvb;
        if (serviceDvb != null)
        {
          textBoxDVBIPNetworkId.Text = serviceDvb.OriginalNetworkId.GetValueOrDefault().ToString();
          textBoxDVBIPTransportId.Text = serviceDvb.TransportStreamId.GetValueOrDefault().ToString();
          textBoxDVBIPServiceId.Text = serviceDvb.ServiceId.GetValueOrDefault().ToString();
          textBoxDVBIPPmtPid.Text = serviceDvb.PmtPid.GetValueOrDefault().ToString();
          textBoxDVBIPProvider.Text = serviceDvb.Provider;
        }

        
        checkBoxDVBIPfta.Checked = (EncryptionSchemeEnum)ServiceDetail.EncryptionScheme.GetValueOrDefault(0) == EncryptionSchemeEnum.Free;


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
      var tuningDetailStream = ServiceDetail.TuningDetail as TuningDetailStream;

      var serviceDvb = ServiceDetail as ServiceDvb;

      //tuningDetailStream.ChannelType = 7;
      ServiceDetail.LogicalChannelNumber = Int32.Parse(textBoxDVBIPChannel.Text).ToString();
      tuningDetailStream.Url = textBoxDVBIPUrl.Text;
      serviceDvb.OriginalNetworkId = Int32.Parse(textBoxDVBIPNetworkId.Text);
      serviceDvb.TransportStreamId = Int32.Parse(textBoxDVBIPTransportId.Text);
      serviceDvb.ServiceId = Int32.Parse(textBoxDVBIPServiceId.Text);
      serviceDvb.PmtPid = Int32.Parse(textBoxDVBIPPmtPid.Text);
      serviceDvb.Provider = textBoxDVBIPProvider.Text;
      if (checkBoxDVBIPfta.Checked)
      {
        serviceDvb.EncryptionScheme = (int)EncryptionSchemeEnum.Free; 
      }
      else
      {
        serviceDvb.EncryptionScheme = (int)EncryptionSchemeEnum.Encrypted; 
      }      
    }

    private bool ValidateInput()
    {
      int lcn, onid, tsid, sid, pmt;
      if (textBoxDVBIPChannel.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a channel number!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPChannel.Text, out lcn))
      {
        MessageBox.Show(this, "Please enter a valid channel number!", "Incorrect input");
        return false;
      }
      if (textBoxDVBIPUrl.Text.Length == 0)
      {
        MessageBox.Show(this, "Please enter a valid URL!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPNetworkId.Text, out onid))
      {
        MessageBox.Show(this, "Please enter a valid network ID!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPTransportId.Text, out tsid))
      {
        MessageBox.Show(this, "Please enter a valid transport ID!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPServiceId.Text, out sid))
      {
        MessageBox.Show(this, "Please enter a valid service ID!", "Incorrect input");
        return false;
      }
      if (onid < 0 || tsid < 0 || sid < 0)
      {
        MessageBox.Show(this, "Please enter valid network, transport and service IDs!", "Incorrect input");
        return false;
      }
      if (!Int32.TryParse(textBoxDVBIPPmtPid.Text, out pmt))
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