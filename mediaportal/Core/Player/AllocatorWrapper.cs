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
	/// Summary description for AllocatorWrapper.
	/// 
	/// </summary>
	public class AllocatorWrapper
	{
		//static Device device = null;
		static PlaneScene scene;
		static IVMRSurfaceAllocatorNotify9 allocNotify;
    static Size m_nativeSize = new Size(0,0);
    static Size m_TextureSize = new Size(0,0);
		//this in reality should be an array, but doesn't work. In the common case
		// only a surface will be requested.
		static IntPtr m_surface1 = IntPtr.Zero;
		static IntPtr m_surface2 = IntPtr.Zero;
    
		[StructLayout(LayoutKind.Sequential)]
	  public class Allocator : IVMRSurfaceAllocator9, IVMRImagePresenter9
		{
			public Allocator()
			{
			}

			public Device Device {
				get {
					return GUIGraphicsContext.DX9Device;
					}
			}

			public Allocator (Control window, PlaneScene renderScene)
			{
  			scene = renderScene;
				scene.Init(GUIGraphicsContext.DX9Device);
			}

      public Size NativeSize
      {
        get{return m_nativeSize;}
      }
      public void SetTextureSize(Size size)
      {
        m_TextureSize=size;
      }

			public int InitializeDevice(Int32 dwUserId, VMR9AllocationInfo allocInfo, IntPtr numBuffers)
      {
        Log.Write("AllocatorWrapper:InitializeDevice({0:x})",dwUserId);
        if (m_surface1!=IntPtr.Zero)
        {
          Marshal.Release(m_surface1);
          m_surface1=IntPtr.Zero;
        }
				if (m_surface2!=IntPtr.Zero)
				{
					Log.Write("alloc 2nd:{0}x{1}", allocInfo.dwWidth,allocInfo.dwHeight);
					if (allocInfo.dwHeight<=576 && allocInfo.dwWidth<=768)
					{
						Log.Write("return surface2");
						m_surface1=m_surface2;
						m_surface2=IntPtr.Zero;
						m_nativeSize = new Size(allocInfo.dwWidth, allocInfo.dwHeight);
						float fTU = (float)(allocInfo.dwWidth ) / (float)(768);
						float fTV = (float)(allocInfo.dwHeight) / (float)(576);
						scene.SetSrcRect( fTU, fTV );
						return 0;
					}
				}

				Caps d3dcaps = GUIGraphicsContext.DX9Device.DeviceCaps;
				Int32 numbuff = Marshal.ReadInt32(numBuffers);
        if (numbuff > 1)
        {
          Log.Write("AllocatorWrapper:multiple surfaces not supported yet");
          throw new Exception("multiple surfaces not supported yet");
        }
        if  (m_TextureSize.Width > allocInfo.szNativeSize.Width)
        {
          allocInfo.dwWidth=m_TextureSize.Width ;
          allocInfo.szNativeSize.Width=m_TextureSize.Width ;
        }
        if  (m_TextureSize.Height > allocInfo.szNativeSize.Height)
        {
          allocInfo.dwHeight=m_TextureSize.Height ;
          allocInfo.szNativeSize.Height=m_TextureSize.Height ;
        }
        m_nativeSize = new Size(allocInfo.szNativeSize.Width,allocInfo.szNativeSize.Height);
        
        if (!d3dcaps.TextureCaps.SupportsPower2) 
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
        Log.Write("AllocatorWrapper:{0}x{1} fmt:{2} minbuffers:{3} pool:{4} ar:{5} size:{6} flags:{7} supportspow2:{8} max:{9}x{10}", 
                        allocInfo.dwWidth, allocInfo.dwHeight, 
                        allocInfo.format,allocInfo.MinBuffers,allocInfo.pool,
                        allocInfo.szAspectRation, allocInfo.szNativeSize,
                        allocInfo.dwFlags,
                        d3dcaps.TextureCaps.SupportsPower2,
                        d3dcaps.MaxTextureWidth,d3dcaps.MaxTextureHeight);
				int hr = allocNotify.AllocateSurfaceHelper(allocInfo, numBuffers, out m_surface1);
        if (hr!=0 || m_surface1==IntPtr.Zero)
        {
          Log.Write("AllocatorWrapper:AllocateSurface failed: no surface:0x{0:X}",hr);
        }
        else
        {
          Log.Write("AllocatorWrapper:AllocateSurface succeeded");
					allocInfo.dwWidth=768;
					allocInfo.dwHeight=576;
					if (m_surface2!=IntPtr.Zero)
					{
						Marshal.Release(m_surface2);
						m_surface2=IntPtr.Zero;
					}
					hr=allocNotify.AllocateSurfaceHelper(allocInfo, numBuffers, out m_surface2);
					if (hr==0) Log.Write("allocted 768x576");
					else Log.Write("failed:allocted 768x576");
					hr=0;
        }
        return hr;
			}

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

			public int AdviseNotify(IVMRSurfaceAllocatorNotify9 allocNotifyP)
      {
        Log.Write("AllocatorWrapper:AdviseNotify()");
				allocNotify = allocNotifyP;
				return 0;
			}

      public int UnAdviseNotify()
      {
        Log.Write("AllocatorWrapper:UnAdviseNotify()");
        if (allocNotify !=null)
          Marshal.ReleaseComObject( allocNotify ); allocNotify = null;
        return 0;
      }

			public int StartPresenting(uint uid)
      {
        Log.Write("AllocatorWrapper:StartPresenting({0:x})",uid);
				return 0;
			}

			public int StopPresenting(uint uid)
      {
        Log.Write("AllocatorWrapper:StopPresenting({0:x})",uid);
				return 0;
			}

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
      }
		}
	}
}
