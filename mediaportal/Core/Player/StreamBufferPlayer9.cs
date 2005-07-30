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
  public class StreamBufferPlayer9 : BaseStreamBufferPlayer
  {

		VMR9Util Vmr9 = null;
    public StreamBufferPlayer9()
    {
    }
		protected override void OnInitialized()
		{
			if (Vmr9!=null)
			{
				Vmr9.Enable(true);
				m_bUpdateNeeded=true;
				SetVideoWindow();
			}
		}
		public override void SetVideoWindow()
		{
			if (GUIGraphicsContext.IsFullScreenVideo!= m_bFullScreen)
			{
				m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
				m_bUpdateNeeded=true;
			}

			if (!m_bUpdateNeeded) return;
      
			m_bUpdateNeeded=false;
			m_bStarted=true;

		}


    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
		  Speed=1;	
			//Log.Write("StreamBufferPlayer9: GetInterfaces()");
      Type comtype = null;
      object comobj = null;
      
      //switch back to directx fullscreen mode
			
	//		Log.Write("StreamBufferPlayer9: switch to fullscreen mode");
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIWindowManager.SendMessage(msg);
//Log.Write("StreamBufferPlayer9: build graph");

      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:DirectX 9 not installed");
          return false;
        }
        comobj = Activator.CreateInstance( comtype );
        graphBuilder = (IGraphBuilder) comobj; comobj = null;
//Log.Write("StreamBufferPlayer9: add vmr9");

				Vmr9= new VMR9Util("mytv");
				Vmr9.AddVMR9(graphBuilder);			
				Vmr9.Enable(false);	


				int hr;
				m_StreamBufferConfig	= new StreamBufferConfig();
				streamConfig2	= m_StreamBufferConfig as IStreamBufferConfigure2;
				if (streamConfig2!=null)
				{
					// setting the StreamBufferEngine registry key
					IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
					IStreamBufferInitialize pTemp = (IStreamBufferInitialize) streamConfig2;
					IntPtr subKey = IntPtr.Zero;

					RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
					hr=pTemp.SetHKEY(subKey);
					hr=streamConfig2.SetFFTransitionRates(8,32);	
					//Log.Write("set FFTransitionRates:{0:X}",hr);
					
					uint max,maxnon;
					hr=streamConfig2.GetFFTransitionRates(out max,out maxnon);	

					streamConfig2.GetBackingFileCount(out minBackingFiles, out maxBackingFiles);
					streamConfig2.GetBackingFileDuration(out backingFileDuration);

				}
				//Log.Write("StreamBufferPlayer9: add sbe");

				// create SBE source
        Guid clsid = Clsid.StreamBufferSource;
        Guid riid = typeof(IStreamBufferSource).GUID;
        Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
        bufferSource = (IStreamBufferSource) comObj; comObj = null;
        if (bufferSource==null) 
        {
          Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:Failed to create instance of SBE (do you have WinXp SP1?)");
          return false;
        }	

		
        IBaseFilter filter = (IBaseFilter) bufferSource;
        hr=graphBuilder.AddFilter(filter, "SBE SOURCE");
        if (hr!=0) 
        {
          Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:Failed to add SBE to graph");
          return false;
        }	
		
        IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
        if (fileSource==null) 
        {
          Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:Failed to get IFileSourceFilter");
          return false;
        }	


//Log.Write("StreamBufferPlayer9: open file:{0}",filename);
				hr = fileSource.Load(filename, IntPtr.Zero);
        if (hr!=0) 
        {
          Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:Failed to open file:{0} :0x{1:x}",filename,hr);
          return false;
        }	


//Log.Write("StreamBufferPlayer9: add codecs");
				// add preferred video & audio codecs
				string strVideoCodec="";
        string strAudioCodec="";
				string strAudioRenderer="";
        bool   bAddFFDshow=false;
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
					strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
					strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
					strAudioRenderer=xmlreader.GetValueAsString("mytv","audiorenderer","");
					string strValue=xmlreader.GetValueAsString("mytv","defaultar","normal");
					if (strValue.Equals("zoom")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
					if (strValue.Equals("stretch")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
					if (strValue.Equals("normal")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
					if (strValue.Equals("original")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
					if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
					if (strValue.Equals("panscan")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;

				}
				if (strVideoCodec.Length>0) videoCodecFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
				if (strAudioCodec.Length>0) audioCodecFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (strAudioRenderer.Length>0) audioRendererFilter=DirectShowUtil.AddAudioRendererToGraph(graphBuilder,strAudioRenderer,false);
				if (bAddFFDshow) ffdShowFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

				// render output pins of SBE
        DirectShowUtil.RenderOutputPins(graphBuilder, (IBaseFilter)fileSource);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
				m_mediaSeeking = bufferSource as IStreamBufferMediaSeeking ;
				m_mediaSeeking2= bufferSource as IStreamBufferMediaSeeking2 ;
				if (m_mediaSeeking==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"Unable to get IMediaSeeking interface#1");
				}
				if (m_mediaSeeking2==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"Unable to get IMediaSeeking interface#2");
				}
        
//        Log.Write("StreamBufferPlayer9:SetARMode");
//        DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);

        //Log.Write("StreamBufferPlayer9: set Deinterlace");

				if ( !Vmr9.IsVMR9Connected )
				{
					//VMR9 is not supported, switch to overlay
					Log.Write("StreamBufferPlayer9: switch to overlay");
					mediaCtrl=null;
					Cleanup();
					return base.GetInterfaces(filename);
				}

				Vmr9.SetDeinterlaceMode();
				return true;

      }
      catch( Exception  ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
        return false;
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
        //Log.Write("StreamBufferPlayer9:cleanup DShow graph {0}",GUIGraphicsContext.InVmr9Render);
        try 
        {
					if(Vmr9!=null)
						Vmr9.Enable(false);
					int counter=0;
					while (GUIGraphicsContext.InVmr9Render)
					{
						counter++;
						System.Threading.Thread.Sleep(1);
						if (counter >200) break;
					}

					if( mediaCtrl != null )
					{
						hr = mediaCtrl.Stop();
					}
					mediaCtrl=null;
					mediaEvt = null;
					m_mediaSeeking=null;
					m_mediaSeeking2=null;
					videoWin=null;
					basicAudio	= null;
					basicVideo	= null;
					bufferSource=null;
		
					if (streamConfig2!=null) 
					{
						while((hr=Marshal.ReleaseComObject(streamConfig2))>0); 
						streamConfig2=null;
					}

					m_StreamBufferConfig=null;

					if(Vmr9!=null)
					{
						Vmr9.RemoveVMR9();
						Vmr9.Release();
						Vmr9=null;
					}
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

					DsUtils.RemoveFilters(graphBuilder);

					if( rotCookie != 0 )
						DsROT.RemoveGraphFromRot( ref rotCookie );
					rotCookie=0;
					if( graphBuilder != null )
					{
						while((hr=Marshal.ReleaseComObject( graphBuilder ))>0); 
						graphBuilder = null;
					}

				GUIGraphicsContext.form.Invalidate(true);
				m_state = PlayState.Init;
				GC.Collect();GC.Collect();GC.Collect();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:exception while cleaning DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }

//Log.Write("StreamBufferPlayer9:switch");
			//switch back to directx windowed mode
			GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
			GUIWindowManager.SendMessage(msg);
			Log.Write("StreamBufferPlayer9:cleanup done");
		}

    protected override void OnProcess()
		{
			if (Vmr9!=null)
			{
				m_iVideoWidth=Vmr9.VideoWidth;
				m_iVideoHeight=Vmr9.VideoHeight;
			}
			if((GUIGraphicsContext.Vmr9Active && Vmr9!=null))
			{
				Vmr9.Process();
				if (GUIGraphicsContext.Vmr9FPS < 1f)
				{
					Vmr9.Repaint();// repaint vmr9
				}
			}
    }


  }
}
