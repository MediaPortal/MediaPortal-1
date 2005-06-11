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
			ScanStart,
			ScanFrequencies,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		AutoTuneCallback										callback = null;
		int                                 currentIndex=-1;
		private System.Windows.Forms.Timer  timer1;
		DVBCList[]													dvbcChannels=new DVBCList[1000];
		int																	count = 0;
		int																	retryCount=0;

		int newChannels, updatedChannels;
		public DVBCTuning()
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

			OpenFileDialog ofd =new OpenFileDialog();
      ofd.RestoreDirectory = true;
			ofd.Filter = "DVBC-Listings (*.dvbc)|*.dvbc";
			ofd.Title = "Choose DVB-C Listing Files";
			DialogResult res=ofd.ShowDialog();
			if(res!=DialogResult.OK) return;
			
			count = 0;
			string line;
			string[] tpdata;
			Log.WriteFile(Log.LogType.Capture,"Opening {0}",ofd.FileName);
			// load dvbcChannelsList list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(ofd.FileName);
			
			int LineNr=0;
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
				{
					LineNr++;
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
								dvbcChannels[count].frequency = Int32.Parse(tpdata[0]) ;
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
								
								dvbcChannels[count].symbolrate = Int32.Parse(tpdata[2]);
								count += 1;
							}
							catch
							{
								Log.WriteFile(Log.LogType.Capture,"Error in line:{0}", LineNr);
							}
						}
					}
				}
			} while (!(line == null));
			tin.Close();
			

			Log.WriteFile(Log.LogType.Capture,"loaded:{0} dvbcChannels", count);
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
			if (currentIndex >= count)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				return;
			}

			timer1.Enabled=false;
			int index=currentIndex;
			if (index<0) index=0;
			float percent = ((float)index) / ((float)count);
			percent *= 100.0f;
			callback.OnProgress((int)percent);
			
			if (retryCount==0)
			{
				ScanNextDVBCChannel();
				if (captureCard.SignalPresent())
				{
					ScanChannels();
				}
				retryCount=1;
			}
			else 
			{
				ScanDVBCChannel();
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
			DVBCList dvbcChan=dvbcChannels[currentIndex];
			string chanDesc=String.Format("freq:{0} Khz, Mod:{1} SR:{2} retry:{3}",dvbcChan.frequency,dvbcChan.modulation.ToString(), dvbcChan.symbolrate, retryCount);
			string description=String.Format("Found signal for channel:{0} {1}, Scanning channels", currentIndex,chanDesc);
			callback.OnStatus(description);

			timer1.Enabled=false;
			Application.DoEvents();
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels);
			callback.OnStatus2( String.Format("new:{0} updated:{1}", newChannels,updatedChannels) );
			callback.UpdateList();
			timer1.Enabled=true;
			return;
		}

		void ScanNextDVBCChannel()
		{
			currentIndex++;
			ScanDVBCChannel();
			Application.DoEvents();
		}

		void ScanDVBCChannel()
		{
			if (currentIndex<0 || currentIndex>=count)
			{
				timer1.Enabled=false;
				callback.OnProgress(100);
				callback.OnEnded();
				captureCard.DeleteGraph();
				return;
			}
			timer1.Enabled=false;
			string chanDesc=String.Format("freq:{0} Khz, Mod:{1} SR:{2} retry:{3}",
												dvbcChannels[currentIndex].frequency,dvbcChannels[currentIndex].modulation.ToString(), dvbcChannels[currentIndex].symbolrate, retryCount);
			string description=String.Format("Channel:{0}/{1} {2}", currentIndex,count,chanDesc);
			callback.OnStatus(description);

			Log.WriteFile(Log.LogType.Capture,"tune dvbcChannel:{0}/{1} {2}",currentIndex ,count,chanDesc);

			DVBChannel newchan = new DVBChannel();
			newchan.NetworkID=-1;
			newchan.TransportStreamID=-1;
			newchan.ProgramNumber=-1;

			newchan.Modulation=dvbcChannels[currentIndex].modulation;
			newchan.Symbolrate=(dvbcChannels[currentIndex].symbolrate)/1000;
			newchan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			newchan.Frequency=dvbcChannels[currentIndex].frequency;
			captureCard.Tune(newchan,0);
			Application.DoEvents();
			timer1.Enabled=true;
		}
		#endregion
	}
}
