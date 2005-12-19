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

		VMR9Util _vmr9 = null;
    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string path)
    {
      int		            hr;
      Type	            comtype = null;
      object	          comobj = null;
      _freeNavigator=true;
      _dvdInfo=null;
      _dvdCtrl=null;
			_videoWin=null;
      string dvdDNavigator="DVD Navigator";
      string aspectRatio="";
      string displayMode="";
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
				dvdDNavigator=xmlreader.GetValueAsString("dvdplayer","navigator","DVD Navigator");
				aspectRatio=xmlreader.GetValueAsString("dvdplayer","armode","").ToLower();
        if ( aspectRatio=="crop") arMode=AmAspectRatioMode.AM_ARMODE_CROP;
        if ( aspectRatio=="letterbox") arMode=AmAspectRatioMode.AM_ARMODE_LETTER_BOX;
        if ( aspectRatio=="stretch") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED;
        if ( aspectRatio=="follow stream") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED_AS_PRIMARY;

        displayMode=xmlreader.GetValueAsString("dvdplayer","displaymode","").ToLower();
        if (displayMode=="default") _videoPref=0;
        if (displayMode=="16:9") _videoPref=1;
        if (displayMode=="4:3 pan scan") _videoPref=2;
        if (displayMode=="4:3 letterbox") _videoPref=3;
      }

      Log.Write("DVD:enable dx9 exclusive mode");
			GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
			GUIWindowManager.SendMessage(msg);

      try 
      {

        _vmr9=new VMR9Util("dvdplayer");
        comtype = Type.GetTypeFromCLSID( Clsid.DvdGraphBuilder );
        if( comtype == null )
          throw new NotSupportedException( "DirectX (8.1 or higher) not installed?" );
        comobj = Activator.CreateInstance( comtype );
        _dvdGraph = (IDvdGraphBuilder) comobj; comobj = null;

        hr = _dvdGraph.GetFiltergraph( out _graphBuilder );
        if( hr != 0 )
          Marshal.ThrowExceptionForHR( hr );

				DsROT.AddGraphToRot( _graphBuilder, out _rotCookie );		// _graphBuilder capGraph
				_vmr9.AddVMR9(_graphBuilder);
				try
				{
					Log.Write("DVDPlayer9:Add {0}",dvdDNavigator);
					_dvdbasefilter=DirectShowUtil.AddFilterToGraph(_graphBuilder,dvdDNavigator);
					if (_dvdbasefilter!=null)
					{
						AddPreferedCodecs(_graphBuilder);
						_dvdCtrl=_dvdbasefilter as IDvdControl2;
						if (_dvdCtrl!=null)
						{
							_dvdInfo = _dvdbasefilter as IDvdInfo2;
							if (path!=null) 
							{
								if (path.Length!=0)
									hr=_dvdCtrl.SetDVDDirectory(path);
								
							}
							_dvdCtrl.SetOption( DvdOptionFlag.HmsfTimeCodeEvt, true );	// use new HMSF timecode format
							_dvdCtrl.SetOption( DvdOptionFlag.ResetOnStop, false );
							DirectShowUtil.RenderOutputPins(_graphBuilder,_dvdbasefilter);
								
							_freeNavigator=false;
						}

						//Marshal.ReleaseComObject( _dvdbasefilter); _dvdbasefilter = null;              
					}
				}
				catch(Exception ex)
				{
					string strEx=ex.Message;
				}
				Guid riid ;

        if (_dvdInfo==null)
				{
					Log.Write("Dvdplayer9:volume rendered, get interfaces");
          riid = typeof( IDvdInfo2 ).GUID;
          hr = _dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          _dvdInfo = (IDvdInfo2) comobj; comobj = null;
        }

        if (_dvdCtrl==null)
        {
					Log.Write("Dvdplayer9: get IDvdControl2");
          riid = typeof( IDvdControl2 ).GUID;
          hr = _dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          _dvdCtrl = (IDvdControl2) comobj; comobj = null;
					if (_dvdCtrl!=null)
						Log.Write("Dvdplayer9: get IDvdControl2");
					else
						Log.WriteFile(Log.LogType.Log,true,"Dvdplayer9: FAILED TO get get IDvdControl2");
        }


        _mediaCtrl	= (IMediaControl)  _graphBuilder;
        _mediaEvt	= (IMediaEventEx)  _graphBuilder;
        _basicAudio	= _graphBuilder as IBasicAudio;
        _mediaPos	= (IMediaPosition) _graphBuilder;
        _basicVideo	= _graphBuilder as IBasicVideo2;

      

        Log.Write("Dvdplayer9:disable line 21");
        // disable Closed Captions!
        IBaseFilter basefilter;
        _graphBuilder.FindFilterByName("Line 21 Decoder", out basefilter);
        if (basefilter==null)
          _graphBuilder.FindFilterByName("Line21 Decoder", out basefilter);
        if (basefilter!=null)
        {
          _line21Decoder=(IAMLine21Decoder)basefilter;
          if (_line21Decoder!=null)
          {
            AMLine21CCState state=AMLine21CCState.Off;
            hr=_line21Decoder.SetServiceState(ref state);
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

        DirectShowUtil.SetARMode(_graphBuilder,arMode);
        DirectShowUtil.EnableDeInterlace(_graphBuilder);
        //m_ovMgr = new OVTOOLLib.OvMgrClass();
        //m_ovMgr.SetGraph(_graphBuilder);



        _videoWidth=_vmr9.VideoWidth;
        _videoHeight=_vmr9.VideoHeight;


				if (!_vmr9.IsVMR9Connected)
				{
					Log.Write("DVDPlayer9:failed vmr9 not connected");
					_mediaCtrl=null;
					Cleanup();
					return base.GetInterfaces(path);
				}
				_vmr9.SetDeinterlaceMode();
        Log.Write("Dvdplayer9:graph created");
        _started=true;
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
			if (_graphBuilder==null) return;
      int hr;
      try 
      {
        Log.Write("DVDPlayer9:cleanup DShow graph");
				if( _mediaCtrl != null )
				{
					hr = _mediaCtrl.Stop();
					_mediaCtrl = null;
				}
        _state = PlayState.Stopped;
				_visible=false;

        _mediaEvt = null;
				_dvdCtrl = null;
				_dvdInfo = null;
				_basicVideo=null;
				_basicAudio=null;
				_mediaPos=null;
				_videoWin=null;

				if (_vmr9!=null) 
				{
					_vmr9.RemoveVMR9();
					_vmr9.Release();
				}
				_vmr9=null;

				
				if (_videoCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_videoCodecFilter))>0); 
					_videoCodecFilter=null;
				}
				if (_audioCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_audioCodecFilter))>0); 
					_audioCodecFilter=null;
				}
				
				if (_audioRendererFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_audioRendererFilter))>0); 
					_audioRendererFilter=null;
				}
				
				if (_ffdShowFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_ffdShowFilter))>0); 
					_ffdShowFilter=null;
				}

    		
        if( _cmdOption.dvdCmd != null )
          Marshal.ReleaseComObject( _cmdOption.dvdCmd ); 
				_cmdOption.dvdCmd = null;
        _pendingCmd = false;

				if( _dvdbasefilter != null )
				{
					while ((hr=Marshal.ReleaseComObject( _dvdbasefilter))>0); 
					_dvdbasefilter = null;              
				}

				if( _dvdGraph != null )
				{
					while ((hr=Marshal.ReleaseComObject( _dvdGraph ))>0); 
					_dvdGraph = null;
				}
				if (_line21Decoder!=null)
				{
					while ((hr=Marshal.ReleaseComObject( _line21Decoder))>0); 
					_line21Decoder=null;
				}


				if (_rotCookie !=0) 
					DsROT.RemoveGraphFromRot( ref _rotCookie );		// _graphBuilder capGraph
				_rotCookie=0;

				if( _graphBuilder != null )
				{
					DsUtils.RemoveFilters(_graphBuilder);
					while ((hr=Marshal.ReleaseComObject( _graphBuilder ))>0); 
					_graphBuilder = null;
				}

        _state = PlayState.Init;

        if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
        {
          Log.Write("DVD:disable dx9 exclusive mode");
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msg);
        }

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
      //m_SourceRect=m_scene.SourceRect;
      //m_VideoRect=m_scene.DestRect;
    }
  }
}
