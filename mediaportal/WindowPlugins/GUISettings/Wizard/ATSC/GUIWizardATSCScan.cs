using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;
namespace WindowPlugins.GUISettings.Wizard.ATSC
{
	/// <summary>
	/// Summary description for GUIWizardATSCCountry.
	/// </summary>
	public class GUIWizardATSCScan : GUIWindow
	{
		[SkinControlAttribute(26)]			protected GUILabelControl lblChannelsFound=null;
		[SkinControlAttribute(27)]			protected GUILabelControl lblStatus=null;
		[SkinControlAttribute(24)]			protected GUIListControl  listChannelsFound=null;
		[SkinControlAttribute(5)]			  protected GUIButtonControl  btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl  btnBack=null;
		[SkinControlAttribute(20)]			protected GUIProgressControl progressBar=null;

		int card=0;
		
		int        currentFrequencyIndex=0;
		bool updateList=false;
		int newChannels=0, updatedChannels=0, newRadioChannels=0, updatedRadioChannels=0;

		public GUIWizardATSCScan()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_ATSC_SCAN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_ATSC_scan.xml");
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
		public void ScanThread()
		{
			newChannels=0;
			updatedChannels=0;
			newRadioChannels=0;
			updatedRadioChannels=0;
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
					if (currentFrequencyIndex >= 200)
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
				progressBar.Percentage=100;
				lblChannelsFound.Label=String.Format("Finished, found {0} tv channels, {1} radio stations",newChannels, newRadioChannels);
				lblStatus.Label="Press Next to continue the setup";
				MapTvToOtherCards(captureCard.ID);
				MapRadioToOtherCards(captureCard.ID);
				GUIControl.FocusControl(GetID,btnNext.GetID);
				GUIPropertyManager.SetProperty("#Wizard.ATSC.Done","yes");
				captureCard=null;

			}
		}
		void ScanChannels(TVCaptureDevice captureCard)
		{
			Log.Write("ATSC-scan:ScanChannels() {0}/{1}",currentFrequencyIndex,200);
			if (currentFrequencyIndex < 0 || currentFrequencyIndex >=200) return;

			string description=String.Format("Found signal at channel:{0} MHz. Scanning channels", currentFrequencyIndex);
			lblChannelsFound.Label=description;
			System.Threading.Thread.Sleep(400);
			Log.Write("ScanChannels() {0} {1}", captureCard.SignalStrength,captureCard.SignalQuality);
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			updateList=true;
			lblStatus.Label=String.Format("Found {0} tv channels, {1} radio stations",newChannels, newRadioChannels);
			Log.Write("ATSC-scan:ScanChannels() done");
		}

		void ScanNextFrequency(TVCaptureDevice captureCard,int offset)
		{
			Log.Write("ATSC-scan:ScanNextFrequency() {0}/{1}",currentFrequencyIndex,200);
			if (currentFrequencyIndex <0) currentFrequencyIndex =0;
			if (currentFrequencyIndex >=200) return;

			DVBChannel chan = new DVBChannel();
			chan.NetworkID=-1;
			chan.TransportStreamID=-1;
			chan.ProgramNumber=-1;
			chan.MinorChannel=-1;
			chan.MajorChannel=-1;
			chan.Frequency=-1;
			chan.PhysicalChannel=currentFrequencyIndex;
			chan.Frequency=-1;
			chan.Symbolrate=-1;
			chan.Modulation=(int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
			chan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;

			string description=String.Format("Channel:{0}", currentFrequencyIndex);

			Log.WriteFile(Log.LogType.Capture,"ATSC-scan:tune:{0}",currentFrequencyIndex);
			captureCard.Tune(chan,0);
			System.Threading.Thread.Sleep(400);
			if (captureCard.SignalQuality <40)
				System.Threading.Thread.Sleep(400);
			Log.WriteFile(Log.LogType.Capture,"ATSC-scan:tuned");
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
			float percent = ((float)currentFreq) / ((float)200);
			percent *= 100.0f;
			
			progressBar.Percentage=(int)percent;
			string description=String.Format("Channel:{0}", currentFreq);
			lblChannelsFound.Label=description;
		}
		void MapTvToOtherCards(int id)
		{
			ArrayList tvchannels = new ArrayList();
			TVDatabase.GetChannelsForCard(ref tvchannels,id);
			for (int i=0; i < Recorder.Count;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);
				if (dev.Network==NetworkType.ATSC && dev.ID != id)
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

				if (dev.Network==NetworkType.ATSC && dev.ID != id)
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
