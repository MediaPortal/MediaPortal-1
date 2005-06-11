using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using System.Xml;


namespace MediaPortal.TV.Recording
{

	/// <summary>
	/// Summary description for ATSCTuning.
	/// </summary>
	public class ATSCTuning : ITuning
	{
		const int MaxATSCChannel=255;
		enum State
		{
			ScanStart,
			ScanFrequencies,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		int                                 currentIndex=-1;
		private System.Windows.Forms.Timer  timer1;
		
		int																	retryCount=0;

		int newChannels, updatedChannels;
		public ATSCTuning()
		{
		}
		#region ITuning Members
		public void Stop()
		{
			timer1.Enabled=false;
			captureCard.DeleteGraph();
		}

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			newChannels=0;
			updatedChannels=0;
			retryCount=0;
			captureCard=card;
			callback=statusCallback;

			currentIndex=-1;
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
			return;
		}

		public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback)
		{
			// TODO:  Add ATSCTuning.AutoTuneRadio implementation
		}

		public void Continue()
		{
			// TODO:  Add ATSCTuning.Continue implementation
		}

		public int MapToChannel(string channel)
		{
			// TODO:  Add ATSCTuning.MapToChannel implementation
			return 0;
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (currentIndex >= MaxATSCChannel)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				return;
			}

			timer1.Enabled=false;
			int index=currentIndex;
			if (index<0) index=0;
			float percent = ((float)index) / ((float)MaxATSCChannel);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			
			if (retryCount==0)
			{
				ScanNextChannel();
				if (captureCard.SignalPresent())
				{
					ScanChannels();
				}
				retryCount=1;
			}
			else 
			{
				ScanChannel();
				if (captureCard.SignalPresent())
				{
					ScanChannels();
				}
				retryCount=0;
			}
			timer1.Enabled=true;
		}

		void ScanChannels()
		{
			string chanDesc=String.Format("Channel:{0} retry:{1}",currentIndex, retryCount);
			string description=String.Format("Found signal for channel:{0} {1}, Scanning channels", currentIndex,chanDesc);
			callback.OnStatus(description);

			timer1.Enabled=false;
			Application.DoEvents();
			callback.OnStatus2( String.Format("new:{0} updated:{1}", newChannels,updatedChannels) );
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels);
			callback.OnStatus2( String.Format("new:{0} updated:{1}", newChannels,updatedChannels) );
			callback.UpdateList();
			timer1.Enabled=true;
			return;
		}

		void ScanNextChannel()
		{
			currentIndex++;
			ScanChannel();
			Application.DoEvents();
		}

		void ScanChannel()
		{
			if (currentIndex<0 || currentIndex>=MaxATSCChannel)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}
			timer1.Enabled=false;

			string chanDesc=String.Format("Channel:{0} retry:{1}",currentIndex, retryCount);
			string description=String.Format("Channel:{0}/{1} {2}", currentIndex,MaxATSCChannel,chanDesc);
			callback.OnStatus(description);

			Log.WriteFile(Log.LogType.Capture,"tune channel:{0}/{1} {2}",currentIndex ,MaxATSCChannel,chanDesc);

			DVBChannel newchan = new DVBChannel();
			newchan.NetworkID=-1;
			newchan.TransportStreamID=-1;
			newchan.ProgramNumber=-1;
			newchan.MinorChannel=-1;
			newchan.MajorChannel=-1;
			newchan.Frequency=-1;
			newchan.PhysicalChannel=currentIndex;

			newchan.Modulation=(int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
			newchan.Symbolrate=-1;
			newchan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			newchan.Frequency=-1;
			captureCard.Tune(newchan,0);
			Application.DoEvents();
			timer1.Enabled=true;
		}
		#endregion
	}
}
