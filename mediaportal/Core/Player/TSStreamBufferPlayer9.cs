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
  public class TStreamBufferPlayer9 : BaseTStreamBufferPlayer
  {

		VMR9Util Vmr9 = null;
    public TStreamBufferPlayer9()
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
			//Log.Write("TStreamBufferPlayer9: GetInterfaces()");
      Type comtype = null;
      object comobj = null;
      
      //switch back to directx fullscreen mode
			
	//		Log.Write("TStreamBufferPlayer9: switch to fullscreen mode");
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIWindowManager.SendMessage(msg);
//Log.Write("TStreamBufferPlayer9: build graph");

      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:DirectX 9 not installed");
          return false;
        }
        comobj = Activator.CreateInstance( comtype );
        graphBuilder = (IGraphBuilder) comobj; comobj = null;
				Vmr9= new VMR9Util("mytv");
				Vmr9.AddVMR9(graphBuilder);			
				Vmr9.Enable(false);	

				bufferSource = new MPTransportStreamReader();
				IBaseFilter filter = (IBaseFilter) bufferSource;
				graphBuilder.AddFilter(filter, "MP TS Reader");
		
				IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
				int hr = fileSource.Load(filename, IntPtr.Zero);
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
				IBaseFilter videoCodec=null;
				if (strVideoCodec.Length>0) 
					videoCodec=DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
				if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (strAudioRenderer.Length>0) DirectShowUtil.AddAudioRendererToGraph(graphBuilder,strAudioRenderer,false);
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

				// render output pins of SBE
        DirectShowUtil.RenderOutputPins(graphBuilder, (IBaseFilter)fileSource);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
				m_mediaSeeking = graphBuilder as IMediaSeeking;
        
				hasVideo=true;
				if ( !Vmr9.IsVMR9Connected )
				{
					hasVideo=false;
					if (videoCodec!=null)
						graphBuilder.RemoveFilter(videoCodec);
					if (Vmr9!=null)
						Vmr9.RemoveVMR9();
					Vmr9=null;
				}

				return true;
      }
      catch( Exception  ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
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
			Cleanup();
		}

		void Cleanup()
		{
				if (graphBuilder==null) return;

        int hr;
        //Log.Write("TStreamBufferPlayer9:cleanup DShow graph {0}",GUIGraphicsContext.InVmr9Render);
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

//Log.Write("TStreamBufferPlayer9:stop graph:{0}",GUIGraphicsContext.InVmr9Render);
          if( mediaCtrl != null )
          {
            hr = mediaCtrl.Stop();
						while ((hr=Marshal.ReleaseComObject(mediaCtrl))>0);
						mediaCtrl=null;

          }
          
//					Log.Write("TStreamBufferPlayer9:stopped:{0}",GUIGraphicsContext.InVmr9Render);

//					Log.Write("TStreamBufferPlayer9:stop notifies");
					if( mediaEvt != null )
					{
						while ((hr=Marshal.ReleaseComObject(mediaEvt))>0);
						mediaEvt = null;
					}
				//added from agree: check if Vmr9 already null
//Log.Write("TStreamBufferPlayer9:clean vmr9");
					if(Vmr9!=null)
					{
						Vmr9.RemoveVMR9();
						Vmr9.Release();
						Vmr9=null;
					}

					if (basicAudio!=null) 
					{
						while ((hr=Marshal.ReleaseComObject(basicAudio))>0); 
						basicAudio	= null;
					}
					if (basicVideo!=null) 
					{
						while ((hr=Marshal.ReleaseComObject(basicVideo))>0); 
						basicVideo	= null;
					}
					if (m_mediaSeeking!=null) 
					{
						while ((hr=Marshal.ReleaseComObject(m_mediaSeeking))>0); 
						m_mediaSeeking=null;
					}
					bufferSource=null;
					if (videoWin!=null) 
					{
						while ((hr=Marshal.ReleaseComObject(videoWin))>0); 
						videoWin=null;
					}


//Log.Write("TStreamBufferPlayer9:remove filters");
					DsUtils.RemoveFilters(graphBuilder);

//Log.Write("TStreamBufferPlayer9:remove graph from rot");
					if( rotCookie != 0 )
						DsROT.RemoveGraphFromRot( ref rotCookie );
					rotCookie=0;
//Log.Write("TStreamBufferPlayer9:remove graph ");
					if( graphBuilder != null )
					{
						while ((hr=Marshal.ReleaseComObject( graphBuilder ))>0); 
						graphBuilder = null;
					}
//Log.Write("TStreamBufferPlayer9:invalidate");

				GUIGraphicsContext.form.Invalidate(true);
				m_state = PlayState.Init;
				GC.Collect();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:exception while cleaning DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }

//Log.Write("TStreamBufferPlayer9:switch");
			//switch back to directx windowed mode
			GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
			GUIWindowManager.SendMessage(msg);
			Log.Write("TStreamBufferPlayer9:cleanup done");
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
