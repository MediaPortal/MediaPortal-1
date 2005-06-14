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
			int ConfigureFilterUsingProfileGuid([In] ref Guid guidProfile);
			[PreserveSig]
			int GetCurrentProfileGuid([Out] out Guid pProfileGuid);
			[PreserveSig]
			int ConfigureFilterUsingProfile([In] IWMProfile pProfile);
			[PreserveSig]
			int GetCurrentProfile([Out] out IWMProfile ppProfile);
			[PreserveSig]
			int SetIndexMode( [In]  bool bIndexFile );
			[PreserveSig]
			int GetIndexMode( [Out] out bool pbIndexFile );
		}
		[ComVisible(true), ComImport,
		Guid("96406BDB-2B2B-11d3-B36B-00C04F6108FF"),
		InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IWMProfile 
		{
			[PreserveSig]
			int GetVersion( [Out] out int pdwVersion );
			[PreserveSig]
			int GetName( [Out] IntPtr pwszName,ref uint pcchName );
			[PreserveSig]
			int SetName( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName );
			[PreserveSig]
			int GetDescription( [Out, MarshalAs(UnmanagedType.LPWStr)] out string pwszDescription, ref uint pcchDescription );
			[PreserveSig]
			int SetDescription( [In,MarshalAs(UnmanagedType.LPWStr)] string pwszDescription );
			[PreserveSig]
			int GetStreamCount( [Out] out uint pcStreams );
			[PreserveSig]
			int GetStream( [In] uint dwStreamIndex,[Out] out IWMStreamConfig ppConfig );
			[PreserveSig]
			int GetStreamByNumber( [In] short wStreamNum,[Out] out IWMStreamConfig ppConfig );
			[PreserveSig]
			int RemoveStream( [In] IWMStreamConfig pConfig );
			[PreserveSig]
			int RemoveStreamByNumber( [In] short wStreamNum );
			[PreserveSig]
			int AddStream( [In] IWMStreamConfig pConfig );
			[PreserveSig]
			int ReconfigStream( [In] IWMStreamConfig pConfig );
			[PreserveSig]
			int CreateNewStream( [In] Guid guidStreamType,[Out] out IWMStreamConfig ppConfig );
			[PreserveSig]
			int GetMutualExclusionCount( [Out] out uint pcME );
			[PreserveSig]
			int GetMutualExclusion( [In] uint dwMEIndex,[Out] out IWMMutualExclusion ppME );
			[PreserveSig]
			int RemoveMutualExclusion( [In] IWMMutualExclusion pME );
			[PreserveSig]
			int AddMutualExclusion( [In] IWMMutualExclusion pME );
			[PreserveSig]
			int CreateNewMutualExclusion( [Out] out IWMMutualExclusion ppME );
		};

		[ComVisible(true), ComImport,
		Guid("96406BDE-2B2B-11d3-B36B-00C04F6108FF"),
		InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IWMMutualExclusion 
		{
			[PreserveSig]
			int GetStreams( [Out] short[] pwStreamNumArray,ref short pcStreams );
			[PreserveSig]
			int AddStream( [In] short wStreamNum );
			[PreserveSig]
			int RemoveStream( [In] short wStreamNum );
			[PreserveSig]
			int GetType( [Out] out Guid pguidType );
			[PreserveSig]
			int SetType( [In] Guid guidType );
		};


		[ComVisible(true), ComImport,
		Guid("96406BDC-2B2B-11d3-B36B-00C04F6108FF"),
		InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IWMStreamConfig 
		{
			[PreserveSig]
			int GetStreamType( [Out] out Guid pGuidStreamType );
			[PreserveSig]
			int GetStreamNumber( [Out] out short pwStreamNum );
			[PreserveSig]
			int SetStreamNumber( [In] short wStreamNum );
			[PreserveSig]
			int GetStreamName( [Out, MarshalAs(UnmanagedType.LPWStr)] out string pwszStreamName, ref short pcchStreamName );
			[PreserveSig]
			int SetStreamName( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszStreamName );
			[PreserveSig]
			int GetConnectionName( [Out, MarshalAs(UnmanagedType.LPWStr)] out string pwszInputName,ref short pcchInputName );
			[PreserveSig]
			int SetConnectionName( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszInputName );
			[PreserveSig]
			int GetBitrate( [Out] out uint pdwBitrate );
			[PreserveSig]
			int SetBitrate( [In] uint  pdwBitrate );
			[PreserveSig]
			int GetBufferWindow( [Out] out uint pmsBufferWindow );
			[PreserveSig]
			int SetBufferWindow( [In] uint msBufferWindow );
		};

		public enum WMT_ATTR_DATATYPE:int
		{

			WMT_TYPE_DWORD      = 0,
			WMT_TYPE_STRING     = 1,
			WMT_TYPE_BINARY     = 2,
			WMT_TYPE_BOOL       = 3,
			WMT_TYPE_QWORD      = 4,
			WMT_TYPE_WORD       = 5,
			WMT_TYPE_GUID       = 6,
		}

		[ComVisible(true), ComImport,
			Guid("962dc1ec-c046-4db8-9cc7-26ceae500817"),
			InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IWMWriterAdvanced2 
		{
			[PreserveSig]
			int GetSinkCount( [Out] out uint pcSinks );
			[PreserveSig]
			int GetSink( [In] uint dwSinkNum,[Out] out IntPtr ppSink );//IWMWriterSink
			[PreserveSig]
			int AddSink( [In] IntPtr pSink );//IWMWriterSink
			[PreserveSig]
			int RemoveSink( [In] IntPtr pSink );//IWMWriterSink
			[PreserveSig]
			int WriteStreamSample( [In] short wStreamNum,
															[In] Int64 cnsSampleTime,
															[In] uint msSampleSendTime,
															[In] Int64 cnsSampleDuration,
															[In] uint dwFlags,
															[In] IntPtr pSample );//INSSBuffer

			[PreserveSig]
			int SetLiveSource( bool fIsLiveSource );
			[PreserveSig]
			int IsRealTime( [Out] out bool pfRealTime );
			[PreserveSig]
			int GetWriterTime( [Out] out Int64 pcnsCurrentTime );
			[PreserveSig]
			int GetStatistics( [In] short wStreamNum,[Out] IntPtr pStats );//WM_WRITER_STATISTICS
			[PreserveSig]
			int SetSyncTolerance(   [In]    uint   msWindow );
			[PreserveSig]
			int GetSyncTolerance(   [Out]   out uint  pmsWindow );
			[PreserveSig]
			int GetInputSetting(
								[In] uint dwInputNum,
								[In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
								[Out] out WMT_ATTR_DATATYPE pType,
								[Out] out IntPtr pValue,
								[In, Out] ref short pcbLength );

			[PreserveSig]
			int SetInputSetting(
							[In] uint dwInputNum,
							[In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
							[In] WMT_ATTR_DATATYPE Type,
							[In] IntPtr pValue,
							[In] short cbLength );
		};

		[ComVisible(true), ComImport,
		Guid("005140c1-7436-11ce-8034-00aa006009fa"),
		InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IServiceProvider 
		{
			[PreserveSig]
			int QueryService([In] ref Guid guidService,[In] Guid riid,[Out, MarshalAs(UnmanagedType.IUnknown) ]		out	object		ppint );
		};

		Guid WMProfile_V80_256Video = new Guid(0xbbc75500,0x33d2,0x4466,0xb8, 0x6b, 0x12, 0x2b, 0x20, 0x1c, 0xc9, 0xae );
		Guid WMProfile_V80_384Video = new Guid(0x29b00c2b,0x9a9,0x48bd,0xad, 0x9, 0xcd, 0xae, 0x11, 0x7d, 0x1d, 0xa7 );
		Guid WMProfile_V80_768Video= new Guid(0x74d01102,0xe71a,0x4820,0x8f, 0xd, 0x13, 0xd2, 0xec, 0x1e, 0x48, 0x72 );
		Guid WMProfile_V80_700NTSCVideo = new Guid(0xc8c2985f,0xe5d9,0x4538,0x9e, 0x23, 0x9b, 0x21, 0xbf, 0x78, 0xf7, 0x45 );
		Guid WMProfile_V80_1400NTSCVideo= new Guid( 0x931d1bee,0x617a,0x4bcd,0x99, 0x5, 0xcc, 0xd0, 0x78, 0x66, 0x83, 0xee );
		Guid WMProfile_V80_384PALVideo = new Guid(0x9227c692,0xae62,0x4f72,0xa7, 0xea, 0x73, 0x60, 0x62, 0xd0, 0xe2, 0x1e );
		Guid WMProfile_V80_700PALVideo = new Guid( 0xec298949,0x639b,0x45e2,0x96, 0xfd, 0x4a, 0xb3, 0x2d, 0x59, 0x19, 0xc2 );
		Guid WMProfile_V80_FAIRVBRVideo= new Guid(0x3510a862,0x5850,0x4886,0x83, 0x5f, 0xd7, 0x8e, 0xc6, 0xa6, 0x40, 0x42 );
		Guid WMProfile_V80_HIGHVBRVideo = new Guid(0xf10d9d3,0x3b04,0x4fb0,0xa3, 0xd3, 0x88, 0xd4, 0xac, 0x85, 0x4a, 0xcc );
		Guid WMProfile_V80_BESTVBRVideo = new Guid(0x48439ba,0x309c,0x440e,0x9c, 0xb4, 0x3d, 0xcc, 0xa3, 0x75, 0x64, 0x23 );

		Guid IID_IWMWriterAdvanced2 = new Guid(0x962dc1ec,0xc046,0x4db8,0x9c,0xc7,0x26,0xce,0xae,0x50,0x08,0x17 );

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

		protected int bitrate;
		protected int fps;
		protected Size screenSize;
		public TranscodeToWMV()
		{
		}
		public void CreateProfile(Size videoSize, int bitRate, int FPS)
		{
			bitrate=bitRate;
			screenSize=videoSize;
			fps=FPS;

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
				switch (quality)
				{
					case Quality.High:
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_BESTVBRVideo);
						break;
					case Quality.Medium:
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_FAIRVBRVideo);
						break;
					case Quality.Low:
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_768Video);
						break;
					case Quality.Portable:
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_256Video);
						break;
					case Quality.Custom:
						//create new profile
						IWMProfile newProfile ;
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_BESTVBRVideo);
						hr=config.GetCurrentProfile(out newProfile);
						if (hr==0)
						{
							uint numberOfStreams;
							uint bitr;
							Guid majorType;
							IWMStreamConfig streamConfig;
							uint chars=256;
							string name;
							int version;
							hr=newProfile.GetVersion(out version);
							IntPtr namePtr=Marshal.AllocCoTaskMem((int)chars);
							hr=newProfile.GetName(namePtr, ref chars);
							name=Marshal.PtrToStringAuto(namePtr);
							hr=newProfile.GetStreamCount(out numberOfStreams);
							
							for (uint i=0; i < numberOfStreams;++i)
							{
								hr=newProfile.GetStream(i, out streamConfig);
								hr=streamConfig.GetBitrate(out bitr);
								hr=streamConfig.GetStreamType(out majorType);
								if (majorType==MediaType.Video)
								{
								}
								if (majorType==MediaType.Audio)
								{
								}
							}
						}
					break;
				}
				/*
				IServiceProvider sProvider = fileWriterbase as IServiceProvider;
				object tmpObj;
				sProvider.QueryService(ref IID_IWMWriterAdvanced2, IID_IWMWriterAdvanced2,out tmpObj);
				IWMWriterAdvanced2 writerAdv = tmpObj as IWMWriterAdvanced2;
				IntPtr intptr=Marshal.AllocCoTaskMem(4);
				Marshal.WriteInt32(intptr,1);
				writerAdv.SetInputSetting(1,"g_wszDeinterlaceMode",WMT_ATTR_DATATYPE.WMT_TYPE_DWORD,intptr,4);
				Marshal.FreeCoTaskMem(intptr);*/
				if (hr!=0 )
				{
					DirectShowUtil.DebugWrite("DVR2WMV:FAILED:unable to set profile :0x{0:X}",hr);
					Cleanup();
					return false;
				}
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
