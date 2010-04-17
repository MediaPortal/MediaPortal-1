#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.DirectX.Direct3D;

namespace WPFMediaKit.DirectX
{
  [StructLayout(LayoutKind.Sequential)]
  public struct D3DDEVICE_CREATION_PARAMETERS
  {
    private uint AdapterOrdinal;
    private DeviceType DeviceType;
    private IntPtr hFocusWindow;
    private int BehaviorFlags;
  }

  public enum D3DSCANLINEORDERING
  {
    D3DSCANLINEORDERING_UNKNOWN = 0,
    D3DSCANLINEORDERING_PROGRESSIVE = 1,
    D3DSCANLINEORDERING_INTERLACED = 2,
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct D3DDISPLAYMODEEX
  {
    public uint Size;
    public uint Width;
    public uint Height;
    public uint RefreshRate;
    public Format Format; // D3DFORMAT
    public D3DSCANLINEORDERING ScanLineOrdering;
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("02177241-69FC-400C-8FF1-93A44DF6861D"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDirect3D9Ex : IDirect3D9
  {
    [PreserveSig]
    new int RegisterSoftwareDevice([In, Out] IntPtr pInitializeFunction);

    [PreserveSig]
    new int GetAdapterCount();

    [PreserveSig]
    new int GetAdapterIdentifier(uint Adapter, uint Flags, uint pIdentifier);

    [PreserveSig]
    new uint GetAdapterModeCount(uint Adapter, D3DFORMAT Format);

    [PreserveSig]
    new int EnumAdapterModes(uint Adapter, D3DFORMAT Format, uint Mode, [Out] out D3DDISPLAYMODE pMode);

    [PreserveSig]
    new int GetAdapterDisplayMode(ushort Adapter, [Out] out D3DFORMAT Format);

    #region Method Placeholders

    [PreserveSig]
    new int CheckDeviceType();

    [PreserveSig]
    new int CheckDeviceFormat();

    [PreserveSig]
    new int CheckDeviceMultiSampleType();

    [PreserveSig]
    new int CheckDepthStencilMatch();

    [PreserveSig]
    new int CheckDeviceFormatConversion();

    [PreserveSig]
    new int GetDeviceCaps();

    #endregion

    [PreserveSig]
    new IntPtr GetAdapterMonitor(uint Adapter);

    [PreserveSig]
    new int CreateDevice(int Adapter,
                         DeviceType DeviceType,
                         IntPtr hFocusWindow,
                         CreateFlags BehaviorFlags,
                         [In, Out] ref D3DPRESENT_PARAMETERS pPresentationParameters,
                         [Out] out IntPtr ppReturnedDeviceInterface);

    [PreserveSig]
    uint GetAdapterModeCountEx();

    [PreserveSig]
    int EnumAdapterModesEx();

    [PreserveSig]
    int GetAdapterDisplayModeEx();

    [PreserveSig]
    int CreateDeviceEx(int Adapter,
                       DeviceType DeviceType,
                       IntPtr hFocusWindow,
                       CreateFlags BehaviorFlags,
                       [In, Out] ref D3DPRESENT_PARAMETERS pPresentationParameters,
                       [In, Out] IntPtr pFullscreenDisplayMode,
                       [Out] out IntPtr ppReturnedDeviceInterface);

    int GetAdapterLUID();

    //[PreserveSig]
    //int ResetEx([In, Out] ref D3DPRESENT_PARAMETERS pPresentationParameters, [In, Out]D3DDISPLAYMODEEX pFullscreenDisplayMode );
  }

  [ComImport, SuppressUnmanagedCodeSecurity,
   Guid("81BDCBCA-64D4-426d-AE8D-AD0147F4275C"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDirect3D9
  {
    [PreserveSig]
    int RegisterSoftwareDevice([In, Out] IntPtr pInitializeFunction);

    [PreserveSig]
    int GetAdapterCount();

    [PreserveSig]
    int GetAdapterIdentifier(uint Adapter, uint Flags, uint pIdentifier);

    [PreserveSig]
    uint GetAdapterModeCount(uint Adapter, D3DFORMAT Format);

    [PreserveSig]
    int EnumAdapterModes(uint Adapter, D3DFORMAT Format, uint Mode, [Out] out D3DDISPLAYMODE pMode);

    [PreserveSig]
    int GetAdapterDisplayMode(ushort Adapter, [Out] out D3DFORMAT Format);

    #region Method Placeholders

    [PreserveSig]
    int CheckDeviceType();

    [PreserveSig]
    int CheckDeviceFormat();

    [PreserveSig]
    int CheckDeviceMultiSampleType();

    [PreserveSig]
    int CheckDepthStencilMatch();

    [PreserveSig]
    int CheckDeviceFormatConversion();

    [PreserveSig]
    int GetDeviceCaps();

    #endregion

    [PreserveSig]
    IntPtr GetAdapterMonitor(uint Adapter);

    [PreserveSig]
    int CreateDevice(int Adapter,
                     DeviceType DeviceType,
                     IntPtr hFocusWindow,
                     CreateFlags BehaviorFlags,
                     [In, Out] ref D3DPRESENT_PARAMETERS pPresentationParameters,
                     [Out] out IntPtr ppReturnedDeviceInterface);
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct D3DDISPLAYMODE
  {
    public uint Width;
    public uint Height;
    public uint RefreshRate;
    public D3DFORMAT Format;
  }

  [Flags]
  public enum D3DFORMAT
  {
    D3DFMT_UNKNOWN = 0,
    D3DFMT_R8G8B8 = 20,
    D3DFMT_A8R8G8B8 = 21,
    D3DFMT_X8R8G8B8 = 22,
    D3DFMT_R5G6B5 = 23,
    D3DFMT_X1R5G5B5 = 24,
    D3DFMT_A1R5G5B5 = 25,
    D3DFMT_A4R4G4B4 = 26,
    D3DFMT_R3G3B2 = 27,
    D3DFMT_A8 = 28,
    D3DFMT_A8R3G3B2 = 29,
    D3DFMT_X4R4G4B4 = 30,
    D3DFMT_A2B10G10R10 = 31,
    D3DFMT_A8B8G8R8 = 32,
    D3DFMT_X8B8G8R8 = 33,
    D3DFMT_G16R16 = 34,
    D3DFMT_A2R10G10B10 = 35,
    D3DFMT_A16B16G16R16 = 36,
    D3DFMT_A8P8 = 40,
    D3DFMT_P8 = 41,
    D3DFMT_L8 = 50,
    D3DFMT_A8L8 = 51,
    D3DFMT_A4L4 = 52,
    D3DFMT_V8U8 = 60,
    D3DFMT_L6V5U5 = 61,
    D3DFMT_X8L8V8U8 = 62,
    D3DFMT_Q8W8V8U8 = 63,
    D3DFMT_V16U16 = 64,
    D3DFMT_A2W10V10U10 = 67,
    D3DFMT_D16_LOCKABLE = 70,
    D3DFMT_D32 = 71,
    D3DFMT_D15S1 = 73,
    D3DFMT_D24S8 = 75,
    D3DFMT_D24X8 = 77,
    D3DFMT_D24X4S4 = 79,
    D3DFMT_D16 = 80,
    D3DFMT_D32F_LOCKABLE = 82,
    D3DFMT_D24FS8 = 83,
    /* Z-Stencil formats valid for CPU access */
    D3DFMT_D32_LOCKABLE = 84,
    D3DFMT_S8_LOCKABLE = 85,
    D3DFMT_L16 = 81,
    D3DFMT_VERTEXDATA = 100,
    D3DFMT_INDEX16 = 101,
    D3DFMT_INDEX32 = 102,
    D3DFMT_Q16W16V16U16 = 110,
    // Floating point surface formats
    // s10e5 formats (16-bits per channel)
    D3DFMT_R16F = 111,
    D3DFMT_G16R16F = 112,
    D3DFMT_A16B16G16R16F = 113,
    // IEEE s23e8 formats (32-bits per channel)
    D3DFMT_R32F = 114,
    D3DFMT_G32R32F = 115,
    D3DFMT_A32B32G32R32F = 116,
    D3DFMT_CxV8U8 = 117,
    // Monochrome 1 bit per pixel format
    D3DFMT_A1 = 118,
    // Binary format indicating that the data has no inherent type
    D3DFMT_BINARYBUFFER = 199,
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct D3DPRESENT_PARAMETERS
  {
    public uint BackBufferWidth;
    public uint BackBufferHeight;
    public Format BackBufferFormat;
    public uint BackBufferCount;
    public MultiSampleType MultiSampleType;
    public int MultiSampleQuality;
    public SwapEffect SwapEffect;
    public IntPtr hDeviceWindow;
    public int Windowed;
    public int EnableAutoDepthStencil;
    public DepthFormat AutoDepthStencilFormat;
    public int Flags;
    /* FullScreen_RefreshRateInHz must be zero for Windowed mode */
    public uint FullScreen_RefreshRateInHz;
    public uint PresentationInterval;
  }

  public class Direct3D
  {
    [DllImport("d3d9.dll", EntryPoint = "Direct3DCreate9", CallingConvention = CallingConvention.StdCall),
     SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Interface)]
    public static extern IDirect3D9 Direct3DCreate9(ushort SDKVersion);

    [DllImport("d3d9.dll", EntryPoint = "Direct3DCreate9Ex", CallingConvention = CallingConvention.StdCall),
     SuppressUnmanagedCodeSecurity]
    public static extern int Direct3DCreate9Ex(ushort SDKVersion, [Out] out IDirect3D9Ex ex);
  }
}