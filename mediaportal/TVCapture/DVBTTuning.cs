using System;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
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
			XmlDocument doc= new XmlDocument();
			doc.Load("dvbt.xml");
			XmlNodeList frequencyList= doc.DocumentElement.SelectNodes("/dvbt/frequencies/frequency");
			foreach (XmlNode node in frequencyList)
			{
				int carrierFrequency= XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"carrier").InnerText);
				frequencies.Add(carrierFrequency);
			}
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
			float percent = ((float)currentFrequencyIndex) / ((float)frequencies.Count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			float frequency=(float)frequencies[currentFrequencyIndex];
			frequency /=1000;
			string description=String.Format("frequency:{0:###.##} MHz.", frequency);

			if (captureCard.SignalPresent())
			{
				description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", frequency);
				currentState=State.ScanChannels;
			}

			if (currentState==State.ScanFrequencies)
			{
				callback.OnStatus(description);
				ScanNextFrequency();
			}

			if (currentState==State.ScanChannels)
			{
				callback.OnStatus(description);
				ScanChannels();
			}
		}

		void ScanChannels()
		{
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

			captureCard.Tune(frequencies[currentFrequencyIndex]);
		}

		#endregion
	}
}
