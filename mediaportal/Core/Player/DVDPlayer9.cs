using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;


using DShowNET;
using DShowNET.Dvd;


namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class DVDPlayer9 : DVDPlayer 
  {
    const uint  VFW_E_DVD_DECNOTENOUGH    =0x8004027B;
    const uint  VFW_E_DVD_RENDERFAIL      =0x8004027A;

		VMR9Util Vmr9 = null;
    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string strPath)
    {
      int		            hr;
      Type	            comtype = null;
      object	          comobj = null;
      m_bFreeNavigator=true;
      dvdInfo=null;
      dvdCtrl=null;

      string strDVDAudioRenderer="";
      string strDVDNavigator="";
      string strARMode="";
      string strDisplayMode="";
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strDVDAudioRenderer=xmlreader.GetValueAsString("dvdplayer","audiorenderer","");
        strDVDNavigator=xmlreader.GetValueAsString("dvdplayer","navigator","");
        strARMode=xmlreader.GetValueAsString("dvdplayer","armode","").ToLower();
        if ( strARMode=="crop") arMode=AmAspectRatioMode.AM_ARMODE_CROP;
        if ( strARMode=="letterbox") arMode=AmAspectRatioMode.AM_ARMODE_LETTER_BOX;
        if ( strARMode=="stretch") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED;
        if ( strARMode=="follow stream") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED_AS_PRIMARY;

        strDisplayMode=xmlreader.GetValueAsString("dvdplayer","displaymode","").ToLower();
        if (strDisplayMode=="default") m_iVideoPref=0;
        if (strDisplayMode=="16:9") m_iVideoPref=1;
        if (strDisplayMode=="4:3 pan scan") m_iVideoPref=2;
        if (strDisplayMode=="4:3 letterbox") m_iVideoPref=3;
      }
			GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
			GUIWindowManager.SendMessage(msg);

      try 
      {

        Vmr9=new VMR9Util("dvdplayer");
        comtype = Type.GetTypeFromCLSID( Clsid.DvdGraphBuilder );
        if( comtype == null )
          throw new NotSupportedException( "DirectX (8.1 or higher) not installed?" );
        comobj = Activator.CreateInstance( comtype );
        dvdGraph = (IDvdGraphBuilder) comobj; comobj = null;

        hr = dvdGraph.GetFiltergraph( out graphBuilder );
        if( hr != 0 )
          Marshal.ThrowExceptionForHR( hr );

				Vmr9.AddVMR9(graphBuilder);
				IBaseFilter dvdbasefilter=null;
				try
				{
					dvdbasefilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strDVDNavigator);
					if (dvdbasefilter!=null)
					{
						IDvdControl2 cntl=(IDvdControl2)dvdbasefilter;
						if (cntl!=null)
						{
							AddPreferedCodecs(graphBuilder);
							if (strPath!=null) cntl.SetDVDDirectory(strPath);
							DirectShowUtil.RenderOutputPins(graphBuilder,dvdbasefilter);
							dvdInfo = (IDvdInfo2) cntl;
							dvdCtrl = (IDvdControl2)cntl;
							if (dvdCtrl!=null)
							Log.Write("Dvdplayer9: got IDvdControl2");
							//videoWin	= graphBuilder as IVideoWindow;
							m_bFreeNavigator=false;
						}

						//Marshal.ReleaseComObject( dvdbasefilter); dvdbasefilter = null;              
					}
				}
				catch(Exception ex)
				{
					string strEx=ex.Message;
				}
				Guid riid ;

				Log.Write("Dvdplayer9:volume rendered, get interfaces");
        if (dvdInfo==null)
        {
          riid = typeof( IDvdInfo2 ).GUID;
          hr = dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          dvdInfo = (IDvdInfo2) comobj; comobj = null;
        }

        if (dvdCtrl==null)
        {
					Log.Write("Dvdplayer9: get IDvdControl2");
          riid = typeof( IDvdControl2 ).GUID;
          hr = dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          dvdCtrl = (IDvdControl2) comobj; comobj = null;
					if (dvdCtrl!=null)
						Log.Write("Dvdplayer9: get IDvdControl2");
					else
						Log.Write("Dvdplayer9: FAILED TO get get IDvdControl2");
        }

        dvdbasefilter=dvdInfo as IBaseFilter;
        if (dvdbasefilter==null)
        {
          Log.Write("DVDPlayer9: unable to get dvd base filter");
          return false;
        }
        
        Log.Write("Dvdplayer9:render output pins");
        DirectShowUtil.RenderOutputPins(graphBuilder,dvdbasefilter);


        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        basicAudio	= graphBuilder as IBasicAudio;
        mediaPos	= (IMediaPosition) graphBuilder;
        basicVideo	= graphBuilder as IBasicVideo2;

      

        Log.Write("Dvdplayer9:disable line 21");
        // disable Closed Captions!
        IBaseFilter basefilter;
        graphBuilder.FindFilterByName("Line 21 Decoder", out basefilter);
        if (basefilter==null)
          graphBuilder.FindFilterByName("Line21 Decoder", out basefilter);
        if (basefilter!=null)
        {
          line21Decoder=(IAMLine21Decoder)basefilter;
          if (line21Decoder!=null)
          {
            AMLine21CCState state=AMLine21CCState.Off;
            hr=line21Decoder.SetServiceState(ref state);
            if (hr==0)
            {
              Log.Write("DVDPlayer:Closed Captions disabled");
            }
            else
            {
              Log.Write("DVDPlayer:failed 2 disable Closed Captions");
            }
          }
        }

        DirectShowUtil.SetARMode(graphBuilder,arMode);
        DirectShowUtil.EnableDeInterlace(graphBuilder);
        //m_ovMgr = new OVTOOLLib.OvMgrClass();
        //m_ovMgr.SetGraph(graphBuilder);



        m_iVideoWidth=Vmr9.VideoWidth;
        m_iVideoHeight=Vmr9.VideoHeight;


				if (!Vmr9.IsVMR9Connected)
				{
					mediaCtrl=null;
					Cleanup();
					return base.GetInterfaces(strPath);
				}
        Log.Write("Dvdplayer9:graph created");
        m_bStarted=true;
        return true;
      }
      catch( Exception )
      {
        //MessageBox.Show( this, "Could not get interfaces\r\n" + ee.Message, "DVDPlayer.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop );
        CloseInterfaces();
        return false;
      }
      finally
      {
        if( comobj != null )
          Marshal.ReleaseComObject( comobj ); comobj = null;
      }
    }

    IBaseFilter AddVMR9(IDvdGraphBuilder dvdBuilder)
    {
      Type comtype = null;
      object comobj = null;

      object objFound;
      int hr;
      //IVMRFilterConfig9
      Guid guidVMR9FilterConfig = typeof( IVMRFilterConfig9 ).GUID;;
      dvdBuilder.GetDvdInterface(ref guidVMR9FilterConfig,out objFound);
      IBaseFilter vmr9Filter = objFound as IBaseFilter;
      if (vmr9Filter==null) 
      {
        Log.Write("Dvdplayer9:Failed to get IVMRFilterConfig9 ");
        return  null;
      }
      graphBuilder.RemoveFilter(vmr9Filter);

      comtype = Type.GetTypeFromCLSID( Clsid.VideoMixingRenderer9 );
      comobj = Activator.CreateInstance( comtype );
      vmr9Filter=(IBaseFilter)comobj; comobj=null;
      if (vmr9Filter==null) 
      {
        Log.Write("Dvdplayer9:Failed to get instance of VMR9 ");
        return null;
      }				

      //IVMRFilterConfig9
      IVMRFilterConfig9 FilterConfig9 = vmr9Filter as IVMRFilterConfig9;
      if (FilterConfig9==null) 
      {
        Log.Write("Dvdplayer9:Failed to get IVMRFilterConfig9");
        return null;
      }				

      hr = FilterConfig9.SetRenderingMode(VMR9.VMRMode_Renderless);
      if (hr!=0) 
      {
        Log.Write("Dvdplayer9:Failed to SetRenderingMode()");
        return null;
      }				

      hr = FilterConfig9.SetNumberOfStreams(1);
      if (hr!=0) 
      {
        Log.Write("Dvdplayer9:Failed to SetNumberOfStreams()");
        return null;
      }				

      return vmr9Filter;
    }
    
    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
		{
			Cleanup();
		}

		void Cleanup()
		{
			if (graphBuilder==null) return;
      int hr;
      try 
      {
        Log.Write("DVDPlayer:cleanup DShow graph");
        if( dvdCtrl != null )
          hr = dvdCtrl.SetOption( DvdOptionFlag.ResetOnStop, true );

        if( mediaCtrl != null )
        {
          hr = mediaCtrl.Stop();
          mediaCtrl = null;
        }
        m_state = PlayState.Stopped;

        if( mediaEvt != null )
        {
          hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_DVD_EVENT, IntPtr.Zero );
          mediaEvt = null;
        }

				if (Vmr9!=null) 
				{
					Vmr9.RemoveVMR9();
					Vmr9.Release();
				}
				Vmr9=null;


        if( audioRenderer != null )
          Marshal.ReleaseComObject( audioRenderer); audioRenderer = null;

          m_bVisible=false;
    		
        if( cmdOption.dvdCmd != null )
          Marshal.ReleaseComObject( cmdOption.dvdCmd ); cmdOption.dvdCmd = null;
        pendingCmd = false;


        if (rotCookie !=0) DsROT.RemoveGraphFromRot( ref rotCookie );		// graphBuilder capGraph
        if( graphBuilder != null )
        {
          DsUtils.RemoveFilters(graphBuilder);
          Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;
        }
        if (m_bFreeNavigator)
        {
          if( dvdCtrl != null )
            Marshal.ReleaseComObject( dvdCtrl ); 
        }
        dvdCtrl = null;

        if (m_bFreeNavigator)
        {
          if( dvdInfo != null )
            Marshal.ReleaseComObject( dvdInfo ); 
        }
        dvdInfo = null;

        if( dvdGraph != null )
          Marshal.ReleaseComObject( dvdGraph ); dvdGraph = null;

        line21Decoder=null;
        dvdInfo=null;
        basicVideo=null;
        basicAudio=null;
        mediaPos=null;
        m_state = PlayState.Init;

				GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
				GUIWindowManager.SendMessage(msg);

        GUIGraphicsContext.form.Invalidate(true);          
        GUIGraphicsContext.form.Activate();
      }
      catch( Exception ex)
      {
        Log.Write("DVDPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }
    }

    protected override void OnProcess()
    {
      if (Paused || menuMode!=MenuMode.No )
      {
        //repaint
        if (Vmr9!=null) Vmr9.Repaint();
      }
      //m_SourceRect=m_scene.SourceRect;
      //m_VideoRect=m_scene.DestRect;
    }
  }
}
