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
	/// Summary description for DVBCTuning.
	/// </summary>
	public class DVBCTuning : ITuning
	{
		struct DVBCList
		{
			public int frequency;		 // frequency
			public int modulation;	 // modulation
			public int symbolrate;	 // symbol rate
		}

		enum State
		{
			ScanFrequencies,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		int                                 currentIndex=0;
		private System.Windows.Forms.Timer  timer1;
		State                               currentState;
		DateTime														channelScanTimeOut;
		DVBCList[]													dvbcChannels=new DVBCList[200];
		int																	count = 0;

		public DVBCTuning()
		{
		}
		#region ITuning Members

		public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
		{
			captureCard=card;
			callback=statusCallback;

			currentState=State.ScanFrequencies;
			currentIndex=0;

			OpenFileDialog ofd =new OpenFileDialog();
			ofd.Filter = "DVBC-Listings (*.dvbc)|*.dvbc";
			ofd.Title = "Choose DVB-C Listing Files";
			DialogResult res=ofd.ShowDialog();
			if(res!=DialogResult.OK) return;
			
			count = 0;
			string line;
			string[] tpdata;
			Log.Write("Opening {0}",ofd.FileName);
			// load dvbcChannelsonder list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(ofd.FileName);
			
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
					if (line.Length > 0)
					{
						if(line.StartsWith(";"))
							continue;
						tpdata = line.Split(new char[]{','});
						if(tpdata.Length!=3)
							tpdata = line.Split(new char[]{';'});
						if (tpdata.Length == 3)
						{
							try
							{
								dvbcChannels[count].frequency = Convert.ToInt16(tpdata[0]) * 1000;
								string mod=tpdata[1].ToUpper();
								dvbcChannels[count].modulation=(int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
								if (mod=="1024QAM") dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_1024QAM;
								if (mod=="112QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_112QAM;
								if (mod=="128QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_128QAM;
								if (mod=="160QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_160QAM;
								if (mod=="16QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_16QAM;
								if (mod=="16VSB")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_16VSB;
								if (mod=="192QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_192QAM;
								if (mod=="224QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_224QAM;
								if (mod=="256QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_256QAM;
								if (mod=="320QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_320QAM;
								if (mod=="384QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_384QAM;
								if (mod=="448QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_448QAM;
								if (mod=="512QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_512QAM;
								if (mod=="640QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_640QAM;
								if (mod=="64QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_64QAM;
								if (mod=="768QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_768QAM;
								if (mod=="80QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_80QAM;
								if (mod=="896QAM")  dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_896QAM;
								if (mod=="8VSB")    dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_8VSB;
								if (mod=="96QAM")   dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_96QAM;
								if (mod=="AMPLITUDE") dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_AMPLITUDE;
								if (mod=="FREQUENCY") dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_ANALOG_FREQUENCY;
								if (mod=="BPSK")    dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_BPSK;
								if (mod=="OQPSK")		dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_OQPSK;
								if (mod=="QPSK")		dvbcChannels[count].modulation = (int)TunerLib.ModulationType.BDA_MOD_QPSK;
								
								dvbcChannels[count].symbolrate = Convert.ToInt16(tpdata[2]);
								count += 1;
							}
							catch
							{}
						}
					}
			} while (!(line == null));
			tin.Close();
			

			Log.Write("loaded:{0} dvbcChannels", count);
			this.timer1 = new System.Windows.Forms.Timer();
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			timer1.Interval=100;
			timer1.Enabled=true;
			callback.OnProgress(0);
			return;
		}

		public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback)
		{
			// TODO:  Add DVBCTuning.AutoTuneRadio implementation
		}

		public void Continue()
		{
			// TODO:  Add DVBCTuning.Continue implementation
		}

		public int MapToChannel(string channel)
		{
			// TODO:  Add DVBCTuning.MapToChannel implementation
			return 0;
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (currentIndex > count)
				return;
			
			float percent = ((float)currentIndex) / ((float)count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			DVBCList dvbcChannelsonder=dvbcChannels[currentIndex];
			string description=String.Format("dvbcChannelsonder:{0}/{1}", currentIndex,count);

			if (currentState==State.ScanFrequencies)
			{
				if (captureCard.SignalPresent())
				{
					Log.Write("Found signal for dvbcChannelsonder:{0}",currentIndex);
					currentState=State.ScanChannels;
					channelScanTimeOut=DateTime.Now;
				}
			}

			if (currentState==State.ScanFrequencies)
			{
				callback.OnStatus(description);
				ScanNextdvbcChannelsonder();
			}

			if (currentState==State.ScanChannels)
			{
				description=String.Format("Found signal for dvbcChannelsonder:{0}, Scanning channels", currentIndex);
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
				captureCard.StoreTunedChannels(false,true);
				callback.UpdateList();
				Log.Write("timeout, goto scanning dvbcChannelsonders");
				currentState=State.ScanFrequencies;
				ScanNextdvbcChannelsonder();
				return;
			}
		}

		void ScanNextdvbcChannelsonder()
		{
			currentIndex++;
			if (currentIndex>=count)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}

			Log.Write("tune dvbcChannel:{0}",currentIndex);
			DVBGraphBDA.DVBCChannel newchan = new DVBGraphBDA.DVBCChannel();
			newchan.ONID=-1;
			newchan.TSID=-1;
			newchan.SID=-1;

			newchan.modulation=dvbcChannels[currentIndex].modulation;
			newchan.symbolRate=dvbcChannels[currentIndex].symbolrate;
			newchan.innerFec=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			newchan.carrierFrequency=dvbcChannels[currentIndex].frequency;
			captureCard.Tune(newchan);
		}

		#endregion
	}
}
