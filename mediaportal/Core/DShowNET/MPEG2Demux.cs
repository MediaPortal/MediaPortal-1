using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices; 

namespace DShowNET
{
  /// <summary>
	/// 
	/// </summary>
	public class MPEG2Demux
	{
		[ComImport, Guid("6CFAD761-735D-4aa5-8AFC-AF91A7D61EBA")]
			class VideoAnalyzer {};

		[ComImport, Guid("2DB47AE5-CF39-43c2-B4D6-0CD8D90946F4")]
			class StreamBufferSink {};

		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
			class StreamBufferConfig {}

		[ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
			class MPEG2Demultiplexer {}
    
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern bool ConvertStringSidToSid(
			string pStringSid, ref IntPtr pSID);

		[DllImport("kernel32", CharSet=CharSet.Auto)]
		private static extern IntPtr  LocalFree( IntPtr hMem);

		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);
    const int WS_CHILD			= 0x40000000;	
    const int WS_CLIPCHILDREN	= 0x02000000;
    const int WS_CLIPSIBLINGS	= 0x04000000;

		IGraphBuilder         m_graphBuilder=null;
    IMediaControl					m_mediaControl=null;
    IMpeg2Demultiplexer   m_demuxer=null;
		IPin                  m_pinAudioOut=null;
		IPin                  m_pinVideoOut=null;
		IPin                  m_pinInput=null;
		IPin                  m_pinAnalyserInput=null;
		IPin                  m_pinAnalyserOutput=null;
		IBaseFilter           m_VidAnalyzer=null;
		IBaseFilter           m_streamBuffer=null;
		IPin                  m_pinStreamBufferIn0=null;
		IPin                  m_pinStreamBufferIn1=null;
		IBaseFilter           m_mpeg2Multiplexer=null;
		IStreamBufferRecordControl m_recControl=null;
		IStreamBufferSink     m_bSink=null;
		IStreamBufferConfigure m_pConfig=null;
    bool                  m_bRendered=false;
    bool                  m_bRunning=false;
    
    VideoAnalyzer         m_VideoAnalyzer=null;
    StreamBufferSink      m_StreamBufferSink=null;
    StreamBufferConfig    m_StreamBufferConfig=null;
    MPEG2Demultiplexer    m_MPEG2Demuxer=null;
    IVideoWindow          m_videoWindow=null;
    IBasicVideo2          m_basicVideo=null;
    Size                  m_FrameSize;

    /// @@@TODO : capture width/height = hardcoded to 720x576
    /// @@@TODO : capture framerate = hardcoded to 25 fps
    static byte[] Mpeg2ProgramVideo = 
                {
                0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
                0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
                0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
                0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
                0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
                0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
                0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
                0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
                0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
                0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

                //0x051736=333667-> 10000000/333667 = 29.97fps
                //0x061A80=400000-> 10000000/400000 = 25fps
                0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
                0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
                0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
                0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
                0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
                0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
                0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
                0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
                0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
                0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
                0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
                0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
                0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
                0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
                0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
                0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
                0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
                0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
                0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
                //0x56, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
                0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
                0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
                0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
                0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
                /*
                 * //  .dwSequenceHeader [1]
                0x00, 0x00, 0x01, 0xB3, 0x2D, 0x01, 0xE0, 0x24,
                0x09, 0xC4, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
                0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
                0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
                0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
                0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
                0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
                0x1A, 0x1A, 0x1A, 0x1A, 0x19, 0x1B, 0x1B, 0x1B, 
                0x1B, 0x1B, 0x1C, 0x1C, 0x1C, 0x1C, 0x1E, 0x1E, 
                0x1E, 0x1F, 0x1F, 0x21, 0x00, 0x00, 0x01, 0xB5, 
                0x14, 0x82, 0x00, 0x01, 0x00, 0x00*/
                0x00, 0x00, 0x00, 0x00
              } ;
    static byte [] MPEG1AudioFormat = 
      {
        0x50, 0x00,             // format type      = 0x0050=WAVE_FORMAT_MPEG
        0x02, 0x00,             // channels
        0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
        0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
        0x00, 0x03,             // nBlockAlign      = 0x0300 = 768
        0x00, 0x00,             // wBitsPerSample   = 0
        0x16, 0x00,             // extra size       = 0x0016 = 22 bytes
        0x02, 0x00,             // fwHeadLayer
        0x00, 0xE8,0x03, 0x00,  // dwHeadBitrate
        0x01, 0x00,             // fwHeadMode
        0x01, 0x00,             // fwHeadModeExt
        0x01, 0x00,             // wHeadEmphasis
        0x1C, 0x00,             // fwHeadFlags
        0x00, 0x00, 0x00, 0x00, // dwPTSLow
        0x00, 0x00, 0x00, 0x00  // dwPTSHigh
      } ;

		public MPEG2Demux(ref IGraphBuilder graphBuilder, Size framesize)
		{
			m_graphBuilder=graphBuilder;
      m_FrameSize=framesize;
			Create();
		}

		public IPin InputPin
		{
			get { return m_pinInput;}
		}

		public IPin AudioOutputPin
		{
			get { return m_pinAudioOut;}
		}

		public IPin VideoOutputPin
		{
			get { return m_pinVideoOut;}
		}

		public bool IsRendered
		{
			get { return m_bRendered;}
		}
    
    public void StopViewing()
    {
      if (m_bRendered) 
      {
        DirectShowUtil.DebugWrite("mpeg2:StopViewing()");

        if (m_videoWindow!=null)
          m_videoWindow.put_Visible( DsHlp.OAFALSE );
        if (m_mediaControl!=null)
        {
          DirectShowUtil.DebugWrite("mpeg2:StopViewing() stop mediactl");
          if (m_bRunning) m_mediaControl.Stop(); 
          m_bRunning=false;
          DirectShowUtil.DebugWrite("mpeg2:StopViewing() stopped");
        }
        return;
      }
    }
    
    public void StartViewing(IntPtr windowHandle)
    {
      if (m_bRendered) 
      {
        DirectShowUtil.DebugWrite("mpeg2:StartViewing()");
        if (m_videoWindow!=null)
          m_videoWindow.put_Visible( DsHlp.OATRUE );

        if (m_mediaControl!=null)
        {
          DirectShowUtil.DebugWrite("mpeg2:StartViewing() start mediactl");
          if (!m_bRunning) m_mediaControl.Run(); 
          m_bRunning=true;
          DirectShowUtil.DebugWrite("mpeg2:StartViewing() started");
        }
        return;
      }

      DirectShowUtil.DebugWrite("mpeg2:StartViewing()");
      int hr=m_graphBuilder.Render(m_pinVideoOut);
      if (hr==0) 
        DirectShowUtil.DebugWrite("mpeg2:demux video out connected ");
      else
        DirectShowUtil.DebugWrite("mpeg2:FAILED to render mpeg2demux video out:0x{0:X}",hr);

      
      hr=m_graphBuilder.Render(m_pinAudioOut);
      if (hr==0) 
        DirectShowUtil.DebugWrite("mpeg2:demux audio out connected ");
      else
        DirectShowUtil.DebugWrite("mpeg2:FAILED to render mpeg2demux audio out:0x{0:X}",hr);

      m_videoWindow = m_graphBuilder as IVideoWindow;
      m_basicVideo  = m_graphBuilder as IBasicVideo2;
      if (m_videoWindow!=null)
      {
        hr = m_videoWindow.put_Owner( windowHandle );
        if( hr != 0 ) 
          DirectShowUtil.DebugWrite("mpeg2:FAILED:set Video window:0x{0:X}",hr);

        hr = m_videoWindow.put_WindowStyle( WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
        if( hr != 0 ) 
          DirectShowUtil.DebugWrite("mpeg2:FAILED:set Video window style:0x{0:X}",hr);

        hr = m_videoWindow.put_Visible( DsHlp.OATRUE );
        if( hr != 0 ) 
          DirectShowUtil.DebugWrite("mpeg2:FAILED:put_Visible:0x{0:X}",hr);
      }
      else 
      {
        DirectShowUtil.DebugWrite("mpeg2:FAILED:could not get IVideoWindow");
      }

      m_bRendered=true;
      if (m_mediaControl != null)
      {
        DirectShowUtil.DebugWrite("mpeg2:StartViewing() start mediactl");
        if (!m_bRunning) m_mediaControl.Run();
        m_bRunning=true;
        DirectShowUtil.DebugWrite("mpeg2:StartViewing() started mediactl");
      }
    }

    public void GetVideoSize(out int iWidth, out int iHeight)
    {
      m_basicVideo.GetVideoSize( out iWidth, out iHeight );
    }

    public void SetDestinationPosition( int x,int y, int width, int height)
    {
      if (!m_bRendered) return;
      if (m_basicVideo==null) return;
      if (width<=0 || height<=0) return;
      if (x<0 || y<0) return;
      int hr=m_basicVideo.SetDestinationPosition(x,y, width, height);
      if( hr != 0 ) 
        DirectShowUtil.DebugWrite("mpeg2:FAILED:SetDestinationPosition:0x{0:X} ({1},{2})-{3},{4})",hr,x,y,width,height);
    }

    public void SetSourcePosition( int x,int y, int width, int height)
    {
      if (!m_bRendered) return;
      if (width<=0 || height<=0) return;
      if (x<0 || y<0) return;
      if (m_basicVideo==null) return;
      int hr=m_basicVideo.SetSourcePosition(x,y, width, height);
      if( hr != 0 ) 
        DirectShowUtil.DebugWrite("mpeg2:FAILED:SetSourcePosition:0x{0:X} ({1},{2})-{3},{4})",hr,x,y,width,height);
    }

    public void SetWindowPosition( int x,int y, int width, int height)
    {
      if (!m_bRendered) return;
      if (width<=0 || height<=0) return;
      if (x<0 || y<0) return;
      if (m_videoWindow==null) return;
      int hr=m_videoWindow.SetWindowPosition(x,y, width, height);
      if( hr != 0 ) 
        DirectShowUtil.DebugWrite("mpeg2:FAILED:SetWindowPosition:0x{0:X} ({1},{2})-{3},{4})",hr,x,y,width,height);
    }


		public void StopTimeShifting()
		{
      try
      {
        if (m_bRendered) 
        {
          int hr;
          DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting()");
          if (m_bSink!=null)
          {
            IStreamBufferSink3 sink3=m_bSink as IStreamBufferSink3;
            if (sink3!=null)
            {
              DirectShowUtil.DebugWrite("mpeg2:unlock profile");
              hr=sink3.UnlockProfile();
              if (hr !=0) DirectShowUtil.DebugWrite("mpeg2:FAILED to set unlock profile:0x{0:X}",hr);
            }
          }
          if (m_mediaControl!=null)
          {
            DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting()  stop mediactl");
            if (m_bRunning) m_mediaControl.Stop(); 
            m_bRunning=false;
            DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting()  mediactl stopped");
          }
          DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting() stopped");
          return;
        }
      }
      catch(Exception ex)
      {
        DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting() exception:"+ex.ToString() );
      }
		}

		public void StartTimeshifting(string strFileName)
		{
      int hr;
      if (m_StreamBufferSink==null)
      {
        CreateSBESink();
      }
      strFileName=System.IO.Path.ChangeExtension(strFileName,".tv" );
      DirectShowUtil.DebugWrite("mpeg2:StartTimeshifting({0})", strFileName);
			int ipos=strFileName.LastIndexOf(@"\");
			string strDir=strFileName.Substring(0,ipos);
			if (!m_bRendered) 
			{
        //DeleteOldTimeShiftFiles(strDir);
        DirectShowUtil.DebugWrite("mpeg2:render graph");
				/// [               ]    [              ]    [                ]
				/// [mpeg2 demux vid] -> [video analyzer] -> [#0              ]
				/// [               ]    [              ]    [  streambuffer  ]
				/// [            aud] ---------------------> [#1              ]
				
	      
				DirectShowUtil.DebugWrite("mpeg2:render to :{0}",strFileName);
				if (m_pinVideoOut==null) return;
				if (m_pinAnalyserInput==null) return;
				if (m_pinStreamBufferIn0==null) return;
	    
				//mpeg2 demux vid->analyzer in
				DirectShowUtil.DebugWrite("mpeg2:connect demux video out->analyzer in");
				hr=m_graphBuilder.Connect(m_pinVideoOut, m_pinAnalyserInput);
				if (hr==0) 
					DirectShowUtil.DebugWrite("mpeg2:demux video out connected to analyzer");
				else
					DirectShowUtil.DebugWrite("mpeg2:FAILED to connect video out to analyzer:0x{0:X}",hr);
	      
				//find analyzer out pin
				m_pinAnalyserOutput=DirectShowUtil.FindPinNr(m_VidAnalyzer,PinDirection.Output,0);
				if (m_pinAnalyserOutput==null) 
				{
					DirectShowUtil.DebugWrite("mpeg2:FAILED to find analyser output pin");
					return;
				}
	  
				//analyzer out ->streambuffer in#0
				DirectShowUtil.DebugWrite("mpeg2:analyzer out->stream buffer");
				hr=m_graphBuilder.Connect(m_pinAnalyserOutput, m_pinStreamBufferIn0);
				if (hr==0) 
					DirectShowUtil.DebugWrite("mpeg2:connected to streambuffer");
				else
					DirectShowUtil.DebugWrite("mpeg2:FAILED to connect analyzer output to streambuffer:0x{0:X}",hr);


				//find streambuffer in#1 pin
				m_pinStreamBufferIn1=DirectShowUtil.FindPinNr(m_streamBuffer,PinDirection.Input,1);
				if (m_pinStreamBufferIn1==null) 
				{
					DirectShowUtil.DebugWrite("mpeg2: FAILED to find input pin#1 of streambuffersink");
					return; 
				}
				//mpeg2 demux audio out->streambuffer in#1
				DirectShowUtil.DebugWrite("mpeg2:demux audio out->stream buffer");
				hr=m_graphBuilder.Connect(m_pinAudioOut, m_pinStreamBufferIn1);
				if (hr==0) 
					DirectShowUtil.DebugWrite("mpeg2:audio out connected to streambuffer");
				else
					DirectShowUtil.DebugWrite("mpeg2:FAILED to connect audio out to streambuffer:0x{0:X}",hr);

				//set mpeg2 demux as reference clock 
				//(m_graphBuilder as IMediaFilter).SetSyncSource(m_mpeg2Multiplexer as IReferenceClock);

				//set filename
				m_bSink = m_streamBuffer as IStreamBufferSink;
				if (m_bSink ==null) DirectShowUtil.DebugWrite("mpeg2:FAILED to get IStreamBufferSink interface");

				DirectShowUtil.DebugWrite("mpeg2:Set folder:{0} filecount 6-8, fileduration:300 sec",strDir);


				// set streambuffer backing file configuration
        m_StreamBufferConfig=new StreamBufferConfig();
				m_pConfig = (IStreamBufferConfigure) m_StreamBufferConfig;

				IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
				IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_pConfig;
				IntPtr subKey = IntPtr.Zero;

				RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
				hr=pTemp.SetHKEY(subKey);
				if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set hkey:0x{0:X}",hr);

				hr=m_pConfig.SetDirectory(strDir);
				if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile folder:0x{0:X}",hr);
	      
#if DEBUG				
        hr=m_pConfig.SetBackingFileCount(4, 6);    //4-6 files
				if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile count:0x{0:X}",hr);

        hr=m_pConfig.SetBackingFileDuration( 60); // 60sec * 4 files= 4 mins
        if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile duration:0x{0:X}",hr);
#else
        hr=m_pConfig.SetBackingFileCount(6, 8);    //6-8 files
				if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile count:0x{0:X}",hr);

        hr=m_pConfig.SetBackingFileDuration( 300); // 300sec * 6 files= 30 mins
				if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile duration:0x{0:X}",hr);
#endif
        }

      // lock profile
      DirectShowUtil.DebugWrite("mpeg2:lock profile");
			hr=m_bSink.LockProfile(strFileName);
			if (hr !=0) DirectShowUtil.DebugWrite("mpeg2:FAILED to set streambuffer filename:0x{0:X}",hr);
			m_bRendered=true;
      if (m_mediaControl!=null)
      {
        DirectShowUtil.DebugWrite("mpeg2:StartTimeshifting() start mediactl");
        if (!m_bRunning) m_mediaControl.Run();
        m_bRunning=true;
        DirectShowUtil.DebugWrite("mpeg2:StartTimeshifting() started mediactl");
      }
		}

    void Create()
    {
      DirectShowUtil.DebugWrite("mpeg2:Create()");
      DirectShowUtil.DebugWrite("mpeg2:add demux to graph");
      try
      {
        m_MPEG2Demuxer=new MPEG2Demultiplexer();
        m_mpeg2Multiplexer = (IBaseFilter) m_MPEG2Demuxer;
      }
      catch(Exception){}
      //m_mpeg2Multiplexer = DirectShowUtil.AddFilterToGraph(m_graphBuilder,"MPEG-2 Demultiplexer");
      if (m_mpeg2Multiplexer==null) 
      {
        DirectShowUtil.DebugWrite("mpeg2:FAILED to create mpeg2 demuxer");
        return ;
      }
      int hr=m_graphBuilder.AddFilter(m_mpeg2Multiplexer,"MPEG-2 Demultiplexer");
      if (hr!=0)
      {
        DirectShowUtil.DebugWrite("mpeg2:FAILED to add mpeg2 demuxer to graph:0x{0:X}",hr);
        return ;
      }

      m_demuxer = m_mpeg2Multiplexer as IMpeg2Demultiplexer ;
      if (m_demuxer==null) return ;

      AMMediaType mpegVideoOut = new AMMediaType();
      mpegVideoOut.majorType = MediaType.Video;
      mpegVideoOut.subType = MediaSubType.MPEG2_Video;

      mpegVideoOut.unkPtr = IntPtr.Zero;
      mpegVideoOut.sampleSize = 0;
      mpegVideoOut.temporalCompression = false;
      mpegVideoOut.fixedSizeSamples = true;

      byte iWidthLo  = (byte)(m_FrameSize.Width & 0xff);
      byte iWidthHi  = (byte)(m_FrameSize.Width >> 8);
      
      byte iHeightLo = (byte)(m_FrameSize.Height & 0xff);
      byte iHeightHi = (byte)(m_FrameSize.Height >> 8);

      Mpeg2ProgramVideo[0x08] = iWidthLo; Mpeg2ProgramVideo[0x09] = iWidthHi;
      Mpeg2ProgramVideo[0x18] = iWidthLo; Mpeg2ProgramVideo[0x19] = iWidthHi;
      Mpeg2ProgramVideo[0x48] = iWidthLo; Mpeg2ProgramVideo[0x49] = iWidthHi;

      Mpeg2ProgramVideo[0x0C] = iHeightLo; Mpeg2ProgramVideo[0x0D] = iHeightHi;
      Mpeg2ProgramVideo[0x1C] = iHeightLo; Mpeg2ProgramVideo[0x1D] = iHeightHi;
      Mpeg2ProgramVideo[0x4C] = iHeightLo; Mpeg2ProgramVideo[0x4D] = iHeightHi;

      if (m_FrameSize.Height==480)
      {
        //ntsc 
        Mpeg2ProgramVideo[0x28] = 0x36; Mpeg2ProgramVideo[0x29] = 0x17; Mpeg2ProgramVideo[0x2a] = 0x05;
      }
      else
      {
        //pal
        Mpeg2ProgramVideo[0x28] = 0x80; Mpeg2ProgramVideo[0x29] = 0x1A; Mpeg2ProgramVideo[0x2a] = 0x06;
      }

      mpegVideoOut.formatType = FormatType.Mpeg2Video;
      mpegVideoOut.formatSize = Mpeg2ProgramVideo.GetLength(0) ;
      mpegVideoOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem( mpegVideoOut.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo,0,mpegVideoOut.formatPtr,mpegVideoOut.formatSize) ;

      AMMediaType mpegAudioOut = new AMMediaType();
      mpegAudioOut.majorType = MediaType.Audio;
      mpegAudioOut.subType = MediaSubType.MPEG2_Audio;
      mpegAudioOut.sampleSize = 0;
      mpegAudioOut.temporalCompression = false;
      mpegAudioOut.fixedSizeSamples = true;
      mpegAudioOut.unkPtr = IntPtr.Zero;
      mpegAudioOut.formatType = FormatType.WaveEx;
      mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
      mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat,0,mpegAudioOut.formatPtr,mpegAudioOut.formatSize) ;
      
      hr = m_demuxer.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out m_pinVideoOut);
      if (hr!=0)
      {
        DirectShowUtil.DebugWrite("mpeg2:FAILED to create videout pin:0x{0:X}",hr);
        return;
      }

      DirectShowUtil.DebugWrite("mpeg2:create audio out pin");
      hr = m_demuxer.CreateOutputPin(ref mpegAudioOut, "audio", out m_pinAudioOut);
      if (hr!=0)
      {
        DirectShowUtil.DebugWrite("mpeg2:FAILED to create audioout pin:0x{0:X}",hr);
        return;
      }

      //  Marshal.FreeCoTaskMem(mpegAudioOut.formatPtr);
      //  Marshal.FreeCoTaskMem(mpegVideoOut.formatPtr);


      DirectShowUtil.DebugWrite("mpeg2:find input pin");
      m_pinInput=DirectShowUtil.FindPinNr(m_mpeg2Multiplexer,PinDirection.Input,0);
      if (m_pinInput!=null)
        DirectShowUtil.DebugWrite("mpeg2:found input pin");
      else
        DirectShowUtil.DebugWrite("mpeg2:FAILED finding input pin");
      
      m_bRunning=false;
      m_mediaControl = m_graphBuilder as IMediaControl;
      if (m_mediaControl==null)
      {
        DirectShowUtil.DebugWrite("mpeg2:FAILED to get IMediaControl interface");
      }
    }

    public void CreateMappings()
    {
      IMPEG2StreamIdMap pStreamId;
      DirectShowUtil.DebugWrite("mpeg2:map stream 0xe0->video");
      pStreamId = (IMPEG2StreamIdMap) m_pinVideoOut;
      int hr = pStreamId.MapStreamId(224, 1 ,0,0); // hr := pStreamId.MapStreamId( 224, MPEG2_PROGRAM_ELEMENTARY_STREAM, 0, 0 );
      if (hr!=0)
        DirectShowUtil.DebugWrite("mpeg2:FAILED to map stream 0xe0->video:0x{0:X}",hr);
      else
        DirectShowUtil.DebugWrite("mpeg2:stream 0xe0->video mapped");

      pStreamId = (IMPEG2StreamIdMap) m_pinAudioOut;
      hr = pStreamId.MapStreamId(0xC0, 1 ,0,0); // hr := pStreamId.MapStreamId( 0xC0, MPEG2_PROGRAM_ELEMENTARY_STREAM, 0, 0 );
      if (hr!=0)
        DirectShowUtil.DebugWrite("mpeg2:FAILED to map stream 0xc0->audio:0x{0:X}",hr);
      else
        DirectShowUtil.DebugWrite("mpeg2:stream 0xc0->audio mapped");
    }

    void CreateSBESink()
    {
			DirectShowUtil.DebugWrite("mpeg2:add Videoanalyzer");
			try
      {

        m_VideoAnalyzer=new VideoAnalyzer();
				m_VidAnalyzer = (IBaseFilter) m_VideoAnalyzer;
			} 
			catch (Exception) {}
			if (m_VidAnalyzer ==null)
			{
				DirectShowUtil.DebugWrite("mpeg2:FAILED to add Videoanalyzer (You need at least Windows XP SP1!!)");
				return;
			}
			m_graphBuilder.AddFilter(m_VidAnalyzer, "MPEG-2 Video Analyzer");
      
			m_pinAnalyserInput=DirectShowUtil.FindPinNr(m_VidAnalyzer,PinDirection.Input,0);
			if (m_pinAnalyserInput==null) DirectShowUtil.DebugWrite("mpeg2:FAILED to find analyser input pin");

      DirectShowUtil.DebugWrite("mpeg2:add streambuffersink");
      m_StreamBufferSink=new StreamBufferSink();
			m_streamBuffer = (IBaseFilter) m_StreamBufferSink;
			if (m_streamBuffer ==null)
			{
				DirectShowUtil.DebugWrite("mpeg2:FAILED to add streambuffer");
				return;
			}

			m_graphBuilder.AddFilter(m_streamBuffer, "SBE SINK");

			IntPtr subKey = IntPtr.Zero;
      
			IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_streamBuffer;
			//  IntPtr[] sids = new IntPtr[] {pSid};
			//  int result = pConfig.SetSIDs(1, sids);
			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			int hr=pConfig.SetHKEY(subKey);
			if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set hkey:0x{0:X}",hr);
      

			m_pinStreamBufferIn0=DirectShowUtil.FindPinNr(m_streamBuffer,PinDirection.Input,0);
			if (m_pinStreamBufferIn0==null) DirectShowUtil.DebugWrite("mpeg2: FAILED to find input pin#0 of streambuffersink");
		}

		public IBaseFilter BaseFilter
		{
			get { return (IBaseFilter)m_demuxer;}
		}

		/// <summary>
		/// This method will start recording and will write all data to strFileName
		/// </summary>
		/// <param name="strFilename">file where recording should b saved</param>
		/// <param name="bContentRecording">
		/// when true it will make a content recording. A content recording writes the data to a new permanent file. 
		/// when false it will make a reference recording. A reference recording creates a stub file that refers to the existing backing files, which are made permanent. Create a reference recording if you want to save data that has already been captured.</param>
		public void Record(string strFilename, bool bContentRecording,DateTime timeProgStart, DateTime timeFirstMoment)
		{		
			//      strFilename=@"C:\media\movies\test.dvr-ms";
			DirectShowUtil.DebugWrite("mpeg2: Record : {0} {1} {2}",strFilename,m_bRendered,bContentRecording);
			IntPtr recorderObj;
			if (m_bSink==null) 
			{
				DirectShowUtil.DebugWrite("mpeg2: CreateRecorder : no sink!");
				return;
			}
			uint iRecordingType=0;
			if (bContentRecording) iRecordingType=0;
			else iRecordingType=1;										
					 
			int hr=m_bSink.CreateRecorder(strFilename, iRecordingType, out recorderObj);
			if (hr!=0) 
			{
				DirectShowUtil.DebugWrite("mpeg2: CreateRecorder FAILED:0x{0:x}",hr );
				return;
			}
			object objRecord=Marshal.GetObjectForIUnknown(recorderObj);
			if (objRecord==null) 
			{
				DirectShowUtil.DebugWrite("mpeg2: FAILED getting Inknown of recorder");
				return;
			}
      
      Marshal.Release(recorderObj);

			m_recControl=objRecord as IStreamBufferRecordControl;
			if (m_recControl==null) 
			{
				DirectShowUtil.DebugWrite("mpeg2: FAILED getting IStreamBufferRecordControl");
				return;
			}

			long lStartTime=0;

			// if we're making a reference recording
			// then record all content from the past as well
			if (!bContentRecording)
			{
				// so set the startttime...
				uint uiSecondsPerFile;
				uint uiMinFiles, uiMaxFiles;
				m_pConfig.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
				m_pConfig.GetBackingFileDuration(out uiSecondsPerFile);
				lStartTime = uiSecondsPerFile;
				lStartTime*= (long)uiMaxFiles;

				// if start of program is given, then use that as our starttime
				if (timeProgStart.Year>2000)
				{
					TimeSpan ts = DateTime.Now-timeProgStart;
					DirectShowUtil.DebugWrite("mpeg2:Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
																		timeProgStart.Hour,timeProgStart.Minute,timeProgStart.Second,
																		ts.TotalHours,ts.TotalMinutes,ts.TotalSeconds);
															
					lStartTime = (long)ts.TotalSeconds;
				}
				else DirectShowUtil.DebugWrite("mpeg2:record entire timeshift buffer");
      
        TimeSpan tsMaxTimeBack=DateTime.Now-timeFirstMoment;
        if (lStartTime > tsMaxTimeBack.TotalSeconds )
        {
          lStartTime =(long)tsMaxTimeBack.TotalSeconds;
        }
        

        lStartTime*=-10000000L;//in reference time 
			}

			hr=m_recControl.Start(ref lStartTime);
			if (hr!=0) 
			{
        //could not start recording...
				DirectShowUtil.DebugWrite("mpeg2: FAILED to start recording:0x{0:x}",hr );
        if (lStartTime!=0)
        {
          // try recording from livepoint instead from the past
          lStartTime=0;
          hr=m_recControl.Start(ref lStartTime);
          if (hr!=0)
          {
            //still fails
            DirectShowUtil.DebugWrite("mpeg2: FAILED to start recording now:0x{0:x}",hr );
            return;
          }
          else
          {
            //that worked!
            DirectShowUtil.DebugWrite("mpeg2: FAILED to record now succeeded");
          }
        }
        else
        {
          //record fails
          return;
        }
			}
			lStartTime/=-10000000L;
			long iHour=lStartTime/3600;
			lStartTime -= (iHour*3600);
			long iMin =lStartTime/60;
			lStartTime -= (iMin*60);
			long iSec =lStartTime;
			DirectShowUtil.DebugWrite("mpeg2: recording started from {0:00}:{1:00}:{2:00} ago",iHour,iMin,iSec);

		}

		public void StopRecording()
		{
			DirectShowUtil.DebugWrite("mpeg2: stop recording");
			if (m_recControl==null) return;
			int hr=m_recControl.Stop(0);
			if (hr!=0) 
			{
				DirectShowUtil.DebugWrite("mpeg2: FAILED to stop recording:0x{0:x}",hr );
				return;
			}
			if (m_recControl!=null) Marshal.ReleaseComObject(m_recControl); m_recControl=null;

		}

    public void CloseInterfaces()
    {
      
      DirectShowUtil.DebugWrite("mpeg2: close interfaces");
      
      if (m_recControl!=null) 
      {
        m_recControl.Stop(0);
        Marshal.ReleaseComObject(m_recControl); m_recControl=null;
      }
      if (m_streamBuffer!=null) 
      {
        m_streamBuffer.Stop();
        Marshal.ReleaseComObject(m_streamBuffer); m_streamBuffer=null;
      }
      if (m_mediaControl!=null)
      {
        DirectShowUtil.DebugWrite("mpeg2: stop mediactl");
        if (m_bRunning) m_mediaControl.Stop(); 
        m_bRunning=false;
        DirectShowUtil.DebugWrite("mpeg2: stopped mediactl");
        m_mediaControl=null;
      }

      
      if ( m_VideoAnalyzer!=null) 
      {
        Marshal.ReleaseComObject(m_VideoAnalyzer);
        m_VideoAnalyzer=null;
      }
      if ( m_StreamBufferSink!=null) 
      {
        Marshal.ReleaseComObject(m_StreamBufferSink);
        m_StreamBufferSink=null;
      }
      if ( m_StreamBufferConfig!=null) 
      {
        Marshal.ReleaseComObject(m_StreamBufferConfig);
        m_StreamBufferConfig=null;
      }
      if ( m_MPEG2Demuxer!=null) 
      {
        Marshal.ReleaseComObject(m_MPEG2Demuxer);
        m_MPEG2Demuxer=null;
      }
      
      m_videoWindow = null;

      if (m_pConfig!=null) Marshal.ReleaseComObject(m_pConfig); m_pConfig=null;
      if (m_mpeg2Multiplexer!=null) Marshal.ReleaseComObject(m_mpeg2Multiplexer); m_mpeg2Multiplexer=null;
      if (m_pinStreamBufferIn1!=null) Marshal.ReleaseComObject(m_pinStreamBufferIn1); m_pinStreamBufferIn1=null;
      if (m_pinStreamBufferIn0!=null) Marshal.ReleaseComObject(m_pinStreamBufferIn0); m_pinStreamBufferIn0=null;
      if (m_bSink!=null) Marshal.ReleaseComObject(m_bSink); m_bSink=null;
			if (m_pinAnalyserOutput!=null) Marshal.ReleaseComObject(m_pinAnalyserOutput); m_pinAnalyserOutput=null;
			if (m_pinAnalyserInput!=null) Marshal.ReleaseComObject(m_pinAnalyserInput); m_pinAnalyserInput=null;
			if (m_VidAnalyzer!=null) Marshal.ReleaseComObject(m_VidAnalyzer); m_VidAnalyzer=null;

			if (m_pinInput!=null) Marshal.ReleaseComObject(m_pinInput); m_pinInput=null;
			if (m_pinVideoOut!=null) Marshal.ReleaseComObject(m_pinVideoOut); m_pinVideoOut=null;
			if (m_pinAudioOut!=null) Marshal.ReleaseComObject(m_pinAudioOut); m_pinAudioOut=null;
			if (m_demuxer!=null) Marshal.ReleaseComObject(m_demuxer); m_demuxer=null;
      m_graphBuilder=null;

		}


	}
}
  