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
  public partial class FormEditTuningDetailTerrestrial : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailTerrestrial()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxBroadcastStandard.Items.Clear();
      comboBoxBroadcastStandard.Items.AddRange(new string[]
      {
        BroadcastStandard.DvbT.GetDescription(),
        BroadcastStandard.DvbT2.GetDescription(),
        BroadcastStandard.IsdbT.GetDescription()
      });

      if (tuningDetail != null)
      {
        Text = "Edit Terrestrial Tuning Detail";
        comboBoxBroadcastStandard.SelectedItem = ((BroadcastStandard)tuningDetail.BroadcastStandard).GetDescription();
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        numericTextBoxBandwidth.Value = tuningDetail.Bandwidth;
        if (numericTextBoxPlpId.Enabled)
        {
          numericTextBoxPlpId.Value = tuningDetail.StreamId;
        }
        else
        {
          numericTextBoxPlpId.Value = -1;
        }
        numericTextBoxOriginalNetworkId.Value = tuningDetail.OriginalNetworkId;
        numericTextBoxTransportStreamId.Value = tuningDetail.TransportStreamId;
        numericTextBoxServiceId.Value = tuningDetail.ServiceId;
        if (numericTextBoxOpenTvChannelId.Enabled)
        {
          numericTextBoxOpenTvChannelId.Value = tuningDetail.OpenTvChannelId;
        }
        else
        {
          numericTextBoxOpenTvChannelId.Value = 0;
        }
        numericTextBoxPmtPid.Value = tuningDetail.PmtPid;
        numericTextBoxEpgOriginalNetworkId.Value = tuningDetail.EpgOriginalNetworkId;
        numericTextBoxEpgTransportStreamId.Value = tuningDetail.EpgTransportStreamId;
        numericTextBoxEpgServiceId.Value = tuningDetail.EpgServiceId;
      }
      else
      {
        Text = "Add Terrestrial Tuning Detail";
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
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      TuningDetail defaults = new TuningDetail();

      tuningDetail.BroadcastStandard = Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem));
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Bandwidth = numericTextBoxBandwidth.Value;
      if (numericTextBoxPlpId.Enabled)
      {
        tuningDetail.StreamId = numericTextBoxPlpId.Value;
      }
      else
      {
        tuningDetail.StreamId = defaults.StreamId;
      }
      tuningDetail.OriginalNetworkId = numericTextBoxOriginalNetworkId.Value;
      tuningDetail.TransportStreamId = numericTextBoxTransportStreamId.Value;
      tuningDetail.ServiceId = numericTextBoxServiceId.Value;
      if (numericTextBoxOpenTvChannelId.Enabled)
      {
        tuningDetail.OpenTvChannelId = numericTextBoxOpenTvChannelId.Value;
      }
      else
      {
        tuningDetail.OpenTvChannelId = defaults.OpenTvChannelId;
      }
      tuningDetail.PmtPid = numericTextBoxPmtPid.Value;
      tuningDetail.EpgOriginalNetworkId = numericTextBoxEpgOriginalNetworkId.Value;
      tuningDetail.EpgTransportStreamId = numericTextBoxEpgTransportStreamId.Value;
      tuningDetail.EpgServiceId = numericTextBoxEpgServiceId.Value;
    }

    private void comboBoxBroadcastStandard_SelectedIndexChanged(object sender, EventArgs e)
    {
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      numericTextBoxPlpId.Enabled = broadcastStandard == BroadcastStandard.DvbT2;
      numericTextBoxOpenTvChannelId.Enabled = BroadcastStandard.MaskOpenTvSi.HasFlag(broadcastStandard);
    }
  }
}