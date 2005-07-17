using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;

namespace WindowPlugins.GUISettings.Wizard.DVBT
{
	/// <summary>
	/// Summary description for GUIWizardDVBTCountry.
	/// </summary>
	public class GUIWizardDVBTScan : GUIWindow
	{
		[SkinControlAttribute(26)]			protected GUILabelControl lblChannelsFound=null;
		[SkinControlAttribute(27)]			protected GUILabelControl lblStatus=null;
		[SkinControlAttribute(24)]			protected GUIListControl  listChannelsFound=null;
		[SkinControlAttribute(5)]			  protected GUIButtonControl  btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl  btnBack=null;
		[SkinControlAttribute(20)]			protected GUIProgressControl progressBar=null;

		int card=0;
		int scanOffset=0;
		ArrayList  frequencies=null;
		int        currentFrequencyIndex=0;
		bool updateList=false;
		int newChannels=0, updatedChannels=0, newRadioChannels=0, updatedRadioChannels=0;

		public GUIWizardDVBTScan()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBT_SCAN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbt_scan.xml");
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			frequencies=null;
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
			currentFrequencyIndex=-1;
			frequencies=new ArrayList();
			card = Int32.Parse( GUIPropertyManager.GetProperty("#WizardCard"));
			string country=GUIPropertyManager.GetProperty("#WizardCountry");
			XmlDocument doc= new XmlDocument();
			doc.Load("Tuningparameters/dvbt.xml");
			XmlNodeList countryList=doc.DocumentElement.SelectNodes("/dvbt/country");
			foreach (XmlNode nodeCountry in countryList)
			{
				string name= nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
				if (name==country)
				{
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
					break;
				}
			}
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
					if (currentFrequencyIndex >= frequencies.Count)
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

					if (scanOffset!=0)
					{
						ScanNextFrequency(captureCard,-scanOffset);
						if (captureCard.SignalPresent())
						{
							ScanChannels(captureCard);
						}
						ScanNextFrequency(captureCard,scanOffset);
						if (captureCard.SignalPresent())
						{
							ScanChannels(captureCard);
						}
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
			}
		}
		void ScanChannels(TVCaptureDevice captureCard)
		{
			Log.Write("dvbt-scan:ScanChannels() {0}/{1}",currentFrequencyIndex,frequencies.Count);
			if (currentFrequencyIndex < 0 || currentFrequencyIndex >=frequencies.Count) return;
			int[] tmp;
			tmp = (int[])frequencies[currentFrequencyIndex];
			string description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", tmp[0]/1000);
			lblChannelsFound.Label=description;
			System.Threading.Thread.Sleep(400);
			Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength,captureCard.SignalQuality);
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			updateList=true;
			lblStatus.Label=String.Format("Found {0} tv channels, {1} radio stations",newChannels, newRadioChannels);
			Log.Write("dvbt-scan:ScanChannels() done");
		}

		void ScanNextFrequency(TVCaptureDevice captureCard,int offset)
		{
			Log.Write("dvbt-scan:ScanNextFrequency() {0}/{1}",currentFrequencyIndex,frequencies.Count);
			if (currentFrequencyIndex <0) currentFrequencyIndex =0;
			if (currentFrequencyIndex >=frequencies.Count) return;

			DVBChannel chan = new DVBChannel();
			int[] tmp;
			tmp = (int[])frequencies[currentFrequencyIndex];
			chan.Frequency=tmp[0];
			chan.Bandwidth=tmp[1];
			chan.Frequency+=offset;

			float frequency =((float)chan.Frequency) / 1000f;
			string description=String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);

			Log.WriteFile(Log.LogType.Capture,"dvbt-scan:tune:{0} bandwidth:{1} offset:{2}",chan.Frequency, chan.Bandwidth,offset);
			captureCard.Tune(chan,0);
			System.Threading.Thread.Sleep(400);
			if (captureCard.SignalQuality <40)
				System.Threading.Thread.Sleep(400);
			Log.WriteFile(Log.LogType.Capture,"dvbt-scan:tuned");
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
			foreach (TVChannel chan in channels)
			{
				GUIListItem item = new GUIListItem();
				item.Label=chan.Name;
				item.IsFolder=false;
				listChannelsFound.Add(item);
			}
			listChannelsFound.ScrollToEnd();
		}
		void UpdateStatus()
		{
			int currentFreq=currentFrequencyIndex;
			if (currentFrequencyIndex<0) currentFreq=0;
			float percent = ((float)currentFreq) / ((float)frequencies.Count);
			percent *= 100.0f;
			
			progressBar.Percentage=(int)percent;
			int[] tmp = frequencies[currentFreq] as int[];
			float frequency = tmp[0];
			frequency /=1000;
			string description=String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
			lblChannelsFound.Label=description;
		}
	}
}
