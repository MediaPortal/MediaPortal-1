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
		GCHandle										myHandle;
		AllocatorWrapper.Allocator	allocator;
		public PlaneScene						m_scene=null;
		public bool									UseVMR9inMYTV=false;
		IRender											m_renderFrame;
		public IBaseFilter          VMR9Filter=null;

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

			Type comtype = Type.GetTypeFromCLSID( Clsid.VideoMixingRenderer9 );
			object comobj = Activator.CreateInstance( comtype );
			VMR9Filter=(IBaseFilter)comobj; comobj=null;
			if (VMR9Filter==null) 
			{
				Error.SetError("Unable to play movie","VMR9 is not installed");
				Log.Write("VMR9:Failed to get instance of VMR9 ");
				return ;
			}
			//IVMRFilterConfig9
			IVMRFilterConfig9 FilterConfig9 = VMR9Filter as IVMRFilterConfig9;
			if (FilterConfig9==null) 
			{
				Error.SetError("Unable to play movie","Unable to initialize VMR9");
				Log.Write("VMR9:Failed to get IVMRFilterConfig9 ");
				return ;
			}
			int hr = FilterConfig9.SetRenderingMode(VMR9.VMRMode_Renderless);
			if (hr!=0) 
			{
				Error.SetError("Unable to play movie","Unable to initialize VMR9");
				Log.Write("VMR9:Failed to set VMR9 to renderless mode");
				return ;
			}

			// needed to put VMR9 in mixing mode instead of pass-through mode
      
			hr = FilterConfig9.SetNumberOfStreams(1);
			if (hr!=0) 
			{
				Error.SetError("Unable to play movie","Unable to initialize VMR9");
				Log.Write("VMR9:Failed to set VMR9 streams to 1");
				return ;
			}


			hr = SetAllocPresenter(VMR9Filter, GUIGraphicsContext.form as Control);
			if (hr!=0) 
			{
				Error.SetError("Unable to play movie","Unable to initialize VMR9");
				Log.Write("VMR9:Failed to set VMR9 allocator/presentor");
				return ;
			}

			hr=graphBuilder.AddFilter(VMR9Filter,"VMR9");
			if (hr!=0) 
			{
				Error.SetError("Unable to play movie","Unable to initialize VMR9");
				Log.Write("VMR9:Failed to add vmr9 to filtergraph");
				return ;
			}
			GUIGraphicsContext.Vmr9Active=true;
		}

		/// <summary>
		/// Set the allocator/presentor for the vmr9 filter
		/// </summary>
		/// <param name="filter">VMR9 filter</param>
		/// <param name="control">Winform control which handles all notifications</param>
		/// <returns>
		/// 0 : allocator/presenter added
		/// -1: failed to set allocator/presenter 
		/// </returns>
		int SetAllocPresenter(IBaseFilter filter, Control control)
		{
			if (!UseVMR9inMYTV) return -1;
			IVMRSurfaceAllocatorNotify9 lpIVMRSurfAllocNotify = filter as IVMRSurfaceAllocatorNotify9;

			if (lpIVMRSurfAllocNotify == null)
			{
				Log.Write("VMR9:Failed to get IVMRSurfaceAllocatorNotify9");
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
				Log.Write("VMR9:Failed to get SetD3DDevice()");
				return hr;
			}
			// this must be global. If it gets garbage collected, pinning won't exist...
			myHandle = GCHandle.Alloc(allocator, GCHandleType.Pinned);
			hr = allocator.AdviseNotify(lpIVMRSurfAllocNotify);
			if (hr != 0)
			{
				Log.Write("VMR9:Failed to AdviseNotify()");
				return hr;
			}
			hr = lpIVMRSurfAllocNotify.AdviseSurfaceAllocator(0xACDCACDC, allocator);
			if (hr !=0)
			{
				Log.Write("VMR9:Failed to AdviseSurfaceAllocator()");
			}
			return hr;
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
			GUIGraphicsContext.Vmr9Active=false;
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
			if( VMR9Filter != null )
				Marshal.ReleaseComObject( VMR9Filter ); VMR9Filter = null;

		}


		/// <summary>
		/// returns the width of the video
		/// </summary>
		public int VideoWidth
		{
			get 
			{
				if (allocator==null) return 0;
				return allocator.NativeSize.Width;;
			}
		}      

		/// <summary>
		/// returns the height of the video
		/// </summary>
		public int VideoHeight
		{
			get 
			{
				if (allocator==null) return 0;
				return allocator.NativeSize.Height;;
			}
		}
		
		/// <summary>
		/// repaints the last frame
		/// </summary>
		public void Repaint()
		{
			if (allocator==null) return;
			if (!UseVMR9inMYTV) return;
			if (VMR9Filter==null) return;
			allocator.Repaint();
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
				if (allocator==null ||VMR9Filter==null ||!UseVMR9inMYTV) 
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

				//check if the input is connected to a video decoder
				pinIn.ConnectedTo(out pinConnected);
				if (pinConnected==null) 
				{
					//no pin is not connected so vmr9 is not possible
					GUIGraphicsContext.Vmr9Active=false;
					return false;
				}
				//all is ok, vmr9 is working
				return true;
			}//get {
		}//public bool IsVMR9Connected
	}//public class VMR9Util
}//namespace MediaPortal.Player 
