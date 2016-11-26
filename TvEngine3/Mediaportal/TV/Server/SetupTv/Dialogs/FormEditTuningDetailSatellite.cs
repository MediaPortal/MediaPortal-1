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
using Mediaportal.TV.Server.Common.Types.Channel.Constant;
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
      comboBoxBroadcastStandard.Items.AddRange(typeof(BroadcastStandard).GetDescriptions((int)BroadcastStandard.MaskSatellite, false));

      IList<Satellite> satellites = ServiceAgents.Instance.TunerServiceAgent.ListAllSatellites();
      Satellite[] satellitesArray = new Satellite[satellites.Count];
      satellites.CopyTo(satellitesArray, 0);
      comboBoxSatellite.Items.Clear();
      comboBoxSatellite.Items.AddRange(satellitesArray);

      comboBoxPolarisation.Items.Clear();
      comboBoxPolarisation.Items.AddRange(typeof(Polarisation).GetDescriptions());

      comboBoxPilotTonesState.Items.Clear();
      comboBoxPilotTonesState.Items.AddRange(typeof(PilotTonesState).GetDescriptions());

      if (tuningDetail != null)
      {
        Text = "Edit Satellite Tuning Detail";
        comboBoxBroadcastStandard.SelectedItem = ((BroadcastStandard)tuningDetail.BroadcastStandard).GetDescription();

        foreach (Satellite satellite in satellites)
        {
          if (tuningDetail.IdSatellite == satellite.IdSatellite)
          {
            comboBoxSatellite.SelectedItem = satellite;
            break;
          }
        }

        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        comboBoxPolarisation.SelectedItem = ((Polarisation)tuningDetail.Polarisation).GetDescription();
        comboBoxModulation.SelectedItem = ((ModulationSchemePsk)tuningDetail.Modulation).GetDescription();
        numericTextBoxSymbolRate.Value = tuningDetail.SymbolRate;
        comboBoxFecCodeRate.SelectedItem = ((FecCodeRate)tuningDetail.FecCodeRate).GetDescription();
        if (comboBoxRollOffFactor.Enabled)
        {
          comboBoxRollOffFactor.SelectedItem = ((RollOffFactor)tuningDetail.RollOffFactor).GetDescription();
        }
        else
        {
          comboBoxRollOffFactor.SelectedItem = RollOffFactor.Automatic.GetDescription();
        }
        if (comboBoxPilotTonesState.Enabled)
        {
          comboBoxPilotTonesState.SelectedItem = ((PilotTonesState)tuningDetail.PilotTonesState).GetDescription();
        }
        else
        {
          comboBoxPilotTonesState.SelectedItem = PilotTonesState.Automatic.GetDescription();
        }
        if (numericTextBoxInputStreamId.Enabled)
        {
          numericTextBoxInputStreamId.Value = tuningDetail.StreamId;
        }
        else
        {
          numericTextBoxInputStreamId.Value = -1;
        }
        numericTextBoxOriginalNetworkId.Value = tuningDetail.OriginalNetworkId;
        numericTextBoxTransportStreamId.Value = tuningDetail.TransportStreamId;
        numericTextBoxServiceId.Value = tuningDetail.ServiceId;
        if (numericTextBoxFreesatChannelId.Enabled)
        {
          numericTextBoxFreesatChannelId.Value = tuningDetail.FreesatChannelId;
        }
        else
        {
          numericTextBoxFreesatChannelId.Value = 0;
        }
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
        Text = "Add Satellite Tuning Detail";
        comboBoxBroadcastStandard.SelectedItem = BroadcastStandard.DvbS2.GetDescription();

        int? defaultLongitude = Satellite.DefaultSatelliteLongitude;
        if (defaultLongitude.HasValue)
        {
          foreach (Satellite satellite in satellites)
          {
            if (defaultLongitude == satellite.Longitude)
            {
              comboBoxSatellite.SelectedItem = satellite;
              break;
            }
          }
        }
        if (comboBoxSatellite.SelectedItem == null)
        {
          comboBoxSatellite.SelectedIndex = 0;
        }

        numericTextBoxFrequency.Value = 11097000;
        comboBoxPolarisation.SelectedItem = Polarisation.Automatic.GetDescription();
        comboBoxModulation.SelectedItem = ModulationSchemePsk.Automatic.GetDescription();
        numericTextBoxSymbolRate.Value = 25000;
        comboBoxFecCodeRate.SelectedItem = FecCodeRate.Automatic.GetDescription();
        comboBoxRollOffFactor.SelectedItem = RollOffFactor.Automatic.GetDescription();
        comboBoxPilotTonesState.SelectedItem = PilotTonesState.Automatic.GetDescription();
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
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      TuningDetail defaults = new TuningDetail();

      tuningDetail.BroadcastStandard = Convert.ToInt32(typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem));

      Satellite satellite = (Satellite)comboBoxSatellite.SelectedItem;
      tuningDetail.Satellite = satellite;

      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Polarisation = Convert.ToInt32(typeof(Polarisation).GetEnumFromDescription((string)comboBoxPolarisation.SelectedItem));
      tuningDetail.Modulation = Convert.ToInt32(typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem));
      tuningDetail.SymbolRate = numericTextBoxSymbolRate.Value;
      tuningDetail.FecCodeRate = Convert.ToInt32(typeof(FecCodeRate).GetEnumFromDescription((string)comboBoxFecCodeRate.SelectedItem));
      if (comboBoxRollOffFactor.Enabled)
      {
        tuningDetail.RollOffFactor = Convert.ToInt32(typeof(RollOffFactor).GetEnumFromDescription((string)comboBoxRollOffFactor.SelectedItem));
      }
      else
      {
        tuningDetail.RollOffFactor = defaults.RollOffFactor;
      }
      if (comboBoxPilotTonesState.Enabled)
      {
        tuningDetail.PilotTonesState = Convert.ToInt32(typeof(PilotTonesState).GetEnumFromDescription((string)comboBoxPilotTonesState.SelectedItem));
      }
      else
      {
        tuningDetail.PilotTonesState = defaults.PilotTonesState;
      }
      if (numericTextBoxInputStreamId.Enabled)
      {
        tuningDetail.StreamId = numericTextBoxInputStreamId.Value;
      }
      else
      {
        tuningDetail.StreamId = defaults.StreamId;
      }
      if (numericTextBoxOriginalNetworkId.Enabled)
      {
        tuningDetail.OriginalNetworkId = numericTextBoxOriginalNetworkId.Value;
      }
      else
      {
        tuningDetail.OriginalNetworkId = defaults.OriginalNetworkId;
      }
      tuningDetail.TransportStreamId = numericTextBoxTransportStreamId.Value;
      tuningDetail.ServiceId = numericTextBoxServiceId.Value;
      if (numericTextBoxFreesatChannelId.Enabled)
      {
        tuningDetail.FreesatChannelId = numericTextBoxFreesatChannelId.Value;
      }
      else
      {
        tuningDetail.FreesatChannelId = defaults.FreesatChannelId;
      }
      if (numericTextBoxOpenTvChannelId.Enabled)
      {
        tuningDetail.OpenTvChannelId = numericTextBoxOpenTvChannelId.Value;
      }
      else
      {
        tuningDetail.OpenTvChannelId = defaults.OpenTvChannelId;
      }
      tuningDetail.PmtPid = numericTextBoxPmtPid.Value;
      if (groupBoxEpgSource.Enabled)
      {
        tuningDetail.EpgOriginalNetworkId = numericTextBoxEpgOriginalNetworkId.Value;
        tuningDetail.EpgTransportStreamId = numericTextBoxEpgTransportStreamId.Value;
        tuningDetail.EpgServiceId = numericTextBoxEpgServiceId.Value;
      }
      else
      {
        tuningDetail.EpgOriginalNetworkId = defaults.EpgOriginalNetworkId;
        tuningDetail.EpgTransportStreamId = defaults.EpgTransportStreamId;
        tuningDetail.EpgServiceId = defaults.EpgServiceId;
      }
    }

    private void comboBoxBroadcastStandard_SelectedIndexChanged(object sender, EventArgs e)
    {
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      bool isDvbS2 = BroadcastStandard.MaskDvbS2.HasFlag(broadcastStandard);
      comboBoxRollOffFactor.Enabled = isDvbS2 || broadcastStandard == BroadcastStandard.DvbDsng;
      comboBoxPilotTonesState.Enabled = isDvbS2;
      numericTextBoxInputStreamId.Enabled = isDvbS2;

      numericTextBoxFreesatChannelId.Enabled = BroadcastStandard.MaskFreesatSi.HasFlag(broadcastStandard);
      numericTextBoxOpenTvChannelId.Enabled = BroadcastStandard.MaskOpenTvSi.HasFlag(broadcastStandard);

      numericTextBoxOriginalNetworkId.Enabled = BroadcastStandard.MaskDvbSi.HasFlag(broadcastStandard);
      groupBoxEpgSource.Enabled = numericTextBoxOriginalNetworkId.Enabled;

      string selectedModulationDescription = (string)comboBoxModulation.SelectedItem;
      comboBoxModulation.BeginUpdate();
      try
      {
        comboBoxModulation.Items.Clear();
        string[] newModulationDescriptions = typeof(ModulationSchemePsk).GetDescriptions(ModCod.SATELLITE[broadcastStandard]);
        foreach (string modulationDescription in newModulationDescriptions)
        {
          comboBoxModulation.Items.Add(modulationDescription);
          if (string.Equals(modulationDescription, selectedModulationDescription))
          {
            comboBoxModulation.SelectedIndex = comboBoxModulation.Items.Count - 1;
          }
        }
        if (comboBoxModulation.SelectedItem == null)
        {
          comboBoxModulation.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxModulation.EndUpdate();
      }
      // Ensure that the choices for FEC code rate are updated when the
      // modulation scheme selection doesn't change. This may be unnecessary.
      comboBoxModulation_SelectedIndexChanged(null, null);

      string selectedRollOffFactorDescription = (string)comboBoxRollOffFactor.SelectedItem;
      comboBoxRollOffFactor.BeginUpdate();
      try
      {
        comboBoxRollOffFactor.Items.Clear();
        string[] newRollOffFactorDescriptions = typeof(RollOffFactor).GetDescriptions(ModCod.SATELLITE_ROLL_OFF_FACTOR[broadcastStandard]);
        foreach (string rollOffFactorDescription in newRollOffFactorDescriptions)
        {
          comboBoxRollOffFactor.Items.Add(rollOffFactorDescription);
          if (string.Equals(rollOffFactorDescription, selectedRollOffFactorDescription))
          {
            comboBoxRollOffFactor.SelectedIndex = comboBoxRollOffFactor.Items.Count - 1;
          }
        }
        if (comboBoxRollOffFactor.SelectedItem == null)
        {
          comboBoxRollOffFactor.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxRollOffFactor.EndUpdate();
      }
    }

    private void comboBoxModulation_SelectedIndexChanged(object sender, EventArgs e)
    {
      BroadcastStandard broadcastStandard = (BroadcastStandard)typeof(BroadcastStandard).GetEnumFromDescription((string)comboBoxBroadcastStandard.SelectedItem);
      ModulationSchemePsk modulationScheme = (ModulationSchemePsk)typeof(ModulationSchemePsk).GetEnumFromDescription((string)comboBoxModulation.SelectedItem);
      string selectedFecCodeRateDescription = (string)comboBoxFecCodeRate.SelectedItem;
      comboBoxFecCodeRate.BeginUpdate();
      try
      {
        comboBoxFecCodeRate.Items.Clear();
        string[] newFecCodeRateDescriptions = typeof(FecCodeRate).GetDescriptions(ModCod.SATELLITE_CODE_RATE[broadcastStandard][modulationScheme]);
        foreach (string fecCodeRateDescription in newFecCodeRateDescriptions)
        {
          comboBoxFecCodeRate.Items.Add(fecCodeRateDescription);
          if (string.Equals(fecCodeRateDescription, selectedFecCodeRateDescription))
          {
            comboBoxFecCodeRate.SelectedIndex = comboBoxFecCodeRate.Items.Count - 1;
          }
        }
        if (comboBoxFecCodeRate.SelectedItem == null)
        {
          comboBoxFecCodeRate.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxFecCodeRate.EndUpdate();
      }
    }
  }
}