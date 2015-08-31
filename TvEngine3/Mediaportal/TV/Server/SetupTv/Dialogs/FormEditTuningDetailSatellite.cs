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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuningDetailSatellite : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailSatellite()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxBroadcastStandard.Items.Clear();
      comboBoxBroadcastStandard.Items.AddRange(new string[]
      {
        BroadcastStandard.DvbS.GetDescription(),
        BroadcastStandard.DvbS2.GetDescription(),
        BroadcastStandard.DigiCipher2.GetDescription(),
        BroadcastStandard.SatelliteTurboFec.GetDescription()
      });

      IList<Satellite> satellites = ServiceAgents.Instance.TunerServiceAgent.ListAllSatellites();
      Satellite[] satellitesArray = new Satellite[satellites.Count];
      satellites.CopyTo(satellitesArray, 0);
      comboBoxSatellite.Items.Clear();
      comboBoxSatellite.Items.AddRange(satellitesArray);

      comboBoxPolarisation.Items.Clear();
      comboBoxPolarisation.Items.AddRange(typeof(Polarisation).GetDescriptions());

      comboBoxModulation.Items.Clear();
      comboBoxModulation.Items.AddRange(typeof(ModulationSchemePsk).GetDescriptions());

      comboBoxFecCodeRate.Items.Clear();
      comboBoxFecCodeRate.Items.AddRange(typeof(FecCodeRate).GetDescriptions());

      comboBoxPilotTonesState.Items.Clear();
      comboBoxPilotTonesState.Items.AddRange(typeof(PilotTonesState).GetDescriptions());

      comboBoxRollOffFactor.Items.Clear();
      comboBoxRollOffFactor.Items.AddRange(typeof(RollOffFactor).GetDescriptions());

      if (tuningDetail != null)
      {
        BroadcastStandard broadcastStandard = (BroadcastStandard)tuningDetail.BroadcastStandard;
        comboBoxBroadcastStandard.SelectedItem = broadcastStandard.GetDescription();
        // TODO select satellite
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        comboBoxPolarisation.SelectedItem = ((Polarisation)tuningDetail.Polarisation).GetDescription();
        comboBoxModulation.SelectedItem = ((ModulationSchemePsk)tuningDetail.Modulation).GetDescription();
        numericTextBoxSymbolRate.Value = tuningDetail.SymbolRate;
        comboBoxFecCodeRate.SelectedItem = ((FecCodeRate)tuningDetail.FecCodeRate).GetDescription();
        comboBoxPilotTonesState.SelectedItem = ((PilotTonesState)tuningDetail.PilotTonesState).GetDescription();
        comboBoxRollOffFactor.SelectedItem = ((RollOffFactor)tuningDetail.RollOffFactor).GetDescription();
        numericTextBoxInputStreamId.Value = tuningDetail.StreamId;
        numericTextBoxOriginalNetworkId.Value = tuningDetail.OriginalNetworkId;
        numericTextBoxTransportStreamId.Value = tuningDetail.TransportStreamId;
        numericTextBoxServiceId.Value = tuningDetail.ServiceId;
        numericTextBoxFreesatChannelId.Value = tuningDetail.FreesatChannelId;
        numericTextBoxOpenTvChannelId.Value = tuningDetail.OpenTvChannelId;
        numericTextBoxPmtPid.Value = tuningDetail.PmtPid;
        numericTextBoxEpgOriginalNetworkId.Value = tuningDetail.EpgOriginalNetworkId;
        numericTextBoxEpgTransportStreamId.Value = tuningDetail.EpgTransportStreamId;
        numericTextBoxEpgServiceId.Value = tuningDetail.EpgServiceId;

        bool isDvbS2 = broadcastStandard == BroadcastStandard.DvbS2 || broadcastStandard == BroadcastStandard.DvbS2X;
        comboBoxPilotTonesState.Enabled = isDvbS2;
        comboBoxRollOffFactor.Enabled = isDvbS2;
        numericTextBoxInputStreamId.Enabled = isDvbS2;
      }
      else
      {
        comboBoxBroadcastStandard.SelectedItem = BroadcastStandard.DvbS2.GetDescription();
        if (comboBoxSatellite.Items.Count > 0)
        {
          comboBoxSatellite.SelectedIndex = 0;
        }
        numericTextBoxFrequency.Value = 11097000;
        comboBoxPolarisation.SelectedItem = Polarisation.Automatic.GetDescription();
        comboBoxModulation.SelectedItem = ModulationSchemePsk.Automatic.GetDescription();
        numericTextBoxSymbolRate.Value = 25000;
        comboBoxFecCodeRate.SelectedItem = FecCodeRate.Automatic.GetDescription();
        comboBoxPilotTonesState.SelectedItem = PilotTonesState.Automatic.GetDescription();
        comboBoxRollOffFactor.SelectedItem = RollOffFactor.Automatic.GetDescription();
        numericTextBoxInputStreamId.Value = -1;
        numericTextBoxOriginalNetworkId.Value = 0;
        numericTextBoxTransportStreamId.Value = 0;
        numericTextBoxServiceId.Value = 0;
        numericTextBoxFreesatChannelId.Value = 0;
        numericTextBoxOpenTvChannelId.Value = 0;
        numericTextBoxPmtPid.Value = 0;
        numericTextBoxEpgOriginalNetworkId.Value = 0;
        numericTextBoxEpgTransportStreamId.Value = 0;
        numericTextBoxEpgServiceId.Value = 0;

        // broadcast standard DVB-S2
        comboBoxPilotTonesState.Enabled = true;
        comboBoxRollOffFactor.Enabled = true;
        numericTextBoxInputStreamId.Enabled = true;
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem));
      // TODO save satellite
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Polarisation = Convert.ToInt32(typeof(Polarisation).GetEnumFromDescription((string)comboBoxPolarisation.SelectedItem));
      tuningDetail.Modulation = Convert.ToInt32(typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem));
      tuningDetail.SymbolRate = numericTextBoxSymbolRate.Value;
      tuningDetail.FecCodeRate = Convert.ToInt32(typeof(FecCodeRate).GetEnumFromDescription((string)comboBoxFecCodeRate.SelectedItem));
      if (comboBoxPilotTonesState.Enabled)
      {
        tuningDetail.PilotTonesState = Convert.ToInt32(typeof(PilotTonesState).GetEnumFromDescription((string)comboBoxPilotTonesState.SelectedItem));
      }
      else
      {
        tuningDetail.PilotTonesState = (int)PilotTonesState.Automatic;
      }
      if (comboBoxRollOffFactor.Enabled)
      {
        tuningDetail.RollOffFactor = Convert.ToInt32(typeof(RollOffFactor).GetEnumFromDescription((string)comboBoxRollOffFactor.SelectedItem));
      }
      else
      {
        tuningDetail.RollOffFactor = (int)RollOffFactor.Automatic;
      }
      if (numericTextBoxInputStreamId.Enabled)
      {
        tuningDetail.StreamId = numericTextBoxInputStreamId.Value;
      }
      else
      {
        tuningDetail.StreamId = -1;
      }
      tuningDetail.OriginalNetworkId = numericTextBoxOriginalNetworkId.Value;
      tuningDetail.TransportStreamId = numericTextBoxTransportStreamId.Value;
      tuningDetail.ServiceId = numericTextBoxServiceId.Value;
      tuningDetail.FreesatChannelId = numericTextBoxFreesatChannelId.Value;
      tuningDetail.OpenTvChannelId = numericTextBoxOpenTvChannelId.Value;
      tuningDetail.PmtPid = numericTextBoxPmtPid.Value;
      tuningDetail.EpgOriginalNetworkId = numericTextBoxEpgOriginalNetworkId.Value;
      tuningDetail.EpgTransportStreamId = numericTextBoxEpgTransportStreamId.Value;
      tuningDetail.EpgServiceId = numericTextBoxEpgServiceId.Value;
    }

    private void comboBoxBroadcastStandard_SelectedIndexChanged(object sender, EventArgs e)
    {
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      bool isDvbS2 = broadcastStandard == BroadcastStandard.DvbS2 || broadcastStandard == BroadcastStandard.DvbS2X;
      comboBoxPilotTonesState.Enabled = isDvbS2;
      comboBoxRollOffFactor.Enabled = isDvbS2;
      numericTextBoxInputStreamId.Enabled = isDvbS2;
    }
  }
}