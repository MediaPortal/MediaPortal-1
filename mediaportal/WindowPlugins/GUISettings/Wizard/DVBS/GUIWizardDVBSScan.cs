using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
	/// <summary>
	/// Summary description for GUIWizardDVBSCountry.
	/// </summary>
	public class GUIWizardDVBSScan : GUIWindow
	{

		struct TPList
		{
			public int TPfreq; // frequency
			public int TPpol;  // polarisation 0=hori, 1=vert
			public int TPsymb; // symbol rate
		}
		[SkinControlAttribute(26)]			protected GUILabelControl lblChannelsFound=null;
		[SkinControlAttribute(27)]			protected GUILabelControl lblStatus=null;
		[SkinControlAttribute(24)]			protected GUIListControl  listChannelsFound=null;
		[SkinControlAttribute(5)]			  protected GUIButtonControl  btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl  btnBack=null;
		[SkinControlAttribute(20)]			protected GUIProgressControl progressBar=null;

		int				 card=0;
		int        currentFrequencyIndex=0;
		bool			 updateList=false;
		int				 newChannels=0, updatedChannels=0, newRadioChannels=0, updatedRadioChannels=0;
		TPList[]	 DVBSChannels=new TPList[1000];
		int				 count = 0;
		int        m_diseqcLoops;
		int        m_currentDiseqc;

		public GUIWizardDVBSScan()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SCAN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_DVBS_scan.xml");
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

			m_currentDiseqc=1;
			m_diseqcLoops=1;
			card = Int32.Parse( GUIPropertyManager.GetProperty("#WizardCard"));
			TVCaptureDevice captureCard = Recorder.Get(card);
			if (captureCard!=null) 
			{
				string filename=String.Format(@"database\card_{0}.xml",captureCard.FriendlyName);
				using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
				{
					if(xmlreader.GetValueAsBool("dvbs","useLNB2",false)==true)
						m_diseqcLoops++;
					if(xmlreader.GetValueAsBool("dvbs","useLNB3",false)==true)
						m_diseqcLoops++;
					if(xmlreader.GetValueAsBool("dvbs","useLNB4",false)==true)
						m_diseqcLoops++;
				}
			}
			Thread WorkerThread = new Thread(new ThreadStart(ScanThread));
			WorkerThread.Start();
		}
		
		void LoadFrequencies()
		{
			count=0;
			currentFrequencyIndex=-1;
			card = Int32.Parse( GUIPropertyManager.GetProperty("#WizardCard"));
			TVCaptureDevice captureCard = Recorder.Get(card);
			string tplFile=String.Empty;
			if (captureCard!=null) 
			{
				string filename=String.Format(@"database\card_{0}.xml",captureCard.FriendlyName);
				using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
				{
					string key=String.Format("sat{0}", m_currentDiseqc);
					tplFile=xmlreader.GetValue("dvbs",key);

				}
			}

			count = 0;
			string line;
			string[] tpdata;
			Log.WriteFile(Log.LogType.Capture,"dvbs-scan:Opening {0}",tplFile);
			// load transponder list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(tplFile);
			
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
			
								DVBSChannels[count].TPfreq = Int32.Parse(tpdata[0]) *1000;
								switch (tpdata[1].ToLower())
								{
									case "v":
						
										DVBSChannels[count].TPpol = 1;
										break;
									case "h":
						
										DVBSChannels[count].TPpol = 0;
										break;
									default:
						
										DVBSChannels[count].TPpol = 0;
										break;
								}
								DVBSChannels[count].TPsymb = Int32.Parse(tpdata[2]);
								count += 1;
							}
							catch
							{}
						}
					}
			} while (!(line == null));
			tin.Close();
			

			Log.WriteFile(Log.LogType.Capture,"dvbs-scan:loaded:{0} transponders", count);
			return;
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
				m_currentDiseqc=1;
				while (true)
				{
					if (currentFrequencyIndex >= count)
					{
						if(m_currentDiseqc>=m_diseqcLoops)
						{
							btnNext.Disabled=false;
							btnBack.Disabled=false;
							return;
						}
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
				GUIPropertyManager.SetProperty("#Wizard.DVBS.Done","yes");

			}
		}
		void ScanChannels(TVCaptureDevice captureCard)
		{
			Log.Write("DVBS-scan:ScanChannels() {0}/{1}",currentFrequencyIndex,count);
			if (currentFrequencyIndex < 0 || currentFrequencyIndex >=count) return;
			TPList DVBSChan=DVBSChannels[currentFrequencyIndex];
			string description=String.Format("Found signal for transponder:{0}, Scanning channels", currentFrequencyIndex);
			lblChannelsFound.Label=description;
			System.Threading.Thread.Sleep(400);
			Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength,captureCard.SignalQuality);
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			updateList=true;
			lblStatus.Label=String.Format("Found {0} tv channels, {1} radio stations",newChannels, newRadioChannels);
			Log.Write("DVBS-scan:ScanChannels() done");
		}

		void ScanNextFrequency(TVCaptureDevice captureCard,int offset)
		{
			Log.Write("DVBS-scan:ScanNextFrequency() {0}/{1}",currentFrequencyIndex,count);
			if (currentFrequencyIndex <0) currentFrequencyIndex =0;
			if (currentFrequencyIndex >=count) return;

			DVBChannel chan = new DVBChannel();
			chan.NetworkID=-1;
			chan.TransportStreamID=-1;
			chan.ProgramNumber=-1;

			chan.Polarity=DVBSChannels[currentFrequencyIndex].TPpol;
			chan.Symbolrate=DVBSChannels[currentFrequencyIndex].TPsymb;
			chan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_DEFINED;
			chan.Frequency=DVBSChannels[currentFrequencyIndex].TPfreq;
			Log.WriteFile(Log.LogType.Capture,"DVBS-scan:tune:{0} bandwidth:{1} offset:{2}",chan.Frequency, chan.Bandwidth,offset);

			captureCard.Tune(chan,0);
			System.Threading.Thread.Sleep(400);
			if (captureCard.SignalQuality <40)
				System.Threading.Thread.Sleep(400);
			Log.WriteFile(Log.LogType.Capture,"DVBS-scan:tuned");
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
			TPList transponder = DVBSChannels[currentFreq];
			string chanDesc=String.Format("freq:{0} Khz, Pol:{1} SR:{2}",transponder.TPfreq, transponder.TPpol, transponder.TPsymb );
			string description=String.Format("Transponder:{0}/{1} {2}", currentFreq,count,chanDesc);

			lblChannelsFound.Label=description;
		}
		void MapTvToOtherCards(int id)
		{
			ArrayList tvchannels = new ArrayList();
			TVDatabase.GetChannelsForCard(ref tvchannels,id);
			for (int i=0; i < Recorder.Count;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);
				if (dev.Network==NetworkType.DVBS && dev.ID != id)
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

				if (dev.Network==NetworkType.DVBS && dev.ID != id)
				{
					foreach (MediaPortal.Radio.Database.RadioStation chan in radioChans)
					{
						MediaPortal.Radio.Database.RadioDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnNext)
			{
				GUIWizardCardsDetected.ScanNextCardType();
				return;
			}
			base.OnClicked (controlId, control, actionType);
		}

	}
}
