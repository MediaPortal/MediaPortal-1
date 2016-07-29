using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Tuner Type
  /// </summary>
  internal enum TunerType
  {
    ttSat = 0,
    ttCable = 1,
    ttTerrestrial = 2,
    ttATSC = 3,
    ttUnknown = -1
  }

  internal enum eModulationTAG
  {
    QAM_4 = 2,
    QAM_16,
    QAM_32,
    QAM_64,
    QAM_128,
    QAM_256,
    MODE_UNKNOWN = -1
  }

  internal enum GuardIntervalType
  {
    Interval_1_32 = 0,
    Interval_1_16,
    Interval_1_8,
    Interval_1_4,
    Interval_Auto
  }

  internal enum BandWidthType
  {
    MHz_6 = 6,
    MHz_7 = 7,
    MHz_8 = 8,
  }

  internal enum B2C2DisEqcType
  {
    None = 0,
    Simple_A,
    Simple_B,
    Level_1_A_A,
    Level_1_A_B,
    Level_1_B_A,
    Level_1_B_B
  }

  internal enum FecType
  {
    Fec_1_2 = 1,
    Fec_2_3,
    Fec_3_4,
    Fec_5_6,
    Fec_7_8,
    Fec_Auto
  }

  internal enum LNBSelectionType
  {
    Lnb0 = 0,
    Lnb22kHz,
    Lnb33kHz,
    Lnb44kHz,
  }

  internal enum PolarityType
  {
    Horizontal = 0,
    Vertical,
  }

  internal enum BusInterface
  {
    DEVICE_INTERFACE_PCI = 0,
    DEVICE_INTERFACE_USB_1_1 = 1
  }

}
