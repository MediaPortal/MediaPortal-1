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
      {}
		}
	}
}
