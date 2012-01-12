#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using DirectShowLib.BDA;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// Tuning types enumeration
  /// </summary>
  [Flags]
  public enum TuningType : uint
  {
    /// <summary>
    /// Not set
    /// </summary>
    NotSet = 0,
    /// <summary>
    /// Analog Tv
    /// </summary>
    AnalogTv = 0x1,
    /// <summary>
    /// Analog Radio
    /// </summary>
    AnalogRadio = 0x2,
    /// <summary>
    /// Analog RfTuner
    /// </summary>
    AnalogRfTuner = 0x4,
    /// <summary>
    /// DVB-T
    /// </summary>
    DvbT = 0x8,
    /// <summary>
    /// DVB-S
    /// </summary>
    DvbS = 0x10,
    /// <summary>
    /// DVB-C
    /// </summary>
    DvbC = 0x20,
    /// <summary>
    /// ATSC
    /// </summary>
    Atsc = 0x40,
    /// <summary>
    /// DVB-IP
    /// </summary>
    DvbIp = 0x80,
    /// <summary>
    /// DVB-S2
    /// </summary>
    DvbS2 = 0x100,
    /// <summary>
    /// ISDB-T
    /// </summary>
    IsdbT = 0x200,
    /// <summary>
    /// ISDB-S
    /// </summary>
    IsdbS = 0x400
  }

  public enum LNB_Source
  {
    LNBSourceNotSet = -1,
    LNBSourceNotDefined = 0,
    LNBSourceA = 1,
    LNBSourceB = 2,
    LNBSourceC = 3,
    LNBSourceD = 4,
    LNBSourceMax,
  } ;

  public enum DiseqC11Switches
  {
    Switch_NOT_SET = -1,
    Switch_0 = 0x0,
    Switch_1 = 0x1,
    Switch_2 = 0x2,
    Switch_3 = 0x3,
    Switch_4 = 0x4,
    Switch_5 = 0x5,
    Switch_6 = 0x6,
    Switch_7 = 0x7,
    Switch_8 = 0x8,
    Switch_9 = 0x9,
    Switch_10 = 0xa,
    Switch_11 = 0xb,
    Switch_12 = 0xc,
    Switch_13 = 0xd,
    Switch_14 = 0xe,
    Switch_15 = 0xf,
  }


  [StructLayout(LayoutKind.Sequential)]
  public struct FrequencySettings
  {
    public uint Multiplier;
    public uint Frequency;
    public uint Bandwidth;
    public Polarisation Polarity;
    public uint Range;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  public struct LnbInfoSettings
  {
    public uint LnbSwitchFrequency;
    public uint LowOscillator;
    public uint HighOscillator;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  public struct DigitalDemodulatorSettings
  {
    public FECMethod InnerFECMethod;
    public BinaryConvolutionCodeRate InnerFECRate;
    public ModulationType Modulation;
    public FECMethod OuterFECMethod;
    public BinaryConvolutionCodeRate OuterFECRate;
    public SpectralInversion SpectralInversion;
    public uint SymbolRate;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  public struct DigitalDemodulator2Settings
  {
    public FECMethod InnerFECMethod;
    public BinaryConvolutionCodeRate InnerFECRate;
    public ModulationType Modulation;
    public FECMethod OuterFECMethod;
    public BinaryConvolutionCodeRate OuterFECRate;
    public SpectralInversion SpectralInversion;
    public uint SymbolRate;
    public GuardInterval GuardInterval;
    public Pilot Pilot;
    public RollOff RollOff;
    public TransmissionMode TransmissionMode;
  } ;

  [StructLayout(LayoutKind.Sequential)]
  public struct DiseqcSatelliteSettings
  {
    public uint Enabled;
    public uint ToneBurstEnabled;
    public LNB_Source Diseq10Selection;
    public DiseqC11Switches Diseq11Selection;
  } ;

  public enum LogLevelOption
  {
    NotSet = -1,
    All = 0,
    Trace = 0,
    Debug = 10000,
    Info = 20000,
    Warn = 30000,
    Error = 40000,
    Fatal = 50000,
    Off = 60000
  } ;

  ///<summary>
  /// Channel scanning callback
  ///</summary>
  [ComVisible(true), ComImport,
   Guid("4564675E-C69B-4e05-853D-30870988DEB9"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDvbNetworkProvider
  {
    [PreserveSig]
    int TuneDVBT(FrequencySettings fSettings);

    [PreserveSig]
    int TuneDVBS(FrequencySettings fSettings, DigitalDemodulator2Settings dSettings, LnbInfoSettings lSettings,
                 DiseqcSatelliteSettings sSettings);

    [PreserveSig]
    int TuneDVBC(FrequencySettings fSettings, DigitalDemodulatorSettings dSettings);

    [PreserveSig]
    int TuneATSC(uint channelNumber, FrequencySettings fSettings, DigitalDemodulatorSettings dSettings);

    [PreserveSig]
    int GetAvailableTuningTypes(out TuningType tuningTypes);

    [PreserveSig]
    int GetSignalStats(out bool tunerLocked, out bool signalPresent, out int signalQuality, out int signalLevel);

    [PreserveSig]
    int ConfigureLogging([In, MarshalAs(UnmanagedType.LPWStr)] string logFilename,
                         [In, MarshalAs(UnmanagedType.LPWStr)] string identifier, LogLevelOption logLevel);
  }
}