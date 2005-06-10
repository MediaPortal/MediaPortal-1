using System;
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

		public TranscodeToWMV()
		{
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
				Encoder.PrepareToEncode(true);
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
