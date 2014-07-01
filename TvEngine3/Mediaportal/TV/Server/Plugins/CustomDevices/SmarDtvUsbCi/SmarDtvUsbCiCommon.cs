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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi
{
  #region enums

  internal enum SmarDtvUsbCiState : int
  {
    Unplugged = 0,
    Empty,
    CamPresent
  }

  #endregion

  #region delegate definitions

  /// <summary>
  /// Invoked by the driver when the CI slot state changes.
  /// </summary>
  /// <param name="ciFilter">The CI filter.</param>
  /// <param name="state">The new state of the slot.</param>
  /// <returns>an HRESULT indicating whether the state change was successfully handled</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate int OnSmarDtvUsbCiState(IBaseFilter ciFilter, SmarDtvUsbCiState state);

  /// <summary>
  /// Invoked by the driver when application information is received from a CAM.
  /// </summary>
  /// <param name="ciFilter">The CI filter.</param>
  /// <param name="info">The application information.</param>
  /// <returns>an HRESULT indicating whether the application information was successfully processed</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate int OnSmarDtvUsbCiApplicationInfo(IBaseFilter ciFilter, [In] ref SmarDtvUsbCiApplicationInfo info);

  /// <summary>
  /// Invoked by the driver when a CAM wants to close an MMI session.
  /// </summary>
  /// <param name="ciFilter">The CI filter.</param>
  /// <returns>an HRESULT indicating whether the MMI session was successfully closed</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate int OnSmarDtvUsbCiCloseMmi(IBaseFilter ciFilter);

  /// <summary>
  /// Invoked by the driver when an application protocol data unit is received from a CAM.
  /// </summary>
  /// <param name="ciFilter">The CI filter.</param>
  /// <param name="apduLength">The length of the APDU buffer in bytes.</param>
  /// <param name="apdu">A buffer containing the APDU.</param>
  /// <returns>an HRESULT indicating whether the APDU was successfully processed</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate int OnSmarDtvUsbCiApdu(IBaseFilter ciFilter, int apduLength, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] apdu);

  #endregion

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct SmarDtvUsbCiCallBack
  {
    public IntPtr Context;  // Optional context that the interface will pass back as a parameter when the delegates are executed.
    public OnSmarDtvUsbCiState OnCiState;
    public OnSmarDtvUsbCiApplicationInfo OnApplicationInfo;
    public OnSmarDtvUsbCiCloseMmi OnCloseMmi;
    public OnSmarDtvUsbCiApdu OnApdu;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
  internal struct SmarDtvUsbCiVersionInfo
  {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string PluginVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string BdaVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string UsbVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string FirmwareVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string FpgaVersion;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct SmarDtvUsbCiApplicationInfo
  {
    public MmiApplicationType ApplicationType;
    private byte Padding;
    public ushort Manufacturer;
    public ushort Code;
    public byte MenuTitleLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 351)]
    public byte[] MenuTitle;
  }
}