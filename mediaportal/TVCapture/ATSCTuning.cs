/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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

		int newChannels, updatedChannels;
		int																	newRadioChannels, updatedRadioChannels;
		public ATSCTuning()
		{
		}
		#region ITuning Members
		public void Stop()
		{
			timer1.Enabled=false;
			captureCard.DeleteGraph();
		}
		public void Start()
		{
			currentIndex=-1;
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
		}
		public void Next()
		{
			if (currentIndex+1 >= MaxATSCChannel) return;
			currentIndex++;

			UpdateStatus();
			ScanChannel();
			if (captureCard.SignalPresent())
			{
				ScanChannels();
			}
		}
		public void Previous()
		{
			if (currentIndex >1) 
			{
				currentIndex--;

				UpdateStatus();
				ScanChannel();
				if (captureCard.SignalPresent())
				{
					ScanChannels();
				}
			}
		}

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			newRadioChannels=0;
			updatedRadioChannels=0;
			newChannels=0;
			updatedChannels=0;
			captureCard=card;
			callback=statusCallback;

			currentIndex=-1;
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
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

		void UpdateStatus()
		{
			int index=currentIndex;
			if (index<0) index=0;
			float percent = ((float)index) / ((float)MaxATSCChannel);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
		}
		private void timer1_Tick(object sender, System.EventArgs e)
		{
			timer1.Enabled=false;
			try
			{
				if (currentIndex >= MaxATSCChannel)
				{
					callback.OnProgress(100);
					callback.OnStatus("Finished");
					callback.OnEnded();
					return;
				}

				UpdateStatus();
				ScanNextChannel();
				if (captureCard.SignalPresent())
				{
						ScanChannels();
				}

			}
			catch(Exception ex)
			{
				Log.Write("Exception:{0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);
			}
			timer1.Enabled=true;
		}

		void ScanChannels()
		{
			Log.Write("atsc-scan:Found signal,scanning for channels. Quality:{0} level:{1}",captureCard.SignalQuality,captureCard.SignalStrength);
			string chanDesc=String.Format("Channel:{0}",currentIndex);
			string description=String.Format("Found signal for channel:{0} {1}, Scanning channels", currentIndex,chanDesc);
			callback.OnStatus(description);

			System.Threading.Thread.Sleep(400);
			callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);
			callback.OnStatus2( String.Format("new tv:{0} new radio:{1}", newChannels,newRadioChannels) );
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			callback.OnStatus2( String.Format("new tv:{0} new radio:{1}", newChannels,newRadioChannels) );

			callback.UpdateList();
			return;
		}

		void ScanNextChannel()
		{
			currentIndex++;
			ScanChannel();
		}

		void ScanChannel()
		{
			if (currentIndex<0 || currentIndex>=MaxATSCChannel)
			{
				callback.OnProgress(100);
				callback.OnStatus("Finished");
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}

			string chanDesc=String.Format("Channel:{0}",currentIndex);
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
			newchan.Frequency=-1;
			newchan.Symbolrate=-1;
			newchan.Modulation=(int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
			newchan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			captureCard.Tune(newchan,0);
			System.Threading.Thread.Sleep(400);

			if (captureCard.SignalQuality <40)
				System.Threading.Thread.Sleep(400);
			callback.OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);
		}
		#endregion
	}
}
