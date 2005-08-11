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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices; 
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Toub.MediaCenter.Dvrms.Metadata;

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
    IPin                  m_pinStreamBufferIn0=null;
    IPin                  m_pinStreamBufferIn1=null;
    IBaseFilter           m_mpeg2Multiplexer=null;
		IBaseFilter           m_VidAnalyzer=null;
		IBaseFilter           m_streamBuffer=null;
    
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
    bool                  m_bOverlayVisible=false;
		int										m_recorderId=-1;
		
		#region Imports
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern bool DvrMsCreate(out int id, IBaseFilter streamBufferSink, [In, MarshalAs(UnmanagedType.LPWStr)]string strPath, uint dwRecordingType);
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void DvrMsStart(int id, uint startTime);
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void DvrMsStop(int id);
		#endregion

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
        0x10, 0x00,             // wBitsPerSample   = 16
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

    /// <summary>
    /// StopViewing() 
    /// If we're currently in viewing mode 
    /// then we just close the overlay window and stop the graph
    /// </summary>
    public void StopViewing()
    {
      if (m_bRendered) 
      {
        Log.WriteFile(Log.LogType.Capture,"mpeg2:StopViewing()");

        m_bOverlayVisible=false;
				if (m_videoWindow!=null)
				{
					m_videoWindow.put_Visible( DsHlp.OAFALSE );
					m_videoWindow.put_MessageDrain(IntPtr.Zero);
				}
        if (m_mediaControl!=null)
        {
          Log.WriteFile(Log.LogType.Capture,"mpeg2:StopViewing() stop mediactl");
          if (m_bRunning) m_mediaControl.Stop(); 

          m_bRunning=false;
          Log.WriteFile(Log.LogType.Capture,"mpeg2:StopViewing() stopped");
        }
        return;
      }
    }
    
		public void StartListening()
		{
			Log.WriteFile(Log.LogType.Capture,"mpeg2:StartListening() start mediactl");
			if (m_bRendered)  
			{
				if (m_mediaControl==null)
					m_mediaControl = m_graphBuilder as IMediaControl;
				if (m_mediaControl!=null && !m_bRunning)
				{
					Log.WriteFile(Log.LogType.Capture,"mpeg2:StartListening() start mediactl");
					m_mediaControl.Run(); 
					m_bRunning=true;
				}
				return;
			}
			int hr=m_graphBuilder.Render(m_pinAudioOut);
			if (hr==0) 
				Log.WriteFile(Log.LogType.Capture,"mpeg2:demux audio out connected ");
			else
				Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to render mpeg2demux audio out:0x{0:X}",hr);
			m_bRendered=true;
			if (m_mediaControl==null)
				m_mediaControl = m_graphBuilder as IMediaControl;

			if (m_mediaControl != null && !m_bRunning)
			{
				Log.WriteFile(Log.LogType.Capture,"mpeg2:StartListening() start mediactl");
				hr=m_mediaControl.Run();
				m_bRunning=true;
			}
		}
		/// <summary>
		/// StartViewing()
		/// Will start the graph and create a new overlay window to show the live-tv in
		/// </summary>
		/// <param name="windowHandle">handle to parent window</param>
    public void StartViewing(IntPtr windowHandle,VMR9Util vmr9)
    {
			// if the video window already has been created, but is hidden right now
			// then just show it and start the graph
      if (m_bRendered) 
      {
        m_bOverlayVisible=true;
        Log.WriteFile(Log.LogType.Capture,"mpeg2:StartViewing()");

				Overlay=false;

				// start graph
        if (m_mediaControl!=null && !m_bRunning)
        {
					Log.WriteFile(Log.LogType.Capture,"mpeg2:StartViewing() start mediactl");
					SetVideoWindow();
          m_mediaControl.Run(); 
          m_bRunning=true;
          Log.WriteFile(Log.LogType.Capture,"mpeg2:StartViewing() started");
        }
				Overlay=true;
        return;
      }

			// video window has not been created yet, so create it
      Log.WriteFile(Log.LogType.Capture,"mpeg2:StartViewing()");

			//render the video output. This will create the overlay render filter
      int hr=m_graphBuilder.Render(m_pinVideoOut);
      if (hr==0) 
        Log.WriteFile(Log.LogType.Capture,"mpeg2:demux video out connected ");
      else
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to render mpeg2demux video out:0x{0:X}",hr);

      
			//render the audio output pin, this will create the audio renderer which plays the audio part
			hr=m_graphBuilder.Render(m_pinAudioOut);
      if (hr==0) 
        Log.WriteFile(Log.LogType.Capture,"mpeg2:demux audio out connected ");
      else
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to render mpeg2demux audio out:0x{0:X}",hr);

			bool useOverlay=true;
			if (vmr9!=null && vmr9.IsVMR9Connected && vmr9.UseVMR9inMYTV)
				useOverlay=false;
			if (useOverlay)
			{
				// get the interfaces of the overlay window
				m_videoWindow = m_graphBuilder as IVideoWindow;
				m_basicVideo  = m_graphBuilder as IBasicVideo2;
				if (m_videoWindow!=null)
				{
					// set window message handler
					m_videoWindow.put_MessageDrain(GUIGraphicsContext.form.Handle);

					// set the properties of the overlay window
					hr = m_videoWindow.put_Owner( GUIGraphicsContext.form.Handle );
					if( hr != 0 ) 
						Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED:set Video window:0x{0:X}",hr);

					hr = m_videoWindow.put_WindowStyle( WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
					if( hr != 0 ) 
						Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED:set Video window style:0x{0:X}",hr);

					// make the overlay window visible
					//    m_bOverlayVisible=true;
					hr = m_videoWindow.put_Visible( DsHlp.OAFALSE );
					//    if( hr != 0 ) 
					//      Log.WriteFile(Log.LogType.Capture,"mpeg2:FAILED:put_Visible:0x{0:X}",hr);
				}
				else 
				{
					Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED:could not get IVideoWindow");
				}
			}
			else
			{
				if (vmr9!=null)
					vmr9.SetDeinterlaceMode();
			}

			Overlay=false;

			// start the graph so we actually get to see the video
      m_bRendered=true;
      if (m_mediaControl != null && !m_bRunning)
      {
				SetVideoWindow();
        Log.WriteFile(Log.LogType.Capture,"mpeg2:StartViewing() start mediactl");
        hr=m_mediaControl.Run();
        m_bRunning=true;
				if (hr!=0 && hr!=1)
					Log.WriteFile(Log.LogType.Capture,true,"mpeg2:StartViewing() started mediactl:0x{0:X}",hr);
      }
			Overlay=true;
    }

		void SetVideoWindow()
		{
			if (m_videoWindow==null) return;
			
			int iVideoWidth,iVideoHeight;
			int aspectX, aspectY;
			GetVideoSize( out iVideoWidth, out iVideoHeight );
			GetPreferredAspectRatio(out aspectX, out aspectY);
			if (GUIGraphicsContext.IsFullScreenVideo|| false==GUIGraphicsContext.ShowBackground)
			{
				float x=GUIGraphicsContext.OverScanLeft;
				float y=GUIGraphicsContext.OverScanTop;
				int  nw=GUIGraphicsContext.OverScanWidth;
				int  nh=GUIGraphicsContext.OverScanHeight;
				if (nw <=0 || nh <=0) return;

				System.Drawing.Rectangle rSource,rDest;
				MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
				m_geometry.ImageWidth=iVideoWidth;
				m_geometry.ImageHeight=iVideoHeight;
				m_geometry.ScreenWidth=nw;
				m_geometry.ScreenHeight=nh;
				m_geometry.ARType=GUIGraphicsContext.ARType;
				m_geometry.PixelRatio=GUIGraphicsContext.PixelRatio;
				m_geometry.GetWindow(aspectX,aspectY,out rSource, out rDest);
				rDest.X += (int)x;
				rDest.Y += (int)y;

				Log.Write("overlay: video WxH  : {0}x{1}",iVideoWidth,iVideoHeight);
				Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
				Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
				Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
				Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
				Log.Write("overlay: src        : ({0},{1})-({2},{3})",
					rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
				Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
					rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);

				SetSourcePosition( rSource.Left,rSource.Top,rSource.Width,rSource.Height);
				SetDestinationPosition(0,0,rDest.Width,rDest.Height );
				SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
			}
			else
			{
				if (iVideoWidth>0 && iVideoHeight>0)
					SetSourcePosition(0,0,iVideoWidth,iVideoHeight);
  
				if (GUIGraphicsContext.VideoWindow.Width>0 && GUIGraphicsContext.VideoWindow.Height>0)
					SetDestinationPosition(0,0,GUIGraphicsContext.VideoWindow.Width,GUIGraphicsContext.VideoWindow.Height);
  
				if (GUIGraphicsContext.VideoWindow.Width>0 && GUIGraphicsContext.VideoWindow.Height>0)
					SetWindowPosition( GUIGraphicsContext.VideoWindow.Left,GUIGraphicsContext.VideoWindow.Top,GUIGraphicsContext.VideoWindow.Width,GUIGraphicsContext.VideoWindow.Height);
			}
		}

		/// <summary>
		/// Returns the width/height of the live tv
		/// </summary>
		/// <param name="iWidth">width in pixels of the live tv</param>
		/// <param name="iHeight">height in pixels of the live tv</param>
    public void GetVideoSize(out int iWidth, out int iHeight)
    {
			iWidth=0;
			iHeight =0;
			if (m_basicVideo==null) return;
      m_basicVideo.GetVideoSize( out iWidth, out iHeight );
    }
		public void GetPreferredAspectRatio(out int aspectX, out int aspectY)
		{
			aspectX=4; aspectY=3;
			if (m_basicVideo==null) return;
			m_basicVideo.GetPreferredAspectRatio( out aspectX, out aspectY );

		}

    public void SetDestinationPosition( int x,int y, int width, int height)
    {
      if (!m_bRendered) return;
      if (m_basicVideo==null) return;
      if (width<=0 || height<=0) return;
      if (x<0 || y<0) return;
      int hr=m_basicVideo.SetDestinationPosition(x,y, width, height);
      if( hr != 0 ) 
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED:SetDestinationPosition:0x{0:X} ({1},{2})-{3},{4})",hr,x,y,width,height);
    }

    public void SetSourcePosition( int x,int y, int width, int height)
    {
      if (!m_bRendered) return;
      if (width<=0 || height<=0) return;
      if (x<0 || y<0) return;
      if (m_basicVideo==null) return;
      int hr=m_basicVideo.SetSourcePosition(x,y, width, height);
      if( hr != 0 ) 
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED:SetSourcePosition:0x{0:X} ({1},{2})-{3},{4})",hr,x,y,width,height);
    }

    public void SetWindowPosition( int x,int y, int width, int height)
    {
      if (!m_bRendered) return;
      if (width<=0 || height<=0) return;
      if (x<0 || y<0) return;
      if (m_videoWindow==null) return;
      int hr=m_videoWindow.SetWindowPosition(x,y, width, height);
      if( hr != 0 ) 
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED:SetWindowPosition:0x{0:X} ({1},{2})-{3},{4})",hr,x,y,width,height);
    }


    public void StopTimeShifting()
    {
      try
      {
        if (m_bRendered) 
        {
          int hr;
          Log.WriteFile(Log.LogType.Capture,"mpeg2:StopTimeShifting()");
          if (m_bSink!=null)
          {
            IStreamBufferSink3 sink3=m_bSink as IStreamBufferSink3;
            if (sink3!=null)
            {
              Log.WriteFile(Log.LogType.Capture,"mpeg2:unlock profile");
              hr=sink3.UnlockProfile();
              //if (hr !=0) Log.WriteFile(Log.LogType.Capture,"mpeg2:FAILED to set unlock profile:0x{0:X}",hr);
            }
          }
          if (m_mediaControl!=null)
          {
            Log.WriteFile(Log.LogType.Capture,"mpeg2:StopTimeShifting()  stop mediactl");
            if (m_bRunning) m_mediaControl.Stop(); 
            m_bRunning=false;
            Log.WriteFile(Log.LogType.Capture,"mpeg2:StopTimeShifting()  mediactl stopped");
          }
          Log.WriteFile(Log.LogType.Capture,"mpeg2:StopTimeShifting() stopped");
          return;
        }
      }
      catch(Exception ex)
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:StopTimeShifting() exception:"+ex.ToString() );
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
      Log.WriteFile(Log.LogType.Capture,"mpeg2:StartTimeshifting({0})", strFileName);
      int ipos=strFileName.LastIndexOf(@"\");
      string strDir=strFileName.Substring(0,ipos);
      if (!m_bRendered) 
      {
        //DeleteOldTimeShiftFiles(strDir);
        Log.WriteFile(Log.LogType.Capture,"mpeg2:render graph");
        /// [               ]    [              ]    [                ]
        /// [mpeg2 demux vid] -> [video analyzer] -> [#0              ]
        /// [               ]    [              ]    [  streambuffer  ]
        /// [            aud] ---------------------> [#1              ]
				
	      
        Log.WriteFile(Log.LogType.Capture,"mpeg2:render to :{0}",strFileName);
        if (m_pinVideoOut==null) return;
        if (m_pinAnalyserInput==null) return;
        if (m_pinStreamBufferIn0==null) return;
	    
        //mpeg2 demux vid->analyzer in
        Log.WriteFile(Log.LogType.Capture,"mpeg2:connect demux video out->analyzer in");
        hr=m_graphBuilder.Connect(m_pinVideoOut, m_pinAnalyserInput);
        if (hr==0) 
          Log.WriteFile(Log.LogType.Capture,"mpeg2:demux video out connected to analyzer");
        else
          Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to connect video out to analyzer:0x{0:X}",hr);
	      
        //find analyzer out pin
        m_pinAnalyserOutput=DirectShowUtil.FindPinNr(m_VidAnalyzer,PinDirection.Output,0);
        if (m_pinAnalyserOutput==null) 
        {
          Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to find analyser output pin");
          return;
        }
	  
        //analyzer out ->streambuffer in#0
        Log.WriteFile(Log.LogType.Capture,"mpeg2:analyzer out->stream buffer");
        hr=m_graphBuilder.Connect(m_pinAnalyserOutput, m_pinStreamBufferIn0);
        if (hr==0) 
          Log.WriteFile(Log.LogType.Capture,"mpeg2:connected to streambuffer");
        else
          Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to connect analyzer output to streambuffer:0x{0:X}",hr);


        //find streambuffer in#1 pin
        m_pinStreamBufferIn1=DirectShowUtil.FindPinNr(m_streamBuffer,PinDirection.Input,1);
        if (m_pinStreamBufferIn1==null) 
        {
          Log.WriteFile(Log.LogType.Capture,true,"mpeg2: FAILED to find input pin#1 of streambuffersink");
          return; 
        }
        //mpeg2 demux audio out->streambuffer in#1
        Log.WriteFile(Log.LogType.Capture,"mpeg2:demux audio out->stream buffer");
        hr=m_graphBuilder.Connect(m_pinAudioOut, m_pinStreamBufferIn1);
        if (hr==0) 
          Log.WriteFile(Log.LogType.Capture,"mpeg2:audio out connected to streambuffer");
        else
          Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to connect audio out to streambuffer:0x{0:X}",hr);

        //set mpeg2 demux as reference clock 
        (m_graphBuilder as IMediaFilter).SetSyncSource(m_mpeg2Multiplexer as IReferenceClock);

        //set filename
        m_bSink = m_streamBuffer as IStreamBufferSink;
        if (m_bSink ==null) Log.WriteFile(Log.LogType.Capture,"mpeg2:FAILED to get IStreamBufferSink interface");

				int iTimeShiftBuffer=30;
				using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					iTimeShiftBuffer= xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
					if (iTimeShiftBuffer<5) iTimeShiftBuffer=5;
				}
				iTimeShiftBuffer*=60; //in seconds
				int iFileDuration = iTimeShiftBuffer/6;

        Log.WriteFile(Log.LogType.Capture,"mpeg2:Set folder:{0} filecount 6-8, fileduration:{1} sec",strDir,iFileDuration);


        // set streambuffer backing file configuration
        m_StreamBufferConfig=new StreamBufferConfig();
        m_pConfig = (IStreamBufferConfigure) m_StreamBufferConfig;

        IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
        IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_pConfig;
        IntPtr subKey = IntPtr.Zero;

        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr=pTemp.SetHKEY(subKey);
        if (hr!=0) Log.WriteFile(Log.LogType.Capture,true,"mpeg2: FAILED to set hkey:0x{0:X}",hr);

        hr=m_pConfig.SetDirectory(strDir);
        if (hr!=0) Log.WriteFile(Log.LogType.Capture,true,"mpeg2: FAILED to set backingfile folder:0x{0:X}",hr);
	      
#if DEBUG				
        hr=m_pConfig.SetBackingFileCount(4, 6);    //4-6 files
				if (hr!=0) Log.WriteFile(Log.LogType.Capture,"mpeg2: FAILED to set backingfile count:0x{0:X}",hr);

        hr=m_pConfig.SetBackingFileDuration( 60); // 60sec * 4 files= 4 mins
        if (hr!=0) Log.WriteFile(Log.LogType.Capture,"mpeg2: FAILED to set backingfile duration:0x{0:X}",hr);
#else
        hr=m_pConfig.SetBackingFileCount(6, 8);    //6-8 files
        if (hr!=0) Log.WriteFile(Log.LogType.Capture,true,"mpeg2: FAILED to set backingfile count:0x{0:X}",hr);

        hr=m_pConfig.SetBackingFileDuration( (uint)iFileDuration); 
        if (hr!=0) Log.WriteFile(Log.LogType.Capture,true,"mpeg2: FAILED to set backingfile duration:0x{0:X}",hr);
#endif
				IStreamBufferConfigure2 streamConfig2	= m_StreamBufferConfig as IStreamBufferConfigure2;
				if (streamConfig2!=null)
					streamConfig2.SetFFTransitionRates(8,32);

      }

      // lock profile
      Log.WriteFile(Log.LogType.Capture,"mpeg2:lock profile");
      hr=m_bSink.LockProfile(strFileName);
      if (hr !=0) Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to set streambuffer filename:0x{0:X}",hr);
			m_bRendered=true;
			Overlay=false;
      if (m_mediaControl!=null)
      {
        Log.WriteFile(Log.LogType.Capture,"mpeg2:StartTimeshifting() start mediactl");
				if (!m_bRunning) 
				{
					SetVideoWindow();
					m_mediaControl.Run();
				}
        m_bRunning=true;
        Log.WriteFile(Log.LogType.Capture,"mpeg2:StartTimeshifting() started mediactl");
      }
			Overlay=true;
    }

    void Create()
    {
      Log.WriteFile(Log.LogType.Capture,"mpeg2:add new MPEG2 Demultiplexer to graph");
      try
      {
        m_MPEG2Demuxer=new MPEG2Demultiplexer();
         m_mpeg2Multiplexer = (IBaseFilter) m_MPEG2Demuxer;
      }
      catch(Exception){}
      //m_mpeg2Multiplexer = DirectShowUtil.AddFilterToGraph(m_graphBuilder,"MPEG-2 Demultiplexer");
      if (m_mpeg2Multiplexer==null) 
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to create mpeg2 demuxer");
        return ;
      }
      int hr=m_graphBuilder.AddFilter(m_mpeg2Multiplexer,"MPEG-2 Demultiplexer");
      if (hr!=0)
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to add mpeg2 demuxer to graph:0x{0:X}",hr);
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
      
			Log.WriteFile(Log.LogType.Capture,"mpeg2:create video out pin on MPEG2 demuxer");
			hr = m_demuxer.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out m_pinVideoOut);
      if (hr!=0)
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to create videout pin:0x{0:X}",hr);
        return;
      }

      Log.WriteFile(Log.LogType.Capture,"mpeg2:create audio out pin on MPEG2 demuxer");
      hr = m_demuxer.CreateOutputPin(ref mpegAudioOut, "audio", out m_pinAudioOut);
      if (hr!=0)
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to create audioout pin:0x{0:X}",hr);
        return;
      }

      //  Marshal.FreeCoTaskMem(mpegAudioOut.formatPtr);
      //  Marshal.FreeCoTaskMem(mpegVideoOut.formatPtr);


      Log.WriteFile(Log.LogType.Capture,"mpeg2:find MPEG2 demuxer input pin");
      m_pinInput=DirectShowUtil.FindPinNr(m_mpeg2Multiplexer,PinDirection.Input,0);
      if (m_pinInput!=null)
        Log.WriteFile(Log.LogType.Capture,"mpeg2:found MPEG2 demuxer input pin");
      else
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED finding MPEG2 demuxer input pin");
      
      m_bRunning=false;
      m_mediaControl = m_graphBuilder as IMediaControl;
      if (m_mediaControl==null)
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to get IMediaControl interface");
      }
    }

    public void CreateMappings()
    {
      IMPEG2StreamIdMap pStreamId;
      Log.WriteFile(Log.LogType.Capture,"mpeg2:MPEG2 demuxer map MPG stream 0xe0->video output pin");
      pStreamId = (IMPEG2StreamIdMap) m_pinVideoOut;
      int hr = pStreamId.MapStreamId(224, 1 ,0,0); // hr := pStreamId.MapStreamId( 224, MPEG2_PROGRAM_ELEMENTARY_STREAM, 0, 0 );
      if (hr!=0)
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to map stream 0xe0->video:0x{0:X}",hr);
      else
        Log.WriteFile(Log.LogType.Capture,"mpeg2:mapped MPEG2 demuxer stream 0xe0->video output ");

			Log.WriteFile(Log.LogType.Capture,"mpeg2:MPEG2 demuxer map MPG stream 0xc0->audio output pin");
      pStreamId = (IMPEG2StreamIdMap) m_pinAudioOut;
      hr = pStreamId.MapStreamId(0xC0, 1 ,0,0); // hr := pStreamId.MapStreamId( 0xC0, MPEG2_PROGRAM_ELEMENTARY_STREAM, 0, 0 );
      if (hr!=0)
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to map stream 0xc0->audio:0x{0:X}",hr);
      else
        Log.WriteFile(Log.LogType.Capture,"mpeg2:mapped MPEG2 demuxer stream 0xc0->audio output");
    }

    void CreateSBESink()
    {
      Log.WriteFile(Log.LogType.Capture,"mpeg2:add Videoanalyzer");
      try
      {

        m_VideoAnalyzer=new VideoAnalyzer();
        m_VidAnalyzer = (IBaseFilter) m_VideoAnalyzer;
      } 
      catch (Exception) {}
      if (m_VidAnalyzer ==null)
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to add Videoanalyzer (You need at least Windows XP SP1!!)");
        return;
      }
      m_graphBuilder.AddFilter(m_VidAnalyzer, "MPEG-2 Video Analyzer");
      
      m_pinAnalyserInput=DirectShowUtil.FindPinNr(m_VidAnalyzer,PinDirection.Input,0);
      if (m_pinAnalyserInput==null) Log.WriteFile(Log.LogType.Capture,"mpeg2:FAILED to find analyser input pin");

      Log.WriteFile(Log.LogType.Capture,"mpeg2:add streambuffersink");
      m_StreamBufferSink=new StreamBufferSink();
      m_streamBuffer = (IBaseFilter) m_StreamBufferSink;
      if (m_streamBuffer ==null)
      {
        Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to add streambuffer");
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
      if (hr!=0) Log.WriteFile(Log.LogType.Capture,true,"mpeg2: FAILED to set hkey:0x{0:X}",hr);
      

      m_pinStreamBufferIn0=DirectShowUtil.FindPinNr(m_streamBuffer,PinDirection.Input,0);
      if (m_pinStreamBufferIn0==null) Log.WriteFile(Log.LogType.Capture,true,"mpeg2: FAILED to find input pin#0 of streambuffersink");
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
    public void Record(Hashtable attribtutes,string strFilename, bool bContentRecording,DateTime timeProgStart, DateTime timeFirstMoment)
    {		
      //      strFilename=@"C:\media\movies\test.dvr-ms";
      Log.WriteFile(Log.LogType.Capture,"mpeg2: Record : {0} {1} {2}",strFilename,m_bRendered,bContentRecording);
			uint iRecordingType=0;
			if (bContentRecording) iRecordingType=0;
			else iRecordingType=1;										
      
			bool success=DvrMsCreate(out m_recorderId,(IBaseFilter)m_bSink,strFilename,iRecordingType);
			if (!success)
			{
				Log.WriteFile(Log.LogType.Capture,true,"mpeg2:StartRecording() FAILED to create recording");
				return ;
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
          Log.WriteFile(Log.LogType.Capture,"mpeg2:Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
            timeProgStart.Hour,timeProgStart.Minute,timeProgStart.Second,
            ts.TotalHours,ts.TotalMinutes,ts.TotalSeconds);
															
          lStartTime = (long)ts.TotalSeconds;
        }
        else Log.WriteFile(Log.LogType.Capture,"mpeg2:record entire timeshift buffer");
      
        TimeSpan tsMaxTimeBack=DateTime.Now-timeFirstMoment;
        if (lStartTime > tsMaxTimeBack.TotalSeconds )
        {
          lStartTime =(long)tsMaxTimeBack.TotalSeconds;
        }
        

        lStartTime*=-10000000L;//in reference time 
      }

/*
			foreach (MetadataItem item in attribtutes.Values)
			{
				try
				{
					if (item.Type == MetadataItemType.String)
						m_recorder.SetAttributeString(item.Name,item.Value.ToString());
					if (item.Type == MetadataItemType.Dword)
						m_recorder.SetAttributeDWORD(item.Name,UInt32.Parse(item.Value.ToString()));
				}
				catch(Exception){}
			}
*/
			DvrMsStart(m_recorderId,(uint)lStartTime);
    }

    public void StopRecording()
    {
      Log.WriteFile(Log.LogType.Capture,"mpeg2: stop recording");
			if (m_recorderId>=0) 
			{
				DvrMsStop(m_recorderId);
				m_recorderId=-1;

			}
    }

    public void CloseInterfaces()
    {
      int hr=0;
      Log.WriteFile(Log.LogType.Capture,"mpeg2: close interfaces");
      
			if (m_recorderId>=0) 
			{
				DvrMsStop(m_recorderId);
				m_recorderId=-1;

			}
			if (m_mediaControl!=null)
			{
				Log.WriteFile(Log.LogType.Capture,"mpeg2: stop mediactl");
				if (m_bRunning) m_mediaControl.Stop(); 
				m_bRunning=false;
				Log.WriteFile(Log.LogType.Capture,"mpeg2: stopped mediactl");
				m_mediaControl=null;
			}

			if (m_pinAudioOut!=null) Marshal.ReleaseComObject(m_pinAudioOut);m_pinAudioOut=null;
			if (m_pinVideoOut!=null) Marshal.ReleaseComObject(m_pinVideoOut);m_pinVideoOut=null;
			if (m_pinInput!=null) Marshal.ReleaseComObject(m_pinInput);m_pinInput=null;
			if (m_pinAnalyserInput!=null) Marshal.ReleaseComObject(m_pinAnalyserInput);m_pinAnalyserInput=null;
			if (m_pinAnalyserOutput!=null) Marshal.ReleaseComObject(m_pinAnalyserOutput);m_pinAnalyserOutput=null;
			if (m_pinStreamBufferIn0!=null) Marshal.ReleaseComObject(m_pinStreamBufferIn0);m_pinStreamBufferIn0=null;
			if (m_pinStreamBufferIn1!=null) Marshal.ReleaseComObject(m_pinStreamBufferIn1);m_pinStreamBufferIn1=null;

      if (m_streamBuffer!=null) 
      {
        m_streamBuffer.Stop();
				while ((hr=Marshal.ReleaseComObject(m_streamBuffer))>0); 
				m_streamBuffer=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_streamBuffer):{0}",hr);
      }

      
			if ( m_VideoAnalyzer!=null) 
      {
				while ((hr=Marshal.ReleaseComObject(m_VideoAnalyzer))>0);
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_VideoAnalyzer):{0}",hr);
        m_VideoAnalyzer=null;
      }

			m_StreamBufferSink=null;
      if ( m_StreamBufferConfig!=null) 
      {
				while ((hr=Marshal.ReleaseComObject(m_StreamBufferConfig))>0);
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_StreamBufferConfig):{0}",hr);
        m_StreamBufferConfig=null;
      }
      if ( m_MPEG2Demuxer!=null) 
      {
				while ((hr=Marshal.ReleaseComObject(m_MPEG2Demuxer))>0);
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_MPEG2Demuxer):{0}",hr);
        m_MPEG2Demuxer=null;
      }
      
			if (m_videoWindow!=null)
			{
				m_videoWindow.put_Visible( DsHlp.OAFALSE );
			}
      m_videoWindow = null;

			if (m_pConfig!=null) 
			{
				m_pConfig=null;
			}
			if (m_mpeg2Multiplexer!=null) 
			{
				m_mpeg2Multiplexer=null;
			}
			if (m_bSink!=null) 
			{
//				while ((hr=Marshal.ReleaseComObject(m_bSink))>0); 
				m_bSink=null;
//				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_bSink):{0}",hr);
			}
			if (m_VidAnalyzer!=null) 
			{
//				while ((hr=Marshal.ReleaseComObject(m_VidAnalyzer))>0); 
				m_VidAnalyzer=null;
//				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_VidAnalyzer):{0}",hr);
			}
			if (m_demuxer!=null) 
			{
				Marshal.ReleaseComObject(m_demuxer); 
				m_demuxer=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_demuxer):{0}",hr);
			}
			m_graphBuilder=null;
    }

    public bool Overlay
    {
      get 
      {
        return m_bOverlayVisible;
      }
      set 
      {
        if (value==m_bOverlayVisible) return;
				if (m_videoWindow==null) return;
        m_bOverlayVisible=value;
        if (!m_bOverlayVisible)
        {
          if (m_videoWindow!=null)
            m_videoWindow.put_Visible( DsHlp.OAFALSE );

        }
        else
        {
          if (m_videoWindow!=null)
            m_videoWindow.put_Visible( DsHlp.OATRUE );

        }
      }
    }
	}
}
  