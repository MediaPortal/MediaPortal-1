using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DShowNET;

namespace MediaPortal.Player 
{
  public class StreamBufferPlayer9 : BaseStreamBufferPlayer
  {
    GCHandle                   m_myHandle;
    AllocatorWrapper.Allocator m_allocator;
    PlaneScene                 m_scene=null;

    public StreamBufferPlayer9()
    {
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
      Type comtype = null;
      object comobj = null;
      
      //switch back to directx fullscreen mode
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIGraphicsContext.SendMessage(msg);

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
			
        //IVideoMixingRenderer9
        comtype = Type.GetTypeFromCLSID( Clsid.VideoMixingRenderer9 );
        comobj = Activator.CreateInstance( comtype );
        IBaseFilter VMR9Filter=(IBaseFilter)comobj; comobj=null;
        if (VMR9Filter==null) 
        {
          Log.Write("StreamBufferPlayer9:Failed to get instance of VMR9 ");
          return false;
        }				

        //IVMRFilterConfig9
        IVMRFilterConfig9 FilterConfig9 = VMR9Filter as IVMRFilterConfig9;
        if (FilterConfig9==null) 
        {
          Log.Write("StreamBufferPlayer9:Failed to get IVMRFilterConfig9");
          return false;
        }				

        int hr = FilterConfig9.SetRenderingMode(VMR9.VMRMode_Renderless);
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to SetRenderingMode()");
          return false;
        }				

        hr = FilterConfig9.SetNumberOfStreams(1);
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to SetNumberOfStreams()");
          return false;
        }				

        hr = SetAllocPresenter(VMR9Filter, GUIGraphicsContext.form as Control);
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to SetAllocPresenter()");
          return false;
        }				

        hr=graphBuilder.AddFilter(VMR9Filter,"VMR9");
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to VMR9 to graph");
          return false;
        }	
        // create SBE source
        Guid clsid = Clsid.StreamBufferSource;
        Guid riid = typeof(IStreamBufferSource).GUID;
        Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
        bufferSource = (IStreamBufferSource) comObj; comObj = null;
        if (bufferSource==null) 
        {
          Log.Write("StreamBufferPlayer9:Failed to create instance of SBE (do you have WinXp SP1?)");
          return false;
        }	

		
        IBaseFilter filter = (IBaseFilter) bufferSource;
        hr=graphBuilder.AddFilter(filter, "SBE SOURCE");
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to add SBE to graph");
          return false;
        }	
		
        IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
        if (fileSource==null) 
        {
          Log.Write("StreamBufferPlayer9:Failed to get IFileSourceFilter");
          return false;
        }	
        hr = fileSource.Load(filename, IntPtr.Zero);
        if (hr!=0) 
        {
          Log.Write("StreamBufferPlayer9:Failed to open file:{0} :0x{1:x}",filename,hr);
          return false;
        }	


				// add preferred video & audio codecs
				string strVideoCodec="";
        string strAudioCodec="";
        bool   bAddFFDshow=false;
				using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
					strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
					strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
				}
				if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
        if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

				// render output pins of SBE
        DirectShowUtil.RenderOutputPins(graphBuilder, (IBaseFilter)fileSource);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        m_mediaSeeking = bufferSource as IStreamBufferMediaSeeking ;
        if (m_mediaSeeking==null)
        {
          Log.Write("StreamBufferPlayer9:Unable to get IMediaSeeking interface#1");
          return false;
        }
        
//        Log.Write("StreamBufferPlayer9:SetARMode");
//        DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);

        Log.Write("StreamBufferPlayer9: set Deinterlace");
        DirectShowUtil.EnableDeInterlace(graphBuilder);

        //Log.Write("StreamBufferPlayer9: done");
        if( FilterConfig9 != null )
          Marshal.ReleaseComObject( FilterConfig9 ); FilterConfig9 = null;

        
        if( VMR9Filter != null )
          Marshal.ReleaseComObject( VMR9Filter ); VMR9Filter = null;
        return true;
      }
      catch( Exception  ex)
      {
        Log.Write("StreamBufferPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
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
      m_allocator = new AllocatorWrapper.Allocator(control, m_scene);
      IntPtr hMonitor;
      AdapterInformation ai = Manager.Adapters.Default;

      hMonitor = Manager.GetAdapterMonitor(ai.Adapter);
      IntPtr upDevice = DsUtils.GetUnmanagedDevice(m_allocator.Device);
				 
      int hr = lpIVMRSurfAllocNotify.SetD3DDevice(upDevice, hMonitor);
      //Marshal.AddRef(upDevice);
      if (hr != 0)
        return hr;

      // this must be global. If it gets garbage collected, pinning won't exist...
      m_myHandle = GCHandle.Alloc(m_allocator, GCHandleType.Pinned);
      hr = m_allocator.AdviseNotify(lpIVMRSurfAllocNotify);
      if (hr != 0)
        return hr;
      hr = lpIVMRSurfAllocNotify.AdviseSurfaceAllocator(0xACDCACDC, m_allocator);

      return hr;
    }




    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
    {
      //lock(this)
      {
        int hr;
        Log.Write("StreamBufferPlayer9:cleanup DShow graph");
        try 
        {
          //          Log.Write("StreamBufferPlayer9:stop graph");
          if( mediaCtrl != null )
          {
            hr = mediaCtrl.Stop();
          }
          
//          Log.Write("StreamBufferPlayer9:stop notifies");
          if( mediaEvt != null )
          {
            hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
            mediaEvt = null;
          }


//          Log.Write("StreamBufferPlayer9:cleanup");
          if (m_allocator!=null)
          {
            m_allocator.UnAdviseNotify();
          }
          if (m_myHandle.IsAllocated)
          {
            m_myHandle.Free();
          }
          m_allocator=null;
          
          if (m_scene!=null)
          {
            m_scene.Stop();
            m_scene.Deinit();
            m_scene=null;
          }

          basicAudio	= null;
          m_mediaSeeking=null;
          mediaCtrl = null;
	
		      DsUtils.RemoveFilters(graphBuilder);

          if( rotCookie != 0 )
            DsROT.RemoveGraphFromRot( ref rotCookie );
          rotCookie=0;

          if( graphBuilder != null )
            Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;


          GUIGraphicsContext.form.Invalidate(true);
          //GUIGraphicsContext.form.Update();
          //GUIGraphicsContext.form.Validate();
          m_state = PlayState.Init;
          GC.Collect();
//          Log.Write("StreamBufferPlayer9:cleaned up");
        }
        catch( Exception ex)
        {
          Log.Write("StreamBufferPlayer9:exception while cleaning DShow graph {0} {1}",ex.Message, ex.StackTrace);
        }
        //switch back to directx windowed mode
        GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
        GUIGraphicsContext.SendMessage(msg);
      }
    }
		public override bool DoesOwnRendering
		{
			get { return true;}
		}      

    protected override void OnProcess()
    {
      if (Paused)
      {
        //repaint
        m_allocator.Repaint();
      }
    }

  }
}
