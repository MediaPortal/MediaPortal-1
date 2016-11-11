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
using Mediaportal.TV.Server.Common.Types.Channel.Constant;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuningDetailCable : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailCable()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxBroadcastStandard.Items.Clear();
      comboBoxBroadcastStandard.Items.AddRange(new string[]
      {
        BroadcastStandard.DvbC.GetDescription(),
        BroadcastStandard.IsdbC.GetDescription()
      });

      comboBoxModulation.Items.Clear();
      comboBoxModulation.Items.AddRange(typeof(ModulationSchemeQam).GetDescriptions(ModCod.CABLE[BroadcastStandard.DvbC]));

      if (tuningDetail != null)
      {
        Text = "Edit Cable Tuning Detail";
        comboBoxBroadcastStandard.SelectedItem = ((BroadcastStandard)tuningDetail.BroadcastStandard).GetDescription();
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        if (comboBoxModulation.Enabled)
        {
          comboBoxModulation.SelectedItem = ((ModulationSchemeQam)tuningDetail.Modulation).GetDescription();
        }
        else
        {
          comboBoxModulation.SelectedItem = ModulationSchemeQam.Automatic.GetDescription();
        }
        if (numericTextBoxSymbolRate.Enabled)
        {
          numericTextBoxSymbolRate.Value = tuningDetail.SymbolRate;
        }
        else
        {
          numericTextBoxSymbolRate.Value = 6875;
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
        Text = "Add Cable Tuning Detail";
        comboBoxBroadcastStandard.SelectedItem = BroadcastStandard.DvbC.GetDescription();
        numericTextBoxFrequency.Value = 388000;
        comboBoxModulation.SelectedItem = ModulationSchemeQam.Automatic.GetDescription();
        numericTextBoxSymbolRate.Value = 6875;
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
      if (comboBoxModulation.Enabled)
      {
        tuningDetail.Modulation = Convert.ToInt32(typeof(ModulationSchemeQam).GetEnumFromDescription((string)comboBoxModulation.SelectedItem));
      }
      else
      {
        tuningDetail.Modulation = defaults.Modulation;
      }
      tuningDetail.SymbolRate = numericTextBoxSymbolRate.Value;
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
      bool isDvbC = broadcastStandard == BroadcastStandard.DvbC;
      comboBoxModulation.Enabled = isDvbC;
      numericTextBoxSymbolRate.Enabled = isDvbC;
      numericTextBoxOpenTvChannelId.Enabled = BroadcastStandard.MaskOpenTvSi.HasFlag(broadcastStandard);
    }
  }
}