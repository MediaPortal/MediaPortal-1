using System;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using System.Xml;

namespace MediaPortal.TV.Recording
{

	/// <summary>
	/// Summary description for DVBTTuning.
	/// </summary>
	public class DVBTTuning : ITuning
	{
		enum State
		{
			ScanFrequencies,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		ArrayList                           frequencies=new ArrayList();
		int                                 currentFrequencyIndex=0;
		private System.Windows.Forms.Timer  timer1;
		State                               currentState;
		DateTime														channelScanTimeOut;

		public DVBTTuning()
		{
		}
		#region ITuning Members

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			captureCard=card;
			callback=statusCallback;

			currentState=State.ScanFrequencies;
			frequencies.Clear();
			currentFrequencyIndex=0;

			Log.Write("Opening dvbt.xml");
			XmlDocument doc= new XmlDocument();
			doc.Load("dvbt.xml");
			XmlNodeList frequencyList= doc.DocumentElement.SelectNodes("/dvbt/frequencies/frequency");
			foreach (XmlNode node in frequencyList)
			{
				int carrierFrequency= XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"carrier").InnerText);
				frequencies.Add(carrierFrequency);
				//Log.Write("added:{0}", carrierFrequency);
			}
			
			Log.Write("loaded:{0} frequencies", frequencies.Count);
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
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

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (currentFrequencyIndex > frequencies.Count)
				return;
			
			float percent = ((float)currentFrequencyIndex) / ((float)frequencies.Count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			float frequency=(float)((int)frequencies[currentFrequencyIndex]);
			frequency /=1000;
			string description=String.Format("frequency:{0:###.##} MHz.", frequency);


			if (currentState==State.ScanFrequencies)
			{
				if (captureCard.SignalPresent())
				{
					Log.Write("Found signal at:{0} MHz",frequency);
					currentState=State.ScanChannels;
					channelScanTimeOut=DateTime.Now;
				}
			}

			if (currentState==State.ScanFrequencies)
			{
				callback.OnStatus(description);
				ScanNextFrequency();
			}

			if (currentState==State.ScanChannels)
			{
				description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", frequency);
				callback.OnStatus(description);
				ScanChannels();
			}
			
		}

		void ScanChannels()
		{
			captureCard.Process();

			TimeSpan ts = DateTime.Now-channelScanTimeOut;
			if (ts.TotalSeconds>=15)
			{
				
				Log.Write("timeout, goto scanning frequencies");
				currentState=State.ScanFrequencies;
				ScanNextFrequency();
				return;
			}


		}

		void ScanNextFrequency()
		{
			
			currentFrequencyIndex++;
			if (currentFrequencyIndex>=frequencies.Count)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}

			Log.Write("tune:{0}",frequencies[currentFrequencyIndex]);
			captureCard.Tune(frequencies[currentFrequencyIndex]);
		}

		#endregion
	}
}
