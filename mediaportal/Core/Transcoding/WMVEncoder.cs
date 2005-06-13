using System;
using System.Drawing;
using System.Runtime.InteropServices;
using DShowNET;
using MediaPortal.GUI.Library;
namespace MediaPortal.Core.Transcoding
{
	/// <summary>
	/// This class encodes an video file to .wmv format
	/// </summary>
	public class TranscodeToWMV : ITranscode	
	{

		[ComVisible(true), ComImport,
		Guid("45086030-F7E4-486a-B504-826BB5792A3B"),
		InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IConfigAsfWriter 
		{
			[PreserveSig]
			int ConfigureFilterUsingProfileId([In] uint dwProfileId);
			[PreserveSig]
			int GetCurrentProfileId([Out] out uint pdwProfileId);
			[PreserveSig]
			int ConfigureFilterUsingProfileGuid([In] Guid guidProfile);
			[PreserveSig]
			int GetCurrentProfileGuid([Out] out Guid pProfileGuid);
			[PreserveSig]
			int ConfigureFilterUsingProfile([In] IntPtr  pProfile);
			[PreserveSig]
			int GetCurrentProfile([Out] out IntPtr ppProfile);
			[PreserveSig]
			int SetIndexMode( [In]  bool bIndexFile );
			[PreserveSig]
			int GetIndexMode( [Out] out bool pbIndexFile );
		}

		protected int												rotCookie = 0;
		protected  IGraphBuilder			  			      graphBuilder =null;
		protected  IStreamBufferSource 			        bufferSource=null ;
		protected IMediaControl											mediaControl=null;
		protected IMediaSeeking											mediaSeeking=null;
		protected IBaseFilter												powerDvdMuxer =null;
		protected IMediaEventEx											mediaEvt=null;
		protected IFileSinkFilter										fileWriterFilter = null;			// DShow Filter: file writer
		protected IBaseFilter												Mpeg2VideoCodec =null;
		protected IBaseFilter												Mpeg2AudioCodec =null;

		public TranscodeToWMV()
		{
		}
		public void CreateProfile(string name, int KBPS, Size videoSize, int bitRate, int FPS)
		{


		}
		
		public bool Supports(VideoFormat format)
		{
			if (format==VideoFormat.Wmv) return true;
			return false;
		}

		public bool Transcode(TranscodeInfo info, VideoFormat format,Quality quality)
		{
			try
			{
				if (!Supports(format)) return false;
				string ext=System.IO.Path.GetExtension(info.file);
				if (ext.ToLower() !=".dvr-ms" && ext.ToLower() !=".sbe" ) return false;

				Type comtype = null;
				object comobj = null;
				comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
				if( comtype == null )
				{
					Log.Write("StreamBufferPlayer9:DirectX 9 not installed");
					return false;
				}
				comobj = Activator.CreateInstance( comtype );
				graphBuilder = (IGraphBuilder) comobj; comobj = null;
		
				DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph

				Guid clsid = Clsid.StreamBufferSource;
				Guid riid = typeof(IStreamBufferSource).GUID;
				Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
				bufferSource = (IStreamBufferSource) comObj; comObj = null;

	
				IBaseFilter filter = (IBaseFilter) bufferSource;
				graphBuilder.AddFilter(filter, "SBE SOURCE");
	
				IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
				int hr = fileSource.Load(info.file, IntPtr.Zero);

				//add mpeg2 audio/video codecs
				string strVideoCodec="Mpeg2Dec Filter";
				string strAudioCodec="MPEG/AC3/DTS/LPCM Audio Decoder";
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec",strVideoCodec);
					strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec",strAudioCodec);
				}
			
				Mpeg2VideoCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
				if (Mpeg2VideoCodec==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to add mpeg2 video codec");
					Cleanup();
					return false;
				}
				Mpeg2AudioCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (Mpeg2AudioCodec==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to add mpeg2 audio codec");
					Cleanup();
					return false;
				}

				//connect output #0 of streambuffer source->mpeg2 audio codec pin 1
				//connect output #1 of streambuffer source->mpeg2 video codec pin 1
				IPin pinOut0, pinOut1;
				IPin pinIn0, pinIn1;
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,0,out pinOut0);
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,1,out pinOut1);
				if (pinOut0==null || pinOut1==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to get pins of source");
					Cleanup();
					return false;
				}

				DsUtils.GetPin(Mpeg2VideoCodec,PinDirection.Input,0,out pinIn0);
				DsUtils.GetPin(Mpeg2AudioCodec,PinDirection.Input,0,out pinIn1);
				if (pinIn0==null || pinIn1==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to get pins of mpeg2 video/audio codec");
					Cleanup();
					return false;
				}
			
				AMMediaType amAudio= new AMMediaType();
				amAudio.majorType = MediaType.Audio;
				amAudio.subType = MediaSubType.MPEG2_Audio;
				pinOut0.Connect(pinIn1,ref amAudio);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to connect audio pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}

			
				AMMediaType amVideo= new AMMediaType();
				amVideo.majorType = MediaType.Video;
				amVideo.subType = MediaSubType.MPEG2_Video;
				pinOut1.Connect(pinIn0,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to connect video pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}


				//add asf file writer
				string monikerAsfWriter=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7C23220E-55BB-11D3-8B16-00C04FB6BD3D}";

				IBaseFilter fileWriterbase = Marshal.BindToMoniker( monikerAsfWriter ) as IBaseFilter;
				if (fileWriterbase==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:Unable to create FileWriter");
					Cleanup();
					return false;
				}

			
				fileWriterFilter = fileWriterbase as IFileSinkFilter;
				if (fileWriterFilter ==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:Add unable to get IFileSinkFilter for filewriter");
					Cleanup();
					return false;
				}
				//set output filename
				string outputFileName=System.IO.Path.ChangeExtension(info.file,".wmv");
				AMMediaType mt = new AMMediaType();
				hr=fileWriterFilter.SetFileName(outputFileName, ref mt);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to set filename for filewriter :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				hr = graphBuilder.AddFilter( fileWriterbase , "WM ASF Writer" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:Add FileWriter to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}


				//connect output #0 of videocodec->asf writer pin 1
				//connect output #0 of audiocodec->asf writer pin 0
				DsUtils.GetPin((IBaseFilter)Mpeg2AudioCodec,PinDirection.Output,0,out pinOut0);
				DsUtils.GetPin((IBaseFilter)Mpeg2VideoCodec,PinDirection.Output,0,out pinOut1);
				if (pinOut0==null || pinOut1==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to get outpins of video codec");
					Cleanup();
					return false;
				}

				DsUtils.GetPin(fileWriterbase,PinDirection.Input,0,out pinIn0);
				DsUtils.GetPin(fileWriterbase,PinDirection.Input,1,out pinIn1);
				if (pinIn0==null || pinIn1==null)
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to get pins of asf wm writer");
					Cleanup();
					return false;
				}
			
				amAudio= new AMMediaType();
				pinOut0.Connect(pinIn0,ref amAudio);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to connect audio pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}

			
				amVideo= new AMMediaType();
				pinOut1.Connect(pinIn1,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to connect video pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				IConfigAsfWriter config = fileWriterbase as IConfigAsfWriter;
				//config.ConfigureFilterUsingProfileGuid();

				mediaControl= graphBuilder as IMediaControl;
				mediaSeeking= graphBuilder as IMediaSeeking;
				mediaEvt    = graphBuilder as IMediaEventEx;
				hr=mediaControl.Run();
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to start graph :0x{0:X}",hr);
					Cleanup();
					return false;
				}
			} 
			catch (Exception e) 
			{  
				// TODO: Handle exceptions.
				Log.Write("unable to transcode file:{0} message:{1}", info.file,e.Message);

				return false;
			}
			return true;
		}
		public bool IsFinished()
		{
			if (mediaControl==null) return true;
			FilterState state;

			mediaControl.GetState(200, out state);
			if (state==FilterState.Stopped)
			{
				Cleanup();
				return true;
			}
			int p1, p2, hr = 0;
			DsEvCode code;
			hr = mediaEvt.GetEvent( out code, out p1, out p2, 0 );
			hr = mediaEvt.FreeEventParams( code, p1, p2 );
			if( code == DsEvCode.Complete || code== DsEvCode.ErrorAbort)
			{
				Cleanup();
				return true;
			}
			return false;
		}

		public int Percentage()
		{
			if (mediaSeeking==null) return 100;
			long lDuration,lCurrent;
			mediaSeeking.GetCurrentPosition(out lCurrent);
			mediaSeeking.GetDuration(out lDuration);
			float percent = ((float)lCurrent) / ((float)lDuration);
			percent*=100.0f;
			return (int)percent;
		}

		public bool IsTranscoding()
		{
			if (IsFinished()) return false;
			return true;
		}

		void Cleanup()
		{
			if( rotCookie != 0 )
				DsROT.RemoveGraphFromRot( ref rotCookie );

			if( mediaControl != null )
			{
				mediaControl.Stop();
				mediaControl = null;
			}
			mediaSeeking=null;
			mediaEvt=null;

			
			
			if ( Mpeg2AudioCodec != null )
				Marshal.ReleaseComObject( Mpeg2AudioCodec );
			Mpeg2AudioCodec=null;
			
			if ( Mpeg2VideoCodec != null )
				Marshal.ReleaseComObject( Mpeg2VideoCodec );
			Mpeg2VideoCodec=null;

			if ( fileWriterFilter != null )
				Marshal.ReleaseComObject( fileWriterFilter );
			fileWriterFilter=null;

			if ( bufferSource != null )
				Marshal.ReleaseComObject( bufferSource );
			bufferSource = null;

			DsUtils.RemoveFilters(graphBuilder);

			if( graphBuilder != null )
				Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;
		}

	}
}
