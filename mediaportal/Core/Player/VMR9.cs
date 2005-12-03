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
	/// // {324FAA1F-7DA6-4778-833B-3993D8FF4151}

	[ComVisible(true), ComImport,
	Guid("324FAA1F-7DA6-4778-833B-3993D8FF4151"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IVMR9PresentCallback
	{
		[PreserveSig]
		int PresentImage(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint dwImg);
		[PreserveSig]
		int PresentSurface(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint dwImg);
	}
  public class VMR9Util
  {

		const uint MixerPref_RenderTargetMask=0x000FF000;
		const uint MixerPref_RenderTargetYUV=0x00002000;
		#region imports
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern bool Vmr9Init(IVMR9PresentCallback callback, uint dwD3DDevice, IBaseFilter vmr9Filter,uint monitor);
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void Vmr9Deinit();
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void Vmr9SetDeinterlaceMode(Int16 mode);
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void Vmr9SetDeinterlacePrefs(uint dwMethod);
		#endregion

		static public VMR9Util g_vmr9=null;
		static int instanceCounter = 0;

    public PlaneScene m_scene = null;
    public bool UseVMR9inMYTV = false;
    IRender m_renderFrame;
    public IBaseFilter VMR9Filter = null;
    public int textureCount = 0;
		int videoHeight, videoWidth;
		int videoAspectRatioX, videoAspectRatioY;
    
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
			if (!GUIGraphicsContext.VMR9Allowed)
			{
				UseVMR9inMYTV = false;
				return;
			}
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
			//Log.Write("VMR9Helper:AddVmr9");
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
      
      Vmr9Init(m_scene, (uint)upDevice.ToInt32(), VMR9Filter, (uint)hMonitor.ToInt32());

      int hr = graphBuilder.AddFilter(VMR9Filter, "Video Mixing Renderer 9");
      if (hr != 0)
      {
				Vmr9Deinit();
				m_scene.Stop();
				m_scene.Deinit();
				m_scene=null;
				
				Marshal.ReleaseComObject(VMR9Filter);
				VMR9Filter=null;
        Error.SetError("Unable to play movie", "Unable to initialize VMR9");
        Log.WriteFile(Log.LogType.Log, true, "VMR9Helper:Failed to add vmr9 to filtergraph");
        return;
      }

			quality = VMR9Filter as IQualProp ;
			m_mixerBitmap=VMR9Filter as IVMRMixerBitmap9;
			m_graphBuilder=graphBuilder;			
			instanceCounter++;
			vmr9Initialized=true;
			SetDeinterlacePrefs();
			System.OperatingSystem os=Environment.OSVersion;
			if (os.Platform==System.PlatformID.Win32NT)
			{
				long version=os.Version.Major*10000000 + os.Version.Minor*10000 + os.Version.Build;
				if (version >= 50012600) // we need at least win xp sp2 for VMR9 YUV mixing mode
				{
					IVMRMixerControl9 mixer = VMR9Filter as IVMRMixerControl9;
					if (mixer!=null)
					{
            //Log.Write("VMR9: enable YUV mixing");
						uint dwPrefs;
						mixer.GetMixingPrefs(out dwPrefs);
						dwPrefs  &= ~MixerPref_RenderTargetMask; 
						dwPrefs |= MixerPref_RenderTargetYUV;
						mixer.SetMixingPrefs(dwPrefs);
					}
				}
			}
      GUIGraphicsContext.Vmr9Active = true;
      g_vmr9 = this;
      //Log.Write("VMR9Helper:Vmr9 Added");
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
        //Log.Write("VMR9Helper:RemoveVMR9");
        //Log.Write("VMR9Helper:stop vmr9 helper");
				if (VMR9Filter != null)
				{
				
					if (m_scene != null)
					{
            //Log.Write("VMR9Helper:stop planescene");
						m_scene.Stop();
						instanceCounter--;
						m_scene.Deinit();
						GUIGraphicsContext.Vmr9Active = false;
						GUIGraphicsContext.Vmr9FPS=0f;
						GUIGraphicsContext.InVmr9Render=false;
						currentVmr9State = Vmr9PlayState.Playing;
					}
					int result;
					
					//if (m_mixerBitmap!= null)
					//	while ( (result=Marshal.ReleaseComObject(m_mixerBitmap))>0); 
					//result=Marshal.ReleaseComObject(m_mixerBitmap);
					m_mixerBitmap = null;

	//				if (quality != null)
	//					while ( (result=Marshal.ReleaseComObject(quality))>0); 
					//Marshal.ReleaseComObject(quality);
					quality = null;
					
					try
					{
						result=m_graphBuilder.RemoveFilter(VMR9Filter);
						if (result!=0) Log.Write("VMR9:RemoveFilter():{0}",result);
					}
					catch(Exception )
					{
					}
					Vmr9Deinit();
					
					try
					{
						result=Marshal.ReleaseComObject(VMR9Filter);
						if (result!=0) Log.Write("VMR9:ReleaseComObject():{0}",result);
					}
					catch(Exception )
					{
					}
					VMR9Filter = null;
					m_graphBuilder=null;
					m_scene = null;
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
      if (currentVmr9State == Vmr9PlayState.Playing) return;
      m_scene.Repaint();
    }

		public IQualProp Quality
		{
			get { return quality;}
		}
		public void SetRepaint()
		{
			if (!vmr9Initialized) return;
			if( !GUIGraphicsContext.Vmr9Active) return;
      //Log.Write("VMR9Helper: SetRepaint()");
			FrameCounter=0;
			repaintTimer=DateTime.Now;
			currentVmr9State = Vmr9PlayState.Repaint;
			m_scene.DrawVideo=false;
		}
    public void Process()
		{
			if (!vmr9Initialized) return;
			if( !GUIGraphicsContext.Vmr9Active) return;
			TimeSpan ts = DateTime.Now - repaintTimer;
			int frames = FrameCounter;
      if (ts.TotalMilliseconds >= 1000)
      {
        GUIGraphicsContext.Vmr9FPS = ((float)(frames*1000)) / ((float)ts.TotalMilliseconds);
       // Log.Write("VMR9Helper:frames:{0} fps:{1} time:{2}", frames, GUIGraphicsContext.Vmr9FPS,ts.TotalMilliseconds);
        FrameCounter = 0;
        VideoRendererStatistics.Update(quality);
        repaintTimer = DateTime.Now;
      }
      if (currentVmr9State == Vmr9PlayState.Repaint && frames>0 )
      {
        Log.Write("VMR9Helper: repaint->playing {0}",frames);
        GUIGraphicsContext.Vmr9FPS = 50f;
        currentVmr9State = Vmr9PlayState.Playing;
        m_scene.DrawVideo = true;
        repaintTimer = DateTime.Now;
      }
      if (currentVmr9State == Vmr9PlayState.Playing && GUIGraphicsContext.Vmr9FPS<5f)
      {
        Log.Write("VMR9Helper: playing->repaint");
        GUIGraphicsContext.Vmr9FPS = 0f;
        currentVmr9State = Vmr9PlayState.Repaint;
        m_scene.DrawVideo = false;
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
            //Log.Write("vmr9: pin:{0} is connected",i);
						if (pinIn!=null)
							hr=Marshal.ReleaseComObject(pinIn);
						if (pinConnected!=null)
							hr=Marshal.ReleaseComObject(pinConnected);
						return true;
					}
					if (pinIn!=null)
						hr=Marshal.ReleaseComObject(pinIn);
					if (pinConnected!=null)
						hr=Marshal.ReleaseComObject(pinConnected);
				}
				return false;
      }//get {
    }//public bool IsVMR9Connected

    public void SetDeinterlacePrefs()
		{
			if (!vmr9Initialized) return;
      int DeInterlaceMode = 3;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        //None
        //Bob
        //Weave
        //Best
        DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
      }
      Vmr9SetDeinterlacePrefs((uint)DeInterlaceMode);
    }
    public void SetDeinterlaceMode()
		{
			if (!vmr9Initialized) return;
			int DeInterlaceMode = 3;
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				//None
				//Bob
				//Weave
				//Best
				DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
			}
			Vmr9SetDeinterlaceMode((short)DeInterlaceMode);
    }
    public void Enable(bool onOff)
		{
      // Log.Write("Vmr9:Enable:{0}", onOff);
			if (!vmr9Initialized) return;
      if (m_scene != null) m_scene.Enabled = onOff;
      if (onOff)
      {
        repaintTimer = DateTime.Now;
        FrameCounter = 50;
      }
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
