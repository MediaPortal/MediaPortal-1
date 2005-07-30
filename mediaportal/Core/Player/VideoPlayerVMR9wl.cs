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
	public class VideoPlayerVMR9wl : VideoPlayerVMR7
	{
		IVMRMixerControl9 m_mixerControl =null;
		IVMRWindowlessControl9 m_pWC =null;
		IVMRMixerBitmap9 m_bitmapMixer =null;
		Bitmap m_memBmp = null;
		Graphics memDC=null; 
		Graphics clientDC =null;
		IntPtr memdc=IntPtr.Zero;
		IntPtr hdc=IntPtr.Zero;
		VMR9AlphaBitmap bitmap = new VMR9AlphaBitmap();
		//Surface m_surface=null;

		public VideoPlayerVMR9wl()
		{
		}

		/// <summary> create the used COM components and get the interfaces. </summary>
		protected override bool GetInterfaces()
		{
			m_bVisible=true;
			Type comtype = null;
			object comobj = null;
    
			m_memBmp = new Bitmap(GUIGraphicsContext.Width, GUIGraphicsContext.Height); 
			clientDC = GUIGraphicsContext.form.CreateGraphics();
			hdc = clientDC.GetHdc(); 
      memdc = Win32Support.CreateCompatibleDC(hdc); 
      Win32Support.SelectObject(memdc, m_memBmp.GetHbitmap()); 
			memDC = Graphics.FromHdc(memdc); 

			try 
			{
				comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
				if( comtype == null )
				{
					Log.Write("VideoPlayerVMR9wl:DirectX 9 not installed");
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
          Log.Write("VideoPlayer9:Failed create instance of VMR9");
          return false;
        }

				//IVMRFilterConfig9
				IVMRFilterConfig9 FilterConfig9 = VMR9Filter as IVMRFilterConfig9;
        if (FilterConfig9==null)
        {
          Log.Write("VideoPlayer9:Failed to get IVMRFilterConfig9");
          return false;
        }

				int hr = FilterConfig9.SetRenderingMode(VMR9.VMRMode_Windowless);
        hr = FilterConfig9.SetNumberOfStreams(1);
        if (hr!=0) 
        {
          Log.Write("VideoPlayer9:Failed to set VMR9 streams to 1");
          return false;
        }


        m_pWC = VMR9Filter as IVMRWindowlessControl9;
        if (m_pWC==null)
        {
          Log.Write("VideoPlayer9:Failed to get IVMRWindowlessControl9");
          return false;
        }
				RGB color;
				color.red = 0;
				color.green = 0;
				color.blu = 0;
        hr = m_pWC.SetBorderColor(color);
        if (hr!=0)
        {
          Log.Write("VideoPlayer9:Failed to SetBorderColor");
          return false;
        }
        hr = m_pWC.SetVideoClippingWindow(GUIGraphicsContext.form.Handle);
        if (hr!=0)
        {
          Log.Write("VideoPlayer9:Failed to SetVideoClippingWindow");
          return false;
        }

				DsRECT rect = new DsRECT();
				rect.Top = 0;
				rect.Bottom =GUIGraphicsContext.Height;
				rect.Left = 0;
				rect.Right = GUIGraphicsContext.Width;
				hr = m_pWC.SetVideoPosition(null, rect);
				
				m_mixerControl = m_pWC as IVMRMixerControl9;
        if (m_mixerControl==null)
        {
          Log.Write("VideoPlayer9:Failed to get IVMRMixerControl9");
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
        using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
  
      
				int iARwidth, iARHeight;
        hr=m_pWC.GetNativeVideoSize(out m_iVideoWidth, out m_iVideoHeight, out iARwidth, out iARHeight);
        if (hr!=0)
        {
          Log.Write("VideoPlayer9:Failed to get GetNativeVideoSize()");
          return false;
        }

				VMR9NormalizedRect rect1 = new VMR9NormalizedRect();
				VMR9NormalizedRect rect2 = new VMR9NormalizedRect();
				rect1.top = 0;
				rect1.bottom = 1;
				rect1.left = 0;
				rect1.right = 1;

				rect2.top = 0.5f;
				rect2.bottom = 1;
				rect2.left = 0.5f;
				rect2.right = 1;


        hr = m_mixerControl.SetOutputRect(0, rect1);
        if (hr!=0)
        {
          Log.Write("VideoPlayer9:Failed to SetOutputRect()");
          return false;
        }
        hr = m_mixerControl.SetZOrder(0, 1);
        if (hr!=0)
        {
          Log.Write("VideoPlayer9:Failed to SetZOrder()");
          return false;
        }
        hr = m_mixerControl.SetAlpha(0, 1f);
        if (hr!=0)
        {
          Log.Write("VideoPlayer9:Failed to SetAlpha()");
          return false;
        }
        m_bitmapMixer = VMR9Filter as IVMRMixerBitmap9 ;
        if (m_bitmapMixer==null)
        {
          Log.Write("VideoPlayer9:Failed to get IVMRMixerBitmap9()");
          return false;
        }

//				m_surface=GUIGraphicsContext.DX9Device.CreateOffscreenPlainSurface(GUIGraphicsContext.Width,GUIGraphicsContext.Height,Format.X8R8G8B8,Pool.SystemMemory);

				//Log.Write("VideoPlayerVMR9wl: done");
				//if( FilterConfig9 != null )
				//	Marshal.ReleaseComObject( FilterConfig9 ); FilterConfig9 = null;

        
				//if( VMR9Filter != null )
				//	Marshal.ReleaseComObject( VMR9Filter ); VMR9Filter = null;
				return true;
			}
			catch( Exception  ex)
			{
				Log.Write("VideoPlayerVMR9wl:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
				return false;
			}
			finally
			{
				if( comobj != null )
					Marshal.ReleaseComObject( comobj ); comobj = null;
			}
		}


		/// <summary> do cleanup and release DirectShow. </summary>
		protected override void CloseInterfaces()
		{

			int hr;
			Log.Write("VideoPlayer:cleanup DShow graph");
			try 
			{
				if( rotCookie != 0 )
					DsROT.RemoveGraphFromRot( ref rotCookie );

        rotCookie=0;
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

				if ( mediaSeek != null )
					Marshal.ReleaseComObject( mediaSeek );
				mediaSeek= null;
        

				if (m_mixerControl!=null)
					Marshal.ReleaseComObject( m_mixerControl );
				m_mixerControl= null;

				if (m_bitmapMixer!=null)
					Marshal.ReleaseComObject( m_bitmapMixer );
				m_bitmapMixer= null;

				if (m_pWC!=null)
				{
					Marshal.ReleaseComObject( m_pWC );
				}
				m_pWC= null;


				basicVideo	= null;
        
				basicAudio	= null;

				DsUtils.RemoveFilters(graphBuilder);

				if( graphBuilder != null )
					Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;

				if (m_memBmp!=null)
					m_memBmp.Dispose();
				m_memBmp=null;

        if (memDC!=null)
        {
          //if (memdc!=IntPtr.Zero) memDC.ReleaseHdc(memdc);
          Win32Support.DeleteDC(memdc);
          memdc=IntPtr.Zero;
          memDC.Dispose();
          memDC=null;
        }
				if (clientDC!=null)
				{
					if (hdc!=IntPtr.Zero) clientDC.ReleaseHdc(hdc);
					clientDC.Dispose();
					clientDC=null;
					hdc=IntPtr.Zero;
				}

				//if (m_surface!=null)
				//{
				//	m_surface.Dispose();
				//	m_surface=null;
				//}

				m_state = PlayState.Init;
				GUIGraphicsContext.form.Invalidate(true);
				GC.Collect();
			}
			catch( Exception ex)
			{
				Log.Write("VideoPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
			}
		}

		protected override void SetVideoPosition(System.Drawing.Rectangle rRect)
		{
			int hr =0;
			if (m_pWC!=null)
			{

				Log.Write("set videorect {0},{1}-{2},{3}", rRect.Left,rRect.Top,rRect.Right,rRect.Bottom);
				//video
				VMR9NormalizedRect rect1 = new VMR9NormalizedRect();
				rect1.left  = ((float)rRect.Left)  / ((float)GUIGraphicsContext.Width);
				rect1.right = ((float)rRect.Right) / ((float)GUIGraphicsContext.Width);

				rect1.top    = ((float)rRect.Top)    / ((float)GUIGraphicsContext.Height);
				rect1.bottom = ((float)rRect.Bottom) / ((float)GUIGraphicsContext.Height);

				hr = m_mixerControl.SetOutputRect(0, rect1);				

				//gui
				rect1.left=0f;
				rect1.right=1f;
				rect1.top=0f;
				rect1.right=1f;
				m_mixerControl.SetOutputRect(1, rect1);				

			}
		}



		protected override void OnProcess()
		{
			if (!m_bVisible) return; 
			

      if(!GUIWindowManager.NeedRefresh()) return;
			memDC.FillRectangle(Brushes.Black, 0 , 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height);
			GUIGraphicsContext.graphics=memDC;

			bool bRenderGUIFirst=false;
			if (!GUIGraphicsContext.IsFullScreenVideo )
			{
				if (GUIGraphicsContext.ShowBackground)
				{
					bRenderGUIFirst=true;
				}
			}
			if (bRenderGUIFirst)
			{
				m_mixerControl.SetZOrder(0,1);
				m_mixerControl.SetZOrder(1,0);
			}
			else
			{
				m_mixerControl.SetZOrder(0,0);
				m_mixerControl.SetZOrder(1,1);
			}


			GUIGraphicsContext.RenderGUI.RenderFrame(0);
			GUIGraphicsContext.graphics=null;
			bitmap.HDC = memdc ;  
			bitmap.pDDS=IntPtr.Zero;
			bitmap.dwFlags = 0x2+0x8; // 0x2=use HDC , 0x8=use colorkey  (0x04=use directdrawsurface)

/*

			if (m_renderFrame!=null) m_renderFrame.RenderFrame();
			bitmap.pDDS=DsUtils.GetUnmanagedSurface(m_surface);
			bitmap.HDC = IntPtr.Zero;  
			bitmap.dwFlags = 0x08; // 0x2=use HDC , 0x8=use colorkey  (0x04=use directdrawsurface)
*/			
			bitmap.rSrc = new DsRECT();
			bitmap.rSrc.Top = 0;
			bitmap.rSrc.Bottom = GUIGraphicsContext.Height;
			bitmap.rSrc.Left = 0;
			bitmap.rSrc.Right = GUIGraphicsContext.Width;
			bitmap.fAlpha = 1f; // show GUI 
			bitmap.color = new RGB();
			bitmap.color.blu = 0;   //key color=black
			bitmap.color.green = 0;
			bitmap.color.red = 0;
			bitmap.rDest = new VMR9NormalizedRect();
			bitmap.rDest.top = 0f;
			bitmap.rDest.bottom = 1f;
			bitmap.rDest.left = 0f;
			bitmap.rDest.right = 1;
			bitmap.dwFilterMode = 1;

			int hr = m_bitmapMixer.SetAlphaBitmap(bitmap);

		}
	}
}
