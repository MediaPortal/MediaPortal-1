/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Hardware;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Hardware
{
  /// <summary>
  /// TeVii hw control class
  /// </summary>
  public class TeVii : IDisposable, IHardwareProvider, IDiSEqCController
  {
    #region Dll Imports

    //////////////////////////////////////////////////////////////////////////
    // Information functions
    // these functions don't require opening of device

    /// <summary>
    /// get version of SDK API
    /// This is optional.
    /// Can be used to check if API is not lower than originally used.
    /// </summary>
    /// <returns>API Version</returns>
    [DllImport("TeVii.dll", EntryPoint = "GetAPIVersion", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetAPIVersion();

    /// <summary>
    /// Enumerate devices in system.
    /// This function should be called before any other.
    /// Only first call will really enumerate. All subsequent 
    /// calls will just return number of previously found devices.
    /// </summary>
    /// <returns>number of found devices</returns>
    [DllImport("TeVii.dll", EntryPoint = "FindDevices", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern int FindDevices();

    /// <summary>
    /// Get name of device
    /// </summary>
    /// <param name="idx">idx - device index (0 &lt;= idx &lt; FindDevices())</param>
    /// <returns>string with device name or NULL (on failure). Do not modify or free memory used by this string!</returns>
    [DllImport("TeVii.dll", EntryPoint = "GetDeviceName", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetDeviceName(int idx);

    /// <summary>
    /// Get device path
    /// </summary>
    /// <param name="idx">idx - device index (0 lt;= idx &lt; FindDevices())</param>
    /// <returns>string with device path or NULL (on failure). Do not modify or free memory used by this string!</returns>
    [DllImport("TeVii.dll", EntryPoint = "GetDevicePath", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetDevicePath(int idx);

    //////////////////////////////////////////////////////////////////////////
    // Following functions work only after call OpenDevice()
    //

    /// <summary>
    /// Open device. Application may open several devices simultaneously. 
    /// They will be distinguished by idx parameter.
    /// </summary>
    /// <param name="idx">idx - device index (0 &lt;= idx &lt; FindDevices())</param>
    /// <param name="func">func - capture function which will receive stream.</param>
    /// <param name="lParam">lParam - application defined parameter which will be passed to capture function</param>
    /// <returns>non-zero on success</returns>
    [DllImport("TeVii.dll", EntryPoint = "OpenDevice", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 OpenDevice(int idx, IntPtr func, IntPtr lParam);

    /// <summary>
    /// Close Device
    /// </summary>
    /// <param name="idx">idx - device index of previously opened device (0 &lt;= idx &lt; FindDevices())</param>
    /// <returns> non-zero on success</returns>
    [DllImport("TeVii.dll", EntryPoint = "CloseDevice", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 CloseDevice(int idx);

    /// <summary>
    /// Tune to transponder
    /// </summary>
    /// <param name="idx">idx - device index of previously opened device (0 &lt;= idx &lt; FindDevices())</param>
    /// <param name="Freq">Freq - frequency in kHz, for example: 12450000 (12.45 GHz)</param>
    /// <param name="SymbRate">SymbRate - Symbol rate in sps, for example: 27500000 (27500 Ksps)</param>
    /// <param name="LOF"></param>
    /// <param name="Pol">Pol - polarization, see TPolarization above</param>
    /// <param name="F22KHz">F22KHz - 22KHz tone on/off</param>
    /// <param name="MOD">MOD - modulation, see TMOD above. Note: it's better to avoid use AUTO for DVBS2 signal, otherwise locking time will be long</param>
    /// <param name="FEC">FEC - see TFEC above. Note: it's better to avoid use AUTO for DVBS2 signal, otherwise locking time will be long</param>
    /// <returns>non-zero if signal is locked</returns>
    [DllImport("TeVii.dll", EntryPoint = "TuneTransponder", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 TuneTransponder(int idx, Int32 Freq, Int32 SymbRate, Int32 LOF, TPolarization Pol,
                                               Int32 F22KHz, TMOD MOD, TFEC FEC);

    /// <summary>
    /// Get signal status
    /// </summary>
    /// <param name="idx">idx - device index of previously opened device (0 &lt;= idx &lt; FindDevices())</param>
    /// <param name="IsLocked">isLocked - non-zero if signal is present</param>
    /// <param name="Strength">Strength - 0..100 signal strength</param>
    /// <param name="Quality">Quality - 0..100 signal quality</param>
    /// <returns>non-zero on success</returns>
    [DllImport("TeVii.dll", EntryPoint = "GetSignalStatus", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 GetSignalStatus(int idx, out bool IsLocked, out Int32 Strength, out Int32 Quality);

    /// <summary>
    /// Send DiSEqC message
    /// </summary>
    /// <param name="idx">idx - device index of previously opened device (0 &lt;= idx &lt; FindDevices())</param>
    /// <param name="Data"></param>
    /// <param name="Len">Data,Len - buffer with DiSEqC command</param>
    /// <param name="Repeats">Repeats - repeat DiSEqC message n times. 0 means send one time</param>
    /// <param name="Flg">Flg - non-zero means replace first byte (0xE0) of DiSEqC message with 0xE1 on second and following repeats.</param>
    /// <returns>non-zero on success</returns>
    [DllImport("TeVii.dll", EntryPoint = "SendDiSEqC", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 SendDiSEqC(int idx, byte[] Data, Int32 Len, Int32 Repeats, Int32 Flg);

    /// <summary>
    /// Set the remote control
    /// </summary>
    /// <param name="idx">idx - device index of previously opened device (0 &lt;= idx &lt; FindDevices())</param>
    /// <param name="lpCallback">lpCallback - application defined procedure to receive keys. NULL to disable RC.</param>
    /// <param name="lParam">lParam - application defined parameter which will be passed to callback function</param>
    /// <returns>non-zero on success</returns>
    [DllImport("TeVii.dll", EntryPoint = "SetRemoteControl", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 SetRemoteControl(int idx, IntPtr lpCallback, IntPtr lParam);

    #endregion

    #region enums

#pragma warning disable 1591

    /// <summary>
    /// Enum for FEC values
    /// </summary>
    public enum TFEC
    {
      TFEC_AUTO,
      TFEC_1_2,
      TFEC_1_3,
      TFEC_1_4,
      TFEC_2_3,
      TFEC_2_5,
      TFEC_3_4,
      TFEC_3_5,
      TFEC_4_5,
      TFEC_5_6,
      TFEC_5_11,
      TFEC_6_7,
      TFEC_7_8,
      TFEC_8_9,
      TFEC_9_10
    } ;

    /// <summary>
    /// Enum for modulations
    /// </summary>
    public enum TMOD
    {
      TMOD_AUTO,
      TMOD_QPSK,
      TMOD_BPSK,
      TMOD_16QAM,
      TMOD_32QAM,
      TMOD_64QAM,
      TMOD_128QAM,
      TMOD_256QAM,
      TMOD_8VSB,
      TMOD_DVBS2_QPSK,
      TMOD_DVBS2_8PSK,
      TMOD_DVBS2_16PSK,
      TMOD_DVBS2_32PSK,
      TMOD_TURBO_QPSK,
      TMOD_TURBO_8PSK,
      TMOD_TURBO_16PSK
    } ;

    /// <summary>
    /// Enum for Polarization
    /// </summary>
    public enum TPolarization
    {
      TPol_None,
      TPol_Vertical,
      TPol_Horizontal
    } ;

#pragma warning restore 1591

    #endregion

    #region variables

    private int m_iDeviceIndex;
    private bool m_bIsTeVii = false;
    private string m_devicePath;

    #endregion

    /// <summary>
    ///  Initializes a new instance of the <see cref="TeVii"/> class.
    /// </summary>
    public TeVii() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="TeVii"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public TeVii(IBaseFilter tunerFilter) {}

    /// <summary>
    /// Helper function to get managed string from return value
    /// </summary>
    /// <param name="ConstCharPtr">IntPtr to unmanaged string</param>
    /// <returns>String</returns>
    public String GetUnmanagedString(IntPtr ConstCharPtr)
    {
      return Marshal.PtrToStringAnsi(ConstCharPtr);
    }

    /// <summary>
    /// Close HW 
    /// </summary>
    private void Close()
    {
      Log.Log.Debug("TeVii: Closing card {0} ", m_iDeviceIndex);
      CloseDevice(m_iDeviceIndex);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (m_bIsTeVii)
      {
        Close();
      }
    }

    #region IHardwareProvider Member

    /// <summary>
    /// Init Hardware Provider
    /// </summary>
    /// <param name="tunerFilter"></param>
    public void Init(IBaseFilter tunerFilter)
    {
      // TeVii doesn't care about tunerFilters
    }

    /// <summary>
    /// GET or SET the DeviceIndex for/after detection 
    /// </summary>
    public int DeviceIndex
    {
      get { return (int)m_iDeviceIndex; }
      set { m_iDeviceIndex = value; }
    }

    /// <summary>
    /// Provider Name
    /// </summary>
    public String Provider
    {
      get { return "TeVii"; }
    }

    /// <summary>
    /// GET or SET the DevicePath for detection 
    /// </summary>
    public String DevicePath
    {
      get { return m_devicePath; }
      set { m_devicePath = value; }
    }

    /// <summary>
    /// Loading Priority
    /// </summary>
    public int Priority
    {
      get { return 10; }
    }

    /// <summary>
    /// Checks for suitable hardware and opens it
    /// </summary>
    public void CheckAndOpen()
    {
      Int32 numberDevices = FindDevices();

      // no devices found
      if (numberDevices == 0)
      {
        m_bIsTeVii = false;
        return;
      }
      Log.Log.Debug("TeVii: number of devices: {0}", numberDevices);

      String deviceName = String.Empty;
      String devicePath = String.Empty;

      //Log.Log.Debug("TeVii: DevicePath of active card: {0}", m_devicePath);
      for (int deviceIdx = 0; deviceIdx < numberDevices; deviceIdx++)
      {
        deviceName = GetUnmanagedString(GetDeviceName(deviceIdx));
        devicePath = GetUnmanagedString(GetDevicePath(deviceIdx));

        //Log.Log.Debug("TeVii: compare to {0} {1}", deviceName, devicePath);
        if (devicePath == m_devicePath)
        {
          m_iDeviceIndex = deviceIdx;
          m_bIsTeVii = true;
          break;
        }
      }

      if (!m_bIsTeVii) return;

      Log.Log.Debug("TeVii: card {0} detected: {1} (API v{2})", m_iDeviceIndex, deviceName, GetAPIVersion());
      if (OpenDevice(m_iDeviceIndex, IntPtr.Zero, IntPtr.Zero) == 0)
      {
        Log.Log.Debug("TeVii: card {0} open failed !", m_iDeviceIndex);
        m_bIsTeVii = false;
        return;
      }
      Log.Log.Debug("TeVii: card {0} successful opened", m_iDeviceIndex);
    }

    /// <summary>
    /// return true if hardware supported
    /// </summary>
    public bool IsSupported
    {
      get { return m_bIsTeVii; }
    }

    /// <summary>
    /// Returns the Capabilities
    /// </summary>
    public CapabilitiesType Capabilities
    {
      get { return CapabilitiesType.None; }
    }

    /// <summary>
    /// Instructs the cam/ci module to use hardware filter and only send the pids listed in pids to the pc
    /// </summary>
    /// <param name="subChannel">The sub channel id</param>
    /// <param name="channel">The current tv/radio channel.</param>
    /// <param name="HwPids">The pids.</param>
    /// <remarks>when the pids array is empty, pid filtering is disabled and all pids are received</remarks>
    public void SendPids(int subChannel, TvLibrary.Channels.DVBBaseChannel channel, List<ushort> HwPids) {}

    /// <summary>
    /// Set parameter to null when stopping the Graph.
    /// </summary>
    public void OnStopGraph() {}

    #endregion

    #region IDiSEqCController Member

    /// <summary>
    /// Sets the DVB s2 modulation.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="channel">The channel.</param>
    public DVBSChannel SetDVBS2Modulation(ScanParameters parameters, DVBSChannel channel)
    {
      return channel;
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scanparameters.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (m_bIsTeVii == false)
        return;
      Log.Log.Debug("TeVii: SendDiseqc: {0},{1}", parameters.ToString(), channel.ToString());

      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                           (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);

      byte[] ucMessage = new byte[4];
      ucMessage[0] = 0xE0; //framing byte
      ucMessage[1] = 0x10; //address byte
      ucMessage[2] = 0x38; //command byte
      ucMessage[3] = cmd; //data byte (port group 0)
      //byte[] ucMessage = DiSEqC_Helper.ChannelToDiSEqC(parameters, channel);
      SendDiSEqCCommand(ucMessage);
    }

    /// <summary>
    /// Send DiSEqC Command
    /// </summary>
    /// <param name="diSEqC">byte[] with commands</param>
    /// <returns>true if successful</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      int res = SendDiSEqC(m_iDeviceIndex, diSEqC, diSEqC.Length, 0, 0);
      if (res == 0)
      {
        Log.Log.Debug("TeVii: Send DiSEqC failed");
        return false;
      }

      Log.Log.Debug("TeVii: Send DiSEqC successful.");
      return true;
    }

    /// <summary>
    /// Read command
    /// </summary>
    /// <param name="reply"></param>
    /// <returns></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      Log.Log.Debug("ReadDiSEqCCommand not supported");
      reply = new byte[0];
      return true;
    }

    #endregion
  }
}