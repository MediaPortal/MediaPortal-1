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
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormSelectTuningDetailType : Form
  {
    private FormEditTuningDetailCommon _tuningDetailForm;
    private MediaType _mediaType;

    public FormSelectTuningDetailType(MediaType mediaType)
    {
      InitializeComponent();
      _mediaType = mediaType;
      radioButtonAnalogTv.Enabled = mediaType == MediaType.Television;
      radioButtonFmRadio.Enabled = !radioButtonAnalogTv.Enabled;
    }

    public FormEditTuningDetailCommon TuningDetailForm
    {
      get
      {
        return _tuningDetailForm;
      }
    }

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      BroadcastStandard broadcastStandard = BroadcastStandard.Unknown;
      if (radioButtonAmRadio.Checked)
      {
        broadcastStandard = BroadcastStandard.AmRadio;
      }
      else if (radioButtonAnalogTv.Checked)
      {
        broadcastStandard = BroadcastStandard.AnalogTelevision;
      }
      else if (radioButtonAtsc.Checked)
      {
        broadcastStandard = BroadcastStandard.Atsc;
      }
      else if (radioButtonCapture.Checked)
      {
        broadcastStandard = BroadcastStandard.ExternalInput;
      }
      else if (radioButtonCable.Checked)
      {
        broadcastStandard = BroadcastStandard.DvbC;
      }
      else if (radioButtonTerrestrial.Checked)
      {
        broadcastStandard = BroadcastStandard.DvbT;
      }
      else if (radioButtonFmRadio.Checked)
      {
        broadcastStandard = BroadcastStandard.FmRadio;
      }
      else if (radioButtonSatellite.Checked)
      {
        broadcastStandard = BroadcastStandard.DvbS;
      }
      else if (radioButtonScte.Checked)
      {
        broadcastStandard = BroadcastStandard.Scte;
      }
      else if (radioButtonStream.Checked)
      {
        broadcastStandard = BroadcastStandard.DvbIp;
      }
      else
      {
        return;
      }

      _tuningDetailForm = GetTuningDetailFormForBroadcastStandard(broadcastStandard);

      DialogResult = DialogResult.OK;
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    public static FormEditTuningDetailCommon GetTuningDetailFormForBroadcastStandard(BroadcastStandard broadcastStandard)
    {
      switch (broadcastStandard)
      {
        case BroadcastStandard.AmRadio:
        case BroadcastStandard.FmRadio:
          return new FormEditTuningDetailAnalogRadio(broadcastStandard);
        case BroadcastStandard.AnalogTelevision:
          return new FormEditTuningDetailAnalogTv();
        case BroadcastStandard.Atsc:
        case BroadcastStandard.Scte:
          return new FormEditTuningDetailAtscScte(broadcastStandard);
        case BroadcastStandard.ExternalInput:
          return new FormEditTuningDetailCapture();
        case BroadcastStandard.DvbC:
        case BroadcastStandard.IsdbC:
          return new FormEditTuningDetailCable();
        case BroadcastStandard.DvbT:
        case BroadcastStandard.DvbT2:
        case BroadcastStandard.IsdbT:
          return new FormEditTuningDetailTerrestrial();
        case BroadcastStandard.DvbDsng:
        case BroadcastStandard.DvbS:
        case BroadcastStandard.DvbS2:
        case BroadcastStandard.DvbS2Pro:
        case BroadcastStandard.DvbS2X:
        case BroadcastStandard.IsdbS:
        case BroadcastStandard.SatelliteTurboFec:
        case BroadcastStandard.DigiCipher2:
        case BroadcastStandard.DirecTvDss:
          return new FormEditTuningDetailSatellite();
        case BroadcastStandard.DvbIp:
          return new FormEditTuningDetailStream();

        // Not implemented.
        case BroadcastStandard.DvbC2:
        case BroadcastStandard.Dab:
        default:
          return null;
      }
    }
  }
}