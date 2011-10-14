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
using DirectShowLib.BDA;
using TvDatabase;

namespace SetupControls
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
      string channelName = "";
      long channelFrequency = 0;
      int channelNumber = 0;
      int country = 31;
      int tunerSource = 0;
      int videoInputType = 0;
      int audioInputType = 0;
      int symbolRate = 0;
      int modulation = 0;
      int polarisation = 0;
      int switchFrequency = 0;
      int diseqc = 0;
      int bandwidth = 8;
      int pmtPid = -1;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int band = 0;
      int satIndex = -1;
      var innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      var pilot = (int)Pilot.NotSet;
      var rollOff = (int)RollOff.NotSet;
      string url = "";
      return new TuningDetail(-1, channelName, provider,
                              channelType, channelNumber, (int)channelFrequency, country, false, false,
                              networkId, transportId, serviceId, pmtPid, true,
                              modulation, polarisation, symbolRate, diseqc, switchFrequency,
                              bandwidth, majorChannel, minorChannel, videoInputType,
                              audioInputType, false, tunerSource, band,
                              satIndex,
                              innerFecRate, pilot, rollOff, url, 0);
    }
  }
}