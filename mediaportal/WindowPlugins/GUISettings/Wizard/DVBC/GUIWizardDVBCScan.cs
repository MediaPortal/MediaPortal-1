using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.Wizard.DVBC
{
	/// <summary>
	/// Summary description for GUIWizardDVBCCountry.
	/// </summary>
	public class GUIWizardDVBCScan : GUIWindow
	{
		struct DVBCList
		{
			public int frequency;		 // frequency
			public int modulation;	 // modulation
			public int symbolrate;	 // symbol rate
		}

		[SkinControlAttribute(26)]			protected GUILabelControl lblChannelsFound=null;
		[SkinControlAttribute(27)]			protected GUILabelControl lblStatus=null;
		[SkinControlAttribute(24)]			protected GUIListControl  listChannelsFound=null;
		[SkinControlAttribute(5)]			  protected GUIButtonControl  btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl  btnBack=null;
		[SkinControlAttribute(20)]			protected GUIProgressControl progressBar=null;

		int card=0;
		int scanOffset=0;
		int        currentFrequencyIndex=0;
		bool updateList=false;
		int newChannels=0, updatedChannels=0, newRadioChannels=0, updatedRadioChannels=0;
		DVBCList[]													dvbcChannels=new DVBCList[1000];
		int																	count = 0;

		public GUIWizardDVBCScan()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBC_SCAN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_DVBC_scan.xml");
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			GUIGraphicsContext.VMR9Allowed=true;
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			GUIGraphicsContext.VMR9Allowed=false;
			btnNext.Disabled=true;
			btnBack.Disabled=true;
			progressBar.Percentage=0;
			progressBar.Disabled=false;
			progressBar.IsVisible=true;
			UpdateList();
			Thread WorkerThread = new Thread(new ThreadStart(ScanThread));
			WorkerThread.Start();
		}
		
		void LoadFrequencies()
		{
			count=0;
			currentFrequencyIndex=-1;
			card = Int32.Parse( GUIPropertyManager.GetProperty("#WizardCard"));
			string country=GUIPropertyManager.GetProperty("#WizardCountry");
			Log.Write("dvbc-scan: load {0}", country);
			// load dvbcChannelsList list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(country);
			
			string line=null;
			string[] tpdata;
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
								Log.WriteFile(Log.LogType.Capture,"dvbc-scan:Error in line:{0}", LineNr);
							}
						}
					}
				}
			} while (!(line == null));
			tin.Close();
			Log.Write("dvbc-scan: loaded {0} frequencies", count);
		}

		public void ScanThread()
		{
			newChannels=0;
			updatedChannels=0;
			newRadioChannels=0;
			updatedRadioChannels=0;
			LoadFrequencies();
			TVCaptureDevice captureCard=null;
			if (card >=0 && card < Recorder.Count)
			{
				captureCard =Recorder.Get(card);
				captureCard.CreateGraph();
			}
			else
			{
				btnNext.Disabled=false;
				btnBack.Disabled=false;
				return;
			}
			try
			{
				updateList=false;
				if (captureCard==null) return;
				currentFrequencyIndex=0;
				while (true)
				{
					if (currentFrequencyIndex >= count)
					{
						btnNext.Disabled=false;
						btnBack.Disabled=false;
						return;
					}

					UpdateStatus();
					ScanNextFrequency(captureCard,0);
					if (captureCard.SignalPresent())
					{
						ScanChannels(captureCard);
					}
					currentFrequencyIndex++;
				}
			}
			finally
			{
				captureCard.DeleteGraph();
				captureCard=null;
				progressBar.Percentage=100;
				lblChannelsFound.Label=String.Format("Finished, found {0} tv channels, {1} radio stations",newChannels, newRadioChannels);
				lblStatus.Label="Press Next to continue the setup";
				MapTvToOtherCards(captureCard.ID);
				MapRadioToOtherCards(captureCard.ID);
				GUIControl.FocusControl(GetID,btnNext.GetID);

			}
		}
		void ScanChannels(TVCaptureDevice captureCard)
		{
			Log.Write("DVBC-scan:ScanChannels() {0}/{1}",currentFrequencyIndex,count);
			if (currentFrequencyIndex < 0 || currentFrequencyIndex >=count) return;
			DVBCList dvbcChan=dvbcChannels[currentFrequencyIndex];
			string chanDesc=String.Format("freq:{0} Khz, Mod:{1} SR:{2}",dvbcChan.frequency,dvbcChan.modulation.ToString(), dvbcChan.symbolrate);
			string description=String.Format("Found signal for channel:{0} {1}, Scanning channels", currentFrequencyIndex,chanDesc);
			lblChannelsFound.Label=description;
			System.Threading.Thread.Sleep(400);
			Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength,captureCard.SignalQuality);
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			updateList=true;
			lblStatus.Label=String.Format("Found {0} tv channels, {1} radio stations",newChannels, newRadioChannels);
			Log.Write("DVBC-scan:ScanChannels() done");
		}

		void ScanNextFrequency(TVCaptureDevice captureCard,int offset)
		{
			Log.Write("DVBC-scan:ScanNextFrequency() {0}/{1}",currentFrequencyIndex,count);
			if (currentFrequencyIndex <0) currentFrequencyIndex =0;
			if (currentFrequencyIndex >=count) return;

			DVBChannel chan = new DVBChannel();
			chan.NetworkID=-1;
			chan.TransportStreamID=-1;
			chan.ProgramNumber=-1;

			chan.Modulation=dvbcChannels[currentFrequencyIndex].modulation;
			chan.Symbolrate=(dvbcChannels[currentFrequencyIndex].symbolrate)/1000;
			chan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			chan.Frequency=dvbcChannels[currentFrequencyIndex].frequency;

			Log.WriteFile(Log.LogType.Capture,"DVBC-scan:tune:{0} bandwidth:{1} offset:{2}",chan.Frequency, chan.Bandwidth,offset);

			captureCard.Tune(chan,0);
			System.Threading.Thread.Sleep(400);
			if (captureCard.SignalQuality <40)
				System.Threading.Thread.Sleep(400);
			Log.WriteFile(Log.LogType.Capture,"DVBC-scan:tuned");
			return;
		}

		public override void Process()
		{
			if (updateList)
			{
				UpdateList();
				updateList=false;
			}
	
			base.Process ();
		}

		void UpdateList()
		{
			listChannelsFound.Clear();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			if (channels.Count==0)
			{
				GUIListItem item = new GUIListItem();
				item.Label="No channels found";
				item.IsFolder=false;
				listChannelsFound.Add(item);
				return;

			}
			int count=1;
			foreach (TVChannel chan in channels)
			{
				GUIListItem item = new GUIListItem();
				item.Label=String.Format("{0}. {1}", count,chan.Name);
				item.IsFolder=false;
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				item.ThumbnailImage=strLogo;
				item.IconImage=strLogo;
				item.IconImageBig=strLogo;
				listChannelsFound.Add(item);
				count++;
			}
			listChannelsFound.ScrollToEnd();
		}
		void UpdateStatus()
		{
			int currentFreq=currentFrequencyIndex;
			if (currentFrequencyIndex<0) currentFreq=0;
			float percent = ((float)currentFreq) / ((float)count);
			percent *= 100.0f;
			
			progressBar.Percentage=(int)percent;
			string chanDesc=String.Format("freq:{0} Khz, Mod:{1} SR:{2}",dvbcChannels[currentFrequencyIndex].frequency,dvbcChannels[currentFrequencyIndex].modulation.ToString(), dvbcChannels[currentFrequencyIndex].symbolrate);
			string description=String.Format("Scan channel:{0} {1}", currentFrequencyIndex,chanDesc);

			lblChannelsFound.Label=description;
		}
		void MapTvToOtherCards(int id)
		{
			ArrayList tvchannels = new ArrayList();
			TVDatabase.GetChannelsForCard(ref tvchannels,id);
			for (int i=0; i < Recorder.Count;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);
				if (dev.Network==NetworkType.DVBC && dev.ID != id)
				{
					foreach (TVChannel chan in tvchannels)
					{
						TVDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}
		void MapRadioToOtherCards(int id)
		{
			ArrayList radioChans = new ArrayList();
			MediaPortal.Radio.Database.RadioDatabase.GetStationsForCard(ref radioChans,id);
			for (int i=0; i < Recorder.Count;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);

				if (dev.Network==NetworkType.DVBC && dev.ID != id)
				{
					foreach (MediaPortal.Radio.Database.RadioStation chan in radioChans)
					{
						MediaPortal.Radio.Database.RadioDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}
	}
}
