using System;
using System.Collections;
using System.Xml;
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
		[SkinControlAttribute(24)]			protected GUIListControl  listChannelsFound=null;
		[SkinControlAttribute(5)]			  protected GUIButtonControl  btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl  btnBack=null;

		int card=0;
		int scanOffset=0;
		ArrayList  frequencies=null;
		int        currentFrequencyIndex=0;
		TVCaptureDevice captureCard=null;

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
			if (captureCard!=null)
			{
				captureCard.DeleteGraph();
				captureCard=null;
			}
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadFrequencies();
			if (card >=0 && card < Recorder.Count)
			{
				captureCard =Recorder.Get(card);
				captureCard.CreateGraph();
			}
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

		public override void Process()
		{
			if (captureCard==null) return;
			if (currentFrequencyIndex >= frequencies.Count)
				return;

			ScanNextFrequency(0);
			if (captureCard.SignalPresent())
			{
				ScanChannels();
			}

			if (scanOffset!=0)
			{
				ScanNextFrequency(-scanOffset);
				if (captureCard.SignalPresent())
				{
					ScanChannels();
				}
				ScanNextFrequency(scanOffset);
				if (captureCard.SignalPresent())
				{
					ScanChannels();
				}
			}
			currentFrequencyIndex++;
		}

		void ScanChannels()
		{
			int newChannels=0, updatedChannels=0, newRadioChannels=0, updatedRadioChannels=0;
			Log.Write("dvbt-scan:ScanChannels() {0}/{1}",currentFrequencyIndex,frequencies.Count);
			if (currentFrequencyIndex < 0 || currentFrequencyIndex >=frequencies.Count) return;
			int[] tmp;
			tmp = (int[])frequencies[currentFrequencyIndex];
			string description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", tmp[0]/1000);
			System.Threading.Thread.Sleep(400);
			Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength,captureCard.SignalQuality);
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			UpdateList();
			Log.Write("dvbt-scan:ScanChannels() done");
		}

		void ScanNextFrequency(int offset)
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

		void UpdateList()
		{
			listChannelsFound.Clear();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			foreach (TVChannel chan in channels)
			{
				GUIListItem item = new GUIListItem();
				item.Label=chan.Name;
				item.IsFolder=false;
				listChannelsFound.Add(item);
			}
		}
	}
}
