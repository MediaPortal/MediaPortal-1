using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Data;
using System.Reflection;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.PrivateImplementationDetails;
using DShowNET;
using MediaPortal.GUI.Library;
namespace MediaPortal.Player 
{
	/// <summary>
	/// This class implements our custom allocator/presenter for VMR9 renderless
	/// 
	/// Some classes which work together:
	///  VMR9Util								: general helper class
	///  AllocatorWrapper.cs		: implements our own allocator/presentor for vmr9 by implementing
	///                           IVMRSurfaceAllocator9 and IVMRImagePresenter9
	///  PlaneScene.cs          : class which draws the video texture onscreen and mixes it with the GUI, OSD,...                          
	/// </summary>
	public class AllocatorWrapper
	{
		//static Device device = null;
		static PlaneScene										scene;
		static IVMRSurfaceAllocatorNotify9	allocNotify;
    static Size     										m_nativeSize = new Size(0,0);
		static IntPtr												m_surface1    = IntPtr.Zero;
		//static IntPtr[] 										extraTextures = new IntPtr[10];
		static int													textureCount  = 1;
		static int												  MaxTextureWidth=1900;
		static int												  MaxTextureHeight=1200;
    
		[StructLayout(LayoutKind.Sequential)]
	  public class Allocator : IVMRSurfaceAllocator9, IVMRImagePresenter9
		{
			public Allocator()
			{

			}

			/// <summary>
			/// Property to get the Direct3d device
			/// </summary>
			public Device Device {
				get {
					return GUIGraphicsContext.DX9Device;
					}
			}
			/// <summary>
			/// specify how many extra video textures should be allocated
			/// </summary>
			/// <param name="count">number of extra video textures (1-10)</param>
			public void SetTextureCount(int count)
			{
				textureCount=count;
			}

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="window">Window control to use</param>
			/// <param name="renderScene">instance of Planescene to use for presenting the video frames</param>
			public Allocator (Control window, PlaneScene renderScene)
			{
				//clear the extra video textures
				//for (int i=0; i < 10; ++i)
				//	extraTextures[i]=IntPtr.Zero;

  			scene = renderScene;
				scene.Init();
			}

			/// <summary>
			/// Property which returns the size of the video
			/// </summary>
      public Size NativeSize
      {
        get{return m_nativeSize;}
      }


			/// <summary>
			/// Callback from VMR9 when it needs a new direct3d texture
			/// </summary>
			/// <param name="dwUserId">user id</param>
			/// <param name="allocInfo">structure which describes what kind of surface VMR9 needs</param>
			/// <param name="numBuffers">number of surfaces wanted</param>
			/// <returns></returns>
			public int InitializeDevice(Int32 dwUserId, VMR9AllocationInfo allocInfo, IntPtr numBuffers)
      {
				float fTU ,fTV;
        Log.Write("AllocatorWrapper:InitializeDevice({0:x})",dwUserId);
				scene.SetSrcRect( 1.0f, 1.0f);
				if (m_surface1!=IntPtr.Zero) 
				{
					Log.Write("AllocatorWrapper: re-use surface {0}x{1}", allocInfo.dwWidth, allocInfo.dwHeight);
					fTU = (float)(allocInfo.dwWidth ) / ((float)MaxTextureWidth);
					fTV = (float)(allocInfo.dwHeight) / ((float)MaxTextureHeight);
					scene.SetSrcRect( fTU, fTV );
					//scene.SetSurface(m_surface1);
					return 0;
				}
/*
				if (m_surface1!=IntPtr.Zero)
        {
					scene.ReleaseSurface(m_surface1);
          Marshal.Release(m_surface1);
          m_surface1=IntPtr.Zero;
        }

				//first check if we still got a texture available
				for (int i=0; i < textureCount; i++)
				{
					//is this texture valid
					if (extraTextures[i]!=IntPtr.Zero)
					{
						//yes, then use it
						Log.Write("AllocatorWrapper:alloc extra texture#[0} :{1}x{2}", i,allocInfo.dwWidth,allocInfo.dwHeight);
						if (allocInfo.dwHeight<=MaxTextureHeight && allocInfo.dwWidth<=MaxTextureWidth)
						{
							Log.Write("AllocatorWrapper:return extra texture:{0}",i);
							m_surface1=extraTextures[i];
							extraTextures[i]=IntPtr.Zero; // dont use it anymore after this
							m_nativeSize = new Size(allocInfo.dwWidth, allocInfo.dwHeight);
							float fTU = (float)(allocInfo.dwWidth ) / ((float)MaxTextureWidth);
							float fTV = (float)(allocInfo.dwHeight) / ((float)MaxTextureHeight);
							scene.SetSrcRect( fTU, fTV );
							scene.SetSurface(m_surface1);
							return 0;
						}
					}
				}
*/
				Caps d3dcaps = GUIGraphicsContext.DX9Device.DeviceCaps;
				Int32 numbuff = Marshal.ReadInt32(numBuffers);
        if (numbuff > 1)
        {
          Log.Write("AllocatorWrapper:multiple surfaces not supported yet");
          throw new Exception("multiple surfaces not supported yet");
        }
        m_nativeSize = new Size(allocInfo.szNativeSize.Width,allocInfo.szNativeSize.Height);
				allocInfo.dwWidth =MaxTextureWidth;
				allocInfo.dwHeight=MaxTextureHeight;
				fTU = (float)(m_nativeSize.Width)  / ((float)MaxTextureWidth);
				fTV = (float)(m_nativeSize.Height) / ((float)MaxTextureHeight);
				scene.SetSrcRect( fTU, fTV );

				allocInfo.dwFlags = allocInfo.dwFlags | VMR9.VMR9AllocFlag_TextureSurface; 
        Log.Write("AllocatorWrapper:{0}x{1} fmt:{2} minbuffers:{3} pool:{4} ar:{5} size:{6} flags:{7} supportsPow2:{8} supportsNonPow2:{9} max:{10}x{11}", 
                        allocInfo.dwWidth, allocInfo.dwHeight, 
                        allocInfo.format,allocInfo.MinBuffers,allocInfo.pool,
                        allocInfo.szAspectRation, allocInfo.szNativeSize,
                        allocInfo.dwFlags,
                        d3dcaps.TextureCaps.SupportsPower2,
												d3dcaps.TextureCaps.SupportsNonPower2Conditional,
                        d3dcaps.MaxTextureWidth,d3dcaps.MaxTextureHeight);
				int hr = allocNotify.AllocateSurfaceHelper(allocInfo, numBuffers, out m_surface1);
        if (hr!=0 || m_surface1==IntPtr.Zero)
        {
          Log.Write("AllocatorWrapper:AllocateSurface failed: no surface:0x{0:X}",hr);
        }
        else
        {
					Log.Write("AllocatorWrapper:AllocateSurface succeeded");
					scene.SetSurface(m_surface1);
					/*
					//allocate the extra video textures
					if (allocInfo.dwWidth>MaxTextureWidth) MaxTextureWidth=allocInfo.dwWidth;
					if (allocInfo.dwHeight>MaxTextureHeight) MaxTextureHeight=allocInfo.dwHeight;
					allocInfo.dwWidth=MaxTextureWidth;
					allocInfo.dwHeight=MaxTextureHeight;
					for (int i=0; i < textureCount;++i)
					{
						//release first
						if (extraTextures[i]!=IntPtr.Zero)
						{
							Marshal.Release(extraTextures[i]);
							extraTextures[i]=IntPtr.Zero;
						}
						//and alloc
						hr=allocNotify.AllocateSurfaceHelper(allocInfo, numBuffers, out extraTextures[i]);
						if (hr==0) Log.Write("AllocatorWrapper:  allocated extra texture#{0} {1}x{2}",i,MaxTextureWidth,MaxTextureHeight);
						else Log.Write("AllocatorWrapper:failed:  allocated extra texture#{0} {1}x{2}",i,MaxTextureWidth,MaxTextureHeight);
					}
*/
					hr=0;
        }
        return hr;
			}

			/// <summary>
			/// Callback from VMR9. Called when it needs a texture
			/// </summary>
			/// <param name="dwUserId">used id</param>
			/// <param name="surfaceIndex">out surface index</param>
			/// <param name="SurfaceFlags">flags (not used)</param>
			/// <param name="surfacePtr">out pointer which receives the texture</param>
			/// <returns></returns>
			public int GetSurface(int dwUserId, int surfaceIndex, int SurfaceFlags, out IntPtr surfacePtr)
			{
        if (surfaceIndex!=0 || m_surface1==IntPtr.Zero) 
        {
          surfacePtr=IntPtr.Zero;
          unchecked 
          {
            return (int)0x80004005;//E_FAIL
          }
        }

        surfacePtr = m_surface1;
        Marshal.AddRef(m_surface1);
				return 0;
			}

			/// <summary>
			/// Callback from VMR9. Called when all directx surfaces should be released
			/// </summary>
			/// <param name="dwUserId">user id</param>
			/// <returns></returns>
			public int TerminateDevice(int dwUserId)
			{
        Log.Write("AllocatorWrapper:TerminateDevice({0:x})",dwUserId);
        if (m_surface1!=IntPtr.Zero)
				{
					//scene.ReleaseSurface(m_surface1);
          //Marshal.Release(m_surface1);
          //m_surface1=IntPtr.Zero;
        }
        return 0;
			}

			/// <summary>
			/// method to specify the IVMRSurfaceAllocatorNotify9 interface
			/// </summary>
			/// <param name="allocNotifyP">IVMRSurfaceAllocatorNotify9 interface</param>
			/// <returns></returns>
			public int AdviseNotify(IVMRSurfaceAllocatorNotify9 allocNotifyP)
      {
        Log.Write("AllocatorWrapper:AdviseNotify()");
				allocNotify = allocNotifyP;
				return 0;
			}

			/// <summary>
			/// method to specify the IVMRSurfaceAllocatorNotify9 interface
			/// </summary>
			/// <param name="allocNotifyP">IVMRSurfaceAllocatorNotify9 interface</param>
			/// <returns></returns>
      public int UnAdviseNotify()
      {
        Log.Write("AllocatorWrapper:UnAdviseNotify()");
        if (allocNotify !=null)
          Marshal.ReleaseComObject( allocNotify );
				allocNotify = null;
				if (m_surface1 !=IntPtr.Zero)
				{
					scene.ReleaseSurface(m_surface1);
					Marshal.Release(m_surface1); 
					m_surface1= IntPtr.Zero;
				}
/*
				//release the extra video textures
				for (int i=0; i < textureCount;++i)
				{
					if (extraTextures[i] !=IntPtr.Zero)
					{
						Marshal.Release(extraTextures[i]); 
					}
					extraTextures[i]= IntPtr.Zero;				
				}*/
				return 0;
      }

			/// <summary>
			/// callback from VMR9 when its ready to present video
			/// </summary>
			/// <param name="uid"></param>
			/// <returns></returns>
			public int StartPresenting(uint uid)
      {
        Log.Write("AllocatorWrapper:StartPresenting({0:x})",uid);
				return 0;
			}

			/// <summary>
			/// callback from VMR9 when its about to stop presenting video
			/// </summary>
			/// <param name="uid"></param>
			/// <returns></returns>
			public int StopPresenting(uint uid)
      {
        Log.Write("AllocatorWrapper:StopPresenting({0:x})",uid);
				return 0;
			}

			/// <summary>
			/// Callback from VMR9 when it has a new video frame which should be shown onscreen
			/// </summary>
			/// <param name="uid">user id</param>
			/// <param name="presInfo">structure holding the video frame</param>
			/// <returns></returns>
      public int PresentImage(uint uid, VMR9PresentationInfo presInfo)
      {
				scene.Render(m_nativeSize);
        return 0;
      }
      
			/// <summary>
			/// Method to redraw the last video frame
			/// This is used in pause mode. In pause mode VMR9 will not redraw the screen by itself
			/// MP then calls this repaint() method periodaly to redraw the video frame
			/// </summary>
			public void Repaint()
      {
        if (m_surface1==IntPtr.Zero) return;
				scene.Render(m_nativeSize);
      }//Repaint()
		}//public class Allocator : IVMRSurfaceAllocator9, IVMRImagePresenter9
	}//public class AllocatorWrapper
}//namespace MediaPortal.Player 
