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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuningDetailDvbT : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailDvbT()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxBroadcastStandard.Items.Clear();
      comboBoxBroadcastStandard.Items.AddRange(new string[]
      {
        BroadcastStandard.DvbT.GetDescription(),
        BroadcastStandard.DvbT2.GetDescription()
      });

      if (tuningDetail != null)
      {
        Text = "Edit DVB-T/T2 Tuning Detail";
        BroadcastStandard broadcastStandard = (BroadcastStandard)tuningDetail.BroadcastStandard;
        comboBoxBroadcastStandard.SelectedItem = broadcastStandard.GetDescription();
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        numericTextBoxBandwidth.Value = tuningDetail.Bandwidth;
        numericTextBoxPlpId.Value = tuningDetail.StreamId;
        numericTextBoxOriginalNetworkId.Value = tuningDetail.OriginalNetworkId;
        numericTextBoxTransportStreamId.Value = tuningDetail.TransportStreamId;
        numericTextBoxServiceId.Value = tuningDetail.ServiceId;
        numericTextBoxOpenTvChannelId.Value = tuningDetail.OpenTvChannelId;
        numericTextBoxPmtPid.Value = tuningDetail.PmtPid;
        numericTextBoxEpgOriginalNetworkId.Value = tuningDetail.EpgOriginalNetworkId;
        numericTextBoxEpgTransportStreamId.Value = tuningDetail.EpgTransportStreamId;
        numericTextBoxEpgServiceId.Value = tuningDetail.EpgServiceId;

        numericTextBoxPlpId.Enabled = broadcastStandard == BroadcastStandard.DvbT2;
      }
      else
      {
        Text = "Add DVB-T/T2 Tuning Detail";
        comboBoxBroadcastStandard.SelectedItem = BroadcastStandard.DvbT.GetDescription();
        numericTextBoxFrequency.Value = 500000;
        numericTextBoxBandwidth.Value = 8000;
        numericTextBoxPlpId.Value = -1;
        numericTextBoxOriginalNetworkId.Value = 0;
        numericTextBoxTransportStreamId.Value = 0;
        numericTextBoxServiceId.Value = 0;
        numericTextBoxOpenTvChannelId.Value = 0;
        numericTextBoxPmtPid.Value = 0;
        numericTextBoxEpgOriginalNetworkId.Value = 0;
        numericTextBoxEpgTransportStreamId.Value = 0;
        numericTextBoxEpgServiceId.Value = 0;

        // broadcast standard not DVB-T2
        numericTextBoxPlpId.Enabled = false;
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem));
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Bandwidth = numericTextBoxBandwidth.Value;
      if (numericTextBoxPlpId.Enabled)
      {
        tuningDetail.StreamId = numericTextBoxPlpId.Value;
      }
      else
      {
        tuningDetail.StreamId = -1;
      }
      tuningDetail.OriginalNetworkId = numericTextBoxOriginalNetworkId.Value;
      tuningDetail.TransportStreamId = numericTextBoxTransportStreamId.Value;
      tuningDetail.ServiceId = numericTextBoxServiceId.Value;
      tuningDetail.OpenTvChannelId = numericTextBoxOpenTvChannelId.Value;
      tuningDetail.PmtPid = numericTextBoxPmtPid.Value;
      tuningDetail.EpgOriginalNetworkId = numericTextBoxEpgOriginalNetworkId.Value;
      tuningDetail.EpgTransportStreamId = numericTextBoxEpgTransportStreamId.Value;
      tuningDetail.EpgServiceId = numericTextBoxEpgServiceId.Value;
    }

    private void comboBoxBroadcastStandard_SelectedIndexChanged(object sender, EventArgs e)
    {
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      numericTextBoxPlpId.Enabled = broadcastStandard == BroadcastStandard.DvbT2;
    }
  }
}