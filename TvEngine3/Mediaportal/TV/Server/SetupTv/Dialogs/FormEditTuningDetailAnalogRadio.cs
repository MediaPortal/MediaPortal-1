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
  public partial class FormEditTuningDetailAnalogRadio : FormEditTuningDetailCommon
  {
    private BroadcastStandard _broadcastStandard = BroadcastStandard.AmRadio;

    public FormEditTuningDetailAnalogRadio(BroadcastStandard broadcastStandard)
    {
      InitializeComponent();

      if (broadcastStandard != BroadcastStandard.AmRadio && broadcastStandard != BroadcastStandard.FmRadio)
      {
        throw new NotSupportedException();
      }
      _broadcastStandard = broadcastStandard;
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      if (_broadcastStandard == BroadcastStandard.AmRadio)
      {
        numericTextBoxFrequency.Value = 1449;
        numericTextBoxFrequency.MinimumValue = 531;
        numericTextBoxFrequency.MaximumValue = 1700;
      }
      else
      {
        numericTextBoxFrequency.Value = 96200;
        numericTextBoxFrequency.MinimumValue = 76000;
        numericTextBoxFrequency.MaximumValue = 108000;
      }

      if (tuningDetail != null)
      {
        Text = string.Format("Edit {0} Tuning Detail", _broadcastStandard.GetDescription());
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
      }
      else
      {
        Text = string.Format("Add {0} Tuning Detail", _broadcastStandard.GetDescription());
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = (int)_broadcastStandard;
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.GrabEpg = false;
    }
  }
}