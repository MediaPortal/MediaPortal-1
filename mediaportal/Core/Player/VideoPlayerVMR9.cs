using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;


using MediaPortal.GUI.Library;
using DShowNET;
namespace MediaPortal.Player 
{


	public class VideoPlayerVMR9 : VideoPlayerVMR7
	{
    GCHandle                  myHandle;
    AllocatorWrapper.Allocator allocator;
    PlaneScene                 m_scene=null;

		
		public VideoPlayerVMR9()
		{
		}

		/// <summary> create the used COM components and get the interfaces. </summary>
		protected override bool GetInterfaces()
		{
      //switch back to directx fullscreen mode
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIGraphicsContext.SendMessage(msg);

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
					Log.Write("VideoPlayer9:DirectX 9 not installed");
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
          Log.Write("VideoPlayer9:Failed to get instance of VMR9 ");
          return false;
        }
				

				//IVMRFilterConfig9
        IVMRFilterConfig9 FilterConfig9 = VMR9Filter as IVMRFilterConfig9;
        if (FilterConfig9==null) 
        {
          Log.Write("VideoPlayer9:Failed to get IVMRFilterConfig9 ");
          return false;
        }
				int hr = FilterConfig9.SetRenderingMode(VMR9.VMRMode_Renderless);
        if (hr!=0) 
        {
          Log.Write("VideoPlayer9:Failed to set VMR9 to renderless mode");
          return false;
        }

        // needed to put VMR9 in mixing mode instead of pass-through mode
        
        hr = FilterConfig9.SetNumberOfStreams(1);
        if (hr!=0) 
        {
          Log.Write("VideoPlayer9:Failed to set VMR9 streams to 1");
          return false;
        }


        hr = SetAllocPresenter(VMR9Filter, GUIGraphicsContext.form as Control);
        if (hr!=0) 
        {
          Log.Write("VideoPlayer9:Failed to set VMR9 allocator/presentor");
          return false;
        }

        hr=graphBuilder.AddFilter(VMR9Filter,"VMR9");
        if (hr!=0) 
        {
          Log.Write("VideoPlayer9:Failed to add vmr9 to filtergraph");
          return false;
        }

        // add preferred video & audio codecs
        string strVideoCodec="";
        string strAudioCodec="";
        bool   bAddFFDshow=false;
        using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("movieplayer","ffdshow",false);
          strVideoCodec=xmlreader.GetValueAsString("movieplayer","mpeg2videocodec","");
          strAudioCodec=xmlreader.GetValueAsString("movieplayer","mpeg2audiocodec","");
        }
        string strExt=System.IO.Path.GetExtension(m_strCurrentFile).ToLower();
        if (strExt.Equals(".mpg") ||strExt.Equals(".mpeg")||strExt.Equals(".bin")||strExt.Equals(".dat"))
        {
          //if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
          //if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
        }
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");


				hr = DsUtils.RenderFileToVMR9(graphBuilder, m_strCurrentFile, VMR9Filter, false);
        if (hr!=0) 
        {
          Log.Write("VideoPlayer9:Failed to render file -> vmr9");
          return false;
        }
        
        mediaCtrl	= (IMediaControl)  graphBuilder;
				mediaEvt	= (IMediaEventEx)  graphBuilder;
				mediaSeek	= (IMediaSeeking)  graphBuilder;
				mediaPos	= (IMediaPosition) graphBuilder;
				basicAudio	= graphBuilder as IBasicAudio;
				//DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);
				DirectShowUtil.EnableDeInterlace(graphBuilder);
        m_iVideoWidth=allocator.NativeSize.Width;
        m_iVideoHeight=allocator.NativeSize.Height;

        IBaseFilter filter;
        graphBuilder.FindFilterByName("DirectVobSub (auto-loading version)", out filter);
        vobSub = null;
        vobSub = (IDirectVobSub)filter;
        if (vobSub!=null)
        {
          using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
          {
            string strTmp="";
            string strFont=xmlreader.GetValueAsString("subtitles","fontface","Arial");
            int    iFontSize=xmlreader.GetValueAsInt("subtitles","fontsize",18);
            bool   bBold=xmlreader.GetValueAsBool("subtitles","bold",true);
            
            strTmp=xmlreader.GetValueAsString("subtitles","color","ffffff");
            long iColor=Convert.ToInt64(strTmp,16);
            int  iShadow=xmlreader.GetValueAsInt("subtitles","shadow",5);
          
            LOGFONT logFont = new LOGFONT();
            int txtcolor;
            bool fShadow, fOutLine, fAdvancedRenderer = false;
            int size = Marshal.SizeOf(typeof(LOGFONT));
            vobSub.get_TextSettings(logFont, size,out txtcolor, out fShadow, out fOutLine, out fAdvancedRenderer);

            FontStyle fontStyle=FontStyle.Regular;
            if (bBold) fontStyle=FontStyle.Bold;
            System.Drawing.Font Subfont = new System.Drawing.Font(strFont,iFontSize,fontStyle);
            Subfont.ToLogFont(logFont);
            int R=(int)((iColor>>16)&0xff);
            int G=(int)((iColor>>8)&0xff);
            int B=(int)((iColor)&0xff);
            txtcolor=(B<<16)+(G<<8)+R;
            if (iShadow>0) fShadow=true;
            int res = vobSub.put_TextSettings(logFont, size, txtcolor,  fShadow, fOutLine, fAdvancedRenderer);
          }
        }


        if( FilterConfig9 != null )
          Marshal.ReleaseComObject( FilterConfig9 ); FilterConfig9 = null;

        
        if( VMR9Filter != null )
          Marshal.ReleaseComObject( VMR9Filter ); VMR9Filter = null;
				return true;
			}
			catch( Exception  ex)
			{
				Log.Write("VideoPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
				return false;
			}
		}


    int SetAllocPresenter(IBaseFilter filter, Control control)
    {
      IVMRSurfaceAllocatorNotify9 lpIVMRSurfAllocNotify = filter as IVMRSurfaceAllocatorNotify9;

      if (lpIVMRSurfAllocNotify == null)
      {
        Log.Write("VideoPlayer9:Failed to get IVMRSurfaceAllocatorNotify9");
        return -1;
      }
      m_scene= new PlaneScene(m_renderFrame);
      allocator = new AllocatorWrapper.Allocator(control, m_scene);
      IntPtr hMonitor;
      AdapterInformation ai = Manager.Adapters.Default;

      hMonitor = Manager.GetAdapterMonitor(ai.Adapter);
      IntPtr upDevice = DsUtils.GetUnmanagedDevice(allocator.Device);
				 
      int hr = lpIVMRSurfAllocNotify.SetD3DDevice(upDevice, hMonitor);
      //Marshal.AddRef(upDevice);
      if (hr != 0)
      {
        Log.Write("VideoPlayer9:Failed to get SetD3DDevice()");
        return hr;
      }
      // this must be global. If it gets garbage collected, pinning won't exist...
      myHandle = GCHandle.Alloc(allocator, GCHandleType.Pinned);
      hr = allocator.AdviseNotify(lpIVMRSurfAllocNotify);
      if (hr != 0)
      {
        Log.Write("VideoPlayer9:Failed to AdviseNotify()");
        return hr;
      }
      hr = lpIVMRSurfAllocNotify.AdviseSurfaceAllocator(0xACDCACDC, allocator);
      if (hr !=0)
      {
        Log.Write("VideoPlayer9:Failed to AdviseSurfaceAllocator()");
      }
      return hr;
    }


		protected override void OnProcess()
		{
			if (Paused)
			{
				//repaint
        allocator.Repaint();
			}
		}

		/// <summary> do cleanup and release DirectShow. </summary>
		protected override void CloseInterfaces()
		{
      //lock(this)
      {
        int hr;
        Log.Write("VideoPlayer9:cleanup DShow graph");
        try 
        {
          if( mediaCtrl != null )
          {
            hr = mediaCtrl.Stop();
            System.Threading.Thread.Sleep(500);
            System.Threading.Thread.Sleep(500);
          }
  	      if( mediaEvt != null )
          {
            hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
            mediaEvt = null;
          }



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
			
          DsUtils.RemoveFilters(graphBuilder);

          if( rotCookie != 0 )
            DsROT.RemoveGraphFromRot( ref rotCookie );
          rotCookie=0;

          if( graphBuilder != null )
            Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;


          GUIGraphicsContext.form.Invalidate(true);
          m_state = PlayState.Init;
          GC.Collect();
        }
        catch( Exception ex)
        {
          Log.Write("VideoPlayer9:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
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


	}
}
