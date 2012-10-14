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
      string channelName = "";
      int channelFrequency = 0;
      int channelNumber = 0;
      int country = 31;
      bool isRadio = false;
      bool isTv = false;
      int tunerSource = 0;
      int videoInputType = 0;
      int audioInputType = 0;
      bool isVcrSignal = false;
      int symbolRate = 0;
      int modulation = (int)ModulationType.ModNotSet;
      int polarisation = (int)Polarisation.NotSet;
      int diseqc = 0;
      int bandwidth = 8;
      int pmtPid = -1;
      bool freeToAir = true;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int idLnbType = 0;
      int satIndex = -1;
      var innerFecRate = (int)BinaryConvolutionCodeRate.RateNotSet;
      var pilot = (int)Pilot.NotSet;
      var rollOff = (int)RollOff.NotSet;
      string url = "";
      int bitrate = 0;

      var initialTuningDetail = new TuningDetail
      {
        Name = channelName,
        Provider = provider,
        ChannelType = channelType,
        ChannelNumber = channelNumber,
        Frequency = (int)channelFrequency,
        CountryId = country,
        NetworkId = networkId,
        TransportId = transportId,
        ServiceId = serviceId,
        PmtPid = pmtPid,
        FreeToAir = true,
        Modulation = modulation,
        Polarisation = polarisation,
        Symbolrate = symbolRate,
        DiSEqC = diseqc,
        Bandwidth = bandwidth,
        MajorChannel = majorChannel,
        MinorChannel = minorChannel,
        VideoSource = videoInputType,
        AudioSource = audioInputType,
        IsVCRSignal = false,
        TuningSource = tunerSource,
        SatIndex = satIndex,
        InnerFecRate = innerFecRate,
        Pilot = pilot,
        RollOff = rollOff,
        Url = url,
        Bitrate = 0,
        IdLnbType = idLnbType
      };

      return initialTuningDetail;
    }
  }
}