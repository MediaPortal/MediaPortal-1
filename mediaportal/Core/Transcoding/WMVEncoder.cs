/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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
		#region imports

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern int SetWmvProfile(DShowNET.IBaseFilter filter, int bitrate, int fps, int screenX, int screenY);
		#endregion

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




		Guid WMProfile_V80_100KBPS = new Guid("{A2E300B4-C2D4-4FC0-B5DD-ECBD948DC0DF}");
		Guid WMProfile_V80_256KBPS = new Guid("{BBC75500-33D2-4466-B86B-122B201CC9AE}");
		Guid WMProfile_V80_384KBPS = new Guid("{29B00C2B-09A9-48BD-AD09-CDAE117D1DA7}" );
		Guid WMProfile_V80_768KBPS = new Guid("{74D01102-E71A-4820-8F0D-13D2EC1E4872}");
		
		Guid IID_IWMWriterAdvanced2 = new Guid(0x962dc1ec,0xc046,0x4db8,0x9c,0xc7,0x26,0xce,0xae,0x50,0x08,0x17 );

		protected int												rotCookie = 0;
		protected  IGraphBuilder			  			      graphBuilder =null;
		protected  IStreamBufferSource 			        bufferSource=null ;
		protected IMediaControl											mediaControl=null;
		protected IMediaPosition										mediaPos=null;
		protected IStreamBufferMediaSeeking					mediaSeeking=null;
		protected IBaseFilter												powerDvdMuxer =null;
		protected IMediaEventEx											mediaEvt=null;
		protected IBaseFilter												Mpeg2VideoCodec =null;
		protected IBaseFilter												Mpeg2AudioCodec =null;
		protected IBaseFilter												fileWriterbase=null;
		protected long m_dDuration;
		protected int bitrate;
		protected int fps;
		protected Size screenSize;
		protected const int WS_CHILD			= 0x40000000;	// attributes for video window
		protected const int WS_CLIPCHILDREN	= 0x02000000;
		protected const int WS_CLIPSIBLINGS	= 0x04000000;

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

				
				Log.Write("DVR2WMV: create graph");
				Type comtype = null;
				object comobj = null;
				comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
				if( comtype == null )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:DirectX 9 not installed");
					return false;
				}
				comobj = Activator.CreateInstance( comtype );
				graphBuilder = (IGraphBuilder) comobj; comobj = null;
		
				DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph

				
				Log.Write("DVR2WMV: add streambuffersource");
				Guid clsid = Clsid.StreamBufferSource;
				Guid riid = typeof(IStreamBufferSource).GUID;
				Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
				bufferSource = (IStreamBufferSource) comObj; comObj = null;

	
				IBaseFilter filter = (IBaseFilter) bufferSource;
				graphBuilder.AddFilter(filter, "SBE SOURCE");
	
				Log.Write("DVR2WMV: load file:{0}",info.file);
				IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
				int hr = fileSource.Load(info.file, IntPtr.Zero);

				//add mpeg2 audio/video codecs
				string strVideoCodec=@"DScaler Mpeg2 Video Decoder";
				string strAudioCodec="MPEG/AC3/DTS/LPCM Audio Decoder";
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					//strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","MPEG2Dec Filter");
				}

				Log.Write("DVR2WMV: add mpeg2 video codec:{0}", strVideoCodec);
				Mpeg2VideoCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
				if( hr != 0 ) 
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:Add mpeg2 video  to filtergraph :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				Log.Write("DVR2WMV: add mpeg2 audio codec:{0}", strAudioCodec);
				Mpeg2AudioCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (Mpeg2AudioCodec==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to add mpeg2 audio codec");
					Cleanup();
					return false;
				}
				
				Log.Write("DVR2WMV: connect streambufer source->mpeg audio/video decoders");				
				//connect output #0 of streambuffer source->mpeg2 audio codec pin 1
				//connect output #1 of streambuffer source->mpeg2 video codec pin 1
				IPin pinOut0, pinOut1;
				IPin pinIn0, pinIn1;
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,0,out pinOut0);//audio
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,1,out pinOut1);//video
				if (pinOut0==null || pinOut1==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to get pins of source");
					Cleanup();
					return false;
				}

				DsUtils.GetPin(Mpeg2VideoCodec,PinDirection.Input,0,out pinIn0);//video
				DsUtils.GetPin(Mpeg2AudioCodec,PinDirection.Input,0,out pinIn1);//audio
				if (pinIn0==null || pinIn1==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to get pins of mpeg2 video/audio codec");
					Cleanup();
					return false;
				}
			
				hr=graphBuilder.Connect(pinOut0,pinIn1);
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to connect audio pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}

			
				hr=graphBuilder.Connect(pinOut1,pinIn0);
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to connect video pins :0x{0:X}",hr);
					Cleanup();
					return false;
				}

				string outputFilename=System.IO.Path.ChangeExtension(info.file,".wmv");
				if (!AddWmAsfWriter(outputFilename,quality)) return false;

				Log.Write("DVR2WMV: start pre-run");
				//hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
				//if (hr!=0)
				//	Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:to SetSyncSource :0x{0:X}",hr);
				mediaControl= graphBuilder as IMediaControl;
				mediaSeeking= bufferSource as IStreamBufferMediaSeeking;
				mediaEvt    = graphBuilder as IMediaEventEx;
				mediaPos    = graphBuilder as IMediaPosition;
				//get file duration
				long lTime=5*60*60;
				lTime*=10000000;
				long pStop=0;
				hr=mediaSeeking.SetPositions(ref lTime, SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
				if (hr==0)
				{
					long lStreamPos;
					mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
					m_dDuration=lStreamPos;
					lTime=0;
					mediaSeeking.SetPositions(ref lTime, SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
				}
				double duration=m_dDuration/10000000d;
				Log.Write("DVR2WMV: movie duration:{0}",Util.Utils.SecondsToHMSString((int)duration));				


				//hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
				//if (hr!=0)
				//	Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:to SetSyncSource :0x{0:X}",hr);

				hr=mediaControl.Run();
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to start graph :0x{0:X}",hr);
					Cleanup();
					return false;
				}
				int maxCount=20;
				while (true)
				{
					long lCurrent;
					mediaSeeking.GetCurrentPosition(out lCurrent);
					double dpos=(double)lCurrent;
					dpos/=10000000d;
					System.Threading.Thread.Sleep(100);
					if (dpos >=2.0d) break;
					maxCount--;
					if (maxCount<=0) break;
				}
				Log.Write("DVR2WMV: pre-run done");
				Log.Write("DVR2WMV: Get duration of movie");				
				mediaControl.Stop();
				FilterState state;
				mediaControl.GetState(500,out state);
				GC.Collect();GC.Collect();GC.Collect();GC.WaitForPendingFinalizers();
				Log.Write("DVR2WMV: reconnect mpeg2 video codec->ASF WM Writer");

				graphBuilder.RemoveFilter(fileWriterbase);
				if (!AddWmAsfWriter(outputFilename,quality)) return false;


				//hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
				//if (hr!=0)
				//	Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:to SetSyncSource :0x{0:X}",hr);

				Log.Write("DVR2WMV: Start transcoding");				
				hr=mediaControl.Run();
				if (hr!=0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to start graph :0x{0:X}",hr);
					Cleanup();
					return false;
				}
			} 
			catch (Exception e) 
			{  
				// TODO: Handle exceptions.
				Log.WriteFile(Log.LogType.Log,true,"unable to transcode file:{0} message:{1}", info.file,e.Message);

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
			long lCurrent;
			mediaSeeking.GetCurrentPosition(out lCurrent);
			float percent = ((float)lCurrent) / ((float)m_dDuration);
			percent*=50.0f;
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
			Log.Write("DVR2WMV: cleanup");
			if( mediaControl != null )
			{
				mediaControl.Stop();
				mediaControl = null;
			}
			mediaSeeking=null;
			mediaEvt=null;
			mediaPos=null;
			mediaControl=null;
			
			
			if ( Mpeg2AudioCodec != null )
				Marshal.ReleaseComObject( Mpeg2AudioCodec );
			Mpeg2AudioCodec=null;
			
			if ( Mpeg2VideoCodec != null )
				Marshal.ReleaseComObject( Mpeg2VideoCodec );
			Mpeg2VideoCodec=null;


			if ( bufferSource != null )
				Marshal.ReleaseComObject( bufferSource );
			bufferSource = null;

			DsUtils.RemoveFilters(graphBuilder);
			if( rotCookie != 0 )
				DsROT.RemoveGraphFromRot( ref rotCookie );

			if( graphBuilder != null )
				Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;
			GC.Collect();
			GC.Collect();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void Stop()
		{
			if (mediaControl!=null)
			{
				mediaControl.Stop();
			}
			Cleanup();
		}

		bool AddWmAsfWriter(string fileName,Quality quality)
		{
			//add asf file writer
			IPin pinOut0, pinOut1;
			IPin pinIn0, pinIn1;

			Log.Write("DVR2WMV: add WM ASF Writer to graph");				
			string monikerAsfWriter=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7C23220E-55BB-11D3-8B16-00C04FB6BD3D}";

			fileWriterbase = Marshal.BindToMoniker( monikerAsfWriter ) as IBaseFilter;
			if (fileWriterbase==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:Unable to create ASF WM Writer");
				Cleanup();
				return false;
			}
			

			int hr = graphBuilder.AddFilter( fileWriterbase , "WM ASF Writer" );
			if( hr != 0 ) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:Add ASF WM Writer to filtergraph :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			IFileSinkFilter2 fileWriterFilter = fileWriterbase as IFileSinkFilter2;
			if (fileWriterFilter ==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2XVID:FAILED:Add unable to get IFileSinkFilter for filewriter");
				Cleanup();
				return false;
			}
			hr=fileWriterFilter.SetFileName(fileName,IntPtr.Zero);
			hr=fileWriterFilter.SetMode(1);


			Log.Write("DVR2WMV: connect audio/video codecs outputs -> ASF WM Writer");
			//connect output #0 of videocodec->asf writer pin 1
			//connect output #0 of audiocodec->asf writer pin 0
			DsUtils.GetPin((IBaseFilter)Mpeg2AudioCodec,PinDirection.Output,0,out pinOut0);
			DsUtils.GetPin((IBaseFilter)Mpeg2VideoCodec,PinDirection.Output,0,out pinOut1);
			if (pinOut0==null || pinOut1==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to get outpins of video codec");
				Cleanup();
				return false;
			}

			DsUtils.GetPin(fileWriterbase,PinDirection.Input,0,out pinIn0);
			if (pinIn0==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to get pins of asf wm writer");
				Cleanup();
				return false;
			}
			
			hr=graphBuilder.Connect(pinOut0,pinIn0);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to connect audio pins :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			
			DsUtils.GetPin(fileWriterbase,PinDirection.Input,1,out pinIn1);
			if (pinIn1==null)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to get pins of asf wm writer");
				Cleanup();
				return false;
			}
			hr=graphBuilder.Connect(pinOut1,pinIn1);
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to connect video pins :0x{0:X}",hr);
				Cleanup();
				return false;
			}

			IConfigAsfWriter config= fileWriterbase as IConfigAsfWriter;
			switch (quality)
			{
				case Quality.High:
					Log.Write("DVR2WMV: set WMV High quality profile");
					hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_768KBPS);
					SetWmvProfile(fileWriterbase,0,0,0,0);
					break;
				case Quality.Medium:
					Log.Write("DVR2WMV: set WMV Medium quality profile");
					hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_384KBPS);
					SetWmvProfile(fileWriterbase,0,0,0,0);
					break;
				case Quality.Low:
					Log.Write("DVR2WMV: set WMV Low quality profile");
					hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_256KBPS);
					SetWmvProfile(fileWriterbase,0,0,0,0);
					break;
				case Quality.Portable:
					Log.Write("DVR2WMV: set WMV Portable quality profile");
					hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_100KBPS);
					SetWmvProfile(fileWriterbase,0,0,0,0);
					break;
				case Quality.Custom:
					//create new profile
					Log.Write("DVR2WMV: set WMV Custom quality profile");
					if (bitrate==768)
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_768KBPS);
					else if (bitrate==384)
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_384KBPS);
					else if (bitrate==256)
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_256KBPS);
					else
						hr=config.ConfigureFilterUsingProfileGuid(ref WMProfile_V80_100KBPS);
					SetWmvProfile(fileWriterbase,(int)bitrate,(int)fps,(int)screenSize.Width,(int)screenSize.Height);
					break;
			}
			if (hr!=0 )
			{
				Log.WriteFile(Log.LogType.Log,true,"DVR2WMV:FAILED:unable to set profile :0x{0:X}",hr);
				Cleanup();
				return false;
			}
			return true;
		}
	}
}
