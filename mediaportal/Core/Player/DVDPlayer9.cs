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
			videoWin=null;
      string strDVDNavigator="DVD Navigator";
      string strARMode="";
      string strDisplayMode="";
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
				strDVDNavigator=xmlreader.GetValueAsString("dvdplayer","navigator","DVD Navigator");
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

				DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph
				Vmr9.AddVMR9(graphBuilder);
				try
				{
					Log.Write("DVDPlayer9:Add {0}",strDVDNavigator);
					dvdbasefilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strDVDNavigator);
					if (dvdbasefilter!=null)
					{
						AddPreferedCodecs(graphBuilder);
						dvdCtrl=dvdbasefilter as IDvdControl2;
						if (dvdCtrl!=null)
						{
							dvdInfo = dvdbasefilter as IDvdInfo2;
							if (strPath!=null) 
							{
								if (strPath.Length!=0)
									hr=dvdCtrl.SetDVDDirectory(strPath);
								
							}
							dvdCtrl.SetOption( DvdOptionFlag.HmsfTimeCodeEvt, true );	// use new HMSF timecode format
							dvdCtrl.SetOption( DvdOptionFlag.ResetOnStop, false );
							DirectShowUtil.RenderOutputPins(graphBuilder,dvdbasefilter);
								
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

        if (dvdInfo==null)
				{
					Log.Write("Dvdplayer9:volume rendered, get interfaces");
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
						Log.WriteFile(Log.LogType.Log,true,"Dvdplayer9: FAILED TO get get IDvdControl2");
        }


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
              Log.Write("DVDPlayer9:Closed Captions disabled");
            }
            else
            {
              Log.Write("DVDPlayer9:failed 2 disable Closed Captions");
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
					Log.Write("DVDPlayer9:failed vmr9 not connected");
					mediaCtrl=null;
					Cleanup();
					return base.GetInterfaces(strPath);
				}
				Vmr9.SetDeinterlaceMode();
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
      }
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
        Log.Write("DVDPlayer9:cleanup DShow graph");
				if( mediaCtrl != null )
				{
					hr = mediaCtrl.Stop();
					mediaCtrl = null;
				}
        m_state = PlayState.Stopped;
				m_bVisible=false;

        mediaEvt = null;
				dvdCtrl = null;
				dvdInfo = null;
				basicVideo=null;
				basicAudio=null;
				mediaPos=null;
				videoWin=null;

				if (Vmr9!=null) 
				{
					Vmr9.RemoveVMR9();
					Vmr9.Release();
				}
				Vmr9=null;

				
				if (videoCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(videoCodecFilter))>0); 
					videoCodecFilter=null;
				}
				if (audioCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(audioCodecFilter))>0); 
					audioCodecFilter=null;
				}
				
				if (audioRendererFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(audioRendererFilter))>0); 
					audioRendererFilter=null;
				}
				
				if (ffdShowFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(ffdShowFilter))>0); 
					ffdShowFilter=null;
				}

    		
        if( cmdOption.dvdCmd != null )
          Marshal.ReleaseComObject( cmdOption.dvdCmd ); 
				cmdOption.dvdCmd = null;
        pendingCmd = false;

				if( dvdbasefilter != null )
				{
					while ((hr=Marshal.ReleaseComObject( dvdbasefilter))>0); 
					dvdbasefilter = null;              
				}

				if( dvdGraph != null )
				{
					while ((hr=Marshal.ReleaseComObject( dvdGraph ))>0); 
					dvdGraph = null;
				}
				if (line21Decoder!=null)
				{
					while ((hr=Marshal.ReleaseComObject( line21Decoder))>0); 
					line21Decoder=null;
				}


				if (rotCookie !=0) 
					DsROT.RemoveGraphFromRot( ref rotCookie );		// graphBuilder capGraph
				rotCookie=0;

				if( graphBuilder != null )
				{
					DsUtils.RemoveFilters(graphBuilder);
					while ((hr=Marshal.ReleaseComObject( graphBuilder ))>0); 
					graphBuilder = null;
				}

        m_state = PlayState.Init;

				GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
				GUIWindowManager.SendMessage(msg);

        GUIGraphicsContext.form.Invalidate(true);          
        GUIGraphicsContext.form.Activate();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer9:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }
    }

    protected override void OnProcess()
    {
			if (GUIGraphicsContext.Vmr9Active && Vmr9!=null)
			{
				Vmr9.Process();
				if (GUIGraphicsContext.Vmr9FPS < 1f)
				{
					Vmr9.Repaint();// repaint vmr9
				}
			}
      //m_SourceRect=m_scene.SourceRect;
      //m_VideoRect=m_scene.DestRect;
    }
  }
}
