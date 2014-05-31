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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormDVBSTuningDetail : FormTuningDetailCommon
  {    
    public FormDVBSTuningDetail()
    {
      InitializeComponent();
    }

    protected ServiceDvb CreateInitialServiceDetail()
    {
      var initialServiceDetail = new ServiceDvb { TuningDetail = new TuningDetailDvbS2() };
      return initialServiceDetail;
    }

    private void FormDVBSTuningDetail_Load(object sender, EventArgs e)
    {
      IList<Satellite> tempSatellites = ServiceAgents.Instance.CardServiceAgent.ListAllSatellites();
      comboBoxSatellite.Items.AddRange(tempSatellites.Cast<object>().ToArray());      

      var serviceDetailDvb = ServiceDetail as ServiceDvb;
      if (serviceDetailDvb != null)
      {

        var tuningDetail = (TuningDetailDvbS2)ServiceDetail.TuningDetail;

        textBoxFrequency.Text = tuningDetail.Frequency.GetValueOrDefault().ToString();
        textBoxNetworkId.Text = serviceDetailDvb.OriginalNetworkId.GetValueOrDefault().ToString();
        textBoxTransportId.Text = serviceDetailDvb.TransportStreamId.GetValueOrDefault().ToString();
        textBoxServiceId.Text = serviceDetailDvb.ServiceId.GetValueOrDefault().ToString();
        textBoxSymbolRate.Text = tuningDetail.SymbolRate.GetValueOrDefault(0).ToString();
        textBoxDVBSChannel.Text = ServiceDetail.LogicalChannelNumber;


        textBoxDVBSPmt.Text = serviceDetailDvb.PmtPid.GetValueOrDefault().ToString();
        textBoxDVBSProvider.Text = serviceDetailDvb.Provider;

        mpRadioFree.Checked = (ServiceDetail.EncryptionScheme == (int)EncryptionSchemeEnum.Free);
        mpRadioEncrypted.Checked = (ServiceDetail.EncryptionScheme == (int)EncryptionSchemeEnum.Encrypted);
        mpRadioSometimesEncrypted.Checked = (ServiceDetail.EncryptionScheme == (int)EncryptionSchemeEnum.SometimesEncrypted);
        

        comboBoxPol.SelectedIndex = tuningDetail.Polarisation.GetValueOrDefault(0) + 1;
        comboBoxModulation.SelectedIndex = tuningDetail.Modulation.GetValueOrDefault(0) + 1;
        comboBoxInnerFecRate.SelectedIndex = tuningDetail.FecRate.GetValueOrDefault(0) + 1;

        comboBoxPilot.SelectedIndex = tuningDetail.Pilot.GetValueOrDefault(0) + 1;
        comboBoxRollOff.SelectedIndex = tuningDetail.RollOff.GetValueOrDefault(0) + 1;

        int idSatellite = 0;
        if (tuningDetail.Satellite.TunerSatellites.Count > 0)
        {
          idSatellite = tuningDetail.IdSatellite.GetValueOrDefault();
        }

        
        if ( idSatellite > 0)
        {
          IEnumerator en = comboBoxSatellite.Items.GetEnumerator();
          while (en.MoveNext())
          {
            var satellite = (Satellite)en.Current;
            if (satellite != null && satellite.IdSatellite == idSatellite)
            {
              comboBoxSatellite.SelectedItem = en.Current;
              break;
            }
          }  
        }
        
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
        mpRadioFree.Checked = true;
        mpRadioEncrypted.Checked = false;
        mpRadioSometimesEncrypted.Checked = false;
        comboBoxPol.SelectedIndex = -1;
        comboBoxModulation.SelectedIndex = -1;
        comboBoxInnerFecRate.SelectedIndex = -1;
        comboBoxPilot.SelectedIndex = -1;
        comboBoxRollOff.SelectedIndex = -1;
        comboBoxSatellite.SelectedIndex = -1;        
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
      var tuningDetail = (TuningDetailDvbS2)ServiceDetail.TuningDetail;
      if (comboBoxPilot.SelectedIndex == -1 && comboBoxRollOff.SelectedIndex == -1)
      {
        ServiceDetail.TuningDetail = new TuningDetailSatellite();
      }
      else
      {
        tuningDetail.Pilot = (int)(Pilot)(comboBoxPilot.SelectedIndex - 1);
        tuningDetail.RollOff = (int)(RollOff)(comboBoxRollOff.SelectedIndex - 1);  
      }

      var serviceDetailDvb = ServiceDetail as ServiceDvb;
      tuningDetail.Frequency = Int32.Parse(textBoxFrequency.Text);
      serviceDetailDvb.OriginalNetworkId = Int32.Parse(textBoxNetworkId.Text);
      serviceDetailDvb.TransportStreamId = Int32.Parse(textBoxTransportId.Text);
      serviceDetailDvb.ServiceId = Int32.Parse(textBoxServiceId.Text);
      tuningDetail.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);
      tuningDetail.Polarisation = (int)(Polarisation)(comboBoxPol.SelectedIndex - 1);
      tuningDetail.FecRate = (int)(BinaryConvolutionCodeRate)(comboBoxInnerFecRate.SelectedIndex - 1);

      tuningDetail.Modulation = (int)(ModulationType)(comboBoxModulation.SelectedIndex - 1);
      ServiceDetail.LogicalChannelNumber = Int32.Parse(textBoxDVBSChannel.Text).ToString();
      serviceDetailDvb.PmtPid = Int32.Parse(textBoxDVBSPmt.Text);
      serviceDetailDvb.Provider = textBoxDVBSProvider.Text;
                  

      if (mpRadioFree.Checked)
      {
        ServiceDetail.EncryptionScheme = (int)EncryptionSchemeEnum.Free;
      }
      else if (mpRadioEncrypted.Checked)
      {
        ServiceDetail.EncryptionScheme = (int)EncryptionSchemeEnum.Encrypted;
      }
      else if (mpRadioSometimesEncrypted.Checked)
      {
        ServiceDetail.EncryptionScheme = (int)EncryptionSchemeEnum.SometimesEncrypted;
      }


      //gibman ...todo
      /*
      TrackableCollection<TunerSatellite> sats = tuningDetail.Satellite.TunerSatellites;
      

      bool foundDiseqc = false;
      if (sats.Count > 0)
      {
        foreach (TunerSatellite tunerSatellite in sats)
        {
          if (tunerSatellite.DiseqcSwitchSetting == comboBoxDiseqc.SelectedIndex)
          {
            foundDiseqc = true;
            break;
          }
        }
      }

      if (!foundDiseqc)
      {
        var tunerSatellite = new TunerSatellite();
        tunerSatellite.DiseqcSwitchSetting = comboBoxDiseqc.SelectedIndex;
        tunerSatellite.IdCard = 0;
        tunerSatellite.LnbType;

        LnbType l;
        l.SwitchFrequency

      }

      tuningDetail.DiSEqC = comboBoxDiseqc.SelectedIndex;

      // This should be safe because we've validated the selection in ValidateInput().
      tuningDetail.IdLnbType = ((LnbType)comboBoxLnbType.SelectedItem).IdLnbType;
       */
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
      if (comboBoxSatellite.SelectedIndex < 0)
      {
        MessageBox.Show(this, "Please select a valid satellite!", "Incorrect input");
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