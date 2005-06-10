using System;
using System.Drawing;
using WMEncoderLib;
using MediaPortal.GUI.Library;
namespace MediaPortal.Core.Transcoding
{
	/// <summary>
	/// This class encodes an video file to .wmv format
	/// </summary>
	public class TranscodeToWMV : ITranscode	
	{
		WMEncoder Encoder ;
		bool      finishedEncoding=false;
		WMEncProfile2  profile = null;

		public TranscodeToWMV()
		{
		}
		public void CreateProfile(string name, int KBPS, Size videoSize, int bitRate, int FPS)
		{
			profile =new WMEncProfile2 ();
			profile.ValidateMode=true;
			profile.ProfileName=name;
			profile.ContentType=17;//audio & video
			profile.set_VBRMode(WMENC_SOURCE_TYPE.WMENC_VIDEO, 0, WMENC_PROFILE_VBR_MODE.WMENC_PVM_BITRATE_BASED);
			profile.AddAudience(KBPS);
			// Create an audience object then loop through all of the audiences
			// in the current profile, making the same changes to each audience.
			IWMEncAudienceObj Audnc;
			for (int i = 0; i < profile.AudienceCount; i++)
			{
				Audnc = profile.get_Audience(i);
				// The Windows Media 9 codec is used by default, but you can change
				// it as follows. Be sure to make this change for each audience.
				//Audnc.set_VideoCodec(0, 2);

				// Make the video output size match the input size by setting
				// the height and width to 0.
				Audnc.set_VideoHeight(0, videoSize.Height);
				Audnc.set_VideoWidth(0, videoSize.Width);
				Audnc.set_VideoFPS(0,FPS);
				
				Audnc.SetAudioConfig(0,2,44100,192000,16);
				
				int videoBitRate=KBPS;
				videoBitRate-=9*1024;//overhead
				videoBitRate-= (192000);//audio
				Audnc.set_VideoBitrate(0,videoBitRate);
			

				// Change the buffer size to 5 seconds. By default, the end user's
				// default setting is used.
				Audnc.set_VideoBufferSize(0, 5000);
				Audnc.set_VideoImageSharpness(0, 85);

			}

			profile.Validate();



		}
		
		public bool Supports(VideoFormat format)
		{
			if (format==VideoFormat.Wmv) return true;
			return false;
		}

		public bool Transcode(TranscodeInfo info, VideoFormat format,Quality quality)
		{
			if (!Supports(format)) return false;
			try 
			{
				// Create a WMEncoder object.
				Encoder = new WMEncoder();
				Encoder.OnStateChange +=new _IWMEncoderEvents_OnStateChangeEventHandler(OnStateChange);

				// Retrieve the source group collection.
				IWMEncSourceGroupCollection SrcGrpColl = Encoder.SourceGroupCollection;

				// Add a source group to the collection.
				IWMEncSourceGroup SrcGrp  = SrcGrpColl.Add("SG_1");

				// Add a video and audio source to the source group.
				IWMEncSource SrcAud = SrcGrp.AddSource(WMENC_SOURCE_TYPE.WMENC_AUDIO);
				SrcAud.SetInput(info.file, "", "");

				IWMEncVideoSource2 SrcVid = (IWMEncVideoSource2)SrcGrp.AddSource(WMENC_SOURCE_TYPE.WMENC_VIDEO);
				SrcVid.SetInput(info.file, "", "");

				// Crop 2 pixels from each edge of the video image.
				SrcVid.CroppingBottomMargin = 2;
				SrcVid.CroppingTopMargin = 2;
				SrcVid.CroppingLeftMargin = 2;
				SrcVid.CroppingRightMargin = 2;

				string outputFile=System.IO.Path.ChangeExtension(info.file,".wmv");
				// Specify a file object in which to save encoded content.
				IWMEncFile File = Encoder.File;
				File.LocalFileName = outputFile;

				// Choose a profile from the collection.
				if (profile==null)
				{
					IWMEncProfileCollection ProColl = Encoder.ProfileCollection;
					IWMEncProfile Pro;
					string name="Windows Media Video 8 for Local Area Network (384 Kbps)";
					if (quality==Quality.Medium)
						name="Windows Media Video 8 for Local Area Network (100 Kbps)";
					if (quality==Quality.Low)
						name="Windows Media Video 8 for Dial-up Modems (56 Kbps)";
					for (int i = 0; i < ProColl.Count; i++)
					{
						Pro = ProColl.Item(i);
						//Trace.WriteLine(Pro.Name+" - "+Pro.Description);
						if (Pro.Name == name)
						{
							SrcGrp.set_Profile(Pro);
							break;
						}
					}
				}
				else
				{
					SrcGrp.set_Profile(profile);
				}

				// Fill in the description object members.
				
				IWMEncDisplayInfo Descr = Encoder.DisplayInfo;
				Descr.Author = info.Author;
				Descr.Copyright = info.Copyright;
				Descr.Description = info.Description;
				Descr.Rating = info.Rating;
				Descr.Title = info.Title;
				

				// Add an attribute to the collection.
				IWMEncAttributes Attr = Encoder.Attributes;
				Attr.Add ("URL", "IP address");

				// Start the encoding process.
				// Wait until the encoding process stops before exiting the application.
				//Encoder.PrepareToEncode(true);
				Encoder.Start();
			} 
			catch (Exception e) 
			{  
				// TODO: Handle exceptions.
				Log.Write("unable to transcode file:{0} message:{1}", info.file,e.Message);
				Encoder=null;
				return false;
			}
			finishedEncoding=false;
			return true;
		}

		public bool IsTranscoding()
		{
			if (IsFinished()) return false;

			return true;
		}

		public bool IsFinished()
		{
			if (Encoder==null) return true;
			if (!finishedEncoding) return false;
			Encoder.Stop();
			Encoder=null;
			return true;
		}

		
		public int Percentage()
		{
			if (Encoder==null) return 100;
			return -1;
		}

		private void OnStateChange(WMENC_ENCODER_STATE enumState)
		{
				switch( enumState )
		 {
			 case WMENC_ENCODER_STATE.WMENC_ENCODER_RUNNING:
				 // TODO: Handle running state.
				 break;

			 case WMENC_ENCODER_STATE.WMENC_ENCODER_STOPPED:
				 // TODO: Handle stopped state.
				 Encoder.OnStateChange -= new _IWMEncoderEvents_OnStateChangeEventHandler(OnStateChange);
				 finishedEncoding=true;
				 break;

			 case WMENC_ENCODER_STATE.WMENC_ENCODER_STARTING:
				 // TODO: Handle starting state.
				 break;

			 case WMENC_ENCODER_STATE.WMENC_ENCODER_PAUSING:
				 // TODO: Handle pausing state.
				 break;

			 case WMENC_ENCODER_STATE.WMENC_ENCODER_STOPPING:
				 // TODO: Handle stopping state.
				 break;

			 case WMENC_ENCODER_STATE.WMENC_ENCODER_PAUSED:
				 // TODO: Handle paused state.
				 break;

			 case WMENC_ENCODER_STATE.WMENC_ENCODER_END_PREPROCESS:
				 // TODO: Handle end preprocess state.
				 break;
		 }
		}
	}
}
