using System;
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
		enum State
		{
			ScanFrequencies,
			ScanChannels,
			ScanOffset
		}
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		ArrayList                           frequencies=new ArrayList();
		int                                 currentFrequencyIndex=0;
		int																	scanOffset = 0;
		private System.Windows.Forms.Timer  timer1;
		State                               currentState;
		int																	currentOffset=0;
		int                                 tunedFrequency=0;
		int																	newChannels, updatedChannels;

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
			newChannels=0;
			updatedChannels=0;
			tunedFrequency=0;
			currentOffset=0;
			captureCard=card;
			callback=statusCallback;

			currentState=State.ScanFrequencies;
			frequencies.Clear();
			currentFrequencyIndex=-1;
			String countryCode = String.Empty;

			Log.WriteFile(Log.LogType.Capture,"Opening dvbt.xml");
			XmlDocument doc= new XmlDocument();
			doc.Load("dvbt.xml");

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
			Log.WriteFile(Log.LogType.Capture,"auto tune for {0}", countryName);
			frequencies.Clear();

			countryList=doc.DocumentElement.SelectNodes("/dvbt/country");
			foreach (XmlNode nodeCountry in countryList)
			{
				string name= nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
				if (name!=countryName) continue;
				Log.WriteFile(Log.LogType.Capture,"found country {0} in dvbt.xml", countryName);
				try
				{
					scanOffset =  XmlConvert.ToInt32(nodeCountry.Attributes.GetNamedItem(@"offset").InnerText);
					Log.WriteFile(Log.LogType.Capture,"scanoffset: {0} ", scanOffset);
				}
				catch(Exception){}

				XmlNodeList frequencyList = nodeCountry.SelectNodes("carrier");
				Log.WriteFile(Log.LogType.Capture,"number of carriers:{0}", frequencyList.Count);
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
					Log.WriteFile(Log.LogType.Capture,"added:{0}", carrier[0]);
				}
			}
			if (frequencies.Count==0) return;

			Log.WriteFile(Log.LogType.Capture,"loaded:{0} frequencies", frequencies.Count);
			Log.WriteFile(Log.LogType.Capture,"{0} has a scan offset of {1}KHz", countryCode, scanOffset);
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
			if (currentFrequencyIndex >= frequencies.Count)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				return;
			}
			
			int currentFreq=currentFrequencyIndex;
			if (currentFrequencyIndex<0) currentFreq=0;
			float percent = ((float)currentFreq) / ((float)frequencies.Count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			int[] tmp = frequencies[currentFreq] as int[];
			//Log.WriteFile(Log.LogType.Capture,"FREQ: {0} BWDTH: {1}", tmp[0], tmp[1]);
			float frequency = tunedFrequency;
			frequency /=1000;
			string description=String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
			callback.OnStatus(description);

			if (currentState==State.ScanFrequencies)
			{
				if (frequency>0)
				{
					if (captureCard.SignalPresent())
					{
						Log.WriteFile(Log.LogType.Capture,"Found signal at:{0} MHz,scan for channels",frequency);
						currentState=State.ScanChannels;
						currentOffset=0;
					}
				}
			}

			if (currentState==State.ScanFrequencies)
			{
				timer1.Enabled=false;
				callback.OnStatus(description);
				ScanNextFrequency();
				timer1.Enabled=true;
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
			Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength,captureCard.SignalQuality);
			timer1.Enabled=false;
			callback.OnStatus2( String.Format("new:{0} updated:{1}", newChannels,updatedChannels) );
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels);
			callback.OnStatus2( String.Format("new:{0} updated:{1}", newChannels,updatedChannels) );
			callback.UpdateList();
			currentState=State.ScanFrequencies;
			currentOffset=0;
			currentFrequencyIndex++;
			ScanNextFrequency();
			timer1.Enabled=true;
		}

		void ScanNextFrequency()
		{
		
			Log.Write("ScanNextFrequency() {0}/{1} {2}",currentFrequencyIndex,frequencies.Count,currentOffset);
			if (currentFrequencyIndex >=frequencies.Count) return;

			DVBChannel chan = new DVBChannel();
			int[] tmp;
			if (currentFrequencyIndex<0)
			{
				currentOffset=0;
				currentFrequencyIndex=0;
				if (currentFrequencyIndex>=frequencies.Count)
				{
					timer1.Enabled=false;
					callback.OnProgress(100);
					callback.OnEnded();
					captureCard.DeleteGraph();
					return;
				}

				tmp = (int[])frequencies[currentFrequencyIndex];
				chan.Frequency=tmp[0];
				chan.Bandwidth=tmp[1];
				Log.WriteFile(Log.LogType.Capture,"tune:{0} bandwidth:{1} (i)",chan.Frequency, chan.Bandwidth);
				captureCard.Tune(chan,0);
				return;
			}

			tmp = (int[])frequencies[currentFrequencyIndex];
			chan.Frequency=tmp[0];
			chan.Bandwidth=tmp[1];
			tunedFrequency=chan.Frequency;

			if (currentOffset==0)
			{
				Log.WriteFile(Log.LogType.Capture,"tune:{0} bandwidth:{1} (1)",chan.Frequency, chan.Bandwidth);
				captureCard.Tune(chan,0);
				if (scanOffset==0) currentOffset=3;
				else currentOffset=1;
			}
			else if (currentOffset==1)
			{
				tunedFrequency-=scanOffset;
				chan.Frequency-=scanOffset;
				Log.WriteFile(Log.LogType.Capture,"tune:{0} bandwidth:{1} (2)",chan.Frequency, chan.Bandwidth);
				captureCard.Tune(chan,0);
				currentOffset=2;
			}
			else if (currentOffset==2)
			{
				tunedFrequency+=(scanOffset);
				chan.Frequency+=(scanOffset);
				Log.WriteFile(Log.LogType.Capture,"tune:{0} bandwidth:{1} (3)",chan.Frequency, chan.Bandwidth);
				captureCard.Tune(chan,0);
				currentOffset=3;
			}
			else
			{
				currentOffset=0;
				currentFrequencyIndex++;
				if (currentFrequencyIndex>=frequencies.Count)
				{
					timer1.Enabled=false;
					callback.OnProgress(100);
					callback.OnEnded();
					captureCard.DeleteGraph();
					return;
				}
			}
//			System.Threading.Thread.Sleep(100);
		}

		#endregion
	}
}
