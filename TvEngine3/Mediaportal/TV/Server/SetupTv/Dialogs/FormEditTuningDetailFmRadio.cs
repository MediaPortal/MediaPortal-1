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

using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTuningDetailFmRadio : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailFmRadio()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      if (tuningDetail != null)
      {
        Text = "Edit FM Radio Tuning Detail";
        numericTextBoxFrequency.Value = tuningDetail.Frequency;
      }
      else
      {
        Text = "Add FM Radio Tuning Detail";
        numericTextBoxFrequency.Value = 96200;
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = (int)BroadcastStandard.FmRadio;
      tuningDetail.Frequency = numericTextBoxFrequency.Value;
      tuningDetail.GrabEpg = false;
    }
  }
}