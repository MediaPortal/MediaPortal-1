using System;
using System.Collections;
using System.Windows.Forms;

using DShowNET;
using DirectX.Capture;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for RadioAutoTuningForm.
	/// </summary>
	public class RadioAutoTuningForm : AutoTuningForm
	{
		int		currentChannel = 0;
		Capture captureDevice = null;

		public RadioAutoTuningForm(Capture captureDevice)
		{
			this.captureDevice = captureDevice;

			//
			// Setup progress bar
			//
			SetStep(100000);
			SetInterval(captureDevice.Tuner.ChanelMinMax[0], captureDevice.Tuner.ChanelMinMax[1]);
		}

		public override void OnStartTuning(int startValue)
		{
			captureDevice.Tuner.Channel = startValue;
		}

		public override void OnStopTuning()
		{
      Text=String.Format("Radio tuning");
		}

		public override void OnPerformTuning(int stepSize)
		{
      try
      {
        if(captureDevice.Tuner.SignalPresent == true)
        {
          //
          // We have found a channel!
          //
          RadioStation newStation =new RadioStation(currentChannel, captureDevice.Tuner.Channel);
          newStation.Name=String.Format("Station{0}", currentChannel++);
          newStation.Type="Radio";
          newStation.Frequency=captureDevice.Tuner.Channel;
          AddItem(newStation);
        }
      }
      catch(NotSupportedException)
      {
        //
        // The device does not support checking the signal strength, thus we can't perform
        // autotuning.
        //
        MessageBox.Show("Failed to perform autotuning, the tuner card does not support reading of signal strength.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

			//
			// Move forward
			//
      try
      {
        captureDevice.Tuner.Channel += stepSize;
        double dFreq=(double)captureDevice.Tuner.Channel ;
        dFreq/=1000000d;
        Text=String.Format("Radio tuning:{0:#,##} MHz", captureDevice.Tuner.Channel );
      }
      catch(Exception)
      {
        MessageBox.Show("Failed to perform autotuning, the tuner card did not supply the data needed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
		}
	}
}
