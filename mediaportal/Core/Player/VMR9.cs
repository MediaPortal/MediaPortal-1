using System;
using System.Runtime.InteropServices; 
using System.Windows.Forms;
using DShowNET;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.Player 
{
	/// <summary>
	/// General helper class to add the Video Mixing Render9 filter to a graph
	/// , set it to renderless mode and provide it our own allocator/presentor
	/// This will allow us to render the video to a direct3d texture
	/// which we can use to draw the transparent OSD on top of it
	/// Some classes which work together:
	///  VMR9Util								: general helper class
	///  AllocatorWrapper.cs		: implements our own allocator/presentor for vmr9 by implementing
	///                           IVMRSurfaceAllocator9 and IVMRImagePresenter9
	///  PlaneScene.cs          : class which draws the video texture onscreen and mixes it with the GUI, OSD,...                          
	/// </summary>
	public class VMR9Util
	{
		public PlaneScene						m_scene=null;
		public bool									UseVMR9inMYTV=false;
		IRender											m_renderFrame;
		public IBaseFilter          VMR9Filter=null;
		public int                  textureCount=0;
		int												  videoHeight,videoWidth;
		DirectShowHelperLib.VMR9HelperClass vmr9Helper=null;
		float                       lastTime;
		int													frameCounter=0;
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">
		/// key in mediaportal.xml to check if vmr9 should be enabled or not
		/// </param>
		public VMR9Util(string key)
		{
			
			// add vmr9 if necessary
			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				int iUseVMR9inMYTV=xmlreader.GetValueAsInt(key,"vmr9",0);
				if (iUseVMR9inMYTV!=0) UseVMR9inMYTV=true;
				else UseVMR9inMYTV=false;

				m_renderFrame=GUIGraphicsContext.RenderGUI;
			}
			if (GUIGraphicsContext.DX9Device==null) UseVMR9inMYTV=false;
			if (m_renderFrame==null) UseVMR9inMYTV=false;
		}

		public void SetTextureCount(int count)
		{
			textureCount=count;
		}

		/// <summary>
		/// Add VMR9 filter to graph and configure it
		/// </summary>
		/// <param name="graphBuilder"></param>
		public void AddVMR9(IGraphBuilder graphBuilder)
		{
			if (!UseVMR9inMYTV) return;
			if (VMR9Filter!=null)
			{
				RemoveVMR9();
			}
			Log.Write("VMR9Helper:AddVMR9()");

			Type comtype = Type.GetTypeFromCLSID( Clsid.VideoMixingRenderer9 );
			object comobj = Activator.CreateInstance( comtype );
			VMR9Filter=(IBaseFilter)comobj; comobj=null;
			if (VMR9Filter==null) 
			{
				Error.SetError("Unable to play movie","VMR9 is not installed");
				Log.WriteFile(Log.LogType.Log,true,"VMR9Helper:Failed to get instance of VMR9 ");
				return ;
			}

			IntPtr hMonitor;
			AdapterInformation ai = Manager.Adapters.Default;
			hMonitor = Manager.GetAdapterMonitor(ai.Adapter);
			IntPtr upDevice = DShowNET.DsUtils.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

			m_scene = new PlaneScene(m_renderFrame,this);
			m_scene.Init();
			vmr9Helper = new DirectShowHelperLib.VMR9HelperClass();
			DirectShowHelperLib.IBaseFilter baseFilter = VMR9Filter as DirectShowHelperLib.IBaseFilter;

			vmr9Helper.Init(m_scene,  (uint)upDevice.ToInt32(), baseFilter,(uint)hMonitor.ToInt32());

			int hr=graphBuilder.AddFilter(VMR9Filter,"Video Mixing Renderer 9");
			if (hr!=0) 
			{
				Error.SetError("Unable to play movie","Unable to initialize VMR9");
				Log.WriteFile(Log.LogType.Log,true,"VMR9Helper:Failed to add vmr9 to filtergraph");
				return ;
			}
			SetDeinterlacePrefs();
			GUIGraphicsContext.Vmr9Active=true;
		}
		public void Release()
		{
		}

		/// <summary>
		/// removes the vmr9 filter from the graph and free up all unmanaged resources
		/// </summary>
		public void RemoveVMR9()
		{
			if (!UseVMR9inMYTV) return;
			Log.Write("VMR9Helper:RemoveVMR9()");
			GUIGraphicsContext.Vmr9Active=false;
			if (vmr9Helper!=null)
			{
				vmr9Helper=null;
			}
			
			if (m_scene!=null)
			{
				m_scene.Stop();
				m_scene.Deinit();
				m_scene=null;
			}
			if( VMR9Filter != null )
				Marshal.ReleaseComObject( VMR9Filter ); VMR9Filter = null;

		}

		public int FrameCounter
		{
			get { return frameCounter;}
			set { frameCounter=value;}
		}

		/// <summary>
		/// returns the width of the video
		/// </summary>
		public int VideoWidth
		{
			get 
			{
				return videoWidth;
			}
			set
			{
				videoWidth=value;
			}
		}      

		/// <summary>
		/// returns the height of the video
		/// </summary>
		public int VideoHeight
		{
			get 
			{
				return videoHeight;
			}
			set
			{
				videoHeight=value;
			}
		}
		
		/// <summary>
		/// repaints the last frame
		/// </summary>
		public void Repaint()
		{
			if (!UseVMR9inMYTV) return;
			if (VMR9Filter==null) return;
			m_scene.Repaint();
		}

		public void Process()
		{
			float time = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);
			if (time - lastTime >= 0.2f)
			{
				GUIGraphicsContext.Vmr9FPS    = ((float)frameCounter) / (time - lastTime);
				lastTime=time;
				frameCounter=0;
			}
		}

		/// <summary>
		/// This method returns true if VMR9 is enabled AND WORKING!
		/// this allows players to check if if VMR9 is working after setting up the playing graph
		/// by checking if VMR9 is possible they can for example fallback to the overlay device
		/// </summary>
		public bool IsVMR9Connected
		{
			get
			{
				// check if vmr9 is enabled and if initialized
				if (VMR9Filter==null ||!UseVMR9inMYTV) 
				{
					GUIGraphicsContext.Vmr9Active=false;
					return false;
				}

				//get the VMR9 input pin#0 is connected
				IPin pinIn,pinConnected;
				DsUtils.GetPin(VMR9Filter, PinDirection.Input,0,out pinIn);
				if (pinIn==null) 
				{
					//no input pin found, vmr9 is not possible
					GUIGraphicsContext.Vmr9Active=false;
					return false;
				}
				//Marshal.ReleaseComObject(pinIn);

				//check if the input is connected to a video decoder
				pinIn.ConnectedTo(out pinConnected);
				if (pinConnected==null) 
				{
					//no pin is not connected so vmr9 is not possible
					GUIGraphicsContext.Vmr9Active=false;
					return false;
				}
				//Marshal.ReleaseComObject(pinConnected);
				//all is ok, vmr9 is working
				return true;
			}//get {
		}//public bool IsVMR9Connected
		public void SetDeinterlacePrefs()
		{
			
			int DeInterlaceMode=3;
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				//None
				//Bob
				//Weave
				//Best
				DeInterlaceMode= xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
			}
			vmr9Helper.SetDeinterlacePrefs((uint)DeInterlaceMode);
		}
		public void SetDeinterlaceMode()
		{
			vmr9Helper.SetDeinterlaceMode();
		}
	}//public class VMR9Util
}//namespace MediaPortal.Player 
