using System;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{

	/// <summary>
	/// Summary description for DVBTTuning.
	/// </summary>
	public class DVBTTuning : ITuning
	{
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		public DVBTTuning()
		{
		}
		#region ITuning Members

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			captureCard=card;
			callback=statusCallback;
			return;
		}

		public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback)
		{
			// TODO:  Add DVBTTuning.AutoTuneRadio implementation
		}

		public void Continue()
		{
			// TODO:  Add DVBTTuning.Continue implementation
		}

		public int MapToChannel(string channel)
		{
			// TODO:  Add DVBTTuning.MapToChannel implementation
			return 0;
		}

		#endregion
	}
}
