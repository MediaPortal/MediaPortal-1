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

		public DVBTTuning()
		{
		}
		#region ITuning Members

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			tunedFrequency=0;
			currentOffset=0;
			captureCard=card;
			callback=statusCallback;

			currentState=State.ScanFrequencies;
			frequencies.Clear();
			currentFrequencyIndex=-1;
			String countryCode = String.Empty;

			Log.Write("Opening dvbt.xml");
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
			Log.Write("auto tune for {0}", countryName);
			frequencies.Clear();

			countryList=doc.DocumentElement.SelectNodes("/dvbt/country");
			foreach (XmlNode nodeCountry in countryList)
			{
				string name= nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
				if (name!=countryName) continue;
				Log.Write("found country {0} in dvbt.xml", countryName);
				try
				{
					scanOffset =  XmlConvert.ToInt32(nodeCountry.Attributes.GetNamedItem(@"offset").InnerText);
					Log.Write("scanoffset: {0} ", scanOffset);
				}
				catch(Exception){}

				XmlNodeList frequencyList = nodeCountry.SelectNodes("carrier");
				Log.Write("number of carriers:{0}", frequencyList.Count);
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

					frequencies.Add(carrier);
					Log.Write("added:{0}", carrier[0]);
				}
			}
			if (frequencies.Count==0) return;

			Log.Write("loaded:{0} frequencies", frequencies.Count);
			Log.Write("{0} has a scan offset of {1}KHz", countryCode, scanOffset);
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
			//Log.Write("FREQ: {0} BWDTH: {1}", tmp[0], tmp[1]);
			float frequency = tunedFrequency;
			frequency /=1000;
			string description=String.Format("frequency:{0:###.##} MHz.", frequency);
			callback.OnStatus(description);

			if (currentState==State.ScanFrequencies)
			{
				if (frequency>0)
				{
					if (captureCard.SignalPresent())
					{
						Log.Write("Found signal at:{0} MHz,scan for channels",frequency);
						currentState=State.ScanChannels;
						currentOffset=0;
					}
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
			timer1.Enabled=false;
			captureCard.StoreTunedChannels(false,true);
			callback.UpdateList();
			currentState=State.ScanFrequencies;
			currentOffset=0;
			currentFrequencyIndex++;
			ScanNextFrequency();
			timer1.Enabled=true;
		}

		void ScanNextFrequency()
		{
			if (currentFrequencyIndex >=frequencies.Count) return;

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
				Log.Write("tune:{0}",tunedFrequency);
				captureCard.Tune(tunedFrequency);
				return;
			}

			tmp = (int[])frequencies[currentFrequencyIndex];
			tunedFrequency=tmp[0];
			if (currentOffset==0)
			{
				Log.Write("tune:{0}",tunedFrequency);
				captureCard.Tune(tunedFrequency);
				if (scanOffset==0) currentOffset=3;
				else currentOffset++;
			}
			else if (currentOffset==1)
			{
				tunedFrequency-=scanOffset;
				Log.Write("tune:{0}",tunedFrequency);
				captureCard.Tune(tunedFrequency);
				currentOffset++;
			}
			else if (currentOffset==2)
			{
				tunedFrequency+=scanOffset;
				Log.Write("tune:{0}",tunedFrequency);
				captureCard.Tune(tunedFrequency);
				currentOffset++;
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
