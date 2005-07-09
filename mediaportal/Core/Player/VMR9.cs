using System;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing;

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
		static public VMR9Util g_vmr9=null;
		static int instanceCounter = 0;

    public PlaneScene m_scene = null;
    public bool UseVMR9inMYTV = false;
    IRender m_renderFrame;
    public IBaseFilter VMR9Filter = null;
    public int textureCount = 0;
		int videoHeight, videoWidth;
		int videoAspectRatioX, videoAspectRatioY;
    DirectShowHelperLib.VMR9HelperClass vmr9Helper = null;
		IQualProp quality=null;
    int frameCounter = 0;
    DateTime repaintTimer = DateTime.Now;
	  IVMRMixerBitmap9 m_mixerBitmap=null;
		IGraphBuilder m_graphBuilder=null;
		bool vmr9Initialized=false;
    enum Vmr9PlayState
    {
      Playing,
      Repaint
    }
    Vmr9PlayState currentVmr9State = Vmr9PlayState.Playing;
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key">
    /// key in mediaportal.xml to check if vmr9 should be enabled or not
    /// </param>
    public VMR9Util(string key)
    {
      // add vmr9 if necessary
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        int iUseVMR9inMYTV = xmlreader.GetValueAsInt(key, "vmr9", 0);
        if (iUseVMR9inMYTV != 0) UseVMR9inMYTV = true;
        else UseVMR9inMYTV = false;

        m_renderFrame = GUIGraphicsContext.RenderGUI;
      }
      if (GUIGraphicsContext.DX9Device == null) UseVMR9inMYTV = false;
      if (m_renderFrame == null) UseVMR9inMYTV = false;
			if (g_vmr9!=null || GUIGraphicsContext.Vmr9Active) 
				UseVMR9inMYTV = false;
    }

    public void SetTextureCount(int count)
    {
      textureCount = count;
    }

    /// <summary>
    /// Add VMR9 filter to graph and configure it
    /// </summary>
    /// <param name="graphBuilder"></param>
    public void AddVMR9(IGraphBuilder graphBuilder)
    {
      if (!UseVMR9inMYTV) return;
      if (vmr9Initialized) return;
			Log.Write("VMR9Helper:AddVmr9");
			if (instanceCounter != 0)
			{
				Log.WriteFile(Log.LogType.Log, true, "VMR9Helper:Multiple instances of VMR9 running!!!");
				throw new Exception("VMR9Helper:Multiple instances of VMR9 running!!!");
			}

      Type comtype = Type.GetTypeFromCLSID(Clsid.VideoMixingRenderer9);
      object comobj = Activator.CreateInstance(comtype);
      VMR9Filter = (IBaseFilter)comobj; comobj = null;
      if (VMR9Filter == null)
      {
        Error.SetError("Unable to play movie", "VMR9 is not installed");
        Log.WriteFile(Log.LogType.Log, true, "VMR9Helper:Failed to get instance of VMR9 ");
        return;
      }

      IntPtr hMonitor;
      AdapterInformation ai = Manager.Adapters.Default;
      hMonitor = Manager.GetAdapterMonitor(ai.Adapter);
      IntPtr upDevice = DShowNET.DsUtils.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      m_scene = new PlaneScene(m_renderFrame, this);
      m_scene.Init();
      vmr9Helper = new DirectShowHelperLib.VMR9HelperClass();
      DirectShowHelperLib.IBaseFilter baseFilter = VMR9Filter as DirectShowHelperLib.IBaseFilter;

      vmr9Helper.Init(m_scene, (uint)upDevice.ToInt32(), baseFilter, (uint)hMonitor.ToInt32());

      int hr = graphBuilder.AddFilter(VMR9Filter, "Video Mixing Renderer 9");
      if (hr != 0)
      {
				vmr9Helper.Deinit();
				m_scene.Stop();
				m_scene.Deinit();
				m_scene=null;
				vmr9Helper=null;
				Marshal.ReleaseComObject(VMR9Filter);
				VMR9Filter=null;
        Error.SetError("Unable to play movie", "Unable to initialize VMR9");
        Log.WriteFile(Log.LogType.Log, true, "VMR9Helper:Failed to add vmr9 to filtergraph");
        return;
      }

			quality = VMR9Filter as IQualProp ;
			m_mixerBitmap=baseFilter as IVMRMixerBitmap9;
			m_graphBuilder=graphBuilder;			
			instanceCounter++;
			GUIGraphicsContext.Vmr9Active = true;
			g_vmr9=this;
			vmr9Initialized=true;
			SetDeinterlacePrefs();
			Log.Write("VMR9Helper:Vmr9 Added");
		}

    public void Release()
    {
    }

    /// <summary>
    /// removes the vmr9 filter from the graph and free up all unmanaged resources
    /// </summary>
    public void RemoveVMR9()
    {
			if (vmr9Initialized)
			{
				Log.Write("VMR9Helper:RemoveVMR9");
				if (vmr9Helper != null)
				{
					Log.Write("VMR9Helper:stop vmr9 helper");
					vmr9Helper.Deinit();
					vmr9Helper = null;
				}

				if (m_scene != null)
				{
					Log.Write("VMR9Helper:stop planescene");
					m_scene.Stop();
					instanceCounter--;
					m_scene.Deinit();
					m_scene = null;
					GUIGraphicsContext.Vmr9Active = false;
					GUIGraphicsContext.Vmr9FPS=0f;
					GUIGraphicsContext.InVmr9Render=false;
					currentVmr9State = Vmr9PlayState.Playing;
				}
				int result;
				if (quality != null)
					result=Marshal.ReleaseComObject(quality); 
				quality = null;
				
				if (m_mixerBitmap!= null)
					result=Marshal.ReleaseComObject(m_mixerBitmap); 
				m_mixerBitmap = null;
					
				if (VMR9Filter != null)
				{
					result=Marshal.ReleaseComObject(VMR9Filter); 
					VMR9Filter = null;
					try
					{
						m_graphBuilder.RemoveFilter(VMR9Filter);
					}
					catch(Exception){}
					m_graphBuilder=null;
				}
				g_vmr9=null;
			}
		}

    public int FrameCounter
    {
      get
      {
          return frameCounter;
      }
      set
      {
          frameCounter = value;
      }
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
        videoWidth = value;
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
        videoHeight = value;
      }
    }


		/// <summary>
		/// returns the width of the video
		/// </summary>
		public int VideoAspectRatioX
		{
			get
			{
				return videoAspectRatioX;
			}
			set
			{
				videoAspectRatioX = value;
			}
		}

		/// <summary>
		/// returns the height of the video
		/// </summary>
		public int VideoAspectRatioY
		{
			get
			{
				return videoAspectRatioY;
			}
			set
			{
				videoAspectRatioY = value;
			}
		}

    /// <summary>
    /// repaints the last frame
    /// </summary>
    public void Repaint()
    {
      if (!vmr9Initialized) return;
      if (m_scene.Enabled == false) return;
      if (currentVmr9State == Vmr9PlayState.Playing)
      {
        Log.Write("VMR9Helper: playing->repaint");
        currentVmr9State = Vmr9PlayState.Repaint;
				m_scene.DrawVideo=false;
      }
      m_scene.Repaint();
    }

		public IQualProp Quality
		{
			get { return quality;}
		}
    public void Process()
		{
			if (!vmr9Initialized) return;
			if( !GUIGraphicsContext.Vmr9Active) return;
      TimeSpan ts = DateTime.Now - repaintTimer;
      if (ts.TotalMilliseconds > 1000)
      {
        int frames = FrameCounter;
        frames *= 1000;
        GUIGraphicsContext.Vmr9FPS = ((float)frames) / ((float)ts.TotalMilliseconds);
        //Log.Write("VMR9Helper:frames:{0} fps:{1} time:{2}", frames, GUIGraphicsContext.Vmr9FPS,ts.TotalMilliseconds);
        repaintTimer = DateTime.Now;
        FrameCounter = 0;
				VideoRendererStatistics.Update(quality);
      }
      if (GUIGraphicsContext.Vmr9FPS > 1f)
      {
        if (currentVmr9State == Vmr9PlayState.Repaint)
        {
          Log.Write("VMR9Helper: repaint->playing");
          currentVmr9State = Vmr9PlayState.Playing;
					m_scene.DrawVideo=true;
        }
      }
    }
	  /// <summary>
	  /// returns a IVMRMixerBitmap9 interface
	  /// </summary>
	  public IVMRMixerBitmap9 MixerBitmapInterface
	  {
		  get{return m_mixerBitmap;}
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
				
				if (!vmr9Initialized) return false;
        // check if vmr9 is enabled and if initialized
        if (VMR9Filter == null || !UseVMR9inMYTV)
        {
					Log.Write("vmr9: not used or no filter:{0} {1:x}",UseVMR9inMYTV,VMR9Filter);
          return false;
        }

        //get the VMR9 input pin#0 is connected
				for (int i=0; i < 3; ++i)
				{
					IPin pinIn, pinConnected;
					int hr= DsUtils.GetPin(VMR9Filter, PinDirection.Input, i, out pinIn);
					if (pinIn == null)
					{
						//no input pin found, vmr9 is not possible
						Log.Write("vmr9: no input pin {0} found:{1:x}",i,hr);
						continue;
					}

					//check if the input is connected to a video decoder
					hr=pinIn.ConnectedTo(out pinConnected);
					if (pinConnected == null)
					{
						//no pin is not connected so vmr9 is not possible
						Log.Write("vmr9: pin:{0} not connected:{1:x}",i, hr);
					}
					else
					{
						Log.Write("vmr9: pin:{0} is connected",i);
						if (pinIn!=null)
							Marshal.ReleaseComObject(pinIn);
						if (pinConnected!=null)
							Marshal.ReleaseComObject(pinConnected);
						return true;
					}
					if (pinIn!=null)
						Marshal.ReleaseComObject(pinIn);
					if (pinConnected!=null)
						Marshal.ReleaseComObject(pinConnected);
				}
				return false;
      }//get {
    }//public bool IsVMR9Connected

    public void SetDeinterlacePrefs()
		{
			if (!vmr9Initialized) return;
      if (vmr9Helper == null) return;
      int DeInterlaceMode = 3;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        //None
        //Bob
        //Weave
        //Best
        DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
      }
      vmr9Helper.SetDeinterlacePrefs((uint)DeInterlaceMode);
    }
    public void SetDeinterlaceMode()
		{
			if (!vmr9Initialized) return;
			if (vmr9Helper == null) return;
			int DeInterlaceMode = 3;
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				//None
				//Bob
				//Weave
				//Best
				DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
			}
			vmr9Helper.SetDeinterlaceMode(DeInterlaceMode);
    }
    public void Enable(bool onOff)
		{
			if (!vmr9Initialized) return;
      if (m_scene != null) m_scene.Enabled = onOff;
    }

		public bool SaveBitmap(System.Drawing.Bitmap bitmap,bool show,bool transparent,float alphaValue)
		{
			if (!vmr9Initialized) return false;
			if (VMR9Filter==null) 
				return false;
			
			if(MixerBitmapInterface==null)
				return false;

			if(GUIGraphicsContext.Vmr9Active==false)
			{
				Log.Write("SaveVMR9Bitmap() failed, no VMR9");
				return false;
			}
			int hr=0;
			// transparent image?
			using (System.IO.MemoryStream mStr=new System.IO.MemoryStream())
			{
				if(bitmap!=null)
				{
					if(transparent==true)
						bitmap.MakeTransparent(Color.Black);
					bitmap.Save(mStr,System.Drawing.Imaging.ImageFormat.Bmp);
					mStr.Position=0;
				}
			
				VMR9AlphaBitmap bmp=new VMR9AlphaBitmap();

				if(show==true)
				{

					// get AR for the bitmap
					Rectangle src,dest;
					VMR9Util.g_vmr9.GetVideoWindows(out src,out dest);
					
					int width=VMR9Util.g_vmr9.VideoWidth;
					int height=VMR9Util.g_vmr9.VideoHeight;

					float xx=(float)src.X/width;
					float yy=(float)src.Y/height;
					float fx=(float)(src.X+src.Width)/width;
					float fy=(float)(src.Y+src.Height)/height;
					//

					using (Microsoft.DirectX.Direct3D.Surface surface=GUIGraphicsContext.DX9Device.CreateOffscreenPlainSurface(GUIGraphicsContext.Width,GUIGraphicsContext.Height,Microsoft.DirectX.Direct3D.Format.X8R8G8B8,Microsoft.DirectX.Direct3D.Pool.SystemMemory))
					{
						Microsoft.DirectX.Direct3D.SurfaceLoader.FromStream(surface,mStr,Microsoft.DirectX.Direct3D.Filter.None,0);
						bmp.dwFlags=4|8;
						bmp.color.blu=0;
						bmp.color.green=0;
						bmp.color.red=0;
						unsafe
						{
							bmp.pDDS=(System.IntPtr)surface.UnmanagedComPointer;
						}
						bmp.rDest=new VMR9NormalizedRect();
						bmp.rDest.top=yy;
						bmp.rDest.left=xx;
						bmp.rDest.bottom=fy;
						bmp.rDest.right=fx;
						bmp.fAlpha=alphaValue;
						//Log.Write("SaveVMR9Bitmap() called");
						hr=VMR9Util.g_vmr9.MixerBitmapInterface.SetAlphaBitmap(bmp);
						if(hr!=0)
						{
							//Log.Write("SaveVMR9Bitmap() failed: error {0:X} on SetAlphaBitmap()",hr);
							return false;
						}
					}					
				}
				else
				{
					bmp.dwFlags=1;
					bmp.color.blu=0;
					bmp.color.green=0;
					bmp.color.red=0;
					bmp.rDest=new VMR9NormalizedRect();
					bmp.rDest.top=0.0f;
					bmp.rDest.left=0.0f;
					bmp.rDest.bottom=1.0f;
					bmp.rDest.right=1.0f;
					bmp.fAlpha=alphaValue;
					hr=VMR9Util.g_vmr9.MixerBitmapInterface.UpdateAlphaBitmapParameters(bmp);
					if(hr!=0)
					{
						return false;
					}
				}
			}
			// dispose
			return true;
		}// savevmr9bitmap

		public void GetVideoWindows(out System.Drawing.Rectangle rSource,out System.Drawing.Rectangle rDest)
		{
			MediaPortal.GUI.Library.Geometry			m_geometry = new MediaPortal.GUI.Library.Geometry();
			// get the window where the video/tv should be shown
			float x  = GUIGraphicsContext.VideoWindow.X;
			float y  = GUIGraphicsContext.VideoWindow.Y;
			float nw = GUIGraphicsContext.VideoWindow.Width;
			float nh = GUIGraphicsContext.VideoWindow.Height; 

			//sanity checks
			if (nw > GUIGraphicsContext.OverScanWidth)
				nw = GUIGraphicsContext.OverScanWidth;
			if (nh > GUIGraphicsContext.OverScanHeight)
				nh = GUIGraphicsContext.OverScanHeight;

			//are we supposed to show video in fullscreen or in a preview window?
			if (GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.ShowBackground)
			{
				//yes fullscreen, then use the entire screen
				x  = GUIGraphicsContext.OverScanLeft;
				y  = GUIGraphicsContext.OverScanTop;
				nw = GUIGraphicsContext.OverScanWidth;
				nh = GUIGraphicsContext.OverScanHeight;
			}
        
			//calculate the video window according to the current aspect ratio settings
			float fVideoWidth  = (float)VideoWidth;
			float fVideoHeight = (float)VideoHeight;
			m_geometry.ImageWidth   = (int)fVideoWidth;
			m_geometry.ImageHeight  = (int)fVideoHeight;
			m_geometry.ScreenWidth  = (int)nw;
			m_geometry.ScreenHeight = (int)nh;
			m_geometry.ARType       = GUIGraphicsContext.ARType;
			m_geometry.PixelRatio   = GUIGraphicsContext.PixelRatio;
			m_geometry.GetWindow(VideoAspectRatioX, VideoAspectRatioY,out rSource, out rDest);
			rDest.X += (int)x;
			rDest.Y += (int)y;
			m_geometry=null;
		}
  }//public class VMR9Util
}//namespace MediaPortal.Player 
