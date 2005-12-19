/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
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
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;



using MediaPortal.GUI.Library;
using DShowNET;
namespace MediaPortal.Player 
{
  public class RecordingPlayer : IPlayer
  {
    public enum PlayState
    {
      Init,
      Playing,
      Paused
    }
    int 											m_iPositionX=0;
    int 											m_iPositionY=0;
    int 											m_iWidth=200;
    int 											m_iHeight=100;
    int                       m_iVideoWidth=100;
    int                       m_iVideoHeight=100;
    string                    m_strCurrentFile="";
    bool											m_bUpdateNeeded=false;
    MediaPortal.GUI.Library.Geometry.Type             m_ar=MediaPortal.GUI.Library.Geometry.Type.Normal;
    bool											m_bFullScreen=true;
    PlayState								  m_state=PlayState.Init;
    int                       m_iVolume=100;
    double                            m_dCurrentPos;
    double                            m_dDuration;
    private IGraphBuilder			graphBuilder;
    bool          m_bStarted=false;
    private int		rotCookie = 0;
    int                        m_iSpeed=1;
    /// <summary> control interface. </summary>
    private IMediaControl			mediaCtrl;

    /// <summary> graph event interface. </summary>
    private IMediaEventEx			mediaEvt;

    /// <summary> seek interface for positioning in stream. </summary>
    private IMediaSeeking			mediaSeek;
    /// <summary> seek interface to set position in stream. </summary>
    private IMediaPosition			mediaPos;
    /// <summary> video preview window interface. </summary>
    private IVideoWindow			videoWin;
    /// <summary> interface to get information and control video. </summary>
    private IBasicVideo2			basicVideo;
    /// <summary> interface to single-step video. </summary>
    private IVideoFrameStep			videoStep;

    /// <summary> audio interface used to control volume. </summary>
    private IBasicAudio				basicAudio;
    private const int WM_GRAPHNOTIFY	= 0x00008001;	// message from graph

    private const int WS_CHILD			= 0x40000000;	// attributes for video window
    private const int WS_CLIPCHILDREN	= 0x02000000;
    private const int WS_CLIPSIBLINGS	= 0x04000000;
    bool        m_bVisible=false;
    public RecordingPlayer()
    {
    }


    public override bool Play(string strFile)
    {
      m_bVisible=false;
      m_iVolume=100;
      m_state=PlayState.Init;
      m_strCurrentFile=strFile;
      m_iSpeed=1; 
      m_dCurrentPos=0d;
      m_dDuration=0d;
      
      try
      {
        if (!System.IO.File.Exists(strFile)) return false;
        using ( System.IO.FileStream stream= new System.IO.FileStream(strFile,System.IO.FileMode.Open,System.IO.FileAccess.Read,System.IO.FileShare.ReadWrite,1024,false))
        {
          long lLength=stream.Length;
          if (lLength< 1024*1024L) return false;
        }
      } 
      catch (Exception)
      {
      }
      m_bUpdateNeeded=true;
      Log.Write("RecordingPlayer:play {0}", strFile);
      //lock ( typeof(RecordingPlayer) )
      {
        GC.Collect();
        CloseInterfaces();
        GC.Collect();
        m_bStarted=false;
        if( ! GetInterfaces() )
        {
          m_strCurrentFile="";
          return false;
        }
        int hr = mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
        if (hr < 0)
        {
          m_strCurrentFile="";
          CloseInterfaces();
          return false;
        }
        videoWin.put_Visible( DsHlp.OAFALSE );
        videoWin.put_Owner( GUIGraphicsContext.ActiveForm );
        videoWin.put_WindowStyle( WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN );
        hr = basicVideo.GetVideoSize( out m_iVideoWidth, out m_iVideoHeight );
        if (hr < 0)
        {
          m_strCurrentFile="";
          CloseInterfaces();
          return false;
        }
        GetFrameStepInterface();

        /*GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target, Color.Black, 1.0f, 0);
        try
        {
          // Show the frame on the primary surface.
          GUIGraphicsContext.DX9Device.Present();
        }
        catch(DeviceLostException)
        {
        }*/

        DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph
        hr = mediaCtrl.Run();
        if (hr < 0)
        {
          m_strCurrentFile="";
          CloseInterfaces();
          return false;
        }
        GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED,0,0,0,0,0,null);
        msg.Label=strFile;
        GUIWindowManager.SendThreadMessage(msg);

        Log.Write("RecordingPlayer:set window...");
        m_iPositionX=GUIGraphicsContext.VideoWindow.X;
        m_iPositionY=GUIGraphicsContext.VideoWindow.Y;
        m_iWidth    =GUIGraphicsContext.VideoWindow.Width;
        m_iHeight   =GUIGraphicsContext.VideoWindow.Height;
        m_ar        =GUIGraphicsContext.ARType;
        m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
        m_bUpdateNeeded=true;
        SetVideoWindow();
        m_state=PlayState.Playing;
        Log.Write("RecordingPlayer:Playing...");
      }
      return true;
    }

    public override void SetVideoWindow()
    {
      if (videoWin==null) return;
      if (GUIGraphicsContext.IsFullScreenVideo!= m_bFullScreen)
      {
        m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
        m_bUpdateNeeded=true;
      }

      if (!m_bUpdateNeeded) return;
      m_bUpdateNeeded=false;
      m_bStarted=true;
      float x=m_iPositionX;
      float y=m_iPositionY;
      
      int nw=m_iWidth;
      int nh=m_iHeight;
      if (nw > GUIGraphicsContext.OverScanWidth)
        nw=GUIGraphicsContext.OverScanWidth;
      if (nh > GUIGraphicsContext.OverScanHeight)
        nh=GUIGraphicsContext.OverScanHeight;
      //lock ( typeof(RecordingPlayer) )
      {
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          x=m_iPositionX=GUIGraphicsContext.OverScanLeft;
          y=m_iPositionY=GUIGraphicsContext.OverScanTop;
          nw=m_iWidth=GUIGraphicsContext.OverScanWidth;
          nh=m_iHeight=GUIGraphicsContext.OverScanHeight;
        }
        if (nw <=0 || nh <=0) return;

        System.Drawing.Rectangle rSource,rDest;
        MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
        m_geometry.ImageWidth=m_iVideoWidth;
        m_geometry.ImageHeight=m_iVideoHeight;
        m_geometry.ScreenWidth=nw;
        m_geometry.ScreenHeight=nh;
        m_geometry.ARType=GUIGraphicsContext.ARType;
        m_geometry.PixelRatio=GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;
        

        Log.Write("RecordingPlayer:Window ({0},{1})-({2},{3}) - ({4},{5})-({6},{7})", 
          rSource.X,rSource.Y, rSource.Right, rSource.Bottom,
          rDest.X, rDest.Y, rDest.Right, rDest.Bottom);


        basicVideo.SetSourcePosition(rSource.Left,rSource.Top,rSource.Width,rSource.Height);
        basicVideo.SetDestinationPosition(0,0,rDest.Width,rDest.Height);
        videoWin.SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
   

        
      }
    }

    void MovieEnded(bool bManual)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      if (!bManual) 
      {
        Log.Write("re-run#1");
        SeekAsolutePercentage(98);
        Log.Write("re-run#2");
        mediaCtrl.Run();
        Log.Write("re-run#3");
        //Pause();
        return;
      }
      else
      {
          Log.Write("RecordingPlayer:ended {0}", m_strCurrentFile);
      }
    }


    public override void Process()
    {
      if ( !Playing) return;
      if ( !m_bStarted) return;


      mediaPos.get_CurrentPosition(out m_dCurrentPos);

      long lDuration;
      mediaSeek.GetDuration(out lDuration);
      m_dDuration=(double)lDuration;


      if (GUIGraphicsContext.VideoWindow.Width<10&& GUIGraphicsContext.IsFullScreenVideo==false)
      {
        if (m_bVisible)
        {
          m_bVisible=false;
          videoWin.put_Visible( DsHlp.OAFALSE );
        }
      }
      else if (!m_bVisible)
      {
        m_bVisible=true;
        videoWin.put_Visible( DsHlp.OATRUE );
      }      
    }


    

    public override int PositionX
    {
      get { return m_iPositionX;}
      set 
      { 
        if (value != m_iPositionX)
        {
          m_iPositionX=value;
          m_bUpdateNeeded=true;
        }
      }
    }

    public override int PositionY
    {
      get { return m_iPositionY;}
      set 
      {
        if (value != m_iPositionY)
        {
          m_iPositionY=value;
          m_bUpdateNeeded=true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return m_iWidth;}
      set 
      {
        if (value !=m_iWidth)
        {
          m_iWidth=value;
          m_bUpdateNeeded=true;
        }
      }
    }
    public override int RenderHeight
    {
      get { return m_iHeight;}
      set 
      {
        if (value != m_iHeight)
        {
          m_iHeight=value;
          m_bUpdateNeeded=true;
        }
      }
    }

    public override double Duration
    {
      get 
      {
        return m_dDuration;
      }
    }

    public override double CurrentPosition
    {
      get 
      {
        return m_dCurrentPos;
      }
    }

    public override bool FullScreen
    {
      get 
      { 
        return GUIGraphicsContext.IsFullScreenVideo;
      }
      set
      {
        if (value != m_bFullScreen )
        {
          m_bFullScreen=value;
          m_bUpdateNeeded=true;          
        }
      }
    }
    public override int Width
    {
      get 
      { 
        return m_iVideoWidth;
      }
    }

    public override int Height
    {
      get 
      {
        return m_iVideoHeight;
      }
    }

    public override void Pause()
    {
      if (m_state==PlayState.Paused) 
      {
        mediaCtrl.Run();
        m_state=PlayState.Playing;
      }
      else if (m_state==PlayState.Playing) 
      {
        m_state=PlayState.Paused;
        mediaCtrl.Pause();
      }
    }

    public override bool Paused
    {
      get 
      {
        return (m_state==PlayState.Paused);
      }
    }

    public override bool Playing
    {
      get 
      { 
        return (m_state==PlayState.Playing||m_state==PlayState.Paused);
      }
    }

    public override bool Stopped
    {
      get 
      { 
        return (m_state==PlayState.Init);
      }
    }

    public override string CurrentFile
    {
      get { return m_strCurrentFile;}
    }

    public override void Stop()
    {
      //lock (this)
      {
        if (m_state!=PlayState.Init)
        {
          Log.Write("RecordingPlayer9:stop");

          MovieEnded(true);        
          CloseInterfaces();
          Log.Write("RecordingPlayer9:stopped");
        }
      }
    }
    public override int Speed
    {
      get 
      { 
        //lock (this)
        {
          return m_iSpeed;
        }
      }
      set 
      {
        if (m_state!=PlayState.Init)
        {
          if (mediaPos!=null)
          {
            m_iSpeed=value;
            mediaPos.put_Rate( (double) value);
          }
        }
      }
    }

    public override int Volume
    {
      get { return m_iVolume;}
      set 
      {
        if (m_iVolume!=value)
        {
          m_iVolume=value;
          if (m_state!=PlayState.Init)
          {
            if (basicAudio!=null)
            {
              // Divide by 100 to get equivalent decibel value. For example, –10,000 is –100 dB. 
              float fPercent=(float)m_iVolume/100.0f;
              int iVolume=(int)(5000.0f*fPercent);
              basicAudio.put_Volume( (iVolume-5000));
            }
          }
        }
      }
    }

    public override MediaPortal.GUI.Library.Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType;}
      set 
      {
        if (m_ar != value)
        {
          m_ar=value;
          m_bUpdateNeeded=true;
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          
          double dCurTime=m_dCurrentPos;
          
          dTime=dCurTime+dTime;
          if (dTime<0.0d) dTime=0.0d;
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          if (dTime<0.0d) dTime=0.0d;
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          double dCurrentPos=m_dCurrentPos;
          double dDuration=Duration;

          double fCurPercent=(dCurrentPos/Duration)*100.0d;
          double fOnePercent=Duration/100.0d;
          fCurPercent=fCurPercent + (double)iPercentage;
          fCurPercent*=fOnePercent;
          if (fCurPercent<0.0d) fCurPercent=0.0d;
          if (fCurPercent<Duration)
          {
            mediaPos.put_CurrentPosition(fCurPercent);
          }
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          if (iPercentage<0) iPercentage=0;
          if (iPercentage>=100) iPercentage=100;
          double fPercent=Duration/100.0f;
          fPercent*=(double)iPercentage;
          mediaPos.put_CurrentPosition(fPercent);
        }
      }
    }

    
    public override bool HasVideo
    {
      get { return true;}
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    bool GetInterfaces()
    {
      Type comtype = null;
      object comobj = null;
      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.Write("RecordingPlayer:DirectX 9 not installed");
          return false;
        }
        comobj = Activator.CreateInstance( comtype );
        graphBuilder = (IGraphBuilder) comobj; comobj = null;
			
        Log.Write("RecordingPlayer:add mp reader filter");
        IBaseFilter bfileReader= DirectShowUtil.AddFilterToGraph(graphBuilder,"MP Reader");
        if (bfileReader==null) 
          Marshal.ThrowExceptionForHR( -1 );

        Log.Write("RecordingPlayer:get IFileSourceFilter interface");
        IFileSourceFilter source=(IFileSourceFilter)bfileReader;
        if (source==null)
        {
          Marshal.ThrowExceptionForHR( -1 );
        }
        Log.Write("RecordingPlayer:set source");
        int hr=source.Load(m_strCurrentFile,IntPtr.Zero);
        if (hr<0)
          Marshal.ThrowExceptionForHR( -1 );

        DirectShowUtil.RenderOutputPins(graphBuilder,bfileReader);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        mediaSeek	= (IMediaSeeking)  graphBuilder;
        mediaPos	= (IMediaPosition) graphBuilder;

        videoWin	= graphBuilder as IVideoWindow;
        basicVideo	= graphBuilder as IBasicVideo2;
        basicAudio	= graphBuilder as IBasicAudio;
        DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);
        DirectShowUtil.EnableDeInterlace(graphBuilder);

        return true;
      }
      catch( Exception  ex)
      {
        Log.Write("RecordingPlayer:VideoPlayer:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
        return false;
      }
      finally
      {
        if( comobj != null )
          Marshal.ReleaseComObject( comobj ); comobj = null;
      }
    }



    /// <summary> try to get the step interfaces. </summary>
    bool GetFrameStepInterface()
    {
      videoStep = graphBuilder as IVideoFrameStep;
      if( videoStep == null )
        return false;

      // Check if this decoder can step
      int hr = videoStep.CanStep( 0, null );
      if( hr != 0 )
      {
        videoStep = null;
        return false;
      }
      return true;
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    void CloseInterfaces()
    {
      int hr;
      Log.Write("RecordingPlayer:cleanup DShow graph");
      try 
      {
        if( rotCookie != 0 )
          DsROT.RemoveGraphFromRot( ref rotCookie );

        if( mediaCtrl != null )
        {
          hr = mediaCtrl.Stop();
          mediaCtrl = null;
        }

        m_state = PlayState.Init;

        if( mediaEvt != null )
        {
          hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
          mediaEvt = null;
        }

        if( videoWin != null )
        {
          m_bVisible=false;
          hr = videoWin.put_Visible( DsHlp.OAFALSE );
          hr = videoWin.put_Owner( IntPtr.Zero );
          videoWin = null;
        }

        mediaSeek	= null;
        mediaPos	= null;
        basicVideo	= null;
        videoStep	= null;
        basicAudio	= null;

        if( graphBuilder != null )
          Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;
        GUIGraphicsContext.form.Invalidate(true);

        m_state = PlayState.Init;
        GC.Collect();
      }
      catch( Exception ex)
      {
        Log.Write("RecordingPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }
    }
    public override void WndProc( ref Message m )
    {
      if( m.Msg == WM_GRAPHNOTIFY )
      {
        if( mediaEvt != null )
          OnGraphNotify();
        return;
      }
      base.WndProc( ref m );
    }

    void OnGraphNotify()
    {
      int p1, p2, hr = 0;
      DsEvCode code;
      do
      {
        hr = mediaEvt.GetEvent( out code, out p1, out p2, 0 );
        if( hr < 0 )
          break;
        hr = mediaEvt.FreeEventParams( code, p1, p2 );
        if( code == DsEvCode.Complete || code== DsEvCode.ErrorAbort)
        {
          Log.Write("RecordingPlayer9:on notify complete");
          MovieEnded(false);
          //CloseInterfaces();
          //m_state=PlayState.Init;
          //GUIGraphicsContext.IsPlaying=false;

        }
      }
      while( hr == 0 );
    }


    public override bool IsTV
    {
      get 
      {
        return true;
      }
    }    
    #region IDisposable Members

    public override void Release()
    {
      CloseInterfaces();
    }
    #endregion 
  }
}

