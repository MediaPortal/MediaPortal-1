using System;
using System.Drawing;
using Microsoft.Win32;
using DShowNET;
using MediaPortal.GUI.Library;

using System.Runtime.InteropServices;
namespace MediaPortal.Core.Transcoding
{
	/// <summary>
	/// Summary description for Dvrms2XVID.
	/// </summary>
	public class Dvrms2XVID : ITranscode
	{
		protected int												rotCookie = 0;
		protected  IGraphBuilder			  			      graphBuilder =null;
		protected  IStreamBufferSource 			        bufferSource=null ;
		protected IFileSinkFilter										fileWriterFilter = null;			// DShow Filter: file writer
		protected IMediaControl											mediaControl=null;
		protected IMediaSeeking											mediaSeeking=null;
		protected IBaseFilter												xvidCodec =null;
		protected IBaseFilter												mp3Codec =null;
		protected IBaseFilter												Mpeg2VideoCodec =null;
		protected IBaseFilter												Mpeg2AudioCodec =null;
		protected IBaseFilter												aviMuxer =null;
		protected IMediaEventEx											mediaEvt=null;
		protected int bitrate;
		protected int fps;
		protected Size screenSize;

		public Dvrms2XVID()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region ITranscode Members

		public bool Supports(MediaPortal.Core.Transcoding.VideoFormat format)
		{
			if (format==VideoFormat.Xvid) return true;
			return false;
		}
		public void CreateProfile(Size videoSize, int bitRate, int FPS)
		{
			bitrate=bitRate;
			screenSize=videoSize;
			fps=FPS;

		}

		public bool Transcode(TranscodeInfo info, MediaPortal.Core.Transcoding.VideoFormat format, MediaPortal.Core.Transcoding.Quality quality)
		{
			if (!Supports(format)) return false;
			string ext=System.IO.Path.GetExtension(info.file);
			if (ext.ToLower() !=".dvr-ms" && ext.ToLower() !=".sbe" ) return false;

			//disable xvid status window while encoding
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				RegistryKey subkey = hkcu.OpenSubKey(@"Software\GNU\XviD",true);
				if (subkey != null)
				{
					Int32 uivalue=0;
					subkey.SetValue("display_status", (Int32)uivalue);
					subkey.SetValue("debug", (Int32)uivalue);
					subkey.SetValue("bitrate", (Int32)bitrate);

					uivalue=1;
					subkey.SetValue("interlacing", (Int32)uivalue);
					subkey.Close();
				}
				hkcu.Close();
			}
			catch(Exception)
			{
			}
			Type comtype = null;
			object comobj = null;
			try 
			{
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


				string monikerXVID=@"@device:cm:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\xvid";
				xvidCodec = Marshal.BindToMoniker( monikerXVID ) as IBaseFilter;
				if (xvidCodec==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Unable to create XviD MPEG-4 Codec");
					Cleanup();
					return false;
				}

				hr = graphBuilder.AddFilter( xvidCodec, "XviD MPEG-4 Codec" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Add XviD MPEG-4 Codec to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}


				string monikerMPEG3=@"@device:cm:{33D9A761-90C8-11D0-BD43-00A0C911CE86}\85MPEG Layer-3";
				mp3Codec = Marshal.BindToMoniker( monikerMPEG3 ) as IBaseFilter;
				if (mp3Codec==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Unable to create MPEG Layer-3 Codec");
					Cleanup();
					return false;
				}

				hr = graphBuilder.AddFilter( mp3Codec, "MPEG Layer-3" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Add MPEG Layer-3 to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				//add filewriter 
				string monikerFileWrite=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{8596E5F0-0DA5-11D0-BD21-00A0C911CE86}";
				IBaseFilter fileWriterbase = Marshal.BindToMoniker( monikerFileWrite ) as IBaseFilter;
				if (fileWriterbase==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Unable to create FileWriter");
					Cleanup();
					return false;
				}

				
				fileWriterFilter = fileWriterbase as IFileSinkFilter;
				if (fileWriterFilter ==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Add unable to get IFileSinkFilter for filewriter");
					Cleanup();
					return false;
				}

				hr = graphBuilder.AddFilter( fileWriterbase , "FileWriter" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Add FileWriter to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}


				//set output filename
				AMMediaType mt = new AMMediaType();
				string outputFileName=System.IO.Path.ChangeExtension(info.file,".avi");
				mt.majorType=MediaType.Stream;
				mt.subType=MediaSubType.Avi;
				mt.formatType=FormatType.None;
				hr=fileWriterFilter.SetFileName(outputFileName, ref mt);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to set filename for filewriter :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				// add avi muxer
				string monikerAviMuxer=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{E2510970-F137-11CE-8B67-00AA00A3F1A6}";
				aviMuxer = Marshal.BindToMoniker( monikerAviMuxer ) as IBaseFilter;
				if (aviMuxer==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Unable to create AviMux");
					Cleanup();
					return false;
				}


				hr = graphBuilder.AddFilter( aviMuxer , "AviMux" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Add AviMux to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				//add mpeg2 audio/video codecs
				string strVideoCodecMoniker=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{F50B3F13-19C4-11CF-AA9A-02608C9BABA2}";
				string strAudioCodec="MPEG/AC3/DTS/LPCM Audio Decoder";
				Mpeg2VideoCodec = Marshal.BindToMoniker( strVideoCodecMoniker ) as IBaseFilter;
				if (Mpeg2VideoCodec==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to add Elecard mpeg2 video decoder");
					Cleanup();
					return false;
				}
				hr = graphBuilder.AddFilter( Mpeg2VideoCodec , "Elecard mpeg2 video decoder" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:Add Elecard mpeg2 video  to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				Mpeg2AudioCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (Mpeg2AudioCodec==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to add mpeg2 audio codec");
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
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to get pins of source");
					Cleanup();
					return false;
				}

				DsUtils.GetPin(Mpeg2VideoCodec,PinDirection.Input,0,out pinIn0);
				DsUtils.GetPin(Mpeg2AudioCodec,PinDirection.Input,0,out pinIn1);
				if (pinIn0==null || pinIn1==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to get pins of mpeg2 video/audio codec");
					Cleanup();
					return false;
				}
				
				AMMediaType amAudio= new AMMediaType();
				amAudio.majorType = MediaType.Audio;
				amAudio.subType = MediaSubType.MPEG2_Audio;
				pinOut0.Connect(pinIn1,ref amAudio);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to connect audio pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				
				AMMediaType amVideo= new AMMediaType();
				amVideo.majorType = MediaType.Video;
				amVideo.subType = MediaSubType.MPEG2_Video;
				pinOut1.Connect(pinIn0,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to connect video pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}


				//connect output of mpeg2 codec to xvid codec
				IPin pinOut, pinIn;
				hr=DsUtils.GetPin(xvidCodec,PinDirection.Input,0,out pinIn);
				if (hr!=0 || pinIn==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get input pin of xvid codec:0x{0:X}",hr);
					Cleanup();
					return false;
				}
				hr=DsUtils.GetPin(Mpeg2VideoCodec,PinDirection.Output,0,out pinOut);
				if (hr!=0 || pinOut==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get output pin of mpeg2 video codec :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				amVideo= new AMMediaType();
				pinOut.Connect(pinIn,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to connect mpeg2 video codec->xvid:0x{0:X}",hr);
					Cleanup();
					return false;
				}

				//connect output of mpeg2 audio codec to mpeg3 codec
				hr=DsUtils.GetPin(mp3Codec,PinDirection.Input,0,out pinIn);
				if (hr!=0 || pinIn==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get input pin of mp3 codec:0x{0:X}",hr);
					Cleanup();
					return false;
				}
				hr=DsUtils.GetPin(Mpeg2AudioCodec,PinDirection.Output,0,out pinOut);
				if (hr!=0 || pinOut==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get output pin of mpeg2 audio codec :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				amVideo= new AMMediaType();
				pinOut.Connect(pinIn,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to connect mpeg2 audio codec->mpeg3:0x{0:X}",hr);
					Cleanup();
					return false;
				}



				//connect output of mpeg3 codec to pin#0 of avimux
				hr=DsUtils.GetPin(mp3Codec,PinDirection.Output,0,out pinOut);
				if (hr!=0 || pinOut==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get input pin of mp3 codec:0x{0:X}",hr);
					Cleanup();
					return false;
				}
				hr=DsUtils.GetPin(aviMuxer,PinDirection.Input,0,out pinIn);
				if (hr!=0 || pinIn==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get output pin of mpeg2 audio codec :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				amVideo= new AMMediaType();
				pinOut.Connect(pinIn,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to connect mpeg3 codec->avimux:0x{0:X}",hr);
					Cleanup();
					return false;
				}

				//connect output of xvid codec to pin#1 of avimux
				hr=DsUtils.GetPin(xvidCodec,PinDirection.Output,0,out pinOut);
				if (hr!=0 || pinOut==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get input pin of mp3 codec:0x{0:X}",hr);
					Cleanup();
					return false;
				}
				hr=DsUtils.GetPin(aviMuxer,PinDirection.Input,1,out pinIn);
				if (hr!=0 || pinIn==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get output#1 pin of avimux :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				amVideo= new AMMediaType();
				pinOut.Connect(pinIn,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to connect xvid codec->avimux:0x{0:X}",hr);
					Cleanup();
					return false;
				}


				//connect avi mux out->filewriter in
				hr=DsUtils.GetPin(aviMuxer,PinDirection.Output,0,out pinOut);
				if (hr!=0 || pinOut==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get output pin of avimux:0x{0:X}",hr);
					Cleanup();
					return false;
				}

				hr=DsUtils.GetPin(fileWriterbase,PinDirection.Input,0,out pinIn);
				if (hr!=0 || pinIn==null)
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:cannot get input pin of Filewriter :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				hr=pinOut.Connect(pinIn,ref mt);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:connect muxer->filewriter :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				mediaControl= graphBuilder as IMediaControl;
				mediaSeeking= graphBuilder as IMediaSeeking;
				mediaEvt    = graphBuilder as IMediaEventEx;
				hr=mediaControl.Run();
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2XVID:FAILED:unable to start graph :0x{0:X}",hr);
					Cleanup();
					return false;
				}
			}
			catch(Exception ex)
			{
				Log.Write("DVR2XVID:Unable create graph", ex.Message);
				Cleanup();
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
			if (percent >100) percent=100;
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

			if ( xvidCodec != null )
				Marshal.ReleaseComObject( xvidCodec );
			xvidCodec=null;
			
			if ( mp3Codec != null )
				Marshal.ReleaseComObject( mp3Codec );
			mp3Codec=null;

			if ( aviMuxer != null )
				Marshal.ReleaseComObject( aviMuxer );
			aviMuxer=null;
			
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

		#endregion
	}
}
