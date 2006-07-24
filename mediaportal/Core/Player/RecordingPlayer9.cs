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
  public class RecordingPlayer9 : IPlayer
  {
    public enum PlayState
    {
      Init,
      Playing,
      Paused
    }
    string                    m_strCurrentFile="";
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

    /// <summary> audio interface used to control volume. </summary>
    private IBasicAudio				basicAudio;
    private const int WM_GRAPHNOTIFY	= 0x00008001;	// message from graph

    private const int WS_CHILD			= 0x40000000;	// attributes for video window
    private const int WS_CLIPCHILDREN	= 0x02000000;
    private const int WS_CLIPSIBLINGS	= 0x04000000;
    bool        m_bVisible=false;
    GCHandle                  myHandle;
    AllocatorWrapper.Allocator allocator;
    PlaneScene                 m_scene=null;

    public RecordingPlayer9()
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
      Log.Write("RecordingPlayer9:play {0}", strFile);
//      lock ( typeof(RecordingPlayer9) )
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
        Log.Write("RecordingPlayer9:set window");
        int hr = mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
        if (hr < 0)
        {
          m_strCurrentFile="";
          CloseInterfaces();
          return false;
        }
        
        Log.Write("RecordingPlayer9:run");
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

        Log.Write("RecordingPlayer9:set window...");
        m_state=PlayState.Playing;
        m_bStarted=true;
        Log.Write("RecordingPlayer9:Playing...");
      }
      return true;
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
        Log.Write("RecordingPlayer9:ended {0}", m_strCurrentFile);
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
        }
      }
      else if (!m_bVisible)
      {
        m_bVisible=true;
      }
      
      if (Paused )
      {
        //repaint
        allocator.Repaint();
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
      Log.Write("RecordingPlayer9:GetInterfaces()");
      Type comtype = null;
      object comobj = null;
      RGB color;
      color.red = 0;
      color.green = 0;
      color.blu = 0;

      DsRECT rect = new DsRECT();
      rect.Top = 0;
      rect.Bottom =GUIGraphicsContext.form.Height;
      rect.Left = 0;
      rect.Right = GUIGraphicsContext.form.Width;
				

      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.Write("RecordingPlayer9:DirectX 9 not installed");
          return false;
        }
        comobj = Activator.CreateInstance( comtype );
        graphBuilder = (IGraphBuilder) comobj; comobj = null;
			
        //IVideoMixingRenderer9
        comtype = Type.GetTypeFromCLSID( Clsid.VideoMixingRenderer9 );
        comobj = Activator.CreateInstance( comtype );
        IBaseFilter VMR9Filter=(IBaseFilter)comobj; comobj=null;
				

        //IVMRFilterConfig9
        DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph
        IVMRFilterConfig9 FilterConfig9 = VMR9Filter as IVMRFilterConfig9;
        int hr = FilterConfig9.SetRenderingMode(VMR9.VMRMode_Renderless);
        hr = FilterConfig9.SetNumberOfStreams(1);
        hr = SetAllocPresenter(VMR9Filter, GUIGraphicsContext.form as Control);

        graphBuilder.AddFilter(VMR9Filter,"VMR9");
			
        Log.Write("RecordingPlayer9:add mp reader filter");
        IBaseFilter bfileReader= DirectShowUtil.AddFilterToGraph(graphBuilder,"MP Reader");
        if (bfileReader==null) 
          Marshal.ThrowExceptionForHR( -1 );

        Log.Write("RecordingPlayer9:get IFileSourceFilter interface");
        IFileSourceFilter source=(IFileSourceFilter)bfileReader;
        if (source==null)
        {
          Marshal.ThrowExceptionForHR( -1 );
        }
        Log.Write("RecordingPlayer9:set source");
        hr=source.Load(m_strCurrentFile,IntPtr.Zero);
        if (hr<0)
          Marshal.ThrowExceptionForHR( -1 );

        DirectShowUtil.RenderOutputPins(graphBuilder,bfileReader);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        mediaSeek	= (IMediaSeeking)  graphBuilder;
        mediaPos	= (IMediaPosition) graphBuilder;

        basicAudio	= graphBuilder as IBasicAudio;
        DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);
        DirectShowUtil.EnableDeInterlace(graphBuilder);

        if( FilterConfig9 != null )
          Marshal.ReleaseComObject( FilterConfig9 ); FilterConfig9 = null;

        
        if( VMR9Filter != null )
          Marshal.ReleaseComObject( VMR9Filter ); VMR9Filter = null;

        return true;
      }
      catch( Exception  ex)
      {
        Log.Write("RecordingPlayer9:VideoPlayer:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
        return false;
      }
      finally
      {
        if( comobj != null )
          Marshal.ReleaseComObject( comobj ); comobj = null;
      }
    }




    int SetAllocPresenter(IBaseFilter filter, Control control)
    {
      IVMRSurfaceAllocatorNotify9 lpIVMRSurfAllocNotify = filter as IVMRSurfaceAllocatorNotify9;

      if (lpIVMRSurfAllocNotify == null)
        return -1;

      m_scene= new PlaneScene(m_renderFrame);
      allocator = new AllocatorWrapper.Allocator(control, m_scene);
      IntPtr hMonitor;
      AdapterInformation ai = Manager.Adapters.Default;

      hMonitor = Manager.GetAdapterMonitor(ai.Adapter);
      IntPtr upDevice = DsUtils.GetUnmanagedDevice(allocator.Device);
				 
      int hr = lpIVMRSurfAllocNotify.SetD3DDevice(upDevice, hMonitor);
      //Marshal.AddRef(upDevice);
      if (hr != 0)
        return hr;

      // this must be global. If it gets garbage collected, pinning won't exist...
      myHandle = GCHandle.Alloc(allocator, GCHandleType.Pinned);
      hr = allocator.AdviseNotify(lpIVMRSurfAllocNotify);
      if (hr != 0)
        return hr;
      hr = lpIVMRSurfAllocNotify.AdviseSurfaceAllocator(0xACDCACDC, allocator);

      return hr;
    }


    /// <summary> do cleanup and release DirectShow. </summary>
    void CloseInterfaces()
    {
      //lock(this)
      {
        int hr;
        Log.Write("RecordingPlayer9:cleanup DShow graph");
        try 
        {
          Log.Write("RecordingPlayer9:stop graph");
          if( mediaCtrl != null )
          {
            hr = mediaCtrl.Stop();
          }
          Log.Write("RecordingPlayer9:stop notifies");
          if( mediaEvt != null )
          {
            hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
            mediaEvt = null;
          }


          Log.Write("RecordingPlayer9:cleanup");
          if (allocator!=null)
          {
            allocator.UnAdviseNotify();
          }
          if (myHandle.IsAllocated)
          {
            myHandle.Free();
          }
          allocator=null;
          
          if (m_scene!=null)
          {
            m_scene.Stop();
            m_scene.Deinit();
            m_scene=null;
          }

          mediaSeek	= null;
          mediaPos	= null;
          basicAudio	= null;
          mediaCtrl = null;
			
          if( rotCookie != 0 )
            DsROT.RemoveGraphFromRot( ref rotCookie );

          if( graphBuilder != null )
            Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;



          GUIGraphicsContext.form.Invalidate(true);          
          m_state = PlayState.Init;
          GC.Collect();
          Log.Write("RecordingPlayer9:cleaned up");
        }
        catch( Exception ex)
        {
          Log.Write("RecordingPlayer9:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
        }
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

    public override bool DoesOwnRendering
    {
      get { return true;}
    }      

  }
}

