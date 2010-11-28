using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace FFDShow.Interfaces
{
  [ComImport, SuppressUnmanagedCodeSecurity,
  Guid("c1960960-17f5-11d1-abe1-00a0c905f375"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IAMStreamSelect
  {
    [PreserveSig]
    int Count([Out] out int pcStreams);

    [PreserveSig]
    int Info(
      [In] int lIndex,
      [Out] out AMMediaType ppmt,
      [Out] out AMStreamSelectInfoFlags pdwFlags,
      [Out] out int plcid,
      [Out] out int pdwGroup,
      [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszName,
      [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppObject,
      [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppUnk
      );

    [PreserveSig]
    int Enable(
      [In] int lIndex,
      [In] AMStreamSelectEnableFlags dwFlags
      );
  }

  /// <summary>
  /// From _AMSTREAMSELECTINFOFLAGS
  /// </summary>
  [Flags]
  public enum AMStreamSelectInfoFlags
  {
    Disabled = 0x0,
    Enabled = 0x01,
    Exclusive = 0x02
  }

  /// <summary>
  /// From _AMSTREAMSELECTENABLEFLAGS
  /// </summary>
  [Flags]
  public enum AMStreamSelectEnableFlags
  {
    DisableAll = 0x0,
    Enable = 0x01,
    EnableAll = 0x02
  }

  /// <summary>
  /// From AM_MEDIA_TYPE - When you are done with an instance of this class,
  /// it should be released with FreeAMMediaType() to avoid leaking
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public class AMMediaType
  {
    public Guid majorType;
    public Guid subType;
    [MarshalAs(UnmanagedType.Bool)]
    public bool fixedSizeSamples;
    [MarshalAs(UnmanagedType.Bool)]
    public bool temporalCompression;
    public int sampleSize;
    public Guid formatType;
    public IntPtr unkPtr; // IUnknown Pointer
    public int formatSize;
    public IntPtr formatPtr; // Pointer to a buff determined by formatType
  }
}
