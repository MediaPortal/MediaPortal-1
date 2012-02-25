using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TvLibrary.Implementations.DVB
{

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), ComVisible(true)]
  internal struct tTunerCapabilities
  {
    public TunerType eModulation;
    public int dwConstellationSupported; // Show if SetModulation() is supported
    public int dwFECSupported; // Show if SetFec() is suppoted
    public int dwMinTransponderFreqInKHz;
    public int dwMaxTransponderFreqInKHz;
    public int dwMinTunerFreqInKHz;
    public int dwMaxTunerFreqInKHz;
    public int dwMinSymbolRateInBaud;
    public int dwMaxSymbolRateInBaud;
    public int bAutoSymbolRate; // Obsolte		
    public int dwPerformanceMonitoring; // See bitmask definitions below
    public int dwLockTimeInMilliSecond; // lock time in millisecond
    public int dwKernelLockTimeInMilliSecond; // lock time for kernel
    public int dwAcquisitionCapabilities;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), ComVisible(true)]
  internal struct tagDEVICE_INFORMATION
  {
    public uint dwDeviceID;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] ucMACAddress;
    public TunerType eTunerModulation;
    public BusInterface eBusInterface;
    public ushort wInUse;
    public uint dwProductID;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public String wsProductName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
    public String wsProductDescription;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
    public String wsProductRevision;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 61)]
    public String wsProductFrontEnd;
  }


}
