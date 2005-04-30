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

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
			
			Log.Write("StreamBufferPlayer9: GetInterfaces()");
      Type comtype = null;
      object comobj = null;
      
      //switch back to directx fullscreen mode
			
			Log.Write("StreamBufferPlayer9: switch to fullscreen mode");
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIWindowManager.SendMessage(msg);

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
				Vmr9= new VMR9Util("mytv");
				Vmr9.AddVMR9(graphBuilder);			
				Vmr9.Enable(false);	

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
        int hr=graphBuilder.AddFilter(filter, "SBE SOURCE");
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
        hr = fileSource.Load(filename, IntPtr.Zero);
        if (hr!=0) 
        {
          Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:Failed to open file:{0} :0x{1:x}",filename,hr);
          return false;
        }	


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
				}
				if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
				if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (strAudioRenderer.Length>0) DirectShowUtil.AddAudioRendererToGraph(graphBuilder,strAudioRenderer,false);
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

				// render output pins of SBE
        DirectShowUtil.RenderOutputPins(graphBuilder, (IBaseFilter)fileSource);

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        m_mediaSeeking = bufferSource as IStreamBufferMediaSeeking ;
        if (m_mediaSeeking==null)
        {
          Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:Unable to get IMediaSeeking interface#1");
          return false;
        }
        
//        Log.Write("StreamBufferPlayer9:SetARMode");
//        DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);

        Log.Write("StreamBufferPlayer9: set Deinterlace");

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
        Log.Write("StreamBufferPlayer9:cleanup DShow graph {0}",GUIGraphicsContext.InVmr9Render);
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

Log.Write("StreamBufferPlayer9:stop graph:{0}",GUIGraphicsContext.InVmr9Render);
          if( mediaCtrl != null )
          {
            hr = mediaCtrl.Stop();
          }
          
					Log.Write("StreamBufferPlayer9:stopped:{0}",GUIGraphicsContext.InVmr9Render);

					Log.Write("StreamBufferPlayer9:stop notifies");
					if( mediaEvt != null )
					{
						hr = mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
						mediaEvt = null;
					}
				//added from agree: check if Vmr9 already null
Log.Write("StreamBufferPlayer9:clean vmr9");
					if(Vmr9!=null)
				{
					Vmr9.RemoveVMR9();
					Vmr9.Release();
					Vmr9=null;
				}

				basicAudio	= null;
				m_mediaSeeking=null;
				mediaCtrl = null;


Log.Write("StreamBufferPlayer9:remove filters");
					DsUtils.RemoveFilters(graphBuilder);

Log.Write("StreamBufferPlayer9:remove graph from rot");
				if( rotCookie != 0 )
				DsROT.RemoveGraphFromRot( ref rotCookie );
				rotCookie=0;
Log.Write("StreamBufferPlayer9:remove graph ");
				if( graphBuilder != null )
				Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;

Log.Write("StreamBufferPlayer9:invalidate");

				GUIGraphicsContext.form.Invalidate(true);
				//GUIGraphicsContext.form.Update();
				//GUIGraphicsContext.form.Validate();
				m_state = PlayState.Init;
				GC.Collect();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer9:exception while cleaning DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }

Log.Write("StreamBufferPlayer9:switch");
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

		public override void Reset()
		{
			if (Vmr9==null || !GUIGraphicsContext.Vmr9Active || mediaCtrl==null) return;

			Log.Write("StreamBufferPlayer9:Reset graph");
			mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
			double pos=CurrentPosition;
			mediaCtrl.Stop();
			mediaCtrl.Run();
			SeekAbsolute(pos);
			mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
			m_state=PlayState.Playing;
		}

  }
}
