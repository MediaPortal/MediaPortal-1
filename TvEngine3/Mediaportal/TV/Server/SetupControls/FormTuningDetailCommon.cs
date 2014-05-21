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
using System.ComponentModel;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.SetupControls
{
  public partial class FormTuningDetailCommon : Form
  {
    protected FormTuningDetailCommon()
    {
      InitializeComponent();
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TuningDetail TuningDetail { get; set; }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    protected static TuningDetail CreateInitialTuningDetail()
    {
      var initialTuningDetail = new TuningDetail
      {
        Name = string.Empty,
        Provider = string.Empty,
        ChannelType = 0,
        ChannelNumber = 0,
        Frequency = 0,
        CountryId = 31, // The Netherlands
        NetworkId = 0,
        TransportId = 0,
        ServiceId = 0,
        PmtPid = 0,
        FreeToAir = true,
        Modulation = -1,
        Polarisation = -1,
        Symbolrate = 0,
        DiSEqC = 0,
        Bandwidth = 8000,
        MajorChannel = 0,
        MinorChannel = 0,
        VideoSource = 0,
        AudioSource = 0,
        IsVCRSignal = false,
        TuningSource = 0,
        SatIndex = -1,
        InnerFecRate = -1,
        Pilot = -1,
        RollOff = -1,
        Url = string.Empty,
        Bitrate = 0,
        IdLnbType = null
      };

      return initialTuningDetail;
    }
  }
}