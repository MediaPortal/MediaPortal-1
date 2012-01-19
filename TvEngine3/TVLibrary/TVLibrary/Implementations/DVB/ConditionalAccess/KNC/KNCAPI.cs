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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for KNC One tuners, including
  /// compatible models from Mystique and Satelco.
  /// </summary>
  public class KNCAPI : IDiSEqCController, ICiMenuActions, IDisposable
  {
    #region enums

    private enum KncCiState
    {
      Initialising = 0,       // Indicates that a CAM has been inserted.

      Transport = 1,
      Resource = 2,
      Application = 3,
      ConditionalAccess = 4,

      Ready = 5,              // Indicates that the CAM is ready for interaction.
      OpenService = 6,
      Releasing = 7,          // Indicates that the CAM has been removed.

      CloseMmi = 8,
      Request = 9,
      Menu = 10,
      MenuChoice = 11,

      OpenDisplay = 12,
      CloseDisplay = 13,

      None = 99
    }

    #endregion

    #region Dll imports

    /// <summary>
    /// Enable the conditional access interface.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="graphBuilder">The graph containing the tuner filter. Note: should be set to null for non-PCIe tuners.</param>
    /// <param name="filter">The filter which supports the proprietary property sets. This is the tuner filter for PCI devices and the capture filter for PCI-e devices.</param>
    /// <param name="callbacks">Callback structure pointer.</param>
    /// <returns><c>true</c> if the interface is successfully enabled, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_Enable", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_Enable(int deviceIndex, IGraphBuilder graphBuilder, IBaseFilter filter, IntPtr callbacks);

    /// <summary>
    /// Disable the conditional access interface.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <returns><c>true</c> if the interface is successfully disabled, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_Disable", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_Disable(int deviceIndex);

    /// <summary>
    /// Check if this device currently has conditional access capabilities.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <returns><c>true</c> if a CI slot is connected with a CAM inserted, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_IsAvailable", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_IsAvailable(int deviceIndex);

    /// <summary>
    /// Detect if the CAM is ready to interact.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <returns><c>true</c> if the CAM is ready, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_IsReady", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_IsReady(int deviceIndex);

    /// <summary>
    /// Enable the use of the KNCBDA_CI_* functions by initialising internal variables and interfaces.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="param"><c>True</c> to enable the CI slot; <c>false</c>to disable the CI slot.</param>
    /// <returns>???</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_HW_Enable", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_HW_Enable(int deviceIndex, bool param);

    /// <summary>
    /// Get the name/brand/type of the CAM inserted in the CI slot. This string is likely
	/// to be the root menu entry from the CA information.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="name">A buffer to hold the CAM name.</param>
    /// <param name="bufferSize">The size of the CAM name buffer in bytes.</param>
    /// <returns>the name/brand/type of the CAM</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_GetName", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_GetName(int deviceIndex, [MarshalAs(UnmanagedType.LPStr)] StringBuilder name,
                                                uint bufferSize);

    /// <summary>
    /// Send CA PMT to the CAM to request that one or more services be descrambled.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="caPmt">A pointer to a buffer containing the CA PMT.</param>
    /// <param name="caPmtLength">The length of the CA PMT buffer in bytes.</param>
    /// <returns><c>true</c> if the CA PMT is successfully passed to the CAM, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_SendPMTCommand", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_SendPMTCommand(int deviceIndex, IntPtr caPmt, uint caPmtLength);

    /// <summary>
    /// Enter the CAM menu.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="slotIndex">The index (0..n) of the CI slot that the CAM is inserted in.</param>
    /// <returns>???</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_EnterMenu", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_EnterMenu(int deviceIndex, byte slotIndex);

    /// <summary>
    /// Select an entry in the CAM menu.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="slotIndex">The index (0..n) of the CI slot that the CAM is inserted in.</param>
    /// <param name="choice">The index (0..n) of the menu choice selected by the user.</param>
    /// <returns>???</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_SelectMenu", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_SelectMenu(int deviceIndex, byte slotIndex, byte choice);

    /// <summary>
    /// Close the CAM menu.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="slotIndex">The index (0..n) of the CI slot that the CAM is inserted in.</param>
    /// <returns>???</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_CloseMenu", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_CloseMenu(int deviceIndex, byte slotIndex);

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="slotIndex">The index (0..n) of the CI slot that the CAM is inserted in.</param>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="menuAnswer">The user's response.</param>
    /// <returns>???</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_CI_SendMenuAnswer", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool KNCBDA_CI_SendMenuAnswer(int deviceIndex, byte slotIndex, bool cancel,
                                                       [In, MarshalAs(UnmanagedType.LPStr)] String menuAnswer);

    /// <summary>
    /// Enable the use of the KNCBDA_HW_* functions by initialising internal variables and interfaces.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="filter">The filter which supports the proprietary property sets. This is the tuner filter for PCI devices and the capture filter for PCI-e devices.</param>
    /// <returns><c>true</c> if the hardware interfaces are successfully initialised, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_HW_Enable", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern bool KNCBDA_HW_Enable(int deviceIndex, IBaseFilter filter);

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="deviceIndex">Device index 0..n.</param>
    /// <param name="command">A pointer to a buffer containing the command.</param>
    /// <param name="commandLength">The length of the command.</param>
    /// <param name="repeatCount">The number of times to resend the command.</param>
    /// <returns><c>true</c> if the tuner successfully sent the command, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "KNCBDA_HW_DiSEqCWrite", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern bool KNCBDA_HW_DiSEqCWrite(int deviceIndex, IntPtr command, UInt32 commandLength, UInt32 repeatCount);

    /// <summary>
    /// PCI-e products (Philips/NXP/Trident SAA7160 based) have a main device filter - one
    /// main device filter per card. This function enumerates and returns the total number of
    /// main devices filters in the system. Note that this function does *not* exclude non-KNC
    /// SAA716x based main devices.
    /// </summary>
    /// <returns>the number of SAA716x main device filters in the system</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "PCIE_EnumerateMainDevices", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern int PCIE_EnumerateMainDevices();

    /// <summary>
    /// Returns the filter name for a specific main device.
    /// </summary>
    /// <param name="mainDeviceIndex">Main device index 0..n.</param>
    /// <returns>the name for the main device corresponding with the index parameter</returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "PCIE_GetDeviceItem", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private static extern String PCIE_GetDeviceItem(int mainDeviceIndex);

    /// <summary>
    /// Open a main device. You need to do this if you want to swap the CI/CAM inputs on the corresponding
    /// card. You can only have one main device open at a time.
    /// </summary>
    /// <param name="mainDeviceIndex">Main device index 0..n.</param>
    /// <returns><c>true</c> if the device is successfully opened, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "PCIE_OpenMainDevice", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern bool PCIE_OpenMainDevice(int mainDeviceIndex);

    /// <summary>
    /// Close the currently open main device. You can only have one main device open at a time.
    /// </summary>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "PCIE_CloseMainDevice", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern void PCIE_CloseMainDevice();

    /// <summary>
    /// Set the value of a tuner property.
    /// </summary>
    /// <param name="propertySet">The GUID of the property set.</param>
    /// <param name="propertyIndex">The index of the property within the property set.</param>
    /// <param name="data">A pointer to a buffer containing the property value.</param>
    /// <param name="dataLength">The length of the property value in bytes.</param>
    /// <returns><c>true</c> if the value of the property is set successfully, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "PCIE_SetProperty", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern bool PCIE_SetProperty(Guid propertySet, UInt32 propertyIndex, IntPtr data, UInt32 dataLength);

    /// <summary>
    /// Get the value of a tuner property.
    /// </summary>
    /// <param name="propertySet">The GUID of the property set.</param>
    /// <param name="propertyIndex">The index of the property within the property set.</param>
    /// <param name="data">A pointer to a buffer to hold the property value.</param>
    /// <param name="dataLength">The length of the property value in bytes.</param>
    /// <returns><c>true</c> if the value of the property is retrieved successfully, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "PCIE_GetProperty", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern bool PCIE_GetProperty(Guid propertySet, UInt32 propertyIndex, IntPtr data, out UInt32 dataLength);

    /// <summary>
    /// Swap the CI slot/CAM associated with the tuners on a card. Tuners can only access a single
    /// CI slot/CAM at any given time.
    /// </summary>
    /// <param name="swap"><c>True</c> to swap CI slot/CAM inputs.</param>
    /// <returns><c>true</c> if the CI slot/CAM inputs on the device are successfully swapped, otherwise <c>false</c></returns>
    [DllImport("KNCBDACTRL.dll", EntryPoint = "PCIE_SwapCAMInput", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.StdCall)]
    private static extern bool PCIE_SwapCAMInput(bool swap);

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private unsafe struct KncCiCallbacks
    {
      /// Optional context that the interface will pass back
      /// as a parameter when the delegates are executed.
      public IntPtr Context;

      /// Delegate for CI state changes.
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnKncCiState OnCiState;

      /// Delegate for entering the CAM menu.
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnKncCiOpenDisplay OnOpenDisplay;

      /// Delegate for CAM menu meta-data (header, footer etc.).
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnKncCiMenu OnCiMenu;

      /// Delegate for CAM menu entries.
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnKncCiMenuEntry OnCiMenuEntry;

      /// Delegate for CAM requests.
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnKncCiRequest OnRequest;

      /// Delegate for closing the CAM menu.
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnKncCiCloseDisplay OnCloseDisplay;
    }

    #endregion

    #region callback delegate definitions

    /// <summary>
    /// Called by the tuner driver when the state of a CI slot changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot that changed state.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <param name="menuTitle">The CAM root menu title. This will be blank when the CAM has not been completely
    ///   initialised. Typically this will be the same string as can be retrieved by KNCBDA_CI_GetName().</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private unsafe delegate void OnKncCiState(
      byte slotIndex, KncCiState state, [MarshalAs(UnmanagedType.LPStr)] String menuTitle, IntPtr context);

    /// <summary>
    /// Called by the tuner driver when the CAM menu is successfully opened.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private unsafe delegate void OnKncCiOpenDisplay(byte slotIndex, IntPtr context);

    /// <summary>
    /// Called by the tuner driver to pass the menu meta-data when the user is browsing the CAM menu.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="title">The menu title.</param>
    /// <param name="subTitle">The menu sub-title.</param>
    /// <param name="footer">The menu footer.</param>
    /// <param name="numEntries">The number of entries in the menu.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private unsafe delegate void OnKncCiMenu(byte slotIndex, [MarshalAs(UnmanagedType.LPStr)] String title,
                                            [MarshalAs(UnmanagedType.LPStr)] String subTitle,
                                            [MarshalAs(UnmanagedType.LPStr)] String footer,
                                            uint numEntries, IntPtr context);

    /// <summary>
    /// Called by the tuner driver for each menu entry when the user is browsing the CAM menu.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="entryIndex">The index of the entry within the menu.</param>
    /// <param name="text">The text associated with the entry.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private unsafe delegate void OnKncCiMenuEntry(
      byte slotIndex, uint entryIndex, [MarshalAs(UnmanagedType.LPStr)] String text, IntPtr context);

    /// <summary>
    /// Called by the tuner driver when the CAM requests input from the user.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="blind"><c>True</c> if the input should be hidden (eg. password).</param>
    /// <param name="answerLength">The expected answer length.</param>
    /// <param name="text">The request context text from the CAM.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private unsafe delegate void OnKncCiRequest(
      byte slotIndex, bool blind, uint answerLength, [MarshalAs(UnmanagedType.LPStr)] String text, IntPtr context);

    /// <summary>
    /// Called by the tuner driver when the CAM wants to close the menu.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="delay">The delay (in milliseconds) after which the menu should be closed.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private unsafe delegate void OnKncCiCloseDisplay(byte slotIndex, uint delay, IntPtr context);

    #endregion

    #region constants

    private static readonly string[] ValidTunerNames = new string[]
    {
      "KNC BDA DVB-S",
      "KNC BDA DVB-S2",
      "KNC BDA DVB-C",
      "KNC BDA DVB-T",
      "7160 KNC BDA DVBS2 Tuner",   // PCI-e: DVB-S2 Twin

      "Mystique SaTiX DVB-S",
      "Mystique SaTiX DVB-S2",
      "Mystique CaBiX DVB-C2",
      "Mystique TeRiX DVB-T2",
      "Mystique SaTiX-S",
      "Mystique SaTiX-S2",
      "Mystique CaBiX-C2",
      "Mystique TeRiX-T2",

      "Satelco EasyWatch PCI (DVB-S)",
      "Satelco EasyWatch PCI (DVB-S2)",
      "Satelco EasyWatch PCI (DVB-C)",
      "Satelco EasyWatch PCI (DVB-T)"
    };

    private static readonly string[] ValidDevicePaths = new string[]
    {
      // DVB-S - Old
      "ven_1131&dev_7146&subsys_4f561131",  // KNC

      // DVB-S - SH2
      "ven_1131&dev_7146&subsys_00101894",  // KNC
      "ven_1131&dev_7146&subsys_00111894",  // Mystique
      "ven_1131&dev_7146&subsys_001a1894",  // Satelco

      // DVB-S - X4
      "ven_1131&dev_7146&subsys_00161894",  // KNC
      "ven_1131&dev_7146&subsys_00151894",  // Mystique
      "ven_1131&dev_7146&subsys_001b1894",  // Satelco

      // DVB-S - X4 (no CI)
      "ven_1131&dev_7146&subsys_00141894",  // KNC
      "ven_1131&dev_7146&subsys_001e1894",  // Satelco

      // DVB-S - X6
      "ven_1131&dev_7146&subsys_00191894",  // KNC
      "ven_1131&dev_7146&subsys_00181894",  // Mystique
      "ven_1131&dev_7146&subsys_001d1894",  // Satelco
      "ven_1131&dev_7146&subsys_001f1894",  // Satelco

      // DVB-S2 - Sharp
      "ven_1131&dev_7146&subsys_00501894",  // KNC
      "ven_1131&dev_7146&subsys_00511894",  // Mystique
      "ven_1131&dev_7146&subsys_00521894",  // Satelco

      // DVB-S - X8
      "ven_1131&dev_7146&subsys_00561894",  // KNC
      "ven_1131&dev_7146&subsys_00551894",  // Mystique
      "ven_1131&dev_7146&subsys_005b1894",  // Satelco

      // DVB-S - X8 (no CI)
      "ven_1131&dev_7146&subsys_00541894",  // KNC
      "ven_1131&dev_7146&subsys_005e1894",  // Satelco

      // DVB-C - MK2
      "ven_1131&dev_7146&subsys_00201894",  // KNC
      "ven_1131&dev_7146&subsys_00211894",  // Mystique
      "ven_1131&dev_7146&subsys_002a1894",  // Satelco

      // DVB-C - MK3
      "ven_1131&dev_7146&subsys_00221894",  // KNC
      "ven_1131&dev_7146&subsys_00231894",  // Mystique
      "ven_1131&dev_7146&subsys_002c1894",  // Satelco

      // DVB-C - MK32
      "ven_1131&dev_7146&subsys_00281894",  //KNC

      // DVB-T
      "ven_1131&dev_7146&subsys_00301894",  // KNC
      "ven_1131&dev_7146&subsys_00311894",  // Mystique
      "ven_1131&dev_7146&subsys_003a1894",  // Satelco

      // New KNC PCI-e tuners
      "ven_1131&dev_7160&subsys_01101894",  // DVB-S/DVB-S2 (not yet released)
      "ven_1131&dev_7160&subsys_02101894",  // DVB-S2/DVB-S2 (DVB-S2 Twin)
      "ven_1131&dev_7160&subsys_03101894",  // DVB-T/DVB-C (not yet released)
    };

    private const int CallbackStructSize = 28;
    private const int MaxDiseqcCommandLength = 64;

    #endregion

    #region variables

    private readonly IntPtr _diseqcBuffer = IntPtr.Zero;
    private readonly IntPtr _callbackBuffer = IntPtr.Zero;

    private IBaseFilter _tunerFilter = null;
    private IBaseFilter _captureFilter = null;
    private IGraphBuilder _graphBuilder = null;

    private bool _isKnc = false;
    private int _deviceIndex = -1;
    private bool _isPcie = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;
    private byte _slotIndex = 0;
    private KncCiState _ciState = KncCiState.Releasing;

    private KncCiCallbacks _callbacks;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="KNCAPI"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="tunerDevicePath">The tuner device path.</param>
    public KNCAPI(IBaseFilter tunerFilter, String tunerDevicePath)
    {
      // Stage 1: check if this tuner is supported by the KNC API.
      FilterInfo tunerInfo;
      int hr = tunerFilter.QueryFilterInfo(out tunerInfo);
      if (hr != 0)
      {
        Log.Log.Debug("KNC: failed to get the tuner name, hr = 0x{0:x} - {1}", HResult.GetDXErrorString(hr));
        return;
      }
      foreach (String validTunerName in ValidTunerNames)
      {
        if (tunerInfo.achName.Equals(validTunerName))
        {
          Log.Log.Debug("KNC: recognised tuner name \"{0}\"", tunerInfo.achName);
          _isKnc = true;
          break;
        }
      }

      if (_isKnc)
      {
        // Stage 2: attempt to get the KNC device index corresponding with this tuner.
        _isKnc = false;
        String devicePath = "";
        if (tunerDevicePath != null)
        {
          devicePath = tunerDevicePath.ToLowerInvariant();
        }
        _deviceIndex = GetDeviceIndex(devicePath);
        if (_deviceIndex < 0)
        {
          Log.Log.Debug("KNC: failed to calculate the device index");
        }
        else
        {
          // Stage 3: ensure we can get a reference to the filter that implements the proprietary
          // interfaces.
          Log.Log.Debug("KNC: device index is {0}", _deviceIndex);
          if (devicePath.Contains("dev_7160"))
          {
            Log.Log.Debug("KNC: this is a PCIe tuner");
            _isPcie = true;

            // We need a reference to the capture filter.
            IPin tunerOutputPin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
            IPin captureInputPin;
            hr = tunerOutputPin.ConnectedTo(out captureInputPin);
            Release.ComObject(tunerOutputPin);
            if (hr != 0)
            {
              Log.Log.Debug("KNC: failed to get the capture filter input pin, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              _captureFilter = null;
            }
            else
            {
              PinInfo captureInfo;
              hr = captureInputPin.QueryPinInfo(out captureInfo);
              if (hr != 0)
              {
                Log.Log.Debug("KNC: failed to get the capture filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                _captureFilter = null;
              }
              else
              {
                _captureFilter = captureInfo.filter;
                _isKnc = true;
              }
              Release.ComObject(captureInputPin);
            }
          }
          else
          {
            // No further work required for PCI devices.
            _isKnc = true;
          }
        }
      }

      if (!_isKnc)
      {
        if (tunerInfo.pGraph != null)
        {
          Release.ComObject(tunerInfo.pGraph);
          tunerInfo.pGraph = null;
        }
        return;
      }

      Log.Log.Debug("KNC: supported tuner detected");
      // If we have references to the required filters then allocate buffers.
      _diseqcBuffer = Marshal.AllocCoTaskMem(MaxDiseqcCommandLength);
      _callbackBuffer = Marshal.AllocCoTaskMem(CallbackStructSize);
      _tunerFilter = tunerFilter;
      _graphBuilder = (IFilterGraph2)tunerInfo.pGraph;

      // Prepare the hardware interface for use. This seems to always succeed...
      if (!KNCBDA_HW_Enable(_deviceIndex, _isPcie ? _captureFilter : _tunerFilter))
      {
        Log.Log.Debug("KNC: failed to enable the hardware");
      }
      OpenCi();
    }

    /// <summary>
    /// Calculates the correct device index for a given tuner. The index represents a
    /// position in an ordered list of KNC-compatible tuners installed in the system.
    /// </summary>
    /// <param name="devicePath">The device path of the tuner.</param>
    /// <returns>the device index for this tuner, -1 if the tuner is not KNC-compatible</returns>
    private static int GetDeviceIndex(String devicePath)
    {
      // Build a list of the device paths of all KNC-compatible tuners installed in this system.
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      List<String> devicePaths = new List<String>();
      foreach (DsDevice device in devices)
      {
        foreach (String validTunerName in ValidTunerNames)
        {
          if (device.Name != null && device.Name.Equals(validTunerName))
          {
            devicePaths.Add(device.DevicePath);
            break;
          }
        }
      }

      if (devicePaths.Count == 0)
      {
        return -1;
      }

      // Sort the list - we want the devices in the same order as in Windows device manager.
      devicePaths.Sort();

      // Find the index of the device.
      int idx = 0;
      foreach (String path in devicePaths)
      {
        if (devicePath.Equals(path))
        {
          return idx;
        }
        idx++;
      }
      return -1;
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a KNC-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a KNC-compatible tuner, otherwise <c>false</c></value>
    public bool IsKnc
    {
      get
      {
        return _isKnc;
      }
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("KNC: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      if (ch.ModulationType == ModulationType.ModQpsk || ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.Mod8Vsb;
      }
      // I don't think any KNC tuners or clones support demodulating anything
      // higher than 8 PSK. Nevertheless...
      else if (ch.ModulationType == ModulationType.Mod16Apsk)
      {
        ch.ModulationType = ModulationType.Mod16Vsb;
      }
      else if (ch.ModulationType == ModulationType.Mod32Apsk)
      {
        ch.ModulationType = ModulationType.ModOqpsk;
      }
      Log.Log.Debug("  modulation = {0}", ch.ModulationType);
      return ch as DVBBaseChannel;
    }

    #region conditional access

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    private void OpenCi()
    {
      Log.Log.Debug("KNC: tuner {0} open conditional access interface", _deviceIndex);
      _callbacks = new KncCiCallbacks();
      _callbacks.OnCiMenu = OnCiMenu;
      _callbacks.OnCiMenuEntry = OnCiMenuEntry;
      _callbacks.OnCiState = OnCiState;
      _callbacks.OnCloseDisplay = OnCiCloseDisplay;
      _callbacks.OnOpenDisplay = OnCiOpenDisplay;
      _callbacks.OnRequest = OnCiRequest;
      _callbacks.Context = IntPtr.Zero;

      _ciState = KncCiState.Releasing;

      unsafe
      {
        Marshal.StructureToPtr(_callbacks, _callbackBuffer, true);
        // Open the conditional access interface.
        bool result = false;
        if (_isPcie)
        {
          result = KNCBDA_CI_Enable(_deviceIndex, _graphBuilder, _captureFilter, _callbackBuffer);
        }
        else
        {
          result = KNCBDA_CI_Enable(_deviceIndex, null, _tunerFilter, _callbackBuffer);
        }
        if (!result)
        {
          Log.Log.Debug("KNC: CI enable failed");
          return;
        }

        // Prepare the conditional access interface for use. This seems to always succeed...
        if (!KNCBDA_CI_HW_Enable(_deviceIndex, true))
        {
          Log.Log.Debug("KNC: CI HW enable failed");
          return;
        }

        Log.Log.Debug("KNC: CI interface opened successfully");

        // Check if a CI slot is connected.
        _isCiSlotPresent = IsCiSlotPresent();
        if (!_isCiSlotPresent)
        {
          return;
        }

        // Check if a CAM is in the CI slot.
        _isCamPresent = IsCamPresent();
        if (!_isCiSlotPresent)
        {
          return;
        }

        // Check if the CAM is currently ready. CI state change callbacks will tell us if state changes.
        _isCamReady = IsCamReady();
        if (!_isCamReady)
        {
          return;
        }

        StringBuilder nameBuffer = new StringBuilder(100);
        if (KNCBDA_CI_GetName(_deviceIndex, nameBuffer, (uint)nameBuffer.MaxCapacity))
        {
          Log.Log.Debug("KNC: CAM name/type is {0}", nameBuffer);
        }
        else
        {
          Log.Log.Debug("KNC: failed to get the CAM name/type");
        }
      }
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    private void CloseCi()
    {
      Log.Log.Debug("KNC: tuner {0} close conditional access interface", _deviceIndex);
      if (!KNCBDA_CI_Disable(_deviceIndex))
      {
        Log.Log.Debug("KNC: CI disable failed");
      }
      _isCiSlotPresent = false;
      _isCamPresent = false;
      _isCamReady = false;
      _ciState = KncCiState.Releasing;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    public void ResetCi()
    {
      CloseCi();
      OpenCi();
    }

    /// <summary>
    /// Determines whether a CI slot is present.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      // It is not possible to tell if a CI slot is present - KNCBDA_CI_IsAvailable()
      // only returns true when a CI slot is present *and* a CAM is inserted. Best to
      // assume there is always a CI slot...
      Log.Log.Debug("KNC: is CI slot present");
	  Log.Log.Debug("KNC: result = {0}", true);
      return true;
    }

    /// <summary>
    /// Determines whether a CAM is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present, otherwise <c>false</c></returns>
    public bool IsCamPresent()
    {
      Log.Log.Debug("KNC: is CAM present");
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("KNC: CI slot not present");
        return false;
      }

      bool camPresent = KNCBDA_CI_IsAvailable(_deviceIndex);
      Log.Log.Debug("KNC: result = {0}", camPresent);
      return camPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present and ready for interaction.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present and ready, otherwise <c>false</c></returns>
    public bool IsCamReady()
    {
      Log.Log.Debug("KNC: is CAM ready");
      bool camReady = KNCBDA_CI_IsReady(_deviceIndex);
      Log.Log.Debug("KNC: result = {0}", camReady);
      return camReady;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the service is successfully descrambled, otherwise <c>false</c></returns>
    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("KNC: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCamPresent)
      {
        Log.Log.Debug("KNC: CAM not available");
        return true;
      }
      if (pmt == null || pmt.Length == 0)
      {
        Log.Log.Debug("KNC: no PMT");
        return true;
      }

      // KNC supports the standard CA PMT format.
      ChannelInfo info = new ChannelInfo();
      info.DecodePmt(pmt);
      info.caPMT.CommandId = command;
      info.caPMT.CAPmt_Listmanagement = listAction;
      foreach (CaPmtEs es in info.caPMT.CaPmtEsList)
      {
        es.CommandId = command;
      }
      int caPmtLength;
      byte[] caPmt = info.caPMT.CaPmtStruct(out caPmtLength);

      // Send the data to the CAM. Use a local buffer since PMT updates are asynchronous.
      IntPtr pmtBuffer = Marshal.AllocCoTaskMem(caPmtLength);
      try
      {
        Marshal.Copy(caPmt, 0, pmtBuffer, caPmtLength);
        //DVB_MMI.DumpBinary(pmtBuffer, 0, caPmtLength);
        bool succeeded = KNCBDA_CI_SendPMTCommand(_deviceIndex, pmtBuffer, (uint)caPmtLength);
        Log.Log.Debug("KNC: result = {0}", succeeded);
        return succeeded;
      }
      finally
      {
        Marshal.FreeCoTaskMem(pmtBuffer);
      }
    }

    #endregion

    #region callback handlers

    /// <summary>
    /// Called by the tuner driver when the state of a CI slot changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot that changed state.</param>
    /// <param name="state">The new state of the slot.</param>
    /// <param name="menuTitle">The CAM root menu title.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    private void OnCiState(byte slotIndex, KncCiState state, String menuTitle, IntPtr context)
    {
      lock (this)
      {
        Log.Log.Debug("KNC: tuner {0} CI state change callback, slot = {1}", _deviceIndex, slotIndex);
        if (state == KncCiState.Ready)
        {
          _isCamPresent = true;
          _isCamReady = true;
        }
        else if (state == KncCiState.Initialising)
        {
          _isCamPresent = true;
          _isCamReady = false;
        }
        else if (state == KncCiState.Releasing)
        {
          _isCamPresent = false;
          _isCamReady = false;
        }
        Log.Log.Debug("  old state  = {0}", _ciState);
        Log.Log.Debug("  new state  = {0}", state);
        Log.Log.Debug("  menu title = {0}", menuTitle);
        _ciState = state;
      }
    }

    /// <summary>
    /// Called by the tuner driver when the CAM menu is successfully opened.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    private void OnCiOpenDisplay(byte slotIndex, IntPtr context)
    {
      lock (this)
      {
        Log.Log.Debug("KNC: tuner {0} open menu callback, slot = {1}", _deviceIndex, slotIndex);
      }
    }

    /// <summary>
    /// Called by the tuner driver to pass the menu meta-data when the user is browsing the CAM menu.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="title">The menu title.</param>
    /// <param name="subTitle">The menu sub-title.</param>
    /// <param name="footer">The menu footer.</param>
    /// <param name="numEntries">The number of entries in the menu.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    private void OnCiMenu(byte slotIndex, String title, String subTitle, String footer, uint numEntries, IntPtr context)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("KNC: tuner {0} menu callback, slot = {1}", _deviceIndex, slotIndex);
          Log.Log.Debug("  title     = {0}", title);
          Log.Log.Debug("  sub-title = {0}", subTitle);
          Log.Log.Debug("  footer    = {0}", footer);
          Log.Log.Debug("  # entries = {0}", numEntries);
          if (_ciMenuCallbacks != null)
          {
            _ciMenuCallbacks.OnCiMenu(title, subTitle, footer, (int)numEntries);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("KNC: menu callback exception: {0}", ex.ToString());
      }
    }

    /// <summary>
    /// Called by the tuner driver for each menu entry when the user is browsing the CAM menu.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="entryIndex">The index of the entry within the menu.</param>
    /// <param name="text">The text associated with the entry.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    private void OnCiMenuEntry(byte slotIndex, uint entryIndex, String text, IntPtr context)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("KNC: tuner {0} menu entry callback, slot = {1}", _deviceIndex, slotIndex);
          Log.Log.Debug("  entry {0:-2} = {1}", entryIndex, text);
          if (_ciMenuCallbacks != null)
          {
            _ciMenuCallbacks.OnCiMenuChoice((int)entryIndex, text);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("KNC: menu entry callback exception: {0}", ex.ToString());
      }
    }

    /// <summary>
    /// Called by the tuner driver when the CAM requests input from the user.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="blind"><c>True</c> if the input should be hidden (eg. password).</param>
    /// <param name="answerLength">The expected answer length.</param>
    /// <param name="text">The request context text from the CAM.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    private void OnCiRequest(byte slotIndex, bool blind, uint answerLength, String text, IntPtr context)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("KNC: tuner {0} request callback, slot = {1}", _deviceIndex, slotIndex);
          Log.Log.Debug("  text   = {0}", text);
          Log.Log.Debug("  length = {0}", answerLength);
          Log.Log.Debug("  blind  = {0}", blind);
          if (_ciMenuCallbacks != null)
          {
            _ciMenuCallbacks.OnCiRequest(blind, answerLength, text);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("KNC: request callback exception: {0}", ex.ToString());
      }
    }

    /// <summary>
    /// Called by the tuner driver when the CAM wants to close the menu.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="delay">The delay (in milliseconds) after which the menu should be closed.</param>
    /// <param name="context">The optional context passed to the interface when the interface was opened.</param>
    private void OnCiCloseDisplay(byte slotIndex, uint delay, IntPtr context)
    {
      try
      {
        lock (this)
        {
          Log.Log.Debug("KNC: tuner {0} request callback, slot = {1}, delay = {2}", _deviceIndex, slotIndex, delay);
          if (_ciMenuCallbacks != null)
          {
            _ciMenuCallbacks.OnCiCloseDisplay((int)delay);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("KNC: close menu callback exception: {0}", ex.ToString());
      }
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Sets the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("KNC: tuner {0} slot {1} enter menu", _deviceIndex, _slotIndex);
      return KNCBDA_CI_EnterMenu(_deviceIndex, _slotIndex);
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("KNC: tuner {0} slot {1} close menu", _deviceIndex, _slotIndex);
      return KNCBDA_CI_CloseMenu(_deviceIndex, _slotIndex);
    }

    /// <summary>
    /// Sends a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("KNC: tuner {0} slot {1} select menu entry, choice = {2}", _deviceIndex, _slotIndex, choice);
      return KNCBDA_CI_SelectMenu(_deviceIndex, _slotIndex, choice);
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = "";
      }
      Log.Log.Debug("KNC: tuner {0} slot {1} send menu answer, answer = {2}, cancel = {3}", _deviceIndex, _slotIndex, answer, cancel);
      return KNCBDA_CI_SendMenuAnswer(_deviceIndex, _slotIndex, cancel, answer);
    }

    #endregion

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      // TODO: think about how to handle this when this class is merged - maybe it can be done with
      // a base hardware provider that logs errors on all methods.
      if (channel.DisEqc == DisEqcType.SimpleA || channel.DisEqc == DisEqcType.SimpleB)
      {
        Log.Log.Debug("KNC: tuner {0} tone/data burst not supported", _deviceIndex);
        return true;    // Don't retry.
      }
      else if (channel.DisEqc != DisEqcType.None)
      {
        bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                              (channel.Polarisation == Polarisation.CircularL));
        byte command = 0xf0;
        command |= (byte)(isHighBand ? 1 : 0);
        command |= (byte)((isHorizontal) ? 2 : 0);
        command |= (byte)((antennaNr - 1) << 2);
        return SendDiSEqCCommand(new byte[4] { 0xe0, 0x10, 0x38, command });
      }
      return true;
    }

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("KNC: tuner {0} send DiSEqC command", _deviceIndex);

      int length = command.Length;
      if (length > MaxDiseqcCommandLength)
      {
        Log.Log.Debug("KNC: command too long, length = {0}", command.Length);
        return false;
      }
      for (int i = 0; i < length; i++)
      {
        Marshal.WriteByte(_diseqcBuffer, i, command[i]);
      }
      //DVB_MMI.DumpBinary(_diseqcBuffer, 0, length);

      bool success = KNCBDA_HW_DiSEqCWrite(_deviceIndex, _diseqcBuffer, (uint)length, 0);
      Log.Log.Debug("KNC: result = {0}", success);
      return success;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      // (Not implemented...)
      reply = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close the conditional access interface and free unmanaged memory buffers and COM objects.
    /// </summary>
    public void Dispose()
    {
      if (!_isKnc)
      {
        return;
      }
      CloseCi();
      Marshal.FreeCoTaskMem(_diseqcBuffer);
      Marshal.FreeCoTaskMem(_callbackBuffer);
      Release.ComObject(_graphBuilder);
      if (_captureFilter != null)
      {
        Release.ComObject(_captureFilter);
      }
    }

    #endregion
  }
}