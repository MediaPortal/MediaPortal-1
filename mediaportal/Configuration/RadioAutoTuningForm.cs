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

			//
			// Disable start button until we've completed the implementation
			//
			startButton.Enabled = false;
		}

		public override void OnStartTuning()
		{
		}

		public override void OnStopTuning()
		{
		}

		public override void OnPerformTuning()
		{
			if(captureDevice.Tuner.SignalPresent == true)
			{
				//
				// We have found a channel!
				//
				AddItem(new RadioStation(currentChannel, captureDevice.Tuner.Channel));

				//
				// Move forward
				//
			}
			else
			{
				//
				// No match, 
			}
		}
	}
}
