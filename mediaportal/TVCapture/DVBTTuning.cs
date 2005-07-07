using System;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.GUI.Library;
using System.Xml;

namespace MediaPortal.TV.Recording
{

	/// <summary>
	/// Summary description for DVBTTuning.
	/// </summary>
	public class DVBTTuning : ITuning
	{
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		ArrayList                           frequencies=new ArrayList();
		int                                 currentFrequencyIndex=0;
		int																	scanOffset = 0;
		private System.Windows.Forms.Timer  timer1;
		int                                 tunedFrequency=0;
		int																	newChannels, updatedChannels;
		int																	newRadioChannels, updatedRadioChannels;

		public DVBTTuning()
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
			newRadioChannels=0;
			updatedRadioChannels=0;
			newChannels=0;
			updatedChannels=0;
			tunedFrequency=0;
			captureCard=card;
			callback=statusCallback;

			frequencies.Clear();
			currentFrequencyIndex=-1;
			String countryCode = String.Empty;

			Log.WriteFile(Log.LogType.Capture,"dvbt-scan:Opening dvbt.xml");
			XmlDocument doc= new XmlDocument();
			doc.Load("Tuningparameters/dvbt.xml");

			FormCountry formCountry = new FormCountry();
			XmlNodeList countryList=doc.DocumentElement.SelectNodes("/dvbt/country");
			foreach (XmlNode nodeCountry in countryList)
			{
				string name= nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
				formCountry.AddCountry(name);
			}
			formCountry.ShowDialog();
			string countryName=formCountry.countryName;
			if (countryName==String.Empty) return;
			Log.WriteFile(Log.LogType.Capture,"dvbt-scan:auto tune for {0}", countryName);
			frequencies.Clear();

			countryList=doc.DocumentElement.SelectNodes("/dvbt/country");
			foreach (XmlNode nodeCountry in countryList)
			{
				string name= nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
				if (name!=countryName) continue;
				Log.WriteFile(Log.LogType.Capture,"dvbt-scan:found country {0} in dvbt.xml", countryName);
				try
				{
					scanOffset =  XmlConvert.ToInt32(nodeCountry.Attributes.GetNamedItem(@"offset").InnerText);
					Log.WriteFile(Log.LogType.Capture,"dvbt-scan:scanoffset: {0} ", scanOffset);
				}
				catch(Exception){}

				XmlNodeList frequencyList = nodeCountry.SelectNodes("carrier");
				Log.WriteFile(Log.LogType.Capture,"dvbt-scan:number of carriers:{0}", frequencyList.Count);
				int[] carrier;
				foreach (XmlNode node in frequencyList)
				{
					carrier = new int[2];
					carrier[0] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"frequency").InnerText);
					try
					{
						carrier[1] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"bandwidth").InnerText);
					}
					catch(Exception){}

					if (carrier[1]==0) carrier[1]=8;
					frequencies.Add(carrier);
					Log.WriteFile(Log.LogType.Capture,"dvbt-scan:added:{0}", carrier[0]);
				}
			}
			if (frequencies.Count==0) return;

			Log.WriteFile(Log.LogType.Capture,"dvbt-scan:loaded:{0} frequencies", frequencies.Count);
			Log.WriteFile(Log.LogType.Capture,"dvbt-scan:{0} has a scan offset of {1}KHz", countryCode, scanOffset);
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			return;
		}

		public void Start()
		{
			currentFrequencyIndex=-1;
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
		}

		public void Next()
		{
			if (currentFrequencyIndex+1>=frequencies.Count) return;
			currentFrequencyIndex++;
			UpdateStatus();

			ScanNextFrequency();
			if (captureCard.SignalPresent())
			{
				ScanChannels();
			}
		}
		public void Previous()
		{
			if (currentFrequencyIndex>=1) 
			{
				currentFrequencyIndex--;

				UpdateStatus();
				ScanNextFrequency();
				if (captureCard.SignalPresent())
				{
					ScanChannels();
				}
			}
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

		void UpdateStatus()
		{
			int currentFreq=currentFrequencyIndex;
			if (currentFrequencyIndex<0) currentFreq=0;
			float percent = ((float)currentFreq) / ((float)frequencies.Count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			int[] tmp = frequencies[currentFreq] as int[];
			//Log.WriteFile(Log.LogType.Capture,"dvbt-scan:FREQ: {0} BWDTH: {1}", tmp[0], tmp[1]);
			float frequency = tmp[0];
			frequency /=1000;
			string description=String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
			callback.OnStatus(description);
		}
		private void timer1_Tick(object sender, System.EventArgs e)
		{
			timer1.Enabled=false;
			if (currentFrequencyIndex >= frequencies.Count)
			{
				callback.OnProgress(100);
				callback.OnEnded();
				callback.OnStatus("Finished");
				return;
			}

			UpdateStatus();
			ScanNextFrequency();
			if (captureCard.SignalPresent())
			{
				ScanChannels();
			}
			currentFrequencyIndex++;
			timer1.Enabled=true;
		}

		void ScanChannels()
		{
			if (currentFrequencyIndex < 0 || currentFrequencyIndex >=frequencies.Count) return;
			int[] tmp;
			tmp = (int[])frequencies[currentFrequencyIndex];
			string description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", tmp[0]);
			callback.OnStatus(description);
			for (int i=0; i < 8; ++i)
			{
				System.Threading.Thread.Sleep(100);
				Application.DoEvents();
			}
			Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength,captureCard.SignalQuality);
			callback.OnStatus2( String.Format("new tv:{0} new radio:{1}", newChannels,newRadioChannels) );
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			callback.OnStatus2( String.Format("new tv:{0} new radio:{1}", newChannels,newRadioChannels) );
			callback.UpdateList();
		}

		void ScanNextFrequency()
		{
			if (currentFrequencyIndex <0) currentFrequencyIndex =0;
			if (currentFrequencyIndex >=frequencies.Count) return;
			Log.Write("ScanNextFrequency() {0}/{1}",currentFrequencyIndex,frequencies.Count);

			DVBChannel chan = new DVBChannel();
			int[] tmp;
			tmp = (int[])frequencies[currentFrequencyIndex];
			chan.Frequency=tmp[0];
			chan.Bandwidth=tmp[1];
			Log.WriteFile(Log.LogType.Capture,"dvbt-scan:tune:{0} bandwidth:{1} (i)",chan.Frequency, chan.Bandwidth);
			captureCard.Tune(chan,0);
			for (int i=0; i < 8; ++i)
			{
				System.Threading.Thread.Sleep(100);
				Application.DoEvents();
			}
			return;
		}

		#endregion
	}
}
