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
      int pcrPid = -1;
      int pmtPid = -1;
      int networkId = -1;
      int serviceId = -1;
      int transportId = -1;
      int minorChannel = -1;
      int majorChannel = -1;
      string provider = "";
      int channelType = 0;
      int videoPid = -1;
      int audioPid = -1;
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
                                    bandwidth, majorChannel, minorChannel, pcrPid, videoInputType,
                                    audioInputType, false, tunerSource, videoPid, audioPid, band,
                                    satIndex,
                                    innerFecRate, pilot, rollOff, url, 0);
    }
  }
}
