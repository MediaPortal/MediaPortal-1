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
		static PlaneScene scene;
		static IVMRSurfaceAllocatorNotify9 allocNotify;
    static Size m_nativeSize = new Size(0,0);
		//this in reality should be an array, but doesn't work. In the common case
		// only a surface will be requested.
		static IntPtr		m_surface1 = IntPtr.Zero;
		static IntPtr[] m_surface2 = new IntPtr[10];
		static int			textureCount=1;
    
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
				for (int i=0; i < 10; ++i)
					m_surface2[i]=IntPtr.Zero;
  			scene = renderScene;
				scene.Init(GUIGraphicsContext.DX9Device);
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
        Log.Write("AllocatorWrapper:InitializeDevice({0:x})",dwUserId);
				scene.SetSrcRect( 1.0f, 1.0f);
				if (m_surface1!=IntPtr.Zero)
        {
          Marshal.Release(m_surface1);
          m_surface1=IntPtr.Zero;
        }

				for (int i=0; i < textureCount; i++)
				{
					if (m_surface2[i]!=IntPtr.Zero)
					{
						Log.Write("AllocatorWrapper:alloc 2nd:{0}x{1}", allocInfo.dwWidth,allocInfo.dwHeight);
						if (allocInfo.dwHeight<=576 && allocInfo.dwWidth<=1024)
						{
							Log.Write("AllocatorWrapper:return surface2:{0}",i);
							m_surface1=m_surface2[i];
							m_surface2[i]=IntPtr.Zero;
							m_nativeSize = new Size(allocInfo.dwWidth, allocInfo.dwHeight);
							float fTU = (float)(allocInfo.dwWidth ) / 1024.0f;
							float fTV = (float)(allocInfo.dwHeight) / 576.0f;
							scene.SetSrcRect( fTU, fTV );
							return 0;
						}
					}
				}

				Caps d3dcaps = GUIGraphicsContext.DX9Device.DeviceCaps;
				Int32 numbuff = Marshal.ReadInt32(numBuffers);
        if (numbuff > 1)
        {
          Log.Write("AllocatorWrapper:multiple surfaces not supported yet");
          throw new Exception("multiple surfaces not supported yet");
        }
        m_nativeSize = new Size(allocInfo.szNativeSize.Width,allocInfo.szNativeSize.Height);
        
        if (!d3dcaps.TextureCaps.SupportsNonPower2Conditional) 
				{
					Int32 width = 1;
					Int32 height = 1;
					while ( width < allocInfo.dwWidth )
						width = width <<1;
					while ( height < allocInfo.dwHeight )
						height = height <<1;

          
          float fTU = (float)(allocInfo.dwWidth ) / (float)(width);
          float fTV = (float)(allocInfo.dwHeight) / (float)(height);
          scene.SetSrcRect( fTU, fTV );

					allocInfo.dwHeight = height;
					allocInfo.dwWidth = width;

				}
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

					//double check the allocated texture dimensions
					Surface orig = new Surface(m_surface1);
					if (orig!=null)
					{
						Marshal.AddRef(m_surface1);
        
						Texture tex=orig.GetContainer(D3DGuids.Texture) as Texture;
						if (tex!=null)
						{
							SurfaceDescription desc;
							desc=tex.GetLevelDescription(0);
							float texWidth = (float)desc.Width;
							float texHeight = (float)desc.Height;							
							Log.Write("  texture allocated: {0}x{1}", (int)texWidth, (int)texHeight);
							if (texWidth != allocInfo.dwWidth || texHeight!=allocInfo.dwHeight)
							{
								float fTU = (float)(m_nativeSize.Width ) / (float)(texWidth);
								float fTV = (float)(m_nativeSize.Height) / (float)(texHeight);
								scene.SetSrcRect( fTU, fTV );
							}
							tex.Dispose();
						}
						orig.Dispose();
						Marshal.Release(m_surface1);
						Marshal.Release(m_surface1);
					}

					allocInfo.dwWidth=1024;
					allocInfo.dwHeight=576;
					for (int i=0; i < textureCount;++i)
					{
						if (m_surface2[i]!=IntPtr.Zero)
						{
							Marshal.Release(m_surface2[i]);
							m_surface2[i]=IntPtr.Zero;
						}
						hr=allocNotify.AllocateSurfaceHelper(allocInfo, numBuffers, out m_surface2[i]);
						if (hr==0) Log.Write("AllocatorWrapper:  allocted {0} 1024x576",i);
						else Log.Write("AllocatorWrapper:failed:  allocted {1} 1024x576",i);
					}
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
          Marshal.Release(m_surface1);
          m_surface1=IntPtr.Zero;
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
          Marshal.ReleaseComObject( allocNotify ); allocNotify = null;
				if (m_surface1 !=IntPtr.Zero)
					Marshal.Release(m_surface1); m_surface1= IntPtr.Zero;
				for (int i=0; i < textureCount;++i)
				{
					if (m_surface2[i] !=IntPtr.Zero)
						Marshal.Release(m_surface2[i]); m_surface2[i]= IntPtr.Zero;				
				}
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
        unchecked
        {
          if (presInfo==null) return (int)0x80004003L;// E_POINTER
          if (presInfo.lpSurf==IntPtr.Zero) return (int)0x80004003L; // E_POINTER

          Surface orig = new Surface(presInfo.lpSurf);
          if (orig!=null)
          {
            Marshal.AddRef(presInfo.lpSurf);
          
            Texture tex=orig.GetContainer(D3DGuids.Texture) as Texture;
            if (tex!=null)
            {
              scene.Render(GUIGraphicsContext.DX9Device, tex, m_nativeSize);
              tex.Dispose();
            }
            else 
            {
              orig.Dispose();
              Marshal.Release(presInfo.lpSurf);
              Marshal.Release(presInfo.lpSurf);
              return (int)0x80004003L; // E_POINTER
            }
            orig.Dispose();
            Marshal.Release(presInfo.lpSurf);
            Marshal.Release(presInfo.lpSurf);
          }
          else return (int)0x80004003L; // E_POINTER
        }
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

        Surface orig = new Surface(m_surface1);
        if (orig!=null)
        {
          Marshal.AddRef(m_surface1);
        
          Texture tex=orig.GetContainer(D3DGuids.Texture) as Texture;
          if (tex!=null)
          {
            scene.Render(GUIGraphicsContext.DX9Device, tex, m_nativeSize);
            tex.Dispose();
          }
          orig.Dispose();
          Marshal.Release(m_surface1);
          Marshal.Release(m_surface1);
        }
      }//Repaint()
		}//public class Allocator : IVMRSurfaceAllocator9, IVMRImagePresenter9
	}//public class AllocatorWrapper
}//namespace MediaPortal.Player 
