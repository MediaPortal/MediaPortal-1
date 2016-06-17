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
  public partial class FormEditTuningDetailScte : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailScte()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxModulation.Items.Clear();
      comboBoxModulation.Items.AddRange(new string[]
      {
        ModulationSchemeQam.Automatic.GetDescription(),
        ModulationSchemeQam.Qam64.GetDescription(),
        ModulationSchemeQam.Qam256.GetDescription()
      });

      if (tuningDetail != null)
      {
        Text = "Edit SCTE Tuning Detail";
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
        comboBoxModulation.SelectedItem = ((ModulationSchemeQam)tuningDetail.Modulation).GetDescription();
        numericTextBoxTransportStreamId.Value = tuningDetail.TransportStreamId;
        numericTextBoxProgramNumber.Value = tuningDetail.ServiceId;
        numericTextBoxSourceId.Value = tuningDetail.SourceId;
        numericTextBoxPmtPid.Value = tuningDetail.PmtPid;
      }
      else
      {
        Text = "Add SCTE Tuning Detail";
        numericTextBoxFrequency.Value = 0;      // switched digital video
        comboBoxModulation.SelectedItem = ModulationSchemeQam.Automatic.GetDescription();
        numericTextBoxTransportStreamId.Value = 0;
        numericTextBoxProgramNumber.Value = 0;
        numericTextBoxSourceId.Value = 0;
        numericTextBoxPmtPid.Value = 0;
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = (int)BroadcastStandard.Scte;
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.Modulation = Convert.ToInt32(typeof(ModulationSchemeQam).GetEnumFromDescription((string)comboBoxModulation.SelectedItem));
      tuningDetail.TransportStreamId = numericTextBoxTransportStreamId.Value;
      tuningDetail.ServiceId = numericTextBoxProgramNumber.Value;
      tuningDetail.SourceId = numericTextBoxSourceId.Value;
      tuningDetail.PmtPid = numericTextBoxPmtPid.Value;
    }
  }
}