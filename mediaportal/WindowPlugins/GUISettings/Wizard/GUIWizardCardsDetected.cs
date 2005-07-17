using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

using DShowNET;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using TVCapture;
namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// Summary description for GUIWizardCardsDetected.
	/// </summary>
	public class GUIWizardCardsDetected: GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUITextControl tbCards=null;

		public GUIWizardCardsDetected()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_CARDS_DETECTED;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcards_detected.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			AddAllCards();
		}

		public void AddAllCards()
		{
			ArrayList captureCards = new ArrayList();
			
			ArrayList availableVideoDevices = FilterHelper.GetVideoInputDevices();
			ArrayList availableVideoDeviceMonikers	= FilterHelper.GetVideoInputDeviceMonikers();
			ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();
			string recFolder=Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			recFolder+=@"\My Recordings";
			try
			{
				System.IO.Directory.CreateDirectory(recFolder);
			}
			catch(Exception){}

			string cardsDetected=String.Empty;
			//enum all cards known in capturedefinitions.xml
			foreach (CaptureCardDefinition ccd  in CaptureCardDefinitions.CaptureCards)
			{
				//enum all video capture devices on this system
				for (int i = 0; i < availableVideoDevices.Count; i++)
				{
					//treat the SSE2 DVB-S card as a general H/W card
					if( ((string)(availableVideoDevices[i])) == "B2C2 MPEG-2 Source")
					{
						TVCaptureDevice cd		= new TVCaptureDevice();
						cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
						cd.VideoDevice				= (string)availableVideoDevices[i];
						cd.CommercialName			= "General H/W encoding card";
						cd.IsBDACard					= false;
						cd.IsMCECard					= false;
						cd.SupportsMPEG2			= true;
						cd.DeviceId						= (string)availableVideoDevices[i];
						cd.FriendlyName			  = String.Format("card{0}",captureCards.Count+1);
						cd.DeviceType					= "hw";
						cd.RecordingPath			= recFolder;
						cd.UseForRecording=true;
						cd.UseForTV=true;
						cd.Priority=10;
						captureCards.Add(cd);
						if ( cardsDetected!=String.Empty) cardsDetected+= "\n";
						cardsDetected+="SkyStar 2 DVB-S";


						string filename=String.Format(@"database\card_{0}.xml",cd.FriendlyName);
						// save settings for get the filename in mp.xml
						using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
						{
							xmlwriter.SetValue("dvb_ts_cards","filename",filename);
						}
						availableVideoDeviceMonikers.RemoveAt(i);
						availableVideoDevices.RemoveAt(i);
						continue;
					}
					if (ccd.CaptureName==String.Empty) continue;
					if (((string)(availableVideoDevices[i]) == ccd.CaptureName) &&
						((availableVideoDeviceMonikers[i]).ToString().IndexOf(ccd.DeviceId) > -1 )) 
					{
						TVCaptureDevice cd		= new TVCaptureDevice();
						cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
						cd.VideoDevice				= ccd.CaptureName;
						cd.CommercialName			= ccd.CommercialName;
						cd.LoadDefinitions();
						cd.IsBDACard					= ccd.Capabilities.IsBDADevice;
						cd.IsMCECard					= ccd.Capabilities.IsMceDevice;
						cd.SupportsMPEG2			= ccd.Capabilities.IsMpeg2Device;
						cd.DeviceId						= ccd.DeviceId;
						cd.FriendlyName			  = String.Format("card{0}",captureCards.Count+1);
						cd.DeviceType					= ccd.DeviceId;
						cd.RecordingPath			= recFolder;
						if (cd.IsBDACard) cd.Priority=10;
						else cd.Priority=1;
						cd.UseForRecording=true;
						cd.UseForTV=true;

						if ( cardsDetected!=String.Empty) cardsDetected+= "\n";
						cardsDetected+=cd.CommercialName;
						captureCards.Add(cd);
						availableVideoDeviceMonikers.RemoveAt(i);
						availableVideoDevices.RemoveAt(i);
					}
				}
			}
			SaveCaptureCards(captureCards);
			if (cardsDetected==String.Empty)
				cardsDetected="No TV cards detected";
			tbCards.Label=cardsDetected;
		}

		void SaveCaptureCards(ArrayList availableCards)
		{
			using(FileStream fileStream = new FileStream("capturecards.xml", FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				SoapFormatter formatter = new SoapFormatter();
				formatter.Serialize(fileStream, availableCards);
				fileStream.Close();
			}
		}

	}
}
