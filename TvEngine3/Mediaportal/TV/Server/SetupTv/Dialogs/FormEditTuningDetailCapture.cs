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
  public partial class FormEditTuningDetailCapture : FormEditTuningDetailCommon
  {
    public FormEditTuningDetailCapture()
    {
      InitializeComponent();
    }

    protected override void LoadProperties(TuningDetail tuningDetail)
    {
      comboBoxVideoSource.Items.Clear();
      comboBoxVideoSource.Items.AddRange(typeof(CaptureSourceVideo).GetDescriptions(~(int)CaptureSourceVideo.Tuner));

      comboBoxAudioSource.Items.Clear();
      comboBoxAudioSource.Items.AddRange(typeof(CaptureSourceAudio).GetDescriptions(~(int)CaptureSourceAudio.Tuner));

      if (tuningDetail != null)
      {
        Text = "Edit Capture Tuning Detail";
        comboBoxVideoSource.SelectedItem = ((CaptureSourceVideo)tuningDetail.VideoSource).GetDescription();
        comboBoxAudioSource.SelectedItem = ((CaptureSourceAudio)tuningDetail.AudioSource).GetDescription();
        checkBoxIsVcrSignal.Checked = tuningDetail.IsVcrSignal;
      }
      else
      {
        Text = "Add Capture Tuning Detail";
        comboBoxVideoSource.SelectedItem = CaptureSourceVideo.TunerDefault.GetDescription();
        comboBoxAudioSource.SelectedItem = CaptureSourceAudio.TunerDefault.GetDescription();
        checkBoxIsVcrSignal.Checked = false;
      }
    }

    protected override void UpdateProperties(TuningDetail tuningDetail)
    {
      tuningDetail.BroadcastStandard = (int)BroadcastStandard.ExternalInput;
      tuningDetail.VideoSource = Convert.ToInt32(typeof(CaptureSourceVideo).GetEnumFromDescription((string)comboBoxVideoSource.SelectedItem));
      tuningDetail.AudioSource = Convert.ToInt32(typeof(CaptureSourceAudio).GetEnumFromDescription((string)comboBoxAudioSource.SelectedItem));
      tuningDetail.IsVcrSignal = checkBoxIsVcrSignal.Checked;
    }
  }
}