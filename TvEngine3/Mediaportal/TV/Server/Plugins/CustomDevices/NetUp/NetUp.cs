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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.NetUp
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for NetUP devices.
  /// </summary>
  public class NetUp : BaseCustomDevice, IConditionalAccessProvider, ICiMenuActions, IDiseqcDevice
  {
    #region enums

    private enum NetUpIoControl : uint
    {
      Diseqc = 0x100000,

      CiStatus = 0x200000,
      ApplicationInfo = 0x210000,
      ConditionalAccessInfo = 0x220000,
      CiReset = 0x230000,

      MmiEnterMenu = 0x300000,
      MmiGetMenu = 0x310000,
      MmiAnswerMenu = 0x320000,
      MmiClose = 0x330000,
      MmiGetEnquiry = 0x340000,
      MmiPutAnswer = 0x350000,

      PmtListChange = 0x400000
    }

    [Flags]
    private enum NetUpCiState
    {
      Empty = 0,
      CamPresent = 1,
      MmiMenuReady = 2,
      MmiEnquiryReady = 4
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ApplicationInfo    // NETUP_CAM_APPLICATION_INFO
    {
      public MmiApplicationType ApplicationType;
      public UInt16 Manufacturer;
      public UInt16 ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String RootMenuTitle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct CiStateInfo    // NETUP_CAM_STATUS
    {
      public NetUpCiState CiState;

      // These fields don't ever seem to be filled, but that is okay since
      // we can query for application info directly.
      public UInt16 Manufacturer;
      public UInt16 ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String RootMenuTitle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CaInfo   // TYP_SLOT_INFO
    {
      public UInt32 NumberOfCaSystemIds;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCaSystemIds)]
      public UInt16[] CaSystemIds;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MmiMenuEntry
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MmiMenu    // NETUP_CAM_MENU
    {
      public bool IsMenu;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Title;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String SubTitle;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCamMenuEntries)]
      public MmiMenuEntry[] Entries;
      public UInt32 EntryCount;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MmiEnquiry   // NETUP_CAM_MMI_ENQUIRY
    {
      public bool IsBlindAnswer;
      public byte ExpectedAnswerLength;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Prompt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MmiAnswer    // NETUP_CAM_MMI_ANSWER
    {
      public byte AnswerLength;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Answer;
    }

    #endregion

    /// <summary>
    /// This class is used to "hide" the complexity of filling the command buffer.
    /// </summary>
    private class NetUpCommand
    {
      #region variables

      private static int _operatingSystemIntSize = 0;
      private UInt32 _controlCode;
      private IntPtr _inBuffer;
      private Int32 _inBufferSize;
      private IntPtr _outBuffer;
      private Int32 _outBufferSize;

      #endregion

      public NetUpCommand(NetUpIoControl controlCode, IntPtr inBuffer, Int32 inBufferSize, IntPtr outBuffer, Int32 outBufferSize)
      {
        _controlCode = (UInt32)controlCode;
        _inBuffer = inBuffer;
        _inBufferSize = inBufferSize;
        _outBuffer = outBuffer;
        _outBufferSize = outBufferSize;
      }

      /// <summary>
      /// This function determines whether the operating system is a 64 bit operating system.
      /// </summary>
      /// <returns><c>true</c> if the operating system is a 64 bit operating system, otherwise <c>false</c></returns>
      private bool Is64BitOperatingSystem()
      {
        this.LogDebug("NetUP: check is 64 bit operating system");

        if (IntPtr.Size == 8)
        {
          this.LogDebug("NetUP: 64 bit process under 64 bit operating system");
          return true;
        }

        // This isn't a 64 bit process, but it could be a 32 bit process under a 64 bit operating system.
        // If the IsWow64Process() function isn't present in kernel32.dll then the OS doesn't have 64 bit
        // support.
        IntPtr moduleHandle = NativeMethods.GetModuleHandle("kernel32.dll");
        if (moduleHandle == IntPtr.Zero)
        {
          this.LogDebug("NetUP: failed to get kernel32 module handle");
          return false;
        }
        IntPtr functionHandle = NativeMethods.GetProcAddress(moduleHandle, "IsWow64Process");
        moduleHandle = IntPtr.Zero;
        if (functionHandle == IntPtr.Zero)
        {
          this.LogDebug("NetUP: failed to get IsWow64Process function handle");
          return false;
        }
        functionHandle = IntPtr.Zero;

        bool is64bitOs = false;
        try
        {
          if (NativeMethods.IsWow64Process(Process.GetCurrentProcess().Handle, out is64bitOs))
          {
            if (is64bitOs)
            {
              this.LogDebug("NetUP: 32 bit process under 64 bit operating system");
            }
            else
            {
              this.LogDebug("NetUP: 32 bit process under 32 bit operating system");
            }
            return is64bitOs;
          }
          else
          {
            this.LogDebug("NetUP: failed to get IsWow64Process function handle");
            return false;
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "NetUP: invoking IsWow64Process caused an exception");
          return false;
        }
      }

      /// <summary>
      /// Execute a set operation on a property set.
      /// </summary>
      /// <param name="psGuid">The property set GUID.</param>
      /// <param name="ps">The property set to operate on.</param>
      /// <param name="returnedByteCount">The number of bytes returned by the operation.</param>
      /// <returns>an HRESULT indicating whether the operation was successful</returns>
      public int Execute(Guid psGuid, IKsPropertySet ps, out int returnedByteCount)
      {
        returnedByteCount = 0;
        int hr = 1; // fail
        if (ps == null)
        {
          return hr;
        }

        // Right now NetUP's drivers require 4 byte integers on 32 bit systems and 8 byte integers on
        // 64 bit systems. This is somewhat inconvenient. I have reported the issue to NetUP but there
        // has been no response.
        // mm1352000, 01-04-2012
        if (_operatingSystemIntSize == 0)
        {
          _operatingSystemIntSize = 4;
          if (Is64BitOperatingSystem())
          {
            _operatingSystemIntSize = 8;
          }
        }

        IntPtr instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
        IntPtr commandBuffer = Marshal.AllocCoTaskMem(CommandSize);
        IntPtr returnedByteCountBuffer = Marshal.AllocCoTaskMem(sizeof(int));
        try
        {
          // Clear buffers. This is probably not actually needed, but better
          // to be safe than sorry!
          for (int i = 0; i < InstanceSize; i++)
          {
            Marshal.WriteByte(instanceBuffer, i, 0);
          }
          Marshal.WriteInt32(returnedByteCountBuffer, 0);

          if (_operatingSystemIntSize == 8)
          {
            Marshal.WriteInt64(commandBuffer, 0, _controlCode);
            Marshal.WriteInt64(commandBuffer, 8, _inBuffer.ToInt64());
            Marshal.WriteInt64(commandBuffer, 16, _inBufferSize);
            Marshal.WriteInt64(commandBuffer, 24, _outBuffer.ToInt64());
            Marshal.WriteInt64(commandBuffer, 32, _outBufferSize);
            Marshal.WriteInt64(commandBuffer, 40, returnedByteCountBuffer.ToInt64());
          }
          else
          {
            Marshal.WriteInt32(commandBuffer, 0, (int)_controlCode);
            Marshal.WriteInt32(commandBuffer, 4, _inBuffer.ToInt32());
            Marshal.WriteInt32(commandBuffer, 8, _inBufferSize);
            Marshal.WriteInt32(commandBuffer, 12, _outBuffer.ToInt32());
            Marshal.WriteInt32(commandBuffer, 16, _outBufferSize);
            Marshal.WriteInt32(commandBuffer, 20, returnedByteCountBuffer.ToInt32());
            Marshal.WriteInt32(commandBuffer, 24, 0);
            Marshal.WriteInt32(commandBuffer, 28, 0);
            Marshal.WriteInt32(commandBuffer, 32, 0);
            Marshal.WriteInt32(commandBuffer, 36, 0);
            Marshal.WriteInt32(commandBuffer, 40, 0);
            Marshal.WriteInt32(commandBuffer, 44, 0);
          }

          hr = ps.Set(psGuid, 0, instanceBuffer, InstanceSize, commandBuffer, CommandSize);
          if (hr == 0)
          {
            returnedByteCount = Marshal.ReadInt32(returnedByteCountBuffer);
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(instanceBuffer);
          Marshal.FreeCoTaskMem(commandBuffer);
          Marshal.FreeCoTaskMem(returnedByteCountBuffer);
        }
        return hr;
      }
    }

    #region constants

    private static readonly Guid NetUpBdaExtensionPropertySet = new Guid(0x5aa642f2, 0xbf94, 0x4199, 0xa9, 0x8c, 0xc2, 0x22, 0x20, 0x91, 0xe3, 0xc3);

    private const int InstanceSize = 32;    // The size of a property instance (KSP_NODE) parameter.
    private const int CommandSize = 48;
    private const int ApplicationInfoSize = 6 + MaxStringLength;
    private const int CiStateInfoSize = 8 + MaxStringLength;
    private const int CaInfoSize = 4 + (MaxCaSystemIds * 2);
    private const int MmiMenuSize = 8 + (MaxStringLength * (3 + MaxCamMenuEntries));
    private const int MmiEnquirySize = 8 + MaxStringLength;
    private const int MmiAnswerSize = 4 + MaxStringLength;
    private const int MaxBufferSize = 65536;
    private const int MaxStringLength = 256;
    private const int MaxCaSystemIds = 256;
    private const int MaxCamMenuEntries = 64;
    private const int MaxDiseqcMessageLength = 64;        // This is to reduce the _generalBuffer size - the driver limit is MaxBufferSize.

    private const int MmiHandlerThreadSleepTime = 2000;   // unit = ms

    #endregion

    #region variables

    private bool _isNetUp = false;
    private bool _isCamPresent = false;

    // Functions that are called from both the main TV service threads
    // as well as the MMI handler thread use their own local buffer to
    // avoid buffer data corruption. Otherwise functions called exclusively
    // by the MMI handler thread use the MMI buffer and other functions
    // use the general buffer.
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;

    private Thread _mmiHandlerThread = null;
    private bool _stopMmiHandlerThread = false;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Accessor for the property set GUID. This allows other classes with identical structs but different
    /// GUIDs to easily inherit the methods defined in this class.
    /// </summary>
    /// <value>the GUID for the driver's custom property set</value>
    protected virtual Guid BdaExtensionPropertySet
    {
      get
      {
        return NetUpBdaExtensionPropertySet;
      }
    }

    /// <summary>
    /// Read the conditional access application information.
    /// </summary>
    /// <returns>an HRESULT indicating whether the application information was successfully retrieved</returns>
    private int ReadApplicationInformation()
    {
      this.LogDebug("NetUP: read application information");

      for (int i = 0; i < ApplicationInfoSize; i++)
      {
        Marshal.WriteByte(_mmiBuffer, i, 0);
      }
      NetUpCommand command = new NetUpCommand(NetUpIoControl.ApplicationInfo, IntPtr.Zero, 0, _mmiBuffer, ApplicationInfoSize);
      int returnedByteCount;
      int hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);

      if (hr == 0 && returnedByteCount == ApplicationInfoSize)
      {
        ApplicationInfo info = (ApplicationInfo)Marshal.PtrToStructure(_mmiBuffer, typeof(ApplicationInfo));
        this.LogDebug("  type         = {0}", info.ApplicationType);
        this.LogDebug("  manufacturer = 0x{0:x}", info.Manufacturer);
        this.LogDebug("  code         = 0x{0:x}", info.ManufacturerCode);
        this.LogDebug("  menu title   = {0}", info.RootMenuTitle);
      }
      else
      {
        this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return hr;
    }

    /// <summary>
    /// Read the conditional access information.
    /// </summary>
    /// <returns>an HRESULT indicating whether the conditional access information was successfully retrieved</returns>
    private int ReadConditionalAccessInformation()
    {
      this.LogDebug("NetUP: read conditional access information");

      for (int i = 0; i < CaInfoSize; i++)
      {
        Marshal.WriteByte(_mmiBuffer, i, 0);
      }
      NetUpCommand command = new NetUpCommand(NetUpIoControl.ConditionalAccessInfo, IntPtr.Zero, 0, _mmiBuffer, CaInfoSize);
      int returnedByteCount;
      int hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      if (hr == 0 && returnedByteCount == CaInfoSize)
      {
        CaInfo info = (CaInfo)Marshal.PtrToStructure(_mmiBuffer, typeof(CaInfo));
        this.LogDebug("  # CAS IDs = {0}", info.NumberOfCaSystemIds);
        for (int i = 0; i < info.NumberOfCaSystemIds; i++)
        {
          this.LogDebug("  {0,-2}        = 0x{1:x4}", i + 1, info.CaSystemIds[i]);
        }
      }
      else
      {
        this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      return hr;
    }

    /// <summary>
    /// Get the conditional access interface status.
    /// </summary>
    /// <param name="ciState">State of the CI slot.</param>
    /// <returns>an HRESULT indicating whether the CI status was successfully retrieved</returns>
    private int GetCiStatus(out CiStateInfo ciState)
    {
      ciState = new CiStateInfo();

      // Use a local buffer here because this function is called from the MMI
      // polling thread as well as indirectly from the main TV service thread.
      IntPtr buffer = Marshal.AllocCoTaskMem(CiStateInfoSize);
      for (int i = 0; i < CiStateInfoSize; i++)
      {
        Marshal.WriteByte(buffer, i, 0);
      }
      NetUpCommand command = new NetUpCommand(NetUpIoControl.CiStatus, IntPtr.Zero, 0, buffer, CiStateInfoSize);
      int returnedByteCount;
      int hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      if (hr == 0 && returnedByteCount == CiStateInfoSize)
      {
        ciState = (CiStateInfo)Marshal.PtrToStructure(buffer, typeof(CiStateInfo));
      }
      Marshal.FreeCoTaskMem(buffer);
      return hr;
    }

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if there is no purpose for it.
      if (!_isNetUp)
      {
        return;
      }

      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        this.LogDebug("NetUP: aborting old MMI handler thread");
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        this.LogDebug("NetUP: starting new MMI handler thread");
        _stopMmiHandlerThread = false;
        _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
        _mmiHandlerThread.Name = "NetUP MMI handler";
        _mmiHandlerThread.IsBackground = true;
        _mmiHandlerThread.Priority = ThreadPriority.Lowest;
        _mmiHandlerThread.Start();
      }
    }

    /// <summary>
    /// Thread function for handling MMI responses from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      this.LogDebug("NetUP: MMI handler thread start polling");
      NetUpCiState ciState = NetUpCiState.Empty;
      NetUpCiState prevCiState = NetUpCiState.Empty;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          Thread.Sleep(MmiHandlerThreadSleepTime);

          CiStateInfo info;
          int hr = GetCiStatus(out info);
          if (hr != 0)
          {
            this.LogDebug("NetUP: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            continue;
          }

          // Handle CI slot state changes.
          ciState = info.CiState;
          if (ciState != prevCiState)
          {
            this.LogDebug("NetUP: CI state change");
            this.LogDebug("  old state    = {0}", prevCiState.ToString());
            this.LogDebug("  new state    = {0}", ciState.ToString());

            prevCiState = ciState;
            if (ciState == NetUpCiState.Empty)
            {
              _isCamPresent = false;
            }
            else
            {
              _isCamPresent = true;
            }
          }

          if ((ciState & NetUpCiState.MmiEnquiryReady) != 0)
          {
            HandleEnquiry();
          }
          if ((ciState & NetUpCiState.MmiMenuReady) != 0)
          {
            HandleMenu();
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "NetUP: exception in MMI handler thread");
        return;
      }
    }

    /// <summary>
    /// Read an MMI menu object and invoke callbacks as appropriate.
    /// </summary>
    /// <returns>an HRESULT indicating whether the menu object was successfully handled</returns>
    private int HandleMenu()
    {
      this.LogDebug("NetUP: read menu");

      MmiMenu mmi;
      lock (this)
      {
        for (int i = 0; i < MmiMenuSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }

        NetUpCommand command = new NetUpCommand(NetUpIoControl.MmiGetMenu, IntPtr.Zero, 0, _mmiBuffer, MmiMenuSize);
        int returnedByteCount;
        int hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
        if (hr != 0 || returnedByteCount != MmiMenuSize)
        {
          this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return hr;
        }
        mmi = (MmiMenu)Marshal.PtrToStructure(_mmiBuffer, typeof(MmiMenu));
      }

      if (_ciMenuCallbacks == null)
      {
        this.LogDebug("NetUP: menu callbacks are not set");
      }

      this.LogDebug("  is menu   = {0}", mmi.IsMenu);
      this.LogDebug("  title     = {0}", mmi.Title);
      this.LogDebug("  sub-title = {0}", mmi.SubTitle);
      this.LogDebug("  footer    = {0}", mmi.Footer);
      this.LogDebug("  # entries = {0}", mmi.EntryCount);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiMenu(mmi.Title, mmi.SubTitle, mmi.Footer, (int)mmi.EntryCount);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "NetUP: menu header callback exception");
          return 1;
        }
      }

      for (int i = 0; i < mmi.EntryCount; i++)
      {
        this.LogDebug("  entry {0,-2}  = {1}", i + 1, mmi.Entries[i].Text);
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiMenuChoice(i, mmi.Entries[i].Text);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "NetUP: menu choice callback exception");
            return 1;
          }
        }
      }
      return 0; // success
    }

    /// <summary>
    /// Read an MMI enquiry object and invoke callbacks as appropriate.
    /// </summary>
    /// <returns>an HRESULT indicating whether the enquiry object was successfully handled</returns>
    private int HandleEnquiry()
    {
      this.LogDebug("NetUP: read enquiry");

      MmiEnquiry mmi;
      lock (this)
      {
        for (int i = 0; i < MmiEnquirySize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }

        NetUpCommand command = new NetUpCommand(NetUpIoControl.MmiGetEnquiry, IntPtr.Zero, 0, _mmiBuffer, MmiEnquirySize);
        int returnedByteCount;
        int hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
        if (hr != 0 || returnedByteCount != MmiEnquirySize)
        {
          this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return hr;
        }
        //DVB_MMI.DumpBinary(_mmiBuffer, 0, MmiEnquirySize);
        mmi = (MmiEnquiry)Marshal.PtrToStructure(_mmiBuffer, typeof(MmiEnquiry));
      }

      if (_ciMenuCallbacks == null)
      {
        this.LogDebug("NetUP: menu callbacks are not set");
      }

      this.LogDebug("  prompt = {0}", mmi.Prompt);
      this.LogDebug("  length = {0}", mmi.ExpectedAnswerLength);
      this.LogDebug("  blind  = {0}", mmi.IsBlindAnswer);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiRequest(mmi.IsBlindAnswer, mmi.ExpectedAnswerLength, mmi.Prompt);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "NetUP: CAM request callback exception");
          return 1;
        }
      }
      return 0; // success
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return "NetUP";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      this.LogDebug("NetUP: initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("NetUP: tuner filter is null");
        return false;
      }
      if (_isNetUp)
      {
        this.LogDebug("NetUP: device is already initialised");
        return true;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("NetUP: pin is not a property set");
        if (pin != null)
        {
          DsUtils.ReleaseComObject(pin);
          pin = null;
        }
        return false;
      }

      // The NetUP property set is found on the tuner filter output pin. Since current NetUP tuners are
      // implemented as a combined filter, the filter output pin is normally going to be unconnected when this
      // function is called. That is a problem because a pin won't correctly report whether it supports a property
      // set unless it is connected to a filter. If this filter pin is currently unconnected, we temporarily
      // connect an infinite tee so that we can check if the pin supports the property set.
      IPin connected;
      int hr = pin.ConnectedTo(out connected);
      if (hr == 0 && connected != null)
      {
        // We don't need to connect the infinite tee in this case.
        KSPropertySupport support;
        hr = _propertySet.QuerySupported(BdaExtensionPropertySet, 0, out support);
        if (hr != 0 || (support & KSPropertySupport.Set) == 0)
        {
          this.LogDebug("NetUP: device does not support the NetUP property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }
        this.LogDebug("NetUP: supported device detected");
        _isNetUp = true;
        _generalBuffer = Marshal.AllocCoTaskMem(MaxDiseqcMessageLength);
        return true;
      }

      try
      {
        // Get a reference to the filter graph.
        FilterInfo filterInfo;
        hr = tunerFilter.QueryFilterInfo(out filterInfo);
        if (hr != 0)
        {
          this.LogDebug("NetUP: failed to get filter info, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }
        IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
        if (graph == null)
        {
          this.LogDebug("NetUP: filter info graph is null");
          return false;
        }

        // Add an infinite tee.
        IBaseFilter infTee = (IBaseFilter)new InfTee();
        IPin infTeeInputPin = null;
        try
        {
          hr = graph.AddFilter(infTee, "Temp Infinite Tee");
          if (hr != 0)
          {
            this.LogDebug("NetUP: failed to add inf tee to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          // Connect the infinite tee to the filter.
          infTeeInputPin = DsFindPin.ByDirection(infTee, PinDirection.Input, 0);
          if (infTeeInputPin == null)
          {
            this.LogDebug("NetUP: failed to find the inf tee input pin, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }
          hr = graph.Connect(pin, infTeeInputPin);
          if (hr != 0)
          {
            this.LogDebug("NetUP: failed to connect pins, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          // Check if the NetUP property set is supported.
          KSPropertySupport support;
          hr = _propertySet.QuerySupported(BdaExtensionPropertySet, 0, out support);
          if (hr != 0 || (support & KSPropertySupport.Set) == 0)
          {
            this.LogDebug("NetUP: device does not support the NetUP property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          this.LogDebug("NetUP: supported device detected");
          _isNetUp = true;
          _generalBuffer = Marshal.AllocCoTaskMem(MaxDiseqcMessageLength);
          return true;
        }
        finally
        {
          pin.Disconnect();
          if (infTeeInputPin != null)
          {
            DsUtils.ReleaseComObject(infTeeInputPin);
            infTeeInputPin = null;
          }
          graph.RemoveFilter(infTee);
          DsUtils.ReleaseComObject(infTee);
          infTee = null;
          DsUtils.ReleaseComObject(graph);
          graph = null;
        }
      }
      finally
      {
        if (!_isNetUp)
        {
          DsUtils.ReleaseComObject(pin);
          pin = null;
        }
      }
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked after a tune request is submitted, when the device is running but before
    /// signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnRunning(ITVCard tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    #endregion

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenInterface()
    {
      this.LogDebug("NetUP: open conditional access interface");

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }
      if (_mmiBuffer != IntPtr.Zero)
      {
        this.LogDebug("NetUP: interface is already open");
        return false;
      }

      _mmiBuffer = Marshal.AllocCoTaskMem(MmiMenuSize);

      _isCamPresent = IsInterfaceReady();

      StartMmiHandlerThread();

      this.LogDebug("NetUP: result = success");
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      this.LogDebug("NetUP: close conditional access interface");
      if (_mmiHandlerThread != null && _mmiHandlerThread.IsAlive)
      {
        _stopMmiHandlerThread = true;
        // In the worst case scenario it should take approximately
        // twice the thread sleep time to cleanly stop the thread.
        Thread.Sleep(MmiHandlerThreadSleepTime * 2);
        _mmiHandlerThread = null;
      }

      _isCamPresent = false;
      if (_mmiBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiBuffer);
        _mmiBuffer = IntPtr.Zero;
      }

      this.LogDebug("NetUP: result = true");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool rebuildGraph)
    {
      this.LogDebug("NetUP: reset conditional access interface");
      rebuildGraph = false;

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }

      bool success = CloseInterface();

      NetUpCommand command = new NetUpCommand(NetUpIoControl.CiReset, IntPtr.Zero, 0, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      if (hr == 0)
      {
        this.LogDebug("NetUP: result = success");
      }
      else
      {
        this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }

      return success && OpenInterface();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      this.LogDebug("NetUP: is conditional access interface ready");

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }

      CiStateInfo info;
      int hr = GetCiStatus(out info);
      if (hr != 0)
      {
        this.LogDebug("NetUP: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      this.LogDebug("NetUP: state = {0}", info.CiState.ToString());

      // We can only tell whether a CAM is present or not.
      bool camPresent = false;
      if (info.CiState != NetUpCiState.Empty)
      {
        camPresent = true;
      }
      this.LogDebug("NetUP: result = {0}", camPresent);
      return camPresent;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The programme map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      this.LogDebug("NetUP: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }
      if (listAction == CaPmtListManagementAction.Add || listAction == CaPmtListManagementAction.Update)
      {
        this.LogDebug("NetUP: list action {0} is not supported", listAction);
        return false;
      }
      if (command == CaPmtCommand.NotSelected)
      {
        this.LogDebug("NetUP: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null)
      {
        this.LogDebug("NetUP: PMT not supplied");
        return true;
      }

      // The NetUP driver accepts standard PMT and converts it to CA PMT internally.
      ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
      NetUpIoControl code = (NetUpIoControl)((uint)NetUpIoControl.PmtListChange | ((byte)listAction << 8) | (uint)command);
      IntPtr buffer = Marshal.AllocCoTaskMem(rawPmt.Count);
      for (int i = 0; i < rawPmt.Count; i++)
      {
        Marshal.WriteByte(buffer, i, rawPmt[i]);
      }
      //DVB_MMI.DumpBinary(buffer, 0, rawPmt.Count);
      NetUpCommand ncommand = new NetUpCommand(code, buffer, rawPmt.Count, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = ncommand.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      Marshal.FreeCoTaskMem(buffer);
      if (hr == 0)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Set the CAM menu callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        StartMmiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      this.LogDebug("NetUP: enter menu");

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        this.LogDebug("NetUP: the CAM is not present");
        return false;
      }

      int hr;
      lock (this)
      {
        ReadApplicationInformation();
        ReadConditionalAccessInformation();

        NetUpCommand command = new NetUpCommand(NetUpIoControl.MmiEnterMenu, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      this.LogDebug("NetUP: close menu");

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        this.LogDebug("NetUP: the CAM is not present");
        return false;
      }

      int hr;
      lock (this)
      {
        NetUpCommand command = new NetUpCommand(NetUpIoControl.MmiClose, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      this.LogDebug("NetUP: select menu entry, choice = {0}", (int)choice);

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        this.LogDebug("NetUP: the CAM is not present");
        return false;
      }

      int hr;
      lock (this)
      {
        NetUpIoControl code = (NetUpIoControl)((uint)NetUpIoControl.MmiAnswerMenu | choice << 8);
        NetUpCommand command = new NetUpCommand(code, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      this.LogDebug("NetUP: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        this.LogDebug("NetUP: the CAM is not present");
        return false;
      }

      // We have a limit for the answer string length.
      if (answer.Length > MaxStringLength)
      {
        this.LogDebug("NetUP: answer too long, length = {0}", answer.Length);
        return false;
      }

      MmiAnswer mmi = new MmiAnswer();
      mmi.AnswerLength = (byte)answer.Length;
      mmi.Answer = answer;
      MmiResponseType responseType = MmiResponseType.Answer;
      if (cancel)
      {
        responseType = MmiResponseType.Cancel;
      }

      int hr;
      lock (this)
      {
        Marshal.StructureToPtr(mmi, _mmiBuffer, true);
        //DVB_MMI.DumpBinary(_mmiBuffer, 0, MmiAnswerSize);
        NetUpIoControl code = (NetUpIoControl)((uint)NetUpIoControl.MmiPutAnswer | ((byte)responseType << 8));
        NetUpCommand command = new NetUpCommand(code, _mmiBuffer, MmiAnswerSize, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The NetUP interface does not support sending tone burst commands, and the 22 kHz tone state
    /// cannot be set directly. The tuning request LNB frequency parameters can be used to manipulate the
    /// tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public virtual bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      // Not supported.
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public virtual bool SendCommand(byte[] command)
    {
      this.LogDebug("NetUP: send DiSEqC command");

      if (!_isNetUp || _propertySet == null)
      {
        this.LogDebug("NetUP: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("NetUP: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        this.LogDebug("NetUP: command too long, length = {0}", command.Length);
        return false;
      }

      Marshal.Copy(command, 0, _generalBuffer, command.Length);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, command.Length);

      NetUpCommand ncommand = new NetUpCommand(NetUpIoControl.Diseqc, _generalBuffer, command.Length, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = ncommand.Execute(BdaExtensionPropertySet, _propertySet, out returnedByteCount);
      if (hr == 0)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogDebug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public virtual bool ReadResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      CloseInterface();
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (_propertySet != null)
      {
        DsUtils.ReleaseComObject(_propertySet);
        _propertySet = null;
      }
      _isNetUp = false;
    }

    #endregion
  }
}
