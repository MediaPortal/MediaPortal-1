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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuningDetailAtscScte : FormEditTuningDetailCommon
  {
    private BroadcastStandard _broadcastStandard = BroadcastStandard.Atsc;
    private Type _modulationSchemeType = typeof(ModulationSchemeVsb);

    public FormEditTuningDetailAtscScte(BroadcastStandard broadcastStandard = BroadcastStandard.Atsc)
    {
      InitializeComponent();

      if (broadcastStandard != BroadcastStandard.Atsc && broadcastStandard != BroadcastStandard.Scte)
      {
        throw new NotSupportedException();
      }
      _broadcastStandard = broadcastStandard;
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxModulation.Items.Clear();
      if (_broadcastStandard == BroadcastStandard.Atsc)
      {
        numericTextBoxFrequency.Value = 57000;   // physical channel 2
        _modulationSchemeType = typeof(ModulationSchemeVsb);
        comboBoxModulation.Items.AddRange(_modulationSchemeType.GetDescriptions());
      }
      else
      {
        numericTextBoxFrequency.Value = ChannelScte.FREQUENCY_SWITCHED_DIGITAL_VIDEO;
        _modulationSchemeType = typeof(ModulationSchemeQam);
        comboBoxModulation.Items.AddRange(_modulationSchemeType.GetDescriptions(ModCod.CABLE[_broadcastStandard]));
      }
      numericTextBoxFrequency.MinimumValue = numericTextBoxFrequency.Value;

      if (tuningDetail != null)
      {
        Text = string.Format("Edit {0} Tuning Detail", _broadcastStandard.GetDescription());
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        comboBoxModulation.SelectedItem = ((Enum)Enum.ToObject(_modulationSchemeType, tuningDetail.Modulation)).GetDescription();
        numericTextBoxTransportStreamId.Value = tuningDetail.TransportStreamId;
        numericTextBoxProgramNumber.Value = tuningDetail.ServiceId;
        numericTextBoxSourceId.Value = tuningDetail.SourceId;
        numericTextBoxPmtPid.Value = tuningDetail.PmtPid;
      }
      else
      {
        Text = string.Format("Add {0} Tuning Detail", _broadcastStandard.GetDescription());
        comboBoxModulation.SelectedIndex = 0;
        numericTextBoxTransportStreamId.Value = 0;
        numericTextBoxProgramNumber.Value = 0;
        numericTextBoxSourceId.Value = 0;
        numericTextBoxPmtPid.Value = 0;
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = (int)_broadcastStandard;
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Modulation = Convert.ToInt32(_modulationSchemeType.GetEnumFromDescription((string)comboBoxModulation.SelectedItem));
      tuningDetail.TransportStreamId = numericTextBoxTransportStreamId.Value;
      tuningDetail.ServiceId = numericTextBoxProgramNumber.Value;
      tuningDetail.SourceId = numericTextBoxSourceId.Value;
      tuningDetail.PmtPid = numericTextBoxPmtPid.Value;
    }
  }
}