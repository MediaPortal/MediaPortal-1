using System;
using DShowNET;
using MediaPortal.GUI.Library;

using System.Runtime.InteropServices;
namespace MediaPortal.Core.Transcoding
{
	/// <summary>
	/// Summary description for Dvrms2Mpeg.
	/// </summary>
	public class Dvrms2Mpeg : ITranscode
	{
		protected int												rotCookie = 0;
		protected  IGraphBuilder			  			      graphBuilder =null;
		protected  IStreamBufferSource 			        bufferSource=null ;
		protected IFileSinkFilter										fileWriterFilter = null;			// DShow Filter: file writer
		protected IMediaControl											mediaControl=null;
		protected IBaseFilter												powerDvdMuxer =null;

		public Dvrms2Mpeg()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region ITranscode Members

		public bool Supports(MediaPortal.Core.Transcoding.VideoFormat format)
		{
			if (format==VideoFormat.Mpeg2) return true;
			return false;
		}

		public bool Transcode(TranscodeInfo info, MediaPortal.Core.Transcoding.VideoFormat format, MediaPortal.Core.Transcoding.Quality quality)
		{
			if (!Supports(format)) return false;
			string ext=System.IO.Path.GetExtension(info.file);
			if (ext.ToLower() !=".dvr-ms") return false;

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


				string monikerPowerDvdMuxer=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{4B5C6BC0-D60E-11D2-8F3F-0080C84E9806}";
				powerDvdMuxer = Marshal.BindToMoniker( monikerPowerDvdMuxer ) as IBaseFilter;
				if (powerDvdMuxer==null)
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:Unable to create Cyberlink MPEG Muxer (PowerDVD)");
					Cleanup();
					return false;
				}

				hr = graphBuilder.AddFilter( powerDvdMuxer, "Cyberlink MPEG Muxer" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:Add Cyberlink MPEG Muxer to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				//add filewriter 
				string monikerFileWrite=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{8596E5F0-0DA5-11D0-BD21-00A0C911CE86}";
				IBaseFilter fileWriterbase = Marshal.BindToMoniker( monikerFileWrite ) as IBaseFilter;
				if (fileWriterbase==null)
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:Unable to create FileWriter");
					Cleanup();
					return false;
				}

				
				fileWriterFilter = fileWriterbase as IFileSinkFilter;
				if (fileWriterFilter ==null)
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:Add unable to get IFileSinkFilte for filewriter");
					Cleanup();
					return false;
				}

				hr = graphBuilder.AddFilter( fileWriterbase , "FileWriter" );
				if( hr != 0 ) 
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:Add FileWriter to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}


				//connect output #0 of streambuffer source->powerdvd audio in
				//connect output #1 of streambuffer source->powerdvd video in
				IPin pinOut0, pinOut1;
				IPin pinIn0, pinIn1;
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,0,out pinOut0);
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,1,out pinOut1);

				DsUtils.GetPin(powerDvdMuxer,PinDirection.Input,0,out pinIn0);
				DsUtils.GetPin(powerDvdMuxer,PinDirection.Input,1,out pinIn1);
				if (pinOut0==null || pinOut1==null || pinIn0==null || pinIn1==null)
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:unable to get pins of muxer&source");
					Cleanup();
					return false;
				}
				
				AMMediaType amAudio= new AMMediaType();
				amAudio.majorType = MediaType.Audio;
				amAudio.subType = MediaSubType.MPEG2_Audio;
				pinOut0.Connect(pinIn1,ref amAudio);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:unable to connect audio pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				
				AMMediaType amVideo= new AMMediaType();
				amVideo.majorType = MediaType.Video;
				amVideo.subType = MediaSubType.MPEG2_Video;
				pinOut1.Connect(pinIn0,ref amVideo);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:unable to connect video pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}


				//connect output of powerdvd muxer->input of filewriter
				IPin pinOut, pinIn;
				hr=DsUtils.GetPin(powerDvdMuxer,PinDirection.Output,0,out pinOut);
				if (hr!=0 || pinOut==null)
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:cannot get output pin of Cyberlink MPEG muxer :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				hr=DsUtils.GetPin(fileWriterbase,PinDirection.Input,0,out pinIn);
				if (hr!=0 || pinIn==null)
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:cannot get input pin of Filewriter :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				AMMediaType mt = new AMMediaType(); 
				hr=pinOut.Connect(pinIn,ref mt);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:connect muxer->filewriter :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				//set output filename
				string outputFileName=System.IO.Path.ChangeExtension(info.file,".mpg");
				mt.majorType=MediaType.Stream;
				mt.subType=MediaSubType.MPEG2;

				hr=fileWriterFilter.SetFileName(outputFileName, ref mt);
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:unable to set filename for filewriter :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				mediaControl= graphBuilder as IMediaControl;
				hr=mediaControl.Run();
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2MPG:FAILED:unable to start graph :0x{0:X}",hr);
					Cleanup();
					return false;
				}
			}
			catch(Exception ex)
			{
				Log.Write("DVR2MPG:Unable create graph", ex.Message);
				Cleanup();
				return false;
			}
			return true;
		}

		public bool IsFinished()
		{
			if (mediaControl==null) return true;
			int state;

			mediaControl.GetState(200, out state);
			if (state==0)
			{
				Cleanup();
				return true;
			}
			return false;
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

			if ( powerDvdMuxer != null )
				Marshal.ReleaseComObject( powerDvdMuxer );
			powerDvdMuxer=null;

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
